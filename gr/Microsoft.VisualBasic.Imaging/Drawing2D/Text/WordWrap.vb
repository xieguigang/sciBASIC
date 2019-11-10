Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace Drawing2D.Text

    Public Module WordWrap

        <Extension>
        Public Function DoWordWrap(str$, width%, Optional splitChars$ = " -" & ASCII.TAB) As String
            Dim words = Explode(str, splitChars)
            Dim curLineLength = 0
            Dim sb As New StringBuilder()

            For i As Integer = 0 To words.Length - 1
                Dim word = words(i)

                ' If adding the New word to the current line would be too long,
                ' then put it on a New line (And split it up if it's too long).
                If (curLineLength + word.Length > width) Then

                    ' Only move down to a New line if we have text on the current line.
                    ' Avoids situation where wrapped whitespace causes emptylines in text.
                    If (curLineLength > 0) Then
                        sb.Append(Environment.NewLine)
                        curLineLength = 0
                    End If

                    ' If the current word Is too long to fit on a line even on it's own then
                    ' split the word up.
                    Do While (word.Length > width)
                        sb.Append(word.Substring(0, width - 1) & "-")
                        word = word.Substring(width - 1)
                        sb.Append(Environment.NewLine)
                    Loop

                    ' Remove leading whitespace from the word so the New line starts flush to the left.
                    word = word.TrimStart()
                End If

                sb.Append(word)
                curLineLength += word.Length
            Next

            Return sb.ToString()
        End Function

        Public Function Explode(str$, splitChars As Char()) As String()
            Dim parts As New List(Of String)()
            Dim startIndex = 0

            Do While True
                Dim index = str.IndexOfAny(splitChars, startIndex)

                If (index = -1) Then
                    Return parts + str.Substring(startIndex)
                End If

                Dim word = str.Substring(startIndex, index - startIndex)
                Dim nextChar = str.Substring(index, 1)(0)

                ' Dashes And the likes should stick to the word occuring before it. 
                ' Whitespace doesn't have to.
                If (Char.IsWhiteSpace(nextChar)) Then
                    parts.Add(word)
                    parts.Add(nextChar.ToString())
                Else
                    parts.Add(word + nextChar)
                End If

                startIndex = index + 1
            Loop

            Throw New Exception("This exception will never happends")
        End Function
    End Module
End Namespace