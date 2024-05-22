#Region "Microsoft.VisualBasic::9b5473ef17970d4763c1bad5a25a7475, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Tasks\BackgroundTask.vb"

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

    '   Total Lines: 79
    '    Code Lines: 52 (65.82%)
    ' Comment Lines: 12 (15.19%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (18.99%)
    '     File Size: 2.34 KB


    '     Delegate Function
    ' 
    ' 
    '     Class BackgroundTask
    ' 
    '         Properties: ExecuteException, TaskHandle, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Start, ToString
    ' 
    '         Sub: Abort, doInvokeTask
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Parallel.Tasks

    ''' <summary>
    ''' 背景线程的任务抽象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Delegate Function IBackgroundTask(Of T)() As T

    Friend Class BackgroundTask(Of T) : Inherits IParallelTask

        ''' <summary>
        ''' 获取得到任务线程执行的输出结果
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As T
            Get
                If Not TaskComplete Then
                    Call Start()
                    Call WaitForExit()
                End If
                Return __getValue
            End Get
        End Property

        Public ReadOnly Property TaskHandle As IBackgroundTask(Of T)

        ReadOnly _taskThread As Threading.Thread

        Dim __getValue As T

        Public ReadOnly Property ExecuteException As Exception

        Public Overrides Function ToString() As String
            Return TaskHandle.ToString
        End Function

        Sub New(task As IBackgroundTask(Of T))
            _TaskHandle = task
            _taskThread = New Threading.Thread(AddressOf doInvokeTask)
        End Sub

        ''' <summary>
        ''' 取消当前的任务的执行，在线程内部产生的异常可以在<see cref="ExecuteException"/>获取得到
        ''' </summary>
        Public Sub Abort()
#Disable Warning
            Call _taskThread.Abort()
#Enable Warning

            TaskComplete = False
            _RunningTask = False
        End Sub

        Public Function Start() As BackgroundTask(Of T)
            If Not TaskRunning Then
                _taskThread.Start()
                TaskComplete = False
            End If

            Return Me
        End Function

        Protected Overrides Sub doInvokeTask()
            Me._RunningTask = True
            Me.TaskComplete = False
            Try
                __getValue = _TaskHandle()
            Catch ex As Exception
                _ExecuteException =
                    New Exception(MethodBase.GetCurrentMethod.GetFullName, ex)
            End Try
            Me._RunningTask = False
            Me.TaskComplete = True
        End Sub
    End Class
End Namespace
