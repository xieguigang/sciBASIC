#Region "Microsoft.VisualBasic::7f151a5d56c0cfae8c675f665c839c21, ..\sciBASIC#\Data\DataFrame\IO\csv\HTMLWriter.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace IO

    <PackageNamespace("Csv.HTML.Writer")>
    Public Module HTMLWriter

        <Extension> Public Function ToHTML(Of T As Class)(source As IEnumerable(Of T), Optional Title As String = "", Optional describ As String = "", Optional css As String = "") As String
            Dim csv As IO.File = source.ToCsvDoc(False)

            If String.IsNullOrEmpty(describ) Then
                describ = GetType(T).Description
            End If
            If String.IsNullOrEmpty(Title) Then
                Title = $"Document for {GetType(T).FullName}"
            End If

            Return csv.ToHTML(Title, describ, css)
        End Function

        <ExportAPI("ToHTML")>
        <Extension> Public Function ToHTML(doc As IO.File, Optional Title As String = "", Optional describ As String = "", Optional css As String = "") As String
            If String.IsNullOrEmpty(css) Then
                css = My.Resources.foundation
            End If

            Dim html As StringBuilder = New StringBuilder(My.Resources.HTML_Template)
            Call html.Replace("{Title}", Title)
            Call html.Replace("{CSS}", css)

            Dim innerDoc As New StringBuilder($"<p>{describ}</p>")
            Call innerDoc.AppendLine(doc.ToHTMLTable)

            Call html.Replace("{doc}", innerDoc.ToString)

            Return html.ToString
        End Function

        <Extension> Public Function ToHTMLTable(Of T As Class)(
            source As IEnumerable(Of T),
            Optional className$ = "",
            Optional tableID$ = Nothing,
            Optional width$ = "",
            Optional removes$() = Nothing,
            Optional theadSpace As Boolean = False,
            Optional alt$ = Nothing) As String

            Dim csv As File = source.ToCsvDoc(False)
            Return csv.ToHTMLTable(
                className,
                tableID,
                width,
                removes,
                theadSpace,
                alt)
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="csvTable"></param>
        ''' <param name="width">100%|px</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("ToHTML.Table")>
        <Extension> Public Function ToHTMLTable(
            csvTable As File,
            Optional className$ = "",
            Optional tableID$ = Nothing,
            Optional width$ = "",
            Optional removes$() = Nothing,
            Optional theadSpace As Boolean = False,
            Optional alt$ = Nothing) As String

            Dim innerDoc As New StringBuilder("<table", 4096)
            Dim removeList As New Index(Of String)(removes)
            Dim removeIndex As New Index(Of Integer)(
                removes _
                .SafeQuery _
                .Select(Function(name)
                            Return csvTable.Headers.IndexOf(name)
                        End Function))

            If Not String.IsNullOrEmpty(className) Then
                Call innerDoc.Append($" class=""{className}""")
            End If
            If Not String.IsNullOrEmpty(tableID) Then
                Call innerDoc.Append($" id=""{tableID}""")
            End If
            If Not String.IsNullOrEmpty(width) Then
                Call innerDoc.Append($" width=""{width}""")
            End If

            Call innerDoc.AppendLine(">")
            Call innerDoc.AppendLine(csvTable.Headers.__titleRow(removeList, theadSpace))          

            For Each row As SeqValue(Of RowObject) In csvTable.Skip(1).SeqIterator
                If alt.StringEmpty Then
                    Call (+row).__contentRow(innerDoc, removeIndex, Nothing)
                Else
                    If row Mod 2 = 0 Then
                        Call (+row).__contentRow(innerDoc, removeIndex, alt)
                    Else
                        Call (+row).__contentRow(innerDoc, removeIndex, Nothing)
                    End If
                End If
            Next

            Call innerDoc.AppendLine("</table>")

            Return innerDoc.ToString
        End Function

        <Extension> Private Function __titleRow(row As RowObject, removes As Index(Of String), theadSpace As Boolean) As String
            Dim doc As New StringBuilder
            Dim rowText$ = row _
                .Where(Function(t) removes(t) = -1) _
                .ToArray(Function(x)
                             Return $"<td id=""{x}""><strong>{If(theadSpace, x & "&nbsp;&nbsp;&nbsp;", x)}</strong></td>"
                         End Function) _
                .JoinBy("")

            Call doc.AppendLine("<thead>")
            Call doc.AppendLine(rowText)
            Call doc.AppendLine("</thead>")

            Return doc.ToString
        End Function

        <Extension> Private Sub __contentRow(row As RowObject, ByRef doc As StringBuilder, removes As Index(Of Integer), alt$)
            Dim rowText$ = row.ToArray _
                .SeqIterator _
                .Where(Function(i) removes(i.i) = -1) _
                .Select(Function(x) $"<td>{+x}</td>") _
                .JoinBy("")

            If alt.StringEmpty Then
                Call doc.Append("<tr>")
            Else
                Call doc.Append($"<tr class=""{alt}"">")
            End If

            Call doc.AppendLine(rowText)
            Call doc.Append("</tr>")
        End Sub
    End Module
End Namespace
