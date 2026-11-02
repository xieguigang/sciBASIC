' ---------------------------------------------------------------------------
' 对称归一化邻接 Â = D̃^(-1/2) Ã D̃^(-1/2)，Ã = A + I
'
' 为什么需要一个专门的类型，而不是复用 GNN.Graph.GetNormalizedAdjacencyMatrix()：
'   1. GNN 那个方法只给稠密 fp64 矩阵。GCN 需要第二条路径 —— CSR 稀疏（SpMM），
'      否则十万级以上节点的图光是 n² 就先把内存打爆；
'   2. 稠密矩阵按需构建：只想跑稀疏路径的调用方不该为此付出 8n² 字节；
'   3. 归一化系数必须一次算好并缓存。它是纯拓扑量（与特征、时间步无关），
'      而 GCN 每层每次前向都要用 —— 每帧重算 O(n² + m) 是纯粹的浪费。
'
' 数值细节（这些不是吹毛求疵，是真会改变结果的）：
'   * 度用 Double 累加：节点数上万时单精度加和会丢掉尾部贡献，
'     而度值直接进入 1/√d̃，误差会乘以整行权重；
'   * 归一化在 fp64 里做：Â 的元素被反复用于前向与反向，先降精度等于把误差
'     写进“常量”里，之后怎么算都补不回来；
'   * 对角元严格为 1.0（自环）：输入边表里出现的自环被忽略，避免 A 的自环与 I 叠加。
' ---------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

''' <summary>
''' 消息传递路径：决定用稠密矩阵乘还是 CSR 稀疏乘。
''' </summary>
Public Enum PropagationMode

    ''' <summary>按稀疏度自动选择（默认）</summary>
    Auto = 0
    ''' <summary>稠密矩阵乘：小图更省心，且能吃到 GPU 上的大 GEMM</summary>
    Dense = 1
    ''' <summary>CSR 稀疏乘：大图 / 高稀疏图，显存与算力都只花在非零元上</summary>
    Sparse = 2

End Enum

''' <summary>
''' 对称归一化邻接 <c>Â = D̃^(-1/2) Ã D̃^(-1/2)</c> 的载体（<c>Ã = A + I</c>）。
''' </summary>
''' <remarks>
''' <para>
''' 稠密矩阵与 CSR 结构共享同一份归一化系数，因此两条路径的结果在数值上一致
''' （差异只来自求和次序与精度，不含语义差异）。构造一次即可被所有层的每次前向复用。
''' </para>
''' <para>
''' 重复构造没有意义：<c>NormalizedAdjacency.Get</c> 以图对象为键做了弱引用缓存，
''' 并以 <see cref="Graph.NumNodes"/> / <see cref="Graph.NumEdges"/> 作为“拓扑是否变了”的
''' 廉价判据（<see cref="Graph"/> 的结构版本号是私有的，外部拿不到）。
''' 若就地修改了边权而没有增减边，请调用 <see cref="Invalidate"/>。
''' </para>
''' </remarks>
Public Class NormalizedAdjacency

    ''' <summary>
    ''' 密度低于该值时 <see cref="PropagationMode.Auto"/> 选择 CSR
    ''' </summary>
    ''' <remarks>
    ''' 25% 是经验起点：再密下去 CSR 的行指针/列索引开销就超过了省下的零元乘法，
    ''' 而稠密路径还能直接落到设备端的 GEMM 内核上。
    ''' </remarks>
    Public Shared Property DenseDensityThreshold As Double = 0.25

    Private Shared ReadOnly _cache As New ConditionalWeakTable(Of Graph, CacheEntry)()

    ''' <summary>弱引用缓存条目。用 NotInheritable 嵌套类是为了满足 GetOrCreateValue 的无参构造约束。</summary>
    Private NotInheritable Class CacheEntry

        Public Property Adjacency As NormalizedAdjacency
        Public Property Nodes As Integer = -1
        Public Property Edges As Integer = -1
        Public Property Mode As PropagationMode = PropagationMode.Auto

    End Class

    Private ReadOnly _nodes As Integer
    Private ReadOnly _edges As Integer
    Private ReadOnly _degree As Double()
    Private ReadOnly _rowPtr As Integer()
    Private ReadOnly _colIdx As Integer()
    Private ReadOnly _values As Double()

    Private _mode As PropagationMode
    Private _dense As Tensor
    Private _sparse As SparseCsr
    Private ReadOnly _syncRoot As New Object()

    ''' <summary>
    ''' 取（或复用）某张图的归一化邻接。
    ''' </summary>
    ''' <param name="graph">图结构；节点特征与拓扑无关，本方法只读 <see cref="Graph.Edges"/></param>
    ''' <param name="mode">传播路径；Auto 表示按稀疏度自动选择</param>
    Public Shared Function [Get](graph As Graph, Optional mode As PropagationMode = PropagationMode.Auto) As NormalizedAdjacency
        If graph Is Nothing Then Throw New ArgumentNullException(NameOf(graph))

        Dim entry = _cache.GetOrCreateValue(graph)

        SyncLock entry
            ' NumEdges 变化 = 拓扑变化；这是 Graph 对外可见的唯一廉价判据
            If entry.Adjacency Is Nothing OrElse
               entry.Nodes <> graph.NumNodes OrElse
               entry.Edges <> graph.NumEdges OrElse
               entry.Mode <> mode Then

                entry.Adjacency = FromGraph(graph, mode)
                entry.Nodes = graph.NumNodes
                entry.Edges = graph.NumEdges
                entry.Mode = mode
            End If

            Return entry.Adjacency
        End SyncLock
    End Function

    ''' <summary>
    ''' 让某张图的缓存失效（就地改过边权、或需要强制重建时调用）。
    ''' </summary>
    Public Shared Sub Invalidate(graph As Graph)
        If graph Is Nothing Then Return

        Dim entry = Nothing

        If _cache.TryGetValue(graph, entry) Then
            SyncLock entry
                entry.Adjacency = Nothing
            End SyncLock
        End If
    End Sub

    ''' <summary>
    ''' 由图结构构建归一化邻接。
    ''' </summary>
    ''' <param name="graph">图结构</param>
    ''' <param name="mode">传播路径</param>
    ''' <param name="symmetrize">
    ''' 是否把有向边对称化。GCN 的对称归一化 <c>D̃^(-1/2) Ã D̃^(-1/2)</c> 只有在 Ã 对称时
    ''' 才是对称矩阵；一旦不对称，<see cref="PropagationMode.Sparse"/> 路径复用的
    ''' “稠密 × 稀疏” 契约就不再等价于 “稀疏 × 稠密”，结果会错。因此默认对称化：
    ''' 单向边按无向边处理，双向边取两个方向的均值（无向图里两个方向权重相同，均值即原值）。
    ''' </param>
    ''' <param name="buildDense">是否立即构建稠密矩阵（否则首次访问 <see cref="Dense"/> 时才建）</param>
    Public Shared Function FromGraph(graph As Graph,
                                     Optional mode As PropagationMode = PropagationMode.Auto,
                                     Optional symmetrize As Boolean = True,
                                     Optional buildDense As Boolean = False) As NormalizedAdjacency

        If graph Is Nothing Then Throw New ArgumentNullException(NameOf(graph))

        Dim n = graph.NumNodes

        If n <= 0 Then Throw New ArgumentException("图的节点数必须为正整数", NameOf(graph))

        ' ---- 第一步：把边表折叠成“无向边 + 权重” ----
        ' 用两个字典分别记录 u→v 与 v→u，才能在有向图上做出合理的对称化。
        Dim forward As New Dictionary(Of Long, Double)()
        Dim backward As New Dictionary(Of Long, Double)()

        For Each edge In graph.Edges
            ' 自环由 Ã = A + I 统一贡献 1.0，这里重复计入会让该行权重凭空变大
            If edge.Source = edge.Target Then Continue For

            Dim lo = std.Min(edge.Source, edge.Target)
            Dim hi = std.Max(edge.Source, edge.Target)
            Dim key As Long = (CLng(lo) << 32) Or CLng(hi)
            Dim bucket = If(edge.Source = lo, forward, backward)

            Dim prev As Double

            If bucket.TryGetValue(key, prev) Then
                bucket(key) = prev + edge.Weight
            Else
                bucket(key) = edge.Weight
            End If
        Next

        Dim keys As New HashSet(Of Long)(forward.Keys)

        Call keys.UnionWith(backward.Keys)

        Dim rows As New List(Of Integer)()
        Dim cols As New List(Of Integer)()
        Dim vals As New List(Of Double)()

        For Each key As Long In keys
            Dim wForward As Double = 0.0
            Dim wBackward As Double = 0.0
            Dim hasForward = forward.TryGetValue(key, wForward)
            Dim hasBackward = backward.TryGetValue(key, wBackward)

            Dim w As Double

            If hasForward AndAlso hasBackward Then
                w = If(symmetrize, (wForward + wBackward) / 2.0, wForward)
            ElseIf hasForward Then
                w = wForward
            Else
                w = wBackward
            End If

            If w = 0.0 Then Continue For

            Dim lo = CInt(key >> 32)
            Dim hi = CInt(key And &HFFFFFFFFL)

            Call rows.Add(lo)
            Call cols.Add(hi)
            Call vals.Add(w)
            Call rows.Add(hi)
            Call cols.Add(lo)
            Call vals.Add(w)
        Next

        Dim undirectedEdges = rows.Count \ 2

        ' ---- 第二步：加自环 Ã = A + I ----
        For i = 0 To n - 1
            Call rows.Add(i)
            Call cols.Add(i)
            Call vals.Add(1.0)
        Next

        ' ---- 第三步：度与 1/√d̃（全部 fp64 累加） ----
        Dim degree(n - 1) As Double

        For k = 0 To vals.Count - 1
            degree(rows(k)) += vals(k)
        Next

        Dim dInvSqrt(n - 1) As Double

        For i = 0 To n - 1
            dInvSqrt(i) = If(degree(i) > 0.0, 1.0 / std.Sqrt(degree(i)), 0.0)
        Next

        For k = 0 To vals.Count - 1
            vals(k) = vals(k) * dInvSqrt(rows(k)) * dInvSqrt(cols(k))
        Next

        ' ---- 第四步：计数排序压成 CSR（O(n + m)） ----
        Dim m = vals.Count
        Dim rowPtr(n) As Integer

        For k = 0 To m - 1
            rowPtr(rows(k) + 1) += 1
        Next

        For i = 0 To n - 1
            rowPtr(i + 1) += rowPtr(i)
        Next

        Dim cursor = CType(rowPtr.Clone(), Integer())
        Dim colIdx(m - 1) As Integer
        Dim values(m - 1) As Double

        For k = 0 To m - 1
            Dim r = rows(k)
            Dim pos = cursor(r)

            colIdx(pos) = cols(k)
            values(pos) = vals(k)
            cursor(r) = pos + 1
        Next

        Dim adjacency As New NormalizedAdjacency(n, undirectedEdges, degree, rowPtr, colIdx, values, mode)

        If buildDense Then Call adjacency.EnsureDense()

        Return adjacency
    End Function

    Private Sub New(nodes As Integer,
                    edges As Integer,
                    degree As Double(),
                    rowPtr As Integer(),
                    colIdx As Integer(),
                    values As Double(),
                    mode As PropagationMode)

        _nodes = nodes
        _edges = edges
        _degree = degree
        _rowPtr = rowPtr
        _colIdx = colIdx
        _values = values
        _mode = mode
    End Sub

    ''' <summary>节点数（矩阵阶数）</summary>
    Public ReadOnly Property Nodes As Integer
        Get
            Return _nodes
        End Get
    End Property

    ''' <summary>对称化后的无向边数（不含自环）</summary>
    Public ReadOnly Property UndirectedEdges As Integer
        Get
            Return _edges
        End Get
    End Property

    ''' <summary>CSR 的非零元个数（含自环）</summary>
    Public ReadOnly Property NonZeros As Integer
        Get
            Return _values.Length
        End Get
    End Property

    ''' <summary>稠密度 = 非零元 / n²，用于 Auto 模式的选择与诊断</summary>
    Public ReadOnly Property Density As Double
        Get
            Return If(_nodes > 0, NonZeros / (CDbl(_nodes) * _nodes), 0.0)
        End Get
    End Property

    ''' <summary>传播路径；构造后改写可以让 Auto 之外的调用方显式指定</summary>
    Public Property Mode As PropagationMode
        Get
            Return _mode
        End Get
        Set(value As PropagationMode)
            _mode = value
        End Set
    End Property

    ''' <summary>实际生效的传播路径（把 Auto 解析成 Dense / Sparse）</summary>
    Public ReadOnly Property EffectiveMode As PropagationMode
        Get
            Select Case _mode
                Case PropagationMode.Dense, PropagationMode.Sparse
                    Return _mode
                Case Else
                    Return If(Density <= DenseDensityThreshold, PropagationMode.Sparse, PropagationMode.Dense)
            End Select
        End Get
    End Property

    ''' <summary>度向量（含自环，fp64）</summary>
    Public ReadOnly Property Degree As Double()
        Get
            Return _degree
        End Get
    End Property

    ''' <summary>CSR 形式的 Â（fp64）。首次访问时构建，之后复用。</summary>
    Public ReadOnly Property Sparse As SparseCsr
        Get
            SyncLock _syncRoot
                If _sparse Is Nothing Then
                    _sparse = New SparseCsr(_nodes, _nodes, _rowPtr, _colIdx, _values)
                End If

                Return _sparse
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' 稠密形式的 Â（fp64，n×n）。首次访问时构建，之后复用。
    ''' </summary>
    ''' <remarks>
    ''' 只为稀疏路径服务的调用方不需要它 —— 8n² 字节在 n=10⁵ 时是 80GB。
    ''' 用 <see cref="ReleaseDense"/> 可以在跑完一批稠密前向后把它放掉。
    ''' </remarks>
    Public ReadOnly Property Dense As Tensor
        Get
            EnsureDense()
            Return _dense
        End Get
    End Property

    ''' <summary>
    ''' 立即构建稠密镜像（预热用：把“首次访问才分配 8n² 字节”的开销挪到构造阶段）
    ''' </summary>
    Public Sub EnsureDense()
        SyncLock _syncRoot
            If _dense IsNot Nothing Then Return

            Dim matrix = New Tensor(_nodes, _nodes)
            Dim data = matrix.Data

            For r = 0 To _nodes - 1
                Dim rowOffset = r * _nodes

                For k = _rowPtr(r) To _rowPtr(r + 1) - 1
                    ' 逐元素写底层数组，避免索引器 setter 每个元素都自增一次版本号
                    data(rowOffset + _colIdx(k)) = _values(k)
                Next
            Next

            Call matrix.MarkHostModified()

            _dense = matrix
        End SyncLock
    End Sub

    ''' <summary>放掉稠密镜像，只保留 CSR（纯稀疏训练时用来回收内存）</summary>
    Public Sub ReleaseDense()
        SyncLock _syncRoot
            _dense = Nothing
        End SyncLock
    End Sub

    ''' <summary>把 Â 还原成稠密矩阵（主要给测试与调试用，不参与训练）</summary>
    Public Function ToDense() As Tensor
        Return Dense.Clone()
    End Function

    Public Overrides Function ToString() As String
        Return $"NormalizedAdjacency(n={_nodes}, m={_edges}, nnz={NonZeros}, density={Density:P2}, mode={EffectiveMode})"
    End Function

End Class
