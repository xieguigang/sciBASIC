#Region "Microsoft.VisualBasic::8bec5cce2b1485587680a0ffb22f0bdd, Data_science\MachineLearning\MachineLearning\NeuralNetwork\StoreProcedure\NeuralNetwork.vb"

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

    '     Class NeuralNetwork
    ' 
    '         Properties: connections, hiddenlayers, inputlayer, learnRate, momentum
    '                     neurons, outputlayer
    ' 
    '         Function: GetGuids, GetLayerNodes, GetNodeConnections, GetPredictLambda, Snapshot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' Xml文件存储格式
    ''' </summary>
    <XmlRoot("NeuralNetwork", [Namespace]:="http://machinelearning.scibasic.net/ANN/")>
    Public Class NeuralNetwork : Inherits XmlDataModel

        Public Property learnRate As Double
        Public Property momentum As Double

        Public Property neurons As NeuronNode()
        Public Property connections As Synapse()
        Public Property inputlayer As NeuronLayer
        Public Property outputlayer As NeuronLayer
        Public Property hiddenlayers As HiddenLayer

        ''' <summary>
        ''' 输入一个样本信息然后输出分类结果预测
        ''' 
        ''' 这个函数假设目标样本输入是在当前的这个<paramref name="normalize"/>矩阵的范围之中的
        ''' </summary>
        ''' <param name="normalize">进行所输入的样本数据的归一化的矩阵</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPredictLambda(normalize As NormalizeMatrix) As Func(Of Sample, Double())
            With Me.LoadModel
                Return Function(sample)
                           Return .Compute(normalize.NormalizeInput(sample))
                       End Function
            End With
        End Function

        Private Shared Iterator Function GetLayerNodes(layer As Layer) As IEnumerable(Of NeuronNode)
            For Each neuron As Neuron In layer.Neurons
                Yield New NeuronNode With {
                    .bias = neuron.Bias,
                    .delta = neuron.BiasDelta,
                    .gradient = neuron.Gradient,
                    .id = neuron.Guid
                }
            Next
        End Function

        Private Shared Iterator Function GetNodeConnections(layer As Layer) As IEnumerable(Of Synapse)
            For Each neuron As Neuron In layer.Neurons
                For Each edge In neuron.PopulateAllSynapses
                    Yield New Synapse With {
                        .[in] = edge.InputNeuron.Guid,
                        .out = edge.OutputNeuron.Guid,
                        .w = edge.Weight,
                        .delta = edge.WeightDelta
                    }
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetGuids(layer As Layer) As String()
            Return layer.Neurons _
                .Select(Function(node) node.Guid) _
                .ToArray
        End Function

        ''' <summary>
        ''' Dump the given Neuron <see cref="Network"/> as xml model data
        ''' </summary>
        ''' <param name="instance"></param>
        ''' <returns></returns>
        Public Shared Function Snapshot(instance As Network) As NeuralNetwork
            Dim nodes As New List(Of NeuronNode)
            Dim hiddenlayers As New List(Of NeuronLayer)
            Dim inputlayer As String()
            Dim outputlayer As String()
            Dim connections As New List(Of Synapse)

            ' nodes
            nodes += GetLayerNodes(instance.InputLayer)
            inputlayer = GetGuids(instance.InputLayer)

            For Each layer As SeqValue(Of Layer) In instance.HiddenLayer.SeqIterator
                nodes += GetLayerNodes(layer)
                hiddenlayers += New NeuronLayer With {
                    .id = layer.i + 1,
                    .neurons = GetGuids(layer)
                }
            Next

            nodes += GetLayerNodes(instance.OutputLayer)
            outputlayer = GetGuids(instance.OutputLayer)

            ' edges
            connections += GetNodeConnections(instance.InputLayer)

            For Each layer As Layer In instance.HiddenLayer
                connections += GetNodeConnections(layer)
            Next

            connections += GetNodeConnections(instance.OutputLayer)
            connections = connections _
                .GroupBy(Function(n) $"{n.in} = {n.out}") _
                .Select(Function(g) g.First) _
                .AsList

            Return New NeuralNetwork With {
                .learnRate = instance.LearnRate,
                .momentum = instance.Momentum,
                .neurons = nodes,
                .hiddenlayers = New HiddenLayer With {
                    .activation = instance.Activations!hiddens,
                    .layers = hiddenlayers
                },
                .inputlayer = New NeuronLayer With {
                    .id = "input",
                    .neurons = inputlayer,
                    .activation = instance.Activations!input
                },
                .outputlayer = New NeuronLayer With {
                    .id = "output",
                    .neurons = outputlayer,
                    .activation = instance.Activations!output
                },
                .connections = connections
            }
        End Function
    End Class
End Namespace
