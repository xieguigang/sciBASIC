#Region "Microsoft.VisualBasic::network_kernel_facade, Data_science\MachineLearning\DeepLearning\NeuralNetwork\NetworkKernel.vb"

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

    '   Module NetworkKernel
    ' 
    '       Function: (+2 Overloads) BuildCNN, CNNLayer, SyncFromCNN, SyncOutput
    ' 
    ' 

    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports act = Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations

Namespace NeuralNetwork

    ''' <summary>
    ''' 以 CNN 的 <see cref="ConvolutionalNN"/> 全连接网络作为 NeuralNetwork 的统一计算内核。
    ''' 本模块负责：
    ''' 
    ''' 1. 通过 <see cref="LayerBuilder"/> 构建等价的 CNN 全连接网络；
    ''' 2. 将遗留的激活函数映射为 CNN 的激活层；
    ''' 3. 在 CNN 内核完成前向/反向计算之后，将权重、偏置与输出数值
    '''    回写到遗留的 <see cref="Layer"/>/<see cref="Neuron"/>/<see cref="Synapse"/> 数据图
    '''    （供 <see cref="StoreProcedure"/> 快照、<see cref="ANNTrainer"/>、<see cref="Importance"/> 等既有消费者读取）。
    ''' 
    ''' 这样即可在"彻底使用 CNN 内核完成计算"的同时，保持 NeuralNetwork 对外公开接口的兼容性。
    ''' </summary>
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
        Public Function BuildCNN(inputSize%,
                               hiddenSize%(),
                               outputSize%,
                               hiddenAct As Layer,
                               outputAct As Layer,
                               Optional regression As Boolean = True) As ConvolutionalNN
            Dim builder As New LayerBuilder

            Call builder.buildInputLayer(New Dimension(inputSize, 1), 1)

            For i As Integer = 0 To hiddenSize.Length - 1
                Call builder.buildFullyConnectedLayer(hiddenSize(i))
                Call builder.add(hiddenAct)
            Next

            Call builder.buildFullyConnectedLayer(outputSize)
            Call builder.add(outputAct)

            If regression Then
                Call builder.buildRegressionLayer()
            Else
                Call builder.buildSoftmaxLayer()
            End If

            Return New ConvolutionalNN(builder)
        End Function

        ''' <summary>
        ''' 将遗留的 <see cref="act.IActivationFunction"/> 激活函数映射为 CNN 的激活层。
        ''' </summary>
        ''' <remarks>
        ''' 对于任意无法精确识别的激活函数，统一回退到 <see cref="SigmoidLayer"/>（与旧 Netz 的默认激活一致）。
        ''' </remarks>
        Public Function CNNLayer(active As act.IActivationFunction) As Layer
            Dim name$ = active.GetType.Name

            If name.TextEquals("RectifiedLinearUnits") OrElse
               name.TextEquals("RectifiedLinearUnitsFunction") OrElse
               name.TextEquals("ReLU") Then
                Return New RectifiedLinearUnitsLayer
            ElseIf name.TextEquals("Tanh") OrElse name.TextEquals("TanhFunction") Then
                Return New TanhLayer
            ElseIf name.TextEquals("LeakyReLU") OrElse name.TextEquals("LeakyRectifiedLinearUnits") Then
                Return New LeakyReluLayer
            Else
                ' Sigmoid 以及其它未知激活都回退到 Sigmoid
                Return New SigmoidLayer
            End If
        End Function

        ''' <summary>
        ''' 将 CNN 内核之中各个全连接层的权重与偏置，回写到遗留的数据图对象之上。
        ''' 在每一次前向传播或反向传播之后调用，即可让 <see cref="StoreProcedure"/> /
        ''' <see cref="Importance"/> / <see cref="ANNTrainer"/> 等消费者读取到最新的参数。
        ''' </summary>
        Public Sub SyncFromCNN(net As ConvolutionalNN,
                                graph As (input As Layer, hidden As HiddenLayers, output As Layer),
                                inputSize%,
                                hiddenSize%(),
                                outputSize%)
            Dim fcLayers As New List(Of Layer)

            For i As Integer = 0 To net.LayerNum - 1
                If net.Layer(i).Type = LayerTypes.FullyConnected Then
                    fcLayers.Add(net.Layer(i))
                End If
            Next

            Dim legacyLayers As Layer() = {graph.input} _
                .Join(graph.hidden.Layers) _
                .Join({graph.output}) _
                .ToArray

            For k As Integer = 0 To fcLayers.Length - 1
                Dim results = fcLayers(k).BackPropagationResult.ToArray
                Dim outDepth = results.Length - 1
                Dim layer = legacyLayers(k)

                For j As Integer = 0 To layer.Neurons.Length - 1
                    layer.Neurons(j).Bias = results(outDepth).Weights(j)

                    For kk As Integer = 0 To layer.Neurons(j).InputSynapses.Count - 1
                        layer.Neurons(j).InputSynapses(kk).Weight = results(j).Weights(kk)
                    Next
                Next
            Next
        End Sub

        ''' <summary>
        ''' 将输入样本与 CNN 前向传播的输出数值，回写到遗留数据图的神经元 <see cref="Neuron.Value"/> 之上。
        ''' </summary>
        Public Sub SyncOutput(graph As (input As Layer, hidden As HiddenLayers, output As Layer),
                                input As Double(),
                                output As Double())
            For i As Integer = 0 To graph.input.Neurons.Length - 1
                graph.input.Neurons(i).Value = input(i)
            Next

            For j As Integer = 0 To graph.output.Neurons.Length - 1
                graph.output.Neurons(j).Value = output(j)
            Next
        End Sub

        ''' <summary>
        ''' 将原始输入样本写入 <see cref="DataBlock"/> 的权重向量，不做图像归一化（避免 -0.5 偏移，
        ''' 从而与旧 Netz/Network 对原始实数的输入语义保持一致）。
        ''' </summary>
        Public Function BuildDataBlock(inputs As Double()) As DataBlock
            Dim db As New DataBlock(inputs.Length, 1, 1, 0)
            Call Array.ConstrainedCopy(inputs, 0, db.w, 0, inputs.Length)
            Return db
        End Function
    End Module
End Namespace
