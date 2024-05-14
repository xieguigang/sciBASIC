#Region "Microsoft.VisualBasic::d24b98fe3d16ba62414111fa114dcf46, Data_science\MachineLearning\DeepLearning\NeuralNetwork\StoreProcedure\Formats\Scattered.vb"

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

    '   Total Lines: 166
    '    Code Lines: 110
    ' Comment Lines: 28
    '   Blank Lines: 28
    '     File Size: 6.83 KB


    '     Module Scattered
    ' 
    '         Function: parseEdges, parseNode, ScatteredLoader, ScatteredStore
    ' 
    '         Sub: writeCsv
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 将模型快照数据分散为多个文件进行保存和读取的存储过程
    ''' </summary>
    Public Module Scattered

        Const mainPart$ = "main.Xml"
        Const inputLayer$ = "layers/input.Xml"
        Const hiddenLayer$ = "layers/hidden.Xml"
        Const outputLayer$ = "layers/output.Xml"

        Const nodes$ = "network/nodes.csv"
        Const edges$ = "network/edges.csv"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="store">
        ''' 模型文件所存储的文件夹的路径
        ''' </param>
        ''' <returns>
        ''' 返回来的完整的结果可以通过<see cref="IntegralLoader.LoadModel(NeuralNetwork,Boolean)"/>
        ''' 函数将数据模型来加载为计算模型
        ''' </returns>
        Public Function ScatteredLoader(store As IFileSystemEnvironment, Optional mute As Boolean = False) As StoreProcedure.NeuralNetwork
            Dim main = store.ReadAllText($"/{mainPart}").LoadFromXml(Of StoreProcedure.NeuralNetwork)
            Dim previousMuteConfig As Boolean = VBDebugger.Mute

            VBDebugger.Mute = mute

            Call "Load network parts...".__DEBUG_ECHO

            main.inputlayer = store.ReadAllText($"/{inputLayer}").LoadFromXml(Of NeuronLayer)
            main.hiddenlayers = store.ReadAllText($"/{hiddenLayer}").LoadFromXml(Of StoreProcedure.HiddenLayer)
            main.outputlayer = store.ReadAllText($"/{outputLayer}").LoadFromXml(Of NeuronLayer)

            Call "Load neuron nodes...".__DEBUG_ECHO

            ' 因为下面的数据较大，所以需要使用流的方式进行读取
            ' 节点数据比较小
            ' 可以一次性完全加载
            main.neurons = store.ReadAllText($"/{nodes}") _
                .LineTokens _
                .Skip(1) _
                .AsParallel _
                .Select(Function(line) line.Split(","c).parseNode) _
                .ToArray

            Call "Load neuron synapse edges...".__DEBUG_ECHO

            main.connections = store _
                .OpenFile($"/{edges}", FileMode.Open, FileAccess.Read) _
                .parseEdges _
                .ToArray

            Call "Load neuron network model success!".__INFO_ECHO

            VBDebugger.Mute = previousMuteConfig

            Return main
        End Function

        <Extension>
        Private Function parseEdges(dataframe As Stream) As IEnumerable(Of Synapse)
            Using csv As New StreamReader(dataframe)
                Return csv.IteratesStream _
                    .Skip(1) _
                    .AsParallel _
                    .Select(Function(line)
                                Dim tokens = line.Split(","c)
                                Dim edge As New Synapse With {
                                    .[in] = tokens(Scan0),
                                    .out = tokens(1),
                                    .w = Val(tokens(2)),
                                    .delta = Val(tokens(3))
                                }

                                Return edge
                            End Function) _
                    .ToArray
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function parseNode(tokens As String()) As NeuronNode
            Return New NeuronNode With {
                .id = tokens(Scan0),
                .bias = Val(tokens(1)),
                .delta = Val(tokens(2)),
                .gradient = Val(tokens(3))
            }
        End Function

        ''' <summary>
        ''' 将一个超大的网络快照以分散文件的形式保存在一个给定的文件夹之中
        ''' </summary>
        ''' <param name="snapshot"></param>
        ''' <param name="store">A directory path for save the network snapshot.</param>
        ''' <returns></returns>
        <Extension>
        Public Function ScatteredStore(snapshot As NeuralNetwork, store As IFileSystemEnvironment) As Boolean
            Dim main As New NeuralNetwork With {
                .errors = snapshot.errors,
                .learnRate = snapshot.learnRate,
                .momentum = snapshot.momentum
            }

            Call store.WriteText(snapshot.inputlayer.GetXml, $"/{inputLayer}")
            Call store.WriteText(snapshot.hiddenlayers.GetXml, $"/{hiddenLayer}")
            Call store.WriteText(snapshot.outputlayer.GetXml, $"/{outputLayer}")

            ' csv file format
            Using csv As New StreamWriter(store.OpenFile($"/{nodes}", FileMode.OpenOrCreate, FileAccess.Write))
                With csv
                    Call .writeCsv(
                        NameOf(NeuronNode.id),
                        NameOf(NeuronNode.bias),
                        NameOf(NeuronNode.delta),
                        NameOf(NeuronNode.gradient)
                    )

                    For Each node As NeuronNode In snapshot.neurons
                        Call .writeCsv(
                            node.id,
                            node.bias.ToString("G17"),
                            node.delta.ToString("G17"),
                            node.gradient.ToString("G17")
                        )
                    Next

                    Call .Flush()
                End With
            End Using

            Using csv As New StreamWriter(store.OpenFile($"/{edges}", FileMode.OpenOrCreate, FileAccess.Write))
                With csv
                    Call .writeCsv(NameOf(Synapse.in), NameOf(Synapse.out), NameOf(Synapse.w), NameOf(Synapse.delta))

                    For Each synapse As Synapse In snapshot.connections
                        Call .writeCsv(synapse.in, synapse.out, synapse.w.ToString("G17"), synapse.delta.ToString("G17"))
                    Next

                    Call .Flush()
                End With
            End Using

            Return store.WriteText(main.GetXml, $"/{mainPart}")
        End Function

        ''' <summary>
        ''' 为了减少模块间的引用,并且由于神经元节点和突触链接对象的结构都非常简单,所以在这里就直接以这个拓展函数来写文件了
        ''' </summary>
        ''' <param name="csv"></param>
        ''' <param name="columns"></param>
        <Extension>
        Private Sub writeCsv(csv As StreamWriter, ParamArray columns As String())
            Call csv.WriteLine(columns.JoinBy(","))
        End Sub
    End Module
End Namespace
