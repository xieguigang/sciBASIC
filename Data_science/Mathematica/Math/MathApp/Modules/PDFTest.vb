#Region "Microsoft.VisualBasic::84fb9859fe35175c7c3c7b617fb4214b, sciBASIC#\Data_science\Mathematica\Math\MathApp\Modules\PDFTest.vb"

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

    '   Total Lines: 98
    '    Code Lines: 53
    ' Comment Lines: 29
    '   Blank Lines: 16
    '     File Size: 3.18 KB


    ' Module PDFTest
    ' 
    '     Sub: betaTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Distributions
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Module PDFTest

    Public Sub betaTest()




        'Dim xdata As New NamedValue(Of DoubleRange) With {.Name = "x", .Value = New DoubleRange(0, Math.PI)}

        'Call xdata.Plot("sin(x)+cos(2*x)").SaveAs("x:\test.png")
        'Call xdata.Value.Plot(Function(a) Math.Sin(a) + Math.Cos(2 * a), title:="Value Of ""sin(x)+cos(2*x)""").SaveAs("x:\test_lambda.png")


        Dim x As New Vector(VBMathExtensions.seq(0.02, 0.98, 0.005))


        'Dim range As New DoubleRange(0, 1)
        'Dim f1 = Function(xx) Beta.beta(xx, 2, 5)
        'Dim f2 = Function(xx) Beta.beta(xx, 2, 2)
        'Dim a1 As New HistProfile(range, f1, 0.025) With {
        '    .legend = New Legend With {
        '        .color = "green",
        '        .fontstyle = CSSFont.Win10Normal,
        '        .style = LegendStyles.Triangle,
        '        .title = "α = 2, β = 5"
        '    }
        '}
        'Dim a2 As New HistProfile(range, f2, 0.05) With {
        '    .legend = New Legend With {
        '        .color = "yellow",
        '        .fontstyle = CSSFont.Win7Normal,
        '        .style = LegendStyles.Triangle,
        '        .title = "α = β = 2"
        '    }
        '}
        'Dim hist As New HistogramGroup({a2, a1})

        'Call Histogram.Plot(
        '    hist,,
        '    New Size(2000, 1300),
        '    alpha:=230) _
        '    .SaveAs("./beta_hist.png")

        '   Pause()
        Dim s1 = Scatter.FromVector(
            Beta.beta(x, 0.5, 0.5), "red",
            ptSize:=5,
            width:=10,
            xrange:=x,
            title:="α = β = 0.5",
            alpha:=230)
        Dim s2 = Scatter.FromVector(
            Beta.beta(x, 5, 1), "blue",
            ptSize:=5,
            width:=10,
            xrange:=x,
            title:="α = 5, β = 1",
            alpha:=230)
        Dim s3 = Scatter.FromVector(
            Beta.beta(x, 1, 3), "green",
            ptSize:=5,
            width:=10,
            xrange:=x,
            title:="α = 1, β = 3",
            alpha:=230)
        Dim s4 = Scatter.FromVector(
            Beta.beta(x, 2, 2), "purple",
            ptSize:=5,
            width:=10,
            xrange:=x,
            title:="α = 2, β = 2",
            alpha:=230)
        Dim s5 = Scatter.FromVector(
            Beta.beta(x, 2, 5), "orange",
            ptSize:=5,
            width:=10,
            xrange:=x,
            title:="α = 2, β = 5",
            alpha:=150)

        Dim canvasSize As New Size(1600, 1200)
        Dim png = Scatter.Plot({s1, s2, s3, s4, s5}, canvasSize)

        Call png.Save("./beta_PDF.png")

        Pause()
    End Sub
End Module
