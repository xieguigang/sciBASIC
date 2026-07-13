Imports Microsoft.VisualBasic.Text

#If NETSTANDARD Then
Namespace Language

    Public Module Strings

        Public Function LCase(s As String) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.ToLower
            End If
        End Function

        Public Function UCase(s As String) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.ToUpper
            End If
        End Function

        Public Function Len(s As String) As Integer
            If s Is Nothing Then
                Return 0
            Else
                Return s.Length
            End If
        End Function

        Public Function Trim(s As String) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.Trim(ASCII.CR, ASCII.LF, ASCII.TAB, " "c)
            End If
        End Function

        Public Function Val(x As Object) As Double
            If x Is Nothing Then
                Return 0
            End If


        End Function

        Public Function InStr(s As String, find As String) As Integer

        End Function

        Public Function Mid(s As String, start As Integer, Optional len As Integer = -1) As String

        End Function
    End Module
End Namespace
#End If