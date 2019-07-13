#Region "Microsoft.VisualBasic::a59375363005739182add8546eaa2fa8, gr\Microsoft.VisualBasic.Imaging\test\ConvexHull_demo.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module ConvexHull_demo
    ' 
    '     Sub: Draw, GrahamScanDemo, JarvisMarchDemo, Main
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

    Public Sub Main()

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
