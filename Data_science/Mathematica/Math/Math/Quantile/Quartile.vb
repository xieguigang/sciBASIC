Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Quantile

    ''' <summary>
    ''' 四分位数（Quartile）是在统计学中把所有数值由小到大排列并分成四等份够，处于三个分割点位置的数值。
    ''' </summary>
    Public Module Quartile

        ''' <summary>
        ''' + 第一四分位数 (Q1)，又称“较小四分位数”，等于该样本中所有数值由小到大排列后第25%的数字。
        ''' + 第二四分位数 (Q2)，又称“中位数”，等于该样本中所有数值由小到大排列后第50%的数字。
        ''' + 第三四分位数 (Q3)，又称“较大四分位数”，等于该样本中所有数值由小到大排列后第75%的数字。
        ''' + 第三四分位数与第一四分位数的差距又称四分位距（InterQuartile Range,IQR）。
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="altPosition">
        ''' True for n-1 method
        ''' False for n+1 method
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

            Dim q1 = vector(q(0))
            Dim q2 = vector(q(1))
            Dim q3 = vector(q(2))
            Dim IQR = q3 - q1

            Return (q1, q2, q3, IQR, New DoubleRange(vector))
        End Function
    End Module
End Namespace
