Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Debugging.Logging

    Module ErrorLog

        <Extension>
        Private Function GetErrorLines(ex As Exception) As String()
            If ex Is Nothing Then
                Return {"<Unknown Error>"}
            Else
                Return ex.ToString.LineTokens
            End If
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
                .AppendLine(New String("=", 120)) _
                .Append(LogFile.SystemInfo) _
                .AppendLine(New String("=", 120)) _
                .AppendLine() _
                .AppendLine($"Environment Variables from {GetType(App).FullName}:") _
                .AppendLine(ConfigEngine.Prints(App.GetAppVariables)) _
                .AppendLine(New String("=", 120)) _
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