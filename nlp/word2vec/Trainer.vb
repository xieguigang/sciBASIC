#Region "Microsoft.VisualBasic::036cbb66f6cbdd6a5f97eda2b9219bc0, Data\word2vec\Trainer.vb"

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

    '   Total Lines: 103
    '    Code Lines: 78 (75.73%)
    ' Comment Lines: 7 (6.80%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (17.48%)
    '     File Size: 4.46 KB


    ' Class Trainer
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: run
    ' 
    '     Sub: computeAlpha, training
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports std = System.Math

Public Class Trainer : Implements ITaskDriver

    Private ReadOnly outerInstance As Word2Vec
    Friend corpusQueue As Queue(Of LinkedList(Of String()))
    Friend corpusToBeTrained As LinkedList(Of String())
    Friend trainingWordCount As Integer
    Friend tempAlpha As Double

    Public Sub New(outerInstance As Word2Vec, corpus As LinkedList(Of String()))
        Me.outerInstance = outerInstance
        corpusToBeTrained = corpus
        trainingWordCount = 0
        corpusQueue = New Queue(Of LinkedList(Of String()))
        corpusQueue.Enqueue(corpus)
    End Sub

    Public Sub New(outerInstance As Word2Vec, corpusQueue As Queue(Of LinkedList(Of String())))
        Me.outerInstance = outerInstance
        Me.corpusQueue = corpusQueue
    End Sub

    Friend Sub computeAlpha()
        SyncLock outerInstance.alphaLock
            outerInstance.currentWordCount += trainingWordCount
            outerInstance.alpha = outerInstance.initialAlpha * (1 - outerInstance.currentWordCount / (outerInstance.totalWordCount + 1))

            If outerInstance.alpha < outerInstance.initialAlpha * 0.0001 Then
                outerInstance.alpha = outerInstance.initialAlpha * 0.0001
            End If
            '                logger.info("alpha:" + tempAlpha + "\tProgress: "
            '                        + (int) (currentWordCount / (double) (totalWordCount + 1) * 100) + "%");
            VBDebugger.EchoLine("alpha:" & tempAlpha & vbTab & "Progress: " & outerInstance.currentWordCount / (outerInstance.totalWordCount + 1) * 100 & "%" & vbTab)
        End SyncLock
    End Sub

    Friend Sub training()
        '            long nextRandom = 5;
        For Each tokenizer As String() In corpusToBeTrained
            Dim sentence As New List(Of WordNeuron)()

            trainingWordCount += tokenizer.Length

            For Each token As String In tokenizer
                Dim entry = outerInstance.neuronMap.GetValueOrNull(token)

                If entry Is Nothing Then
                    Continue For
                End If
                ' The subsampling randomly discards frequent words while keeping the ranking same
                If outerInstance.sample > 0 Then
                    Dim ran = (std.Sqrt(entry.frequency / (outerInstance.sample * outerInstance.totalWordCount)) + 1) *
                        (outerInstance.sample * outerInstance.totalWordCount) / entry.frequency
                    outerInstance.nextRandom = outerInstance.nextRandom * 25214903917L + 11

                    If ran < (outerInstance.nextRandom And &HFFFF) / 65536 Then
                        Continue For
                    End If

                    sentence.Add(entry)
                End If
            Next

            For index As Integer = 0 To sentence.Count - 1
                outerInstance.nextRandom = outerInstance.nextRandom * 25214903917L + 11

                Select Case outerInstance.trainMethod
                    Case TrainMethod.CBow
                        outerInstance.cbowGram(index, sentence, CInt(outerInstance.nextRandom) Mod outerInstance.windowSize, tempAlpha)
                    Case TrainMethod.Skip_Gram
                        outerInstance.skipGram(index, sentence, CInt(outerInstance.nextRandom) Mod outerInstance.windowSize, tempAlpha)
                End Select
            Next
        Next
    End Sub

    Public Function run() As Integer Implements ITaskDriver.Run
        Dim hasCorpusToBeTrained = True

        While hasCorpusToBeTrained
            '                    System.out.println("get a corpus");
            corpusToBeTrained = corpusQueue.Dequeue
            '                    System.out.println("队列长度:" + corpusQueue.size());
            If Nothing IsNot corpusToBeTrained Then
                tempAlpha = outerInstance.alpha
                trainingWordCount = 0
                training()
                computeAlpha() '更新alpha

                If corpusQueue.Count = 0 Then
                    Return 0
                End If
            Else
                ' 超过2s还没获得数据，认为主线程已经停止投放语料，即将停止训练。
                hasCorpusToBeTrained = False
            End If
        End While

        Return 0
    End Function
End Class
