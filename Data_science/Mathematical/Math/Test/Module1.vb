Imports Microsoft.VisualBasic.Mathematical.Scripting

Module Module1

    Sub Main()

        Call "x <- 123+3^3!".Evaluate
        Call "log((x+699)*9/3!)".Evaluate

        Pause()

        Call "log((x+699)*9/3!)+sin(99)".Evaluate

        Pause()
    End Sub
End Module
