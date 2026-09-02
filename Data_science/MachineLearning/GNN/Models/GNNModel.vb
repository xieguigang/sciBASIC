#Region "Microsoft.VisualBasic::7af7ac997f526e4718a74179ac53201d, Data_science\MachineLearning\GNN\Models\GNNModel.vb"

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

    '   Total Lines: 86
    '    Code Lines: 47 (54.65%)
    ' Comment Lines: 28 (32.56%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (12.79%)
    '     File Size: 2.51 KB


    ' Class GNNModel
    ' 
    '     Properties: Name
    ' 
    '     Function: GetGradients, GetParameters
    ' 
    '     Sub: PrintModelInfo, SetTraining
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' GNN模型基类
''' 定义了图神经网络模型的基本接口
''' </summary>
Public MustInherit Class GNNModel

    ''' <summary>
    ''' 模型名称
    ''' </summary>
    Private _Name As String
    ''' <summary>
    ''' 模型中的所有层
    ''' </summary>
    Protected _layers As List(Of Layer) = New List(Of Layer)()

    Public Property Name As String
        Get
            Return _Name
        End Get
        Protected Set(value As String)
            _Name = value
        End Set
    End Property

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public MustOverride Function Forward(nodeFeatures As Tensor, graph As Graph) As Tensor

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public MustOverride Function Backward(gradient As Tensor, graph As Graph) As Tensor

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Overridable Function GetParameters() As List(Of Tensor)
        Dim parameters = New List(Of Tensor)()
        For Each layer In _layers
            parameters.AddRange(layer.GetParameters())
        Next
        Return parameters
    End Function

    ''' <summary>
    ''' 获取所有参数的梯度
    ''' </summary>
    Public Overridable Function GetGradients() As List(Of Tensor)
        Dim gradients = New List(Of Tensor)()
        For Each layer In _layers
            gradients.AddRange(layer.GetGradients())
        Next
        Return gradients
    End Function

    ''' <summary>
    ''' 设置训练/评估模式
    ''' </summary>
    Public Overridable Sub SetTraining(isTraining As Boolean)
        For Each layer In _layers
            layer.IsTraining = isTraining
        Next
    End Sub

    ''' <summary>
    ''' 打印模型结构
    ''' </summary>
    Public Overridable Sub PrintModelInfo()
        Console.WriteLine($"模型: {Name}")
        Console.WriteLine($"层数: {_layers.Count}")

        Dim totalParams = 0
        For Each layer In _layers
            Dim layerParams = layer.GetParameters()
            Dim layerParamCount = layerParams.Sum(Function(p) p.Length)
            totalParams += layerParamCount
            Console.WriteLine($"  - {layer.Name}: {layerParamCount} 参数")
        Next

        Console.WriteLine($"总参数量: {totalParams}")
    End Sub
End Class
