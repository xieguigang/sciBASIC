Namespace ApplicationServices.Debugging.Diagnostics

    Public Class StackFrame

        ''' <summary>
        ''' Method call
        ''' </summary>
        ''' <returns></returns>
        Public Property Method As Method
        ''' <summary>
        ''' The file path of the source file
        ''' </summary>
        ''' <returns></returns>
        Public Property File As String
        ''' <summary>
        ''' The line number in current source <see cref="File"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Property Line As String

        Public Overrides Function ToString() As String
            Return $"{Method} at {File}:line {Line}"
        End Function

        Public Shared Function Parser(line As String) As StackFrame
            With line.Replace("位置", "in").Replace("行号", "line")
                Dim t = .StringSplit(" in ")
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