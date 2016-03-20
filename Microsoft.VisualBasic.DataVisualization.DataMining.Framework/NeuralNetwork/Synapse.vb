Namespace NeuralNetwork
    Public Class Synapse
#Region "-- Properties --"
        Public Property InputNeuron() As Neuron
            Get
                Return m_InputNeuron
            End Get
            Set
                m_InputNeuron = Value
            End Set
        End Property
        Private m_InputNeuron As Neuron
        Public Property OutputNeuron() As Neuron
            Get
                Return m_OutputNeuron
            End Get
            Set
                m_OutputNeuron = Value
            End Set
        End Property
        Private m_OutputNeuron As Neuron
        Public Property Weight() As Double
            Get
                Return m_Weight
            End Get
            Set
                m_Weight = Value
            End Set
        End Property
        Private m_Weight As Double
        Public Property WeightDelta() As Double
            Get
                Return m_WeightDelta
            End Get
            Set
                m_WeightDelta = Value
            End Set
        End Property
        Private m_WeightDelta As Double
#End Region

#Region "-- Constructor --"
        Public Sub New(inputNeuron__1 As Neuron, outputNeuron__2 As Neuron)
            InputNeuron = inputNeuron__1
            OutputNeuron = outputNeuron__2
            Weight = Network.GetRandom()
        End Sub
#End Region
    End Class
End Namespace
