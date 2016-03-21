Namespace NeuralNetwork

    ''' <summary>
    ''' Tools for training the neuron network
    ''' </summary>
    Public Class TrainingUtils

        Public Property TrainingType As TrainingType = TrainingType.Epoch
        Public ReadOnly Property NeuronNetwork As Network

        ReadOnly _dataSets As New List(Of DataSet)

        Public Sub Encouraging()
            Call Train()
        End Sub

        Public Sub Train()
            Call Helpers.Train(NeuronNetwork, _dataSets, TrainingType)
        End Sub

        Public Sub Corrects(input As Double(), convertedResults As Double(), expectedResults As Double())
            Dim offendingDataSet = _dataSets.FirstOrDefault(Function(x) x.Values.SequenceEqual(input) AndAlso x.Targets.SequenceEqual(convertedResults))
            _dataSets.Remove(offendingDataSet)

            If Not _dataSets.Exists(Function(x) x.Values.SequenceEqual(input) AndAlso x.Targets.SequenceEqual(expectedResults)) Then
                Call _dataSets.Add(New DataSet(input, expectedResults))
            End If

            Call Train()
        End Sub
    End Class
End Namespace