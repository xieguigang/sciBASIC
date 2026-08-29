#Region "Microsoft.VisualBasic::87ab5ced43aac5d1bb21595c4d2b4a11, Data_science\MachineLearning\GNN\Layers.vb"

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

    '   Total Lines: 687
    '    Code Lines: 381 (55.46%)
    ' Comment Lines: 195 (28.38%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 111 (16.16%)
    '     File Size: 23.02 KB


    ' Class Layer
    ' 
    '     Properties: IsTraining, Name
    ' 
    ' Class LinearLayer
    ' 
    '     Properties: InFeatures, OutFeatures, UseBias
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetBias, GetGradients, GetParameters
    '               GetWeights
    ' 
    ' Class ActivationLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters
    ' 
    ' Class DropoutLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters
    ' 
    ' Class GCNConvLayer
    ' 
    '     Properties: InFeatures, OutFeatures
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) Backward, (+3 Overloads) Forward, GetGradients, GetParameters
    ' 
    ' Class GATLayer
    ' 
    '     Properties: InFeatures, OutFeatures
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, (+2 Overloads) Forward, GetGradients, GetParameters, LeakyReLU
    ' 
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
''' 神经网络层基类
''' 定义了所有层必须实现的接口
''' </summary>
Public MustInherit Class Layer
    ''' <summary>
    ''' 层的名称
    ''' </summary>
    Private _Name As String

    Public Property Name As String
        Get
            Return _Name
        End Get
        Protected Set(value As String)
            _Name = value
        End Set
    End Property

    ''' <summary>
    ''' 层是否处于训练模式
    ''' </summary>
    Public Property IsTraining As Boolean = True

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public MustOverride Function Forward(input As Tensor) As Tensor

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public MustOverride Function Backward(gradient As Tensor) As Tensor

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public MustOverride Function GetParameters() As List(Of Tensor)

    ''' <summary>
    ''' 获取所有参数的梯度
    ''' </summary>
    Public MustOverride Function GetGradients() As List(Of Tensor)
End Class
