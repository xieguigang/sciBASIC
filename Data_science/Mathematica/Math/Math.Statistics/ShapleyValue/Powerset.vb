Namespace ShapleyValue

    Public Class Powerset

        Public Shared ReadOnly nullSet As New HashSet(Of Integer)()

        Public Shared Function calculate(nbElements As Integer) As IEnumerable(Of Integer())
            Dim inputSet As New HashSet(Of Integer)()

            For i As Integer = 1 To nbElements
                inputSet.Add(i)
            Next

            Dim result As IEnumerable(Of Integer()) = inputSet.PowerSet()

            Return result
        End Function

    End Class
End Namespace
