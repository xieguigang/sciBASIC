#Region "Microsoft.VisualBasic::aeac6967faadad97a2c9eea28aa83cc1, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Logging\LogFile\LogReader.vb"

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

'   Total Lines: 14
'    Code Lines: 7 (50.00%)
' Comment Lines: 3 (21.43%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 4 (28.57%)
'     File Size: 296 B


'     Class LogReader
' 
'         Constructor: (+1 Overloads) Sub New
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

        Shared ReadOnly header As New Regex("\[.+\]\[(INFOM)|(ERROR)|(WARNG)|(DEBUG)|(FINEST)\]\[.+\]", RegexOptions.Compiled)

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

        End Function
    End Class
End Namespace
