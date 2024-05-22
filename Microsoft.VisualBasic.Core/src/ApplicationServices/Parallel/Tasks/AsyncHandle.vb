#Region "Microsoft.VisualBasic::df3f3d6bc0e4aef67d91a551a747f264, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Tasks\AsyncHandle.vb"

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

    '   Total Lines: 99
    '    Code Lines: 44 (44.44%)
    ' Comment Lines: 40 (40.40%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 15 (15.15%)
    '     File Size: 3.62 KB


    '     Class AsyncHandle
    ' 
    '         Properties: IsCompleted, StartTicks, Task
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTaskExecTimeSpan, GetValue, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading

Namespace Parallel.Tasks

    ''' <summary>
    ''' Represents the status of an asynchronous operation.(背景线程加载数据)
    ''' </summary>
    ''' <typeparam name="TOut"></typeparam>
    ''' <remarks>
    ''' 这个后台任务模块是为了更加方便的构建出匿名方法的调用过程，因为这个对象的
    ''' 工作原理是基于匿名方法的``BeginInvoke``函数来工作的。
    ''' </remarks>
    Public Class AsyncHandle(Of TOut)

        ''' <summary>
        ''' 封装一个方法，该方法不具有参数，且返回由<typeparamref name="TOut"></typeparamref>参数指定的类型的值。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Task As Func(Of TOut)
        ' ''' <summary>
        ' ''' 对后台任务的访问
        ' ''' </summary>
        ' ''' <returns></returns>
        ' Public ReadOnly Property Handle As IAsyncResult

        Dim handle As TOut

        ''' <summary>
        ''' Gets a value that indicates whether the asynchronous operation has completed.
        ''' (获取一个值，该值指示异步操作是否已完成。)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsCompleted As Boolean
        Public ReadOnly Property StartTicks As Double

        ''' <summary>
        ''' Creates a new background task from a function handle.
        ''' </summary>
        ''' <param name="Task"></param>
        Sub New(Task As Func(Of TOut))
            Me.Task = Task
            Me.IsCompleted = True
        End Sub

        ''' <summary>
        ''' Start the background task thread.(启动后台背景线程)
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As AsyncHandle(Of TOut)
            If IsCompleted Then
                handle = Nothing

                _IsCompleted = False
                _StartTicks = App.ElapsedMilliseconds

                ' 假若没有执行完毕也调用的话，会改变handle
                ' _Handle = Task.BeginInvoke(Nothing, Nothing)
                ' due to the reason of platform not supported on unix .net 5
                ' use thread model instead of the async model
                Call New Thread(
                    Sub()
                        handle = _Task()
                        _IsCompleted = True
                    End Sub).Start()
            End If

            Return Me
        End Function

        ''' <summary>
        ''' Current thread will be blocked at here if the background task not finished.
        ''' (没有完成的时候会一直在这里阻塞当前的线程)
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValue() As TOut
            If Handle Is Nothing Then
                Return _Task()
            Else
                ' Return Task.EndInvoke(Handle)
                Do While Not IsCompleted
                    Call Thread.Sleep(1)
                Loop

                Return handle
            End If
        End Function

        Public Function GetTaskExecTimeSpan() As TimeSpan
            Dim dtMs As Double = App.ElapsedMilliseconds - StartTicks
            Dim span As TimeSpan = TimeSpan.FromMilliseconds(dtMs)

            Return span
        End Function

        Public Async Function GetValueAsyn() As Threading.Tasks.Task(Of TOut)
            Return Await New Threading.Tasks.Task(Of TOut)(AddressOf GetValue)
        End Function
    End Class
End Namespace
