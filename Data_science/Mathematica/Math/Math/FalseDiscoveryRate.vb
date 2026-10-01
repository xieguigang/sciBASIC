Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics
Imports std = System.Math

Public Module FalseDiscoveryRate

    ''' <summary>
    ''' FDR错误控制法是Benjamini于1995年提出一种方法,通过控制FDR(False Discovery Rate)来决定P值的域值. 
    ''' 假设你挑选了``R``个差异表达的基因，其中有``S``个是真正有差异表达的，另外有``V``个其实是没有差异表达的，是假阳性的。
    ''' 实践中希望错误比例``Q＝V/R``平均而言不能超过某个预先设定的值（比如0.05），在统计学上，
    ''' 这也就等价于控制FDR不能超过5％.
    ''' 
    ''' 对所有候选基因的p值进行从小到大排序，则若想控制fdr不能超过q，则只需找到最大的正整数i，使得 
    ''' ``p(i)&lt;= (i*q)/m``.然后，挑选对应p(1),p(2),...,p(i)的基因做为差异表达基因，这样就能从统计学上
    ''' 保证fdr不超过q。因此，FDR的计算公式如下
    ''' 
    ''' ``FDR = length(pvalue)*pvalue/rank(pvalue)``
    ''' </summary>
    ''' <param name="pvalue"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FDR(pvalue As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Vector
        Dim x As New Vector(pvalue)
        Dim fdr_result = (If(n, x.Dim) * x) / x.FractionalRanking

        Return fdr_result
    End Function

    <Extension>
    Public Iterator Function FDR(Of T As IStatFDR)(result As IEnumerable(Of T)) As IEnumerable(Of T)
        Dim sortPval As T() = result.OrderBy(Function(a) a.pValue).ToArray
        Dim fdrVal As Vector = sortPval.Select(Function(a) a.pValue).FDR

        For i As Integer = 0 To sortPval.Length - 1
            sortPval(i).adjPVal = fdrVal(i)
            Yield sortPval(i)
        Next
    End Function

    ''' <summary>
    ''' Benjamini-Hochberg FDR校正
    ''' <para>控制错误发现率 (False Discovery Rate)</para>
    ''' </summary>
    ''' <param name="pValues">原始p值数组</param>
    ''' <returns>校正后的q值数组 (与输入顺序一致)</returns>
    Public Function BHCorrection(pValues As IEnumerable(Of Double)) As Double()
        Dim pList As Double() = pValues.ToArray()
        Dim n As Integer = pList.Length
        If n = 0 Then Return New Double(-1) {}

        ' 创建 (p值, 原始索引) 对, 按p值升序排列
        Dim indexed = pList.Select(Function(p, i) (p, i)).OrderBy(Function(x) x.Item1).ToArray()

        Dim q(n - 1) As Double
        Dim prevQ As Double = 1.0

        ' 从大到小遍历, 保证单调性: q(i) = min(q(i+1), p(i) * n / rank(i))
        For idx As Integer = n - 1 To 0 Step -1
            Dim rank As Integer = idx + 1 ' 1-indexed rank
            Dim raw As Double = indexed(idx).Item1 * CDbl(n) / CDbl(rank)
            ' 取与后一个q值的最小值, 保证单调不减
            prevQ = std.Min(prevQ, raw)
            ' 上限为1
            prevQ = std.Min(1.0, prevQ)
            q(idx) = prevQ
        Next

        ' 将q值放回原始顺序
        Dim result(n - 1) As Double
        For idx As Integer = 0 To n - 1
            result(indexed(idx).Item2) = q(idx)
        Next

        Return result
    End Function

    ''' <summary>
    ''' 多重检验校正方法枚举，对应 R 的 ``p.adjust.methods``。
    ''' </summary>
    Public Enum PValueAdjustMethod
        Holm
        Hochberg
        Hommel
        Bonferroni
        BH
        BY
        FDR
        None
    End Enum

    ''' <summary>
    ''' Bonferroni 多重检验校正：``p_adj = min(1, n * p)``。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    Public Function BonferroniCorrection(pValues As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Double()
        Dim p As Double() = pValues.ToArray()
        Dim m As Integer = If(n, p.Length)
        Dim ret As Double() = New Double(p.Length - 1) {}

        For i As Integer = 0 To p.Length - 1
            ret(i) = std.Min(1.0, m * p(i))
        Next

        Return ret
    End Function

    ''' <summary>
    ''' Holm 步降（step-down）多重检验校正。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    ''' <remarks>对应 R ``p.adjust(method = "holm")``。</remarks>
    Public Function HolmCorrection(pValues As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Double()
        Dim p As Double() = pValues.ToArray()
        Dim lp As Integer = p.Length

        If lp = 0 Then Return New Double() {}

        Dim m As Integer = If(n, lp)
        Dim o As Integer() = Enumerable.Range(0, lp).OrderBy(Function(i) p(i)).ToArray()
        Dim qv As Double() = New Double(lp - 1) {}
        Dim running As Double = 0.0

        ' pmin(1, cummax((n + 1 - i) * p[i]))，i 为升序秩
        For k As Integer = 0 To lp - 1
            Dim raw As Double = (m - k) * p(o(k))
            If raw > running Then running = raw
            qv(k) = std.Min(1.0, running)
        Next

        Dim ret As Double() = New Double(lp - 1) {}

        For k As Integer = 0 To lp - 1
            ret(o(k)) = qv(k)
        Next

        Return ret
    End Function

    ''' <summary>
    ''' Hochberg 步升（step-up）多重检验校正。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    ''' <remarks>对应 R ``p.adjust(method = "hochberg")``。</remarks>
    Public Function HochbergCorrection(pValues As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Double()
        Dim p As Double() = pValues.ToArray()
        Dim lp As Integer = p.Length

        If lp = 0 Then Return New Double() {}

        Dim m As Integer = If(n, lp)
        Dim o As Integer() = Enumerable.Range(0, lp).OrderByDescending(Function(i) p(i)).ToArray()
        Dim qv As Double() = New Double(lp - 1) {}
        Dim running As Double = Double.PositiveInfinity

        ' pmin(1, cummin((n + 1 - i) * p[i]))，i 为降序秩
        For k As Integer = 0 To lp - 1
            Dim raw As Double = (m - lp + 1 + k) * p(o(k))
            If raw < running Then running = raw
            qv(k) = std.Min(1.0, running)
        Next

        Dim ret As Double() = New Double(lp - 1) {}

        For k As Integer = 0 To lp - 1
            ret(o(k)) = qv(k)
        Next

        Return ret
    End Function

    ''' <summary>
    ''' Benjamini-Hochberg FDR 校正（支持检验总数 ``n &gt;= length(p)``）。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    ''' <remarks>对应 R ``p.adjust(method = "BH")``。</remarks>
    Public Function BenjaminiHochbergCorrection(pValues As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Double()
        Dim p As Double() = pValues.ToArray()
        Dim lp As Integer = p.Length

        If lp = 0 Then Return New Double() {}

        Dim m As Integer = If(n, lp)
        Dim o As Integer() = Enumerable.Range(0, lp).OrderByDescending(Function(i) p(i)).ToArray()
        Dim qv As Double() = New Double(lp - 1) {}
        Dim running As Double = Double.PositiveInfinity

        ' pmin(1, cummin(n / i * p[i]))，i 为降序秩
        For k As Integer = 0 To lp - 1
            Dim raw As Double = m / CDbl(lp - k) * p(o(k))
            If raw < running Then running = raw
            qv(k) = std.Min(1.0, running)
        Next

        Dim ret As Double() = New Double(lp - 1) {}

        For k As Integer = 0 To lp - 1
            ret(o(k)) = qv(k)
        Next

        Return ret
    End Function

    ''' <summary>
    ''' Benjamini-Yekutieli FDR 校正（依赖条件下的 FDR 控制）。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    ''' <remarks>
    ''' 对应 R ``p.adjust(method = "BY")``，其中 ``q = Σ_{k=1..n} 1/k``。
    ''' </remarks>
    Public Function BenjaminiYekutieliCorrection(pValues As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Double()
        Dim p As Double() = pValues.ToArray()
        Dim lp As Integer = p.Length

        If lp = 0 Then Return New Double() {}

        Dim m As Integer = If(n, lp)
        Dim q As Double = 0.0

        For k As Integer = 1 To m
            q += 1.0 / k
        Next

        Dim o As Integer() = Enumerable.Range(0, lp).OrderByDescending(Function(i) p(i)).ToArray()
        Dim qv As Double() = New Double(lp - 1) {}
        Dim running As Double = Double.PositiveInfinity

        For k As Integer = 0 To lp - 1
            Dim raw As Double = q * m / CDbl(lp - k) * p(o(k))
            If raw < running Then running = raw
            qv(k) = std.Min(1.0, running)
        Next

        Dim ret As Double() = New Double(lp - 1) {}

        For k As Integer = 0 To lp - 1
            ret(o(k)) = qv(k)
        Next

        Return ret
    End Function

    ''' <summary>
    ''' Hommel 多重检验校正。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    ''' <remarks>
    ''' 严格按照 R ``p.adjust(method = "hommel")`` 的实现移植
    ''' （Gordon Smyth 版本）。当 ``n == 2`` 时后退为 Hochberg 方法。
    ''' </remarks>
    Public Function HommelCorrection(pValues As IEnumerable(Of Double), Optional n As Integer? = Nothing) As Double()
        Dim p As Double() = pValues.ToArray()
        Dim lp As Integer = p.Length

        If lp = 0 Then Return New Double() {}

        Dim m As Integer = If(n, lp)

        If m <= 1 Then Return p
        If m = 2 Then Return HochbergCorrection(p, n)

        ' 当 n > lp 时用 1 补齐
        Dim ps0 As Double() = New Double(m - 1) {}

        For i As Integer = 0 To m - 1
            ps0(i) = If(i < lp, p(i), 1.0)
        Next

        Dim o As Integer() = Enumerable.Range(0, m).OrderBy(Function(i) ps0(i)).ToArray()
        Dim ps As Double() = o.Select(Function(i) ps0(i)).ToArray()
        Dim ro As Integer() = New Integer(m - 1) {}

        For k As Integer = 0 To m - 1
            ro(o(k)) = k
        Next

        ' q = pa = min_i (n * p[i] / i)
        Dim minVal As Double = Double.PositiveInfinity

        For i As Integer = 1 To m
            Dim v As Double = m * ps(i - 1) / i
            If v < minVal Then minVal = v
        Next

        Dim q As Double() = New Double(m - 1) {}
        Dim pa As Double() = New Double(m - 1) {}

        For i As Integer = 0 To m - 1
            q(i) = minVal
            pa(i) = minVal
        Next

        For j As Integer = m - 1 To 2 Step -1
            Dim nij As Integer = m - j + 1
            Dim q1 As Double = Double.PositiveInfinity

            ' i2 = (n-j+2):n 与除数 2:j
            For k As Integer = 2 To j
                Dim idx As Integer = m - j + k - 1
                Dim v As Double = j * ps(idx) / k
                If v < q1 Then q1 = v
            Next

            For t As Integer = 0 To nij - 1
                q(t) = std.Min(j * ps(t), q1)
            Next

            For t As Integer = nij To m - 1
                q(t) = q(nij - 1)
            Next

            For t As Integer = 0 To m - 1
                pa(t) = std.Max(pa(t), q(t))
            Next
        Next

        Dim finalSorted As Double() = New Double(m - 1) {}

        For k As Integer = 0 To m - 1
            finalSorted(k) = std.Max(pa(k), ps(k))
        Next

        Dim ret As Double() = New Double(lp - 1) {}

        For j As Integer = 0 To lp - 1
            ret(j) = finalSorted(ro(j))
        Next

        Return ret
    End Function

    ''' <summary>
    ''' 通用的 p 值多重检验校正入口，对应 R 的 ``p.adjust``。
    ''' </summary>
    ''' <param name="pValues">原始 p 值。</param>
    ''' <param name="method">校正方法。</param>
    ''' <param name="n">检验总数；缺省为 p 值个数。</param>
    ''' <returns>校正后的 p 值（保持输入顺序）。</returns>
    Public Function PValueAdjust(pValues As IEnumerable(Of Double),
                                 Optional method As PValueAdjustMethod = PValueAdjustMethod.FDR,
                                 Optional n As Integer? = Nothing) As Double()

        Dim p As Double() = pValues.ToArray()

        Select Case method
            Case PValueAdjustMethod.Holm : Return HolmCorrection(p, n)
            Case PValueAdjustMethod.Hochberg : Return HochbergCorrection(p, n)
            Case PValueAdjustMethod.Hommel : Return HommelCorrection(p, n)
            Case PValueAdjustMethod.Bonferroni : Return BonferroniCorrection(p, n)
            Case PValueAdjustMethod.BH, PValueAdjustMethod.FDR : Return BenjaminiHochbergCorrection(p, n)
            Case PValueAdjustMethod.BY : Return BenjaminiYekutieliCorrection(p, n)
            Case Else : Return p
        End Select
    End Function
End Module
