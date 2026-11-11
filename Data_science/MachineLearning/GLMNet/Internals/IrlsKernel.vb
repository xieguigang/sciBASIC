Imports std = System.Math

''' <summary>
''' 二项 / 泊松 / 多分类共用的 IRLS 数学工具：工作响应、工作权重与偏差。
''' </summary>
''' <remarks>
''' glmnet 统一最小化 <c>1/(2n)*deviance + lambda*P(beta)</c>；
''' IRLS 在当前的线性预测子处把偏差局部二次化：
''' <c>z = eta + (y - mu)/q</c>、<c>w = base * q</c>，
''' 其中 <c>q</c> 是 Fisher 权重。返回的 <c>z</c> 已剥离 offset，
''' 因此坐标下降只负责拟合 <c>a0 + X*beta</c>。
''' </remarks>
Friend Module IrlsKernel

    ''' <summary>sigmoid 链接的逆函数，带溢出保护。</summary>
    Public Function Sigmoid(eta As Double) As Double
        Dim e As Double = Convergence.ClampEta(eta)

        If e >= 0.0R Then
            Return 1.0R / (1.0R + std.Exp(-e))
        Else
            Dim t As Double = std.Exp(e)
            Return t / (1.0R + t)
        End If
    End Function

    ''' <summary>
    ''' 二项式 IRLS：由含 offset 的线性预测子计算工作响应与工作权重。
    ''' </summary>
    ''' <param name="eta">完整线性预测子（含 offset）</param>
    ''' <param name="y">0/1 或比例响应</param>
    ''' <param name="baseWeights">归一化到和为 n 的观测权重</param>
    ''' <param name="offset">link scale 偏移</param>
    ''' <param name="workingResponse">输出：剥离 offset 的工作响应</param>
    ''' <param name="workingWeights">输出：归一化到和为 n 的工作权重</param>
    Public Sub BinomialWorking(eta As Double(), y As Double(), baseWeights As Double(), offset As Double(),
                               ByRef workingResponse As Double(), ByRef workingWeights As Double())
        Dim n As Integer = y.Length
        workingResponse = New Double(n - 1) {}
        workingWeights = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim p As Double = Sigmoid(eta(i))
            p = std.Min(std.Max(p, GlmnetControl.MinProbability), 1.0R - GlmnetControl.MinProbability)
            Dim q As Double = std.Max(p * (1.0R - p), GlmnetControl.MinCurvature)

            ' 牛顿步：z = eta + (y - mu)/V，W_i = w_i*V。
            ' W 不能再归一化到 sum(w)=n：那等价于整体放大数据项、
            ' 相对削弱 lambda，会让系数路径收缩过快（与 R 5.1 对拍已验证）。
            workingResponse(i) = (eta(i) - offset(i)) + (y(i) - p) / q
            workingWeights(i) = baseWeights(i) * q
        Next
    End Sub

    ''' <summary>
    ''' 泊松 IRLS：对数链接下 <c>mu = exp(eta)</c>。
    ''' </summary>
    Public Sub PoissonWorking(eta As Double(), y As Double(), baseWeights As Double(), offset As Double(),
                              weightScale As Double,
                              ByRef workingResponse As Double(), ByRef workingWeights As Double())
        Dim n As Integer = y.Length
        Dim scale As Double = std.Max(weightScale, GlmnetControl.MinCurvature)
        workingResponse = New Double(n - 1) {}
        workingWeights = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim mu As Double = std.Min(std.Max(Convergence.SafeExp(eta(i)), GlmnetControl.MinProbability), 1000000000000.0R)
            Dim q As Double = std.Max(mu, GlmnetControl.MinCurvature)

            ' 把 Fisher 权重按固定常数缩放，可让坐标下降的曲率回到 1 附近；
            ' 调用方必须同步把 lambda 除以同一个常数，二者相除后目标函数
            ' 只差一个整体常数倍，最优解保持不变（glmnet fishnet 的数值稳定做法）。
            workingResponse(i) = (eta(i) - offset(i)) + (y(i) - mu) / q
            workingWeights(i) = baseWeights(i) * q / scale
        Next
    End Sub

    ''' <summary>高斯偏差：<c>sum w*(y-eta)^2</c>（未除以 n，与 R 的 nulldev 同尺度）。</summary>
    Public Function GaussianDeviance(y As Double(), eta As Double(), weights As Double()) As Double
        Dim dev As Double = 0.0R
        For i As Integer = 0 To y.Length - 1
            Dim d As Double = y(i) - eta(i)
            dev += weights(i) * d * d
        Next
        Return dev
    End Function

    ''' <summary>二项式偏差：<c>-2*sum w*[y*log(p) + (1-y)*log(1-p)]</c>。</summary>
    Public Function BinomialDeviance(y As Double(), eta As Double(), weights As Double()) As Double
        Dim dev As Double = 0.0R
        For i As Integer = 0 To y.Length - 1
            Dim p As Double = std.Min(std.Max(Sigmoid(eta(i)), GlmnetControl.MinProbability), 1.0R - GlmnetControl.MinProbability)
            dev -= 2.0R * weights(i) * (y(i) * Convergence.SafeLog(p) + (1.0R - y(i)) * Convergence.SafeLog(1.0R - p))
        Next
        Return std.Max(0.0R, dev)
    End Function

    ''' <summary>泊松偏差：<c>2*sum w*[y*log(y/mu) - (y-mu)]</c>。</summary>
    Public Function PoissonDeviance(y As Double(), eta As Double(), weights As Double()) As Double
        Dim dev As Double = 0.0R
        For i As Integer = 0 To y.Length - 1
            Dim mu As Double = std.Max(Convergence.SafeExp(eta(i)), GlmnetControl.MinProbability)
            Dim yi As Double = y(i)
            Dim ylog As Double = If(yi > 0.0R, yi * Convergence.SafeLog(yi / mu), 0.0R)
            dev += 2.0R * weights(i) * (ylog - (yi - mu))
        Next
        Return std.Max(0.0R, dev)
    End Function

    ''' <summary>多分类偏差：<c>-2*sum_i w_i*sum_c y_ic*log(p_ic)</c>。</summary>
    Public Function MultinomialDeviance(y As Double()(), probs As Double()(), weights As Double()) As Double
        Dim dev As Double = 0.0R
        Dim n As Integer = y.Length
        Dim k As Integer = y(0).Length

        For i As Integer = 0 To n - 1
            For c As Integer = 0 To k - 1
                If y(i)(c) > 0.0R Then
                    dev -= 2.0R * weights(i) * y(i)(c) * Convergence.SafeLog(probs(i)(c))
                End If
            Next
        Next

        Return std.Max(0.0R, dev)
    End Function

    ''' <summary>按权重求加权均值（权重和为 n）。</summary>
    Public Function WeightedMean(values As Double(), weights As Double()) As Double
        Dim sum As Double = 0.0R
        Dim n As Integer = values.Length
        For i As Integer = 0 To n - 1
            sum += weights(i) * values(i)
        Next
        Return sum / CDbl(n)
    End Function

    ''' <summary>多分类 softmax（数值稳定的 log-sum-exp）。</summary>
    Public Sub Softmax(eta As Double()(), ByRef probs As Double()())
        Dim n As Integer = eta.Length
        Dim k As Integer = eta(0).Length
        probs = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            Dim maxEta As Double = Double.NegativeInfinity
            For c As Integer = 0 To k - 1
                If eta(i)(c) > maxEta Then maxEta = eta(i)(c)
            Next

            Dim row As Double() = New Double(k - 1) {}
            Dim total As Double = 0.0R
            For c As Integer = 0 To k - 1
                row(c) = std.Exp(Convergence.ClampEta(eta(i)(c) - maxEta))
                total += row(c)
            Next
            For c As Integer = 0 To k - 1
                row(c) = std.Max(row(c) / total, GlmnetControl.MinProbability)
            Next
            probs(i) = row
        Next
    End Sub

End Module
