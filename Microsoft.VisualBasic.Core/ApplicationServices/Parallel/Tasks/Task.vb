#Region "Microsoft.VisualBasic::ace065b1c8fa32d27a4802a089772660, ApplicationServices\Parallel\Tasks\Task.vb"

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

    '     Class Task
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetValue, Start
    ' 
    '         Sub: __invokeTask
    ' 
    '     Class Task
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Start
    ' 
    '         Sub: __invokeTask, RaisingEvent
    ' 
    '     Class Task
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Start
    ' 
    '         Sub: __invokeTask, RaisingEvent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Parallel.Tasks

    ''' <summary>
    ''' 更加底层的线程模式，和LINQ相比不会受到CPU核心数目的限制
    ''' </summary>
    ''' <typeparam name="T">后台任务的执行参数</typeparam>
    ''' <typeparam name="TOut">后台任务的执行结果</typeparam>
    Public Class Task(Of T, TOut) : Inherits IParallelTask

        Dim _Handle As Func(Of T, TOut)
        Dim _Input As T

        ''' <summary>
        ''' 假若任务已经完成，则会返回计算值，假若没有完成，则只会返回空值，假若想要在任何情况之下都会得到后台任务所执行的计算结果，请使用<see cref="GetValue()"/>方法
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As TOut

        ''' <summary>
        ''' 假若后台任务还没有完成，则函数会一直阻塞在这里直到任务执行完毕，假若任务早已完成，则函数会立即返回数据
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValue() As TOut
            If Not Me.TaskRunning Then
                Call Start()
            End If

            Call WaitForExit()
            Return _Value
        End Function

        Sub New(Input As T, Handle As Func(Of T, TOut))
            _Handle = Handle
            _Input = Input
        End Sub

        Public Function Start() As Task(Of T, TOut)
            _TaskComplete = False
            _RunningTask = True
            Call New Threading.Thread(AddressOf __invokeTask).Start()
            Return Me
        End Function

        Protected Overrides Sub __invokeTask()

            _TaskComplete = False
            _RunningTask = True
            _Value = _Handle(_Input)
            _TaskComplete = True
            _RunningTask = False

        End Sub
    End Class

    ''' <summary>
    ''' 这个是带有<see cref="Task.TaskJobComplete"/>事件的任务对象
    ''' </summary>
    Public Class Task : Inherits IParallelTask

        ''' <summary>
        ''' 当所请求的任务执行完毕之后触发
        ''' </summary>
        Public Event TaskJobComplete()

        Dim _Handle As Action

        Sub New(Handle As Action)
            _Handle = Handle
        End Sub

        Protected Overrides Sub __invokeTask()
            _TaskComplete = False
            _RunningTask = True
            Call _Handle()
            _TaskComplete = True
            _RunningTask = False
        End Sub

        Public Function Start() As Task
            _TaskComplete = False
            _RunningTask = True
            Call New Threading.Thread(AddressOf __invokeTask).Start()
            Call Threading.Thread.Sleep(1)
            Call New Threading.Thread(AddressOf RaisingEvent).Start()
            Return Me
        End Function

        Private Sub RaisingEvent()
            Call WaitForExit()
            RaiseEvent TaskJobComplete()
        End Sub
    End Class

    Public Class Task(Of T) : Inherits IParallelTask

        Dim _Input As T
        Dim _Handle As Action(Of T)

        Public Event TaskJobComplete()

        Sub New(Input As T, Handle As Action(Of T))
            _Input = Input
            _Handle = Handle
        End Sub

        Public Function Start() As Task(Of T)
            _TaskComplete = False
            _RunningTask = True
            Call New Threading.Thread(AddressOf __invokeTask).Start()
            Call Threading.Thread.Sleep(1)
            Call New Threading.Thread(AddressOf RaisingEvent).Start()
            Return Me
        End Function

        Private Sub RaisingEvent()
            Call WaitForExit()
            RaiseEvent TaskJobComplete()
        End Sub

        Protected Overrides Sub __invokeTask()
            _TaskComplete = False
            _RunningTask = True
            Call _Handle(_Input)
            _TaskComplete = True
            _RunningTask = False
        End Sub
    End Class
End Namespace
