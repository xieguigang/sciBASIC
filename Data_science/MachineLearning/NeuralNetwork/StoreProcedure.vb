Imports System.Runtime.CompilerServices
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
            Dim inputLayer As Neuron() = model.inputlayer.createNeurons(activations.input, neuronDataTable).ToArray
            Dim outputLayer As Neuron() = model.outputlayer.createNeurons(activations.output, neuronDataTable).ToArray

            Return New Network(activations) With {
                .LearnRate = model.learnRate,
                .Momentum = model.momentum,
                .InputLayer = New Layer(inputLayer),
                .OutputLayer = New Layer(outputLayer)
            }
        End Function

        <Extension>
        Private Iterator Function createNeurons(layer As NeuronLayer, active As IActivationFunction, neuronDataTable As Dictionary(Of String, NeuronNode)) As IEnumerable(Of Neuron)
            For Each id As String In layer.neurons
                Dim data As NeuronNode = neuronDataTable(id)
                Dim neuron As New Neuron(active)

                Yield neuron
            Next
        End Function
    End Module
End Namespace