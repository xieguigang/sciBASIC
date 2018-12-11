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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Encouraging()
            Call Train()
        End Sub

        Sub New(net As Network)
            NeuronNetwork = net
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(inputSize As Integer, hiddenSize As Integer(), outputSize As Integer,
                       Optional learnRate As Double = 0.1,
                       Optional momentum As Double = 0.9,
                       Optional active As IActivationFunction = Nothing)
            Call Me.New(New Network(inputSize, hiddenSize, outputSize, learnRate, momentum, active))
        End Sub

        Public Sub RemoveLast()
            If Not _dataSets.Count = 0 Then
                Call _dataSets.RemoveLast
            End If
        End Sub

        ''' <summary>
        ''' 在这里添加训练使用的数据集
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
        Public Sub Train()
            Call Helpers.Train(NeuronNetwork, _dataSets, TrainingType)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input">The inputs data</param>
        ''' <param name="convertedResults">The error outputs</param>
        ''' <param name="expectedResults">The corrects output</param>
        Public Sub Corrects(input As Double(), convertedResults As Double(), expectedResults As Double(), Optional train As Boolean = True)
            Dim offendingDataSet As Sample = _dataSets _
                .FirstOrDefault(Function(x)
                                    Return x.Status.SequenceEqual(input) AndAlso x.Target.SequenceEqual(convertedResults)
                                End Function)
            _dataSets.Remove(offendingDataSet)

            If Not _dataSets.Exists(Function(x) x.Status.SequenceEqual(input) AndAlso x.Target.SequenceEqual(expectedResults)) Then
                Call _dataSets.Add(New Sample(input, expectedResults))
            End If

            If train Then
                Call Me.Train()
            End If
        End Sub

        Public Sub Corrects(dataset As Sample, expectedResults As Double(), Optional train As Boolean = True)
            Call Corrects(dataset.Status, dataset.Target, expectedResults, train)
        End Sub
    End Class
End Namespace
