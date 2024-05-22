#Region "Microsoft.VisualBasic::8db4dc255405672baee3ffcea4275ba7, Data_science\Visualization\test\ScatterTest.vb"

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

    '   Total Lines: 147
    '    Code Lines: 96 (65.31%)
    ' Comment Lines: 21 (14.29%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 30 (20.41%)
    '     File Size: 5.35 KB


    ' Module ScatterTest
    ' 
    '     Sub: Main, qqplotTest, StyleTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports csvFile = Microsoft.VisualBasic.Data.csv.IO.File
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging

Module ScatterTest

    Sub StyleTest()

        Dim s As New List(Of PointData)
        Dim s2 As New List(Of PointData)

        With New Random
            For i As Integer = 0 To 10
                s += New PointData(New PointF(.Next(0, 1000) * 1.0!, .Next(0, 500) * 1.0!))
                s2 += New PointData(New PointF(.Next(0, 1000) * 1.0!, .Next(0, 500) * 1.0!))
            Next
        End With

        Dim ser As New SerialData With {
            .color = Color.DarkCyan,
            .lineType = DashStyle.Dash,
            .width = 5,
            .title = "Random",
            .PointSize = 15,
            .pts = s.OrderBy(Function(p) p.pt.X).ToArray
        }

        Dim s22 As New SerialData With {
            .color = Color.DarkRed,
            .lineType = DashStyle.Dash,
            .width = 5,
            .title = "Random2222",
            .PointSize = 15,
            .pts = s2.OrderBy(Function(p) p.pt.X).ToArray
        }


        Call Scatter.Plot({ser, s22},
                            size:="1440,1000",
                            showLegend:=True,
                            padding:=g.DefaultPadding,
                            fillPie:=True,
                          fill:=True,
                          preferPositive:=True).Save($"./line.png")
    End Sub

    Sub qqplotTest()
        Dim rnd As New Random
        Dim a = seq(0, 100, 0.23).Select(Function(x) rnd.NextDouble * x).ToArray
        Dim b = seq(-10, 20, 0.1).ToArray

        Dim set1 As New NamedValue(Of Double())("AAAAA", a)
        Dim set2 As New NamedValue(Of Double())("BBBBBB", b)

        Call QQPlot.Plot(set1, set2).AsGDIImage.SaveAs("./qqqqqq.png")


        Pause()
    End Sub


    Sub Main()

        qqplotTest()

        StyleTest()

        Pause()

        'For Each file As String In (ls - l - r - "*.csv" <= "D:\OneDrive\2017-9-25\TCL")
        '    Dim csv As csv = csv.Load(file)
        '    Dim X = csv.Columns(0).Skip(1).AsNumeric
        '    Dim Y = csv.Columns(1).Skip(1).AsNumeric.AsVector

        '    Y = Vector.Log(Y, base:=10)

        '    Dim points = X.SeqIterator.Select(Function(i) New PointData With {.pt = New PointF With {.X = i.value, .Y = Y(i)}}).ToArray
        '    Dim s As New SerialData With {
        '        .color = Color.DarkCyan,
        '        .lineType = DashStyle.Solid,
        '        .width = 5,
        '        .title = file.BaseName,
        '        .PointSize = 15,
        '        .pts = points
        '    }

        '    Call Scatter.Plot({s},
        '                      size:="1440,1000",
        '                      showLegend:=False,
        '                      padding:=g.DefaultPadding,
        '                      fillPie:=False).Save($"D:\OneDrive\2017-9-25\TCL\{file.BaseName}.png")
        'Next

        'Pause()

        For Each file As String In (ls - l - r - "*.csv" <= "D:\OneDrive\2017-9-25\DENV-1234")
            Dim csv As csvFile = csvFile.Load(file)
            Dim X = csv.Columns(0).Skip(1).AsDouble
            Dim Y = csv.Columns(csv.Headers.IndexOf("mean")).Skip(1).AsDouble
            Dim err = csv.Columns(csv.Headers.IndexOf("sem")).Skip(1).AsDouble
            Dim points = X.SeqIterator.Select(Function(i) New PointData With {.errMinus = err(i), .errPlus = err(i), .pt = New PointF With {.X = i.value, .Y = Y(i)}}).ToArray
            Dim s As New SerialData With {
                .color = Color.DarkCyan,
                .lineType = DashStyle.Dash,
                .width = 5,
                .title = file.BaseName,
                .PointSize = 15,
                .pts = points
            }

            csv = csv.Load(file.ParentPath.ParentPath & $"\TCL\{file.BaseName}.csv")

            X = csv.Columns(0).Skip(1).AsDouble
            Dim YV = csv.Columns(1).Skip(1).AsDouble.AsVector

            YV = Vector.Log(YV, base:=10)

            points = X.SeqIterator.Select(Function(i) New PointData With {.pt = New PointF With {.X = i.value, .Y = YV(i)}}).ToArray
            Dim s2 As New SerialData With {
                .color = Color.Red,
                .lineType = DashStyle.Solid,
                .width = 5,
                .title = "TCL",
                .PointSize = 1,
                .pts = points
            }


            Call Scatter.Plot({s, s2},
                              size:="1440,1000",
                              showLegend:=True,
                              padding:=g.DefaultPadding,
                              fillPie:=True).Save($"D:\OneDrive\2017-9-25\{file.BaseName}.png")
        Next
    End Sub
End Module
