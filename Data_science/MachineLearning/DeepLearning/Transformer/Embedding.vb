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

        Public ReadOnly Property DictionarySize As Integer
            Get
                Return one_hot.Count
            End Get
        End Property

        Public Property EmbeddingSize As Integer
            Get
                Return _EmbeddingSize
            End Get
            Private Set(value As Integer)
                _EmbeddingSize = value
            End Set
        End Property

        Public Property SequenceLength As Integer
            Get
                Return _SequenceLength
            End Get
            Private Set(value As Integer)
                _SequenceLength = value
            End Set
        End Property

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="embeddingSize"></param>
        ''' <param name="sequenceLength"></param>
        ''' <param name="sentences"></param>
        Public Sub New(embeddingSize As Integer, sequenceLength As Integer, sentences As List(Of List(Of String)))
            Me.EmbeddingSize = embeddingSize
            Me.SequenceLength = sequenceLength

            OneHotEmbedding(sentences)
            embeddingLayer = New Tensor(DictionarySize, Me.EmbeddingSize)
            embeddingLayer.GenerateNormalRandomValues()

            embeddingLayerOptimizer = New Optimizer(embeddingLayer)

            dropoutMask = New Boolean(embeddingSize - 1) {}
        End Sub

        ''' <summary>
        ''' Multiply the one-hot embeddings with the embedding layer to project onto a smaller space
        ''' </summary>
        ''' <param name="sentences"></param>
        ''' <param name="isTraining"></param>
        ''' <returns></returns>
        Public Function Embed(sentences As List(Of List(Of String)), isTraining As Boolean) As Tensor
            Dim batchSize = sentences.Count
            Dim wordEmbeddings As Tensor = New Tensor(batchSize, SequenceLength, EmbeddingSize)

            Dim s = 0
            For Each sentence In sentences
                Dim word_count = 0
                For Each word In sentence
                    ' No need for matrix multiplication since only one element of vector is nonzero
                    Dim pos As Integer = one_hot(word.ToLower())
                    For i = 0 To EmbeddingSize - 1
                        wordEmbeddings(s, word_count, i) = embeddingLayer(pos, i)
                    Next
                    word_count += 1
                Next

                AddPositionalEncoding(wordEmbeddings, s, sentence.Count())
                s += 1
            Next

            If isTraining AndAlso dropoutRate > 0 Then wordEmbeddings = wordEmbeddings.Dropout(dropoutMask, dropoutRate)

            Return wordEmbeddings
        End Function

        ''' <summary>
        ''' Cross entropy loss function between the correct word in a sentence and the decoder output word.
        ''' For a batch of several sentences the loss is accumulated.
        ''' </summary>
        ''' <param name="filteredOutout"></param>
        ''' <param name="correctSpanishSentences"></param>
        ''' <param name="w"></param>
        ''' <param name="loss"></param>
        Public Sub CalculateLossFunction(filteredOutout As Tensor, correctSpanishSentences As List(Of List(Of String)), w As Integer, ByRef loss As Rev)
            For s = 0 To correctSpanishSentences.Count() - 1
                If w >= correctSpanishSentences(s).Count() Then Continue For

                Dim correctWord = correctSpanishSentences(s)(w)

                Dim ind = GetWordIndex(correctWord)
                loss -= filteredOutout(s, 0, ind).Log()
            Next
        End Sub

        ''' <summary>
        ''' Get a word based on its index in the dictionary
        ''' </summary>
        ''' <param name="indexes"></param>
        ''' <returns></returns>
        Public Function GetWords(indexes As Integer()) As String()
            Dim words = New String(indexes.Length - 1) {}
            For s = 0 To indexes.Length - 1
                words(s) = allWords(indexes(s))
            Next

            Return words
        End Function

        ''' <summary>
        ''' Get the index of a specific word in a dictionary
        ''' </summary>
        ''' <param name="word"></param>
        ''' <returns></returns>
        Public Function GetWordIndex(word As String) As Integer
            Return one_hot(word)
        End Function

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
                        one_hot.Add(word.ToLower(), std.Min(Threading.Interlocked.Increment(word_index), word_index - 1))
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
            For pos = 0 To sentenceLength - 1
                For i = 0 To EmbeddingSize - 1
                    Dim pe As Double
                    If i Mod 2 = 0 Then
                        pe = std.Sin(pos / std.Pow(10000, i / EmbeddingSize))
                    Else
                        pe = std.Cos(pos / std.Pow(10000, (i - 1) / EmbeddingSize))
                    End If
                    wordEmbeddings(s, pos, i) += pe
                Next
            Next
        End Sub

        Public Sub SetDropoutNodes(dropoutRate As Double)
            If dropoutRate < 0 OrElse dropoutRate >= 1 Then Throw New ArgumentException("Error: dropout rate must be >= 0 and < 1")

            Me.dropoutRate = dropoutRate

            For i = 0 To EmbeddingSize - 1
                dropoutMask(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask(i) = True
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            embeddingLayerOptimizer.MakeTrainingStep(learningRate, [step], embeddingLayer)
        End Sub

    End Class
End Namespace
