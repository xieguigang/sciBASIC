#Region "Microsoft.VisualBasic::bd732f4f0eb94ba2de9bdb8786f86a91, sciBASIC#\Data\word2vec\Word2Vec.vb"

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

    '   Total Lines: 404
    '    Code Lines: 273
    ' Comment Lines: 55
    '   Blank Lines: 76
    '     File Size: 15.21 KB


    '     Class Word2Vec
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: outputVector
    ' 
    '         Sub: buildVocabulary, cbowGram, computeExp, readTokens, saveModel
    '              skipGram, training
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils
Imports Microsoft.VisualBasic.Text
Imports stdNum = System.Math

Namespace NlpVec

    ''' <summary>
    ''' Created by fangy on 13-12-19.
    ''' Word2Vec 算法实现
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/siegfang/word2vec
    ''' </remarks>
    Public Class Word2Vec

        Friend windowSize As Integer '文字窗口大小
        Private vectorSize As Integer '词向量的元素个数

        Friend trainMethod As TrainMethod ' 神经网络学习方法
        Friend sample As Double
        '    private int negativeSample;
        Friend alpha As Double ' 学习率，并行时由线程更新
        Private alphaThresold As Double
        Friend initialAlpha As Double ' 初始学习率
        Private freqThresold As Integer = 5
        Friend ReadOnly alphaLock As SByte() = New SByte(-1) {} ' alpha同步锁
        Private ReadOnly treeLock As SByte() = New SByte(-1) {} ' alpha同步锁
        Private ReadOnly vecLock As SByte() = New SByte(-1) {} ' alpha同步锁
        Private expTable As Double()
        Private Const EXP_TABLE_SIZE As Integer = 1000
        Private Const MAX_EXP As Integer = 6
        Friend neuronMap As IDictionary(Of String, WordNeuron)
        '    private List<Word> words;
        Friend totalWordCount As Integer ' 语料中的总词数
        Friend currentWordCount As Integer ' 当前已阅的词数，并行时由线程更新
        Private numOfThread As Integer ' 线程个数

        ' 单词或短语计数器
        Private wordCounter As New Counter(Of String)()
        Private tempCorpus As String
        Private tempCorpusWriter As StreamWriter

        Friend Sub New(factory As Word2VecFactory)
            vectorSize = factory.vectorSize
            windowSize = factory.windowSize
            freqThresold = factory.freqThresold
            trainMethod = factory.trainMethod
            sample = factory.sample
            '        negativeSample = factory.negativeSample;
            alpha = factory.alpha
            initialAlpha = alpha
            alphaThresold = factory.alphaThreshold
            numOfThread = factory.numOfThread
            totalWordCount = 0
            expTable = New Double(999) {}
            computeExp()
        End Sub

        ''' <summary>
        ''' 预先计算并保存sigmoid函数结果，加快后续计算速度
        ''' f(x) = x / (x + 1)
        ''' </summary>
        Private Sub computeExp()
            For i = 0 To EXP_TABLE_SIZE - 1
                expTable(i) = stdNum.Exp((i / EXP_TABLE_SIZE * 2 - 1) * MAX_EXP)
                expTable(i) = expTable(i) / (expTable(i) + 1)
            Next
        End Sub

        ''' <summary>
        ''' 读取一段文本，统计词频和相邻词语出现的频率，
        ''' 文本将输出到一个临时文件中，以方便之后的训练 </summary>
        ''' <param name="tokenizer"> 标记 </param>
        Public Sub readTokens(tokenizer As Tokenizer)
            If tokenizer Is Nothing OrElse tokenizer.size() < 1 Then
                Return
            End If

            currentWordCount += tokenizer.size()
            ' 读取文本中的词，并计数词频
            While tokenizer.hasMoreTokens()
                wordCounter.add(tokenizer.nextToken())
            End While
            ' 将文本输出到临时文件中，供后续训练之用
            Try

                If tempCorpus Is Nothing Then
                    Dim tempDir As String = App.GetTempFile

                    If Not tempDir.DirectoryExists Then
                        Dim tempCreated As Boolean = tempDir.MakeDir

                        If Not tempCreated Then
                            Call ("unable to create temp file in " & tempDir.GetDirectoryFullPath).Warning
                        End If
                    End If

                    tempCorpus = TempFileSystem.GetAppSysTempFile(".txt", App.PID, "tempCorpus")
                    tempCorpusWriter = New StreamWriter(tempCorpus.Open, Encoding.UTF8)
                End If

                tempCorpusWriter.Write(tokenizer.ToString(" "))
                tempCorpusWriter.WriteLine()
            Catch e As Exception
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)

                Try
                    tempCorpusWriter.Close()
                Catch e1 As Exception
                    Console.WriteLine(e1.ToString())
                    Console.Write(e1.StackTrace)
                End Try
            End Try
        End Sub

        Private Sub buildVocabulary()
            neuronMap = New Dictionary(Of String, WordNeuron)()

            For Each wordText As String In wordCounter.keySet
                Dim freq = wordCounter.get(wordText)

                If freq < freqThresold Then
                    Continue For
                End If

                neuronMap(wordText) = New WordNeuron(wordText, wordCounter.get(wordText), vectorSize)
            Next

            Call ("read " & neuronMap.Count & " word totally.").__INFO_ECHO
            '        System.out.println("共读取了 " + neuronMap.size() + " 个词。");

        End Sub

        Public Sub training()
            If tempCorpus Is Nothing Then
                Throw New NullReferenceException("训练语料为空，如果之前调用了training()，" & "请调用readLine(String sentence)重新输入语料")
            End If

            buildVocabulary()
            HuffmanTree.make(neuronMap.Values)
            ' 重新遍历语料
            totalWordCount = currentWordCount
            currentWordCount = 0
            tempCorpusWriter.Close()

            Dim corpus As LinkedList(Of String) = New LinkedList(Of String)() '若干文本组成的语料
            Dim trainBlockSize = 500 '语料中句子个数
            Dim trainer As New Trainer(Me, corpus)

            For Each li As String In tempCorpus.LineIterators(Encodings.UTF8)
                'Dim corpusQueue As BlockingQueue(Of LinkedList(Of String)) = New ArrayBlockingQueue(Of LinkedList(Of String))(numOfThread)
                'Dim futures As LinkedList(Of Future) = New LinkedList(Of Future)() '每个线程的返回结果，用于等待线程

                'For thi = 0 To numOfThread - 1
                '    '                threadPool.execute(new Trainer(corpusQueue));
                '    futures.AddLast(threadPool.submit(New Trainer(Me, corpusQueue)))
                'Next

                corpus.AddLast(li)

                If corpus.Count = trainBlockSize Then
                    '放进任务队列，供线程处理
                    '                    futures.add(threadPool.submit(new Trainer(corpus)));

                    ' corpusQueue.put(corpus)
                    '                    System.out.println("put a corpus");

                End If
            Next

            '            futures.add(threadPool.submit(new Trainer(corpus)));

            Call ("the task queue has been allocated completely, " & "please wait the thread pool (" & numOfThread & ") to process...").__INFO_ECHO

            ' 等待线程处理完语料
            Call trainer.run()
        End Sub

        Friend Sub skipGram(index As Integer, sentence As IList(Of WordNeuron), b As Integer, alpha As Double)
            Dim word = sentence(index)
            Dim a As Integer, c = 0

            For a = b To windowSize * 2 + 1 - b - 1

                If a = windowSize Then
                    Continue For
                End If

                c = index - windowSize + a

                If c < 0 OrElse c >= sentence.Count Then
                    Continue For
                End If

                Dim neu1e = New Double(vectorSize - 1) {} '误差项
                'Hierarchical Softmax
                Dim pathNeurons = word.pathNeurons
                Dim we = sentence(c)

                For neuronIndex = 0 To pathNeurons.Count - 1 - 1
                    Dim out = CType(pathNeurons(neuronIndex), HuffmanNeuron)
                    Dim f As Double = 0
                    ' Propagate hidden -> output
                    For j = 0 To vectorSize - 1
                        f += we.vector(j) * out.vector(j)
                    Next

                    If f <= -MAX_EXP OrElse f >= MAX_EXP Then
                        '                    System.out.println("F: " + f);
                        Continue For
                    Else
                        f = (f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2)
                        f = expTable(f)
                    End If
                    ' 'g' is the gradient multiplied by the learning rate
                    Dim outNext = CType(pathNeurons(neuronIndex + 1), HuffmanNeuron)
                    Dim g = (1 - outNext.code - f) * alpha

                    For c = 0 To vectorSize - 1
                        neu1e(c) += g * out.vector(c)
                    Next
                    ' Learn weights hidden -> output
                    For c = 0 To vectorSize - 1
                        out.vector(c) += g * we.vector(c)
                    Next
                Next
                ' Learn weights input -> hidden
                For j = 0 To vectorSize - 1
                    we.vector(j) += neu1e(j)
                Next
            Next

            '        if (word.getName().equals("手")){
            '            for (Double value : word.vector){
            '                System.out.print(value + "\t");
            '            }
            '            System.out.println();
            '        }
        End Sub

        Friend Sub cbowGram(index As Integer, sentence As IList(Of WordNeuron), b As Integer, alpha As Double)
            Dim word = sentence(index)
            Dim a As Integer, c = 0
            Dim neu1e = New Double(vectorSize - 1) {} '误差项
            Dim neu1 = New Double(vectorSize - 1) {} '误差项
            Dim last_word As WordNeuron

            For a = b To windowSize * 2 + 1 - b - 1

                If a <> windowSize Then
                    c = index - windowSize + a

                    If c < 0 Then
                        Continue For
                    End If

                    If c >= sentence.Count Then
                        Continue For
                    End If

                    last_word = sentence(c)

                    If last_word Is Nothing Then
                        Continue For
                    End If

                    For c = 0 To vectorSize - 1
                        neu1(c) += last_word.vector(c)
                    Next
                End If
            Next
            'Hierarchical Softmax
            Dim pathNeurons = word.pathNeurons

            For neuronIndex = 0 To pathNeurons.Count - 1 - 1
                Dim out = CType(pathNeurons(neuronIndex), HuffmanNeuron)
                Dim f As Double = 0
                ' Propagate hidden -> output
                For c = 0 To vectorSize - 1
                    f += neu1(c) * out.vector(c)
                Next

                If f <= -MAX_EXP Then
                    Continue For
                ElseIf f >= MAX_EXP Then
                    Continue For
                Else
                    f = expTable((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))
                End If
                ' 'g' is the gradient multiplied by the learning rate
                Dim outNext = CType(pathNeurons(neuronIndex + 1), HuffmanNeuron)
                Dim g = (1 - outNext.code - f) * alpha
                '
                For c = 0 To vectorSize - 1
                    neu1e(c) += g * out.vector(c)
                Next
                ' Learn weights hidden -> output
                For c = 0 To vectorSize - 1
                    out.vector(c) += g * neu1(c)
                Next
            Next

            For a = b To windowSize * 2 + 1 - b - 1

                If a <> windowSize Then
                    c = index - windowSize + a

                    If c < 0 Then
                        Continue For
                    End If

                    If c >= sentence.Count Then
                        Continue For
                    End If

                    last_word = sentence(c)

                    If last_word Is Nothing Then
                        Continue For
                    End If

                    For c = 0 To vectorSize - 1
                        last_word.vector(c) += neu1e(c)
                    Next
                End If
            Next
        End Sub

        Friend nextRandom As Long = 5

        ''' <summary>
        ''' 保存训练得到的模型 </summary>
        ''' <param name="file"> 模型存放路径 </param>
        Public Sub saveModel(file As FileStream)
            Dim dataOutputStream As BinaryWriter = Nothing

            Try
                dataOutputStream = New BinaryWriter(file)
                dataOutputStream.Write(neuronMap.Count)
                dataOutputStream.Write(vectorSize)

                For Each element In neuronMap.SetOfKeyValuePairs()
                    dataOutputStream.Write(element.Key)

                    For Each d In element.Value.vector
                        dataOutputStream.Write(CType(d, Double?).Value)
                    Next
                Next

                Call ("saving model successfully in " & file.Name).__INFO_ECHO
            Catch e As IOException
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            Finally

                Try

                    If dataOutputStream IsNot Nothing Then
                        dataOutputStream.Close()
                    End If

                Catch ioe As IOException
                    Console.WriteLine(ioe.ToString())
                    Console.Write(ioe.StackTrace)
                End Try
            End Try
        End Sub

        Public Function outputVector() As VectorModel
            Dim wordMapConverted As IDictionary(Of String, Single()) = New Dictionary(Of String, Single())()
            Dim wordKey As String
            Dim vector As Single()
            Dim vectorLength As Double
            Dim vectorNorm As Double()

            For Each element In neuronMap.SetOfKeyValuePairs()
                wordKey = element.Key
                vectorNorm = element.Value.vector
                vector = New Single(vectorSize - 1) {}
                vectorLength = 0

                For vi = 0 To vectorNorm.Length - 1
                    vectorLength += CSng(vectorNorm(vi)) * vectorNorm(vi)
                    vector(vi) = CSng(vectorNorm(vi))
                Next

                vectorLength = stdNum.Sqrt(vectorLength)

                For vi = 0 To vector.Length - 1
                    vector(vi) /= CSng(vectorLength)
                Next

                wordMapConverted(wordKey) = vector
            Next

            Return New VectorModel(wordMapConverted, vectorSize)
        End Function
    End Class
End Namespace
