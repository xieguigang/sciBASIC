#Region "Microsoft.VisualBasic::a04555c5e4860a75b9a61edc81a3ee15, Data_science\MachineLearning\NeuralNetwork\Models\Network.vb"

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
    '         Properties: Activations, HiddenLayer, InputLayer, LearnRate, Momentum
    '                     OutputLayer
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CalculateError, Compute, ForwardPropagate, ToString
    ' 
    '         Sub: BackPropagate, (+2 Overloads) Train
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Text

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
        ''' <remarks>
        ''' 这个属性在这里只是起着存储到XML模型之中的作用,并没有实际的计算功能
        ''' </remarks>
        Public ReadOnly Property Activations As IReadOnlyDictionary(Of String, ActiveFunction)
#End Region

        ''' <summary>
        ''' 这个构造函数是给XML模型加载操作所使用的
        ''' </summary>
        ''' <param name="activations"></param>
        Friend Sub New(activations As LayerActives)
            Me.Activations = activations.GetXmlModels
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="inputSize">``>=2``</param>
        ''' <param name="hiddenSize">``>=2``</param>
        ''' <param name="outputSize">``>=1``</param>
        ''' <param name="learnRate"></param>
        ''' <param name="momentum"></param>
        Public Sub New(inputSize%, hiddenSize%(), outputSize%,
                       Optional learnRate# = 0.1,
                       Optional momentum# = 0.9,
                       Optional active As LayerActives = Nothing)

            Dim activations As LayerActives = active Or LayerActives.GetDefaultConfig

            Me.LearnRate = learnRate
            Me.Momentum = momentum
            Me.Activations = activations.GetXmlModels

            InputLayer = New Layer(inputSize, activations.input)
            HiddenLayer = New HiddenLayers(InputLayer, hiddenSize, activations.hiddens)
            OutputLayer = New Layer(outputSize, activations.output, input:=HiddenLayer.Output)
        End Sub

        Public Overrides Function ToString() As String
            Dim summary As New StringBuilder

            Call summary.AppendLine($"learnRate:={LearnRate}")
            Call summary.AppendLine($"momentum:={Momentum}")

            Call summary.AppendLine()
            Call summary.AppendLine("input layer:")
            Call summary.AppendLine("active function using: " & Activations!input.ToString)
            Call summary.AppendLine(InputLayer.ToString)
            Call summary.AppendLine("hiddens layer:")
            Call summary.AppendLine("active function using: " & Activations!hiddens.ToString)
            Call summary.AppendLine(HiddenLayer.ToString)
            Call summary.AppendLine()

            For Each layer As Layer In HiddenLayer
                Call summary.AppendLine($"   {layer.ToString}")
            Next

            Call summary.AppendLine()
            Call summary.AppendLine("output layer:")
            Call summary.AppendLine("active function using: " & Activations!output.ToString)
            Call summary.AppendLine(OutputLayer.ToString)

            Return summary.ToString
        End Function

#Region "-- Training --"
        Public Sub Train(dataSets As Sample(), numEpochs As Integer, Optional parallel As Boolean = False)
            Using progress As New ProgressBar("Training ANN...")
                Dim tick As New ProgressProvider(numEpochs)
                Dim msg$
                Dim errors As New List(Of Double)()

                For i As Integer = 0 To numEpochs - 1
                    For Each dataSet As Sample In dataSets
                        Call ForwardPropagate(dataSet.status, parallel)
                        Call BackPropagate(dataSet.target, parallel)
                        Call errors.Add(CalculateError(dataSet.target))
                    Next

                    msg = $"Iterations: [{i}/{numEpochs}], Err={errors.Average}"
                    progress.SetProgress(tick.StepProgress, msg)
                Next
            End Using
        End Sub

        Public Sub Train(dataSets As Sample(), minimumError As Double, Optional parallel As Boolean = False)
            Dim [error] = 1.0
            Dim numEpochs = 0

            While [error] > minimumError AndAlso numEpochs < Integer.MaxValue
                Dim errors As New List(Of Double)()

                For Each dataSet As Sample In dataSets
                    Call ForwardPropagate(dataSet.status, parallel)
                    Call BackPropagate(dataSet.target, parallel)
                    Call errors.Add(CalculateError(dataSet.target))
                Next

                [error] = errors.Average()
                numEpochs += 1

                Call $"{numEpochs}{ASCII.TAB}Error:={[error]}{ASCII.TAB}progress:={((minimumError / [error]) * 100).ToString("F2")}%".__DEBUG_ECHO
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
        Private Function ForwardPropagate(inputs As Double(), parallel As Boolean) As Layer
            Call InputLayer.Input(data:=inputs)
            Call HiddenLayer.ForwardPropagate(parallel)
            Call OutputLayer.CalculateValue()

            Return OutputLayer
        End Function

        ''' <summary>
        ''' 反向传播
        ''' </summary>
        ''' <param name="targets"></param>
        Private Sub BackPropagate(targets As Double(), parallel As Boolean)
            Call OutputLayer.CalculateGradient(targets)
            Call HiddenLayer.BackPropagate(LearnRate, Momentum, parallel)
            Call OutputLayer.UpdateWeights(LearnRate, Momentum, parallel)
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
            Return ForwardPropagate(inputs, parallel:=False).Output
        End Function
#End Region
    End Class
End Namespace
