Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers

<Assembly: InternalsVisibleTo("MLkit")>

Namespace CNN

    Public Class LayerBuilder

        ReadOnly m_layers As New List(Of Layer)
        ReadOnly def As New OutputDefinition

        ''' <summary>
        ''' the layers object in this builder is already been initialized?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Initialized As Boolean = False

        Sub New()
            Initialized = False
        End Sub

        Sub New(initialized As Boolean)
            _Initialized = initialized
        End Sub

        Public Function add(layer As Layer) As LayerBuilder
            m_layers.Add(layer)
            Return Me
        End Function

        Public Overridable Function buildInputLayer(mapSize As Dimension, Optional depth As Integer = 1) As LayerBuilder
            Return add(New InputLayer(def, mapSize.x, mapSize.y, depth))
        End Function

        Public Function buildConvLayer(sx As Integer, filters As Integer, stride As Integer, padding As Integer) As LayerBuilder
            Return add(New ConvolutionLayer(def, sx, filters, stride, padding))
        End Function

        Public Function buildReLULayer() As LayerBuilder
            Return add(New RectifiedLinearUnitsLayer)
        End Function

        Public Function buildSigmoidLayer() As LayerBuilder
            Return add(New SigmoidLayer)
        End Function

        Public Function buildPoolLayer(sx As Integer, stride As Integer, padding As Integer) As LayerBuilder
            Return add(New PoolingLayer(def, sx, stride, padding))
        End Function

        Public Function buildFullyConnectedLayer(num_neurons As Integer) As LayerBuilder
            Return add(New FullyConnectedLayer(def, num_neurons))
        End Function

        Public Function buildTanhLayer() As LayerBuilder
            Return add(New TanhLayer)
        End Function

        ''' <summary>
        ''' LRN
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function buildLocalResponseNormalizationLayer(n As Integer) As LayerBuilder
            Return add(New LocalResponseNormalizationLayer(n))
        End Function

        Public Function buildDropoutLayer(Optional drop_prob As Double = 0.5) As LayerBuilder
            Return add(New DropoutLayer(def, drop_prob))
        End Function

        Public Function buildSoftmaxLayer() As LayerBuilder
            Return add(New SoftMaxLayer(def))
        End Function

        Public Function buildMaxoutLayer() As LayerBuilder
            Return add(New MaxoutLayer(def))
        End Function

        Public Function buildRegressionLayer() As LayerBuilder
            Return add(New RegressionLayer(def))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{m_layers.Count} CNN layers: {m_layers.JoinBy(" -> ")}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(lb As LayerBuilder) As List(Of Layer)
            Return lb.m_layers.AsEnumerable.AsList
        End Operator
    End Class
End Namespace