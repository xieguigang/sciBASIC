Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class VariationalAutoencoder

    Public ReadOnly encoder As VariationalEncoder
    Public ReadOnly decoder As Decoder

    Sub New(latent_dims As Integer)
        encoder = New VariationalEncoder(latent_dims)
        decoder = New Decoder(latent_dims)
    End Sub

    Public Function forward(x As Vector) As Vector
        x = encoder.forward(x)
        x = decoder.forward(x)
        Return x
    End Function

    Public Sub backward(loss As Vector)
        Call decoder.backward(loss)
        Call encoder.backward(loss)
    End Sub
End Class
