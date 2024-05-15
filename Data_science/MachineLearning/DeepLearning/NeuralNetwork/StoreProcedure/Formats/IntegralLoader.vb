#Region "Microsoft.VisualBasic::0c079b3ff789758c7faa3dab764fcb99, Data_science\MachineLearning\DeepLearning\NeuralNetwork\StoreProcedure\Formats\IntegralLoader.vb"

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

    '   Total Lines: 179
    '    Code Lines: 132
    ' Comment Lines: 22
    '   Blank Lines: 25
    '     File Size: 7.81 KB


    '     Module IntegralLoader
    ' 
    '         Function: addLink, createNeuronBuckets, createNeurons, LoadModel
    '         Class neuronLoader
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports ANN = Microsoft.VisualBasic.MachineLearning.NeuralNetwork

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 与<see cref="CreateSnapshot"/>之中的快照生成函数执行相反的操作,从模型数据文件之中创建计算用的对象模型
    ''' </summary>
    <HideModuleName>
    Public Module IntegralLoader

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' 在人工神经网络之中, 神经元节点并不是太多, 数量上产生影响的是神经元之间的链接对象
        ''' </remarks>
        Private Class neuronLoader

            Public inputLayer As Dictionary(Of String, Neuron)
            Public outputLayer As Dictionary(Of String, Neuron)
            Public hiddenLayer As List(Of Dictionary(Of String, Neuron))

            Default Public ReadOnly Property GetById(id As String) As Neuron
                Get
                    SyncLock inputLayer
                        If inputLayer.ContainsKey(id) Then
                            Return inputLayer(id)
                        End If
                    End SyncLock
                    SyncLock outputLayer
                        If outputLayer.ContainsKey(id) Then
                            Return outputLayer(id)
                        End If
                    End SyncLock
                    SyncLock hiddenLayer
                        For Each layer In hiddenLayer
                            If layer.ContainsKey(id) Then
                                Return layer(id)
                            End If
                        Next
                    End SyncLock

                    Throw New InvalidProgramException("Network data model was broken!")
                End Get
            End Property

        End Class

        <Extension>
        Private Function createNeuronBuckets(model As StoreProcedure.NeuralNetwork, activations As LayerActives, mute As Boolean) As neuronLoader
            Dim neuronDataTable = model.neurons.ToDictionary(Function(n) n.id)

            Call "Start to create neuron nodes...".__DEBUG_ECHO(mute:=mute)

            Dim inputLayer As Dictionary(Of String, Neuron) = model.inputlayer _
                .createNeurons(Nothing, neuronDataTable) _
                .ToDictionary(Function(n) n.Name,
                              Function(n) n.Value)
            Dim outputLayer As Dictionary(Of String, Neuron) = model.outputlayer _
                .createNeurons(activations.output, neuronDataTable) _
                .ToDictionary(Function(n) n.Name,
                              Function(n) n.Value)
            Dim hiddenLayer = model.hiddenlayers _
                .layers _
                .OrderBy(Function(layer) layer.id) _
                .Select(Function(layer)
                            Return layer _
                                .createNeurons(activations.output, neuronDataTable) _
                                .ToDictionary(Function(n) n.Name,
                                              Function(n) n.Value)
                        End Function) _
                .AsList

            ' 构建神经元之间的链接
            ' Dim neurons As New BucketDictionary(Of String, Neuron)(hiddenLayer + inputLayer + outputLayer)
            Dim loader As New neuronLoader With {
                .hiddenLayer = hiddenLayer,
                .inputLayer = inputLayer,
                .outputLayer = outputLayer', .neuronBucket = neurons
            }

            Call "Job done!".__INFO_ECHO(silent:=mute)

            Return loader
        End Function

        <Extension>
        Private Function addLink(edge As Synapse, neurons As neuronLoader) As Integer
            Dim inNeuron As Neuron = neurons(edge.in)
            Dim outNeuron As Neuron = neurons(edge.out)
            Dim output As New ANN.Synapse(inNeuron, outNeuron) With {
                .Weight = edge.w,
                .WeightDelta = edge.delta
            }
            Dim input As New ANN.Synapse(inNeuron, outNeuron) With {
                .Weight = edge.w,
                .WeightDelta = edge.delta
            }

            SyncLock inNeuron
                inNeuron.OutputSynapses.Add(output)
            End SyncLock
            SyncLock outNeuron
                outNeuron.InputSynapses.Add(input)
            End SyncLock

            Return 0
        End Function

        ''' <summary>
        ''' 将保存在Xml文件之中的已经训练好的模型加载为人工神经网络对象用来进行后续的分析操作
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个加载器函数主要是针对小文件使用的
        ''' </remarks>
        <Extension>
        Public Function LoadModel(model As StoreProcedure.NeuralNetwork, Optional mute As Boolean = False) As Network
            Dim activations As LayerActives = LayerActives.FromXmlModel(
                functions:=New Dictionary(Of String, ActiveFunction) From {
                    {"input", model.inputlayer.activation},
                    {"output", model.outputlayer.activation},
                    {"hiddens", model.hiddenlayers.activation}
                })
            Dim neurons As neuronLoader = model.createNeuronBuckets(activations, mute)

            Call "Create neuron synapse links in parallel...".__DEBUG_ECHO(mute:=mute)

            ' The size of edge links between the neuron nodes in ANN network is huge
            ' parallel can make this process fast
            ' maybe
            Call model.connections _
                .GroupBy(Function(edge) $"{edge.in} = {edge.out}") _
                .AsParallel _
                .Select(Function(edge) edge.First) _
                .Select(Function(edge) edge.addLink(neurons)) _
                .ToArray

            Call "Job done!".__INFO_ECHO(silent:=mute)

            Return New Network(activations) With {
                .LearnRate = model.learnRate,
                .Momentum = model.momentum,
                .InputLayer = New Layer(neurons.inputLayer.Values.ToArray),
                .OutputLayer = New Layer(neurons.outputLayer.Values.ToArray),
                .HiddenLayer = New HiddenLayers(neurons.hiddenLayer.Select(Function(c) New Layer(c.Values.ToArray)))
            }
        End Function

        <Extension>
        Private Iterator Function createNeurons(layer As NeuronLayer,
                                                active As IActivationFunction,
                                                neuronDataTable As Dictionary(Of String, NeuronNode)) As IEnumerable(Of NamedValue(Of Neuron))

            Dim null As Func(Of Double) = Function() 0

            For Each id As String In layer.neurons
                Dim data As NeuronNode = neuronDataTable(id)
                Dim neuron As New Neuron(null, active) With {
                    .Bias = data.bias,
                    .BiasDelta = data.delta,
                    .Gradient = data.gradient
                }

                Yield New NamedValue(Of Neuron) With {
                    .Name = id,
                    .Value = neuron
                }
            Next
        End Function
    End Module
End Namespace
