#Region "Microsoft.VisualBasic::a1ca97083d30f4a5bd0e650546d6e8d4, sciBASIC#\docs\guides\parameter_expression\Demo\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 52
    '    Code Lines: 39
    ' Comment Lines: 1
    '   Blank Lines: 12
    '     File Size: 2.11 KB


    ' Module Program
    ' 
    '     Function: Test
    ' 
    '     Sub: __stringExpression, Main, StringTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        ' Call __stringExpression()

        Test(1, 2, 3, z:="a+b")
        Test(2, 3, 1, y:="(a+b)*c", z:="x+y")
        StringTest(100.23)
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

    Sub StringTest(z#,
                   Optional pvalue# = 0.005,
                   Optional title$ = "@This is title string interpolate test: \$z value is $z",
                   Optional ylabel$ = "@Plots of the experiment data with \np-value cutoff: $pvalue, \nand z-value: $z (km/s).")

        Call $"Parameter before the expression evaluation is: { ({z, pvalue, title, ylabel}).GetJson }".__DEBUG_ECHO
        Call ParameterExpression.Apply(Function() {z, pvalue, title, ylabel})
        Call $"Parameters after the expression evaluation is: { ({z, pvalue, title, ylabel}).GetJson(True) }".__DEBUG_ECHO

    End Sub

    Function Test(a!, b&, c#,
                  Optional x$ = "(A + b^2)! * 100",
                  Optional y$ = "(cos(x/33)+1)^2 -3",
                  Optional z$ = "log(-Y) + 9") As (before As Object(), after As Object())

        Dim before As New Value(Of Object()), after As New Value(Of Object())

        Call $"Parameter before the expression evaluation is: { (before = {a, b, c, x, y, z}).GetJson }".__DEBUG_ECHO
        Call ParameterExpression.Apply(Function() {a, b, c, x, y, z})
        Call $"Parameters after the expression evaluation is: { (after = {a, b, c, x, y, z}).GetJson(True) }".__DEBUG_ECHO

        Return (+before, +after)
    End Function
End Module
