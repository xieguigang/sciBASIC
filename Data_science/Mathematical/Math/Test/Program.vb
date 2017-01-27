Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()

        Call Test(1, 2, 3, z:="a+b")

        Pause()

        Call "x <- 123+3^3!".Evaluate
        ' Call "log((x+699)*9/3!)".Evaluate
        Call "x <- log((x+699)*9/3!)+sin(99)".Evaluate
        Call "x!".Evaluate
        Call "(1+2)!/5".Evaluate

        Pause()
    End Sub

    Sub Test(a!, b&, c#, Optional x$ = "(A + b^2)! * 100", Optional y$ = "(cos(x/33)+1)^2 -3", Optional z$ = "log(-Y) + 9")
        'Dim params As Dictionary(Of String, Double) = ParameterExpression.Evaluate(Function() {a, b, x, y, z})
        'Dim json$ = params.GetJson(True)

        Dim before = {a, b, c, x, y, z}

        Call ParameterExpression.Apply(Function() {a, b, x, y, z})

        Dim after = {a, b, c, x, y, z}

        'Call json.__DEBUG_ECHO
    End Sub
End Module