Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' Implements Sigmoid nonlinearity elementwise x to 1/(1+e^(-x))
    ''' so the output is between 0 and 1.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    <Serializable>
    Public Class SigmoidLayer : Inherits DataLink
        Implements Layer

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Sigmoid
            End Get
        End Property

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            Dim V2 As DataBlock = db.cloneAndZero()
            Dim N = db.Weights.Length
            For i = 0 To N - 1
                V2.setWeight(i, 1.0 / (1.0 + std.Exp(-V2.getWeight(i))))
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
                Dim v2wi = V2.getWeight(i)
                V.setGradient(i, v2wi * (1.0 - v2wi) * V2.getGradient(i))
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return "sigmoid()"
        End Function
    End Class

End Namespace
