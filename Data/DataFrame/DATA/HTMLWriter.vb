#Region "Microsoft.VisualBasic::53af31abb577d95198e796bdd56d2b73, sciBASIC#\Data\DataFrame\DATA\HTMLWriter.vb"

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

    '   Total Lines: 149
    '    Code Lines: 111
    ' Comment Lines: 18
    '   Blank Lines: 20
    '     File Size: 5.65 KB


    '     Module HTMLWriter
    ' 
    '         Function: html, titleRow, ToHTML, ToHTMLTable
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

    ''' <summary>
    ''' file generator for mshtml
    ''' </summary>
    Public Module HTMLWriter

        ''' <summary>
        ''' Render target object collection as a html table
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="title"></param>
        ''' <param name="evenRowClassName"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToHTML(Of T As Class)(source As IEnumerable(Of T), Optional title As String = "", Optional evenRowClassName$ = "even") As String
            Return source.ToCsvDoc(False).html(title:=title Or GetType(T).FullName.AsDefault, altClassName:=evenRowClassName)
        End Function

        <Extension>
        Public Function ToHTMLTable(Of T As Class)(source As IEnumerable(Of T),
            Optional className$ = "",
            Optional tableID$ = Nothing,
            Optional width$ = "",
            Optional removes$() = Nothing,
            Optional title$ = "",
            Optional altClassName$ = Nothing,
            Optional numFormat$ = "G5") As String

            Return source.ToCsvDoc(False, numFormat:=numFormat).html(
                [class]:=className,
                id:=tableID,
                width:=width,
                removes:=removes,
                title:=title,
                altClassName:=altClassName
            )
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="width">100%|px</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("ToHTML.Table")>
        <Extension> Public Function html(table As File,
            Optional class$ = "",
            Optional id$ = Nothing,
            Optional width$ = "",
            Optional title$ = Nothing,
            Optional removes$() = Nothing,
            Optional altClassName$ = Nothing) As String

            Dim innerDoc As New StringBuilder("<table", 4096)
            Dim removeList As New Index(Of String)(removes)
            Dim removeIndex As New Index(Of Integer)(
                removes _
                .SafeQuery _
                .Select(Function(name)
                            Return table.Headers.IndexOf(name)
                        End Function))

            If Not String.IsNullOrEmpty([class]) Then
                Call innerDoc.Append($" class=""{[class]}""")
            End If
            If Not String.IsNullOrEmpty(id) Then
                Call innerDoc.Append($" id=""{id}""")
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
                    If altClassName.StringEmpty Then
                        Call .bodyRow(innerDoc, removeIndex, Nothing)
                    Else
                        If (row + 1) Mod 2 = 0 Then
                            Call .bodyRow(innerDoc, removeIndex, altClassName)
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

        <Extension> Private Sub bodyRow(row As RowObject, ByRef doc As StringBuilder, removes As Index(Of Integer), altClassName$)
            Dim rowText$ = row _
                .SeqIterator _
                .Where(Function(i) removes(x:=i.i) = -1) _
                .Select(Function(x) $"<td>{+x}</td>") _
                .JoinBy("")

            If altClassName.StringEmpty Then
                Call doc.Append("<tr>")
            Else
                Call doc.Append($"<tr class=""{altClassName}"">")
            End If

            Call doc.AppendLine(rowText)
            Call doc.Append("</tr>")
        End Sub
    End Module
End Namespace
