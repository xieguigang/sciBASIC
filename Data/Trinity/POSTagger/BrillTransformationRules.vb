Imports System.Text.RegularExpressions

Namespace POSTagger

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/Korthax/pos-net
    ''' </remarks>
    Public Class BrillTransformationRules

        ReadOnly _rules As New List(Of Action(Of List(Of PartOfSpeech), Integer)) From {
            AddressOf Rule1,
            AddressOf Rule2,
            AddressOf Rule3,
            AddressOf Rule4,
            AddressOf Rule5,
            AddressOf Rule6,
            AddressOf Rule7,
            AddressOf Rule8
        }

        Public Function GetRule(ByVal index As Integer) As Action(Of List(Of PartOfSpeech), Integer)
            Return _rules(index)
        End Function

        Public Sub SetRule(ByVal index As Integer, ByVal rule As Action(Of List(Of PartOfSpeech), Integer))
            _rules(index) = rule
        End Sub

        Public Sub AppendRule(ByVal rule As Action(Of List(Of PartOfSpeech), Integer))
            _rules.Add(rule)
        End Sub

        Public Sub SetRules(ByVal newRules As IEnumerable(Of Action(Of List(Of PartOfSpeech), Integer)))
            _rules.Clear()
            _rules.AddRange(newRules)
        End Sub

        Public Function GetRules() As IEnumerable(Of Action(Of List(Of PartOfSpeech), Integer))
            Return _rules
        End Function

        Private Shared Sub Rule1(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If index <= 0 OrElse Not Equals(taggedSentence(index - 1).Tag, "DT") Then
                Return
            End If
            If Equals(taggedSentence(index).Tag, "VBD") OrElse Equals(taggedSentence(index).Tag, "VBP") OrElse Equals(taggedSentence(index).Tag, "VB") Then
                taggedSentence(index).Tag = "NN"
            End If
        End Sub

        ' rule 2: convert a noun to a number (CD) if "." appears in the word
        Private Shared Sub Rule2(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If Not taggedSentence(index).Tag.StartsWith("N") Then Return

            If taggedSentence(index).Word.Contains(".") Then
                ' url if there are two contiguous alpha characters
                taggedSentence(index).Tag = If(Regex.IsMatch(taggedSentence(index).Word, "/[a-zA-Z]{2}/"), "URL", "CD")
            End If

            ' Attempt to convert into a number
            If Single.TryParse(taggedSentence(CInt(index)).Word, Nothing) Then
                taggedSentence(index).Tag = "CD"
            End If
        End Sub

        ' rule 3: convert a noun to a past participle if words[i] ends with "ed"
        Private Shared Sub Rule3(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If taggedSentence(index).Tag.StartsWith("N") AndAlso taggedSentence(index).Word.EndsWith("ed") Then
                taggedSentence(index).Tag = "VBN"
            End If
        End Sub

        ' rule 4: convert any type to adverb if it ends in "ly";
        Private Shared Sub Rule4(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If taggedSentence(index).Word.EndsWith("ly") Then
                taggedSentence(index).Tag = "RB"
            End If
        End Sub

        ' rule 5: convert a common noun (NN or NNS) to a adjective if it ends with "al"
        Private Shared Sub Rule5(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If taggedSentence(index).Tag.StartsWith("NN") AndAlso taggedSentence(index).Word.EndsWith("al") Then
                taggedSentence(index).Tag = "JJ"
            End If
        End Sub

        ' rule 6: convert a noun to a verb if the preceding work is "would"
        Private Shared Sub Rule6(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If index > 0 AndAlso taggedSentence(index).Tag.StartsWith("NN") AndAlso Equals(taggedSentence(index - 1).Word.ToLower(), "would") Then
                taggedSentence(index).Tag = "VB"
            End If
        End Sub

        ' rule 7: if a word has been categorized as a common noun and it ends with "s",
        '         then set its type to plural common noun (NNS)
        Private Shared Sub Rule7(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If Equals(taggedSentence(index).Tag, "NN") AndAlso taggedSentence(index).Word.EndsWith("s") Then
                taggedSentence(index).Tag = "NNS"
            End If
        End Sub

        ' rule 8: convert a common noun to a present participle verb (i.e., a gerund)
        Private Shared Sub Rule8(ByVal taggedSentence As List(Of PartOfSpeech), ByVal index As Integer)
            If taggedSentence(index).Tag.StartsWith("NN") AndAlso taggedSentence(index).Word.EndsWith("ing") Then
                taggedSentence(index).Tag = "VBG"
            End If
        End Sub
    End Class
End Namespace
