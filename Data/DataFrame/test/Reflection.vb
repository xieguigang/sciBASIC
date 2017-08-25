Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO

Module Reflection

    Sub Main()

        Dim visitors As Visitor() = "../../../../Example/visitors.csv".LoadCsv(Of Visitor)
        Dim dynamics As EntityObject() = EntityObject.LoadDataSet("../../../../Example/visitors.csv").ToArray

        Call visitors.SaveTo("./test.csv")
        Call dynamics.SaveTo("./test2.csv")

        For Each visit In dynamics
            With visit
                Call println("%s visit %s at %s", !ip, !url, !time)
            End With
        Next

        Pause()
    End Sub
End Module

Public Class Visitor

    Public Property uid As String
    Public Property time As String
    Public Property ip As String
    Public Property url As String
    Public Property success As String
    Public Property method As String
    Public Property ua As String
    Public Property ref As String
    Public Property data As String

    Public Overrides Function ToString() As String
        Return $"{ip}@{time}"
    End Function

End Class
