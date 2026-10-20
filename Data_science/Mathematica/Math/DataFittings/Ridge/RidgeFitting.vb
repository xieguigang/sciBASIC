Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Ridge

    ''' <summary>
    ''' 岭回归（Ridge，L2 正则化的线性回归）的求解器
    ''' 
    ''' 岭回归求解的是下面这个带 L2 惩罚项的最小二乘问题：
    ''' 
    ''' ```
    ''' min ||y - X*b||^2 + n * lambda * ||b||^2
    ''' ```
    ''' 
    ''' 它对参数仍然是线性的，因此存在闭式解；惩罚项让正规方程的系数矩阵
    ''' 变成 ``X'X + n*lambda*I``，即便 ``X'X`` 因为自变量完全共线性而奇异，
    ''' 只要 ``lambda > 0`` 这个矩阵就一定是正定的，于是可以直接用 Cholesky 分解求解。
    ''' 
    ''' ### 为什么要标准化
    ''' 
    ''' L2 惩罚对各个系数是同等对待的，自变量的量纲不同会让惩罚的效果完全不一样，
    ''' 因此标准做法是先对每一列做中心化与标准化再拟合，最后把系数还原回原始量纲；
    ''' 截距则单独估计、**不施加惩罚**（否则模型会依赖于 y 的原点位置）。
    ''' 
    ''' ### 尺度约定
    ''' 
    ''' 惩罚项写作 ``n * lambda * ||b||^2`` 而不是 ``lambda * ||b||^2``，
    ''' 是为了让 lambda 与相关矩阵 ``R = X'X/n`` 的特征值
    ''' （取值范围 ``[0, p]``）同尺度，从而**与样本量 n 无关**，
    ''' 同一组 lambda 网格在不同样本量的数据上具有相同的含义。
    ''' </summary>
    Public Module RidgeFitting

        ''' <summary>
        ''' 默认的 lambda 网格下界
        ''' </summary>
        Public Const DefaultMinLambda As Double = 0.0001
        ''' <summary>
        ''' 默认的 lambda 网格上界
        ''' </summary>
        Public Const DefaultMaxLambda As Double = 10.0
        ''' <summary>
        ''' 默认的 lambda 网格步数
        ''' </summary>
        Public Const DefaultLambdaSteps As Integer = 100

        ''' <summary>
        ''' 对给定的自变量矩阵与观测值向量执行岭回归拟合
        ''' </summary>
        ''' <param name="x">自变量矩阵，每一行是一个样本、每一列是一个特征</param>
        ''' <param name="y">与 <paramref name="x"/> 各行对应的观测值向量</param>
        ''' <param name="lambda">
        ''' 指定的正则化强度。给出了这个参数的时候就只在该 lambda 之上求解一次
        ''' （路径上只有 1 步）；否则会在对数间隔的 lambda 网格上面求解出
        ''' 完整的正则化路径，再用 GCV 准则挑出最优的那一步
        ''' </param>
        ''' <param name="numberOfLambdas">lambda 网格的步数</param>
        ''' <param name="lambdaMin">lambda 网格的下界</param>
        ''' <param name="lambdaMax">lambda 网格的上界</param>
        ''' <param name="standardize">
        ''' 是否先对每一列做中心化与标准化；False 的时候只做中心化
        ''' </param>
        ''' <param name="featureNames">特征列的名称，缺省时自动生成 ``[1]..[p]``</param>
        ''' <returns>岭回归的拟合结果（含正则化路径）</returns>
        Public Function Fit(x As Double()(),
                            y As Double(),
                            Optional lambda As Double? = Nothing,
                            Optional numberOfLambdas As Integer = 100,
                            Optional lambdaMin As Double = 0.0001,
                            Optional lambdaMax As Double = 10.0,
                            Optional standardize As Boolean = True,
                            Optional featureNames As String() = Nothing) As RidgeFit

            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.Length = 0 OrElse x(0).Length = 0 Then
                Throw New ArgumentException("the ridge regression requires a non-empty feature matrix!", NameOf(x))
            End If
            If x.Length <> y.Length Then
                Throw New ArgumentException($"the size of the feature matrix ({x.Length}) is not matched with the size of the observed values ({y.Length})!", NameOf(y))
            End If

            Dim n As Integer = y.Length
            Dim p As Integer = x(0).Length

            For i As Integer = 0 To n - 1
                If x(i).Length <> p Then
                    Throw New ArgumentException($"the row {i} of the feature matrix has {x(i).Length} features, but the first row has {p}!", NameOf(x))
                End If
            Next

            ' 1) 预处理：中心化（+标准化）
            Dim means As Double() = New Double(p - 1) {}
            Dim scales As Double() = New Double(p - 1) {}
            Dim yMean As Double = y.Average()

            For j As Integer = 0 To p - 1
                Dim sum As Double = 0.0

                For i As Integer = 0 To n - 1
                    sum += x(i)(j)
                Next

                means(j) = sum / n

                If standardize Then
                    Dim var As Double = 0.0

                    For i As Integer = 0 To n - 1
                        Dim d As Double = x(i)(j) - means(j)

                        var += d * d
                    Next

                    ' 常数列（以及只有一个样本的列）没有尺度可言，守卫成 1，
                    ' 否则会在这里除零、把整列数据都变成 NaN
                    scales(j) = If(n > 1 AndAlso var > 0.0, std.Sqrt(var / (n - 1)), 1.0)
                Else
                    scales(j) = 1.0
                End If
            Next

            Dim xc As Double()() = RectangularArray.Matrix(Of Double)(n, p)
            Dim yc As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                yc(i) = y(i) - yMean

                For j As Integer = 0 To p - 1
                    xc(i)(j) = (x(i)(j) - means(j)) / scales(j)
                Next
            Next

            ' 2) R = Xc'Xc / n
            '    只计算上三角再镜像，保证 R 逐位对称：Cholesky 分解的对称
            '    性判据用的是精确比较（A(k)(j) = A(j)(k)），浮点不对称会被判成非正定
            Dim R As Double()() = RectangularArray.Matrix(Of Double)(p, p)

            For a As Integer = 0 To p - 1
                For b As Integer = a To p - 1
                    Dim sum As Double = 0.0

                    For i As Integer = 0 To n - 1
                        sum += xc(i)(a) * xc(i)(b)
                    Next

                    R(a)(b) = sum / n
                    R(b)(a) = sum / n
                Next
            Next

            ' 3) xty = Xc'yc / n
            Dim xty As Double() = New Double(p - 1) {}

            For a As Integer = 0 To p - 1
                Dim sum As Double = 0.0

                For i As Integer = 0 To n - 1
                    sum += xc(i)(a) * yc(i)
                Next

                xty(a) = sum / n
            Next

            Dim sst As Double = 0.0

            For i As Integer = 0 To n - 1
                sst += yc(i) * yc(i)
            Next

            ' 4) lambda 网格
            Dim grid As Double()

            If lambda.HasValue Then
                If lambda.Value <= 0.0 Then
                    Throw New ArgumentException("the ridge lambda must be a positive number!", NameOf(lambda))
                End If

                grid = New Double() {lambda.Value}
            Else
                grid = LambdaGrid(numberOfLambdas, lambdaMin, lambdaMax)
            End If

            ' 5) 在每一个 lambda 上求解
            Dim size As Integer = grid.Length
            Dim lambdas As Double() = New Double(size - 1) {}
            Dim betas As Double()() = RectangularArray.Matrix(Of Double)(size, p)
            Dim rss As Double() = New Double(size - 1) {}
            Dim rsquared As Double() = New Double(size - 1) {}
            Dim dfs As Double() = New Double(size - 1) {}
            Dim gcvs As Double() = New Double(size - 1) {}
            Dim used As Integer = 0

            For k As Integer = 0 To size - 1
                Dim beta As Double() = Nothing
                Dim df As Double = 0.0

                If Not SolveRidge(R, xty, p, grid(k), beta, df) Then
                    ' 该 lambda 上没有得到正定的系数矩阵，跳过这一步
                    Continue For
                End If

                Dim err As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim yhat As Double = 0.0

                    For j As Integer = 0 To p - 1
                        yhat += xc(i)(j) * beta(j)
                    Next

                    Dim d As Double = yc(i) - yhat

                    err += d * d
                Next

                ' GCV = RSS / (n * (1 - (df + 1) / n)^2)，其中的 +1 是截距占掉的自由度
                Dim shrink As Double = 1.0 - (df + 1.0) / n

                lambdas(used) = grid(k)
                betas(used) = beta
                rss(used) = err
                dfs(used) = df
                rsquared(used) = If(sst > 0.0, 1.0 - err / sst, If(err <= 0.0, 1.0, 0.0))
                gcvs(used) = If(shrink > 0.0, err / (n * shrink * shrink), Double.PositiveInfinity)

                used += 1
            Next

            If used = 0 Then
                Throw New InvalidOperationException("the ridge regression can not be solved on the given data, please increase the lambda value!")
            End If

            ' 6) 把路径上被跳过的步骤裁剪掉
            If used < size Then
                Dim trimLambda As Double() = New Double(used - 1) {}
                Dim trimRss As Double() = New Double(used - 1) {}
                Dim trimR2 As Double() = New Double(used - 1) {}
                Dim trimDf As Double() = New Double(used - 1) {}
                Dim trimGcv As Double() = New Double(used - 1) {}
                Dim trimBeta As Double()() = New Double(used - 1)() {}

                Call Array.Copy(lambdas, trimLambda, used)
                Call Array.Copy(rss, trimRss, used)
                Call Array.Copy(rsquared, trimR2, used)
                Call Array.Copy(dfs, trimDf, used)
                Call Array.Copy(gcvs, trimGcv, used)
                Call Array.Copy(betas, trimBeta, used)

                lambdas = trimLambda
                rss = trimRss
                rsquared = trimR2
                dfs = trimDf
                gcvs = trimGcv
                betas = trimBeta
            End If

            ' 7) 还原到原始量纲：coef = beta / s，截距 = ybar - sum(beta * m / s)
            Dim coefs As Double()() = RectangularArray.Matrix(Of Double)(used, p)
            Dim intercepts As Double() = New Double(used - 1) {}

            For k As Integer = 0 To used - 1
                Dim shift As Double = 0.0

                For j As Integer = 0 To p - 1
                    coefs(k)(j) = betas(k)(j) / scales(j)
                    shift += betas(k)(j) * means(j) / scales(j)
                Next

                intercepts(k) = yMean - shift
            Next

            ' 8) GCV 最小的那一步就是最优的 lambda
            Dim best As Integer = 0

            For k As Integer = 1 To used - 1
                If gcvs(k) < gcvs(best) Then
                    best = k
                End If
            Next

            Dim names As String() = featureNames

            If names Is Nothing OrElse names.Length <> p Then
                names = Enumerable.Range(1, p).Select(Function(i) $"[{i}]").ToArray
            End If

            Dim result As New RidgeFit With {
                .lambdas = lambdas,
                .intercepts = intercepts,
                .coefficients = coefs,
                .rsquared = rsquared,
                .rss = rss,
                .df = dfs,
                .gcv = gcvs,
                .featureNames = names,
                .standardize = standardize,
                .bestIndex = best,
                .SSE = rss(best),
                .SST = sst
            }

            ' 逐点误差测试点
            Dim tests As IFitError() = New IFitError(n - 1) {}
            Dim bestCoef As Double() = coefs(best)
            Dim bestIcept As Double = intercepts(best)

            For i As Integer = 0 To n - 1
                Dim yhat As Double = bestIcept

                For j As Integer = 0 To p - 1
                    yhat += bestCoef(j) * x(i)(j)
                Next

                tests(i) = New TestPoint With {
                    .Y = y(i),
                    .Yfit = yhat
                }
            Next

            result.ErrorTest = tests

            Return result
        End Function

        ''' <summary>
        ''' 生成对数间隔的 lambda 网格
        ''' </summary>
        Private Function LambdaGrid(steps As Integer, lambdaMin As Double, lambdaMax As Double) As Double()
            If lambdaMin <= 0.0 OrElse lambdaMax <= 0.0 Then
                Throw New ArgumentException("the lambda grid bounds must be positive numbers!")
            End If
            If lambdaMax < lambdaMin Then
                Dim swap As Double = lambdaMin

                lambdaMin = lambdaMax
                lambdaMax = swap
            End If
            If steps < 1 Then
                steps = 1
            End If

            Dim grid As Double() = New Double(steps - 1) {}
            Dim logMin As Double = std.Log(lambdaMin)
            Dim logMax As Double = std.Log(lambdaMax)

            If steps = 1 Then
                grid(0) = std.Exp((logMin + logMax) / 2.0)
                Return grid
            End If

            For i As Integer = 0 To steps - 1
                grid(i) = std.Exp(logMin + (logMax - logMin) * i / (steps - 1))
            Next

            Return grid
        End Function

        ''' <summary>
        ''' 求解 ``(R + lambda*I) * beta = xty``，并且给出该 lambda 之下的有效自由度
        ''' 
        ''' 帽子矩阵 ``H = Xc(Xc'Xc + n*lambda*I)^(-1)Xc'`` 的迹就是有效自由度，
        ''' 由 ``tr(H) = p - lambda * tr((R + lambda*I)^(-1))`` 给出
        ''' （lambda 趋于 0 时 df 趋于 p，lambda 趋于无穷时 df 趋于 0）。
        ''' </summary>
        ''' <returns>系数矩阵是否正定（否则该 lambda 无解）</returns>
        Private Function SolveRidge(R As Double()(),
                                    xty As Double(),
                                    p As Integer,
                                    lambda As Double,
                                    ByRef beta As Double(),
                                    ByRef df As Double) As Boolean

            Dim B As Double()() = RectangularArray.Matrix(Of Double)(p, p)

            For i As Integer = 0 To p - 1
                For j As Integer = 0 To p - 1
                    B(i)(j) = R(i)(j)
                Next

                B(i)(i) += lambda
            Next

            Dim A As NumericMatrix = New NumericMatrix(B)
            Dim chol As CholeskyDecomposition = A.chol()

            If Not chol.SPD Then
                Return False
            End If

            ' beta = (R + lambda*I)^(-1) * xty
            Dim rhs As NumericMatrix = New NumericMatrix(xty, p)
            Dim sol As NumericMatrix = CType(chol.Solve(rhs), NumericMatrix)

            beta = New Double(p - 1) {}

            For i As Integer = 0 To p - 1
                beta(i) = sol(i, 0)
            Next

            ' 对单位阵求解就是求逆，取对角线求和得到 tr(B^(-1))
            Dim inv As NumericMatrix = CType(chol.Solve(NumericMatrix.Identity(p)), NumericMatrix)
            Dim trace As Double = 0.0

            For i As Integer = 0 To p - 1
                trace += inv(i, i)
            Next

            df = p - lambda * trace

            ' 数值误差可能让 df 越出 [0, p]，这里夹回去
            If df < 0.0 Then
                df = 0.0
            ElseIf df > p Then
                df = p
            End If

            Return True
        End Function
    End Module
End Namespace
