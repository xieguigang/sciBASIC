Imports System
Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace Convolutional

    Public Class SoftMax : Inherits Layer

        Public Overrides ReadOnly Property type As LayerTypes
            Get
                Return LayerTypes.SoftMax
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(inputTensorDims As Integer())
            Call MyBase.New(inputTensorDims)
        End Sub

        Protected Overrides Function layerFeedNext() As Layer
            Dim max = Single.MinValue

            For i As Integer = 0 To inputTensor.TotalLength - 1
                If inputTensor.data(i) > max Then
                    max = inputTensor.data(i)
                End If
            Next

            Dim sum As Single = 0
            Dim nLMR As Single() = nextLayer.inputTensor.data

            For i As Integer = 0 To inputTensor.TotalLength - 1
                nLMR(i) = CSng(stdNum.Exp(inputTensor.data(i) - max))
                sum += nLMR(i)
            Next

            For i As Integer = 0 To inputTensor.TotalLength - 1
                nLMR(i) /= sum
            Next

            Call disposeInputTensor()

            Return Me
        End Function
    End Class
End Namespace
