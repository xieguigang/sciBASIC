#Region "Microsoft.VisualBasic::c34204250eaeef9094e055522fb33ca2, Data_science\Mathematica\Math\Math.Statistics\Example\Fisher.vb"

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

    ' Module Fisher
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics

Module Fisher

    Sub Main()
        Dim p = FisherTest.FisherPvalue(1, 9, 11, 3) '0.0013460761879122358
        Dim p2 = FisherTest.FisherPvalue(0, 10, 12, 2) '3.3651904697805894E-05
        Dim p3 = FisherTest.FisherPvalue(3, 40, 297, 19960) '0.021858115774312393

        Pause()
    End Sub
End Module
