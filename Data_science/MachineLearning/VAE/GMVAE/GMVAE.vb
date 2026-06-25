' ============================================================================
' GMVAE.vb - 高斯混合变分自编码器 (Gaussian Mixture VAE)
'
' GMVAE 是一种深度生成模型, 结合了变分自编码器 (VAE) 和高斯混合模型 (GMM):
'   - 离散隐变量 y ~ Cat(π)  (类别先验, K 个成分)
'   - 连续隐变量 z ~ N(μ_y, σ_y²)  (类别条件高斯先验)
'   - 解码器 p(x|z) 重构输入
'
' 模型架构:
'   编码器 q(y|x): x → 类别分布 logits
'   编码器 q(z|x): x → (μ_z, log σ²_z)
'   先验 p(z|y): 可学习的 (μ_y, σ_y) 每个类别
'   解码器 p(x|z): z → x_recon
'
' 损失 (ELBO 负数):
'   L = ReconLoss + β·KL(q(y|x) || p(y)) + γ·KL(q(z|x) || p(z|y))
'
' 使用重参数化技巧 (reparameterization trick) 实现端到端反向传播
'
' 作者: Qingyan Agent
' ============================================================================

Imports std = System.Math

''' <summary>
''' 高斯混合变分自编码器 (Gaussian Mixture VAE)
''' </summary>
''' <remarks>
''' 通过引入类别隐变量 y 和连续隐变量 z, 实现对数据的多模态建模
''' 适合于聚类 + 生成任务
''' </remarks>
Public Class GMVAE

#Region "私有字段"

    Private _inputDim As Integer
    Private _latentDim As Integer
    Private _nComponents As Integer
    Private _hiddenDim As Integer
    Private _seed As Integer?

    ' 网络层
    Private _encY_W1, _encY_b1, _encY_W2, _encY_b2 As Tensor   ' q(y|x) 编码器
    Private _encZ_W1, _encZ_b1, _encZ_W2, _encZ_b2 As Tensor   ' q(z|x) 编码器 (输出 μ)
    Private _encZ_W3, _encZ_b3 As Tensor                        ' q(z|x) 编码器 (输出 log σ²)
    Private _dec_W1, _dec_b1, _dec_W2, _dec_b2 As Tensor        ' 解码器 p(x|z)

    ' 先验参数 p(z|y)
    Private _priorMu As Tensor      ' [K, latentDim] 每个类别的先验均值
    Private _priorLogVar As Tensor  ' [K, latentDim] 每个类别的先验 log 方差
    Private _priorLogits As Tensor  ' [K] 类别先验 logits (softmax 前的)

    ' 训练历史
    Private _lossHistory As New List(Of Double)

#End Region

#Region "属性"

    ''' <summary>输入维度</summary>
    Public ReadOnly Property InputDim As Integer
        Get
            Return _inputDim
        End Get
    End Property

    ''' <summary>连续隐变量维度</summary>
    Public ReadOnly Property LatentDim As Integer
        Get
            Return _latentDim
        End Get
    End Property

    ''' <summary>混合成分数量 (类别数)</summary>
    Public ReadOnly Property NComponents As Integer
        Get
            Return _nComponents
        End Get
    End Property

    ''' <summary>训练损失历史</summary>
    Public ReadOnly Property LossHistory As List(Of Double)
        Get
            Return _lossHistory
        End Get
    End Property

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建 GMVAE 模型
    ''' </summary>
    ''' <param name="inputDim">输入数据维度</param>
    ''' <param name="latentDim">连续隐变量维度</param>
    ''' <param name="nComponents">混合成分数量 K</param>
    ''' <param name="hiddenDim">隐藏层维度</param>
    ''' <param name="seed">随机种子</param>
    Public Sub New(inputDim As Integer,
                   Optional latentDim As Integer = 8,
                   Optional nComponents As Integer = 5,
                   Optional hiddenDim As Integer = 64,
                   Optional seed As Integer? = Nothing)
        _inputDim = inputDim
        _latentDim = latentDim
        _nComponents = nComponents
        _hiddenDim = hiddenDim
        _seed = seed

        InitializeNetwork()
    End Sub

    ''' <summary>
    ''' 初始化所有网络参数
    ''' </summary>
    Private Sub InitializeNetwork()
        Dim rng = If(_seed.HasValue, New Random(_seed.Value), New Random())

        ' q(y|x): inputDim → hiddenDim → nComponents
        _encY_W1 = Tensor.RandomNormal({_hiddenDim, _inputDim}, 0.0F, CSng(std.Sqrt(2.0 / _inputDim)), rng.Next())
        _encY_b1 = Tensor.Zeros({_hiddenDim})
        _encY_W2 = Tensor.RandomNormal({_nComponents, _hiddenDim}, 0.0F, CSng(std.Sqrt(2.0 / _hiddenDim)), rng.Next())
        _encY_b2 = Tensor.Zeros({_nComponents})

        ' q(z|x): inputDim → hiddenDim → latentDim (μ 和 log σ²)
        _encZ_W1 = Tensor.RandomNormal({_hiddenDim, _inputDim}, 0.0F, CSng(std.Sqrt(2.0 / _inputDim)), rng.Next())
        _encZ_b1 = Tensor.Zeros({_hiddenDim})
        _encZ_W2 = Tensor.RandomNormal({_latentDim, _hiddenDim}, 0.0F, CSng(std.Sqrt(2.0 / _hiddenDim)), rng.Next())
        _encZ_b2 = Tensor.Zeros({_latentDim})
        _encZ_W3 = Tensor.RandomNormal({_latentDim, _hiddenDim}, 0.0F, CSng(std.Sqrt(2.0 / _hiddenDim)), rng.Next())
        _encZ_b3 = Tensor.Zeros({_latentDim})

        ' 解码器 p(x|z): latentDim → hiddenDim → inputDim
        _dec_W1 = Tensor.RandomNormal({_hiddenDim, _latentDim}, 0.0F, CSng(std.Sqrt(2.0 / _latentDim)), rng.Next())
        _dec_b1 = Tensor.Zeros({_hiddenDim})
        _dec_W2 = Tensor.RandomNormal({_inputDim, _hiddenDim}, 0.0F, CSng(std.Sqrt(2.0 / _hiddenDim)), rng.Next())
        _dec_b2 = Tensor.Zeros({_inputDim})

        ' 先验 p(z|y): 每个类别一个均值和 log 方差
        _priorMu = Tensor.RandomNormal({_nComponents, _latentDim}, 0.0F, 1.0F, rng.Next())
        _priorLogVar = Tensor.Zeros({_nComponents, _latentDim})
        _priorLogits = Tensor.Zeros({_nComponents})  ' 均匀先验
    End Sub

#End Region

#Region "前向传播"

    ''' <summary>
    ''' 编码器 q(y|x): 输入 x, 输出类别 logits
    ''' </summary>
    Private Function EncodeY(x As Tensor) As Tensor
        ' x: [batch, inputDim]
        Dim h = LinearReLU(x, _encY_W1, _encY_b1)
        Dim logits = Linear(h, _encY_W2, _encY_b2)
        Return logits
    End Function

    ''' <summary>
    ''' 编码器 q(z|x): 输入 x, 输出 (μ_z, log σ²_z)
    ''' </summary>
    Private Sub EncodeZ(x As Tensor, ByRef mu As Tensor, ByRef logVar As Tensor)
        Dim h = LinearReLU(x, _encZ_W1, _encZ_b1)
        mu = Linear(h, _encZ_W2, _encZ_b2)
        logVar = Linear(h, _encZ_W3, _encZ_b3)
    End Sub

    ''' <summary>
    ''' 解码器 p(x|z): 输入 z, 输出重构 x_recon
    ''' </summary>
    Private Function Decode(z As Tensor) As Tensor
        Dim h = LinearReLU(z, _dec_W1, _dec_b1)
        Dim xRecon = Linear(h, _dec_W2, _dec_b2)
        Return xRecon
    End Function

    ''' <summary>
    ''' 完整前向传播
    ''' </summary>
    ''' <param name="x">输入 [batch, inputDim]</param>
    ''' <param name="rng">随机数生成器 (用于重参数化采样)</param>
    Public Sub Forward(x As Tensor, rng As Random,
                       ByRef yLogits As Tensor,
                       ByRef zMean As Tensor, ByRef zLogVar As Tensor,
                       ByRef zSample As Tensor, ByRef xRecon As Tensor)

        yLogits = EncodeY(x)
        EncodeZ(x, zMean, zLogVar)

        ' 重参数化采样: z = μ + σ * ε
        zSample = Reparameterize(zMean, zLogVar, rng)

        ' 解码
        xRecon = Decode(zSample)
    End Sub

#End Region

#Region "训练"

    ''' <summary>
    ''' 训练 GMVAE 模型
    ''' </summary>
    ''' <param name="X">训练数据 [N, inputDim]</param>
    ''' <param name="epochs">训练轮数</param>
    ''' <param name="batchSize">批大小</param>
    ''' <param name="learningRate">学习率</param>
    ''' <param name="beta">类别 KL 散度权重</param>
    ''' <param name="gamma">连续 KL 散度权重</param>
    ''' <param name="verbose">是否打印训练进度</param>
    Public Sub Fit(X As Tensor,
                   Optional epochs As Integer = 100,
                   Optional batchSize As Integer = 32,
                   Optional learningRate As Double = 0.001,
                   Optional beta As Double = 1.0,
                   Optional gamma As Double = 1.0,
                   Optional verbose As Boolean = True)

        Dim N = X.Shape(0)
        Dim rng = If(_seed.HasValue, New Random(_seed.Value + 1), New Random())

        _lossHistory.Clear()

        For epoch = 1 To epochs
            Dim epochLoss As Double = 0
            Dim epochReconLoss As Double = 0
            Dim epochKLY As Double = 0
            Dim epochKLZ As Double = 0
            Dim nBatches = 0

            ' 随机打乱数据
            Dim indices = Enumerable.Range(0, N).ToArray()
            For i = N - 1 To 1 Step -1
                Dim j = rng.Next(i + 1)
                Dim tmp = indices(i)
                indices(i) = indices(j)
                indices(j) = tmp
            Next

            ' 小批量训练
            For batchStart = 0 To N - 1 Step batchSize
                Dim batchEnd = std.Min(batchStart + batchSize, N)
                Dim bs = batchEnd - batchStart

                ' 构造批次数据
                Dim xBatch = New Tensor(bs, _inputDim)
                For i = 0 To bs - 1
                    For d = 0 To _inputDim - 1
                        xBatch(i, d) = X(indices(batchStart + i), d)
                    Next
                Next

                ' 前向传播
                Dim yLogits, zMean, zLogVar, zSample, xRecon As Tensor
                Forward(xBatch, rng, yLogits, zMean, zLogVar, zSample, xRecon)

                ' 计算损失
                Dim reconLoss, klY, klZ, totalLoss As Double
                ComputeLoss(xBatch, yLogits, zMean, zLogVar, zSample, xRecon,
                            beta, gamma, reconLoss, klY, klZ, totalLoss)

                ' 反向传播 + 更新参数
                BackwardAndUpdate(xBatch, yLogits, zMean, zLogVar, zSample, xRecon,
                                  beta, gamma, learningRate, bs)

                epochLoss += totalLoss
                epochReconLoss += reconLoss
                epochKLY += klY
                epochKLZ += klZ
                nBatches += 1
            Next

            epochLoss /= nBatches
            epochReconLoss /= nBatches
            epochKLY /= nBatches
            epochKLZ /= nBatches
            _lossHistory.Add(epochLoss)

            If verbose AndAlso (epoch Mod 10 = 0 OrElse epoch = 1 OrElse epoch = epochs) Then
                Console.WriteLine($"  Epoch {epoch,4}/{epochs} | Loss={epochLoss:F4} | Recon={epochReconLoss:F4} | KL(y)={epochKLY:F4} | KL(z)={epochKLZ:F4}")
            End If
        Next
    End Sub

    ''' <summary>
    ''' 计算 GMVAE 损失
    ''' L = ReconLoss + β·KL(q(y|x) || p(y)) + γ·KL(q(z|x) || p(z|y))
    ''' </summary>
    Private Sub ComputeLoss(x As Tensor, yLogits As Tensor,
                            zMean As Tensor, zLogVar As Tensor, zSample As Tensor,
                            xRecon As Tensor,
                            beta As Double, gamma As Double,
                            ByRef reconLoss As Double, ByRef klY As Double, ByRef klZ As Double,
                            ByRef totalLoss As Double)

        Dim bs = x.Shape(0)

        ' 1. 重构损失 (MSE)
        reconLoss = 0
        For i = 0 To bs - 1
            For d = 0 To _inputDim - 1
                Dim diff = x(i, d) - xRecon(i, d)
                reconLoss += diff * diff
            Next
        Next
        reconLoss /= bs

        ' 2. KL(q(y|x) || p(y))
        ' q(y|x) = softmax(yLogits), p(y) = softmax(priorLogits)
        klY = 0
        Dim qY = SoftmaxBatch(yLogits)        ' [bs, K]
        Dim pY = Softmax(_priorLogits)         ' [K]
        For i = 0 To bs - 1
            For k = 0 To _nComponents - 1
                If qY(i, k) > 0.0000001 Then
                    klY += qY(i, k) * std.Log(qY(i, k) / (pY(k) + 0.0000001))
                End If
            Next
        Next
        klY /= bs

        ' 3. KL(q(z|x) || p(z|y))
        ' 使用 q(y|x) 作为 y 的分布, 计算期望 KL
        ' KL(N(μ1,σ1) || N(μ2,σ2)) = 0.5 * [log(σ2²/σ1²) + (σ1² + (μ1-μ2)²)/σ2² - 1]
        klZ = 0
        For i = 0 To bs - 1
            ' 对每个类别 k 计算 KL(q(z|x) || p(z|y=k)), 然后用 q(y=k|x) 加权
            For k = 0 To _nComponents - 1
                Dim klZk As Double = 0
                For d = 0 To _latentDim - 1
                    Dim mu1 = zMean(i, d)
                    Dim logVar1 = zLogVar(i, d)
                    Dim var1 = std.Exp(logVar1)
                    Dim mu2 = _priorMu(k, d)
                    Dim var2 = std.Exp(_priorLogVar(k, d))
                    klZk += 0.5 * (std.Log(var2 / (var1 + 0.0000001)) +
                                   (var1 + (mu1 - mu2) * (mu1 - mu2)) / var2 - 1.0)
                Next
                klZ += qY(i, k) * klZk
            Next
        Next
        klZ /= bs

        totalLoss = reconLoss + beta * klY + gamma * klZ
    End Sub

#End Region

#Region "反向传播 (简化版, 基于数值梯度下降)"

    ''' <summary>
    ''' 反向传播与参数更新
    ''' 注: 由于 Tensor 类未实现自动微分, 这里使用解析梯度对关键参数进行更新
    ''' 为简化实现, 解码器和编码器使用重构损失梯度, KL 项通过解析公式直接对相关参数更新
    ''' </summary>
    Private Sub BackwardAndUpdate(x As Tensor, yLogits As Tensor,
                                  zMean As Tensor, zLogVar As Tensor, zSample As Tensor,
                                  xRecon As Tensor,
                                  beta As Double, gamma As Double,
                                  lr As Double, bs As Integer)

        ' ============================================================
        ' 1. 解码器梯度 (基于重构 MSE 损失)
        ' dL_recon/dxRecon = -2(x - xRecon) / bs
        ' ============================================================
        Dim dxRecon = New Tensor(bs, _inputDim)
        For i = 0 To bs - 1
            For d = 0 To _inputDim - 1
                dxRecon(i, d) = -2.0 * (x(i, d) - xRecon(i, d)) / bs
            Next
        Next

        ' 反向通过解码器第二层: h = ReLU(W1·z + b1), xRecon = W2·h + b2
        ' dL/dW2 = dxRecon^T · h, dL/db2 = sum(dxRecon), dL/dh = dxRecon · W2^T
        Dim h1 = LinearReLU(zSample, _dec_W1, _dec_b1)  ' 重新计算 h1
        Dim dW2 = MatMul(dxRecon.Transpose(), h1)
        Dim db2 = SumRows(dxRecon, _inputDim)
        Dim dh1 = MatMul(dxRecon, _dec_W2)

        ' ReLU 反向
        For i = 0 To bs - 1
            For j = 0 To _hiddenDim - 1
                If h1(i, j) <= 0 Then dh1(i, j) = 0
            Next
        Next

        ' 反向通过解码器第一层
        Dim dW1 = MatMul(dh1.Transpose(), zSample)
        Dim db1 = SumRows(dh1, _hiddenDim)
        Dim dzSample = MatMul(dh1, _dec_W1)

        ' 更新解码器参数
        UpdateTensor(_dec_W2, dW2, lr)
        UpdateTensor(_dec_b2, db2, lr)
        UpdateTensor(_dec_W1, dW1, lr)
        UpdateTensor(_dec_b1, db1, lr)

        ' ============================================================
        ' 2. 编码器 q(z|x) 梯度
        ' z = μ + σ * ε, 所以 dz/dμ = 1, dz/dσ = ε
        ' dL/dμ = dL/dz, dL/dlogVar = dL/dz * 0.5 * exp(logVar/2) * ε
        ' 加上 KL 项对 μ 和 logVar 的梯度
        ' ============================================================
        Dim dzMean = New Tensor(dzSample.Shape)
        For i = 0 To bs - 1
            For d = 0 To _latentDim - 1
                dzMean(i, d) = dzSample(i, d)
            Next
        Next

        ' KL(q(z|x) || p(z|y)) 对 μ 的梯度 (用 q(y|x) 加权)
        ' dKL/dμ1 = (μ1 - μ2) / var2 * q(y=k|x)
        Dim dzMeanKL = New Tensor(bs, _latentDim)
        Dim qY = SoftmaxBatch(yLogits)
        For i = 0 To bs - 1
            For d = 0 To _latentDim - 1
                Dim s As Double = 0
                For k = 0 To _nComponents - 1
                    Dim mu2 = _priorMu(k, d)
                    Dim var2 = std.Exp(_priorLogVar(k, d))
                    s += qY(i, k) * (zMean(i, d) - mu2) / var2
                Next
                dzMeanKL(i, d) = gamma * s / bs
            Next
        Next

        ' 合并重构和 KL 对 μ 的梯度
        For i = 0 To bs - 1
            For d = 0 To _latentDim - 1
                dzMean(i, d) += dzMeanKL(i, d)
            Next
        Next

        ' KL 对 logVar 的梯度
        ' dKL/dlogVar1 = 0.5 * (1 - var2/var1) * q(y=k|x) ... 简化
        Dim dzLogVar = New Tensor(bs, _latentDim)
        For i = 0 To bs - 1
            For d = 0 To _latentDim - 1
                Dim s As Double = 0
                Dim var1 = std.Exp(zLogVar(i, d))
                For k = 0 To _nComponents - 1
                    Dim var2 = std.Exp(_priorLogVar(k, d))
                    s += qY(i, k) * 0.5 * (1.0 - var2 / (var1 + 0.0000001))
                Next
                ' 重参数化: z = μ + exp(logVar/2) * ε, dL/dlogVar = dL/dz * 0.5 * exp(logVar/2) * ε
                ' 这里简化: 假设 ε 已包含在 dzSample 中
                dzLogVar(i, d) = gamma * s / bs + dzSample(i, d) * 0.5 * std.Exp(zLogVar(i, d) * 0.5) / bs
            Next
        Next

        ' 反向通过 q(z|x) 编码器
        ' 拼接 dzMean 和 dzLogVar 的反向通过共享的隐藏层
        ' 简化: 分别反向, 然后合并隐藏层梯度
        Dim hEncZ = LinearReLU(x, _encZ_W1, _encZ_b1)

        ' μ 分支: μ = W2·h + b2
        Dim dW2_z = MatMul(dzMean.Transpose(), hEncZ)
        Dim db2_z = SumRows(dzMean, _latentDim)
        Dim dhFromMu = MatMul(dzMean, _encZ_W2)

        ' logVar 分支: logVar = W3·h + b3
        Dim dW3_z = MatMul(dzLogVar.Transpose(), hEncZ)
        Dim db3_z = SumRows(dzLogVar, _latentDim)
        Dim dhFromLogVar = MatMul(dzLogVar, _encZ_W3)

        ' 合并隐藏层梯度
        Dim dhEncZ = New Tensor(bs, _hiddenDim)
        For i = 0 To bs - 1
            For j = 0 To _hiddenDim - 1
                dhEncZ(i, j) = dhFromMu(i, j) + dhFromLogVar(i, j)
            Next
        Next

        ' ReLU 反向
        For i = 0 To bs - 1
            For j = 0 To _hiddenDim - 1
                If hEncZ(i, j) <= 0 Then dhEncZ(i, j) = 0
            Next
        Next

        Dim dW1_z = MatMul(dhEncZ.Transpose(), x)
        Dim db1_z = SumRows(dhEncZ, _hiddenDim)

        UpdateTensor(_encZ_W2, dW2_z, lr)
        UpdateTensor(_encZ_b2, db2_z, lr)
        UpdateTensor(_encZ_W3, dW3_z, lr)
        UpdateTensor(_encZ_b3, db3_z, lr)
        UpdateTensor(_encZ_W1, dW1_z, lr)
        UpdateTensor(_encZ_b1, db1_z, lr)

        ' ============================================================
        ' 3. 编码器 q(y|x) 梯度 (基于 KL(q(y|x) || p(y)))
        ''' dKL/dlogits = β * (qY - pY) / bs
        ' ============================================================
        Dim dyLogits = New Tensor(bs, _nComponents)
        Dim pY = Softmax(_priorLogits)
        For i = 0 To bs - 1
            For k = 0 To _nComponents - 1
                dyLogits(i, k) = beta * (qY(i, k) - pY(k)) / bs
            Next
        Next

        Dim hEncY = LinearReLU(x, _encY_W1, _encY_b1)
        Dim dW2_y = MatMul(dyLogits.Transpose(), hEncY)
        Dim db2_y = SumRows(dyLogits, _nComponents)
        Dim dhEncY = MatMul(dyLogits, _encY_W2)

        For i = 0 To bs - 1
            For j = 0 To _hiddenDim - 1
                If hEncY(i, j) <= 0 Then dhEncY(i, j) = 0
            Next
        Next

        Dim dW1_y = MatMul(dhEncY.Transpose(), x)
        Dim db1_y = SumRows(dhEncY, _hiddenDim)

        UpdateTensor(_encY_W2, dW2_y, lr)
        UpdateTensor(_encY_b2, db2_y, lr)
        UpdateTensor(_encY_W1, dW1_y, lr)
        UpdateTensor(_encY_b1, db1_y, lr)

        ' ============================================================
        ' 4. 更新先验参数 p(z|y) 和 p(y)
        ' ============================================================
        ' p(z|y) 的 μ 和 logVar 梯度 (来自 KL 项)
        For k = 0 To _nComponents - 1
            For d = 0 To _latentDim - 1
                Dim gradMu As Double = 0
                Dim gradLogVar As Double = 0
                For i = 0 To bs - 1
                    Dim mu1 = zMean(i, d)
                    Dim var1 = std.Exp(zLogVar(i, d))
                    Dim mu2 = _priorMu(k, d)
                    Dim var2 = std.Exp(_priorLogVar(k, d))
                    ' dKL/dμ2 = -(μ1 - μ2) / var2
                    gradMu += -qY(i, k) * (mu1 - mu2) / var2 / bs
                    ' dKL/dlogVar2 = 0.5 * (var1/var2 - 1) * (-var2) ... 简化
                    gradLogVar += qY(i, k) * 0.5 * (var1 / (var2 + 0.0000001) - 1.0) * gamma / bs
                Next
                _priorMu(k, d) -= lr * gamma * gradMu
                _priorLogVar(k, d) -= lr * gradLogVar
            Next
        Next

        ' p(y) 的 logits 梯度
        For k = 0 To _nComponents - 1
            Dim s As Double = 0
            For i = 0 To bs - 1
                s += (qY(i, k) - pY(k)) / bs
            Next
            _priorLogits(k) -= lr * beta * s
        Next
    End Sub

#End Region

#Region "推理与生成"

    ''' <summary>
    ''' 编码输入数据, 返回 (类别概率, 隐变量均值, 隐变量 log 方差)
    ''' </summary>
    Public Sub Encode(x As Tensor,
                      ByRef yProba As Tensor,
                      ByRef zMean As Tensor, ByRef zLogVar As Tensor)
        Dim yLogits = EncodeY(x)
        yProba = SoftmaxBatch(yLogits)
        EncodeZ(x, zMean, zLogVar)
    End Sub

    ''' <summary>
    ''' 重构输入数据
    ''' </summary>
    Public Function Reconstruct(x As Tensor, Optional useMean As Boolean = True) As Tensor
        Dim zMean, zLogVar As Tensor
        EncodeZ(x, zMean, zLogVar)
        Dim zSample As Tensor
        If useMean Then
            zSample = zMean
        Else
            Dim rng = New Random()
            zSample = Reparameterize(zMean, zLogVar, rng)
        End If
        Return Decode(zSample)
    End Function

    ''' <summary>
    ''' 从指定类别采样生成数据
    ''' </summary>
    ''' <param name="n">采样数量</param>
    ''' <param name="componentIdx">类别索引 (-1 表示按先验混合采样)</param>
    ''' <param name="seed">随机种子</param>
    Public Function Generate(n As Integer, Optional componentIdx As Integer = -1, Optional seed As Integer? = Nothing) As Tensor
        Dim rng = If(seed.HasValue, New Random(seed.Value), New Random())
        Dim result = New Tensor(n, _inputDim)

        ' 累积先验概率
        Dim pY = Softmax(_priorLogits)
        Dim cumP = New Double(_nComponents - 1) {}
        cumP(0) = pY(0)
        For k = 1 To _nComponents - 1
            cumP(k) = cumP(k - 1) + pY(k)
        Next

        For i = 0 To n - 1
            Dim selK As Integer
            If componentIdx >= 0 AndAlso componentIdx < _nComponents Then
                selK = componentIdx
            Else
                Dim r = rng.NextDouble()
                selK = _nComponents - 1
                For k = 0 To _nComponents - 1
                    If r <= cumP(k) Then
                        selK = k
                        Exit For
                    End If
                Next
            End If

            ' 从 p(z|y=selK) 采样
            Dim z = New Tensor(1, _latentDim)
            For d = 0 To _latentDim - 1
                Dim u1 = 1.0 - rng.NextDouble()
                Dim u2 = 1.0 - rng.NextDouble()
                Dim eps = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
                Dim mu = _priorMu(selK, d)
                Dim sigma = std.Exp(_priorLogVar(selK, d) * 0.5)
                z(0, d) = mu + sigma * eps
            Next

            ' 解码
            Dim xGen = Decode(z)
            For d = 0 To _inputDim - 1
                result(i, d) = xGen(0, d)
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 聚类: 返回每个样本最可能属于的类别
    ''' </summary>
    Public Function PredictClusters(X As Tensor) As Integer()
        Dim yLogits = EncodeY(X)
        Dim yProba = SoftmaxBatch(yLogits)
        Dim N = X.Shape(0)
        Dim labels = New Integer(N - 1) {}

        For i = 0 To N - 1
            Dim maxVal As Double = Double.NegativeInfinity
            Dim maxIdx = 0
            For k = 0 To _nComponents - 1
                If yProba(i, k) > maxVal Then
                    maxVal = yProba(i, k)
                    maxIdx = k
                End If
            Next
            labels(i) = maxIdx
        Next
        Return labels
    End Function

#End Region

#Region "神经网络辅助函数"

    ''' <summary>线性层: y = W·x + b</summary>
    Private Function Linear(x As Tensor, W As Tensor, b As Tensor) As Tensor
        ' x: [bs, in], W: [out, in], b: [out]
        ' 结果: [bs, out]
        Dim bs = x.Shape(0)
        Dim outDim = W.Shape(0)
        Dim inDim = W.Shape(1)
        Dim result = New Tensor(bs, outDim)

        For i = 0 To bs - 1
            For j = 0 To outDim - 1
                Dim s As Double = b(j)
                For k = 0 To inDim - 1
                    s += W(j, k) * x(i, k)
                Next
                result(i, j) = s
            Next
        Next
        Return result
    End Function

    ''' <summary>线性层 + ReLU 激活</summary>
    Private Function LinearReLU(x As Tensor, W As Tensor, b As Tensor) As Tensor
        Dim y = Linear(x, W, b)
        For i = 0 To y.Length - 1
            If y(i) < 0 Then y(i) = 0
        Next
        Return y
    End Function

    ''' <summary>Softmax (1D)</summary>
    Private Function Softmax(logits As Tensor) As Tensor
        Dim n = logits.Length
        Dim maxVal = Double.NegativeInfinity
        For i = 0 To n - 1
            If logits(i) > maxVal Then maxVal = logits(i)
        Next

        Dim result = New Tensor(n)
        Dim sumExp As Double = 0
        For i = 0 To n - 1
            result(i) = std.Exp(logits(i) - maxVal)
            sumExp += result(i)
        Next
        For i = 0 To n - 1
            result(i) /= sumExp
        Next
        Return result
    End Function

    ''' <summary>批量 Softmax (2D, 沿第 1 维)</summary>
    Private Function SoftmaxBatch(logits As Tensor) As Tensor
        Dim bs = logits.Shape(0)
        Dim n = logits.Shape(1)
        Dim result = New Tensor(bs, n)

        For i = 0 To bs - 1
            Dim maxVal = Double.NegativeInfinity
            For j = 0 To n - 1
                If logits(i, j) > maxVal Then maxVal = logits(i, j)
            Next
            Dim sumExp As Double = 0
            For j = 0 To n - 1
                result(i, j) = std.Exp(logits(i, j) - maxVal)
                sumExp += result(i, j)
            Next
            For j = 0 To n - 1
                result(i, j) /= sumExp
            Next
        Next
        Return result
    End Function

    ''' <summary>重参数化采样: z = μ + σ * ε</summary>
    Private Function Reparameterize(mu As Tensor, logVar As Tensor, rng As Random) As Tensor
        Dim bs = mu.Shape(0)
        Dim dim = mu.Shape(1)
        Dim result = New Tensor(bs, dim)

        For i = 0 To bs - 1
            For d = 0 To dim - 1
                Dim u1 = 1.0 - rng.NextDouble()
                Dim u2 = 1.0 - rng.NextDouble()
                Dim eps = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
                Dim sigma = std.Exp(logVar(i, d) * 0.5)
                result(i, d) = mu(i, d) + sigma * eps
            Next
        Next
        Return result
    End Function

    ''' <summary>矩阵乘法 (封装 Tensor.MatMul)</summary>
    Private Function MatMul(a As Tensor, b As Tensor) As Tensor
        Return a.MatMul(b)
    End Function

    ''' <summary>对行求和, 返回 1D 张量</summary>
    Private Function SumRows(mat As Tensor, outDim As Integer) As Tensor
        Dim result = New Tensor(outDim)
        For j = 0 To outDim - 1
            Dim s As Double = 0
            For i = 0 To mat.Shape(0) - 1
                s += mat(i, j)
            Next
            result(j) = s
        Next
        Return result
    End Function

    ''' <summary>梯度下降更新参数: param -= lr * grad</summary>
    Private Sub UpdateTensor(ByRef param As Tensor, grad As Tensor, lr As Double)
        For i = 0 To param.Length - 1
            param(i) -= lr * grad(i)
        Next
    End Sub

#End Region

#Region "其他"

    Public Overrides Function ToString() As String
        Return $"GMVAE(inputDim={_inputDim}, latentDim={_latentDim}, K={_nComponents}, hiddenDim={_hiddenDim})"
    End Function

#End Region

End Class
