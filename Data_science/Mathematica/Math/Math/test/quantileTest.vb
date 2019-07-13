#Region "Microsoft.VisualBasic::43043136898fc0034c6f09b4be51bfe6, Data_science\Mathematica\Math\Math\test\quantileTest.vb"

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

    ' Module quantileTest
    ' 
    '     Sub: Main, quartile
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile

Module quantileTest

    Sub Main()

        Call quantileTest.quartile()


        Dim q = New Double() {5, 100, 200, 2000, 300, 20, 20, 20, 20, 3000, 9999999, 1, 1, 1, 1, 1, 99}.GKQuantile

        For Each l In {0, 0.25, 0.5, 0.75, 1}
            Call q.Query(l).__DEBUG_ECHO
        Next
    End Sub

    Sub quartile()
        Dim test = Sub(x As Vector)
                       Dim q = x.Quartile
                       Dim div = x.AsVector.Outlier(q)
                   End Sub

        Call test({3465, 345, 73, 495})
        Call test({3, 46, 5, 345, 73, 49, 5})
        Call test({3465, 3, 45, 73, 49, 5})
        Call test({3465, 345, 73495})
        Call test({3465, 34573495})
        Call test({3465345})


        Pause()
    End Sub
End Module
