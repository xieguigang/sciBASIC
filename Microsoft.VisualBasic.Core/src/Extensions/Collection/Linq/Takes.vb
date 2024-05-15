#Region "Microsoft.VisualBasic::ae4de2be1628622acbfba8995d6f4152, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\Takes.vb"

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

    '   Total Lines: 153
    '    Code Lines: 78
    ' Comment Lines: 60
    '   Blank Lines: 15
    '     File Size: 6.46 KB


    '     Module TakesExtension
    ' 
    '         Function: doReversedTake, (+2 Overloads) TakeRandomly, (+3 Overloads) Takes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math

Namespace Linq

    Public Module TakesExtension

        ''' <summary>
        ''' An wrapper of <see cref="Array.ConstrainedCopy"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Takes(Of T)(source As T(), count As Integer) As T()
            Dim bufs As T() = New T(count - 1) {}
            Call Array.ConstrainedCopy(source, Scan0, bufs, Scan0, count)
            Return bufs
        End Function

        ''' <summary>
        ''' 将所有对应的index处的<paramref name="flags"/>值为True的元素返回
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="flags"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Takes(Of T)(source As IEnumerable(Of T), flags As Boolean()) As IEnumerable(Of T)
            Return source.Where(Function(obj, i) flags(i))
        End Function

        ''' <summary>
        ''' Take elements by <paramref name="index"/> list. 
        ''' (将指定<paramref name="index"/>下标的元素从原始数据<paramref name="source"/>序列之中提取出来)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="index">所要获取的目标对象的下表的集合</param>
        ''' <param name="reversed">是否为反向选择，即返回所有不在目标index集合之中的元素列表</param>
        ''' <param name="OffSet">当进行反选的时候，本参数将不会起作用</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2018-3-30 函数经过测试没有问题
        ''' </remarks>
        <ExportAPI("takes")>
        <Extension>
        Public Function Takes(Of T)(source As IEnumerable(Of T),
                                    index%(),
                                    Optional offSet% = 0,
                                    Optional reversed As Boolean = False) As T()
            If reversed Then
                Return source.doReversedTake(index).ToArray
            End If

            Dim result As T() = New T(index.Length - 1) {}
            Dim indices As Index(Of Integer) = index _
                .Select(Function(oi) oi + offSet) _
                .Indexing

            For Each x As SeqValue(Of T) In source.SeqIterator
                ' 在这里得到的是x的index在indexs参数之中的索引位置
                Dim i% = indices.IndexOf(x:=x.i)

                ' 当前的原始的下表位于indexs参数值中，则第i个indexs元素所指向的source的元素
                ' 就是x， 将其放入对应的结果列表之中
                If i > -1 Then
                    result(i) = x.value
                End If
            Next

            Return result
        End Function

        ''' <summary>
        ''' 反选，即将所有不出现在<paramref name="index"></paramref>之中的元素都选取出来
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="collection"></param>
        ''' <param name="index"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Private Iterator Function doReversedTake(Of T)(collection As IEnumerable(Of T), index As Integer()) As IEnumerable(Of T)
            Dim indices As New Index(Of Integer)(index)

            For Each x As SeqValue(Of T) In collection.SeqIterator
                ' 不存在于顶点的列表之中，即符合反选的条件，则添加进入结果之中
                If indices.IndexOf(x:=x.i) = -1 Then
                    Yield x.value
                End If
            Next
        End Function

        ''' <summary>
        ''' 随机的在目标集合中选取指定数目的子集合
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="counts">当目标数目大于或者等于目标集合的数目的时候，则返回目标集合</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TakeRandomly(Of T)(source As IEnumerable(Of T), counts%) As T()
            Return source.SafeQuery _
                .ToArray _
                .TakeRandomly(counts) _
                .ToArray
        End Function

        ''' <summary>
        ''' 随机的在目标集合中选取指定数目的子集合
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <param name="counts">当目标数目大于或者等于目标集合的数目的时候，则返回目标集合</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function use the default <see cref="RandomExtensions.seeds"/>
        ''' </remarks>
        <Extension>
        Public Function TakeRandomly(Of T)(array As T(), counts%, Optional unsafe As Boolean = True) As IEnumerable(Of T)
            If counts <= 0 AndAlso Not unsafe Then
                Return New T() {}
            ElseIf counts >= array.Length Then
                Dim out As T() = New T(array.Length - 1) {}
                Call System.Array.ConstrainedCopy(array, Scan0, out, Scan0, array.Length)
                Return out
            Else
                Dim out As T() = New T(counts - 1) {}
                Dim input As New List(Of T)(array)
                Dim ind As Integer

                For i As Integer = 0 To counts - 1
                    ind = seeds.Next(input.Count)
                    out(i) = input(ind)
                    input.RemoveAt(ind)
                Next

                Return out
            End If
        End Function
    End Module
End Namespace
