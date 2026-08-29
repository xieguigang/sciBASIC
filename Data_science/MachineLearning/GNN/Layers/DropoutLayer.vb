#Region "Microsoft.VisualBasic::bb4c5b3a05d1c5b998961a0d52be7e1e, Data_science\MachineLearning\GNN\Layers\DropoutLayer.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 6 (11.11%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.80 KB


    ' Class DropoutLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' Dropout层
''' 在训练时随机丢弃部分神经元，防止过拟合
''' </summary>
Public Class DropoutLayer
    Inherits Layer
    Private ReadOnly _dropRate As Single
    Private _mask As Tensor
    Private _random As Random

    Public Sub New(Optional dropRate As Single = 0.5F, Optional name As String = Nothing, Optional seed As Integer? = Nothing)
        If dropRate < 0 OrElse dropRate >= 1 Then Throw New ArgumentException("Dropout率必须在[0, 1)范围内")

        _dropRate = dropRate
        _random = If(seed.HasValue, New Random(seed.Value), New Random())
        MyBase.Name = If(name, $"Dropout_{dropRate}")
    End Sub

    Public Overrides Function Forward(input As Tensor) As Tensor
        If Not IsTraining Then
            ' 测试时不进行dropout
            Return input
        End If

        ' 创建dropout掩码
        _mask = New Tensor(input.Shape)
        Dim scale = 1.0F / (1.0F - _dropRate)

        For i = 0 To input.Length - 1
            If _random.NextDouble() >= _dropRate Then
                _mask(i) = scale ' 保留并缩放
            Else
                _mask(i) = 0 ' 丢弃
            End If
        Next

        Return input.ElementwiseMultiply(_mask)
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        If Not IsTraining Then Return gradient

        Return gradient.ElementwiseMultiply(_mask)
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class
