#Region "Microsoft.VisualBasic::36b9eee47f98a54170bf995ce3592d7c, Data_science\MachineLearning\DeepLearning\RNN\net\RNNLayer.vb"

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

    '   Total Lines: 445
    '    Code Lines: 202 (45.39%)
    ' Comment Lines: 152 (34.16%)
    '    - Xml Docs: 40.13%
    ' 
    '   Blank Lines: 91 (20.45%)
    '     File Size: 15.29 KB


    '     Class RNNLayer
    ' 
    '         Properties: HiddenSize, InputSize, LearningRate, OutputSize
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: getdx, getdy, getLoss, getp, getProbabilities
    '                   gety, (+2 Overloads) ixTox, saveHiddenState
    ' 
    '         Sub: backward, (+2 Overloads) forward, initialize, restoreHiddenState
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow.Math
Imports std = System.Math

Namespace RNN

    ''' <summary>
    ''' An RNN Layer with support for multi-layer networks.
    ''' </summary>
    <Serializable>
    Public Class RNNLayer

        ' Dimensions

        Private m_inputSize As Integer ' input vector size
        Private m_hiddenSize As Integer ' hidden state size
        Private m_outputSize As Integer ' input vector size

        ' Defaults

        ''' <summary>Default input vector size.</summary>
        Public Const defaultInputSize As Integer = 50
        ''' <summary>Default hidden state size.</summary>
        Public Const defaultHiddenSize As Integer = 100
        ''' <summary>Default output vector size.</summary>
        Public Const defaultOutputSize As Integer = 50
        ''' <summary>Default learning rate.</summary>
        Public Const defaultLearningRate As Double = 0.1

        ' Network state
        '
        ' 全部状态改用 TensorFlow 的 Tensor（双精度），自研的 RNN.Matrix 已被移除。
        '
        ' 形状约定（**列向量**）：
        '     x[t] : (inputSize, 1)
        '     h[t] : (hiddenSize, 1)
        '     y[t] : (outputSize, 1)
        '     Wxh  : (hiddenSize, inputSize)
        '     Whh  : (hiddenSize, hiddenSize)
        '     Why  : (outputSize, hiddenSize)
        '     bh   : (hiddenSize, 1)
        '     by   : (outputSize, 1)
        ' 这样每一步的矩阵乘维度都是天然匹配的，不需要 Matrix 时代那种"向量自动转置"。

        Private Wxh As Tensor ' input layer weights
        Private Whh As Tensor ' hidden layer weights
        Private Why As Tensor ' output layer weights
        Private bh As Tensor ' hidden bias
        Private by As Tensor ' output bias

        Private h As Tensor ' last hidden state

        ' Training state

        Private gWxh As Tensor ' gradient descent params: input layer
        Private gWhh As Tensor ' gradient descent params: hidden layer
        Private gWhy As Tensor ' gradient descent params: output layer
        Private gbh As Tensor ' gradient descent params: hidden bias
        Private gby As Tensor ' gradient descent params: output bias

        Private xAt As Tensor() ' input vectors through time
        Private hAt As Tensor() ' hidden state vectors through time
        Private yAt As Tensor() ' unnormalized output probability vectors through
        ' time
        Private pAt As Tensor() ' normalized output probability vectors through time
        Private dxAt As Tensor() ' output gradient from a backwards pass

        Private lastSequenceLength As Integer ' Number of steps in the last forward pass
        ' (must match the steps for the backward
        ' pass)

        Private initialized As Boolean

        ''' <summary>
        ''' Init: Creates a net with default parameters.
        ''' </summary>
        Public Sub New()
            m_inputSize = defaultInputSize
            m_hiddenSize = defaultHiddenSize
            m_inputSize = defaultOutputSize

            _LearningRate = defaultLearningRate
        End Sub

        ''' <summary>
        ''' Creates a net with custom parameters.
        ''' </summary>
        ''' <param name="inputSize">Size of the input vector.</param>
        ''' <param name="hiddenSize">Size of the hidden state.</param>
        ''' <param name="outputSize">Size of the output vector.</param>
        ''' <param name="learningRate">The learning rate.</param>
        Public Sub New(inputSize As Integer, hiddenSize As Integer, outputSize As Integer, learningRate As Double)
            m_inputSize = inputSize
            m_hiddenSize = hiddenSize
            m_outputSize = outputSize

            _LearningRate = learningRate
        End Sub

        ''' <summary>
        ''' Set the input size.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property InputSize As Integer
            Set(value As Integer)
                initialized = False
                m_inputSize = value
            End Set
            Get
                Return m_inputSize
            End Get
        End Property

        ''' <summary>
        ''' Set the hidden size.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property HiddenSize As Integer
            Set(value As Integer)
                initialized = False

                m_hiddenSize = value
            End Set
            Get
                Return m_hiddenSize
            End Get
        End Property

        ''' <summary>
        ''' Set the output size.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property OutputSize As Integer
            Set(value As Integer)
                initialized = False

                m_outputSize = value
            End Set
            Get
                Return m_outputSize
            End Get
        End Property

        ''' <summary>
        ''' Backpropagation parameter.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Hyperparameters
        ''' </remarks>
        Public Overridable Property LearningRate As Double

        ''' <summary>
        ''' Initialize the net with random weights.
        ''' </summary>
        Public Overridable Sub initialize()

            ' create weight matrices

            Dim scale = 0.1

            Wxh = tf.multiply_scalar(Random.randn(m_hiddenSize, m_inputSize), scale)
            Whh = tf.multiply_scalar(Random.randn(m_hiddenSize, m_hiddenSize), scale)
            Why = tf.multiply_scalar(Random.randn(m_outputSize, m_hiddenSize), scale)
            bh = Col(m_hiddenSize, 0.0)
            by = Col(m_outputSize, 0.0)

            gWxh = ZerosLike(Wxh)
            gWhh = ZerosLike(Whh)
            gWhy = ZerosLike(Why)
            gbh = ZerosLike(bh)
            gby = ZerosLike(by)

            h = Random.randn(m_hiddenSize)

            initialized = True
        End Sub

        ' ---------------- 形状辅助 ----------------
        '
        ' 原 Matrix 用 zeros(M,N) / zeros(k) / zerosLike / oneHot 这些工厂方法隐藏形状细节，
        ' 且带"行向量还是列向量"的隐式约定。改用 Tensor 之后形状必须显式，
        ' 因此这里把四个常用构造收成具名的小工具，避免散落的字面量形状写错。

        ''' <summary>
        ''' 构造长度为 <paramref name="length"/> 的张量数组（元素为 Nothing）。
        ''' </summary>
        ''' <remarks>
        ''' 为什么不直接写 <c>New Tensor(n) {}</c>：<see cref="Tensor"/> 带一个
        ''' <c>New(ParamArray shape As Integer())</c> 构造函数，于是
        ''' <c>New Tensor(n) {}</c> 会被编译器当成"调用该构造函数"而不是"建数组"
        ''' （报 BC30106 索引超出维度）。显式声明数组类型可以彻底绕开这个歧义。
        ''' </remarks>
        Private Shared Function NewArray(length As Integer) As Tensor()
            Dim arr(length - 1) As Tensor
            Return arr
        End Function

        ''' <summary>构造一个 (n, 1) 的列向量，全部元素为 <paramref name="value"/></summary>
        Private Shared Function Col(n As Integer, value As Double) As Tensor
            Dim data(n - 1) As Double

            For i = 0 To n - 1
                data(i) = value
            Next

            Return New Tensor(data, n, 1)
        End Function

        ''' <summary>构造一个与 <paramref name="template"/> 同形的全零张量</summary>
        ''' <remarks>参数名不能叫 like —— Like 是 VB 保留关键字。</remarks>
        Private Shared Function ZerosLike(template As Tensor) As Tensor
            Return Tensor.Zeros(template.Shape)
        End Function

        ''' <summary>构造一个与 <paramref name="template"/> 同形的全一张量</summary>
        Private Shared Function OnesLike(template As Tensor) As Tensor
            Return Tensor.Ones(template.Shape)
        End Function

        ''' <summary>
        ''' 张量快照。
        ''' </summary>
        ''' <remarks>
        ''' 原 Matrix 的算术方法是**原地**的，旧代码靠 <c>New Matrix(x)</c> 到处做防御性拷贝。
        ''' Tensor 的算子返回新张量，正常路径已不需要拷贝；
        ''' 但"把上一步的状态存进 <c>hAt(t)</c> 这种会被后续步骤覆盖的槽位"仍需一次快照，
        ''' 否则 <c>hAt(0)</c> 会与 <c>h</c> 共享同一份数据。
        ''' </remarks>
        Private Shared Function CloneOf(t As Tensor) As Tensor
            Return Tensor.Wrap(CType(t.Data.Clone(), Double()), t.Shape)
        End Function

        ''' <summary>one-hot 列向量 (size, 1)，第 <paramref name="index"/> 位为 1</summary>
        ''' <remarks>函数名不能叫 OneHot —— VB 不区分大小写，会与局部变量 oneHot 撞名。</remarks>
        Private Shared Function HotVector(size As Integer, index As Integer) As Tensor
            Dim data(size - 1) As Double
            data(index) = 1.0
            Return New Tensor(data, size, 1)
        End Function

        ' 
        ' 		    Converts the indices (x's) of a sequence to one-hot vectors
        ' 		    for use as the x parameter for the first layer of a network.
        ' 	
        ' 		    Requirement: ix can't be null, or empty. ix[i] < inputSize
        ' 		
        Friend Overridable Function ixTox(ix As Integer()) As Tensor()
            ' start at t = 1
            ' 长度必须是 ix.Length + 1：下标 0 位留空不用，序列从 t = 1 开始
            ' （旧代码的 New Matrix(ix.Length + 1 - 1) {} 在 VB 里同样表示 ix.Length + 1 个元素）
            Dim oneHot As Tensor() = NewArray(ix.Length + 1)
            For t = 1 To ix.Length + 1 - 1
                oneHot(t) = HotVector(m_inputSize, ix(t - 1))
            Next

            Return oneHot
        End Function

        ''' <summary>
        ''' Like ixTox, but a single index instead of an array
        ''' </summary>
        ''' <param name="ix"></param>
        ''' <returns></returns>
        Friend Overridable Function ixTox(ix As Integer) As Tensor
            Return HotVector(m_inputSize, ix)
        End Function

        ' 
        ' 		    Training forward pass.
        ' 	
        ' 		    TRAINING
        ' 	
        ' 		    Takes x as input. For the first layer, x[t] should be a one-hot vector,
        ' 		    for the others it's the output of the previous layer.
        ' 	
        ' 		    Yields y: unnormalized probabilities (inputs to next layers)
        ' 		          p: normalized probabilities (for sampling)
        ' 		    and dy, if provided with targets y's (input for the backward pass)
        ' 	
        ' 		    SAMPLING
        ' 	
        ' 		    Same as training, but x should be treated as a seed sequence.
        ' 	
        ' 		    Requirement:
        ' 		    x came from ixTox or the previous layer's forward pass result y.
        ' 		
        Friend Overridable Sub forward(x As Tensor())
            Dim t = 1

            ' Initialize the forward pass 

            xAt = x ' save the inputs (needed for backpropagation)

            ' Reset outputs

            lastSequenceLength = x.Length - 1

            ' hidden state vectors through time
            hAt = NewArray(lastSequenceLength + 1)

            ' normalized probability vectors through time
            pAt = NewArray(lastSequenceLength + 1)

            ' normalized probability vectors through time
            yAt = NewArray(lastSequenceLength + 1)

            ' copy the current state。
            ' 注意：原 Matrix 的算术方法是**原地**的，所以旧代码到处用 New Matrix(x) 做防御性拷贝；
            ' Tensor 的算子全部返回新张量，这里只需复制引用语义上的"快照"即可。
            hAt(0) = CloneOf(h)

            ' Forward pass 
            t = 1

            While t < lastSequenceLength + 1
                ' find the new hidden state: tanh(Wxh·x[t] + Whh·h[t-1] + bh)
                Dim pre = tf.add(tf.add(Wxh.MatMul(xAt(t)), Whh.MatMul(hAt(t - 1))), bh)
                hAt(t) = tf.tanh(pre)

                ' find unnormalized output probabilities
                yAt(t) = tf.add(Why.MatMul(hAt(t)), by)

                ' normalize output probabilities
                pAt(t) = Math.softmax(yAt(t))
                Threading.Interlocked.Increment(t)
            End While

            ' Update the hidden state 

            h = hAt(lastSequenceLength)
        End Sub

        ''' <summary>
        ''' Forward pass for a single seed.
        ''' </summary>
        ''' <param name="x"></param>
        Friend Overridable Sub forward(x As Tensor)
            Dim xa As Tensor() = NewArray(2)
            xa(1) = x
            forward(xa)
        End Sub

        ' 
        ' 		    Calculates the cross-entropy loss of the last forward pass
        ' 		    given target outputs.
        ' 	
        ' 		    iy - the target indices
        ' 	
        ' 		    Requirements: iy must be the size of the last sequence length, iy[i] <
        ' 		   outputSize
        ' 		
        Friend Overridable Function getLoss(iy As Integer()) As Double
            Dim loss = 0.0
            Dim t = 1

            While t < lastSequenceLength + 1
                ' 列向量 (outputSize, 1) 的扁平下标就是类别编号
                loss += -std.Log(pAt(t).Data(iy(t - 1))) ' calculate the cross-entropy loss
                Threading.Interlocked.Increment(t) ' start at t = 1
            End While

            Return loss
        End Function

        ' Returns y: the unnormalized probabilities - output of the last forward
        ' pass, starting at t = 1.
        Friend Overridable Function gety() As Tensor()
            Return yAt
        End Function

        ' Returns p: the normalized probabilities - output of the last forward
        ' pass, starting at t = 1.
        Friend Overridable Function getp() As Tensor()
            Return pAt
        End Function

        ' 
        ' 		    Returns dy: the gradients to be used as input to the last layer's
        ' 		    backward pass, starting at t = 1. given iy - the target indices.
        ' 	
        ' 		    Requirement:
        ' 		    iy must be the size of the last sequence length, iy[i] < outputSize
        ' 		
        Friend Overridable Function getdy(iy As Integer()) As Tensor()
            Dim dyAt As Tensor() = NewArray(lastSequenceLength + 1) ' start at t = 1

            Dim t = 1

            While t < lastSequenceLength + 1
                ' backprop into y,
                ' http://cs231n.github.io/neural-networks-case-study/#grad
                Dim probs As Tensor = CloneOf(pAt(t))
                Dim expected = iy(t - 1)

                probs.Data(expected) = probs.Data(expected) - 1.0
                Call probs.MarkHostModified()

                dyAt(t) = probs
                Threading.Interlocked.Increment(t)
            End While

            Return dyAt
        End Function

        ' 
        ' 		    Training backward pass.
        ' 	
        ' 		    Takes dy - the gradient to backpropagate.
        ' 	
        ' 		    Yields dx, which can be used as input to previous layer's backward pass,
        ' 		    if present, and the updated weights.
        ' 	
        ' 		    Requirement: x came from this layer's getdy() or the next layer's
        ' 		    backward pass result dx.
        ' 		
        Friend Overridable Sub backward(dy As Tensor())
            Dim t = 1

            ' Initialize backward pass 

            Dim dyAt = dy ' just an alias

            Dim dWxh = ZerosLike(Wxh)
            Dim dWhh = ZerosLike(Whh)
            Dim dWhy = ZerosLike(Why)
            Dim dbh = ZerosLike(bh)
            Dim dby = ZerosLike(by)

            Dim dhNext = ZerosLike(h)

            ' gradients to be passed to the next backwards pass
            dxAt = NewArray(lastSequenceLength + 1)

            ' Backward pass 

            t = lastSequenceLength

            While t >= 1
                ' y updates: dWhy += dy[t] · h[t]ᵀ   (out,1)×(1,hid) = (out,hid)
                dWhy = tf.add(dWhy, dyAt(t).MatMul(hAt(t).Transpose()))
                dby = tf.add(dby, dyAt(t))

                ' backprop into h and through tanh nonlinearity
                Dim dh As Tensor = tf.add(Why.Transpose().MatMul(dyAt(t)), dhNext)

                ' tanh' = 1 - h²；逐元素乘 dh
                Dim dhRaw As Tensor = tf.multiply(
                    tf.subtract(OnesLike(hAt(t)), tf.multiply(hAt(t), hAt(t))),
                    dh)

                ' h updates
                dWxh = tf.add(dWxh, dhRaw.MatMul(xAt(t).Transpose()))
                dWhh = tf.add(dWhh, dhRaw.MatMul(hAt(t - 1).Transpose()))
                dbh = tf.add(dbh, dhRaw)

                ' save dhNext for the next iteration
                dhNext = Whh.Transpose().MatMul(dhRaw)

                ' multi-layer only - save dx
                dxAt(t) = Wxh.Transpose().MatMul(dh)
                Threading.Interlocked.Decrement(t)
            End While

            ' clip exploding gradients

            Dim clip_a = -5.0
            Dim dparams As Tensor() = {dWxh, dWhh, dWhy, dbh, dby}

            For ci = 0 To dparams.Length - 1
                dparams(ci) = tf.clip_by_value(dparams(ci), clip_a, -clip_a)
            Next

            t = 1

            While t < lastSequenceLength + 1
                dxAt(t) = tf.clip_by_value(dxAt(t), clip_a, -clip_a)
                Threading.Interlocked.Increment(t)
            End While


            ' Update weights with Adagrad
            '
            ' gparam += dparam²
            ' param  += -lr · dparam / (sqrt(gparam) + eps)
            '
            ' 原实现全程**原地**修改（Matrix 的 add/mul/div 返回 Me），
            ' 这里改成"算出新值再写回字段" —— 语义等价，但要注意每个字段都要真的写回。

            Dim params As Tensor() = {Wxh, Whh, Why, bh, by}
            Dim gparams As Tensor() = {gWxh, gWhh, gWhy, gbh, gby}

            Dim i = 0

            While i < dparams.Length
                Dim dparam As Tensor = dparams(i)
                Dim gparam As Tensor = gparams(i)

                ' 二阶矩累积：g += d²
                gparam = tf.add(gparam, tf.multiply(dparam, dparam))
                gparams(i) = gparam

                Dim denom As Tensor = gparam.Apply(Function(elem As Double) std.Sqrt(elem) + 0.00000001)

                ' 局部变量不能叫 step —— Step 是 VB 保留关键字
                Dim delta As Tensor = tf.divide(tf.multiply_scalar(dparam, -LearningRate), denom)

                params(i) = tf.add(params(i), delta)
                Threading.Interlocked.Increment(i)
            End While

            ' 把更新后的参数与二阶矩写回字段
            Wxh = params(0)
            Whh = params(1)
            Why = params(2)
            bh = params(3)
            by = params(4)

            gWxh = gparams(0)
            gWhh = gparams(1)
            gWhy = gparams(2)
            gbh = gparams(3)
            gby = gparams(4)
        End Sub

        ''' <summary>
        ''' Returns dx: the gradients to be used as input to the previous layer's
        ''' backward pass.
        ''' </summary>
        ''' <returns></returns>
        Friend Overridable Function getdx() As Tensor()
            Return dxAt
        End Function

        ' * Sampling ** 


        ' Returns softmax(last y,temp) as an array of doubles: like last p, but
        ' the probabilities of the next indices are normalized using a softmax
        ' with temperature = temp in (0.0,1.0].
        Friend Overridable Function getProbabilities(temp As Double) As Double()
            Return Math.softmax(yAt(yAt.Length - 1), temp).Data
        End Function

        ''' <summary>
        ''' Save the hidden state before sampling.
        ''' </summary>
        ''' <returns></returns>
        Friend Overridable Function saveHiddenState() As Tensor
            Return CloneOf(h)
        End Function

        ''' <summary>
        ''' Restore the hidden state after sampling.
        ''' </summary>
        ''' <param name="h"></param>
        Friend Overridable Sub restoreHiddenState(h As Tensor)
            Me.h = h
        End Sub

    End Class
End Namespace
