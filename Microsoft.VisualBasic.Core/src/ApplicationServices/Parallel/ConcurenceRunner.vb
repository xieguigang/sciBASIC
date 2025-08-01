#Region "Microsoft.VisualBasic::3c081dee1904d6aaf3f9ebab5b343088, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\ConcurenceRunner.vb"

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

    '   Total Lines: 176
    '    Code Lines: 84 (47.73%)
    ' Comment Lines: 69 (39.20%)
    '    - Xml Docs: 98.55%
    ' 
    '   Blank Lines: 23 (13.07%)
    '     File Size: 6.54 KB


    '     Class VectorTask
    ' 
    '         Properties: num_threads, span_size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Allocate, CopyMemory, Run, Solve
    ' 
    '         Sub: CopyMemory, ParallelFor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Parallel

    ''' <summary>
    ''' the parallel task helper
    ''' </summary>
    Public MustInherit Class VectorTask

        ''' <summary>
        ''' the input pending task sequence size
        ''' </summary>
        Protected workLen As Integer
        ''' <summary>
        ''' set this flag value to value TRUE for run algorithm debug
        ''' </summary>
        Protected sequenceMode As Boolean = False

        ''' <summary>
        ''' <see cref="n_threads"/>
        ''' </summary>
        Protected ReadOnly cpu_count As Integer = n_threads

        Dim opt As ParallelOptions
        Dim is_verbose As Boolean = False

        ''' <summary>
        ''' set number of cpu threads for run current <see cref="VectorTask"/> parallel. 
        ''' this thread value MUST BE configured before construct of the task object.
        ''' </summary>
        Public Shared n_threads As Integer = 4

        Public ReadOnly Property num_threads As Integer
            Get
                Return cpu_count
            End Get
        End Property

        Public ReadOnly Property span_size As Integer
            Get
                Return workLen / cpu_count
            End Get
        End Property

        ''' <summary>
        ''' construct a new parallel task executator
        ''' </summary>
        ''' <param name="nsize"></param>
        ''' <remarks>
        ''' the thread count for run the parallel task is configed
        ''' via the <see cref="n_threads"/> by default.
        ''' </remarks>
        Sub New(nsize As Integer, Optional verbose As Boolean = False, Optional workers As Integer? = Nothing)
            workLen = nsize
            cpu_count = If(workers, n_threads)
            opt = New ParallelOptions With {.MaxDegreeOfParallelism = cpu_count}
            is_verbose = verbose
        End Sub

        ''' <summary>
        ''' solve a sub task
        ''' </summary>
        ''' <param name="start">index offset start from zero</param>
        ''' <param name="ends"></param>
        Protected MustOverride Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)

        ''' <summary>
        ''' Run in sequence
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Solve() As VectorTask
            If is_verbose Then
                VBDebugger.EchoLine("solve problem in sequence mode.")
            End If

            sequenceMode = True
            Solve(0, workLen - 1, 0)
            Return Me
        End Function

        ''' <summary>
        ''' Run in parallel
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As VectorTask
            If sequenceMode OrElse cpu_count = 1 OrElse span_size < 1 Then
                ' run in sequence
                Call Solve()
            Else
                If is_verbose Then
                    Call VBDebugger.EchoLine($"solve problem in parallel mode with {cpu_count} cpu threads!")
                    Call VBDebugger.EchoLine($"each task span size: {span_size}.")
                End If

                System.Threading.Tasks.Parallel.For(
                    fromInclusive:=0,
                    toExclusive:=cpu_count + cpu_worker_overflow,
                    parallelOptions:=opt,
                    body:=Sub(i) ParallelFor(i, span_size)
                )
            End If

            Return Me
        End Function

        Const cpu_worker_overflow As Integer = 3

        ''' <summary>
        ''' allocate a block of result output memory data for one thread task
        ''' </summary>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="all">
        ''' allocate all result(len=<see cref="workLen"/>) when set this parameter value TRUE or 
        ''' just for cpu worker thread(len=<see cref="cpu_count"/>) if set this parameter value 
        ''' FALSE?
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' element count problem see the dev comments about the 
        ''' parameter ``thread_id`` from function 
        ''' <see cref="ParallelFor"/>
        ''' </remarks>
        Protected Function Allocate(Of TOut)(all As Boolean) As TOut()
            If all Then
                Return New TOut(workLen - 1) {}
            Else
                Return New TOut(cpu_count + cpu_worker_overflow) {}
            End If
        End Function

        ''' <summary>
        ''' implements the parallel for use the thread pool
        ''' </summary>
        ''' <param name="span_size"></param>
        ''' <param name="thread_id">
        ''' the thread id is start from based ZERO, but the upper index of 
        ''' the thread id is equals to the <see cref="cpu_count"/>, so 
        ''' group result vector should has value with ``<see cref="cpu_count"/> + 1`` elements.
        ''' </param>
        Private Sub ParallelFor(thread_id As Integer, span_size As Integer)
            Dim start As Integer = thread_id * span_size
            Dim ends As Integer = start + span_size - 1

            If start >= workLen Then
                Return
            End If
            If ends >= workLen Then
                ends = workLen - 1
            End If

            If is_verbose Then
                Call VBDebugger.EchoLine($"[{Me.GetType.Name}$t_{thread_id}] {start}...{ends}@total={workLen}")
            End If

            Call Solve(start, ends, cpu_id:=thread_id)
        End Sub

        Public Shared Function CopyMemory(Of T)(v As T(), start As Integer, ends As Integer) As T()
            Dim copy As T() = New T(start - ends - 1) {}
            Array.ConstrainedCopy(v, start, copy, Scan0, copy.Length)
            Return copy
        End Function

        ''' <summary>
        ''' copy all data of <paramref name="span"/> to the target region inside <paramref name="v"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="v"></param>
        ''' <param name="span">may be a part of region inside <paramref name="v"/></param>
        ''' <param name="start"></param>
        Public Shared Sub CopyMemory(Of T)(v As T(), span As T(), start As Integer)
            Array.ConstrainedCopy(span, Scan0, v, start, span.Length)
        End Sub
    End Class
End Namespace
