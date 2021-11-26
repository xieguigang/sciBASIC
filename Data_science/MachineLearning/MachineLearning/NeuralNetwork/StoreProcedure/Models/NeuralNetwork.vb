#Region "Microsoft.VisualBasic::3fad47eab733fb86ba832d99a340e914, Data_science\MachineLearning\MachineLearning\NeuralNetwork\StoreProcedure\Models\NeuralNetwork.vb"

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

'     Class NeuralNetwork
' 
'         Properties: connections, errors, hiddenlayers, inputlayer, learnRate
'                     momentum, neurons, outputlayer
' 
'         Function: GetPredictLambda, GetPredictLambda2, LoadModel, Snapshot
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' Xml/json文件存储格式
    ''' </summary>
    <XmlRoot("NeuralNetwork", [Namespace]:="http://machinelearning.scibasic.net/ANN/")>
    Public Class NeuralNetwork : Inherits XmlDataModel

        Public Property learnRate As Double
        Public Property momentum As Double
        ''' <summary>
        ''' 当前的这个模型快照在训练数据集上的预测误差
        ''' </summary>
        ''' <returns></returns>
        Public Property errors As Double()

        Public Property neurons As NeuronNode()
        Public Property connections As Synapse()
        Public Property inputlayer As NeuronLayer
        Public Property outputlayer As NeuronLayer
        Public Property hiddenlayers As HiddenLayer

        ''' <summary>
        ''' 输入一个样本信息然后输出分类结果预测
        ''' 
        ''' 这个函数假设目标样本输入是在当前的这个<paramref name="normalize"/>矩阵的范围之中的
        ''' 目标输入的样本会在这个函数返回的预测函数之中自动归一化
        ''' </summary>
        ''' <param name="normalize">进行所输入的样本数据的归一化的矩阵</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPredictLambda(normalize As NormalizeMatrix, Optional method As Methods = Methods.NormalScaler) As Func(Of Sample, Double())
            With Me.LoadModel
                Return Function(sample)
                           Return .Compute(normalize.NormalizeInput(sample, method))
                       End Function
            End With
        End Function

        Public Function GetPredictLambda2(normalize As NormalizeMatrix,
                                          Optional method As Methods = Methods.NormalScaler,
                                          Optional mute As Boolean = False) As Func(Of Double(), Double())
            With Me.LoadModel(mute:=mute)
                Return Function(sample)
                           Return .Compute(normalize.NormalizeInput(sample, method))
                       End Function
            End With
        End Function

        ''' <summary>
        ''' Dump the given Neuron <see cref="Network"/> as xml model data
        ''' </summary>
        ''' <param name="instance"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Snapshot(instance As Network, Optional errors As Double() = Nothing) As NeuralNetwork
            Return StoreProcedure.TakeSnapshot(instance, errors)
        End Function

        ''' <summary>
        ''' 这个函数自动兼容XML文档模型或者超大型的文件夹模型数据
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <returns></returns>
        Public Shared Function LoadModel(handle As String) As NeuralNetwork
            If handle.FileLength > 0 Then
                Return handle.LoadXml(Of NeuralNetwork)
            Else
                Return ScatteredLoader(store:=handle)
            End If
        End Function
    End Class
End Namespace
