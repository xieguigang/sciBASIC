Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text

Namespace Academic

    Public Module Extensions

        Public Sub Build_KB(term$, out$, Optional pages% = 10, Optional flat As Boolean = True, Optional sleepInterval% = 5000)
            For Each entry As (refer$, list As NamedValue(Of String)()) In AcademicSearch.Query(term, pages)
                For Each url As NamedValue(Of String) In entry.list
                    Try
                        With url.GetDetails(refer:=entry.refer)
                            Dim path$

                            If flat Then
                                path = $"{out}/{ .GetProfileID}.xml"
                            Else
                                path = $"{out}/{Mid(.GetProfileID, 3)}/{ .GetProfileID}.xml"
                            End If

                            Call .GetXml _
                                 .SaveTo(path, TextEncodings.UTF8WithoutBOM)
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