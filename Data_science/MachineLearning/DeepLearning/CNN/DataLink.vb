#Region "Microsoft.VisualBasic::efbbd18b67f28e41a0a20dc65c52bab3, Data_science\MachineLearning\DeepLearning\CNN\DataLink.vb"

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

    '   Total Lines: 22
    '    Code Lines: 8 (36.36%)
    ' Comment Lines: 10 (45.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (18.18%)
    '     File Size: 746 B


    '     Class DataLink
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN

    ''' <summary>
    ''' Base class for layer links that hold the input and output <see cref="DataBlock"/> of a layer.
    ''' </summary>
    Public MustInherit Class DataLink

        ''' <summary>
        ''' The input and output activations of the layer.
        ''' </summary>
        ''' <remarks>
        ''' These data objects link the current layer to the next layer and therefore do not need to be persisted into
        ''' the model file.
        ''' </remarks>
        <IgnoreDataMember> Protected in_act As DataBlock
        <IgnoreDataMember> Protected out_act As DataBlock

        ''' <summary>
        ''' 反向传播时，把本层算出的"对输入的梯度"**累加**进输入块，而不是先清零再覆盖。
        ''' </summary>
        ''' <remarks>
        ''' 默认 <c>False</c>，即保持串行网络(``ConvolutionalNN``)的既有语义：
        ''' 每个激活块只有一个下游消费者，反向时先 <c>clearGradient()</c> 再写入，
        ''' 这样同一个 mini-batch 之内跨样本不会残留上一次的激活梯度。
        ''' <para>
        ''' U-Net 这类有向图必须置 <c>True</c>：同一个激活块会被"池化分支"与"跳跃连接分支"
        ''' 两个消费者同时使用，覆盖写会丢掉其中一支的贡献。置 <c>True</c> 之后，
        ''' 调用方(计算图执行器)负责在每次反向传播之前把各激活块的梯度统一清零。
        ''' </para>
        ''' <para>
        ''' 注意这一开关只影响**激活**梯度；参数梯度(卷积核/偏置)始终是跨样本累加的，
        ''' 由训练器在更新之后统一清零，与本开关无关。
        ''' </para>
        ''' </remarks>
        <IgnoreDataMember>
        Public Property AccumulateGradient As Boolean = False

        ''' <summary>
        ''' 写回本层对输入 <paramref name="target"/> 的梯度；写入方式由 <see cref="AccumulateGradient"/> 决定。
        ''' </summary>
        ''' <param name="target">本层的输入激活块（通常是 <c>in_act</c>）。</param>
        ''' <param name="values">算出的梯度数据，与 <paramref name="target"/> 同形。</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Sub WriteInputGradient(target As DataBlock, values As Double())
            If AccumulateGradient Then
                Call target.AddGradients(values)
            Else
                Call target.SetGradients(values)
            End If
        End Sub

    End Class
End Namespace
