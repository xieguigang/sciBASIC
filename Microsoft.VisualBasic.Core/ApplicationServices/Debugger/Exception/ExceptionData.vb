Namespace ApplicationServices.Debugging.Diagnostics

    Public Class ExceptionData

        Public Property TypeFullName As String
        Public Property Message As String()
        Public Property StackTrace As StackFrame()

        Public Overrides Function ToString() As String
            Return TypeFullName
        End Function

        Public Shared Function CreateInstance(messages$(), stackTrace$(), type$) As ExceptionData
            Return New ExceptionData With {
                .TypeFullName = type,
                .Message = messages,
                .StackTrace = stackTrace _
                    .Select(AddressOf StackFrame.Parser) _
                    .ToArray
            }
        End Function

        Public Shared Function GetCurrentStackTrace() As StackFrame()
            Return Environment.StackTrace _
                .lTokens _
                .Where(Function(s) Not s.StringEmpty) _
                .Skip(3) _
                .Select(Function(s)
                            s = Mid(s, 6).Trim
                            Return StackFrame.Parser(s)
                        End Function) _
                .ToArray
        End Function
    End Class

    Public Class StackFrame

        Public Property Method As Method
        Public Property File As String
        Public Property Line As String

        Public Overrides Function ToString() As String
            Return $"{Method} at {File}:line {Line}"
        End Function

        Public Shared Function Parser(line As String) As StackFrame
            With line.Replace("位置", "at").Replace("行号", "line")
                Dim t = .StringSplit(" at ")
                Dim method = t(0)
                Dim location = t.ElementAtOrDefault(1)
                Dim file$, lineNumber$

                If Not location.StringEmpty Then
                    t = location.StringSplit("[:]line ")
                    file = t(0)
                    lineNumber = t(1)
                Else
                    file = "Unknown"
                    lineNumber = 0
                End If

                Return New StackFrame With {
                    .Method = New Method(method),
                    .File = file,
                    .Line = lineNumber
                }
            End With
        End Function
    End Class

    Public Class Method

        Public Property [Namespace] As String
        Public Property [Module] As String
        Public Property Method As String

        Sub New(s As String)
            Dim t = s.Split("."c).AsList

            Method = t(-1)
            [Module] = t(-2)
            [Namespace] = t.Take(t.Count - 2).JoinBy(".")
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[Namespace]}.{[Module]}.{Method}"
        End Function
    End Class
End Namespace