#Region "Microsoft.VisualBasic::df2f6c24fb1c2beba4d9e0a7c4daf145, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\HiddenLayers.vb"

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

    '   Total Lines: 119
    '    Code Lines: 64
    ' Comment Lines: 39
    '   Blank Lines: 16
    '     File Size: 4.91 KB


    '     Class HiddenLayers
    ' 
    '         Properties: Layers, Output, Size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetAllNeurons, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: BackPropagate, ForwardPropagate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NeuralNetwork

    ''' <summary>
    ''' 隐藏层,由多个神经元层所构成的
    ''' 
    ''' ##### 20181212
    ''' 
    ''' 请注意,这个隐藏层的大小虽然可以是任意规模的,但是并不是越大越好的
    ''' 当隐藏层越大的话,会导致训练的效率降低,并且预测能力也会下降
    ''' 如果问题比较简单的话,三层隐藏层已经足够了
    ''' 
    ''' 一般来说,隐藏层之中每一层的神经元的数量应该要大于输入和输出的节点数的最大值.
    ''' </summary>
    Public Class HiddenLayers : Implements IEnumerable(Of Layer)

        Public ReadOnly Property Layers As Layer()
        Public ReadOnly Property Size As Integer

        ''' <summary>
        ''' 使用最后一层作为输出层
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Output As Layer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Layers(Size - 1)
            End Get
        End Property

        ''' <summary>
        ''' 在反向传播的过程中,layer之间的计算顺序是反过来的
        ''' 在这里构建这样子的一个颠倒顺序的缓存,可以减少一些不必要的操作
        ''' </summary>
        ReadOnly reverseLayers As Layer()

        Friend Sub New(layers As IEnumerable(Of Layer))
            Me.Layers = layers.ToArray
            Me.Size = Me.Layers.Length
            Me.reverseLayers = Me.Layers.Reverse.ToArray
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input">s神经网络的输入层会作为隐藏层的输入</param>
        ''' <param name="size%"></param>
        ''' <param name="active"></param>
        Sub New(input As Layer, size%(), weight As Func(Of Double), active As IActivationFunction, guid As i32)
            Dim hiddenPortal As New Layer(size(Scan0), active, weight, input, guid)

            Layers = New Layer(size.Length - 1) {}
            Layers(Scan0) = hiddenPortal

            ' 在隐藏层之中,前一层神经网络会作为后面的输出
            For i As Integer = 1 To size.Length - 1
                Layers(i) = New Layer(size(i), active, weight, input:=hiddenPortal, guid:=guid)
                hiddenPortal = Layers(i)
            Next

            Me.Size = size.Length
            Me.reverseLayers = Me.Layers.Reverse.ToArray
        End Sub

        ''' <summary>
        ''' 前向传播
        ''' </summary>
        ''' <remarks>
        ''' 因为输入的样本信息在网络之中的传播是有方向性的
        ''' 所以这个函数的layer之间不可以出现并行关系
        ''' </remarks>
        Public Sub ForwardPropagate(parallel As Boolean, truncate As Double)
            For Each layer As Layer In Layers
                ' 2018-12-19
                ' 虽然隐藏层数量比较少,但是每一个隐藏层之中,神经元节点数量可能会很多
                ' 所以下面的这个函数的调用,内部实现应该是并行的?
                Call layer.CalculateValue(parallel, truncate)
            Next
        End Sub

        Public Sub BackPropagate(learnRate#, momentum#, truncate#, parallel As Boolean)
            ' 因为在调用函数计算之后,值变了
            ' 所以在这里会需要使用两个for each
            ' 不然计算会出bug
            For Each layer As Layer In reverseLayers
                Call layer.CalculateGradient(parallel, truncate)
            Next
            For Each layer As Layer In reverseLayers
                Call layer.UpdateWeights(learnRate, momentum, truncate, parallel)
            Next
        End Sub

        Public Iterator Function GetAllNeurons() As IEnumerable(Of Neuron)
            For Each layer As Layer In Me
                For Each node As Neuron In layer
                    Yield node
                Next
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"{Size} hidden layers => {Layers.Select(Function(l) l.Neurons.Length).ToArray.GetJson }"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Layer) Implements IEnumerable(Of Layer).GetEnumerator
            For Each layer As Layer In Layers
                Yield layer
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
