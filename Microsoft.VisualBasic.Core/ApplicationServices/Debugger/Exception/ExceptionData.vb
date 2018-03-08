Namespace ApplicationServices.Debugging.Diagnostics

    Public Class ExceptionData

        Public Property TypeFullName As String
        Public Property Message As String()
        Public Property StackTrace As StackFrame()

        Public Overrides Function ToString() As String
            Return $"{TypeFullName}::{Message.JoinBy(" ---> ")}"
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
    End Class

    Public Class StackFrame

        Public Property Method As String
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
                    .Method = method,
                    .File = file,
                    .Line = lineNumber
                }
            End With
        End Function
    End Class
End Namespace