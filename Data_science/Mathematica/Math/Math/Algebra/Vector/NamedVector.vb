
Namespace LinearAlgebra

    ''' <summary>
    ''' <see cref="Dictionary(Of String, Double)"/>
    ''' </summary>
    Public Class NamedVector

        Dim factors As Factor(Of String)()
        Dim vector As Vector

        Sub New(vector As Dictionary(Of String, Double))
            Me.factors = FactorExtensions.factors(vector.Keys)
            Me.vector = factors _
                .Select(Function(factor) vector(factor.FactorValue)) _
                .AsVector
        End Sub

        Public Overrides Function ToString() As String
            Return vector.ToString
        End Function
    End Class
End Namespace