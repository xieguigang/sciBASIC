Public MustInherit Class SampleObservation

    Dim samples As New List(Of Double)

    Sub New(samples As IEnumerable(Of Double))
        Me.samples = New List(Of Double)(samples)
    End Sub

    Public Sub Add(observation As Double)
        Call samples.Add(observation)
        Call addObservation(observation)
    End Sub

    Protected MustOverride Sub addObservation(observation As Double)
    Protected MustOverride Function getEigenvalue() As Double

    Protected Iterator Function getRaw() As IEnumerable(Of Double)
        For Each obs As Double In samples
            Yield obs
        Next
    End Function

    Public Shared Narrowing Operator CType(observation As SampleObservation) As Double
        Return observation.getEigenvalue
    End Operator
End Class
