#Region "Microsoft.VisualBasic::7992fa5d604c8ad89f6e2300dcf49175, www\Microsoft.VisualBasic.NETProtocol\Test\emailParser.vb"

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

    '   Total Lines: 46
    '    Code Lines: 37
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.69 KB


    ' Module emailParser
    ' 
    '     Function: getInnerTable
    ' 
    '     Sub: Main, parseDataFiles
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Net.Mailto
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Public Module emailParser

    Sub Main()
        Dim rawDir = "C:\Users\Administrator\Downloads"

        Call parseDataFiles(rawDir.ListFiles("*.eml"))

        Pause()
    End Sub

    Sub parseDataFiles(files As IEnumerable(Of String))
        For Each raw As String In files
            Dim email = EmlReader.ParseEMail(raw)
            Dim tables = email.BodyContent.GetTablesHTML
            Dim test = getInnerTable(tables(59)).GetColumnsHTML.Select(Function(c) c.StripHTMLTags).ToArray
            Dim data = tables.Select(Function(t)
                                         Return getInnerTable(t).GetColumnsHTML.Select(Function(r) r.StripHTMLTags()).ToArray
                                     End Function).Where(Function(r) r.Length > 3) _
                .ToArray

            Using book As New StreamWriter($"{raw.ParentPath}/{email.Date.ToString("yyyy-MM-dd")}.txt".Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
                For Each line In data.Where(Function(n) n.Length = 8 OrElse n(Scan0).StartsWith("人民币(CNY)"))
                    Call book.WriteLine(line.JoinBy(vbTab))
                Next
            End Using
        Next
    End Sub

    Function getInnerTable(text As String) As String
        Do While True
            Dim newText = text.GetTablesHTML

            If newText.IsNullOrEmpty Then
                Return text
            Else
                text = newText(0).Substring(1)
            End If
        Loop

        Return Nothing
    End Function
End Module
