#Region "Microsoft.VisualBasic::6b7bdcc50f923f13c89ff34bcef19bf3, Data\DataFrame\DataSet\HTMLWriter.vb"

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

    '   Total Lines: 174
    '    Code Lines: 133 (76.44%)
    ' Comment Lines: 17 (9.77%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 24 (13.79%)
    '     File Size: 6.66 KB


    '     Class HTMLWriter
    ' 
    '         Constructor: (+2 Overloads) Sub New
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
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DATA

    ''' <summary>
    ''' file generator for mshtml
    ''' </summary>
    Public Class HTMLWriter

        Dim title As String
        Dim altClassName As String
        Dim class$ = ""
        Dim id$ = Nothing
        Dim width$ = ""
        Dim removes$() = Nothing
        Dim thwidth As Dictionary(Of String, String)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(Optional title As String = "", Optional evenRowClassName$ = "even")
            Call Me.New(title:=title, altClassName:=evenRowClassName)
        End Sub

        Sub New(Optional className$ = "",
                Optional tableID$ = Nothing,
                Optional width$ = "",
                Optional removes$() = Nothing,
                Optional title$ = "",
                Optional altClassName$ = Nothing,
                Optional thwidth As Dictionary(Of String, String) = Nothing)

            Me.class = className
            Me.id = tableID
            Me.width = width
            Me.removes = removes
            Me.title = title
            Me.altClassName = altClassName
            Me.thwidth = thwidth
        End Sub

        ''' <summary>
        ''' Render target object collection as a html table
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="title"></param>
        ''' <param name="evenRowClassName"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ToHTML(Of T As Class)(source As IEnumerable(Of T),
                                                     Optional title As String = "",
                                                     Optional evenRowClassName$ = "even") As String

            Return New HTMLWriter(
                title:=title Or GetType(T).Name.AsDefault,
                evenRowClassName:=evenRowClassName
            ).html(source.ToCsvDoc(False))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ToHTMLTable(Of T As Class)(source As IEnumerable(Of T),
            Optional className$ = "",
            Optional tableID$ = Nothing,
            Optional width$ = "",
            Optional removes$() = Nothing,
            Optional title$ = "",
            Optional altClassName$ = Nothing,
            Optional numFormat$ = "G5") As String

            Return New HTMLWriter(
                className:=className,
                tableID:=tableID,
                width:=width,
                removes:=removes,
                title:=title,
                altClassName:=altClassName
            ).html(source.ToCsvDoc(False, numFormat:=numFormat))
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' 
        Public Function html(table As File) As String
            Dim innerDoc As New StringBuilder("<table", 4096)
            Dim removeList As New Index(Of String)(removes)
            Dim removeIndex As New Index(Of Integer)(removes _
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

            Call innerDoc.AppendLine(titleRow(table.Headers, removeList))
            Call innerDoc.AppendLine("<tbody>")

            For Each row As SeqValue(Of RowObject) In table.Skip(1).SeqIterator
                If altClassName.StringEmpty Then
                    Call bodyRow(row.value, innerDoc, removeIndex, Nothing)
                Else
                    If (row + 1) Mod 2 = 0 Then
                        Call bodyRow(row.value, innerDoc, removeIndex, altClassName)
                    Else
                        Call bodyRow(row.value, innerDoc, removeIndex, Nothing)
                    End If
                End If
            Next

            Call innerDoc.AppendLine("</tbody>")
            Call innerDoc.AppendLine("</table>")

            Return innerDoc.ToString
        End Function

        Private Function titleRow(row As RowObject, removes As Index(Of String)) As String
            Dim doc As New StringBuilder
            Dim rowText$ = row _
                .Where(Function(t) removes(t) = -1) _
                .Select(Function(x)
                            If thwidth.IsNullOrEmpty OrElse Not thwidth.ContainsKey(x) Then
                                Return $"<th id=""{x}"">{x}</th>"
                            Else
                                Return $"<th id=""{x}"" width=""{thwidth(x)}"">{x}</th>"
                            End If
                        End Function) _
                .JoinBy("")

            Call doc.AppendLine("<thead>")
            Call doc.AppendLine(rowText)
            Call doc.AppendLine("</thead>")

            Return doc.ToString
        End Function

        Private Sub bodyRow(row As RowObject, ByRef doc As StringBuilder, removes As Index(Of Integer), altClassName$)
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
    End Class
End Namespace
