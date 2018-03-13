Imports Microsoft.VisualBasic.Linq

Module Module2

    Sub Main()

        Dim nnn = TryCast(Nothing, String()).SafeQuery

        Dim list = populateNothing()?.data

        For Each s In list.SafeQuery
        Next
        For Each s In populateNothing()?.data.SafeQuery
        Next

        Pause()
    End Sub

    Public Class List
        Public Property data As String()
    End Class

    Public Function populateNothing() As List
        Return Nothing
    End Function
End Module
