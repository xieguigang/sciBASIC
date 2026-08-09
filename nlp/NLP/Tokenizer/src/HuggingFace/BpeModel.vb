#Region "Microsoft.VisualBasic::e168c8f1deb5acfb4aaf2d411529fa56, nlp\NLP\Tokenizer\src\HuggingFace\BpeModel.vb"

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

    '   Total Lines: 268
    '    Code Lines: 169 (63.06%)
    ' Comment Lines: 48 (17.91%)
    '    - Xml Docs: 79.17%
    ' 
    '   Blank Lines: 51 (19.03%)
    '     File Size: 10.59 KB


    '     Class BpeModel
    ' 
    '         Properties: VocabSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ApplyAffixes, GetMerged, IdToToken, MergeWord, Tokenize
    '                   TokenToId
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' the sciBASIC framework declares its own 'Encoding' symbol in the global
' namespace, so an explicit alias is required here to reference the BCL type.
Imports TextEncoding = System.Text.Encoding

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 字节对编码（Byte Pair Encoding）子词模型。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' BPE 的切分过程为：先把输入分片拆解为最小的符号单元（字符），随后不断地在相邻
    ''' 符号对中挑选<b>合并优先级最高</b>（即在 <c>merges</c> 列表中出现得最早）的一对
    ''' 予以合并，直到没有任何相邻符号对存在于合并规则表中为止。
    ''' </para>
    ''' <para>
    ''' 单个分片长度为 L 时该过程的复杂度约为 O(L²)，考虑到预分词之后的分片通常都很短
    ''' （一般不超过 20 个字符），实际开销可以忽略。此外这里还对分片的切分结果做了
    ''' 有界缓存，重复出现的词可以直接命中缓存，显著提升长文本的吞吐量。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class BpeModel : Implements ITokenizerModel

        ''' <summary>
        ''' 分片切分结果缓存的容量上限，超出之后整体清空。
        ''' </summary>
        Private Const CacheCapacity As Integer = 65536

        Private ReadOnly _vocab As Dictionary(Of String, Integer)
        Private ReadOnly _idToToken As String()
        ''' <summary>
        ''' 合并规则表：键为 <c>"左符号 右符号"</c>，值为合并优先级（越小越优先）。
        ''' </summary>
        Private ReadOnly _ranks As Dictionary(Of String, Integer)
        Private ReadOnly _cache As Dictionary(Of String, String())
        Private ReadOnly _cacheLock As New Object

        Private ReadOnly _unkToken As String
        Private ReadOnly _unkId As Integer = -1
        Private ReadOnly _fuseUnk As Boolean
        Private ReadOnly _byteFallback As Boolean
        Private ReadOnly _ignoreMerges As Boolean
        Private ReadOnly _continuingSubwordPrefix As String
        Private ReadOnly _endOfWordSuffix As String

        ''' <summary>
        ''' 词表规模。
        ''' </summary>
        Public ReadOnly Property VocabSize As Integer Implements ITokenizerModel.VocabSize
            Get
                Return _vocab.Count
            End Get
        End Property

        Public Sub New(model As ModelSection)
            _vocab = model.Vocab
            _idToToken = model.IdToToken
            _unkToken = model.UnkToken
            _fuseUnk = model.FuseUnk
            _byteFallback = model.ByteFallback
            _ignoreMerges = model.IgnoreMerges
            _continuingSubwordPrefix = model.ContinuingSubwordPrefix
            _endOfWordSuffix = model.EndOfWordSuffix
            _cache = New Dictionary(Of String, String())

            If Not _unkToken Is Nothing Then
                Dim id As Integer

                If _vocab.TryGetValue(_unkToken, id) Then
                    _unkId = id
                End If
            End If

            Dim merges As List(Of (Left As String, Right As String)) = model.Merges

            _ranks = New Dictionary(Of String, Integer)(If(merges Is Nothing, 0, merges.Count))

            If Not merges Is Nothing Then
                For i As Integer = 0 To merges.Count - 1
                    Dim key As String = merges(i).Left & " " & merges(i).Right

                    ' the first occurrence always wins since it owns a higher priority
                    If Not _ranks.ContainsKey(key) Then
                        _ranks.Add(key, i)
                    End If
                Next
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

            Dim id As Integer

            ' when the ignore_merges option is enabled, a fragment which already
            ' exists in the vocabulary should never be splitted any more
            If _ignoreMerges AndAlso _vocab.TryGetValue(fragment, id) Then
                Yield New Token(id, fragment, 0, fragment.Length)
                Return
            End If

            Dim symbols As String() = GetMerged(fragment)
            Dim offset As Integer = 0
            Dim pendingUnk As Boolean = False

            For Each symbol As String In symbols
                ' the offsets are measured on the fragment before the affixes are
                ' attached, so advance the cursor with the raw symbol length
                Dim start As Integer = offset
                offset += symbol.Length

                Dim piece As String = ApplyAffixes(symbol, start = 0, offset >= fragment.Length)

                If _vocab.TryGetValue(piece, id) Then
                    pendingUnk = False
                    Yield New Token(id, piece, start, offset)
                    Continue For
                End If

                If _byteFallback Then
                    Dim resolved As Boolean = True
                    Dim fallback As New List(Of Token)

                    For Each b As Byte In TextEncoding.UTF8.GetBytes(symbol)
                        Dim name As String = "<0x" & b.ToString("X2") & ">"

                        If _vocab.TryGetValue(name, id) Then
                            fallback.Add(New Token(id, name, start, offset))
                        Else
                            resolved = False
                            Exit For
                        End If
                    Next

                    If resolved Then
                        pendingUnk = False

                        For Each token As Token In fallback
                            Yield token
                        Next

                        Continue For
                    End If
                End If

                If _unkId >= 0 Then
                    ' fuse_unk: the consecutive unknown tokens are merged into one
                    If _fuseUnk AndAlso pendingUnk Then
                        Continue For
                    End If

                    pendingUnk = True
                    Yield New Token(_unkId, _unkToken, start, offset)
                End If
            Next
        End Function

        ''' <summary>
        ''' 为子词附加 <c>continuing_subword_prefix</c> 与 <c>end_of_word_suffix</c>。
        ''' </summary>
        Private Function ApplyAffixes(symbol As String, isFirst As Boolean, isLast As Boolean) As String
            If Not isFirst AndAlso Not String.IsNullOrEmpty(_continuingSubwordPrefix) Then
                symbol = _continuingSubwordPrefix & symbol
            End If
            If isLast AndAlso Not String.IsNullOrEmpty(_endOfWordSuffix) Then
                symbol = symbol & _endOfWordSuffix
            End If

            Return symbol
        End Function

        ''' <summary>
        ''' 读取分片的合并结果，优先命中缓存。
        ''' </summary>
        Private Function GetMerged(fragment As String) As String()
            Dim cached As String() = Nothing

            SyncLock _cacheLock
                If _cache.TryGetValue(fragment, cached) Then
                    Return cached
                End If
            End SyncLock

            cached = MergeWord(fragment)

            SyncLock _cacheLock
                ' a plain bounded cache: the tokenizer instance is designed to be
                ' shared globally, so the memory footprint has to stay predictable
                If _cache.Count >= CacheCapacity Then
                    Call _cache.Clear()
                End If

                _cache(fragment) = cached
            End SyncLock

            Return cached
        End Function

        ''' <summary>
        ''' 对单个分片执行字节对合并。
        ''' </summary>
        ''' <remarks>
        ''' 每一轮扫描全部相邻符号对并选出优先级最高的一对进行合并，直到再也找不到
        ''' 可用的合并规则为止。这里按<b>代理对</b>（surrogate pair）而非 UTF-16 码元
        ''' 进行初始拆分，从而保证 emoji 等增补平面字符不会被拆散。
        ''' </remarks>
        Private Function MergeWord(fragment As String) As String()
            Dim symbols As New List(Of String)(fragment.Length)
            Dim i As Integer = 0

            Do While i < fragment.Length
                Dim n As Integer = If(Char.IsHighSurrogate(fragment(i)) AndAlso i + 1 < fragment.Length AndAlso Char.IsLowSurrogate(fragment(i + 1)), 2, 1)

                symbols.Add(fragment.Substring(i, n))
                i += n
            Loop

            If symbols.Count < 2 OrElse _ranks.Count = 0 Then
                Return symbols.ToArray()
            End If

            Do
                Dim bestRank As Integer = Integer.MaxValue
                Dim bestIndex As Integer = -1

                For k As Integer = 0 To symbols.Count - 2
                    Dim rank As Integer

                    If _ranks.TryGetValue(symbols(k) & " " & symbols(k + 1), rank) AndAlso rank < bestRank Then
                        bestRank = rank
                        bestIndex = k
                    End If
                Next

                If bestIndex < 0 Then
                    Exit Do
                End If

                symbols(bestIndex) = symbols(bestIndex) & symbols(bestIndex + 1)
                symbols.RemoveAt(bestIndex + 1)
            Loop While symbols.Count > 1

            Return symbols.ToArray()
        End Function

    End Class

End Namespace
