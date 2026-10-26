Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Statistics
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
' the hypothesis test module is named 't' and its alternative-hypothesis enum is named
' 'Hypothesis', so both are aliased to keep the call sites readable
Imports TTest = Microsoft.VisualBasic.Math.Statistics.Hypothesis.t
Imports AltHypothesis = Microsoft.VisualBasic.Math.Statistics.Hypothesis.Hypothesis

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
    Public Function mean(x As Double()) As Double
        If x Is Nothing OrElse x.Length = 0 Then Return Double.NaN

        Dim sum As Double = 0

        For i As Integer = 0 To x.Length - 1
            sum += x(i)
        Next

        Return sum / x.Length
    End Function

    ''' <summary>求和。</summary>
    Public Function sum(x As Double()) As Double
        Dim acc As Double = 0

        If x Is Nothing Then Return acc

        For i As Integer = 0 To x.Length - 1
            acc += x(i)
        Next

        Return acc
    End Function

    ''' <summary>最小值。</summary>
    Public Function min(x As Double()) As Double
        Return If(x Is Nothing OrElse x.Length = 0, Double.NaN, x.Min)
    End Function

    ''' <summary>最大值。</summary>
    Public Function max(x As Double()) As Double
        Return If(x Is Nothing OrElse x.Length = 0, Double.NaN, x.Max)
    End Function

    ''' <summary>
    ''' 方差；<paramref name="sample"/> 为 True 时按样本口径（n-1）计算。
    ''' </summary>
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
    Public Function sd(x As Double(), Optional sample As Boolean = True) As Double
        Return System.Math.Sqrt(variance(x, sample))
    End Function

    ''' <summary>中位数。</summary>
    Public Function median(x As Double()) As Double
        Return quantile(x, 0.5)
    End Function

    ''' <summary>
    ''' 分位数（线性插值口径，与 R 的 type = 7 一致）。
    ''' </summary>
    ''' <param name="p">分位点，取值 [0, 1]</param>
    Public Function quantile(x As Double(), p As Double) As Double
        If x Is Nothing OrElse x.Length = 0 Then Return Double.NaN
        If p <= 0 Then Return min(x)
        If p >= 1 Then Return max(x)

        Dim ordered As Double() = x.OrderBy(Function(v) v).ToArray
        Dim h As Double = (ordered.Length - 1) * p
        Dim lo As Integer = CInt(System.Math.Floor(h))
        Dim hi As Integer = CInt(System.Math.Ceiling(h))

        Return ordered(lo) + (h - lo) * (ordered(hi) - ordered(lo))
    End Function

    ''' <summary>皮尔逊相关系数。</summary>
    Public Function cor(x As Double(), y As Double()) As Double
        Return SpecialFunctions.Correlation(x, y)
    End Function

    ''' <summary>协方差。</summary>
    Public Function cov(x As Double(), y As Double()) As Double
        Return SpecialFunctions.Covariance(x, y)
    End Function

    ''' <summary>
    ''' 一组数据的描述统计摘要（count / min / max / mean / median / sd / variance）。
    ''' </summary>
    Public Function describe(x As Double()) As Dictionary(Of String, Double)
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
    Public Function t_test(x As Double(), y As Double(), Optional alpha As Double = 0.05) As TwoSampleResult
        Return TTest.Test(x, y, alternative:=AltHypothesis.TwoSided, alpha:=alpha)
    End Function

    ''' <summary>单样本 t 检验（检验均值是否等于 <paramref name="mu"/>）。</summary>
    Public Function t_test_one(x As Double(), Optional mu As Double = 0, Optional alpha As Double = 0.05) As TtestResult
        Return TTest.Test(x, alternative:=AltHypothesis.TwoSided, mu:=mu, alpha:=alpha)
    End Function

    ''' <summary>
    ''' 卡方检验；<paramref name="expected"/> 为空时按 2×2 独立性检验计算期望频数。
    ''' </summary>
    Public Function chisq(observed As Double()(), Optional expected As Double()() = Nothing) As ChiSquareTest
        If expected Is Nothing Then
            Return ChiSquareTest.Test(observed)
        End If

        Return ChiSquareTest.Test(observed, expected)
    End Function
End Module
