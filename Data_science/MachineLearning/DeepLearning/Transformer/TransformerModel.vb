Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.Transformer.Utils

Namespace Transformer
    ''' <summary>
    ''' Transformer architecture as described in "Attention is all you need"
    ''' </summary>
    Public Class TransformerModel
        Private sequenceLength As Integer
        Private dropout As Double

        Private encoder As EncoderStack
        Private decoder As DecoderStack
        Private englishEmbedding As Embedding
        Private spanishEmbedding As Embedding
        Private outputLayer As OutputLayer

        Private loss As Rev

        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer, batchSize As Integer, dropout As Double, allEnglishSentences As List(Of List(Of String)), allSpanishSentences As List(Of List(Of String)))
            Me.dropout = dropout

            InsertStartAndStopCharacters(allSpanishSentences)
            sequenceLength = CalculateSequenceLength(allEnglishSentences, allSpanishSentences)

            englishEmbedding = New Embedding(embeddingSize, sequenceLength, allEnglishSentences)
            spanishEmbedding = New Embedding(embeddingSize, sequenceLength, allSpanishSentences)
            encoder = New EncoderStack(Nx, embeddingSize, dk, dv, h, dff)
            decoder = New DecoderStack(Nx, embeddingSize, dk, dv, h, dff)
            outputLayer = New OutputLayer(sequenceLength, embeddingSize, spanishEmbedding.DictionarySize)
        End Sub

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
                        SetDropoutNodes()
                        Dim __ As List(Of List(Of String)) = Nothing
                        Dim loss = Me.Translate(batchSize, True, englishSentences, spanishSentences, __)
                        Console.WriteLine("Step: " & [step].ToString() & ", loss: " & loss.ToString())
                        MakeTrainingStep(learningRate, [step])
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

                Translate(1, False, englishSentence, Nothing, translatedSpanishSentence)
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
        ''' Translate a batch of sentenses by generating one word at a time for each sentence with the decoder until max 
        ''' length or stopping character.
        ''' </summary>
        Private Function Translate(batchSize As Integer, isTraining As Boolean, englishSentences As List(Of List(Of String)), correctSpanishSentences As List(Of List(Of String)), <Out> ByRef translatedSpanishSentences As List(Of List(Of String))) As Double
            loss = New Rev(0.0)
            Call Checkpoints.Instance.ClearCheckpoints()

            Dim english_word_embeddings = englishEmbedding.Embed(englishSentences, isTraining)
            Dim encoderOutput = encoder.Encode(english_word_embeddings, isTraining)

            translatedSpanishSentences = InitializeSpanishSentences(batchSize)
            Dim nrWords = If(isTraining, CalculateMaxSentenceLength(correctSpanishSentences), sequenceLength)
            For w = 1 To nrWords - 1
                Dim spanish_word_embeddings = spanishEmbedding.Embed(translatedSpanishSentences, isTraining)
                Dim decoder_output = decoder.Decode(encoderOutput, spanish_word_embeddings, isTraining)
                Dim output = outputLayer.Output(decoder_output)

                If isTraining Then spanishEmbedding.CalculateLossFunction(output, correctSpanishSentences, w, loss)

                Dim spanishWordIndexes As Integer() = output.GetMaxIndex()
                Dim spanishWords = spanishEmbedding.GetWords(spanishWordIndexes)
                AddWordsToSentences(batchSize, isTraining, spanishWords, translatedSpanishSentences)
            Next

            If isTraining Then loss /= sequenceLength * batchSize

            Return loss

        End Function

        Private Sub SetDropoutNodes()
            spanishEmbedding.SetDropoutNodes(dropout)
            englishEmbedding.SetDropoutNodes(dropout)
            encoder.SetDropoutNodes(dropout)
            decoder.SetDropoutNodes(dropout)
        End Sub

        Private Sub CalculateGradient()
            loss.CalculateDerivative(1)
            Call Checkpoints.Instance.CalculateCheckpointGradients()
        End Sub

        Private Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            CalculateGradient()

            englishEmbedding.MakeTrainingStep(learningRate, [step])
            spanishEmbedding.MakeTrainingStep(learningRate, [step])
            encoder.MakeTrainingStep(learningRate, [step])
            decoder.MakeTrainingStep(learningRate, [step])
            outputLayer.MakeTrainingStep(learningRate, [step])
        End Sub


    End Class
End Namespace
