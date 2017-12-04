#Region "Microsoft.VisualBasic::b0e2de86cfa54ac6966df4e5d4e8eb60, ..\sciBASIC#\www\Microsoft.VisualBasic.Webservices.Bing\Academic\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
                            Dim id$ = .GetProfileID

                            If flat Then
                                path = $"{out}/{id}.xml"
                            Else
                                path = $"{out}/{Mid(id, 1, 3)}/{id}.xml"
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
