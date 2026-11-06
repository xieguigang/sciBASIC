' ---------------------------------------------------------------------------
' LayerNormAffine —— 带仿射参数的可训练 LayerNorm
'
' 与 `TensorOps.AddNorm*` 的区别（后者服务于英西翻译 demo，不能承载 torch 权重）：
'   * 有可学习的 gamma / beta；
'   * 有偏方差 + eps 位于 sqrt 内（与 `nn.LayerNorm` 一致）；
'   * 只做归一化，不做残差相加（残差由调用方显式相加）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' 可训练的仿射 LayerNorm：<c>y = γ·(x−μ)/sqrt(σ²+eps) + β</c>。
    ''' </summary>
    ''' <remarks>
    ''' 前向缓存均值与 <c>1/sqrt(σ²+eps)</c>；反向按标准公式
    ''' <c>dx = invStd·(dx̂ − mean(dx̂) − x̂·mean(dx̂⊙x̂))</c> 回传，
    ''' 同时累加 <c>γ</c> 与 <c>β</c> 的梯度。
    ''' </remarks>
    Public Class LayerNormAffine

        ''' <summary>前向缓存。</summary>
        Public Class Cache
            ''' <summary>本层输入。</summary>
            Public Input As Tensor
            ''' <summary>每个样本的均值。</summary>
            Public Mean As Double()
            ''' <summary>每个样本的 <c>1/sqrt(σ²+eps)</c>。</summary>
            Public InvStd As Double()
        End Class

        Private ReadOnly _gamma As Tensor
        Private ReadOnly _beta As Tensor
        Private ReadOnly _gammaOptimizer As Optimizer
        Private ReadOnly _betaOptimizer As Optimizer
        Private ReadOnly _eps As Double
        Private _lastCache As Cache

        ''' <summary>缩放参数（初始化为全 1）。</summary>
        Public ReadOnly Property Gamma As Tensor
            Get
                Return _gamma
            End Get
        End Property

        ''' <summary>偏移参数（初始化为全 0）。</summary>
        Public ReadOnly Property Beta As Tensor
            Get
                Return _beta
            End Get
        End Property

        ''' <summary>最近一次前向的缓存（反向传播需要）。</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>创建一个仿射 LayerNorm。</summary>
        ''' <param name="features">归一化的特征维（= 模型宽度）。</param>
        ''' <param name="eps">数值稳定项，与 <c>nn.LayerNorm</c> 默认值一致。</param>
        Public Sub New(features As Integer, Optional eps As Double = 0.00001)
            If features <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(features))

            _eps = eps
            _gamma = New Tensor(features)
            _beta = New Tensor(features)

            For i As Integer = 0 To features - 1
                _gamma.Data(i) = 1.0
            Next

            Call _gamma.MarkHostModified()

            _gammaOptimizer = New Optimizer(_gamma)
            _betaOptimizer = New Optimizer(_beta)
        End Sub

        ''' <summary>执行归一化，并缓存反向所需统计量。</summary>
        ''' <param name="x">输入张量（最后一维为特征维）。</param>
        Public Function Forward(x As Tensor) As Tensor
            Dim mean As Double() = Nothing
            Dim invStd As Double() = Nothing
            Dim y As Tensor = TensorOpsEx.LayerNormForward(x, _gamma, _beta, mean, invStd, _eps)

            _lastCache = New Cache With {
                .Input = x,
                .Mean = mean,
                .InvStd = invStd
            }

            Return y
        End Function

        ''' <summary>
        ''' 反向传播：返回对输入的梯度，并把 <c>γ</c>/<c>β</c> 的梯度累加到各自的优化器。
        ''' </summary>
        ''' <param name="forwardCache">前向缓存。</param>
        ''' <param name="dOut">上游梯度。</param>
        Public Function Backward(forwardCache As Cache, dOut As Tensor) As Tensor
            Dim cache As Cache = If(forwardCache, _lastCache)

            If cache Is Nothing OrElse cache.Input Is Nothing Then
                Throw New InvalidOperationException("必须先执行前向传播才能反向传播")
            End If

            Dim dGamma As Tensor = Nothing
            Dim dBeta As Tensor = Nothing
            Dim dx As Tensor = TensorOpsEx.LayerNormBackward(dOut, cache.Input, _gamma,
                                                            cache.Mean, cache.InvStd,
                                                            dGamma, dBeta)

            Call TensorOps.Accumulate(_gammaOptimizer.Gradient, dGamma)
            Call TensorOps.Accumulate(_betaOptimizer.Gradient, dBeta)

            Return dx
        End Function

        ''' <summary>清零 <c>γ</c>/<c>β</c> 的梯度累加器。</summary>
        Public Sub ZeroGradients()
            _gammaOptimizer.ZeroGrad()
            _betaOptimizer.ZeroGrad()
        End Sub

        ''' <summary>对 <c>γ</c>/<c>β</c> 执行一次优化器更新。</summary>
        ''' <param name="learningRate">学习率。</param>
        ''' <param name="step">训练步序号（从 1 开始，用于 Adam 偏置校正）。</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            _gammaOptimizer.MakeTrainingStep(learningRate, [step], _gamma)
            _betaOptimizer.MakeTrainingStep(learningRate, [step], _beta)
        End Sub

    End Class

End Namespace
