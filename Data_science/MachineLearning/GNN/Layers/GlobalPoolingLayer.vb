#Region "Microsoft.VisualBasic::c6302f64ba92f1b9c388ed4330abdaf3, Data_science\MachineLearning\GNN\Layers\GlobalPoolingLayer.vb"

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

    '   Total Lines: 97
    '    Code Lines: 72 (74.23%)
    ' Comment Lines: 14 (14.43%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 11 (11.34%)
    '     File Size: 3.46 KB


    ' Class GlobalPoolingLayer
    ' 
    ' 
    '     Enum PoolingType
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters, MaxPooling
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 全局池化层
''' 将节点特征聚合为图级别特征
''' 用于图分类任务
''' </summary>
Public Class GlobalPoolingLayer
    Inherits Layer
    Public Enum PoolingType
        Sum    ' 求和池化
        Mean   ' 平均池化
        Max     ' 最大池化
    End Enum

    Private ReadOnly _poolingType As PoolingType
    Private _lastInput As Tensor

    Public Sub New(Optional type As PoolingType = PoolingType.Mean, Optional name As String = Nothing)
        _poolingType = type
        MyBase.Name = If(name, $"GlobalPooling_{type}")
    End Sub

    ''' <summary>
    ''' 前向传播：将节点特征聚合为图级别特征
    ''' </summary>
    ''' <param name="input">节点特征 [numNodes, features]</param>
    ''' <returns>图特征 [1, features]</returns>
    Public Overrides Function Forward(input As Tensor) As Tensor
        _lastInput = input

        Select Case _poolingType
            Case PoolingType.Sum : Return input.Sum(0)
            Case PoolingType.Mean : Return input.Mean(0)
            Case PoolingType.Max : Return MaxPooling(input)
            Case Else
                Throw New ArgumentException($"未知的池化类型: {_poolingType}")
        End Select
    End Function

    Private Function MaxPooling(input As Tensor) As Tensor
        Dim result = New Tensor(1, input.Shape(1))
        For j = 0 To input.Shape(1) - 1
            Dim maxVal = Single.MinValue
            For i = 0 To input.Shape(0) - 1
                If input(i, j) > maxVal Then maxVal = input(i, j)
            Next
            result(0, j) = maxVal
        Next
        Return result
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        ' 将梯度广播回所有节点
        Dim inputGradient = New Tensor(_lastInput.Shape)

        Dim numNodes = _lastInput.Shape(0)

        If _poolingType = PoolingType.Sum Then
            ' 求和池化：每个节点获得相同的梯度
            For i = 0 To numNodes - 1
                For j = 0 To gradient.Shape(1) - 1
                    inputGradient(i, j) = gradient(0, j)
                Next
            Next
        ElseIf _poolingType = PoolingType.Mean Then
            ' 平均池化：梯度除以节点数
            For i = 0 To numNodes - 1
                For j = 0 To gradient.Shape(1) - 1
                    inputGradient(i, j) = gradient(0, j) / numNodes
                Next
            Next
        ElseIf _poolingType = PoolingType.Max Then
            ' 最大池化：只有最大值位置获得梯度
            For j = 0 To gradient.Shape(1) - 1
                Dim maxVal = Single.MinValue
                Dim maxIdx = 0
                For i = 0 To numNodes - 1
                    If _lastInput(i, j) > maxVal Then
                        maxVal = _lastInput(i, j)
                        maxIdx = i
                    End If
                Next
                inputGradient(maxIdx, j) = gradient(0, j)
            Next
        End If

        Return inputGradient
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class
