Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Poisson

    ''' <summary>
    ''' 泊松回归的拟合结果：系数、统计推断量与过度离散诊断。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 系数分成两部分保存：<see cref="Intercept"/> 是独立的截距，
    ''' <see cref="Coefficients"/> 是与输入特征列一一对应的斜率
    ''' （也是 <see cref="IFitted.Polynomial"/> 的 <see cref="Formula.Factors"/>）。
    ''' 而 <see cref="StandardErrors"/>、<see cref="ZValues"/>、
    ''' <see cref="IncidenceRateRatios"/> 与 <see cref="FeatureNames"/>
    ''' 的长度都是待估参数的个数，顺序为「截距（若拟合）→ 各个特征」。
    ''' </para>
    ''' <para>
    ''' <see cref="GetY"/> 返回单位暴露量之下的期望计数（事件率）；若拟合时使用了
    ''' 暴露量 offset，请用 <see cref="Predict"/> 传入对应的暴露量得到期望计数，
    ''' 或者直接读取 <see cref="FittedValues"/>（已经乘上了每个样本各自的暴露量）。
    ''' </para>
    ''' <para>
    ''' <see cref="IFitted.R2"/> 返回的是基于残差偏差的伪 R²
    ''' （``1 - Deviance / NullDeviance``，裁剪到 ``[0,1]``），
    ''' 而不是线性回归的决定系数。
    ''' </para>
    ''' </remarks>
    Public Class PoissonFit : Implements IFitted

        ''' <summary>截距项（对数尺度）</summary>
        Public Property Intercept As Double
        ''' <summary>各个特征列对应的回归系数，不包含截距</summary>
        Public Property Coefficients As Double()
        ''' <summary>是否拟合了独立截距项</summary>
        Public Property AddIntercept As Boolean
        ''' <summary>参数名称，顺序为「截距（若拟合）→ 各个特征列」</summary>
        Public Property FeatureNames As String()
        ''' <summary>每个样本的暴露量；没有指定 offset 时全部为 1</summary>
        Public Property Exposures As Double()
        ''' <summary>拟合时是否使用了非平凡的暴露量 offset</summary>
        Public Property HasOffset As Boolean

        ''' <summary>系数的标准误，长度等于待估参数个数</summary>
        Public Property StandardErrors As Double()
        ''' <summary>Wald Z 统计量 = beta / SE</summary>
        Public Property ZValues As Double()
        ''' <summary>发生率比 IRR = exp(beta)，截距对应基准期望计数</summary>
        Public Property IncidenceRateRatios As Double()
        ''' <summary>逐样本的期望计数（已含各自暴露量）</summary>
        Public Property FittedValues As Double()

        ''' <summary>残差偏差 ``D = 2 * sum(y*log(y/mu) - (y - mu))``</summary>
        Public Property Deviance As Double
        ''' <summary>仅含截距的零模型偏差</summary>
        Public Property NullDeviance As Double
        ''' <summary>残差自由度 = 样本数 - 待估参数个数</summary>
        Public Property ResidualDf As Integer
        ''' <summary>Pearson 卡方统计量</summary>
        Public Property PearsonChiSquare As Double
        ''' <summary>收敛时的对数似然（含 ``-log(y!)`` 常数项）</summary>
        Public Property LogLikelihood As Double
        ''' <summary>AIC = 2*p - 2*logLik</summary>
        Public Property AIC As Double

        ''' <summary>是否在迭代上限之内收敛</summary>
        Public Property Converged As Boolean
        ''' <summary>实际执行的 Newton / IRLS 迭代轮数</summary>
        Public Property Iterations As Integer
        ''' <summary>逐点误差测试结果</summary>
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

        ''' <summary>
        ''' 全部待估参数（截距 + 斜率），顺序与
        ''' <see cref="StandardErrors"/> / <see cref="FeatureNames"/> 一致
        ''' </summary>
        Public ReadOnly Property AllCoefficients As Double()
            Get
                Dim p As Integer = Coefficients.Length + If(AddIntercept, 1, 0)
                Dim all As Double() = New Double(p - 1) {}
                Dim k As Integer = 0

                If AddIntercept Then
                    all(0) = Intercept
                    k = 1
                End If

                For j As Integer = 0 To Coefficients.Length - 1
                    all(k + j) = Coefficients(j)
                Next

                Return all
            End Get
        End Property

        ''' <summary>
        ''' 离散比 = 残差偏差 / 残差自由度。接近 1 说明等散性假设成立，
        ''' 远大于 1（经验上 &gt; 1.5）提示过度离散，此时系数标准误会被低估。
        ''' </summary>
        Public ReadOnly Property DispersionRatio As Double
            Get
                If ResidualDf <= 0 Then
                    Return Double.NaN
                End If

                Return Deviance / ResidualDf
            End Get
        End Property

        ''' <summary>Pearson 卡方 / 残差自由度，离散比的另一种估计</summary>
        Public ReadOnly Property PearsonDispersion As Double
            Get
                If ResidualDf <= 0 Then
                    Return Double.NaN
                End If

                Return PearsonChiSquare / ResidualDf
            End Get
        End Property

        ''' <summary>离散比是否超过 1.5（提示应改用负二项 / 准泊松等扩展模型）</summary>
        Public ReadOnly Property IsOverDispersed As Boolean
            Get
                Dim ratio As Double = DispersionRatio
                Return Not Double.IsNaN(ratio) AndAlso ratio > 1.5
            End Get
        End Property

        ''' <summary>
        ''' 基于偏差的伪 R² = ``1 - Deviance / NullDeviance``，裁剪到 ``[0,1]``。
        ''' 零模型偏差为 0（响应变量几乎为常数）时返回 0。
        ''' </summary>
        Public ReadOnly Property PseudoR2 As Double
            Get
                If NullDeviance <= 0.0 Then
                    Return 0.0
                End If

                Dim r As Double = 1.0 - Deviance / NullDeviance

                If Double.IsNaN(r) OrElse Double.IsInfinity(r) Then
                    Return 0.0
                End If

                Return std.Min(1.0, std.Max(0.0, r))
            End Get
        End Property

        ''' <summary>
        ''' <see cref="IFitted.R2"/> 的泊松回归语义：基于偏差的伪 R²，
        ''' 而不是普通最小二乘的决定系数。
        ''' </summary>
        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                Return PseudoR2
            End Get
        End Property

        ''' <summary>以对数链接多项式形式表示的模型</summary>
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            Get
                Return New PoissonPolynomial With {
                    .Intercept = Intercept,
                    .Factors = Coefficients
                }
            End Get
        End Property

        ''' <summary>
        ''' 预测单位暴露量之下的期望计数（事件率）：``mu = exp(c + sum(b*x))``。
        ''' </summary>
        ''' <param name="x">自变量向量，顺序与特征列一致</param>
        ''' <returns>恒为正的期望计数</returns>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If Coefficients Is Nothing Then
                Throw New InvalidOperationException("the poisson model has not been fitted yet!")
            End If
            If x.Length < Coefficients.Length Then
                Throw New ArgumentException($"the poisson model requires {Coefficients.Length} feature values, but only {x.Length} were given!", NameOf(x))
            End If

            Dim eta As Double = Intercept

            For i As Integer = 0 To Coefficients.Length - 1
                eta += Coefficients(i) * x(i)
            Next

            Return std.Exp(PoissonFitting.ClampEta(eta))
        End Function

        ''' <summary>
        ''' 预测给定暴露量之下的期望计数：``mu = exposure * exp(c + sum(b*x))``。
        ''' 没有 offset 时 <paramref name="exposure"/> 取缺省值 1，结果与
        ''' <see cref="GetY"/> 相同。
        ''' </summary>
        ''' <param name="x">自变量向量</param>
        ''' <param name="exposure">暴露量（人年、人口数等），必须为正数</param>
        ''' <returns>期望计数值</returns>
        Public Function Predict(x As Double(), Optional exposure As Double = 1.0) As Double
            If Not Double.IsFinite(exposure) OrElse exposure <= 0.0 Then
                Throw New ArgumentException($"the exposure must be a finite positive number, but {exposure} was given!", NameOf(exposure))
            End If

            Return GetY(x) * exposure
        End Function

        Public Overrides Function ToString() As String
            Return $"Poisson(log link): mu = exp({Intercept:F4} + sum(b*x)) @ D={Deviance:F4}, df={ResidualDf}, dispersion={DispersionRatio:F4}, pseudoR2={PseudoR2:F4}"
        End Function
    End Class
End Namespace
