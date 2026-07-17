#Region "Microsoft.VisualBasic::fdfaaa354b70a13ceb542dffa60918ad, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\Network.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

'     Class Network
' 
'         Properties: Activations, HiddenLayer, InputLayer, LearnRate, LearnRateDecay
'                     Momentum, OutputLayer, Truncate
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: Compute, ForwardPropagate, ToString
' 
'         Sub: BackPropagate, DoDropOut, TrainBatch
' 
' 

' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports System.Runtime.CompilerServices
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.MachineLearning.CNN.SaveModelCNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.ReadModelCNN
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Namespace NeuralNetwork

    ''' <summary>
    ''' 人工神经网络计算用的对象模型。
    ''' 
    ''' 本类以 CNN 的 <see cref="ConvolutionalNN"/> 全连接网络为统一的内部计算内核：
    ''' 前向传播与反向传播（含参数更新）均由 CNN 内核完成；
    ''' 对外公开的 <see cref="InputLayer"/> / <see cref="HiddenLayer"/> / <see cref="OutputLayer"/> /
    ''' <see cref="Activations"/> 等以只读视图（<see cref="NetworkLayerView"/> / <see cref="HiddenLayersView"/>）形式保留，
    ''' 不再维护任何遗留的 Layer/Neuron/Synapse 数据图。
    ''' </summary>
    ''' <remarks>
    ''' > https://github.com/trentsartain/Neural-Network
    ''' </remarks>
    Public Class Network : Inherits Model

#Region "-- Properties --"
        Public Property LearnRate As Double
        Public Property Momentum As Double
        Public Property Truncate As Double = -1

        ''' <summary>
        ''' 输入层的只读视图（从 CNN 内核派生规模与输出）
        ''' </summary>
        ''' <returns></returns>
        Public Property InputLayer As NetworkLayerView
        ''' <summary>
        ''' 隐藏层集合的只读视图
        ''' </summary>
        ''' <returns></returns>
        Public Property HiddenLayer As HiddenLayersView
        ''' <summary>
        ''' 输出层的只读视图（从 CNN 内核派生规模与输出）
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputLayer As NetworkLayerView

        ''' <summary>
        ''' 1 - <see cref="LearnRateDecay"/>
        ''' </summary>
        Dim remains As Double

        ''' <summary>
        ''' 学习率的衰减速率
        ''' </summary>
        ''' <returns></returns>
        Public Property LearnRateDecay As Double
            Get
                Return 1 - remains
            End Get
            Set(value As Double)
                remains = 1 - value
            End Set
        End Property

        ''' <summary>
        ''' 激活函数配置（仅作为元数据保留，无实际计算功能；计算由 CNN 激活层完成）
        ''' </summary>
        ''' <returns></returns>
        Public Property Activations As IReadOnlyDictionary(Of String, String)
#End Region

#Region "-- CNN Kernel --"
        ''' <summary>
        ''' 作为统一计算内核的 CNN 全连接网络
        ''' </summary>
        Private ReadOnly cnn As ConvolutionalNN
        ''' <summary>
        ''' 用于在线逐样本训练的 CNN 训练器（batch_size=1，复现旧的在线 SGD 语义）
        ''' </summary>
        Private ReadOnly alg As SGDTrainer
        ''' <summary>
        ''' 缓存上一次前向传播所使用的输入样本
        ''' </summary>
        Private m_lastInput As Double()
        Private m_inputSize As Integer
        Private m_hiddenSize As Integer()
        Private m_outputSize As Integer
        ''' <summary>
        ''' DropOut 配置（仅在构建期通过 dropOutRate 插入 DropoutLayer 时生效）
        ''' </summary>
        Private m_dropOutMode As Boolean
        Private m_dropOutRate As Double
#End Region

        ''' <summary>
        ''' 由已经加载的 CNN 内核还原 Network（供 <see cref="Load"/> 使用）
        ''' </summary>
        Friend Sub New(kernel As ConvolutionalNN)
            cnn = kernel

            ' 从 CNN 内核推导网络规模
            m_inputSize = cnn.input.dims.x

            Dim fcLayers As New List(Of CNN.layers.Layer)
            For i As Integer = 0 To cnn.LayerNum - 1
                If cnn.Layer(i).Type = LayerTypes.FullyConnected Then
                    fcLayers.Add(cnn.Layer(i))
                End If
            Next

            m_hiddenSize = New Integer(fcLayers.Count - 2) {}
            For k As Integer = 0 To m_hiddenSize.Length - 1
                m_hiddenSize(k) = fcLayers(k).BackPropagationResult.ToArray.Length - 1
            Next
            m_outputSize = fcLayers(fcLayers.Count - 1).BackPropagationResult.ToArray.Length - 1

            Call BuildViews(m_inputSize, m_hiddenSize, m_outputSize)

            Me.LearnRate = 0.1
            Me.Momentum = 0.9
            Me.LearnRateDecay = 0.00000001
            Me.Activations = New Dictionary(Of String, String) From {
                {"input", "sigmoid"},
                {"hiddens", "sigmoid"},
                {"output", "sigmoid"}
            }

            alg = New SGDTrainer(batch_size:=1, l2_decay:=0)
            alg.SetKernel(cnn)
            alg.learning_rate = LearnRate
            alg.momentum = Momentum
        End Sub

        ''' <summary>
        ''' 根据网络规模构造一个以 CNN 全连接网络为内核的 <see cref="Network"/>。
        ''' </summary>
        ''' <param name="inputSize">``>=2``</param>
        ''' <param name="hiddenSize">``>=2``</param>
        ''' <param name="outputSize">``>=1``</param>
        ''' <param name="learnRate"></param>
        ''' <param name="momentum"></param>
        ''' <param name="active">
        ''' 隐藏层/输出层所使用的激活函数名称，可取值：``sigmoid``（默认）、``relu``、``tanh``、``leakyrelu``。
        ''' </param>
        ''' <param name="dropOutRate">
        ''' [0,1) 的 DropOut 比率；当大于 0 时会在每个全连接层之后插入 DropoutLayer。
        ''' </param>
        ''' <param name="weightInit">
        ''' （保留以兼容旧接口）权重初始化由 CNN 内核负责，此参数不再被使用。
        ''' </param>
        ''' <remarks>
        ''' 会在创建的时候赋值一个 guid
        ''' </remarks>
        Public Sub New(inputSize%, hiddenSize%(), outputSize%,
                       Optional learnRate# = 0.1,
                       Optional momentum# = 0.9,
                       Optional active As String = "sigmoid",
                       Optional dropOutRate As Double = 0,
                       Optional weightInit As Func(Of Double) = Nothing)

            Dim actName = If(active Is Nothing, "sigmoid", active).ToLower

            Me.LearnRate = learnRate
            Me.Momentum = momentum
            Me.LearnRateDecay = 0.00000001
            Me.m_dropOutMode = dropOutRate > 0
            Me.m_dropOutRate = dropOutRate
            Me.Activations = New Dictionary(Of String, String) From {
                {"input", "sigmoid"},
                {"hiddens", actName},
                {"output", actName}
            }

            Call BuildViews(inputSize, hiddenSize, outputSize)

            ' 以 CNN 全连接网络作为统一计算内核，替换旧的矩阵计算
            cnn = NetworkKernel.BuildCNN(
                inputSize:=inputSize,
                hiddenSize:=hiddenSize,
                outputSize:=outputSize,
                hiddenAct:=ParseActivation(actName),
                outputAct:=ParseActivation(actName),
                regression:=True,
                dropOutRate:=dropOutRate
            )

            alg = New SGDTrainer(batch_size:=1, l2_decay:=0)
            alg.SetKernel(cnn)
            alg.learning_rate = learnRate
            alg.momentum = momentum
        End Sub

        ''' <summary>
        ''' 根据网络规模构建只读视图对象（InputLayer / HiddenLayer / OutputLayer）
        ''' </summary>
        Private Sub BuildViews(inputSize%, hiddenSize%(), outputSize%)
            InputLayer = New NetworkLayerView(inputSize)

            Dim hViews As New List(Of NetworkLayerView)
            For Each h As Integer In hiddenSize
                Call hViews.Add(New NetworkLayerView(h))
            Next
            HiddenLayer = New HiddenLayersView(hViews)

            OutputLayer = New NetworkLayerView(outputSize)

            m_inputSize = inputSize
            m_hiddenSize = hiddenSize
            m_outputSize = outputSize
        End Sub

        ''' <summary>
        ''' 将激活函数名称映射为 CNN 的激活层对象
        ''' </summary>
        Private Shared Function ParseActivation(name As String) As CNN.layers.Layer
            Select Case name.ToLower
                Case "relu", "rectifiedlinearunits"
                    Return New RectifiedLinearUnitsLayer
                Case "tanh"
                    Return New TanhLayer
                Case "leakyrelu", "leakyrectifiedlinearunits"
                    Return New LeakyReluLayer
                Case Else
                    ' Sigmoid 以及其它未知激活都回退到 Sigmoid
                    Return New SigmoidLayer
            End Select
        End Function

        ''' <summary>
        ''' 随机失活隐藏层之中的一部分神经元节点（DropOut 正则化）。
        ''' </summary>
        ''' <param name="percentage">
        ''' [0,1] 之间，表示被随机删除的节点数量百分比。
        ''' </param>
        ''' <remarks>
        ''' 真正的 DropOut 通过在构造期传入 <paramref name="dropOutRate"/> 参数、
        ''' 在 CNN 内核之中插入 <see cref="Microsoft.VisualBasic.MachineLearning.CNN.layers.DropoutLayer"/> 实现
        ''' （训练时生效，推理时自动关闭）。本方法保留为兼容接口：运行时调用仅更新比率标记，
        ''' 需要在构造期配置方能在后续训练中生效。
        ''' </remarks>
        Public Sub DoDropOut(Optional percentage As Double = 0.5)
            m_dropOutMode = percentage > 0
            m_dropOutRate = percentage
        End Sub

        Public Overrides Function ToString() As String
            Dim summary As New StringBuilder

            Call summary.AppendLine($"learnRate:={LearnRate}")
            Call summary.AppendLine($"momentum:={Momentum}")

            Call summary.AppendLine()
            Call summary.AppendLine("input layer:")
            Call summary.AppendLine("active function using: " & Activations!input)
            Call summary.AppendLine(InputLayer.ToString)
            Call summary.AppendLine("hiddens layer:")
            Call summary.AppendLine("active function using: " & Activations!hiddens)
            Call summary.AppendLine(HiddenLayer.ToString)
            Call summary.AppendLine()

            For Each layer As NetworkLayerView In HiddenLayer
                Call summary.AppendLine($"   {layer.ToString}")
            Next

            Call summary.AppendLine()
            Call summary.AppendLine("output layer:")
            Call summary.AppendLine("active function using: " & Activations!output)
            Call summary.AppendLine(OutputLayer.ToString)

            Return summary.ToString
        End Function

#Region "ANN compute"

        ''' <summary>
        ''' 这个函数会返回<see cref="OutputLayer"/>。
        ''' </summary>
        ''' <param name="inputs">
        ''' 神经网路的输入层的输入数据,应该都是被归一化为[0,1]或者[-1,1]这两个区间内了的
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function just calculate the network value
        ''' </remarks>
        Public Function ForwardPropagate(inputs As Double(), parallel As Boolean) As NetworkLayerView
            If cnn Is Nothing Then
                Return OutputLayer
            End If

            m_lastInput = inputs

            Dim db = NetworkKernel.BuildDataBlock(inputs)

            ' 逐层前向传播，将各层（输入/隐藏/输出）的激活向量填充到只读视图
            Dim act = cnn.Layer(0).forward(db, training:=Nothing)
            InputLayer.Output = act.Weights

            Dim hIdx As Integer = 0
            For i As Integer = 1 To cnn.LayerNum - 1
                act = cnn.Layer(i).forward(act, training:=Nothing)

                If cnn.Layer(i).Type = LayerTypes.FullyConnected Then
                    If hIdx < HiddenLayer.Count Then
                        HiddenLayer(hIdx).Output = act.Weights
                        hIdx += 1
                    Else
                        OutputLayer.Output = act.Weights
                    End If
                End If
            Next

            Return OutputLayer
        End Function

        ''' <summary>
        ''' adjust neuron weight based on the error.(反向传播)
        ''' </summary>
        ''' <param name="targets"></param>
        ''' <remarks>
        ''' 在反向传播之后,网络只会修改节点之间的突触边链接的权重值以及节点
        ''' 的梯度值,没有修改节点的输出值.
        ''' </remarks>
        Public Sub BackPropagate(targets As Double(), parallel As Boolean)
            If cnn Is Nothing Then
                Return
            End If

            LearnRate = LearnRate * remains

            alg.learning_rate = LearnRate
            alg.momentum = Momentum

            Dim db = NetworkKernel.BuildDataBlock(m_lastInput)

            ' 在线逐样本训练：batch_size=1 时每个样本都立即更新权重，
            ' 与旧 Network 的在线反向传播语义一致
            Call alg.train(db, targets, Nothing)
        End Sub

        ''' <summary>
        ''' Compute result output for the neuron network <paramref name="inputs"/>.
        ''' (请注意ANN的输出值是在0-1之间的，所以还需要进行额外的编码和解码)
        ''' </summary>
        ''' <param name="inputs"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Compute(ParamArray inputs As Double()) As Double()
            Return ForwardPropagate(inputs, parallel:=False).Output
        End Function

        ''' <summary>
        ''' 批量训练入口：将 <paramref name="samples"/> 转换为 CNN 的
        ''' <see cref="SampleData"/> 之后，调用 <see cref="CNN.Trainer.train"/> 完成批量 SGD 训练；
        ''' 在线的逐样本训练请见 <see cref="BackPropagate"/>。
        ''' </summary>
        ''' <param name="samples">训练样本集合，元素为 (输入向量, 目标向量) 元组</param>
        ''' <param name="maxLoops">批量训练的迭代（轮）数上限</param>
        Public Sub TrainBatch(samples As (input As Double(), target As Double())(), maxLoops As Integer)
            If cnn Is Nothing Then
                Return
            End If

            Dim sampleData As New List(Of SampleData)

            For Each s As (input As Double(), target As Double()) In samples
                ' CNN 的 Trainer.train 内部会按列做数据集归一化
                Call sampleData.Add(New SampleData(s.input, s.target))
            Next

            Dim sgd As New SGDTrainer(batch_size:=1, l2_decay:=0)
            Call sgd.SetKernel(cnn)
            sgd.learning_rate = LearnRate
            sgd.momentum = Momentum

            Dim trainer As New Microsoft.VisualBasic.MachineLearning.CNN.Trainer(sgd)
            Call trainer.train(sampleData.ToArray, maxLoops)
        End Sub
#End Region

#Region "-- Persistence (CNN binary format) --"

        ''' <summary>
        ''' 将当前的 CNN 内核模型以 CNN 二进制格式持久化保存
        ''' </summary>
        Public Overridable Sub Save(path As String)
            Using file As Stream = New FileStream(path, FileMode.Create, FileAccess.Write)
                Call Save(file)
            End Using
        End Sub

        ''' <summary>
        ''' 将当前的 CNN 内核模型以 CNN 二进制格式保存到流
        ''' </summary>
        Public Overridable Sub Save(stream As Stream)
            If Not cnn Is Nothing Then
                Call SaveModelCNN.Write(cnn, stream)
            End If
        End Sub

        ''' <summary>
        ''' 从 CNN 二进制模型文件之中加载 NeuralNetwork 模型
        ''' </summary>
        Public Shared Function Load(path As String) As Network
            Using file As Stream = New FileStream(path, FileMode.Open, FileAccess.Read)
                Return Load(file)
            End Using
        End Function

        ''' <summary>
        ''' 从流之中加载 CNN 二进制模型并还原为 NeuralNetwork 模型
        ''' </summary>
        Public Shared Function Load(stream As Stream) As Network
            Return New Network(ReadModelCNN.Read(stream))
        End Function
#End Region
    End Class
End Namespace
