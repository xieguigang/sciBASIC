#Region "Microsoft.VisualBasic::acf3c5c6559225404f18de25a15706b0, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\TrainingUtils.vb"

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

'     Class TrainingUtils
' 
'         Properties: NeuronNetwork, TrainingType, XP
' 
'         Constructor: (+1 Overloads) Sub New
'         Sub: (+2 Overloads) Add, (+2 Overloads) Corrects, Encouraging, RemoveLast, Train
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork

    ''' <summary>
    ''' Tools for training the neuron network
    ''' </summary>
    Public Class TrainingUtils

        Public Property TrainingType As TrainingType = TrainingType.Epoch
        Public Property MinError As Double = Helpers.MinimumError

        ''' <summary>
        ''' 最终得到的训练结果神经网络
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NeuronNetwork As Network

        ReadOnly _dataSets As New List(Of Sample)

        ''' <summary>
        ''' 训练所使用到的经验数量,即数据集的大小s
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property XP As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _dataSets.Count
            End Get
        End Property

        ''' <summary>
        ''' 将训练的成果状态进行快照,转换为可以保存的XML文件对象
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TakeSnapshot() As StoreProcedure.NeuralNetwork
            Return StoreProcedure.NeuralNetwork.Snapshot(NeuronNetwork)
        End Function

        Sub New(net As Network)
            NeuronNetwork = net
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(inputSize As Integer, hiddenSize As Integer(), outputSize As Integer,
                       Optional learnRate As Double = 0.1,
                       Optional momentum As Double = 0.9,
                       Optional active As LayerActives = Nothing)
            Call Me.New(New Network(inputSize, hiddenSize, outputSize, learnRate, momentum, active))
        End Sub

        Public Sub RemoveLast()
            If Not _dataSets.Count = 0 Then
                Call _dataSets.RemoveLast
            End If
        End Sub

        ''' <summary>
        ''' 在这里添加训练使用的数据集
        ''' (请注意,因为ANN的output结果向量只输出``[0,1]``之间的结果,所以在训练的时候,output应该是被编码为0或者1的;
        ''' input可以接受任意实数的向量,但是这个要求所有属性都应该在同一个scale区间内,所以最好也归一化编码为0到1之间的小数)
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="output"></param>
        Public Sub Add(input As Double(), output As Double())
            Call _dataSets.Add(New Sample(input, output))
        End Sub

        Public Sub Add(x As Sample)
            Call _dataSets.Add(x)
        End Sub

        ''' <summary>
        ''' 开始进行训练
        ''' </summary>
        ''' <param name="parallel">
        ''' 小型的人工神经网络的训练,并不建议使用并行化
        ''' </param>
        Public Sub Train(Optional parallel As Boolean = False, Optional normalize As Boolean = False)
            Dim trainingDataSet As Sample() = _dataSets.ToArray

            If normalize Then
                trainingDataSet = trainingDataSet.NormalizeSamples
            End If

            Call Helpers.Train(NeuronNetwork, trainingDataSet, TrainingType, minErr:=MinError, parallel:=parallel)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input">The inputs data</param>
        ''' <param name="convertedResults">The error outputs</param>
        ''' <param name="expectedResults">The corrects output</param>
        Public Sub Corrects(input As Double(), convertedResults As Double(), expectedResults As Double(),
                            Optional train As Boolean = True,
                            Optional parallel As Boolean = False,
                            Optional normalize As Boolean = False)

            Dim offendingDataSet As Sample = _dataSets _
                .FirstOrDefault(Function(x)
                                    Return x.status.SequenceEqual(input) AndAlso x.target.SequenceEqual(convertedResults)
                                End Function)
            _dataSets.Remove(offendingDataSet)

            If Not _dataSets.Exists(Function(x) x.status.SequenceEqual(input) AndAlso x.target.SequenceEqual(expectedResults)) Then
                Call _dataSets.Add(New Sample(input, expectedResults))
            End If

            If train Then
                Call Me.Train(parallel, normalize)
            End If
        End Sub

        Public Sub Corrects(dataset As Sample, expectedResults As Double(),
                            Optional train As Boolean = True,
                            Optional parallel As Boolean = False)
            Call Corrects(dataset.status, dataset.target, expectedResults, train, parallel)
        End Sub
    End Class
End Namespace
