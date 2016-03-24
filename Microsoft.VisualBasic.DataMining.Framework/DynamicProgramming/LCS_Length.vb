Imports System

Namespace DynamicProgramming

    ''' <summary>
    ''' Longest Common Subsequence
    ''' </summary>
    Public Module LCS_Length

        Public Function MaxLengthSubString(a As String, b As String) As String
            Return MaxSet(a.ToArray, b.ToArray, AddressOf __equals)
        End Function

        Private Function __equals(a As Char, b As Char) As Boolean
            Return a = b
        End Function

        Public Function MaxSet(Of T)(a As T(), b As T(), equals As Equals(Of T)) As T()
            Dim m As Integer = a.Length
            Dim n As Integer = b.Length
            Dim len()() As Integer = MAT(Of Integer)(m + 1, n + 1)
            Dim p()() As Char = MAT(Of Char)(m + 1, n + 1)

            For i As Integer = 1 To m
                For j As Integer = 1 To n

                    If equals(a(i - 1), b(j - 1)) Then
                        len(i)(j) = len(i - 1)(j - 1) + 1
                        p(i)(j) = "-"c
                    ElseIf len(i - 1)(j) >= len(i)(j - 1) Then
                        len(i)(j) = len(i - 1)(j)
                        p(i)(j) = "<"c
                    Else
                        len(i)(j) = len(i)(j - 1)
                        p(i)(j) = ">"c
                    End If
                Next j
            Next i

            Dim lst As New List(Of T)

            __LCS(p, a, a.Length, b.Length, lst)

            Call lst.Reverse()

            Return lst.ToArray
        End Function

        Private Sub __LCS(Of T)(p()() As Char, a() As T, i As Integer, j As Integer, ByRef lst As List(Of T))
            If i = 0 OrElse j = 0 Then
                Return
            End If

            If p(i)(j) = "-"c Then
                __LCS(p, a, i - 1, j - 1, lst)
                lst += a(i - 1)

            ElseIf p(i)(j) = "<"c Then
                __LCS(p, a, i - 1, j, lst)

            ElseIf p(i)(j) = ">"c Then
                __LCS(p, a, i, j - 1, lst)
            End If
        End Sub
    End Module
End Namespace
