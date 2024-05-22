#Region "Microsoft.VisualBasic::167993104bf5e209289d3f11c031d468, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\LQuerySchedule\LQuerySchedule.vb"

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

    '   Total Lines: 175
    '    Code Lines: 95 (54.29%)
    ' Comment Lines: 55 (31.43%)
    '    - Xml Docs: 94.55%
    ' 
    '   Blank Lines: 25 (14.29%)
    '     File Size: 8.32 KB


    '     Module LQuerySchedule
    ' 
    '         Properties: CPU_NUMBER
    ' 
    '         Function: [Where], AutoConfig, DefaultConfig, (+3 Overloads) LQuery
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Parallel.Linq

    ''' <summary>
    ''' Parallel Linq query library for VisualBasic.
    ''' (用于高效率执行批量查询操作和用于检测操作超时的工具对象，请注意，为了提高查询的工作效率，请尽量避免在查询操作之中生成新的临时对象
    ''' 并行版本的LINQ查询和原始的线程操作相比具有一些性能上面的局限性)
    ''' </summary>
    ''' <remarks>
    ''' 在使用``Parallel LINQ``的时候，请务必要注意不能够使用Let语句操作共享变量，因为排除死锁的开销比较大
    ''' 
    ''' 在设计并行任务的时候应该遵循的一些原则:
    ''' 
    ''' 1. 假若每一个任务之间都是相互独立的话，则才可以进行并行化调用
    ''' 2. 在当前程序域之中只能够通过线程的方式进行并行化，对于时间较短的任务而言，非并行化会比并行化更加有效率
    ''' 3. 但是对于这些短时间的任务，仍然可以将序列进行分区合并为一个大型的长时间任务来产生并行化
    ''' 4. 对于长时间的任务，可以直接使用并行化Linq拓展执行并行化
    ''' 
    ''' 这个模块主要是针对大量的短时间的任务序列的并行化的，用户可以在这里配置线程的数量自由的控制并行化的程度
    ''' </remarks>
    Public Module LQuerySchedule

        ''' <summary>
        ''' Get the number of processors on the current machine.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>(获取当前的系统主机的CPU核心数)</remarks>
        Public ReadOnly Property CPU_NUMBER As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Environment.ProcessorCount
            End Get
        End Property

        Public Function DefaultConfig() As [Default](Of Integer?)
            Return CPU_NUMBER
        End Function

        ''' <summary>
        ''' 假如小于0，则认为是自动配置，0被认为是单线程，反之直接返回
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function AutoConfig(n As Integer) As Integer
            If n < 0 Then
                Return CPU_NUMBER
            ElseIf n = 0 OrElse n = 1 Then
                Return 1
            Else
                Return n
            End If
        End Function

        ''' <summary>
        ''' 将大量的短时间的任务进行分区，合并，然后再执行并行化，请注意，<paramref name="task"/>参数不能够使lambda表达式，否则会出现EntryNotFound的错误
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="inputs"></param>
        ''' <param name="task"></param>
        ''' <param name="parTokens">函数参数是每一个分区里面的元素的数量</param>
        ''' <returns></returns>
        Public Iterator Function LQuery(Of T, TOut)(inputs As IEnumerable(Of T),
                                                    task As Func(Of T, TOut),
                                                    Optional parTokens As Integer = 20000) As IEnumerable(Of TOut)

            Call $"Start schedule task pool for {GetType(T).FullName}  -->  {GetType(TOut).FullName}".__DEBUG_ECHO

            Dim buf = TaskPartitions.Partitioning(inputs, parTokens, task)
            Dim LQueryInvoke = From part As Func(Of TOut())
                               In buf.AsParallel
                               Select New AsyncHandle(Of TOut())(part).Run

            For Each part As AsyncHandle(Of TOut()) In LQueryInvoke
                If part Is Nothing Then
                    Call VBDebugger.Warning("Parts of the data operation timeout!")
                    Continue For
                End If

                For Each x As TOut In part.GetValue
                    Yield x
                Next
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function

        ''' <summary>
        ''' 将大量的短时间的任务进行分区，合并，然后再执行并行化
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="inputs"></param>
        ''' <param name="task"></param>
        ''' <param name="where">Processing where test on the inputs</param>
        ''' <returns></returns>
        Public Iterator Function LQuery(Of T, TOut)(inputs As IEnumerable(Of T),
                                                    task As Func(Of T, TOut),
                                                    Optional where As Func(Of T, Boolean) = Nothing,
                                                    Optional partitionSize As Integer = 20000) As IEnumerable(Of TOut)

            Call $"Start schedule task pool for {GetType(T).FullName}  -->  {GetType(TOut).FullName}".__DEBUG_ECHO

            Dim buf As IEnumerable(Of Func(Of TOut())) =
                If(where Is Nothing,
                TaskPartitions.Partitioning(inputs, partitionSize, task),
                TaskPartitions.Partitioning(inputs, partitionSize, task, where))
            Dim LQueryInvoke = From part As Func(Of TOut())
                               In buf.AsParallel
                               Select part()

            For Each part As TOut() In LQueryInvoke
                For Each x As TOut In part
                    Yield x
                Next
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function

        ''' <summary>
        ''' 将大量的短时间的任务进行分区，合并，然后再执行并行化
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="inputs"></param>
        ''' <param name="task"></param>
        ''' <param name="outWhere">Processing where test on the output</param>
        ''' <returns></returns>
        Public Iterator Function LQuery(Of T, TOut)(inputs As IEnumerable(Of T),
                                                    task As Func(Of T, TOut),
                                                    outWhere As Func(Of TOut, Boolean),
                                                    Optional partitionSize As Integer = 20000) As IEnumerable(Of TOut)

            Call $"Start schedule task pool for {GetType(T).FullName}  -->  {GetType(TOut).FullName}".__DEBUG_ECHO

            Dim buf As IEnumerable(Of Func(Of TOut())) = TaskPartitions.Partitioning(inputs, partitionSize, task)
            Dim LQueryInvoke = From part As Func(Of TOut())
                               In buf.AsParallel
                               Select part()

            For Each part As TOut() In LQueryInvoke
                For Each x As TOut In From o As TOut
                                      In part
                                      Where True = outWhere(o)
                                      Select o
                    Yield x
                Next
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function

        Public Iterator Function [Where](Of T)(source As IEnumerable(Of T),
                                               test As Func(Of T, Boolean),
                                               Optional parTokens As Integer = 20000) As IEnumerable(Of T())
            Call $"Start schedule task pool for {GetType(T).FullName}".__DEBUG_ECHO

            Dim buf As IEnumerable(Of Func(Of T())) = TaskPartitions.Partitions(source, parTokens, test)
            Dim LQueryInvoke = From part As Func(Of T())
                               In buf.AsParallel
                               Select part()

            For Each part As T() In LQueryInvoke
                Yield part
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function
    End Module
End Namespace
