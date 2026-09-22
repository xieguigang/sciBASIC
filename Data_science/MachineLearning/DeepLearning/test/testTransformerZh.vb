#Region "Microsoft.VisualBasic::db2df0262b06164ac9de32c34f9c16f5, Data_science\MachineLearning\DeepLearning\test\testTransformerZh.vb"

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

    '   Total Lines: 327
    '    Code Lines: 214 (65.44%)
    ' Comment Lines: 36 (11.01%)
    '    - Xml Docs: 44.44%
    ' 
    '   Blank Lines: 77 (23.55%)
    '     File Size: 13.95 KB


    ' Module testTransformerZh
    ' 
    '     Function: BuildChineseTokenizer, ChineseSentence, CloneSentences, CountDistinctTokens, LocateCorpus
    '               TokenizeChinese, TokenizeEnglish, TranslateWithModel
    ' 
    '     Sub: Evaluate, LoadCorpus, run
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' testTransformerZh —— 基于 Transformer 的英译中翻译 demo
'
' 流程：
'   1. 读取内置英中平行语料 Transformer/TrainingData/english-chinese.txt（制表符分隔）；
'   2. 英文按空白切词（保留 ? / ! 作为独立 token），中文用 NLP 包提供的
'      ChineseTokenizer（词典最大匹配 + HMM 混合分词）切词，并用「两遍法」把
'      第一遍出现频次较高的多字词补进词典以稳定切分；
'   3. 构建 TransformerModel 并训练，逐 step 打印 loss；
'   4. 对若干英文测试句输出模型译文与参考译文对照。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.Transformer
Imports nlp = Microsoft.VisualBasic.Data.NLP.ChineseTokenizer

''' <summary>
''' 通过 Transformer 模型进行英译中翻译的 demo 测试。
''' </summary>
Module testTransformerZh

    ''' <summary>相对源码树的语料路径（用于从可执行目录逐级向上定位）。</summary>
    Private ReadOnly corpusCandidates() As String = {
        "Data_science\MachineLearning\DeepLearning\Transformer\TrainingData\english-chinese.txt",
        "MachineLearning\DeepLearning\Transformer\TrainingData\english-chinese.txt",
        "DeepLearning\Transformer\TrainingData\english-chinese.txt"
    }

    Public Sub run(Optional corpusPath As String = Nothing)
        Console.WriteLine()
        Console.WriteLine("============================================================")
        Console.WriteLine(" Transformer 英译中翻译 demo")
        Console.WriteLine("============================================================")

        If String.IsNullOrEmpty(corpusPath) Then corpusPath = LocateCorpus()

        If corpusPath Is Nothing Then
            Console.WriteLine("[transformer-zh] 未找到英中平行语料 english-chinese.txt，demo 终止。")
            Console.WriteLine("[transformer-zh] 请通过参数显式指定语料文件路径。")
            Return
        End If

        Console.WriteLine($"[transformer-zh] 语料文件：{corpusPath}")

        Dim englishRaw As New List(Of String)()
        Dim chineseRaw As New List(Of String)()

        Call LoadCorpus(corpusPath, englishRaw, chineseRaw)

        If englishRaw.Count = 0 Then
            Console.WriteLine("[transformer-zh] 语料为空，demo 终止。")
            Return
        End If

        Console.WriteLine($"[transformer-zh] 平行句对数：{englishRaw.Count}")

        ' ---- 中文分词器：两遍法稳定词典 ----
        Dim tokenizer = BuildChineseTokenizer(chineseRaw)

        Dim allEnglish As New List(Of List(Of String))()
        Dim allChinese As New List(Of List(Of String))()

        For i As Integer = 0 To englishRaw.Count - 1
            allEnglish.Add(TokenizeEnglish(englishRaw(i)))
            allChinese.Add(TokenizeChinese(tokenizer, chineseRaw(i)))
        Next

        Dim maxEnglishTokens As Integer = allEnglish.Max(Function(x) x.Count)
        Dim maxChineseTokens As Integer = allChinese.Max(Function(x) x.Count) + 2   ' 加上 < 与 > 起止符

        Console.WriteLine($"[transformer-zh] 英文词表：{CountDistinctTokens(allEnglish)}，中文词表：{CountDistinctTokens(allChinese)}")
        Console.WriteLine($"[transformer-zh] 最长标题序列：英文 {maxEnglishTokens} token，中文 {maxChineseTokens} token，sequenceLength = {Math.Max(maxEnglishTokens, maxChineseTokens)}")

        ' ---- 模型超参 ----
        ' 这是一个「玩具规模」的 demo：语料很小、模型很小，用若干轮训练让模型记住
        ' 训练集里的句对，从而验证迁移后的前向 / 反向传播链路是完整且可收敛的。
        ' 固定随机种子，保证每次运行的初始化与训练结果完全一致。
        TensorOps.Seed = 20260919

        Dim batchSize As Integer = 10
        Dim embeddingSize As Integer = 16
        Dim dk As Integer = 8
        Dim dv As Integer = 8
        Dim h As Integer = 2
        Dim dff As Integer = 32
        Dim Nx As Integer = 2
        Dim dropoutRate As Double = 0.0

        Dim nrEpochs As Integer = 150
        Dim nrTrainingSteps As Integer = 8
        Dim learningRate As Double = 0.02

        ' Train() 内部按 batchSize 整除取 batch，这里先把训练集裁剪到 batchSize 的整数倍
        Dim nrSentences As Integer = Math.Min(allEnglish.Count, 20)
        nrSentences -= nrSentences Mod batchSize

        Dim trainEnglish = CloneSentences(allEnglish, nrSentences)
        Dim trainChinese = CloneSentences(allChinese, nrSentences)

        Console.WriteLine($"[transformer-zh] 训练句对数：{nrSentences}，batchSize：{batchSize}，epochs：{nrEpochs}，steps/epoch：{nrTrainingSteps}")

        Dim model As New TransformerModel(Nx, embeddingSize, dk, dv, h, dff, batchSize, dropoutRate, trainEnglish, trainChinese)

        Call model.Train(nrEpochs, nrTrainingSteps, learningRate, batchSize, trainEnglish, trainChinese)

        ' ---- 测试：对若干训练句做译文输出 ----
        Call Evaluate(model, tokenizer, allEnglish, allChinese, nrSentences)

        Console.WriteLine()
        Console.WriteLine("[transformer-zh] demo 结束。")
    End Sub

#Region "语料加载与分词"

    ''' <summary>从测试工程的输出目录逐级向上定位语料文件。</summary>
    Private Function LocateCorpus() As String
        Dim roots As New List(Of String)()

        Dim dir = New DirectoryInfo(AppContext.BaseDirectory)
        For i As Integer = 0 To 8
            If dir Is Nothing Then Exit For
            roots.Add(dir.FullName)
            dir = dir.Parent
        Next

        roots.Add(Environment.CurrentDirectory)

        For Each root In roots
            For Each relative In corpusCandidates
                Dim candidate = IO.Path.Combine(root, relative)
                If File.Exists(candidate) Then Return candidate
            Next
        Next

        Return Nothing
    End Function

    Private Sub LoadCorpus(path As String, englishRaw As List(Of String), chineseRaw As List(Of String))
        Using reader As New StreamReader(path, Encoding.UTF8)
            Dim line As String = reader.ReadLine()

            While line IsNot Nothing
                If Not String.IsNullOrWhiteSpace(line) Then
                    Dim parts = line.Split(ControlChars.Tab)

                    If parts.Length >= 2 Then
                        Dim en = parts(0).Trim()
                        Dim zh = parts(1).Trim()

                        If en.Length > 0 AndAlso zh.Length > 0 Then
                            englishRaw.Add(en)
                            chineseRaw.Add(zh)
                        End If
                    End If
                End If

                line = reader.ReadLine()
            End While
        End Using
    End Sub

    ''' <summary>
    ''' 用 NLP 包的中文分词器构造切分器：先做一次分词统计，把出现频次 &gt;= 2 的
    ''' 多字词补进词典，再做正式切分，从而让整份语料的切分保持稳定。
    ''' </summary>
    Private Function BuildChineseTokenizer(sentences As List(Of String)) As nlp.Tokenizer
        Dim tokenizer As nlp.Tokenizer = nlp.Tokenizer.CreateDefault()
        Dim dictionary = tokenizer.Dictionary
        Dim baseSize As Integer = dictionary.Count
        Dim baseMaxLength As Integer = dictionary.MaxWordLength

        Dim frequency As New Dictionary(Of String, Integer)()

        For Each sentence In sentences
            For Each word In tokenizer.Segment(sentence)
                If word.Length >= 2 AndAlso nlp.MaxMatchTokenizer.IsChineseChar(word(0)) Then
                    Dim cnt As Integer = 0

                    If frequency.TryGetValue(word, cnt) Then
                        frequency(word) = cnt + 1
                    Else
                        frequency(word) = 1
                    End If
                End If
            Next
        Next

        Dim added As Integer = 0

        For Each item In frequency
            If item.Value >= 2 AndAlso Not dictionary.Contains(item.Key) Then
                dictionary.Add(item.Key, item.Value)
                added += 1
            End If
        Next

        Console.WriteLine($"[transformer-zh] 中文词典：基础 {baseSize} 词（最长 {baseMaxLength} 字），新增 {added} 词，当前 {dictionary.Count} 词")

        Return tokenizer
    End Function

    ''' <summary>
    ''' 英文切词：与 <see cref="TextProcessing.ProcessSentence"/> 保持一致的处理方式
    ''' （保留 ? / ! 为独立 token，去掉句点）。
    ''' </summary>
    Private Function TokenizeEnglish(sentence As String) As List(Of String)
        Dim normalized = sentence.ToLower()
        normalized = normalized.Replace("?", " ?")
        normalized = normalized.Replace("!", " !")
        normalized = normalized.Replace(".", "")

        Return New List(Of String)(normalized.Split(New Char() {" "c, ChrW(9)}, StringSplitOptions.RemoveEmptyEntries))
    End Function

    ''' <summary>中文切词：交给 NLP 包的中文分词器（词典 + HMM 混合）。</summary>
    Private Function TokenizeChinese(tokenizer As nlp.Tokenizer, sentence As String) As List(Of String)
        Return tokenizer.Segment(sentence)
    End Function

    Private Function CountDistinctTokens(sentences As List(Of List(Of String))) As Integer
        Dim words As New HashSet(Of String)()

        For Each sentence In sentences
            For Each word In sentence
                words.Add(word)
            Next
        Next

        Return words.Count
    End Function

    Private Function CloneSentences(source As List(Of List(Of String)), count As Integer) As List(Of List(Of String))
        Dim result As New List(Of List(Of String))()

        For i As Integer = 0 To count - 1
            result.Add(New List(Of String)(source(i)))
        Next

        Return result
    End Function

#End Region

#Region "译文评估"

    Private Sub Evaluate(model As TransformerModel, tokenizer As nlp.Tokenizer,
                         allEnglish As List(Of List(Of String)),
                         allChinese As List(Of List(Of String)),
                         nrSentences As Integer)
        Console.WriteLine()
        Console.WriteLine("============================================================")
        Console.WriteLine(" 翻译测试（源句来自训练集，用于验证模型是否记住了训练数据）")
        Console.WriteLine("============================================================")

        ' 1) 训练集整体完全匹配率：客观反映模型是否记住了训练句对
        Dim exact As Integer = 0
        Dim evaluated As Integer = 0

        For idx As Integer = 0 To nrSentences - 1
            Dim reference = ChineseSentence(allChinese(idx))
            Dim translated = ChineseSentence(TranslateWithModel(model, allEnglish(idx)))

            evaluated += 1

            If translated = reference Then exact += 1
        Next

        Console.WriteLine($"  训练集完全匹配：{exact}/{evaluated} ({If(evaluated > 0, exact / evaluated, 0):P1})")
        Console.WriteLine()

        ' 2) 抽样明细
        Dim indices() As Integer = {0, 2, 5, 9, 14, 20, 26, 33, 41, 50, 58, 66, 75, 84, 93}
        Dim emptyCount As Integer = 0
        Dim testedCount As Integer = 0

        For Each idx In indices
            If idx >= nrSentences Then Continue For

            testedCount += 1

            Dim source = String.Join(" ", allEnglish(idx))
            Dim reference = ChineseSentence(allChinese(idx))
            Dim tokens = TranslateWithModel(model, allEnglish(idx))
            Dim translated = ChineseSentence(tokens)

            If translated.Length = 0 Then emptyCount += 1

            Console.WriteLine($"  英文     ：{source}")
            Console.WriteLine($"  参考译文 ：{reference}")
            Console.WriteLine($"  模型译文 ：{If(translated.Length = 0, "<空>", translated)}")
            Console.WriteLine($"  输出 token：{If(tokens.Count = 0, "<空>", String.Join("|", tokens))}")
            Console.WriteLine()
        Next

        Console.WriteLine($"共测试 {testedCount} 句，其中 {emptyCount} 句模型在首词即输出结束符。")
        Console.WriteLine("提示：中文译文由 ChineseTokenizer 的切分结果拼接而成，未做分词后处理。")
    End Sub

    ''' <summary>调用模型做一次翻译，返回原始 token 序列（含 &lt; / &gt; 起止符）。</summary>
    Private Function TranslateWithModel(model As TransformerModel, englishWords As List(Of String)) As List(Of String)
        Try
            Dim translated = model.Infer(englishWords)

            If translated Is Nothing OrElse translated.Count = 0 Then Return New List(Of String)()

            Return translated(0)
        Catch ex As Exception
            Return New List(Of String) From {"<翻译失败：" & ex.Message & ">"}
        End Try
    End Function

    ''' <summary>把中文 token 序列拼成句子（中文不使用空格分隔）。</summary>
    Private Function ChineseSentence(tokens As List(Of String)) As String
        Dim sb As New StringBuilder()

        For Each token In tokens
            If token = "<" OrElse token = ">" Then Continue For
            sb.Append(token)
        Next

        Return sb.ToString()
    End Function

#End Region

End Module

