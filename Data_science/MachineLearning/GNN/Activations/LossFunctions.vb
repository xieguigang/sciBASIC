#Region "Microsoft.VisualBasic::2d8fb0f0b681fa4114bd5ce04eb8c7cc, Data_science\MachineLearning\GNN\Activations\LossFunctions.vb"

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

    '   Total Lines: 39
    '    Code Lines: 24 (61.54%)
    ' Comment Lines: 12 (30.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (7.69%)
    '     File Size: 1.37 KB


    ' Enum LossType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Module LossFunctions
    ' 
    '     Function: Compute, Gradient
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 损失函数类型枚举
''' </summary>
Public Enum LossType
    MeanSquaredError   ' 均方误差
    CrossEntropy       ' 交叉熵
    BinaryCrossEntropy ' 二元交叉熵
    SoftmaxCrossEntropy ' Softmax交叉熵
End Enum

''' <summary>
''' 损失函数工具类
''' </summary>
Public Module LossFunctions
    ''' <summary>
    ''' 计算损失值
    ''' </summary>
    Public Function Compute(predicted As Tensor, target As Tensor, type As LossType) As Single
        Select Case type
            Case LossType.MeanSquaredError : Return Loss.MeanSquaredError(predicted, target)
            Case LossType.CrossEntropy : Return Loss.CrossEntropy(predicted, target)
            Case Else
                Throw New ArgumentException($"不支持的损失函数类型: {type}")
        End Select
    End Function

    ''' <summary>
    ''' 计算损失梯度
    ''' </summary>
    Public Function Gradient(predicted As Tensor, target As Tensor, type As LossType) As Tensor
        Select Case type
            Case LossType.MeanSquaredError : Return Loss.MeanSquaredErrorGradient(predicted, target)
            Case Else
                Throw New ArgumentException($"不支持的损失函数梯度: {type}")
        End Select
    End Function
End Module
