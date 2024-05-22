#Region "Microsoft.VisualBasic::24926215db8aa9a68b30ece4cecc7234, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\TrainingUtils.vb"

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

    '   Total Lines: 55
    '    Code Lines: 35 (63.64%)
    ' Comment Lines: 11 (20.00%)
    '    - Xml Docs: 63.64%
    ' 
    '   Blank Lines: 9 (16.36%)
    '     File Size: 2.37 KB


    '     Class TrainingUtils
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: runTraining, TakeSnapshot
    ' 
    '         Sub: SaveSnapshot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork

    ''' <summary>
    ''' Tools for training the neuron network
    ''' </summary>
    Public Class TrainingUtils : Inherits ANNTrainer

        Public Sub New(net As Network)
            MyBase.New(net)
        End Sub

        Public Sub New(inputSize As Integer,
                       hiddenSize() As Integer,
                       outputSize As Integer,
                       Optional learnRate As Double = 0.1,
                       Optional momentum As Double = 0.9,
                       Optional active As LayerActives = Nothing,
                       Optional weightInit As Func(Of Double) = Nothing)

            MyBase.New(inputSize, hiddenSize, outputSize, learnRate, momentum, active, weightInit)
        End Sub

        Protected Overrides Sub SaveSnapshot()
            Call "save trained ANN model!".__DEBUG_ECHO

            If Not snapshotSaveLocation.StringEmpty Then
                Call TakeSnapshot.ScatteredStore(Directory.FromLocalFileSystem(snapshotSaveLocation))
            Else
                Call "Snapshot location is empty, trained model will not saved...".__DEBUG_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 将训练的成果状态进行快照,转换为可以保存的XML文件对象
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TakeSnapshot() As StoreProcedure.NeuralNetwork
            Return StoreProcedure.NeuralNetwork.Snapshot(Network, errors)
        End Function

        Protected Overrides Function runTraining(parallel As Boolean) As Double()
            ' 20190701 数据不打乱，网络极大可能拟合前面几个batch的样本分布
            ' 
            ' 训练所使用的样本数据的顺序可能会对结果产生影响
            ' 所以在训练之前会需要打乱样本的顺序来避免出现问题
            Return trainingImpl(network, dataSets.Shuffles, parallel, Selective, dropOutRate, backPropagate:=True)
        End Function
    End Class
End Namespace
