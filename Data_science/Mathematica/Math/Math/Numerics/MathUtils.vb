' ============================================================================
' MathUtils.vb - 通用概率/统计学/信息论工具函数
'
' 本模块由 SMRUCC.genomics.Analysis.metaTraits (Traitar.Utils.MathUtils) 迁移而来，
' 并与算法库内已有的实现做了去重合并：凡是库中已有等价（且性能更好）的实现，
' 这里都不再重复实现，调用方直接使用既有 API：
'
'   已移除的冗余实现                -> 库内既有实现
'   ---------------------------------------------------------------------------
'   DotProduct(a, b)                -> Microsoft.VisualBasic.Math.LinearAlgebra.Vector.dot(a, b)  (SIMD)
'   L1Norm(v)                       -> Microsoft.VisualBasic.Math.SIMD.SimdReduce.L1Norm(v)
'   L2Norm(v)                       -> Microsoft.VisualBasic.Math.SIMD.SimdReduce.L2Norm(v)
'   L2NormSquared(v)                -> Microsoft.VisualBasic.Math.SIMD.SimdReduce.SumSquares(v)
'   Mean(v)                         -> Microsoft.VisualBasic.Math.SIMD.SimdReduce.Mean(v)
'   Variance(v)                     -> Microsoft.VisualBasic.Math.Statistics.Linq.Variance(v, isSample:=False)
'   StandardDeviation(v)            -> Microsoft.VisualBasic.Math.Statistics.Linq.StandardDeviation(v)
'   Gamma(x)                        -> Microsoft.VisualBasic.Math.Distributions.MathGamma.gamma(x)
'   LogGamma(x)                     -> Microsoft.VisualBasic.Math.Distributions.MathGamma.lngamm(x)
'   Transpose(m)                    -> Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.MatrixOps.Transpose(m)
'   SetSeed/RandDouble/RandInt/Shuffle -> Microsoft.VisualBasic.Math.RandomExtensions
'
' 注意：SimdReduce 系列函数在输入为空数组时会抛出 ArgumentException，
'       而旧的 MathUtils 版本会静默返回 0；调用方需要自行处理空输入。
' ============================================================================

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

Namespace Numerics

    ''' <summary>
    ''' 基础数学工具函数：概率运算、信息论度量、特殊函数与 Hinge 损失。
    ''' </summary>
    ''' <remarks>
    ''' 本模块只保留基础算法库中尚无等价实现的计算逻辑。
    ''' </remarks>
    Public Module MathUtils

#Region "1. 概率论运算"

        ''' <summary>
        ''' 计算分支上发生 Gain 或 Loss 的联合概率（两个独立事件至少发生一个的概率）
        ''' 论文公式：<c>x = g + l - g × l</c>
        ''' </summary>
        ''' <param name="gainProb">Gain 概率 g</param>
        ''' <param name="lossProb">Loss 概率 l</param>
        ''' <returns><c>P(A∪B) = P(A) + P(B) - P(A)×P(B)</c></returns>
        Public Function GainOrLossProbability(gainProb As Double,
                                              lossProb As Double) As Double
            Return gainProb + lossProb - gainProb * lossProb
        End Function

        ''' <summary>
        ''' 计算两个事件同时发生的联合概率（假设独立）：<c>P(A∩B) = P(A) × P(B)</c>
        ''' </summary>
        Public Function JointProbability(probA As Double, probB As Double) As Double
            Return probA * probB
        End Function

        ''' <summary>
        ''' 阈值过滤：将概率转为离散标签，并标记该样本是否置信。
        ''' </summary>
        ''' <param name="probability">概率值</param>
        ''' <param name="threshold">置信阈值，一般为 0.5</param>
        ''' <param name="isConfident">概率落在 [threshold, 1 - threshold] 区间之外时为 True</param>
        ''' <returns>1 = 事件发生，0 = 事件未发生，-1 = 不确定（应当丢弃该样本）</returns>
        Public Function ThresholdFilter(probability As Double,
                                        threshold As Double,
                                        <Out()> ByRef isConfident As Boolean) As Integer
            If probability >= threshold Then
                isConfident = True
                Return 1
            ElseIf probability <= 1.0 - threshold Then
                isConfident = True
                Return 0
            Else
                ' 不确定样本，丢弃
                isConfident = False
                Return -1
            End If
        End Function

        ''' <summary>
        ''' 阈值过滤：将概率转为离散标签（不返回置信度标记）。
        ''' </summary>
        ''' <param name="probability">概率值</param>
        ''' <param name="threshold">置信阈值，默认为 0.5</param>
        ''' <returns>1 = 事件发生，0 = 事件未发生，-1 = 不确定（应当丢弃该样本）</returns>
        Public Function ThresholdFilter(probability As Double,
                                        Optional threshold As Double = 0.5) As Integer
            Dim isConfident As Boolean
            Return ThresholdFilter(probability, threshold, isConfident)
        End Function

#End Region

#Region "2. 统计学运算"

        ''' <summary>
        ''' 计算两个 0/1 二值向量之间的皮尔逊相关系数。
        ''' </summary>
        ''' <remarks>
        ''' 相关系数本身的计算直接复用基础算法库中的 Correlations.GetPearson，
        ''' 这里只做整数向量到实数向量的类型适配。
        ''' </remarks>
        Public Function BinaryPearsonCorrelation(x As Integer(), y As Integer()) As Double
            If x Is Nothing OrElse y Is Nothing Then Return 0.0
            If x.Length <> y.Length OrElse x.Length = 0 Then Return 0.0

            Dim n As Integer = x.Length
            Dim xD As Double() = New Double(n - 1) {}
            Dim yD As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                xD(i) = x(i)
                yD(i) = y(i)
            Next

            Return GetPearson(xD, yD)
        End Function

#End Region

#Region "3. 信息论与对数运算"

        ''' <summary>
        ''' 自然对数（安全版本，避免 log(0) 产生的 -Infinity）
        ''' </summary>
        Public Function SafeLog(x As Double) As Double
            If x <= 0.0 Then Return -700.0  ' 接近负无穷
            Return std.Log(x)
        End Function

        ''' <summary>
        ''' 以 2 为底的对数（安全版本）
        ''' </summary>
        Public Function Log2(x As Double) As Double
            If x <= 0.0 Then Return -700.0
            Return std.Log(x) / std.Log(2.0)
        End Function

        ''' <summary>
        ''' 计算伯努利分布的对数似然
        ''' <c>LL = Σ(y_i × log(p_i) + (1 - y_i) × log(1 - p_i))</c>
        ''' </summary>
        ''' <param name="observed">观测到的 0/1 标签</param>
        ''' <param name="predictedProb">对应的预测概率</param>
        Public Function LogLikelihood(observed As Integer(),
                                      predictedProb As Double()) As Double
            If observed Is Nothing OrElse predictedProb Is Nothing Then Return 0.0
            If observed.Length <> predictedProb.Length Then Return 0.0

            Dim ll As Double = 0.0

            For i As Integer = 0 To observed.Length - 1
                Dim p As Double = std.Max(0.0000000001, std.Min(1.0 - 0.0000000001, predictedProb(i)))

                If observed(i) = 1 Then
                    ll += SafeLog(p)
                Else
                    ll += SafeLog(1.0 - p)
                End If
            Next

            Return ll
        End Function

#End Region

#Region "4. 特殊函数"

        ''' <summary>
        ''' Logistic/Sigmoid 函数：<c>σ(x) = 1 / (1 + e^(-x))</c>
        ''' </summary>
        Public Function Sigmoid(x As Double) As Double
            If x < -700 Then Return 0.0
            If x > 700 Then Return 1.0
            Return 1.0 / (1.0 + std.Exp(-x))
        End Function

        ''' <summary>
        ''' Logistic 函数的反函数（Logit）：<c>logit(p) = log(p / (1 - p))</c>
        ''' </summary>
        Public Function Logit(p As Double) As Double
            If p <= 0.0 Then Return -700.0
            If p >= 1.0 Then Return 700.0
            Return std.Log(p / (1.0 - p))
        End Function

#End Region

#Region "5. Hinge 损失"

        ''' <summary>
        ''' Hinge Loss（L2 损失版本）：<c>L(y, f(x)) = max(0, 1 - y·f(x))²</c>
        ''' </summary>
        Public Function L2HingeLoss(y As Integer, score As Double) As Double
            Dim loss As Double = 1.0 - y * score
            If loss < 0 Then loss = 0
            Return loss * loss
        End Function

        ''' <summary>
        ''' Hinge Loss（L1 损失版本）：<c>L(y, f(x)) = max(0, 1 - y·f(x))</c>
        ''' </summary>
        Public Function L1HingeLoss(y As Integer, score As Double) As Double
            Dim loss As Double = 1.0 - y * score
            If loss < 0 Then loss = 0
            Return loss
        End Function

#End Region

    End Module

End Namespace
