#Region "Microsoft.VisualBasic::91fd0f9eaa4c0b944c343e903bd2f92a, Data_science\MachineLearning\NeuralNetwork\Models\Neuron.vb"

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

'     Class Neuron
' 
'         Properties: Bias, BiasDelta, Gradient, InputSynapses, OutputSynapses
'                     Value
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: CalculateError, CalculateGradient, CalculateValue, ToString, UpdateWeights
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations

Namespace NeuralNetwork

    ''' <summary>
    ''' 神经元对象模型
    ''' </summary>
    Public Class Neuron

#Region "-- Properties --"

        ''' <summary>
        ''' 这个神经元对象和上一层神经元之间的突触链接列表
        ''' </summary>
        ''' <returns></returns>
        Public Property InputSynapses As Synapse()
        ''' <summary>
        ''' 这个神经元对象和下一层神经元之间的突触链接列表
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputSynapses As Synapse()
        Public Property Bias As Double
        Public Property BiasDelta As Double
        Public Property Gradient As Double
        ''' <summary>
        ''' 神经元之间传递的值，即预测的输出结果
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 在进行计算的时候，value会被替换为样本的输入值，所以value不需要被存储到Xml之中
        ''' 在进行加速计算的时候也不需要被考虑到
        ''' </remarks>
        Public Property Value As Double

        Public ReadOnly Property Guid As String

        ''' <summary>
        ''' The active function
        ''' </summary>
        Dim activation As IActivationFunction
#End Region

#Region "-- Constructors --"

        ''' <summary>
        ''' 创建的神经链接是空的
        ''' </summary>
        ''' <param name="active"><see cref="Sigmoid"/> as default</param>
        Public Sub New(Optional active As IActivationFunction = Nothing, Optional id As int = Nothing)
            InputSynapses = {}
            OutputSynapses = {}
            Bias = Helpers.GetRandom()
            Value = Helpers.GetRandom
            BiasDelta = Helpers.GetRandom
            activation = active Or defaultActivation

            If Not id Is Nothing Then
                Guid = (id + 1).Hex
            Else
                Guid = GetHashCode().ToHexString
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="inputNeurons"></param>
        ''' <param name="active"><see cref="Sigmoid"/> as default</param>
        Public Sub New(inputNeurons As IEnumerable(Of Neuron), Optional active As IActivationFunction = Nothing, Optional guid As int = Nothing)
            Call Me.New(active, guid)

            For Each inputNeuron As Neuron In inputNeurons
                Dim synapse As New Synapse(inputNeuron, Me)
                inputNeuron.OutputSynapses.Add(synapse)
                InputSynapses.Add(synapse)
            Next
        End Sub
#End Region

        Public Overrides Function ToString() As String
            Return $"bias={Bias}, bias_delta={BiasDelta}, gradient={Gradient}, value={Value}"
        End Function

#Region "-- Values & Weights --"

        ''' <summary>
        ''' 计算分类预测结果
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 赋值给<see cref="Value"/>,然后返回<see cref="Value"/>
        ''' </remarks>
        Public Overridable Function CalculateValue() As Double
            ' 在这里的计算过程为:
            ' 使用突触链接的权重值乘上当该突触的上游输入节点的训练值 + 误差
            ' 使用遗传算法进行优化的时候,可以将bias设置零
            ' 用遗传算法生成的应该是网络的拓扑结构,而网络的拓扑结构是和突触链接的权重相关的
            Value = activation.Function(InputSynapses.Sum(Function(a) a.Weight * a.InputNeuron.Value) + Bias)
            Return Value
        End Function

        ''' <summary>
        ''' 计算当前的结果和测试结果数据之间的误差大小
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CalculateError(target As Double) As Double
            Return target - Value
        End Function

        Public Function CalculateGradient(target As Double) As Double
            Gradient = CalculateError(target) * activation.Derivative(Value)
            Return Gradient
        End Function

        Public Function CalculateGradient() As Double
            Gradient = OutputSynapses.Sum(Function(a) a.OutputNeuron.Gradient * a.Weight) * activation.Derivative(Value)
            Return Gradient
        End Function

        ''' <summary>
        ''' 调用这个函数会修改突触链接权重，dias误差值等
        ''' </summary>
        ''' <param name="learnRate"></param>
        ''' <param name="momentum"></param>
        ''' <returns></returns>
        Public Function UpdateWeights(learnRate As Double, momentum As Double) As Integer
            Dim prevDelta = BiasDelta
            BiasDelta = learnRate * Gradient
            Bias += BiasDelta + momentum * prevDelta

            For Each synapse As Synapse In InputSynapses
                prevDelta = synapse.WeightDelta
                synapse.WeightDelta = learnRate * Gradient * synapse.InputNeuron.Value
                synapse.Weight += synapse.WeightDelta + momentum * prevDelta
            Next

            Return 0
        End Function
#End Region
    End Class
End Namespace
