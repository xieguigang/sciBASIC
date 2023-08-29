Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.layers

    ''' <summary>
    ''' This is a layer of neurons that applies the non-saturating activation
    ''' function f(x)=max(0,x). It increases the nonlinear properties of the
    ''' decision function and of the overall network without affecting the
    ''' receptive fields of the convolution layer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    ''' <remarks>
    ''' ReLU
    ''' </remarks>
    Public Class RectifiedLinearUnitsLayer : Inherits DataLink
        Implements Layer

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.ReLU
            End Get
        End Property

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            Dim V2 As DataBlock = db.clone()
            Dim N = db.Weights.Length
            Dim V2w = V2.Weights
            For i = 0 To N - 1
                If V2w(i) < 0 Then
                    V2.setGradient(i, 0) ' threshold at 0
                End If
            Next
            out_act = V2
            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Dim V = in_act ' we need to set dw of this
            Dim V2 = out_act
            Dim N = V.Weights.Length
            V.clearGradient() ' zero out gradient wrt data
            For i = 0 To N - 1
                If V2.getWeight(i) <= 0 Then
                    V.setGradient(i, 0) ' threshold
                Else
                    V.setGradient(i, V2.getGradient(i))
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"ReLU()"
        End Function
    End Class

End Namespace
