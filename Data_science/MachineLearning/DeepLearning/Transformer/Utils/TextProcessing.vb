#Region "Microsoft.VisualBasic::1506b383b896ec3aaa0ddc0beb16ea55, Data_science\MachineLearning\DeepLearning\Transformer\Utils\TextProcessing.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 160
    '    Code Lines: 90 (56.25%)
    ' Comment Lines: 50 (31.25%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (12.50%)
    '     File Size: 8.38 KB


    '     Module TextProcessing
    ' 
    '         Function: CalculateMaxSentenceLength, CalculateSequenceLength, InitializeSpanishSentences, (+2 Overloads) ProcessSentence
    ' 
    '         Sub: AddWordsToSentences, InsertStartAndStopCharacters, Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports std = System.Math

Namespace Transformer
    ''' <summary>
    ''' Text preprocessing helpers used by the translation demo: loading parallel sentence pairs, tokenizing sentences,
    ''' computing sequence lengths and adding the start / stop markers.
    ''' </summary>
    Public Module TextProcessing
        ''' <summary>
        ''' Loads up to <paramref name="nrSentences"/> parallel sentence pairs from a tab separated training file.
        ''' </summary>
        ''' <param name="filename">Path of the training data file.</param>
        ''' <param name="nrSentences">Maximum number of sentence pairs to read.</param>
        ''' <param name="englishSentences">Receives the tokenized source sentences.</param>
        ''' <param name="spanishSentences">Receives the tokenized target sentences.</param>
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
                        If std.Min(Threading.Interlocked.Increment(s), s - 1) >= nrSentences Then Exit While

                        line = reader.ReadLine()
                    End While
                End If
            End Using
        End Sub

        ''' <summary>
        ''' Tokenizes a sentence and separates trailing question marks and exclamation marks.
        ''' </summary>
        ''' <param name="sentenceString">The sentence to tokenize.</param>
        ''' <returns>The tokenized sentence wrapped in a single element batch.</returns>
        Public Function ProcessSentence(sentenceString As String) As List(Of List(Of String))
            Dim sentence = New List(Of List(Of String))()

            sentenceString = sentenceString.Replace("?", " ?")
            sentenceString = sentenceString.Replace("!", " !")
            sentenceString = sentenceString.Replace(".", "")

            Dim splitchars = New Char() {" "c, ChrW(9)}
            sentence.Add(New List(Of String)(sentenceString.Split(splitchars, StringSplitOptions.RemoveEmptyEntries)))

            Return sentence
        End Function

        ''' <summary>
        ''' Joins the tokens of one sentence of a batch back into a string.
        ''' </summary>
        ''' <param name="sentence">The batch of tokenized sentences.</param>
        ''' <param name="s">Index of the sentence to render.</param>
        ''' <returns>The joined sentence.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="s"/> is out of range.</exception>
        Public Function ProcessSentence(sentence As List(Of List(Of String)), s As Integer) As String
            If sentence.Count <= s Then Throw New ArgumentException("Index out of range")

            Dim sentenceString = ""
            For i = 0 To sentence(s).Count - 1
                sentenceString += sentence(s)(i).ToString() & " "
            Next

            Return sentenceString.Trim()
        End Function

        ''' <summary>
        ''' Computes the length of the longest sentence across both languages.
        ''' </summary>
        ''' <param name="englishSentences">The source sentences.</param>
        ''' <param name="spanishSentences">The target sentences.</param>
        ''' <returns>The maximum sentence length.</returns>
        Public Function CalculateSequenceLength(englishSentences As List(Of List(Of String)), spanishSentences As List(Of List(Of String))) As Integer
            Dim sequenceLength = 0
            For s = 0 To englishSentences.Count - 1
                sequenceLength = std.Max(sequenceLength, englishSentences(s).Count())
                sequenceLength = std.Max(sequenceLength, spanishSentences(s).Count())
            Next

            Return sequenceLength
        End Function

        ''' <summary>
        ''' Computes the length of the longest sentence in the given collection.
        ''' </summary>
        ''' <param name="sentences">The sentences to inspect.</param>
        ''' <returns>The maximum sentence length.</returns>
        Public Function CalculateMaxSentenceLength(sentences As List(Of List(Of String))) As Integer
            Dim maxSentenceLength = 0
            For s = 0 To sentences.Count - 1
                maxSentenceLength = std.Max(maxSentenceLength, sentences(s).Count())
            Next

            Return maxSentenceLength
        End Function

        ''' <summary>
        ''' Adds the start (<c>&lt;</c>) and stop (<c>&gt;</c>) markers to every target sentence, when not already present.
        ''' </summary>
        ''' <param name="correctSpanishSentences">The target sentences to modify in place.</param>
        Public Sub InsertStartAndStopCharacters(correctSpanishSentences As List(Of List(Of String)))
            For s = 0 To correctSpanishSentences.Count - 1
                If Not Equals(correctSpanishSentences(s)(0), "<") Then correctSpanishSentences(s).Insert(0, "<")
                If Not Equals(correctSpanishSentences(s)(correctSpanishSentences(s).Count - 1), ">") Then correctSpanishSentences(s).Add(">")
            Next
        End Sub

        ''' <summary>
        ''' Creates the initial decoder input: one sentence per batch element, containing only the start marker.
        ''' </summary>
        ''' <param name="batchSize">Number of sentences in the batch.</param>
        ''' <returns>The initialized decoder input.</returns>
        Public Function InitializeSpanishSentences(batchSize As Integer) As List(Of List(Of String))
            Dim translatedSpanishSentences As List(Of List(Of String)) = New List(Of List(Of String))()
            For s = 0 To batchSize - 1
                translatedSpanishSentences.Add(New List(Of String)() From {
                    "<"
                })
            Next

            Return translatedSpanishSentences
        End Function

        ''' <summary>
        ''' Appends the predicted word of every batch element to its sentence, skipping already finished sentences.
        ''' </summary>
        ''' <param name="batchSize">Number of sentences in the batch.</param>
        ''' <param name="isTraining">Reserved for compatibility; the current implementation behaves the same in both modes.</param>
        ''' <param name="spanishWords">The predicted word of each batch element.</param>
        ''' <param name="translatedSpanishSentences">The sentences to append to, modified in place.</param>
        Public Sub AddWordsToSentences(batchSize As Integer, isTraining As Boolean, spanishWords As String(), translatedSpanishSentences As List(Of List(Of String)))
            For s = 0 To batchSize - 1
                Dim sentenceLength = translatedSpanishSentences(s).Count
                If sentenceLength > 0 AndAlso Equals(translatedSpanishSentences(s)(sentenceLength - 1), ">") Then Continue For

                translatedSpanishSentences(s).Add(spanishWords(s))
            Next
        End Sub

    End Module
End Namespace
