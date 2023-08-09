Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class VariationalEncoder

    Dim linear1 As Linear
    Dim linear2 As Linear
    Dim linear3 As Linear

    Public ReadOnly Property kl As Double = 0

    Sub New(latent_dims As Integer)
        linear1 = New Linear(784, 512)
        linear2 = New Linear(512, latent_dims)
        linear3 = New Linear(512, latent_dims)
    End Sub

    Public Function forward(x As Vector)
        x = ReLU.ReLU(linear1.Fit(x))

        Dim mu = linear2.Fit(x)
        Dim sigma = linear3.Fit(x).Exp
        Dim z = mu + sigma * Vector.norm(size:=mu.Length, mu:=0, sigma:=1)
        _kl = (sigma ^ 2 + mu ^ 2 - sigma.Log - 1 / 2).Sum
        Return z
    End Function

    Public Sub backward(loss As Vector)
        Call linear1.backward(loss)
        Call linear2.backward(loss)
        Call linear3.backward(loss)
    End Sub
End Class
