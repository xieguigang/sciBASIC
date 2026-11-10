Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Registration
Imports std = System.Math

Namespace Applications

    ''' <summary>
    ''' 6-DoF 位姿估计结果。
    ''' </summary>
    Public Class PoseResult

        ''' <summary>目标（模型）在场景坐标系中的位姿：<c>p_scene ≈ Transform · p_model</c>。</summary>
        Public Property Transform As RigidTransform

        ''' <summary>内点率（重叠度）。</summary>
        Public Property Fitness As Double

        ''' <summary>内点 RMSE。</summary>
        Public Property Rmse As Double

        ''' <summary>
        ''' 置信度 <c>[0,1]</c>：内点率与残差的综合评分。
        ''' </summary>
        Public Property Confidence As Double

        ''' <summary>是否收敛。</summary>
        Public Property Converged As Boolean

        ''' <summary>使用的方法链（诊断用）。</summary>
        Public Property Method As String

        ''' <summary>
        ''' 置信度是否达到可用阈值。
        ''' </summary>
        Public Function IsReliable(Optional threshold As Double = 0.5) As Boolean
            Return Confidence >= threshold AndAlso Transform IsNot Nothing
        End Function

        Public Overrides Function ToString() As String
            Return $"pose: confidence={Confidence:P1}, fitness={Fitness:P1}, rmse={Rmse:G4} ({Method})"
        End Function
    End Class

    ''' <summary>
    ''' 6-DoF 位姿估计与实时跟踪。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么用“特征粗配准 + ICP 精化”而不是端到端回归</b>
    ''' （即 GraspNet/AnyGrasp 这类深度学习方法）：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>不需要训练数据</b>：工业现场每个新型号都要重新采集标注，
    ''' 成本极高；几何方法只需要目标零件的 CAD/参考点云。</description></item>
    ''' <item><description><b>可解释、可验证</b>：置信度直接来自内点率与残差，
    ''' 可以给出“为什么失败”（重叠不足/特征不足/初始位姿太远）。</description></item>
    ''' <item><description><b>精度可达亚毫米</b>：对刚性零件，ICP 精化后的位姿精度
    ''' 通常优于深度回归（后者受训练分布限制）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>能力边界</b>：几何方法依赖“参考模型已知 + 表面有足够几何特征”。
    ''' 对“纹理单一且几何对称”的零件（例如正方体、圆柱端面），
    ''' 会存在旋转方向的位姿歧义（多解），这时必须借助
    ''' 额外约束（夹持方向、已知朝向、条码/标记）或改用深度学习方法。
    ''' </para>
    ''' <para>
    ''' <b>跟踪</b>：<see cref="Track(PoseResult, PointCloudBuffer, PointCloudBuffer, PoseOptions, CloudyOptions)"/>
    ''' 以上一帧位姿为初值做局部 ICP，并可用匀速模型预测。
    ''' 这是动态产线上“每帧几毫秒”的关键——避免每帧重跑粗配准。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class PoseEstimation

        Private Sub New()
        End Sub

        ''' <summary>位姿估计参数。</summary>
        Public Class PoseOptions

            ''' <summary>是否执行粗配准（<c>False</c> 时直接用给定初值做 ICP）。</summary>
            Public Property UseCoarse As Boolean = True

            ''' <summary>精配准最大迭代次数（默认 60）。</summary>
            Public Property MaxIterations As Integer = 60

            ''' <summary>对应点最大距离；<c>≤ 0</c> 自适应。</summary>
            Public Property MaxCorrespondenceDistance As Double = 0.0

            ''' <summary>部分重叠时的截尾比例。</summary>
            Public Property TrimRatio As Double = 0.9

            ''' <summary>置信度中的残差尺度（默认取点间距的 2 倍）。</summary>
            Public Property RmseScale As Double = 0.0

            ''' <summary>是否使用点到面 ICP（否则点到点）。</summary>
            Public Property PointToPlane As Boolean = True
        End Class

        ''' <summary>
        ''' 单帧位姿估计。
        ''' </summary>
        ''' <param name="scene">场景点云（含目标与其他物体）</param>
        ''' <param name="model">目标参考点云（CAD 采样）</param>
        ''' <param name="initial">初始位姿（<c>Nothing</c> 表示从粗配准开始）</param>
        ''' <param name="options">参数</param>
        ''' <param name="optionsCloudy">并行选项</param>
        Public Shared Function Estimate(scene As PointCloudBuffer, model As PointCloudBuffer,
                                        Optional initial As RigidTransform = Nothing,
                                        Optional options As PoseOptions = Nothing,
                                        Optional optionsCloudy As CloudyOptions = Nothing) As PoseResult
            If scene Is Nothing Then Throw New ArgumentNullException(NameOf(scene))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim opt As PoseOptions = If(options, New PoseOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            Dim transform As RigidTransform = If(initial, RigidTransform.Identity)
            Dim methods As New List(Of String)()

            If initial Is Nothing AndAlso opt.UseCoarse Then
                Dim coarse As RegistrationResult = RansacRegistration.Align(model, scene, Nothing, global_)

                If coarse IsNot Nothing AndAlso coarse.Fitness > 0.05 Then
                    transform = coarse.Transform
                    Call methods.Add("RANSAC")
                End If
            End If

            Dim icpOptions As New IcpRegistration.IcpOptions With {
                .MaxIterations = opt.MaxIterations,
                .MaxCorrespondenceDistance = opt.MaxCorrespondenceDistance,
                .TrimRatio = opt.TrimRatio
            }

            Dim refined As RegistrationResult

            If opt.PointToPlane Then
                refined = IcpRegistration.AlignPointToPlane(model, scene, transform, icpOptions, global_)
            Else
                refined = IcpRegistration.AlignPointToPoint(model, scene, transform, icpOptions, global_)
            End If

            Call methods.Add(refined.Method)

            Dim scale As Double = If(opt.RmseScale > 0.0, opt.RmseScale, std.Max(model.EstimateAverageSpacing() * 2.0, 1.0E-6))

            Return New PoseResult With {
                .Transform = refined.Transform,
                .Fitness = refined.Fitness,
                .Rmse = refined.InlierRmse,
                .Converged = refined.Converged,
                .Method = String.Join(" + ", methods),
                .Confidence = ComputeConfidence(refined.Fitness, refined.InlierRmse, scale)
            }
        End Function

        ''' <summary>
        ''' 帧间跟踪：以上一帧位姿为初值做局部 ICP。
        ''' </summary>
        ''' <param name="previous">上一帧的位姿结果</param>
        ''' <param name="scene">当前帧场景点云</param>
        ''' <param name="model">目标参考点云</param>
        ''' <param name="options">参数</param>
        ''' <param name="optionsCloudy">并行选项</param>
        ''' <remarks>
        ''' 跟踪失败（置信度骤降）时调用方应触发“重新识别”（重跑粗配准）。
        ''' 典型判据：<see cref="PoseResult.Confidence"/> &lt; 0.3 或
        ''' 位姿跳变超过“最大速度 × 帧间隔”。
        ''' </remarks>
        Public Shared Function Track(previous As PoseResult, scene As PointCloudBuffer, model As PointCloudBuffer,
                                     Optional options As PoseOptions = Nothing,
                                     Optional optionsCloudy As CloudyOptions = Nothing) As PoseResult
            If previous Is Nothing Then Throw New ArgumentNullException(NameOf(previous))

            Dim opt As PoseOptions = If(options, New PoseOptions())

            opt.UseCoarse = False
            opt.MaxIterations = std.Min(opt.MaxIterations, 20)

            Return Estimate(scene, model, previous.Transform, opt, optionsCloudy)
        End Function

        ''' <summary>
        ''' 匀速预测：用上一帧与当前帧位姿外推下一帧初值。
        ''' </summary>
        ''' <remarks>
        ''' 工业机器人通常做匀速运动，因此“上一帧速度”是很好的初值，
        ''' 可以把局部 ICP 的迭代次数压到 3~5 次而保持精度。
        ''' </remarks>
        Public Shared Function Predict(previous As PoseResult, current As PoseResult) As RigidTransform
            If previous Is Nothing OrElse current Is Nothing Then
                Throw New ArgumentNullException(NameOf(previous))
            End If

            Dim velocity As RigidTransform = previous.Transform.Inverse().Compose(current.Transform)

            Return current.Transform.Compose(velocity)
        End Function

        Private Shared Function ComputeConfidence(fitness As Double, rmse As Double, scale As Double) As Double
            If fitness <= 0.0 Then Return 0.0

            Dim quality As Double = std.Exp(-rmse / std.Max(scale, 1.0E-9))

            Return std.Max(0.0, std.Min(1.0, fitness * quality))
        End Function

    End Class
End Namespace
