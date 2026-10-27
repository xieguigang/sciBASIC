Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Math.Statistics
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports AltHypothesis = Microsoft.VisualBasic.Math.Statistics.Hypothesis.Hypothesis
' the hypothesis test module is named 't' and its alternative-hypothesis enum is named
' 'Hypothesis', so both are aliased to keep the call sites readable
Imports TTest = Microsoft.VisualBasic.Math.Statistics.Hypothesis.t
Imports std = System.Math
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Distributions.Summary

''' <summary>
''' 描述统计与假设检验模块。
''' <para>
''' 所有入口都直接接收脚本里的数值数组（<c>Double()</c>），
''' 因此 VBS 与 NJS 都可以把 <c>table.feature("x")</c> 的结果直接传进来。
''' </para>
''' </summary>
Public Module stats

    ' ======================================================== 描述统计

    ''' <summary>算术平均值。</summary>
    ''' 
    <Extension>
    Public Function mean(x As IEnumerable(Of Double)) As Double
        If x Is Nothing Then
            Return 0
        Else
            Dim vx As Double() = x.ToArray

            If vx.Length = 0 Then
                Return 0
            Else
                Return SimdReduce.Mean(vx)
            End If
        End If
    End Function

    ''' <summary>求和。</summary>
    ''' 
    <Extension>
    Public Function sum(x As IEnumerable(Of Double)) As Double
        Return If(x Is Nothing, 0, SimdReduce.Sum(x.ToArray))
    End Function

    ''' <summary>最小值。</summary>
    ''' 
    <Extension>
    Public Function min(x As IEnumerable(Of Double)) As Double
        If x Is Nothing Then
            Return Double.NaN
        Else
            Dim vx As Double() = x.ToArray

            If vx.Length = 0 Then
                Return Double.NaN
            Else
                Return SimdReduce.Min(vx)
            End If
        End If
    End Function

    ''' <summary>最大值。</summary>
    ''' 
    <Extension>
    Public Function max(x As IEnumerable(Of Double)) As Double
        If x Is Nothing Then
            Return Double.NaN
        Else
            Dim vx As Double() = x.ToArray

            If vx.Length = 0 Then
                Return Double.NaN
            Else
                Return SimdReduce.Max(vx)
            End If
        End If
    End Function

    ''' <summary>
    ''' 方差；<paramref name="sample"/> 为 True 时按样本口径（n-1）计算。
    ''' </summary>
    ''' 
    <Extension>
    Public Function variance(x As Double(), Optional sample As Boolean = True) As Double
        If x Is Nothing OrElse x.Length < 2 Then Return Double.NaN

        Dim avg As Double = mean(x)
        Dim acc As Double = 0

        For i As Integer = 0 To x.Length - 1
            acc += (x(i) - avg) ^ 2
        Next

        Return acc / (x.Length - If(sample, 1, 0))
    End Function

    ''' <summary>标准差；口径与 <see cref="variance"/> 一致。</summary>
    ''' 
    <Extension>
    Public Function sd(x As Double(), Optional sample As Boolean = True) As Double
        Return std.Sqrt(variance(x, sample))
    End Function

    ''' <summary>中位数。</summary>
    ''' 
    <Extension>
    Public Function median(x As Double()) As Double
        Return quantile(x, 0.5)
    End Function

    ''' <summary>
    ''' 分位数（线性插值口径，与 R 的 type = 7 一致）。
    ''' </summary>
    ''' <param name="p">分位点，取值 [0, 1]</param>
    ''' 
    <Extension>
    Public Function quantile(x As Double(), p As Double) As Double
        Return x.LinearQuantile(p)
    End Function

    ''' <summary>皮尔逊相关系数（按定义实现，避免底层实现的统计口径差异）。</summary>
    Public Function cor(x As IEnumerable(Of Double), y As IEnumerable(Of Double)) As Double
        Return Correlations.GetPearson(x.ToNullArray, y.ToNullArray)
    End Function

    ''' <summary>协方差；<paramref name="sample"/> 为 True 时按样本口径（n-1）计算。</summary>
    Public Function cov(x As Double(), y As Double(), Optional sample As Boolean = True) As Double
        If x Is Nothing OrElse y Is Nothing OrElse x.Length <> y.Length OrElse x.Length < 2 Then
            Return Double.NaN
        End If

        Dim terms As (sxy As Double, sxx As Double, syy As Double) = Deviations(x, y)

        Return terms.sxy / (x.Length - If(sample, 1, 0))
    End Function

    ''' <summary>离差平方和/交叉积。</summary>
    Private Function Deviations(x As Double(), y As Double()) As (sxy As Double, sxx As Double, syy As Double)
        If x Is Nothing OrElse y Is Nothing OrElse x.Length <> y.Length OrElse x.Length = 0 Then
            Return (Double.NaN, Double.NaN, Double.NaN)
        End If

        Dim avgX As Double = mean(x)
        Dim avgY As Double = mean(y)
        Dim sxy As Double = 0
        Dim sxx As Double = 0
        Dim syy As Double = 0

        For i As Integer = 0 To x.Length - 1
            Dim dx As Double = x(i) - avgX
            Dim dy As Double = y(i) - avgY

            sxy += dx * dy
            sxx += dx * dx
            syy += dy * dy
        Next

        Return (sxy, sxx, syy)
    End Function

    ''' <summary>
    ''' 一组数据的描述统计摘要（count / min / max / mean / median / sd / variance）。
    ''' </summary>
    ''' 
    <Extension>
    Public Function describe(x As IEnumerable(Of Double)) As Dictionary(Of String, Double)
        Dim sample As New SampleDistribution(x)
        Dim d As New Dictionary(Of String, Double) From {
            {"count", If(x Is Nothing, 0, x.Length)},
            {"min", min(x)},
            {"max", max(x)},
            {"mean", mean(x)},
            {"median", median(x)},
            {"sd", sd(x)},
            {"variance", variance(x)}
        }

        Return d
    End Function

    ''' <summary>
    ''' 表格每个特征列的描述统计摘要（行 = 统计量，列 = 特征），返回新表。
    ''' </summary>
    Public Function summary(source As NumericTable) As NumericTable
        Dim rows As New List(Of Double())
        Dim statsName As String() = {"count", "min", "max", "mean", "median", "sd", "variance"}

        For Each featureName As String In source.featureNames
            Dim values As Double() = source.Feature(featureName)
            Dim d As Dictionary(Of String, Double) = describe(values)
            Dim row(statsName.Length - 1) As Double

            For i As Integer = 0 To statsName.Length - 1
                row(i) = d(statsName(i))
            Next

            rows.Add(row)
        Next

        Return NumericTable.FromRows(source.featureNames, rows.ToArray, statsName)
    End Function

    ' ======================================================== 假设检验

    ''' <summary>
    ''' 双样本 t 检验（默认双侧、方差不齐）。
    ''' </summary>
    Public Function t_test(x As Double(), y As Double(), Optional alpha As Double = 0.05) As TestResult
        Dim result As TwoSampleResult = TTest.Test(x, y, alternative:=AltHypothesis.TwoSided, alpha:=alpha)

        Return New TestResult With {
            .test = "two.sample",
            .statistic = result.TestValue,
            .pvalue = result.Pvalue,
            .df = result.DegreeFreedom,
            .mean = result.Mean,
            .meanX = result.MeanX,
            .meanY = result.MeanY,
            .alpha = alpha
        }
    End Function

    ''' <summary>单样本 t 检验（检验均值是否等于 <paramref name="mu"/>）。</summary>
    Public Function t_test_one(x As Double(), Optional mu As Double = 0, Optional alpha As Double = 0.05) As TestResult
        Dim result As TtestResult = TTest.Test(x, alternative:=AltHypothesis.TwoSided, mu:=mu, alpha:=alpha)

        Return New TestResult With {
            .test = "one.sample",
            .statistic = result.TestValue,
            .pvalue = result.Pvalue,
            .df = result.DegreeFreedom,
            .mean = result.Mean,
            .alpha = alpha
        }
    End Function

    ''' <summary>
    ''' 卡方检验；<paramref name="expected"/> 为空时按 2×2 独立性检验计算期望频数。
    ''' </summary>
    Public Function chisq(observed As Double()(), Optional expected As Double()() = Nothing) As ChiSquareResult
        Dim result As ChiSquareTest = If(expected Is Nothing, ChiSquareTest.Test(observed), ChiSquareTest.Test(observed, expected))
        Dim rows As Integer = result.observed.Length
        Dim cols As Integer = result.observed(0).Length

        Return New ChiSquareResult With {
            .chiSquare = result.chi_square,
            .pvalue = result.pvalue,
            .df = (rows - 1) * (cols - 1),
            .observed = result.observed,
            .expected = result.expected
        }
    End Function
End Module
