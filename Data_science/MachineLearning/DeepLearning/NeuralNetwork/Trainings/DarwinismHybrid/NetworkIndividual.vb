#Region "Microsoft.VisualBasic::b8a255cd8ca52b239ddaa429ce09e4cf, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\DarwinismHybrid\NetworkIndividual.vb"

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

    '   Total Lines: 204
    '    Code Lines: 154 (75.49%)
    ' Comment Lines: 10 (4.90%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 40 (19.61%)
    '     File Size: 7.90 KB


    '     Class NetworkIndividual
    ' 
    '         Properties: MutationRate, UniqueHashKey
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Clone, copyLayer, Crossover, GetNodeTable, (+2 Overloads) Mutate
    ' 
    '         Sub: Crossover
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace NeuralNetwork.DarwinismHybrid

    Public Class NetworkIndividual : Implements Chromosome(Of NetworkIndividual), ICloneable

        Friend target As Network

        Dim guid As String

        Public Property MutationRate As Double Implements Chromosome(Of NetworkIndividual).MutationRate

        Public ReadOnly Property UniqueHashKey As String Implements Chromosome(Of NetworkIndividual).Identity
            Get
                If guid Is Nothing Then
                    guid = (target.GetHashCode.ToString & Now.ToString).MD5
                End If

                Return guid
            End Get
        End Property

        Sub New()

        End Sub

        Sub New(network As Network)
            target = network
        End Sub

        Private Function copyLayer(layer As Layer) As Layer
            Dim neurons As Neuron() = layer.Neurons _
                .Select(Function(a)
                            Return New Neuron(Function() 0, a.activation, a.Guid) With {
                                .Bias = a.Bias,
                                .BiasDelta = a.BiasDelta,
                                .Gradient = a.Gradient,
                                .isDroppedOut = a.isDroppedOut,
                                .Value = a.Value
                            }
                        End Function) _
                .ToArray
            Dim copy As New Layer(neurons) With {
                .doDropOutMode = layer.doDropOutMode,
                .softmaxNormalization = layer.softmaxNormalization
            }

            Return copy
        End Function

        Private Shared Function GetNodeTable(network As Network) As Dictionary(Of String, Neuron)
            Dim neurons As New Dictionary(Of String, Neuron)

            For Each node In network.InputLayer
                neurons.Add(node.Guid, node)
            Next

            For Each node In network.OutputLayer
                neurons.Add(node.Guid, node)
            Next

            For Each hidden In network.HiddenLayer
                For Each node In hidden
                    neurons.Add(node.Guid, node)
                Next
            Next

            Return neurons
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim copy As New Network(target.Activations) With {
                .LearnRate = target.LearnRate,
                .Momentum = target.Momentum,
                .Truncate = target.Truncate,
                .LearnRateDecay = target.LearnRateDecay,
                .InputLayer = copyLayer(target.InputLayer),
                .OutputLayer = copyLayer(target.OutputLayer),
                .HiddenLayer = New HiddenLayers(target.HiddenLayer.Select(AddressOf copyLayer))
            }

            ' create neuron links
            Dim copyTable = GetNodeTable(copy)
            Dim rawTable = GetNodeTable(target)
            Dim linkUnit As Synapse

            For Each node As Neuron In copyTable.Values
                Dim raw As Neuron = rawTable(node.Guid)

                For Each link As Synapse In raw.InputSynapses.JoinIterates(raw.OutputSynapses)
                    linkUnit = New Synapse With {
                        .Weight = link.Weight,
                        .WeightDelta = link.WeightDelta,
                        .InputNeuron = copyTable(link.InputNeuron.Guid),
                        .OutputNeuron = copyTable(link.OutputNeuron.Guid)
                    }
                    node.InputSynapses.Add(linkUnit)
                Next
            Next

            Return New NetworkIndividual With {
                .MutationRate = MutationRate,
                .target = copy
            }
        End Function

        Private Overloads Sub Crossover(x As Layer, y As Layer)
            Dim n As Integer = CInt(x.Count / 4)
            Dim link1, link2 As Synapse
            Dim wtemp, deltatemp As Double

            For j As Integer = 0 To x.Count - 1
                Dim i As Integer = randf.NextInteger(x.Count)
                Dim nodeX = x.Neurons(i)
                Dim nodeY = x.Neurons(i)

                If Not nodeX.InputSynapses.IsNullOrEmpty Then
                    i = randf.NextInteger(nodeX.InputSynapses.Count)
                    link1 = nodeX.InputSynapses(i)
                    link2 = nodeY.InputSynapses(i)

                    wtemp = link2.Weight
                    deltatemp = link2.Weight

                    link2.Weight = link1.Weight
                    link2.WeightDelta = link1.WeightDelta
                    link1.Weight = wtemp
                    link1.WeightDelta = deltatemp
                End If
                If Not nodeX.OutputSynapses.IsNullOrEmpty Then
                    i = randf.NextInteger(nodeX.OutputSynapses.Count)
                    link1 = nodeX.OutputSynapses(i)
                    link2 = nodeY.OutputSynapses(i)

                    wtemp = link2.Weight
                    deltatemp = link2.Weight

                    link2.Weight = link1.Weight
                    link2.WeightDelta = link1.WeightDelta
                    link1.Weight = wtemp
                    link1.WeightDelta = deltatemp
                End If
            Next
        End Sub

        ''' <summary>
        ''' 进行若干连接的权重值的交换
        ''' </summary>
        ''' <param name="another"></param>
        ''' <returns></returns>
        Public Overloads Iterator Function Crossover(another As NetworkIndividual) As IEnumerable(Of NetworkIndividual) Implements Chromosome(Of NetworkIndividual).Crossover
            Dim copyX As NetworkIndividual = Me.Clone
            Dim copyY As NetworkIndividual = another.Clone

            Call Crossover(copyX.target.InputLayer, copyY.target.InputLayer)
            Call Crossover(copyX.target.OutputLayer, copyY.target.OutputLayer)

            For i As Integer = 0 To copyX.target.HiddenLayer.Count - 1
                Call Crossover(copyX.target.HiddenLayer.Layers(i), copyY.target.HiddenLayer.Layers(i))
            Next

            Yield copyX
            Yield copyY
        End Function

        Private Overloads Shared Function Mutate(target As Layer) As Neuron
            Dim neuron As Neuron = target.Neurons(randf.NextInteger(target.Count))
            Dim link As Synapse

            If Not neuron.InputSynapses.IsNullOrEmpty Then
                link = neuron.InputSynapses(randf.NextInteger(neuron.InputSynapses.Count))
                link.Weight += randf.randf(-1, 1)
                link.WeightDelta += randf.randf(-1, 1)
            End If
            If Not neuron.OutputSynapses.IsNullOrEmpty Then
                link = neuron.OutputSynapses(randf.NextInteger(neuron.OutputSynapses.Count))
                link.Weight += randf.randf(-1, 1)
                link.WeightDelta += randf.randf(-1, 1)
            End If

            Return neuron
        End Function

        ''' <summary>
        ''' 进行若干连接阶段权重值的修改
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Mutate() As NetworkIndividual Implements Chromosome(Of NetworkIndividual).Mutate
            Dim copy As NetworkIndividual = DirectCast(Clone(), NetworkIndividual)
            Dim neuron As Network = copy.target

            Call Mutate(neuron.InputLayer)
            Call Mutate(neuron.OutputLayer)

            For Each layer In neuron.HiddenLayer
                Call Mutate(layer)
            Next

            Return copy
        End Function
    End Class
End Namespace
