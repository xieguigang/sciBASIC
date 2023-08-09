Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Trainer

    Public ReadOnly Property autoencoder As VariationalAutoencoder

    Sub New(w As Integer, h As Integer, latent_dims As Integer)
        autoencoder = New VariationalAutoencoder(latent_dims)
    End Sub

    Public Function train(data As IEnumerable(Of Vector),
                          Optional epochs As Integer = 20,
                          Optional learning_rate As Double = 0.001) As VariationalAutoencoder

        For i As Integer = 0 To epochs
            For Each x As Vector In data
                Dim xi = autoencoder.forward(x)
                Dim loss = ((x - xi) ^ 2) + autoencoder.encoder.kl

                Call autoencoder.backward(loss * learning_rate)
            Next
        Next

        Return autoencoder
    End Function
End Class
