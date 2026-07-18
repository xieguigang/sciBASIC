#Region "Microsoft.VisualBasic::9798f53ed94de306a556e3e3c3f54a1f, Data_science\MachineLearning\DeepLearning\NeuralNetwork\NetworkKernel.vb"

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

    '   Total Lines: 87
    '    Code Lines: 43 (49.43%)
    ' Comment Lines: 33 (37.93%)
    '    - Xml Docs: 87.88%
    ' 
    '   Blank Lines: 11 (12.64%)
    '     File Size: 4.19 KB


    '     Module NetworkKernel
    ' 
    '         Function: BuildCNN, BuildDataBlock
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers

Namespace NeuralNetwork

    ''' <summary>
    ''' 以 CNN 的 <see cref="ConvolutionalNN"/> 全连接网络作为 NeuralNetwork 的统一计算内核。
    ''' 本模块通过 <see cref="LayerBuilder"/> 构建等价的 CNN 全连接网络，
    ''' 供 <see cref="Netz"/> 与 <see cref="Network"/> 两个公开类作为内部计算内核使用。
    ''' </summary>
    ''' <remarks>
    ''' 旧的前向/反向/权重更新计算代码（Layer/Neuron/Synapse 等）已彻底移除，
    ''' 所有数值计算均由 CNN 内核完成，对外公开接口保持不变。
    ''' </remarks>
    Public Module NetworkKernel

        ''' <summary>
        ''' 根据网络规模构建以全连接层为主体的 CNN 网络，作为统一计算内核。
        ''' </summary>
        ''' <param name="inputSize">输入节点的数量</param>
        ''' <param name="hiddenSize">每一个隐藏层的节点数量</param>
        ''' <param name="outputSize">输出节点的数量</param>
        ''' <param name="hiddenAct">隐藏层所使用的 CNN 激活层</param>
        ''' <param name="outputAct">输出层所使用的 CNN 激活层</param>
        ''' <param name="regression">
        ''' 目标为连续数值回归（默认）时使用 <see cref="RegressionLayer"/>；
        ''' 若为分类问题，则改用 <see cref="SoftMaxLayer"/>。
        ''' </param>
        ''' <param name="dropOutRate">
        ''' [0,1) 的 DropOut 比率；当大于 0 时，会在每个全连接层之后插入
        ''' <see cref="DropoutLayer"/> 以进行正则化。
        ''' </param>
        Public Function BuildCNN(inputSize%,
                               hiddenSize%(),
                               outputSize%,
                               hiddenAct As CNN.layers.Layer,
                               outputAct As CNN.layers.Layer,
                               Optional regression As Boolean = True,
                               Optional dropOutRate As Double = 0) As ConvolutionalNN
            Dim builder As New LayerBuilder

            Call builder.buildInputLayer(New Dimension(inputSize, 1), 1)

            For i As Integer = 0 To hiddenSize.Length - 1
                Call builder.buildFullyConnectedLayer(hiddenSize(i))
                Call builder.add(hiddenAct)

                If dropOutRate > 0 Then
                    Call builder.buildDropoutLayer(dropOutRate)
                End If
            Next

            Call builder.buildFullyConnectedLayer(outputSize)

            If dropOutRate > 0 Then
                Call builder.buildDropoutLayer(dropOutRate)
            End If

            If regression Then
                ' 回归任务的输出层应为线性（无激活），以允许输出取任意实数，
                ' 从而正确拟合连续数值目标（例如 a+b ∈ [0,2]）；
                ' 若在此处追加 Sigmoid 等激活，输出会被压到 (0,1)，
                ' 既无法表示大于 1 的目标，也会使训练退化为常数解。
                Call builder.buildRegressionLayer()
            Else
                Call builder.add(outputAct)
                Call builder.buildSoftmaxLayer()
            End If

            Return New ConvolutionalNN(builder)
        End Function

        ''' <summary>
        ''' 将原始输入样本写入 <see cref="DataBlock"/> 的权重向量，不做图像归一化
        ''' （避免 -0.5 偏移，从而与旧 Netz/Network 对原始实数的输入语义保持一致）。
        ''' </summary>
        Public Function BuildDataBlock(inputs As Double()) As DataBlock
            Dim db As New DataBlock(inputs.Length, 1, 1, 0)
            Call Array.ConstrainedCopy(inputs, 0, db.w, 0, inputs.Length)
            Return db
        End Function
    End Module
End Namespace
