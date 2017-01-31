Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        Call __stringExpression()

        Test(1, 2, 3, z:="a+b")
        Test(2, 3, 1, y:="(a+b)*c", z:="x+y")
        Pause()

        Dim math As New Expression
        Dim result# = math.Evaluation("(cos(x/33)+1)^2-3")
    End Sub

    Private Sub __stringExpression()
        With New Expression
            Call .SetVariable("x", "23")
            Call .SetVariable("y", "x*3")
            Call .SetVariable("z23", "Y*23")
            Call "\$ $x*3\n\t=$y \n$y*23=$z23".Interpolate(AddressOf .GetValue).__DEBUG_ECHO
        End With
    End Sub

    Function Test(a!, b&, c#,
                  Optional x$ = "(A + b^2)! * 100",
                  Optional y$ = "(cos(x/33)+1)^2 -3",
                  Optional z$ = "log(-Y) + 9",
                  Optional title$ = "@This is title string interpolate test: \$z value is $z") As (before As Object(), after As Object())

        Dim before As New Value(Of Object()), after As New Value(Of Object())

        Call $"Parameter before the expression evaluation is: { (before = {a, b, c, x, y, z}).GetJson }".__DEBUG_ECHO
        Call ParameterExpression.Apply(Function() {a, b, c, x, y, z})
        Call $"Parameters after the expression evaluation is: { (after = {a, b, c, x, y, z}).GetJson }".__DEBUG_ECHO

        Return (+before, +after)
    End Function
End Module