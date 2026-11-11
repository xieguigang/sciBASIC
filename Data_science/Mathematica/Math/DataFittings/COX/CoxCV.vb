Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

Namespace Cox

    ''' <summary>Cox 交叉验证可用的折外指标（readme：只支持 deviance 与 C 指数）</summary>
    Public Enum CoxCVMeasure
        ''' <summary>部分似然偏差，越小越好</summary>
        Deviance
        ''' <summary>Harrell 一致性指数，越大越好</summary>
        CIndex
    End Enum

    ''' <summary>
    ''' Cox 模型的 K 折交叉验证（对应 ``cv.glmnet(..., family="cox")``）。
    ''' </summary>
    ''' <remarks>
    ''' 先在完整数据上生成统一 lambda 网格，逐折用训练子集拟合同一网格（每折只用自己的
    ''' 训练集统计量做标准化）。CIndex 记录留出集上的 Harrell C 指数（越大越好）；
    ''' Deviance 记录部分似然偏差（越小越好）。grouped=True（默认）用 readme 的
    ''' 「减法」方式：同一组折模型系数下，全数据对数偏似然减去训练子集对数偏似然，
    ''' 再按留出集样本数归一化；grouped=False 则只在留出集内部计算偏似然。
    ''' </remarks>
    Public Module CoxCV

        ''' <summary>默认折数</summary>
        Public Const DefaultFolds As Integer = 10
        ''' <summary>默认分折种子</summary>
        Public Const DefaultSeed As Integer = 1

        ''' <summary>运行 K 折交叉验证</summary>
        Public Function CrossValidate(x As Double()(),
                                      y As Double()(),
                                      Optional nfolds As Integer = DefaultFolds,
                                      Optional measure As CoxCVMeasure = CoxCVMeasure.Deviance,
                                      Optional grouped As Boolean = True,
                                      Optional seed As Integer = DefaultSeed,
                                      Optional lambda As Double() = Nothing,
                                      Optional alpha As Double = 1.0,
                                      Optional numberOfLambdas As Integer = CoxFitting.DefaultNumberOfLambdas,
                                      Optional standardize As Boolean = True,
                                      Optional ties As CoxTies = CoxTies.Efron,
                                      Optional penaltyFactor As Double() = Nothing,
                                      Optional maxIterations As Integer = CoxFitting.DefaultMaxIterations,
                                      Optional tolerance As Double = CoxFitting.DefaultTolerance,
                                      Optional featureNames As String() = Nothing,
                                      Optional Silent As Boolean = False) As CoxCVFit

            Dim n As Integer = 0
            Dim p As Integer = 0

            Call CoxFitting.ValidateData(x, y, alpha, numberOfLambdas, maxIterations, tolerance,
                                         lambda, penaltyFactor, featureNames, ties,
                                         NameOf(CrossValidate), n, p)

            If nfolds < 2 OrElse nfolds > n Then
                Throw New ArgumentException($"nfolds must be in [2, {n}], but {nfolds} was given!", NameOf(nfolds))
            End If
            If Not System.Enum.IsDefined(GetType(CoxCVMeasure), measure) Then
                Throw New ArgumentException($"unsupported cv measure: {measure}!", NameOf(measure))
            End If

            Dim fullModel As CoxFit = CoxFitting.Fit(
                x:=x, y:=y, lambda:=lambda, alpha:=alpha,
                numberOfLambdas:=numberOfLambdas, standardize:=standardize,
                ties:=ties, penaltyFactor:=penaltyFactor,
                maxIterations:=maxIterations, tolerance:=tolerance,
                featureNames:=featureNames, Silent:=Silent)

            Dim grid As Double() = fullModel.lambdas
            Dim steps As Integer = grid.Length
            Dim foldId As Integer() = MakeFolds(n, nfolds, seed)
            Dim foldErrors As Double()() = RectangularArray.Matrix(Of Double)(nfolds, steps)

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
                Dim xTest As Double()() = New Double(testIndex.Count - 1)() {}
                Dim yTest As Double()() = New Double(testIndex.Count - 1)() {}

                For t As Integer = 0 To trainIndex.Count - 1
                    xTrain(t) = x(trainIndex(t))
                    yTrain(t) = y(trainIndex(t))
                Next
                For t As Integer = 0 To testIndex.Count - 1
                    xTest(t) = x(testIndex(t))
                    yTest(t) = y(testIndex(t))
                Next

                Dim foldModel As CoxFit = CoxFitting.Fit(
                    x:=xTrain, y:=yTrain, lambda:=grid, alpha:=alpha,
                    numberOfLambdas:=steps, standardize:=standardize,
                    ties:=ties, penaltyFactor:=penaltyFactor,
                    maxIterations:=maxIterations, tolerance:=tolerance,
                    featureNames:=featureNames, Silent:=True)

                Dim testTime As Double() = New Double(testIndex.Count - 1) {}
                Dim testStatus As Double() = New Double(testIndex.Count - 1) {}

                For t As Integer = 0 To testIndex.Count - 1
                    testTime(t) = yTest(t)(0)
                    testStatus(t) = yTest(t)(1)
                Next

                For li As Integer = 0 To steps - 1
                    Dim coef As Double() = foldModel.getCoefficients(li)
                    Dim score As Double

                    If measure = CoxCVMeasure.CIndex Then
                        Dim riskScores As Double() = foldModel.Predict(xTest, li)
                        score = CoxFitting.ConcordanceIndex(testTime, testStatus, riskScores)
                    ElseIf grouped Then
                        Dim plAll As Double = CoxFitting.EvaluateLogPL(x, y, coef, ties)
                        Dim plTrain As Double = CoxFitting.EvaluateLogPL(xTrain, yTrain, coef, ties)
                        score = -2.0 * (plAll - plTrain) / CDbl(testIndex.Count)
                    Else
                        Dim plTest As Double = CoxFitting.EvaluateLogPL(xTest, yTest, coef, ties)
                        score = -2.0 * plTest / CDbl(testIndex.Count)
                    End If

                    foldErrors(f)(li) = score
                Next
            Next

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

            Dim maximize As Boolean = (measure = CoxCVMeasure.CIndex)
            Dim bestIndex As Integer = 0

            For li As Integer = 1 To steps - 1
                Dim better As Boolean = If(maximize,
                                           cvMean(li) > cvMean(bestIndex),
                                           cvMean(li) < cvMean(bestIndex))
                If better Then
                    bestIndex = li
                End If
            Next

            Dim bound As Double = If(maximize,
                                     cvMean(bestIndex) - cvSe(bestIndex),
                                     cvMean(bestIndex) + cvSe(bestIndex))
            Dim bestIndex1se As Integer = bestIndex

            For li As Integer = 0 To steps - 1
                Dim within As Boolean = If(maximize,
                                           cvMean(li) >= bound,
                                           cvMean(li) <= bound)
                If within Then
                    bestIndex1se = li
                    Exit For
                End If
            Next

            Return New CoxCVFit With {
                .Model = fullModel,
                .nfolds = nfolds,
                .measure = measure,
                .grouped = grouped,
                .cvMean = cvMean,
                .cvSe = cvSe,
                .foldErrors = foldErrors,
                .foldId = foldId,
                .BestIndex = bestIndex,
                .BestIndex1se = bestIndex1se
            }
        End Function

        ''' <summary>固定种子打乱后交错分折，保证可复现</summary>
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
