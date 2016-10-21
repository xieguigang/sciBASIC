Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LP

    Public Structure ObjectiveFunction

        Dim type As OptimizationType
        Dim xyz#()
        Dim Z#

        Public Overrides Function ToString() As String
            Dim eq As New Equation With {
                .c = Z,
                .xyz = xyz
            }
            Return $"{type.ToString}( {eq.ToString} )"
        End Function
    End Structure
End Namespace