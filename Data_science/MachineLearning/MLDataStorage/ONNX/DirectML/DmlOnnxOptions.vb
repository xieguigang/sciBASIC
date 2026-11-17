Namespace ONNX.DirectML

    ''' <summary>
    ''' DirectML 推理后端的选项。
    ''' </summary>
    Public Class DmlOnnxOptions

        ''' <summary>使用的适配器序号（从 0 开始）。</summary>
        Public Property AdapterIndex As Integer = 0

        ''' <summary>
        ''' 额外的原生库搜索目录（例如 <c>Microsoft.AI.DirectML</c> 的分发目录）。
        ''' </summary>
        Public Property DirectMLDirectory As String

        ''' <summary>是否启用 D3D12 / DirectML 调试层。</summary>
        Public Property EnableDebugLayer As Boolean = False

        ''' <summary>是否允许 DirectML 内部使用 FP16 计算（更快，误差更大）。</summary>
        Public Property AllowHalfPrecision As Boolean = False

        ''' <summary>
        ''' 当 DirectML 不可用或算子不受支持时，是否允许回退到
        ''' <see cref="OnnxGraphEvaluator"/>（CPU 双精度）。
        ''' </summary>
        ''' <remarks>
        ''' 默认关闭：推理结果静默地来自另一个后端会让问题难以定位。
        ''' 打开后，<see cref="DmlOnnxSession.Backend"/> 会明确报告实际使用的后端。
        ''' </remarks>
        Public Property AllowCpuFallback As Boolean = False

        ''' <summary>组装/编译失败时的原因（由 <see cref="DmlOnnxSession.TryCreate"/> 填充）。</summary>
        Public Property ErrorMessage As String

        ''' <summary>
        ''' 可选的执行阶段回调（编译/执行各步骤），用于定位与原生驱动交互时的崩溃。
        ''' </summary>
        Public Property Trace As Action(Of String)

    End Class

End Namespace
