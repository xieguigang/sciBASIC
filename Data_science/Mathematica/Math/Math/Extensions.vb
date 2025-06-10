#Region "Microsoft.VisualBasic::4a9aba5fe9198b7d64e9a52939eda1f5, Data_science\Mathematica\Math\Math\Extensions.vb"

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

    '   Total Lines: 349
    '    Code Lines: 187 (53.58%)
    ' Comment Lines: 122 (34.96%)
    '    - Xml Docs: 95.08%
    ' 
    '   Blank Lines: 40 (11.46%)
    '     File Size: 12.95 KB


    ' Module Extensions
    ' 
    '     Function: [Shadows], AsSample, (+4 Overloads) AsVector, DoubleRange, FDR
    '               FirstDecrease, FirstIncrease, FlipCoin, IntRange, IsInside
    '               Iterates, (+2 Overloads) Range, Reach, seq2, Sim
    '               SSM, SSM_SIMD, Tanimoto, X, Y
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
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting
Imports std = System.Math

' i++
' Math.Min(Threading.Interlocked.Increment(i), i - 1)

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
    ''' <param name="x"></param>
    ''' <param name="y"></param>
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
End Module
