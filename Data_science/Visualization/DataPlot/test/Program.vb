Imports Microsoft.VisualBasic.Imaging.Driver

Module Program
    Sub Main(args As String())
        Call ImageDriver.Register()
        Call Examples.RunAll("Z:/data-plots")
    End Sub
End Module
