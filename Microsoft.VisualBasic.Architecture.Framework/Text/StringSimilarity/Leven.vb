Imports System.Threading

Namespace Text.Similarity
    '
    'Matching two strings
    'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
    'Copyright (c) 2005 by Thanh Ngoc Dao.
    '

    ''' <summary>
    ''' Summary description for Leven.
    ''' </summary>
    Friend Class Leven : Implements ISimilarity

        Private Function Min3(a As Integer, b As Integer, c As Integer) As Integer
            Return System.Math.Min(System.Math.Min(a, b), c)
        End Function

        Private Function ComputeDistance(s As String, t As String) As Integer
            Dim n As Integer = s.Length
            Dim m As Integer = t.Length
            Dim distance As Integer(,) = New Integer(n, m) {}
            ' matrix
            Dim cost As Integer = 0

            If n = 0 Then
                Return m
            End If
            If m = 0 Then
                Return n
            End If
            'init1
            Dim i As Integer = 0
            While i <= n
                distance(i, 0) = Math.Max(Interlocked.Increment(i), i - 1)
            End While
            Dim j As Integer = 0
            While j <= m
                distance(0, j) = Math.Max(Interlocked.Increment(j), j - 1)
            End While

            'find min distance
            For i = 1 To n
                For j = 1 To m
                    cost = (If(t.Substring(j - 1, 1) = s.Substring(i - 1, 1), 0, 1))
                    distance(i, j) = Min3(distance(i - 1, j) + 1, distance(i, j - 1) + 1, distance(i - 1, j - 1) + cost)
                Next
            Next

            Return distance(n, m)
        End Function

        Public Function GetSimilarity(string1 As System.String, string2 As System.String) As Single Implements ISimilarity.GetSimilarity

            Dim dis As Single = ComputeDistance(string1, string2)
            Dim maxLen As Single = string1.Length
            If maxLen < CSng(string2.Length) Then
                maxLen = string2.Length
            End If

            Dim minLen As Single = string1.Length
            If minLen > CSng(string2.Length) Then
                minLen = string2.Length
            End If


            If maxLen = 0.0F Then
                Return 1.0F
            Else
                'return 1.0F - dis/maxLen ;
                'return (float) Math.Round(1.0F - dis/maxLen, 1) * 10 ;
                Return maxLen - dis
            End If
        End Function

        '
        ' TODO: Add constructor logic here
        '
        Public Sub New()
        End Sub
    End Class
End Namespace