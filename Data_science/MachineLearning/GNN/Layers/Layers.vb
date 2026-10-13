#Region "Microsoft.VisualBasic::724fe99971e7518b5970fe8b10a8c74a, Data_science\MachineLearning\GNN\Layers\Layers.vb"

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

    '   Total Lines: 46
    '    Code Lines: 17 (36.96%)
    ' Comment Lines: 22 (47.83%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (15.22%)
    '     File Size: 1.18 KB


    ' Class Layer
    ' 
    '     Properties: IsTraining, Name
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

    ''' <summary>
    ''' 把本层的所有梯度归零。
    ''' </summary>
    ''' <remarks>
    ''' 为什么需要它：绝大多数层（GRULayer、SimpleRNNLayer、GCNConvLayer 底下的 LinearLayer
    ''' 之外的部分）在 Backward 里用的是 `grad += …` 的累加语义，这本身是对的
    ''' （一个序列/一个 batch 内要把各时间步、各样本的贡献加起来）。
    ''' 但训练循环必须在每个样本反向之前清一次，否则梯度会跨样本、
    ''' 跨 epoch 无界累积：Adam 的二阶矩要算 g²，单精度下很快就会溢出成 Infinity，
    ''' 进而让 Inf/Inf 得到 NaN 并污染全部参数。
    ''' <para>
    ''' 默认实现直接遍历 <see cref="GetGradients"/> 归零，适用于所有按持久张量暴露梯度的层；
    ''' 有特殊布局的层可以覆写。
    ''' </para>
    ''' </remarks>
    Public Overridable Sub ResetGradients()
        For Each grad In GetGradients()
            If grad Is Nothing Then Continue For

            ' 直接写底层数组：索引器 setter 每个元素都要自增一次版本号，
            ' 而归零是纯写操作、没有中间读取，批量化更划算。
            Dim data = grad.Data

            For i = 0 To data.Length - 1
                data(i) = 0.0
            Next

            Call grad.MarkHostModified()
        Next
    End Sub
End Class
