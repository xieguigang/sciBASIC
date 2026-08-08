#Region "Microsoft.VisualBasic::03bc5494726655937f14819528368c90, nlp\NLP\Tokenizer\src\HuggingFace\UnigramModel.vb"

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

    '   Total Lines: 267
    '    Code Lines: 178 (66.67%)
    ' Comment Lines: 36 (13.48%)
    '    - Xml Docs: 80.56%
    ' 
    '   Blank Lines: 53 (19.85%)
    '     File Size: 9.66 KB


    '     Class UnigramModel
    ' 
    '         Properties: VocabSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetScore, IdToToken, Tokenize, TokenToId, Viterbi
    '         Class TrieNode
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' the sciBASIC framework declares its own 'Encoding' symbol in the global
' namespace, so an explicit alias is required here to reference the BCL type.
Imports TextEncoding = System.Text.Encoding

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' Unigram 语言模型子词切分（SentencePiece）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 BPE 的贪心合并不同，Unigram 会在<b>所有可能的切分方案</b>中挑选对数概率之和
    ''' 最大的那一种。这里使用 Viterbi 动态规划求解：设 <c>best(i)</c> 为前 i 个字符的
    ''' 最优得分，则 <c>best(j) = max(best(i) + score(text[i..j]))</c>，最后从末端回溯
    ''' 即可得到最优切分路径。
    ''' </para>
    ''' <para>
    ''' 为了避免在每个位置上都去枚举全部长度的子串，这里为词表构建了一棵字典树，
    ''' 从位置 i 出发沿树推进即可一次性枚举出所有以 i 开头的候选词，整体复杂度为
    ''' O(n × maxTokenLength)。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class UnigramModel : Implements ITokenizerModel

        ''' <summary>
        ''' 字典树节点。
        ''' </summary>
        Private NotInheritable Class TrieNode
            Public ReadOnly Children As New Dictionary(Of Char, TrieNode)
            ''' <summary>该节点所对应的词表编号，-1 表示不是终结点。</summary>
            Public Id As Integer = -1
        End Class

        ''' <summary>
        ''' 未登录字符的惩罚项，与 sentencepiece 的实现保持一致。
        ''' </summary>
        Private Const UnkPenalty As Double = 10.0

        Private ReadOnly _vocab As Dictionary(Of String, Integer)
        Private ReadOnly _idToToken As String()
        Private ReadOnly _scores As Double()
        Private ReadOnly _root As New TrieNode
        Private ReadOnly _unkId As Integer
        Private ReadOnly _byteFallback As Boolean
        Private ReadOnly _fuseUnk As Boolean
        Private ReadOnly _minScore As Double

        Public ReadOnly Property VocabSize As Integer Implements ITokenizerModel.VocabSize
            Get
                Return _vocab.Count
            End Get
        End Property

        Public Sub New(model As ModelSection)
            _vocab = model.Vocab
            _idToToken = model.IdToToken
            _scores = If(model.UnigramScores, New Double(_idToToken.Length - 1) {})
            _unkId = model.UnkId
            _byteFallback = model.ByteFallback
            _fuseUnk = model.FuseUnk
            _minScore = Double.MaxValue

            If _unkId < 0 AndAlso Not model.UnkToken Is Nothing Then
                Dim id As Integer

                If _vocab.TryGetValue(model.UnkToken, id) Then
                    _unkId = id
                End If
            End If

            For Each entry As KeyValuePair(Of String, Integer) In _vocab
                If String.IsNullOrEmpty(entry.Key) Then
                    Continue For
                End If

                Dim node As TrieNode = _root

                For Each c As Char In entry.Key
                    Dim [next] As TrieNode = Nothing

                    If Not node.Children.TryGetValue(c, [next]) Then
                        [next] = New TrieNode
                        node.Children.Add(c, [next])
                    End If

                    node = [next]
                Next

                node.Id = entry.Value

                If entry.Value < _scores.Length AndAlso _scores(entry.Value) < _minScore Then
                    _minScore = _scores(entry.Value)
                End If
            Next

            If _minScore = Double.MaxValue Then
                _minScore = 0.0
            End If
        End Sub

        Public Function TokenToId(token As String) As Integer? Implements ITokenizerModel.TokenToId
            Dim id As Integer

            If token IsNot Nothing AndAlso _vocab.TryGetValue(token, id) Then
                Return id
            Else
                Return Nothing
            End If
        End Function

        Public Function IdToToken(id As Integer) As String Implements ITokenizerModel.IdToToken
            If id >= 0 AndAlso id < _idToToken.Length Then
                Return _idToToken(id)
            Else
                Return Nothing
            End If
        End Function

        Public Iterator Function Tokenize(fragment As String) As IEnumerable(Of Token) Implements ITokenizerModel.Tokenize
            If String.IsNullOrEmpty(fragment) Then
                Return
            End If

            Dim path As List(Of Token) = Viterbi(fragment)
            Dim pendingUnk As Boolean = False

            For Each token As Token In path
                If token.Id >= 0 Then
                    pendingUnk = False
                    Yield token
                    Continue For
                End If

                ' the piece is not covered by the vocabulary
                If _byteFallback Then
                    Dim resolved As Boolean = True
                    Dim fallback As New List(Of Token)

                    For Each b As Byte In TextEncoding.UTF8.GetBytes(token.Value)
                        Dim name As String = "<0x" & b.ToString("X2") & ">"
                        Dim id As Integer

                        If _vocab.TryGetValue(name, id) Then
                            fallback.Add(New Token(id, name, token.Start, token.End))
                        Else
                            resolved = False
                            Exit For
                        End If
                    Next

                    If resolved Then
                        pendingUnk = False

                        For Each item As Token In fallback
                            Yield item
                        Next

                        Continue For
                    End If
                End If

                If _unkId >= 0 Then
                    If _fuseUnk AndAlso pendingUnk Then
                        Continue For
                    End If

                    pendingUnk = True
                    Yield New Token(_unkId, IdToToken(_unkId), token.Start, token.End)
                End If
            Next
        End Function

        ''' <summary>
        ''' 使用 Viterbi 动态规划求解最优切分路径。
        ''' </summary>
        ''' <returns>
        ''' 切分结果，其中 <see cref="Token.Id"/> 为 -1 的元素表示该片段未被词表覆盖。
        ''' </returns>
        Private Function Viterbi(text As String) As List(Of Token)
            Dim n As Integer = text.Length
            Dim best As Double() = New Double(n) {}
            Dim prev As Integer() = New Integer(n) {}
            Dim ids As Integer() = New Integer(n) {}

            For i As Integer = 1 To n
                best(i) = Double.NegativeInfinity
                prev(i) = -1
                ids(i) = -1
            Next

            ' the penalty score which is used for an unknown single character
            Dim unkScore As Double = _minScore - UnkPenalty

            For i As Integer = 0 To n - 1
                If Double.IsNegativeInfinity(best(i)) Then
                    Continue For
                End If

                ' enumerate every vocabulary entry which starts at the position i
                Dim node As TrieNode = _root
                Dim k As Integer = i

                Do While k < n
                    Dim [next] As TrieNode = Nothing

                    If Not node.Children.TryGetValue(text(k), [next]) Then
                        Exit Do
                    End If

                    node = [next]
                    k += 1

                    If node.Id < 0 Then
                        Continue Do
                    End If

                    Dim score As Double = best(i) + GetScore(node.Id)

                    If score > best(k) Then
                        best(k) = score
                        prev(k) = i
                        ids(k) = node.Id
                    End If
                Loop

                ' always keep a single character fallback so that the dynamic
                ' programming would never be blocked by an unknown character
                Dim step__ As Integer = If(Char.IsHighSurrogate(text(i)) AndAlso i + 1 < n AndAlso Char.IsLowSurrogate(text(i + 1)), 2, 1)
                Dim j As Integer = i + step__

                If best(i) + unkScore > best(j) Then
                    best(j) = best(i) + unkScore
                    prev(j) = i
                    ids(j) = -1
                End If
            Next

            Dim result As New List(Of Token)
            Dim pos As Integer = n

            Do While pos > 0
                Dim from As Integer = prev(pos)

                If from < 0 Then
                    Exit Do
                End If

                result.Add(New Token(ids(pos), text.Substring(from, pos - from), from, pos))
                pos = from
            Loop

            result.Reverse()

            Return result
        End Function

        Private Function GetScore(id As Integer) As Double
            If id >= 0 AndAlso id < _scores.Length Then
                Return _scores(id)
            Else
                Return 0.0
            End If
        End Function

    End Class

End Namespace
