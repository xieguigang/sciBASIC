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
                        Call .writeCsv(node.id, node.bias, node.delta, node.gradient)
                    Next

                    Call .Flush()
                End With
            End Using

            Using csv As StreamWriter = $"{store}/{edges}".OpenWriter
                With csv
                    Call .writeCsv(NameOf(Synapse.in), NameOf(Synapse.out), NameOf(Synapse.w), NameOf(Synapse.delta))

                    For Each synapse As Synapse In snapshot.connections
                        Call .writeCsv(synapse.in, synapse.out, synapse.w, synapse.delta)
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