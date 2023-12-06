Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions.distance
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nlp.encode

Namespace nlp


    ''' <summary>
    ''' Created by kenny on 5/23/14.
    ''' </summary>
    Public Class WordDictionary

        ' a unique word vector for each word (storing a 1xN matrix)
        Private wordVectorsField As IDictionary(Of String, DenseMatrix) = New Dictionary(Of String, DenseMatrix)()

        Private wordEncoder As WordEncoder = New DiscreteRandomWordEncoder()

        Public Sub New()
        End Sub

        ' load a list of new line separated words
        Public Sub New(file As String)
            load(file)
        End Sub

        Private Sub load(file As String)
            Dim lines = readLines(file)
            For Each line In lines
                If line.StartsWith("#", StringComparison.Ordinal) Then
                    Continue For
                End If ' ignore comments
                add(line)
            Next
        End Sub

        Public Overridable Sub add(word As String)
            ' LOGGER.info("adding: " + word);
            If wordVectorsField.ContainsKey(word) Then
                Return
            End If

            wordVectorsField(word) = wordEncoder.encode(word)
        End Sub

        Public Overridable Function contains(word As String) As Boolean
            Return wordVectorsField.ContainsKey(word)
        End Function

        Public Overridable Function getVector(word As String) As DenseMatrix
            Return wordVectorsField(word)
        End Function

        Public Overridable ReadOnly Property WordVectors As IList(Of DenseMatrix)
            Get
                Return New List(Of DenseMatrix)(wordVectorsField.Values)
            End Get
        End Property

        Public Overridable Function buildSentence(wordVectors As IList(Of DenseMatrix)) As String
            Dim words As IList(Of String) = New List(Of String)()
            For Each wordVector In wordVectors
                words.Add(getClosestWord(wordVector))
            Next
            Return words.JoinBy("-")
        End Function

        ' TODO speed up
        Public Overridable Function getClosestWord(wordVector As DenseMatrix) As String

            Dim distanceFunction As DistanceFunction = New EuclideanDistanceFunction()

            Dim closest As String = Nothing
            Dim minDistance = Double.MaxValue
            For Each entry As KeyValuePair(Of String, DenseMatrix) In wordVectorsField
                Dim distance = distanceFunction.distance(entry.Value, wordVector)
                If distance < minDistance Then
                    minDistance = distance
                    closest = entry.Key
                End If
            Next
            Return closest
        End Function

        Public Overridable Function size() As Integer
            Return wordVectorsField.Count
        End Function

        Private Function readLines(file As String) As IList(Of String)
            Return file.IterateAllLines().ToList()
        End Function

    End Class

End Namespace
