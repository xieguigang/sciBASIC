#Region "Microsoft.VisualBasic::410ba1fb1ce4952cde622c10231834f7, Data\DataFrame\DATA\HTMLWriter.vb"

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

    '     Module HTMLWriter
    ' 
    '         Function: titleRow, ToHTML, (+2 Overloads) ToHTMLTable
    ' 
    '         Sub: bodyRow
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace DATA

    <Package("Csv.HTML.Writer")>
    Public Module HTMLWriter

        ''' <summary>
        ''' Render target object collection as a html table
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="title"></param>
        ''' <param name="describ"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToHTML(Of T As Class)(source As IEnumerable(Of T), Optional title As String = "", Optional describ As String = "") As String
            Return source.ToCsvDoc(False).ToHTMLTable(title Or GetType(T).FullName.AsDefault, describ Or GetType(T).Description.AsDefault)
        End Function

        <Extension>
        Public Function ToHTMLTable(Of T As Class)(source As IEnumerable(Of T),
            Optional className$ = "",
            Optional tableID$ = Nothing,
            Optional width$ = "",
            Optional removes$() = Nothing,
            Optional alt$ = Nothing) As String

            Return source.ToCsvDoc(False).ToHTMLTable(
                className,
                tableID,
                width,
                removes,
                alt)
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="width">100%|px</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("ToHTML.Table")>
        <Extension> Public Function ToHTMLTable(table As File,
            Optional className$ = "",
            Optional tableID$ = Nothing,
            Optional width$ = "",
            Optional title$ = Nothing,
            Optional removes$() = Nothing,
            Optional alt$ = Nothing) As String

            Dim innerDoc As New StringBuilder("<table", 4096)
            Dim removeList As New Index(Of String)(removes)
            Dim removeIndex As New Index(Of Integer)(
                removes _
                .SafeQuery _
                .Select(Function(name)
                            Return table.Headers.IndexOf(name)
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

            If Not title.StringEmpty Then
                innerDoc.AppendLine($"<caption>{title}</caption>")
            End If

            Call innerDoc.AppendLine(table.Headers.titleRow(removeList))
            Call innerDoc.AppendLine("<tbody>")

            For Each row As SeqValue(Of RowObject) In table.Skip(1).SeqIterator
                With row.value
                    If alt.StringEmpty Then
                        Call .bodyRow(innerDoc, removeIndex, Nothing)
                    Else
                        If row Mod 2 = 0 Then
                            Call .bodyRow(innerDoc, removeIndex, alt)
                        Else
                            Call .bodyRow(innerDoc, removeIndex, Nothing)
                        End If
                    End If
                End With
            Next

            Call innerDoc.AppendLine("</tbody>")
            Call innerDoc.AppendLine("</table>")

            Return innerDoc.ToString
        End Function

        <Extension> Private Function titleRow(row As RowObject, removes As Index(Of String)) As String
            Dim doc As New StringBuilder
            Dim rowText$ = row _
                .Where(Function(t) removes(t) = -1) _
                .Select(Function(x)
                            Return $"<th id=""{x}"">{x}</th>"
                        End Function) _
                .JoinBy("")

            Call doc.AppendLine("<thead>")
            Call doc.AppendLine(rowText)
            Call doc.AppendLine("</thead>")

            Return doc.ToString
        End Function

        <Extension> Private Sub bodyRow(row As RowObject, ByRef doc As StringBuilder, removes As Index(Of Integer), alt$)
            Dim rowText$ = row _
                .SeqIterator _
                .Where(Function(i) removes(x:=i.i) = -1) _
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
