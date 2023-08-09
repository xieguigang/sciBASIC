Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Math

    ''' <summary>
    ''' implements torch.nn.Linear
    ''' </summary>
    Public Class Linear

        Dim w As NumericMatrix
        Dim b As Double

        Sub New(in_features As Integer, out_features As Integer, Optional bias As Boolean = True)
            w = New NumericMatrix(in_features, out_features)
        End Sub

        Public Function Fit(x As Vector) As Vector
            Return w.DotMultiply(x) + b
        End Function
    End Class
End Namespace