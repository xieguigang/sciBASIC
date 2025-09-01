Namespace ShapleyValue

    Public Class Powerset

        Public Shared ReadOnly nullSet As ISet(Of Integer) = New HashSet(Of Integer)()

        Public Shared Function calculate(nbElements As Integer) As IEnumerable(Of Integer())

            Dim inputSet As ISet(Of Integer) = New HashSet(Of Integer)()
            For i = 1 To nbElements
                inputSet.Add(i)
            Next

            Dim result As IEnumerable(Of Integer()) = inputSet.PowerSet()

            Return result
        End Function

    End Class
End Namespace
