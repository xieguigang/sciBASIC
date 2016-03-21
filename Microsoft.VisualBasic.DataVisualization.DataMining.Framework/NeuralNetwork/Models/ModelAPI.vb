Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork.Models

    ''' <summary>
    ''' 网络可视化工具
    ''' </summary>
    Public Module NetworkModelAPI

        <Extension> Public Function VisualizeModel(net As Network) As FileStream.Network
            Dim network As New FileStream.Network
            Dim hash = (New List(Of Neuron) + net.HiddenLayer.ToArray + net.InputLayer.ToArray + net.OutputLayer.ToArray).SeqIterator _
                .ToDictionary(Function(x) x.obj,
                              Function(x) x.Pos)

            network += net.HiddenLayer.ToArray(Function(x) x.__node(NameOf(net.HiddenLayer), hash))
            network += net.InputLayer.ToArray(Function(x) x.__node(NameOf(net.InputLayer), hash))
            network += net.OutputLayer.ToArray(Function(x) x.__node(NameOf(net.OutputLayer), hash))

            network += net.HiddenLayer.ToArray(Function(x) x.__edges(NameOf(net.HiddenLayer), hash)).MatrixAsIterator
            network += net.InputLayer.ToArray(Function(x) x.__edges(NameOf(net.InputLayer), hash)).MatrixAsIterator
            network += net.OutputLayer.ToArray(Function(x) x.__edges(NameOf(net.OutputLayer), hash)).MatrixAsIterator

            Return network
        End Function

        <Extension>
        Private Function __node(neuron As Neuron, type As String, uidhash As Dictionary(Of Neuron, Integer)) As FileStream.Node
            Dim uid As String = uidhash(neuron).ToString
            Return New FileStream.Node With {
                .Identifier = uid,
                .NodeType = type
            }
        End Function

        <Extension>
        Private Function __edges(neuron As Neuron, type As String, uidHash As Dictionary(Of Neuron, Integer)) As FileStream.NetworkEdge()
            Dim LQuery = (From c As Synapse
                          In neuron.InputSynapses
                          Where c.Weight <> 0R  ' 忽略掉没有链接强度的神经元链接
                          Let itName As String = $"{type}-{NameOf(neuron.InputSynapses)}"
                          Select c.__synapse(itName, uidHash)).ToList + (From c As Synapse
                                                                         In neuron.OutputSynapses
                                                                         Where c.Weight <> 0R
                                                                         Select c.__synapse(type & "-" & NameOf(neuron.OutputSynapses), uidHash))
            Return LQuery.ToArray
        End Function

        <Extension>
        Private Function __synapse(synapse As Synapse, type As String, uidHash As Dictionary(Of Neuron, Integer)) As FileStream.NetworkEdge
            Return New FileStream.NetworkEdge With {
                .Confidence = synapse.Weight,
                .FromNode = CStr(uidHash(synapse.InputNeuron)),
                .ToNode = CStr(uidHash(synapse.OutputNeuron)),
                .InteractionType = type
            }
        End Function
    End Module
End Namespace