#Region "Microsoft.VisualBasic::7a9342e4bb330ddce0bf445d2ed63e51, Data_science\MachineLearning\VAE\GMM\GMM.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 832
    '    Code Lines: 559 (67.19%)
    ' Comment Lines: 158 (18.99%)
    '    - Xml Docs: 54.43%
    ' 
    '   Blank Lines: 115 (13.82%)
    '     File Size: 27.51 KB


    ' Class GaussianMixtureModel
    ' 
    '     Properties: Converged, Covariances, LogLikelihood, LogLikelihoodHistory, Means
    '                 NIter, Weights
    '     Enum InitMethod
    ' 
    '         KMeansPP, Random
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ComputeLogGaussianPDF, EStep, GetComponentMean, GetComponentStdDev, GetComponentVariance
    '               LogDeterminant, MatrixInverse, Predict, PredictProba, (+2 Overloads) Predicts
    '               Sample, Score, ScoreSamples, ToString
    ' 
    '     Sub: Fit, InitializeParameters, MStep
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' GMM.vb - 高斯混合模型 (Gaussian Mixture Model)
'
' 基于 EM (Expectation-Maximization) 算法实现的多变量高斯混合模型
' 使用用户提供的 Tensor 类作为底层数值计算结构
'
' 算法参考:
'   - Bishop, "Pattern Recognition and Machine Learning", Chapter 9
'   - EM 算法: E步计算后验责任度, M步更新参数
'
' 作者: Qingyan Agent
' ============================================================================

Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 高斯混合模型 (Gaussian Mixture Model)
''' 使用 EM 算法对数据进行聚类 / 密度估计 / 信号分解
''' </summary>
''' <remarks>
''' 模型形式: p(x) = Σ_k π_k · N(x | μ_k, Σ_k)
''' 其中 π_k 为混合权重, μ_k 为均值向量, Σ_k 为协方差矩阵
''' </remarks>
Public Class GaussianMixtureModel

#Region "私有字段"

    ' 混合成分数量
    Private _nComponents As Integer

    ' 最大迭代次数
    Private _maxIter As Integer

    ' 收敛阈值 (对数似然变化)
    Private _tol As Double

    ' 协方差正则化项 (防止奇异协方差矩阵)
    Private _regCovar As Double

    ' 随机数种子
    Private _seed As Integer?

    ' 是否使用对角协方差矩阵 (高维数据时推荐)
    Private _diagCovariance As Boolean

    ' 初始化方式
    Private _initMethod As InitMethod

#End Region

#Region "模型参数 (训练后可获得)"

    ''' <summary>混合权重 π_k, 形状 [K]</summary>
    Public Property Weights As Tensor

    ''' <summary>均值向量 μ_k, 形状 [K, D]</summary>
    Public Property Means As Tensor

    ''' <summary>协方差矩阵 Σ_k, 形状 [K, D, D] (全协方差) 或 [K, D] (对角协方差)</summary>
    Public Property Covariances As Tensor

    ''' <summary>是否已收敛</summary>
    Public Property Converged As Boolean = False

    ''' <summary>实际迭代次数</summary>
    Public Property NIter As Integer = 0

    ''' <summary>最终对数似然</summary>
    Public Property LogLikelihood As Double = Double.NegativeInfinity

    ''' <summary>训练历史对数似然值</summary>
    Public Property LogLikelihoodHistory As New List(Of Double)

#End Region

#Region "枚举与构造"

    ''' <summary>参数初始化方式</summary>
    Public Enum InitMethod
        ''' <summary>随机初始化 (从数据中随机采样)</summary>
        Random
        ''' <summary>K-Means++ 风格初始化</summary>
        KMeansPP
    End Enum

    ''' <summary>
    ''' 创建高斯混合模型
    ''' </summary>
    ''' <param name="nComponents">混合成分数量 K</param>
    ''' <param name="maxIter">最大 EM 迭代次数</param>
    ''' <param name="tol">对数似然收敛阈值</param>
    ''' <param name="regCovar">协方差正则化项, 防止矩阵奇异</param>
    ''' <param name="diagCovariance">是否使用对角协方差 (高维数据推荐)</param>
    ''' <param name="initMethod">参数初始化方式</param>
    ''' <param name="seed">随机数种子</param>
    Public Sub New(Optional nComponents As Integer = 3,
                   Optional maxIter As Integer = 200,
                   Optional tol As Double = 0.000001,
                   Optional regCovar As Double = 0.000001,
                   Optional diagCovariance As Boolean = False,
                   Optional initMethod As InitMethod = InitMethod.KMeansPP,
                   Optional seed As Integer? = Nothing)
        If nComponents < 1 Then
            Throw New ArgumentException("混合成分数量必须大于 0")
        End If
        _nComponents = nComponents
        _maxIter = maxIter
        _tol = tol
        _regCovar = regCovar
        _diagCovariance = diagCovariance
        _initMethod = initMethod
        _seed = seed
    End Sub

#End Region

#Region "训练 (EM 算法)"

    ''' <summary>
    ''' 使用 EM 算法拟合模型
    ''' </summary>
    ''' <param name="X">输入数据, 形状 [N, D], N 为样本数, D 为特征维度</param>
    Public Sub Fit(X As Tensor)
        If X.Rank <> 2 Then
            Throw New ArgumentException("输入数据必须是二维张量 [N, D]")
        End If

        Dim N = X.Shape(0)
        Dim D = X.Shape(1)
        Dim K = _nComponents
        Dim rng = If(_seed.HasValue, New Random(_seed.Value), New Random())

        ' ---------- 1. 初始化参数 ----------
        InitializeParameters(X, K, D, rng)

        ' ---------- 2. EM 迭代 ----------
        Dim prevLogLik As Double = Double.NegativeInfinity

        For iter = 1 To _maxIter
            ' E 步: 计算责任度 (log 域稳定计算)
            Dim logResp As Tensor = Nothing
            Dim logLik As Double = EStep(X, logResp)

            ' 记录对数似然历史
            LogLikelihoodHistory.Add(logLik)

            ' 检查收敛
            If iter > 1 AndAlso std.Abs(logLik - prevLogLik) < _tol Then
                Converged = True
                NIter = iter
                LogLikelihood = logLik
                Exit For
            End If

            prevLogLik = logLik

            ' M 步: 更新参数
            MStep(X, logResp)

            NIter = iter
            LogLikelihood = logLik
        Next

        If Not Converged Then
            ' 未收敛也保留最后一次结果
        End If
    End Sub

    ''' <summary>
    ''' 初始化模型参数
    ''' </summary>
    Private Sub InitializeParameters(X As Tensor, K As Integer, D As Integer, rng As Random)
        Dim N = X.Shape(0)

        ' 初始化权重为均匀分布
        Dim wData = New Double(K - 1) {}
        For K = 0 To K - 1
            wData(K) = 1.0 / K
        Next
        Weights = New Tensor(wData, K)

        ' 初始化均值
        Dim meansData = New Double(K * D - 1) {}
        Select Case _initMethod
            Case InitMethod.Random
                ' 从数据中随机采样 K 个点作为初始均值
                For K = 0 To K - 1
                    Dim idx = rng.Next(N)
                    For D = 0 To D - 1
                        meansData(K * D + D) = X(idx, D)
                    Next
                Next
            Case InitMethod.KMeansPP
                ' K-Means++ 风格初始化
                Dim firstIdx = rng.Next(N)
                For D = 0 To D - 1
                    meansData(D) = X(firstIdx, D)
                Next

                Dim chosenIndices = New List(Of Integer) From {firstIdx}

                For K = 1 To K - 1
                    ' 计算每个点到最近已选中心的距离平方
                    Dim dists = New Double(N - 1) {}
                    Dim totalDist As Double = 0
                    For i = 0 To N - 1
                        Dim minDist As Double = Double.MaxValue
                        For Each ci In chosenIndices
                            Dim dist As Double = 0
                            For D = 0 To D - 1
                                Dim diff = X(i, D) - X(ci, D)
                                dist += diff * diff
                            Next
                            If dist < minDist Then minDist = dist
                        Next
                        dists(i) = minDist
                        totalDist += minDist
                    Next

                    ' 按距离平方加权随机选择下一个中心
                    Dim r = rng.NextDouble() * totalDist
                    Dim cumDist As Double = 0
                    Dim nextIdx = N - 1
                    For i = 0 To N - 1
                        cumDist += dists(i)
                        If cumDist >= r Then
                            nextIdx = i
                            Exit For
                        End If
                    Next

                    chosenIndices.Add(nextIdx)
                    For D = 0 To D - 1
                        meansData(K * D + D) = X(nextIdx, D)
                    Next
                Next
        End Select
        Means = New Tensor(meansData, K, D)

        ' 初始化协方差矩阵 (使用全局数据协方差)
        If _diagCovariance Then
            ' 对角协方差: [K, D]
            Dim covData = New Double(K * D - 1) {}
            ' 计算全局方差
            Dim globalVar = New Double(D - 1) {}
            For i = 0 To N - 1
                For D = 0 To D - 1
                    globalVar(D) += X(i, D) * X(i, D)
                Next
            Next
            For D = 0 To D - 1
                globalVar(D) = globalVar(D) / N + _regCovar
            Next
            For K = 0 To K - 1
                For D = 0 To D - 1
                    covData(K * D + D) = globalVar(D)
                Next
            Next
            Covariances = New Tensor(covData, K, D)
        Else
            ' 全协方差: [K, D, D]
            Dim covData = New Double(K * D * D - 1) {}
            ' 计算全局协方差矩阵
            Dim globalCov = New Double(D * D - 1) {}
            For i = 0 To N - 1
                For r = 0 To D - 1
                    For c = 0 To D - 1
                        globalCov(r * D + c) += X(i, r) * X(i, c)
                    Next
                Next
            Next
            For r = 0 To D - 1
                For c = 0 To D - 1
                    globalCov(r * D + c) = globalCov(r * D + c) / N
                    If r = c Then globalCov(r * D + c) += _regCovar
                Next
            Next
            For K = 0 To K - 1
                For j = 0 To D * D - 1
                    covData(K * D * D + j) = globalCov(j)
                Next
            Next
            Covariances = New Tensor(covData, K, D, D)
        End If
    End Sub

    ''' <summary>
    ''' E 步: 计算对数责任度 (log-responsibilities) 和对数似然
    ''' 使用 log-sum-exp 技巧保证数值稳定性
    ''' </summary>
    Private Function EStep(X As Tensor, ByRef logResp As Tensor) As Double
        Dim N = X.Shape(0)
        Dim D = X.Shape(1)
        Dim K = _nComponents

        ' 计算 log(π_k * N(x_n | μ_k, Σ_k)) 对所有 n, k
        Dim logProb = ComputeLogGaussianPDF(X)  ' [N, K]

        ' 加上 log 权重
        Dim weightedLogProb = New Tensor(N, K)
        For N = 0 To N - 1
            For K = 0 To K - 1
                weightedLogProb(N, K) = logProb(N, K) + std.Log(Weights(K))
            Next
        Next

        ' 计算 log-sum-exp (沿 K 维度)
        Dim logLik As Double = 0
        logResp = New Tensor(N, K)

        For N = 0 To N - 1
            ' 找最大值
            Dim maxVal As Double = Double.NegativeInfinity
            For K = 0 To K - 1
                If weightedLogProb(N, K) > maxVal Then
                    maxVal = weightedLogProb(N, K)
                End If
            Next

            ' 计算 sum exp
            Dim sumExp As Double = 0
            For K = 0 To K - 1
                sumExp += std.Exp(weightedLogProb(N, K) - maxVal)
            Next

            Dim logSumExp = maxVal + std.Log(sumExp)
            logLik += logSumExp

            ' 计算责任度 (log 域)
            For K = 0 To K - 1
                logResp(N, K) = weightedLogProb(N, K) - logSumExp
            Next
        Next

        Return logLik
    End Function

    ''' <summary>
    ''' M 步: 根据责任度更新参数
    ''' </summary>
    Private Sub MStep(X As Tensor, logResp As Tensor)
        Dim N = X.Shape(0)
        Dim D = X.Shape(1)
        Dim K = _nComponents

        ' 将 log 责任度转换为责任度
        Dim resp = New Tensor(N, K)
        For N = 0 To N - 1
            For K = 0 To K - 1
                resp(N, K) = std.Exp(logResp(N, K))
            Next
        Next

        ' 计算 N_k = Σ_n resp(n, k)
        Dim Nk = New Double(K - 1) {}
        For K = 0 To K - 1
            Dim s As Double = 0
            For N = 0 To N - 1
                s += resp(N, K)
            Next
            Nk(K) = s
        Next

        ' 更新权重: π_k = N_k / N
        Dim wData = New Double(K - 1) {}
        For K = 0 To K - 1
            wData(K) = (Nk(K) + 0.0000001) / (N + K * 0.0000001)
        Next
        Weights = New Tensor(wData, K)

        ' 更新均值: μ_k = (1/N_k) Σ_n resp(n,k) * x_n
        Dim meansData = New Double(K * D - 1) {}
        For K = 0 To K - 1
            For D = 0 To D - 1
                Dim s As Double = 0
                For N = 0 To N - 1
                    s += resp(N, K) * X(N, D)
                Next
                meansData(K * D + D) = s / (Nk(K) + 0.0000001)
            Next
        Next
        Means = New Tensor(meansData, K, D)

        ' 更新协方差
        If _diagCovariance Then
            Dim covData = New Double(K * D - 1) {}
            For K = 0 To K - 1
                For D = 0 To D - 1
                    Dim s As Double = 0
                    For N = 0 To N - 1
                        Dim diff = X(N, D) - Means(K, D)
                        s += resp(N, K) * diff * diff
                    Next
                    covData(K * D + D) = s / (Nk(K) + 0.0000001) + _regCovar
                Next
            Next
            Covariances = New Tensor(covData, K, D)
        Else
            Dim covData = New Double(K * D * D - 1) {}
            For K = 0 To K - 1
                For r = 0 To D - 1
                    For c = 0 To D - 1
                        Dim s As Double = 0
                        For N = 0 To N - 1
                            Dim diffR = X(N, r) - Means(K, r)
                            Dim diffC = X(N, c) - Means(K, c)
                            s += resp(N, K) * diffR * diffC
                        Next
                        covData(K * D * D + r * D + c) = s / (Nk(K) + 0.0000001)
                        If r = c Then
                            covData(K * D * D + r * D + c) += _regCovar
                        End If
                    Next
                Next
            Next
            Covariances = New Tensor(covData, K, D, D)
        End If
    End Sub

#End Region

#Region "概率密度计算"

    ''' <summary>
    ''' 计算所有样本在所有成分下的对数高斯概率密度
    ''' </summary>
    ''' <param name="X">输入数据 [N, D]</param>
    ''' <returns>对数概率密度 [N, K]</returns>
    Public Function ComputeLogGaussianPDF(X As Tensor) As Tensor
        If Means Is Nothing Then
            Throw New InvalidOperationException("模型尚未训练, 请先调用 Fit 方法")
        End If

        Dim N = X.Shape(0)
        Dim D = X.Shape(1)
        Dim K = _nComponents
        Dim result = New Tensor(N, K)

        If _diagCovariance Then
            ' 对角协方差情况
            For K = 0 To K - 1
                ' 计算 log|Σ_k| = Σ_d log(σ²_kd)
                Dim logDetCov As Double = 0
                For D = 0 To D - 1
                    logDetCov += std.Log(Covariances(K, D))
                Next

                ' 常数项: -0.5 * (D * log(2π) + log|Σ_k|)
                Dim constTerm = -0.5 * (D * std.Log(2.0 * std.PI) + logDetCov)

                For N = 0 To N - 1
                    ' 计算 (x - μ)^T Σ^(-1) (x - μ) = Σ_d (x_d - μ_d)² / σ²_d
                    Dim mahalDist As Double = 0
                    For D = 0 To D - 1
                        Dim diff = X(N, D) - Means(K, D)
                        mahalDist += diff * diff / Covariances(K, D)
                    Next
                    result(N, K) = constTerm - 0.5 * mahalDist
                Next
            Next
        Else
            ' 全协方差情况
            For K = 0 To K - 1
                ' 提取第 k 个协方差矩阵 [D, D]
                Dim cov = New Tensor(D, D)
                For r = 0 To D - 1
                    For c = 0 To D - 1
                        cov(r, c) = Covariances(K, r, c)
                    Next
                Next

                ' 计算协方差矩阵的逆和 log 行列式
                Dim invCov = MatrixInverse(cov, D)
                Dim logDetCov = LogDeterminant(cov, D)

                Dim constTerm = -0.5 * (D * std.Log(2.0 * std.PI) + logDetCov)

                For N = 0 To N - 1
                    ' 计算 (x - μ)^T Σ^(-1) (x - μ)
                    Dim mahalDist As Double = 0
                    For r = 0 To D - 1
                        Dim sumRow As Double = 0
                        For c = 0 To D - 1
                            sumRow += invCov(r, c) * (X(N, c) - Means(K, c))
                        Next
                        mahalDist += (X(N, r) - Means(K, r)) * sumRow
                    Next
                    result(N, K) = constTerm - 0.5 * mahalDist
                Next
            Next
        End If

        Return result
    End Function

#End Region

#Region "预测与采样"

    ''' <summary>
    ''' 预测每个样本属于每个成分的责任度 (后验概率)
    ''' </summary>
    ''' <param name="X">输入数据 [N, D]</param>
    ''' <returns>责任度矩阵 [N, K]</returns>
    Public Function PredictProba(X As Tensor) As Tensor
        Dim logResp As Tensor = Nothing
        Call EStep(X, logResp)

        Dim N = X.Shape(0)
        Dim K = _nComponents
        Dim resp = New Tensor(N, K)
        For N = 0 To N - 1
            For K = 0 To K - 1
                resp(N, K) = std.Exp(logResp(N, K))
            Next
        Next
        Return resp
    End Function

    ''' <summary>
    ''' 预测每个样本最可能属于的成分 (硬聚类)
    ''' </summary>
    ''' <param name="X">输入数据 [N, D]</param>
    ''' <returns>标签数组 [N]</returns>
    Public Function Predict(X As Tensor) As Integer()
        Dim resp = PredictProba(X)
        Dim N = X.Shape(0)
        Dim K = _nComponents
        Dim labels = New Integer(N - 1) {}

        For N = 0 To N - 1
            Dim maxVal As Double = Double.NegativeInfinity
            Dim maxIdx = 0
            For K = 0 To K - 1
                If resp(N, K) > maxVal Then
                    maxVal = resp(N, K)
                    maxIdx = K
                End If
            Next
            labels(N) = maxIdx
        Next
        Return labels
    End Function

    ''' <summary>
    ''' 计算给定数据的平均对数似然
    ''' </summary>
    Public Function Score(X As Tensor) As Double
        Dim logResp As Tensor = Nothing
        Dim logLik = EStep(X, logResp)
        Return logLik / X.Shape(0)
    End Function

    ''' <summary>
    ''' 计算给定数据点处的概率密度 p(x) = Σ_k π_k N(x | μ_k, Σ_k)
    ''' </summary>
    ''' <param name="X">输入数据 [N, D]</param>
    ''' <returns>概率密度 [N]</returns>
    Public Function ScoreSamples(X As Tensor) As Tensor
        Dim logProb = ComputeLogGaussianPDF(X)
        Dim N = X.Shape(0)
        Dim K = _nComponents
        Dim result = New Tensor(N)

        For N = 0 To N - 1
            Dim maxVal As Double = Double.NegativeInfinity
            For K = 0 To K - 1
                Dim v = logProb(N, K) + std.Log(Weights(K))
                If v > maxVal Then maxVal = v
            Next
            Dim sumExp As Double = 0
            For K = 0 To K - 1
                sumExp += std.Exp(logProb(N, K) + std.Log(Weights(K)) - maxVal)
            Next
            result(N) = maxVal + std.Log(sumExp)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 从拟合好的模型中采样生成数据
    ''' </summary>
    ''' <param name="n">采样数量</param>
    ''' <param name="seed">随机种子</param>
    ''' <returns>采样数据 [n, D]</returns>
    Public Function Sample(n As Integer, Optional seed As Integer? = Nothing) As Tensor
        If Means Is Nothing Then
            Throw New InvalidOperationException("模型尚未训练, 请先调用 Fit 方法")
        End If

        Dim D = Means.Shape(1)
        Dim K = _nComponents
        Dim rng = If(seed.HasValue, New Random(seed.Value), New Random())
        Dim result = New Tensor(n, D)

        ' 累积权重用于选择成分
        Dim cumWeights = New Double(K - 1) {}
        cumWeights(0) = Weights(0)
        For K = 1 To K - 1
            cumWeights(K) = cumWeights(K - 1) + Weights(K)
        Next

        For i = 0 To n - 1
            ' 选择成分
            Dim r = rng.NextDouble()
            Dim selK = K - 1
            For K = 0 To K - 1
                If r <= cumWeights(K) Then
                    selK = K
                    Exit For
                End If
            Next

            ' 从该成分的高斯分布中采样 (Box-Muller)
            For D = 0 To D - 1
                Dim u1 = 1.0 - rng.NextDouble()
                Dim u2 = 1.0 - rng.NextDouble()
                Dim z = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)

                If _diagCovariance Then
                    Dim std_ = std.Sqrt(Covariances(selK, D))
                    result(i, D) = Means(selK, D) + std_ * z
                Else
                    ' 简化: 仅使用对角元素 (Cholesky 分解略)
                    Dim var = Covariances(selK, D, D)
                    Dim std_ = std.Sqrt(var)
                    result(i, D) = Means(selK, D) + std_ * z
                End If
            Next
        Next

        Return result
    End Function

#End Region

#Region "线性代数辅助函数"

    ''' <summary>
    ''' 矩阵求逆 (Gauss-Jordan 消元法)
    ''' </summary>
    Public Shared Function MatrixInverse(mat As Tensor, n As Integer) As Tensor
        ' 构造增广矩阵 [A | I]
        Dim aug = New Double(n - 1, 2 * n - 1) {}
        For i = 0 To n - 1
            For j = 0 To n - 1
                aug(i, j) = mat(i, j)
            Next
            aug(i, n + i) = 1.0
        Next

        ' 前向消元 + 部分主元选取
        For col = 0 To n - 1
            ' 找列主元
            Dim maxRow = col
            Dim maxVal = std.Abs(aug(col, col))
            For row = col + 1 To n - 1
                If std.Abs(aug(row, col)) > maxVal Then
                    maxVal = std.Abs(aug(row, col))
                    maxRow = row
                End If
            Next

            ' 交换行
            If maxRow <> col Then
                For j = 0 To 2 * n - 1
                    Dim tmp = aug(col, j)
                    aug(col, j) = aug(maxRow, j)
                    aug(maxRow, j) = tmp
                Next
            End If

            ' 主元不能为 0
            If std.Abs(aug(col, col)) < 0.0000000001 Then
                Throw New InvalidOperationException("矩阵不可逆 (奇异矩阵)")
            End If

            ' 归一化主元行
            Dim pivot = aug(col, col)
            For j = 0 To 2 * n - 1
                aug(col, j) /= pivot
            Next

            ' 消去其他行
            For row = 0 To n - 1
                If row = col Then Continue For
                Dim factor = aug(row, col)
                For j = 0 To 2 * n - 1
                    aug(row, j) -= factor * aug(col, j)
                Next
            Next
        Next

        ' 提取逆矩阵
        Dim result = New Tensor(n, n)
        For i = 0 To n - 1
            For j = 0 To n - 1
                result(i, j) = aug(i, n + j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 计算矩阵的 log 行列式 (通过 LU 分解)
    ''' </summary>
    Public Shared Function LogDeterminant(mat As Tensor, n As Integer) As Double
        ' 复制矩阵
        Dim a = New Double(n - 1, n - 1) {}
        For i = 0 To n - 1
            For j = 0 To n - 1
                a(i, j) = mat(i, j)
            Next
        Next

        Dim logDet As Double = 0

        ' LU 分解 (带部分主元)
        For col = 0 To n - 1
            ' 找主元
            Dim maxRow = col
            Dim maxVal = std.Abs(a(col, col))
            For row = col + 1 To n - 1
                If std.Abs(a(row, col)) > maxVal Then
                    maxVal = std.Abs(a(row, col))
                    maxRow = row
                End If
            Next

            If maxRow <> col Then
                For j = 0 To n - 1
                    Dim tmp = a(col, j)
                    a(col, j) = a(maxRow, j)
                    a(maxRow, j) = tmp
                Next
                logDet = -logDet  ' 行交换改变行列式符号
            End If

            If std.Abs(a(col, col)) < 0.0000000001 Then
                Return Double.NegativeInfinity
            End If

            logDet += std.Log(std.Abs(a(col, col)))

            ' 消去下方
            For row = col + 1 To n - 1
                Dim factor = a(row, col) / a(col, col)
                For j = col To n - 1
                    a(row, j) -= factor * a(col, j)
                Next
            Next
        Next

        Return logDet
    End Function

#End Region

#Region "辅助方法"

    ''' <summary>
    ''' 获取第 k 个成分的均值向量 (1D 张量)
    ''' </summary>
    Public Function GetComponentMean(k As Integer) As Tensor
        Dim D = Means.Shape(1)
        Dim result = New Tensor(D)
        For D = 0 To D - 1
            result(D) = Means(k, D)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 获取第 k 个成分的方差向量 (对角协方差情况)
    ''' </summary>
    Public Function GetComponentVariance(k As Integer) As Tensor
        Dim D = Means.Shape(1)
        Dim result = New Tensor(D)
        If _diagCovariance Then
            For D = 0 To D - 1
                result(D) = Covariances(k, D)
            Next
        Else
            For D = 0 To D - 1
                result(D) = Covariances(k, D, D)
            Next
        End If
        Return result
    End Function

    ''' <summary>
    ''' 获取第 k 个成分的标准差向量
    ''' </summary>
    Public Function GetComponentStdDev(k As Integer) As Tensor
        Dim var = GetComponentVariance(k)
        Dim result = New Tensor(var.Length)
        For i = 0 To var.Length - 1
            result(i) = std.Sqrt(var(i))
        Next
        Return result
    End Function

    Public Overrides Function ToString() As String
        Return $"GMM(K={_nComponents}, diagCov={_diagCovariance}, converged={Converged}, nIter={NIter}, logLik={LogLikelihood:F4})"
    End Function

    Public Shared Function Predicts(rowdatas() As ClusterEntity, components As Integer, threshold As Double, strict As Boolean) As GaussianMixtureModel
        Dim gmm As New GaussianMixtureModel(components, tol:=threshold)
        Dim w As Integer = rowdatas(0).Length
        Dim samples As Double() = New Double(rowdatas.Length * w - 1) {}

        For i As Integer = 0 To rowdatas.Length - 1
            Array.Copy(rowdatas(i).entityVector, 0, samples, i * w, w)
        Next

        Dim X = New Tensor(samples, rowdatas.Length, w)
        gmm.Fit(X)
        Return gmm
    End Function

    Public Shared Function Predicts(samples() As Double, components As Integer, threshold As Double, verbose As Boolean) As GaussianMixtureModel
        Dim gmm As New GaussianMixtureModel(components, tol:=threshold)
        Dim X = New Tensor(samples, samples.Length, 1)
        gmm.Fit(X)
        Return gmm
    End Function

#End Region

End Class

