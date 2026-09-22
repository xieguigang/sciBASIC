' ---------------------------------------------------------------------------
' RotaryEmbedding —— 旋转位置编码（RoPE）
'
' 核心思想：不改动词嵌入，而是把 Q / K 向量按"位置"成对旋转。第 (2i, 2i+1) 维在
' 位置 m 处旋转的角度是 m·ω_i，其中 ω_i = θ^(-2i/d_head)。
'
'     [q'_2i  ]   [cos(mω_i)  -sin(mω_i)] [q_2i  ]
'     [q'_2i+1] = [sin(mω_i)   cos(mω_i)] [q_2i+1]
'
' 这样设计的精妙之处在于点积只依赖相对距离：
'
'     (R_m q)·(R_n k) = qᵀ R_mᵀR_n k = qᵀ R_{n-m} k
'
' 即"绝对位置编码"被自动转换成了"相对位置编码"，无需显式构造相对位置矩阵。
' Llama / Qwen / Mistral / DeepSeek 均采用此方案。
'
' 与 KV Cache 的配合：增量解码时新 token 的绝对位置已知，只需按该位置旋转它的
' Q / K，历史位置早已旋转好并缓存在 KV Cache 中，完全不需要重算。
'
' 长上下文扩展：把 θ 从 10000 调大（NTK-aware / YaRN / LongRoPE 的本质之一）
' 即可让相同的维度索引对应更低的频率，从而把有效上下文拉长。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' RoPE 旋转位置编码：预计算 cos/sin 表，并按绝对位置对最后两维做旋转。
''' </summary>
''' <remarks>
''' 本类是无状态的纯函数集合（除只读的预计算表），因此同一个实例可以被模型内
''' 全部注意力层共享。
''' </remarks>
Public Class RotaryEmbedding

    Private ReadOnly _headDim As Integer
    Private ReadOnly _halfDim As Integer
    Private ReadOnly _theta As Double
    Private ReadOnly _maxSeqLen As Integer

    ''' <summary>cos 表，下标为 <c>pos * halfDim + i</c>，形状等价于 <c>[maxSeqLen, halfDim]</c></summary>
    Private ReadOnly _cos As Double()

    ''' <summary>sin 表，布局与 <see cref="_cos"/> 相同</summary>
    Private ReadOnly _sin As Double()

    ''' <summary>每个注意力头的维度。</summary>
    Public ReadOnly Property HeadDim As Integer
        Get
            Return _headDim
        End Get
    End Property

    ''' <summary>参与旋转的维度对数（= HeadDim / 2）。</summary>
    Public ReadOnly Property HalfDim As Integer
        Get
            Return _halfDim
        End Get
    End Property

    ''' <summary>预计算表的覆盖长度。</summary>
    Public ReadOnly Property MaxSeqLen As Integer
        Get
            Return _maxSeqLen
        End Get
    End Property

    ''' <summary>旋转基数 θ。</summary>
    Public ReadOnly Property Theta As Double
        Get
            Return _theta
        End Get
    End Property

    ''' <param name="headDim">单个注意力头的维度，必须是偶数（成对旋转）</param>
    ''' <param name="maxSeqLen">预计算到的最长位置</param>
    ''' <param name="theta">旋转基数 θ，默认 10000</param>
    Public Sub New(headDim As Integer, maxSeqLen As Integer,
                   Optional theta As Double = LLMTensorOps.DefaultRopeTheta)

        If headDim <= 0 OrElse headDim Mod 2 <> 0 Then
            Throw New ArgumentException($"RoPE 的 headDim 必须为正偶数，实际 {headDim}")
        End If
        If maxSeqLen <= 0 Then
            Throw New ArgumentException($"RoPE 的 maxSeqLen 必须为正数，实际 {maxSeqLen}")
        End If

        _headDim = headDim
        _halfDim = headDim \ 2
        _theta = theta
        _maxSeqLen = maxSeqLen

        _cos = New Double(maxSeqLen * _halfDim - 1) {}
        _sin = New Double(maxSeqLen * _halfDim - 1) {}

        ' 预计算全部 (位置, 频率) 组合。注意频率与位置无关，
        ' 因此这里相当于把"频率向量 × 位置向量"的外积一次性算完。
        For pos As Integer = 0 To maxSeqLen - 1
            Dim baseIdx = pos * _halfDim

            For i As Integer = 0 To _halfDim - 1
                Dim freq = 1.0 / std.Pow(theta, (2.0 * i) / headDim)
                Dim angle = pos * freq

                _cos(baseIdx + i) = std.Cos(angle)
                _sin(baseIdx + i) = std.Sin(angle)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 就地施加旋转：<c>x ← R(pos) · x</c>。
    ''' </summary>
    ''' <param name="x">形状 <c>[B, S, H, headDim]</c> 的张量（通常是 Q 或 K 投影的输出）</param>
    ''' <param name="positions">长度 S 的绝对位置序列；同一位置对所有 batch / head 生效</param>
    ''' <remarks>
    ''' 就地修改是安全的：反向传播只需要 <c>R(-pos)</c> 作用于上游梯度，
    ''' 而不需要旋转前的原始值（旋转矩阵可逆且不依赖输入）。
    ''' </remarks>
    Public Sub Apply(x As Tensor, positions As Integer())
        Call Rotate(x, positions, inverse:=False)
    End Sub

    ''' <summary>
    ''' 就地施加反向旋转：<c>x ← R(pos)⁻¹ · x = R(-pos) · x</c>。
    ''' </summary>
    ''' <remarks>
    ''' 因为前向是 <c>y = R·x</c>，所以 <c>dx = Rᵀ·dy = R(-pos)·dy</c> ——
    ''' 这正是"反向传播穿过 RoPE"的全部内容。
    ''' </remarks>
    Public Sub ApplyInverse(x As Tensor, positions As Integer())
        Call Rotate(x, positions, inverse:=True)
    End Sub

    Private Sub Rotate(x As Tensor, positions As Integer(), inverse As Boolean)
        If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))

        Dim shape = x.Shape

        If shape.Length <> 4 OrElse shape(3) <> _headDim Then
            Throw New ArgumentException(
                $"RoPE 要求输入形状 [B, S, H, {_headDim}]，实际 [{String.Join(",", shape)}]")
        End If

        Dim nBatch = shape(0), nSeq = shape(1), nHeads = shape(2)

        If positions Is Nothing OrElse positions.Length <> nSeq Then
            Throw New ArgumentException($"RoPE 需要长度为 S={nSeq} 的 positions 序列")
        End If

        Dim data = x.Data
        Dim half = _halfDim
        Dim headDim = _headDim

        For bi As Integer = 0 To nBatch - 1
            For si As Integer = 0 To nSeq - 1
                Dim pos = positions(si)

                If pos < 0 OrElse pos >= _maxSeqLen Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(positions), $"位置 {pos} 超出 RoPE 预计算范围 [0, {_maxSeqLen})")
                End If

                Dim tableBase = pos * half

                For hi As Integer = 0 To nHeads - 1
                    Dim base = ((bi * nSeq + si) * nHeads + hi) * headDim

                    For i As Integer = 0 To half - 1
                        Dim c = _cos(tableBase + i)
                        Dim sn = _sin(tableBase + i)

                        Dim x0 = data(base + 2 * i)
                        Dim x1 = data(base + 2 * i + 1)

                        If inverse Then
                            ' 逆旋转 = 旋转 -θ
                            data(base + 2 * i) = x0 * c + x1 * sn
                            data(base + 2 * i + 1) = -x0 * sn + x1 * c
                        Else
                            data(base + 2 * i) = x0 * c - x1 * sn
                            data(base + 2 * i + 1) = x0 * sn + x1 * c
                        End If
                    Next
                Next
            Next
        Next

        Call x.MarkHostModified()
    End Sub

End Class


