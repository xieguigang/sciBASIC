#Region "Microsoft.VisualBasic::8a1e40038193c35737e2ea618ec124ed, Data\DataFrame\test\Reflection.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Reflection
    ' 
    '     Sub: Main
    ' 
    ' Class Visitor
    ' 
    '     Properties: data, ip, method, ref, success
    '                 time, ua, uid, url
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports VisualBasic = Microsoft.VisualBasic.Language.Runtime

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

        With New VisualBasic
            Call New DataFrame(
                !X = {1, 2, 3, 4, 5},
                !Y = {9, 8, 7, 6, 5}
            ).csv _
             .Save("./dataframe_test.csv")
        End With

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
