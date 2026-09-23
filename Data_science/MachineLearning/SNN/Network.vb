#Region "Microsoft.VisualBasic::de7e00d1b7c23a6fef30ddb54f2f5cf3, Data_science\MachineLearning\SNN\Network.vb"

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

    '   Total Lines: 451
    '    Code Lines: 284 (62.97%)
    ' Comment Lines: 95 (21.06%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 72 (15.96%)
    '     File Size: 18.43 KB


    ' Structure LossGradient
    ' 
    ' 
    ' 
    ' Module Losses
    ' 
    '     Function: SoftmaxCrossEntropy
    ' 
    ' Class AdamOptimizer
    ' 
    '     Properties: Beta1, Beta2, ClipNorm, Epsilon, LearningRate
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Update
    ' 
    ' Class SpikingNetwork
    ' 
    '     Properties: Encoding, Layers, Rng, SparseInputMap, SparseLayer
    '                 TimeSteps
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Accuracy, AddLayer, (+2 Overloads) AddSparseLayer, ComputeGradients, Encode
    '               ForwardSparse, ForwardSpikes, Predict, ScatterInput, TrainStep
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' Network.vb — SNN 网络模型 + 损失函数 + Adam 优化器
'
' 信息流（readme 第四节）：
'   1. 输入编码  连续值 x[batch,f] → T 个时间步的脉冲序列 X[1..T]
'   2. 时间展开  逐时间步、逐层执行 LIF 积分-泄漏-触发-复位
'   3. 跨层传播  上层输出脉冲直接作为下层输入（无延迟）
'   4. 输出解码  累积输出层脉冲计数 count = Σ_t S_out[t] → softmax → argmax
'   5. 训练      softmax 交叉熵 + 替代梯度 BPTT + Adam（含梯度范数裁剪）
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math


''' <summary>Softmax 交叉熵的返回结构</summary>
Public Structure LossGradient
    ''' <summary>平均交叉熵损失</summary>
    Public Loss As Double
    ''' <summary>dL/dlogits（logits = 脉冲计数）</summary>
    Public Grad As Tensor
End Structure

''' <summary>损失函数库</summary>
Public Module Losses

    ''' <summary>
    ''' Softmax + 交叉熵（数值稳定：先减去行最大值）。
    ''' dL/dz_j = softmax(z)_j − onehot(y)_j
    ''' </summary>
    Public Function SoftmaxCrossEntropy(logits As Tensor, labels As Integer()) As LossGradient
        Dim batch = logits.Shape(0)
        Dim n = logits.Shape(1)
        If labels.Length <> batch Then
            Throw New ArgumentException("labels 数量与 batch 大小不一致")
        End If

        Dim grad = New Tensor(batch, n)
        Dim loss = 0.0

        For b = 0 To batch - 1
            Dim mx = Double.MinValue
            For j = 0 To n - 1
                If logits(b, j) > mx Then mx = logits(b, j)
            Next

            Dim sum = 0.0
            For j = 0 To n - 1
                sum += std.Exp(logits(b, j) - mx)
            Next
            Dim logSum = std.Log(sum)

            ' −ln softmax(z_y) = −(z_y − mx − ln Σexp(z−mx))
            loss -= (logits(b, labels(b)) - mx - logSum)

            For j = 0 To n - 1
                grad(b, j) = std.Exp(logits(b, j) - mx) / sum - If(j = labels(b), 1.0, 0.0)
            Next
        Next

        Return New LossGradient With {
            .Loss = loss / batch,
            .Grad = grad * CSng(1.0 / batch)
        }
    End Function

End Module

''' <summary>
''' Adam 优化器（含梯度 L2 范数裁剪）。
''' key 用于区分不同参数的动量状态（例如各层权重）。
''' </summary>
Public Class AdamOptimizer

    Public Property LearningRate As Double
    Public Property Beta1 As Double = 0.9
    Public Property Beta2 As Double = 0.999
    Public Property Epsilon As Double = 0.00000001

    ''' <summary>梯度 L2 范数裁剪上限（≤0 表示不裁剪）</summary>
    Public Property ClipNorm As Double

    Private _step As Long
    Private _m As New Dictionary(Of String, Double())()
    Private _v As New Dictionary(Of String, Double())()

    Public Sub New(Optional learningRate As Double = 0.002, Optional clipNorm As Double = 1.0)
        Me.LearningRate = learningRate
        Me.ClipNorm = clipNorm
    End Sub

    Public Sub Update(param As Tensor, grad As Tensor, key As String)
        Dim g = grad.ToDoubleArray()

        ' 梯度裁剪（SNN 训练中抑制脉冲稀疏引起的梯度尖峰）
        If ClipNorm > 0 Then
            Dim sq = 0.0
            For Each v In g : sq += v * v : Next
            Dim norm = std.Sqrt(sq)
            If norm > ClipNorm Then
                Dim s = ClipNorm / norm
                For i = 0 To g.Length - 1 : g(i) *= s : Next
            End If
        End If

        If Not _m.ContainsKey(key) Then
            _m(key) = New Double(g.Length - 1) {}
            _v(key) = New Double(g.Length - 1) {}
        End If

        _step += 1L
        Dim m = _m(key)
        Dim vArr = _v(key)
        Dim bc1 = 1.0 - std.Pow(Beta1, _step)
        Dim bc2 = 1.0 - std.Pow(Beta2, _step)

        For i = 0 To g.Length - 1
            m(i) = Beta1 * m(i) + (1.0 - Beta1) * g(i)
            vArr(i) = Beta2 * vArr(i) + (1.0 - Beta2) * g(i) * g(i)
            Dim mHat = m(i) / bc1
            Dim vHat = vArr(i) / bc2
            param(i) = param(i) - CSng(LearningRate * mHat / (std.Sqrt(vHat) + Epsilon))
        Next
    End Sub

End Class

''' <summary>
''' 脉冲神经网络：编码器 + 多个 LIF 层 + 时间展开 + 计数解码。
'''
''' 支持两种连接拓扑（互斥）：
'''   1. 全连接多层（<see cref="AddLayer"/>）：前馈 LIF 层栈，支持 BPTT 训练。
'''   2. 稀疏自定义连接（<see cref="AddSparseLayer(Integer(), Integer(), Double(), Integer, SparseNormalization, Double, Double, LIFResetMode, Integer())"/>）：
'''      单个大稀疏矩阵，含层内递归（循环连接与自反馈），仅支持前向仿真。
''' 后者用于加载 FlyWire 等真实突触连接组（十万级神经元/千万级突触）。
''' </summary>
Public Class SpikingNetwork

    ''' <summary>
    ''' 全连接 LIF 层栈（前馈/可训练）。与 <see cref="SparseLayer"/> 互斥：
    ''' 使用稀疏自定义连接时本集合为空。
    ''' </summary>
    Public ReadOnly Property Layers As New List(Of LIFLayer)()

    ''' <summary>
    ''' 稀疏自定义连接层（单个大稀疏矩阵，含循环连接与自反馈）。
    ''' 与 <see cref="Layers"/> 互斥；仅支持前向仿真，不参与训练。
    ''' </summary>
    Public Property SparseLayer As SparseLIFLayer

    ''' <summary>
    ''' 稀疏模式的输入注入映射：第 f 个输入特征注入到神经元 SparseInputMap(f)。
    ''' 为 Nothing 时表示特征与神经元 1:1 对应（要求 inputSize = SparseLayer.Units）。
    ''' </summary>
    Public Property SparseInputMap As Integer()

    ''' <summary>仿真时间步数 T</summary>
    Public Property TimeSteps As Integer

    ''' <summary>输入脉冲编码方式</summary>
    Public Property Encoding As SpikeEncoding

    ''' <summary>频率编码随机源（推理时同样有编码噪声，加长 T 可降低方差）</summary>
    Public Property Rng As New Random(42)

    Private _inputSize As Integer

    Public Sub New(inputSize As Integer, timeSteps As Integer,
                   Optional encoding As SpikeEncoding = SpikeEncoding.RateCoding)
        _inputSize = inputSize
        Me.TimeSteps = timeSteps
        Me.Encoding = encoding
    End Sub

    ''' <summary>追加一个 LIF 层（输入维度自动衔接上一层输出）</summary>
    Public Function AddLayer(units As Integer,
                             Optional beta As Double = 0.9,
                             Optional threshold As Double = 1.0,
                             Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike,
                             Optional surrogate As SurrogateKind = SurrogateKind.FastSigmoid,
                             Optional alpha As Double = 2.0,
                             Optional seed As Integer? = Nothing) As LIFLayer
        If SparseLayer IsNot Nothing Then
            Throw New InvalidOperationException(
                "本网络已配置稀疏自定义连接层（SparseLayer），不能与全连接 LIF 层混用")
        End If

        Dim inSize = If(Layers.Count = 0, _inputSize, Layers(Layers.Count - 1).Units)
        Dim layer = New LIFLayer($"LIF{Layers.Count}", inSize, units,
                                 beta, threshold, resetMode, surrogate, alpha, seed)
        Layers.Add(layer)
        Return layer
    End Function

#Region "稀疏自定义连接"

    ''' <summary>
    ''' 由预构建的稀疏连接矩阵配置本网络的稀疏递归层（单个大稀疏层）。
    ''' 稀疏层仅支持前向仿真，构造后不能再 <see cref="AddLayer"/> 全连接层。
    ''' </summary>
    ''' <param name="synapses">方阵稀疏突触连接 W[N, N]（行=突触前，列=突触后）</param>
    ''' <param name="inputMap">
    ''' 输入特征 → 神经元索引 的注入映射；为 Nothing 时要求 inputSize = N（1:1）。
    ''' </param>
    Public Function AddSparseLayer(synapses As SparseMatrix,
                                   Optional beta As Double = 0.9,
                                   Optional threshold As Double = 1.0,
                                   Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike,
                                   Optional inputMap As Integer() = Nothing) As SparseLIFLayer
        If synapses Is Nothing Then
            Throw New ArgumentNullException(NameOf(synapses))
        End If
        If Layers.Count > 0 Then
            Throw New InvalidOperationException(
                "已存在全连接 LIF 层，不能再配置稀疏自定义连接层（二者互斥）")
        End If
        If synapses.Rows <> synapses.Columns Then
            Throw New ArgumentException(
                $"稀疏连接矩阵必须为方阵（pre/post 为同一神经元群），实际 {synapses.Rows}x{synapses.Columns}")
        End If

        Dim n = synapses.Columns
        If inputMap Is Nothing Then
            If _inputSize <> n Then
                Throw New ArgumentException(
                    $"未提供 inputMap 时要求 inputSize({_inputSize}) = 神经元数({n})，请提供 inputMap 指定注入映射")
            End If
        Else
            If inputMap.Length <> _inputSize Then
                Throw New ArgumentException(
                    $"inputMap 长度({inputMap.Length})应等于 inputSize({_inputSize})")
            End If
            For i = 0 To inputMap.Length - 1
                If inputMap(i) < 0 OrElse inputMap(i) >= n Then
                    Throw New ArgumentOutOfRangeException(
                        $"inputMap({i})={inputMap(i)} 超出神经元索引范围 [0, {n})")
                End If
            Next
        End If

        Dim layer = New SparseLIFLayer("Sparse0", _inputSize, synapses, beta, threshold, resetMode)
        Me.SparseLayer = layer
        Me.SparseInputMap = inputMap
        Return layer
    End Function

    ''' <summary>
    ''' 由 FlyWire 风格三元组 (pre, post, weight) 构建稀疏连接并配置稀疏递归层。
    ''' 相同 (pre, post) 的重复边按权重累加合并。
    ''' </summary>
    ''' <param name="units">神经元总数 N（pre/post 索引范围 [0, N)）</param>
    ''' <param name="normalization">权重归一化方式，默认按扇入（列）归一化</param>
    ''' <param name="inputMap">输入特征 → 神经元索引 的注入映射（可选）</param>
    Public Function AddSparseLayer(pre As Integer(), post As Integer(), weight As Double(),
                                   units As Integer,
                                   Optional normalization As SparseNormalization = SparseNormalization.FanIn,
                                   Optional beta As Double = 0.9,
                                   Optional threshold As Double = 1.0,
                                   Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike,
                                   Optional inputMap As Integer() = Nothing) As SparseLIFLayer
        Dim synapses = SparseMatrix.FromTriplets(pre, post, weight, units, units)
        synapses.Normalize(normalization)
        Return AddSparseLayer(synapses, beta, threshold, resetMode, inputMap)
    End Function

#End Region

    ''' <summary>把连续输入编码为 T 个时间步的脉冲序列</summary>
    Private Function Encode(x As Tensor) As List(Of Tensor)
        Select Case Encoding
            Case SpikeEncoding.RateCoding
                Return SpikeEncoders.RateEncode(x, TimeSteps, Rng)
            Case SpikeEncoding.DirectCurrent
                ' 直接电流注入：每步注入同一连续电流（不做脉冲采样）。
                ' 各步内容完全相同，因此共享一份缓冲：既省掉 T 次分配，
                ' 也让 GPU 后端只上传一次外部电流（否则每步都是一次 1.1 MB 的 H2D 拷贝，
                ' 在 WDDM 上约 0.4 ms/步，足以盖过稀疏内核的收益）。
                Return SpikeEncoders.DirectCurrentEncode(x, TimeSteps, shareBuffer:=True)
            Case Else
                Return SpikeEncoders.LatencyEncode(x, TimeSteps)
        End Select
    End Function

    ''' <summary>
    ''' 编码序列的每一步是否都是同一个张量（恒流编码的特征）。
    ''' </summary>
    ''' <remarks>
    ''' 用于判断"可以只散射一次然后复用"：恒流下各步的外部电流逐元素相同，
    ''' 复用同一个张量既省分配，也让设备端缓存能按引用命中（只上传一次）。
    ''' </remarks>
    Private Function IsConstantSequence(sequence As List(Of Tensor)) As Boolean
        If sequence Is Nothing OrElse sequence.Count = 0 Then Return False

        For t As Integer = 1 To sequence.Count - 1
            If Not sequence(t) Is sequence(0) Then Return False
        Next

        Return True
    End Function

    ''' <summary>
    ''' 前向仿真 T 个时间步，返回输出层脉冲计数（计数解码的 logits）[batch, n_out]。
    ''' 开始前自动重置所有层的膜电位。
    ''' </summary>
    Public Function ForwardSpikes(x As Tensor) As Tensor
        If x.Rank <> 2 OrElse x.Shape(1) <> _inputSize Then
            Throw New ArgumentException($"输入形状应为 [batch, {_inputSize}]，实际 [{String.Join(",", x.Shape)}]")
        End If

        ' 稀疏自定义连接路径：单个大稀疏层，含层内递归（循环连接与自反馈）
        If SparseLayer IsNot Nothing Then
            Return ForwardSparse(x)
        End If

        Dim batch = x.Shape(0)
        Dim seq = Encode(x)

        For Each l In Layers : l.ResetState(batch) : Next

        Dim outUnits = Layers(Layers.Count - 1).Units
        Dim counts = New Tensor(batch, outUnits)

        For t = 0 To TimeSteps - 1
            Dim sig = seq(t)
            For Each l In Layers
                sig = l.ForwardStep(sig)
            Next
            counts = counts + sig
        Next

        Return counts
    End Function

    ''' <summary>
    ''' 稀疏自定义连接前向仿真：编码 → 按 inputMap 散射注入外部电流 → 逐时间步
    ''' 稀疏递归 LIF 传播（I = I_ext + W·S[t−1]）→ 累加各神经元脉冲计数。
    ''' 返回 [batch, N] 的脉冲计数（N = 神经元总数）。
    ''' </summary>
    Private Function ForwardSparse(x As Tensor) As Tensor
        Dim batch = x.Shape(0)
        Dim units = SparseLayer.Units
        Dim seq = Encode(x)

        SparseLayer.ResetState(batch)

        ' 计数由层内的累加器维护：融合路径下它在<b>设备端</b>累加，
        ' 逐算子路径下由层内主机循环累加。于是这里不再需要每步回读 [batch, N] 脉冲张量，
        ' 也不再需要 O(T·N) 的主机求和循环 —— 后者在十万神经元 / T=30 下是 400 万次加法，
        ' 并且会强制 GPU 后端每步做一次显存回读。
        '
        ' 恒流编码下各步的外部电流相同：散射一次后复用，避免每步重新分配与重新上传。
        Dim sharedExt = If(IsConstantSequence(seq), ScatterInput(seq(0), batch, units), Nothing)

        For t = 0 To TimeSteps - 1
            Dim ext

            If sharedExt IsNot Nothing Then
                ext = sharedExt
            Else
                ext = ScatterInput(seq(t), batch, units)
            End If

            Call SparseLayer.ForwardStep(ext)
        Next

        ' 设备为主副本：整段仿真只需在结束时同步一次
        Call SparseLayer.SyncFromDevice()

        ' 返回独立副本：避免调用方就地修改层内累加器（下一轮 ResetState 会重新分配）
        Return CType(SparseLayer.Counts.Clone(), Tensor)
    End Function

    ''' <summary>
    ''' 把编码后的脉冲特征 [batch, inputSize] 散射为神经元注入电流 [batch, Units]。
    ''' 无 inputMap 时要求 inputSize = Units，直接 1:1 返回。
    ''' </summary>
    Private Function ScatterInput(spikes As Tensor, batch As Integer, units As Integer) As Tensor
        Dim map = Me.SparseInputMap

        If map Is Nothing Then
            Return spikes      ' 1:1 映射：spikes 形状已为 [batch, Units]
        End If

        Dim ext = New Tensor(batch, units)
        Dim ed = ext.Data
        Dim sd = spikes.Data
        Dim f = map.Length

        For b = 0 To batch - 1
            Dim so = b * f
            Dim eo = b * units
            For i = 0 To f - 1
                Dim v = sd(so + i)
                If v <> 0.0 Then
                    ed(eo + map(i)) += v
                End If
            Next
        Next

        ' 绕过索引器就地写入：声明主机数据已修改，使设备端缓存失效
        ext.MarkHostModified()

        Return ext
    End Function

    ''' <summary>前向推理：返回各样本的预测类别（计数解码 argmax）</summary>
    Public Function Predict(x As Tensor) As Integer()
        Dim counts = ForwardSpikes(x)
        Dim batch = counts.Shape(0)
        Dim n = counts.Shape(1)
        Dim pred(batch - 1) As Integer
        For b = 0 To batch - 1
            Dim best = 0
            For j = 1 To n - 1
                If counts(b, j) > counts(b, best) Then best = j
            Next
            pred(b) = best
        Next
        Return pred
    End Function

    ''' <summary>
    ''' 前向 + 反向传播，计算并缓存所有层的权重梯度（不更新参数）。
    ''' 返回本批次平均损失。梯度自检依赖"不更新"这一性质。
    ''' </summary>
    Public Function ComputeGradients(x As Tensor, labels As Integer()) As Double
        If SparseLayer IsNot Nothing Then
            Throw New NotSupportedException(
                "稀疏连接层当前仅支持前向仿真，不支持 BPTT 训练（ComputeGradients/TrainStep）")
        End If

        Dim counts = ForwardSpikes(x)
        Dim lg = Losses.SoftmaxCrossEntropy(counts, labels)

        ' counts = Σ_t S_out[t] → 每个时间步的 dS_out 都等于 dL/dcounts
        Dim dS As List(Of Tensor) = Nothing
        For i = Layers.Count - 1 To 0 Step -1
            If dS Is Nothing Then
                Dim rep As New List(Of Tensor)()
                For t = 1 To TimeSteps : rep.Add(lg.Grad) : Next
                dS = Layers(i).BackwardTime(rep)
            Else
                dS = Layers(i).BackwardTime(dS)
            End If
        Next

        Return lg.Loss
    End Function

    ''' <summary>
    ''' 单次训练步：前向 → softmax 交叉熵 → BPTT → Adam 更新。
    ''' 返回本批次平均损失。
    ''' </summary>
    Public Function TrainStep(x As Tensor, labels As Integer(), opt As AdamOptimizer) As Double
        If SparseLayer IsNot Nothing Then
            Throw New NotSupportedException(
                "稀疏连接层当前仅支持前向仿真，不支持 BPTT 训练（ComputeGradients/TrainStep）")
        End If

        Dim loss = ComputeGradients(x, labels)

        For Each l In Layers
            opt.Update(l.Weight, l.WeightGrad, l.Name)
        Next

        Return loss
    End Function

    ''' <summary>分类准确率</summary>
    Public Shared Function Accuracy(pred As Integer(), truth As Integer()) As Double
        Dim hit = 0
        For i = 0 To pred.Length - 1
            If pred(i) = truth(i) Then hit += 1
        Next
        Return hit / CDbl(pred.Length)
    End Function

End Class
