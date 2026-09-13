#Region "Microsoft.VisualBasic::0cf8a9e96d1abb20c06de205d37d33a6, Data_science\MachineLearning\DeepLearning\CNN\data\BackPropResult.vb"

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

    '   Total Lines: 53
    '    Code Lines: 36 (67.92%)
    ' Comment Lines: 7 (13.21%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 10 (18.87%)
    '     File Size: 1.65 KB


    '     Class BackPropResult
    ' 
    '         Properties: Gradients, L1DecayMul, L2DecayMul, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CNN.data

    ''' <summary>
    ''' When we have done a back propagation of the network we will receive a
    ''' result of weight adjustments required to learn. This result set will
    ''' contain the data used by the trainer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class BackPropResult

        Friend l1_decay_mul, l2_decay_mul As Double

        Dim w As Double()
        Dim dw As Double()

        Public Overridable ReadOnly Property L1DecayMul As Double
            Get
                Return l1_decay_mul
            End Get
        End Property

        Public Overridable ReadOnly Property L2DecayMul As Double
            Get
                Return l2_decay_mul
            End Get
        End Property

        Public Overridable ReadOnly Property Weights As Double()
            Get
                Return w
            End Get
        End Property

        Public Overridable ReadOnly Property Gradients As Double()
            Get
                Return dw
            End Get
        End Property

        ''' <summary>
        ''' 训练器就地改写 <see cref="Weights"/> / <see cref="Gradients"/> 之后调用的回调。
        ''' </summary>
        ''' <remarks>
        ''' 这两个数组往往就是某个 <c>DataBlock</c> 的底层存储，而 CUDA 后端会把它们缓存到显存；
        ''' 训练器绕过 Tensor 索引器直接写数组，Tensor 自身无从感知，必须由本回调显式声明失效，
        ''' 否则 GPU 侧会一直复用旧副本，造成静默的数值错误。
        ''' 为 <c>Nothing</c> 表示该参数块不涉及设备端缓存。
        ''' </remarks>
        Public Property ModifiedHandler As Action

        Public Sub New(w As Double(), dw As Double(), l1_decay_mul As Double, l2_decay_mul As Double,
                       Optional modifiedHandler As Action = Nothing)
            Me.w = w
            Me.dw = dw
            Me.l1_decay_mul = l1_decay_mul
            Me.l2_decay_mul = l2_decay_mul
            Me.ModifiedHandler = modifiedHandler
        End Sub

        ''' <summary>由训练器在就地改写参数/梯度之后调用，声明设备端缓存副本已经失效</summary>
        Public Sub NotifyModified()
            Dim handler = ModifiedHandler

            If handler IsNot Nothing Then Call handler()
        End Sub

        Public Overrides Function ToString() As String
            Return $"[len:{w.Length}] l1_decay_mul:{l1_decay_mul}, l2_decay_mul:{l2_decay_mul}; w:{w.Take(13).JoinBy(", ")}...; dw:{dw.Take(13).JoinBy(", ")}..."
        End Function

    End Class
End Namespace
