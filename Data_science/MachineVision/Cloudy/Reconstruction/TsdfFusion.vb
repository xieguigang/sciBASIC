Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 相机内参（针孔模型）。
    ''' </summary>
    Public Structure CameraIntrinsics

        ''' <summary>X 方向焦距（像素）。</summary>
        Public ReadOnly Fx As Double
        ''' <summary>Y 方向焦距（像素）。</summary>
        Public ReadOnly Fy As Double
        ''' <summary>主点 X（像素）。</summary>
        Public ReadOnly Cx As Double
        ''' <summary>主点 Y（像素）。</summary>
        Public ReadOnly Cy As Double
        ''' <summary>图像宽度。</summary>
        Public ReadOnly Width As Integer
        ''' <summary>图像高度。</summary>
        Public ReadOnly Height As Integer

        Public Sub New(fx As Double, fy As Double, cx As Double, cy As Double, width As Integer, height As Integer)
            Me.Fx = fx
            Me.Fy = fy
            Me.Cx = cx
            Me.Cy = cy
            Me.Width = width
            Me.Height = height
        End Sub

        ''' <summary>由水平视场角与图像尺寸估算内参（工业相机的常见近似）。</summary>
        Public Shared Function FromFieldOfView(width As Integer, height As Integer, horizontalFovDegrees As Double) As CameraIntrinsics
            Dim fx As Double = (width * 0.5) / std.Tan(horizontalFovDegrees * std.PI / 360.0)

            Return New CameraIntrinsics(fx, fx, width * 0.5, height * 0.5, width, height)
        End Function
    End Structure

    ''' <summary>
    ''' 一帧深度数据 + 相机位姿。
    ''' </summary>
    Public Class DepthFrame

        ''' <summary>深度图（行优先，单位与点云一致；0 或 NaN 表示无效）。</summary>
        Public Property Depth As Double()()

        ''' <summary>相机内参。</summary>
        Public Property Intrinsics As CameraIntrinsics

        ''' <summary>相机在世界坐标系中的位姿（相机 → 世界）。</summary>
        Public Property Pose As RigidTransform

        Public Sub New(depth As Double()(), intrinsics As CameraIntrinsics, pose As RigidTransform)
            Me.Depth = depth
            Me.Intrinsics = intrinsics
            Me.Pose = pose
        End Sub
    End Class

    ''' <summary>
    ''' TSDF(Truncated Signed Distance Function)多帧融合重建。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>原理</b>：在一个固定的体素体积里，对每个体素维护
    ''' “到最近表面的有符号距离”（沿相机视线方向）与“观测权重”。
    ''' 每来一帧深度图，就把该帧的观测融合进去：
    ''' <c>TSDF_new = (w_old·TSDF_old + w·tsdf)/(w_old + w)</c>。
    ''' 融合完成后取 <c>TSDF = 0</c> 的等值面即得表面。
    ''' </para>
    ''' <para>
    ''' <b>为什么需要“截断”</b>：只有表面附近 <c>±τ</c> 范围内的距离是可靠的
    ''' （远处的射线可能穿过空洞）。截断后每个体素只被少数几帧有效更新，
    ''' 噪声被平均掉，同时保持表面锐利——这是 KinectFusion 系列的核心技巧。
    ''' </para>
    ''' <para>
    ''' <b>与泊松的差别</b>：TSDF 是<b>增量式、有界</b>的，适合“边扫描边重建”（SLAM）；
    ''' 泊松需要一次性拿到全部点云。TSDF 的分辨率受内存限制（1mm 分辨率的
    ''' 1m³ 体积就是 10⁹ 体素），因此工业上常配合“滑动体积”使用。
    ''' </para>
    ''' <para>
    ''' <b>本实现的取舍</b>：体素值支持 <c>Integrate</c> 增量更新，
    ''' 但一次调用 <see cref="Reconstruct(DepthFrame(), TsdfOptions, CloudyOptions)"/>
    ''' 只做“一次性融合”（先按所有帧的包围盒建体积）。
    ''' 需要实时滑动时请直接用 <see cref="TsdfVolume"/> 并自行管理体积平移。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class TsdfFusion

        Private Sub New()
        End Sub

        ''' <summary>TSDF 参数。</summary>
        Public Class TsdfOptions

            ''' <summary>体素边长（默认按场景尺度自动：包围盒对角线 / 128）。</summary>
            Public Property VoxelSize As Double = 0.0

            ''' <summary>截断距离（体素倍数，默认 4）。</summary>
            Public Property TruncationVoxels As Integer = 4

            ''' <summary>内存预算（MB）：超限时自动放大体素。</summary>
            Public Property MemoryBudgetMB As Integer = 1024

            ''' <summary>包围盒外扩（相对对角线比例，默认 0.05）。</summary>
            Public Property Padding As Double = 0.05
        End Class

        ''' <summary>
        ''' 融合重建（一次性）。
        ''' </summary>
        Public Shared Function Reconstruct(frames As DepthFrame(), Optional options As TsdfOptions = Nothing,
                                           Optional optionsCloudy As CloudyOptions = Nothing) As MeshData
            If frames Is Nothing Then Throw New ArgumentNullException(NameOf(frames))
            If frames.Length = 0 Then Throw New ArgumentException("at least one depth frame is required!", NameOf(frames))

            Dim opt As TsdfOptions = If(options, New TsdfOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            ' 由第一帧反投影出包围盒（逐帧取并集）
            Dim box As Aabb3 = Aabb3.Empty

            For Each frame As DepthFrame In frames
                Dim cloud As PointCloudBuffer = Unproject(frame)

                If cloud.Count > 0 Then box = box.Union(cloud.Bounds())
            Next

            If box.IsEmpty Then
                Throw New InvalidOperationException("all depth frames are empty!")
            End If

            box = box.ExpandBy(std.Max(box.Diagonal * std.Max(0.0, opt.Padding), 1.0E-6))

            Dim voxel As Double = If(opt.VoxelSize > 0.0, opt.VoxelSize, box.Diagonal / 128.0)
            Dim budget As Double = std.Max(64, opt.MemoryBudgetMB) * 1024.0 * 1024.0

            While True
                Dim size As Vector3d = box.Size
                Dim nx As Integer = CInt(std.Ceiling(size.X / voxel)) + 2
                Dim ny As Integer = CInt(std.Ceiling(size.Y / voxel)) + 2
                Dim nz As Integer = CInt(std.Ceiling(size.Z / voxel)) + 2
                Dim bytes As Double = 2.0 * 8.0 * nx * ny * nz

                If bytes <= budget Then Exit While

                voxel *= 1.25
            End While

            Dim volume As New TsdfVolume(box, voxel, opt.TruncationVoxels)

            For Each frame As DepthFrame In frames
                Call volume.Integrate(frame, global_)
            Next

            Return volume.ExtractMesh()
        End Function

        ''' <summary>
        ''' 把深度帧反投影为点云（相机坐标系 → 世界坐标系）。
        ''' </summary>
        Public Shared Function Unproject(frame As DepthFrame) As PointCloudBuffer
            If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))
            If frame.Depth Is Nothing Then Throw New ArgumentNullException(NameOf(frame), "the depth image is NULL!")

            Dim image As Double()() = frame.Depth
            Dim h As Integer = image.Length
            Dim w As Integer = If(h > 0, image(0).Length, 0)

            Dim xs As New List(Of Double)(w * h \ 4)
            Dim ys As New List(Of Double)(w * h \ 4)
            Dim zs As New List(Of Double)(w * h \ 4)
            Dim k As CameraIntrinsics = frame.Intrinsics
            Dim pose As RigidTransform = If(frame.Pose, RigidTransform.Identity)

            For v As Integer = 0 To h - 1
                For u As Integer = 0 To w - 1
                    Dim d As Double = image(v)(u)

                    If d <= 0.0 OrElse Double.IsNaN(d) Then Continue For

                    Dim x As Double = (u - k.Cx) * d / k.Fx
                    Dim y As Double = (v - k.Cy) * d / k.Fy
                    Dim p As Vector3d = pose.TransformPoint(New Vector3d(x, y, d))

                    Call xs.Add(p.X)
                    Call ys.Add(p.Y)
                    Call zs.Add(p.Z)
                Next
            Next

            Return PointCloudBuffer.Create(xs.ToArray(), ys.ToArray(), zs.ToArray())
        End Function

    End Class

    ''' <summary>
    ''' 稠密的 TSDF 体素体积（可增量融合）。
    ''' </summary>
    Public NotInheritable Class TsdfVolume

        Private ReadOnly m_bounds As Aabb3
        Private ReadOnly m_voxel As Double
        Private ReadOnly m_truncation As Double
        Private ReadOnly m_nx As Integer
        Private ReadOnly m_ny As Integer
        Private ReadOnly m_nz As Integer

        Private ReadOnly m_tsdf As Double()()()
        Private ReadOnly m_weight As Double()()()

        ''' <summary>已融合的帧数。</summary>
        Public Property IntegratedFrames As Integer

        Public Sub New(bounds As Aabb3, voxelSize As Double, Optional truncationVoxels As Integer = 4)
            If voxelSize <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(voxelSize), "the voxel size should be positive!")

            m_bounds = bounds
            m_voxel = voxelSize
            m_truncation = std.Max(1, truncationVoxels) * voxelSize

            Dim size As Vector3d = bounds.Size

            m_nx = CInt(std.Ceiling(size.X / voxelSize)) + 2
            m_ny = CInt(std.Ceiling(size.Y / voxelSize)) + 2
            m_nz = CInt(std.Ceiling(size.Z / voxelSize)) + 2

            m_tsdf = NewVolume(m_nx, m_ny, m_nz)
            m_weight = NewVolume(m_nx, m_ny, m_nz)
        End Sub

        ''' <summary>体素数量。</summary>
        Public ReadOnly Property VoxelCount As Integer
            Get
                Return m_nx * m_ny * m_nz
            End Get
        End Property

        ''' <summary>
        ''' 融合一帧深度数据。
        ''' </summary>
        ''' <param name="frame">深度帧</param>
        ''' <param name="options">并行选项</param>
        Public Sub Integrate(frame As DepthFrame, Optional options As CloudyOptions = Nothing)
            If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))
            If frame.Depth Is Nothing Then Throw New ArgumentNullException(NameOf(frame), "the depth image is NULL!")

            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)
            Dim image As Double()() = frame.Depth
            Dim k As CameraIntrinsics = frame.Intrinsics
            Dim pose As RigidTransform = If(frame.Pose, RigidTransform.Identity)
            Dim inverse As RigidTransform = pose.Inverse()

            ' 逐体素投影：外层按 i 分块并行（每个体素独立写入自己的格子）
            Call global_.ForRange(m_nx,
                Sub(start, ends)
                    For i As Integer = start To ends - 1
                        Dim x As Double = m_bounds.Min.X + i * m_voxel

                        For j As Integer = 0 To m_ny - 1
                            Dim y As Double = m_bounds.Min.Y + j * m_voxel

                            For kk As Integer = 0 To m_nz - 1
                                Dim z As Double = m_bounds.Min.Z + kk * m_voxel
                                Dim camera As Vector3d = inverse.TransformPoint(New Vector3d(x, y, z))

                                If camera.Z <= 1.0E-6 Then Continue For

                                Dim u As Integer = CInt(std.Round(camera.X * k.Fx / camera.Z + k.Cx))
                                Dim v As Integer = CInt(std.Round(camera.Y * k.Fy / camera.Z + k.Cy))

                                If u < 0 OrElse u >= k.Width OrElse v < 0 OrElse v >= k.Height Then Continue For
                                If v >= image.Length OrElse u >= image(v).Length Then Continue For

                                Dim measured As Double = image(v)(u)

                                If measured <= 0.0 OrElse Double.IsNaN(measured) Then Continue For

                                Dim sdf As Double = measured - camera.Z

                                If sdf < -m_truncation Then Continue For

                                Dim tsdf As Double = std.Max(-1.0, std.Min(1.0, sdf / m_truncation))
                                Dim w As Double = 1.0
                                Dim old As Double = m_tsdf(i)(j)(kk)

                                If m_weight(i)(j)(kk) <= 0.0 Then
                                    m_tsdf(i)(j)(kk) = tsdf
                                Else
                                    m_tsdf(i)(j)(kk) = (old + tsdf) * 0.5
                                End If

                                m_weight(i)(j)(kk) += w
                            Next
                        Next
                    Next
                End Sub, 8)

            IntegratedFrames += 1
        End Sub

        ''' <summary>
        ''' 提取 <c>TSDF = 0</c> 的等值面。
        ''' </summary>
        Public Function ExtractMesh() As MeshData
            ' 未观测的体素用 +1（外部）填充，避免在空白区域生成曲面
            Dim field As Double()()() = New Double(m_nx - 1)()() {}

            For i As Integer = 0 To m_nx - 1
                field(i) = New Double(m_ny - 1)() {}

                For j As Integer = 0 To m_ny - 1
                    field(i)(j) = New Double(m_nz - 1) {}

                    For k As Integer = 0 To m_nz - 1
                        field(i)(j)(k) = If(m_weight(i)(j)(k) > 0.0, m_tsdf(i)(j)(k), 1.0)
                    Next
                Next
            Next

            Return MarchingCubes.Extract(field, m_bounds.Min, New Vector3d(m_voxel, m_voxel, m_voxel), 0.0)
        End Function

        Private Shared Function NewVolume(nx As Integer, ny As Integer, nz As Integer) As Double()()()
            Dim volume As Double()()() = New Double(nx - 1)()() {}

            For i As Integer = 0 To nx - 1
                volume(i) = New Double(ny - 1)() {}

                For j As Integer = 0 To ny - 1
                    volume(i)(j) = New Double(nz - 1) {}
                Next
            Next

            Return volume
        End Function

    End Class
End Namespace
