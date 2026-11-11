Imports std = System.Math

''' <summary>
''' lambda 网格：由零模型梯度（KKT 条件）推导 <c>lambda.max</c>，并按对数等间隔下降。
''' </summary>
''' <remarks>
''' glmnet 对所有 GLM 统一最小化 <c>1/(2n)*deviance + lambda*P(beta)</c>，
''' 因此在 beta=0 处，变量 j 的梯度为
''' <c>-1/n * sum_i w_i*x_ij*(y_i - mu_i)</c>，KKT 条件给出
''' <c>lambda.max = max_j |score_j| / (alpha * pf_j)</c>。
''' 对 gaussian 该式退化为 <c>max_j |sum_i w_i*x_ij*(y_i - ybar)| / (n*alpha)</c>。
''' </remarks>
Friend Module LambdaGrid

    ''' <summary>
    ''' 由逐变量梯度分数计算 lambda.max。
    ''' </summary>
    ''' <param name="scores">每个变量的 <c>|sum_i w_i*x_ij*(y_i-mu_i)|/n</c></param>
    ''' <param name="alpha">弹性网混合参数</param>
    ''' <param name="pf">重缩放后的惩罚因子</param>
    Public Function LambdaMax(scores As Double(), alpha As Double, pf As PenaltyFactors) As Double
        Dim top As Double = 0.0R

        For j As Integer = 0 To scores.Length - 1
            If Not pf.CanEnter(j) OrElse pf.Values(j) <= 0.0R Then
                Continue For
            End If

            Dim candidate As Double = scores(j) / pf.Values(j)

            If Double.IsFinite(candidate) AndAlso candidate > top Then
                top = candidate
            End If
        Next

        If alpha > GlmnetControl.MinCurvature Then
            top /= alpha
        End If
        If top <= 0.0R OrElse Not Double.IsFinite(top) Then
            ' 零模型已经完全拟合，或者全部变量被 exclude：
            ' 退回一个正的哨兵值，保证网格仍然可用。
            top = GlmnetControl.MinCurvature
        End If

        Return top
    End Function

    ''' <summary>
    ''' 生成网格：优先使用调用方给出的 lambda 序列，否则由 lambda.max 自动生成。
    ''' </summary>
    Public Function Build(options As GlmnetOptions, scores As Double(), n As Integer, p As Integer, alpha As Double, pf As PenaltyFactors) As Double()
        If options.Lambda IsNot Nothing Then
            Return CType(options.Lambda.Clone(), Double())
        End If

        Dim top As Double = LambdaMax(scores, alpha, pf)
        Dim ratio As Double = options.ResolveLambdaMinRatio(n, p)

        Return GlmnetControl.MakeLambdaGrid(top, options.NLambda, ratio)
    End Function

    ''' <summary>
    ''' 计算标准化列上的零模型梯度分数。
    ''' </summary>
    ''' <param name="columns">标准化后的列主序矩阵</param>
    ''' <param name="weightedResidual">每个样本的 <c>w_i*(y_i - mu_i)</c></param>
    ''' <param name="n">样本数</param>
    Public Function GradientScores(columns As Double()(), weightedResidual As Double(), n As Integer) As Double()
        Dim p As Integer = columns.Length
        Dim scores As Double() = New Double(p - 1) {}

        For j As Integer = 0 To p - 1
            Dim col As Double() = columns(j)
            Dim sum As Double = 0.0R

            For i As Integer = 0 To n - 1
                sum += col(i) * weightedResidual(i)
            Next

            scores(j) = std.Abs(sum) / CDbl(n)
        Next

        Return scores
    End Function

    ''' <summary>
    ''' 多响应 / 多分类的分组梯度分数：取每个变量在 K 个响应上的梯度 2-范数。
    ''' </summary>
    Public Function GroupGradientScores(columns As Double()(), weightedResidual As Double()(), n As Integer, k As Integer) As Double()
        Dim p As Integer = columns.Length
        Dim scores As Double() = New Double(p - 1) {}

        For j As Integer = 0 To p - 1
            Dim col As Double() = columns(j)
            Dim norm As Double = 0.0R

            For c As Integer = 0 To k - 1
                Dim sum As Double = 0.0R

                For i As Integer = 0 To n - 1
                    sum += col(i) * weightedResidual(i)(c)
                Next

                sum /= CDbl(n)
                norm += sum * sum
            Next

            scores(j) = std.Sqrt(norm)
        Next

        Return scores
    End Function

End Module
