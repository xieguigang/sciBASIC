#Region "Microsoft.VisualBasic::9640ce47a52db63f7f1dae3fed621d5a, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\DynamicProgramming\Levenshtein\ResultVisualize.vb"

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

    '   Total Lines: 170
    '    Code Lines: 147 (86.47%)
    ' Comment Lines: 4 (2.35%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (11.18%)
    '     File Size: 6.59 KB


    '     Module ResultVisualize
    ' 
    '         Function: doVisualizeHTML, HTMLVisualize, internalMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports std = System.Math

Namespace ComponentModel.Algorithm.DynamicProgramming.Levenshtein

    Public Module ResultVisualize

        ''' <summary>
        ''' Output HTML result for visualization
        ''' </summary>
        ''' <returns></returns>
        <Extension>
        Public Function HTMLVisualize(result As DistResult) As String
            Try
                Return result.CreateVisualizeHTML().FormatHTML
            Catch ex As Exception
                Call App.LogException(ex)
                Return _
                    <html>
                        <head>
                            <title><%= NameOf(HTTP_RFC.RFC_INTERNAL_SERVER_ERROR) %></title>
                        </head>
                        <body>
                            <pre><%= ex.ToString %></pre>
                        </body>
                    </html>
            End Try
        End Function

        <Extension>
        Private Function CreateVisualizeHTML(dist As DistResult) As String
            Dim html As New XmlBuilder()
            Dim edits$ = dist.DistEdits

            html += <h3>Summary</h3>
            html += "<table>"
            html += <tr>
                        <td>Reference: </td>
                        <td><%= dist.__getReference.Length %>(chars)</td>
                        <td><%= dist.Reference %></td>
                    </tr>
            html += <tr>
                        <td>Hypotheses: </td>
                        <td><%= dist.__getSubject.Length %>(chars)</td>
                        <td><%= dist.Hypotheses %></td>
                    </tr>
            html += dist.__innerInsert
            html += <tr>
                        <td>Distance: </td>
                        <td></td>
                        <td><%= dist.Distance %></td>
                    </tr>
            html += <tr>
                        <td>Similarity: </td>
                        <td></td>
                        <td><%= dist.MatchSimilarity * 100 %>%</td>
                    </tr>
            html += "</table>"
            html += <hr/>
            html += <h4>Levenshtein Edit Distance Table</h4>
            html += <p>
                        Levenshtein Edit: <br/>
                        <strong><%= edits %></strong> have (<%= edits.Count("m"c) %>/<%= Len(edits) %>) matches.<br/>

                        <p>
                            <table>
                                <tr><td>d</td><td> -> Delete</td></tr>
                                <tr><td>i</td><td> -> Insert</td></tr>
                                <tr><td>m</td><td> -> Match</td></tr>
                                <tr><td>s</td><td> -> Substitute</td></tr>
                            </table>
                        </p>
                    </p>

            html += dist.internalMatrix

            Return (<html>
                        <head>
                            <title><%= dist.ToString %></title>
                        </head>
                        <body style="font-family:Ubuntu;">
                            $content
                        </body>
                    </html>).ToString _
                            .Replace("$content", html.ToString)
        End Function

        <Extension>
        Private Function internalMatrix(matrix As DistResult) As String
            Dim dict As Dictionary(Of Integer, Integer())

            If matrix.DistTable Is Nothing Then
                Return ""
            End If

            If matrix.Path.IsNullOrEmpty Then
                dict = New Dictionary(Of Integer, Integer())
            Else
                Dim g = From cell As Point
                        In matrix.Path
                        Select cell
                        Group cell By cell.X Into Group

                dict = g _
                    .ToDictionary(Function(row) row.X,
                                  Function(row)
                                      Return row _
                                        .Group _
                                        .Select(Function(cell) cell.Y) _
                                        .ToArray
                                  End Function)
            End If

            Dim Reference As String = matrix.__getReference()
            Dim Hypotheses As String = matrix.__getSubject()
            Dim MAT As New XmlBuilder
            Dim hyps$() = Hypotheses _
                .Select(Function(c)
                            Return <td>
                                       <strong><%= c %></strong>
                                   </td>
                        End Function) _
                .Select(Function(s) s.ToString) _
                .ToArray

            MAT += (<tr>
                        <td></td>
                        $content
                    </tr>).ToString _
                          .Replace("$content", hyps.JoinBy(""))

            For i As Integer = 0 To Len(Reference) - 1
                Dim r As New XmlBuilder

                For j As Integer = 0 To Len(Hypotheses) - 1
                    Dim c = std.Round(matrix.DistTable(i)(j), 2)

                    If dict.ContainsKey(i) AndAlso Array.IndexOf(dict(i), j) > -1 Then
                        r += <td style="background-color:green;color:white">
                                 <strong><%= c %></strong>
                             </td>
                    Else
                        r += <td><%= c %></td>
                    End If
                Next

                MAT += (<tr>
                            <td>
                                <strong><%= Reference(i) %></strong>
                            </td>
                                $content
                           <td style="background-color:blue;color:white">
                                <strong><%= matrix.DistEdits(i) %></strong>
                            </td>
                        </tr>).ToString _
                              .Replace("$content", r.ToString)
            Next

            Return (<table>
                        $content
                    </table>).ToString _
                             .Replace("$content", MAT.ToString)
        End Function
    End Module
End Namespace
