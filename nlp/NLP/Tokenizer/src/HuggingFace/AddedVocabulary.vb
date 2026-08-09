#Region "Microsoft.VisualBasic::a0f3f375a542d7f0400b981fcdfad2a9, nlp\NLP\Tokenizer\src\HuggingFace\AddedVocabulary.vb"

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

    '   Total Lines: 219
    '    Code Lines: 135 (61.64%)
    ' Comment Lines: 43 (19.63%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 41 (18.72%)
    '     File Size: 7.91 KB


    '     Class AddedVocabulary
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ExtractSpecial, IdToToken, IsSpecial, IsWordBoundary, IsWordChar
    '                   TokenToId
    '         Class TrieNode
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 追加词表：在归一化与预分词之前优先命中的 token 集合。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>added_tokens</c> 中的词（例如 <c>&lt;｜begin▁of▁sentence｜&gt;</c> 这类特殊
    ''' 标记）必须作为一个整体被识别出来，既不能被归一化器改写，也不能被预分词器拆散。
    ''' 因此这里在流水线的最前端先对输入串做一次整串扫描，命中的片段直接产出其对应的
    ''' 编号，剩余的片段才会继续走后续的常规流程。
    ''' </para>
    ''' <para>
    ''' 这份模型的追加词多达十余万条，逐词调用 <c>IndexOf</c> 轮询显然不可接受，
    ''' 所以此处构建了一棵字典树（Trie）用于最左最长匹配，整体扫描复杂度为
    ''' O(n × maxTokenLength)，且与追加词的数量无关。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class AddedVocabulary

        ''' <summary>
        ''' 字典树节点。
        ''' </summary>
        Private NotInheritable Class TrieNode
            Public ReadOnly Children As New Dictionary(Of Char, TrieNode)
            ''' <summary>命中的追加词，为 <see langword="Nothing"/> 时表示该节点不是终结点。</summary>
            Public Value As AddedTokenInfo
        End Class

        Private ReadOnly _root As New TrieNode
        Private ReadOnly _tokenToId As New Dictionary(Of String, Integer)
        Private ReadOnly _idToToken As New Dictionary(Of Integer, AddedTokenInfo)
        Private ReadOnly _count As Integer

        ''' <summary>
        ''' 追加词的数量。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _count
            End Get
        End Property

        Public Sub New(tokens As IEnumerable(Of AddedTokenInfo))
            If tokens Is Nothing Then
                Return
            End If

            For Each token As AddedTokenInfo In tokens
                If token Is Nothing OrElse String.IsNullOrEmpty(token.Content) Then
                    Continue For
                End If

                Dim node As TrieNode = _root

                For Each c As Char In token.Content
                    Dim [next] As TrieNode = Nothing

                    If Not node.Children.TryGetValue(c, [next]) Then
                        [next] = New TrieNode
                        node.Children.Add(c, [next])
                    End If

                    node = [next]
                Next

                node.Value = token
                _tokenToId(token.Content) = token.Id
                _idToToken(token.Id) = token
                _count += 1
            Next
        End Sub

        ''' <summary>
        ''' 查询追加词对应的编号。
        ''' </summary>
        Public Function TokenToId(token As String) As Integer?
            Dim id As Integer

            If token IsNot Nothing AndAlso _tokenToId.TryGetValue(token, id) Then
                Return id
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 查询编号对应的追加词。
        ''' </summary>
        Public Function IdToToken(id As Integer) As String
            Dim token As AddedTokenInfo = Nothing

            If _idToToken.TryGetValue(id, token) Then
                Return token.Content
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 判断某个编号是否属于特殊 token。
        ''' </summary>
        Public Function IsSpecial(id As Integer) As Boolean
            Dim token As AddedTokenInfo = Nothing

            Return _idToToken.TryGetValue(id, token) AndAlso token.Special
        End Function

        ''' <summary>
        ''' 把输入文本切分为"追加词分片"与"普通文本分片"交替出现的序列。
        ''' </summary>
        ''' <remarks>
        ''' 采用最左最长匹配：在每一个起始位置上沿字典树尽可能地向前推进，取所能匹配到
        ''' 的最长追加词。<c>single_word</c> 要求命中片段的左右两侧均不是单词字符，
        ''' <c>lstrip</c> / <c>rstrip</c> 则会额外吞掉命中片段两侧的空白字符。
        ''' </remarks>
        Public Function ExtractSpecial(text As String) As List(Of Split)
            Dim result As New List(Of Split)

            If String.IsNullOrEmpty(text) Then
                Return result
            End If

            If _count = 0 Then
                result.Add(New Split(text, 0, text.Length))
                Return result
            End If

            Dim plainStart As Integer = 0
            Dim i As Integer = 0

            Do While i < text.Length
                Dim node As TrieNode = _root
                Dim matched As AddedTokenInfo = Nothing
                Dim matchedEnd As Integer = -1
                Dim k As Integer = i

                Do While k < text.Length
                    Dim [next] As TrieNode = Nothing

                    If Not node.Children.TryGetValue(text(k), [next]) Then
                        Exit Do
                    End If

                    node = [next]
                    k += 1

                    If node.Value IsNot Nothing Then
                        matched = node.Value
                        matchedEnd = k
                    End If
                Loop

                If matched Is Nothing Then
                    i += 1
                    Continue Do
                End If

                Dim start As Integer = i
                Dim [end] As Integer = matchedEnd

                If matched.SingleWord AndAlso Not IsWordBoundary(text, start, [end]) Then
                    i += 1
                    Continue Do
                End If

                If matched.LStrip Then
                    Do While start > plainStart AndAlso Char.IsWhiteSpace(text(start - 1))
                        start -= 1
                    Loop
                End If

                If matched.RStrip Then
                    Do While [end] < text.Length AndAlso Char.IsWhiteSpace(text([end]))
                        [end] += 1
                    Loop
                End If

                If start > plainStart Then
                    result.Add(New Split(text.Substring(plainStart, start - plainStart), plainStart, start))
                End If

                result.Add(New Split(text.Substring(i, matchedEnd - i), start, [end]) With {
                    .IsSpecial = True,
                    .SpecialId = matched.Id
                })

                plainStart = [end]
                i = [end]
            Loop

            If plainStart < text.Length Then
                result.Add(New Split(text.Substring(plainStart), plainStart, text.Length))
            End If

            Return result
        End Function

        ''' <summary>
        ''' 判断命中片段的左右两侧是否均为非单词字符。
        ''' </summary>
        Private Shared Function IsWordBoundary(text As String, start As Integer, [end] As Integer) As Boolean
            If start > 0 AndAlso IsWordChar(text(start - 1)) Then
                Return False
            End If
            If [end] < text.Length AndAlso IsWordChar(text([end])) Then
                Return False
            End If

            Return True
        End Function

        Private Shared Function IsWordChar(c As Char) As Boolean
            Return Char.IsLetterOrDigit(c) OrElse c = "_"c
        End Function

    End Class

End Namespace
