#Region "Microsoft.VisualBasic::HuggingFace/HuggingFaceTokenizer.vb"

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

    '     Class HuggingFaceTokenizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FromPretrained, FromFile, Encode, EncodeToIds, Tokenize
    '                   Decode, TokenToId, IdToToken
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' the sciBASIC framework declares its own 'File' symbol in the global
' namespace, so the explicit aliases are required here.
Imports SysDirectory = System.IO.Directory
Imports SysPath = System.IO.Path

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' 与 Hugging Face <c>transformers</c> / <c>tokenizers</c> 兼容的子词分词器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 该类型完整复刻了 huggingface 的五段式分词流水线：
    ''' </para>
    ''' <code>
    ''' 输入文本
    '''   → AddedVocabulary  追加词优先命中，不进入子词模型
    '''   → Normalizer       文本归一化
    '''   → PreTokenizer     预分词
    '''   → Model            BPE / WordPiece / Unigram 子词切分
    '''   → PostProcessor    后处理，按需添加特殊 token
    '''   → Encoding         Ids + Tokens
    ''' </code>
    ''' <para>
    ''' 用法与 python 端的 <c>AutoTokenizer.from_pretrained(dir)</c> 保持一致：
    ''' </para>
    ''' <code>
    ''' Dim tokenizer = HuggingFaceTokenizer.FromPretrained("./hugging_face_tokenizer")
    ''' Dim ids = tokenizer.EncodeToIds("Hello!")
    ''' Dim text = tokenizer.Decode(ids)
    ''' </code>
    ''' <para>
    ''' 加载完成之后的实例是<b>只读且线程安全</b>的，应当在全局范围内复用，避免为
    ''' 每一次分词都重复解析体积可观的模型文件。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class HuggingFaceTokenizer

        Private ReadOnly _addedVocab As AddedVocabulary
        Private ReadOnly _normalizer As INormalizer
        Private ReadOnly _preTokenizer As IPreTokenizer
        Private ReadOnly _model As ITokenizerModel
        Private ReadOnly _postProcessor As IPostProcessor
        Private ReadOnly _decoder As IDecoder
        Private ReadOnly _config As TokenizerConfig
        Private ReadOnly _modelType As String

        ''' <summary>
        ''' 子词模型的类型：<c>BPE</c> / <c>WordPiece</c> / <c>Unigram</c>。
        ''' </summary>
        Public ReadOnly Property ModelType As String
            Get
                Return _modelType
            End Get
        End Property

        ''' <summary>
        ''' 词表规模（不含追加词）。
        ''' </summary>
        Public ReadOnly Property VocabSize As Integer
            Get
                Return _model.VocabSize
            End Get
        End Property

        ''' <summary>
        ''' tokenizer_config.json 中的配置。
        ''' </summary>
        Public ReadOnly Property Config As TokenizerConfig
            Get
                Return _config
            End Get
        End Property

        ''' <summary>
        ''' 句首特殊 token。
        ''' </summary>
        Public ReadOnly Property BosToken As String
            Get
                Return _config.BosToken
            End Get
        End Property

        ''' <summary>
        ''' 句尾特殊 token。
        ''' </summary>
        Public ReadOnly Property EosToken As String
            Get
                Return _config.EosToken
            End Get
        End Property

        ''' <summary>
        ''' 填充特殊 token。
        ''' </summary>
        Public ReadOnly Property PadToken As String
            Get
                Return _config.PadToken
            End Get
        End Property

        Private Sub New(json As TokenizerJson, config As TokenizerConfig)
            _config = config
            _modelType = json.Model.Type
            _addedVocab = New AddedVocabulary(json.AddedTokens)
            _normalizer = TokenizerFactory.CreateNormalizer(json.Normalizer)
            _preTokenizer = TokenizerFactory.CreatePreTokenizer(json.PreTokenizer)
            _model = TokenizerFactory.CreateModel(json.Model)
            _postProcessor = TokenizerFactory.CreatePostProcessor(json.PostProcessor, AddressOf _model.TokenToId)
            _decoder = TokenizerFactory.CreateDecoder(json.Decoder)
        End Sub

        ''' <summary>
        ''' 从一个模型目录中加载分词器，等价于 python 端的
        ''' <c>AutoTokenizer.from_pretrained(dir)</c>。
        ''' </summary>
        ''' <param name="directory">
        ''' 包含 <c>tokenizer.json</c>（必需）与 <c>tokenizer_config.json</c>（可选）
        ''' 的模型目录。
        ''' </param>
        ''' <param name="verbose">为真时在加载完成之后打印模型的摘要信息。</param>
        ''' <exception cref="IO.DirectoryNotFoundException">目录不存在时抛出。</exception>
        ''' <exception cref="IO.FileNotFoundException">tokenizer.json 不存在时抛出。</exception>
        Public Shared Function FromPretrained(directory As String, Optional verbose As Boolean = False) As HuggingFaceTokenizer
            If Not SysDirectory.Exists(directory) Then
                Throw New IO.DirectoryNotFoundException($"the given tokenizer model directory is not found: {directory}")
            End If

            Return FromFile(
                SysPath.Combine(directory, "tokenizer.json"),
                SysPath.Combine(directory, "tokenizer_config.json"),
                verbose
            )
        End Function

        ''' <summary>
        ''' 从指定的模型文件中加载分词器。
        ''' </summary>
        ''' <param name="modelFile">tokenizer.json 的文件路径。</param>
        ''' <param name="configFile">
        ''' tokenizer_config.json 的文件路径，文件不存在时使用缺省配置。
        ''' </param>
        ''' <param name="verbose">为真时在加载完成之后打印模型的摘要信息。</param>
        Public Shared Function FromFile(modelFile As String,
                                        Optional configFile As String = Nothing,
                                        Optional verbose As Boolean = False) As HuggingFaceTokenizer

            Dim json As TokenizerJson = TokenizerJson.ParseFile(modelFile)
            Dim config As TokenizerConfig = If(
                String.IsNullOrEmpty(configFile),
                TokenizerConfig.Default(),
                TokenizerConfig.ParseFile(configFile)
            )
            Dim tokenizer As New HuggingFaceTokenizer(json, config)

            If verbose Then
                Call Console.WriteLine($"[tokenizer] model={json.Model.Type}, vocab_size={tokenizer.VocabSize}, merges={If(json.Model.Merges Is Nothing, 0, json.Model.Merges.Count)}, added_tokens={tokenizer._addedVocab.Count}")
            End If

            Return tokenizer
        End Function

        ''' <summary>
        ''' 对文本进行编码。
        ''' </summary>
        ''' <param name="text">待编码的文本。</param>
        ''' <param name="addSpecialTokens">
        ''' 是否添加特殊 token。默认为 <see langword="Nothing"/>，此时依据
        ''' tokenizer_config.json 中的 <c>add_bos_token</c> / <c>add_eos_token</c>
        ''' 决定，从而与 python 端 <c>tokenizer.encode(text)</c> 的默认语义保持一致。
        ''' </param>
        Public Function Encode(text As String, Optional addSpecialTokens As Boolean? = Nothing) As Encoding
            Dim withSpecial As Boolean = If(addSpecialTokens.HasValue,
                addSpecialTokens.Value,
                _config.AddBosToken OrElse _config.AddEosToken)

            Dim tokens As List(Of Token) = EncodeCore(text)

            tokens = _postProcessor.Process(tokens, withSpecial)
            tokens = ApplyConfigSpecialTokens(tokens, addSpecialTokens)

            Dim n As Integer = tokens.Count
            Dim ids As Integer() = New Integer(n - 1) {}
            Dim values As String() = New String(n - 1) {}
            Dim typeIds As Integer() = New Integer(n - 1) {}
            Dim mask As Integer() = New Integer(n - 1) {}
            Dim offsets As (Start As Integer, [End] As Integer)() = New(Start As Integer, [End] As Integer)(n - 1) {}

            For i As Integer = 0 To n - 1
                ids(i) = tokens(i).Id
                values(i) = tokens(i).Value
                typeIds(i) = 0
                mask(i) = 1
                offsets(i) = (tokens(i).Start, tokens(i).End)
            Next

            Return New Encoding With {
                .Ids = ids,
                .Tokens = values,
                .TypeIds = typeIds,
                .AttentionMask = mask,
                .Offsets = offsets
            }
        End Function

        ''' <summary>
        ''' 对文本进行编码并只返回 token 编号序列。
        ''' </summary>
        ''' <remarks>
        ''' 这是与 python 端 <c>tokenizer.encode(text)</c> 直接对应的方法。
        ''' </remarks>
        Public Function EncodeToIds(text As String, Optional addSpecialTokens As Boolean? = Nothing) As Integer()
            Return Encode(text, addSpecialTokens).Ids
        End Function

        ''' <summary>
        ''' 对文本进行分词并只返回 token 的字面值序列。
        ''' </summary>
        ''' <remarks>
        ''' 这是与 python 端 <c>tokenizer.tokenize(text)</c> 直接对应的方法，
        ''' 默认不会添加任何特殊 token。
        ''' </remarks>
        Public Function Tokenize(text As String) As String()
            Return Encode(text, addSpecialTokens:=False).Tokens
        End Function

        ''' <summary>
        ''' 把 token 编号序列解码还原为文本。
        ''' </summary>
        ''' <param name="ids">token 编号序列。</param>
        ''' <param name="skipSpecialTokens">为真时跳过特殊 token。</param>
        Public Function Decode(ids As IEnumerable(Of Integer), Optional skipSpecialTokens As Boolean = True) As String
            Dim tokens As New List(Of String)

            For Each id As Integer In ids
                If skipSpecialTokens AndAlso _addedVocab.IsSpecial(id) Then
                    Continue For
                End If

                Dim token As String = IdToToken(id)

                If token IsNot Nothing Then
                    tokens.Add(token)
                End If
            Next

            Dim text As String = _decoder.Decode(tokens)

            If _config.CleanUpTokenizationSpaces Then
                text = WordPieceDecoder.CleanUp(text)
            End If

            Return text
        End Function

        ''' <summary>
        ''' 查询 token 对应的编号，追加词表优先。
        ''' </summary>
        Public Function TokenToId(token As String) As Integer?
            Dim id As Integer? = _addedVocab.TokenToId(token)

            Return If(id.HasValue, id, _model.TokenToId(token))
        End Function

        ''' <summary>
        ''' 查询编号对应的 token，追加词表优先。
        ''' </summary>
        Public Function IdToToken(id As Integer) As String
            Dim token As String = _addedVocab.IdToToken(id)

            Return If(token, _model.IdToToken(id))
        End Function

        ''' <summary>
        ''' 执行"追加词切分 → 归一化 → 预分词 → 子词模型"这一核心流程。
        ''' </summary>
        Private Function EncodeCore(text As String) As List(Of Token)
            Dim tokens As New List(Of Token)

            If String.IsNullOrEmpty(text) Then
                Return tokens
            End If

            For Each part As Split In _addedVocab.ExtractSpecial(text)
                If part.IsSpecial Then
                    tokens.Add(New Token(part.SpecialId, part.Value, part.Start, part.End))
                    Continue For
                End If

                Dim normalized As String = _normalizer.Normalize(part.Value)

                If String.IsNullOrEmpty(normalized) Then
                    Continue For
                End If

                Dim splits As New List(Of Split) From {
                    New Split(normalized, part.Start, part.End)
                }

                If _preTokenizer IsNot Nothing Then
                    splits = _preTokenizer.PreTokenize(splits)
                End If

                For Each fragment As Split In splits
                    If fragment.IsSpecial Then
                        tokens.Add(New Token(fragment.SpecialId, fragment.Value, fragment.Start, fragment.End))
                        Continue For
                    End If

                    For Each token As Token In _model.Tokenize(fragment.Value)
                        ' remap the fragment local offsets back onto the input text
                        tokens.Add(New Token(token.Id, token.Value, fragment.Start, fragment.End))
                    Next
                Next
            Next

            Return tokens
        End Function

        ''' <summary>
        ''' 依据 tokenizer_config.json 的配置在序列两端补上 BOS / EOS。
        ''' </summary>
        ''' <remarks>
        ''' 仅当后处理器本身没有添加特殊 token 时才会生效，从而避免重复添加。
        ''' 对于 DeepSeek 的模型，<c>add_bos_token</c> 与 <c>add_eos_token</c> 均为
        ''' <c>false</c>，因此这里不会做任何改动。
        ''' </remarks>
        Private Function ApplyConfigSpecialTokens(tokens As List(Of Token), addSpecialTokens As Boolean?) As List(Of Token)
            ' an explicit False disables the special tokens completely
            If addSpecialTokens.HasValue AndAlso Not addSpecialTokens.Value Then
                Return tokens
            End If
            If TypeOf _postProcessor IsNot ByteLevelPostProcessor AndAlso TypeOf _postProcessor IsNot NullPostProcessor Then
                ' the post processor has already taken care of the special tokens
                Return tokens
            End If

            If _config.AddBosToken AndAlso Not String.IsNullOrEmpty(_config.BosToken) Then
                Dim id As Integer? = TokenToId(_config.BosToken)

                If id.HasValue Then
                    tokens.Insert(0, New Token(id.Value, _config.BosToken, 0, 0))
                End If
            End If

            If _config.AddEosToken AndAlso Not String.IsNullOrEmpty(_config.EosToken) Then
                Dim id As Integer? = TokenToId(_config.EosToken)

                If id.HasValue Then
                    tokens.Add(New Token(id.Value, _config.EosToken, 0, 0))
                End If
            End If

            Return tokens
        End Function

    End Class

End Namespace
