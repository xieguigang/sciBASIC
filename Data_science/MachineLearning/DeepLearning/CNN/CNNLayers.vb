#Region "Microsoft.VisualBasic::2c31ff3add9bb534bef9fff02436ab3e, Data_science\MachineLearning\DeepLearning\CNN\CNNLayers.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 216
    '    Code Lines: 81 (37.50%)
    ' Comment Lines: 109 (50.46%)
    '    - Xml Docs: 99.08%
    ' 
    '   Blank Lines: 26 (12.04%)
    '     File Size: 11.60 KB


    '     Module CNNLayers
    ' 
    '         Function: conv_layer, conv_transpose_layer, dropout_layer, full_connected_layer, gaussian_layer
    '                   (+2 Overloads) input_layer, leaky_relu_layer, lrn_layer, maxout_layer, pool_layer
    '                   regression_layer, relu_layer, sigmoid_layer, softmax_layer, spec
    '                   tanh_layer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN

    ''' <summary>
    ''' Factory functions for building the layer specifications of a CNN network.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The function names correspond one to one with the MLkit of R#
    ''' (<c>studio\Rsharp_kit\MLkit\MachineLearning\CNN.vb</c>), so a network definition written in an R# script can
    ''' be translated line by line into VB. Taking <c>tutorials\..\CNN_image\auto_encoder.R</c> as an example:
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
    ''' The only difference is that VB requires the binary operator at the end of the previous line for an implicit
    ''' line continuation, whereas R# allows it at the start of the next line.
    ''' </para>
    ''' <para>
    ''' These functions all return <see cref="CNNLayerArguments"/> objects (layers that have not been created yet).
    ''' The <see cref="LayerBuilder"/> <c>+</c> operator instantiates them from left to right, so the resulting layer
    ''' sequence is identical to calling <c>buildXxxLayer</c> for every layer individually.
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
        ''' Input layer: passes the data into the network and declares the image size and channel count.
        ''' </summary>
        ''' <param name="size">The image size, given as <c>{width, height}</c>.</param>
        ''' <param name="depth">Number of channels; 1 for grayscale images.</param>
        ''' <returns>The input layer specification.</returns>
        Public Function input_layer(size As Integer(), Optional depth As Integer = 1) As CNNLayerArguments
            If size Is Nothing OrElse size.Length < 2 Then
                Throw New ArgumentException("需要形如 {width, height} 的图像尺寸", NameOf(size))
            End If

            Dim dims As New Dimension(size(0), size(1))

            Return spec($"input_layer(size=[{dims.x}, {dims.y}], depth={depth})",
                        Function(cnn) cnn.buildInputLayer(dims, depth))
        End Function

        ''' <summary>
        ''' Input layer variant that takes the image size directly as a <see cref="Dimension"/>.
        ''' </summary>
        ''' <param name="dims">The image size.</param>
        ''' <param name="depth">Number of channels; 1 for grayscale images.</param>
        ''' <returns>The input layer specification.</returns>
        Public Function input_layer(dims As Dimension, Optional depth As Integer = 1) As CNNLayerArguments
            Return spec($"input_layer(size=[{dims.x}, {dims.y}], depth={depth})",
                        Function(cnn) cnn.buildInputLayer(dims, depth))
        End Function

        ''' <summary>
        ''' Convolution layer: extracts local features (edges, textures and so on) with a bank of filters.
        ''' </summary>
        ''' <param name="sx">Side length of the (square) filter window.</param>
        ''' <param name="filters">Number of filters, i.e. the number of output channels.</param>
        ''' <param name="stride">Sliding step of the filter window.</param>
        ''' <param name="padding">Width of the zero padding applied to the input.</param>
        ''' <returns>The convolution layer specification.</returns>
        Public Function conv_layer(sx As Integer,
                                   filters As Integer,
                                   Optional stride As Integer = 1,
                                   Optional padding As Integer = 0) As CNNLayerArguments

            Return spec($"conv_layer(sx={sx}, filters={filters}, stride={stride}, padding={padding})",
                        Function(cnn) cnn.buildConvLayer(sx, filters, stride, padding))
        End Function

        ''' <summary>
        ''' Transposed convolution layer: upsamples a feature map back to a larger spatial size (decoder side of an
        ''' auto encoder, semantic segmentation, and similar scenarios).
        ''' </summary>
        ''' <param name="dims">Target output size <c>{width, height, depth}</c>.</param>
        ''' <param name="filter">Filter window <c>{width, height}</c>.</param>
        ''' <param name="filters">Number of filters.</param>
        ''' <param name="stride">Stride.</param>
        ''' <returns>The transposed convolution layer specification.</returns>
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
        ''' Pooling layer: downsamples the feature map inside a sliding window.
        ''' </summary>
        ''' <param name="sx">Side length of the (square) pooling window.</param>
        ''' <param name="stride">Stride of the pooling window.</param>
        ''' <param name="padding">Width of the zero padding applied to the input.</param>
        ''' <returns>The pooling layer specification.</returns>
        Public Function pool_layer(sx As Integer, stride As Integer, padding As Integer) As CNNLayerArguments
            Return spec($"pool_layer(sx={sx}, stride={stride}, padding={padding})",
                        Function(cnn) cnn.buildPoolLayer(sx, stride, padding))
        End Function

        ''' <summary>
        ''' Fully connected layer: every neuron is connected to the complete output of the previous layer.
        ''' </summary>
        ''' <param name="size">Number of neurons.</param>
        ''' <returns>The fully connected layer specification.</returns>
        Public Function full_connected_layer(size As Integer) As CNNLayerArguments
            Return spec($"full_connected_layer(size={size})",
                        Function(cnn) cnn.buildFullyConnectedLayer(size))
        End Function

        ''' <summary>ReLU activation: <c>f(x) = max(0, x)</c>.</summary>
        ''' <returns>The ReLU layer specification.</returns>
        Public Function relu_layer() As CNNLayerArguments
            Return spec(NameOf(relu_layer), Function(cnn) cnn.buildReLULayer())
        End Function

        ''' <summary>
        ''' LeakyReLU activation: keeps a small slope on the negative half axis so neurons do not "die".
        ''' </summary>
        ''' <returns>The LeakyReLU layer specification.</returns>
        Public Function leaky_relu_layer() As CNNLayerArguments
            Return spec(NameOf(leaky_relu_layer), Function(cnn) cnn.buildLeakyReLULayer())
        End Function

        ''' <summary>Sigmoid activation: <c>f(x) = 1 / (1 + exp(-x))</c>, with output in <c>(0, 1)</c>.</summary>
        ''' <returns>The sigmoid layer specification.</returns>
        Public Function sigmoid_layer() As CNNLayerArguments
            Return spec(NameOf(sigmoid_layer), Function(cnn) cnn.buildSigmoidLayer())
        End Function

        ''' <summary>Tanh activation: output in <c>(-1, 1)</c>.</summary>
        ''' <returns>The tanh layer specification.</returns>
        Public Function tanh_layer() As CNNLayerArguments
            Return spec(NameOf(tanh_layer), Function(cnn) cnn.buildTanhLayer())
        End Function

        ''' <summary>Maxout activation: takes the maximum value inside each group.</summary>
        ''' <returns>The maxout layer specification.</returns>
        Public Function maxout_layer() As CNNLayerArguments
            Return spec(NameOf(maxout_layer), Function(cnn) cnn.buildMaxoutLayer())
        End Function

        ''' <summary>Gaussian activation layer.</summary>
        ''' <returns>The Gaussian layer specification.</returns>
        Public Function gaussian_layer() As CNNLayerArguments
            Return spec(NameOf(gaussian_layer), Function(cnn) cnn.buildGaussian())
        End Function

        ''' <summary>
        ''' Local response normalization (LRN): lateral inhibition around strongly responding neurons increases the
        ''' contrast of high frequency features.
        ''' </summary>
        ''' <param name="n">Size of the neighborhood involved in the normalization.</param>
        ''' <returns>The LRN layer specification.</returns>
        Public Function lrn_layer(Optional n As Integer = 5) As CNNLayerArguments
            Return spec($"lrn_layer(n={n})", Function(cnn) cnn.buildLocalResponseNormalizationLayer(n))
        End Function

        ''' <summary>
        ''' Dropout layer: randomly drops a fraction of the activations during training to reduce overfitting.
        ''' </summary>
        ''' <param name="drop_prob">Probability of dropping an activation.</param>
        ''' <returns>The dropout layer specification.</returns>
        Public Function dropout_layer(Optional drop_prob As Double = 0.5) As CNNLayerArguments
            Return spec($"dropout_layer(drop_prob={drop_prob})",
                        Function(cnn) cnn.buildDropoutLayer(drop_prob))
        End Function

        ''' <summary>Softmax: turns the activations into a probability distribution in <c>[0, 1]</c> (multi-class output layer).</summary>
        ''' <returns>The softmax layer specification.</returns>
        Public Function softmax_layer() As CNNLayerArguments
            Return spec(NameOf(softmax_layer), Function(cnn) cnn.buildSoftmaxLayer())
        End Function

        ''' <summary>Regression loss layer: the loss used for continuous outputs (auto encoders, variational auto encoders).</summary>
        ''' <returns>The regression layer specification.</returns>
        Public Function regression_layer() As CNNLayerArguments
            Return spec(NameOf(regression_layer), Function(cnn) cnn.buildRegressionLayer())
        End Function

    End Module
End Namespace

