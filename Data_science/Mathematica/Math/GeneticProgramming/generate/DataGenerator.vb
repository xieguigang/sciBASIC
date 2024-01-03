
Public Class DataGenerator

    Private Sub New()
    End Sub

    Public Shared Function generateDataTuples([function] As [Function], from As Double, [to] As Double, size As Integer) As IList(Of Tuple)

        Dim data As IList(Of Tuple) = New List(Of Tuple)(size)
        Dim delta = ([to] - from) / size
        Dim x = from

        While x <= [to]
            data.Add(New Tuple(x, [function].eval(x)))
            x += delta
        End While
        Return data
    End Function

End Class
