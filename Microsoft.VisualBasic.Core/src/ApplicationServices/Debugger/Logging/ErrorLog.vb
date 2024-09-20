#Region "Microsoft.VisualBasic::28e4191721c5a14d8f5a2c58f81bf29c, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Logging\ErrorLog.vb"

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

    '   Total Lines: 83
    '    Code Lines: 70 (84.34%)
    ' Comment Lines: 7 (8.43%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 6 (7.23%)
    '     File Size: 3.43 KB


    '     Module ErrorLog
    ' 
    '         Function: BugsFormatter, EnvironmentInfo, GetErrorLines
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace ApplicationServices.Debugging.Logging

    Public Module ErrorLog

        <Extension>
        Private Function GetErrorLines(ex As Exception) As String()
            If ex Is Nothing Then
                Return {"<Unknown Error>"}
            Else
                Return ex.ToString.LineTokens
            End If
        End Function

        Public Function EnvironmentInfo() As String
            Return New StringBuilder() _
                .AppendLine(New String("=", 120)) _
                .Append(LogFile.SystemInfo) _
                .AppendLine(New String("=", 120)) _
                .AppendLine() _
                .AppendLine($"Environment Variables from {GetType(App).FullName}:") _
                .AppendLine(ConfigEngine.Prints(App.GetAppVariables)) _
                .AppendLine(New String("=", 120)) _
                .ToString
        End Function

        ''' <summary>
        ''' Generates the formatted error log file content.(生成简单的日志板块的内容)
        ''' </summary>
        ''' <param name="ex"></param>
        ''' <param name="trace"></param>
        ''' <returns></returns>
        '''
        <ExportAPI("Bugs.Formatter")>
        <Extension>
        Public Function BugsFormatter(ex As Exception, <CallerMemberName> Optional trace$ = Nothing) As String
            Dim logs As String() = ex.GetErrorLines
            Dim errorName As String = If(ex Is Nothing, "<Unknown Error>", ex.GetType.FullName)
            Dim stackTrace = logs _
                .Where(Function(s)
                           Return InStr(s, "   在 ") = 1 OrElse InStr(s, "   at ") = 1
                       End Function) _
                .AsList
            Dim message = logs _
                .Where(Function(s)
                           Return Not s.IsPattern("\s+[-]{3}.+?[-]{3}\s*") AndAlso stackTrace.IndexOf(s) = -1
                       End Function) _
                .JoinBy(ASCII.LF) _
                .Trim _
                .StringSplit("\s[-]{3}>\s")

            Return New StringBuilder() _
                .AppendLine("TIME:  " & Now.ToString) _
                .AppendLine("TRACE: " & trace) _
                .AppendLine(EnvironmentInfo) _
                .AppendLine() _
                .AppendLine(errorName & ":") _
                .AppendLine() _
                .AppendLine(message _
                    .Select(Function(s) "    ---> " & s) _
                    .JoinBy(ASCII.LF)) _
                .AppendLine() _
                .AppendLine(stackTrace _
                    .Select(Function(s)
                                If InStr(s, "   在 ") = 1 Then
                                    Return Mid(s, 6).Trim
                                ElseIf InStr(s, "   at ") = 1 Then
                                    Return Mid(s, 7).Trim
                                Else
                                    Return s
                                End If
                            End Function) _
                    .Select(Function(s) "   at " & s) _
                    .JoinBy(ASCII.LF)) _
                .ToString()
        End Function
    End Module
End Namespace
