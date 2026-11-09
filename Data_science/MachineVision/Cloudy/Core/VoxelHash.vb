Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 稀疏体素哈希：把点云映射到规则网格，支持 O(1) 的体素定位与常数时间的邻域枚举。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 <see cref="KdTree3D"/> 的分工：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>KD 树</b>：任意半径的精确 kNN/半径查询，适合“每点的邻域”这类几何计算。</description></item>
    ''' <item><description><b>体素哈希</b>：按固定尺寸分桶，适合“全局重复性操作”——体素降采样、欧氏聚类、FEC、TSDF 融合、
    ''' 法向扩散。这类操作需要一个稳定的“单元编号”，而且希望遍历顺序与空间邻接一致。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>为什么不建稠密三维数组</b>：工业点云的空间占用率通常低于 1%
    ''' （扫描表面只是一层壳），稠密数组会浪费两个数量级的内存。
    ''' 这里用 <see cref="Dictionary(Of Long, List(Of Integer))"/> 只存储被占用的单元，
    ''' 单元键由三个 21 位整数打包成 <see cref="Long"/>（每轴约 ±100 万格，足够覆盖车间尺度）。
    ''' </para>
    ''' <para>
    ''' <b>哈希键的符号处理</b>：负坐标通过 <c>And &amp;H1FFFFF</c> 取低 21 位，
    ''' 因此 <c>−1</c> 与 <c>&amp;H1FFFFF</c> 落在同一个格，且不会与正坐标冲突
    ''' （在 21 位范围内是一一映射，超出范围的点云本身就不适合体素化）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class VoxelHash

        ''' <summary>每轴 21 位的掩码。</summary>
        Private Const AxisMask As Long = &H1FFFFFL
        Private Const AxisBits As Integer = 21

        Private ReadOnly m_cloud As PointCloudBuffer
        Private ReadOnly m_cellSize As Double
        Private ReadOnly m_invCell As Double
        Private ReadOnly m_buckets As Dictionary(Of Long, List(Of Integer))
        Private ReadOnly m_keys As Long()

        ''' <summary>
        ''' 以给点云与体素边长构建哈希。
        ''' </summary>
        ''' <param name="cloud">点云</param>
        ''' <param name="cellSize">体素边长（与点云同单位，通常取平均点距的 2~5 倍）</param>
        Public Sub New(cloud As PointCloudBuffer, cellSize As Double)
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If cellSize <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(cellSize), "the voxel size should be positive!")

            m_cloud = cloud
            m_cellSize = cellSize

            ' 用乘法代替除法：每点一次除法在 1e6 点上是可观测的开销
            m_invCell = 1.0 / cellSize
            m_keys = New Long(cloud.Count - 1) {}
            m_buckets = New Dictionary(Of Long, List(Of Integer))(std.Max(16, cloud.Count \ 4))

            For i As Integer = 0 To cloud.Count - 1
                Dim key As Long = KeyOf(cloud.X(i), cloud.Y(i), cloud.Z(i))
                Dim bucket As List(Of Integer) = Nothing

                m_keys(i) = key

                If Not m_buckets.TryGetValue(key, bucket) Then
                    bucket = New List(Of Integer)(8)
                    m_buckets(key) = bucket
                End If

                Call bucket.Add(i)
            Next
        End Sub

        ''' <summary>
        ''' 体素边长。
        ''' </summary>
        Public ReadOnly Property CellSize As Double
            Get
                Return m_cellSize
            End Get
        End Property

        ''' <summary>
        ''' 点数量。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return m_keys.Length
            End Get
        End Property

        ''' <summary>
        ''' 被占用的体素数量。
        ''' </summary>
        Public ReadOnly Property OccupiedCells As Integer
            Get
                Return m_buckets.Count
            End Get
        End Property

        ''' <summary>
        ''' 体素占用率（用于体素尺寸选型诊断：接近 1 说明体素过小、接近 0 说明体素过大）。
        ''' </summary>
        Public ReadOnly Property Occupancy As Double
            Get
                If Count = 0 Then Return 0.0

                Return CDbl(m_buckets.Count) / Count
            End Get
        End Property

        ''' <summary>
        ''' 单轴体素坐标。
        ''' </summary>
        Public Function AxisIndex(value As Double) As Integer
            Return CInt(std.Floor(value * m_invCell))
        End Function

        ''' <summary>
        ''' 把三维体素坐标打包成 64 位键。
        ''' </summary>
        Public Shared Function PackKey(ix As Integer, iy As Integer, iz As Integer) As Long
            Return ((CLng(ix) And AxisMask) << (AxisBits * 2)) Or
                ((CLng(iy) And AxisMask) << AxisBits) Or
                (CLng(iz) And AxisMask)
        End Function

        ''' <summary>
        ''' 坐标对应的体素键。
        ''' </summary>
        Public Function KeyOf(x As Double, y As Double, z As Double) As Long
            Return PackKey(AxisIndex(x), AxisIndex(y), AxisIndex(z))
        End Function

        ''' <summary>
        ''' 第 <paramref name="index"/> 个点所在的体素键（构建时已缓存）。
        ''' </summary>
        Public Function KeyAt(index As Integer) As Long
            Return m_keys(index)
        End Function

        ''' <summary>
        ''' 取出体素内的点下标列表（不存在时返回空列表）。
        ''' </summary>
        Public Function Bucket(key As Long) As List(Of Integer)
            Dim found As List(Of Integer) = Nothing

            If m_buckets.TryGetValue(key, found) Then Return found

            Return EmptyBucket
        End Function

        Private Shared ReadOnly EmptyBucket As New List(Of Integer)()
        ''' <summary>
        ''' 第 <paramref name="index"/> 个点所在体素的所有点。
        ''' </summary>
        Public Function BucketOf(index As Integer) As List(Of Integer)
            Return Bucket(m_keys(index))
        End Function

        ''' <summary>
        ''' 所有被占用的体素（键 → 点下标列表），遍历顺序即插入顺序。
        ''' </summary>
        Public Function AllCells() As IEnumerable(Of KeyValuePair(Of Long, List(Of Integer)))
            Return m_buckets
        End Function

        ''' <summary>
        ''' 按体素键升序排列的键列表（让聚类/融合的输出具有确定性）。
        ''' </summary>
        Public Function SortedKeys() As Long()
            Dim keys As Long() = New Long(m_buckets.Count - 1) {}

            Call m_buckets.Keys.CopyTo(keys, 0)
            Array.Sort(keys)

            Return keys
        End Function

        ''' <summary>
        ''' 体素内点的质心。
        ''' </summary>
        Public Function CellMean(key As Long) As Vector3d
            ' 注意：局部变量不能命名为 bucket —— VB 的作用域规则让“自身初始化表达式”
            ' 也落在该局部变量的作用域内，于是 Bucket(key) 会被解析成
            ' “对局部变量做默认属性索引”，而不是调用 Bucket 方法。
            Dim members As List(Of Integer) = Me.Bucket(key)
            Dim n As Integer = members.Count

            If n = 0 Then Return Vector3d.Zero

            Dim sx As Double = 0, sy As Double = 0, sz As Double = 0

            For i As Integer = 0 To n - 1
                Dim p As Integer = members(i)

                sx += m_cloud.X(p)
                sy += m_cloud.Y(p)
                sz += m_cloud.Z(p)
            Next

            Return New Vector3d(sx / n, sy / n, sz / n)
        End Function

        ''' <summary>
        ''' 与查询点在 <paramref name="radius"/> 内的所有点（体素预筛 + 精确距离判定）。
        ''' </summary>
        ''' <param name="qx">查询点 X</param>
        ''' <param name="qy">查询点 Y</param>
        ''' <param name="qz">查询点 Z</param>
        ''' <param name="radius">搜索半径</param>
        ''' <param name="result">结果列表（会被清空）</param>
        ''' <param name="excludeIndex">需要排除的点下标（不排除时传 -1）</param>
        ''' <remarks>
        ''' 体素搜索的范围是 <c>ceil(radius / cellSize)</c> 个格；
        ''' 当半径远大于体素尺寸时（例如欧氏聚类的连通半径），这个循环是
        ''' <c>(2r/c+1)³</c> 次哈希查找——所以在聚类场景里应当让 <c>cellSize ≈ radius</c>，
        ''' 从而退化为 27 次查找。
        ''' </remarks>
        Public Function NeighborsInRadius(qx As Double, qy As Double, qz As Double, radius As Double,
                                          result As List(Of Integer), Optional excludeIndex As Integer = -1) As Integer
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))

            Call result.Clear()

            If radius <= 0.0 OrElse Count = 0 Then Return 0

            Dim reach As Integer = CInt(std.Ceiling(radius * m_invCell))
            Dim cx As Integer = AxisIndex(qx)
            Dim cy As Integer = AxisIndex(qy)
            Dim cz As Integer = AxisIndex(qz)
            Dim radiusSquared As Double = radius * radius

            For ix As Integer = cx - reach To cx + reach
                For iy As Integer = cy - reach To cy + reach
                    For iz As Integer = cz - reach To cz + reach
                        Dim bucket As List(Of Integer) = Nothing

                        If Not m_buckets.TryGetValue(PackKey(ix, iy, iz), bucket) Then Continue For

                        For k As Integer = 0 To bucket.Count - 1
                            Dim p As Integer = bucket(k)

                            If p = excludeIndex Then Continue For

                            Dim dx As Double = m_cloud.X(p) - qx
                            Dim dy As Double = m_cloud.Y(p) - qy
                            Dim dz As Double = m_cloud.Z(p) - qz

                            If dx * dx + dy * dy + dz * dz <= radiusSquared Then
                                Call result.Add(p)
                            End If
                        Next
                    Next
                Next
            Next

            Return result.Count
        End Function

        ''' <summary>
        ''' 第 <paramref name="index"/> 个点的半径邻域。
        ''' </summary>
        Public Function NeighborsInRadius(index As Integer, radius As Double, result As List(Of Integer)) As Integer
            Return NeighborsInRadius(m_cloud.X(index), m_cloud.Y(index), m_cloud.Z(index), radius, result, index)
        End Function

        ''' <summary>
        ''' 26 邻域的体素键（不含自身）。
        ''' </summary>
        ''' <remarks>
        ''' 区域生长/连通性传播只需要“相邻体素”，不需要精确距离；
        ''' 预计算这 26 个偏移量可以避免在内层循环里做三次加法与打包。
        ''' 体素键的分段打包使得“相邻键”不能简单相加（会跨位溢出），
        ''' 因此这里返回的是偏移量而不是键增量。
        ''' </remarks>
        Public Shared Function NeighborOffsets() As (dx As Integer, dy As Integer, dz As Integer)()
            Dim offsets As New List(Of (dx As Integer, dy As Integer, dz As Integer))(26)

            For dx As Integer = -1 To 1
                For dy As Integer = -1 To 1
                    For dz As Integer = -1 To 1
                        If dx = 0 AndAlso dy = 0 AndAlso dz = 0 Then Continue For

                        Call offsets.Add((dx, dy, dz))
                    Next
                Next
            Next

            Return offsets.ToArray()
        End Function

        Public Overrides Function ToString() As String
            Return $"voxel hash: cell={m_cellSize:G4}, {Count} points in {m_buckets.Count} cells (occupancy={Occupancy:P1})"
        End Function
    End Class
End Namespace
