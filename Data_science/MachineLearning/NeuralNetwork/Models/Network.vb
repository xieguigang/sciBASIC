#Region "Microsoft.VisualBasic::9ef41e1ebef365810b0030fc71625602, Data_science\MachineLearning\NeuralNetwork\Models\Network.vb"

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
    '         Function: Compute, ForwardPropagate, ToString
    ' 
    '         Sub: BackPropagate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork

    ''' <summary>
    ''' https://github.com/trentsartain/Neural-Network
    ''' </summary>
    Public Class Network : Inherits Model

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
        ''' <remarks>
        ''' 会在创建的时候赋值一个guid
        ''' </remarks>
        Public Sub New(inputSize%, hiddenSize%(), outputSize%,
                       Optional learnRate# = 0.1,
                       Optional momentum# = 0.9,
                       Optional active As LayerActives = Nothing)

            Dim activations As LayerActives = active Or LayerActives.GetDefaultConfig
            Dim guid As int = 100

            Me.LearnRate = learnRate
            Me.Momentum = momentum
            Me.Activations = activations.GetXmlModels

            InputLayer = New Layer(inputSize, activations.input, guid:=guid)
            HiddenLayer = New HiddenLayers(InputLayer, hiddenSize, activations.hiddens, guid)
            OutputLayer = New Layer(outputSize, activations.output, input:=HiddenLayer.Output, guid:=guid)
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

#Region "ANN compute"

        ''' <summary>
        ''' 这个函数会返回<see cref="OutputLayer"/>
        ''' </summary>
        ''' <param name="inputs"></param>
        ''' <returns></returns>
        Public Function ForwardPropagate(inputs As Double(), parallel As Boolean) As Layer
            Call InputLayer.Input(data:=inputs)
            Call HiddenLayer.ForwardPropagate(parallel)
            Call OutputLayer.CalculateValue()

            Return OutputLayer
        End Function

        ''' <summary>
        ''' 反向传播
        ''' </summary>
        ''' <param name="targets"></param>
        Public Sub BackPropagate(targets As Double(), truncate As Double, parallel As Boolean)
            LearnRate = LearnRate * 0.999999
            Momentum = 1 - LearnRate

            Call OutputLayer.CalculateGradient(targets, truncate)
            Call HiddenLayer.BackPropagate(LearnRate, Momentum, truncate, parallel)
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
