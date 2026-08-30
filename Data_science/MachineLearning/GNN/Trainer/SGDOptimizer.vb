#Region "Microsoft.VisualBasic::d429cf59f88aaa6deb52ea46dd73a8d1, Data_science\MachineLearning\GNN\Trainer\SGDOptimizer.vb"

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

    '   Total Lines: 59
    '    Code Lines: 29 (49.15%)
    ' Comment Lines: 22 (37.29%)
    '    - Xml Docs: 77.27%
    ' 
    '   Blank Lines: 8 (13.56%)
    '     File Size: 1.97 KB


    ' Class SGDOptimizer
    ' 
    '     Properties: Momentum
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: [Step]
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 随机梯度下降优化器 (SGD)
''' 最基础的优化器：θ = θ - lr * ∇θ
''' </summary>
Public Class SGDOptimizer
    Inherits Optimizer
    ''' <summary>
    ''' 动量系数
    ''' </summary>
    Public Property Momentum As Single

    ''' <summary>
    ''' 速度（动量）缓存
    ''' </summary>
    Private _velocities As List(Of Tensor)

    ''' <summary>
    ''' 创建SGD优化器
    ''' </summary>
    ''' <param name="parameters">需要优化的参数</param>
    ''' <param name="gradients">参数对应的梯度</param>
    ''' <param name="learningRate">学习率</param>
    ''' <param name="momentum">动量系数（0表示不使用动量）</param>
    Public Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), Optional learningRate As Single = 0.01F, Optional momentum As Single = 0.0F)

        MyBase.New(parameters, gradients, learningRate)
        Me.Momentum = momentum

        ' 初始化速度缓存
        _velocities = New List(Of Tensor)()
        For Each param In parameters
            _velocities.Add(New Tensor(param.Shape))
        Next
    End Sub

    Public Overrides Sub [Step]()
        For i = 0 To _parameters.Count - 1
            Dim param = _parameters(i)
            Dim grad = _gradients(i)
            Dim velocity = _velocities(i)

            For j = 0 To param.Length - 1
                If Momentum > 0 Then
                    ' 带动量的更新
                    ' v = momentum * v - lr * grad
                    ' param = param + v
                    velocity(j) = Momentum * velocity(j) - LearningRate * grad(j)
                    param(j) += velocity(j)
                Else
                    ' 普通SGD
                    param(j) -= LearningRate * grad(j)
                End If
            Next
        Next
    End Sub
End Class

