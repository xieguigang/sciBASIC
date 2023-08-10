Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SIMD

Public Class Trainer

    Public ReadOnly Property autoencoder As VariationalAutoencoder

    Sub New(w As Integer, h As Integer, latent_dims As Integer)
        autoencoder = New VariationalAutoencoder(latent_dims)
        ' SIMDEnvironment.config = SIMDConfiguration.legacy
    End Sub

    Public Function train(data As IEnumerable(Of Vector),
                          Optional epochs As Integer = 20,
                          Optional learning_rate As Double = 0.00001) As VariationalAutoencoder

        Dim loess As New List(Of (Vector, Vector))
        Dim loss1 As Vector = Nothing
        Dim loss2 As Vector = Nothing

        For i As Integer = 0 To epochs
            For Each x As Vector In data
                Call backward(x, autoencoder.forward(x), learning_rate, loss1, loss2)
                Call loess.Add((loss1, loss2))

                Call VBDebugger.EchoLine(loss1.Sum + loss2.Sum)
            Next
        Next

        Return autoencoder
    End Function

    Private Sub backward(x As Vector, xi As Vector, learning_rate As Double,
                         <Out> ByRef loss1 As Vector,
                         <Out> ByRef loss2 As Vector)

        loss1 = ((x - xi)) * (autoencoder.encoder.kl * learning_rate)
        loss1(loss1 > 0.5) = Vector.Scalar(0.5)
        loss1(loss1 < -0.5) = Vector.Scalar(-0.5)

        Call autoencoder.decoder.backward(loss1)

        loss2 = ((autoencoder.encoder.forward(x) - autoencoder.encoder.forward(xi))) * (autoencoder.encoder.kl * learning_rate)
        loss2(loss2 > 0.5) = Vector.Scalar(0.5)
        loss2(loss2 < -0.5) = Vector.Scalar(-0.5)

        Call autoencoder.encoder.backward(loss2)
    End Sub
End Class
