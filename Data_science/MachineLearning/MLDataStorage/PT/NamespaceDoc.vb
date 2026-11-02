' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\NamespaceDoc.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Global.Microsoft.VisualBasic.MachineLearning.DataStorage.PT

    ''' <summary>
    ''' PyTorch <c>.pt</c> 模型权重文件的读写支持。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本命名空间把 PyTorch 的权重归档与框架的
    ''' <c>Microsoft.VisualBasic.MachineLearning.TensorFlow.Tensor</c> 对接起来：既可以把外部训练产出的
    ''' <c>.pt</c> 文件读成张量，也可以把自身的张量重新写出成 PyTorch 可加载的 <c>.pt</c> 文件。
    ''' </para>
    ''' <para>
    ''' 归档结构（新式 zip 序列化，即 <c>torch.save(obj, "model.pt")</c> 的默认格式）：
    ''' </para>
    ''' <code>
    ''' model.pt (zip)
    ''' ├── model/data.pkl          pickle 流：名称 → _rebuild_tensor_v2(存储引用, offset, size, stride, ...)
    ''' ├── model/data/0            存储块 0 的原始字节（小端序）
    ''' ├── model/data/1            ...
    ''' └── model/version           内容 "3"
    ''' </code>
    ''' <para>
    ''' 前缀（<c>model/</c>）由保存时的文件名决定，读取时由以 <c>data.pkl</c> 结尾的条目反推，
    ''' 因此 <c>degree.pt</c>（前缀 <c>degree/</c>）与 <c>model_KcatNet.pt</c>（前缀 <c>archive/</c>）
    ''' 都能直接读取。
    ''' </para>
    ''' <list type="table">
    ''' <item>
    '''   <term><see cref="PtFile"/></term>
    '''   <description>文件门面：<c>Load</c> / <c>LoadStateDict</c> / <c>Save</c> / <c>ReadInfo</c>。</description>
    ''' </item>
    ''' <item>
    '''   <term><see cref="PtStateDict"/></term>
    '''   <description>名称 → 张量的权重字典，附带每项的元素类型与 <c>requires_grad</c> 标记。</description>
    ''' </item>
    ''' <item>
    '''   <term><see cref="PtElementType"/></term>
    '''   <description>落盘元素类型，以及 <see cref="PtDataType"/> 提供的字节编解码。</description>
    ''' </item>
    ''' <item>
    '''   <term><see cref="PtStorage"/></term>
    '''   <description>存储块：归档中真正存放数值的连续字节段。</description>
    ''' </item>
    ''' <item>
    '''   <term><see cref="PtPickleFactory"/></term>
    '''   <description>pickle 反序列化扩展：把 <c>_rebuild_tensor_v2</c> 还原成 <see cref="Tensor"/>。</description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' 元素类型映射（新式 dtype 记号与旧式 storage 类名都支持读取）：
    ''' </para>
    ''' <list type="table">
    ''' <listheader>
    '''   <term>dtype 记号</term>
    '''   <description>旧式 storage 类名 / 字节数 / 说明</description>
    ''' </listheader>
    ''' <item><term>torch.float32</term><description>torch.FloatStorage，4 字节，默认写出精度</description></item>
    ''' <item><term>torch.float64</term><description>torch.DoubleStorage，8 字节，无损通行</description></item>
    ''' <item><term>torch.float16</term><description>torch.HalfStorage，2 字节，读取无损、写出按就近舍入</description></item>
    ''' <item><term>torch.bfloat16</term><description>torch.BFloat16Storage，2 字节</description></item>
    ''' <item><term>torch.int64 / int32 / int16 / int8</term><description>Long / Int / Short / CharStorage，8/4/2/1 字节</description></item>
    ''' <item><term>torch.uint8 / torch.bool</term><description>Byte / BoolStorage，1 字节</description></item>
    ''' </list>
    ''' <para>
    ''' 已知限制：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>
    ''' 只支持**权重字典**（<c>state_dict</c>，dict / OrderedDict，可嵌套字典）。
    ''' <c>torch.save</c> 保存的完整 <c>nn.Module</c> 对象图、优化器中的非张量状态、以及
    ''' 依赖 <c>copyreg</c> 扩展注册表的自定义类不在支持范围内。
    ''' </item>
    ''' <item>
    ''' 张量数值在内存中统一为双精度，因此 int64 中超过 2^53 的整数会丢失精度
    ''' （这与框架 <see cref="Tensor"/> 只支持双精度有关，而非归档格式的问题）。
    ''' </item>
    ''' <item>
    ''' 写出时每个张量独占一块存储，不会还原原始的存储共享 / 视图（转置、切片）关系；
    ''' 数值本身无损，但 PyTorch 端重新加载后权重绑定关系不再保留。
    ''' </item>
    ''' <item>
    ''' 0 阶（标量）张量在框架内按形状 <c>{1}</c> 表示。
    ''' </item>
    ''' </list>
    ''' </remarks>
    Public Class NamespaceDoc
    End Class

End Namespace
