#Region "Microsoft.VisualBasic::877809b6d4f73434367331665cf23e75, nlp\NLP\Tokenizer\src\HuggingFace\Abstractions.vb"

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

    '   Total Lines: 216
    '    Code Lines: 67 (31.02%)
    ' Comment Lines: 115 (53.24%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 34 (15.74%)
    '     File Size: 7.70 KB


    '     Structure Token
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Split
    ' 
    '         Properties: [End], IsSpecial, SpecialId, Start, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Encoding
    ' 
    '         Properties: AttentionMask, Count, Ids, Offsets, Tokens
    '                     TypeIds
    ' 
    '         Function: ToString
    ' 
    '     Interface INormalizer
    ' 
    '         Function: Normalize
    ' 
    '     Interface IPreTokenizer
    ' 
    '         Function: PreTokenize
    ' 
    '     Interface ITokenizerModel
    ' 
    '         Properties: VocabSize
    ' 
    '         Function: IdToToken, Tokenize, TokenToId
    ' 
    '     Interface IPostProcessor
    ' 
    '         Function: Process
    ' 
    '     Interface IDecoder
    ' 
    '         Function: Decode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 单个 token 的切分结果。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Start"/> 与 <see cref="[End]"/> 描述该 token 在 <b>预分词分片</b> 
    ''' 内部的字符偏移量（半开区间 <c>[Start, End)</c>），在流水线的后续阶段会被
    ''' 重新映射到原始输入文本上。
    ''' </remarks>
    Public Structure Token

        ''' <summary>
        ''' 词表编号。当该 token 无法在词表中找到并且模型未配置 unk 时为 -1。
        ''' </summary>
        Public Id As Integer
        ''' <summary>
        ''' token 的字面值（对于 ByteLevel 模型而言是字节映射之后的可见字符串）。
        ''' </summary>
        Public Value As String
        ''' <summary>
        ''' 起始字符偏移量（包含）。
        ''' </summary>
        Public Start As Integer
        ''' <summary>
        ''' 结束字符偏移量（不包含）。
        ''' </summary>
        Public [End] As Integer

        Public Sub New(id As Integer, value As String, start As Integer, [end] As Integer)
            Me.Id = id
            Me.Value = value
            Me.Start = start
            Me.End = [end]
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{Id}] {Value}"
        End Function

    End Structure

    ''' <summary>
    ''' 预分词之后产生的文本分片。
    ''' </summary>
    ''' <remarks>
    ''' 该类型同时携带分片文本与其在原始输入串上的偏移量，使得预分词器可以被串联
    ''' 组合（<c>Sequence</c>）而不会丢失位置信息。<see cref="IsSpecial"/> 用于标记
    ''' 由 <c>added_tokens</c> 命中的分片，这类分片不会再进入子词模型。
    ''' </remarks>
    Public Class Split

        ''' <summary>
        ''' 分片的文本内容。
        ''' </summary>
        Public Property Value As String
        ''' <summary>
        ''' 在原始输入文本中的起始偏移量（包含）。
        ''' </summary>
        Public Property Start As Integer
        ''' <summary>
        ''' 在原始输入文本中的结束偏移量（不包含）。
        ''' </summary>
        Public Property [End] As Integer
        ''' <summary>
        ''' 该分片是否为 added_token / special token，为真时跳过子词模型。
        ''' </summary>
        Public Property IsSpecial As Boolean
        ''' <summary>
        ''' 当 <see cref="IsSpecial"/> 为真时对应的词表编号。
        ''' </summary>
        Public Property SpecialId As Integer

        Public Sub New(value As String, start As Integer, [end] As Integer)
            Me.Value = value
            Me.Start = start
            Me.End = [end]
            Me.IsSpecial = False
            Me.SpecialId = -1
        End Sub

        Public Overrides Function ToString() As String
            Return If(IsSpecial, $"<special:{SpecialId}>{Value}", Value)
        End Function

    End Class

    ''' <summary>
    ''' 编码结果，等价于 HuggingFace 中的 <c>Encoding</c> 对象。
    ''' </summary>
    Public Class Encoding

        ''' <summary>
        ''' token 编号序列，等价于 python 端 <c>tokenizer.encode(text)</c> 的返回值。
        ''' </summary>
        Public Property Ids As Integer()
        ''' <summary>
        ''' 与 <see cref="Ids"/> 一一对应的 token 字面值序列。
        ''' </summary>
        Public Property Tokens As String()
        ''' <summary>
        ''' 句子编号，单句输入时全部为 0。
        ''' </summary>
        Public Property TypeIds As Integer()
        ''' <summary>
        ''' 注意力掩码，非 padding 位置为 1。
        ''' </summary>
        Public Property AttentionMask As Integer()
        ''' <summary>
        ''' 每一个 token 在原始输入文本中的字符偏移量区间。
        ''' </summary>
        Public Property Offsets As (Start As Integer, [End] As Integer)()

        ''' <summary>
        ''' token 的数量。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return If(Ids Is Nothing, 0, Ids.Length)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{String.Join(", ", If(Ids, New Integer() {}))}]"
        End Function

    End Class

    ''' <summary>
    ''' 文本归一化器契约，对应 tokenizer.json 中的 <c>normalizer</c> 段。
    ''' </summary>
    ''' <remarks>
    ''' 实现必须是无副作用的只读操作，从而允许分词器实例在多线程之间安全共享。
    ''' </remarks>
    Public Interface INormalizer

        ''' <summary>
        ''' 对输入文本执行归一化变换。
        ''' </summary>
        Function Normalize(text As String) As String

    End Interface

    ''' <summary>
    ''' 预分词器契约，对应 tokenizer.json 中的 <c>pre_tokenizer</c> 段。
    ''' </summary>
    ''' <remarks>
    ''' 预分词器接受上一级产出的分片集合并继续细分，因此 <c>Sequence</c> 类型的
    ''' 预分词器可以通过逐级 map-flatten 的方式简单地组合起来。实现必须是无副作用
    ''' 的只读操作。
    ''' </remarks>
    Public Interface IPreTokenizer

        ''' <summary>
        ''' 对上一级产出的每一个分片继续切分。
        ''' </summary>
        Function PreTokenize(splits As List(Of Split)) As List(Of Split)

    End Interface

    ''' <summary>
    ''' 子词模型契约：BPE / WordPiece / Unigram 均实现该接口。
    ''' </summary>
    ''' <remarks>
    ''' 实现必须是无副作用的只读操作（内部缓存需自行保证线程安全）。
    ''' </remarks>
    Public Interface ITokenizerModel

        ''' <summary>
        ''' 对一个预分词分片执行子词切分。
        ''' </summary>
        Function Tokenize(fragment As String) As IEnumerable(Of Token)

        ''' <summary>
        ''' 查询 token 对应的词表编号，不存在时返回 <see langword="Nothing"/>。
        ''' </summary>
        Function TokenToId(token As String) As Integer?

        ''' <summary>
        ''' 查询词表编号对应的 token，不存在时返回 <see langword="Nothing"/>。
        ''' </summary>
        Function IdToToken(id As Integer) As String

        ''' <summary>
        ''' 词表规模（不包含 added_tokens 中超出词表范围的追加词）。
        ''' </summary>
        ReadOnly Property VocabSize As Integer

    End Interface

    ''' <summary>
    ''' 后处理器契约，对应 tokenizer.json 中的 <c>post_processor</c> 段。
    ''' </summary>
    Public Interface IPostProcessor

        ''' <summary>
        ''' 对模型产出的 token 序列执行后处理（例如添加 CLS/SEP 等特殊 token）。
        ''' </summary>
        ''' <param name="addSpecialTokens">是否允许添加特殊 token。</param>
        Function Process(tokens As List(Of Token), addSpecialTokens As Boolean) As List(Of Token)

    End Interface

    ''' <summary>
    ''' 解码器契约，对应 tokenizer.json 中的 <c>decoder</c> 段。
    ''' </summary>
    Public Interface IDecoder

        ''' <summary>
        ''' 将 token 字面值序列还原为文本。
        ''' </summary>
        Function Decode(tokens As IEnumerable(Of String)) As String

    End Interface

End Namespace
