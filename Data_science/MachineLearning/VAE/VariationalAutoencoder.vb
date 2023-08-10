Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class VariationalAutoencoder

    Public ReadOnly encoder As VariationalEncoder
    Public ReadOnly decoder As Decoder

    Sub New(latent_dims As Integer)
        encoder = New VariationalEncoder(latent_dims)
        decoder = New Decoder(latent_dims)
    End Sub

    Public Function Reconstruct(z As Vector) As Vector
        Return decoder.forward(z)
    End Function

    Public Function forward(x As Vector) As Vector
        x = encoder.forward(x)
        x = decoder.forward(x)
        Return x
    End Function
End Class
