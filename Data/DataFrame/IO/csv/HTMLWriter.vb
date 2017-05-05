#Region "Microsoft.VisualBasic::e3c083c7adb19571cc01940f95e6c755, ..\sciBASIC#\Data\DataFrame\IO\csv\HTMLWriter.vb"

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
            Optional className As String = "",
            Optional width As String = "",
            Optional removes$() = Nothing,
            Optional theadSpace As Boolean = False) As String

            Dim csv As File = source.ToCsvDoc(False)
            Return csv.ToHTMLTable(className, width, removes, theadSpace)
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <param name="width">100%|px</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("ToHTML.Table")>
        <Extension> Public Function ToHTMLTable(doc As IO.File, Optional className As String = "", Optional width As String = "", Optional removes$() = Nothing, Optional theadSpace As Boolean = False) As String
            Dim innerDoc As New StringBuilder("<table", 4096)
            Dim removeList As New IndexOf(Of String)(removes)
            Dim removeIndex As New IndexOf(Of Integer)(
                removes _
                .SafeQuery _
                .Select(Function(name)
                            Return doc.Headers.IndexOf(name)
                        End Function))

            If Not String.IsNullOrEmpty(className) Then
                Call innerDoc.Append($" class=""{className}""")
            End If
            If Not String.IsNullOrEmpty(width) Then
                Call innerDoc.Append($" width=""{width}""")
            End If

            Call innerDoc.Append(">")
            Call innerDoc.AppendLine(doc.Headers.__titleRow(removeList, theadSpace))
            Call innerDoc.AppendLine("<hr />")
            For Each row As RowObject In doc.Skip(1)
                Call row.__contentRow(innerDoc, removeIndex)
            Next
            Call innerDoc.AppendLine("</table>")

            Return innerDoc.ToString
        End Function

        <Extension> Private Function __titleRow(row As RowObject, removes As IndexOf(Of String), theadSpace As Boolean) As String
            Dim doc As New StringBuilder
            Dim rowText$ = row _
                .Where(Function(t) removes(t) = -1) _
                .ToArray(Function(x) $"<td><strong>{If(theadSpace, x & "&nbsp;&nbsp;", x)}</strong></td>") _
                .JoinBy("")

            Call doc.Append("<thead>")
            Call doc.AppendLine(rowText)
            Call doc.Append("</thead>")

            Return doc.ToString
        End Function

        <Extension> Private Sub __contentRow(row As RowObject, ByRef doc As StringBuilder, removes As IndexOf(Of Integer))
            Dim rowText$ = row.ToArray _
                .SeqIterator _
                .Where(Function(i) removes(i.i) = -1) _
                .Select(Function(x) $"<td>{+x}</td>") _
                .JoinBy("")

            Call doc.Append("<tr>")
            Call doc.AppendLine(rowText)
            Call doc.Append("</tr>")
        End Sub
    End Module
End Namespace
