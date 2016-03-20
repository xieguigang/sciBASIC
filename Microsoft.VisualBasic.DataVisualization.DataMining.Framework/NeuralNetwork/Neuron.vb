Imports System.Collections.Generic
Imports System.Linq

Namespace NeuralNetwork
    Public Class Neuron
#Region "-- Properties --"
        Public Property InputSynapses() As List(Of Synapse)
            Get
                Return m_InputSynapses
            End Get
            Set
                m_InputSynapses = Value
            End Set
        End Property
        Private m_InputSynapses As List(Of Synapse)
        Public Property OutputSynapses() As List(Of Synapse)
            Get
                Return m_OutputSynapses
            End Get
            Set
                m_OutputSynapses = Value
            End Set
        End Property
        Private m_OutputSynapses As List(Of Synapse)
        Public Property Bias() As Double
            Get
                Return m_Bias
            End Get
            Set
                m_Bias = Value
            End Set
        End Property
        Private m_Bias As Double
        Public Property BiasDelta() As Double
            Get
                Return m_BiasDelta
            End Get
            Set
                m_BiasDelta = Value
            End Set
        End Property
        Private m_BiasDelta As Double
        Public Property Gradient() As Double
            Get
                Return m_Gradient
            End Get
            Set
                m_Gradient = Value
            End Set
        End Property
        Private m_Gradient As Double
        Public Property Value() As Double
            Get
                Return m_Value
            End Get
            Set
                m_Value = Value
            End Set
        End Property
        Private m_Value As Double
#End Region

#Region "-- Constructors --"
        Public Sub New()
            InputSynapses = New List(Of Synapse)()
            OutputSynapses = New List(Of Synapse)()
            Bias = Network.GetRandom()
        End Sub

        Public Sub New(inputNeurons As IEnumerable(Of Neuron))
            Me.New()
            For Each inputNeuron As Neuron In inputNeurons
                Dim synapse = New Synapse(inputNeuron, Me)
                inputNeuron.OutputSynapses.Add(synapse)
                InputSynapses.Add(synapse)
            Next
        End Sub
#End Region

#Region "-- Values & Weights --"
        Public Overridable Function CalculateValue() As Double
            Return InlineAssignHelper(Value, Sigmoid.Output(InputSynapses.Sum(Function(a) a.Weight * a.InputNeuron.Value) + Bias))
        End Function

        Public Function CalculateError(target As Double) As Double
            Return target - Value
        End Function

        Public Function CalculateGradient(Optional target As System.Nullable(Of Double) = Nothing) As Double
            If target Is Nothing Then
                Return InlineAssignHelper(Gradient, OutputSynapses.Sum(Function(a) a.OutputNeuron.Gradient * a.Weight) * Sigmoid.Derivative(Value))
            End If

            Return InlineAssignHelper(Gradient, CalculateError(target.Value) * Sigmoid.Derivative(Value))
        End Function

        Public Sub UpdateWeights(learnRate As Double, momentum As Double)
            Dim prevDelta = BiasDelta
            BiasDelta = learnRate * Gradient
            Bias += BiasDelta + momentum * prevDelta

            For Each synapse As Synapse In InputSynapses
                prevDelta = synapse.WeightDelta
                synapse.WeightDelta = learnRate * Gradient * synapse.InputNeuron.Value
                synapse.Weight += synapse.WeightDelta + momentum * prevDelta
            Next
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
#End Region
    End Class
End Namespace
