Namespace ONNX.DirectML

    ''' <summary>
    ''' DirectML 推理后端：把 <see cref="OnnxModel"/> 的计算图编译成 DirectML 算子图并在 GPU 上执行。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这一层是"ONNX 语义"与"DirectML 算子"之间的桥梁：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><c>DmlOnnxGraphCompiler</c> 负责遍历 ONNX 节点、推导形状、构造 DML 算子；</description></item>
    ''' <item><description><c>DmlOnnxSession</c> 负责编译缓存与多输入多输出执行，API 形状与 <see cref="ONNX.OnnxGraphEvaluator"/> 对齐；</description></item>
    ''' <item><description><c>DmlOnnxCapabilities</c> 给出当前设备对 ONNX 算子的支持矩阵。</description></item>
    ''' </list>
    ''' <para>
    ''' 数值策略：ONNX 张量在 <see cref="ONNX.OnnxTensor"/> 里是双精度，DirectML 只接受 FP16/FP32 等
    ''' 原生类型，因此转换只发生在**图的输入与输出边界**一次，中间结果全程驻留显存。
    ''' </para>
    ''' </remarks>
    Friend Class NamespaceDoc
    End Class

End Namespace
