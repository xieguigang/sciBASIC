Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Math

    ''' <summary>
    ''' implements torch.nn.Linear
    ''' </summary>
    Public Class Linear

        Dim w As NumericMatrix
        Dim b As Double = 0.0
        Dim bias As Boolean

        Sub New(in_features As Integer, out_features As Integer, Optional bias As Boolean = True)
            Me.bias = bias
            Me.w = New NumericMatrix(out_features, in_features)
        End Sub

        Public Function Fit(x As Vector) As Vector
            Return w.DotMultiply(x) + b
        End Function

        Public Sub backward(loss As Vector)
            w = w + loss

            If bias Then
                b += loss.Sum
            End If
        End Sub
    End Class
End Namespace