Namespace NeuralNetwork

    ''' <summary>
    ''' some console api is not working well on unix platform,
    ''' the helper is used for solve such problem
    ''' </summary>
    Public Class Reporter

        ReadOnly trainer As ANNTrainer

        Sub New(trainer As ANNTrainer)
            Me.trainer = trainer
        End Sub

        Public Sub DoReport(i As Integer, errors As Double())
            If App.IsMicrosoftPlatform Then
                Call ReportWindows()
            Else
                Call ReportUnix()
            End If
        End Sub

        Private Sub ReportWindows()

        End Sub

        Private Sub ReportUnix()

        End Sub
    End Class
End Namespace