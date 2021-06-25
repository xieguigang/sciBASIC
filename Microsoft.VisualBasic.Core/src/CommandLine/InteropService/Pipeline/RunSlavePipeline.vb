Namespace CommandLine.InteropService.Pipeline

    Public Class RunSlavePipeline

        Public Event SetProgress(percentage As Double, details As String)
        Public Event SetMessage(message As String)

        ReadOnly app As String
        ReadOnly arguments As String

        Sub New(app$, arguments$)
            Me.app = app
            Me.arguments = arguments
        End Sub

        Public Function Run() As Integer
            Return PipelineProcess.ExecSub(app, arguments, AddressOf ProcessMessage)
        End Function

        Private Sub ProcessMessage(line As String)
            If line.StringEmpty Then
                Return
            End If

            If line.StartsWith("[SET_MESSAGE]") Then
                ' [SET_MESSAGE] message text
                RaiseEvent SetMessage(line.GetTagValue(" ", trim:=True).Value)
            ElseIf line.StartsWith("[SET_PROGRESS]") Then
                ' [SET_PROGRESS] percentage message text
                Dim data = line.GetTagValue(" ", trim:=True).Value.GetTagValue(" ", trim:=True)
                Dim percentage As Double = Val(data.Name)
                Dim message As String = data.Value

                RaiseEvent SetProgress(percentage, message)
            End If
        End Sub

        Public Shared Sub SendMessage(message As String)
            Call Console.WriteLine($"[SET_MESSAGE] {message}")
        End Sub

        Public Shared Sub SendProgress(percentage As Double, message As String)
            Call Console.WriteLine($"[SET_PROGRESS] {percentage} {message}")
        End Sub

    End Class
End Namespace