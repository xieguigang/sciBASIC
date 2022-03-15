#Region "Microsoft.VisualBasic::c60e2faf059cf1476ebed585647a59f1, sciBASIC#\Data_science\Mathematica\Math\MathApp\Modules\DEBUG.vb"

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

    '   Total Lines: 535
    '    Code Lines: 264
    ' Comment Lines: 141
    '   Blank Lines: 130
    '     File Size: 22.25 KB


    ' Module DEBUG
    ' 
    '     Function: avadsfdsfds, Main, xxx
    ' 
    '     Sub: bubblePlots, pieChartTest, randdddTest, scatterWithAnnotation, ShowCharacterData
    '     Class [Integer]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.EstimatesProtocol
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo.Example
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

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

    Sub bubblePlots()
        Dim test As New List(Of csv.SerialData)
        Dim rnd As New Random

        For i = 0 To 300
            test += New csv.SerialData With {.value = rnd.[Next](10, 100), .serial = rnd.Next(1, 10), .X = rnd.Next(1, 4000), .Y = rnd.Next(1, 3000)}
        Next

        Call test.SaveTo("G:\GCModeller\src\runtime\visualbasic_App\DataSciences\Math\images\BubbleTest.csv")
        Call Bubble.Plot(csv.SerialData.GetData("G:\GCModeller\src\runtime\visualbasic_App\DataSciences\Math\images\BubbleTest.csv"), legend:=False).Save("./Bubble.png")
    End Sub


    Private Sub ShowCharacterData()
        'Dim result = ODEsOut.LoadFromDataFrame("G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\bootstrapping\test\Kinetics_of_influenza_A_virus_infection_in_humans.csv")
        'Dim analysis = GetAnalysis(result)
        'Dim i = analysis("I")(result.y("I").x)
        'Dim t = analysis("T")(result.y("T").x)
        'Dim v = analysis("V")(result.y("V").x)
    End Sub

    '''' <summary>
    '''' 通过数据特征来分析结果
    '''' </summary>
    '''' <returns></returns>
    'Public Function GetAnalysis(odes As ODEsOut) As Dictionary(Of String, GetPoints)
    '    Dim dx As Double = odes.dx
    '    Dim I As GetPoints = Function(data)
    '                             Dim a = data.FirstIncrease(dx)
    '                             Dim iMax = data.MaxIndex
    '                             Dim z = data.Skip(iMax).Reach(data.First) + iMax
    '                             Return {a, iMax, z}
    '                         End Function
    '    Dim T As GetPoints = Function(data)
    '                             Dim a = data.FirstDecrease
    '                             Dim b = data.Reach(data.First * 0.01)
    '                             Return {a, b}
    '                         End Function
    '    Dim V As GetPoints = Function(data)
    '                             Dim a = data.FirstIncrease(dx)
    '                             Dim b = data.MaxIndex
    '                             Return {a, b}
    '                         End Function

    '    Return New Dictionary(Of String, GetPoints) From {
    '        {NameOf(I), I},
    '        {NameOf(T), T},
    '        {NameOf(V), V}
    '    }
    'End Function

    Public Sub scatterWithAnnotation()
        Dim s As New SerialData With {
            .color = Color.LightSkyBlue,
            .lineType = DashStyle.Dot,
            .PointSize = 1,
            .title = "223 - legend",
            .width = 3,
            .pts = {New PointData With {.pt = New PointF(-10.1, 32)}, New PointData With {.pt = New PointF(0.1, 2)}, New PointData With {.pt = New PointF(2, 20)}, New PointData With {.pt = New PointF(5, 12)}},
            .DataAnnotations = {New Annotation With {.Font = CSSFont.Win10Normal, .Legend = LegendStyles.Pentacle, .Text = "就是这2", .X = 2, .color = "yellow"}, New Annotation With {.Font = CSSFont.Win10Normal, .Legend = LegendStyles.Diamond, .Text = "就是这", .X = 5}}
        }

        Call Scatter.Plot({s}).Save("x:\ffff.png")

    End Sub

    Private Sub randdddTest()
        Dim rrr As New Random
        Dim ll As New List(Of Double)

        For i = 0 To 10000
            ll += Math.Log10(rrr.NextDouble)
        Next

        Call ll.GetJson.SaveTo("x:\sfdsfds.json")
        '   MsgBox(ll.Min)

        Call RandomRange.Testing(-1.0E+101, -1.22E-18).GetJson.SaveTo("x:\@\-1.0E+101, -1.22E-18.json")
        Call RandomRange.Testing(-100, 20).GetJson.SaveTo("x:\@\-100, 20.json")
        Call RandomRange.Testing(-1000, -20).GetJson.SaveTo("x:\@\-1000, -20.json")
        Call RandomRange.Testing(-100, -3.0E-20).GetJson.SaveTo("x:\@\-100, -3.0E-20.json")
        Call RandomRange.Testing(-5.0E-100, 200).GetJson.SaveTo("x:\@\-5.0E-100, 200.json")
        Call RandomRange.Testing(3, 200).GetJson.SaveTo("x:\@\3, 200.json")
        Call RandomRange.Testing(3.0E-99, 1.0E+21).GetJson.SaveTo("x:\@\3.0E-99, 1.0E+21.json")

        Pause()
    End Sub


    Private Function avadsfdsfds(x As Color) As Color()
        Dim llll As New List(Of Color)

        For Each y In AllDotNetPrefixColors
            Try
                ColorCube.GetColorSequence(x, y, 100)
                llll += y
            Catch ex As Exception

            End Try
        Next

        Return llll
    End Function


    Sub pieChartTest()
        Call {
            New NamedValue(Of Integer)("s1", 123),
            New NamedValue(Of Integer)("s2", 235),
            New NamedValue(Of Integer)("s3", 99),
            New NamedValue(Of Integer)("s4", 499),
            New NamedValue(Of Integer)("s5", 123),
            New NamedValue(Of Integer)("s6", 235),
            New NamedValue(Of Integer)("s7", 99),
            New NamedValue(Of Integer)("s8", 499)
        }.FromData(schema:="Set1:c8") _
         .Plot(reorder:=0, size:=New Size(1500, 1000)) _
         .Save("./pie_chart.png")

        Pause()
    End Sub

    Public Function Main() As Integer
        Call pieChartTest()

        Dim bdata As BarDataGroup = csv.LoadBarData(
            "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\Excels\FigurePlot-Reference-Unigenes.absolute.level1.csv",
            "Paired:c8")

        Call BarPlot.BarPlotAPI.Plot(bdata, New Size(2000, 1400), stacked:=True, legendFont:=New Font(FontFace.BookmanOldStyle, 18)) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\FigurePlot-Reference-Unigenes.absolute.level1.png")

        Pause()

        'Dim ddddd = DataSet.LoadDataSet("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\Quick_correlation_matrix_heatmap\mtcars.csv")
        'Call ddddd.CorrelatesNormalized() _
        '    .Plot(mapName:="PRGn:c6", mapLevels:=20, legendFont:=New Font(FontFace.BookmanOldStyle, 32)) _
        '    .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap.png")

        Pause()

        Dim data = csv.LoadBarData(
            "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\Fruit_consumption.csv",
            {
                "rgb(124,181,236)",
                "rgb(67,67,72)",
                "gray"
            })

        Call BarPlot.BarPlotAPI.Plot(data, New Size(1500, 1000)) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\Fruit_consumption-bar.png")
        Call BarPlot.Plot2(data, New Size(1500, 1000)) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\Fruit_consumption-bar2.png")

        Call Pyramid.Plot(
            {
                New NamedValue(Of Integer)("Eaten", 55),
                New NamedValue(Of Integer)("Tinned", 70),
                New NamedValue(Of Integer)("Killed", 187),
                New NamedValue(Of Integer)("Engaged", 235),
                New NamedValue(Of Integer)("Monster Met", 340)
            }.FromData(schema:="office2010")) _
             .Save("./Pyramid.png")

        Call TreeMap.Plot(
            {
                New NamedValue(Of Integer)("Eaten", 55),
                New NamedValue(Of Integer)("Tinned", 70),
                New NamedValue(Of Integer)("Killed", 187),
                New NamedValue(Of Integer)("Engaged", 235),
                New NamedValue(Of Integer)("Monster Met", 340)
            }.FromData(schema:="office2010")) _
             .Save("./treemap.png")





        Dim ode As New ODE With {
            .df = Function(x, y) 0.1 * Math.Cos(x),
            .y0 = 0.340302,
            .Id = "0.1 * Cos(x)"
        }
        Dim ode2 As New ODE With {
            .df = Function(x, y) Math.Sin(x) / x - 0.005,
            .y0 = 0,
            .Id = "Sin(x) / x - 0.005"
        }
        Call ode.RK4(150, 1, 50)
        Call ode2.RK4(150, 1, 50)

        Dim serials = {ode.FromODE("red"), ode2.FromODE("lime", DashStyle.Solid)}

        Call Scatter.Plot(serials).Save("./cos.png")
        'Call Histogram.Plot(
        '    FromODE({ode2, ode}, {"green", "yellow"}), alpha:=210) _
        '    .SaveAs("./cos.hist.png")

        'Pause()
        '    Call PDFTest.betaTest()


        'Pause()

        'Call Plot3D.Scatter.Plot(Function(x, y) x * y,
        '                         New DoubleRange(-10, 10),
        '                         New DoubleRange(-10, 10),
        '                         New Camera With {
        '                         .screen = New Size(1600, 1000),
        '                         .angle = -60,
        '                         .ViewDistance = -40
        '                         }).SaveAs("x:\@@@@@fdsdfdseeee.png")

        ' Dim dadasdasdasdasXXXXXX = New Double() {42, 5, 43, 6, 54, 8, 60, 5, 4, 78, -38, 5, 2, 9, 33, 48, 2, 4, 82, 3, 0, 94, 8, 2, 30, 9, 4, 823}
        '  Dim dadasdasdasdasYYYYYY = New Double() {42, 5, 43, 6, 54, 8, 60, 5, 4, 78, -38, 5, 2, 9, 33, 48, 2, 4, 82, 3, 0, 94, 8, 2, 30, 9, 4, 823}

        '  Call QQPlot.Plot(dadasdasdasdasXXXXXX, dadasdasdasdasYYYYYY, xcol:="red").SaveAs("x:\asfsdfsdfsd.png")
        '   Pause()
        ' Pause()
        Call Scatter.Plot(New TestObservation().Solve(100, 0, 10).FromODEs, fill:=True, fillPie:=False).Save("x:\fsdfsfsdfds.png")

        '  Dim ava As Dictionary(Of String, String()) = (From x In AllDotNetPrefixColors.AsParallel Select c = ColorTranslator.ToHtml(x), vd = avadsfdsfds(x)).ToDictionary(Function(x) x.c, Function(x) x.vd.ToArray(AddressOf ColorTranslator.ToHtml))

        '  Call ava.GetJson.SaveTo("./avacolors.json")


        Dim dddddserew = Designer.Colors({Color.Red, Color.Green, Color.Blue})

        Call Colors.ColorMapLegend(dddddserew, "ffffff", "sfsdf", "wrwerew").Save("x:\hhhh.png")
        Pause()


        '   Call randdddTest()

        Pause()
        Call RandomRange.Testing(-1000, 1000).GetJson.__DEBUG_ECHO



        Dim ttttdsfsd = GetType(Example)
        Console.WriteLine(ttttdsfsd.IsInheritsFrom(GetType(MonteCarlo.Model)) AndAlso Not ttttdsfsd.IsAbstract)
        Dim iiisddsfd = 1
        For Each sfsdfsds In TestObservation.Compares(100, 0, 10, New Dictionary(Of String, Double) From {{"a", 18.689678431519159}, {"f", 1.0614939771775227}, {"sin", -56.777710793912966}})
            Call sfsdfsds.Plot().Save($"x:\{iiisddsfd}.png")
            iiisddsfd += 1
        Next

        Pause()

        Dim trueData As ODEsOut = New TestObservation().Solve(100, 0, 10)
        For Each yasdas In trueData.y0
            trueData.params.Add(yasdas.Key, yasdas.Value)
        Next

        Dim result = Iterations(
            "./Microsoft.VisualBasic.Data.Bootstrapping.dll",
            trueData,
            1000, 5,,,, )

        Pause()

        Call scatterWithAnnotation()


        Call ShowCharacterData()

        Dim type As New Value(Of LegendStyles)
        Dim legends As Legend() = {
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "red", .style = type = LegendStyles.Hexagon, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "blue", .style = type = LegendStyles.Rectangle, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "lime", .style = type = LegendStyles.Diamond, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "skyblue", .style = type = LegendStyles.Triangle, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "black", .style = type = LegendStyles.Circle, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "skyblue", .style = type = LegendStyles.DashLine, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "black", .style = type = LegendStyles.SolidLine, .title = type.ToString},
            New Legend With {.fontstyle = CSSFont.Win7Normal, .color = "yellow", .style = type = LegendStyles.Pentacle, .title = type.ToString}
        }

        Call GraphicsPlots(
            New Size(350, 600), g.DefaultPadding, "white",
            Sub(ByRef g, grect)
                Call LegendPlotExtensions.DrawLegends(g, New Point(20, 60), legends, New SizeF(200, 50),)
            End Sub).Save("./legends_test.png")

        Dim vars = {
            New NamedValue(Of DoubleRange) With {.Name = "a", .Value = New DoubleRange(-1, 1)},
            New NamedValue(Of DoubleRange) With {.Name = "b", .Value = New DoubleRange(-1, 1)},
            New NamedValue(Of DoubleRange) With {.Name = "c", .Value = New DoubleRange(-1, 1)}
        }

        Dim ysssss = {New NamedValue(Of DoubleRange) With {.Name = "P", .Value = New DoubleRange(-10, 10)},
            New NamedValue(Of DoubleRange) With {.Name = "yC", .Value = New DoubleRange(-10, 10)}}

        '   Dim mcTest = BootstrapIterator.Bootstrapping(Of ODEsTest)(vars, ysssss, 1, 100, 0, 100).ToArray


        '  Call bubblePlots()

        Dim xxxx = Mathematical.SyntaxAPI.MathExtension.Normal.rnorm(130, 0, 1)

        Call Scatter.Plot(xxxx).Save("x:\dddd.png")

        xxxx = New Vector(Sample(20).OrderBy(Function(ddd) ddd))

        Pause()

        '        Call ODEsTest.test()
        ' Dim odes As New ODEsTest
        ' Dim test_out = odes.Solve(100, 0, 100)

        '  Call New ODEsTest().Solve(300, 0, 500).Plot.SaveAs("./ODEs.png")

        ' Dim P As New var(NameOf(P))
        ' Dim C As New var(NameOf(C))

        ' Call New GenericODEs(P = 2, C = 1) With {
        '.df = Sub(dx, ByRef dy)
        'dy(P) = 0.1 * P - 0.1 * C * P
        'dy(C) = 0.1 * P * C - 0.1 * C
        'End Sub
        '}.Solve(10000, 0, 500).Plot(,,, 8, 6).SaveAs("./ODEs_test2.png")

        'Call New GenericODEs(P = 2, C = 1) With {
        '.df = Sub(dx, ByRef dy)
        'dy(P) = 0.1 * P - 0.1 * C * P
        'dy(C) = 0.1 * P * C - 0.1 * C
        'End Sub
        '}.Solve(100000, 0, 500).Plot(,,, 8, 6).SaveAs("./ODEs_test3.png")

        Call {
            New NamedValue(Of Integer)("s1", 123),
            New NamedValue(Of Integer)("s2", 235),
            New NamedValue(Of Integer)("s3", 99),
            New NamedValue(Of Integer)("s4", 499),
            New NamedValue(Of Integer)("s5", 499)
        }.FromData(schema:="paper").Plot(minRadius:=100).Save("./pie_chart_vars.png")



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
        Dim func = FuncParser.TryParse(f)

        Call Expression.DefaultEngine.Functions.Add(f)
        Call Expression.Evaluate("f(1,2,3,3)").__DEBUG_ECHO

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
        Dim expr = MathExpression(s2)
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
