#Region "Microsoft.VisualBasic::5895cbe2276da2eb731d27e43f502ad3, Data_science\MachineLearning\GNN\Activations.vb"

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

    '   Total Lines: 301
    '    Code Lines: 162 (53.82%)
    ' Comment Lines: 103 (34.22%)
    '    - Xml Docs: 94.17%
    ' 
    '   Blank Lines: 36 (11.96%)
    '     File Size: 10.39 KB


    ' Module Activation
    ' 
    '     Function: LeakyReLU, LeakyReLUDerivative, ReLU, ReLUDerivative, Sigmoid
    '               SigmoidDerivative, (+2 Overloads) Softmax, Tanh, TanhDerivative
    ' 
    ' Enum ActivationType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Module ActivationFunctions
    ' 
    '     Function: Apply, Derivative
    ' 
    ' Module Loss
    ' 
    '     Function: BinaryCrossEntropy, CrossEntropy, MeanSquaredError, MeanSquaredErrorGradient, NegativeLogLikelihood
    '               SoftmaxCrossEntropy
    ' 
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
Imports std = System.Math

''' <summary>
''' 激活函数集合
''' 激活函数为神经网络引入非线性，使网络能够学习复杂的模式
''' </summary>
Public Module Activation
    ''' <summary>
    ''' ReLU激活函数: f(x) = max(0, x)
    ''' 优点：计算简单，缓解梯度消失问题
    ''' 缺点：存在"死亡ReLU"问题（负值永远为0）
    ''' </summary>
    Public Function ReLU(x As Single) As Single
        Return std.Max(0, x)
    End Function

    ''' <summary>
    ''' ReLU的导数
    ''' </summary>
    Public Function ReLUDerivative(x As Single) As Single
        Return If(x > 0, 1.0F, 0.0F)
    End Function

    ''' <summary>
    ''' Sigmoid激活函数: f(x) = 1 / (1 + e^(-x))
    ''' 输出范围: (0, 1)
    ''' 常用于二分类问题的输出层
    ''' </summary>
    Public Function Sigmoid(x As Single) As Single
        ' 防止数值溢出
        If x > 20 Then Return 1.0F
        If x < -20 Then Return 0.0F
        Return 1.0F / (1.0F + CSng(std.Exp(-x)))
    End Function

    ''' <summary>
    ''' Sigmoid的导数
    ''' </summary>
    Public Function SigmoidDerivative(x As Single) As Single
        Dim s = Sigmoid(x)
        Return s * (1 - s)
    End Function

    ''' <summary>
    ''' Tanh激活函数: f(x) = (e^x - e^(-x)) / (e^x + e^(-x))
    ''' 输出范围: (-1, 1)
    ''' 零中心化，收敛速度通常比sigmoid快
    ''' </summary>
    Public Function Tanh(x As Single) As Single
        Return std.Tanh(x)
    End Function

    ''' <summary>
    ''' Tanh的导数
    ''' </summary>
    Public Function TanhDerivative(x As Single) As Single
        Dim t = Tanh(x)
        Return 1 - t * t
    End Function

    ''' <summary>
    ''' LeakyReLU激活函数: f(x) = x if x > 0 else alpha * x
    ''' 解决ReLU的"死亡"问题
    ''' </summary>
    Public Function LeakyReLU(x As Single, Optional alpha As Single = 0.01F) As Single
        Return If(x > 0, x, alpha * x)
    End Function

    ''' <summary>
    ''' LeakyReLU的导数
    ''' </summary>
    Public Function LeakyReLUDerivative(x As Single, Optional alpha As Single = 0.01F) As Single
        Return If(x > 0, 1.0F, alpha)
    End Function

    ''' <summary>
    ''' Softmax函数
    ''' 将向量转换为概率分布，所有元素和为1
    ''' 常用于多分类问题的输出层
    ''' </summary>
    ''' <param name="input">输入向量</param>
    ''' <returns>概率分布向量</returns>
    Public Function Softmax(input As Single()) As Single()
        ' 数值稳定性：减去最大值
        Dim maxVal As Single = input.Max()
        Dim expValues = New Single(input.Length - 1) {}
        Dim sumExp As Single = 0

        For i = 0 To input.Length - 1
            expValues(i) = CSng(std.Exp(input(i) - maxVal))
            sumExp += expValues(i)
        Next

        Dim result = New Single(input.Length - 1) {}
        For i = 0 To input.Length - 1
            result(i) = expValues(i) / sumExp
        Next

        Return result
    End Function

    ''' <summary>
    ''' 对张量的每一行应用Softmax
    ''' </summary>
    Public Function Softmax(input As Tensor) As Tensor
        If input.Rank <> 2 Then Throw New ArgumentException("Softmax只支持二维张量")

        Dim result = New Tensor(input.Shape)
        Dim rows = input.Shape(0)
        Dim cols = input.Shape(1)

        For i = 0 To rows - 1
            ' 提取一行
            Dim row = New Single(cols - 1) {}
            For j = 0 To cols - 1
                row(j) = input(i, j)
            Next

            ' 应用Softmax
            Dim softmaxRow = Softmax(row)

            ' 写入结果
            For j = 0 To cols - 1
                result(i, j) = softmaxRow(j)
            Next
        Next

        Return result
    End Function
End Module

