#Region "Microsoft.VisualBasic::d10830366e27aa92dd630ed96b543a7e, Data_science\Mathematica\Math\Math\Extensions.vb"

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

    '   Total Lines: 416
    '    Code Lines: 232 (55.77%)
    ' Comment Lines: 133 (31.97%)
    '    - Xml Docs: 91.73%
    ' 
    '   Blank Lines: 51 (12.26%)
    '     File Size: 15.52 KB


    ' Module Extensions
    ' 
    '     Function: [Shadows], AsSample, (+4 Overloads) AsVector, BHCorrection, DoubleRange
    '               (+2 Overloads) FDR, FilterNaN, FirstDecrease, FirstIncrease, FlipCoin
    '               ImputeNA, IntRange, IsInside, Iterates, (+2 Overloads) Range
    '               Reach, seq2, Sim, SSM, SSM_SIMD
    '               Tanimoto, X, Y
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Distributions.Summary
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Statistics
Imports std = System.Math

' i++
' Math.Min(i+=1, i - 1)

''' <summary>
''' 向量以及统计函数拓展
''' </summary>
<HideModuleName> Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsSample(data As IEnumerable(Of Double)) As SampleDistribution
        Return New SampleDistribution(data)
    End Function

    ''' <summary>
    ''' Create the vector model from target .NET object collection.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function [Shadows](Of T)(source As IEnumerable(Of T)) As IVector(Of T)
        Return New IVector(Of T)(source)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="headsCutoff">这个参数用来调整事件的发生概率，这个参数值越小，事件越容易发生</param>
    ''' <param name="ntimes%"></param>
    ''' <returns></returns>
    Public Function FlipCoin(Optional headsCutoff% = 50, Optional ntimes% = 100) As Boolean
        Dim rand As Integer = randf(0, ntimes)

        If rand >= headsCutoff Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' 计算两个离散信号之间的相似度
    ''' </summary>
    ''' <param name="q"></param>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SSM(q As Vector, s As Vector) As Double
        If q.All(Function(a) a = 0.0R) OrElse s.All(Function(a) a = 0.0R) Then
            Return 0
        Else
            Dim score As Double = (q * s).Sum / std.Sqrt((q ^ 2).Sum * (s ^ 2).Sum)

            If score.IsNaNImaginary Then
                Return 0
            Else
                Return score
            End If
        End If
    End Function

    ''' <summary>
    ''' SIMD version of the modified cosine score
    ''' </summary>
    ''' <param name="q"></param>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Public Function SSM_SIMD(q As Double(), s As Double()) As Double
        If q.All(Function(a) a = 0.0R) OrElse s.All(Function(a) a = 0.0R) Then
            Return 0.0
        End If

        Dim qs_sum As Double = SIMD.Multiply.f64_op_multiply_f64(q, s).Sum
        Dim qqss_sum As Double = SIMD.Multiply.f64_op_multiply_f64(q, q).Sum * SIMD.Multiply.f64_op_multiply_f64(s, s).Sum
        Dim score As Double = qs_sum / std.Sqrt(qqss_sum)

        If score.IsNaNImaginary Then
            Return 0
        Else
            Return score
        End If
    End Function

    ''' <summary>
    ''' Construct the <see cref="Vector"/> class from a numeric collecton.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsVector(data As IEnumerable(Of Double)) As Vector
        Return New Vector(data)
    End Function

    ''' <summary>
    ''' Create a <see cref="Vector"/> from a specific <see cref="Integer"/> abstract vector.
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsVector(v As Vector(Of Integer)) As Vector
        Return v.Select(Function(i) CDbl(i)).AsVector
    End Function

    ''' <summary>
    ''' Create a <see cref="Vector"/> from a specific numeric collection.
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsVector(v As Vector(Of Double)) As Vector
        Return v.Select(Function(x) x).AsVector
    End Function

    ''' <summary>
    ''' Create a <see cref="Vector"/> from a given subset of the dynamics object property values.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="keys$"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsVector(data As DynamicPropertyBase(Of Double), keys$()) As Vector
        Return data.ItemValue(keys).AsVector
    End Function

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

    ''' <summary>
    ''' Tuple range iterates
    ''' </summary>
    ''' <param name="range">Number values iterates from value ``from`` to value ``to``.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' step 1 or -1 based on the to - from delta value its sign symbol.
    ''' </remarks>
    <Extension>
    Public Iterator Function Iterates(range As (From%, To%)) As IEnumerable(Of Integer)
        Dim step% = std.Sign(range.To - range.From)

        For i As Integer = range.From To range.To Step [step]
            Yield i
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Range(data As IEnumerable(Of Double)) As (min#, max#)
        With data.ToArray
            Return (.Min, .Max)
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Range(data As IEnumerable(Of Integer)) As (min#, max#)
        With data.ToArray
            Return (.Min, .Max)
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function IntRange(range As (From%, To%)) As IntRange
        With range
            Return New IntRange(.From, .To)
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DoubleRange(range As (From#, To#)) As DoubleRange
        With range
            Return New DoubleRange(.From, .To)
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function IsInside(vector As Vector, range As DoubleRange) As BooleanVector
        Return vector.Select(Function(d) range.IsInside(d)).ToVector
    End Function

    ''' <summary>
    ''' 返回数值序列之中的首次出现符合条件的减少的位置
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="ratio"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstDecrease(data As IEnumerable(Of Double), Optional ratio As Double = 10) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x As SeqValue(Of Double) In data.SeqIterator
            Dim d = (pre - x.value)

            If d / pr > ratio Then
                Return x.i
            Else
                pr = d
                pre = x.value
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    ''' <summary>
    ''' 只对单调递增的那一部分曲线有效
    ''' </summary>
    ''' <param name="data">y值</param>
    ''' <param name="alpha"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FirstIncrease(data As IEnumerable(Of Double), dx As Double, Optional alpha As Double = 30) As Integer
        Dim pre As Double = data.First
        Dim pr As Double = 1000000

        For Each x As SeqValue(Of Double) In data _
            .Skip(1) _
            .SeqIterator(offset:=1)

            Dim dy = (x.value - pre) ' 对边
            Dim tanX As Double = dy / dx
            Dim a As Double = Atn(tanX)

            If a >= alpha Then
                Return x.i
            End If
        Next

        Return -1 ' 没有找到符合条件的点
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="n"></param>
    ''' <param name="offset">距离目标数据点<paramref name="n"/>的正负偏移量</param>
    ''' <returns></returns>
    <Extension>
    Public Function Reach(data As IEnumerable(Of Double), n As Double, Optional offset As Double = 0) As Integer
        For Each x As SeqValue(Of Double) In data.SeqIterator
            If std.Abs(x.value - n) <= offset Then
                Return x.i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' [Sequence Generation] Generate regular sequences. seq is a standard generic with a default method.
    ''' </summary>
    ''' <param name="From">
    ''' the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
    ''' </param>
    ''' <param name="To">
    ''' the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
    ''' </param>
    ''' <param name="By">number: increment of the sequence</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function seq2(from#, to#, Optional by# = 0.1) As Vector
        Return New Vector(seq(from, [to], by))
    End Function

    ''' <summary>
    ''' 余弦相似度
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Sim(x As Vector, y As Vector) As Double
        Return (x * y).Sum / (x.Mod * y.Mod)
    End Function

    ''' <summary>
    ''' 这是x和y所共有的属性个数与x或y所具有的属性个数之间的比率。这个函数被称为Tanimoto系数或Tanimoto距离，
    ''' 它经常用在信息检索和生物学分类中。(余弦度量的一个简单的变种)
    ''' 当属性是二值属性时，余弦相似性函数可以用共享特征或属性解释。假设如果xi=1，则对象x具有第i个属性。于是，
    ''' x·y是x和y共同具有的属性数，而xy是x具有的属性数与y具有的属性数的几何均值。于是，sim(x,y)是公共属性相
    ''' 对拥有的一种度量。
    ''' </summary>
    ''' <param name="x">vector of elements of 0 or 1 binary data</param>
    ''' <param name="y">vector of elements of 0 or 1 binary data</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' http://xiao5461.blog.163.com/blog/static/22754562201211237567238/
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Tanimoto(x As Vector, y As Vector) As Double
        Return (x * y).Sum / ((x * x).Sum + (y * y).Sum - (x * y).Sum)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Y(points As IEnumerable(Of PointF)) As Vector
        Return points.Select(Function(pt) CDbl(pt.Y)).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function X(points As IEnumerable(Of PointF)) As Vector
        Return points.Select(Function(pt) CDbl(pt.X)).AsVector
    End Function

    <Extension>
    Public Function FilterNaN(ByRef m As Double()(), replace As Double) As Double()()
        For Each xi As Double() In m
            For i As Integer = 0 To xi.Length - 1
                If xi(i).IsNaNImaginary Then
                    xi(i) = replace
                End If
            Next
        Next

        Return m
    End Function

    <Extension>
    Public Function ImputeNA(v As Vector, fill_as As Double) As Vector
        Return New Vector(From xi As Double In v.Array Select If(xi.IsNaNImaginary, fill_as, xi))
    End Function
End Module
