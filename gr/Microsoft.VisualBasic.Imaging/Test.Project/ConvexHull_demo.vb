#Region "Microsoft.VisualBasic::88c1053ef28aaf06bdaab9b79b453953, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Test.Project\ConvexHull_demo.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module ConvexHull_demo

    <Extension>
    Private Sub GrahamScanDemo(points As List(Of Point))
        Dim stopwatch As Stopwatch = Stopwatch.StartNew
        Dim result = ConvexHull.GrahamScan(points)
        stopwatch.[Stop]()
        Dim elapsed_time As Single = stopwatch.ElapsedMilliseconds
        Console.WriteLine("Elapsed time: {0} milliseconds", elapsed_time)
        Call points.Draw(result)
    End Sub

    <Extension>
    Private Sub JarvisMarchDemo(points As List(Of Point))
        Dim stopwatch = New Stopwatch()
        stopwatch.Start()
        Dim result = ConvexHull.JarvisMatch(points)
        stopwatch.[Stop]()
        Dim elapsed_time As Single = stopwatch.ElapsedMilliseconds
        Console.WriteLine("Elapsed time: {0} milliseconds", elapsed_time)
        Call points.Draw(result)
    End Sub

    <Extension>
    Public Sub Draw(points As IEnumerable(Of Point), vex As Point(), <CallerMemberName> Optional method$ = Nothing)
        Using g As Graphics2D = vex.GetBounds.Size.Enlarge(1.25).CreateGDIDevice

            For Each p In points.AsList
                Call g.FillPie(Brushes.Black, New Rectangle(p, New Size(8, 8)), 0, 360)
            Next
            For Each p In vex
                Call g.FillPie(Brushes.Blue, New Rectangle(p, New Size(5, 5)), 0, 360)
            Next

            For Each pair In vex.SlideWindows(2)
                Dim a = pair.First
                Dim b = pair.Last

                Call g.DrawLine(Pens.Red, a, b)
            Next

            Call g.DrawLine(Pens.Red, vex.First, vex.Last)

            Dim enlarge = vex.Enlarge(1.25)

            For Each pair In enlarge.SlideWindows(2)
                Dim a = pair.First
                Dim b = pair.Last

                Call g.DrawLine(Pens.Blue, a, b)
            Next

            Call g.DrawLine(Pens.Blue, enlarge.First, enlarge.Last)

            Call g.Save(App.HOME & $"/{method}.png", ImageFormats.Png)

        End Using
    End Sub

    Public Sub Main()
        Dim size = 30
        Dim x = New DoubleRange(50, 2000).rand(size)
        Dim y = New DoubleRange(50, 1200).rand(size)
        Dim points = size.Sequence.Select(Function(i) New Point(x(i), y(i))).AsList

        Call points.GrahamScanDemo()
        Call points.JarvisMarchDemo()
        Call points.Run_ConcaveHull_demo

        Pause()
    End Sub
End Module

