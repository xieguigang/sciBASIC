Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' Xml文件存储格式
    ''' </summary>
    Public Class NeuralNetwork

        Public Property ActiveFunction As ActiveFunction
        Public Property LearnRate As Double
        Public Property Momentum As Double

        Public Property neurons As NeuronNode()
        Public Property connections As Synapse()
        Public Property inputlayer As TermsVector
        Public Property outputlayer As TermsVector
        Public Property hiddenlayers As TermsVector()

        Private Shared Iterator Function GetLayerNodes(layer As Layer, hash2Uid As Dictionary(Of Neuron, String), id As Uid) As IEnumerable(Of NeuronNode)
            Dim guid$

            For Each neuron As Neuron In layer.Neurons
                guid = (++id).PadLeft(6, "0"c)
                hash2Uid(neuron) = guid

                Yield New NeuronNode With {
                    .Bias = neuron.Bias,
                    .BiasDelta = neuron.BiasDelta,
                    .Gradient = neuron.Gradient,
                    .ID = guid,
                    .Value = neuron.Value
                }
            Next
        End Function

        Private Shared Iterator Function GetNodeConnections(layer As Layer, hash2Uid As Dictionary(Of Neuron, String)) As IEnumerable(Of Synapse)
            For Each neuron As Neuron In layer.Neurons
                For Each edge In neuron.PopulateAllSynapses
                    Yield New Synapse With {
                        .InputNeuron = hash2Uid(edge.InputNeuron),
                        .OutputNeuron = hash2Uid(edge.OutputNeuron),
                        .Weight = edge.Weight,
                        .WeightDelta = edge.WeightDelta
                    }
                Next
            Next
        End Function

        Private Shared Function GetGuids(layer As Layer, hash2Uid As Dictionary(Of Neuron, String)) As String()
            Return layer.Neurons _
                .Select(Function(node) hash2Uid(node)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Dump the given Neuron <see cref="Network"/> as xml model data
        ''' </summary>
        ''' <param name="instance"></param>
        ''' <returns></returns>
        Public Shared Function Snapshot(instance As Network) As NeuralNetwork
            Dim id As New Uid
            Dim hash2Uid As New Dictionary(Of Neuron, String)
            Dim nodes As New List(Of NeuronNode)
            Dim hiddenlayers As New List(Of TermsVector)
            Dim inputlayer As String()
            Dim outputlayer As String()
            Dim connections As New List(Of Synapse)

            ' nodes
            nodes += GetLayerNodes(instance.InputLayer, hash2Uid, id)
            inputlayer = GetGuids(instance.InputLayer, hash2Uid)

            For Each layer As Layer In instance.HiddenLayer.Layers
                nodes += GetLayerNodes(layer, hash2Uid, id)
                hiddenlayers += New TermsVector With {
                    .Terms = GetGuids(layer, hash2Uid)
                }
            Next

            nodes += GetLayerNodes(instance.OutputLayer, hash2Uid, id)
            outputlayer = GetGuids(instance.OutputLayer, hash2Uid)

            ' edges
            connections += GetNodeConnections(instance.InputLayer, hash2Uid)

            For Each layer As Layer In instance.HiddenLayer.Layers
                connections += GetNodeConnections(layer, hash2Uid)
            Next

            connections += GetNodeConnections(instance.OutputLayer, hash2Uid)

            Return New NeuralNetwork With {
                .ActiveFunction = instance.ActiveFunction,
                .LearnRate = instance.LearnRate,
                .Momentum = instance.Momentum,
                .neurons = nodes,
                .hiddenlayers = hiddenlayers,
                .inputlayer = New TermsVector With {.Terms = inputlayer},
                .outputlayer = New TermsVector With {.Terms = outputlayer},
                .connections = connections
            }
        End Function
    End Class
End Namespace