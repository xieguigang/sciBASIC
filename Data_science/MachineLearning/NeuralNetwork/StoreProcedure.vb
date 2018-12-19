Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork

    Public Module StoreProcedureExtensions

        ''' <summary>
        ''' 将保存在Xml文件之中的已经训练好的模型加载为人工神经网络对象用来进行后续的分析操作
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadModel(model As StoreProcedure.NeuralNetwork) As Network
            Dim activations As LayerActives = LayerActives.FromXmlModel(
                functions:=New Dictionary(Of String, ActiveFunction) From {
                    {"input", model.inputlayer.activation},
                    {"output", model.outputlayer.activation},
                    {"hiddens", model.hiddenlayers.activation}
                })
            Dim neuronDataTable = model.neurons.ToDictionary(Function(n) n.id)
            Dim inputLayer As Dictionary(Of String, Neuron) = model.inputlayer _
                .createNeurons(activations.input, neuronDataTable) _
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
            Dim neurons As New BucketDictionary(Of String, Neuron)(hiddenLayer + inputLayer + outputLayer)
            Dim connectedLinks As New Index(Of String)

            For Each edge As StoreProcedure.Synapse In model.connections
                If connectedLinks.IndexOf($"{edge.in} = {edge.out}") = -1 Then
                    Dim inNeuron As Neuron = neurons(edge.in)
                    Dim outNeuron As Neuron = neurons(edge.out)
                    Dim output As New Synapse(inNeuron, outNeuron) With {
                        .Weight = edge.w,
                        .WeightDelta = edge.delta
                    }
                    Dim input As New Synapse(inNeuron, outNeuron) With {
                        .Weight = edge.w,
                        .WeightDelta = edge.delta
                    }

                    inNeuron.OutputSynapses.Add(output)
                    outNeuron.InputSynapses.Add(input)
                End If
            Next

            Return New Network(activations) With {
                .LearnRate = model.learnRate,
                .Momentum = model.momentum,
                .InputLayer = New Layer(inputLayer.Values.ToArray),
                .OutputLayer = New Layer(outputLayer.Values.ToArray),
                .HiddenLayer = New HiddenLayers(hiddenLayer.Select(Function(c) New Layer(c.Values.ToArray)))
            }
        End Function

        <Extension>
        Private Iterator Function createNeurons(layer As NeuronLayer, active As IActivationFunction, neuronDataTable As Dictionary(Of String, NeuronNode)) As IEnumerable(Of NamedValue(Of Neuron))
            For Each id As String In layer.neurons
                Dim data As NeuronNode = neuronDataTable(id)
                Dim neuron As New Neuron(active) With {
                    .Bias = data.bias,
                    .BiasDelta = data.delta,
                    .Gradient = data.gradient,
                    .Value = data.value
                }

                Yield New NamedValue(Of Neuron) With {
                    .Name = id,
                    .Value = neuron
                }
            Next
        End Function
    End Module
End Namespace