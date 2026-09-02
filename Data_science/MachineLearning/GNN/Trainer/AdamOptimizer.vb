#Region "Microsoft.VisualBasic::6398418edd33e027f5d9e3efd2daba90, Data_science\MachineLearning\GNN\Trainer\AdamOptimizer.vb"

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

    '   Total Lines: 88
    '    Code Lines: 41 (46.59%)
    ' Comment Lines: 31 (35.23%)
    '    - Xml Docs: 83.87%
    ' 
    '   Blank Lines: 16 (18.18%)
    '     File Size: 2.61 KB


    ' Class AdamOptimizer
    ' 
    '     Properties: Beta1, Beta2, Epsilon
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: [Step]
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' Adam优化器
''' 自适应学习率优化器，结合了动量和RMSprop的优点
''' 论文: Adam: A Method for Stochastic Optimization (Kingma & Ba, ICLR 2015)
''' </summary>
Public Class AdamOptimizer
    Inherits Optimizer
    ''' <summary>
    ''' 一阶矩估计的指数衰减率
    ''' </summary>
    Public Property Beta1 As Single

    ''' <summary>
    ''' 二阶矩估计的指数衰减率
    ''' </summary>
    Public Property Beta2 As Single

    ''' <summary>
    ''' 数值稳定性常数
    ''' </summary>
    Public Property Epsilon As Single

    ''' <summary>
    ''' 当前时间步
    ''' </summary>
    Private _t As Integer

    ''' <summary>
    ''' 一阶矩估计（梯度的移动平均）
    ''' </summary>
    Private _m As List(Of Tensor)

    ''' <summary>
    ''' 二阶矩估计（梯度平方的移动平均）
    ''' </summary>
    Private _v As List(Of Tensor)

    ''' <summary>
    ''' 创建Adam优化器
    ''' </summary>
    Public Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), Optional learningRate As Single = 0.001F, Optional beta1 As Single = 0.9F, Optional beta2 As Single = 0.999F, Optional epsilon As Single = 0.00000001F)

        MyBase.New(parameters, gradients, learningRate)
        Me.Beta1 = beta1
        Me.Beta2 = beta2
        Me.Epsilon = epsilon
        _t = 0

        ' 初始化矩估计
        _m = New List(Of Tensor)()
        _v = New List(Of Tensor)()
        For Each param In parameters
            _m.Add(New Tensor(param.Shape))
            _v.Add(New Tensor(param.Shape))
        Next
    End Sub

    Public Overrides Sub [Step]()
        _t += 1

        For i = 0 To _parameters.Count - 1
            Dim param = _parameters(i)
            Dim grad = _gradients(i)
            Dim m = _m(i)
            Dim v = _v(i)

            For j = 0 To param.Length - 1
                Dim g = grad(j)

                ' 更新一阶矩估计
                m(j) = Beta1 * m(j) + (1 - Beta1) * g

                ' 更新二阶矩估计
                v(j) = Beta2 * v(j) + (1 - Beta2) * g * g

                ' 偏差修正
                Dim mHat = m(j) / (1 - CSng(std.Pow(Beta1, _t)))
                Dim vHat = v(j) / (1 - CSng(std.Pow(Beta2, _t)))

                ' 参数更新
                param(j) -= LearningRate * mHat / (CSng(std.Sqrt(vHat)) + Epsilon)
            Next
        Next
    End Sub
End Class
