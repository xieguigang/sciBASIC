#Region "Microsoft.VisualBasic::df0b0c1889460db79326c0b1c9812b72, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\Combination\CombinationExtensions.vb"

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

    '   Total Lines: 98
    '    Code Lines: 64 (65.31%)
    ' Comment Lines: 20 (20.41%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 14 (14.29%)
    '     File Size: 3.43 KB


    '     Module CombinationExtensions
    ' 
    '         Function: AllCombinations, CreateCombos, FullCombination, Generate, (+2 Overloads) Iterates
    '                   Iteration
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' 任意多个集合之间的对象之间相互组成组合输出
    ''' </summary>
    ''' <remarks></remarks>
    Public Module CombinationExtensions

        ''' <summary>
        ''' 生成两个序列的两两组合 ``{<paramref name="seq_1"/> -> <paramref name="seq_2"/>}()``
        ''' </summary>
        ''' <typeparam name="TA"></typeparam>
        ''' <typeparam name="TB"></typeparam>
        ''' <param name="seq_1"></param>
        ''' <param name="seq_2"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function CreateCombos(Of TA, TB)(seq_1 As IEnumerable(Of TA), seq_2 As IEnumerable(Of TB)) As IEnumerable(Of (a As TA, b As TB))
            Dim b As TB() = seq_2.ToArray

            For Each i As TA In seq_1
                For Each j As TB In b
                    Yield (i, j)
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FullCombination(Of T)(seq As IEnumerable(Of T)) As IEnumerable(Of (a As T, b As T))
            With seq.ToArray
                Return .CreateCombos(.ByRef)
            End With
        End Function

        ''' <summary>
        ''' create a vs b vs c ...
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Iteration(Of T)(source As T()()) As IEnumerable(Of T())
            Dim first As T() = source.First

            ' 只剩下两个的时候，会退出递归操作
            If source.Length = 2 Then
                Dim last As T() = source.Last

                For Each x As T In first
                    For Each _item As T In last
                        Yield {x, _item}
                    Next
                Next
            Else
                For Each x As T In first
                    ' 递归组合迭代
                    For Each subArray As T() In source.Skip(1).ToArray.Iteration
                        Yield New List(Of T)(x) + subArray
                    Next
                Next
            End If
        End Function

        Public Function Generate(Of T)(source As T()()) As T()()
            Return source.Iteration.ToArray
        End Function

        <Extension>
        Public Iterator Function AllCombinations(Of T)(source As T(), Optional size As Integer = -1) As IEnumerable(Of T())
            If size <= 0 Then
                size = source.Length
            End If

            Dim combinator As New Combination(Of T)(source, size)

            Do While combinator.CanRun
                Yield combinator.Execute
            Loop
        End Function

        <Extension>
        Public Iterator Function Iterates(Of T)(comb As (T, T)) As IEnumerable(Of T)
            Yield comb.Item1
            Yield comb.Item2
        End Function

        <Extension>
        Public Iterator Function Iterates(Of T)(comb As Tuple(Of T, T)) As IEnumerable(Of T)
            Yield comb.Item1
            Yield comb.Item2
        End Function
    End Module
End Namespace
