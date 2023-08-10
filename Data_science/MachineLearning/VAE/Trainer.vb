Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Trainer

    Public ReadOnly Property autoencoder As VariationalAutoencoder

    Sub New(w As Integer, h As Integer, latent_dims As Integer)
        autoencoder = New VariationalAutoencoder(latent_dims)
    End Sub

    Public Function train(data As IEnumerable(Of Vector),
                          Optional epochs As Integer = 20,
                          Optional learning_rate As Double = 0.00001) As VariationalAutoencoder

        For i As Integer = 0 To epochs
            For Each x As Vector In data
                Call backward(x, autoencoder.forward(x), learning_rate)
            Next
        Next

        Return autoencoder
    End Function

    Private Sub backward(x As Vector, xi As Vector, learning_rate As Double)
        Dim loss = ((x - xi) ^ 2) * autoencoder.encoder.kl * learning_rate

        loss(loss > 0.5) = Vector.Scalar(0.5)
        loss(loss < -0.5) = Vector.Scalar(-0.5)

        Call autoencoder.decoder.backward(loss)

        loss = ((autoencoder.encoder.forward(x) - autoencoder.encoder.forward(xi)) ^ 2) * autoencoder.encoder.kl * learning_rate

        loss(loss > 0.5) = Vector.Scalar(0.5)
        loss(loss < -0.5) = Vector.Scalar(-0.5)

        Call autoencoder.encoder.backward(loss)
    End Sub
End Class
