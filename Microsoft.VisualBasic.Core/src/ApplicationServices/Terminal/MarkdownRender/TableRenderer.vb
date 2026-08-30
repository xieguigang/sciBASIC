#Region "Microsoft.VisualBasic::699506d14d361ed10ed2329c56940a05, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\TableRenderer.vb"

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

    '   Total Lines: 105
    '    Code Lines: 56 (53.33%)
    ' Comment Lines: 33 (31.43%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 16 (15.24%)
    '     File Size: 3.96 KB


    '     Module TableRenderer
    ' 
    '         Function: IsDelimiterRow, Render, SplitCells
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' renders the pipe table block into a set of the plain text lines
    ''' </summary>
    Friend Module TableRenderer

        ''' <summary>
        ''' render the markdown pipe table
        ''' </summary>
        ''' <param name="rows">
        ''' the raw table lines, the first line is the header row and the second
        ''' line is the ``--|---`` delimiter row, all of the remaining lines are
        ''' the table body rows.
        ''' </param>
        ''' <param name="theme"></param>
        ''' <returns>the rendered table lines, without the line feed char.</returns>
        Public Function Render(rows As System.Collections.Generic.List(Of String), theme As MarkdownTheme) As String()
            If rows Is Nothing OrElse rows.Count = 0 Then
                ' the empty table buffer must return at here, or the following
                ' rows(0) statement throws an IndexOutOfRangeException
                Return {}
            End If

            Dim header As String() = SplitCells(rows(0))
            Dim body As New System.Collections.Generic.List(Of String())()

            ' the rows(1) is the ``--|---`` delimiter row, which is not a data row
            For i As Integer = 2 To rows.Count - 1
                If Not String.IsNullOrWhiteSpace(rows(i)) Then
                    Call body.Add(SplitCells(rows(i)))
                End If
            Next

            Dim table As New ConsoleTableBaseData(header, body)
            Dim print As String = ConsoleTableBuilder.From(table) _
                .WithFormat(theme.Table) _
                .Export _
                .ToString

            Return print.LineTokens
        End Function

        ''' <summary>
        ''' splits one table line into its cells
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The pipe delimited syntax ``| a | b |`` produces a leading and a trailing
        ''' empty cell when it is split by the pipe char, these two empty cells must
        ''' be dropped or the rendered table will have two extra empty columns. Note
        ''' that the empty cells in the middle are parts of the data, so that they
        ''' must be kept.
        ''' </remarks>
        Private Function SplitCells(line As String) As String()
            Dim cells As String() = line _
                .Split("|"c) _
                .Select(Function(cell) cell.Trim()) _
                .ToArray

            If cells.Length > 1 Then
                If cells(0).Length = 0 Then
                    cells = cells.Skip(1).ToArray
                End If
                If cells.Length > 1 AndAlso cells(cells.Length - 1).Length = 0 Then
                    cells = cells.Take(cells.Length - 1).ToArray
                End If
            End If

            If cells.Length = 0 Then
                cells = {""}
            End If

            Return cells
        End Function

        ''' <summary>
        ''' is the given line the ``--|----|:---:|`` delimiter row of a table?
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns></returns>
        Public Function IsDelimiterRow(line As String) As Boolean
            If line Is Nothing OrElse line.IndexOf("|"c) < 0 Then
                Return False
            End If

            Dim test As String = line.Replace("|", "").Replace(" ", "").Trim()

            If test.Length < 3 Then
                Return False
            End If

            For Each c As Char In test
                If c <> "-"c AndAlso c <> ":"c Then
                    Return False
                End If
            Next

            Return True
        End Function
    End Module
End Namespace
