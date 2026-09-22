#Region "Microsoft.VisualBasic::398b607230f8125617f96cb5cc61ca98, Data_science\MachineLearning\DeepLearning\Transformer\TransformerModel.vb"

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

    '   Total Lines: 314
    '    Code Lines: 189 (60.19%)
    ' Comment Lines: 71 (22.61%)
    '    - Xml Docs: 73.24%
    ' 
    '   Blank Lines: 54 (17.20%)
    '     File Size: 15.96 KB


    '     Class TransformerModel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CloneSentences, Infer, Translate
    ' 
    '         Sub: Backward, Infer, MakeTrainingStep, SetDropoutNodes, Train
    '         Class DecoderStep
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' TransformerModel —— "Attention is all you need" 的编码器/解码器实现
'
' 迁移说明：
'   旧实现依赖 AutomaticDifferentiation 张量的自动微分 + Checkpoints 断点机制，
'   反向时只需 loss.CalculateDerivative(1) 即可自动回传整张计算图。
'   迁移到纯数值 Tensor 之后改为显式 BPTT：
'
'     1. 前向阶段逐解码步记录「解码输入句子快照 / 解码器各层缓存 / 输出概率 /
'        交叉熵梯度」，编码器只前向一次；
'     2. 反向阶段逆序回传各解码步，把每步对 encoderOutput 的梯度累加后再统一
'        回传编码器，最后回传两侧词嵌入；
'     3. 各组件用 Adam 参数-梯度对完成原地更新。
'
' 公开 API（构造函数、Train、Infer）保持不变，原有英西翻译 demo 无需修改。
' ---------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' Transformer architecture as described in "Attention is all you need"
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/jaksc00p/Transformer/tree/master
    ''' </remarks>
    Public Class TransformerModel

        Private sequenceLength As Integer
        Private dropout As Double

        Private encoder As EncoderStack
        Private decoder As DecoderStack
        Private englishEmbedding As Embedding
        Private spanishEmbedding As Embedding
        Private outputLayer As OutputLayer

        ''' <summary>一个解码步的前向记录，供 BPTT 反向使用。</summary>
        Private Class DecoderStep
            ''' <summary>该步解码器输入句子的快照（<c>translatedSpanishSentences</c> 是原地增长的，必须深拷贝）</summary>
            Public Sentences As List(Of List(Of String))
            ''' <summary>该步目标语言词嵌入</summary>
            Public SpanishEmbeddings As Tensor
            ''' <summary>该步输出层 softmax 概率，形状 [batch, 1, dictSize]</summary>
            Public Probs As Tensor
            ''' <summary>交叉熵对 logits 的梯度（未缩放）</summary>
            Public Delta As Tensor
            ''' <summary>该步解码器各层的前向缓存快照</summary>
            Public Caches As List(Of DecoderLayer.Cache)
            ''' <summary>该步输出层的前向缓存快照</summary>
            Public OutputCache As OutputLayer.Cache
        End Class

        Private _steps As List(Of DecoderStep)
        Private _encoderOutput As Tensor
        Private _englishEmbeddings As Tensor
        Private _englishSentences As List(Of List(Of String))
        Private _batchSize As Integer

        ''' <summary>
        ''' Creates a transformer translation model and prepares its embeddings from the provided training sentences.
        ''' </summary>
        ''' <param name="Nx">Number of encoder and decoder layers.</param>
        ''' <param name="embeddingSize">Width of the model.</param>
        ''' <param name="dk">Dimension of the query and key projections per head.</param>
        ''' <param name="dv">Dimension of the value projection per head.</param>
        ''' <param name="h">Number of attention heads.</param>
        ''' <param name="dff">Hidden width of the feed forward networks.</param>
        ''' <param name="batchSize">Number of sentences per training batch.</param>
        ''' <param name="dropout">Dropout rate used during training.</param>
        ''' <param name="allEnglishSentences">The source language sentences used to build the source vocabulary.</param>
        ''' <param name="allSpanishSentences">The target language sentences used to build the target vocabulary.</param>
        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer, batchSize As Integer, dropout As Double,
                       allEnglishSentences As List(Of List(Of String)),
                       allSpanishSentences As List(Of List(Of String)))

            Me.dropout = dropout

            Call InsertStartAndStopCharacters(allSpanishSentences)
            sequenceLength = CalculateSequenceLength(allEnglishSentences, allSpanishSentences)

            englishEmbedding = New Embedding(embeddingSize, sequenceLength, allEnglishSentences)
            spanishEmbedding = New Embedding(embeddingSize, sequenceLength, allSpanishSentences)
            encoder = New EncoderStack(Nx, embeddingSize, dk, dv, h, dff)
            decoder = New DecoderStack(Nx, embeddingSize, dk, dv, h, dff)
            outputLayer = New OutputLayer(sequenceLength, embeddingSize, spanishEmbedding.DictionarySize)
        End Sub

        ''' <summary>
        ''' Trains the model with explicit backpropagation through time over the decoder steps.
        ''' </summary>
        ''' <param name="nrEpochs">Number of passes over the data set.</param>
        ''' <param name="nrTrainingSteps">Number of decoder steps per batch.</param>
        ''' <param name="learningRate">The learning rate.</param>
        ''' <param name="batchSize">Number of sentence pairs per batch.</param>
        ''' <param name="allEnglishSentences">The source language sentences.</param>
        ''' <param name="allSpanishSentences">The target language sentences.</param>
        ''' <exception cref="ArgumentException">Thrown when the two sentence collections have different lengths.</exception>
        Public Sub Train(nrEpochs As Integer, nrTrainingSteps As Integer, learningRate As Double, batchSize As Integer, allEnglishSentences As List(Of List(Of String)), allSpanishSentences As List(Of List(Of String)))
            If allEnglishSentences.Count() <> allSpanishSentences.Count() Then Throw New ArgumentException("Number of sentence pairs must be equal")

            Console.WriteLine("Training:")
            Dim nrSentences As Integer = allSpanishSentences.Count()
            For epoch = 1 To nrEpochs
                For b As Integer = 0 To nrSentences / batchSize - 1
                    Console.WriteLine()
                    Console.WriteLine("Epoch: " & epoch.ToString() & ", Batch: " & (b + 1).ToString())
                    Dim englishSentences = allEnglishSentences.GetRange(b * batchSize, batchSize)
                    Dim spanishSentences = allSpanishSentences.GetRange(b * batchSize, batchSize)

                    For [step] = 1 To nrTrainingSteps
                        Call SetDropoutNodes()
                        Dim __ As List(Of List(Of String)) = Nothing
                        Dim stepLoss = Me.Translate(batchSize, True, englishSentences, spanishSentences, __)

                        ' 只在每个 batch 的最后一个 step 打印 loss，避免长训练把控制台刷屏
                        If [step] = nrTrainingSteps Then
                            Console.WriteLine("Step: " & [step].ToString() & ", loss: " & stepLoss.ToString())
                        End If

                        Call MakeTrainingStep(learningRate, [step])
                    Next
                Next
            Next
        End Sub

        Public Sub Infer()
            Console.WriteLine()
            Console.WriteLine("Inference (type q to quit):")
            Console.WriteLine()

            Dim wrongWord As String = Nothing, translatedSpanishSentence As List(Of List(Of String)) = Nothing

            While True
                Console.WriteLine("Write English sentence (max " & sequenceLength.ToString() & ") words:")
                Dim line As String = Console.ReadLine().Trim()
                If Equals(line.ToLower(), "q") Then Return

                Dim englishSentence = ProcessSentence(line)
                If englishSentence(0).Count > sequenceLength Then
                    Console.WriteLine("Sentence too long:")
                    Console.WriteLine()
                    Continue While
                End If

                If Not englishEmbedding.AllWordsInDictionary(englishSentence, wrongWord) Then
                    Console.WriteLine(wrongWord & " not in dictionary")
                    Console.WriteLine()
                    Continue While
                End If

                Call Translate(1, False, englishSentence, Nothing, translatedSpanishSentence)
                Console.WriteLine("Spanish translation:")
                Dim translation = ProcessSentence(translatedSpanishSentence, 0)
                translation = translation.Replace("<", "")
                translation = translation.Replace(">", "")
                translation = translation.Trim()
                Console.WriteLine(translation)
                Console.WriteLine()
            End While
        End Sub

        ''' <summary>
        ''' Translates a single sentence token by token.
        ''' </summary>
        ''' <param name="words">The source sentence tokens.</param>
        ''' <returns>
        ''' The translated sentences, or <c>Nothing</c> when the source sentence contains a word that is not in the dictionary.
        ''' </returns>
        Public Function Infer(words As IEnumerable(Of String)) As List(Of List(Of String))
            Dim from As New List(Of List(Of String)) From {New List(Of String)(words)}
            Dim wrongWord As String = Nothing
            Dim translatedSpanishSentence As List(Of List(Of String)) = Nothing

            If Not englishEmbedding.AllWordsInDictionary(from, wrongWord) Then
                Console.WriteLine(wrongWord & " not in dictionary")
                Console.WriteLine()
                Return Nothing
            End If

            Call Translate(1, False, from, Nothing, translatedSpanishSentence)

            Return translatedSpanishSentence
        End Function

        ''' <summary>
        ''' Translate a batch of sentenses by generating one word at a time for each 
        ''' sentence with the decoder until max length or stopping character.
        ''' </summary>
        ''' <returns>该批次的交叉熵损失（训练模式下按 <c>sequenceLength * batchSize</c> 归一化）</returns>
        Private Function Translate(batchSize As Integer, isTraining As Boolean,
                                   englishSentences As List(Of List(Of String)),
                                   correctSpanishSentences As List(Of List(Of String)),
                                   <Out>
                                   ByRef translatedSpanishSentences As List(Of List(Of String))) As Double

            If isTraining Then
                _steps = New List(Of DecoderStep)()
                _batchSize = batchSize
                _englishSentences = englishSentences
            End If

            Dim english_word_embeddings = englishEmbedding.Embed(englishSentences, isTraining)
            Dim encoderOutput = encoder.Encode(english_word_embeddings, isTraining)

            If isTraining Then
                _englishEmbeddings = english_word_embeddings
                _encoderOutput = encoderOutput
            End If

            translatedSpanishSentences = InitializeSpanishSentences(batchSize)
            Dim nrWords = If(isTraining, CalculateMaxSentenceLength(correctSpanishSentences), sequenceLength)
            Dim totalLoss As Double = 0.0

            For w = 1 To nrWords - 1
                ' 解码输入句子会在下一行被原地追加，因此训练时需要先做快照
                Dim sentencesSnapshot = If(isTraining, CloneSentences(translatedSpanishSentences), Nothing)

                Dim spanish_word_embeddings = spanishEmbedding.Embed(translatedSpanishSentences, isTraining)
                Dim decoder_output = decoder.Decode(encoderOutput, spanish_word_embeddings, isTraining)
                Dim output = outputLayer.Output(decoder_output)

                If isTraining Then
                    Dim dLogits As Tensor = Nothing
                    totalLoss += spanishEmbedding.CalculateLossAndGradient(output, correctSpanishSentences, w, dLogits)

                    _steps.Add(New DecoderStep With {
                        .Sentences = sentencesSnapshot,
                        .SpanishEmbeddings = spanish_word_embeddings,
                        .Probs = output,
                        .Delta = dLogits,
                        .Caches = decoder.LastCaches,
                        .OutputCache = outputLayer.LastCache
                    })
                End If

                Dim spanishWordIndexes As Integer() = TensorOps.ArgMaxLastDim(output)
                Dim spanishWords = spanishEmbedding.GetWords(spanishWordIndexes)
                Call AddWordsToSentences(batchSize, isTraining, spanishWords, translatedSpanishSentences)
            Next

            If isTraining Then totalLoss /= sequenceLength * batchSize

            Return totalLoss
        End Function

        Private Sub SetDropoutNodes()
            spanishEmbedding.SetDropoutNodes(dropout)
            englishEmbedding.SetDropoutNodes(dropout)
            encoder.SetDropoutNodes(dropout)
            decoder.SetDropoutNodes(dropout)
        End Sub

        ''' <summary>
        ''' 显式 BPTT：逆序回传各解码步，累加对 encoder 输出的梯度，再统一回传编码器与两侧词嵌入。
        ''' </summary>
        Private Sub Backward()
            If _steps Is Nothing OrElse _steps.Count = 0 Then Return

            Dim scale = 1.0 / (sequenceLength * _batchSize)
            Dim dEncoderOutput As Tensor = Nothing
            Dim applyDropout = dropout > 0

            For i = _steps.Count - 1 To 0 Step -1
                Dim st = _steps(i)

                ' 损失被除以 sequenceLength * batchSize，梯度同样需要按该系数缩放
                Call TensorOps.ScaleInPlace(st.Delta, scale)

                Dim dDecoderOutput = outputLayer.Backward(st.OutputCache, st.Delta)
                Dim dEncoderStep As Tensor = Nothing
                Dim dSpanishEmbeddings = decoder.Backward(st.Caches, dDecoderOutput, dEncoderStep)

                If dEncoderStep IsNot Nothing Then
                    If dEncoderOutput Is Nothing Then
                        dEncoderOutput = dEncoderStep
                    Else
                        Call TensorOps.Accumulate(dEncoderOutput, dEncoderStep)
                    End If
                End If

                spanishEmbedding.Backward(dSpanishEmbeddings, st.Sentences, applyDropout)
            Next

            Dim dEnglishEmbeddings = encoder.Backward(dEncoderOutput)

            englishEmbedding.Backward(dEnglishEmbeddings, _englishSentences, applyDropout)
        End Sub

        Private Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            Call Backward()

            englishEmbedding.MakeTrainingStep(learningRate, [step])
            spanishEmbedding.MakeTrainingStep(learningRate, [step])
            encoder.MakeTrainingStep(learningRate, [step])
            decoder.MakeTrainingStep(learningRate, [step])
            outputLayer.MakeTrainingStep(learningRate, [step])
        End Sub

        ''' <summary>深拷贝一组句子（BPTT 需要保存每一步解码输入的快照）。</summary>
        Private Shared Function CloneSentences(sentences As List(Of List(Of String))) As List(Of List(Of String))
            Dim result As New List(Of List(Of String))()

            For Each sentence In sentences
                result.Add(New List(Of String)(sentence))
            Next

            Return result
        End Function

    End Class
End Namespace
