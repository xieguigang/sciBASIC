Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Applications

    ''' <summary>
    ''' 一个候选抓取位姿。
    ''' </summary>
    Public Class GraspPose

        ''' <summary>抓取点（吸盘/夹爪中心）。</summary>
        Public Property Position As Vector3d

        ''' <summary>末端执行器位姿（工具坐标系 → 世界）。</summary>
        Public Property Orientation As RigidTransform

        ''' <summary>综合评分（0~1，越大越好）。</summary>
        Public Property Score As Double

        ''' <summary>夹持宽度 / 吸盘尺寸。</summary>
        Public Property Width As Double

        ''' <summary>支撑面平整度（越小平坦）。</summary>
        Public Property Flatness As Double

        ''' <summary>可用覆盖比例（足迹内落在零件上的比例）。</summary>
        Public Property Coverage As Double

        Public Overrides Function ToString() As String
            Return $"grasp at {Position}: score={Score:P1}, coverage={Coverage:P1}, flatness={Flatness:G4}"
        End Function
    End Class

    ''' <summary>
    ''' 抓取规划结果。
    ''' </summary>
    Public Class GraspResult

        ''' <summary>候选抓取位姿（按评分降序）。</summary>
        Public Property Poses As List(Of GraspPose)

        ''' <summary>最佳抓取（无候选时为 <c>Nothing</c>）。</summary>
        Public ReadOnly Property Best As GraspPose
            Get
                If Poses Is Nothing OrElse Poses.Count = 0 Then Return Nothing

                Return Poses(0)
            End Get
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return If(Poses Is Nothing, 0, Poses.Count)
            End Get
        End Property

        Public ReadOnly Property Success As Boolean
            Get
                Return Count > 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Count} grasp candidates, best score={If(Best Is Nothing, 0.0, Best.Score):P1}"
        End Function
    End Class

    ''' <summary>
    ''' 抓取规划：形态学网格搜索 + 对跖点采样。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>场景</b>：船舶混料钢板这类“大而平的堆叠件”，机器人通常用<b>真空吸盘</b>
    ''' 从上方抓取。能否抓取取决于三件事：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>吸盘足迹内必须几乎全是零件表面（不能悬空太多）；</description></item>
    ''' <item><description>足迹内的高度必须足够平坦（倾斜/弯曲会导致漏气）；</description></item>
    ''' <item><description>附近不能有更高障碍挡住吸盘下降路径。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>算法（形态学网格搜索）</b>：把点云投影到 XY 平面栅格化得到占用图与高度图，
    ''' 再用“吸盘足迹”作为结构元素在栅格上滑动：足迹内的占用比例即覆盖率
    ''' （形态学腐蚀的连续版本），高度极差即平整度。用<b>积分图</b>
    ''' （summed-area table）把覆盖率查询降到 <c>O(1)</c>，
    ''' 于是整张图的搜索是 <c>O(栅格数)</c>。
    ''' </para>
    ''' <para>
    ''' <b>与 GraspNet 的差别</b>：GraspNet 直接从点云回归 6-DoF 位姿候选并做碰撞评分，
    ''' 能处理任意形状与任意接近方向；本方法只做“自上而下的平面抓取”，
    ''' 但对大面积板材的<b>安全区搜索</b>更精确（覆盖率/平整度都是可解释的物理量），
    ''' 且不需要训练。两者可以互补：GraspNet 给候选，本类给“可抓取区域”约束。
    ''' </para>
    ''' <para>
    ''' <b>对跖点采样</b>（<see cref="AntipodalPairs"/>）用于<b>平行夹爪</b>：
    ''' 找一对“法向相反、间距在夹爪行程内”的表面点，夹爪沿两点连线闭合即可夹紧。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class GraspPlanning

        Private Sub New()
        End Sub

        ''' <summary>抓取规划参数。</summary>
        Public Class GraspOptions

            ''' <summary>栅格分辨率；<c>≤ 0</c> 表示按点距自适应（2 倍）。</summary>
            Public Property Resolution As Double = 0.0

            ''' <summary>吸盘宽度（X 方向，与点云同单位）。</summary>
            Public Property GripperWidth As Double = 0.0

            ''' <summary>吸盘深度（Y 方向）。</summary>
            Public Property GripperDepth As Double = 0.0

            ''' <summary>允许的“悬空”比例（足迹内可以有多少比例不在零件上，默认 0.1）。</summary>
            Public Property OverhangTolerance As Double = 0.1

            ''' <summary>允许的高度极差（平整度容差）。</summary>
            Public Property FlatnessTolerance As Double = 0.0

            ''' <summary>最多输出多少个候选。</summary>
            Public Property MaxCandidates As Integer = 5

            ''' <summary>候选之间的最小间距（避免输出一堆相邻点）。</summary>
            Public Property MinSeparation As Double = 0.0

            ''' <summary>最低评分阈值（低于该值丢弃）。</summary>
            Public Property MinScore As Double = 0.3
        End Class

        ''' <summary>
        ''' 自上而下的平面抓取搜索（真空吸盘）。
        ''' </summary>
        Public Shared Function PlanTopDown(cloud As PointCloudBuffer, Optional options As GraspOptions = Nothing,
                                           Optional optionsCloudy As CloudyOptions = Nothing) As GraspResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As GraspOptions = If(options, New GraspOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            If cloud.Count = 0 Then Return New GraspResult With {.Poses = New List(Of GraspPose)()}

            Dim spacing As Double = cloud.EstimateAverageSpacing()
            Dim resolution As Double = If(opt.Resolution > 0.0, opt.Resolution, std.Max(spacing * 2.0, 1.0E-6))
            Dim width As Double = If(opt.GripperWidth > 0.0, opt.GripperWidth, resolution * 4.0)
            Dim depth As Double = If(opt.GripperDepth > 0.0, opt.GripperDepth, width)
            Dim flatnessTolerance As Double = If(opt.FlatnessTolerance > 0.0, opt.FlatnessTolerance, std.Max(spacing * 3.0, 1.0E-6))

            Dim box As Aabb3 = cloud.Bounds()
            Dim nx As Integer = std.Max(2, CInt(std.Ceiling(box.Size.X / resolution)) + 1)
            Dim ny As Integer = std.Max(2, CInt(std.Ceiling(box.Size.Y / resolution)) + 1)

            Dim occupied As Integer(,) = New Integer(nx - 1, ny - 1) {}
            Dim heightSum As Double(,) = New Double(nx - 1, ny - 1) {}
            Dim heightMin As Double(,) = New Double(nx - 1, ny - 1) {}
            Dim heightMax As Double(,) = New Double(nx - 1, ny - 1) {}

            For i As Integer = 0 To nx - 1
                For j As Integer = 0 To ny - 1
                    heightMin(i, j) = Double.MaxValue
                    heightMax(i, j) = Double.MinValue
                Next
            Next

            For p As Integer = 0 To cloud.Count - 1
                Dim i As Integer = CInt(std.Floor((cloud.X(p) - box.Min.X) / resolution))
                Dim j As Integer = CInt(std.Floor((cloud.Y(p) - box.Min.Y) / resolution))

                If i < 0 OrElse i >= nx OrElse j < 0 OrElse j >= ny Then Continue For

                occupied(i, j) += 1
                heightSum(i, j) += cloud.Z(p)
                heightMin(i, j) = std.Min(heightMin(i, j), cloud.Z(p))
                heightMax(i, j) = std.Max(heightMax(i, j), cloud.Z(p))
            Next

            ' 积分图（占用计数）
            Dim integral As Long(,) = New Long(nx, ny) {}

            For i As Integer = 0 To nx - 1
                For j As Integer = 0 To ny - 1
                    integral(i + 1, j + 1) = occupied(i, j) + integral(i, j + 1) + integral(i + 1, j) - integral(i, j)
                Next
            Next

            Dim cellsX As Integer = std.Max(1, CInt(std.Round(width / resolution)))
            Dim cellsY As Integer = std.Max(1, CInt(std.Round(depth / resolution)))
            Dim footprint As Integer = cellsX * cellsY
            Dim minOccupied As Double = footprint * (1.0 - std.Max(0.0, std.Min(1.0, opt.OverhangTolerance)))
            Dim candidates As New List(Of GraspPose)()

            For i As Integer = 0 To nx - cellsX
                For j As Integer = 0 To ny - cellsY
                    Dim count As Long = RectSum(integral, i, j, i + cellsX, j + cellsY)

                    If count < minOccupied Then Continue For

                    ' 平整度：足迹内高度极差
                    Dim lo As Double = Double.MaxValue
                    Dim hi As Double = Double.MinValue
                    Dim sumZ As Double = 0.0
                    Dim samples As Integer = 0

                    For a As Integer = i To i + cellsX - 1
                        For b As Integer = j To j + cellsY - 1
                            If occupied(a, b) = 0 Then Continue For

                            lo = std.Min(lo, heightMin(a, b))
                            hi = std.Max(hi, heightMax(a, b))
                            sumZ += heightSum(a, b) / occupied(a, b)
                            samples += 1
                        Next
                    Next

                    If samples = 0 Then Continue For

                    Dim flatness As Double = hi - lo
                    Dim coverage As Double = std.Min(1.0, count / footprint)
                    Dim score As Double = coverage * std.Exp(-flatness / flatnessTolerance)

                    If score < opt.MinScore Then Continue For

                    Dim cx As Double = box.Min.X + (i + cellsX * 0.5) * resolution
                    Dim cy As Double = box.Min.Y + (j + cellsY * 0.5) * resolution
                    Dim cz As Double = sumZ / samples
                    Dim position As New Vector3d(cx, cy, cz)

                    ' 姿态：工具 Z 轴指向 -Z（从上往下压），X 轴沿栅格 X
                    Dim rotation As Double()() = {
                        New Double() {1, 0, 0},
                        New Double() {0, -1, 0},
                        New Double() {0, 0, -1}
                    }

                    Call candidates.Add(New GraspPose With {
                        .Position = position,
                        .Orientation = New RigidTransform(rotation, position),
                        .Score = score,
                        .Width = width,
                        .Coverage = coverage,
                        .Flatness = flatness
                    })
                Next
            Next

            Call candidates.Sort(Function(x, y) y.Score.CompareTo(x.Score))

            ' 去重：保证候选之间至少相隔 MinSeparation
            Dim separation As Double = If(opt.MinSeparation > 0.0, opt.MinSeparation, width * 1.5)
            Dim selected As New List(Of GraspPose)()

            For Each candidate As GraspPose In candidates
                Dim tooClose As Boolean = False

                For Each accepted As GraspPose In selected
                    If (accepted.Position - candidate.Position).Length < separation Then
                        tooClose = True

                        Exit For
                    End If
                Next

                If tooClose Then Continue For

                Call selected.Add(candidate)

                If selected.Count >= std.Max(1, opt.MaxCandidates) Then Exit For
            Next

            Return New GraspResult With {.Poses = selected}
        End Function

        ''' <summary>
        ''' 对跖点采样（平行夹爪的候选夹持对）。
        ''' </summary>
        ''' <param name="cloud">输入点云（缺法向时自动估计）</param>
        ''' <param name="maxWidth">夹爪最大行程</param>
        ''' <param name="maxPairs">最多输出多少对</param>
        ''' <param name="options">并行选项</param>
        ''' <remarks>
        ''' 判据：两点的法向近似相反（<c>n_i·n_j &lt; −0.8</c>），
        ''' 距离在 <c>[0.2·maxWidth, maxWidth]</c> 之间，且两点连线与法向的夹角很小
        ''' （保证夹爪是“正面”夹住而不是斜插）。
        ''' </remarks>
        Public Shared Function AntipodalPairs(cloud As PointCloudBuffer, maxWidth As Double,
                                              Optional maxPairs As Integer = 200,
                                              Optional options As CloudyOptions = Nothing) As List(Of (a As Integer, b As Integer, width As Double))
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If maxWidth <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(maxWidth), "the gripper width should be positive!")

            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)

            Dim tree As New KdTree3D(source)
            Dim pairs As New List(Of (a As Integer, b As Integer, width As Double))()
            Dim buffer As New Neighborhood(128)

            For i As Integer = 0 To source.Count - 1
                Call tree.RadiusSearch(source.X(i), source.Y(i), source.Z(i), maxWidth, buffer, i)

                Dim normal As Vector3d = source.Normal(i)

                If normal.IsZero(1.0E-9) Then Continue For

                For k As Integer = 0 To buffer.Count - 1
                    Dim j As Integer = buffer.Indices(k)

                    If j <= i Then Continue For

                    Dim other As Vector3d = source.Normal(j)

                    If Vector3d.Dot(normal, other) > -0.8 Then Continue For

                    Dim width As Double = std.Sqrt(buffer.Distances(k))

                    If width < 0.2 * maxWidth Then Continue For

                    Dim direction As Vector3d = (source.Item(j) - source.Item(i)).Normalize()

                    If std.Abs(Vector3d.Dot(direction, normal)) < 0.8 Then Continue For

                    Call pairs.Add((i, j, width))
                Next
            Next

            Call pairs.Sort(Function(x, y) x.width.CompareTo(y.width))

            If pairs.Count > maxPairs Then Call pairs.RemoveRange(maxPairs, pairs.Count - maxPairs)

            Return pairs
        End Function

        ''' <summary>
        ''' 积分图矩形求和（半开区间 <c>[i0, i1) × [j0, j1)</c>）。
        ''' </summary>
        Private Shared Function RectSum(integral As Long(,), i0 As Integer, j0 As Integer,
                                        i1 As Integer, j1 As Integer) As Long
            Return integral(i1, j1) - integral(i0, j1) - integral(i1, j0) + integral(i0, j0)
        End Function

    End Class
End Namespace
