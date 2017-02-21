Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()


        Call AxisScalling.GetAxisValues(98,, -83.4023).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(198,, -83.4023).GetJson.__DEBUG_ECHO

        '   Pause()

        Call AxisScalling.GetAxisValues(98,, 83.4023).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(98,, 3).GetJson.__DEBUG_ECHO

        Call AxisScalling.GetAxisValues(7.9,, 1.3).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(-7.9,, -21.3).GetJson.__DEBUG_ECHO

        Call AxisScalling.GetAxisValues(0.98,, 0.03, [decimal]:=2).GetJson.__DEBUG_ECHO
        Call AxisScalling.GetAxisValues(2.98,, 0.03, [decimal]:=2).GetJson.__DEBUG_ECHO

        Call AxisScalling.GetAxisValues(-0.0198,, -0.903, [decimal]:=2).GetJson.__DEBUG_ECHO

        Pause()

    End Sub

End Module
