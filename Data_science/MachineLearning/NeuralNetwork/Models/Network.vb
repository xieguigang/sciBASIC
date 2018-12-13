#Region "Microsoft.VisualBasic::36cbe805a4982e33065e0153c2770c49, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\Network.vb"

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

'     Class Network
' 
'         Properties: HiddenLayer, InputLayer, LearnRate, Momentum, OutputLayer
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: CalculateError, Compute
' 
'         Sub: BackPropagate, ForwardPropagate, (+2 Overloads) Train
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Terminal.ProgressBar

Namespace NeuralNetwork

    ''' <summary>
    ''' https://github.com/trentsartain/Neural-Network
    ''' </summary>
    Public Class Network

#Region "-- Properties --"
        Public Property LearnRate As Double
        Public Property Momentum As Double
        Public Property InputLayer As Layer
        Public Property HiddenLayer As HiddenLayers
        Public Property OutputLayer As Layer

        ''' <summary>
        ''' 激活函数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ActiveFunction As ActiveFunction
#End Region

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="inputSize">``>=2``</param>
        ''' <param name="hiddenSize">``>=2``</param>
        ''' <param name="outputSize">``>=1``</param>
        ''' <param name="learnRate"></param>
        ''' <param name="momentum"></param>
        Public Sub New(inputSize As Integer, hiddenSize As Integer(), outputSize As Integer,
                       Optional learnRate As Double = 0.1,
                       Optional momentum As Double = 0.9,
                       Optional active As IActivationFunction = Nothing)

            Me.LearnRate = learnRate
            Me.Momentum = momentum
            Me.ActiveFunction = (active Or defaultActivation).Store

            InputLayer = New Layer(inputSize, active)
            HiddenLayer = New HiddenLayers(InputLayer, hiddenSize, active)
            OutputLayer = New Layer(outputSize, active, input:=HiddenLayer.Output)
        End Sub

        Public Overrides Function ToString() As String
            Dim summary As New StringBuilder

            Call summary.AppendLine(InputLayer.ToString)
            Call summary.AppendLine(HiddenLayer.ToString)
            Call summary.AppendLine(OutputLayer.ToString)

            Return summary.ToString
        End Function

#Region "-- Training --"
        Public Sub Train(dataSets As List(Of Sample), numEpochs As Integer)
            Using progress As New ProgressBar("Training ANN...")
                Dim tick As New ProgressProvider(numEpochs)

                For i As Integer = 0 To numEpochs - 1
                    For Each dataSet As Sample In dataSets
                        ForwardPropagate(dataSet.status)
                        BackPropagate(dataSet.target)
                    Next

                    Call progress.SetProgress(tick.StepProgress, $"Iterations: [{i}/{numEpochs}]")
                Next
            End Using
        End Sub

        Public Sub Train(dataSets As List(Of Sample), minimumError As Double)
            Dim [error] = 1.0
            Dim numEpochs = 0

            While [error] > minimumError AndAlso numEpochs < Integer.MaxValue
                Dim errors As New List(Of Double)()

                For Each dataSet As Sample In dataSets
                    ForwardPropagate(dataSet.status)
                    BackPropagate(dataSet.target)
                    errors.Add(CalculateError(dataSet.target))
                Next

                [error] = errors.Average()
                numEpochs += 1

                Call $"{numEpochs}\t{(minimumError / [error] * 100).ToString("F2")}%".__DEBUG_ECHO
            End While
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function CalculateError(ParamArray targets As Double()) As Double
            Return OutputLayer _
                .Neurons _
                .Select(Function(n, i)
                            Return Math.Abs(n.CalculateError(targets(i)))
                        End Function) _
                .Sum()
        End Function
#End Region

#Region "ANN compute"

        ''' <summary>
        ''' 这个函数会返回<see cref="OutputLayer"/>
        ''' </summary>
        ''' <param name="inputs"></param>
        ''' <returns></returns>
        Private Function ForwardPropagate(inputs As Double()) As Layer
            Call InputLayer.Input(data:=inputs)
            Call HiddenLayer.ForwardPropagate()
            Call OutputLayer.CalculateValue()

            Return OutputLayer
        End Function

        ''' <summary>
        ''' 反向传播
        ''' </summary>
        ''' <param name="targets"></param>
        Private Sub BackPropagate(ParamArray targets As Double())
            Call OutputLayer.CalculateGradient(targets)
            Call HiddenLayer.BackPropagate(LearnRate, Momentum)
            Call OutputLayer.UpdateWeights(LearnRate, Momentum)
        End Sub

        ''' <summary>
        ''' Compute result output for the neuron network <paramref name="inputs"/>.
        ''' (请注意ANN的输出值是在0-1之间的，所以还需要进行额外的编码和解码)
        ''' </summary>
        ''' <param name="inputs"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Compute(ParamArray inputs As Double()) As Double()
            Return ForwardPropagate(inputs).Output
        End Function
#End Region
    End Class
End Namespace
