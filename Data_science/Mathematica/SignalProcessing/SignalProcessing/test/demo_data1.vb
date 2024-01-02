Imports Microsoft.VisualBasic.Math.SignalProcessing.EmGaussian
Imports Microsoft.VisualBasic.Serialization.JSON

Module demo_data1

    Sub Main()

        Dim v = {0, 0.1, 0.2, 0.5, 0.9, 1.3, 1.25, 0.99, 0.7, 0.35, 0.4, 0.5, 0.6, 0.65, 0.45, 0.4, 0.35, 0.2, 0.1, 0}
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
End Module
