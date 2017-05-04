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
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace IO

    <PackageNamespace("Csv.HTML.Writer")>
    Public Module HTMLWriter

        <Extension> Public Function ToHTML(Of T As Class)(source As Generic.IEnumerable(Of T), Optional Title As String = "", Optional describ As String = "", Optional css As String = "") As String
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

        <Extension> Public Function ToHTMLTable(Of T As Class)(source As Generic.IEnumerable(Of T), Optional className As String = "", Optional width As String = "") As String
            Dim Csv As IO.File = source.ToCsvDoc(False)
            Return Csv.ToHTMLTable(className, width)
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <param name="width">100%|px</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("ToHTML.Table")>
        <Extension> Public Function ToHTMLTable(doc As IO.File, Optional className As String = "", Optional width As String = "") As String
            Dim innerDoc As New StringBuilder("<table", 4096)

            If Not String.IsNullOrEmpty(className) Then
                Call innerDoc.Append($" class=""{className}""")
            End If
            If Not String.IsNullOrEmpty(width) Then
                Call innerDoc.Append($" width=""{width}""")
            End If

            Call innerDoc.Append(">")
            Call innerDoc.AppendLine(doc.First.__titleRow)
            For Each row As RowObject In doc.Skip(1)
                Call row.__contentRow(innerDoc)
            Next
            Call innerDoc.AppendLine("</table>")

            Return innerDoc.ToString
        End Function

        <Extension> Private Function __titleRow(row As RowObject) As String
            Dim doc As StringBuilder = New StringBuilder

            Call doc.AppendLine("<tr>")
            Call doc.AppendLine(row.ToArray(Function(x) $"<td><strong>{x}</strong></td>").JoinBy(vbCrLf))
            Call doc.AppendLine("</tr>")

            Return doc.ToString
        End Function

        <Extension> Private Sub __contentRow(row As RowObject, ByRef doc As StringBuilder)
            Call doc.AppendLine("<tr>")
            Call doc.AppendLine(row.ToArray(Function(x) $"<td>{x}</td>").JoinBy(vbCrLf))
            Call doc.AppendLine("</tr>")
        End Sub
    End Module
End Namespace
