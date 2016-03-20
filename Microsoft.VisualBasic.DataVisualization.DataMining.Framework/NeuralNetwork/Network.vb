Imports System.Collections.Generic
Imports System.Linq

Namespace NeuralNetwork
    Public Class Network
#Region "-- Properties --"
        Public Property LearnRate() As Double
            Get
                Return m_LearnRate
            End Get
            Set
                m_LearnRate = Value
            End Set
        End Property
        Private m_LearnRate As Double
        Public Property Momentum() As Double
            Get
                Return m_Momentum
            End Get
            Set
                m_Momentum = Value
            End Set
        End Property
        Private m_Momentum As Double
        Public Property InputLayer() As List(Of Neuron)
            Get
                Return m_InputLayer
            End Get
            Set
                m_InputLayer = Value
            End Set
        End Property
        Private m_InputLayer As List(Of Neuron)
        Public Property HiddenLayer() As List(Of Neuron)
            Get
                Return m_HiddenLayer
            End Get
            Set
                m_HiddenLayer = Value
            End Set
        End Property
        Private m_HiddenLayer As List(Of Neuron)
        Public Property OutputLayer() As List(Of Neuron)
            Get
                Return m_OutputLayer
            End Get
            Set
                m_OutputLayer = Value
            End Set
        End Property
        Private m_OutputLayer As List(Of Neuron)
#End Region

#Region "-- Globals --"
        Private Shared ReadOnly Random As New Random()
#End Region

#Region "-- Constructor --"
        Public Sub New(inputSize As Integer, hiddenSize As Integer, outputSize As Integer, Optional learnRate__1 As System.Nullable(Of Double) = Nothing, Optional momentum__2 As System.Nullable(Of Double) = Nothing)
            LearnRate = If(learnRate__1, 0.4)
            Momentum = If(momentum__2, 0.9)
            InputLayer = New List(Of Neuron)()
            HiddenLayer = New List(Of Neuron)()
            OutputLayer = New List(Of Neuron)()

            For i As Integer = 0 To inputSize - 1
                InputLayer.Add(New Neuron())
            Next

            For i As Integer = 0 To hiddenSize - 1
                HiddenLayer.Add(New Neuron(InputLayer))
            Next

            For i As Integer = 0 To outputSize - 1
                OutputLayer.Add(New Neuron(HiddenLayer))
            Next
        End Sub
#End Region

#Region "-- Training --"
        Public Sub Train(dataSets As List(Of DataSet), numEpochs As Integer)
            For i As Integer = 0 To numEpochs - 1
                For Each DataSet As NeuralNetwork.DataSet In dataSets
                    ForwardPropagate(DataSet.Values)
                    BackPropagate(DataSet.Targets)
                Next
            Next
        End Sub

        Public Sub Train(dataSets As List(Of DataSet), minimumError As Double)
            Dim [error] = 1.0
            Dim numEpochs = 0

            While [error] > minimumError AndAlso numEpochs < Integer.MaxValue
                Dim errors = New List(Of Double)()
                For Each dataSet As NeuralNetwork.DataSet In dataSets
                    ForwardPropagate(dataSet.Values)
                    BackPropagate(dataSet.Targets)
                    errors.Add(CalculateError(dataSet.Targets))
                Next
                [error] = errors.Average()
                numEpochs += 1
            End While
        End Sub

        Private Sub ForwardPropagate(ParamArray inputs As Double())
            For i As Integer = 0 To inputs.Length - 1
                InputLayer(i).Value = inputs(i)
            Next

            HiddenLayer.ForEach(Function(a) a.CalculateValue())
            OutputLayer.ForEach(Function(a) a.CalculateValue())
        End Sub

        Private Sub BackPropagate(ParamArray targets As Double())
            For i As Integer = 0 To targets.Length - 1
                OutputLayer(i).CalculateGradient(targets(i))
            Next

            HiddenLayer.ForEach(Function(a) a.CalculateGradient())
            HiddenLayer.ForEach(Sub(a) a.UpdateWeights(LearnRate, Momentum))
            OutputLayer.ForEach(Sub(a) a.UpdateWeights(LearnRate, Momentum))
        End Sub

        Public Function Compute(ParamArray inputs As Double()) As Double()
            ForwardPropagate(inputs)
            Return OutputLayer.[Select](Function(a) a.Value).ToArray()
        End Function

        Private Function CalculateError(ParamArray targets As Double()) As Double
            Dim sum As Double = 0
            Dim i As Integer = 0
            For Each a In OutputLayer
                sum += Math.Abs(a.CalculateError(targets(i)))
                i += 1
            Next
            Return sum
        End Function
#End Region

#Region "-- Helpers --"
        Public Shared Function GetRandom() As Double
            Return 2 * Random.NextDouble() - 1
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
#End Region
    End Class

#Region "-- Enum --"
    Public Enum TrainingType
        Epoch
        MinimumError
    End Enum
#End Region
End Namespace
