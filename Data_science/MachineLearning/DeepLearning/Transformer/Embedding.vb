#Region "Microsoft.VisualBasic::98fedf0adb86b5eee8710e66033e5519, Data_science\MachineLearning\DeepLearning\Transformer\Embedding.vb"

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
    '    Code Lines: 182 (55.66%)
    ' Comment Lines: 90 (27.52%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 55 (16.82%)
    '     File Size: 14.29 KB


    '     Class Embedding
    ' 
    '         Properties: DictionarySize, EmbeddingSize, Parameters, SequenceLength
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AllWordsInDictionary, CalculateLossAndGradient, Embed, GetWordIndex, GetWords
    ' 
    '         Sub: AddPositionalEncoding, Backward, MakeTrainingStep, OneHotEmbedding, SetDropoutNodes
    '              ZeroGradients
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' Embedding —— 词嵌入层（迁移到 TensorFlow\Tensor.vb + 手写反向传播）
'
' 前向：按 one-hot 索引把 embeddingLayer 的对应行拷贝到 [batch, seq, emb]，
'       再叠加正弦位置编码，训练时按最后一维做 dropout。
' 反向：穿过 dropout 之后，把每个 (sentence, position) 的梯度散射累加回
'       embeddingLayer 对应行（同一词出现多次会自然累加）。
' ---------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' Use a learned embedding layer to reduce the size of the word embedding space.
    ''' </summary>
    Public Class Embedding

        Private _EmbeddingSize As Integer, _SequenceLength As Integer
        Private allWords As List(Of String) = New List(Of String)()
        Private one_hot As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()
        Private dropoutMask As Boolean()
        Private dropoutRate As Double = 0

        ''' <summary>
        ''' Learned linear embedding layer
        ''' </summary>
        Private embeddingLayer As Tensor

        Private embeddingLayerOptimizer As Optimizer

        ''' <summary>Gets the number of distinct words in the dictionary.</summary>
        Public ReadOnly Property DictionarySize As Integer
            Get
                Return one_hot.Count
            End Get
        End Property

        ''' <summary>Gets the width of the embedding vectors.</summary>
        Public Property EmbeddingSize As Integer
            Get
                Return _EmbeddingSize
            End Get
            Private Set(value As Integer)
                _EmbeddingSize = value
            End Set
        End Property

        ''' <summary>Gets the fixed sequence length used by this embedding layer.</summary>
        Public Property SequenceLength As Integer
            Get
                Return _SequenceLength
            End Get
            Private Set(value As Integer)
                _SequenceLength = value
            End Set
        End Property

        ''' <summary>
        ''' 与 <see cref="embeddingLayer"/> 同形的梯度累加器。
        ''' </summary>
        Friend ReadOnly Property Parameters As Tensor
            Get
                Return embeddingLayer
            End Get
        End Property

        ''' <summary>
        ''' Creates the embedding layer and builds the one-hot dictionary from the given sentences.
        ''' </summary>
        ''' <param name="embeddingSize">Width of the embedding vectors.</param>
        ''' <param name="sequenceLength">Fixed sequence length of the inputs.</param>
        ''' <param name="sentences">The sentences used to build the word dictionary.</param>
        Public Sub New(embeddingSize As Integer, sequenceLength As Integer, sentences As List(Of List(Of String)))
            Me.EmbeddingSize = embeddingSize
            Me.SequenceLength = sequenceLength

            Call OneHotEmbedding(sentences)

            embeddingLayer = TensorOps.HeNormalInit(New Integer() {DictionarySize, Me.EmbeddingSize})
            embeddingLayerOptimizer = New Optimizer(embeddingLayer)

            dropoutMask = New Boolean(embeddingSize - 1) {}
        End Sub

        ''' <summary>
        ''' Looks up the embedding of every word, adds the positional encoding and optionally applies dropout.
        ''' </summary>
        ''' <param name="sentences">The batch of tokenized sentences.</param>
        ''' <param name="isTraining">When <c>True</c> dropout is applied where configured.</param>
        ''' <returns>The embedded sentences, shaped <c>[batch, seq, emb]</c>.</returns>
        Public Function Embed(sentences As List(Of List(Of String)), isTraining As Boolean) As Tensor
            Dim batchSize = sentences.Count
            Dim wordEmbeddings As Tensor = New Tensor(batchSize, SequenceLength, EmbeddingSize)
            Dim emb = wordEmbeddings.Data
            Dim layer = embeddingLayer.Data
            Dim n = EmbeddingSize
            Dim s = 0

            For Each sentence In sentences
                Dim word_count = 0

                For Each word In sentence
                    ' No need for matrix multiplication since only one element of vector is nonzero
                    Dim pos As Integer = one_hot(word.ToLower())
                    Array.Copy(layer, pos * n, emb, (s * SequenceLength + word_count) * n, n)
                    word_count += 1
                Next

                Call AddPositionalEncoding(wordEmbeddings, s, sentence.Count())
                s += 1
            Next

            Call wordEmbeddings.MarkHostModified()

            If isTraining AndAlso dropoutRate > 0 Then wordEmbeddings = TensorOps.DropoutMask(wordEmbeddings, dropoutMask, dropoutRate)

            Return wordEmbeddings
        End Function

        ''' <summary>
        ''' Backpropagates through the embedding layer: the gradient of every (sentence, position) pair is scattered back into
        ''' the row of the embedding matrix that belongs to the word.
        ''' </summary>
        ''' <param name="dWordEmbeddings">Gradient with respect to the <see cref="Embed"/> output, shaped <c>[batch, seq, emb]</c>.</param>
        ''' <param name="sentences">The sentences used during the forward pass, needed to recover the word indices.</param>
        ''' <param name="applyDropout">Whether dropout was applied during the forward pass.</param>
        Public Sub Backward(dWordEmbeddings As Tensor, sentences As List(Of List(Of String)), applyDropout As Boolean)
            Dim dOut = dWordEmbeddings

            If applyDropout AndAlso dropoutRate > 0 Then
                dOut = TensorOps.DropoutMaskBackward(dOut, dropoutMask, dropoutRate)
            End If

            Dim grad = embeddingLayerOptimizer.Gradient.Data
            Dim dIn = dOut.Data
            Dim n = EmbeddingSize
            Dim s = 0

            For Each sentence In sentences
                Dim word_count = 0

                For Each word In sentence
                    Dim pos As Integer = one_hot(word.ToLower())
                    Dim src = (s * SequenceLength + word_count) * n
                    Dim dst = pos * n

                    For i As Integer = 0 To n - 1
                        grad(dst + i) += dIn(src + i)
                    Next

                    word_count += 1
                Next

                s += 1
            Next

            Call embeddingLayerOptimizer.Gradient.MarkHostModified()
        End Sub

        ''' <summary>
        ''' Computes the cross entropy loss and its gradient with respect to the output layer logits.
        ''' </summary>
        ''' <remarks>
        ''' For softmax followed by cross entropy the derivative is simply <c>d(logits) = softmax - onehot</c>, so the chain rule
        ''' through log and softmax is skipped. The returned value is the unscaled loss contribution of this step; the caller
        ''' divides it by <c>sequenceLength * batchSize</c>.
        ''' </remarks>
        ''' <param name="filteredOutput">Output layer softmax probabilities, shaped <c>[batch, 1, dictSize]</c>.</param>
        ''' <param name="correctSentences">The correct target sentences.</param>
        ''' <param name="w">Index of the word position that is currently being predicted.</param>
        ''' <param name="dLogits">Receives the gradient with respect to the logits, with the same shape as <paramref name="filteredOutput"/>.</param>
        ''' <returns>The unscaled cross entropy loss of this step.</returns>
        Public Function CalculateLossAndGradient(filteredOutput As Tensor,
                                                 correctSentences As List(Of List(Of String)),
                                                 w As Integer,
                                                 ByRef dLogits As Tensor) As Double
            Dim dictSize = filteredOutput.Shape(filteredOutput.Rank - 1)
            Dim batchSize = filteredOutput.Shape(0)

            dLogits = New Tensor(filteredOutput.Shape)

            Dim p = filteredOutput.Data
            Dim g = dLogits.Data
            Dim total As Double = 0.0

            For s = 0 To correctSentences.Count() - 1
                If w >= correctSentences(s).Count() Then Continue For

                Dim ind = GetWordIndex(correctSentences(s)(w))
                Dim baseIdx = s * dictSize

                For j = 0 To dictSize - 1
                    g(baseIdx + j) = p(baseIdx + j)
                Next

                g(baseIdx + ind) -= 1.0
                total -= std.Log(std.Max(p(baseIdx + ind), 1.0E-12))
            Next

            Call dLogits.MarkHostModified()

            Return total
        End Function

        ''' <summary>
        ''' Gets the words that correspond to the given dictionary indices.
        ''' </summary>
        ''' <param name="indexes">The dictionary indices.</param>
        ''' <returns>The words stored at those indices.</returns>
        Public Function GetWords(indexes As Integer()) As String()
            Dim words = New String(indexes.Length - 1) {}
            For s = 0 To indexes.Length - 1
                words(s) = allWords(indexes(s))
            Next

            Return words
        End Function

        ''' <summary>
        ''' Gets the dictionary index of a word.
        ''' </summary>
        ''' <param name="word">The word to look up.</param>
        ''' <returns>The dictionary index of <paramref name="word"/>.</returns>
        Public Function GetWordIndex(word As String) As Integer
            Return one_hot(word)
        End Function

        ''' <summary>
        ''' Checks whether every word of the given sentences exists in the dictionary.
        ''' </summary>
        ''' <param name="sentences">The sentences to check.</param>
        ''' <param name="wordNotInDictionary">Receives the first word that is missing from the dictionary.</param>
        ''' <returns><c>True</c> when all words are known; otherwise <c>False</c>.</returns>
        Public Function AllWordsInDictionary(sentences As List(Of List(Of String)), <Out> ByRef wordNotInDictionary As String) As Boolean
            wordNotInDictionary = ""

            For Each sentence In sentences
                For Each w In sentence
                    If Not one_hot.ContainsKey(w) Then
                        wordNotInDictionary = w
                        Return False
                    End If
                Next
            Next

            Return True
        End Function

        ''' <summary>
        ''' Encode all words in a dictionary with one-hot embedding
        ''' </summary>
        ''' <param name="sentences"></param>
        Private Sub OneHotEmbedding(sentences As List(Of List(Of String)))
            Dim word_index = 0
            For Each sentence In sentences
                For Each word In sentence
                    If Not one_hot.ContainsKey(word.ToLower()) Then
                        allWords.Add(word.ToLower())
                        one_hot.Add(word.ToLower(), word_index)
                        word_index += 1
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' Add positional encoding to embedded words according to "Attention is all you need"
        ''' </summary>
        ''' <param name="wordEmbeddings"></param>
        ''' <param name="s"></param>
        ''' <param name="sentenceLength"></param>
        Private Sub AddPositionalEncoding(wordEmbeddings As Tensor, s As Integer, sentenceLength As Integer)
            Dim data = wordEmbeddings.Data
            Dim n = EmbeddingSize
            Dim baseIdx = s * SequenceLength * n

            For pos = 0 To sentenceLength - 1
                Dim posBase = baseIdx + pos * n

                For i = 0 To EmbeddingSize - 1
                    Dim pe As Double
                    If i Mod 2 = 0 Then
                        pe = std.Sin(pos / std.Pow(10000, i / EmbeddingSize))
                    Else
                        pe = std.Cos(pos / std.Pow(10000, (i - 1) / EmbeddingSize))
                    End If
                    data(posBase + i) += pe
                Next
            Next
        End Sub

        ''' <summary>
        ''' Configures dropout for the embedding output and draws a new dropout mask.
        ''' </summary>
        ''' <param name="dropoutRate">Dropout rate in <c>[0, 1)</c>.</param>
        ''' <exception cref="ArgumentException">Thrown when the rate is outside <c>[0, 1)</c>.</exception>
        Public Sub SetDropoutNodes(dropoutRate As Double)
            If dropoutRate < 0 OrElse dropoutRate >= 1 Then Throw New ArgumentException("Error: dropout rate must be >= 0 and < 1")

            Me.dropoutRate = dropoutRate

            For i = 0 To EmbeddingSize - 1
                dropoutMask(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask(i) = True
            Next
        End Sub

        ''' <summary>Clears the gradient accumulator of the embedding matrix.</summary>
        Public Sub ZeroGradients()
            embeddingLayerOptimizer.ZeroGrad()
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to the embedding matrix.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            embeddingLayerOptimizer.MakeTrainingStep(learningRate, [step], embeddingLayer)
        End Sub

    End Class
End Namespace

