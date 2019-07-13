#Region "Microsoft.VisualBasic::36ba2ab99112b3987e5604d0e8a9094b, Data_science\Mathematica\data\Student's T-test\t.test\Program.vb"

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

    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        Dim a#() = {115, 108, 108, 119, 105, 101, 120, 115, 104, 100.9}
        Dim b#() = {185, 169, 173, 173, 188, 186, 175, 174, 179, 180}

        With t.Test(a, b)
            Call $"alternative hypothesis: { .Valid}".__DEBUG_ECHO
            Call .GetJson(True).__DEBUG_ECHO
        End With

        Dim x#() = {0, 1, 1, 1}

        With t.Test(x, mu:=1)
            Call $"alternative hypothesis: { .Valid}".__DEBUG_ECHO
            Call .GetJson(True).__DEBUG_ECHO
        End With

        a = {6846523.253, 6840877.665, 5806323.704}
        b = {3056565.388, 1831431.105, 2933659.497}

        Call t.Test(a, b).GetJson(indent:=True).__DEBUG_ECHO
        Call t.Test(a, b, varEqual:=False).GetJson(indent:=True).__DEBUG_ECHO


        Pause()
    End Sub
End Module
