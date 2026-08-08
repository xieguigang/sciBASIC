#Region "Microsoft.VisualBasic::HuggingFace/TokenizerJson.vb"

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

    '     Class AddedTokenInfo
    ' 
    '     Class ModelSection
    ' 
    '     Class TokenizerJson
    ' 
    '         Function: ParseFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' tokenizer.json 中 <c>added_tokens</c> 数组的元素。
    ''' </summary>
    Public Class AddedTokenInfo

        ''' <summary>
        ''' 该追加词的词表编号，允许超出 <c>model.vocab</c> 的范围。
        ''' </summary>
        Public Property Id As Integer
        ''' <summary>
        ''' 追加词的字面值。
        ''' </summary>
        Public Property Content As String
        ''' <summary>
        ''' 为真时仅当该词是一个独立的单词（左右均为非单词字符）才会被匹配。
        ''' </summary>
        Public Property SingleWord As Boolean
        ''' <summary>
        ''' 为真时会一并吞掉该词左侧的空白字符。
        ''' </summary>
        Public Property LStrip As Boolean
        ''' <summary>
        ''' 为真时会一并吞掉该词右侧的空白字符。
        ''' </summary>
        Public Property RStrip As Boolean
        ''' <summary>
        ''' 为真时该词会先经过归一化器处理再参与匹配。
        ''' </summary>
        Public Property Normalized As Boolean
        ''' <summary>
        ''' 该词是否为特殊 token（解码时可以通过参数跳过）。
        ''' </summary>
        Public Property Special As Boolean

        Public Overrides Function ToString() As String
            Return $"[{Id}] {Content}"
        End Function

    End Class

    ''' <summary>
    ''' tokenizer.json 中 <c>model</c> 段的强类型描述。
    ''' </summary>
    Public Class ModelSection

        ''' <summary>
        ''' 子词模型类型：<c>BPE</c> / <c>WordPiece</c> / <c>Unigram</c>。
        ''' </summary>
        Public Property Type As String
        ''' <summary>
        ''' 词表：token 字面值到词表编号的映射。
        ''' </summary>
        Public Property Vocab As Dictionary(Of String, Integer)
        ''' <summary>
        ''' 词表编号到 token 字面值的反查表，按最大编号预分配。
        ''' </summary>
        Public Property IdToToken As String()
        ''' <summary>
        ''' BPE 的合并规则，元素形如 <c>("Ġ", "t")</c>，下标即为合并优先级。
        ''' </summary>
        Public Property Merges As List(Of (Left As String, Right As String))
        ''' <summary>
        ''' Unigram 模型的对数概率表，与 <see cref="Vocab"/> 的编号一一对应。
        ''' </summary>
        Public Property UnigramScores As Double()
        ''' <summary>
        ''' 未知词的字面值，未配置时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property UnkToken As String
        ''' <summary>
        ''' Unigram 模型中未知词的编号。
        ''' </summary>
        Public Property UnkId As Integer = -1
        ''' <summary>
        ''' WordPiece / BPE 中续接子词的前缀，WordPiece 默认为 <c>##</c>。
        ''' </summary>
        Public Property ContinuingSubwordPrefix As String
        ''' <summary>
        ''' BPE 中词尾子词的后缀，例如 <c>&lt;/w&gt;</c>。
        ''' </summary>
        Public Property EndOfWordSuffix As String
        ''' <summary>
        ''' 是否将连续的未知词合并为单个未知词。
        ''' </summary>
        Public Property FuseUnk As Boolean
        ''' <summary>
        ''' 是否启用字节回退（未登录字符退化为 <c>&lt;0xXX&gt;</c> 形式的 token）。
        ''' </summary>
        Public Property ByteFallback As Boolean
        ''' <summary>
        ''' 为真时词表中已存在的完整词不再执行 BPE 合并。
        ''' </summary>
        Public Property IgnoreMerges As Boolean
        ''' <summary>
        ''' WordPiece 中单词的最大字符数，超过则整词判定为未知词。
        ''' </summary>
        Public Property MaxInputCharsPerWord As Integer = 100

    End Class

    ''' <summary>
    ''' tokenizer.json 的强类型描述。
    ''' </summary>
    ''' <remarks>
    ''' <c>normalizer</c> / <c>pre_tokenizer</c> / <c>post_processor</c> / <c>decoder</c>
    ''' 这四个段落是依据 <c>type</c> 字段区分的多态结构，因此这里保留其原始的
    ''' <see cref="JsonNode"/> 形态，交由 <see cref="TokenizerFactory"/> 按需构造。
    ''' </remarks>
    Public Class TokenizerJson

        ''' <summary>
        ''' 追加词表，包含 BOS/EOS/PAD 等特殊 token。
        ''' </summary>
        Public Property AddedTokens As List(Of AddedTokenInfo)
        ''' <summary>
        ''' 归一化器配置节点，未配置时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property Normalizer As JsonNode
        ''' <summary>
        ''' 预分词器配置节点，未配置时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property PreTokenizer As JsonNode
        ''' <summary>
        ''' 后处理器配置节点，未配置时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property PostProcessor As JsonNode
        ''' <summary>
        ''' 解码器配置节点，未配置时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property Decoder As JsonNode
        ''' <summary>
        ''' 子词模型段。
        ''' </summary>
        Public Property Model As ModelSection

        ''' <summary>
        ''' 解析一个 tokenizer.json 文件。
        ''' </summary>
        ''' <param name="file">tokenizer.json 的文件路径。</param>
        ''' <exception cref="System.IO.FileNotFoundException">文件不存在时抛出。</exception>
        ''' <exception cref="FormatException">缺少必要的配置段时抛出。</exception>
        Public Shared Function ParseFile(file As String) As TokenizerJson
            Return FromJson(JsonReader.ParseFile(file))
        End Function

        ''' <summary>
        ''' 从已经解析好的 json 节点构建。
        ''' </summary>
        Public Shared Function FromJson(root As JsonNode) As TokenizerJson
            If root Is Nothing OrElse root.Type <> JsonNodeType.Object Then
                Throw New FormatException("the root node of a tokenizer.json file should be a json object!")
            End If

            Dim modelNode As JsonNode = root("model")

            If modelNode Is Nothing Then
                Throw New FormatException("the required 'model' section is missing in the given tokenizer.json file!")
            End If

            Return New TokenizerJson With {
                .AddedTokens = ParseAddedTokens(root("added_tokens")),
                .Normalizer = root("normalizer"),
                .PreTokenizer = root("pre_tokenizer"),
                .PostProcessor = root("post_processor"),
                .Decoder = root("decoder"),
                .Model = ParseModel(modelNode)
            }
        End Function

        Private Shared Function ParseAddedTokens(node As JsonNode) As List(Of AddedTokenInfo)
            Dim list As New List(Of AddedTokenInfo)

            If node Is Nothing OrElse node.Type <> JsonNodeType.Array Then
                Return list
            End If

            For Each item As JsonNode In node.Items
                If item Is Nothing OrElse item.Type <> JsonNodeType.Object Then
                    Continue For
                End If

                Dim content As JsonNode = item("content")

                If content Is Nothing Then
                    Continue For
                End If

                list.Add(New AddedTokenInfo With {
                    .Id = If(item("id")?.AsInteger(-1), -1),
                    .Content = content.AsString,
                    .SingleWord = If(item("single_word")?.AsBoolean(False), False),
                    .LStrip = If(item("lstrip")?.AsBoolean(False), False),
                    .RStrip = If(item("rstrip")?.AsBoolean(False), False),
                    .Normalized = If(item("normalized")?.AsBoolean(False), False),
                    .Special = If(item("special")?.AsBoolean(False), False)
                })
            Next

            Return list
        End Function

        Private Shared Function ParseModel(node As JsonNode) As ModelSection
            Dim model As New ModelSection With {
                .Type = If(node("type")?.AsString, "BPE"),
                .UnkToken = ParseTokenLiteral(node("unk_token")),
                .ContinuingSubwordPrefix = node("continuing_subword_prefix")?.AsString,
                .EndOfWordSuffix = node("end_of_word_suffix")?.AsString,
                .FuseUnk = If(node("fuse_unk")?.AsBoolean(False), False),
                .ByteFallback = If(node("byte_fallback")?.AsBoolean(False), False),
                .IgnoreMerges = If(node("ignore_merges")?.AsBoolean(False), False),
                .MaxInputCharsPerWord = If(node("max_input_chars_per_word")?.AsInteger(100), 100),
                .UnkId = If(node("unk_id")?.AsInteger(-1), -1)
            }

            Call ParseVocab(node("vocab"), model)
            Call ParseMerges(node("merges"), model)

            Return model
        End Function

        ''' <summary>
        ''' 解析词表。
        ''' </summary>
        ''' <remarks>
        ''' BPE 与 WordPiece 的词表是 <c>{ "token": id }</c> 形式的对象；
        ''' Unigram 的词表则是 <c>[ ["token", log_prob], ... ]</c> 形式的数组，
        ''' 其数组下标即为词表编号。这里对两种形态统一处理。
        ''' </remarks>
        Private Shared Sub ParseVocab(node As JsonNode, model As ModelSection)
            If node Is Nothing Then
                Throw New FormatException("the required 'model.vocab' section is missing in the given tokenizer.json file!")
            End If

            If node.Type = JsonNodeType.Array Then
                ' unigram: [[token, log_prob], ...]
                Dim n As Integer = node.Items.Count
                Dim vocab As New Dictionary(Of String, Integer)(n)
                Dim tokens As String() = New String(n - 1) {}
                Dim scores As Double() = New Double(n - 1) {}

                For i As Integer = 0 To n - 1
                    Dim entry As JsonNode = node.Items(i)

                    If entry Is Nothing OrElse entry.Type <> JsonNodeType.Array OrElse entry.Items.Count < 1 Then
                        Continue For
                    End If

                    Dim token As String = entry.Items(0).AsString

                    tokens(i) = token
                    scores(i) = If(entry.Items.Count > 1, entry.Items(1).AsDouble(0.0), 0.0)
                    ' the first occurrence wins so that the smallest id is kept
                    If Not vocab.ContainsKey(token) Then
                        vocab.Add(token, i)
                    End If
                Next

                model.Vocab = vocab
                model.IdToToken = tokens
                model.UnigramScores = scores
            ElseIf node.Type = JsonNodeType.Object Then
                ' bpe / wordpiece: { "token": id }
                Dim members As Dictionary(Of String, JsonNode) = node.Members
                Dim vocab As New Dictionary(Of String, Integer)(members.Count)
                Dim maxId As Integer = -1

                For Each member As KeyValuePair(Of String, JsonNode) In members
                    Dim id As Integer = member.Value.AsInteger(-1)

                    If id < 0 Then
                        Continue For
                    End If

                    vocab(member.Key) = id

                    If id > maxId Then
                        maxId = id
                    End If
                Next

                Dim tokens As String() = New String(maxId) {}

                For Each entry As KeyValuePair(Of String, Integer) In vocab
                    tokens(entry.Value) = entry.Key
                Next

                model.Vocab = vocab
                model.IdToToken = tokens
            Else
                Throw New FormatException("the 'model.vocab' section should be a json object or a json array!")
            End If
        End Sub

        ''' <summary>
        ''' 解析 BPE 合并规则。
        ''' </summary>
        ''' <remarks>
        ''' 兼容两种格式：旧版的 <c>"A B"</c> 字符串（按<b>第一个</b>空格切分，因为
        ''' 合并对两侧的 token 自身不会含有空格字符）与新版的 <c>["A", "B"]</c> 数组。
        ''' </remarks>
        Private Shared Sub ParseMerges(node As JsonNode, model As ModelSection)
            Dim merges As New List(Of (Left As String, Right As String))

            If node Is Nothing OrElse node.Type <> JsonNodeType.Array Then
                model.Merges = merges
                Return
            End If

            merges.Capacity = node.Items.Count

            For Each item As JsonNode In node.Items
                If item Is Nothing Then
                    Continue For
                End If

                If item.Type = JsonNodeType.String Then
                    Dim text As String = item.StringValue
                    Dim space As Integer = text.IndexOf(" "c)

                    If space > 0 Then
                        merges.Add((text.Substring(0, space), text.Substring(space + 1)))
                    End If
                ElseIf item.Type = JsonNodeType.Array AndAlso item.Items.Count >= 2 Then
                    merges.Add((item.Items(0).AsString, item.Items(1).AsString))
                End If
            Next

            model.Merges = merges
        End Sub

        ''' <summary>
        ''' 读取一个 token 字面值，兼容纯字符串与 <c>AddedToken</c> 对象两种写法。
        ''' </summary>
        Friend Shared Function ParseTokenLiteral(node As JsonNode) As String
            If node Is Nothing Then
                Return Nothing
            ElseIf node.Type = JsonNodeType.String Then
                Return node.StringValue
            ElseIf node.Type = JsonNodeType.Object Then
                Return node("content")?.AsString
            Else
                Return Nothing
            End If
        End Function

    End Class

End Namespace
