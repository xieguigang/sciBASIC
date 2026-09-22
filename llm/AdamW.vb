' ---------------------------------------------------------------------------
' AdamW —— 带解耦权重衰减的 Adam 优化器
'
' 与既有 Transformer\Optimizer.vb（纯 Adam）的唯一差别是权重衰减的施加方式：
'
'   Adam + L2      :  g ← g + wd·θ  再走 Adam 的自适应缩放
'                    → 衰减项被 1/sqrt(v) 缩放，对大梯度维度反而衰减得少，
'                      与"权重越大惩罚越强"的初衷相悖；
'   AdamW（解耦）  :  先按 Adam 更新，再独立地做  θ ← θ − lr·wd·θ
'                    → 衰减量与梯度统计无关，超参之间不再互相纠缠。
'
' 这是 LLM 预训练的事实标准（GPT-3 / Llama / DeepSeek 均用 AdamW）。
'
' 状态组织沿用本仓库的既有约定：每个参数张量配对一份 <see cref="AdamW"/> 实例，
' 实例内部持有与其同形的一阶矩 m、二阶矩 v 与梯度累加器 g；反向阶段只往 g 里
' 原地 +=，训练步结束时统一更新参数并清零 g。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace LLM

    ''' <summary>
    ''' AdamW：解耦权重衰减的 Adam 优化器。
    ''' </summary>
    Public Class AdamW

        Private Const Beta1 As Double = 0.9
        Private Const Beta2 As Double = 0.999
        Private Const Eps As Double = 0.00000001

        ''' <summary>一阶矩估计（与参数同形）</summary>
        Private ReadOnly _m As Tensor

        ''' <summary>二阶矩估计（与参数同形）</summary>
        Private ReadOnly _v As Tensor

        ''' <summary>梯度累加器（与参数同形）</summary>
        Private ReadOnly _gradient As Tensor

        ''' <summary>
        ''' 与该参数关联的权重衰减系数。0 表示退化为纯 Adam。
        ''' </summary>
        ''' <remarks>
        ''' 惯例上只对权重矩阵（含嵌入矩阵）施加衰减，不对 RMSNorm 的 γ 与偏置施加，
        ''' 因此这里把它做成"逐参数可配"的。
        ''' </remarks>
        Public Property WeightDecay As Double = 0.0

        ''' <summary>与参数同形的梯度累加器；反向阶段由调用方原地累加。</summary>
        Public ReadOnly Property Gradient As Tensor
            Get
                Return _gradient
            End Get
        End Property

        ''' <summary>按参数张量的形状创建优化器状态。</summary>
        ''' <param name="param">待优化的参数张量（本类只读它的形状）</param>
        ''' <param name="weightDecay">解耦权重衰减系数</param>
        Public Sub New(param As Tensor, Optional weightDecay As Double = 0.0)
            _m = New Tensor(param.Shape)
            _v = New Tensor(param.Shape)
            _gradient = New Tensor(param.Shape)
            Me.WeightDecay = weightDecay
        End Sub

        ''' <summary>把梯度累加器清零。</summary>
        Public Sub ZeroGrad()
            Call LLMTensorOps.ZeroInPlace(_gradient)
        End Sub

        ''' <summary>当前梯度累加器的 L2 范数（用于逐参数的观测诊断）。</summary>
        Public Function GradientNorm() As Double
            Return Tensor.computeKernel.L2Norm(_gradient)
        End Function

        ''' <summary>
        ''' 是否允许走设备端（GPU）AdamW：把参数与两个矩钉在显存里，由融合内核就地更新。
        ''' </summary>
        ''' <remarks>
        ''' 只应在"该参数的主机副本不再被任何主机循环读取"时打开。
        ''' 典型安全对象是只被 GEMM 消费的权重矩阵（<c>MatMul</c> 会通过设备常驻表
        ''' 直接拿到最新值）。反例：
        '''   * RMSNorm 的 γ —— <c>RmsNorm</c> 在主机循环里读 <c>Gamma.Data</c>；
        '''   * 词嵌入 —— <c>LLMModel.Embed</c> 在主机上按行查表。
        ''' 这两个若被钉住，主机侧读到的是陈旧值，会静默算错。
        ''' </remarks>
        Public Property UseDeviceResidency As Boolean = False

        ''' <summary>一阶矩张量（供设备常驻与显存统计使用）。</summary>
        Public ReadOnly Property Momentum As Tensor
            Get
                Return _m
            End Get
        End Property

        ''' <summary>二阶矩张量（供设备常驻与显存统计使用）。</summary>
        Public ReadOnly Property Velocity As Tensor
            Get
                Return _v
            End Get
        End Property

        ''' <summary>
        ''' 试做一次设备端（GPU）AdamW 更新。
        ''' </summary>
        ''' <param name="learningRate">学习率</param>
        ''' <param name="step">训练步序号（从 1 开始，用于偏差校正）</param>
        ''' <param name="param">待更新的参数张量</param>
        ''' <returns>
        ''' <c>True</c> 表示本轮更新已在设备上完成，调用方<b>不得</b>再调
        ''' <see cref="MakeTrainingStep"/>；<c>False</c> 表示需回退主机实现。
        ''' </returns>
        ''' <remarks>
        ''' 为什么值得单独做一条设备路径：主机侧的 AdamW 是"每个参数的 4 个数组各遍历一遍"，
        ''' 对 2 亿参数就是 8 亿次带 <c>sqrt</c> / 除法的元素操作，实测单步可达秒级。
        ''' 设备端把它压成一次内核启动。
        ''' <para>
        ''' 注意"梯度不钉住"是刻意的：梯度由主机侧反向传播逐层累加，每步都带新版本号，
        ''' 走 LRU 缓存重新上传即可。内核在设备缓冲上写的"清零"由本方法在主机侧
        ''' 用 <see cref="ZeroGrad"/> 同步，两者必须成对出现。
        ''' </para>
        ''' </remarks>
        Public Function TryDeviceStep(learningRate As Double, [step] As Integer, param As Tensor) As Boolean
            If Not UseDeviceResidency Then Return False
            If param Is Nothing Then Return False

            Dim kernel = Tensor.computeKernel

            If Not kernel.SupportsDeviceResidency Then Return False

            If Not _devicePinned Then
                ' 首次调用时把参数与两个矩钉住；任一失败就整体放弃设备路径
                If Not kernel.PinDevice(param, "adamw.param", False) Then Return False
                If Not kernel.PinDevice(_m, "adamw.m", True) Then Return False
                If Not kernel.PinDevice(_v, "adamw.v", True) Then Return False

                _devicePinned = True
            End If

            Dim bc1 = 1.0 - std.Pow(Beta1, [step])
            Dim bc2 = 1.0 - std.Pow(Beta2, [step])

            If Not kernel.TryAdamWStep(param, _gradient, _m, _v, learningRate,
                                       Beta1, Beta2, Eps, bc1, bc2, WeightDecay) Then
                Return False
            End If

            ' 内核把清零写在了设备缓冲上，主机数组必须同步清零：
            ' 否则下一步上传梯度时会把旧值重新带回设备
            Call ZeroGrad()

            Return True
        End Function

        ''' <summary>设备路径是否已经建立（参数与两个矩都已钉住）。</summary>
        Private _devicePinned As Boolean

        ''' <summary>
        ''' 按 AdamW 规则原地更新参数，并在更新完成后清零梯度累加器。
        ''' </summary>
        ''' <param name="learningRate">学习率</param>
        ''' <param name="step">训练步序号（从 1 开始，用于偏差校正）</param>
        ''' <param name="param">待更新的参数张量</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer, param As Tensor)
            Dim p = param.Data
            Dim g = _gradient.Data
            Dim m = _m.Data
            Dim v = _v.Data

            Dim bc1 = 1.0 - std.Pow(Beta1, [step])
            Dim bc2 = 1.0 - std.Pow(Beta2, [step])
            Dim wd = WeightDecay

            For i As Integer = 0 To p.Length - 1
                Dim gi = g(i)
                Dim mi = Beta1 * m(i) + (1.0 - Beta1) * gi
                Dim vi = Beta2 * v(i) + (1.0 - Beta2) * gi * gi

                m(i) = mi
                v(i) = vi

                Dim mHat = mi / bc1
                Dim vHat = vi / bc2

                ' Adam 自适应项
                Dim delta = learningRate * mHat / (std.Sqrt(vHat) + Eps)

                ' 解耦权重衰减：与梯度统计完全无关的一项
                If wd > 0 Then delta += learningRate * wd * p(i)

                p(i) -= delta
            Next

            Call param.MarkHostModified()
            Call ZeroGrad()
        End Sub

    End Class

End Namespace
