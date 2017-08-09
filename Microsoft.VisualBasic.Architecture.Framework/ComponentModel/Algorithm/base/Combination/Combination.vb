#Region "Microsoft.VisualBasic::d0de9ada8b20e046ac3257c9c022f1e9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Algorithm\base\Combination\Combination.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' 任意多个集合之间的对象之间相互组成组合输出
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Combination

        ''' <summary>
        ''' 生成两个序列的两两组合 ``{<paramref name="seq_1"/> -> <paramref name="seq_2"/>}()``
        ''' </summary>
        ''' <typeparam name="TA"></typeparam>
        ''' <typeparam name="TB"></typeparam>
        ''' <param name="seq_1"></param>
        ''' <param name="seq_2"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function CreateCombos(Of TA, TB)(seq_1 As IEnumerable(Of TA), seq_2 As IEnumerable(Of TB)) As IEnumerable(Of Tuple(Of TA, TB))
            Dim b As TB() = seq_2.ToArray

            For Each i As TA In seq_1
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

        <Extension>
        Public Iterator Function Iterates(Of T)(comb As Tuple(Of T, T)) As IEnumerable(Of T)
            Yield comb.Item1
            Yield comb.Item2
        End Function
    End Module
End Namespace
