#Region "Microsoft.VisualBasic::4303da61e875f9cbd160a934ea3bcad8, Data_science\MachineLearning\DeepLearning\CNN\LayerBuilder.vb"

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

    '   Total Lines: 109
    '    Code Lines: 75 (68.81%)
    ' Comment Lines: 9 (8.26%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 25 (22.94%)
    '     File Size: 4.05 KB


    '     Class LayerBuilder
    ' 
    '         Properties: Initialized
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: add, buildConv2DTransposeLayer, buildConvLayer, buildDropoutLayer, buildFullyConnectedLayer
    '                   buildGaussian, buildInputLayer, buildLeakyReLULayer, buildLocalResponseNormalizationLayer, buildMaxoutLayer
    '                   buildPoolLayer, buildRegressionLayer, buildReLULayer, buildSigmoidLayer, buildSoftmaxLayer
    '                   buildTanhLayer, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers

<Assembly: InternalsVisibleTo("MLkit")>

Namespace CNN

    ''' <summary>
    ''' Builder that assembles the ordered layer list of a <see cref="ConvolutionalNN"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Layers are appended either through the fluent <c>+</c> operator (see <see cref="CNNLayers"/>) or through the
    ''' <c>buildXxxLayer</c> methods. Every <c>build</c> method instantiates the concrete layer with the builder's shared
    ''' <see cref="data.OutputDefinition"/>, so consecutive layers automatically inherit the correct input size.
    ''' </para>
    ''' <para>
    ''' A builder can be converted to <c>List(Of Layer)</c> through the narrowing <c>CType</c> operator, which is how
    ''' <see cref="ConvolutionalNN"/> consumes it.
    ''' </para>
    ''' </remarks>
    Public Class LayerBuilder

        ReadOnly m_layers As New List(Of Layer)
        ReadOnly def As New OutputDefinition

        ''' <summary>
        ''' Indicates whether the layers of this builder have already been initialized, for example after loading them from
        ''' a model file.
        ''' </summary>
        Public ReadOnly Property Initialized As Boolean = False

        ''' <summary>Creates an empty, not yet initialized builder.</summary>
        Sub New()
            Initialized = False
        End Sub

        ''' <summary>
        ''' Creates a builder that is marked as initialized, used when the layers are loaded from an existing model.
        ''' </summary>
        ''' <param name="initialized">The initial value of <see cref="Initialized"/>.</param>
        Sub New(initialized As Boolean)
            _Initialized = initialized
        End Sub

        ''' <summary>
        ''' Appends an already constructed layer and returns this builder so the fluent chain can continue.
        ''' </summary>
        ''' <param name="layer">The layer to append.</param>
        ''' <returns>This builder.</returns>
        Public Function add(layer As Layer) As LayerBuilder
            m_layers.Add(layer)
            Return Me
        End Function

        ''' <summary>
        ''' Fluent operator that appends a layer specification:
        ''' <c>New LayerBuilder() + conv_layer(5, 32, 1, 2)</c>.
        ''' </summary>
        ''' <param name="builder">The network being built.</param>
        ''' <param name="args">
        ''' A layer specification produced by a factory function of <see cref="CNNLayers"/>
        ''' (<c>input_layer</c>, <c>conv_layer</c>, ...).
        ''' </param>
        ''' <returns>The builder with the new layer appended.</returns>
        ''' <remarks>
        ''' The operator is evaluated from left to right, so
        ''' <c>New LayerBuilder() + input_layer(...) + conv_layer(...) + ...</c> produces exactly the same layer sequence as
        ''' calling <c>buildInputLayer</c>, <c>buildConvLayer</c>, ... in order.
        ''' </remarks>
        Public Shared Operator +(builder As LayerBuilder, args As CNNLayerArguments) As LayerBuilder
            If builder Is Nothing Then
                Throw New ArgumentNullException(NameOf(builder), $"请以 New {NameOf(LayerBuilder)}() 作为链式表达式的起点")
            End If
            If args Is Nothing Then
                Throw New ArgumentNullException(NameOf(args), "不能向网络里追加一个空的层规格")
            End If

            Return args.CreateLayer(builder)
        End Operator

        ''' <summary>
        ''' Fluent operator that appends an already constructed layer object: <c>builder + someLayer</c>.
        ''' </summary>
        ''' <param name="builder">The network being built.</param>
        ''' <param name="layer">The layer instance to append.</param>
        ''' <returns>The builder with the new layer appended.</returns>
        ''' <remarks>
        ''' Use this when you need to construct a layer yourself (for example to reuse an existing layer instance); for
        ''' ordinary network definitions prefer the layer specification factories in <see cref="CNNLayers"/>.
        ''' </remarks>
        Public Shared Operator +(builder As LayerBuilder, layer As Layer) As LayerBuilder
            If builder Is Nothing Then
                Throw New ArgumentNullException(NameOf(builder), $"请以 New {NameOf(LayerBuilder)}() 作为链式表达式的起点")
            End If
            If layer Is Nothing Then
                Throw New ArgumentNullException(NameOf(layer), "不能向网络里追加一个空层")
            End If

            Return builder.add(layer)
        End Operator

        ''' <summary>Appends a Gaussian activation layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildGaussian() As LayerBuilder
            Return add(New GaussianLayer(def))
        End Function

        ''' <summary>Appends a transposed (de)convolution layer.</summary>
        ''' <param name="dims">The target output size.</param>
        ''' <param name="filter">The filter window size.</param>
        ''' <param name="filters">Number of filters.</param>
        ''' <param name="stride">Stride.</param>
        ''' <returns>This builder.</returns>
        Public Function buildConv2DTransposeLayer(dims As OutputDefinition, filter As Dimension, filters As Integer, stride As Integer) As LayerBuilder
            Return add(New Conv2DTransposeLayer(def, dims, filter, filters, stride))
        End Function

        ''' <summary>Appends the input layer and declares the input image size.</summary>
        ''' <param name="mapSize">The input image size.</param>
        ''' <param name="depth">Number of input channels.</param>
        ''' <returns>This builder.</returns>
        Public Overridable Function buildInputLayer(mapSize As Dimension, Optional depth As Integer = 1) As LayerBuilder
            Return add(New InputLayer(def, mapSize.x, mapSize.y, depth))
        End Function

        ''' <summary>Appends a convolution layer.</summary>
        ''' <param name="sx">Side length of the square filter window.</param>
        ''' <param name="filters">Number of output filters.</param>
        ''' <param name="stride">Sliding stride.</param>
        ''' <param name="padding">Zero padding width.</param>
        ''' <returns>This builder.</returns>
        Public Function buildConvLayer(sx As Integer, filters As Integer, stride As Integer, padding As Integer) As LayerBuilder
            Return add(New ConvolutionLayer(def, sx, filters, stride, padding))
        End Function

        ''' <summary>Appends a ReLU activation layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildReLULayer() As LayerBuilder
            Return add(New RectifiedLinearUnitsLayer)
        End Function

        ''' <summary>Appends a LeakyReLU activation layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildLeakyReLULayer() As LayerBuilder
            Return add(New LeakyReluLayer)
        End Function

        ''' <summary>Appends a sigmoid activation layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildSigmoidLayer() As LayerBuilder
            Return add(New SigmoidLayer)
        End Function

        ''' <summary>Appends a max pooling layer.</summary>
        ''' <param name="sx">Side length of the square pooling window.</param>
        ''' <param name="stride">Stride of the pooling window.</param>
        ''' <param name="padding">Zero padding width.</param>
        ''' <returns>This builder.</returns>
        Public Function buildPoolLayer(sx As Integer, stride As Integer, padding As Integer) As LayerBuilder
            Return add(New PoolingLayer(def, sx, stride, padding))
        End Function

        ''' <summary>Appends a fully connected layer.</summary>
        ''' <param name="num_neurons">Number of neurons.</param>
        ''' <returns>This builder.</returns>
        Public Function buildFullyConnectedLayer(num_neurons As Integer) As LayerBuilder
            Return add(New FullyConnectedLayer(def, num_neurons))
        End Function

        ''' <summary>Appends a tanh activation layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildTanhLayer() As LayerBuilder
            Return add(New TanhLayer)
        End Function

        ''' <summary>
        ''' Appends a local response normalization (LRN) layer.
        ''' </summary>
        ''' <param name="n">Size of the normalization neighborhood.</param>
        ''' <returns>This builder.</returns>
        Public Function buildLocalResponseNormalizationLayer(n As Integer) As LayerBuilder
            Return add(New LocalResponseNormalizationLayer(n))
        End Function

        ''' <summary>Appends a dropout layer.</summary>
        ''' <param name="drop_prob">Probability of dropping an activation.</param>
        ''' <returns>This builder.</returns>
        Public Function buildDropoutLayer(Optional drop_prob As Double = 0.5) As LayerBuilder
            Return add(New DropoutLayer(def, drop_prob))
        End Function

        ''' <summary>Appends a softmax loss layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildSoftmaxLayer() As LayerBuilder
            Return add(New SoftMaxLayer(def))
        End Function

        ''' <summary>Appends a maxout activation layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildMaxoutLayer() As LayerBuilder
            Return add(New MaxoutLayer(def))
        End Function

        ''' <summary>Appends a regression loss layer.</summary>
        ''' <returns>This builder.</returns>
        Public Function buildRegressionLayer() As LayerBuilder
            Return add(New RegressionLayer(def))
        End Function

        ''' <summary>Returns a one line description of the layers collected so far.</summary>
        ''' <returns>A text that lists the layers joined by <c>-&gt;</c>.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{m_layers.Count} CNN layers: {m_layers.JoinBy(" -> ")}"
        End Function

        ''' <summary>
        ''' Narrowing conversion that exposes the collected layers as a <c>List(Of Layer)</c>.
        ''' </summary>
        ''' <param name="lb">The builder to convert.</param>
        ''' <returns>The list of layers held by the builder.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(lb As LayerBuilder) As List(Of Layer)
            Return lb.m_layers.AsEnumerable.AsList
        End Operator
    End Class
End Namespace
