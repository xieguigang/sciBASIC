#Region "Microsoft.VisualBasic::3be8ec3add62e335b773f8f69d1cf0b5, Data_science\MachineLearning\GNN\Trainer\Optimizer.vb"

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

    '   Total Lines: 47
    '    Code Lines: 20 (42.55%)
    ' Comment Lines: 19 (40.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (17.02%)
    '     File Size: 1.27 KB


    ' Class Optimizer
    ' 
    '     Properties: LearningRate
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: ZeroGrad
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 优化器基类
''' 定义了参数更新的接口
''' </summary>
Public MustInherit Class Optimizer
    ''' <summary>
    ''' 学习率
    ''' </summary>
    Public Property LearningRate As Single

    ''' <summary>
    ''' 需要优化的参数列表
    ''' </summary>
    Protected _parameters As List(Of Tensor)

    ''' <summary>
    ''' 参数对应的梯度列表
    ''' </summary>
    Protected _gradients As List(Of Tensor)

    Protected Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), learningRate As Single)
        If parameters.Count <> gradients.Count Then Throw New ArgumentException("参数数量和梯度数量必须相同")

        _parameters = parameters
        _gradients = gradients
        Me.LearningRate = learningRate
    End Sub

    ''' <summary>
    ''' 执行一步参数更新
    ''' </summary>
    Public MustOverride Sub [Step]()

    ''' <summary>
    ''' 清零梯度
    ''' </summary>
    Public Overridable Sub ZeroGrad()
        For Each grad In _gradients
            For i = 0 To grad.Length - 1
                grad(i) = 0
            Next
        Next
    End Sub
End Class
