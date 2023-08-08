Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Vae

    Public ReadOnly Property encoder As Encoder
    Public ReadOnly Property decoder As Decoder

    Dim NUMBER_WEIGHTS As Integer
    Dim generated_image As Vector
    Dim latent_variables As Vector

    Public ReadOnly Property loss As Double
        Get
            Return encoder.loss + decoder.loss
        End Get
    End Property

    Sub New(N1 As Integer, N2 As Integer, Optional W2 As Integer = 100)
        generated_image = New Vector(m:=N1 * N2)
        latent_variables = New Vector(m:=N1 * W2)
        set_encoder(N2, W2)
        set_decoder(W2, N2)
    End Sub

    Public Sub set_encoder(s1 As Integer, s2 As Integer)
        _encoder = New Encoder(NUMBER_WEIGHTS, {s1, s2}, {1, 1})
    End Sub

    Public Sub set_decoder(s1 As Integer, s2 As Integer)
        _decoder = New Decoder(NUMBER_WEIGHTS, {s1, s2}, {1, 1})
    End Sub

    Friend Sub update(input() As Double)
        decoder.update(generated_image, input)
        encoder.update(latent_variables, input)
    End Sub

    Friend Sub decode()
        decoder.get_output(generated_image, latent_variables)
    End Sub

    Friend Sub encode(input() As Double)
        encoder.get_output(latent_variables, input)
    End Sub
End Class
