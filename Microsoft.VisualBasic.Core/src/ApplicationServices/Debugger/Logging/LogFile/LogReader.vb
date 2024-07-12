﻿#Region "Microsoft.VisualBasic::d4c229fe481f75fd59ebf7017212cb90, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Logging\LogFile\LogReader.vb"

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

    '   Total Lines: 67
    '    Code Lines: 46 (68.66%)
    ' Comment Lines: 8 (11.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (19.40%)
    '     File Size: 2.37 KB


    '     Class LogReader
    ' 
    '         Function: (+2 Overloads) Parse, testStartLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Debugging.Logging

    ''' <summary>
    ''' 分析服务器的问题原因所需要的日志分析工具
    ''' </summary>
    Public Class LogReader

        Shared ReadOnly header As New Regex("\[.+\]\[((INFOM)|(ERROR)|(WARNG)|(DEBUG)|(FINEST))\]\[.+\]", RegexOptions.Compiled)

        Private Shared Function testStartLine(line As String) As Boolean
            If line.StringEmpty(, True) Then
                Return False
            End If

            Dim m = header.Match(line)

            If Not m.Success Then
                Return False
            End If

            Return line.StartsWith(m.Value)
        End Function

        ''' <summary>
        ''' a file parser for the document text of <see cref="LogFile"/>
        ''' </summary>
        ''' <param name="logFile"></param>
        ''' <returns></returns>
        Public Shared Iterator Function Parse(logFile As String) As IEnumerable(Of LogEntry)
            Dim buf As New List(Of String)

            For Each line As String In logFile.IterateAllLines
                If testStartLine(line) Then
                    If buf > 0 Then
                        Yield Parse(buf.PopAll)
                    End If
                End If

                buf += line
            Next

            If buf > 0 Then
                Yield Parse(buf.PopAll)
            End If
        End Function

        Private Shared Function Parse(data As String()) As LogEntry
            Dim header As String = data(0)
            Dim text As String = data.Skip(1).JoinBy(vbCrLf)
            Dim sections = header.Matches("\[.*?\]").ToArray
            Dim time As Date = Date.Parse(sections(0).GetStackValue("[", "]"))
            Dim level As String = sections(1).GetStackValue("[", "]")
            Dim objName As String = sections(2).GetStackValue("[", "]")
            Dim remains As String = header.Substring(sections.Take(3).JoinBy("").Length)

            Return New LogEntry With {
                .level = [Enum].Parse(GetType(MSG_TYPES), level),
                .message = remains & vbCrLf & text,
                .[object] = objName,
                .time = time
            }
        End Function
    End Class
End Namespace
