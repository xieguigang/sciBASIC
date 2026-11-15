#Region "Microsoft.VisualBasic::b81e4c2d7a0f39e5c1d6b4a82f7e3d90, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\PermutationTest.vb"

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

    ' Class PermutationTestResult
    ' 
    '     Properties: Analysis, Component, Correlation, Count, InvalidReason
    '                 IsValid, Method, PValue, Q2, Q2Fit, Q2Intercept
    '                 Q2PValue, Q2Slope, R2Y, R2YFit, R2YIntercept
    '                 R2YSlope, OriginalQ2, OriginalR2Y
    ' 
    ' Module PermutationTest
    ' 
    '     Function: FitLine, Pearson, Permute
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports System.Threading.Tasks
Imports std = System.Math

''' <summary>
''' 置换检验（permutation test）的计算结果。
''' 
''' <para>
''' 对应 OPLS-DA / PLS-DA 的标准验证图：横轴是「置换后的响应变量与原始响应变量的相关系数」，
''' 纵轴分别是每次置换模型的 <c>R2Y</c> 与累计 <c>Q2</c>；两条最小二乘回归线的截距
''' 越接近 0（R2Y 截距）与越负（Q2 截距），说明模型越不可能是偶然相关得到的。
''' </para>
''' </summary>
Public Class PermutationTestResult

    ''' <summary>
    ''' 参与置换检验的分析类型：<see cref="PLS"/> 或者 <see cref="OPLS"/>。
    ''' </summary>
    Public Property Analysis As Type

    ''' <summary>
    ''' 分析方法的名称标识（<c>pls</c> / <c>opls</c>）。
    ''' </summary>
    Public Property Method As String

    ''' <summary>
    ''' 建模时使用的成分数量。
    ''' </summary>
    Public Property Component As Integer

    ''' <summary>
    ''' 实际完成的有效置换次数（排除了计算失败的置换）。
    ''' </summary>
    Public Property Count As Integer

    ''' <summary>
    ''' 每次置换的响应变量与原始响应变量之间的 Pearson 相关系数，即置换图的横轴。
    ''' </summary>
    Public Property Correlation As Double()

    ''' <summary>
    ''' 每次置换模型的 R2Y，即置换图的纵轴之一。
    ''' </summary>
    Public Property R2Y As Double()

    ''' <summary>
    ''' 每次置换模型的累计 Q2，即置换图的另一个纵轴。
    ''' </summary>
    Public Property Q2 As Double()

    ''' <summary>
    ''' R2Y ~ 相关系数 回归线的斜率。
    ''' </summary>
    Public Property R2YSlope As Double = Double.NaN

    ''' <summary>
    ''' R2Y ~ 相关系数 回归线的截距。
    ''' </summary>
    Public Property R2YIntercept As Double = Double.NaN

    ''' <summary>
    ''' R2Y ~ 相关系数 回归线的决定系数（拟合优度，仅用于标注）。
    ''' </summary>
    Public Property R2YFit As Double = Double.NaN

    ''' <summary>
    ''' Q2 ~ 相关系数 回归线的斜率。
    ''' </summary>
    Public Property Q2Slope As Double = Double.NaN

    ''' <summary>
    ''' Q2 ~ 相关系数 回归线的截距。
    ''' </summary>
    Public Property Q2Intercept As Double = Double.NaN

    ''' <summary>
    ''' Q2 ~ 相关系数 回归线的决定系数（拟合优度，仅用于标注）。
    ''' </summary>
    Public Property Q2Fit As Double = Double.NaN

    ''' <summary>
    ''' 原始模型（未置换）的 R2Y。
    ''' </summary>
    Public Property OriginalR2Y As Double = Double.NaN

    ''' <summary>
    ''' 原始模型（未置换）的累计 Q2。
    ''' </summary>
    Public Property OriginalQ2 As Double = Double.NaN

    ''' <summary>
    ''' 基于 R2Y 的单侧 p 值：<c>(count(perm R2Y >= original R2Y) + 1) / (N + 1)</c>。
    ''' </summary>
    Public Property PValue As Double = Double.NaN

    ''' <summary>
    ''' 基于累计 Q2 的单侧 p 值，计算方式与 <see cref="PValue"/> 一致。
    ''' </summary>
    Public Property Q2PValue As Double = Double.NaN

    ''' <summary>
    ''' 结果是否可用于解释/绘图（样本数不足、模型无法收敛时为 False）。
    ''' </summary>
    Public Property IsValid As Boolean = True

    ''' <summary>
    ''' 当 <see cref="IsValid"/> 为 False 时给出原因。
    ''' </summary>
    Public Property InvalidReason As String

    Public Overrides Function ToString() As String
        If Not IsValid Then
            Return $"invalid permutation test: {InvalidReason}"
        End If

        Return $"{Method} permutation test [N={Count}]: R2Y={OriginalR2Y.ToString("F4")} (p={PValue.ToString("F4")}), " &
            $"Q2={OriginalQ2.ToString("F4")} (p={Q2PValue.ToString("F4")})"
    End Function

End Class

''' <summary>
''' 多元统计模型的置换检验。
''' 
''' <para>
''' 实现方式是：固定 X，对响应变量 Y 做随机重排（相当于打散样本与类别之间的对应关系），
''' 然后重新跑一次完整的 PLS/OPLS 建模，记录该次置换模型的 R2Y 与累计 Q2；
''' 重复 N 次之后，用 R2Y/Q2 对置换相关系数做最小二乘回归，
''' 并统计「置换模型的指标不低于原始模型」的比例作为单侧 p 值。
''' </para>
''' 
''' <para>
''' 性能说明：每次迭代都通过 <see cref="StatisticsObject.WithResponse"/> 构建只替换 Y 的浅拷贝，
''' 因此 X 的标准化结果被所有迭代共享；迭代之间没有共享可变状态，可以安全地用 <see cref="Parallel"/> 并行。
''' 代价是 PLS/OPLS 内部仍然会跑一遍交叉验证（Q2 的来源），所以样本量与变量数较大时需要相应降低置换次数。
''' </para>
''' </summary>
Public Module PermutationTest

    ''' <summary>
    ''' 对给定的统计对象执行置换检验。
    ''' </summary>
    ''' <param name="statObject">包含 X 与原始 Y 的统计对象</param>
    ''' <param name="numPermutations">置换次数（默认 200，与常见的 OPLS-DA 验证图一致）</param>
    ''' <param name="component">建模成分数量（PLS 为潜变量个数，OPLS 为正交成分个数）</param>
    ''' <param name="method">分析方法：<c>pls</c> 或者 <c>opls</c></param>
    ''' <param name="seed">随机种子，指定之后置换序列可复现</param>
    ''' <param name="parallel">是否并行执行各个置换迭代</param>
    ''' <param name="progress">每完成一次置换调用的回调，参数为已完成的次数（可能来自不同线程）</param>
    Public Function Test(statObject As StatisticsObject,
                         Optional numPermutations As Integer = 200,
                         Optional component As Integer = 2,
                         Optional method As String = "pls",
                         Optional seed As Integer? = Nothing,
                         Optional parallel As Boolean = True,
                         Optional progress As Action(Of Integer) = Nothing) As PermutationTestResult

        Dim name As String = If(method, "pls").Trim.ToLower

        Select Case name
            Case "pls", "plsda", "pls-da"
                Return Run(statObject, GetType(PLS), numPermutations, component, seed, parallel, progress)
            Case "opls", "oplsda", "opls-da"
                Return Run(statObject, GetType(OPLS), numPermutations, component, seed, parallel, progress)
            Case Else
                Return New PermutationTestResult With {
                    .Method = name,
                    .IsValid = False,
                    .InvalidReason = $"unsupported analysis method: {method}"
                }
        End Select
    End Function

    ''' <summary>
    ''' 对给定的统计对象执行置换检验，直接指定分析类型。
    ''' </summary>
    ''' <param name="statObject">包含 X 与原始 Y 的统计对象</param>
    ''' <param name="analysis">分析类型：<see cref="PLS"/> 或者 <see cref="OPLS"/></param>
    Public Function Test(statObject As StatisticsObject,
                         analysis As Type,
                         Optional numPermutations As Integer = 200,
                         Optional component As Integer = 2,
                         Optional seed As Integer? = Nothing,
                         Optional parallel As Boolean = True,
                         Optional progress As Action(Of Integer) = Nothing) As PermutationTestResult

        Return Run(statObject, analysis, numPermutations, component, seed, parallel, progress)
    End Function

    Private Function Run(statObject As StatisticsObject,
                         analysis As Type,
                         numPermutations As Integer,
                         component As Integer,
                         seed As Integer?,
                         parallel As Boolean,
                         progress As Action(Of Integer)) As PermutationTestResult

        Dim method As String = If(analysis Is GetType(OPLS), "opls", "pls")
        Dim result As New PermutationTestResult With {
            .Analysis = analysis,
            .Method = method,
            .Component = component
        }

        ' ---- 输入校验 ----
        If statObject Is Nothing Then
            Return Invalid(result, "the statistics object is null")
        End If
        If statObject.XDataMatrix Is Nothing OrElse statObject.XScaled Is Nothing Then
            Return Invalid(result, "the statistics object is not initialized (X matrix is missing)")
        End If

        Dim y As Double() = statObject.YVariables

        If y Is Nothing OrElse y.Length = 0 Then
            Return Invalid(result, "the response variable Y is missing")
        End If
        If y.Length <> statObject.XDataMatrix.GetLength(0) Then
            Return Invalid(result, $"the response vector length {y.Length} does not match the sample size {statObject.XDataMatrix.GetLength(0)}")
        End If
        If y.Length < 5 Then
            Return Invalid(result, $"at least 5 samples are required, but only {y.Length} were given")
        End If
        If numPermutations < 1 Then
            Return Invalid(result, "the permutation count must be greater than zero")
        End If

        ' ---- 原始模型 ----
        Dim original As MultivariateAnalysisResult

        Try
            original = Model(statObject, analysis, component)
        Catch ex As Exception
            Return Invalid(result, $"the original model failed: {ex.Message}")
        End Try

        Dim r2yOriginal As Double = MultivariateMetrics.R2Y(original)
        Dim q2Original As Double = MultivariateMetrics.Q2(original)

        result.OriginalR2Y = r2yOriginal
        result.OriginalQ2 = q2Original

        If Double.IsNaN(r2yOriginal) Then
            Return Invalid(result, "the original model does not provide a valid R2Y value")
        End If

        ' ---- 预处理置换序列（串行生成，保证随机种子可复现） ----
        Dim rnd As Random = If(seed.HasValue, New Random(seed.Value), New Random())
        Dim permutations As Double()() = New Double(numPermutations - 1)() {}
        Dim correlations As Double() = New Double(numPermutations - 1) {}

        For i As Integer = 0 To numPermutations - 1
            Dim permuted As Double() = Permute(y, rnd)

            permutations(i) = permuted
            correlations(i) = Pearson(permuted, y)
        Next

        ' ---- 逐个置换建模 ----
        Dim r2yValues As Double() = New Double(numPermutations - 1) {}
        Dim q2Values As Double() = New Double(numPermutations - 1) {}
        Dim completed As Integer = 0

        Dim body As Action(Of Integer) =
            Sub(i As Integer)
                Try
                    Dim permutedModel As MultivariateAnalysisResult = Model(statObject.WithResponse(permutations(i)), analysis, component)

                    r2yValues(i) = MultivariateMetrics.R2Y(permutedModel)
                    q2Values(i) = ClampQ2(MultivariateMetrics.Q2(permutedModel))
                Catch ex As Exception
                    ' 单次置换失败（例如某次重排之后模型无法收敛）不应该影响整个检验
                    r2yValues(i) = Double.NaN
                    q2Values(i) = Double.NaN
                End Try

                If progress IsNot Nothing Then
                    Call progress(Interlocked.Increment(completed))
                End If
            End Sub

        If parallel AndAlso numPermutations > 1 Then
            ' 这里必须写全名：项目根命名空间下的 `Parallel` 会被解析成 Microsoft.VisualBasic.Parallel
            Call System.Threading.Tasks.Parallel.For(0, numPermutations, body)
        Else
            For i As Integer = 0 To numPermutations - 1
                Call body(i)
            Next
        End If

        ' ---- 汇总指标与回归线 ----
        result.Correlation = correlations
        result.R2Y = r2yValues
        result.Q2 = q2Values
        result.Count = numPermutations

        Dim slope#, intercept#, fit#

        Call FitLine(correlations, r2yValues, slope, intercept, fit)

        result.R2YSlope = slope
        result.R2YIntercept = intercept
        result.R2YFit = fit

        Call FitLine(correlations, q2Values, slope, intercept, fit)

        result.Q2Slope = slope
        result.Q2Intercept = intercept
        result.Q2Fit = fit

        result.PValue = UpperTailPValue(r2yValues, r2yOriginal)
        result.Q2PValue = If(Double.IsNaN(q2Original), Double.NaN, UpperTailPValue(q2Values, q2Original))

        Return result
    End Function

    ''' <summary>
    ''' 置换模型的 Q2 会出现「残差平方和趋近于 0 导致比值爆炸」的数值伪影
    ''' （例如 -70）。对于置换图而言，任何小于 -1 的 Q2 都只代表「完全没有预测能力」，
    ''' 因此统一压到 -1，避免纵轴被个别伪影撑开、把 R2Y 序列压成一条贴边的直线。
    ''' </summary>
    Private Function ClampQ2(value As Double) As Double
        If IsInvalid(value) Then
            Return Double.NaN
        End If

        Return std.Max(-1.0, value)
    End Function

    Private Function Model(statObject As StatisticsObject, analysis As Type, component As Integer) As MultivariateAnalysisResult
        If analysis Is GetType(OPLS) Then
            Return OPLS.OrthogonalProjectionsToLatentStructures(statObject, component)
        Else
            Return PLS.PartialLeastSquares(statObject, component)
        End If
    End Function

    ''' <summary>
    ''' Fisher-Yates 洗牌，返回 <paramref name="y"/> 的一个随机重排副本。
    ''' </summary>
    Private Function Permute(y As Double(), rnd As Random) As Double()
        Dim copy As Double() = DirectCast(y.Clone(), Double())

        For i As Integer = copy.Length - 1 To 1 Step -1
            Dim j As Integer = rnd.Next(i + 1)
            Dim tmp As Double = copy(i)

            copy(i) = copy(j)
            copy(j) = tmp
        Next

        Return copy
    End Function

    ''' <summary>
    ''' 单侧上尾 p 值，加 1 修正避免出现 p = 0 的伪精确结果。
    ''' </summary>
    Private Function UpperTailPValue(values As Double(), reference As Double) As Double
        Dim valid As Integer = 0
        Dim hits As Integer = 0

        For Each v As Double In values
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                Continue For
            End If

            valid += 1

            If v >= reference Then
                hits += 1
            End If
        Next

        If valid = 0 Then
            Return Double.NaN
        End If

        Return (hits + 1) / (valid + 1)
    End Function

    ''' <summary>
    ''' 最小二乘直线拟合 <c>y = slope * x + intercept</c>，同时给出回归的决定系数。
    ''' 任何一侧的非法值（NaN/Infinity）会成对剔除。
    ''' </summary>
    Private Sub FitLine(x As Double(), y As Double(), ByRef slope As Double, ByRef intercept As Double, ByRef r2 As Double)
        slope = Double.NaN
        intercept = Double.NaN
        r2 = Double.NaN

        If x Is Nothing OrElse y Is Nothing OrElse x.Length <> y.Length Then
            Return
        End If

        Dim n As Integer = 0
        Dim sumX#, sumY#, sumXX#, sumXY#, sumYY#

        For i As Integer = 0 To x.Length - 1
            If IsInvalid(x(i)) OrElse IsInvalid(y(i)) Then
                Continue For
            End If

            n += 1
            sumX += x(i)
            sumY += y(i)
            sumXX += x(i) * x(i)
            sumXY += x(i) * y(i)
            sumYY += y(i) * y(i)
        Next

        If n < 2 Then
            Return
        End If

        Dim denominator As Double = n * sumXX - sumX * sumX

        If std.Abs(denominator) < 0.0000000001 Then
            ' 所有 x 相同：只能给出水平线
            slope = 0
            intercept = sumY / n
            Return
        End If

        slope = (n * sumXY - sumX * sumY) / denominator
        intercept = (sumY - slope * sumX) / n

        Dim ssTot As Double = sumYY - sumY * sumY / n
        Dim ssReg As Double = slope * (sumXY - sumX * sumY / n)

        If ssTot > 0 Then
            r2 = ssReg / ssTot
        End If
    End Sub

    ''' <summary>
    ''' Pearson 相关系数，任一侧方差为零（或者长度不匹配）时返回 <see cref="Double.NaN"/>。
    ''' </summary>
    Private Function Pearson(x As Double(), y As Double()) As Double
        If x Is Nothing OrElse y Is Nothing OrElse x.Length <> y.Length OrElse x.Length < 2 Then
            Return Double.NaN
        End If

        Dim n As Integer = x.Length
        Dim meanX As Double = 0
        Dim meanY As Double = 0

        For i As Integer = 0 To n - 1
            meanX += x(i)
            meanY += y(i)
        Next

        meanX /= n
        meanY /= n

        Dim sxy#, sxx#, syy#

        For i As Integer = 0 To n - 1
            Dim dx As Double = x(i) - meanX
            Dim dy As Double = y(i) - meanY

            sxy += dx * dy
            sxx += dx * dx
            syy += dy * dy
        Next

        If sxx <= 0 OrElse syy <= 0 Then
            Return Double.NaN
        End If

        Return sxy / std.Sqrt(sxx * syy)
    End Function

    Private Function IsInvalid(value As Double) As Boolean
        Return Double.IsNaN(value) OrElse Double.IsInfinity(value)
    End Function

    Private Function Invalid(result As PermutationTestResult, reason As String) As PermutationTestResult
        result.IsValid = False
        result.InvalidReason = reason

        Return result
    End Function

End Module
