#Region "Microsoft.VisualBasic::6265426c834056129bea220c05049bb0, Data_science\Mathematica\Math\Math\Bootstraping.vb"

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

    ' Module Bootstraping
    ' 
    '     Function: [Shadows], AboveStandardDistribution, BelowStandardDistribution, BetweenStandardDistribution, DeviationStandardization
    '               Distributes, Hist, Logistic, OutsideStandardDistribution, (+2 Overloads) ProbabilityDensity
    '               Sample, (+2 Overloads) Samples, Sampling, StandardDistribution, TrapezodialRule
    '               TruncNDist, (+2 Overloads) Z
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports sys = System.Math

Namespace Distributions

    ''' <summary>
    ''' Data sampling bootstrapping extensions
    ''' </summary>
    Public Module Bootstraping

        ''' <summary>
        ''' Generate a numeric <see cref="Vector"/> by <see cref="Permutation"/> <paramref name="x"/> times.
        ''' </summary>
        ''' <param name="x%"></param>
        ''' <returns></returns>
        Public Function Sample(x%) As Vector
            Return New Random().Permutation(x, x).AsVector
        End Function

        ''' <summary>
        ''' bootstrap是一种非参数估计方法，它用到蒙特卡洛方法。bootstrap算法如下：
        ''' 假设样本容量为N
        '''
        ''' + 有放回的从样本中随机抽取N次(所以可能x1..xn中有的值会被抽取多次)，每次抽取一个元素。并将抽到的元素放到集合S中；
        ''' + 重复**步骤1** B次（例如``B = 100``）， 得到B个集合， 记作S1, S2,…, SB;
        ''' + 对每个``Si(i=1, 2, ..., B)``，用蒙特卡洛方法估计随机变量的数字特征d，分别记作d1,d2,…,dB;
        ''' + 用d1,d2,…dB来近似d的分布；
        ''' 
        ''' 本质上，bootstrap算法是最大似然估计的一种实现，它和最大似然估计相比的优点在于，它不需要用参数来刻画总体分布。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="N"></param>
        ''' <param name="B"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Samples(Of T)(source As IEnumerable(Of T), N As Integer, Optional B As Integer = 100) As IEnumerable(Of IntegerTagged(Of T()))
            Dim array As T() = source.ToArray
            Dim rnd As New Random

            For i As Integer = 0 To B
                Dim ls As New List(Of T)

                For k As Integer = 0 To N - 1
                    ls += array(rnd.Next(array.Length))
                Next

                Yield New IntegerTagged(Of T()) With {
                .Tag = i,
                .Value = ls.ToArray
            }
            Next
        End Function

        <Extension>
        Public Iterator Function Sampling(source As IEnumerable(Of Double), N%, Optional B% = 100) As IEnumerable(Of IntegerTagged(Of Vector))
            For Each x As IntegerTagged(Of Double()) In Samples(source, N, B)
                Yield New IntegerTagged(Of Vector) With {
                .Tag = x.Tag,
                .Value = New Vector(x.Value)
            }
            Next
        End Function

        <Extension>
        Public Function Samples(Of T)(source As IEnumerable(Of T), getValue As Func(Of T, Double), N As Integer, Optional B As Integer = 100) As IEnumerable(Of IntegerTagged(Of Vector))
            Return source.Select(getValue).Sampling(N, B)
        End Function

        ''' <summary>
        ''' ###### Z-score 标准化(zero-mean normalization)
        ''' 
        ''' 也叫标准差标准化，经过处理的数据符合标准正态分布，即均值为0，标准差为1
        ''' 其中<paramref name="μ"/>为所有样本数据的均值，<paramref name="σ"/>为所有样本数据的标准差。
        ''' </summary>
        ''' <param name="x#">Sample data</param>
        ''' <param name="μ#">μ is the mean of the population.</param>
        ''' <param name="σ#">σ is the standard deviation of the population.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 注：是否要进行标准化，要根据具体实验定。如果特征非常稀疏，并且有大量的0（现实应用中很多特征都具有这个特点），
        ''' ``Z-score`` 标准化的过程几乎就是一个除0的过程，结果不可预料。
        ''' </remarks>
        Public Function Z#(x#, μ#, σ#)
            Dim answer As Double = (x - μ) / σ
            Return answer
        End Function

        ''' <summary>
        ''' ## Standard score
        ''' 
        ''' In statistics, the standard score is the signed number of standard deviations by which the value of 
        ''' an observation or data point is above the mean value of what is being observed or measured. Observed 
        ''' values above the mean have positive standard scores, while values below the mean have negative 
        ''' standard scores. The standard score is a dimensionless quantity obtained by subtracting the population 
        ''' mean from an individual raw score and then dividing the difference by the population standard deviation. 
        ''' This conversion process is called standardizing or normalizing (however, "normalizing" can refer to 
        ''' many types of ratios; see normalization for more).
        ''' 
        ''' > https://en.wikipedia.org/wiki/Standard_score
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://blog.163.com/huai_jing@126/blog/static/171861983201321074124426/
        ''' </remarks>
        <Extension> Public Function Z(x As Vector) As Vector
            Dim μ# = x.Average ' μ is the mean of the population.
            Dim σ# = x.SD   ' σ is the standard deviation of the population.
            Dim x1 As Vector = (x - μ) / σ
            Return x1
        End Function

        ''' <summary>
        ''' A logistic function or logistic curve is a common "S" shape (sigmoid curve)
        ''' > https://en.wikipedia.org/wiki/Logistic_function
        ''' </summary>
        ''' <param name="L#">the curve's maximum value</param>
        ''' <param name="x#">current x value</param>
        ''' <param name="x0#">the x-value of the sigmoid's midpoint,</param>
        ''' <param name="k#">the steepness of the curve.</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Logistic(L#, x#, x0#, k#) As Double
            Return L / (1 + sys.E ^ (-k * (x - x0)))
        End Function

        ''' <summary>
        ''' ###### 0-1标准化(0-1 normalization)
        ''' 也叫离差标准化，是对原始数据的线性变换，使结果落到[0,1]区间
        ''' 其中max为样本数据的最大值，min为样本数据的最小值。这种方法有一个缺陷就是当有新数据加入时，可能导致max和min的变化，需要重新定义。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 数据的标准化（normalization）是将数据按比例缩放，使之落入一个小的特定区间。这样去除数据的单位限制，
        ''' 将其转化为无量纲的纯数值，便于不同单位或量级的指标能够进行比较和加权。
        ''' 其中最典型的就是0-1标准化和Z标准化
        ''' </remarks>
        <Extension> Public Function DeviationStandardization(x As Vector) As Vector
            Dim max# = x.Max
            Dim min# = x.Min
            Dim x1 As Vector = (x - min) / (max - min)
            Return x1
        End Function

        ''' <summary>
        ''' ###### 频数分布表与直方图
        ''' 
        ''' 返回来的标签数据之中的标签是在某个区间范围内的数值集合的平均值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="base"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Distributes(data As IEnumerable(Of Double), Optional base! = 10.0F) As Dictionary(Of Integer, DoubleTagged(Of Integer))
            Dim array As DoubleTagged(Of Double)() = data _
                .Select(Function(x)
                            Return New DoubleTagged(Of Double) With {
                                .Tag = sys.Log(x, base),
                                .Value = x
                            }
                        End Function) _
                .ToArray
            Dim min As Integer = CInt(array.Min(Function(x) x.Tag)) - 1
            Dim max As Integer = CInt(array.Max(Function(x) x.Tag)) + 1
            Dim l As int = min, low As Integer = min
            Dim out As New Dictionary(Of Integer, DoubleTagged(Of Integer))

            Do While ++l < max
                Dim LQuery As DoubleTagged(Of Double)() =
                    LinqAPI.Exec(Of DoubleTagged(Of Double)) <=
 _
                    From x As DoubleTagged(Of Double)
                    In array
                    Where x.Tag >= low AndAlso
                        x.Tag < l
                    Select x

                out(l) = New DoubleTagged(Of Integer) With {
                    .Tag = If(LQuery.Length = 0, 0, LQuery.Average(Function(x) x.Value)),
                    .Value = LQuery.Length
                }
                low = l
            Loop

            If out(min + 1).Value = 0 Then
                Call out.Remove(min)
            End If
            If out(max - 1).Value = 0 Then
                Call out.Remove(max)
            End If

            Return out
        End Function

        ''' <summary>
        ''' ###### 频数分布表与直方图
        ''' 
        ''' 这个函数返回来的是频数以及区间内的所有的数的平均值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="step!"></param>
        ''' <returns>
        ''' 返回来的数据为区间的下限 -> {频数, 平均值}
        ''' </returns>
        <Extension>
        Public Function Hist(data As Double(), Optional step! = 1) As Dictionary(Of Double, IntegerTagged(Of Double))
            Dim out As New Dictionary(Of Double, IntegerTagged(Of Double))
            Dim i As int = 0
            Dim x As New Value(Of Double)
            Dim stop# = Fix(data.Max) + 1
            Dim len% = data.Length

            ' 升序排序方便进行快速计算
            data = data _
                .OrderBy(Function(n) n) _
                .ToArray

            For min As Double = Fix(data.Min) - 1 To [stop] Step [step]
                Dim upbound# = min + [step]
                Dim n As Integer = 0
                Dim list As New List(Of Double)

                ' 因为数据已经是经过排序了的，所以在这里可以直接进行区间计数
                Do While i < len AndAlso (x = data(++i)) >= min AndAlso x < upbound
                    n += 1
                    list += x.Value
                Loop

                Call out.Add(
                    min, New IntegerTagged(Of Double) With {
                        .Tag = n,
                        .Value = If(list.Count = 0, 0R, list.Average),
                        .TagStr = $"[{min}, {upbound}]"
                    })

                If i.Value = len Then
                    Exit For
                End If
            Next

            Return out
        End Function
    End Module
End Namespace