#Region "Microsoft.VisualBasic::9de059e0c9219a5af0c76441a392924c, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Netz.vb"

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

    '   Total Lines: 280
    '    Code Lines: 226 (80.71%)
    ' Comment Lines: 6 (2.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 48 (17.14%)
    '     File Size: 10.98 KB


    '     Class Netz
    ' 
    '         Properties: Bias, HiddenLayerCount, HiddenNeuronCount, InputNeuronCount, MaxOutputNeuronIndex
    '                     Neurons, Output, OutputNeuronCount, TotalError, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: predict
    ' 
    '         Sub: addInput, adjustWeightsAndBias2, calculateError2, run, train
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.CNN.SaveModelCNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.ReadModelCNN
Imports std = System.Math

Namespace NeuralNetwork

    ''' <summary>
    ''' Neural Network for regression analysis.
    ''' 
    ''' 本类已重构为 CNN 的 <see cref="ConvolutionalNN"/> 全连接网络门面：
    ''' 所有前向传播与反向传播计算均由 CNN 内核完成，对外公开的接口保持不变。
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/brokkoli71/NeuralNetwork
    ''' </remarks>
    Public Class Netz : Inherits Model

        Public LERNRATE As Double = 0.01

        Private ReadOnly m_INPUTNEURONCOUNT As Integer
        Private ReadOnly m_HIDDENNEURONCOUNT As Integer
        Private ReadOnly m_HIDDENLAYERCOUNT As Integer
        Private ReadOnly m_OUTPUTNEURONCOUNT As Integer

        ''' <summary>
        ''' the CNN full-connected network as the unified compute kernel
        ''' </summary>
        Private ReadOnly cnn As ConvolutionalNN
        Private ReadOnly alg As SGDTrainer

        ''' <summary>
        ''' cache for the last forward propagation result
        ''' </summary>
        Private m_lastInput As Double()
        Private m_output As Double()
        Private m_lastError As Double

        ''' <summary>
        ''' 保留旧接口之中的激活函数引用（用于兼容），实际计算由 CNN 激活层完成。
        ''' 任意自定义 <paramref name="activate"/> 无法在 CNN 层体系内精确表达时，回退到 Sigmoid（与旧 Netz 默认一致）。
        ''' </summary>
        Private ReadOnly m_activate As Func(Of Double, Double)

        Public Sub New(inputNeurons As Integer, hiddenNeurons As Integer, hiddenLayers As Integer, outputNeurons As Integer, activate As Func(Of Double, Double))
            m_INPUTNEURONCOUNT = inputNeurons
            m_HIDDENNEURONCOUNT = hiddenNeurons
            m_HIDDENLAYERCOUNT = hiddenLayers
            m_OUTPUTNEURONCOUNT = outputNeurons
            m_activate = activate

            Dim hiddenSize = New Integer(hiddenLayers - 1) {}
            For i As Integer = 0 To hiddenLayers - 1
                hiddenSize(i) = hiddenNeurons
            Next

            ' 与旧 Netz 默认激活(Sigmoid.doCall)一致，使用 Sigmoid 作为 CNN 激活层
            cnn = NetworkKernel.BuildCNN(
                inputSize:=inputNeurons,
                hiddenSize:=hiddenSize,
                outputSize:=outputNeurons,
                hiddenAct:=New SigmoidLayer,
                outputAct:=New SigmoidLayer,
                regression:=True
            )

            alg = New SGDTrainer(batch_size:=1, l2_decay:=0)
            alg.SetKernel(cnn)
            alg.learning_rate = LERNRATE
            alg.momentum = 0

            m_output = New Double(outputNeurons - 1) {}
        End Sub

        ''' <summary>
        ''' 由已经加载的 CNN 内核还原 Netz 实例（供 <see cref="Load"/> 使用）
        ''' </summary>
        Friend Sub New(kernel As ConvolutionalNN)
            cnn = kernel

            m_INPUTNEURONCOUNT = cnn.input.dims.x
            m_OUTPUTNEURONCOUNT = cnn.output.out_depth

            Dim fc As New List(Of CNN.layers.Layer)
            For i As Integer = 0 To cnn.LayerNum - 1
                If cnn.Layer(i).Type = LayerTypes.FullyConnected Then
                    fc.Add(cnn.Layer(i))
                End If
            Next

            Dim hidden = New List(Of Integer)

            For i As Integer = 0 To fc.Count - 2
                ' 每个全连接层 BackPropagationResult 包含 out_depth 个 filter + 1 个 bias，
                ' 故隐藏层规模 = 结果数量 - 1
                hidden.Add(fc(i).BackPropagationResult.ToArray.Length - 1)
            Next

            m_HIDDENLAYERCOUNT = If(fc.Count - 1 > 0, fc.Count - 1, 0)
            m_HIDDENNEURONCOUNT = If(hidden.Count > 0, hidden(0), 0)

            alg = New SGDTrainer(batch_size:=1, l2_decay:=0)
            alg.SetKernel(cnn)
            alg.learning_rate = LERNRATE
            alg.momentum = 0

            m_output = New Double(m_OUTPUTNEURONCOUNT - 1) {}
        End Sub

        ''' <summary>
        ''' 构造一个与输入规模一致的数据块；直接写入原始输入，不做图像归一化（避免 -0.5 偏移）
        ''' </summary>
        Private Shared Function BuildDataBlock(inputs As Double()) As DataBlock
            Dim db As New DataBlock(inputs.Length, 1, 1, 0)
            Call Array.ConstrainedCopy(inputs, 0, db.w, 0, inputs.Length)
            Return db
        End Function

        Private Sub addInput(input As Double())
            m_lastInput = input
        End Sub

        Public Overridable Sub run(input As Double())
            Call addInput(input)

            Dim db = BuildDataBlock(input)
            m_output = cnn.forward(db, training:=Nothing).Weights
        End Sub

        Public Overridable ReadOnly Property Output As Double()
            Get
                Return m_output.Clone
            End Get
        End Property

        Public Overridable Function predict(input As Double()) As Double()
            Call run(input)
            Return Output
        End Function

        Public Overridable Sub train(input As Double(), goodOutput As Double())
            Call addInput(input)

            Dim db = BuildDataBlock(input)

            ' 先得到当前输入的前向结果（与旧 Netz 一致：Output 反映的是本次输入的前向值）
            m_output = cnn.forward(db, training:=Nothing).Weights

            ' 在线逐样本 SGD：batch_size=1 时每个样本都立即更新权重，
            ' 学习率使用 LERNRATE，动量置零（与旧 Netz 简单在线训练语义一致）
            alg.learning_rate = LERNRATE
            alg.momentum = 0

            ' 单输出回归：CNN 训练器在目标长度为 1 时会把目标当作分类类别索引（整数），
            ' 需填充为长度 2 的数组以强制走回归（double 数组）反传分支，
            ' RegressionLayer.backward 只会读取 y(0)，第二个占位被忽略。
            Dim tr = alg.train(db, EnsureRegressionTarget(goodOutput), Nothing)
            m_lastError = tr.Loss
        End Sub

        ''' <summary>
        ''' 将单输出回归目标调整为 CNN 训练器可正确处理的格式（详见
        ''' <see cref="Network.EnsureRegressionTarget"/>）。
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

        Public Overridable ReadOnly Property TotalError As Double
            Get
                ' 以最后一次训练样本的前向误差作为公开的总误差（与旧 Netz 的语义一致：Output 反映本次输入的前向值）
                Return m_lastError
            End Get
        End Property

        Private Shared Iterator Function EachFCLayer(cnn As ConvolutionalNN) As IEnumerable(Of CNN.layers.Layer)
            For i As Integer = 0 To cnn.LayerNum - 1
                If cnn.Layer(i).Type = LayerTypes.FullyConnected Then
                    Yield cnn.Layer(i)
                End If
            Next
        End Function

        Public Overridable ReadOnly Property Weights As List(Of Double)
            Get
                Dim allWeights As New List(Of Double)()

                For Each layer As CNN.layers.Layer In EachFCLayer(cnn)
                    Dim results = layer.BackPropagationResult.ToArray

                    ' 仅取权重（每一层前 out_depth 个元素为 filters），不含偏置
                    For j As Integer = 0 To results.Length - 2
                        allWeights.AddRange(results(j).Weights)
                    Next
                Next

                Return allWeights
            End Get
        End Property

        Public Overridable ReadOnly Property Neurons As List(Of Double)
            Get
                Return New List(Of Double)(m_output)
            End Get
        End Property

        Public Overridable ReadOnly Property Bias As List(Of Double)
            Get
                Dim allBias As New List(Of Double)()

                For Each layer As CNN.layers.Layer In EachFCLayer(cnn)
                    ' 偏置位于每一个全连接层 BackPropagationResult 的最后一个元素
                    Dim results = layer.BackPropagationResult.ToArray
                    allBias.AddRange(results(results.Length - 1).Weights)
                Next

                Return allBias
            End Get
        End Property

        Public Overridable ReadOnly Property HiddenLayerCount As Integer
            Get
                Return m_HIDDENLAYERCOUNT
            End Get
        End Property

        Public Overridable ReadOnly Property InputNeuronCount As Integer
            Get
                Return m_INPUTNEURONCOUNT
            End Get
        End Property

        Public Overridable ReadOnly Property HiddenNeuronCount As Integer
            Get
                Return m_HIDDENNEURONCOUNT
            End Get
        End Property

        Public Overridable ReadOnly Property OutputNeuronCount As Integer
            Get
                Return m_OUTPUTNEURONCOUNT
            End Get
        End Property

        Public Overridable ReadOnly Property MaxOutputNeuronIndex As Integer
            Get
                Dim output = m_output
                Dim maximumOutput = 0

                For i As Integer = 1 To output.Length - 1
                    If output(maximumOutput) < output(i) Then
                        maximumOutput = i
                    End If
                Next

                Return maximumOutput
            End Get
        End Property

        ''' <summary>
        ''' 将当前的 CNN 内核模型以 CNN 二进制格式持久化保存
        ''' </summary>
        Public Overridable Sub Save(path As String)
            Using file As Stream = New FileStream(path, FileMode.Create, FileAccess.Write)
                Call Save(file)
            End Using
        End Sub

        ''' <summary>
        ''' 将当前的 CNN 内核模型以 CNN 二进制格式持久化保存到流
        ''' </summary>
        Public Overridable Sub Save(stream As Stream)
            Call SaveModelCNN.Write(cnn, stream)
        End Sub

        ''' <summary>
        ''' 从 CNN 二进制模型文件之中加载 NeuralNetwork 模型
        ''' </summary>
        Public Shared Function Load(path As String) As Netz
            Using file As Stream = New FileStream(path, FileMode.Open, FileAccess.Read)
                Return Load(file)
            End Using
        End Function

        ''' <summary>
        ''' 从流之中加载 CNN 二进制模型并还原为 NeuralNetwork 模型
        ''' </summary>
        Public Shared Function Load(stream As Stream) As Netz
            Return New Netz(ReadModelCNN.Read(stream))
        End Function
    End Class
End Namespace
