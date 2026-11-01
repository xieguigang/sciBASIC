Imports Microsoft.VisualBasic.Math.SIMD
Imports simdmath = Microsoft.VisualBasic.Math.SIMD.SimdMath
Imports sysmath = System.Math
Imports sysvec = System.Numerics.Vector(Of Double)
Imports sysvecs = System.Numerics.Vector(Of Single)
Imports vmath = System.Numerics.Vector
Imports vmaths = System.Numerics.Vector

Namespace train.kernels

    ''' <summary>
    ''' SIMD 桥接层：训练/推理热路径唯一允许直接调用 SIMD 库的地方。
    '''
    ''' 每个方法内部都先判定「是否启用 + 长度阈值」，不满足时执行与改造前**逐值等价**的
    ''' 标量循环。好处有两处：
    ''' + 精度/回退策略集中在一处，回归里可以整体切换 <see cref="SimdOptions.UseSimd"/>；
    ''' + 热路径不再直接依赖 <c>SimdEngine</c> / <c>SimdReduce</c> 的分配语义
    '''   （它们多数返回新数组，在这里统一替换为就地写回预分配缓冲）。
    '''
    ''' 关于浮点求和顺序：<see cref="SumRange"/> 使用 4 累加器展开（与
    ''' <c>SimdReduce.Sum</c> 同构），因此与逐次累加不再逐位一致，差异在 1e-15 相对量级。
    ''' </summary>
    Friend Module SimdOps

        ''' <summary>
        ''' 该长度是否值得向量化
        ''' </summary>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Friend Function CanVectorize(count As Integer) As Boolean
            Return SimdOptions.IsActive AndAlso count >= SimdOptions.MinVectorLength
        End Function

        ''' <summary>
        ''' 数值稳定的标量 sigmoid（与 <c>LogisticLoss.sigmoid</c> 等价）
        ''' </summary>
        ''' <param name="z"></param>
        ''' <returns></returns>
        Private Function sigmoidScalar(z As Double) As Double
            If z >= 0 Then
                Dim e As Double = sysmath.Exp(-z)
                Return 1.0R / (1.0R + e)
            Else
                Dim e As Double = sysmath.Exp(z)
                Return e / (1.0R + e)
            End If
        End Function

        ''' <summary>
        ''' 一次产出逻辑损失的梯度与海森：<c>grad = p − y</c>、<c>hess = p(1 − p)</c>。
        '''
        ''' 实现要点：先用 <c>exp(−|z|)</c> 做区间约减（避免 <c>exp(−z)</c> 上溢），
        ''' 再用「符号选择」还原 <c>p</c>，最后用向量乘加同时得到 grad/hess。
        ''' </summary>
        ''' <param name="pred">margin（log-odds）</param>
        ''' <param name="label">标签（0/1）</param>
        ''' <param name="grad">输出：梯度</param>
        ''' <param name="hess">输出：海森</param>
        ''' <param name="count">有效长度</param>
        ''' <param name="workspace">复用的暂存缓冲</param>
        Friend Sub SigmoidGradHess(pred As Double(), label As Double(), grad As Double(), hess As Double(),
                                   count As Integer, workspace As SimdWorkspace)

            If count <= 0 Then
                Return
            End If

            If Not CanVectorize(count) Then
                For i As Integer = 0 To count - 1
                    Dim p As Double = sigmoidScalar(pred(i))

                    grad(i) = p - label(i)
                    hess(i) = p * (1.0R - p)
                Next

                Return
            End If

            Dim e As Double()

            If workspace Is Nothing Then
                e = New Double(count - 1) {}
            Else
                Call workspace.Ensure(count)
                e = workspace.Prob
            End If

            ' e(i) = exp(-|z|)
            If SimdOptions.UseFastExp Then

                For i As Integer = 0 To count - 1
                    e(i) = -sysmath.Abs(pred(i))
                Next

                Call simdmath.ExpInPlace(e, count)
            Else

                For i As Integer = 0 To count - 1
                    e(i) = sysmath.Exp(-sysmath.Abs(pred(i)))
                Next
            End If

            Dim width As Integer = sysvec.Count
            Dim one As New sysvec(1.0R)
            Dim zero As New sysvec(0.0R)
            Dim i2 As Integer = 0

            While i2 <= count - width
                Dim z As New sysvec(pred, i2)
                Dim ex As New sysvec(e, i2)
                Dim den As sysvec = one + ex
                Dim inv As sysvec = one / den
                Dim mask As sysvec = vmath.GreaterThanOrEqual(z, zero)
                Dim p As sysvec = vmath.ConditionalSelect(mask, inv, ex * inv)
                Dim y As New sysvec(label, i2)
                Dim g As sysvec = p - y
                Dim h As sysvec = p * (one - p)

                g.CopyTo(grad, i2)
                h.CopyTo(hess, i2)

                i2 += width
            End While

            For j As Integer = i2 To count - 1
                Dim p As Double = If(pred(j) >= 0, 1.0R / (1.0R + e(j)), e(j) / (1.0R + e(j)))

                grad(j) = p - label(j)
                hess(j) = p * (1.0R - p)
            Next
        End Sub

        ''' <summary>
        ''' 就地 sigmoid + 裁剪（用于输出概率）
        ''' </summary>
        ''' <param name="values">margin，就地替换为概率</param>
        ''' <param name="count"></param>
        ''' <param name="low"></param>
        ''' <param name="high"></param>
        Friend Sub SigmoidClampedInPlace(values As Double(), count As Integer, low As Double, high As Double)
            If count <= 0 Then
                Return
            End If

            If Not CanVectorize(count) Then
                For i As Integer = 0 To count - 1
                    values(i) = sysmath.Min(sysmath.Max(sigmoidScalar(values(i)), low), high)
                Next

                Return
            End If

            Dim width As Integer = sysvec.Count
            Dim one As New sysvec(1.0R)
            Dim zero As New sysvec(0.0R)
            Dim lo As New sysvec(low)
            Dim hi As New sysvec(high)
            Dim i2 As Integer = 0

            While i2 <= count - width
                Dim z As New sysvec(values, i2)
                Dim negAbs As sysvec = -vmath.Abs(z)

                ' exp(-|z|)：这里逐元素调用 Math.Exp，保持与标量分支一致的精度
                Dim ex As Double() = New Double(width - 1) {}

                For lane As Integer = 0 To width - 1
                    ex(lane) = sysmath.Exp(negAbs(lane))
                Next

                Dim ev As New sysvec(ex)
                Dim den As sysvec = one + ev
                Dim inv As sysvec = one / den
                Dim mask As sysvec = vmath.GreaterThanOrEqual(z, zero)
                Dim p As sysvec = vmath.ConditionalSelect(mask, inv, ev * inv)

                p = vmath.Min(vmath.Max(p, lo), hi)
                p.CopyTo(values, i2)

                i2 += width
            End While

            For j As Integer = i2 To count - 1
                values(j) = sysmath.Min(sysmath.Max(sigmoidScalar(values(j)), low), high)
            Next
        End Sub

        ''' <summary>
        ''' 就地 <c>y += alpha * x</c>（优先 FMA）。
        '''
        ''' 要求 <paramref name="x"/> 与 <paramref name="y"/> 等长：调用方必须使用行数长度的缓冲。
        ''' </summary>
        ''' <param name="alpha"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        Friend Sub Axpy(alpha As Double, x As Double(), y As Double())
            If x Is Nothing OrElse y Is Nothing OrElse x.Length = 0 OrElse y.Length = 0 Then
                Return
            End If

            Dim n As Integer = sysmath.Min(x.Length, y.Length)

            If Not CanVectorize(n) Then
                For i As Integer = 0 To n - 1
                    y(i) += alpha * x(i)
                Next

                Return
            End If

            Dim width As Integer = sysvec.Count
            Dim splat As New sysvec(alpha)
            Dim i2 As Integer = 0

            While i2 <= n - width
                Dim xv As New sysvec(x, i2)
                Dim yv As New sysvec(y, i2)
                Dim sum As sysvec = yv + splat * xv

                sum.CopyTo(y, i2)

                i2 += width
            End While

            For j As Integer = i2 To n - 1
                y(j) += alpha * x(j)
            Next
        End Sub

        ''' <summary>
        ''' 把「行 -&gt; 叶节点分数」收集成连续缓冲。
        '''
        ''' 这是唯一无法向量化的步骤（按任意行号 gather），但它只读一次指针数组，
        ''' 之后的乘加全部走 <see cref="Axpy"/>。
        ''' </summary>
        ''' <param name="nodes">行 -&gt; 叶节点</param>
        ''' <param name="buffer">输出缓冲</param>
        ''' <param name="count">行数</param>
        ''' <param name="hasLeaf">是否至少有一个行被分配；为 False 时不需要累加</param>
        Friend Sub GatherLeafScores(nodes As TreeNode(), buffer As Double(), count As Integer,
                                    ByRef hasLeaf As Boolean)
            hasLeaf = False

            For i As Integer = 0 To count - 1
                Dim leaf As TreeNode = nodes(i)

                If leaf Is Nothing Then
                    buffer(i) = 0
                Else
                    buffer(i) = leaf.leaf_score
                    hasLeaf = True
                End If
            Next
        End Sub

        ''' <summary>
        ''' 对连续区间求和（4 累加器展开）。
        '''
        ''' <c>SimdReduce</c> 只提供整数组求和，没有区间重载；这里按同样的展开方式实现，
        ''' 避免为了求和而切片拷贝。
        ''' </summary>
        ''' <param name="values"></param>
        ''' <param name="start"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Friend Function SumRange(values As Double(), start As Integer, count As Integer) As Double
            If values Is Nothing OrElse count <= 0 Then
                Return 0
            End If

            Dim ends As Integer = start + count

            If Not CanVectorize(count) Then
                Dim total As Double = 0

                For i As Integer = start To ends - 1
                    total += values(i)
                Next

                Return total
            End If

            Dim width As Integer = sysvec.Count
            Dim step4 As Integer = width * 4
            Dim acc0 As sysvec = sysvec.Zero
            Dim acc1 As sysvec = sysvec.Zero
            Dim acc2 As sysvec = sysvec.Zero
            Dim acc3 As sysvec = sysvec.Zero
            Dim i2 As Integer = start

            While i2 <= ends - step4
                acc0 += New sysvec(values, i2)
                acc1 += New sysvec(values, i2 + width)
                acc2 += New sysvec(values, i2 + width * 2)
                acc3 += New sysvec(values, i2 + width * 3)
                i2 += step4
            End While

            While i2 <= ends - width
                acc0 += New sysvec(values, i2)
                i2 += width
            End While

            Dim sum As Double = vmath.Sum(acc0 + acc1 + acc2 + acc3)

            While i2 < ends
                sum += values(i2)
                i2 += 1
            End While

            Return sum
        End Function

        ''' <summary>
        ''' 整数组求和（走库的 <c>SimdReduce</c>）
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        Friend Function SumFull(values As Double()) As Double
            If values Is Nothing OrElse values.Length = 0 Then
                Return 0
            End If

            If Not CanVectorize(values.Length) Then
                Dim total As Double = 0

                For i As Integer = 0 To values.Length - 1
                    total += values(i)
                Next

                Return total
            End If

            Return SimdReduce.Sum(values)
        End Function

        ''' <summary>
        ''' 掩码乘：<c>values(i) *= mask(i)</c>（<c>1/0</c> 掩码）
        ''' </summary>
        ''' <param name="values"></param>
        ''' <param name="mask"></param>
        ''' <param name="count"></param>
        Friend Sub MaskMultiply(values As Double(), mask As Double(), count As Integer)
            If values Is Nothing OrElse mask Is Nothing OrElse count <= 0 Then
                Return
            End If

            Dim n As Integer = sysmath.Min(count, sysmath.Min(values.Length, mask.Length))

            If Not CanVectorize(n) Then
                For i As Integer = 0 To n - 1
                    values(i) *= mask(i)
                Next

                Return
            End If

            Dim width As Integer = sysvec.Count
            Dim i2 As Integer = 0

            While i2 <= n - width
                Dim v As New sysvec(values, i2)
                Dim m As New sysvec(mask, i2)
                Dim scaled As sysvec = v * m

                scaled.CopyTo(values, i2)

                i2 += width
            End While

            For j As Integer = i2 To n - 1
                values(j) *= mask(j)
            Next
        End Sub

        ''' <summary>
        ''' 就地填充常数
        ''' </summary>
        ''' <param name="values"></param>
        ''' <param name="count"></param>
        ''' <param name="value"></param>
        Friend Sub Fill(values As Double(), count As Integer, value As Double)
            If values Is Nothing OrElse count <= 0 Then
                Return
            End If

            Dim n As Integer = sysmath.Min(count, values.Length)

            If Not CanVectorize(n) Then
                For i As Integer = 0 To n - 1
                    values(i) = value
                Next

                Return
            End If

            Dim width As Integer = sysvec.Count
            Dim splat As New sysvec(value)
            Dim i2 As Integer = 0

            While i2 <= n - width
                splat.CopyTo(values, i2)
                i2 += width
            End While

            For j As Integer = i2 To n - 1
                values(j) = value
            Next
        End Sub

        ''' <summary>
        ''' 点积（线性 booster 的评分主循环）
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Friend Function Dot(a As Double(), b As Double()) As Double
            If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length OrElse a.Length = 0 Then
                Return 0
            End If

            If Not CanVectorize(a.Length) Then
                Dim total As Double = 0

                For i As Integer = 0 To a.Length - 1
                    total += a(i) * b(i)
                Next

                Return total
            End If

            Return SimdIntrinsics.DotFma(a, b)
        End Function

        ''' <summary>
        ''' 单精度点积（线性 booster 的评分热循环）。
        '''
        ''' 语义与逐特征标量累加一致：<b>遇到缺失（NaN）的乘积按 0 处理</b>
        ''' （标量实现是 <c>If Not IsNaN(f) Then psum += f * w</c>）。
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Friend Function DotSingle(a As Single(), b As Single(), count As Integer) As Double
            If a Is Nothing OrElse b Is Nothing OrElse count <= 0 Then
                Return 0
            End If

            Dim n As Integer = sysmath.Min(count, sysmath.Min(a.Length, b.Length))

            If n <= 0 Then
                Return 0
            End If

            Dim width As Integer = sysvecs.Count

            If Not CanVectorize(n) Then
                Dim total As Double = 0

                For i As Integer = 0 To n - 1
                    Dim f As Double = a(i)

                    If Not Double.IsNaN(f) Then
                        total += f * b(i)
                    End If
                Next

                Return total
            End If

            Dim acc As sysvecs = sysvecs.Zero
            Dim zero As New sysvecs(0.0F)
            Dim i2 As Integer = 0

            While i2 <= n - width
                Dim av As New sysvecs(a, i2)
                Dim bv As New sysvecs(b, i2)
                Dim prod As sysvecs = av * bv
                Dim notNaN As sysvecs = vmaths.Equals(prod, prod)

                acc += vmaths.ConditionalSelect(notNaN, prod, zero)

                i2 += width
            End While

            Dim total2 As Double = vmaths.Sum(acc)

            For j As Integer = i2 To n - 1
                Dim f As Double = a(j)

                If Not Double.IsNaN(f) Then
                    total2 += f * b(j)
                End If
            Next

            Return total2
        End Function
    End Module
End Namespace
