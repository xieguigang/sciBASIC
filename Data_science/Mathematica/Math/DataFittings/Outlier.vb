Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile

Public Module Outlier

    ''' <summary>
    ''' 返回所给定的数据点序列之中的异常点的下标值
    ''' </summary>
    ''' <param name="seq">采用四分位数方法进行异常点的计算分析</param>
    ''' <returns></returns>
    <Extension> Public Iterator Function OutlierIndex(seq As Vector) As IEnumerable(Of Integer)
        Dim quartile = seq.Quartile(altPosition:=True)
        Dim out = seq.Outlier(quartile).Outlier

        For Each x As SeqValue(Of Double) In seq.SeqIterator
            For Each o In out
                If Math.Abs(x.value - o) <= 0.000001 Then
                    Yield x.i
                    Exit For
                End If
            Next
        Next
    End Function
End Module
