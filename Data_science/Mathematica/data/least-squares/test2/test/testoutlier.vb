Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Data.Bootstrapping.Outlier

Module testoutlier

    Sub Main()

        Dim x As Vector = {0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649}

        Dim index = x.OutlierIndex.ToArray

        Pause()

    End Sub
End Module
