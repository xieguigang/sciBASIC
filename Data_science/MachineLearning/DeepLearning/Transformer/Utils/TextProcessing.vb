Imports System.IO
Imports System.Runtime.InteropServices

Namespace Transformer
    Public Module TextProcessing
        Public Sub Load(filename As String, nrSentences As Integer, <Out> ByRef englishSentences As List(Of List(Of String)), <Out> ByRef spanishSentences As List(Of List(Of String)))
            englishSentences = New List(Of List(Of String))()
            spanishSentences = New List(Of List(Of String))()

            Using reader = New StreamReader(filename)
                If reader IsNot Nothing Then
                    Dim s = 1
                    Dim splitchars = New Char() {"."c, "?"c, "!"c, ChrW(9)}
                    Dim line As String = reader.ReadLine()
                    While Not Equals(line, Nothing)
                        line = line.ToLower()
                        Dim isquestion = line.Contains("?")
                        Dim isexclamation = line.Contains("!")
                        Dim split = line.Split(splitchars, StringSplitOptions.RemoveEmptyEntries)
                        If isquestion Then
                            split(0) += " ?"
                            split(1) += " ?"
                        End If
                        If isexclamation Then
                            split(0) += " !"
                            split(1) += " !"
                        End If
                        englishSentences.Add(New List(Of String)(split(0).Split()))
                        spanishSentences.Add(New List(Of String)(split(1).Split()))
                        If Math.Min(Threading.Interlocked.Increment(s), s - 1) >= nrSentences Then Exit While

                        line = reader.ReadLine()
                    End While
                End If
            End Using
        End Sub

        Public Function ProcessSentence(sentenceString As String) As List(Of List(Of String))
            Dim sentence = New List(Of List(Of String))()

            sentenceString = sentenceString.Replace("?", " ?")
            sentenceString = sentenceString.Replace("!", " !")
            sentenceString = sentenceString.Replace(".", "")

            Dim splitchars = New Char() {" "c, ChrW(9)}
            sentence.Add(New List(Of String)(sentenceString.Split(splitchars, StringSplitOptions.RemoveEmptyEntries)))

            Return sentence
        End Function

        Public Function ProcessSentence(sentence As List(Of List(Of String)), s As Integer) As String
            If sentence.Count <= s Then Throw New ArgumentException("Index out of range")

            Dim sentenceString = ""
            For i = 0 To sentence(s).Count - 1
                sentenceString += sentence(s)(i).ToString() & " "
            Next

            Return sentenceString.Trim()
        End Function

        Public Function CalculateSequenceLength(englishSentences As List(Of List(Of String)), spanishSentences As List(Of List(Of String))) As Integer
            Dim sequenceLength = 0
            For s = 0 To englishSentences.Count - 1
                sequenceLength = Math.Max(sequenceLength, englishSentences(s).Count())
                sequenceLength = Math.Max(sequenceLength, spanishSentences(s).Count())
            Next

            Return sequenceLength
        End Function

        Public Function CalculateMaxSentenceLength(sentences As List(Of List(Of String))) As Integer
            Dim maxSentenceLength = 0
            For s = 0 To sentences.Count - 1
                maxSentenceLength = Math.Max(maxSentenceLength, sentences(s).Count())
            Next

            Return maxSentenceLength
        End Function

        Public Sub InsertStartAndStopCharacters(correctSpanishSentences As List(Of List(Of String)))
            For s = 0 To correctSpanishSentences.Count - 1
                If Not Equals(correctSpanishSentences(s)(0), "<") Then correctSpanishSentences(s).Insert(0, "<")
                If Not Equals(correctSpanishSentences(s)(correctSpanishSentences(s).Count - 1), ">") Then correctSpanishSentences(s).Add(">")
            Next
        End Sub

        Public Function InitializeSpanishSentences(batchSize As Integer) As List(Of List(Of String))
            Dim translatedSpanishSentences As List(Of List(Of String)) = New List(Of List(Of String))()
            For s = 0 To batchSize - 1
                translatedSpanishSentences.Add(New List(Of String)() From {
                    "<"
                })
            Next

            Return translatedSpanishSentences
        End Function

        Public Sub AddWordsToSentences(batchSize As Integer, isTraining As Boolean, spanishWords As String(), translatedSpanishSentences As List(Of List(Of String)))
            For s = 0 To batchSize - 1
                Dim sentenceLength = translatedSpanishSentences(s).Count
                If sentenceLength > 0 AndAlso Equals(translatedSpanishSentences(s)(sentenceLength - 1), ">") Then Continue For

                translatedSpanishSentences(s).Add(spanishWords(s))
            Next
        End Sub

    End Module
End Namespace
