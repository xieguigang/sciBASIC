Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Trainer

    Public ReadOnly Property autoencoder As VariationalAutoencoder

    Sub New(w As Integer, h As Integer, latent_dims As Integer)
        autoencoder = New VariationalAutoencoder(latent_dims)
    End Sub

    Public Function train(data As IEnumerable(Of Vector),
                          Optional epochs As Integer = 20,
                          Optional learning_rate As Double = 0.00001) As VariationalAutoencoder

        Dim loess As New List(Of Vector)
        Dim loss As Vector

        For i As Integer = 0 To epochs
            For Each x As Vector In data
                loss = backward(x, autoencoder.forward(x), learning_rate)
                loess.Add(loss)

                Call VBDebugger.EchoLine(loss.Sum)
            Next
        Next

        Return autoencoder
    End Function

    Private Function backward(x As Vector, xi As Vector, learning_rate As Double) As Vector
        Dim loss1 = ((x - xi)) * autoencoder.encoder.kl * learning_rate

        loss1(loss1 > 0.5) = Vector.Scalar(0.5)
        loss1(loss1 < -0.5) = Vector.Scalar(-0.5)

        Call autoencoder.decoder.backward(loss1)

        Dim loss2 = ((autoencoder.encoder.forward(x) - autoencoder.encoder.forward(xi))) * autoencoder.encoder.kl * learning_rate

        loss2(loss2 > 0.5) = Vector.Scalar(0.5)
        loss2(loss2 < -0.5) = Vector.Scalar(-0.5)

        Call autoencoder.encoder.backward(loss2)

        Return loss1 + loss2
    End Function
End Class
