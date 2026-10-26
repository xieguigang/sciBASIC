' ---------------------------------------------------------------------------
' 梯度计算
'
' 两种模式：
'   Analytic          —— 一次前向 + 一次反向得到全部参数梯度（默认）
'   FiniteDifference  —— 中心差分（保留用于对拍/回归验证，复杂度 O(参数数 × 全量渲染)）
'
' 解析反传的核心是"屏幕空间中间量 → 参数"的链式法则：
'   dL/dα, dL/dc  —— 像素合成阶段直接得到（见 CpuTileRasterBackend.BackwardTile）
'   dL/dconic, dL/dmean —— 同上，按高斯累加
'   dL/dΣ_cam     —— 由 dL/dconic 经 2x2 逆矩阵导数得到
'   dL/dJ         —— Σ' = J·Σ_cam·Jᵀ
'   dL/dpos       —— J 与 mean 的位置依赖
'   dL/dΣ_world   —— Σ_cam = R_cam·Σ_world·R_camᵀ
'   dL/ds, dL/dq  —— Σ_world = R(q)·diag(s²)·R(q)ᵀ
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>梯度计算模式。</summary>
Public Enum SplatGradientMode

    ''' <summary>解析反向传播（一次前向 + 一次反向）。</summary>
    Analytic = 0
    ''' <summary>中心差分（仅用于对拍/验证，代价极高）。</summary>
    FiniteDifference = 1

End Enum

''' <summary>
''' 梯度计算器。
''' </summary>
Public Class GradientCalculator

    ''' <summary>有限差分步长</summary>
    Public Property Epsilon As Double = 0.01

    ''' <summary>每次迭代采样的高斯数量（0 表示全部；仅有限差分模式使用）</summary>
    Public Property BatchSize As Integer = 0

    ''' <summary>梯度计算模式</summary>
    Public Property Mode As SplatGradientMode = SplatGradientMode.Analytic

    ''' <summary>渲染参数（解析模式使用）</summary>
    Public Property Options As SplatRenderOptions = SplatRenderOptions.Default

    Public Sub New(Optional epsilon As Double = 0.01, Optional batchSize As Integer = 0)
        Me.Epsilon = epsilon
        Me.BatchSize = batchSize
    End Sub

    ''' <summary>
    ''' 计算损失对高斯模型所有参数的梯度（结果写入 <see cref="GaussianModel"/> 的梯度缓冲）。
    ''' </summary>
    Public Sub ComputeGradients(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor))
        model.ZeroGradients()

        If Mode = SplatGradientMode.FiniteDifference Then
            Call ComputeFiniteDifference(model, cameras, targets, Nothing)
        Else
            Call ComputeAnalytic(model, cameras, targets)
        End If

        Call model.MarkModified()
    End Sub

    ''' <summary>返回本次（解析模式）计算得到的损失（多视图平均 MSE）。</summary>
    Public Function ComputeLossAndGradients(model As GaussianModel, cameras As List(Of Camera),
                                            targets As List(Of Tensor)) As Double
        model.ZeroGradients()

        If Mode = SplatGradientMode.FiniteDifference Then
            Dim loss = ComputeFiniteDifference(model, cameras, targets, Nothing)

            Call model.MarkModified()

            Return loss
        End If

        Dim value = ComputeAnalytic(model, cameras, targets)

        Call model.MarkModified()

        Return value
    End Function

    ''' <summary>
    ''' 解析梯度：对每个视图做一次前向 + 一次反向，累加全部参数梯度。
    ''' </summary>
    ''' <returns>多视图平均 MSE</returns>
    Private Function ComputeAnalytic(model As GaussianModel, cameras As List(Of Camera),
                                     targets As List(Of Tensor)) As Double
        Dim viewCount = cameras.Count
        If viewCount = 0 Then Return 0

        Dim perView As New SplatGradients(model.Count)
        Dim total As New SplatGradients(model.Count)
        Dim viewScale = 1.0 / viewCount
        Dim loss = 0.0

        For v = 0 To viewCount - 1
            Dim request As SplatRasterRequest = Nothing
            Dim transmittance As Double() = Nothing
            Dim tForward = PerfTrace.Begin()
            Dim rendered = SplatRenderer.RenderWithRequest(model, cameras(v), Options, request, transmittance)
            Call PerfTrace.Done(PerfPhase.Rasterize, tForward)

            Dim tLoss = PerfTrace.Begin()
            Dim mse = SplatRenderer.MseAndGradient(rendered, targets(v), viewScale)
            Call PerfTrace.Done(PerfPhase.Loss, tLoss)

            loss += mse.Loss

            perView.Clear()

            Dim backend = SplatBackends.Default
            Dim tBackward = PerfTrace.Begin()

            If backend Is Nothing OrElse
               Not backend.RasterizeBackward(request, rendered.Data, mse.Gradient, perView) Then

                Call SplatBackends.Reset()
                Call SplatBackends.Default.RasterizeBackward(request, rendered.Data, mse.Gradient, perView)
            End If

            ' 屏幕空间中间量 → 世界空间参数
            Call SplatProjectionGradient.Chain(model, request.Screen, perView)
            Call PerfTrace.Done(PerfPhase.Backward, tBackward)

            Call SplatProjectionGradient.Accumulate(perView, total)
        Next

        Call SplatProjectionGradient.CopyTo(total, model)

        Return loss / viewCount
    End Function

#Region "有限差分（对拍基线）"

    ''' <summary>
    ''' 中心差分梯度。复杂度 O(参数数 × 全量渲染)，仅用于验证解析梯度。
    ''' </summary>
    Public Function ComputeFiniteDifference(model As GaussianModel, cameras As List(Of Camera),
                                            targets As List(Of Tensor),
                                            indices As IList(Of Integer)) As Double
        Dim targetsToUse = If(indices, SampleIndices(model.Count))

        For Each idx In targetsToUse
            Call DifferencePosition(model, cameras, targets, idx)
            Call DifferenceColor(model, cameras, targets, idx)
            Call DifferenceScale(model, cameras, targets, idx)
            Call DifferenceOpacity(model, cameras, targets, idx)
        Next

        Return ComputeTotalLoss(model, cameras, targets)
    End Function

    Private Function SampleIndices(count As Integer) As IList(Of Integer)
        Dim indices As New List(Of Integer)()

        If BatchSize > 0 AndAlso BatchSize < count Then
            Dim rng As New Random(42)
            Dim pool As New HashSet(Of Integer)()

            While pool.Count < BatchSize
                pool.Add(rng.Next(count))
            End While

            For Each i In pool
                indices.Add(i)
            Next
        Else
            For i = 0 To count - 1
                indices.Add(i)
            Next
        End If

        Return indices
    End Function

    Private Sub DifferencePosition(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor), idx As Integer)
        Dim data = model.PositionsData
        Dim g = model.PositionsGradData

        For d = 0 To 2
            Dim o = idx * 3 + d
            Dim orig = data(o)

            data(o) = orig + Epsilon
            Dim plus = ComputeTotalLoss(model, cameras, targets)
            data(o) = orig - Epsilon
            Dim minus = ComputeTotalLoss(model, cameras, targets)
            data(o) = orig

            g(o) = (plus - minus) / (2 * Epsilon)
        Next
    End Sub

    Private Sub DifferenceColor(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor), idx As Integer)
        Dim data = model.ColorsData
        Dim g = model.ColorsGradData

        For d = 0 To 2
            Dim o = idx * 3 + d
            Dim orig = data(o)

            data(o) = orig + Epsilon
            Dim plus = ComputeTotalLoss(model, cameras, targets)
            data(o) = orig - Epsilon
            Dim minus = ComputeTotalLoss(model, cameras, targets)
            data(o) = orig

            g(o) = (plus - minus) / (2 * Epsilon)
        Next
    End Sub

    Private Sub DifferenceScale(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor), idx As Integer)
        Dim data = model.ScalesData
        Dim g = model.ScalesGradData
        Dim eps = Epsilon

        For d = 0 To 2
            Dim o = idx * 3 + d
            Dim orig = data(o)

            data(o) = orig + eps
            Dim plus = ComputeTotalLoss(model, cameras, targets)
            data(o) = orig - eps
            Dim minus = ComputeTotalLoss(model, cameras, targets)
            data(o) = orig

            g(o) = (plus - minus) / (2 * eps)
        Next
    End Sub

    Private Sub DifferenceOpacity(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor), idx As Integer)
        Dim data = model.OpacitiesData
        Dim g = model.OpacitiesGradData
        Dim eps = Epsilon
        Dim orig = data(idx)

        data(idx) = orig + eps
        Dim plus = ComputeTotalLoss(model, cameras, targets)
        data(idx) = orig - eps
        Dim minus = ComputeTotalLoss(model, cameras, targets)
        data(idx) = orig

        g(idx) = (plus - minus) / (2 * eps)
    End Sub

    ''' <summary>计算模型在所有视图上的平均损失。</summary>
    Public Function ComputeTotalLoss(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor)) As Double
        Dim total = 0.0

        For v = 0 To cameras.Count - 1
            Dim rendered = SplatRenderer.Render(model, cameras(v), Options)
            total += SplatRenderer.ComputeMSE(rendered, targets(v))
        Next

        Return total / cameras.Count
    End Function

#End Region

End Class


''' <summary>
''' 自适应密度控制
'''
''' 3DGS 的关键创新之一：动态调整高斯数量
'''   - **克隆 (Clone)**: 对梯度大且尺寸小的高斯进行复制
'''   - **分裂 (Split)**: 对梯度大且尺寸大的高斯进行分裂
'''   - **剪枝 (Prune)**: 删除不透明度过低或尺寸过大的高斯
''' </summary>
Public Class DensityController

    ''' <summary>位置梯度阈值（超过此值认为高斯需要 densify）</summary>
    Public Property GradThreshold As Double = 0.0002

    ''' <summary>最小不透明度（低于此值剪枝）</summary>
    Public Property MinOpacity As Double = 0.005

    ''' <summary>最大缩放（超过此值剪枝，防止高斯过大）</summary>
    Public Property MaxScale As Double = 0.5

    ''' <summary>分割阈值：最大缩放高于此值走 split，否则 clone</summary>
    Public Property SplitScale As Double = 0.01

    ''' <summary>每隔多少步执行一次密度控制</summary>
    Public Property DensifyInterval As Integer = 100

    Public Sub New(Optional gradThreshold As Double = 0.0002)
        Me.GradThreshold = gradThreshold
    End Sub

    ''' <summary>
    ''' 执行自适应密度控制。
    ''' </summary>
    ''' <returns>操作统计信息</returns>
    Public Function Densify(model As GaussianModel, [step] As Integer) As (Cloned As Integer, Split As Integer, Pruned As Integer)
        If DensifyInterval <= 0 Then Return (0, 0, 0)
        If [step] Mod DensifyInterval <> 0 OrElse [step] = 0 Then Return (0, 0, 0)

        Dim pruned = 0

        ' ---- 1. 剪枝 ----
        Dim toPrune As New HashSet(Of Integer)()
        Dim scale = model.ScalesData

        For i = 0 To model.Count - 1
            If model.GetOpacity(i) < MinOpacity Then
                toPrune.Add(i)
                Continue For
            End If

            Dim o3 = i * 3
            Dim sx = std.Exp(scale(o3))
            Dim sy = std.Exp(scale(o3 + 1))
            Dim sz = std.Exp(scale(o3 + 2))

            If std.Max(sx, std.Max(sy, sz)) > MaxScale Then toPrune.Add(i)
        Next

        If toPrune.Count > 0 Then
            pruned = toPrune.Count
            Call model.RemoveGaussians(toPrune)
        End If

        ' ---- 2. 依据位置梯度范数决定 clone / split ----
        Dim toClone As New List(Of Integer)()
        Dim toSplit As New List(Of Integer)()
        Dim gradPos = model.PositionsGradData
        Dim count = model.Count

        For i = 0 To count - 1
            Dim o3 = i * 3
            Dim gx = gradPos(o3), gy = gradPos(o3 + 1), gz = gradPos(o3 + 2)
            Dim norm = std.Sqrt(gx * gx + gy * gy + gz * gz)

            If norm <= GradThreshold Then Continue For

            Dim sx = std.Exp(scale(o3))
            Dim sy = std.Exp(scale(o3 + 1))
            Dim sz = std.Exp(scale(o3 + 2))

            If std.Max(sx, std.Max(sy, sz)) > SplitScale Then
                toSplit.Add(i)
            Else
                toClone.Add(i)
            End If
        Next

        If toClone.Count = 0 AndAlso toSplit.Count = 0 Then Return (0, 0, pruned)

        ' ---- 3. 一次性构造新增高斯（避免两次整表重分配）----
        Dim extra = toClone.Count + toSplit.Count * 2
        Dim pos(extra * 3 - 1) As Double
        Dim scl(extra * 3 - 1) As Double
        Dim rot(extra * 4 - 1) As Double
        Dim col(extra * 3 - 1) As Double
        Dim opc(extra - 1) As Double

        Dim srcPos = model.PositionsData
        Dim srcScale = model.ScalesData
        Dim srcRot = model.RotationsData
        Dim srcColor = model.ColorsData
        Dim srcOpac = model.OpacitiesData

        Dim rng As New Random([step] + 1)
        Dim k = 0

        For Each srcIdx In toClone
            Dim o3 = srcIdx * 3, o4 = srcIdx * 4
            Dim d3 = k * 3, d4 = k * 4

            pos(d3) = srcPos(o3) : pos(d3 + 1) = srcPos(o3 + 1) : pos(d3 + 2) = srcPos(o3 + 2)
            scl(d3) = srcScale(o3) : scl(d3 + 1) = srcScale(o3 + 1) : scl(d3 + 2) = srcScale(o3 + 2)
            col(d3) = srcColor(o3) : col(d3 + 1) = srcColor(o3 + 1) : col(d3 + 2) = srcColor(o3 + 2)
            rot(d4) = srcRot(o4) : rot(d4 + 1) = srcRot(o4 + 1) : rot(d4 + 2) = srcRot(o4 + 2) : rot(d4 + 3) = srcRot(o4 + 3)
            opc(k) = srcOpac(srcIdx)

            k += 1
        Next

        Dim log2 = std.Log(2.0)

        For Each srcIdx In toSplit
            Dim o3 = srcIdx * 3, o4 = srcIdx * 4

            For child = 0 To 1
                Dim d3 = k * 3, d4 = k * 4

                pos(d3) = srcPos(o3) + (rng.NextDouble() - 0.5) * 0.1
                pos(d3 + 1) = srcPos(o3 + 1) + (rng.NextDouble() - 0.5) * 0.1
                pos(d3 + 2) = srcPos(o3 + 2) + (rng.NextDouble() - 0.5) * 0.1

                scl(d3) = srcScale(o3) - log2
                scl(d3 + 1) = srcScale(o3 + 1) - log2
                scl(d3 + 2) = srcScale(o3 + 2) - log2

                col(d3) = srcColor(o3) : col(d3 + 1) = srcColor(o3 + 1) : col(d3 + 2) = srcColor(o3 + 2)
                rot(d4) = srcRot(o4) : rot(d4 + 1) = srcRot(o4 + 1) : rot(d4 + 2) = srcRot(o4 + 2) : rot(d4 + 3) = srcRot(o4 + 3)
                opc(k) = srcOpac(srcIdx)

                k += 1
            Next
        Next

        Call model.AppendGaussians(pos, scl, rot, col, opc, extra)

        Return (toClone.Count, toSplit.Count * 2, pruned)
    End Function

End Class
