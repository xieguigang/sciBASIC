Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Public Class Encoder : Inherits VAEEnc

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

        Me.weights = New NumericMatrix(N, kernel.First * kernel.Last)
        Me.strides = strides
        Me.dims = kernel.ToArray
        Me.learning_rate = learning_rate

        Call init_random()
    End Sub

    Public Overrides Sub update(output As Vector, input As Vector)
        ' loss is KL-divergence between a gaussian distribution and the output
        Dim I As Integer = std.Min(output.Dim, input.Dim)
        Dim unit_gaussian As Double
        Dim delta As Double = 0
        Dim KL_div As Double = 0
        Dim m As Double = learning_rate

        ' update weights
        For itr As Integer = 0 To I
            unit_gaussian = randf.NextGaussian(mu:=0, sigma:=1)
            '????
            KL_div += unit_gaussian * std.Log(output(itr), 2) - unit_gaussian * std.Log(output(itr), 2)
            delta = (unit_gaussian / output(itr)) * input(itr)

            If output(itr) < 0 Then
                delta *= 0.001
            End If

            weights(itr) += m * delta
        Next

        m_loss = KL_div
    End Sub

    Public Overrides Sub get_output(output As Vector, input As Vector)
        Dim dI As Integer = input.Dim / dims(0)
        Dim dJ As Integer = dims(1)
        Dim dK As Integer = dims(0)

        For i As Integer = 0 To output.Dim - 1
            output(i) = 0.0
        Next

        ' matrix multiplication
        'For i As Integer = 0 To dI - 1
        '    For k As Integer = 0 To dK - 1
        '        For j As Integer = 0 To dJ - 1
        '            output(i * dI + j) += weights(i * dK + k) * input(i * k + j)
        '        Next
        '    Next
        'Next

        output = weights.Multiply(input).ColumnVector(0)

        ' leaky relu activation
        For i As Integer = 0 To dI * dJ - 1
            If output(i) < 0 Then
                output(i) = 0.001
            End If
        Next
    End Sub

    Protected Overrides Sub init_random()
        Dim dI As Integer = weights.Length
        Dim width As Integer = weights.ColumnDimension

        For i As Integer = 0 To dI - 1
            weights(i) = Vector.norm(width, mu:=0, sigma:=1)
        Next
    End Sub
End Class
