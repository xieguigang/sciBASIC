Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 点云容器：SoA(structure of arrays) 布局的坐标 + 可选属性。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么是 SoA</b>：整个库的批量内核（<see cref="SimdVector3"/>、<see cref="SimdReduce"/>、
    ''' <see cref="SimdIndexed"/>）都通过 <c>New Vector(Of Double)(array, offset)</c> 批量装载，
    ''' 这要求同一分量的相邻点连续。如果改成 <c>Point3D</c> 结构体数组，
    ''' 相邻点的 X 之间隔着 Y、Z 两个 double，属于跨步访问，向量化后吞吐反而下降 2~3 倍。
    ''' 工业扫描数据动辄百万点，这个差别直接决定流水线能不能实时。
    ''' </para>
    ''' <para>
    ''' <b>属性是可选的</b>：法向、曲率、强度、颜色、标签都可能是 <c>Nothing</c>，
    ''' 表示“尚未计算/该数据源不提供”。这样预处理阶段就不必为每个点云都分配
    ''' 5 组等长数组（1e6 点 × 8 字节 × 12 = 96MB 的纯浪费）。
    ''' 所有读取属性的算法都必须先检查 <see cref="HasNormals"/> 之类的标志。
    ''' </para>
    ''' <para>
    ''' <b>坐标数组的所有权</b>：<see cref="Create(Double(), Double(), Double())"/> 与构造函数
    ''' 直接持有传入的数组（零拷贝），调用方之后不应再修改它们；
    ''' 需要独立副本时用 <see cref="Clone"/> 或 <see cref="ToPoint3D"/>。
    ''' 这是刻意的设计：预处理链上每一步都是“整块替换”，多余的一次拷贝在大点云上很昂贵。
    ''' </para>
    ''' <para>
    ''' <b>与外部数据的互操作</b>：<see cref="From(IEnumerable(Of PointF3D))"/> 接受任意
    ''' <see cref="PointF3D"/> 序列（包括 Landscape 模块的 <c>Ply.PointCloud</c>），
    ''' <see cref="ToPoint3D"/> 则把结果交给成像/渲染模块。
    ''' 本库不承担任何文件 I/O。
    ''' </para>
    ''' </remarks>
    Public Class PointCloudBuffer

        Private ReadOnly m_x As Double()
        Private ReadOnly m_y As Double()
        Private ReadOnly m_z As Double()

        Private m_nx As Double()
        Private m_ny As Double()
        Private m_nz As Double()
        Private m_curvature As Double()
        Private m_intensity As Double()
        Private m_color As Integer()
        Private m_labels As Integer()

        ''' <summary>
        ''' 以三条坐标数组构造点云（不复制数据，接管数组所有权）。
        ''' </summary>
        Public Sub New(x As Double(), y As Double(), z As Double())
            If x Is Nothing OrElse y Is Nothing OrElse z Is Nothing Then
                Throw New ArgumentNullException(NameOf(x), "the coordinate arrays can not be NULL!")
            End If
            If y.Length <> x.Length OrElse z.Length <> x.Length Then
                Throw New ArgumentException($"the coordinate arrays should have the same length: {x.Length}/{y.Length}/{z.Length}!")
            End If

            m_x = x
            m_y = y
            m_z = z
        End Sub

        ''' <summary>
        ''' 点数量。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return m_x.Length
            End Get
        End Property

        Public ReadOnly Property X As Double()
            Get
                Return m_x
            End Get
        End Property

        Public ReadOnly Property Y As Double()
            Get
                Return m_y
            End Get
        End Property

        Public ReadOnly Property Z As Double()
            Get
                Return m_z
            End Get
        End Property

        ''' <summary>
        ''' 是否为空点云。
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Count = 0
            End Get
        End Property

#Region "attributes"

        ''' <summary>
        ''' 是否已计算法向。
        ''' </summary>
        Public ReadOnly Property HasNormals As Boolean
            Get
                Return m_nx IsNot Nothing AndAlso m_ny IsNot Nothing AndAlso m_nz IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' 法向 X 分量（未计算时为 <c>Nothing</c>）。
        ''' </summary>
        Public ReadOnly Property Nx As Double()
            Get
                Return m_nx
            End Get
        End Property

        Public ReadOnly Property Ny As Double()
            Get
                Return m_ny
            End Get
        End Property

        Public ReadOnly Property Nz As Double()
            Get
                Return m_nz
            End Get
        End Property

        ''' <summary>
        ''' 曲率（表面变化度）数组，未计算时为 <c>Nothing</c>。
        ''' </summary>
        Public Property Curvature As Double()
            Get
                Return m_curvature
            End Get
            Set(value As Double())
                Call CheckAttributeLength(value, NameOf(Curvature))
                m_curvature = value
            End Set
        End Property

        ''' <summary>
        ''' 强度/反射率（激光扫描仪常见通道），未提供时为 <c>Nothing</c>。
        ''' </summary>
        Public Property Intensity As Double()
            Get
                Return m_intensity
            End Get
            Set(value As Double())
                Call CheckAttributeLength(value, NameOf(Intensity))
                m_intensity = value
            End Set
        End Property

        ''' <summary>
        ''' ARGB 颜色（32 位整数），未提供时为 <c>Nothing</c>。
        ''' </summary>
        Public Property Color As Integer()
            Get
                Return m_color
            End Get
            Set(value As Integer())
                Call CheckAttributeLength(value, NameOf(Color))
                m_color = value
            End Set
        End Property

        ''' <summary>
        ''' 逐点标签：实例编号、语义类别或者分割簇号（<c>-1</c> 约定为噪声/未分类）。
        ''' </summary>
        Public Property Labels As Integer()
            Get
                Return m_labels
            End Get
            Set(value As Integer())
                Call CheckAttributeLength(value, NameOf(Labels))
                m_labels = value
            End Set
        End Property

        Private Sub CheckAttributeLength(value As Array, name As String)
            If value Is Nothing Then Return

            If value.Length <> Count Then
                Throw New ArgumentException($"the length of '{name}' ({value.Length}) should equal the point count ({Count})!", name)
            End If
        End Sub

        ''' <summary>
        ''' 分配并清零法向数组（后续由法向估计算法填充）。
        ''' </summary>
        Public Sub EnsureNormals()
            If HasNormals Then Return

            m_nx = New Double(Count - 1) {}
            m_ny = New Double(Count - 1) {}
            m_nz = New Double(Count - 1) {}
        End Sub

        ''' <summary>
        ''' 设置法向数组（接管所有权）。
        ''' </summary>
        Public Sub SetNormals(nx As Double(), ny As Double(), nz As Double())
            If nx Is Nothing OrElse ny Is Nothing OrElse nz Is Nothing Then
                Throw New ArgumentNullException(NameOf(nx), "the normal arrays can not be NULL!")
            End If

            Call CheckAttributeLength(nx, NameOf(nx))
            Call CheckAttributeLength(ny, NameOf(ny))
            Call CheckAttributeLength(nz, NameOf(nz))

            m_nx = nx
            m_ny = ny
            m_nz = nz
        End Sub

        ''' <summary>
        ''' 丢弃法向数组（点云被大幅裁剪/重采样之后，旧法向不再对应当前点集）。
        ''' </summary>
        Public Sub ClearNormals()
            m_nx = Nothing
            m_ny = Nothing
            m_nz = Nothing
        End Sub

        ''' <summary>
        ''' 是否带标签。
        ''' </summary>
        Public ReadOnly Property HasLabels As Boolean
            Get
                Return m_labels IsNot Nothing
            End Get
        End Property

#End Region

#Region "construct / convert"

        ''' <summary>
        ''' 分配一个未初始化坐标的空白点云（后续逐点填充）。
        ''' </summary>
        ''' <remarks>
        ''' 坐标数组用 <see cref="GC.AllocateUninitializedArray(Of T)(Integer, Boolean)"/> 分配，
        ''' 跳过运行时的大块清零：百万点的三组数组清零本身就是一次 24MB 的内存写，
        ''' 而调用方马上会把它们完整覆盖一遍。
        ''' </remarks>
        Public Shared Function NewCloud(count As Integer) As PointCloudBuffer
            If count < 0 Then Throw New ArgumentOutOfRangeException(NameOf(count), "the point count can not be negative!")

            If count = 0 Then Return New PointCloudBuffer(Array.Empty(Of Double)(), Array.Empty(Of Double)(), Array.Empty(Of Double)())

            Return New PointCloudBuffer(
                GC.AllocateUninitializedArray(Of Double)(count),
                GC.AllocateUninitializedArray(Of Double)(count),
                GC.AllocateUninitializedArray(Of Double)(count))
        End Function

        ''' <summary>
        ''' 以三条坐标数组构造（接管所有权，零拷贝）。
        ''' </summary>
        Public Shared Function Create(x As Double(), y As Double(), z As Double()) As PointCloudBuffer
            Return New PointCloudBuffer(x, y, z)
        End Function

        ''' <summary>
        ''' 由任意 <see cref="PointF3D"/> 序列构造（<c>Ply.PointCloud</c> 等外部数据源直接可用）。
        ''' </summary>
        ''' <remarks>
        ''' 实现 <see cref="IPointCloud"/> 的元素会顺带拷贝强度通道；
        ''' 颜色通道（字符串形式）不做解析——它属于数据的“表示层”，
        ''' 由调用方在需要时自行转换。
        ''' </remarks>
        Public Shared Function From(points As IEnumerable(Of PointF3D)) As PointCloudBuffer
            If points Is Nothing Then Throw New ArgumentNullException(NameOf(points))

            Dim list As New List(Of PointF3D)

            For Each p As PointF3D In points
                If p Is Nothing Then Continue For

                Call list.Add(p)
            Next

            Dim n As Integer = list.Count
            Dim cloud As PointCloudBuffer = NewCloud(n)

            If n = 0 Then Return cloud

            Dim hasIntensity As Boolean = False

            For i As Integer = 0 To n - 1
                Dim p As PointF3D = list(i)

                cloud.m_x(i) = p.X
                cloud.m_y(i) = p.Y
                cloud.m_z(i) = p.Z
                If TypeOf p Is IPointCloud Then hasIntensity = True
            Next

            If hasIntensity Then
                Dim intensity As Double() = New Double(n - 1) {}

                For i As Integer = 0 To n - 1
                    Dim p As PointF3D = list(i)

                    If TypeOf p Is IPointCloud Then
                        intensity(i) = DirectCast(p, IPointCloud).intensity
                    End If
                Next

                cloud.Intensity = intensity
            End If

            Return cloud
        End Function

        ''' <summary>
        ''' 由 <see cref="Vector3d"/> 序列构造。
        ''' </summary>
        Public Shared Function FromPoints(points As IEnumerable(Of Vector3d)) As PointCloudBuffer
            If points Is Nothing Then Throw New ArgumentNullException(NameOf(points))

            Dim list As New List(Of Vector3d)

            For Each p As Vector3d In points
                Call list.Add(p)
            Next

            Dim n As Integer = list.Count
            Dim cloud As PointCloudBuffer = NewCloud(n)

            For i As Integer = 0 To n - 1
                cloud.m_x(i) = list(i).X
                cloud.m_y(i) = list(i).Y
                cloud.m_z(i) = list(i).Z
            Next

            Return cloud
        End Function

        ''' <summary>
        ''' 取出第 <paramref name="index"/> 个点的坐标（逐点访问的便捷接口）。
        ''' </summary>
        Public Function Item(index As Integer) As Vector3d
            Return New Vector3d(m_x(index), m_y(index), m_z(index))
        End Function

        ''' <summary>
        ''' 取出第 <paramref name="index"/> 个点（成像模块的 <see cref="Point3D"/> 类型）。
        ''' </summary>
        Public Function Point(index As Integer) As Point3D
            Return New Point3D(m_x(index), m_y(index), m_z(index))
        End Function

        ''' <summary>
        ''' 取出第 <paramref name="index"/> 个点的法向（未计算时为零向量）。
        ''' </summary>
        Public Function Normal(index As Integer) As Vector3d
            If Not HasNormals Then Return Vector3d.Zero

            Return New Vector3d(m_nx(index), m_ny(index), m_nz(index))
        End Function

        ''' <summary>
        ''' 转为 <see cref="Vector3d"/> 数组。
        ''' </summary>
        Public Function ToVector3d() As Vector3d()
            Dim n As Integer = Count
            Dim out(n - 1) As Vector3d

            For i As Integer = 0 To n - 1
                out(i) = New Vector3d(m_x(i), m_y(i), m_z(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 转为成像模块的 <see cref="Point3D"/> 数组（交给渲染/导出模块）。
        ''' </summary>
        Public Function ToPoint3D() As Point3D()
            Dim n As Integer = Count
            Dim out(n - 1) As Point3D

            For i As Integer = 0 To n - 1
                out(i) = New Point3D(m_x(i), m_y(i), m_z(i))
            Next

            Return out
        End Function

#End Region

#Region "derive"

        ''' <summary>
        ''' 深拷贝（含全部可选属性）。
        ''' </summary>
        Public Function Clone() As PointCloudBuffer
            Dim n As Integer = Count
            Dim cloud As PointCloudBuffer = NewCloud(n)

            If n = 0 Then Return cloud

            Array.Copy(m_x, cloud.m_x, n)
            Array.Copy(m_y, cloud.m_y, n)
            Array.Copy(m_z, cloud.m_z, n)

            If HasNormals Then
                cloud.m_nx = New Double(n - 1) {}
                cloud.m_ny = New Double(n - 1) {}
                cloud.m_nz = New Double(n - 1) {}
                Array.Copy(m_nx, cloud.m_nx, n)
                Array.Copy(m_ny, cloud.m_ny, n)
                Array.Copy(m_nz, cloud.m_nz, n)
            End If

            If m_curvature IsNot Nothing Then cloud.m_curvature = DirectCast(m_curvature.Clone(), Double())
            If m_intensity IsNot Nothing Then cloud.m_intensity = DirectCast(m_intensity.Clone(), Double())
            If m_color IsNot Nothing Then cloud.m_color = DirectCast(m_color.Clone(), Integer())
            If m_labels IsNot Nothing Then cloud.m_labels = DirectCast(m_labels.Clone(), Integer())

            Return cloud
        End Function

        ''' <summary>
        ''' 按下标子集抽取新点云（保持给定顺序，重复下标会被重复抽取）。
        ''' </summary>
        ''' <remarks>
        ''' 走 <see cref="SimdIndexed.Gather3"/>：先把坐标压实到连续缓冲，
        ''' 保证后续算法面对的内存布局与原始云完全一致。
        ''' </remarks>
        Public Function Subset(indices As Integer()) As PointCloudBuffer
            If indices Is Nothing Then Throw New ArgumentNullException(NameOf(indices))

            Dim n As Integer = indices.Length
            Dim cloud As PointCloudBuffer = NewCloud(n)

            If n = 0 Then Return cloud

            Call SimdIndexed.Gather3(m_x, m_y, m_z, indices, 0, n, cloud.m_x, cloud.m_y, cloud.m_z)

            If HasNormals Then
                cloud.m_nx = GC.AllocateUninitializedArray(Of Double)(n)
                cloud.m_ny = GC.AllocateUninitializedArray(Of Double)(n)
                cloud.m_nz = GC.AllocateUninitializedArray(Of Double)(n)
                Call SimdIndexed.Gather3(m_nx, m_ny, m_nz, indices, 0, n, cloud.m_nx, cloud.m_ny, cloud.m_nz)
            End If

            If m_curvature IsNot Nothing Then cloud.m_curvature = GatherAttribute(m_curvature, indices)
            If m_intensity IsNot Nothing Then cloud.m_intensity = GatherAttribute(m_intensity, indices)
            If m_color IsNot Nothing Then cloud.m_color = GatherAttribute(m_color, indices)
            If m_labels IsNot Nothing Then cloud.m_labels = GatherAttribute(m_labels, indices)

            Return cloud
        End Function

        Private Shared Function GatherAttribute(Of T)(values As T(), indices As Integer()) As T()
            Dim out(indices.Length - 1) As T

            For i As Integer = 0 To indices.Length - 1
                out(i) = values(indices(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 按布尔掩码抽取（<c>True</c> 表示保留）。
        ''' </summary>
        Public Function Where(mask As Boolean()) As PointCloudBuffer
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))
            If mask.Length <> Count Then
                Throw New ArgumentException($"the mask length ({mask.Length}) should equal the point count ({Count})!", NameOf(mask))
            End If

            Dim kept As New List(Of Integer)(Count)

            For i As Integer = 0 To mask.Length - 1
                If mask(i) Then Call kept.Add(i)
            Next

            Return Subset(kept.ToArray())
        End Function

        ''' <summary>
        ''' 返回施加刚体变换后的新点云（原云不变）。
        ''' </summary>
        Public Function WithTransform(transform As RigidTransform) As PointCloudBuffer
            If transform Is Nothing Then Throw New ArgumentNullException(NameOf(transform))

            Return transform.ApplyToCloud(Me)
        End Function

        ''' <summary>
        ''' 就地施加刚体变换（坐标 + 法向）。
        ''' </summary>
        Public Sub TransformInPlace(transform As RigidTransform)
            If transform Is Nothing Then Throw New ArgumentNullException(NameOf(transform))

            Call transform.ApplyInPlace(Me)
        End Sub

        ''' <summary>
        ''' 合并多个点云（属性取交集语义：只有所有输入都有该属性时结果才带该属性）。
        ''' </summary>
        Public Shared Function Merge(ParamArray clouds As PointCloudBuffer()) As PointCloudBuffer
            If clouds Is Nothing OrElse clouds.Length = 0 Then
                Return New PointCloudBuffer(Array.Empty(Of Double)(), Array.Empty(Of Double)(), Array.Empty(Of Double)())
            End If

            Dim total As Integer = 0

            For Each cloud As PointCloudBuffer In clouds
                If cloud IsNot Nothing Then total += cloud.Count
            Next

            Dim merged As PointCloudBuffer = NewCloud(total)

            If total = 0 Then Return merged

            Dim allNormals As Boolean = True
            Dim allCurvature As Boolean = True
            Dim allIntensity As Boolean = True
            Dim allColor As Boolean = True
            Dim allLabels As Boolean = True

            For Each cloud As PointCloudBuffer In clouds
                If cloud Is Nothing Then Continue For

                allNormals = allNormals AndAlso cloud.HasNormals
                allCurvature = allCurvature AndAlso cloud.Curvature IsNot Nothing
                allIntensity = allIntensity AndAlso cloud.Intensity IsNot Nothing
                allColor = allColor AndAlso cloud.Color IsNot Nothing
                allLabels = allLabels AndAlso cloud.Labels IsNot Nothing
            Next

            If allNormals Then
                merged.m_nx = GC.AllocateUninitializedArray(Of Double)(total)
                merged.m_ny = GC.AllocateUninitializedArray(Of Double)(total)
                merged.m_nz = GC.AllocateUninitializedArray(Of Double)(total)
            End If
            If allCurvature Then merged.m_curvature = New Double(total - 1) {}
            If allIntensity Then merged.m_intensity = New Double(total - 1) {}
            If allColor Then merged.m_color = New Integer(total - 1) {}
            If allLabels Then merged.m_labels = New Integer(total - 1) {}

            Dim offset As Integer = 0

            For Each cloud As PointCloudBuffer In clouds
                If cloud Is Nothing OrElse cloud.Count = 0 Then Continue For

                Dim n As Integer = cloud.Count

                Array.Copy(cloud.m_x, 0, merged.m_x, offset, n)
                Array.Copy(cloud.m_y, 0, merged.m_y, offset, n)
                Array.Copy(cloud.m_z, 0, merged.m_z, offset, n)

                If allNormals Then
                    Array.Copy(cloud.m_nx, 0, merged.m_nx, offset, n)
                    Array.Copy(cloud.m_ny, 0, merged.m_ny, offset, n)
                    Array.Copy(cloud.m_nz, 0, merged.m_nz, offset, n)
                End If
                If allCurvature Then Array.Copy(cloud.m_curvature, 0, merged.m_curvature, offset, n)
                If allIntensity Then Array.Copy(cloud.m_intensity, 0, merged.m_intensity, offset, n)
                If allColor Then Array.Copy(cloud.m_color, 0, merged.m_color, offset, n)
                If allLabels Then Array.Copy(cloud.m_labels, 0, merged.m_labels, offset, n)

                offset += n
            Next

            Return merged
        End Function

#End Region

#Region "statistics"

        ''' <summary>
        ''' 轴对齐包围盒（走 SIMD 批量归约）。
        ''' </summary>
        Public Function Bounds() As Aabb3
            If Count = 0 Then Return Aabb3.Empty

            Dim box As Vector3Bounds = SimdVector3.Bounds3(m_x, m_y, m_z)

            Return New Aabb3(New Vector3d(box.Min.X, box.Min.Y, box.Min.Z),
                             New Vector3d(box.Max.X, box.Max.Y, box.Max.Z))
        End Function

        ''' <summary>
        ''' 质心（等权）。
        ''' </summary>
        Public Function Centroid() As Vector3d
            If Count = 0 Then Return Vector3d.Zero

            Return New Vector3d(SimdReduce.Mean(m_x), SimdReduce.Mean(m_y), SimdReduce.Mean(m_z))
        End Function

        ''' <summary>
        ''' 逐点平均间距的估计值（用于体素尺寸、邻域半径的默认值）。
        ''' </summary>
        ''' <remarks>
        ''' 用包围盒体积与点数反推：<c>(V/n)^(1/3)</c>。
        ''' 它不是严格的最近邻平均距离，但对于“给用户一个合理默认值”已经足够，
        ''' 而且不需要建索引（在预处理的最开始就要用到）。
        ''' </remarks>
        Public Function EstimateAverageSpacing() As Double
            Dim n As Integer = Count

            If n <= 1 Then Return 0.0

            Dim box As Aabb3 = Bounds()

            If box.IsEmpty Then Return 0.0

            Dim volume As Double = box.Volume

            If volume <= 0.0 Then
                Dim size As Vector3d = box.Size

                ' 共面（薄壁/板材）点云：体积为 0，用“面积 / 点数”推二维平均间距。
                ' 这一步很关键：否则平面点云会算出比真实点距小 2~3 个数量级的“间距”，
                ' 让所有以它为默认尺度的算法（体素、邻域、抓取栅格）全部退化。
                Dim area As Double = size.X * size.Y + size.Y * size.Z + size.X * size.Z

                If area > 0.0 Then Return std.Sqrt(area / n)

                ' 共线（单条扫描线）：用最长边上的平均分布
                Return box.Diagonal / n
            End If

            Return std.Pow(volume / n, 1.0 / 3.0)
        End Function

#End Region

        Public Overrides Function ToString() As String
            Dim flags As New List(Of String)

            If HasNormals Then Call flags.Add("normals")
            If m_curvature IsNot Nothing Then Call flags.Add("curvature")
            If m_intensity IsNot Nothing Then Call flags.Add("intensity")
            If m_color IsNot Nothing Then Call flags.Add("color")
            If m_labels IsNot Nothing Then Call flags.Add("labels")

            Dim attr As String = If(flags.Count = 0, "xyz", "xyz+" & String.Join("/", flags))

            If Count = 0 Then Return $"<empty point cloud> [{attr}]"

            Return $"{Count} points [{attr}] bounds={Bounds()}"
        End Function
    End Class
End Namespace
