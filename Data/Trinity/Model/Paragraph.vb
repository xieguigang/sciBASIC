Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Model

    Public Class Paragraph

        Public Property sentences As Sentence()

        Public Overrides Function ToString() As String
            Return sentences.JoinBy(". ")
        End Function

        Public Shared Iterator Function Segmentation(text As String) As IEnumerable(Of Paragraph)
            Dim p As New Value(Of Paragraph)

            For Each block As String() In text.LineTokens.Split(Function(str) str.Trim = "")
                If block.All(Function(str) str.Trim.StringEmpty) Then
                    Continue For
                End If

                If Not p = block _
                    .Select(AddressOf Strings.Trim) _
                    .JoinBy(" ") _
                    .Trim _
                    .DoCall(AddressOf GetParagraph) Is Nothing Then

                    Yield p
                End If
            Next
        End Function

        Private Function Trim() As Paragraph
            Dim list As New List(Of Sentence)

            For Each line As Sentence In sentences
                line = line.Trim

                If Not line.IsEmpty Then
                    list.Add(line)
                End If
            Next

            Return New Paragraph With {
                .sentences = list.ToArray
            }
        End Function

        Private Shared Function GetParagraph(text As String) As Paragraph
            Dim sentences As String() = text.Split("."c, "?"c, "!"c)
            Dim sentenceList As Sentence() = sentences _
                .Select(AddressOf Sentence.Parse) _
                .ToArray
            Dim p As New Paragraph With {
                .sentences = sentenceList
            }

            p = p.Trim()

            If p.sentences.IsNullOrEmpty Then
                Return Nothing
            Else
                Return p
            End If
        End Function

    End Class
End Namespace