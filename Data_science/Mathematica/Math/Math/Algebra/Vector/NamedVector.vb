Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' Factory for <see cref="Dictionary(Of String, Double)"/> to <see cref="Vector"/>
    ''' </summary>
    Public Class NamedVectorFactory

        Public ReadOnly Property Keys As String()

        ReadOnly factors As Factor(Of String)()

        Sub New(factors As IEnumerable(Of String))
            Me.Keys = factors.ToArray
            Me.factors = FactorExtensions.factors(Keys)
        End Sub

        Public Function EmptyVector() As Vector
            Return New Vector(factors.Length - 1)
        End Function

        Public Function AsVector(data As Dictionary(Of String, Double)) As Vector
            Dim vector#() = New Double(factors.Length - 1) {}

            For Each factor As Factor(Of String) In factors
                vector(factor.Value) = data(factor)
            Next

            Return vector.AsVector
        End Function

        Public Function Translate(vector As Vector) As Dictionary(Of String, Double)
            Return factors.ToDictionary(
                Function(factor) factor.FactorValue,
                Function(i) vector(i.Value))
        End Function

        Public Overrides Function ToString() As String
            Return factors _
                .Select(Function(factor) factor.FactorValue) _
                .ToArray _
                .GetJson
        End Function
    End Class
End Namespace