Namespace NeuralNetwork

    ''' <summary>
    ''' （神经元的）突触 a connection between two nerve cells
    ''' </summary>
    Public Class Synapse

#Region "-- Properties --"
        Public Property InputNeuron() As Neuron
        Public Property OutputNeuron() As Neuron
        ''' <summary>
        ''' 两个神经元之间的连接强度
        ''' </summary>
        ''' <returns></returns>
        Public Property Weight() As Double
        Public Property WeightDelta() As Double
#End Region

#Region "-- Constructor --"
        Public Sub New(inputNeuron__1 As Neuron, outputNeuron__2 As Neuron)
            InputNeuron = inputNeuron__1
            OutputNeuron = outputNeuron__2
            Weight = Helpers.GetRandom()
        End Sub
#End Region

    End Class
End Namespace
