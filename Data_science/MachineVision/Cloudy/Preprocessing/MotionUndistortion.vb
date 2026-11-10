Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 带时间戳的位姿采样：IMU 姿态或里程计位姿的统一表示。
    ''' </summary>
    ''' <remarks>
    ''' 时间单位约定为<b>任意一致的秒</b>（扫描仪时间戳、IMU 时间戳必须已对齐到同一时钟）。
    ''' 位姿表示“传感器在该时刻于参考坐标系中的姿态”，即把传感器局部坐标变换到参考坐标系。
    ''' 纯 IMU 数据只有旋转（<see cref="Position"/> 为零），
    ''' 里程计/视觉里程计则两者都有。
    ''' </remarks>
    Public Structure TimedPose

        ''' <summary>时间戳（秒）。</summary>
        Public ReadOnly Time As Double

        ''' <summary>姿态四元数。</summary>
        Public ReadOnly Orientation As Quaternion

        ''' <summary>位置（纯 IMU 数据为零向量）。</summary>
        Public ReadOnly Position As Vector3d

        Public Sub New(time As Double, orientation As Quaternion, position As Vector3d)
            Me.Time = time
            Me.Orientation = orientation
            Me.Position = position
        End Sub

        ''' <summary>
        ''' 由 IMU 姿态构造（无平移）。
        ''' </summary>
        Public Shared Function FromImu(time As Double, orientation As Quaternion) As TimedPose
            Return New TimedPose(time, orientation, Vector3d.Zero)
        End Function

        ''' <summary>
        ''' 构造成刚体变换。
        ''' </summary>
        Public Function ToTransform() As RigidTransform
            Return RigidTransform.FromQuaternion(Orientation, Position)
        End Function

        Public Overrides Function ToString() As String
            Return $"t={Time:G6} {Orientation} {Position}"
        End Function
    End Structure

    ''' <summary>
    ''' 运动畸变补偿：把一帧内不同时刻采集的点统一到帧起始时刻的坐标系。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>问题</b>：机械式激光雷达旋转一周需要 0.1s（10Hz），
    ''' 期间传感器随机器人/扫描臂移动了几毫米到几厘米。
    ''' 如果不做补偿，同一个平面会被扫成“扭曲的螺旋面”，
    ''' 后续配准/测量会出现系统性偏差（而且这种偏差不会被 ICP 修正，
    ''' 因为它对整帧是一致的形变而不是刚体位姿差）。
    ''' </para>
    ''' <para>
    ''' <b>方法</b>：对每个点用它的采集时刻 <c>t(i)</c> 在位姿序列上插值出 <c>T(t(i))</c>，
    ''' 再相对帧起始时刻 <c>T(t0)</c> 求相对变换：
    ''' <c>p'(i) = T(t0)⁻¹ · T(t(i)) · T_ext · p(i)</c>。
    ''' </para>
    ''' <para>
    ''' <b>插值策略</b>：旋转用四元数球面插值（<see cref="Quaternion.Slerp"/>），
    ''' 平移用线性插值。IMU 频率（100~1000Hz）远高于扫描帧率，
    ''' 因此线性插值的误差远小于传感器噪声；若只有低帧率里程计（10~30Hz），
    ''' 建议先用 <see cref="Interpolate(TimedPose(), Double)"/> 生成稠密位姿再补偿。
    ''' </para>
    ''' <para>
    ''' <b>外参</b>：<c>T_ext</c> 是“扫描仪坐标系 → 位姿参考坐标系”的标定结果
    ''' （雷达-相机-IMU 外参）；省略时假设两者重合。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class MotionUndistortion

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 生成均匀时间戳（当扫描仪不提供逐点时间时，按扫描周期线性分布）。
        ''' </summary>
        ''' <param name="count">点数</param>
        ''' <param name="duration">整帧扫描时长（秒）</param>
        Public Shared Function EstimateUniformTimestamps(count As Integer, duration As Double) As Double()
            If count < 0 Then Throw New ArgumentOutOfRangeException(NameOf(count), "the point count can not be negative!")

            Dim times(count - 1) As Double
            Dim stepTime As Double = If(count > 1, duration / (count - 1), 0.0)

            For i As Integer = 0 To count - 1
                times(i) = i * stepTime
            Next

            Return times
        End Function

        ''' <summary>
        ''' 在位姿序列上插值出指定时刻的位姿（超出范围时取端点）。
        ''' </summary>
        ''' <param name="poses">按时间升序排列的位姿序列（至少 1 个）</param>
        ''' <param name="time">目标时刻</param>
        Public Shared Function Interpolate(poses As TimedPose(), time As Double) As TimedPose
            If poses Is Nothing OrElse poses.Length = 0 Then
                Throw New ArgumentException("at least one pose sample is required!", NameOf(poses))
            End If
            If poses.Length = 1 Then Return poses(0)

            If time <= poses(0).Time Then Return poses(0)

            Dim last As Integer = poses.Length - 1

            If time >= poses(last).Time Then Return poses(last)

            Dim index As Integer = FindSegment(poses, time)

            If index >= last Then Return poses(last)

            Dim a As TimedPose = poses(index)
            Dim b As TimedPose = poses(index + 1)
            Dim span As Double = b.Time - a.Time

            If span <= 0.0 Then Return b

            Dim t As Double = (time - a.Time) / span

            Return New TimedPose(time,
                                 Quaternion.Slerp(a.Orientation, b.Orientation, t),
                                 Vector3d.Lerp(a.Position, b.Position, t))
        End Function

        ''' <summary>
        ''' 二分查找：返回满足 <c>poses(i).Time ≤ time &lt; poses(i+1).Time</c> 的下标。
        ''' </summary>
        Private Shared Function FindSegment(poses As TimedPose(), time As Double) As Integer
            Dim lo As Integer = 0
            Dim hi As Integer = poses.Length - 1

            While lo < hi - 1
                Dim mid As Integer = (lo + hi) \ 2

                If poses(mid).Time <= time Then
                    lo = mid
                Else
                    hi = mid
                End If
            End While

            Return lo
        End Function

        ''' <summary>
        ''' 运动畸变补偿：把点云统一到帧起始时刻的坐标系。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="timestamps">逐点采集时刻（与点一一对应，已排序或与点顺序一致）</param>
        ''' <param name="poses">位姿序列（按时间升序）</param>
        ''' <param name="extrinsic">扫描仪坐标系到位姿参考坐标系的外参（可为 <c>Nothing</c>）</param>
        ''' <param name="options">并行选项</param>
        ''' <remarks>
        ''' 输出点云保留原有属性（强度/颜色/标签）；法向按相对旋转一并转动
        ''' （法向不参与平移，平移量对方向向量无意义）。
        ''' </remarks>
        Public Shared Function Undistort(cloud As PointCloudBuffer, timestamps As Double(), poses As TimedPose(),
                                         Optional extrinsic As RigidTransform = Nothing,
                                         Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If timestamps Is Nothing Then Throw New ArgumentNullException(NameOf(timestamps))
            If timestamps.Length <> cloud.Count Then
                Throw New ArgumentException($"the timestamp count ({timestamps.Length}) should equal the point count ({cloud.Count})!", NameOf(timestamps))
            End If
            If poses Is Nothing OrElse poses.Length = 0 Then
                Throw new ArgumentException("at least one pose sample is required!", NameOf(poses))
            End If

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            Dim out As PointCloudBuffer = cloud.Clone()

            If n = 0 Then Return out

            ' 帧起始时刻的位姿作为参考系
            Dim reference As RigidTransform = Interpolate(poses, timestamps(0)).ToTransform()
            Dim inverseRef As RigidTransform = reference.Inverse()

            Call opt.ForRange(n,
                Sub(start, ends)
                    For i As Integer = start To ends - 1
                        Dim p As Vector3d = cloud.Item(i)

                        If extrinsic IsNot Nothing Then p = extrinsic.TransformPoint(p)

                        Dim current As RigidTransform = Interpolate(poses, timestamps(i)).ToTransform()
                        Dim relative As RigidTransform = inverseRef.Compose(current)
                        Dim moved As Vector3d = relative.TransformPoint(p)

                        out.X(i) = moved.X
                        out.Y(i) = moved.Y
                        out.Z(i) = moved.Z

                        If cloud.HasNormals Then
                            Dim normal As Vector3d = relative.TransformDirection(cloud.Normal(i)).Normalize()

                            out.Nx(i) = normal.X
                            out.Ny(i) = normal.Y
                            out.Nz(i) = normal.Z
                        End If
                    Next
                End Sub, 1024)

            Return out
        End Function

        ''' <summary>
        ''' 用 IMU 姿态序列补偿（无平移分量，适用于静止底座 + 旋转扫描）。
        ''' </summary>
        Public Shared Function UndistortByImu(cloud As PointCloudBuffer, timestamps As Double(),
                                              imuTimes As Double(), imuOrientations As Quaternion(),
                                              Optional extrinsic As RigidTransform = Nothing,
                                              Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            If imuTimes Is Nothing Then Throw New ArgumentNullException(NameOf(imuTimes))
            If imuOrientations Is Nothing Then Throw New ArgumentNullException(NameOf(imuOrientations))
            If imuTimes.Length <> imuOrientations.Length Then
                Throw New ArgumentException($"the imu sample count not agree: {imuTimes.Length} vs {imuOrientations.Length}!")
            End If

            Dim poses(imuTimes.Length - 1) As TimedPose

            For i As Integer = 0 To imuTimes.Length - 1
                poses(i) = TimedPose.FromImu(imuTimes(i), imuOrientations(i))
            Next

            Return Undistort(cloud, timestamps, poses, extrinsic, options)
        End Function

    End Class
End Namespace
