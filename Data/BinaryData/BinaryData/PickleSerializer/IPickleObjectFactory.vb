Namespace Pickle

    ''' <summary>
    ''' Pickle 反序列化的对象扩展工厂：允许宿主程序集接管某些第三方全局符号（例如 numpy 的
    ''' <c>numpy.core.multiarray._reconstruct</c>、<c>numpy.dtype</c>）的构造与状态恢复过程。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="MinimalPickleUnpickler"/> 在 <c>REDUCE</c>/<c>NEWOBJ</c>（对象构造）与
    ''' <c>BUILD</c>（<c>__setstate__</c> 状态恢复）两个时机询问工厂：
    ''' 
    ''' <list type="number">
    ''' <item>先调用 <see cref="Construct"/>，返回非 <c>Nothing</c> 表示由工厂接管该类型；</item>
    ''' <item>随后的 <c>BUILD</c> 会调用 <see cref="ApplyState"/>，工厂可以把占位对象替换为最终实例。</item>
    ''' </list>
    ''' 
    ''' 未提供工厂时，反序列化器的行为与旧版本完全一致。
    ''' </remarks>
    Public Interface IPickleObjectFactory

        ''' <summary>
        ''' 构造一个 Python 全局符号（<c>module.class</c>）对应的 .NET 对象。
        ''' </summary>
        ''' <param name="moduleName">来自 <c>GLOBAL</c> 操作码的模块名称，例如 <c>numpy.core.multiarray</c></param>
        ''' <param name="className">来自 <c>GLOBAL</c> 操作码的类/函数名称，例如 <c>_reconstruct</c></param>
        ''' <param name="args"><c>REDUCE</c>/<c>NEWOBJ</c> 携带的构造参数（已展开为对象数组）</param>
        ''' <returns>返回构造出的对象；返回 <c>Nothing</c> 表示该符号不由本工厂处理，交回默认实现。</returns>
        Function Construct(moduleName As String, className As String, args As Object()) As Object

        ''' <summary>
        ''' 把 <c>BUILD</c> 操作码携带的状态对象（Python 的 <c>__setstate__</c> 参数）应用到目标对象上。
        ''' </summary>
        ''' <param name="obj">
        ''' 目标对象，允许被替换：numpy 的 <c>ndarray</c> 采用"先构造空占位对象、再通过 setstate 填充数据"的
        ''' 序列化协议，因此工厂通常需要把占位对象替换为携带真实数据的新实例。
        ''' </param>
        ''' <param name="state">状态对象，可能是 <see cref="PythonTuple"/>、字典或标量</param>
        ''' <returns>返回 <c>True</c> 表示状态已被工厂处理（反序列化器会把 <paramref name="obj"/> 的新值压回栈）；
        ''' 返回 <c>False</c> 表示交回默认实现。</returns>
        Function ApplyState(ByRef obj As Object, state As Object) As Boolean

    End Interface

End Namespace
