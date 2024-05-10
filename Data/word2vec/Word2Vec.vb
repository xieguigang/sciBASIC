#Region "Microsoft.VisualBasic::6439c6c0c697af6e95be2f8c42b01cf8, G:/GCModeller/src/runtime/sciBASIC#/Data/word2vec//Word2Vec.vb"

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

    '   Total Lines: 371
    '    Code Lines: 226
    ' Comment Lines: 78
    '   Blank Lines: 67
    '     File Size: 12.63 KB


    ' Class Word2Vec
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: outputVector
    ' 
    '     Sub: buildVocabulary, cbowGram, computeExp, (+2 Overloads) readTokens, skipGram
    '          training
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.HuffmanTree
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports std = System.Math

''' <summary>
''' Created by fangy on 13-12-19.
''' Word2Vec 算法实现
''' </summary>
''' <remarks>
''' https://github.com/siegfang/word2vec
''' </remarks>
Public Class Word2Vec

    ''' <summary>
    ''' 文字窗口大小
    ''' </summary>
    Friend windowSize As Integer
    ''' <summary>
    ''' 词向量的元素个数
    ''' </summary>
    Private vectorSize As Integer

    ''' <summary>
    ''' 神经网络学习方法
    ''' </summary>
    Friend trainMethod As TrainMethod
    Friend sample As Double
    '    private int negativeSample;
    ''' <summary>
    ''' 学习率，并行时由线程更新
    ''' </summary>
    Friend alpha As Double
    Private alphaThresold As Double
    ''' <summary>
    ''' 初始学习率
    ''' </summary>
    Friend initialAlpha As Double
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
    Private wordCounter As New TokenCounter(Of String)()
    Private tempCorpus As New List(Of String())

    ''' <summary>
    ''' 语料中句子个数
    ''' </summary>
    Friend trainBlockSize As Integer = 500

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
        For i As Integer = 0 To EXP_TABLE_SIZE - 1
            expTable(i) = std.Exp((i / EXP_TABLE_SIZE * 2 - 1) * MAX_EXP)
            expTable(i) = expTable(i) / (expTable(i) + 1)
        Next
    End Sub

    ''' <summary>
    ''' 读取一段文本，统计词频和相邻词语出现的频率，
    ''' 文本将输出到一个临时文件中，以方便之后的训练 </summary>
    ''' <param name="tokenizer"> 标记 </param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub readTokens(tokenizer As Sentence)
        Call readTokens((From wi As Word In tokenizer.words Select wi.str).ToArray)
    End Sub

    ''' <summary>
    ''' input a sentence words
    ''' </summary>
    ''' <param name="tokenizer">
    ''' a collection of the words in current given sentence.
    ''' </param>
    Public Sub readTokens(tokenizer As ICollection(Of String))
        If tokenizer Is Nothing OrElse tokenizer.Count < 1 Then
            Return
        Else
            currentWordCount += tokenizer.Count
        End If

        ' 读取文本中的词，并计数词频
        For Each word As String In tokenizer
            Call wordCounter.add(word)
        Next

        Call tempCorpus.Add(tokenizer.ToArray)
    End Sub

    ''' <summary>
    ''' create a set of the <see cref="WordNeuron"/>
    ''' </summary>
    Private Sub buildVocabulary()
        neuronMap = New Dictionary(Of String, WordNeuron)()

        For Each wordText As String In wordCounter.keySet
            Dim freq = wordCounter.get(wordText)

            If freq < freqThresold Then
                Continue For
            End If

            neuronMap(wordText) = New WordNeuron(wordText, wordCounter.get(wordText), vectorSize)
        Next

        Call VBDebugger.EchoLine("read " & neuronMap.Count & " word totally.")
    End Sub

    Public Sub training()
        ' 若干文本组成的语料
        Dim corpus As New LinkedList(Of String())()
        Dim trainer As New Trainer(Me, corpus)

        Call buildVocabulary()
        Call HuffmanTree.make(neuronMap.Values)

        ' 重新遍历语料
        totalWordCount = currentWordCount
        currentWordCount = 0

        For Each li As String() In tempCorpus
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

        ' Call ("the task queue has been allocated completely, " & "please wait the thread pool (" & numOfThread & ") to process...").__INFO_ECHO

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
            Dim we As WordNeuron = sentence(c)

            For neuronIndex = 0 To pathNeurons.Count - 1 - 1
                Dim out As HuffmanNeuron = pathNeurons(neuronIndex)
                Dim f As Double = 0

                ' Propagate hidden -> output
                For j As Integer = 0 To vectorSize - 1
                    f += we.vector(j) * out.vector(j)
                Next

                If f <= -MAX_EXP OrElse f >= MAX_EXP Then
                    '                    System.out.println("F: " + f);
                    Continue For
                Else
                    f = (f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2)

                    Dim fi As Integer = CInt(f)

                    If fi < 0 Then
                        fi = 0
                    ElseIf fi >= expTable.Length Then
                        fi = expTable.Length - 1
                    End If

                    f = expTable(fi)
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
    ''' export the trained word vector model from this function
    ''' </summary>
    ''' <returns></returns>
    Public Function outputVector() As VectorModel
        Dim wordMapConverted As New Dictionary(Of String, Single())()
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

            vectorLength = std.Sqrt(vectorLength)

            For vi = 0 To vector.Length - 1
                vector(vi) /= CSng(vectorLength)
            Next

            wordMapConverted(wordKey) = vector
        Next

        Return New VectorModel(wordMapConverted, vectorSize)
    End Function
End Class
