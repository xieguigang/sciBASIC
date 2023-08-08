Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Public MustInherit Class VAEEnc

    Protected weights As Double()
    Protected strides As Integer()
    Protected dims As Integer()
    Protected learning_rate As Double

    Public Overridable ReadOnly Property loss As Double

    ''' <summary>
    ''' initialize weights with Gaussian distribution
    ''' </summary>
    Protected MustOverride Sub init_random()

    Public MustOverride Sub update(output As Vector, input As Vector)
    Public MustOverride Sub get_output(output As Vector, input As Vector)

End Class

' N = 2800?

Public Class Decoder : Inherits VAEEnc

    Dim m_loss As Double

    Public Overrides ReadOnly Property loss As Double
        Get
            Return m_loss
        End Get
    End Property

    Sub New(N As Integer,
            kernel As Integer(),
            strides As Integer(),
            Optional learning_rate As Double = 0.00001)

        Me.weights = New Double(N - 1) {}
        Me.strides = New Integer() {1, 1}
        Me.dims = New Integer() {kernel.First, kernel.Last}
        Me.learning_rate = learning_rate

        Call init_random()
    End Sub

    Protected Overrides Sub init_random()
        Dim dI As Integer = weights.Length

        For i As Integer = 0 To dI - 1
            weights(i) = randf.NextGaussian(mu:=0, sigma:=1)
        Next
    End Sub

    Public Overrides Sub update(output As Vector, input As Vector)
        'Loss is the mean square between the real image and the generated image
        Dim dI As Integer = input.Dim
        Dim loss As Double = 0
        Dim delta As Double = 0
        Dim loss_div As Double = 2.0 / dI
        Dim m As Double = learning_rate

        For i As Integer = 0 To dI - 1
            delta = loss_div * (output(i) - input(i)) * input(i)
            loss += loss_div * std.Pow(output(i) - input(i), 2)

            If output(i) < 0 Then
                delta *= 0.001
            End If

            weights(i) += m * delta
        Next

        m_loss = loss
    End Sub

    Public Overrides Sub get_output(output As Vector, input As Vector)
        Dim dI As Integer = input.Dim / dims(0)
        Dim dJ As Integer = dims(1)
        Dim dK As Integer = dims(0)

        For i As Integer = 0 To output.Dim - 1
            output(i) = 0
        Next

        ' matrix multiplication
        For i As Integer = 0 To dI - 1
            For k As Integer = 0 To dK - 1
                For j As Integer = 0 To dJ - 1
                    output(i * dI + j) += weights(i * dK + k) * input(i * k + j)
                Next
            Next
        Next

        ' leaky relu activation
        For i As Integer = 0 To dI * dJ - 1
            If output(i) < 0 Then
                output(i) = 0.001
            End If
        Next
    End Sub
End Class
