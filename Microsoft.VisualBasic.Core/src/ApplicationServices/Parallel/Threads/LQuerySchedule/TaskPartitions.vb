#Region "Microsoft.VisualBasic::b2b58862d64fb542fc56e70f154d98a2, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\LQuerySchedule\TaskPartitions.vb"

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

    '   Total Lines: 164
    '    Code Lines: 99 (60.37%)
    ' Comment Lines: 41 (25.00%)
    '    - Xml Docs: 92.68%
    ' 
    '   Blank Lines: 24 (14.63%)
    '     File Size: 6.94 KB


    '     Module TaskPartitions
    ' 
    '         Function: (+2 Overloads) Partitioning, Partitions, (+2 Overloads) PartTokens, SplitIterator
    '         Structure __taskHelper
    ' 
    '             Function: Invoke, InvokeWhere, ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Parallel.Linq

    ''' <summary>
    ''' 对大量的短时间的任务进行分区的操作是在这里完成的
    ''' </summary>
    Public Module TaskPartitions

        ''' <summary>
        ''' 根据任务总量计算出所需要的线程的数量
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="num_threads"></param>
        ''' <returns></returns>
        ''' <remarks>假设所有的任务都被平均的分配到每一个线程之上</remarks>
        Public Function PartTokens(source As Integer, num_threads As Integer) As Integer
            Return (source / num_threads) - 1
        End Function

        Public Function PartTokens(source As Integer) As Integer
            Return PartTokens(source, num_threads:=LQuerySchedule.CPU_NUMBER)
        End Function

        ''' <summary>
        ''' Performance the partitioning operation on the input sequence.
        ''' (请注意，这个函数适用于数量非常多的序列。对所输入的序列进行分区操作，<paramref name="parTokens"/>函数参数是每一个分区里面的元素的数量)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="parTokens">每一个分区之中的元素数量</param>
        ''' <returns></returns>
        ''' <remarks>对于数量较少的序列，可以使用<see cref="SplitIterator(Of T)(IEnumerable(Of T), Integer, Boolean)"/>进行分区操作，
        ''' 该函数使用数组的<see cref="Array.ConstrainedCopy(Array, Integer, Array, Integer, Integer)"/>方法进行分区复制，效率较高
        ''' 
        ''' 由于本函数需要处理大量的数据，使用Array的方法会内存占用较厉害，所以在这里更改为List操作以降低内存的占用
        ''' </remarks>
        <Extension>
        Public Iterator Function SplitIterator(Of T)(source As IEnumerable(Of T), parTokens%, Optional echo As Boolean = True) As IEnumerable(Of T())
            Dim buf As New List(Of T)
            Dim n As Integer = 0
            Dim count As Integer = 0
            Dim parts As Integer

            For Each x As T In source
                If n = parTokens Then
                    Yield buf.ToArray

                    buf *= 0
                    n = 0
                    parts += 1
                End If

                buf.Add(x)
                n += 1
                count += 1
            Next

            If buf > 0 Then
                Yield buf.ToArray
            End If

            If echo Then
                Call $"Large data_set(size:={count}) partitioning(partitions:={parts}) jobs done!".debug
            End If
        End Function

        ''' <summary>
        ''' 进行分区之后返回一个长时间的任务组合
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="parts">函数参数是每一个分区里面的元素的数量</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Iterator Function Partitioning(Of T, out)(source As IEnumerable(Of T),
                                                         parts As Integer,
                                                         task As Func(Of T, out)) As IEnumerable(Of Func(Of out()))

            Dim buf As IEnumerable(Of T()) = source.SplitIterator(parts)

            For Each part As T() In buf
                Yield AddressOf New __taskHelper(Of T, out) With {
                    .source = part,
                    .task = task
                }.Invoke
            Next
        End Function

        ''' <summary>
        ''' 进行分区之后返回一个长时间的任务组合
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Iterator Function Partitioning(Of T, out)(source As IEnumerable(Of T),
                                                         parts As Integer,
                                                         task As Func(Of T, out),
                                                         where As Func(Of T, Boolean)) As IEnumerable(Of Func(Of out()))

            Dim buf As IEnumerable(Of T()) = source.SplitIterator(parts)

            For Each part As T() In buf
                Yield AddressOf New __taskHelper(Of T, out) With {
                    .source = part,
                    .task = task,
                    .where = where
                }.InvokeWhere
            Next
        End Function

        Public Iterator Function Partitions(Of T)(source As IEnumerable(Of T),
                                                  parts As Integer,
                                                  [where] As Func(Of T, Boolean)) As IEnumerable(Of Func(Of T()))

            Dim buf As IEnumerable(Of T()) = source.SplitIterator(parts)

            For Each part As T() In buf
                Yield Function() LinqAPI.Exec(Of T) <= From x As T
                                                       In part
                                                       Where where(x) = True
                                                       Select x
            Next
        End Function

        ''' <summary>
        ''' 因为在上一层调用之中使用了并行化，所以在这里不能够使用并行化拓展了
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="out"></typeparam>
        Private Structure __taskHelper(Of T, out)

            Dim task As Func(Of T, out)
            Dim source As T()
            Dim where As Func(Of T, Boolean)

            Public Overrides Function ToString() As String
                Return task.ToString
            End Function

            Public Function InvokeWhere() As out()
                Dim __task As Func(Of T, out) = task
                Dim test = where
                Dim LQuery As out() =
                    LinqAPI.Exec(Of out) <= From x As T
                                            In source
                                            Where True = test(x)
                                            Select __task(x)
                Return LQuery
            End Function

            Public Function Invoke() As out()
                Dim __task As Func(Of T, out) = task
                Dim LQuery As out() =
                    LinqAPI.Exec(Of out) <= From x As T
                                            In source
                                            Select __task(x)
                Return LQuery
            End Function
        End Structure
    End Module
End Namespace
