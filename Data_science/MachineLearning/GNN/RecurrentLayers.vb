
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 循环神经网络层
''' 实现RNN、GRU等循环层，用于处理时间序列数据
''' </summary>
''' <summary>
''' GRU层（门控循环单元）
''' 论文: Learning Phrase Representations using RNN Encoder-Decoder (Cho et al., 2014)
''' 
''' GRU通过门控机制解决标准RNN的梯度消失问题
''' 包含两个门：更新门（z）和重置门（r）
''' 
''' 公式:
'''   z_t = σ(W_z * [h_{t-1}, x_t])       更新门
'''   r_t = σ(W_r * [h_{t-1}, x_t])       重置门
'''   h̃_t = tanh(W * [r_t * h_{t-1}, x_t])  候选隐藏状态
'''   h_t = (1 - z_t) * h_{t-1} + z_t * h̃_t  新隐藏状态
''' </summary>
Public Class GRULayer
    Inherits Layer
    ''' <summary>
    ''' 输入特征维度
    ''' </summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>
    ''' 隐藏状态维度
    ''' </summary>
    Public ReadOnly Property HiddenSize As Integer

    ''' <summary>
    ''' 更新门权重 W_z
    ''' </summary>
    Private _Wz As Tensor  ' [hiddenSize, inputSize]
    Private _Uz As Tensor  ' [hiddenSize, hiddenSize]
    Private _bz As Tensor  ' [hiddenSize]

    ''' <summary>
    ''' 重置门权重 W_r
    ''' </summary>
    Private _Wr As Tensor  ' [hiddenSize, inputSize]
    Private _Ur As Tensor  ' [hiddenSize, hiddenSize]
    Private _br As Tensor  ' [hiddenSize]

    ''' <summary>
    ''' 候选隐藏状态权重 W
    ''' </summary>
    Private _Wh As Tensor  ' [hiddenSize, inputSize]
    Private _Uh As Tensor  ' [hiddenSize, hiddenSize]
    Private _bh As Tensor  ' [hiddenSize]

    ''' <summary>
    ''' 梯度存储
    ''' </summary>
    Private _WzGrad As Tensor
    Private _UzGrad As Tensor
    Private _bzGrad As Tensor
    Private _WrGrad As Tensor
    Private _UrGrad As Tensor
    Private _brGrad As Tensor
    Private _WhGrad As Tensor
    Private _UhGrad As Tensor
    Private _bhGrad As Tensor

    ''' <summary>
    ''' 前向传播中间结果（用于反向传播）
    ''' </summary>
    Private _inputs As List(Of Tensor)      ' 每个时间步的输入
    Private _hiddenStates As List(Of Tensor) ' 每个时间步的隐藏状态
    Private _resetGates As List(Of Tensor)   ' 每个时间步的重置门
    Private _updateGates As List(Of Tensor)  ' 每个时间步的更新门
    Private _candidates As List(Of Tensor)   ' 每个时间步的候选状态

    ''' <summary>
    ''' 创建GRU层
    ''' </summary>
    ''' <param name="inputSize">输入特征维度</param>
    ''' <param name="hiddenSize">隐藏状态维度</param>
    ''' <param name="name">层名称</param>
    Public Sub New(inputSize As Integer, hiddenSize As Integer, Optional name As String = Nothing)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        MyBase.Name = If(name, $"GRU_{inputSize}_{hiddenSize}")

        ' 初始化权重（使用Xavier初始化）
        Dim stdDev = std.Sqrt(2.0 / (inputSize + hiddenSize))

        ' 更新门权重
        _Wz = Tensor.RandomNormal(New Integer() {hiddenSize, inputSize}, 0, CSng(stdDev))
        _Uz = Tensor.RandomNormal(New Integer() {hiddenSize, hiddenSize}, 0, CSng(stdDev))
        _bz = New Tensor(hiddenSize)

        ' 重置门权重
        _Wr = Tensor.RandomNormal(New Integer() {hiddenSize, inputSize}, 0, CSng(stdDev))
        _Ur = Tensor.RandomNormal(New Integer() {hiddenSize, hiddenSize}, 0, CSng(stdDev))
        _br = New Tensor(hiddenSize)

        ' 候选状态权重
        _Wh = Tensor.RandomNormal(New Integer() {hiddenSize, inputSize}, 0, CSng(stdDev))
        _Uh = Tensor.RandomNormal(New Integer() {hiddenSize, hiddenSize}, 0, CSng(stdDev))
        _bh = New Tensor(hiddenSize)

        ' 初始化梯度
        _WzGrad = New Tensor(hiddenSize, inputSize)
        _UzGrad = New Tensor(hiddenSize, hiddenSize)
        _bzGrad = New Tensor(hiddenSize)
        _WrGrad = New Tensor(hiddenSize, inputSize)
        _UrGrad = New Tensor(hiddenSize, hiddenSize)
        _brGrad = New Tensor(hiddenSize)
        _WhGrad = New Tensor(hiddenSize, inputSize)
        _UhGrad = New Tensor(hiddenSize, hiddenSize)
        _bhGrad = New Tensor(hiddenSize)

        ' 初始化中间结果存储
        _inputs = New List(Of Tensor)()
        _hiddenStates = New List(Of Tensor)()
        _resetGates = New List(Of Tensor)()
        _updateGates = New List(Of Tensor)()
        _candidates = New List(Of Tensor)()
    End Sub

    ''' <summary>
    ''' 前向传播（处理单个时间步）
    ''' </summary>
    ''' <param name="input">当前时间步的输入 [batchSize, inputSize]</param>
    ''' <param name="hiddenState">上一时间步的隐藏状态 [batchSize, hiddenSize]</param>
    ''' <returns>新的隐藏状态 [batchSize, hiddenSize]</returns>
    Public Overloads Function Forward(input As Tensor, hiddenState As Tensor) As Tensor
        Dim batchSize = input.Shape(0)

        ' 确保隐藏状态形状正确
        If hiddenState Is Nothing Then
            hiddenState = New Tensor(batchSize, HiddenSize)
        End If

        ' 计算更新门: z = σ(Wz * x + Uz * h + bz)
        Dim z = Sigmoid(LinearTransform(input, hiddenState, _Wz, _Uz, _bz))

        ' 计算重置门: r = σ(Wr * x + Ur * h + br)
        Dim r = Sigmoid(LinearTransform(input, hiddenState, _Wr, _Ur, _br))

        ' 计算候选隐藏状态: h̃ = tanh(Wh * x + Uh * (r * h) + bh)
        Dim resetHidden = r.ElementwiseMultiply(hiddenState)
        Dim hTilde = Tanh(LinearTransform(input, resetHidden, _Wh, _Uh, _bh))

        ' 计算新隐藏状态: h = (1 - z) * h_prev + z * h̃
        Dim oneMinusZ = Tensor.Filled(z.Shape, 1.0F) - z
        Dim newHidden = oneMinusZ.ElementwiseMultiply(hiddenState) + z.ElementwiseMultiply(hTilde)

        Return newHidden
    End Function

    ''' <summary>
    ''' 前向传播（处理整个序列）
    ''' </summary>
    ''' <param name="sequence">输入序列 [numTimeSteps, batchSize, inputSize]</param>
    ''' <param name="initialHidden">初始隐藏状态 [batchSize, hiddenSize]（可选）</param>
    ''' <returns>
    ''' Item1: 所有时间步的隐藏状态 [numTimeSteps, batchSize, hiddenSize]
    ''' Item2: 最后时间步的隐藏状态 [batchSize, hiddenSize]
    ''' </returns>
    Public Overloads Function ForwardSequence(sequence As Tensor, Optional initialHidden As Tensor = Nothing) As (allHidden As Tensor, finalHidden As Tensor)
        If sequence.Rank <> 3 Then
            Throw New ArgumentException("输入序列必须是三维张量 [numTimeSteps, batchSize, inputSize]")
        End If

        Dim numTimeSteps = sequence.Shape(0)
        Dim batchSize = sequence.Shape(1)

        ' 清空中间结果
        _inputs.Clear()
        _hiddenStates.Clear()
        _resetGates.Clear()
        _updateGates.Clear()
        _candidates.Clear()

        ' 初始化隐藏状态
        Dim h = If(initialHidden, New Tensor(batchSize, HiddenSize))
        _hiddenStates.Add(h.Clone())

        ' 存储所有时间步的隐藏状态
        Dim allHidden = New Tensor(numTimeSteps, batchSize, HiddenSize)

        For t = 0 To numTimeSteps - 1
            ' 提取当前时间步的输入
            Dim x_t = New Tensor(batchSize, InputSize)
            For b = 0 To batchSize - 1
                For i = 0 To InputSize - 1
                    x_t(b, i) = sequence(t, b, i)
                Next
            Next

            _inputs.Add(x_t.Clone())

            ' 计算更新门
            Dim z = Sigmoid(LinearTransform(x_t, h, _Wz, _Uz, _bz))
            _updateGates.Add(z.Clone())

            ' 计算重置门
            Dim r = Sigmoid(LinearTransform(x_t, h, _Wr, _Ur, _br))
            _resetGates.Add(r.Clone())

            ' 计算候选隐藏状态
            Dim resetHidden = r.ElementwiseMultiply(h)
            Dim hTilde = Tanh(LinearTransform(x_t, resetHidden, _Wh, _Uh, _bh))
            _candidates.Add(hTilde.Clone())

            ' 计算新隐藏状态
            Dim oneMinusZ = Tensor.Filled(z.Shape, 1.0F) - z
            h = oneMinusZ.ElementwiseMultiply(h) + z.ElementwiseMultiply(hTilde)
            _hiddenStates.Add(h.Clone())

            ' 存储到输出
            For b = 0 To batchSize - 1
                For i = 0 To HiddenSize - 1
                    allHidden(t, b, i) = h(b, i)
                Next
            Next
        Next

        Return (allHidden, h)
    End Function

    ''' <summary>
    ''' 反向传播（处理整个序列）
    ''' </summary>
    ''' <param name="gradient">来自上层的梯度 [numTimeSteps, batchSize, hiddenSize]</param>
    ''' <returns>输入序列的梯度 [numTimeSteps, batchSize, inputSize]</returns>
    Public Overloads Function BackwardSequence(gradient As Tensor) As Tensor
        Dim numTimeSteps = gradient.Shape(0)
        Dim batchSize = gradient.Shape(1)

        Dim inputGradient = New Tensor(numTimeSteps, batchSize, InputSize)

        ' 从最后一个时间步开始反向传播
        Dim dhNext = New Tensor(batchSize, HiddenSize)

        For t = numTimeSteps - 1 To 0 Step -1
            ' 获取当前时间步的中间结果
            Dim x_t = _inputs(t)
            Dim h_prev = _hiddenStates(t)
            Dim z = _updateGates(t)
            Dim r = _resetGates(t)
            Dim hTilde = _candidates(t)
            Dim h = _hiddenStates(t + 1)

            ' 获取当前时间步的梯度
            Dim dh = New Tensor(batchSize, HiddenSize)
            For b = 0 To batchSize - 1
                For i = 0 To HiddenSize - 1
                    dh(b, i) = gradient(t, b, i) + dhNext(b, i)
                Next
            Next

            ' 反向传播通过GRU门
            ' dh/dh_prev = (1 - z) + z * (1 - hTilde^2) * Uh * r
            ' 这里简化实现，使用数值近似

            ' 计算各部分的梯度
            Dim oneMinusZ = Tensor.Filled(z.Shape, 1.0F) - z

            ' dh/d(hTilde) = z
            Dim dhTilde = z.ElementwiseMultiply(dh)

            ' dh/dz = hTilde - h_prev
            Dim dz = (hTilde - h_prev).ElementwiseMultiply(dh)

            ' dh/dh_prev = (1 - z)
            dhNext = oneMinusZ.ElementwiseMultiply(dh)

            ' 反向传播通过tanh
            Dim dhTildeRaw = dhTilde.ElementwiseMultiply(TanhDerivative(hTilde))

            ' 计算Wh, Uh, bh的梯度
            Dim resetHidden = r.ElementwiseMultiply(h_prev)
            AccumulateGradient(_WhGrad, dhTildeRaw, x_t)
            AccumulateGradient(_UhGrad, dhTildeRaw, resetHidden)
            AccumulateBiasGradient(_bhGrad, dhTildeRaw)

            ' dx from hTilde
            Dim dx_hTilde = BackwardLinear(dhTildeRaw, _Wh)

            ' dh_prev from hTilde (through reset gate)
            Dim dh_prev_hTilde = BackwardLinearThroughReset(dhTildeRaw, _Uh, r)

            ' 反向传播通过sigmoid (update gate)
            Dim dzRaw = dz.ElementwiseMultiply(SigmoidDerivative(z))
            AccumulateGradient(_WzGrad, dzRaw, x_t)
            AccumulateGradient(_UzGrad, dzRaw, h_prev)
            AccumulateBiasGradient(_bzGrad, dzRaw)

            ' dx from z
            Dim dx_z = BackwardLinear(dzRaw, _Wz)

            ' dh_prev from z
            Dim dh_prev_z = BackwardLinear(dzRaw, _Uz)

            ' 反向传播通过sigmoid (reset gate)
            Dim drRaw = dh_prev_hTilde.ElementwiseMultiply(SigmoidDerivative(r))
            AccumulateGradient(_WrGrad, drRaw, x_t)
            AccumulateGradient(_UrGrad, drRaw, h_prev)
            AccumulateBiasGradient(_brGrad, drRaw)

            ' dx from r
            Dim dx_r = BackwardLinear(drRaw, _Wr)

            ' dh_prev from r
            Dim dh_prev_r = BackwardLinear(drRaw, _Ur)

            ' 累加所有对h_prev的梯度
            dhNext = dhNext + dh_prev_hTilde + dh_prev_z + dh_prev_r

            ' 累加所有对x的梯度
            Dim dx_total = dx_hTilde + dx_z + dx_r
            For b = 0 To batchSize - 1
                For i = 0 To InputSize - 1
                    inputGradient(t, b, i) = dx_total(b, i)
                Next
            Next
        Next

        Return inputGradient
    End Function

    ''' <summary>
    ''' 线性变换: W * x + U * h + b
    ''' </summary>
    Private Function LinearTransform(x As Tensor, h As Tensor, W As Tensor, U As Tensor, b As Tensor) As Tensor
        Dim batchSize = x.Shape(0)
        Dim outputSize = W.Shape(0)

        Dim result = New Tensor(batchSize, outputSize)

        ' W * x
        Dim Wx = x.MatMul(W.Transpose())

        ' U * h
        Dim Uh = h.MatMul(U.Transpose())

        ' 相加
        For i = 0 To batchSize - 1
            For j = 0 To outputSize - 1
                result(i, j) = Wx(i, j) + Uh(i, j) + b(j)
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 累加梯度
    ''' </summary>
    Private Sub AccumulateGradient(grad As Tensor, delta As Tensor, input As Tensor)
        ' grad += delta^T * input
        Dim deltaT = delta.Transpose()
        Dim gradUpdate = deltaT.MatMul(input)

        For i = 0 To grad.Shape(0) - 1
            For j = 0 To grad.Shape(1) - 1
                grad(i, j) += gradUpdate(i, j)
            Next
        Next
    End Sub

    Private Sub AccumulateBiasGradient(grad As Tensor, delta As Tensor)
        ' grad += sum(delta, axis=0)
        For j = 0 To grad.Shape(0) - 1
            Dim sum As Single = 0
            For i = 0 To delta.Shape(0) - 1
                sum += delta(i, j)
            Next
            grad(j) += sum
        Next
    End Sub

    Private Function BackwardLinear(delta As Tensor, W As Tensor) As Tensor
        ' dx = delta * W
        Return delta.MatMul(W)
    End Function

    Private Function BackwardLinearThroughReset(delta As Tensor, U As Tensor, r As Tensor) As Tensor
        ' dh_prev = delta * U * r
        Dim Uh = delta.MatMul(U)
        Return Uh.ElementwiseMultiply(r)
    End Function

    ''' <summary>
    ''' Sigmoid激活函数
    ''' </summary>
    Private Function Sigmoid(x As Tensor) As Tensor
        Return x.Apply(Function(v)
                           If v > 20 Then Return 1.0F
                           If v < -20 Then Return 0.0F
                           Return CSng(1.0 / (1.0 + std.Exp(-v)))
                       End Function)
    End Function

    Private Function SigmoidDerivative(x As Tensor) As Tensor
        Return x.Apply(Function(v)
                           Dim s = If(v > 20, 1.0F, If(v < -20, 0.0F, CSng(1.0 / (1.0 + std.Exp(-v)))))
                           Return s * (1 - s)
                       End Function)
    End Function

    ''' <summary>
    ''' Tanh激活函数
    ''' </summary>
    Private Function Tanh(x As Tensor) As Tensor
        Return x.Apply(Function(v) CSng(std.Tanh(v)))
    End Function

    Private Function TanhDerivative(x As Tensor) As Tensor
        Return x.Apply(Function(v)
                           Dim t = CSng(std.Tanh(v))
                           Return 1 - t * t
                       End Function)
    End Function

    ''' <summary>
    ''' 实现基类的Forward方法（不支持，需要序列输入）
    ''' </summary>
    Public Overrides Function Forward(input As Tensor) As Tensor
        Throw New InvalidOperationException("GRU层需要序列输入，请使用ForwardSequence方法")
    End Function

    ''' <summary>
    ''' 实现基类的Backward方法（不支持，需要序列梯度）
    ''' </summary>
    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Throw New InvalidOperationException("GRU层需要序列梯度，请使用BackwardSequence方法")
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor) From {
            _Wz, _Uz, _bz,
            _Wr, _Ur, _br,
            _Wh, _Uh, _bh
        }
    End Function

    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor) From {
            _WzGrad, _UzGrad, _bzGrad,
            _WrGrad, _UrGrad, _brGrad,
            _WhGrad, _UhGrad, _bhGrad
        }
    End Function

    ''' <summary>
    ''' 重置梯度
    ''' </summary>
    Public Sub ResetGradients()
        For Each grad In GetGradients()
            For i = 0 To grad.Length - 1
                grad(i) = 0
            Next
        Next
    End Sub

    ''' <summary>
    ''' 获取初始隐藏状态
    ''' </summary>
    Public Function GetInitialHidden(batchSize As Integer) As Tensor
        Return New Tensor(batchSize, HiddenSize)
    End Function
End Class

''' <summary>
''' 简单RNN层
''' 实现基本的循环神经网络
''' h_t = tanh(W * x_t + U * h_{t-1} + b)
''' </summary>
Public Class SimpleRNNLayer
    Inherits Layer
    Public ReadOnly Property InputSize As Integer
    Public ReadOnly Property HiddenSize As Integer

    Private _W As Tensor  ' [hiddenSize, inputSize]
    Private _U As Tensor  ' [hiddenSize, hiddenSize]
    Private _b As Tensor  ' [hiddenSize]

    Private _WGrad As Tensor
    Private _UGrad As Tensor
    Private _bGrad As Tensor

    Private _inputs As List(Of Tensor)
    Private _hiddenStates As List(Of Tensor)

    Public Sub New(inputSize As Integer, hiddenSize As Integer, Optional name As String = Nothing)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        MyBase.Name = If(name, $"RNN_{inputSize}_{hiddenSize}")

        Dim stdDev = std.Sqrt(2.0 / (inputSize + hiddenSize))
        _W = Tensor.RandomNormal(New Integer() {hiddenSize, inputSize}, 0, CSng(stdDev))
        _U = Tensor.RandomNormal(New Integer() {hiddenSize, hiddenSize}, 0, CSng(stdDev))
        _b = New Tensor(hiddenSize)

        _WGrad = New Tensor(hiddenSize, inputSize)
        _UGrad = New Tensor(hiddenSize, hiddenSize)
        _bGrad = New Tensor(hiddenSize)

        _inputs = New List(Of Tensor)()
        _hiddenStates = New List(Of Tensor)()
    End Sub

    ''' <summary>
    ''' 前向传播（处理整个序列）
    ''' </summary>
    Public Overloads Function ForwardSequence(sequence As Tensor, Optional initialHidden As Tensor = Nothing) As (allHidden As Tensor, finalHidden As Tensor)
        If sequence.Rank <> 3 Then
            Throw New ArgumentException("输入序列必须是三维张量 [numTimeSteps, batchSize, inputSize]")
        End If

        Dim numTimeSteps = sequence.Shape(0)
        Dim batchSize = sequence.Shape(1)

        _inputs.Clear()
        _hiddenStates.Clear()

        Dim h = If(initialHidden, New Tensor(batchSize, HiddenSize))
        _hiddenStates.Add(h.Clone())

        Dim allHidden = New Tensor(numTimeSteps, batchSize, HiddenSize)

        For t = 0 To numTimeSteps - 1
            ' 提取当前时间步的输入
            Dim x_t = New Tensor(batchSize, InputSize)
            For b = 0 To batchSize - 1
                For i = 0 To InputSize - 1
                    x_t(b, i) = sequence(t, b, i)
                Next
            Next

            _inputs.Add(x_t.Clone())

            ' 计算: h = tanh(W * x + U * h + b)
            Dim Wx = x_t.MatMul(_W.Transpose())
            Dim Uh = h.MatMul(_U.Transpose())

            h = New Tensor(batchSize, HiddenSize)
            For i = 0 To batchSize - 1
                For j = 0 To HiddenSize - 1
                    h(i, j) = CSng(std.Tanh(Wx(i, j) + Uh(i, j) + _b(j)))
                Next
            Next

            _hiddenStates.Add(h.Clone())

            ' 存储到输出
            For b = 0 To batchSize - 1
                For i = 0 To HiddenSize - 1
                    allHidden(t, b, i) = h(b, i)
                Next
            Next
        Next

        Return (allHidden, h)
    End Function

    ''' <summary>
    ''' 反向传播（处理整个序列）
    ''' </summary>
    Public Overloads Function BackwardSequence(gradient As Tensor) As Tensor
        Dim numTimeSteps = gradient.Shape(0)
        Dim batchSize = gradient.Shape(1)

        Dim inputGradient = New Tensor(numTimeSteps, batchSize, InputSize)
        Dim dhNext = New Tensor(batchSize, HiddenSize)

        For t = numTimeSteps - 1 To 0 Step -1
            Dim x_t = _inputs(t)
            Dim h_prev = _hiddenStates(t)
            Dim h = _hiddenStates(t + 1)

            ' 获取当前时间步的梯度
            Dim dh = New Tensor(batchSize, HiddenSize)
            For b = 0 To batchSize - 1
                For i = 0 To HiddenSize - 1
                    dh(b, i) = gradient(t, b, i) + dhNext(b, i)
                Next
            Next

            ' 反向传播通过tanh
            Dim dhRaw = New Tensor(batchSize, HiddenSize)
            For i = 0 To batchSize - 1
                For j = 0 To HiddenSize - 1
                    dhRaw(i, j) = dh(i, j) * (1 - h(i, j) * h(i, j))
                Next
            Next

            ' 计算梯度
            Dim dhRawT = dhRaw.Transpose()
            Dim dW = dhRawT.MatMul(x_t)
            Dim dU = dhRawT.MatMul(h_prev)

            For i = 0 To HiddenSize - 1
                For j = 0 To InputSize - 1
                    _WGrad(i, j) += dW(i, j)
                Next
                For j = 0 To HiddenSize - 1
                    _UGrad(i, j) += dU(i, j)
                Next
                Dim sumBias As Single = 0
                For b = 0 To batchSize - 1
                    sumBias += dhRaw(b, i)
                Next
                _bGrad(i) += sumBias
            Next

            ' 计算输入梯度
            Dim dx = dhRaw.MatMul(_W)
            For b = 0 To batchSize - 1
                For i = 0 To InputSize - 1
                    inputGradient(t, b, i) = dx(b, i)
                Next
            Next

            ' 计算隐藏状态梯度
            dhNext = dhRaw.MatMul(_U)
        Next

        Return inputGradient
    End Function

    Public Overrides Function Forward(input As Tensor) As Tensor
        Throw New InvalidOperationException("RNN层需要序列输入，请使用ForwardSequence方法")
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Throw New InvalidOperationException("RNN层需要序列梯度，请使用BackwardSequence方法")
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor) From {_W, _U, _b}
    End Function

    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor) From {_WGrad, _UGrad, _bGrad}
    End Function

    Public Sub ResetGradients()
        For Each grad In GetGradients()
            For i = 0 To grad.Length - 1
                grad(i) = 0
            Next
        Next
    End Sub
End Class

''' <summary>
''' 双向RNN层包装器
''' 将单向RNN包装为双向RNN
''' </summary>
Public Class BidirectionalRNNLayer
    Inherits Layer
    Private ReadOnly _forwardLayer As GRULayer
    Private ReadOnly _backwardLayer As GRULayer
    Public ReadOnly Property HiddenSize As Integer

    Public Sub New(inputSize As Integer, hiddenSize As Integer, Optional name As String = Nothing)
        MyBase.Name = If(name, $"BiRNN_{inputSize}_{hiddenSize}")
        Me.HiddenSize = hiddenSize * 2

        _forwardLayer = New GRULayer(inputSize, hiddenSize, "forward")
        _backwardLayer = New GRULayer(inputSize, hiddenSize, "backward")
    End Sub

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public Overloads Function ForwardSequence(sequence As Tensor, Optional initialHidden As Tensor = Nothing) As (allHidden As Tensor, finalHidden As Tensor)
        Dim numTimeSteps = sequence.Shape(0)
        Dim batchSize = sequence.Shape(1)

        ' 前向传播
        Dim forwardResult = _forwardLayer.ForwardSequence(sequence, initialHidden)

        ' 反向传播（反转序列）
        Dim reversedSequence = ReverseSequence(sequence)
        Dim backwardResult = _backwardLayer.ForwardSequence(reversedSequence, initialHidden)

        ' 合并前向和反向的隐藏状态
        Dim allHidden = New Tensor(numTimeSteps, batchSize, HiddenSize)
        For t = 0 To numTimeSteps - 1
            Dim backwardT = numTimeSteps - 1 - t
            For b = 0 To batchSize - 1
                For i = 0 To HiddenSize / 2 - 1
                    allHidden(t, b, i) = forwardResult.allHidden(t, b, i)
                    allHidden(t, b, HiddenSize / 2 + i) = backwardResult.allHidden(backwardT, b, i)
                Next
            Next
        Next

        ' 最终隐藏状态
        Dim finalHidden = New Tensor(batchSize, HiddenSize)
        For b = 0 To batchSize - 1
            For i = 0 To HiddenSize / 2 - 1
                finalHidden(b, i) = forwardResult.finalHidden(b, i)
                finalHidden(b, HiddenSize / 2 + i) = backwardResult.finalHidden(b, i)
            Next
        Next

        Return (allHidden, finalHidden)
    End Function

    Private Function ReverseSequence(sequence As Tensor) As Tensor
        Dim numTimeSteps = sequence.Shape(0)
        Dim batchSize = sequence.Shape(1)
        Dim inputSize = sequence.Shape(2)

        Dim result = New Tensor(numTimeSteps, batchSize, inputSize)
        For t = 0 To numTimeSteps - 1
            Dim revT = numTimeSteps - 1 - t
            For b = 0 To batchSize - 1
                For i = 0 To inputSize - 1
                    result(t, b, i) = sequence(revT, b, i)
                Next
            Next
        Next
        Return result
    End Function

    Public Overrides Function Forward(input As Tensor) As Tensor
        Throw New InvalidOperationException("请使用ForwardSequence方法")
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Throw New InvalidOperationException("请使用BackwardSequence方法")
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Dim params = New List(Of Tensor)()
        params.AddRange(_forwardLayer.GetParameters())
        params.AddRange(_backwardLayer.GetParameters())
        Return params
    End Function

    Public Overrides Function GetGradients() As List(Of Tensor)
        Dim grads = New List(Of Tensor)()
        grads.AddRange(_forwardLayer.GetGradients())
        grads.AddRange(_backwardLayer.GetGradients())
        Return grads
    End Function
End Class
