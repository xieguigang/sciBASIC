Imports System
Imports System.Numerics
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math
Imports sysvec = System.Numerics.Vector(Of Double)
Imports vmath = System.Numerics.Vector

Namespace Logistic

    ''' <summary>
    ''' 逻辑回归训练的可复用数值内核。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 所有缓冲区都由 <see cref="Logistic.train"/> 在循环外预分配并复用；本类不会
    ''' 在每轮迭代里分配与样本规模同量级的数组，也不做嵌套并行。
    ''' </para>
    ''' <para>
    ''' <b>布局选择</b>：前向 logits 使用一次性扁平化的 <c>Double(,)</c> 行主序
    ''' 连续内存，配合 <see cref="SIMDIntrinsics.DotFmaRow(Double(,), Integer, Double())"/>
    ''' —— 该内核把 <c>LoadUnsafe</c> 直接写在循环体内，正是为本项目
    ''' <c>Optimize=false</c> / JIT MinOpts 下私有包装不内联的场景优化的。
    ''' 梯度累加则直接使用每个样本原本就连续的 <c>Double()</c> 行，以便命中
    ''' <see cref="SimdEngine.AddScaledInPlace(Double, Double(), Double())"/> 的 FMA AXPY
    ''' 内核（该内核只接受一维数组，扁平矩阵无法直接传入）。
    ''' </para>
    ''' <para>
    ''' sigmoid 使用 <c>exp(-|z|)</c> 区间约减，杜绝 <c>exp(-z)</c> 上溢；
    ''' 指数走 <c>SimdMath.ExpInPlace</c> 的真 AVX2+FMA 多项式路径
    ''' （无 AVX2/FMA 或全局禁用时由内核自动回退标量 <c>Math.Exp</c>，
    ''' 因此标量基线与 SIMD 基线可精确对拍）。交叉熵成本则由已算出的
    ''' <c>h</c> 反推 softplus，只需一趟标量 <c>Math.Log</c>，
    ''' 既不会出现 <c>log(0)</c>，也避免为监控量再付一次全数组预扫描。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class LogisticKernels

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 把锯齿数组一次性扁平化为行主序的连续矩形数组。
        ''' </summary>
        Friend Shared Function Flatten(features As Double()()) As Double(,)
            If features Is Nothing OrElse features.Length = 0 Then
                Throw New ArgumentException("there is no feature row to flatten!", NameOf(features))
            End If

            Dim m As Integer = features.Length
            Dim d As Integer = features(0).Length

            If d = 0 Then
                Throw New ArgumentException("the feature rows are empty!", NameOf(features))
            End If

            Dim flat As Double(,) = New Double(m - 1, d - 1) {}

            For i As Integer = 0 To m - 1
                Dim row As Double() = features(i)

                If row Is Nothing OrElse row.Length <> d Then
                    Throw New ArgumentException($"feature size not agree at row {i}: expected {d}!", NameOf(features))
                End If

                For j As Integer = 0 To d - 1
                    flat(i, j) = row(j)
                Next
            Next

            Return flat
        End Function

        ''' <summary>
        ''' 批量计算 <c>z(i) = x(i)·weights + bias</c>。
        ''' FMA / 标量回退由 <see cref="SIMDIntrinsics.DotFmaRow(Double(,), Integer, Double())"/>
        ''' 按硬件能力与 <see cref="SIMDEnvironment.IsEnabled"/> 自行决定。
        ''' </summary>
        Friend Shared Sub BatchLogits(x As Double(,), weights As Double(), bias As Double, z As Double())
            For i As Integer = 0 To z.Length - 1
                z(i) = SIMDIntrinsics.DotFmaRow(x, i, weights) + bias
            Next
        End Sub

        ''' <summary>
        ''' 批量数值稳定 sigmoid，结果写回调用方缓冲 <paramref name="h"/>。
        ''' <c>p = z >= 0 ? 1/(1+e) : e/(1+e)</c>，其中 <c>e = exp(-|z|)</c>。
        ''' </summary>
        Friend Shared Sub SigmoidInPlace(z As Double(), h As Double())
            Dim m As Integer = z.Length

            If m = 0 Then
                Return
            End If

            For i As Integer = 0 To m - 1
                h(i) = -std.Abs(z(i))
            Next

            Call SimdMath.ExpInPlace(h, m)

            Dim width As Integer = sysvec.Count
            Dim one As New sysvec(1.0R)
            Dim zero As New sysvec(0.0R)
            Dim i2 As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso m >= width Then
                Do While i2 <= m - width
                    Dim zv As New sysvec(z, i2)
                    Dim ev As New sysvec(h, i2)
                    Dim den As sysvec = one + ev
                    Dim inv As sysvec = one / den
                    Dim mask As sysvec = vmath.GreaterThanOrEqual(zv, zero)
                    Dim p As sysvec = vmath.ConditionalSelect(mask, inv, ev * inv)

                    p.CopyTo(h, i2)
                    i2 += width
                Loop
            End If

            For j As Integer = i2 To m - 1
                Dim e As Double = h(j)
                h(j) = If(z(j) >= 0.0R, 1.0R / (1.0R + e), e / (1.0R + e))
            Next
        End Sub

        ''' <summary>
        ''' 平均交叉熵，数值稳定形式 <c>mean(max(z,0) - y*z - log(hz))</c>，
        ''' 其中 <c>hz = z &gt;= 0 ? h : 1-h</c>。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 由 <c>softplus(z) = max(z,0) + log(1 + exp(-|z|))</c> 与
        ''' <c>log(1 + exp(-|z|)) = -log(hz)</c> 推出，可以直接复用
        ''' <c>SigmoidInPlace</c> 已经算好的 <paramref name="h"/>，
        ''' 不必再为成本单独做一趟 exp，省掉一次全数组超越函数与预扫描。
        ''' </para>
        ''' <para>
        ''' 稳定 sigmoid 保证 <c>hz ∈ [0.5, 1)</c>，因此 <c>log</c> 的输入恒为正，
        ''' 既满足 <c>SimdMath.LogInPlace</c> 的契约，也不会出现 <c>log(0)</c>；
        ''' 饱和区（<c>h → 0</c> 或 <c>h → 1</c>）反而得到 <c>log(1) = 0</c> 的精确结果。
        ''' </para>
        ''' <para>
        ''' 成本只是收敛监控量，这里用单趟标量 <c>Math.Log</c> 累加完成，
        ''' 刻意不调用 <c>SimdMath.LogInPlace</c>：后者在 SIMD 分支会先做一次
        ''' 全数组 <c>positive</c> 预扫描再跑多项式，实测在这种每轮都要算的
        ''' 监控量上反而比标量 libm 更贵。训练主路径的 exp / 点积 / AXPY
        ''' 仍然走核心库的向量化内核。
        ''' </para>
        ''' </remarks>
        Friend Shared Function CrossEntropy(z As Double(), h As Double(),
                                            labels As Double()) As Double
            Dim m As Integer = z.Length

            If m = 0 Then
                Return 0.0R
            End If
            If h.Length <> m OrElse labels.Length <> m Then
                Throw New ArgumentException("hypothesis / labels size does not agree with the logits!", NameOf(z))
            End If

            Dim total As Double = 0.0R

            For i As Integer = 0 To m - 1
                Dim hz As Double = If(z(i) >= 0.0R, h(i), 1.0R - h(i))
                total += std.Max(z(i), 0.0R) - labels(i) * z(i) - std.Log(hz)
            Next

            Return total / m
        End Function

        ''' <summary><c>delta(i) = h(i) - y(i)</c></summary>
        Friend Shared Sub FillDelta(h As Double(), labels As Double(), delta As Double())
            Dim m As Integer = h.Length

            If labels.Length <> m OrElse delta.Length <> m Then
                Throw New ArgumentException("hypothesis / labels / delta size does not agree!", NameOf(h))
            End If

            For i As Integer = 0 To m - 1
                delta(i) = h(i) - labels(i)
            Next
        End Sub

        ''' <summary>
        ''' 累加梯度 <c>gradient(j) += Σ delta(i) * x(i,j)</c>，返回截距梯度
        ''' <c>Σ delta(i)</c>。两者都<b>不</b>除以样本数，缩放由调用方统一处理。
        ''' </summary>
        ''' <param name="partials">
        ''' 并行路径的分块局部梯度缓冲，由调用方持有并跨迭代复用（<c>ByRef</c>，
        ''' 首次调用或维度变化时按需分配）。
        ''' </param>
        ''' <param name="parallelApplied">
        ''' 输出：本次调用是否真的走了分块并行路径（供可观测性与测试确认分支被覆盖）。
        ''' </param>
        Friend Shared Function AccumulateGradient(rows As Double()(),
                                                  delta As Double(),
                                                  gradient As Double(),
                                                  useParallel As Boolean,
                                                  minParallelWork As Integer,
                                                  ByRef partials As Double()(),
                                                  ByRef parallelApplied As Boolean) As Double
            Dim m As Integer = rows.Length
            Dim d As Integer = gradient.Length
            Dim gradBias As Double = 0.0R

            parallelApplied = False

            Call Array.Clear(gradient, 0, d)

            ' 截距梯度始终串行累加，保证归约顺序固定、结果可复现
            For i As Integer = 0 To m - 1
                gradBias += delta(i)
            Next

            Dim work As Long = CLng(m) * CLng(d)
            Dim parallel As Boolean = useParallel AndAlso
                                      SimdParallel.Enable AndAlso
                                      m >= 2 AndAlso
                                      work >= CLng(minParallelWork)

            If Not parallel Then
                For i As Integer = 0 To m - 1
                    Call SimdEngine.AddScaledInPlace(delta(i), rows(i), gradient)
                Next

                Return gradBias
            End If

            parallelApplied = True

            ' 按工作量自适应分块：不直接用 SimdParallel 的 65536 元素阈值
            ' （逻辑回归每行的计算量是 d 次乘加，样本数往往远低于该阈值）。
            Dim workers As Integer = std.Max(1, Environment.ProcessorCount)
            Dim chunks As Integer = std.Min(m, workers * 4)
            Dim chunk As Integer = (m + chunks - 1) \ chunks
            Dim nChunks As Integer = (m + chunk - 1) \ chunk

            If partials Is Nothing OrElse partials.Length <> nChunks OrElse
               partials(0) Is Nothing OrElse partials(0).Length <> d Then
                partials = New Double(nChunks - 1)() {}

                For c As Integer = 0 To nChunks - 1
                    partials(c) = New Double(d - 1) {}
                Next
            Else
                For c As Integer = 0 To nChunks - 1
                    Call Array.Clear(partials(c), 0, d)
                Next
            End If

            ' ByRef 参数不能直接进入 lambda，先把数组引用拷到局部变量
            Dim partialsLocal As Double()() = partials

            Call System.Threading.Tasks.Parallel.For(0, nChunks,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim ends As Integer = std.Min(start + chunk, m)
                    Dim local As Double() = partialsLocal(c)

                    ' 块内保持单线程向量化，不嵌套并行
                    For i As Integer = start To ends - 1
                        Call SimdEngine.AddScaledInPlace(delta(i), rows(i), local)
                    Next
                End Sub)

            partials = partialsLocal

            ' 严格按块号顺序合并：固定浮点归约次序，保证结果可复现
            For c As Integer = 0 To nChunks - 1
                Call SimdEngine.AddInPlace(Of Double)(gradient, partials(c))
            Next

            Return gradBias
        End Function

        ''' <summary>
        ''' 公共的成本函数（平均交叉熵），供跨程序集的梯度有限差分检验使用。
        ''' 与训练循环复用同一组内核，因此检验的就是真实的训练路径。
        ''' </summary>
        Public Shared Function Cost(rows As Double()(), labels As Double(),
                                    weights As Double(), bias As Double) As Double
            Dim flat As Double(,) = Flatten(rows)
            Dim z As Double() = New Double(rows.Length - 1) {}
            Dim h As Double() = New Double(rows.Length - 1) {}

            Call BatchLogits(flat, weights, bias, z)
            Call SigmoidInPlace(z, h)

            Return CrossEntropy(z, h, labels)
        End Function

        ''' <summary>
        ''' 公共的解析梯度（已按样本数归一化），供跨程序集的梯度有限差分检验使用。
        ''' </summary>
        Public Shared Sub CostGradient(rows As Double()(), labels As Double(),
                                       weights As Double(), bias As Double,
                                       ByRef grad As Double(), ByRef gradBias As Double)
            Dim m As Integer = rows.Length
            Dim flat As Double(,) = Flatten(rows)
            Dim z As Double() = New Double(m - 1) {}
            Dim h As Double() = New Double(m - 1) {}
            Dim delta As Double() = New Double(m - 1) {}
            Dim partials As Double()() = Nothing
            Dim applied As Boolean = False

            grad = New Double(weights.Length - 1) {}

            Call BatchLogits(flat, weights, bias, z)
            Call SigmoidInPlace(z, h)
            Call FillDelta(h, labels, delta)
            gradBias = AccumulateGradient(rows, delta, grad, False, 0, partials, applied) / m

            For j As Integer = 0 To grad.Length - 1
                grad(j) /= m
            Next
        End Sub
    End Class
End Namespace
