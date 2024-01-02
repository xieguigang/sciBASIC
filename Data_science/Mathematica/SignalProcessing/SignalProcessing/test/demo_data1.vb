Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.SignalProcessing.EmGaussian
Imports Microsoft.VisualBasic.Serialization.JSON

Module demo_data1

    Dim v As Double() = {0, 0.1, 0.2, 0.5, 0.9, 1.3, 1.25, 0.99, 0.7, 0.35, 0.4, 0.5, 0.6, 0.65, 0.45, 0.4, 0.35, 0.2, 0.1, 0}

    Sub Main()
        Call fitCurveTest()
    End Sub

    Sub fitMultipleGauss()
        Dim gauss As New GaussianFit(Opts.GetDefault)
        Dim logp As Double() = Nothing
        Dim result = gauss.fit(v, logp:=logp)

        For Each peak In result
            Call Console.WriteLine(peak.GetJson)
        Next

        ' Call Console.WriteLine(result.GetJson)
        Call Console.WriteLine(logp.GetJson)

        Pause()
    End Sub

    Sub fitCurveTest()
        Dim data As DataPoint() = v.Select(Function(vi, i) New DataPoint(i, vi)).ToArray
        Dim gauss As GaussNewtonSolver.FitFunction =
            Function(x As Double, args As NumericMatrix) As Double
                Return pnorm.ProbabilityDensity(x, args(0, 0), args(1, 0))
            End Function
        Dim solver As New GaussNewtonSolver(gauss)
        Dim result = solver.Fit(data, 9, 2)
        Dim m = result(0)
        Dim sd = result(1)

        Dim y2 As Double() = v.Select(Function(vi, i) pnorm.ProbabilityDensity(i, m, sd)).ToArray

        Call Console.WriteLine(v.GetJson)
        Call Console.WriteLine(y2.GetJson)

        Pause()
    End Sub
End Module
