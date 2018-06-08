Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Data.Bootstrapping.Outlier
Imports Microsoft.VisualBasic.Serialization.JSON

Module testoutlier

    Sub Main()

        Call orderSeq()

        Dim x As Vector = {0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649}

        Dim index = x.OutlierIndex.ToArray


        x = {0, 0.010228592, 12.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, 0.051605649, 0.091605649, 0.851605649, 2.55555, 0.051655649, 0.51655649, 0.8051655649, 1.00151655649, 3.9}

        index = x.OutlierIndex.ToArray

        Dim xy = index.RemovesOutlier(x.AsVector, x.AsVector)


        Call x.ToArray.GetJson(indent:=True).__DEBUG_ECHO
        Call xy.X.ToArray.GetJson(indent:=True).__DEBUG_ECHO

        Pause()

    End Sub

    Sub orderSeq()
        Dim x As Vector = {0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649}

        Dim index = x.OrderSequenceOutlierIndex.ToArray

        Dim xy = index.RemovesOutlier(x.AsVector, x.AsVector)

        Pause()
    End Sub
End Module
