#Region "Microsoft.VisualBasic::0cb14b35413196d04634cbeb887b9907, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Tasks\CallbackTask.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Interface ICallbackTask
    ' 
    '         Properties: CallbackInvoke
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
