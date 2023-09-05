Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.layers

    Public Class LeakyReluLayer : Inherits RectifiedLinearUnitsLayer
        Implements Layer

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.LeakyReLU
            End Get
        End Property

        Dim leakySlope As Double = 0.01

        Sub New(Optional leakySlope As Double = 0.01)
            Me.leakySlope = leakySlope
        End Sub

        Sub New()
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            Dim V2 As DataBlock = db.clone()
            Dim N = db.Weights.Length
            Dim V2w = V2.Weights

            in_act = db

            For i As Integer = 0 To N - 1
                If V2w(i) < threshold Then
                    V2.setWeight(i, V2w(i) * leakySlope) ' threshold at 0
                End If
            Next

            out_act = V2

            Return out_act
        End Function

        Public Overrides Sub backward()
            ' zero out gradient wrt data
            Dim V = in_act.clearGradient() ' we need to set dw of this
            Dim V2 = out_act
            Dim N = V.Weights.Length

            For i As Integer = 0 To N - 1
                If V2.getWeight(i) <= threshold Then
                    V.setGradient(i, V2.getGradient(i) * leakySlope) ' threshold
                Else
                    V.setGradient(i, V2.getGradient(i))
                End If
            Next
        End Sub
    End Class
End Namespace