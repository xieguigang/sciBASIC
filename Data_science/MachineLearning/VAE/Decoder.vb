Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Public MustInherit Class VAEEnc

    Protected weights As NumericMatrix
    Protected strides As Integer()
    Protected dims As Integer()
    Protected learning_rate As Double

    Public Overridable ReadOnly Property loss As Double

    ''' <summary>
    ''' initialize weights with Gaussian distribution
    ''' </summary>
    Protected MustOverride Sub init_random()

    Public MustOverride Sub update(output As Vector, input As Vector)
    Public MustOverride Function get_output(input As Vector) As Vector

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

        Me.weights = New NumericMatrix(kernel.First * kernel.Last, N)
        Me.strides = New Integer() {1, 1}
        Me.dims = New Integer() {kernel.First, kernel.Last}
        Me.learning_rate = learning_rate

        Call init_random()
    End Sub

    Protected Overrides Sub init_random()
        Dim dI As Integer = weights.Length
        Dim width As Integer = weights.ColumnDimension

        For i As Integer = 0 To dI - 1
            weights(i) = Vector.norm(width, mu:=0, sigma:=1)
        Next
    End Sub

    Public Overrides Sub update(output As Vector, input As Vector)
        Dim outX As New NumericMatrix(output.Array.Split(input.Dim))
        'Loss is the mean square between the real image and the generated image
        Dim dI As Integer = input.Dim
        Dim loss_div As Double = 2.0 / dI
        Dim m As Double = learning_rate

        outX = New NumericMatrix(outX.RowVectors.Select(Function(ov)
                                                            Dim delta As Vector = loss_div * (ov - input) * input
                                                            Return delta
                                                        End Function))


        ' weights += m * delta

        m_loss = loss
    End Sub

    Public Overrides Function get_output(input As Vector) As Vector
        ' matrix multiplication
        Dim output = weights.DotMultiply(input)

        ' leaky relu activation
        For i As Integer = 0 To output.Dim - 1
            If output(i) < 0 Then
                output(i) = 0.001
            End If
        Next

        Return output
    End Function
End Class
