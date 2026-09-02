#Region "Microsoft.VisualBasic::6585788f01980565f668c6fd3bb84a92, Data_science\MachineLearning\GNN\Layers\ActivationLayer.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 4 (12.12%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.15%)
    '     File Size: 1.11 KB


    ' Class ActivationLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 激活层
''' 对输入应用非线性激活函数
''' </summary>
Public Class ActivationLayer
    Inherits Layer
    Private ReadOnly _activationType As ActivationType
    Private _lastInput As Tensor

    Public Sub New(type As ActivationType, Optional name As String = Nothing)
        _activationType = type
        MyBase.Name = If(name, $"Activation_{type}")
    End Sub

    Public Overrides Function Forward(input As Tensor) As Tensor
        _lastInput = input
        Return Apply(input, _activationType)
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Dim activationDerivative = Derivative(_lastInput, _activationType)
        Return gradient.ElementwiseMultiply(activationDerivative)
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class
