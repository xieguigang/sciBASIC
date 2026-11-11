Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

Namespace MGaussian

    ''' <summary>交叉验证可用的评估指标（mgaussian 只支持 mse 与 mae）</summary>
    Public Enum CVMeasure
        ''' <summary>所有响应合并计算的均方误差</summary>
        Mse
        ''' <summary>所有响应合并计算的平均绝对误差</summary>
        Mae
    End Enum

    ''' <summary>
    ''' 多响应高斯模型的 K 折交叉验证（对应 glmnet 的 ``cv.glmnet(..., family="mgaussian")``）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 先在完整数据上生成统一的 lambda 网格，再对每一折用**该折自己的训练集统计量**
    ''' 重新做标准化并拟合同一网格的路径，避免把留出集信息泄漏进预处理；
    ''' 留出集上的误差按全部响应合并计算：``mse`` 为平方误差均值，
    ''' ``mae`` 为绝对误差均值。
    ''' </para>
    ''' <para>
    ''' 逐 lambda 汇总各折误差的均值 ``cvMean`` 与标准误 ``cvSe``；
    ''' ``lambda.min`` 为 ``cvMean`` 最小处，``lambda.1se`` 为满足
    ''' ``cvMean &lt;= cvMean(min) + cvSe(min)`` 的最大 lambda（最稀疏模型）。
    ''' </para>
    ''' </remarks>
    Public Module MGaussianCV

        ''' <summary>默认折数</summary>
        Public Const DefaultFolds As Integer = 10
        ''' <summary>默认的分折随机种子（保证结果可复现）</summary>
        Public Const DefaultSeed As Integer = 1

        ''' <summary>
        ''' 运行 K 折交叉验证。
        ''' </summary>
        ''' <param name="x">特征矩阵 ``x(sample)(feature)``</param>
        ''' <param name="y">响应矩阵 ``y(sample)(response)``</param>
        ''' <param name="nfolds">折数，必须在 ``[2, n]`` 之内</param>
        ''' <param name="measure">折外误差指标：mse 或 mae</param>
        ''' <param name="seed">确定性分折的随机种子</param>
        ''' <param name="lambda">自定义 lambda 网格；缺省时由完整数据自动生成</param>
        ''' <param name="alpha">弹性网混合参数</param>
        ''' <param name="numberOfLambdas">自动网格的步数</param>
        ''' <param name="standardize">是否标准化特征列</param>
        ''' <param name="standardizeResponse">是否标准化响应列</param>
        ''' <param name="intercept">是否拟合独立截距</param>
        ''' <param name="weights">可选的逐样本权重</param>
        ''' <param name="maxIterations">每个 lambda 步骤的坐标下降最大轮数</param>
        ''' <param name="tolerance">收敛容差</param>
        ''' <param name="featureNames">特征列名</param>
        ''' <param name="responseNames">响应列名</param>
        ''' <param name="Silent">保留参数，交叉验证不产生控制台输出</param>
        ''' <returns>含完整数据路径、CV 曲线与 lambda.min / lambda.1se 的结果对象</returns>
        Public Function CrossValidate(x As Double()(),
                                      y As Double()(),
                                      Optional nfolds As Integer = DefaultFolds,
                                      Optional measure As CVMeasure = CVMeasure.Mse,
                                      Optional seed As Integer = DefaultSeed,
                                      Optional lambda As Double() = Nothing,
                                      Optional alpha As Double = 1.0,
                                      Optional numberOfLambdas As Integer = MGaussianFitting.DefaultNumberOfLambdas,
                                      Optional standardize As Boolean = True,
                                      Optional standardizeResponse As Boolean = False,
                                      Optional intercept As Boolean = True,
                                      Optional weights As Double() = Nothing,
                                      Optional maxIterations As Integer = MGaussianFitting.DefaultMaxIterations,
                                      Optional tolerance As Double = MGaussianFitting.DefaultTolerance,
                                      Optional featureNames As String() = Nothing,
                                      Optional responseNames As String() = Nothing,
                                      Optional Silent As Boolean = False) As MGaussianCVFit

            Dim n As Integer = 0
            Dim p As Integer = 0
            Dim k As Integer = 0

            Call MGaussianFitting.ValidateData(x, y, alpha, numberOfLambdas,
                                               maxIterations, tolerance, lambda,
                                               weights, featureNames, responseNames,
                                               NameOf(CrossValidate), n, p, k)

            If nfolds < 2 OrElse nfolds > n Then
                Throw New ArgumentException($"nfolds must be in [2, {n}], but {nfolds} was given!", NameOf(nfolds))
            End If
            If Not System.Enum.IsDefined(GetType(CVMeasure), measure) Then
                Throw New ArgumentException($"unsupported cross validation measure: {measure}!", NameOf(measure))
            End If

            ' 1) 完整数据上的拟合：既提供统一 lambda 网格，也是最终可用的模型
            Dim fullModel As MGaussianFit = MGaussianFitting.Fit(
                x:=x, y:=y, lambda:=lambda, alpha:=alpha,
                numberOfLambdas:=numberOfLambdas,
                standardize:=standardize, standardizeResponse:=standardizeResponse,
                intercept:=intercept, weights:=weights,
                maxIterations:=maxIterations, tolerance:=tolerance,
                featureNames:=featureNames, responseNames:=responseNames,
                Silent:=Silent)

            Dim grid As Double() = fullModel.lambdas
            Dim steps As Integer = grid.Length
            Dim foldId As Integer() = MakeFolds(n, nfolds, seed)
            Dim foldErrors As Double()() = RectangularArray.Matrix(Of Double)(nfolds, steps)

            ' 2) 逐折拟合，并计算每个 lambda 的折外误差
            For f As Integer = 0 To nfolds - 1
                Dim trainIndex As New List(Of Integer)()
                Dim testIndex As New List(Of Integer)()

                For i As Integer = 0 To n - 1
                    If foldId(i) = f Then
                        Call testIndex.Add(i)
                    Else
                        Call trainIndex.Add(i)
                    End If
                Next

                If trainIndex.Count = 0 OrElse testIndex.Count = 0 Then
                    Continue For
                End If

                Dim xTrain As Double()() = New Double(trainIndex.Count - 1)() {}
                Dim yTrain As Double()() = New Double(trainIndex.Count - 1)() {}
                Dim wTrain As Double() = If(weights Is Nothing, Nothing, New Double(trainIndex.Count - 1) {})

                For t As Integer = 0 To trainIndex.Count - 1
                    Dim rowIndex As Integer = trainIndex(t)

                    xTrain(t) = x(rowIndex)
                    yTrain(t) = y(rowIndex)

                    If wTrain IsNot Nothing Then
                        wTrain(t) = weights(rowIndex)
                    End If
                Next

                Dim xTest As Double()() = New Double(testIndex.Count - 1)() {}
                Dim yTest As Double()() = New Double(testIndex.Count - 1)() {}

                For t As Integer = 0 To testIndex.Count - 1
                    Dim rowIndex As Integer = testIndex(t)

                    xTest(t) = x(rowIndex)
                    yTest(t) = y(rowIndex)
                Next

                Dim foldModel As MGaussianFit = MGaussianFitting.Fit(
                    x:=xTrain, y:=yTrain, lambda:=grid, alpha:=alpha,
                    numberOfLambdas:=steps,
                    standardize:=standardize, standardizeResponse:=standardizeResponse,
                    intercept:=intercept, weights:=wTrain,
                    maxIterations:=maxIterations, tolerance:=tolerance,
                    featureNames:=featureNames, responseNames:=responseNames,
                    Silent:=True)

                ' 一次算出全部 lambda 的预测，避免在折内重复遍历系数矩阵
                Dim predictions As Double()()() = foldModel.PredictPath(xTest, Nothing)

                For li As Integer = 0 To steps - 1
                    Dim errSum As Double = 0.0
                    Dim count As Integer = 0

                    For t As Integer = 0 To testIndex.Count - 1
                        For c As Integer = 0 To k - 1
                            Dim d As Double = yTest(t)(c) - predictions(li)(t)(c)

                            If measure = CVMeasure.Mae Then
                                errSum += std.Abs(d)
                            Else
                                errSum += d * d
                            End If

                            count += 1
                        Next
                    Next

                    foldErrors(f)(li) = If(count > 0, errSum / CDbl(count), 0.0)
                Next
            Next

            ' 3) 汇总均值与标准误
            Dim cvMean As Double() = New Double(steps - 1) {}
            Dim cvSe As Double() = New Double(steps - 1) {}

            For li As Integer = 0 To steps - 1
                Dim mean As Double = 0.0

                For f As Integer = 0 To nfolds - 1
                    mean += foldErrors(f)(li)
                Next

                mean /= CDbl(nfolds)

                Dim variance As Double = 0.0

                For f As Integer = 0 To nfolds - 1
                    Dim d As Double = foldErrors(f)(li) - mean
                    variance += d * d
                Next

                cvMean(li) = mean
                cvSe(li) = If(nfolds > 1, std.Sqrt(variance / (nfolds - 1) / nfolds), 0.0)
            Next

            ' 4) glmnet 语义挑选 lambda.min 与 lambda.1se
            Dim bestIndex As Integer = 0

            For li As Integer = 1 To steps - 1
                If cvMean(li) < cvMean(bestIndex) Then
                    bestIndex = li
                End If
            Next

            Dim threshold As Double = cvMean(bestIndex) + cvSe(bestIndex)
            Dim bestIndex1se As Integer = bestIndex

            ' lambda 严格递减，下标 0 的惩罚最强：取第一个满足阈值的下标即「最大的 lambda」
            For li As Integer = 0 To steps - 1
                If cvMean(li) <= threshold Then
                    bestIndex1se = li
                    Exit For
                End If
            Next

            Return New MGaussianCVFit With {
                .Model = fullModel,
                .nfolds = nfolds,
                .measure = measure,
                .cvMean = cvMean,
                .cvSe = cvSe,
                .foldErrors = foldErrors,
                .foldId = foldId,
                .BestIndex = bestIndex,
                .BestIndex1se = bestIndex1se
            }
        End Function

        ''' <summary>
        ''' 确定性分折：用固定种子打乱样本下标后交错分配到各折，
        ''' 既保证可复现，也避免原始数据排序造成系统性偏差。
        ''' </summary>
        Private Function MakeFolds(n As Integer, nfolds As Integer, seed As Integer) As Integer()
            Dim order As Integer() = New Integer(n - 1) {}
            Dim foldId As Integer() = New Integer(n - 1) {}
            Dim rnd As New Random(seed)

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            For i As Integer = n - 1 To 1 Step -1
                Dim j As Integer = rnd.Next(i + 1)
                Dim tmp As Integer = order(i)

                order(i) = order(j)
                order(j) = tmp
            Next

            For i As Integer = 0 To n - 1
                foldId(order(i)) = i Mod nfolds
            Next

            Return foldId
        End Function
    End Module
End Namespace
