#Region "Microsoft.VisualBasic::4ba37d35a89a53e730191af6f9a56639, Data_science\Mathematica\data\least-squares\test2\test\testoutlier.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module testoutlier
    ' 
    '     Sub: Main, orderSeq
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Bootstrapping.Outlier
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Module testoutlier

    Sub Main()

        Call orderSeq()

        Dim x As Vector = {0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649}

        '  Dim index = x.OutlierIndex.ToArray


        x = {0, 0.010228592, 12.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, 0.051605649, 0.091605649, 0.851605649, 2.55555, 0.051655649, 0.51655649, 0.8051655649, 1.00151655649, 3.9}

        '  index = x.OutlierIndex.ToArray

        '   Dim xy = index.RemovesOutlier(x.AsVector, x.AsVector)


        Call x.ToArray.GetJson(indent:=True).__DEBUG_ECHO
        ' Call xy.X.ToArray.GetJson(indent:=True).__DEBUG_ECHO

        Pause()

    End Sub

    Sub orderSeq()
        Dim x As Vector = {10, -3, 0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, -10, 1}
        Dim Y As Vector = {9, -5, 10.010228592, 2.578282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, -11, 1.2222}

        Dim index = x.OrderSequenceOutlierIndex.ToArray
        Dim line = x.Select(Function(p, i) New PointF(p, Y(i))).ToArray

        Dim xy = line.DeleteOutlier.ToArray

        Call xy.SaveTo("./line.csv")
        Call line.SaveTo("./line-raw.csv")

        Pause()
    End Sub
End Module
