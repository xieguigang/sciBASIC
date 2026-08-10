#Region "Microsoft.VisualBasic::69fee46d650b8c3d7041c2f5755e14af, nlp\NLP\test\HFTokenizerTest.vb"

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

    '   Total Lines: 288
    '    Code Lines: 177 (61.46%)
    ' Comment Lines: 61 (21.18%)
    '    - Xml Docs: 95.08%
    ' 
    '   Blank Lines: 50 (17.36%)
    '     File Size: 12.06 KB


    ' Module HFTokenizerTest
    ' 
    '     Function: AssertIds, AssertTokens, Quote, ResolveModelDirectory, ShowAndVerify
    '               TestUnigram, TestWordPiece
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.ChineseTokenizer.HuggingFace
Imports SysPath = System.IO.Path

''' <summary>
''' <see cref="HuggingFaceTokenizer"/> 的验证入口。
''' </summary>
''' <remarks>
''' 该模块用于核对 vb.net 侧的分词结果是否与 python 端的
''' <c>hugging_face_tokenizer\deepseek_tokenizer.py</c> 完全一致。
''' </remarks>
Module HFTokenizerTest

    ''' <summary>
    ''' 模型目录，相对于代码仓库中 nlp 目录的位置。
    ''' </summary>
    Const ModelDirectory As String = "../../../../../hugging_face_tokenizer"

    ''' <summary>
    ''' python 端 <c>tokenizer.encode("Hello!")</c> 的输出，用于回归比对。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 由于 <c>add_bos_token</c> 与 <c>add_eos_token</c> 均为 <c>false</c>，
    ''' 该输出是不含任何特殊 token 的纯 BPE 编号序列。
    ''' </para>
    ''' <para>
    ''' 期望值直接取自模型词表本身：第三个 <c>Split</c> 预分词器把 <c>"Hello!"</c>
    ''' 切分为 <c>"Hello"</c> 与 <c>"!"</c> 两个分片，二者在 <c>model.vocab</c> 中的
    ''' 编号分别为 19923 与 3。
    ''' </para>
    ''' </remarks>
    ReadOnly ExpectedHello As Integer() = {19923, 3}

    Sub Main(args As String())
        Dim directory As String = ResolveModelDirectory(args)

        Call Console.WriteLine($"loading the tokenizer model from: {directory}")

        Dim sw As Stopwatch = Stopwatch.StartNew()
        Dim tokenizer As HuggingFaceTokenizer = HuggingFaceTokenizer.FromPretrained(directory, verbose:=True)

        Call Console.WriteLine($"the tokenizer model is loaded in {sw.ElapsedMilliseconds} ms.")
        Call Console.WriteLine()

        Dim failed As Integer = 0

        ' the primary alignment check against the python implementation
        failed += AssertIds(tokenizer, "Hello!", ExpectedHello)

        Call Console.WriteLine()
        Call Console.WriteLine("--- the tokenization samples ---")

        For Each text As String In {
            "Hello!",
            "Hello, world!",
            "DeepSeek-V3 is a strong Mixture-of-Experts language model.",
            "你好，世界！",
            "自然语言处理是人工智能的一个重要分支。",
            "混合输入 mixed 123 456789 tokens",
            "  leading and trailing spaces  ",
            "line1" & vbLf & "line2" & vbCrLf & "line3",
            "emoji 😀🎉 test",
            "<｜begin▁of▁sentence｜>hello<｜end▁of▁sentence｜>"
        }
            failed += ShowAndVerify(tokenizer, text)
        Next

        ' the structural verification of the other two subword models
        failed += TestWordPiece()
        failed += TestUnigram()

        Call Console.WriteLine()

        If failed = 0 Then
            Call Console.WriteLine("all of the test cases are passed.")
        Else
            Call Console.WriteLine($"{failed} test case(s) are failed!")
        End If

        Call Console.WriteLine()
        Call Console.WriteLine("--- the vocabulary lookup ---")
        Call Console.WriteLine($"vocab_size    = {tokenizer.VocabSize}")
        Call Console.WriteLine($"model_type    = {tokenizer.ModelType}")
        Call Console.WriteLine($"bos_token     = {tokenizer.BosToken} => {tokenizer.TokenToId(tokenizer.BosToken)}")
        Call Console.WriteLine($"eos_token     = {tokenizer.EosToken} => {tokenizer.TokenToId(tokenizer.EosToken)}")
        Call Console.WriteLine($"add_bos_token = {tokenizer.Config.AddBosToken}")
        Call Console.WriteLine($"add_eos_token = {tokenizer.Config.AddEosToken}")

        Environment.ExitCode = If(failed = 0, 0, 1)
    End Sub

    ''' <summary>
    ''' 校验 WordPiece 模型（BERT 类）。
    ''' </summary>
    ''' <remarks>
    ''' 这里用一份手工构造的小型 tokenizer.json 做结构性验证：贪心最长匹配应当把
    ''' <c>unaffable</c> 切分为 <c>un</c> / <c>##aff</c> / <c>##able</c>，而词表无法
    ''' 覆盖的单词则整词退化为 <c>[UNK]</c>。
    ''' </remarks>
    Private Function TestWordPiece() As Integer
        Const json As String = "{
            ""added_tokens"": [],
            ""normalizer"": { ""type"": ""BertNormalizer"", ""lowercase"": true },
            ""pre_tokenizer"": { ""type"": ""Whitespace"" },
            ""post_processor"": null,
            ""decoder"": { ""type"": ""WordPiece"", ""prefix"": ""##"", ""cleanup"": true },
            ""model"": {
                ""type"": ""WordPiece"",
                ""unk_token"": ""[UNK]"",
                ""continuing_subword_prefix"": ""##"",
                ""max_input_chars_per_word"": 100,
                ""vocab"": {
                    ""[UNK]"": 0, ""un"": 1, ""##aff"": 2, ""##able"": 3,
                    ""hello"": 4, ""world"": 5, ""!"": 6
                }
            }
        }"

        Call Console.WriteLine()
        Call Console.WriteLine("--- the WordPiece model ---")

        Dim tokenizer As HuggingFaceTokenizer = HuggingFaceTokenizer.FromJson(json)
        Dim failed As Integer = 0

        failed += AssertTokens(tokenizer, "unaffable", {"un", "##aff", "##able"})
        failed += AssertTokens(tokenizer, "hello world!", {"hello", "world", "!"})
        failed += AssertTokens(tokenizer, "zzz", {"[UNK]"})

        Return failed
    End Function

    ''' <summary>
    ''' 校验 Unigram / SentencePiece 模型。
    ''' </summary>
    ''' <remarks>
    ''' Viterbi 会在所有可能的切分方案中选取对数概率之和最大的一种：这里
    ''' <c>ab</c> 的得分（-1.0）优于 <c>a</c> + <c>b</c> 的组合（-2.0 + -3.0），
    ''' 因此 <c>abc</c> 的最优切分应当是 <c>▁</c> / <c>ab</c> / <c>c</c>。
    ''' </remarks>
    Private Function TestUnigram() As Integer
        Const json As String = "{
            ""added_tokens"": [],
            ""normalizer"": null,
            ""pre_tokenizer"": { ""type"": ""Metaspace"", ""replacement"": ""\u2581"", ""prepend_scheme"": ""always"", ""split"": false },
            ""post_processor"": null,
            ""decoder"": { ""type"": ""Metaspace"", ""replacement"": ""\u2581"", ""prepend_scheme"": ""always"" },
            ""model"": {
                ""type"": ""Unigram"",
                ""unk_id"": 0,
                ""vocab"": [
                    [""<unk>"", 0.0],
                    [""\u2581"", -1.5],
                    [""ab"", -1.0],
                    [""a"", -2.0],
                    [""b"", -3.0],
                    [""c"", -2.5],
                    [""\u2581ab"", -0.5]
                ]
            }
        }"

        Call Console.WriteLine()
        Call Console.WriteLine("--- the Unigram model ---")

        Dim tokenizer As HuggingFaceTokenizer = HuggingFaceTokenizer.FromJson(json)
        Dim failed As Integer = 0

        failed += AssertTokens(tokenizer, "abc", {"▁ab", "c"})
        failed += AssertTokens(tokenizer, "ab", {"▁ab"})

        Return failed
    End Function

    ''' <summary>
    ''' 比对分词结果与期望的 token 字面值序列。
    ''' </summary>
    ''' <returns>比对失败时返回 1，否则返回 0。</returns>
    Private Function AssertTokens(tokenizer As HuggingFaceTokenizer, text As String, expected As String()) As Integer
        Dim actual As String() = tokenizer.Tokenize(text)
        Dim ok As Boolean = actual.Length = expected.Length

        If ok Then
            For i As Integer = 0 To expected.Length - 1
                If Not String.Equals(actual(i), expected(i), StringComparison.Ordinal) Then
                    ok = False
                    Exit For
                End If
            Next
        End If

        Call Console.WriteLine($"tokenize({Quote(text)}) => [{String.Join(", ", actual.Select(AddressOf Quote))}] {If(ok, "[PASSED]", $"[FAILED] expected [{String.Join(", ", expected.Select(AddressOf Quote))}]")}")

        Return If(ok, 0, 1)
    End Function

    ''' <summary>
    ''' 定位模型目录：优先使用命令行参数，其次按可执行文件的相对位置推断。
    ''' </summary>
    Private Function ResolveModelDirectory(args As String()) As String
        If args IsNot Nothing AndAlso args.Length > 0 AndAlso IO.Directory.Exists(args(0)) Then
            Return args(0)
        End If

        Dim probe As String = SysPath.GetFullPath(SysPath.Combine(AppContext.BaseDirectory, ModelDirectory))

        If IO.Directory.Exists(probe) Then
            Return probe
        End If

        ' walk up from the current working directory to locate the model folder
        Dim current As IO.DirectoryInfo = New IO.DirectoryInfo(AppContext.BaseDirectory)

        Do While current IsNot Nothing
            Dim candidate As String = SysPath.Combine(current.FullName, "hugging_face_tokenizer")

            If IO.Directory.Exists(candidate) Then
                Return candidate
            End If

            current = current.Parent
        Loop

        Throw New IO.DirectoryNotFoundException("unable to locate the 'hugging_face_tokenizer' model directory, please pass it in as the first command line argument.")
    End Function

    ''' <summary>
    ''' 比对编码结果与期望的 id 序列。
    ''' </summary>
    ''' <returns>比对失败时返回 1，否则返回 0。</returns>
    Private Function AssertIds(tokenizer As HuggingFaceTokenizer, text As String, expected As Integer()) As Integer
        Dim actual As Integer() = tokenizer.EncodeToIds(text)

        Call Console.WriteLine($"encode({Quote(text)})")
        Call Console.WriteLine($"  expected = [{String.Join(", ", expected)}]")
        Call Console.WriteLine($"  actual   = [{String.Join(", ", actual)}]")

        If actual.Length <> expected.Length Then
            Call Console.WriteLine($"  [FAILED] the token count is mismatched: {actual.Length} <> {expected.Length}")
            Return 1
        End If

        For i As Integer = 0 To expected.Length - 1
            If actual(i) <> expected(i) Then
                Call Console.WriteLine($"  [FAILED] the token id at the offset {i} is mismatched: {actual(i)} <> {expected(i)}")
                Return 1
            End If
        Next

        Call Console.WriteLine("  [PASSED]")

        Return 0
    End Function

    ''' <summary>
    ''' 打印分词结果并校验编码/解码的可逆性。
    ''' </summary>
    ''' <returns>回环校验失败时返回 1，否则返回 0。</returns>
    Private Function ShowAndVerify(tokenizer As HuggingFaceTokenizer, text As String) As Integer
        Dim encoding As Encoding = tokenizer.Encode(text)
        Dim decoded As String = tokenizer.Decode(encoding.Ids, skipSpecialTokens:=False)

        Call Console.WriteLine()
        Call Console.WriteLine($"text    : {Quote(text)}")
        Call Console.WriteLine($"ids     : [{String.Join(", ", encoding.Ids)}]")
        Call Console.WriteLine($"tokens  : [{String.Join(", ", encoding.Tokens.Select(AddressOf Quote))}]")
        Call Console.WriteLine($"decoded : {Quote(decoded)}")

        If String.Equals(decoded, text, StringComparison.Ordinal) Then
            Call Console.WriteLine("roundtrip: [PASSED]")
            Return 0
        Else
            Call Console.WriteLine("roundtrip: [FAILED] the decoded text is different from the input.")
            Return 1
        End If
    End Function

    ''' <summary>
    ''' 把不可见字符转义之后再输出，避免换行符干扰控制台的排版。
    ''' </summary>
    Private Function Quote(text As String) As String
        If text Is Nothing Then
            Return "<null>"
        End If

        Return """" & text.Replace("\", "\\").Replace(vbCr, "\r").Replace(vbLf, "\n").Replace(vbTab, "\t") & """"
    End Function

End Module
