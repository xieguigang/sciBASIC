Imports Microsoft.VisualBasic.Language.C.CLangStringFormatProvider

Namespace Framework.Optimization.LBFGSB

    Public Class Debug

        Public Shared flag As Boolean = False

        ' array and matrix cell formatting
        Private Const cellfmt As String = "%12s"
        ' number formatting
        Private Const numfmt As String = "%.6f"
        ' repeat character
        Private Const repeat As Integer = 10

        Public Shared Sub debug(c As Char, s As String)
            Dim b As String = New String(c, repeat)
            Console.WriteLine(b & " " & s & " " & b)
        End Sub

        Public Shared Sub debug(s As String)
            Console.WriteLine(s)
        End Sub

        Public Shared Sub debug(a As Double())
            debug(Nothing, a)
        End Sub

        Public Shared Sub debug(s As String, a As Double())
            If Not ReferenceEquals(s, Nothing) Then
                Console.Write(s)
            End If

            Console.Write("[")
            For Each v In a
                sprintf(cellfmt, sprintf(numfmt, v)).EchoLine
            Next
            Console.WriteLine("]")
        End Sub

        Public Shared Sub debug(m As Matrix)
            debug(Nothing, m)
        End Sub

        Public Shared Sub debug(s As String, m As Matrix)
            Dim shift As String

            If Not ReferenceEquals(s, Nothing) Then
                Console.Write(s & "[")
                shift = New String(" "c, s.Length + 1)
            Else
                Console.Write("[")
                shift = " "
            End If

            For row = 0 To m.rows - 1
                If row > 0 Then
                    Console.Write(shift)
                End If
                For col = 0 To m.cols - 1
                    sprintf(cellfmt, sprintf(numfmt, m.get(row, col))).EchoLine()
                Next
                If row = m.rows - 1 Then
                    Console.Write("]")
                End If
                Console.WriteLine()
            Next
        End Sub

    End Class

End Namespace
