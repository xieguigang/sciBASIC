' ---------------------------------------------------------------------------
' LLMTensorOps —— LLM 模块共用的张量算子与手写反向传播辅助
'
' 与既有的 Transformer\TensorOps.vb 保持完全一致的工程约定：
'   * 前向阶段把中间量写进各组件的 Cache；
'   * 反向阶段显式接收当步 Cache 快照，手写求导公式；
'   * 梯度一律用「与参数同形的张量 + 原地 += 」累加，再交给优化器更新。
'
' 与 TensorOps 的分工：
'   * TensorOps 服务于「编码器-解码器」翻译模型（AddNorm / 最后一维拼接 / 上三角掩码）；
'   * 本模块服务于 decoder-only 语言模型（RMSNorm / RoPE / SwiGLU / 掩码交叉熵 /
'     行查表与行散射 / 全局梯度裁剪）。
' 两者互不依赖，避免改动既有算子而影响翻译 demo 的行为。
' ---------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace LLM

    ''' <summary>
    ''' LLM 模块共用的张量工具与手写反向算子。
    ''' </summary>
    Public Module LLMTensorOps

#Region "默认超参"

        ''' <summary>
        ''' RMSNorm 的默认数值稳定项。
        ''' </summary>
        ''' <remarks>
        ''' 与 <c>Transformer.TensorOps.AddNormEps = 0.001</c> 不同：那个 eps 加在 LayerNorm
        ''' 的<b>方差</b>上，而 RMSNorm 的 eps 加在<b>均方值</b>上，两者量纲不同。
        ''' 现代 LLM（Llama / Qwen / DeepSeek）普遍取 1e-6 ~ 1e-5，这里取 1e-6。
        ''' </remarks>
        Public Const DefaultRmsNormEps As Double = 0.000001

        ''' <summary>
        ''' RoPE 的默认旋转基数 θ。
        ''' </summary>
        ''' <remarks>
        ''' 原始 RoPE 论文取 10000；长上下文扩展（NTK-aware / YaRN）的常见做法就是
        ''' 调大这个基数，因此这里把它做成可配置项。
        ''' </remarks>
        Public Const DefaultRopeTheta As Double = 10000.0

#End Region

#Region "基础工具"

        ''' <summary>创建与 <paramref name="t"/> 同形的全零张量（用作梯度累加器）。</summary>
        Public Function ZerosLike(t As Tensor) As Tensor
            Return New Tensor(t.Shape)
        End Function

        ''' <summary>把张量的全部元素置零（原地）。</summary>
        Public Sub ZeroInPlace(t As Tensor)
            Array.Clear(t.Data, 0, t.Length)
            Call t.MarkHostModified()
        End Sub

        ''' <summary>原地累加：<c>target += grad</c>（要求形状一致）。</summary>
        Public Sub Accumulate(target As Tensor, grad As Tensor)
            If grad Is Nothing Then Return
            If Not target.Shape.SequenceEqual(grad.Shape) Then
                Throw New ArgumentException(
                    $"梯度累加要求形状一致: [{String.Join(",", target.Shape)}] vs [{String.Join(",", grad.Shape)}]")
            End If

            Dim dst = target.Data
            Dim src = grad.Data

            For i As Integer = 0 To dst.Length - 1
                dst(i) += src(i)
            Next

            Call target.MarkHostModified()
        End Sub

        ''' <summary>深拷贝张量（同一个梯度要分发给两个分支时必须各自持有独立副本）。</summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CloneTensor(t As Tensor) As Tensor
            Return CType(t.Clone(), Tensor)
        End Function

        ''' <summary>原地按标量缩放。</summary>
        Public Sub ScaleInPlace(t As Tensor, scalar As Double)
            Dim data = t.Data

            For i As Integer = 0 To data.Length - 1
                data(i) *= scalar
            Next

            Call t.MarkHostModified()
        End Sub

        ''' <summary>
        ''' 以 He 正态分布初始化参数（<c>[fanIn, fanOut]</c>，以倒数第二维为 fan-in）。
        ''' </summary>
        ''' <remarks>
        ''' 直接复用 <c>Transformer.TensorOps.HeNormalInit</c>，从而与既有 Transformer 共享
        ''' 同一个随机序列；配合 <c>Transformer.TensorOps.Seed</c> 即可让整次训练可复现。
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HeNormalInit(shape As Integer()) As Tensor
            Return Transformer.TensorOps.HeNormalInit(shape)
        End Function

#End Region

#Region "全局梯度裁剪"

        ''' <summary>
        ''' 对一组梯度做全局 L2 范数裁剪（in-place）。
        ''' </summary>
        ''' <param name="gradients">本次反传涉及的全部梯度累加器</param>
        ''' <param name="maxNorm">允许的最大全局范数；&lt;= 0 表示不裁剪</param>
        ''' <returns>裁剪前的全局 L2 范数</returns>
        ''' <remarks>
        ''' 小模型 + 高学习率时单步梯度爆炸是训练发散的头号原因；按"全局范数"而不是
        ''' "逐张量范数"裁剪，可以保持各参数之间的相对梯度尺度不变。
        ''' </remarks>
        Public Function ClipGlobalNorm(gradients As IEnumerable(Of Tensor), maxNorm As Double) As Double
            Dim sumSq As Double = 0.0

            For Each g In gradients
                If g Is Nothing Then Continue For
                Dim data = g.Data

                For i As Integer = 0 To data.Length - 1
                    sumSq += data(i) * data(i)
                Next
            Next

            Dim norm = std.Sqrt(sumSq)

            If maxNorm > 0 AndAlso norm > maxNorm AndAlso norm > 0 Then
                Dim scale = maxNorm / norm

                For Each g In gradients
                    If g Is Nothing Then Continue For
                    Call ScaleInPlace(g, scale)
                Next
            End If

            Return norm
        End Function

#End Region

#Region "掩码交叉熵"

        ''' <summary>
        ''' 带掩码的交叉熵损失，并给出对 logits 的梯度。
        ''' </summary>
        ''' <param name="logits">形状 <c>[N, V]</c> 的未归一化打分（N = batch * seq 的有效位置）</param>
        ''' <param name="targets">长度 N 的目标 token 下标；对应 <paramref name="mask"/>=False 的位置会被忽略</param>
        ''' <param name="mask">长度 N 的损失掩码。SFT 阶段用它把 user / tool 结果的 token 排除在损失之外</param>
        ''' <param name="dLogits">
        ''' 输出参数：与 <paramref name="logits"/> 同形。<c>softmax</c> 与交叉熵的导数合并后
        ''' 恰好是 <c>softmax − onehot</c>，这里直接给出该结果（未命中掩码的行全零）。
        ''' </param>
        ''' <returns>平均到每个有效位置上的负对数似然；没有任何有效位置时返回 0</returns>
        ''' <remarks>
        ''' 实现委托给<b>计算后端</b>（<c>Tensor.computeKernel</c>）：
        '''   * CPU 后端走 <c>TensorComputeBase</c> 里逐行三趟循环的参考实现；
        '''   * CUDA 后端走 <c>Kernels\train.cu</c> 的融合内核（每行一个 block +
        '''     共享内存树形归约），把 12.8 万词表下的 3300 万次 <c>exp</c> 搬到设备上。
        ''' 两条路径的损失与梯度定义严格一致，因此 CPU/GPU 结果可比。
        ''' </remarks>
        Public Function MaskedCrossEntropy(logits As Tensor,
                                           targets As Integer(),
                                           mask As Boolean(),
                                           ByRef dLogits As Tensor) As Double

            Return Tensor.computeKernel.MaskedCrossEntropy(logits, targets, mask, dLogits)
        End Function

        ''' <summary>
        ''' 掩码交叉熵的<b>主机参考实现</b>（保留用于对照与调试）。
        ''' </summary>
        ''' <remarks>
        ''' 与后端实现等价，但完全在主机上执行。CUDA 后端可用时不必调用它；
        ''' 需要逐位比对 GPU 结果时可以用它作为基准。
        ''' </remarks>
        Public Function MaskedCrossEntropyHost(logits As Tensor,
                                               targets As Integer(),
                                               mask As Boolean(),
                                               ByRef dLogits As Tensor) As Double

            If logits.Rank <> 2 Then Throw New ArgumentException("掩码交叉熵要求 [rows, vocab] 的二维 logits")

            Dim rows = logits.Shape(0)
            Dim vocab = logits.Shape(1)

            dLogits = New Tensor(logits.Shape)

            Dim src = logits.Data
            Dim grad = dLogits.Data
            Dim total As Double = 0.0
            Dim count As Integer = 0

            For r As Integer = 0 To rows - 1
                If mask IsNot Nothing AndAlso r < mask.Length AndAlso Not mask(r) Then Continue For
                If targets Is Nothing OrElse r >= targets.Length Then Continue For

                Dim t = targets(r)
                If t < 0 OrElse t >= vocab Then Continue For

                count += 1

                Dim offset = r * vocab
                Dim maxVal = Double.NegativeInfinity

                For j As Integer = 0 To vocab - 1
                    If src(offset + j) > maxVal Then maxVal = src(offset + j)
                Next

                Dim sumExp As Double = 0.0

                For j As Integer = 0 To vocab - 1
                    Dim e = std.Exp(src(offset + j) - maxVal)
                    grad(offset + j) = e
                    sumExp += e
                Next

                If sumExp <= 0 Then sumExp = 1.0

                For j As Integer = 0 To vocab - 1
                    grad(offset + j) /= sumExp
                Next

                total -= std.Log(std.Max(grad(offset + t), 1.0E-12))
                grad(offset + t) -= 1.0
            Next

            If count = 0 Then Return 0.0

            Dim inv = 1.0 / count

            For i As Integer = 0 To grad.Length - 1
                grad(i) *= inv
            Next

            Call dLogits.MarkHostModified()

            Return total * inv
        End Function

#End Region

#Region "行查表与行散射"

        ''' <summary>
        ''' 按行查表：<c>y[n, :] = table[indices(n), :]</c>。
        ''' </summary>
        ''' <param name="table">形状 <c>[Rows, D]</c> 的参数表（词嵌入矩阵）</param>
        ''' <param name="indices">长度 N 的行下标序列</param>
        ''' <remarks>
        ''' 这就是"词嵌入"的全部内容：one-hot 乘以嵌入矩阵的结果等价于直接拷贝对应行，
        ''' 因此完全不需要构造 one-hot 向量，也不需要做矩阵乘法。
        ''' </remarks>
        Public Function GatherRows(table As Tensor, indices As Integer()) As Tensor
            Dim width = table.Shape(table.Rank - 1)
            Dim count = indices.Length
            Dim result = New Tensor(count, width)
            Dim src = table.Data
            Dim dst = result.Data

            For r As Integer = 0 To count - 1
                Dim row = indices(r)
                If row < 0 Then row = 0

                Call Array.Copy(src, row * width, dst, r * width, width)
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' <see cref="GatherRows"/> 的反向：把每行的梯度散射累加回表中。
        ''' </summary>
        ''' <param name="tableGradient">形状 <c>[Rows, D]</c> 的梯度累加器（原地 += ）</param>
        ''' <param name="indices">长度 N 的行下标序列</param>
        ''' <param name="dOut">形状 <c>[N, D]</c> 的上游梯度</param>
        ''' <remarks>
        ''' 同一个 token 在一次前向中出现多次时，它的梯度会被自然累加多次 —— 这正是
        ''' 共享嵌入导致"一个词从上下文中获得多份学习信号"的数学体现。
        ''' </remarks>
        Public Sub ScatterAddRows(tableGradient As Tensor, indices As Integer(), dOut As Tensor)
            Dim width = tableGradient.Shape(tableGradient.Rank - 1)
            Dim count = indices.Length
            Dim dst = tableGradient.Data
            Dim src = dOut.Data

            For r As Integer = 0 To count - 1
                Dim row = indices(r)
                If row < 0 Then Continue For

                Dim srcBase = r * width
                Dim dstBase = row * width

                For i As Integer = 0 To width - 1
                    dst(dstBase + i) += src(srcBase + i)
                Next
            Next

            Call tableGradient.MarkHostModified()
        End Sub

#End Region

    End Module

End Namespace
