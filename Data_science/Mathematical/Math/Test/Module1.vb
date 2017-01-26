Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()

        Call ParameterCompute.Demo(123).GetJson.__DEBUG_ECHO

        Pause()

        Call "x <- 123+3^3!".Evaluate
        ' Call "log((x+699)*9/3!)".Evaluate
        Call "x <- log((x+699)*9/3!)+sin(99)".Evaluate
        Call "x!".Evaluate
        Call "(1+2)!/5".Evaluate

        Pause()
    End Sub
End Module
