Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Quantile

    ''' <summary>
    ''' 四分位数（Quartile）是在统计学中把所有数值由小到大排列并分成四等份够，处于三个分割点位置的数值。
    ''' </summary>
    Public Module Quartile

        Public Enum Levels
            Q1 = 1
            Q2 = 2
            Q3 = 3
        End Enum

        ''' <summary>
        ''' + 第一四分位数 (Q1)，又称“较小四分位数”，等于该样本中所有数值由小到大排列后第25%的数字。
        ''' + 第二四分位数 (Q2)，又称“中位数”，等于该样本中所有数值由小到大排列后第50%的数字。
        ''' + 第三四分位数 (Q3)，又称“较大四分位数”，等于该样本中所有数值由小到大排列后第75%的数字。
        ''' + 第三四分位数与第一四分位数的差距又称四分位距（InterQuartile Range,IQR）。
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="altPosition">
        ''' + <see cref="Boolean"/>.True  for n-1 method
        ''' + <see cref="Boolean"/>.False for n+1 method
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Quartile(data As IEnumerable(Of Double), Optional altPosition As Boolean = False) As (Q1#, Q2#, Q3#, IQR#, range As DoubleRange)
            Dim vector = data.OrderBy(Function(x) x).ToArray
            Dim n = vector.Length
            Dim q As Vector

            If altPosition Then
                q = 1 + (n - 1) * {0.25, 0.5, 0.75}.AsVector
            Else
                q = (n + 1) * {0.25, 0.5, 0.75}.AsVector
            End If

            Dim q1 = vector(Math.Truncate(q(0)))
            Dim q2 = vector(Math.Truncate(q(1)))
            Dim q3 = vector(Math.Truncate(q(2)))
            Dim IQR = q3 - q1

            Return (q1, q2, q3, IQR, New DoubleRange(vector))
        End Function

        ''' <summary>
        ''' 异常值被定义为小于``Q1－1.5*IQR``或大于``Q3+1.5*IQR``的值。虽然这种标准有点任意性，但它来源于经验判断，
        ''' 经验表明它在处理需要特别注意的数据方面表现不错。这与识别异常值的经典方法有些不同。众所周知，基于正态分布
        ''' 的3σ法则或z分数方法是以假定数据服从正态分布为前提的，但实际数据往往并不严格服从正态分布。它们判断异常值的
        ''' 标准是以计算数据批的均值和标准差为基础的，而均值和标准差的耐抗性极小，异常值本身会对它们产生较大影响，这样
        ''' 产生的异常值个数不会多于总数0.7%。显然，应用这种方法于非正态分布数据中判断异常值，其有效性是有限的。箱形图
        ''' 的绘制依靠实际数据，不需要事先假定数据服从特定的分布形式，没有对数据作任何限制性要求，它只是真实直观地表现
        ''' 数据形状的本来面貌；另一方面，箱形图判断异常值的标准以四分位数和四分位距为基础，四分位数具有一定的耐抗性，
        ''' 多达25%的数据可以变得任意远而不会很大地扰动四分位数，所以异常值不能对这个标准施加影响，箱形图识别异常值的
        ''' 结果比较客观。
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="quartile"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Outlier(data As Vector, quartile As (Q1#, Q2#, Q3#, IQR#, range As DoubleRange)) As (Normal As Double(), Outlier As Double())
            With quartile
                Dim lowerBound = .Q1 - 1.5 * .IQR
                Dim upperBound = .Q3 + 1.5 * .IQR
                Dim normal#() = data((data >= lowerBound) & (data <= upperBound))
                Dim outliers = data(data < lowerBound) & data(data > upperBound)

                Return (normal, outliers)
            End With
        End Function
    End Module
End Namespace
