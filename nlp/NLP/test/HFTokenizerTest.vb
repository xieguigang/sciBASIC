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
    ''' 该期望值由 <c>deepseek_tokenizer.py</c> 实际运行得到。
    ''' </remarks>
    ReadOnly ExpectedHello As Integer() = {19923, 0}

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
