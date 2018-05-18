Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
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

    <Extension>
    Public Function RemovesOutlier(index As IEnumerable(Of Integer), x As Vector, y As Vector) As (X As Vector, Y As Vector)
        With index.OrderBy(Self(Of Integer)).ToArray
            x = .removesByIndex(x.AsList)
            y = .removesByIndex(y.AsList)
        End With

        Return (x, y)
    End Function

    <Extension>
    Private Function removesByIndex(index%(), list As List(Of Double)) As Vector
        For i As Integer = 0 To index.Length - 1
            Dim ind% = index(i) - i
            list -= ind
        Next

        Return list.AsVector
    End Function
End Module
