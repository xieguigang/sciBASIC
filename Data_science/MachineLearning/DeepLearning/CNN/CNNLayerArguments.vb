Imports System.Runtime.CompilerServices

Namespace CNN

    ''' <summary>
    ''' 层的“规格”(layer specification)：一个被延迟创建的层描述。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 存在的意义是支持用流式写法来搭建网络：由 <see cref="CNNLayers"/> 里的工厂函数产出规格对象，
    ''' 再由 <see cref="LayerBuilder"/> 的 ``+`` 运算符在正确的时机把它实例化并追加进层列表：
    ''' </para>
    ''' <code>
    ''' Dim cnn As LayerBuilder = New LayerBuilder() +
    '''     input_layer({28, 28}, 1) +
    '''     conv_layer(5, 32, 1, 2) +
    '''     pool_layer(2, 2, 0) +
    '''     relu_layer() +
    '''     full_connected_layer(10) +
    '''     softmax_layer()
    ''' </code>
    ''' <para>
    ''' 之所以必须“延迟创建”，是因为本库的层在构造时就需要拿到 <see cref="data.OutputDefinition"/>
    ''' （它保存着上一层的输出尺寸，用来推算本层的输入尺寸），而那个对象是
    ''' <see cref="LayerBuilder"/> 的内部状态，在链式表达式求值之前无法被引用。
    ''' </para>
    ''' <para>
    ''' 与 R# 的 MLkit（``studio\Rsharp_kit\MLkit\MachineLearning\CNNLayerArguments.vb``）中同名类型的
    ''' 差异：那里用 ``type`` 字符串加 ``args`` 字典来携带参数，是因为 R# 需要在脚本层以弱类型的
    ''' 方式传递这些对象；在 VB 里直接用委托表达更类型安全——参数在编译期就被捕获，
    ''' 也不存在“未知层类型”这种运行期分支。
    ''' </para>
    ''' </remarks>
    Public Class CNNLayerArguments

        ''' <summary>
        ''' 可读的规格描述，例如 ``conv_layer(sx=5, filters=32, stride=1, padding=2)``
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' 真正把层实例化并追加到构建器上的动作
        ''' </summary>
        Friend ReadOnly factory As Func(Of LayerBuilder, LayerBuilder)

        Friend Sub New(name As String, factory As Func(Of LayerBuilder, LayerBuilder))
            Me.Name = name
            Me.factory = factory
        End Sub

        ''' <summary>
        ''' 在该构建器上创建并追加本规格所描述的层。
        ''' </summary>
        ''' <param name="builder">正在被构建的网络</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateLayer(builder As LayerBuilder) As LayerBuilder
            Return factory(builder)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Name}"
        End Function

    End Class
End Namespace
