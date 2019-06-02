Imports System.IO
Imports System.Runtime.CompilerServices

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
        ''' 返回来的完整的结果可以通过<see cref="IntegralLoader.LoadModel(NeuralNetwork)"/>
        ''' 函数将数据模型来加载为计算模型
        ''' </returns>
        Public Function ScatteredLoader(store As String) As StoreProcedure.NeuralNetwork
            Dim main = $"{store}/{mainPart}".LoadXml(Of StoreProcedure.NeuralNetwork)

            main.inputlayer = $"{store}/{inputLayer}".LoadXml(Of NeuronLayer)
            main.hiddenlayers = $"{store}/{hiddenLayer}".LoadXml(Of StoreProcedure.HiddenLayer)
            main.outputlayer = $"{store}/{outputLayer}".LoadXml(Of NeuronLayer)

            ' 因为下面的数据较大，所以需要使用流的方式进行读取
            ' 节点数据比较小
            ' 可以一次性完全加载
            main.neurons = $"{store}/{nodes}".ReadAllLines _
                .Skip(1) _
                .Select(Function(line) line.Split(","c).parseNode) _
                .ToArray
            main.connections = $"{store}/{edges}".parseEdges.ToArray

            Return main
        End Function

        <Extension>
        Private Iterator Function parseEdges(dataframe As String) As IEnumerable(Of Synapse)
            Dim tokens$()
            Dim edge As Synapse

            Using csv As StreamReader = dataframe.OpenReader
                For Each line As String In csv.IteratesStream.Skip(1)
                    tokens = line.Split(","c)
                    edge = New Synapse With {
                        .[in] = tokens(Scan0),
                        .out = tokens(1),
                        .w = Val(tokens(2)),
                        .delta = Val(tokens(3))
                    }

                    Yield edge
                Next
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
        Public Function ScatteredStore(snapshot As NeuralNetwork, store$) As Boolean
            Dim main As New NeuralNetwork With {
                .errors = snapshot.errors,
                .learnRate = snapshot.learnRate,
                .momentum = snapshot.momentum
            }

            Call snapshot.inputlayer.GetXml.SaveTo($"{store}/{inputLayer}")
            Call snapshot.hiddenlayers.GetXml.SaveTo($"{store}/{hiddenLayer}")
            Call snapshot.outputlayer.GetXml.SaveTo($"{store}/{outputLayer}")

            ' csv file format
            Using csv As StreamWriter = $"{store}/{nodes}".OpenWriter
                With csv
                    Call .writeCsv(NameOf(NeuronNode.id), NameOf(NeuronNode.bias), NameOf(NeuronNode.delta), NameOf(NeuronNode.gradient))

                    For Each node As NeuronNode In snapshot.neurons
                        Call .writeCsv(node.id, node.bias.ToString("G17"), node.delta.ToString("G17"), node.gradient.ToString("G17"))
                    Next

                    Call .Flush()
                End With
            End Using

            Using csv As StreamWriter = $"{store}/{edges}".OpenWriter
                With csv
                    Call .writeCsv(NameOf(Synapse.in), NameOf(Synapse.out), NameOf(Synapse.w), NameOf(Synapse.delta))

                    For Each synapse As Synapse In snapshot.connections
                        Call .writeCsv(synapse.in, synapse.out, synapse.w.ToString("G17"), synapse.delta.ToString("G17"))
                    Next

                    Call .Flush()
                End With
            End Using

            Return main.GetXml.SaveTo($"{store}/{mainPart}")
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