Namespace Distributions.BinBox

    Public Class Density

        Public Property axis As Double
        Public Property density As Double

        Public Shared Narrowing Operator CType(density As Density) As Double
            Return density.density
        End Operator

    End Class
End Namespace