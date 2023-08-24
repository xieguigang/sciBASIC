Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

<Assembly: InternalsVisibleTo("MLkit")>

Namespace CNN

    Public Class LayerBuilder

        ReadOnly m_layers As New List(Of Layer)

        ''' <summary>
        ''' the layers object in this builder is already been initialized?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Initialized As Boolean = False

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(layer As Layer)
            Initialized = False
            m_layers = New List(Of Layer) From {layer}
        End Sub

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

        Public Overridable Function buildInputLayer(mapSize As Dimension) As LayerBuilder
            m_layers.Add(Layer.buildInputLayer(mapSize))
            Return Me
        End Function

        Public Function buildConvLayer(outMapNum As Integer, kernelSize As Dimension) As LayerBuilder
            m_layers.Add(Layer.buildConvLayer(outMapNum, kernelSize))
            Return Me
        End Function

        Public Function buildSampLayer(scaleSize As Dimension) As LayerBuilder
            m_layers.Add(Layer.buildSampLayer(scaleSize))
            Return Me
        End Function

        Public Function buildOutputLayer(classNum As Integer) As LayerBuilder
            m_layers.Add(Layer.buildOutputLayer(classNum))
            Return Me
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

    Partial Class Layer

        Friend Shared Function buildInputLayer(mapSize As Dimension) As Layer
            Dim layer As Layer = New Layer()
            layer._Type = LayerTypes.Input
            layer._OutMapNum = 1
            layer._MapSize = mapSize
            Return layer
        End Function

        Friend Shared Function buildConvLayer(outMapNum As Integer, kernelSize As Dimension) As Layer
            Dim layer As Layer = New Layer()
            layer._Type = LayerTypes.Convolution
            layer._OutMapNum = outMapNum
            layer._KernelSize = kernelSize
            Return layer
        End Function

        Friend Shared Function buildSampLayer(scaleSize As Dimension) As Layer
            Dim layer As Layer = New Layer()
            layer._Type = LayerTypes.samp
            layer._ScaleSize = scaleSize
            Return layer
        End Function

        Friend Shared Function buildOutputLayer(classNum As Integer) As Layer
            Dim layer As New Layer()
            layer._ClassNum = classNum
            layer._Type = LayerTypes.Output
            layer._MapSize = New Dimension(1, 1)
            layer._OutMapNum = classNum

            Return layer
        End Function
    End Class

End Namespace