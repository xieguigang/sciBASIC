#Region "Microsoft.VisualBasic::d42e865588a0ba13d09d6f8fbd4c03c9, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Tasks\Task.vb"

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

    '   Total Lines: 83
    '    Code Lines: 52 (62.65%)
    ' Comment Lines: 16 (19.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (18.07%)
    '     File Size: 2.66 KB


    '     Class Task
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetValue, Start
    ' 
    '         Sub: doInvokeTask
    ' 
    '     Class Task
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Start
    ' 
    '         Sub: doInvokeTask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' 假若任务已经完成，则会返回计算值，假若没有完成，则只会返回空值，
        ''' 假若想要在任何情况之下都会得到后台任务所执行的计算结果，
        ''' 请使用<see cref="GetValue()"/>方法
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As TOut

        ''' <summary>
        ''' 假若后台任务还没有完成，则函数会一直阻塞在这里直到任务执行完毕，
        ''' 假若任务早已完成，则函数会立即返回数据
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValue() As TOut
            If Not Me.TaskRunning Then
                Call Start()
            End If

            Call WaitForExit()
            Return Value
        End Function

        Sub New(Input As T, Handle As Func(Of T, TOut))
            _Handle = Handle
            _Input = Input
        End Sub

        Public Function Start() As Task(Of T, TOut)
            TaskComplete = False
            _RunningTask = True
            Call RunTask(AddressOf doInvokeTask)
            Return Me
        End Function

        Protected Overrides Sub doInvokeTask()
            TaskComplete = False
            _RunningTask = True
            _Value = _Handle(_Input)
            TaskComplete = True
            _RunningTask = False
        End Sub
    End Class

    Public Class Task(Of T) : Inherits IParallelTask

        Dim inputVal As T
        Dim handleTask As Action(Of T)

        Public Event TaskJobComplete()

        Sub New(Input As T, Handle As Action(Of T))
            inputVal = Input
            handleTask = Handle
        End Sub

        Public Function Start() As Task(Of T)
            Call RunTask(AddressOf doInvokeTask)
            Return Me
        End Function

        Protected Overrides Sub doInvokeTask()
            TaskComplete = False
            _RunningTask = True
            Call handleTask(inputVal)
            TaskComplete = True
            _RunningTask = False

            RaiseEvent TaskJobComplete()
        End Sub
    End Class
End Namespace
