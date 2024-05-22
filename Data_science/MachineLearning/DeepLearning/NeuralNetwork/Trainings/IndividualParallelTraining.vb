#Region "Microsoft.VisualBasic::3db160ac2cb0f11e171ddd00b7c6587e, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\IndividualParallelTraining.vb"

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

    '   Total Lines: 153
    '    Code Lines: 121 (79.08%)
    ' Comment Lines: 4 (2.61%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 28 (18.30%)
    '     File Size: 6.25 KB


    '     Class ParallelNetwork
    ' 
    '         Function: LoadSnapshot, Predicts
    ' 
    '     Class IndividualParallelTraining
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: populateDataSets, runTraining
    ' 
    '         Sub: cloneNetworks, SaveSnapshot, Snapshot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork

    Public Class ParallelNetwork

        Dim parallels As Func(Of Double(), Double())()

        Public Iterator Function Predicts(input As Double()) As IEnumerable(Of Double)
            For i As Integer = 0 To parallels.Length - 1
                Yield parallels(i)(input)(Scan0)
            Next
        End Function

        Public Shared Function LoadSnapshot(dir As String,
                                            normalize As NormalizeMatrix,
                                            Optional method As Methods = Methods.NormalScaler) As ParallelNetwork
            Dim parallels As New List(Of Func(Of Double(), Double()))
            Dim annLambda As Func(Of Double(), Double())
            Dim i As Integer
            Dim fs As Directory

            For Each individual As String In dir _
                .ListDirectory(SearchOption.SearchTopLevelOnly) _
                .OrderBy(Function(name)
                             Return i32.GetHexInteger(name.BaseName)
                         End Function)

                fs = Directory.FromLocalFileSystem(individual)
                annLambda = ScatteredLoader(store:=fs, mute:=True).GetPredictLambda2(normalize, method, mute:=True)
                parallels += annLambda

                i = i32.GetHexInteger(individual.BaseName)

                Call $"load component: {i}".__DEBUG_ECHO
            Next

            Return New ParallelNetwork With {
                .parallels = parallels.ToArray
            }
        End Function

    End Class

    Public Class IndividualParallelTraining : Inherits ANNTrainer

        Dim individualNetworks As SeqValue(Of Network)()

        Sub New(net As Network)
            Call MyBase.New(net)
            Call cloneNetworks()
        End Sub

        Sub New(inputSize As Integer,
                hiddenSize() As Integer,
                outputSize As Integer,
                Optional learnRate As Double = 0.1,
                Optional momentum As Double = 0.9,
                Optional active As LayerActives = Nothing,
                Optional weightInit As Func(Of Double) = Nothing)

            Call MyBase.New(inputSize, hiddenSize, outputSize, learnRate, momentum, active, weightInit)
            Call cloneNetworks()
        End Sub

        Private Sub cloneNetworks()
            Dim clones As New List(Of Network)
            Dim inputSize = network.InputLayer.Count
            Dim outputSize = 1 ' network.OutputLayer.Count
            Dim hiddenSize As New List(Of Integer)
            Dim active As LayerActives = LayerActives.FromXmlModel(network.Activations)

            For Each layer In network.HiddenLayer
                hiddenSize.Add(layer.Count)
            Next

            For i As Integer = 0 To network.OutputLayer.Count - 1
                Call New Network(
                     inputSize:=inputSize,
                     hiddenSize:=hiddenSize,
                     outputSize:=outputSize,
                     learnRate:=network.LearnRate,
                     momentum:=network.Momentum,
                     active:=active,
                     weightInit:=Helpers.randomWeight
                ) With {
                    .Truncate = network.Truncate,
                    .LearnRateDecay = network.LearnRateDecay
                }.DoCall(AddressOf clones.Add)
            Next

            individualNetworks = clones.SeqIterator.ToArray
        End Sub

        Protected Overrides Sub SaveSnapshot()
            Call Snapshot(snapshotSaveLocation)
        End Sub

        Public Sub Snapshot(snapshotSaveLocation As String)
            Dim outputSize As Integer = network.OutputLayer.Count
            Dim index As i32 = 666

            For i As Integer = 0 To outputSize - 1
                Call New Snapshot(individualNetworks(i)).WriteScatteredParts($"{snapshotSaveLocation}/{(index + 1).Hex }/")
            Next
        End Sub

        Protected Overrides Function runTraining(parallel As Boolean) As Double()
            ' 20190701 数据不打乱，网络极大可能拟合前面几个batch的样本分布
            ' 
            ' 训练所使用的样本数据的顺序可能会对结果产生影响
            ' 所以在训练之前会需要打乱样本的顺序来避免出现问题
            Dim dataSets As TrainingSample()() = populateDataSets().ToArray
            Dim errors As Double() = individualNetworks _
                .AsParallel _
                .Select(Function(network)
                            Dim temp = trainingImpl(network, dataSets(network.i), parallel, Selective, dropOutRate, backPropagate:=True)
                            Return (network.i, Err:=temp(Scan0))
                        End Function) _
                .OrderBy(Function(rtvl) rtvl.i) _
                .Select(Function(xi) xi.Err) _
                .ToArray

            Return errors
        End Function

        Private Iterator Function populateDataSets() As IEnumerable(Of TrainingSample())
            Dim raw = Me.dataSets.Shuffles
            Dim outputSize As Integer = raw(Scan0).classify.Length
            Dim j As Integer = 0

            For i As Integer = 0 To outputSize - 1
                j = i

                Yield raw _
                    .Select(Function(a)
                                Return New TrainingSample With {
                                    .classify = {a.classify(j)},
                                    .sample = a.sample,
                                    .sampleID = a.sampleID
                                }
                            End Function) _
                    .ToArray
            Next
        End Function
    End Class
End Namespace
