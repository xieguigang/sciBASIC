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

        Sub New(net As Network)
            NeuronNetwork = net
        End Sub

        Public Sub RemoveLast()
            If Not _dataSets.Count = 0 Then
                Call _dataSets.RemoveLast
            End If
        End Sub

        Public Sub Add(input As Double(), output As Double())
            Call _dataSets.Add(New DataSet(input, output))
        End Sub

        Public Sub Add(x As DataSet)
            Call _dataSets.Add(x)
        End Sub

        Public Sub Train()
            Call Helpers.Train(NeuronNetwork, _dataSets, TrainingType)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input">The inputs data</param>
        ''' <param name="convertedResults">The error outputs</param>
        ''' <param name="expectedResults">The corrects output</param>
        Public Sub Corrects(input As Double(), convertedResults As Double(), expectedResults As Double(), Optional train As Boolean = True)
            Dim offendingDataSet As DataSet = _dataSets.FirstOrDefault(Function(x) x.Values.SequenceEqual(input) AndAlso x.Targets.SequenceEqual(convertedResults))
            _dataSets.Remove(offendingDataSet)

            If Not _dataSets.Exists(Function(x) x.Values.SequenceEqual(input) AndAlso x.Targets.SequenceEqual(expectedResults)) Then
                Call _dataSets.Add(New DataSet(input, expectedResults))
            End If

            If train Then
                Call Me.Train()
            End If
        End Sub

        Public Sub Corrects(dataset As DataSet, expectedResults As Double(), Optional train As Boolean = True)
            Call Corrects(dataset.Values, dataset.Targets, expectedResults, train)
        End Sub
    End Class
End Namespace