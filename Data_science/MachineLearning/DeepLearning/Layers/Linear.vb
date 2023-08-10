Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Math

    ''' <summary>
    ''' implements torch.nn.Linear
    ''' </summary>
    Public Class Linear

        Dim w As NumericMatrix
        Dim b As Vector
        Dim bias As Boolean

        Sub New(in_features As Integer, out_features As Integer, Optional bias As Boolean = True)
            Me.bias = bias
            Me.w = New NumericMatrix(gauss(in_features, out_features))
            Me.b = Vector.Zero(out_features)
        End Sub

        Private Iterator Function gauss(in_features As Integer, out_features As Integer) As IEnumerable(Of Vector)
            For i As Integer = 0 To out_features - 1
                Yield Vector.rand(in_features)
            Next
        End Function

        Public Function Fit(x As Vector) As Vector
            Return w.DotMultiply(x) + b
        End Function

        Public Sub backward(loss As Vector)
            w = w - loss

            If bias Then
                b -= loss
            End If
        End Sub
    End Class
End Namespace