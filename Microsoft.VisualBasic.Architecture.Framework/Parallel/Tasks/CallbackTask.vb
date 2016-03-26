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

        Public Sub Start()
            If __running Then
                Return
            Else
                __running = True
            End If

            Call New Threading.Thread(Sub()
                                          Call Me._Task()
                                          Call Me._Callback()
                                          __running = False
                                      End Sub).Start()
        End Sub

        Public Overrides Function ToString() As String
            Return New With {
                .Task = Task.ToString,
                .callback = Callback.ToString,
                .Running = __running.ToString
            }.GetJson
        End Function
    End Class
End Namespace