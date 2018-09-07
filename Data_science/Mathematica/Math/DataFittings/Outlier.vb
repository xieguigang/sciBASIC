#Region "Microsoft.VisualBasic::3f72107729b5ade1220f801d6a65eb37, Data_science\Mathematica\Math\DataFittings\Outlier.vb"

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

    ' Module Outlier
    ' 
    '     Function: OrderSequenceOutlierIndex, OutlierIndex, removesByIndex, RemovesOutlier
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
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

#Region "Order sequence outlier"

    ' 假设有这样子的一个有序序列
    ' 可以发现除了第一个数据点，序列中其他的点都是递减的
    ' 所以第一个点应该是这个有序序列之中的异常点
    '
    ' 0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649
    '
    ' 因为四分位数方法是无差别对待序列之中的所有点的，所以按照四分位数方法计算出来的异常点为2.278282642
    ' 所以在这里不可以使用四分位数方法来检查有序序列中的异常点

    ''' <summary>
    ''' 采用滑窗外加变异量计算来得到异常点
    ''' </summary>
    ''' <param name="seq"></param>
    ''' <returns></returns>
    <Extension>
    Public Function OrderSequenceOutlierIndex(seq As Vector) As IEnumerable(Of Integer)
        Dim delta = seq.SlideWindows(winSize:=2) _
                       .Select(Function(d)
                                   ' 计算当前的这个滑窗内的变异量
                                   Return d(1) - d(0)
                               End Function) _
                       .AsVector

        Dim normalSign = If(delta(delta > 0).Count > delta(delta <= 0).Count, 1, -1)

        If normalSign = 1 Then
            Return Which.IsTrue(delta < 0)
        Else
            Return Which.IsTrue(delta > 0)
        End If
    End Function
#End Region
End Module
