#Region "Microsoft.VisualBasic::0cb14b35413196d04634cbeb887b9907, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Parallel\Tasks\CallbackTask.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Parallel.Tasks

    Public Interface ICallbackTask
        ReadOnly Property CallbackInvoke As Action
    End Interface

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
