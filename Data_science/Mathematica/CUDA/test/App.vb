Imports Microsoft.VisualBasic.Math

Module App

    Public Declare Function GetPearson Lib "sciKernel.dll" Alias "pearson" (ByRef x As Double(), ByRef y As Double()) As Double

    Sub Main()
        Dim x As Double() = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
        Dim y As Double() = {10, 9, 8, 7, 6, 5, 4, 3, 2, 1}

        Dim pcc1 = GetPearson(x, y)
        Dim pcc2 = Correlations.GetPearson(x, y)

        Call Console.WriteLine($"pcc from rust => {pcc1}")
        Call Console.WriteLine($"pcc from vb.net => {pcc2}")

        Pause()
    End Sub

End Module
