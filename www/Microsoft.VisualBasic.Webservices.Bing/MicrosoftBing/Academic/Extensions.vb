#Region "Microsoft.VisualBasic::25fbcd0712d279fed27abdd65c98993e, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\Academic\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 82
    '    Code Lines: 71
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 2.97 KB


    '     Module Extensions
    ' 
    '         Function: Summary
    ' 
    '         Sub: Build_KB
    ' 
    '     Class Summary
    ' 
    '         Properties: authors, cites, doi, journal, pubmed
    '                     title, year
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text

Namespace Bing.Academic

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Summary(articles As IEnumerable(Of ArticleProfile)) As Summary()
            Return articles _
                .Select(Function(a) New Summary(a)) _
                .ToArray
        End Function
    End Module

    Public Class Summary

        Public Property title As String
        Public Property authors As String()
        Public Property year As String
        Public Property journal As String
        Public Property doi As String
        Public Property cites As Integer
        Public Property pubmed As String

        Sub New()
        End Sub

        Sub New(article As ArticleProfile)
            title = article.title
            authors = article _
                .authors _
                .Select(Function(a) a.title) _
                .ToArray
            year = article.PubDate.Year
            journal = article.journal.title
            doi = article.DOI
            cites = article.cites.Sum(Function(c) c.Volume)
            pubmed = article _
                .source _
                .Where(Function(link)
                           Return link.title.TextEquals("www.ncbi.nlm.nih.gov")
                       End Function) _
                .FirstOrDefault _
                .href
        End Sub

        Public Overrides Function ToString() As String
            Return $"{authors.FirstOrDefault} ({year})"
        End Function
    End Class
End Namespace
