
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' 任意多个集合之间的对象之间相互组成组合输出
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Combination

        <Extension>
        Public Iterator Function CreateCombos(Of TA, TB)(sourceA As IEnumerable(Of TA), sourceB As IEnumerable(Of TB)) As IEnumerable(Of Tuple(Of TA, TB))
            Dim b As TB() = sourceB.ToArray

            For Each i As TA In sourceA
                For Each j As TB In b
                    Yield New Tuple(Of TA, TB)(i, j)
                Next
            Next
        End Function

        <Extension> Public Iterator Function Iteration(Of T)(source As T()()) As IEnumerable(Of T())
            Dim first As T() = source.First

            If source.Length = 2 Then ' 只剩下两个的时候，会退出递归操作
                Dim last As T() = source.Last

                For Each x As T In first
                    For Each _item As T In last
                        Yield {x, _item}
                    Next
                Next
            Else
                For Each x As T In first
                    For Each subArray As T() In source.Skip(1).ToArray.Iteration   ' 递归组合迭代
                        Yield New List(Of T)(x) + subArray
                    Next
                Next
            End If
        End Function

        Public Function Generate(Of T)(source As T()()) As T()()
            Return source.Iteration.ToArray
        End Function
    End Module
End Namespace