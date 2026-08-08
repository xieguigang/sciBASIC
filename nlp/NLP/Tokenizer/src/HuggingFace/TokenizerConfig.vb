#Region "Microsoft.VisualBasic::HuggingFace/TokenizerConfig.vb"

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

    '     Class TokenizerConfig
    ' 
    '         Function: ParseFile, [Default]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' the sciBASIC framework declares its own 'File' symbol in the global
' namespace, so an explicit alias is required here to reference the BCL type.
Imports SysFile = System.IO.File

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' tokenizer_config.json 的强类型描述。
    ''' </summary>
    ''' <remarks>
    ''' 该文件描述的是 <c>transformers</c> 层面的配置（而非 <c>tokenizers</c> 层面的
    ''' 分词流水线），其中最关键的是 <see cref="AddBosToken"/> 与
    ''' <see cref="AddEosToken"/>，它们决定了 <c>encode</c> 的默认行为是否会自动
    ''' 附加句首/句尾特殊 token。
    ''' </remarks>
    Public Class TokenizerConfig

        ''' <summary>
        ''' 编码时是否自动在序列开头添加 <see cref="BosToken"/>。
        ''' </summary>
        Public Property AddBosToken As Boolean
        ''' <summary>
        ''' 编码时是否自动在序列结尾添加 <see cref="EosToken"/>。
        ''' </summary>
        Public Property AddEosToken As Boolean
        ''' <summary>
        ''' 句首特殊 token 的字面值。
        ''' </summary>
        Public Property BosToken As String
        ''' <summary>
        ''' 句尾特殊 token 的字面值。
        ''' </summary>
        Public Property EosToken As String
        ''' <summary>
        ''' 填充 token 的字面值。
        ''' </summary>
        Public Property PadToken As String
        ''' <summary>
        ''' 未知词 token 的字面值。
        ''' </summary>
        Public Property UnkToken As String
        ''' <summary>
        ''' 模型可接受的最大序列长度。
        ''' </summary>
        Public Property ModelMaxLength As Integer = Integer.MaxValue
        ''' <summary>
        ''' python 端所使用的分词器类名，例如 <c>LlamaTokenizerFast</c>。
        ''' </summary>
        Public Property TokenizerClass As String
        ''' <summary>
        ''' 解码时是否清理由分词引入的多余空格。
        ''' </summary>
        Public Property CleanUpTokenizationSpaces As Boolean

        ''' <summary>
        ''' 当模型目录中不存在 tokenizer_config.json 时所使用的缺省配置。
        ''' </summary>
        Public Shared Function [Default]() As TokenizerConfig
            Return New TokenizerConfig
        End Function

        ''' <summary>
        ''' 解析一个 tokenizer_config.json 文件，文件不存在时返回缺省配置。
        ''' </summary>
        Public Shared Function ParseFile(file As String) As TokenizerConfig
            If Not SysFile.Exists(file) Then
                Return [Default]()
            End If

            Return FromJson(JsonReader.ParseFile(file))
        End Function

        ''' <summary>
        ''' 从已经解析好的 json 节点构建。
        ''' </summary>
        Public Shared Function FromJson(root As JsonNode) As TokenizerConfig
            If root Is Nothing OrElse root.Type <> JsonNodeType.Object Then
                Return [Default]()
            End If

            ' the model_max_length could be a very large float literal like 1e30 in
            ' some of the huggingface models, clamp it into the integer range here.
            Dim maxLength As Double = If(root("model_max_length")?.AsDouble(Integer.MaxValue), CDbl(Integer.MaxValue))

            Return New TokenizerConfig With {
                .AddBosToken = If(root("add_bos_token")?.AsBoolean(False), False),
                .AddEosToken = If(root("add_eos_token")?.AsBoolean(False), False),
                .BosToken = TokenizerJson.ParseTokenLiteral(root("bos_token")),
                .EosToken = TokenizerJson.ParseTokenLiteral(root("eos_token")),
                .PadToken = TokenizerJson.ParseTokenLiteral(root("pad_token")),
                .UnkToken = TokenizerJson.ParseTokenLiteral(root("unk_token")),
                .ModelMaxLength = If(maxLength >= Integer.MaxValue, Integer.MaxValue, CInt(maxLength)),
                .TokenizerClass = root("tokenizer_class")?.AsString,
                .CleanUpTokenizationSpaces = If(root("clean_up_tokenization_spaces")?.AsBoolean(False), False)
            }
        End Function

    End Class

End Namespace
