#Region "Microsoft.VisualBasic::fdfaaa354b70a13ceb542dffa60918ad, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\Network.vb"

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


    ' Code Statistics:

    '   Total Lines: 187
    '    Code Lines: 90
    ' Comment Lines: 71
    '   Blank Lines: 26
    '     File Size: 7.50 KB


    '     Class Network
    ' 
    '         Properties: Activations, HiddenLayer, InputLayer, LearnRate, LearnRateDecay
    '                     Momentum, OutputLayer, Truncate
    ' 
    '         Constructor: (+3 Overloads) Sub New
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
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations

Namespace NeuralNetwork

    ''' <summary>
    ''' 人工神经网络计算用的对象模型
    ''' 
    ''' > https://github.com/trentsartain/Neural-Network
    ''' </summary>
    Public Class Network : Inherits Model

#Region "-- Properties --"
        Public Property LearnRate As Double
        Public Property Momentum As Double
        Public Property Truncate As Double = -1

        ''' <summary>
        ''' 通过这个属性可以枚举出所有的输入层的神经元节点
        ''' </summary>
        ''' <returns></returns>
        Public Property InputLayer As Layer
        ''' <summary>
        ''' 通过这个属性可以枚举出所有的隐藏层，然后对每一层隐藏层可以枚举出该隐藏层之中的所有的神经元节点
        ''' </summary>
        ''' <returns></returns>
        Public Property HiddenLayer As HiddenLayers
        ''' <summary>
        ''' 通过这个属性可以枚举出所有的输出层的神经元节点
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputLayer As Layer

        ''' <summary>
        ''' 1 - <see cref="LearnRateDecay"/>
        ''' </summary>
        Dim remains As Double

        ''' <summary>
        ''' 学习率的衰减速率
        ''' </summary>
        ''' <returns></returns>
        Public Property LearnRateDecay As Double
            Get
                Return 1 - remains
            End Get
            Set(value As Double)
                remains = 1 - value
            End Set
        End Property

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
            Me.LearnRateDecay = 0.00000001
            Me.Activations = activations.GetXmlModels
        End Sub

        Friend Sub New(activations As IReadOnlyDictionary(Of String, ActiveFunction))
            Me.LearnRateDecay = 0.00000001
            Me.Activations = activations
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
                       Optional active As LayerActives = Nothing,
                       Optional weightInit As Func(Of Double) = Nothing)

            Dim activations As LayerActives = active Or LayerActives.GetDefaultConfig
            Dim guid As i32 = 100

            weightInit = weightInit Or Helpers.randomWeight

            Me.LearnRate = learnRate
            Me.Momentum = momentum
            Me.Activations = activations.GetXmlModels
            Me.LearnRateDecay = 0.00000001

            InputLayer = New Layer(inputSize, Nothing, weightInit, guid:=guid)
            HiddenLayer = New HiddenLayers(InputLayer, hiddenSize, weightInit, activations.hiddens, guid)
            OutputLayer = New Layer(outputSize, activations.output, weightInit, input:=HiddenLayer.Output, guid:=guid)
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
        ''' <param name="inputs">
        ''' 神经网路的输入层的输入数据,应该都是被归一化为[0,1]或者[-1,1]这两个区间内了的
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function just calculate the network value
        ''' </remarks>
        Public Function ForwardPropagate(inputs As Double(), parallel As Boolean) As Layer
            Call InputLayer.Input(data:=inputs)
            Call HiddenLayer.ForwardPropagate(parallel, Truncate)
            Call OutputLayer.CalculateValue(truncate:=Truncate)

            Return OutputLayer
        End Function

        ''' <summary>
        ''' adjust neuron weight based on the error.(反向传播)
        ''' </summary>
        ''' <param name="targets"></param>
        ''' <remarks>
        ''' 在反向传播之后,网络只会修改节点之间的突触边链接的权重值以及节点
        ''' 的<see cref="Neuron.Gradient"/>值,没有修改节点的<see cref="Neuron.Value"/>值.
        ''' </remarks>
        Public Sub BackPropagate(targets As Double(), parallel As Boolean)
            LearnRate = LearnRate * remains

            Call OutputLayer.CalculateGradient(targets, Truncate)
            Call HiddenLayer.BackPropagate(LearnRate, Momentum, Truncate, parallel)
            Call OutputLayer.UpdateWeights(LearnRate, Momentum, Truncate, parallel)
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
