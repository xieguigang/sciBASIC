Namespace SVG.PathHelper

    ''' <summary>
    ''' the base command for build a svg path
    ''' </summary>
    Public MustInherit Class Command

        Public Property isRelative As Boolean

        Public Shared Function Parse(text As String) As List(Of String)
            Dim s As String = text.Trim()
            Dim tokens = New List(Of String)()
            Dim startIdx = 0
            For i = 0 To s.Length - 1
                If s(i) = "-"c Then i += 1
                While i < s.Length AndAlso s(i) <> "-"c AndAlso s(i) <> " "c AndAlso s(i) <> ","c
                    i += 1
                End While
                Dim stopIdx = i
                If stopIdx > startIdx Then
                    tokens.Add(s.Substring(startIdx, stopIdx - startIdx))
                End If

                startIdx = If(i < s.Length AndAlso s(i) = "-"c, i, i + 1)
            Next
            Return tokens
        End Function

        Public MustOverride Sub Scale(factor As Double)
        Public MustOverride Sub Translate(deltaX As Double, deltaY As Double)
    End Class
End Namespace