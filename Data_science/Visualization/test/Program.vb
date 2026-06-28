Imports Microsoft.VisualBasic.Imaging.Driver

Module Program

    Sub Main()
        Call ImageDriver.Register()
        Call Examples.RunAll("Z:/")
    End Sub
End Module
