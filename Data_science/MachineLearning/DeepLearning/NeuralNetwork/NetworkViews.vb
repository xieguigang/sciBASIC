#Region "Microsoft.VisualBasic::network_views, Data_science\MachineLearning\DeepLearning\NeuralNetwork\NetworkViews.vb"

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


    '     Class NetworkLayerView

    '         Properties: Count, Output

    '     
    '     Class HiddenLayersView

    '         Properties: Count, Item, Layers

    '     

    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork

    ''' <summary>
    ''' 只读视图：表示一个神经网络层（输入层 / 隐藏层 / 输出层）。
    ''' 
    ''' 本视图不依赖任何遗留计算类（Layer/Neuron/Synapse），
    ''' 其规模（<see cref="Count"/>）与输出（<see cref="Output"/>）均从 CNN 内核派生，
    ''' 用于支撑 <see cref="Network"/> 的 <see cref="Network.InputLayer"/> /
    ''' <see cref="Network.HiddenLayer"/> / <see cref="Network.OutputLayer"/> 公开属性。
    ''' </summary>
    Public Class NetworkLayerView

        Private ReadOnly m_count As Integer
        Private m_output As Double()

        ''' <summary>
        ''' 该层神经元节点数量（供 <c>.Count</c> 与 <see cref="ToString"/> 使用）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                Return m_count
            End Get
        End Property

        ''' <summary>
        ''' 该层最近一次前向传播的输出向量（只读镜像，由 CNN 内核派生）
        ''' </summary>
        ''' <returns></returns>
        Public Property Output As Double()
            Get
                Return m_output
            End Get
            Friend Set(value As Double())
                m_output = value
            End Set
        End Property

        Sub New(count As Integer)
            m_count = count
            m_output = New Double(Math.Max(count, 1) - 1) {}
        End Sub

        Public Overrides Function ToString() As String
            Return $"layer with {m_count} neurons"
        End Function
    End Class

    ''' <summary>
    ''' 只读视图：表示网络之中的全部隐藏层集合。
    ''' 
    ''' 保留旧 <see cref="HiddenLayers"/> 公开接口之中的 <see cref="Count"/> 与索引器，
    ''' 但其内部不再维护遗留的 Layer/Neuron/Synapse 数据图。
    ''' </summary>
    Public Class HiddenLayersView : Implements IEnumerable(Of NetworkLayerView)

        Private ReadOnly m_layers As NetworkLayerView()

        ''' <summary>
        ''' 隐藏层的数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                Return m_layers.Length
            End Get
        End Property

        ''' <summary>
        ''' 隐藏层视图数组
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Layers As NetworkLayerView()
            Get
                Return m_layers
            End Get
        End Property

        ''' <summary>
        ''' 按索引访问第 i 个隐藏层视图
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(index%) As NetworkLayerView
            Get
                Return m_layers(index)
            End Get
        End Property

        Sub New(layers As IEnumerable(Of NetworkLayerView))
            m_layers = layers.ToArray
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of NetworkLayerView) Implements IEnumerable(Of NetworkLayerView).GetEnumerator
            Return m_layers.AsEnumerable.GetEnumerator()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return m_layers.GetEnumerator()
        End Function
    End Class
End Namespace
