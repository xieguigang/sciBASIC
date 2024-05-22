#Region "Microsoft.VisualBasic::0f9c2a7b5ffa65682cd9143edff9ab21, gr\Microsoft.VisualBasic.Imaging\test\ConvexHull_demo.vb"

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

    '   Total Lines: 75
    '    Code Lines: 60 (80.00%)
    ' Comment Lines: 1 (1.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (18.67%)
    '     File Size: 2.71 KB


    ' Module ConvexHull_demo
    ' 
    '     Sub: Draw, GrahamScanDemo, JarvisMarchDemo, Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module ConvexHull_demo

    <Extension>
    Private Sub GrahamScanDemo(points As List(Of PointF))
        Dim stopwatch As Stopwatch = Stopwatch.StartNew
        Dim result = ConvexHull.GrahamScan(points)
        stopwatch.[Stop]()
        Dim elapsed_time As Single = stopwatch.ElapsedMilliseconds
        Console.WriteLine("Elapsed time: {0} milliseconds", elapsed_time)
        Call points.Draw(result)
    End Sub

    <Extension>
    Private Sub JarvisMarchDemo(points As List(Of PointF))
        Dim stopwatch = New Stopwatch()
        stopwatch.Start()
        Dim result = ConvexHull.JarvisMatch(points)
        stopwatch.[Stop]()
        Dim elapsed_time As Single = stopwatch.ElapsedMilliseconds
        Console.WriteLine("Elapsed time: {0} milliseconds", elapsed_time)
        Call points.Draw(result)
    End Sub

    <Extension>
    Public Sub Draw(points As IEnumerable(Of PointF), vex As PointF(), <CallerMemberName> Optional method$ = Nothing)
        Using g As Graphics2D = vex.GetBounds.Size.Enlarge(1.25).CreateGDIDevice

            For Each p In points.AsList
                Call g.FillPie(Brushes.Black, New RectangleF(p, New SizeF(8, 8)), 0, 360)
            Next
            For Each p In vex
                Call g.FillPie(Brushes.Blue, New RectangleF(p, New SizeF(5, 5)), 0, 360)
            Next

            Dim red As New Pen(Color.Red, 5) With {
                .DashStyle = DashStyle.Dot
            }

            Call g.DrawHullPolygon(vex, Color.SkyBlue, 8.5, 100)
            ' Call g.DrawPolygon(Pens.Blue, vex.BSpline(degree:=2))

            Call g.Save(App.HOME & $"/{method}.png", ImageFormats.Png)

        End Using
    End Sub

    Public Sub Main1()

        Call Randomize()

        Dim size = 30
        Dim x = New DoubleRange(150, 2000).rand(size)
        Dim y = New DoubleRange(150, 1200).rand(size)
        Dim points = size.Sequence.Select(Function(i) New PointF(x(i), y(i))).AsList

        Call points.GrahamScanDemo()
        Call points.JarvisMarchDemo()
        Call points.Run_ConcaveHull_demo

        Pause()
    End Sub
End Module
