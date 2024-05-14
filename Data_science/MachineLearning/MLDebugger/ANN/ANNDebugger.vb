#Region "Microsoft.VisualBasic::90a38f4530f12696707401b45a55eb0c, Data_science\MachineLearning\MLDebugger\ANN\ANNDebugger.vb"

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

    '   Total Lines: 248
    '    Code Lines: 196
    ' Comment Lines: 15
    '   Blank Lines: 37
    '     File Size: 11.43 KB


    ' Class ANNDebugger
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: createLocationTable
    ' 
    '     Sub: Save, WriteCDF, writeErrors, WriteFrame, writeIndex
    '          writeNodeBias, writeUnixtime, writeWeight
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.DarwinismHybrid
Imports StoreModel = Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure.NeuralNetwork

Public Class ANNDebugger

    Dim networkFrames As BinaryDataWriter
    Dim biasFrames As BinaryDataWriter
    Dim errorFrames As BinaryDataWriter
    Dim timeFrames As BinaryDataWriter

    ' index和error可能不需要临时文件，但是如果迭代的次数长达几十万次的话
    ' 则使用临时文件会比较安全一些

    Dim frameTemp$
    Dim errorTemp$
    Dim timesTemp$
    Dim biasTemp$

    Dim synapses As Synapse()
    Dim neurons As Neuron()
    Dim minErr# = 99999
    Dim snapShotTemp$

    Sub New(model As NeuralNetwork.Network)
        frameTemp = TempFileSystem.GetAppSysTempFile(".bin", App.PID)
        errorTemp = TempFileSystem.GetAppSysTempFile(".bin", App.PID)
        timesTemp = TempFileSystem.GetAppSysTempFile(".bin", App.PID)
        biasTemp = TempFileSystem.GetAppSysTempFile(".bin", App.PID)
        snapShotTemp = TempFileSystem.GetAppSysTempFile(".Xml", App.PID)

        networkFrames = New BinaryDataWriter(frameTemp.Open(doClear:=True))
        errorFrames = New BinaryDataWriter(errorTemp.Open(doClear:=True))
        timeFrames = New BinaryDataWriter(timesTemp.Open(doClear:=True))
        biasFrames = New BinaryDataWriter(biasTemp.Open(doClear:=True))

        synapses = model _
            .GetSynapseGroups _
            .Select(Function(g) g.First) _
            .ToArray
        neurons = model.InputLayer.AsList _
            + model.HiddenLayer.GetAllNeurons _
            + model.OutputLayer
    End Sub

    ''' <summary>
    ''' 将一个迭代的状态数据写入临时文件作为调试器的一帧
    ''' </summary>
    ''' <param name="iteration%"></param>
    ''' <param name="error#"></param>
    ''' <param name="model"></param>
    Public Sub WriteFrame(iteration%, error#, model As NeuralNetwork.Network)
        Call errorFrames.Write([error])
        Call networkFrames.Write(synapses.Select(Function(s) s.Weight).ToArray)
        Call biasFrames.Write(neurons.Select(Function(n) n.Bias).ToArray)
        Call timeFrames.Write(App.ElapsedMilliseconds)

        If [error] < minErr Then
            minErr = [error]
            StoreModel _
                .Snapshot(model) _
                .GetXml _
                .SaveTo(snapShotTemp)
        End If
    End Sub

    ''' <summary>
    ''' 将调试器的临时数据保存到给定的CDF文件之中
    ''' </summary>
    ''' <param name="cdf"></param>
    Public Sub Save(cdf As String, network As Network)
        ' 将所有的临时数据提交到临时文件之中，然后关闭文件句柄
        Call networkFrames.Flush()
        Call networkFrames.Dispose()
        Call biasFrames.Flush()
        Call biasFrames.Dispose()
        Call errorFrames.Flush()
        Call errorFrames.Dispose()
        Call timeFrames.Flush()
        Call timeFrames.Dispose()

        Using debugger As New CDFWriter(cdf)
            Call WriteCDF(debugger, network)
        End Using
    End Sub

    Private Shared Function createLocationTable(network As Network) As Func(Of String, String)
        Dim inputLayer = network.InputLayer.Neurons.Select(Function(n) n.Guid).Indexing
        Dim outputLayer = network.OutputLayer.Neurons.Select(Function(n) n.Guid).Indexing
        Dim hiddens As New List(Of SeqValue(Of Index(Of String)))

        For Each layer In network.HiddenLayer.SeqIterator
            hiddens += New SeqValue(Of Index(Of String)) With {
                .i = layer,
                .value = layer.value _
                    .Neurons _
                    .Select(Function(n) n.Guid) _
                    .Indexing
            }
        Next

        Return Function(guid As String) As String
                   If inputLayer.IndexOf(guid) > -1 Then
                       Return "in"
                   ElseIf outputLayer.IndexOf(guid) > -1 Then
                       Return "out"
                   Else
                       For Each layer In hiddens
                           If layer.value.IndexOf(guid) > -1 Then
                               Return $"hiddens-{layer.i}"
                           End If
                       Next
                   End If

                   Return "NA"
               End Function
    End Function

    Private Sub WriteCDF(debugger As CDFWriter, network As Network)
        Dim neuronLocation = createLocationTable(network)
        Dim hiddenLayout = network.HiddenLayer.Select(Function(l) l.Neurons.Length).JoinBy(", ")
        Dim attrs = {
            New Components.attribute With {.name = "Date", .type = CDFDataTypes.NC_CHAR, .value = Now.ToString},
            New Components.attribute With {.name = "input_layer", .type = CDFDataTypes.NC_CHAR, .value = network.InputLayer.Neurons.Length},
            New Components.attribute With {.name = "output_layer", .type = CDFDataTypes.NC_CHAR, .value = network.OutputLayer.Neurons.Length},
            New Components.attribute With {.name = "hidden_layers", .type = CDFDataTypes.NC_CHAR, .value = hiddenLayout},
            New Components.attribute With {.name = "synapse_edges", .type = CDFDataTypes.NC_CHAR, .value = synapses.Length},
            New Components.attribute With {.name = "times", .type = CDFDataTypes.NC_CHAR, .value = App.ElapsedMilliseconds},
            New Components.attribute With {.name = "ANN", .type = CDFDataTypes.NC_CHAR, .value = network.GetType.FullName},
            New Components.attribute With {.name = "Github", .type = CDFDataTypes.NC_CHAR, .value = LICENSE.githubURL}
        }
        Dim dimensions = {
            New Components.Dimension With {.name = "index_number", .size = 4},
            New Components.Dimension With {.name = GetType(Double).FullName, .size = 8},
            New Components.Dimension With {.name = GetType(String).FullName, .size = 1024},
            New Components.Dimension With {.name = GetType(Long).FullName, .size = 1}
        }

        ' 下面的临时文件读写代码都会被单独放在函数之中
        ' 这个样子GC可以更加容易的回收内存
        debugger.GlobalAttributes(attrs).Dimensions(dimensions)

        Call writeIndex(debugger)
        Call writeErrors(debugger)
        Call writeUnixtime(debugger)

        For Each active In network.Activations
            Call debugger.AddVariable("active=" & active.Key, CType(active.Value.ToString, chars), {GetType(String).FullName})
        Next

        Using reader As BinaryDataReader = biasTemp.OpenBinaryReader
            Dim index As i32 = Scan0

            For Each n As Neuron In neurons
                attrs = {
                    New Components.attribute With {.name = "layer", .type = CDFDataTypes.NC_CHAR, .value = neuronLocation(n.Guid)},
                    New Components.attribute With {.name = "type", .type = CDFDataTypes.NC_CHAR, .value = "neuron"}
                }
                writeNodeBias(debugger, reader, n.Guid, ++index, attrs)
            Next
        End Using

        Using reader As BinaryDataReader = frameTemp.OpenBinaryReader
            Dim index As i32 = Scan0

            For Each s As Synapse In synapses
                attrs = {
                    New Components.attribute With {.name = "type", .type = CDFDataTypes.NC_CHAR, .value = "synapse"},
                    New Components.attribute With {.name = "input", .type = CDFDataTypes.NC_CHAR, .value = s.InputNeuron.Guid},
                    New Components.attribute With {.name = "output", .type = CDFDataTypes.NC_CHAR, .value = s.OutputNeuron.Guid},
                    New Components.attribute With {.name = "input_layer", .type = CDFDataTypes.NC_CHAR, .value = neuronLocation(s.InputNeuron.Guid)},
                    New Components.attribute With {.name = "output_layer", .type = CDFDataTypes.NC_CHAR, .value = neuronLocation(s.OutputNeuron.Guid)}
                }
                writeWeight(debugger, reader, s.ToString, ++index, attrs)
            Next
        End Using
    End Sub

    Private Sub writeNodeBias(debugger As CDFWriter, reader As BinaryDataReader, name$, i As Integer, attrs As Components.attribute())
        Static offsetDouble As Integer = Marshal.SizeOf(Of Double)

        Dim frameOffset% = offsetDouble * neurons.Length
        Dim popBias = Iterator Function() As IEnumerable(Of Double)
                          Do While Not reader.EndOfStream
                              Yield reader.ReadDouble
                              Call reader.Seek(frameOffset, SeekOrigin.Current)
                          Loop
                      End Function

        Call reader.Seek(offsetDouble * i, SeekOrigin.Begin)
        Call debugger.AddVariable(name, CType(popBias().ToArray, doubles), {GetType(Double).FullName}, attrs)
    End Sub

    Private Sub writeWeight(debugger As CDFWriter, reader As BinaryDataReader, name$, i As Integer, attrs As Components.attribute())
        Static offsetDouble As Integer = Marshal.SizeOf(Of Double)

        Dim frameOffset% = offsetDouble * synapses.Length
        Dim popWeights = Iterator Function() As IEnumerable(Of Double)
                             Do While Not reader.EndOfStream
                                 Yield reader.ReadDouble
                                 Call reader.Seek(frameOffset, SeekOrigin.Current)
                             Loop
                         End Function

        Call reader.Seek(offsetDouble * i, SeekOrigin.Begin)
        Call debugger.AddVariable(name, CType(popWeights().ToArray, doubles), {GetType(Double).FullName}, attrs)
    End Sub

    Private Sub writeUnixtime(debugger As CDFWriter)
        With errorTemp.OpenBinaryReader _
            .ReadAsDoubleVector _
            .ToArray

            Call debugger.AddVariable("unixtimestamp", CType(.ByRef, doubles), {GetType(Long).FullName})
        End With
    End Sub

    Private Sub writeIndex(debugger As CDFWriter)
        Dim indexer = Iterator Function() As IEnumerable(Of Integer)
                          Dim i As i32 = Scan0

                          For Each x In errorTemp.OpenBinaryReader.ReadAsDoubleVector
                              Yield ++i
                          Next
                      End Function

        Call debugger.AddVariable("iterations", CType(indexer().ToArray, integers), {"index_number"})
    End Sub

    Private Sub writeErrors(debugger As CDFWriter)
        With errorTemp.OpenBinaryReader _
            .ReadAsDoubleVector _
            .ToArray

            Call debugger.AddVariable("fitness", CType(.ByRef, doubles), {GetType(Double).FullName})
        End With
    End Sub
End Class
