#Region "Microsoft.VisualBasic::HuggingFace/TokenizerFactory.vb"

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

    '     Module TokenizerFactory
    ' 
    '         Function: CreateNormalizer, CreatePreTokenizer, CreateModel
    '                   CreatePostProcessor, CreateDecoder
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 依据 tokenizer.json 中各配置节点的 <c>type</c> 字段构造流水线组件。
    ''' </summary>
    ''' <remarks>
    ''' 遇到尚未支持的组件类型时会抛出携带具体类型名的异常，便于快速定位缺失的能力，
    ''' 而不是静默地退化为空实现导致分词结果出现难以排查的偏差。
    ''' </remarks>
    Public Module TokenizerFactory

        ''' <summary>
        ''' 构造归一化器。
        ''' </summary>
        Public Function CreateNormalizer(node As JsonNode) As INormalizer
            If node Is Nothing Then
                Return NullNormalizer.Instance
            End If

            Dim type As String = node("type")?.AsString

            Select Case type
                Case "Sequence"
                    Dim items As New List(Of INormalizer)
                    Dim list As JsonNode = node("normalizers")

                    If list IsNot Nothing AndAlso list.Type = JsonNodeType.Array Then
                        For Each item As JsonNode In list.Items
                            items.Add(CreateNormalizer(item))
                        Next
                    End If

                    ' an empty sequence is equivalent to a no-op normalizer
                    If items.Count = 0 Then
                        Return NullNormalizer.Instance
                    Else
                        Return New SequenceNormalizer(items)
                    End If

                Case "NFC" : Return New UnicodeNormalizer(NormalizationForm.FormC)
                Case "NFD" : Return New UnicodeNormalizer(NormalizationForm.FormD)
                Case "NFKC" : Return New UnicodeNormalizer(NormalizationForm.FormKC)
                Case "NFKD" : Return New UnicodeNormalizer(NormalizationForm.FormKD)
                Case "Lowercase" : Return New LowercaseNormalizer
                Case "StripAccents" : Return New StripAccentsNormalizer
                Case "Nmt" : Return New NmtNormalizer
                Case "Precompiled" : Return New PrecompiledNormalizer

                Case "Strip"
                    Return New StripNormalizer(
                        If(node("strip_left")?.AsBoolean(True), True),
                        If(node("strip_right")?.AsBoolean(True), True)
                    )

                Case "Replace"
                    Dim isRegex As Boolean
                    Dim pattern As String = ReadPattern(node("pattern"), isRegex)

                    Return New ReplaceNormalizer(pattern, isRegex, node("content")?.AsString)

                Case "Prepend"
                    Return New PrependNormalizer(node("prepend")?.AsString)

                Case "BertNormalizer"
                    Return New BertNormalizer(
                        If(node("clean_text")?.AsBoolean(True), True),
                        If(node("handle_chinese_chars")?.AsBoolean(True), True),
                        If(node("strip_accents")?.AsBoolean(True), True),
                        If(node("lowercase")?.AsBoolean(True), True)
                    )

                Case Else
                    Throw New NotSupportedException($"the normalizer type '{type}' is not supported yet!")
            End Select
        End Function

        ''' <summary>
        ''' 构造预分词器。
        ''' </summary>
        Public Function CreatePreTokenizer(node As JsonNode) As IPreTokenizer
            If node Is Nothing Then
                Return Nothing
            End If

            Dim type As String = node("type")?.AsString

            Select Case type
                Case "Sequence"
                    Dim items As New List(Of IPreTokenizer)
                    Dim list As JsonNode = node("pretokenizers")

                    If list IsNot Nothing AndAlso list.Type = JsonNodeType.Array Then
                        For Each item As JsonNode In list.Items
                            items.Add(CreatePreTokenizer(item))
                        Next
                    End If

                    Return New SequencePreTokenizer(items)

                Case "Split"
                    Dim isRegex As Boolean
                    Dim pattern As String = ReadPattern(node("pattern"), isRegex)

                    Return New SplitPreTokenizer(
                        pattern, isRegex,
                        ParseBehavior(node("behavior")?.AsString),
                        If(node("invert")?.AsBoolean(False), False)
                    )

                Case "ByteLevel"
                    Return New ByteLevelPreTokenizer(
                        If(node("add_prefix_space")?.AsBoolean(True), True),
                        If(node("use_regex")?.AsBoolean(True), True)
                    )

                Case "Metaspace"
                    Return New MetaspacePreTokenizer(
                        node("replacement")?.AsString,
                        ReadPrependScheme(node),
                        If(node("split")?.AsBoolean(True), True)
                    )

                Case "Whitespace" : Return New WhitespacePreTokenizer
                Case "WhitespaceSplit" : Return New WhitespaceSplitPreTokenizer

                Case "Punctuation"
                    Return New PunctuationPreTokenizer(ParseBehavior(node("behavior")?.AsString))

                Case "Digits"
                    Return New DigitsPreTokenizer(If(node("individual_digits")?.AsBoolean(False), False))

                Case "FixedLength"
                    Return New FixedLengthPreTokenizer(If(node("length")?.AsInteger(5), 5))

                Case Else
                    Throw New NotSupportedException($"the pre_tokenizer type '{type}' is not supported yet!")
            End Select
        End Function

        ''' <summary>
        ''' 构造子词模型。
        ''' </summary>
        Public Function CreateModel(model As ModelSection) As ITokenizerModel
            Select Case model.Type
                Case "BPE" : Return New BpeModel(model)
                Case "WordPiece" : Return New WordPieceModel(model)
                Case "Unigram" : Return New UnigramModel(model)
                Case Else
                    Throw New NotSupportedException($"the tokenizer model type '{model.Type}' is not supported yet, only the 'BPE', 'WordPiece' and 'Unigram' models are available.")
            End Select
        End Function

        ''' <summary>
        ''' 构造后处理器。
        ''' </summary>
        Public Function CreatePostProcessor(node As JsonNode, vocab As Func(Of String, Integer?)) As IPostProcessor
            If node Is Nothing Then
                Return NullPostProcessor.Instance
            End If

            Dim type As String = node("type")?.AsString

            Select Case type
                Case "ByteLevel"
                    Return New ByteLevelPostProcessor

                Case "Sequence"
                    Dim items As New List(Of IPostProcessor)
                    Dim list As JsonNode = node("processors")

                    If list IsNot Nothing AndAlso list.Type = JsonNodeType.Array Then
                        For Each item As JsonNode In list.Items
                            items.Add(CreatePostProcessor(item, vocab))
                        Next
                    End If

                    Return New SequencePostProcessor(items)

                Case "BertProcessing", "RobertaProcessing"
                    Dim sep = ReadTokenPair(node("sep"), vocab)
                    Dim cls = ReadTokenPair(node("cls"), vocab)

                    If type = "BertProcessing" Then
                        Return New BertPostProcessor(cls.Token, cls.Id, sep.Token, sep.Id)
                    Else
                        Return New RobertaPostProcessor(cls.Token, cls.Id, sep.Token, sep.Id)
                    End If

                Case "TemplateProcessing"
                    Return New TemplatePostProcessor(ReadTemplate(node("single"), node("special_tokens"), vocab))

                Case Else
                    Throw New NotSupportedException($"the post_processor type '{type}' is not supported yet!")
            End Select
        End Function

        ''' <summary>
        ''' 构造解码器。
        ''' </summary>
        Public Function CreateDecoder(node As JsonNode) As IDecoder
            If node Is Nothing Then
                Return NullDecoder.Instance
            End If

            Dim type As String = node("type")?.AsString

            Select Case type
                Case "ByteLevel" : Return New ByteLevelDecoder
                Case "ByteFallback" : Return New ByteFallbackDecoder
                Case "Fuse" : Return New FuseDecoder

                Case "WordPiece"
                    Return New WordPieceDecoder(
                        If(node("prefix")?.AsString, "##"),
                        If(node("cleanup")?.AsBoolean(True), True)
                    )

                Case "Metaspace"
                    Return New MetaspaceDecoder(node("replacement")?.AsString, ReadPrependScheme(node))

                Case "Replace"
                    Dim isRegex As Boolean
                    Dim pattern As String = ReadPattern(node("pattern"), isRegex)

                    Return New ReplaceDecoder(pattern, isRegex, node("content")?.AsString)

                Case "Strip"
                    Dim content As String = node("content")?.AsString

                    Return New StripDecoder(
                        If(String.IsNullOrEmpty(content), " "c, content(0)),
                        If(node("start")?.AsInteger(0), 0),
                        If(node("stop")?.AsInteger(0), 0)
                    )

                Case "Sequence"
                    Dim items As New List(Of IDecoder)
                    Dim list As JsonNode = node("decoders")

                    If list IsNot Nothing AndAlso list.Type = JsonNodeType.Array Then
                        For Each item As JsonNode In list.Items
                            items.Add(CreateDecoder(item))
                        Next
                    End If

                    Return New SequenceDecoder(items)

                Case Else
                    Throw New NotSupportedException($"the decoder type '{type}' is not supported yet!")
            End Select
        End Function

        ''' <summary>
        ''' 读取 <c>pattern</c> 节点，它形如 <c>{ "Regex": "..." }</c> 或 <c>{ "String": "..." }</c>。
        ''' </summary>
        Private Function ReadPattern(node As JsonNode, ByRef isRegex As Boolean) As String
            isRegex = False

            If node Is Nothing Then
                Return String.Empty
            ElseIf node.Type = JsonNodeType.String Then
                Return node.StringValue
            End If

            Dim regex As JsonNode = node("Regex")

            If regex IsNot Nothing Then
                isRegex = True
                Return regex.AsString
            End If

            Return If(node("String")?.AsString, String.Empty)
        End Function

        ''' <summary>
        ''' 读取 Metaspace 的 <c>prepend_scheme</c>，兼容旧版的 <c>add_prefix_space</c> 布尔字段。
        ''' </summary>
        Private Function ReadPrependScheme(node As JsonNode) As String
            Dim scheme As String = node("prepend_scheme")?.AsString

            If Not String.IsNullOrEmpty(scheme) Then
                Return scheme
            End If

            Dim addPrefixSpace As JsonNode = node("add_prefix_space")

            If addPrefixSpace IsNot Nothing Then
                Return If(addPrefixSpace.AsBoolean(True), "always", "never")
            End If

            Return "always"
        End Function

        Private Function ParseBehavior(behavior As String) As SplitBehavior
            Select Case If(behavior, "Isolated")
                Case "Removed" : Return SplitBehavior.Removed
                Case "MergedWithPrevious" : Return SplitBehavior.MergedWithPrevious
                Case "MergedWithNext" : Return SplitBehavior.MergedWithNext
                Case "Contiguous" : Return SplitBehavior.Contiguous
                Case Else : Return SplitBehavior.Isolated
            End Select
        End Function

        ''' <summary>
        ''' 读取 <c>["token", id]</c> 形式的 token 描述。
        ''' </summary>
        Private Function ReadTokenPair(node As JsonNode, vocab As Func(Of String, Integer?)) As (Token As String, Id As Integer)
            If node Is Nothing Then
                Return (Nothing, -1)
            End If

            If node.Type = JsonNodeType.Array AndAlso node.Items.Count >= 2 Then
                Return (node.Items(0).AsString, node.Items(1).AsInteger(-1))
            End If

            Dim token As String = node.AsString
            Dim id As Integer? = If(vocab Is Nothing, Nothing, vocab(token))

            Return (token, If(id.HasValue, id.Value, -1))
        End Function

        ''' <summary>
        ''' 读取 <c>TemplateProcessing</c> 的单序列模板。
        ''' </summary>
        Private Function ReadTemplate(single__ As JsonNode,
                                      specialTokens As JsonNode,
                                      vocab As Func(Of String, Integer?)) As List(Of TemplatePostProcessor.TemplatePiece)

            Dim pieces As New List(Of TemplatePostProcessor.TemplatePiece)

            If single__ Is Nothing OrElse single__.Type <> JsonNodeType.Array Then
                Return pieces
            End If

            For Each item As JsonNode In single__.Items
                If item Is Nothing OrElse item.Type <> JsonNodeType.Object Then
                    Continue For
                End If

                If item("Sequence") IsNot Nothing Then
                    pieces.Add(New TemplatePostProcessor.TemplatePiece With {.IsSequence = True})
                    Continue For
                End If

                Dim special As JsonNode = item("SpecialToken")

                If special Is Nothing Then
                    Continue For
                End If

                Dim id As String = special("id")?.AsString
                Dim resolved As Integer = ResolveSpecialId(id, specialTokens, vocab)

                pieces.Add(New TemplatePostProcessor.TemplatePiece With {
                    .IsSequence = False,
                    .Value = id,
                    .Id = resolved
                })
            Next

            Return pieces
        End Function

        ''' <summary>
        ''' 依据 <c>special_tokens</c> 表把模板中的特殊 token 名称解析为编号。
        ''' </summary>
        Private Function ResolveSpecialId(id As String, specialTokens As JsonNode, vocab As Func(Of String, Integer?)) As Integer
            If String.IsNullOrEmpty(id) Then
                Return -1
            End If

            If specialTokens IsNot Nothing AndAlso specialTokens.Type = JsonNodeType.Object Then
                Dim entry As JsonNode = specialTokens(id)
                Dim ids As JsonNode = entry?("ids")

                If ids IsNot Nothing AndAlso ids.Type = JsonNodeType.Array AndAlso ids.Items.Count > 0 Then
                    Return ids.Items(0).AsInteger(-1)
                End If
            End If

            Dim fromVocab As Integer? = If(vocab Is Nothing, Nothing, vocab(id))

            Return If(fromVocab.HasValue, fromVocab.Value, -1)
        End Function

    End Module

End Namespace
