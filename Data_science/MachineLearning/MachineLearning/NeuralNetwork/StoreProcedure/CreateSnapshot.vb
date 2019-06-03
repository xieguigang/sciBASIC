Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 
    ''' </summary>
    Module CreateSnapshot

        Private Iterator Function GetLayerNodes(layer As Layer) As IEnumerable(Of NeuronNode)
            For Each neuron As Neuron In layer.Neurons
                Yield New NeuronNode With {
                    .bias = neuron.Bias,
                    .delta = neuron.BiasDelta,
                    .gradient = neuron.Gradient,
                    .id = neuron.Guid
                }
            Next
        End Function

        Private Iterator Function GetNodeConnections(layer As Layer) As IEnumerable(Of Synapse)
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
        Private Function GetGuids(layer As Layer) As String()
            Return layer.Neurons _
                .Select(Function(node) node.Guid) _
                .ToArray
        End Function

        ''' <summary>
        ''' Dump the given Neuron <see cref="Network"/> as xml model data
        ''' </summary>
        ''' <param name="instance"></param>
        ''' <returns></returns>
        Public Function TakeSnapshot(instance As Network, errors#) As NeuralNetwork
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
                .errors = errors,
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
    End Module
End Namespace