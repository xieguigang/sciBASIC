Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()

        Call ParameterCompute.Demo(123).GetJson.__DEBUG_ECHO
        Call Test(1, 2)

        Pause()

        Call "x <- 123+3^3!".Evaluate
        ' Call "log((x+699)*9/3!)".Evaluate
        Call "x <- log((x+699)*9/3!)+sin(99)".Evaluate
        Call "x!".Evaluate
        Call "(1+2)!/5".Evaluate

        Pause()
    End Sub

    Sub Test(a#, b#, Optional x$ = "(A + b)! * 100", Optional y# = 33, Optional z$ = "Y + 9")
        Dim parameters As Dictionary(Of String, Double) =
            New Expression(Of Func(Of Object))() {
                Function() a,
                Function() b,
                Function() x,
                Function() y,
                Function() z
        }.Evaluate

        Dim json$ = parameters.GetJson(True)

        Call json.__DEBUG_ECHO
    End Sub
End Module
