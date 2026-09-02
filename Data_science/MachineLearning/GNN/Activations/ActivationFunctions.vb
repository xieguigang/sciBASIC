#Region "Microsoft.VisualBasic::ad3654865aecc4a942c57d0403e66659, Data_science\MachineLearning\GNN\Activations\ActivationFunctions.vb"

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

    '   Total Lines: 38
    '    Code Lines: 26 (68.42%)
    ' Comment Lines: 10 (26.32%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (5.26%)
    '     File Size: 1.85 KB


    ' Module ActivationFunctions
    ' 
    '     Function: Apply, Derivative
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 激活函数工具类
''' 提供统一的激活函数调用接口
''' </summary>
Public Module ActivationFunctions
    ''' <summary>
    ''' 应用激活函数
    ''' </summary>
    Public Function Apply(input As Tensor, type As ActivationType) As Tensor
        Select Case type
            Case ActivationType.None : Return input.Clone()
            Case ActivationType.ReLU : Return input.Apply(AddressOf Activation.ReLU)
            Case ActivationType.Sigmoid : Return input.Apply(AddressOf Activation.Sigmoid)
            Case ActivationType.Tanh : Return input.Apply(AddressOf Activation.Tanh)
            Case ActivationType.LeakyReLU : Return input.Apply(Function(x) Activation.LeakyReLU(x))
            Case ActivationType.Softmax : Return Activation.Softmax(input)
            Case Else
                Throw New ArgumentException($"未知的激活函数类型: {type}")
        End Select
    End Function

    ''' <summary>
    ''' 计算激活函数的导数
    ''' </summary>
    Public Function Derivative(input As Tensor, type As ActivationType) As Tensor
        Select Case type
            Case ActivationType.None : Return Tensor.Filled(input.Shape, 1.0F)
            Case ActivationType.ReLU : Return input.Apply(AddressOf Activation.ReLUDerivative)
            Case ActivationType.Sigmoid : Return input.Apply(AddressOf Activation.SigmoidDerivative)
            Case ActivationType.Tanh : Return input.Apply(AddressOf Activation.TanhDerivative)
            Case ActivationType.LeakyReLU : Return input.Apply(Function(x) Activation.LeakyReLUDerivative(x))
            Case Else
                Throw New ArgumentException($"不支持的激活函数导数: {type}")
        End Select
    End Function
End Module
