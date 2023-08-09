Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Trainer

    Public Function train(autoencoder As VariationalAutoencoder,
                          data As IEnumerable(Of Vector),
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

    Public Function Reconstruct(autoencoder As VariationalAutoencoder, z As Vector) As Vector
        Return autoencoder.decoder.forward(z)
    End Function
End Module
