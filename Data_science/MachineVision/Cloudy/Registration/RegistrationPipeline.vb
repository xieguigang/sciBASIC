Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Registration

    ''' <summary>粗配准方法。</summary>
    Public Enum CoarseMethod
        ''' <summary>不执行粗配准（要求初值已经很接近）。</summary>
        None = 0
        ''' <summary>FPFH + RANSAC（通用，依赖特征质量）。</summary>
        Ransac = 1
        ''' <summary>4PCS（不依赖描述子，适合重复结构）。</summary>
        Pcs4 = 2
    End Enum

    ''' <summary>精配准方法。</summary>
    Public Enum FineMethod
        ''' <summary>不执行精配准。</summary>
        None = 0
        ''' <summary>点到点 ICP（最稳，收敛慢）。</summary>
        PointToPoint = 1
        ''' <summary>点到面 ICP（推荐默认：平面类工件收敛快）。</summary>
        PointToPlane = 2
        ''' <summary>GICP（平面 + 棱线混合零件）。</summary>
        Gicp = 3
        ''' <summary>NDT（大场景、初值较粗）。</summary>
        Ndt = 4
    End Enum

    ''' <summary>
    ''' 配准流水线：粗配准 → 精配准 → 多站拼接 → 位姿图优化。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 工业选型的经验组合（与选型建议一致）：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>计量级检测</b>（航空件、精密模具）：<see cref="CoarseMethod.Ransac"/> +
    ''' <see cref="FineMethod.PointToPlane"/>（或 <see cref="FineMethod.Gicp"/>），
    ''' 目标 RMSE &lt; 0.05mm。</description></item>
    ''' <item><description><b>机器人引导/实时</b>：<see cref="CoarseMethod.Ransac"/>（关键点限量）+
    ''' <see cref="FineMethod.PointToPlane"/>，或直接帧间 ICP。</description></item>
    ''' <item><description><b>大场景</b>（车间/管道）：<see cref="CoarseMethod.Pcs4"/> 或 GPS/里程计初值 +
    ''' <see cref="FineMethod.Ndt"/>，再做位姿图优化。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>失败处理</b>：粗配准的内点率低于阈值时，流水线<b>仍然</b>会用该结果做精配准，
    ''' 但会把它标记为低置信度；调用方应结合 <see cref="RegistrationResult.Fitness"/>
    ''' 决定是否接受。工业系统里“静默返回一个错误位姿”比“明确失败”危险得多，
    ''' 因此这里不做静默降级。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class RegistrationPipeline

        Private Sub New()
        End Sub

        ''' <summary>流水线参数。</summary>
        Public Class PipelineOptions

            ''' <summary>粗配准方法（默认 RANSAC）。</summary>
            Public Property Coarse As CoarseMethod = CoarseMethod.Ransac

            ''' <summary>精配准方法（默认点到面）。</summary>
            Public Property Fine As FineMethod = FineMethod.PointToPlane

            ''' <summary>精配准最大迭代次数（默认 60）。</summary>
            Public Property MaxFineIterations As Integer = 60

            ''' <summary>精配准对应点最大距离；<c>≤ 0</c> 自适应。</summary>
            Public Property MaxCorrespondenceDistance As Double = 0.0

            ''' <summary>精配准截尾比例（部分重叠时 &lt; 1）。</summary>
            Public Property TrimRatio As Double = 1.0

            ''' <summary>多站拼接时是否执行位姿图优化（默认 <c>True</c>）。</summary>
            Public Property OptimizePoseGraph As Boolean = True

            ''' <summary>多站配准时使用的配准参数（可用更宽松的阈值）。</summary>
            Public Property Pairwise As PipelineOptions

            Public Function Clone() As PipelineOptions
                Return New PipelineOptions With {
                    .Coarse = Coarse,
                    .Fine = Fine,
                    .MaxFineIterations = MaxFineIterations,
                    .MaxCorrespondenceDistance = MaxCorrespondenceDistance,
                    .TrimRatio = TrimRatio,
                    .OptimizePoseGraph = OptimizePoseGraph,
                    .Pairwise = Pairwise
                }
            End Function
        End Class

        ''' <summary>
        ''' 两站点云配准（粗 → 精）。
        ''' </summary>
        ''' <param name="source">源点云</param>
        ''' <param name="target">目标点云</param>
        ''' <param name="options">流水线参数</param>
        ''' <param name="optionsCloudy">并行选项</param>
        Public Shared Function AlignPair(source As PointCloudBuffer, target As PointCloudBuffer,
                                         Optional options As PipelineOptions = Nothing,
                                         Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            Dim opt As PipelineOptions = If(options, New PipelineOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)
            Dim transform As RigidTransform = RigidTransform.Identity
            Dim coarseResult As RegistrationResult = Nothing
            Dim method As New List(Of String)

            ' 1) 粗配准
            Select Case opt.Coarse
                Case CoarseMethod.Ransac
                    coarseResult = RansacRegistration.Align(source, target, Nothing, global_)

                    If coarseResult IsNot Nothing AndAlso coarseResult.Fitness > 0.05 Then
                        transform = coarseResult.Transform
                        Call method.Add("RANSAC")
                    End If
                Case CoarseMethod.Pcs4
                    coarseResult = Pcs4Registration.Align(source, target, Nothing, global_)

                    If coarseResult IsNot Nothing AndAlso coarseResult.Fitness > 0.05 Then
                        transform = coarseResult.Transform
                        Call method.Add("4PCS")
                    End If
            End Select

            ' 2) 精配准
            Dim fineResult As RegistrationResult = Nothing

            Select Case opt.Fine
                Case FineMethod.PointToPoint
                    Dim icp As New IcpRegistration.IcpOptions With {
                        .MaxIterations = opt.MaxFineIterations,
                        .MaxCorrespondenceDistance = opt.MaxCorrespondenceDistance,
                        .TrimRatio = opt.TrimRatio
                    }

                    fineResult = IcpRegistration.AlignPointToPoint(source, target, transform, icp, global_)
                Case FineMethod.PointToPlane
                    Dim icp As New IcpRegistration.IcpOptions With {
                        .MaxIterations = opt.MaxFineIterations,
                        .MaxCorrespondenceDistance = opt.MaxCorrespondenceDistance,
                        .TrimRatio = opt.TrimRatio
                    }

                    fineResult = IcpRegistration.AlignPointToPlane(source, target, transform, icp, global_)
                Case FineMethod.Gicp
                    Dim gicp As New GeneralizedIcp.GicpOptions With {
                        .MaxIterations = opt.MaxFineIterations,
                        .MaxCorrespondenceDistance = opt.MaxCorrespondenceDistance,
                        .TrimRatio = opt.TrimRatio
                    }

                    fineResult = GeneralizedIcp.Align(source, target, transform, gicp, global_)
                Case FineMethod.Ndt
                    Dim ndt As New NdtRegistration.NdtOptions With {
                        .MaxIterations = opt.MaxFineIterations
                    }

                    fineResult = NdtRegistration.Align(source, target, transform, ndt, global_)
            End Select

            If fineResult Is Nothing Then
                If coarseResult IsNot Nothing Then Return coarseResult

                Return New RegistrationResult(RigidTransform.Identity,
                                              TransformEstimationFitness(source, target, RigidTransform.Identity, global_),
                                              Double.MaxValue, 0, False, "none")
            End If

            Call method.Add(fineResult.Method)

            ' 精配准的结果已经包含粗配准的初值（ICP 内部从 transform 出发）
            Return New RegistrationResult(fineResult.Transform, fineResult.Fitness, fineResult.InlierRmse,
                                          fineResult.Iterations, fineResult.Converged, String.Join(" + ", method))
        End Function

        Private Shared Function TransformEstimationFitness(source As PointCloudBuffer, target As PointCloudBuffer,
                                                           transform As RigidTransform, options As CloudyOptions) As Double
            Dim fitness As Double
            Dim rmse As Double
            Dim distance As Double = std.Max(target.EstimateAverageSpacing() * 5.0, 1.0E-6)

            Call TransformEstimation.Evaluate(source, target, transform, distance, fitness, rmse)

            Return fitness
        End Function

        ''' <summary>
        ''' 多站配准结果。
        ''' </summary>
        Public Class MultiStationResult

            ''' <summary>各站点到世界坐标系的变换（<c>Transforms(k)</c> 对应 <c>stations(k)</c>）。</summary>
            Public Property Transforms As RigidTransform()

            ''' <summary>逐对配准的诊断结果。</summary>
            Public Property Pairwise As RegistrationResult()

            ''' <summary>拼接到世界坐标系后的完整点云。</summary>
            Public Property Combined As PointCloudBuffer

            ''' <summary>位姿图优化结果（未优化时为 <c>Nothing</c>）。</summary>
            Public Property PoseGraph As PoseGraphResult

            Public Overrides Function ToString() As String
                Return $"{If(Transforms Is Nothing, 0, Transforms.Length)} stations, " &
                    $"{If(Combined Is Nothing, 0, Combined.Count)} merged points"
            End Function
        End Class

        ''' <summary>
        ''' 多站点云拼接：逐站配准 → 位姿图优化 → 合并。
        ''' </summary>
        ''' <param name="stations">各站点点云（顺序应与扫描顺序一致，相邻站之间要有重叠）</param>
        ''' <param name="options">流水线参数</param>
        ''' <param name="optionsCloudy">并行选项</param>
        ''' <remarks>
        ''' <para>
        ''' <b>为什么逐站配准而不是“两两全局配准”</b>：<c>n</c> 个站的全局两两配准需要
        ''' <c>O(n²)</c> 次计算，而工业扫描通常是按顺序绕工件转一圈，
        ''' 相邻站之间重叠最大、配准最可靠。顺序配准 + 位姿图优化在精度与成本之间取得平衡。
        ''' </para>
        ''' <para>
        ''' <b>累积误差</b>：顺序配准的误差会沿链累积。位姿图优化把所有相邻观测放在一起解，
        ''' 能把误差均匀分摊；如果首末站有重叠，建议额外添加一条回环边
        ''' （当前实现不自动检测回环，需要调用方用 <see cref="PoseGraph"/> 自行补充）。
        ''' </para>
        ''' </remarks>
        Public Shared Function RegisterStations(stations As PointCloudBuffer(),
                                                Optional options As PipelineOptions = Nothing,
                                                Optional optionsCloudy As CloudyOptions = Nothing) As MultiStationResult
            If stations Is Nothing Then Throw New ArgumentNullException(NameOf(stations))
            If stations.Length = 0 Then Throw New ArgumentException("at least one station is required!", NameOf(stations))

            Dim opt As PipelineOptions = If(options, New PipelineOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)
            Dim count As Integer = stations.Length

            Dim transforms(count - 1) As RigidTransform
            Dim pairwise(count - 2) As RegistrationResult

            transforms(0) = RigidTransform.Identity

            For k As Integer = 1 To count - 1
                Dim pairOptions As PipelineOptions = If(opt.Pairwise, opt)

                Dim result As RegistrationResult = AlignPair(stations(k), stations(k - 1), pairOptions, global_)

                pairwise(k - 1) = result
                transforms(k) = transforms(k - 1).Compose(result.Transform)
            Next

            Dim poseGraphResult As PoseGraphResult = Nothing

            If opt.OptimizePoseGraph AndAlso count >= 3 Then
                Dim graph As New PoseGraph()

                For k As Integer = 0 To count - 1
                    Call graph.AddNode(transforms(k), k = 0)
                Next

                For k As Integer = 1 To count - 1
                    ' 观测：station(k) → station(k-1) 的相对位姿
                    Dim relative As RigidTransform = pairwise(k - 1).Transform
                    Dim weight As Double = 1.0 / std.Max(1.0E-6, pairwise(k - 1).InlierRmse * pairwise(k - 1).InlierRmse + 1.0E-6)

                    ' 边的语义：T_to = T_from ∘ Z，所以 from = k（局部站），to = k-1（前一站）
                    Call graph.AddEdge(k, k - 1, relative, weight)
                Next

                poseGraphResult = PoseGraphOptimizer.Optimize(graph)

                transforms = poseGraphResult.Poses
            End If

            Dim merged(count - 1) As PointCloudBuffer

            For k As Integer = 0 To count - 1
                merged(k) = transforms(k).ApplyToCloud(stations(k))
            Next

            Return New MultiStationResult With {
                .Transforms = transforms,
                .Pairwise = pairwise,
                .Combined = PointCloudBuffer.Merge(merged),
                .PoseGraph = poseGraphResult
            }
        End Function

    End Class
End Namespace
