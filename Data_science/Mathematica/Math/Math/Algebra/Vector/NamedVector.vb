
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' Factory for <see cref="Dictionary(Of String, Double)"/> to <see cref="Vector"/>
    ''' </summary>
    Public Class NamedVectorFactory

        ReadOnly factors As Factor(Of String)()

        Sub New(factors As IEnumerable(Of String))
            Me.factors = FactorExtensions.factors(factors)
        End Sub

        Public Function AsVector(data As Dictionary(Of String, Double)) As Vector
            Dim vector#() = New Double(factors.Length - 1) {}

            For Each factor As Factor(Of String) In factors
                vector(factor.Value) = data(factor)
            Next

            Return vector.AsVector
        End Function

        Public Overrides Function ToString() As String
            Return factors _
                .Select(Function(factor) factor.FactorValue) _
                .ToArray _
                .GetJson
        End Function
    End Class
End Namespace