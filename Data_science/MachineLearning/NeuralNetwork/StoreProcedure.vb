Imports System.Runtime.CompilerServices
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
            Dim hiddenLayer As Dictionary(Of String, Neuron)() = model.hiddenlayers _
                .layers _
                .OrderBy(Function(layer) layer.id) _
                .Select(Function(layer)
                            Return layer _
                                .createNeurons(activations.output, neuronDataTable) _
                                .ToDictionary(Function(n) n.Name,
                                              Function(n) n.Value)
                        End Function) _
                .ToArray

            ' 构建神经元之间的链接
            Dim neurons As New 

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