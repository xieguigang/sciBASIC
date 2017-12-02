Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text

Namespace Academic

    Public Module Extensions

        Public Sub Build_KB(term$, out$, Optional pages% = 10, Optional sleepInterval% = 5000)
            For Each entry As (refer$, list As NamedValue(Of String)()) In AcademicSearch.Query(term, pages)
                For Each url As NamedValue(Of String) In entry.list
                    Try
                        With url.GetDetails(refer:=entry.refer)
                            Call .GetXml _
                                 .SaveTo($"{out}/{ .GetProfileID}.xml", TextEncodings.UTF8WithoutBOM)
                        End With
                    Catch ex As Exception
                        ex = New Exception(url.Value, ex)
                        App.LogException(ex)
                    Finally
                        Thread.Sleep(sleepInterval)
                    End Try
                Next
            Next
        End Sub
    End Module
End Namespace