Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Connector = Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Synapse

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 对于大型的神经网络而言，反复的构建XML数据模型将会额外的消耗掉大量的时间，导致训练的时间过长
    ''' 在这里通过这个持久性的快照对象来减少这种反复创建XML数据快照的问题
    ''' </summary>
    Public Class Snapshot

        ReadOnly snapshot As NeuralNetwork
        ReadOnly source As Network

        ''' <summary>
        ''' 节点的数量较少
        ''' </summary>
        ReadOnly neuronLinks As Map(Of Neuron, NeuronNode)()
        ''' <summary>
        ''' 因为链接的数量非常多，可能会超过了一个数组的元素数量上限，
        ''' 所以在这里使用多个分组来避免这个问题
        ''' </summary>
        ReadOnly synapseLinks As Map(Of Connector, Synapse)()()

        Sub New(model As Network)
            source = model
            snapshot = CreateSnapshot.TakeSnapshot(model, 0)
            neuronLinks = createNeuronUpdateMaps(source, snapshot).ToArray
            synapseLinks = createSynapseUpdateMaps(source, snapshot).ToArray
        End Sub

        Private Shared Iterator Function createNeuronUpdateMaps(source As Network, snapshot As NeuralNetwork) As IEnumerable(Of Map(Of Neuron, NeuronNode))
            Dim neuronTable = snapshot.neurons.ToDictionary(Function(n) n.id)

            For Each node As Neuron In source.InputLayer
                Yield New Map(Of Neuron, NeuronNode) With {
                    .Key = node,
                    .Maps = neuronTable(node.Guid)
                }
            Next

            For Each layer In source.HiddenLayer
                For Each node As Neuron In layer
                    Yield New Map(Of Neuron, NeuronNode) With {
                        .Key = node,
                        .Maps = neuronTable(node.Guid)
                    }
                Next
            Next

            For Each node As Neuron In source.OutputLayer
                Yield New Map(Of Neuron, NeuronNode) With {
                    .Key = node,
                    .Maps = neuronTable(node.Guid)
                }
            Next
        End Function

        Private Shared Iterator Function createSynapseUpdateMaps(model As Network, snapshot As NeuralNetwork) As IEnumerable(Of Map(Of Connector, Synapse)())
            Dim linkTable As BucketDictionary(Of String, Synapse) = snapshot _
                .connections _
                .CreateBuckets(Function(s) $"{s.in}|{s.out}")
            Dim populateConnector = Iterator Function(node As Neuron) As IEnumerable(Of Map(Of Connector, Synapse))
                                        If Not node.InputSynapses Is Nothing Then
                                            For Each link In node.InputSynapses
                                                Yield New Map(Of Connector, Synapse) With {
                                                    .Key = link,
                                                    .Maps = linkTable($"{link.InputNeuron.Guid}|{link.OutputNeuron.Guid}")
                                                }
                                            Next
                                        End If
                                    End Function

            Yield model.InputLayer _
                .Select(populateConnector) _
                .IteratesALL _
                .ToArray

            For Each layer As Layer In model.HiddenLayer
                For Each block In layer _
                    .Select(populateConnector) _
                    .IteratesALL _
                    .SplitIterator(100000)

                    Yield block
                Next
            Next

            Yield model.OutputLayer _
                .Select(populateConnector) _
                .IteratesALL _
                .ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="error">
        ''' The calculation errors of current snapshot.
        ''' </param>
        ''' <returns></returns>
        Public Function UpdateSnapshot([error] As Double) As Snapshot
            Dim toNode As NeuronNode
            Dim fromNode As Neuron

            snapshot.errors = [error]
            snapshot.learnRate = source.LearnRate
            snapshot.momentum = source.Momentum

            ' update node and links in current neuron network
            For Each node In neuronLinks
                toNode = node.Maps
                fromNode = node.Key

                toNode.bias = fromNode.Bias
                toNode.delta = fromNode.BiasDelta
                toNode.gradient = fromNode.Gradient
                toNode.id = fromNode.Guid
            Next

            Dim toLink As Synapse
            Dim fromLink As Connector

            For Each layer In synapseLinks
                For Each connection In layer
                    toLink = connection.Maps
                    fromLink = connection.Key

                    toLink.delta = fromLink.WeightDelta
                    toLink.w = fromLink.Weight
                Next
            Next

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteIntegralXML(path As String) As Boolean
            Return snapshot.GetXml.SaveTo(path, throwEx:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteScatteredParts(directory As String) As Boolean
            Return snapshot.ScatteredStore(store:=directory)
        End Function

        Public Overrides Function ToString() As String
            Return source.ToString
        End Function
    End Class
End Namespace