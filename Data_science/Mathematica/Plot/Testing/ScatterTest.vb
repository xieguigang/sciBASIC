#Region "Microsoft.VisualBasic::6101e68b703933beb597a18a11e5f242, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\Testing\ScatterTest.vb"

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

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Module ScatterTest

    Sub StyleTest()

        Dim s As New List(Of PointF)

        With New Random
            For i As Integer = 0 To 10
                s += New PointF(.Next(0, 1000) * 1.0!, .Next(0, 500) * 1.0!)
            Next
        End With

        Dim points = s.OrderBy(Function(p) p.X).Select(Function(i) New PointData With {.pt = i}).ToArray
        Dim ser As New SerialData With {
            .color = Color.DarkCyan,
            .lineType = DashStyle.Dash,
            .width = 5,
            .title = "Random",
            .PointSize = 15,
            .pts = points
        }

        Call Scatter.Plot({ser},
                            size:="1440,1000",
                            showLegend:=True,
                            padding:=g.DefaultPadding,
                            fillPie:=True).Save($"./line.png")
    End Sub

    Sub Main()

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
            Dim csv As csv = csv.Load(file)
            Dim X = csv.Columns(0).Skip(1).AsNumeric
            Dim Y = csv.Columns(csv.Headers.IndexOf("mean")).Skip(1).AsNumeric
            Dim err = csv.Columns(csv.Headers.IndexOf("sem")).Skip(1).AsNumeric
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

            X = csv.Columns(0).Skip(1).AsNumeric
            Dim YV = csv.Columns(1).Skip(1).AsNumeric.AsVector

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
