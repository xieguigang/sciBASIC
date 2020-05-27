#Region "Microsoft.VisualBasic::ba636ff42a319a76a354fe09066cfc04, vs_solutions\dev\VisualStudio\git\log.vb"

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

    ' Class log
    ' 
    '     Properties: [date], author, commit, message
    ' 
    '     Function: ParseLogText
    ' 
    ' /********************************************************************************/

#End Region

Public Class log

    Public Property commit As String
    Public Property author As String
    Public Property [date] As Date
    Public Property message As String

    ''' <summary>
    ''' parse git log text
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Shared Iterator Function ParseLogText(text As String) As IEnumerable(Of log)
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

End Class

