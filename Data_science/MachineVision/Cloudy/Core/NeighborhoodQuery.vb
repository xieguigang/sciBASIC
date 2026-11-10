Imports System.Threading
Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 邻域结果容器：下标 + 平方距离，按需扩容。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 点云算法里“每个点一次邻域查询”是最热的操作（1e6 点 × k=30 的邻域，
    ''' 意味着 3000 万次下标写入）。如果每次查询都新建两个数组，
    ''' 光 GC 就会吃掉大部分运行时间。
    ''' </para>
    ''' <para>
    ''' 因此这里把结果做成<b>可复用容器</b>：调用方持有一个实例，反复 <see cref="Clear"/> 后重用，
    ''' 容量只增不减。<see cref="NeighborhoodQuery"/> 内部使用线程本地池，
    ''' 保证并行时每个线程有自己的缓冲。
    ''' </para>
    ''' <para>
    ''' <b>排序约定</b>：kNN 结果按平方距离<b>升序</b>排列（最近邻在 0 号位）；
    ''' 半径搜索的结果不保证有序（顺序取决于 KD 树遍历顺序），需要排序时调用
    ''' <see cref="SortByDistance"/>。
    ''' </para>
    ''' </remarks>
    Public Class Neighborhood

        ''' <summary>
        ''' 邻域点下标（长度 &gt;= <see cref="Count"/>）。
        ''' </summary>
        Public Indices As Integer()

        ''' <summary>
        ''' 与下标一一对应的<b>平方</b>距离（比较大小不需要开方）。
        ''' </summary>
        Public Distances As Double()

        ''' <summary>
        ''' 有效元素个数。
        ''' </summary>
        Public Count As Integer

        Public Sub New(Optional capacity As Integer = 32)
            Dim n As Integer = std.Max(1, capacity)

            Indices = New Integer(n - 1) {}
            Distances = New Double(n - 1) {}
            Count = 0
        End Sub

        ''' <summary>
        ''' 清空计数（保留已分配容量）。
        ''' </summary>
        Public Sub Clear()
            Count = 0
        End Sub

        ''' <summary>
        ''' 确保容量至少为 <paramref name="capacity"/>（只增不减）。
        ''' </summary>
        Public Sub EnsureCapacity(capacity As Integer)
            If Indices.Length >= capacity AndAlso Distances.Length >= capacity Then Return

            Dim n As Integer = std.Max(capacity, std.Max(1, Indices.Length * 2))

            If n <= Indices.Length Then n = capacity

            Array.Resize(Indices, n)
            Array.Resize(Distances, n)
        End Sub

        ''' <summary>
        ''' 追加一个邻域点（不排序）。
        ''' </summary>
        Public Sub Add(index As Integer, distanceSquared As Double)
            If Count >= Indices.Length Then EnsureCapacity(Count + 1)

            Indices(Count) = index
            Distances(Count) = distanceSquared
            Count += 1
        End Sub

        ''' <summary>
        ''' 按平方距离升序排序（只排前 <see cref="Count"/> 个元素）。
        ''' </summary>
        Public Sub SortByDistance()
            If Count < 2 Then Return

            Array.Sort(Distances, Indices, 0, Count)
        End Sub

        ''' <summary>
        ''' 拷贝出下标数组（长度恰为 <see cref="Count"/>）。
        ''' </summary>
        Public Function ToIndexArray() As Integer()
            Dim out(Count - 1) As Integer

            If Count > 0 Then Array.Copy(Indices, out, Count)

            Return out
        End Function

        ''' <summary>
        ''' 拷贝出平方距离数组（长度恰为 <see cref="Count"/>）。
        ''' </summary>
        Public Function ToDistanceArray() As Double()
            Dim out(Count - 1) As Double

            If Count > 0 Then Array.Copy(Distances, out, Count)

            Return out
        End Function

        ''' <summary>
        ''' 平均（欧氏）距离，空邻域返回 0。
        ''' </summary>
        Public Function MeanDistance() As Double
            If Count = 0 Then Return 0.0

            Dim sum As Double = 0.0

            For i As Integer = 0 To Count - 1
                sum += std.Sqrt(Distances(i))
            Next

            Return sum / Count
        End Function

        ''' <summary>
        ''' 最大（欧氏）距离，空邻域返回 0。
        ''' </summary>
        Public Function MaxDistance() As Double
            Dim max As Double = 0.0

            For i As Integer = 0 To Count - 1
                If Distances(i) > max Then max = Distances(i)
            Next

            Return std.Sqrt(max)
        End Function

        Public Overrides Function ToString() As String
            Return $"neighborhood[{Count}] mean={MeanDistance():G4}"
        End Function
    End Class

    ''' <summary>
    ''' 邻域查询门面：把 KD 树的查询接口与“可复用结果缓冲”组合起来。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这个类型存在的唯一目的是<b>避免逐点分配</b>。直接调用
    ''' <see cref="KdTree3D.Knn(Double, Double, Double, Integer)"/> 的便捷重载会为每个点
    ''' 新建两个数组；在法向估计、ISS、FPFH 这些“每点一次查询”的场景下，
    ''' 1e6 次查询就是 200 万次小数组分配。
    ''' </para>
    ''' <para>
    ''' 这里用 <see cref="ThreadLocal(Of T)"/> 池化结果容器：
    ''' 外层并行分块时每个线程拿到自己的缓冲，块内串行复用，
    ''' 既没有锁也没有跨线程共享。
    ''' </para>
    ''' <para>
    ''' <b>注意</b>：返回的 <see cref="Neighborhood"/> 实例由内部池持有，
    ''' 下一次查询会覆盖它的内容。需要保留结果时请立刻拷贝
    ''' （<see cref="Neighborhood.ToIndexArray"/> / <see cref="Neighborhood.ToDistanceArray"/>）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NeighborhoodQuery

        Private ReadOnly m_tree As KdTree3D
        Private ReadOnly m_pool As ThreadLocal(Of Neighborhood)

        Public Sub New(tree As KdTree3D, Optional initialCapacity As Integer = 64)
            If tree Is Nothing Then Throw New ArgumentNullException(NameOf(tree))

            m_tree = tree
            m_pool = New ThreadLocal(Of Neighborhood)(Function() New Neighborhood(initialCapacity))
        End Sub

        ''' <summary>
        ''' 被包装的 KD 树。
        ''' </summary>
        Public ReadOnly Property Tree As KdTree3D
            Get
                Return m_tree
            End Get
        End Property

        ''' <summary>
        ''' 取得当前线程的结果缓冲（已清空）。
        ''' </summary>
        Public Function Rent() As Neighborhood
            Dim buffer As Neighborhood = m_pool.Value

            Call buffer.Clear()

            Return buffer
        End Function

        ''' <summary>
        ''' 查询第 <paramref name="index"/> 个点的 k 近邻（不含自身，若自身在结果中会被排除）。
        ''' </summary>
        Public Function Knn(index As Integer, k As Integer) As Neighborhood
            Dim buffer As Neighborhood = Rent()

            Call m_tree.Knn(m_tree.X(index), m_tree.Y(index), m_tree.Z(index), k, buffer, index)

            Return buffer
        End Function

        ''' <summary>
        ''' 查询任意查询点的 k 近邻。
        ''' </summary>
        Public Function Knn(query As Vector3d, k As Integer) As Neighborhood
            Dim buffer As Neighborhood = Rent()

            Call m_tree.Knn(query.X, query.Y, query.Z, k, buffer, -1)

            Return buffer
        End Function

        ''' <summary>
        ''' 查询第 <paramref name="index"/> 个点的半径邻域。
        ''' </summary>
        Public Function Radius(index As Integer, searchRadius As Double, Optional maxCount As Integer = 0) As Neighborhood
            Dim buffer As Neighborhood = Rent()

            Call m_tree.RadiusSearch(m_tree.X(index), m_tree.Y(index), m_tree.Z(index), searchRadius, buffer, index, maxCount)

            Return buffer
        End Function

        ''' <summary>
        ''' 查询任意查询点的半径邻域。
        ''' </summary>
        Public Function Radius(query As Vector3d, searchRadius As Double, Optional maxCount As Integer = 0) As Neighborhood
            Dim buffer As Neighborhood = Rent()

            Call m_tree.RadiusSearch(query.X, query.Y, query.Z, searchRadius, buffer, -1, maxCount)

            Return buffer
        End Function

        ''' <summary>
        ''' 高斯权重：<c>w(i) = EXP(−d(i)² / (2σ²))</c>。
        ''' </summary>
        ''' <param name="distancesSquared">平方距离（<see cref="Neighborhood.Distances"/>）</param>
        ''' <param name="count">有效元素个数</param>
        ''' <param name="sigma">高斯核带宽（通常取邻域半径的 1/3 ~ 1/2）</param>
        ''' <param name="dst">权重输出缓冲</param>
        ''' <remarks>
        ''' 邻域加权是法向/曲率/MLS 的标准做法：远离中心的点对局部拟合的贡献应当衰减，
        ''' 否则边界处的法向会被另一侧表面“拉偏”。这里保持标量实现
        ''' （<c>Exp</c> 没有硬件向量指令），但避免在循环里分配。
        ''' </remarks>
        Public Shared Sub GaussianWeights(distancesSquared As Double(), count As Integer,
                                          sigma As Double, dst As Double())
            If distancesSquared Is Nothing Then Throw New ArgumentNullException(NameOf(distancesSquared))
            If dst Is Nothing Then Throw New ArgumentNullException(NameOf(dst))

            Dim inv As Double

            If sigma <= 0.0 Then
                inv = 0.0
            Else
                inv = -1.0 / (2.0 * sigma * sigma)
            End If

            For i As Integer = 0 To count - 1
                dst(i) = std.Exp(distancesSquared(i) * inv)
            Next
        End Sub

    End Class
End Namespace
