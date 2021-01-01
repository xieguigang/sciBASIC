#Region "Microsoft.VisualBasic::beb89ab911b1d121d3d9c05fcad1aea3, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Tasks\CallbackTask.vb"

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

    '     Interface ICallbackTask
    ' 
    '         Properties: CallbackInvoke
    ' 
    '     Class RevokableTask
    ' 
    ' 
    ' 
    '     Class RevokableTaskLoop
    ' 
    '         Sub: RunTask
    ' 
    '     Class RevokableTaskLoop
    ' 
    '         Function: RunTask
    ' 
    '         Sub: populate
    ' 
    '     Class CallbackTask
    ' 
    '         Properties: Task
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Cancel, Start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Parallel.Tasks

    Public Interface ICallbackTask
        ReadOnly Property CallbackInvoke As Action
    End Interface

    Public MustInherit Class RevokableTask

        Protected cancel As Boolean = False

    End Class

    ''' <summary>
    ''' 可以被取消的循环对象
    ''' </summary>
    Public MustInherit Class RevokableTaskLoop : Inherits RevokableTask

        Protected loopIndex As Integer

        Public Sub RunTask()
            Do While Not cancel
                [loop]()
                [loopIndex] += 1
            Loop
        End Sub

        Protected MustOverride Sub [loop]()

    End Class

    ''' <summary>
    ''' 可以被取消的循环对象
    ''' </summary>
    Public MustInherit Class RevokableTaskLoop(Of T) : Inherits RevokableTask

        Protected loopIndex As Integer

        Dim populateVal As T
        Dim reset As Boolean

        Public Iterator Function RunTask() As IEnumerable(Of T)
            Do While Not cancel
                [loop]()
                [loopIndex] += 1

                If reset Then
                    Yield populateVal
                    reset = False
                End If
            Loop
        End Function

        Protected MustOverride Sub [loop]()
        Protected Sub populate(val As T)
            populateVal = val
            reset = True
        End Sub

    End Class

    ''' <summary>
    ''' When the task job complete, then the program will notify user code through callback function.
    ''' </summary>
    Public Class CallbackTask : Inherits ICallbackInvoke

        Public ReadOnly Property Task As Action

        Sub New(task As Action, callback As Action)
            Call MyBase.New(callback)
            Me.Task = task
        End Sub

        Dim __running As Boolean = False
        Dim __cts As New CancellationTokenSource

        Public Sub Cancel()
            Call __cts.Cancel()
        End Sub

        Public Sub Start()
#If NET_40 = 0 Then
            If __running Then
                Return
            Else
                __running = True
            End If

            Call RunTask(Async Sub() Await __run(__cts))
#End If
        End Sub

#If NET_40 = 0 Then
#Disable Warning
        Private Async Function __run(cts As CancellationTokenSource) As Threading.Tasks.Task
#Enable Warning
            Call Me._Task()
            Call Me._execute()
            __running = False
        End Function
#End If
        Public Overrides Function ToString() As String
            Return New With {
                .Task = Task.ToString,
                .callback = CallbackInvoke.ToString,
                .Running = __running.ToString
            }.GetJson
        End Function
    End Class
End Namespace
