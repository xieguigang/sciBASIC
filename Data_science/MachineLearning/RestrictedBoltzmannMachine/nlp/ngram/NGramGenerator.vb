Imports Microsoft.VisualBasic
Imports System.Text

Namespace nlp.ngram

    ''' <summary>
    ''' Created by kenny on 5/23/14.
    ''' </summary>
    Public Class NGramGenerator

        Private ReadOnly n As Integer

        Public Sub New(n As Integer)
            Me.n = n
        End Sub

        Public Overridable Function generate(sentence As String) As IList(Of String)

            Dim ngrams As IList(Of String) = New List(Of String)()
            Dim words = sentence.StringSplit(" ", True)
            For i = 0 To words.Length - n + 1 - 1
                ngrams.Add(concat(words, i, i + n))
            Next
            Return ngrams
        End Function

        Public Overridable Function concat(words As String(), start As Integer, [end] As Integer) As String
            Dim sb As StringBuilder = New StringBuilder()
            For i = start To [end] - 1
                sb.Append(If(i > start, " ", "") & words(i))
            Next
            Return sb.ToString()
        End Function

    End Class

End Namespace
