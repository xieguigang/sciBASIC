Imports System.Threading
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel.Tasks

    ''' <summary>
    ''' When the task job complete, then the program will notify user code through callback function.
    ''' </summary>
    Public Class CallbackTask

        Public ReadOnly Property Task As Action
        Public ReadOnly Property Callback As Action

        Sub New(task As Action, callback As Action)
            Me.Task = task
            Me.Callback = callback
        End Sub

        Dim __running As Boolean = False
        Dim __cts As New CancellationTokenSource

        Public Sub Cancel()
            Call __cts.Cancel()
        End Sub

        Public Sub Start()
            If __running Then
                Return
            Else
                __running = True
            End If

            Call RunTask(Async Sub() Await __run(__cts))
        End Sub

#Disable Warning
        Private Async Function __run(cts As CancellationTokenSource) As Threading.Tasks.Task
#Enable Warning
            Call Me._Task()
            Call Me._Callback()
            __running = False
        End Function

        Public Overrides Function ToString() As String
            Return New With {
                .Task = Task.ToString,
                .callback = Callback.ToString,
                .Running = __running.ToString
            }.GetJson
        End Function
    End Class
End Namespace