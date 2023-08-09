Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Decoder

    Dim linear1 As Linear
    Dim linear2 As Linear

    Sub New(latent_dims As Integer)
        linear1 = New Linear(latent_dims, 512)
        linear2 = New Linear(512, 784)
    End Sub

    Public Function forward(z As Vector) As Vector
        z = ReLU.ReLU(linear1.Fit(z))
        z = SigmoidFunction.Sigmoid(linear2.Fit(z))
        Return z
    End Function

    Public Sub backward(loss As Vector)
        Call linear1.backward(loss)
        Call linear2.backward(loss)
    End Sub
End Class
