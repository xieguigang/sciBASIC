Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 轴对齐包围盒(AABB)：KD 树节点裁剪、体素划分、裁剪滤波、TSDF 体积范围共用。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 使用轴对齐而不是有向包围盒(OBB)的理由：点云的局部包围盒绝大多数用于
    ''' “快速排除不可能相交的点/节点”，此时 AABB 的判定只需要 6 次比较，
    ''' 而 OBB 需要把点变换到局部坐标系（约 15 次乘加）；在 KD 树遍历这种
    ''' 每秒上百万次的路径上，这个差别是决定性的。
    ''' </para>
    ''' <para>
    ''' <see cref="Empty"/> 采用“最小值取 +∞、最大值取 −∞”的约定，
    ''' 因此对任意点集反复 <see cref="Expand(Vector3d)"/> 不需要先做初始化判断，
    ''' 这也是 <see cref="FromPoints(IEnumerable(Of Vector3d))"/> 的实现方式。
    ''' </para>
    ''' </remarks>
    Public Structure Aabb3

        ''' <summary>
        ''' 空包围盒：与任何包围盒合并后都等于后者。
        ''' </summary>
        Public Shared ReadOnly Empty As New Aabb3(
            New Vector3d(Double.MaxValue, Double.MaxValue, Double.MaxValue),
            New Vector3d(Double.MinValue, Double.MinValue, Double.MinValue))

        Public ReadOnly Min As Vector3d
        Public ReadOnly Max As Vector3d

        Public Sub New(min As Vector3d, max As Vector3d)
            Me.Min = min
            Me.Max = max
        End Sub

        Public Sub New(center As Vector3d, halfSize As Double)
            Me.Min = New Vector3d(center.X - halfSize, center.Y - halfSize, center.Z - halfSize)
            Me.Max = New Vector3d(center.X + halfSize, center.Y + halfSize, center.Z + halfSize)
        End Sub

        ''' <summary>
        ''' 是否为空包围盒（任一轴出现负尺寸）。
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Max.X < Min.X OrElse Max.Y < Min.Y OrElse Max.Z < Min.Z
            End Get
        End Property

        ''' <summary>
        ''' 三条轴上的尺寸。
        ''' </summary>
        Public ReadOnly Property Size As Vector3d
            Get
                Return Max - Min
            End Get
        End Property

        ''' <summary>
        ''' 包围盒中心。
        ''' </summary>
        Public ReadOnly Property Center As Vector3d
            Get
                Return New Vector3d((Min.X + Max.X) * 0.5, (Min.Y + Max.Y) * 0.5, (Min.Z + Max.Z) * 0.5)
            End Get
        End Property

        ''' <summary>
        ''' 对角线长度（体素尺寸与容差的常用参考尺度）。
        ''' </summary>
        Public ReadOnly Property Diagonal As Double
            Get
                Return Size.Length
            End Get
        End Property

        ''' <summary>
        ''' 包围盒体积（空盒为 0）。
        ''' </summary>
        Public ReadOnly Property Volume As Double
            Get
                If IsEmpty Then Return 0.0

                Dim s As Vector3d = Size

                Return s.X * s.Y * s.Z
            End Get
        End Property

        ''' <summary>
        ''' 最长轴的下标（0=X, 1=Y, 2=Z）：KD 树分割维选择、体素化的常用启发式。
        ''' </summary>
        Public ReadOnly Property LongestAxis As Integer
            Get
                Dim s As Vector3d = Size

                If s.X >= s.Y AndAlso s.X >= s.Z Then Return 0
                If s.Y >= s.Z Then Return 1

                Return 2
            End Get
        End Property

        ''' <summary>
        ''' 点到包围盒的最近距离（点在盒内时为 0）。
        ''' </summary>
        Public Function DistanceTo(p As Vector3d) As Double
            Dim dx As Double = std.Max(0.0, std.Max(Min.X - p.X, p.X - Max.X))
            Dim dy As Double = std.Max(0.0, std.Max(Min.Y - p.Y, p.Y - Max.Y))
            Dim dz As Double = std.Max(0.0, std.Max(Min.Z - p.Z, p.Z - Max.Z))

            Return std.Sqrt(dx * dx + dy * dy + dz * dz)
        End Function

        ''' <summary>
        ''' 点是否落在包围盒内（含 <paramref name="tolerance"/> 的外扩）。
        ''' </summary>
        Public Function Contains(p As Vector3d, Optional tolerance As Double = 0.0) As Boolean
            Return p.X >= Min.X - tolerance AndAlso p.X <= Max.X + tolerance AndAlso
                p.Y >= Min.Y - tolerance AndAlso p.Y <= Max.Y + tolerance AndAlso
                p.Z >= Min.Z - tolerance AndAlso p.Z <= Max.Z + tolerance
        End Function

        ''' <summary>
        ''' 将单点并入包围盒。
        ''' </summary>
        Public Function Expand(p As Vector3d) As Aabb3
            Return New Aabb3(Vector3d.Min(Min, p), Vector3d.Max(Max, p))
        End Function

        ''' <summary>
        ''' 四周外扩 <paramref name="margin"/>（TSDF 截断距离、碰撞检测安全距离）。
        ''' </summary>
        Public Function ExpandBy(margin As Double) As Aabb3
            Dim d As New Vector3d(margin, margin, margin)

            Return New Aabb3(Min - d, Max + d)
        End Function

        ''' <summary>
        ''' 与另一个包围盒的并集。
        ''' </summary>
        Public Function Union(other As Aabb3) As Aabb3
            Return New Aabb3(Vector3d.Min(Min, other.Min), Vector3d.Max(Max, other.Max))
        End Function

        ''' <summary>
        ''' 与另一个包围盒的交集（不相交时返回 <see cref="Empty"/>）。
        ''' </summary>
        Public Function Intersect(other As Aabb3) As Aabb3
            Dim lo As Vector3d = Vector3d.Max(Min, other.Min)
            Dim hi As Vector3d = Vector3d.Min(Max, other.Max)

            If hi.X < lo.X OrElse hi.Y < lo.Y OrElse hi.Z < lo.Z Then Return Empty

            Return New Aabb3(lo, hi)
        End Function

        ''' <summary>
        ''' 两个包围盒是否相交（含 <paramref name="tolerance"/> 的外扩）。
        ''' </summary>
        Public Function Overlaps(other As Aabb3, Optional tolerance As Double = 0.0) As Boolean
            Return Min.X - tolerance <= other.Max.X AndAlso Max.X + tolerance >= other.Min.X AndAlso
                Min.Y - tolerance <= other.Max.Y AndAlso Max.Y + tolerance >= other.Min.Y AndAlso
                Min.Z - tolerance <= other.Max.Z AndAlso Max.Z + tolerance >= other.Min.Z
        End Function

        ''' <summary>
        ''' 射线与包围盒求交（slab 法），返回是否命中以及参数区间
        ''' <c>[tMin, tMax]</c>（<c>origin + t·direction</c>）。
        ''' </summary>
        ''' <remarks>
        ''' 用于 BVH/KD 树的射线遍历、体素步进（TSDF 光线投射）与遮挡查询；
        ''' 方向分量为 0 时退化为“是否落在 slab 之间”的判定，
        ''' 这里用 <see cref="Double.MaxValue"/> 与 <see cref="Double.MinValue"/> 处理该分支，
        ''' 避免除零产生 NaN 后污染后续比较。
        ''' </remarks>
        Public Function RayIntersect(origin As Vector3d, direction As Vector3d,
                                     ByRef tMin As Double, ByRef tMax As Double) As Boolean
            Dim t0 As Double = 0.0
            Dim t1 As Double = Double.MaxValue

            If Not Slab(origin.X, direction.X, Min.X, Max.X, t0, t1) Then
                tMin = 0.0
                tMax = 0.0

                Return False
            End If
            If Not Slab(origin.Y, direction.Y, Min.Y, Max.Y, t0, t1) Then
                tMin = 0.0
                tMax = 0.0

                Return False
            End If
            If Not Slab(origin.Z, direction.Z, Min.Z, Max.Z, t0, t1) Then
                tMin = 0.0
                tMax = 0.0

                Return False
            End If

            tMin = t0
            tMax = t1

            Return True
        End Function

        Private Shared Function Slab(origin As Double, direction As Double, lo As Double, hi As Double,
                                     ByRef t0 As Double, ByRef t1 As Double) As Boolean
            If std.Abs(direction) < 1.0E-15 Then
                ' 平行于该 slab：只有起点位于 slab 内才可能相交
                Return origin >= lo AndAlso origin <= hi
            End If

            Dim inv As Double = 1.0 / direction
            Dim ta As Double = (lo - origin) * inv
            Dim tb As Double = (hi - origin) * inv

            If ta > tb Then
                Dim swap As Double = ta

                ta = tb
                tb = swap
            End If

            If ta > t0 Then t0 = ta
            If tb < t1 Then t1 = tb

            Return t0 <= t1
        End Function

        ''' <summary>
        ''' 八个角点（按 <c>min/max</c> 的二进制组合顺序：000,001,...,111）。
        ''' </summary>
        Public Function CornerPoints() As Vector3d()
            Return {
                New Vector3d(Min.X, Min.Y, Min.Z),
                New Vector3d(Min.X, Min.Y, Max.Z),
                New Vector3d(Min.X, Max.Y, Min.Z),
                New Vector3d(Min.X, Max.Y, Max.Z),
                New Vector3d(Max.X, Min.Y, Min.Z),
                New Vector3d(Max.X, Min.Y, Max.Z),
                New Vector3d(Max.X, Max.Y, Min.Z),
                New Vector3d(Max.X, Max.Y, Max.Z)
            }
        End Function

        ''' <summary>
        ''' 由点集构建包围盒（单趟扫描）。
        ''' </summary>
        Public Shared Function FromPoints(points As IEnumerable(Of Vector3d)) As Aabb3
            If points Is Nothing Then Throw New ArgumentNullException(NameOf(points))

            Dim box As Aabb3 = Empty

            For Each p As Vector3d In points
                box = box.Expand(p)
            Next

            Return box
        End Function

        ''' <summary>
        ''' 由点云构建包围盒（走 SIMD 批量归约，避免逐点结构体转换）。
        ''' </summary>
        Public Shared Function FromCloud(cloud As PointCloudBuffer) As Aabb3
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Return cloud.Bounds()
        End Function

        Public Overrides Function ToString() As String
            If IsEmpty Then Return "<empty AABB>"

            Return $"[{Min} .. {Max}] size={Size}"
        End Function
    End Structure
End Namespace
