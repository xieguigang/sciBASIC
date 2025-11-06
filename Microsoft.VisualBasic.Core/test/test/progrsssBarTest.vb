Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar

Module progrsssBarTest

    Sub testLoop()
        Const max = 500

        'Create the ProgressBar
        ' Maximum: The Max value in ProgressBar (Default is 100)
        Using pb = New ProgressBar() With {
            .Maximum = Nothing
        }
            For i = 0 To max - 1
                Call Task.Delay(10).Wait() 'Do something
                pb.PerformStep() 'Step in ProgressBar (Default is 1)
            Next
        End Using

        Pause()
    End Sub
End Module
