Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Emit.Parameters

Module Program

    Sub Main()
        Call Test(1, 2)

        Pause()

        Call "x <- 123+3^3!".Evaluate
        ' Call "log((x+699)*9/3!)".Evaluate
        Call "x <- log((x+699)*9/3!)+sin(99)".Evaluate
        Call "x!".Evaluate
        Call "(1+2)!/5".Evaluate

        Pause()
    End Sub

    Sub Test(a#, b#, Optional x$ = "(A + b)! * 100", Optional y# = 33, Optional z$ = "log(Y) + 9")
        Dim params As Dictionary(Of String, Double) = Parameters.Evaluate(Function() {a, b, x, y, z})
        Dim json$ = params.GetJson(True)
        Call json.__DEBUG_ECHO
    End Sub
End Module