#Region "Microsoft.VisualBasic::1720ebb5c7ce23f91a6b4c839286f047, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\Network.vb"

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

    '   Total Lines: 440
    '    Code Lines: 229 (52.05%)
    ' Comment Lines: 153 (34.77%)
    '    - Xml Docs: 90.85%
    ' 
    '   Blank Lines: 58 (13.18%)
    '     File Size: 18.57 KB


    '     Class Network
    ' 
    '         Properties: Activations, HiddenLayer, InputLayer, LearnRate, LearnRateDecay
    '                     Momentum, OutputLayer, Truncate
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Compute, EnsureRegressionTarget, ForwardPropagate, (+2 Overloads) Load, ParseActivation
    '                   ToString
    ' 
    '         Sub: BackPropagate, BuildViews, DoDropOut, (+2 Overloads) Save, TrainBatch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers

Namespace NeuralNetwork

    ''' <summary>
    ''' Object model for artificial neural network computation.
    ''' </summary>
    ''' <remarks>
    ''' The class uses a fully connected <see cref="ConvolutionalNN"/> as its unified internal compute kernel:
    ''' forward and backward propagation (including parameter updates) are performed by the CNN kernel. The
    ''' public members <see cref="InputLayer"/>, <see cref="HiddenLayer"/>, <see cref="OutputLayer"/> and
    ''' <see cref="Activations"/> are kept as read-only views (<see cref="NetworkLayerView"/> /
    ''' <see cref="HiddenLayersView"/>) and no legacy Layer/Neuron/Synapse data graph is maintained any more.
    ''' 
    ''' https://github.com/trentsartain/Neural-Network
    ''' </remarks>
    Public Class Network : Inherits Model

#Region "-- Properties --"
        ''' <summary>Learning rate applied by the online trainer.</summary>
        Public Property LearnRate As Double
        ''' <summary>Momentum factor applied by the online trainer.</summary>
        Public Property Momentum As Double
        ''' <summary>Truncation threshold kept for compatibility with the legacy API.</summary>
        Public Property Truncate As Double = -1

        ''' <summary>Read-only view of the input layer, its size and output derived from the CNN kernel.</summary>
        Public Property InputLayer As NetworkLayerView
        ''' <summary>Read-only view over the collection of hidden layers.</summary>
        Public Property HiddenLayer As HiddenLayersView
        ''' <summary>Read-only view of the output layer, its size and output derived from the CNN kernel.</summary>
        Public Property OutputLayer As NetworkLayerView

        ''' <summary>
        ''' 1 - <see cref="LearnRateDecay"/>
        ''' </summary>
        Dim remains As Double

        ''' <summary>Decay rate applied to <see cref="LearnRate"/> after each backward pass.</summary>
        Public Property LearnRateDecay As Double
            Get
                Return 1 - remains
            End Get
            Set(value As Double)
                remains = 1 - value
            End Set
        End Property

        ''' <summary>
        ''' Activation function configuration retained as metadata only; the actual computation is performed by
        ''' the CNN activation layers.
        ''' </summary>
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
        Private ReadOnly alg As TrainerAlgorithm
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

            alg = New AdaGradTrainer(1, 0)
            alg.SetKernel(cnn)
            alg.learning_rate = LearnRate
            alg.momentum = Momentum
        End Sub

        ''' <summary>
        ''' Creates a <see cref="Network"/> backed by a fully connected CNN kernel.
        ''' </summary>
        ''' <param name="inputSize">Number of input nodes; must be at least two.</param>
        ''' <param name="hiddenSize">Node count of each hidden layer; each value must be at least two.</param>
        ''' <param name="outputSize">Number of output nodes; must be at least one.</param>
        ''' <param name="learnRate">Initial learning rate.</param>
        ''' <param name="momentum">Momentum factor.</param>
        ''' <param name="active">
        ''' Name of the activation function used by the hidden and output layers. Accepted values are
        ''' <c>sigmoid</c> (the default), <c>relu</c>, <c>tanh</c> and <c>leakyrelu</c>.
        ''' </param>
        ''' <param name="dropOutRate">
        ''' Dropout rate in <c>[0, 1)</c>; when greater than zero a DropoutLayer is inserted after every fully
        ''' connected layer.
        ''' </param>
        ''' <param name="weightInit">
        ''' Retained for compatibility with the legacy API; weight initialization is handled by the CNN kernel
        ''' and this parameter is ignored.
        ''' </param>
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

            alg = New AdaGradTrainer(1, 0)
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
        ''' Enables dropout regularization for the hidden layers.
        ''' </summary>
        ''' <param name="percentage">Fraction of nodes to drop, in <c>[0, 1]</c>.</param>
        ''' <remarks>
        ''' The effective dropout is implemented by passing the rate to the constructor, which inserts a
        ''' <see cref="Microsoft.VisualBasic.MachineLearning.CNN.layers.DropoutLayer"/> into the CNN kernel
        ''' (active during training, disabled during inference). This method is kept for compatibility: a runtime
        ''' call only updates the rate flag, so it takes effect only when configured at construction time.
        ''' </remarks>
        Public Sub DoDropOut(Optional percentage As Double = 0.5)
            m_dropOutMode = percentage > 0
            m_dropOutRate = percentage
        End Sub

        ''' <summary>Returns a multi-line summary of the network configuration and layer sizes.</summary>
        ''' <returns>A human readable description of the network.</returns>
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
        ''' Runs a forward pass and fills the read-only layer views with the activations of every layer.
        ''' </summary>
        ''' <param name="inputs">
        ''' The input vector; values are expected to be normalized into <c>[0, 1]</c> or <c>[-1, 1]</c>.
        ''' </param>
        ''' <param name="parallel">Reserved for compatibility; the current kernel runs the pass sequentially.</param>
        ''' <returns>The <see cref="OutputLayer"/> view after the forward pass.</returns>
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
        ''' Performs one online backward pass and updates the network weights from the prediction error.
        ''' </summary>
        ''' <param name="targets">The expected (target) output vector.</param>
        ''' <param name="parallel">Reserved for compatibility; the current kernel runs the pass sequentially.</param>
        ''' <remarks>
        ''' After the backward pass only the synapse weights and the gradients are modified; the neuron output
        ''' values are left unchanged.
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
            ' 注意：CNN 训练器在目标长度为 1 时会把目标当作分类类别索引（整数），
            ' 对于单输出回归会把连续目标截断为 0/1。这里把回归目标填充为长度 2 的
            ' 数组，强制走回归（double 数组）反传分支；RegressionLayer.backward 只读取
            ' y(0)，第二个占位元素被忽略，不影响梯度计算。
            Call alg.train(db, EnsureRegressionTarget(targets), Nothing)
        End Sub

        ''' <summary>
        ''' 将单输出回归目标调整为 CNN 训练器可正确处理的格式。
        ''' 当网络输出为单神经元的回归层、且目标向量长度为 1 时，返回长度 2 的数组
        ''' （第二个元素为占位，会被 <see cref="RegressionLayer"/> 忽略），从而避免
        ''' <see cref="CNN.trainers.TrainerAlgorithm.train"/> 把目标误当作分类类别索引。
        ''' </summary>
        Private Function EnsureRegressionTarget(raw As Double()) As Double()
            If cnn IsNot Nothing AndAlso
               cnn.output.Type = LayerTypes.Regression AndAlso
               raw IsNot Nothing AndAlso
               raw.Length = 1 Then
                Return New Double(1) {raw(0), 0.0}
            End If

            Return raw
        End Function

        ''' <summary>
        ''' Computes the network output for the given <paramref name="inputs"/>.
        ''' </summary>
        ''' <param name="inputs">The input vector.</param>
        ''' <returns>The output vector of the network.</returns>
        ''' <remarks>
        ''' Note that the ANN output typically lies in <c>[0, 1]</c>, so additional encoding and decoding may be
        ''' required by the caller.
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Compute(ParamArray inputs As Double()) As Double()
            Return ForwardPropagate(inputs, parallel:=False).Output
        End Function

        ''' <summary>
        ''' Batch training entry point: iterates <paramref name="samples"/> for <paramref name="maxLoops"/> epochs
        ''' and reuses the same online (batch_size = 1) weight update as <see cref="BackPropagate"/>.
        ''' </summary>
        ''' <param name="samples">Training samples, each a tuple of (input vector, target vector).</param>
        ''' <param name="maxLoops">Maximum number of training epochs.</param>
        ''' <remarks>
        ''' The loop drives the trainer directly instead of <see cref="CNN.Trainer"/> so that no progress is written
        ''' to the console, which would fail with an "handle is invalid" IO exception on redirected or non-console hosts.
        ''' </remarks>
        Public Sub TrainBatch(samples As (input As Double(), target As Double())(), maxLoops As Integer)
            If cnn Is Nothing Then
                Return
            End If

            alg.learning_rate = LearnRate
            alg.momentum = Momentum

            For loopIter As Integer = 1 To maxLoops
                For Each s As (input As Double(), target As Double()) In samples
                    Dim db = NetworkKernel.BuildDataBlock(s.input)

                    ' 单输出回归：目标填充为长度 2 以走正确的 double 数组反传分支
                    ' （详见 EnsureRegressionTarget 与 BackPropagate）
                    Call alg.train(db, EnsureRegressionTarget(s.target), Nothing)
                Next
            Next
        End Sub
#End Region

#Region "-- Persistence (CNN binary format) --"

        ''' <summary>
        ''' Persists the underlying CNN kernel model to a file in the CNN binary format.
        ''' </summary>
        ''' <param name="path">Destination file path.</param>
        Public Overridable Sub Save(path As String)
            Using file As Stream = New FileStream(path, FileMode.Create, FileAccess.Write)
                Call Save(file)
            End Using
        End Sub

        ''' <summary>
        ''' Persists the underlying CNN kernel model to a stream in the CNN binary format.
        ''' </summary>
        ''' <param name="stream">Destination stream.</param>
        Public Overridable Sub Save(stream As Stream)
            If Not cnn Is Nothing Then
                Call SaveModelCNN.Write(cnn, stream)
            End If
        End Sub

        ''' <summary>
        ''' Loads a Network model from a CNN binary model file.
        ''' </summary>
        ''' <param name="path">Path of the CNN binary model file.</param>
        ''' <returns>The reconstructed <see cref="Network"/> model.</returns>
        Public Shared Function Load(path As String) As Network
            Using file As Stream = New FileStream(path, FileMode.Open, FileAccess.Read)
                Return Load(file)
            End Using
        End Function

        ''' <summary>
        ''' Loads a Network model from a stream that contains a CNN binary model.
        ''' </summary>
        ''' <param name="stream">Stream that contains a serialized CNN model.</param>
        ''' <returns>The reconstructed <see cref="Network"/> model.</returns>
        Public Shared Function Load(stream As Stream) As Network
            Return New Network(ReadModelCNN.Read(stream))
        End Function
#End Region
    End Class
End Namespace
