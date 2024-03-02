Imports Microsoft.VisualBasic.Data.Bootstrapping

Module fit_test2

    Sub Main()
        Dim b As New BayesianCurveFitting({1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, 2)

        Dim y = b.getMx(5)

        Pause()
    End Sub
End Module
