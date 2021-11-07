Imports Microsoft.VisualBasic.Linq

Namespace Model

    Public Class Paragraph

        Public Property sentences As Sentence()

        Public Overrides Function ToString() As String
            Return sentences.JoinBy(". ")
        End Function

        Public Shared Iterator Function Segmentation(text As String) As IEnumerable(Of Paragraph)
            For Each block As String() In text.LineTokens.Split(Function(str) str.Trim = "")
                Yield block.JoinBy(" ").DoCall(AddressOf GetParagraph)
            Next
        End Function

        Private Shared Function GetParagraph(text As String) As Paragraph

        End Function

    End Class
End Namespace