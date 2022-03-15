#Region "Microsoft.VisualBasic::844e07299c8ff46a146d216497ab7b91, sciBASIC#\vs_solutions\dev\VisualStudio\VersionControl\git\log.vb"

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

    '   Total Lines: 57
    '    Code Lines: 39
    ' Comment Lines: 13
    '   Blank Lines: 5
    '     File Size: 2.14 KB


    ' Class log
    ' 
    '     Properties: [date], author, commit, message
    ' 
    '     Function: ParseGitLogText, ParseSvnLogText
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

''' <summary>
''' svn log or git log
''' </summary>
Public Class log

    Public Property commit As String
    Public Property author As String
    Public Property [date] As Date
    Public Property message As String

    ''' <summary>
    ''' parse git log text
    ''' </summary>
    ''' <param name="text">``git log [fileName]``</param>
    ''' <returns></returns>
    Public Shared Iterator Function ParseGitLogText(text As String) As IEnumerable(Of log)
        For Each block As String() In text.LineIterators.Split(Function(line) line.StartsWith("commit "), DelimiterLocation.NextFirst)
            Yield New log With {
                .commit = block(Scan0).Trim.Split.Last,
                .author = block(1).GetTagValue(":", trim:=True).Value,
                .[date] = Date.Parse(block(2).GetTagValue(":", trim:=True).Value),
                .message = block _
                    .Skip(3) _
                    .Select(AddressOf Strings.Trim) _
                    .Where(Function(s) Not s.StringEmpty) _
                    .JoinBy("; ")
            }
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="text">``svn log [fileName]``</param>
    ''' <returns></returns>
    Public Shared Iterator Function ParseSvnLogText(text As String) As IEnumerable(Of log)
        For Each block As String() In text.LineIterators.Split(Function(line) line.IsPattern("[-]+"), DelimiterLocation.NotIncludes)
            Dim tokens As String() = block(Scan0) _
                .Split("|"c) _
                .Select(AddressOf Strings.Trim) _
                .ToArray

            Yield New log With {
                .commit = tokens(Scan0),
                .author = tokens(1),
                .[date] = Date.Parse(tokens(2)),
                .message = block _
                    .Skip(1) _
                    .Select(AddressOf Strings.Trim) _
                    .JoinBy(vbCrLf) _
                    .Trim(" ", ASCII.TAB, ASCII.CR, ASCII.LF)
            }
        Next
    End Function
End Class
