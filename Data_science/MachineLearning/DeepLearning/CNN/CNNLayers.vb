Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN

    ''' <summary>
    ''' 搭建 CNN 网络时使用的“层规格”工厂函数。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 函数命名与 R# 的 MLkit（``studio\Rsharp_kit\MLkit\MachineLearning\CNN.vb``）保持一一对应，
    ''' 因此 R# 脚本中的网络定义可以逐行翻译到 VB。以
    ''' ``tutorials\..\CNN_image\auto_encoder.R`` 为例：
    ''' </para>
    ''' <code>
    ''' ' R#:
    ''' let cnn = cnn()
    '''     + input_layer([28, 28], 1)
    '''     + conv_layer(5, 32, 1, 2)
    '''     + pool_layer(2, 2, 0)
    '''     + leaky_relu_layer()
    '''     + softmax_layer();
    '''
    ''' ' VB:
    ''' Dim cnn = New LayerBuilder() +
    '''     input_layer({28, 28}, 1) +
    '''     conv_layer(5, 32, 1, 2) +
    '''     pool_layer(2, 2, 0) +
    '''     leaky_relu_layer() +
    '''     softmax_layer()
    ''' </code>
    ''' <para>
    ''' 两者唯一的差别是 VB 的隐式换行要求二元运算符写在**上一行行尾**（R# 允许写在行首）。
    ''' </para>
    ''' <para>
    ''' 这些函数返回的都是 <see cref="CNNLayerArguments"/>（尚未创建的层），
    ''' 由 <see cref="LayerBuilder"/> 的 ``+`` 运算符按从左到右的顺序实例化，
    ''' 因此最终得到的层序列与逐个调用 ``buildXxxLayer`` 完全一致。
    ''' </para>
    ''' </remarks>
    Public Module CNNLayers

        ''' <summary>
        ''' 构造规格对象；<paramref name="name"/> 只用于 <see cref="CNNLayerArguments.ToString"/> 的可读输出
        ''' </summary>
        Private Function spec(name As String, factory As Func(Of LayerBuilder, LayerBuilder)) As CNNLayerArguments
            Return New CNNLayerArguments(name, factory)
        End Function

        ''' <summary>
        ''' 输入层：把数据透传进网络，同时声明输入图像的尺寸与通道数。
        ''' </summary>
        ''' <param name="size">形如 ``{width, height}`` 的图像尺寸</param>
        ''' <param name="depth">通道数，灰度图为 1</param>
        Public Function input_layer(size As Integer(), Optional depth As Integer = 1) As CNNLayerArguments
            If size Is Nothing OrElse size.Length < 2 Then
                Throw New ArgumentException("需要形如 {width, height} 的图像尺寸", NameOf(size))
            End If

            Dim dims As New Dimension(size(0), size(1))

            Return spec($"input_layer(size=[{dims.x}, {dims.y}], depth={depth})",
                        Function(cnn) cnn.buildInputLayer(dims, depth))
        End Function

        ''' <summary>
        ''' 输入层（直接给出 <see cref="Dimension"/> 的版本）
        ''' </summary>
        Public Function input_layer(dims As Dimension, Optional depth As Integer = 1) As CNNLayerArguments
            Return spec($"input_layer(size=[{dims.x}, {dims.y}], depth={depth})",
                        Function(cnn) cnn.buildInputLayer(dims, depth))
        End Function

        ''' <summary>
        ''' 卷积层：用多个滤波器去提取局部特征（边缘、纹理等）。
        ''' </summary>
        ''' <param name="sx">滤波窗口边长（方窗）</param>
        ''' <param name="filters">滤波器个数，即输出通道数</param>
        ''' <param name="stride">滑动步长</param>
        ''' <param name="padding">零填充宽度</param>
        Public Function conv_layer(sx As Integer,
                                   filters As Integer,
                                   Optional stride As Integer = 1,
                                   Optional padding As Integer = 0) As CNNLayerArguments

            Return spec($"conv_layer(sx={sx}, filters={filters}, stride={stride}, padding={padding})",
                        Function(cnn) cnn.buildConvLayer(sx, filters, stride, padding))
        End Function

        ''' <summary>
        ''' 转置卷积层：把特征图上采样回更大的空间尺寸（自编码器的解码端、语义分割等场景）。
        ''' </summary>
        ''' <param name="dims">目标输出尺寸 ``{width, height, depth}``</param>
        ''' <param name="filter">滤波窗口 ``{width, height}``</param>
        ''' <param name="filters">滤波器个数</param>
        ''' <param name="stride">步长</param>
        Public Function conv_transpose_layer(dims As Integer(),
                                             filter As Integer(),
                                             Optional filters As Integer = 3,
                                             Optional stride As Integer = 1) As CNNLayerArguments

            If dims Is Nothing OrElse dims.Length < 3 Then
                Throw New ArgumentException("需要形如 {width, height, depth} 的输出尺寸", NameOf(dims))
            End If
            If filter Is Nothing OrElse filter.Length < 2 Then
                Throw New ArgumentException("需要形如 {width, height} 的滤波窗口", NameOf(filter))
            End If

            Dim out_dims As New OutputDefinition(dims(0), dims(1), dims(2))
            Dim window As New Dimension(filter(0), filter(1))

            Return spec($"conv_transpose_layer(dims=[{dims(0)}, {dims(1)}, {dims(2)}], filter=[{window.x}, {window.y}], filters={filters}, stride={stride})",
                        Function(cnn) cnn.buildConv2DTransposeLayer(out_dims, window, filters, stride))
        End Function

        ''' <summary>
        ''' 池化层：在窗口内做下采样，缩小特征图尺寸。
        ''' </summary>
        ''' <param name="sx">池化窗口边长（方窗）</param>
        ''' <param name="stride">步长</param>
        ''' <param name="padding">零填充宽度</param>
        Public Function pool_layer(sx As Integer, stride As Integer, padding As Integer) As CNNLayerArguments
            Return spec($"pool_layer(sx={sx}, stride={stride}, padding={padding})",
                        Function(cnn) cnn.buildPoolLayer(sx, stride, padding))
        End Function

        ''' <summary>
        ''' 全连接层：每个神经元与上一层全部输出相连。
        ''' </summary>
        ''' <param name="size">神经元个数</param>
        Public Function full_connected_layer(size As Integer) As CNNLayerArguments
            Return spec($"full_connected_layer(size={size})",
                        Function(cnn) cnn.buildFullyConnectedLayer(size))
        End Function

        ''' <summary>ReLU 激活：``f(x) = max(0, x)``</summary>
        Public Function relu_layer() As CNNLayerArguments
            Return spec(NameOf(relu_layer), Function(cnn) cnn.buildReLULayer())
        End Function

        ''' <summary>LeakyReLU 激活：负半轴保留一个小的斜率，避免神经元“死亡”</summary>
        Public Function leaky_relu_layer() As CNNLayerArguments
            Return spec(NameOf(leaky_relu_layer), Function(cnn) cnn.buildLeakyReLULayer())
        End Function

        ''' <summary>Sigmoid 激活：``f(x) = 1 / (1 + exp(-x))``，输出落在 (0, 1)</summary>
        Public Function sigmoid_layer() As CNNLayerArguments
            Return spec(NameOf(sigmoid_layer), Function(cnn) cnn.buildSigmoidLayer())
        End Function

        ''' <summary>Tanh 激活：输出落在 (-1, 1)</summary>
        Public Function tanh_layer() As CNNLayerArguments
            Return spec(NameOf(tanh_layer), Function(cnn) cnn.buildTanhLayer())
        End Function

        ''' <summary>Maxout 激活：在分组内取最大值</summary>
        Public Function maxout_layer() As CNNLayerArguments
            Return spec(NameOf(maxout_layer), Function(cnn) cnn.buildMaxoutLayer())
        End Function

        ''' <summary>高斯激活层</summary>
        Public Function gaussian_layer() As CNNLayerArguments
            Return spec(NameOf(gaussian_layer), Function(cnn) cnn.buildGaussian())
        End Function

        ''' <summary>
        ''' 局部响应归一化 (LRN)：在大响应神经元附近做侧抑制，增强高频特征的对比度
        ''' </summary>
        ''' <param name="n">参与归一化的邻域大小</param>
        Public Function lrn_layer(Optional n As Integer = 5) As CNNLayerArguments
            Return spec($"lrn_layer(n={n})", Function(cnn) cnn.buildLocalResponseNormalizationLayer(n))
        End Function

        ''' <summary>
        ''' Dropout 层：训练时随机丢弃一部分激活值，用于抑制过拟合
        ''' </summary>
        ''' <param name="drop_prob">丢弃概率</param>
        Public Function dropout_layer(Optional drop_prob As Double = 0.5) As CNNLayerArguments
            Return spec($"dropout_layer(drop_prob={drop_prob})",
                        Function(cnn) cnn.buildDropoutLayer(drop_prob))
        End Function

        ''' <summary>Softmax：把激活值压成 0~1 的概率分布（多分类的输出层）</summary>
        Public Function softmax_layer() As CNNLayerArguments
            Return spec(NameOf(softmax_layer), Function(cnn) cnn.buildSoftmaxLayer())
        End Function

        ''' <summary>回归损失层：连续值输出（自编码器、变分自编码器）的损失</summary>
        Public Function regression_layer() As CNNLayerArguments
            Return spec(NameOf(regression_layer), Function(cnn) cnn.buildRegressionLayer())
        End Function

    End Module
End Namespace
