Imports Microsoft.VisualBasic.Text

Public Class ConfusionChars

    ReadOnly matrix As New Dictionary(Of Char, Dictionary(Of Char, Boolean))

    Sub Add(c1 As Char, c2 As Char)
        Call AddInternal(c1, c2)
        Call AddInternal(c2, c1)
    End Sub

    Public Function Check(c1 As Char, c2 As Char) As Boolean
        If c1 = c2 Then
            Return True
        End If

        If matrix.ContainsKey(c1) Then
            Return matrix(c1).ContainsKey(c2)
        ElseIf matrix.ContainsKey(c2) Then
            Return matrix(c2).ContainsKey(c1)
        End If

        Return False
    End Function

    Private Sub AddInternal(c1 As Char, c2 As Char)
        If Not matrix.ContainsKey(c1) Then
            Call matrix.Add(c1, New Dictionary(Of Char, Boolean))
        End If
        If Not matrix(c1).ContainsKey(c2) Then
            Call matrix(c1).Add(c2, True)
        End If
    End Sub

    Public Shared Function CreateDefaultMatrix() As ConfusionChars
        Dim chars As New ConfusionChars

        For Each c As (c1 As Char, c2 As Char) In GetTuple()
            Call chars.Add(c.c1, c.c2)
        Next

        Return chars
    End Function

    ''' <summary>
    ''' load default in-memory data
    ''' </summary>
    ''' <returns></returns>
    Private Shared Iterator Function GetTuple() As IEnumerable(Of (Char, Char))
        Yield ("2", "z")
        Yield ("0", "O")
        Yield ("i", "l")
        Yield ("1", "l")
        Yield ("7", "l")
        Yield ("s", "5")
        Yield ("q", "9")
        Yield ("6", "b")
        Yield ("-", "_")
        Yield (" ", ASCII.TAB)
        Yield ("7", "t")
        Yield ("d", "0")
        Yield ("l", "j")
        Yield ("3", "t")
    End Function

End Class