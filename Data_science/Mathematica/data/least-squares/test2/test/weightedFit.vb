Imports Microsoft.VisualBasic.Data.Bootstrapping

Module weightedFitTest

    Sub Main()
        Dim X#() = {0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.82}
        Dim Y#() = {4.09818, 4.39655, 4.61435, 4.95867, 5.26182, 5.55079, 5.84748, 6.11208, 6.5333333}
        Dim W#() = {1, 1, 1, 1, 1, 0.65, 0.1, 0.0023, 0.0000002}

        ' W = 1 / (X.AsVector ^ 2)

        ' Dim fit1 As WeightedFit = WeightedLinearRegression.Regress(X, Y, W, 1)
        Dim fit2 As WeightedFit = WeightedLinearRegression.Regress(X, Y, W, 5)

        '  Console.WriteLine(fit1)
        Console.WriteLine(fit2)

        Pause()
    End Sub
End Module
