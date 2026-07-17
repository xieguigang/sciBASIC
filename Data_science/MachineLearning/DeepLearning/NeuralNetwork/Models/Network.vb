#Region "Microsoft.VisualBasic::fdfaaa354b70a13ceb542dffa60918ad, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\Network.vb"

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

'   Total Lines: 187
'    Code Lines: 90 (48.13%)
' Comment Lines: 71 (37.97%)
'    - Xml Docs: 95.77%
' 
'   Blank Lines: 26 (13.90%)
'     File Size: 7.50 KB


'     Class Network
' 
'         Properties: Activations, HiddenLayer, InputLayer, LearnRate, LearnRateDecay
'                     Momentum, OutputLayer, Truncate
' 
'         Constructor: (+3 Overloads) Sub New
' 
'         Function: Compute, ForwardPropagate, ToString
' 
'         Sub: BackPropagate
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations

Namespace NeuralNetwork

    ''' <summary>
    ''' 人工神经网络计算用的对象模型。
    ''' 
    ''' 本类已重构为以 CNN 的 <see cref="ConvolutionalNN"/> 全连接网络为统一计算内核：
    ''' 前向传播与反向传播（含参数更新）均由 CNN 内核完成；
    ''' 对外公开的 <see cref="InputLayer"/> / <see cref="HiddenLayer"/> / <see cref="OutputLayer"/> /
    ''' <see cref="Activations"/> 等仍作为只读镜像保留，供 <see cref="StoreProcedure"/> 快照、
    ''' <see cref="ANNTrainer"/> 与 <see cref="Importance"/> 等既有消费者读取。
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
        ''' 通过这个属性可以枚举出所有的输入层的神经元节点
        ''' </summary>
        ''' <returns></returns>
        Public Property InputLayer As Layer
        ''' <summary>
        ''' 通过这个属性可以枚举出所有的隐藏层，然后对每一层隐藏层可以枚举出该隐藏层之中的所有的神经元节点
        ''' </summary>
        ''' <returns></returns>
        Public Property HiddenLayer As HiddenLayers
        ''' <summary>
        ''' 通过这个属性可以枚举出所有的输出层的神经元节点
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputLayer As Layer

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
        ''' 激活函数
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个属性在这里只是起着存储到XML模型之中的作用,并没有实际的计算功能
        ''' </remarks>
        Public Property Activations As IReadOnlyDictionary(Of String, ActiveFunction)
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
        ''' <summary>
        ''' 遗留数据图镜像（由 CNN 内核权重/数值回写）
        ''' </summary>
        Private m_graph As (input As Layer, hidden As HiddenLayers, output As Layer)
        Private ReadOnly m_inputSize As Integer
        Private ReadOnly m_hiddenSize As Integer()
        Private ReadOnly m_outputSize As Integer
        Private Shared ReadOnly dropOutRand As New System.Random
#End Region

        ''' <summary>
        ''' 这个构造函数是给XML模型加载操作所使用的（仅作为元数据壳，不构建计算图）
        ''' </summary>
        ''' <param name="activations"></param>
        Friend Sub New(activations As LayerActives)
            Me.LearnRateDecay = 0.00000001
            Me.Activations = BuildActivations(activations)
        End Sub

        Friend Sub New(activations As IReadOnlyDictionary(Of String, ActiveFunction))
            Me.LearnRateDecay = 0.00000001
            Me.Activations = activations
        End Sub

        ''' <summary>
        ''' 由已经加载的 CNN 内核还原 Network（供 <see cref="Load"/> 使用）
        ''' </summary>
        Friend Sub New(kernel As ConvolutionalNN)
            cnn = kernel

            ' 从 CNN 内核推导网络规模
            m_inputSize = cnn.input.dims.x

            Dim fcLayers As New List(Of Layer)
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

            Call BuildGraph(m_inputSize, m_hiddenSize, m_outputSize, LayerActives.GetDefaultConfig)
            Call NetworkKernel.SyncFromCNN(cnn, m_graph, m_inputSize, m_hiddenSize, m_outputSize)

            Me.LearnRate = 0.1
            Me.Momentum = 0.9
            Me.LearnRateDecay = 0.00000001
            alg = New SGDTrainer(batch_size:=1, l2_decay:=0)
            alg.SetKernel(cnn)
            alg.learning_rate = LearnRate
            alg.momentum = Momentum
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="inputSize">``>=2``</param>
        ''' <param name="hiddenSize">``>=2``</param>
        ''' <param name="outputSize">``>=1``</param>
        ''' <param name="learnRate"></param>
        ''' <param name="momentum"></param>
        ''' <remarks>
        ''' 会在创建的时候赋值一个guid
        ''' </remarks>
        Public Sub New(inputSize%, hiddenSize%(), outputSize%,
                       Optional learnRate# = 0.1,
                       Optional momentum# = 0.9,
                       Optional active As LayerActives = Nothing,
                       Optional weightInit As Func(Of Double) = Nothing)

            Dim activations As LayerActives = active Or LayerActives.GetDefaultConfig
            Dim guid As i32 = 100

            weightInit = weightInit Or Helpers.randomWeight

            Me.LearnRate = learnRate
            Me.Momentum = momentum
            Me.Activations = BuildActivations(activations)
            Me.LearnRateDecay = 0.00000001

            Call BuildGraph(inputSize, hiddenSize, outputSize, activations)

            ' 以 CNN 全连接网络作为统一计算内核，替换旧的矩阵计算
            cnn = NetworkKernel.BuildCNN(
                inputSize:=inputSize,
                hiddenSize:=hiddenSize,
                outputSize:=outputSize,
                hiddenAct:=NetworkKernel.CNNLayer(activations.hiddens),
                outputAct:=NetworkKernel.CNNLayer(activations.output),
                regression:=True
            )

            alg = New SGDTrainer(batch_size:=1, l2_decay:=0)
            alg.SetKernel(cnn)
            alg.learning_rate = learnRate
            alg.momentum = momentum

            ' 将 CNN 内核初始化的权重/偏置回写到遗留数据图镜像
            Call NetworkKernel.SyncFromCNN(cnn, m_graph, inputSize, hiddenSize, outputSize)
        End Sub

        ''' <summary>
        ''' 构建遗留的 Layer/Neuron/Synapse 数据图（仅作为镜像容器，真实计算由 CNN 内核完成）
        ''' </summary>
        Private Sub BuildGraph(inputSize%, hiddenSize%(), outputSize%, active As LayerActives)
            Dim weightInit As Func(Of Double) = Function() 0.0
            Dim guid As i32 = 100

            InputLayer = New Layer(inputSize, Nothing, weightInit, guid:=guid)
            HiddenLayer = New HiddenLayers(InputLayer, hiddenSize, weightInit, active.hiddens, guid:=guid)
            OutputLayer = New Layer(outputSize, active.output, weightInit, input:=HiddenLayer.Output, guid:=guid)

            m_inputSize = inputSize
            m_hiddenSize = hiddenSize
            m_outputSize = outputSize
            m_graph = (InputLayer, HiddenLayer, OutputLayer)
        End Sub

        ''' <summary>
        ''' 构造兼容 <see cref="StoreProcedure"/> 快照读取的激活函数字典（包含 input/hiddens/output 键）
        ''' </summary>
        Private Shared Function BuildActivations(active As LayerActives) As IReadOnlyDictionary(Of String, ActiveFunction)
            Dim a = active Or LayerActives.GetDefaultConfig
            Dim dict = a.GetXmlModels

            If Not dict.ContainsKey("input") Then
                dict.Add("input", a.hiddens.Store)
            End If

            Return dict
        End Function

        ''' <summary>
        ''' 随机失活隐藏层之中的一部分神经元节点（DropOut 正则化）。
        ''' </summary>
        ''' <param name="percentage">
        ''' [0,1] 之间，表示被随机删除的节点数量百分比。
        ''' </param>
        ''' <remarks>
        ''' 这里是保留的兼容接口：旧的对象图随机失活标记（<see cref="Neuron.isDroppedOut"/>/
        ''' <see cref="Layer.doDropOutMode"/>）仍会被维护，以便 <see cref="StoreProcedure"/> 镜像与
        ''' <see cref="Trainings.ANNTrainer"/> 调用点保持可编译；真正的计算由 CNN 内核完成，
        ''' DropOut 语义下沉到内核（构建期可按 dropOutRate 插入 DropoutLayer）。
        ''' </remarks>
        Public Sub DoDropOut(Optional percentage As Double = 0.5)
            Dim dropOutMode As Boolean = percentage > 0
            Dim p As Double = percentage

            For Each layer As Layer In HiddenLayer
                layer.doDropOutMode = dropOutMode

                For Each neuron As Neuron In layer
                    neuron.isDroppedOut = dropOutMode AndAlso (dropOutRand.NextDouble < p)
                Next
            Next
        End Sub

        Public Overrides Function ToString() As String
            Dim summary As New StringBuilder

            Call summary.AppendLine($"learnRate:={LearnRate}")
            Call summary.AppendLine($"momentum:={Momentum}")

            Call summary.AppendLine()
            Call summary.AppendLine("input layer:")
            Call summary.AppendLine("active function using: " & Activations!input.ToString)
            Call summary.AppendLine(InputLayer.ToString)
            Call summary.AppendLine("hiddens layer:")
            Call summary.AppendLine("active function using: " & Activations!hiddens.ToString)
            Call summary.AppendLine(HiddenLayer.ToString)
            Call summary.AppendLine()

            For Each layer As Layer In HiddenLayer
                Call summary.AppendLine($"   {layer.ToString}")
            Next

            Call summary.AppendLine()
            Call summary.AppendLine("output layer:")
            Call summary.AppendLine("active function using: " & Activations!output.ToString)
            Call summary.AppendLine(OutputLayer.ToString)

            Return summary.ToString
        End Function

#Region "ANN compute"

        ''' <summary>
        ''' 这个函数会返回<see cref="OutputLayer"/>
        ''' </summary>
        ''' <param name="inputs">
        ''' 神经网路的输入层的输入数据,应该都是被归一化为[0,1]或者[-1,1]这两个区间内了的
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function just calculate the network value
        ''' </remarks>
        Public Function ForwardPropagate(inputs As Double(), parallel As Boolean) As Layer
            If cnn Is Nothing Then
                Return OutputLayer
            End If

            m_lastInput = inputs

            Dim out = cnn.forward(NetworkKernel.BuildDataBlock(inputs), training:=Nothing).Weights

            ' 将 CNN 前向传播的输出数值回写到遗留数据图镜像
            Call NetworkKernel.SyncOutput(m_graph, inputs, out)

            Return OutputLayer
        End Function

        ''' <summary>
        ''' adjust neuron weight based on the error.(反向传播)
        ''' </summary>
        ''' <param name="targets"></param>
        ''' <remarks>
        ''' 在反向传播之后,网络只会修改节点之间的突触边链接的权重值以及节点
        ''' 的<see cref="Neuron.Gradient"/>值,没有修改节点的<see cref="Neuron.Value"/>值.
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

            ' 训练完成后，将更新后的 CNN 权重/偏置回写到遗留数据图镜像
            Call NetworkKernel.SyncFromCNN(cnn, m_graph, m_inputSize, m_hiddenSize, m_outputSize)
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
#End Region

#Region "-- Persistence (CNN binary format) --"

        ''' <summary>
        ''' 将当前的 CNN 内核模型以 CNN 二进制格式持久化保存
        ''' </summary>
        Public Overridable Sub Save(path As String)
            Using file As Stream = file.OpenWrite(path)
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
            Using file As Stream = file.OpenRead(path)
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
