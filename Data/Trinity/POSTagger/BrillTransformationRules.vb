#Region "Microsoft.VisualBasic::351d2554aafa50a92b5cb337133f19ac, Data\Trinity\POSTagger\BrillTransformationRules.vb"

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

    '   Total Lines: 112
    '    Code Lines: 78 (69.64%)
    ' Comment Lines: 16 (14.29%)
    '    - Xml Docs: 31.25%
    ' 
    '   Blank Lines: 18 (16.07%)
    '     File Size: 4.90 KB


    '     Class BrillTransformationRules
    ' 
    '         Function: GetRule, GetRules
    ' 
    '         Sub: AppendRule, Rule1, Rule2, Rule3, Rule4
    '              Rule5, Rule6, Rule7, Rule8, SetRule
    '              SetRules
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

        Public Function GetRule(index As Integer) As Action(Of List(Of PartOfSpeech), Integer)
            Return _rules(index)
        End Function

        Public Sub SetRule(index As Integer, rule As Action(Of List(Of PartOfSpeech), Integer))
            _rules(index) = rule
        End Sub

        Public Sub AppendRule(rule As Action(Of List(Of PartOfSpeech), Integer))
            _rules.Add(rule)
        End Sub

        Public Sub SetRules(newRules As IEnumerable(Of Action(Of List(Of PartOfSpeech), Integer)))
            _rules.Clear()
            _rules.AddRange(newRules)
        End Sub

        Public Function GetRules() As IEnumerable(Of Action(Of List(Of PartOfSpeech), Integer))
            Return _rules
        End Function

        Private Shared Sub Rule1(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If index <= 0 OrElse Not Equals(taggedSentence(index - 1).Tag, "DT") Then
                Return
            End If
            If Equals(taggedSentence(index).Tag, "VBD") OrElse Equals(taggedSentence(index).Tag, "VBP") OrElse Equals(taggedSentence(index).Tag, "VB") Then
                taggedSentence(index).Tag = "NN"
            End If
        End Sub

        ' rule 2: convert a noun to a number (CD) if "." appears in the word
        Private Shared Sub Rule2(taggedSentence As List(Of PartOfSpeech), index As Integer)
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
        Private Shared Sub Rule3(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If taggedSentence(index).Tag.StartsWith("N") AndAlso taggedSentence(index).Word.EndsWith("ed") Then
                taggedSentence(index).Tag = "VBN"
            End If
        End Sub

        ' rule 4: convert any type to adverb if it ends in "ly";
        Private Shared Sub Rule4(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If taggedSentence(index).Word.EndsWith("ly") Then
                taggedSentence(index).Tag = "RB"
            End If
        End Sub

        ' rule 5: convert a common noun (NN or NNS) to a adjective if it ends with "al"
        Private Shared Sub Rule5(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If taggedSentence(index).Tag.StartsWith("NN") AndAlso taggedSentence(index).Word.EndsWith("al") Then
                taggedSentence(index).Tag = "JJ"
            End If
        End Sub

        ' rule 6: convert a noun to a verb if the preceding work is "would"
        Private Shared Sub Rule6(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If index > 0 AndAlso taggedSentence(index).Tag.StartsWith("NN") AndAlso Equals(taggedSentence(index - 1).Word.ToLower(), "would") Then
                taggedSentence(index).Tag = "VB"
            End If
        End Sub

        ' rule 7: if a word has been categorized as a common noun and it ends with "s",
        '         then set its type to plural common noun (NNS)
        Private Shared Sub Rule7(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If Equals(taggedSentence(index).Tag, "NN") AndAlso taggedSentence(index).Word.EndsWith("s") Then
                taggedSentence(index).Tag = "NNS"
            End If
        End Sub

        ' rule 8: convert a common noun to a present participle verb (i.e., a gerund)
        Private Shared Sub Rule8(taggedSentence As List(Of PartOfSpeech), index As Integer)
            If taggedSentence(index).Tag.StartsWith("NN") AndAlso taggedSentence(index).Word.EndsWith("ing") Then
                taggedSentence(index).Tag = "VBG"
            End If
        End Sub
    End Class
End Namespace
