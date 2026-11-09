Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 三维 KD 树：法向估计、描述子、ICP 对应点搜索、聚类邻域扩展共用。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>实现要点</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>
    ''' <description>
    ''' <b>扁平节点数组 + 置换下标</b>：树节点存放在并行数组中（分割维、分割值、左右孩子、叶子区间），
    ''' 点的顺序由一份置换下标 <c>indices</c> 表示。这样构建完成后没有指针、没有对象头，
    ''' 遍历时的内存局部性远好于“节点对象 + <c>List</c>”。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>选维启发式</b>：每个节点沿当前点集包围盒的<b>最长轴</b>分割（而不是轮换维度）。
    ''' 真实的工业点云往往极度各向异性（比如一块 2m×0.5m×0.01m 的钢板），
    ''' 最长轴分割能让树保持近似平衡，避免退化成长链。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>中位数分割 + 快速选择</b>：用 Hoare 分区在 <c>O(n)</c> 内把中位数放到正确位置，
    ''' 整棵树构建 <c>O(n·log n)</c>；相比之下“先排序再切分”会退化成 <c>O(n·log²n)</c>。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>叶子阈值</b>：叶子最多存 <c>leafSize</c> 个点（默认 8）。
    ''' 叶子里做线性扫描比继续分割更快（一次数组遍历 vs 多次随机跳转），
    ''' 也让节点数降到 <c>n/(leafSize/2)</c> 量级。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>剪枝</b>：kNN 用“当前第 k 近距离”作为阈值、半径搜索用半径作为阈值，
    ''' 只访问与查询点距离小于阈值的子树，平均复杂度 <c>O(log n + k)</c>。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' <b>线程安全</b>：构建完成后是只读结构，可以被多个线程同时查询；
    ''' 查询结果写入调用方提供的 <see cref="Neighborhood"/>，因此并行时每个线程必须有自己的结果容器
    ''' （见 <see cref="NeighborhoodQuery"/>）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class KdTree3D

        Private Structure BuildTask
            Public ReadOnly Node As Integer
            Public ReadOnly Start As Integer
            Public ReadOnly Ends As Integer

            Public Sub New(node As Integer, start As Integer, ends As Integer)
                Me.Node = node
                Me.Start = start
                Me.Ends = ends
            End Sub
        End Structure

        Private ReadOnly m_x As Double()
        Private ReadOnly m_y As Double()
        Private ReadOnly m_z As Double()

        Private ReadOnly m_indices As Integer()

        Private ReadOnly m_splitDim As Integer()
        Private ReadOnly m_splitValue As Double()
        Private ReadOnly m_left As Integer()
        Private ReadOnly m_right As Integer()
        Private ReadOnly m_start As Integer()
        Private ReadOnly m_count As Integer()
        Private ReadOnly m_nodes As Integer

        ' 每个节点的轴对齐包围盒。用它做剪枝比“用分割平面距离”更稳健：
        ' 即使快速选择在极端重复值下产生轻微的分割不一致，包围盒也总是由真实数据算出，
        ' 因而不会漏掉真正更近的点（代价是每节点多 6 个 double）。
        Private ReadOnly m_minX As Double()
        Private ReadOnly m_minY As Double()
        Private ReadOnly m_minZ As Double()
        Private ReadOnly m_maxX As Double()
        Private ReadOnly m_maxY As Double()
        Private ReadOnly m_maxZ As Double()

        ''' <summary>
        ''' 是否启用包围盒剪枝（默认 <c>True</c>；关闭后搜索退化为全树遍历，仅用于诊断）。
        ''' </summary>
        Public Shared Property EnablePruning As Boolean = True

        ''' <summary>
        ''' 由点云构建 KD 树（不复制坐标，只建立置换下标）。
        ''' </summary>
        ''' <param name="cloud">点云</param>
        ''' <param name="leafSize">叶子最多容纳的点数（默认 8）</param>
        Public Sub New(cloud As PointCloudBuffer, Optional leafSize As Integer = 8)
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            m_x = cloud.X
            m_y = cloud.Y
            m_z = cloud.Z

            Dim n As Integer = cloud.Count
            Dim leaf As Integer = std.Max(1, leafSize)

            If n = 0 Then
                m_indices = Array.Empty(Of Integer)()
                m_splitDim = Array.Empty(Of Integer)()
                m_splitValue = Array.Empty(Of Double)()
                m_left = Array.Empty(Of Integer)()
                m_right = Array.Empty(Of Integer)()
                m_start = Array.Empty(Of Integer)()
                m_count = Array.Empty(Of Integer)()
                m_minX = Array.Empty(Of Double)()
                m_minY = Array.Empty(Of Double)()
                m_minZ = Array.Empty(Of Double)()
                m_maxX = Array.Empty(Of Double)()
                m_maxY = Array.Empty(Of Double)()
                m_maxZ = Array.Empty(Of Double)()
                m_nodes = 0

                Return
            End If

            m_indices = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                m_indices(i) = i
            Next

            Dim maxNodes As Integer = std.Max(4, 2 * (n \ leaf + 1))

            m_splitDim = New Integer(maxNodes - 1) {}
            m_splitValue = New Double(maxNodes - 1) {}
            m_left = New Integer(maxNodes - 1) {}
            m_right = New Integer(maxNodes - 1) {}
            m_start = New Integer(maxNodes - 1) {}
            m_count = New Integer(maxNodes - 1) {}
            m_minX = New Double(maxNodes - 1) {}
            m_minY = New Double(maxNodes - 1) {}
            m_minZ = New Double(maxNodes - 1) {}
            m_maxX = New Double(maxNodes - 1) {}
            m_maxY = New Double(maxNodes - 1) {}
            m_maxZ = New Double(maxNodes - 1) {}

            m_nodes = 1
            m_left(0) = -1
            m_right(0) = -1
            m_start(0) = 0
            m_count(0) = n

            Dim stack As New Stack(Of BuildTask)()

            Call stack.Push(New BuildTask(0, 0, n))

            While stack.Count > 0
                Dim task As BuildTask = stack.Pop()
                Dim count As Integer = task.Ends - task.Start

                Call StoreBounds(task.Node, task.Start, task.Ends)

                If count <= leaf Then
                    m_left(task.Node) = -1

                    Continue While
                End If

                Dim axis As Integer = LongestAxis(task.Start, task.Ends)
                Dim median As Integer = task.Start + count \ 2

                Call NthElement(task.Start, task.Ends, median, axis)

                Dim splitValue As Double = Coordinate(m_indices(median), axis)
                Dim leftNode As Integer = m_nodes
                Dim rightNode As Integer = m_nodes + 1

                If rightNode >= m_left.Length Then
                    ' 理论上不会发生（2·(n/leaf+1) 是上界），保险起见退化为叶子
                    m_left(task.Node) = -1

                    Continue While
                End If

                m_nodes += 2
                m_splitDim(task.Node) = axis
                m_splitValue(task.Node) = splitValue
                m_left(task.Node) = leftNode
                m_right(task.Node) = rightNode
                m_start(task.Node) = task.Start
                m_count(task.Node) = count

                m_left(leftNode) = -1
                m_right(leftNode) = -1
                m_start(leftNode) = task.Start
                m_count(leftNode) = median - task.Start

                m_left(rightNode) = -1
                m_right(rightNode) = -1
                m_start(rightNode) = median
                m_count(rightNode) = task.Ends - median

                Call stack.Push(New BuildTask(leftNode, task.Start, median))
                Call stack.Push(New BuildTask(rightNode, median, task.Ends))
            End While
        End Sub

        ''' <summary>
        ''' 点数量。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return m_x.Length
            End Get
        End Property

        Public ReadOnly Property X(index As Integer) As Double
            Get
                Return m_x(index)
            End Get
        End Property

        Public ReadOnly Property Y(index As Integer) As Double
            Get
                Return m_y(index)
            End Get
        End Property

        Public ReadOnly Property Z(index As Integer) As Double
            Get
                Return m_z(index)
            End Get
        End Property

        ''' <summary>
        ''' 树的节点数量（诊断用）。
        ''' </summary>
        Public ReadOnly Property NodeCount As Integer
            Get
                Return m_nodes
            End Get
        End Property

#Region "build helpers"

        Private Function Coordinate(index As Integer, axis As Integer) As Double
            If axis = 0 Then Return m_x(index)
            If axis = 1 Then Return m_y(index)

            Return m_z(index)
        End Function

        ''' <summary>
        ''' 当前点集包围盒的最长轴。
        ''' </summary>
        ''' <remarks>
        ''' 用“扫描求极值”而不是“父节点最长轴的一半”是因为点云可能极度不均匀
        ''' （例如一半是地面点、一半是立杆点），每次重新计算能让树更平衡；
        ''' 总代价仍是 <c>O(n·log n)</c>。
        ''' </remarks>
        Private Function LongestAxis(start As Integer, ends As Integer) As Integer
            Dim minX As Double = Double.MaxValue, maxX As Double = Double.MinValue
            Dim minY As Double = Double.MaxValue, maxY As Double = Double.MinValue
            Dim minZ As Double = Double.MaxValue, maxZ As Double = Double.MinValue

            For i As Integer = start To ends - 1
                Dim p As Integer = m_indices(i)
                Dim px As Double = m_x(p), py As Double = m_y(p), pz As Double = m_z(p)

                If px < minX Then minX = px
                If px > maxX Then maxX = px
                If py < minY Then minY = py
                If py > maxY Then maxY = py
                If pz < minZ Then minZ = pz
                If pz > maxZ Then maxZ = pz
            Next

            Dim ex As Double = maxX - minX
            Dim ey As Double = maxY - minY
            Dim ez As Double = maxZ - minZ

            If ex >= ey AndAlso ex >= ez Then Return 0
            If ey >= ez Then Return 1

            Return 2
        End Function

        ''' <summary>
        ''' 计算并保存节点包围盒。
        ''' </summary>
        Private Sub StoreBounds(node As Integer, start As Integer, ends As Integer)
            Dim minX As Double = Double.MaxValue, maxX As Double = Double.MinValue
            Dim minY As Double = Double.MaxValue, maxY As Double = Double.MinValue
            Dim minZ As Double = Double.MaxValue, maxZ As Double = Double.MinValue

            For i As Integer = start To ends - 1
                Dim p As Integer = m_indices(i)
                Dim px As Double = m_x(p), py As Double = m_y(p), pz As Double = m_z(p)

                If px < minX Then minX = px
                If px > maxX Then maxX = px
                If py < minY Then minY = py
                If py > maxY Then maxY = py
                If pz < minZ Then minZ = pz
                If pz > maxZ Then maxZ = pz
            Next

            m_minX(node) = minX
            m_minY(node) = minY
            m_minZ(node) = minZ
            m_maxX(node) = maxX
            m_maxY(node) = maxY
            m_maxZ(node) = maxZ
        End Sub

        ''' <summary>
        ''' 查询点到节点包围盒的平方距离（点在盒内时为 0）：kNN/半径搜索的剪枝下界。
        ''' </summary>
        <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Function BoundsDistanceSquared(node As Integer, qx As Double, qy As Double, qz As Double) As Double
            Dim dx As Double = std.Max(0.0, std.Max(m_minX(node) - qx, qx - m_maxX(node)))
            Dim dy As Double = std.Max(0.0, std.Max(m_minY(node) - qy, qy - m_maxY(node)))
            Dim dz As Double = std.Max(0.0, std.Max(m_minZ(node) - qz, qz - m_maxZ(node)))

            Return dx * dx + dy * dy + dz * dz
        End Function

        ''' <summary>
        ''' 快速选择：把 <c>indices(nth)</c> 放到“按 <paramref name="axis"/> 排序后”的正确位置，
        ''' 并保证左侧元素都不大于它、右侧都不小于它。
        ''' </summary>
        Private Sub NthElement(start As Integer, ends As Integer, nth As Integer, axis As Integer)
            Dim lo As Integer = start
            Dim hi As Integer = ends - 1

            While lo < hi
                Dim pivot As Double = Coordinate(m_indices((lo + hi) \ 2), axis)
                Dim i As Integer = lo
                Dim j As Integer = hi

                Do While i <= j
                    Do While Coordinate(m_indices(i), axis) < pivot
                        i += 1
                    Loop
                    Do While Coordinate(m_indices(j), axis) > pivot
                        j -= 1
                    Loop

                    If i <= j Then
                        Dim swap As Integer = m_indices(i)

                        m_indices(i) = m_indices(j)
                        m_indices(j) = swap
                        i += 1
                        j -= 1
                    End If
                Loop

                If nth <= j Then
                    hi = j
                ElseIf nth >= i Then
                    lo = i
                Else
                    Exit While
                End If
            End While
        End Sub

#End Region

#Region "knn"

        ''' <summary>
        ''' k 近邻搜索（结果写入调用方缓冲，按平方距离升序）。
        ''' </summary>
        ''' <param name="qx">查询点 X</param>
        ''' <param name="qy">查询点 Y</param>
        ''' <param name="qz">查询点 Z</param>
        ''' <param name="k">期望的近邻个数</param>
        ''' <param name="result">结果缓冲（会被清空后填充）</param>
        ''' <param name="excludeIndex">需要排除的点下标（通常传查询点自身；不排除时传 -1）</param>
        Public Sub Knn(qx As Double, qy As Double, qz As Double, k As Integer,
                       result As Neighborhood, Optional excludeIndex As Integer = -1)
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))

            Call result.Clear()

            If k <= 0 OrElse Count = 0 Then Return

            Call result.EnsureCapacity(k)

            Call SearchKnn(0, qx, qy, qz, k, result, excludeIndex)
        End Sub

        ''' <summary>
        ''' k 近邻搜索（便捷重载，返回新数组，仅供非热路径使用）。
        ''' </summary>
        Public Function Knn(qx As Double, qy As Double, qz As Double, k As Integer) As Integer()
            Dim buffer As New Neighborhood(k)

            Call Knn(qx, qy, qz, k, buffer)

            Return buffer.ToIndexArray()
        End Function

        ''' <summary>
        ''' 最近邻搜索。
        ''' </summary>
        ''' <returns>最近点下标（空树返回 -1）与平方距离</returns>
        Public Function Nearest(qx As Double, qy As Double, qz As Double,
                                Optional excludeIndex As Integer = -1) As (index As Integer, distanceSquared As Double)
            Dim buffer As New Neighborhood(1)

            Call Knn(qx, qy, qz, 1, buffer, excludeIndex)

            If buffer.Count = 0 Then Return (-1, Double.MaxValue)

            Return (buffer.Indices(0), buffer.Distances(0))
        End Function

        Private Sub SearchKnn(node As Integer, qx As Double, qy As Double, qz As Double, k As Integer,
                              result As Neighborhood, excludeIndex As Integer)
            If node < 0 OrElse node >= m_nodes Then Return

            ' 包围盒剪枝：已找到 k 个点时，若整棵子树都在“当前第 k 近距离”之外，直接跳过
            If result.Count >= k AndAlso KdTree3D.EnablePruning Then
                If BoundsDistanceSquared(node, qx, qy, qz) >= result.Distances(result.Count - 1) Then Return
            End If

            Dim left As Integer = m_left(node)

            If left < 0 Then
                Dim start As Integer = m_start(node)
                Dim ends As Integer = start + m_count(node)

                For i As Integer = start To ends - 1
                    Dim p As Integer = m_indices(i)

                    If p = excludeIndex Then Continue For

                    Dim dx As Double = m_x(p) - qx
                    Dim dy As Double = m_y(p) - qy
                    Dim dz As Double = m_z(p) - qz
                    Dim d As Double = dx * dx + dy * dy + dz * dz

                    Call InsertCandidate(p, d, k, result)
                Next

                Return
            End If

            Dim axis As Integer = m_splitDim(node)
            Dim plane As Double = m_splitValue(node)
            Dim diff As Double

            If axis = 0 Then
                diff = qx - plane
            ElseIf axis = 1 Then
                diff = qy - plane
            Else
                diff = qz - plane
            End If

            Dim nearChild As Integer
            Dim farChild As Integer

            If diff <= 0.0 Then
                nearChild = left
                farChild = m_right(node)
            Else
                nearChild = m_right(node)
                farChild = left
            End If

            Call SearchKnn(nearChild, qx, qy, qz, k, result, excludeIndex)
            Call SearchKnn(farChild, qx, qy, qz, k, result, excludeIndex)
        End Sub

        ''' <summary>
        ''' 有序插入候选点：维持“升序 + 最多 k 个”的不变式。
        ''' </summary>
        ''' <remarks>
        ''' k 通常只有 10~50，插入排序的 <c>O(k)</c> 移位比堆的 <c>O(log k)</c> 更快
        ''' （常数小、连续内存、无分支预测失败），而且天然保持结果有序，
        ''' 省掉查询结束后的一次排序。
        ''' </remarks>
        Private Shared Sub InsertCandidate(index As Integer, distanceSquared As Double, k As Integer, result As Neighborhood)
            Dim count As Integer = result.Count

            If count >= k AndAlso distanceSquared >= result.Distances(count - 1) Then Return

            ' 只有“还没装满 k 个”时才需要扩容；装满之后是替换最后一个元素，
            ' 不需要额外空间（这一点写错会导致 kNN 永远只返回最先遇到的 k 个点）。
            If count < k Then
                Call result.EnsureCapacity(count + 1)

                ' 扩容失败（k 超过缓冲上限）时才放弃
                If count >= result.Indices.Length Then Return
            End If

            Dim pos As Integer = count

            While pos > 0 AndAlso result.Distances(pos - 1) > distanceSquared
                pos -= 1
            End While

            If count < k Then
                Dim tail As Integer = std.Min(count, k - 1)

                For i As Integer = tail To pos + 1 Step -1
                    result.Indices(i) = result.Indices(i - 1)
                    result.Distances(i) = result.Distances(i - 1)
                Next

                result.Indices(pos) = index
                result.Distances(pos) = distanceSquared
                result.Count = count + 1
            Else
                ' 已满：挤掉最后一个（它是当前最远的）
                For i As Integer = k - 1 To pos + 1 Step -1
                    result.Indices(i) = result.Indices(i - 1)
                    result.Distances(i) = result.Distances(i - 1)
                Next

                result.Indices(pos) = index
                result.Distances(pos) = distanceSquared
            End If
        End Sub

#End Region

#Region "radius search"

        ''' <summary>
        ''' 半径搜索（结果写入调用方缓冲，顺序未排序）。
        ''' </summary>
        ''' <param name="qx">查询点 X</param>
        ''' <param name="qy">查询点 Y</param>
        ''' <param name="qz">查询点 Z</param>
        ''' <param name="radius">搜索半径</param>
        ''' <param name="result">结果缓冲（会被清空后填充）</param>
        ''' <param name="excludeIndex">需要排除的点下标（通常传查询点自身；不排除时传 -1）</param>
        ''' <param name="maxCount">最多返回多少个点（<c>0</c> 表示不限制；用于密度自适应场景控制开销）</param>
        Public Sub RadiusSearch(qx As Double, qy As Double, qz As Double, radius As Double,
                                result As Neighborhood, Optional excludeIndex As Integer = -1,
                                Optional maxCount As Integer = 0)
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))

            Call result.Clear()

            If radius <= 0.0 OrElse Count = 0 Then Return

            Call SearchRadius(0, qx, qy, qz, radius * radius, result, excludeIndex, maxCount)
        End Sub

        ''' <summary>
        ''' 半径内的点数量（不做任何分配，用于半径滤波/密度估计）。
        ''' </summary>
        Public Function RadiusCount(qx As Double, qy As Double, qz As Double, radius As Double,
                                    Optional excludeIndex As Integer = -1) As Integer
            If radius <= 0.0 OrElse Count = 0 Then Return 0

            Dim buffer As New Neighborhood(64)

            Call RadiusSearch(qx, qy, qz, radius, buffer, excludeIndex)

            Return buffer.Count
        End Function

        Private Sub SearchRadius(node As Integer, qx As Double, qy As Double, qz As Double, radiusSquared As Double,
                                 result As Neighborhood, excludeIndex As Integer, maxCount As Integer)
            If node < 0 OrElse node >= m_nodes Then Return

            ' 包围盒剪枝：整棵子树都在半径之外时直接跳过
            If BoundsDistanceSquared(node, qx, qy, qz) > radiusSquared Then Return

            Dim left As Integer = m_left(node)

            If left < 0 Then
                Dim start As Integer = m_start(node)
                Dim ends As Integer = start + m_count(node)

                For i As Integer = start To ends - 1
                    Dim p As Integer = m_indices(i)

                    If p = excludeIndex Then Continue For

                    Dim dx As Double = m_x(p) - qx
                    Dim dy As Double = m_y(p) - qy
                    Dim dz As Double = m_z(p) - qz
                    Dim d As Double = dx * dx + dy * dy + dz * dz

                    If d <= radiusSquared Then
                        Call result.Add(p, d)

                        If maxCount > 0 AndAlso result.Count >= maxCount Then Return
                    End If
                Next

                Return
            End If

            Dim axis As Integer = m_splitDim(node)
            Dim plane As Double = m_splitValue(node)
            Dim diff As Double

            If axis = 0 Then
                diff = qx - plane
            ElseIf axis = 1 Then
                diff = qy - plane
            Else
                diff = qz - plane
            End If

            Dim nearChild As Integer
            Dim farChild As Integer

            If diff <= 0.0 Then
                nearChild = left
                farChild = m_right(node)
            Else
                nearChild = m_right(node)
                farChild = left
            End If

            Call SearchRadius(nearChild, qx, qy, qz, radiusSquared, result, excludeIndex, maxCount)
            Call SearchRadius(farChild, qx, qy, qz, radiusSquared, result, excludeIndex, maxCount)
        End Sub

#End Region

    End Class
End Namespace
