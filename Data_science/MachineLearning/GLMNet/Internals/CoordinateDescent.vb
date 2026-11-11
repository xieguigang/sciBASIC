Imports std = System.Math

''' <summary>
''' 共享的加权弹性网坐标下降内核（column-major、残差增量更新）。
''' </summary>
''' <remarks>
''' 求解 <c>1/(2n)*sum_i w_i*(z_i - a0 - x_i'beta)^2
''' + lambda*[alpha*sum_j pf_j*|beta_j| + (1-alpha)/2*sum_j beta_j^2]</c>。
''' 权重已归一化到 <c>sum(w)=n</c>；<c>z</c> 是已剥离 offset 的工作响应。
''' 每次坐标更新为
''' <c>beta_j := soft(rho_j, lambda*alpha*pf_j) / (curv_j + lambda*(1-alpha))</c>，
''' 其中 <c>rho_j = sum_i w_i*x_ij*r_i/n + curv_j*beta_j</c>。
''' </remarks>
Friend Module CoordinateDescent

    ''' <summary>单次坐标下降求解的结果。</summary>
    Friend NotInheritable Class CDResult
        Public Property Beta As Double()
        Public Property A0 As Double
        Public Property Objective As Double
        Public Property Residual As Double()
        Public Property Iterations As Integer
        Public Property Converged As Boolean
    End Class

    ''' <summary>软阈值算子：<c>sign(z)*max(|z|-gamma, 0)</c>。</summary>
    Public Function SoftThreshold(z As Double, gamma As Double) As Double
        If gamma <= 0.0R Then
            Return z
        End If
        If z > gamma Then
            Return z - gamma
        ElseIf z < -gamma Then
            Return z + gamma
        End If

        Return 0.0R
    End Function

    ''' <summary>
    ''' 在给定的工作响应与工作权重之下求解一个单响应加权弹性网。
    ''' </summary>
    ''' <param name="columns">标准化后的列主序矩阵</param>
    ''' <param name="workingResponse">工作响应（已剥离 offset）</param>
    ''' <param name="workingWeights">归一化到和为 n 的工作权重</param>
    ''' <param name="beta">输入为 warm start、输出为解（标准化空间）</param>
    ''' <param name="a0">输入为 warm start、输出为截距（标准化空间）</param>
    ''' <param name="activeMask">强规则给出的候选变量；为空表示全部变量</param>
    Public Function SolveSingle(columns As Double()(),
                                workingResponse As Double(),
                                workingWeights As Double(),
                                lambda As Double,
                                alpha As Double,
                                pf As PenaltyFactors,
                                lower As Double(),
                                upper As Double(),
                                beta As Double(),
                                a0 As Double,
                                fitIntercept As Boolean,
                                thresh As Double,
                                maxIt As Integer,
                                Optional activeMask As Boolean() = Nothing) As CDResult

        Dim p As Integer = columns.Length
        Dim n As Integer = workingResponse.Length
        Dim curvature As Double() = New Double(p - 1) {}
        Dim l1 As Double = lambda * alpha
        Dim l2 As Double = lambda * (1.0R - alpha)

        For j As Integer = 0 To p - 1
            Dim col As Double() = columns(j)
            Dim sum As Double = 0.0R
            For i As Integer = 0 To n - 1
                sum += workingWeights(i) * col(i) * col(i)
            Next
            curvature(j) = sum / CDbl(n)
        Next

        Dim residual As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            residual(i) = workingResponse(i) - a0
        Next
        For j As Integer = 0 To p - 1
            If beta(j) <> 0.0R Then
                Dim col As Double() = columns(j)
                For i As Integer = 0 To n - 1
                    residual(i) -= beta(j) * col(i)
                Next
            End If
        Next

        Dim previousObjective As Double = Double.PositiveInfinity
        Dim objective As Double = 0.0R
        Dim iterations As Integer = 0
        Dim converged As Boolean = False

        While iterations < maxIt
            iterations += 1

            For j As Integer = 0 To p - 1
                If activeMask IsNot Nothing AndAlso Not activeMask(j) Then
                    Continue For
                End If
                If Not pf.CanEnter(j) Then
                    If beta(j) <> 0.0R Then
                        Dim col0 As Double() = columns(j)
                        For i As Integer = 0 To n - 1
                            residual(i) += beta(j) * col0(i)
                        Next
                        beta(j) = 0.0R
                    End If
                    Continue For
                End If

                Dim curv As Double = curvature(j)
                If curv <= GlmnetControl.MinCurvature Then
                    beta(j) = 0.0R
                    Continue For
                End If

                Dim col As Double() = columns(j)
                Dim rho As Double = 0.0R

                For i As Integer = 0 To n - 1
                    rho += workingWeights(i) * col(i) * residual(i)
                Next
                rho = rho / CDbl(n) + curv * beta(j)

                Dim penalty As Double = pf.Values(j)
                Dim denominator As Double = curv + If(penalty > 0.0R, l2, 0.0R)
                Dim updated As Double = SoftThreshold(rho, l1 * penalty) / denominator

                If lower IsNot Nothing AndAlso updated < lower(j) Then updated = lower(j)
                If upper IsNot Nothing AndAlso updated > upper(j) Then updated = upper(j)
                If Not Double.IsFinite(updated) Then
                    Return New CDResult With {
                        .Beta = beta, .A0 = a0, .Iterations = iterations,
                        .Converged = False, .Objective = Double.NaN,
                        .Residual = residual
                    }
                End If

                Dim delta As Double = updated - beta(j)
                If delta <> 0.0R Then
                    beta(j) = updated
                    For i As Integer = 0 To n - 1
                        residual(i) -= delta * col(i)
                    Next
                End If
            Next

            If fitIntercept Then
                Dim shift As Double = 0.0R
                For i As Integer = 0 To n - 1
                    shift += workingWeights(i) * residual(i)
                Next
                shift /= CDbl(n)
                If shift <> 0.0R Then
                    a0 += shift
                    For i As Integer = 0 To n - 1
                        residual(i) -= shift
                    Next
                End If
            End If

            Dim loss As Double = 0.0R
            For i As Integer = 0 To n - 1
                loss += workingWeights(i) * residual(i) * residual(i)
            Next
            loss /= 2.0R * CDbl(n)

            Dim penaltyValue As Double = 0.0R
            For j As Integer = 0 To p - 1
                penaltyValue += l1 * pf.Values(j) * std.Abs(beta(j))
                penaltyValue += 0.5R * If(pf.Values(j) > 0.0R, l2, 0.0R) * beta(j) * beta(j)
            Next
            objective = loss + penaltyValue

            If Double.IsFinite(previousObjective) AndAlso Convergence.HasConverged(previousObjective, objective, thresh) Then
                converged = True
                Exit While
            End If
            previousObjective = objective
        End While

        Return New CDResult With {
            .Beta = beta,
            .A0 = a0,
            .Objective = objective,
            .Residual = residual,
            .Iterations = iterations,
            .Converged = converged
        }
    End Function

    ''' <summary>
    ''' 计算每个变量的 KKT 梯度（用于强规则与收敛后的违规检查）。
    ''' </summary>
    Public Function KktGradients(columns As Double()(), residual As Double(), workingWeights As Double()) As Double()
        Dim p As Integer = columns.Length
        Dim n As Integer = residual.Length
        Dim gradients As Double() = New Double(p - 1) {}

        For j As Integer = 0 To p - 1
            Dim col As Double() = columns(j)
            Dim sum As Double = 0.0R
            For i As Integer = 0 To n - 1
                sum += workingWeights(i) * col(i) * residual(i)
            Next
            gradients(j) = sum / CDbl(n)
        Next

        Return gradients
    End Function

    ''' <summary>
    ''' sequential strong rule：筛掉本步不可能进入模型的变量。
    ''' 已经非零或永不惩罚的变量必须始终保留。
    ''' </summary>
    Public Function StrongRuleMask(gradients As Double(), beta As Double(),
                                  lambda As Double, lambdaPrevious As Double,
                                  alpha As Double, pf As PenaltyFactors) As Boolean()
        Dim p As Integer = gradients.Length
        Dim mask As Boolean() = New Boolean(p - 1) {}
        Dim thresholdBase As Double = alpha * (2.0R * lambda - lambdaPrevious)

        For j As Integer = 0 To p - 1
            mask(j) = pf.CanEnter(j) AndAlso (
                beta(j) <> 0.0R OrElse
                pf.Values(j) <= 0.0R OrElse
                thresholdBase <= 0.0R OrElse
                std.Abs(gradients(j)) >= thresholdBase * pf.Values(j))
        Next

        Return mask
    End Function

    ''' <summary>
    ''' KKT 违规检查：返回那些被强规则剔除、但梯度仍超过 L1 阈值的变量下标。
    ''' </summary>
    Public Function KktViolations(gradients As Double(), beta As Double(), mask As Boolean(),
                                  lambda As Double, alpha As Double, pf As PenaltyFactors) As List(Of Integer)
        Dim violations As New List(Of Integer)()
        Dim threshold As Double = lambda * alpha

        For j As Integer = 0 To pBound(gradients) - 1
            If mask(j) OrElse beta(j) <> 0.0R OrElse Not pf.CanEnter(j) Then
                Continue For
            End If
            If std.Abs(gradients(j)) > threshold * pf.Values(j) + GlmnetControl.DefaultEps Then
                violations.Add(j)
            End If
        Next

        Return violations
    End Function

    Private Function pBound(v As Double()) As Integer
        Return v.Length
    End Function

End Module
