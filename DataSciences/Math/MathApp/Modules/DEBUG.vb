#Region "Microsoft.VisualBasic::4e4ef6d8832b17b9ca3facc0acc20e21, ..\visualbasic_App\Scripting\Math\MathApp\Modules\DEBUG.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic
Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Module DEBUG

    Public Class [Integer] : Inherits ClassObject
        Dim n As Integer

        Sub New(x As Integer)
            n = x
        End Sub

        Public Overrides Function ToString() As String
            Return n
        End Function
    End Class

    Public Iterator Function xxx() As IEnumerable(Of [Integer])
        Yield New [Integer](1)
        Yield New [Integer](2)
        Yield New [Integer](3)
    End Function

    Public Function Main() As Integer
        '        Call ODEsTest.test()


        Call {
            New NamedValue(Of Integer)("s1", 123),
            New NamedValue(Of Integer)("s2", 235),
            New NamedValue(Of Integer)("s3", 99),
            New NamedValue(Of Integer)("s4", 499),
            New NamedValue(Of Integer)("s5", 499)
        }.FromData().Plot().SaveAs("./pie_chart.png")


        Dim ode As New ODE With {
            .df = Function(x, y) Math.Cos(x),
            .y0 = 0.540302
        }
        Dim ode2 As New ODE With {
            .df = Function(x, y) Math.Sin(x),
            .y0 = Math.Sin(0)
        }
        Call ode.RK4(50, 1, 10)
        Call ode2.RK4(50, 1, 10)

        Dim serials = {ode.FromODE("red"), ode2.FromODE("lime", DashStyle.Solid)}

        Call Scatter.Plot(serials).SaveAs("./cos.png")
        Call Histogram.Plot(Histogram.FromODE(ode, ode2)).SaveAs("./cos.hist.png")

        Pause()

        Dim water As New LinguisticVariable("Water")
        water.MembershipFunctionCollection.Add(New MembershipFunction("Cold", 0, 0, 20, 40))
        water.MembershipFunctionCollection.Add(New MembershipFunction("Tepid", 30, 50, 50, 70))
        water.MembershipFunctionCollection.Add(New MembershipFunction("Hot", 50, 80, 100, 100))

        Dim power As LinguisticVariable = New LinguisticVariable("Power")
        power.MembershipFunctionCollection.Add(New MembershipFunction("Low", 0, 25, 25, 50))
        power.MembershipFunctionCollection.Add(New MembershipFunction("High", 25, 50, 50, 75))

        Dim FuzzyEngine As New FuzzyEngine()
        FuzzyEngine.LinguisticVariableCollection.Add(water)
        FuzzyEngine.LinguisticVariableCollection.Add(power)
        FuzzyEngine.Consequent = "Power"
        FuzzyEngine.FuzzyRuleCollection.Add(New FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High"))
        FuzzyEngine.FuzzyRuleCollection.Add(New FuzzyRule("IF (Water IS Hot) THEN Power IS Low"))

        water.InputValue = 60

        Dim xml As String = "fuzzyModel.xml"

        Call FuzzyEngine.Save(xml, Encodings.UTF8)

        FuzzyEngine = Nothing
        FuzzyEngine = Models.FuzzyModel.FromXml(xml)

        Try
            MsgBox(FuzzyEngine.Defuzzify().ToString())
        Catch ex As Exception
            Call ex.PrintException
        End Try



        Dim f As String = "f(x,y,z,aa) x+y+z!+2*aa"
        Dim func = Mathematical.FuncParser.TryParse(f)

        Call Mathematical.Expression.DefaultEngine.Functions.Add(f)
        Call Mathematical.Expression.Evaluate("f(1,2,3,3)").__DEBUG_ECHO

        Dim ls As String = "a and not (andb xor 99>2)"
        ls = "(Water IS Cold) OR (Water IS Tepid)"
        Dim rr = Logical.TokenIcer.TryParse(ls)
        Dim sr = Logical.TokenIcer.Split(rr)

        Dim s As String = "(1+2+3)%(-563.999*6/44)"
        '   Dim list = Mathematical.Helpers.TokenIcer.TryParse(s)


        s = "f(3+6,59,12)+(1+2^2)*3!+PI/PI"
        s = "3!*9"
        s = "min(0,-9)"
        s = "-9*-9"
        s = ExpressionParser.TryParse(s).Evaluate


        s = "1+2+3!+4+5-6/2.5"
        Dim eee = SimpleParser.TryParse(s)
        Dim v = eee.Evaluate

        Dim s2 As String = "((0+69sdfss+fs*(d+f)*w+efsd+f)+sdfs*(dfsdf+w)*e+f+sdf+sd(dd+f,rt)+fsd)"
        s2 = "f(-10,6)+f2(f(0,0),2)"
        Dim expr = Scripting.TokenIcer.MathExpression(s2)
        Call expr.PrintStack
    End Function
End Module
'
'        Dim br As String = "\([^(^)]+?\)"
'        Dim ms = Text.RegularExpressions.Regex.Matches(s2, br)
'        Dim FunctionCalling As String = "[0-9a-zA-Z_]+" & br
'        Dim fs = Text.RegularExpressions.Regex.Matches(s2, FunctionCalling)

'        Dim v As VECTOR = "{3,4,5,6,7,8,99,910}"
'        Dim v2 As VECTOR = {3, 4, 6, 2, 234, 241, 1}

'        Dim v3 = v2 + 4

'        Console.WriteLine("{0} + {1} ={2}  {3}", v2, 4, vbCrLf, v3)

'        Dim m As MATRIX = {
'            {1, 2, 3, 4, -1, 555},
'            {-333, 5, 6, 7, 8, 21},
'            {32, 9, 10, 11, 12, 1000}}
'        m = v2 + 5

'        m = "MAT(width:=2,height:=3){1,2,3,4,5,6}"
'        m = "MAT(2,3){5,6,7,8,9,0}"

'        m = m + 5
'        m = m - 5
'        m = 2 * m - m * 2.5
'        m = m & m.T

'        Dim GaussianElimination As New BasicR.Solvers.GaussianElimination
'        Dim SOR As New BasicR.Solvers.SOR

'        Dim A As Microsoft.VisualBasic.Mathematical.BasicR.MATRIX =
'            "MAT(width:=4,height:=4){-13.9211,21.7540,-14.8743,-7.9025,18.3862,-26.0893,-5.6866,4.4451,-4.1683,3.9325,-33.3169,41.7098,-5.1683,55.9325,33.3169,21.7098}"
'        Dim b As Microsoft.VisualBasic.Mathematical.BasicR.VECTOR =
'            "{136.8721,-126.8849,100.4524,95.7019}"
'        Dim x As Microsoft.VisualBasic.Mathematical.BasicR.VECTOR = GaussianElimination.Solve(A, b)

'        A = "MAT(3,3){12,10,-7,6,5,3,24,-1,5}"
'        b = "(15,14,28)"
'        x = GaussianElimination.Solve(A, b)

'        A = "MAT(4,4){5,-1,-1,-1,-1,10,-1,-1,-1,-1,5,-1,-1,-1,-1,10}"
'        b = "{-4,12,8,34}"
'        x = SOR.Solve(A, b)

'        Dim MA As BasicR.Helpers.MatrixArithmetic = New BasicR.Helpers.MatrixArithmetic

'        A = MA.Evaluate("MAT(2,2){1,2,3,4}", "MAT(2,2){5,6,7,8}", "^")

'        If (True And True Or False) And Not (True And True) And Not Not True And False Then
'            Console.WriteLine(True)
'        End If

'        If Not True And True Then
'            Console.WriteLine(True)
'        End If

'        Dim s As String = "true and 2.1~2.11 and not pow(3,3)>rnd(0,100) or (true and true or false) and not (true and true) and not true and false or not (true and (true or false))"

'        Console.WriteLine(Mathematical.Helpers.Logical.Evaluate(s))


'        'Helpers.Function.Add("f", "x+2*y!+pi")
'        'Helpers.Function.Add("f2", Function(x As Double, y As Double) (x + y) ^ 3)
'        'Console.WriteLine(Expression.Evaluate("f(-10,6)+f2(f(0,0),2)"))

'        'Dim expr As String = "e+32-pio+rpi+pi+pi-e+pi+7!+opi-pi"

'        'Helpers.Constants.Add("pio", 111)
'        'Helpers.Constants.Add("rpi", 0)
'        'Helpers.Constants.Add("opi", 34)

'        'expr = Helpers.Constants.Replace(expr)

'        ''   a = Expression.Evaluate("(1+3+9)*9/(36%5+2^3)%3!")
'        ''    Console.WriteLine(a)
'        ''a = Expression.Evaluate("max((3+9),(8+6)*(9+3^2))*cos(30+5^(4!+sin(664/9^2)))+69%6")
'        'Console.WriteLine(Expression.Evaluate("sin(cos(max((8+8)*3,32)%5))"))
'        'Console.WriteLine(Expression.Evaluate("8^(1/-2)"))
'        'Console.WriteLine(Expression.Evaluate("rnd(-15,9)"))

'        Return Console.Read()
'    End Function
'End Module
