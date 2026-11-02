' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\NamespaceDoc.vb
' /********************************************************************************/

Namespace Global.Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX

    ''' <summary>
    ''' ONNX 模型文件的读写与互操作支持。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本命名空间为 <c>Microsoft.VisualBasic.MachineLearning.CNN</c> 里的卷积网络补齐了业界通用的模型交换格式：
    ''' </para>
    ''' <list type="table">
    ''' <item>
    ''' <term><see cref="OnnxModel"/></term>
    ''' <description>ONNX 文件的对象视图：读写文件、枚举初始化器、查询输入输出形状、打印结构摘要。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="OnnxExport"/></term>
    ''' <description>把 <c>ConvolutionalNN</c> 翻译成标准 ONNX 计算图（NCHW 布局，默认单精度）。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="OnnxImport"/></term>
    ''' <description>把 ONNX 计算图还原成可继续训练 / 推理的 <c>ConvolutionalNN</c>。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="OnnxGraphEvaluator"/></term>
    ''' <description>不依赖外部推理引擎的最小前向求值器，用于没有对应层的模型（例如强化学习策略网）。</description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' 不支持的结构不会被静默忽略：导出时会抛出 <see cref="NotSupportedException"/>，
    ''' 推理 / 导入时会抛出 <see cref="OnnxUnsupportedOperatorException"/>（带有节点名与算子名）。
    ''' 已覆盖的算子清单见 <see cref="OnnxGraphEvaluator"/> 的实现分派。
    ''' </para>
    ''' <para>
    ''' 既有的私有二进制模型格式（<c>SaveModelCNN</c> / <c>ReadModelCNN</c>）保持完全不变，
    ''' ONNX 支持是叠加的独立模块。
    ''' </para>
    ''' </remarks>
    Public Class NamespaceDoc
    End Class

End Namespace
