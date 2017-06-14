Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull

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
    Private Sub Draw(points As IEnumerable(Of Point), vex As Point(), <CallerMemberName> Optional method$ = Nothing)
        Using g As Graphics2D = vex.GetBounds.Size.CreateGDIDevice

            For Each p In points.AsList + vex
                Call g.FillPie(Brushes.Black, New Rectangle(p, New Size(3, 3)), 0, 360)
            Next

            For Each pair In vex.SlideWindows(2)
                Dim a = pair.First
                Dim b = pair.Last

                Call g.DrawLine(Pens.Red, a, b)
            Next

            Call g.DrawLine(Pens.Red, vex.First, vex.Last)
            Call g.Save(App.HOME & $"/{method}.png", ImageFormats.Png)

        End Using
    End Sub

    Public Sub Main()
        Dim points As New List(Of Point)()
        points.Add(New Point(9, 1))
        points.Add(New Point(4, 3))
        points.Add(New Point(4, 5))
        points.Add(New Point(3, 2))
        points.Add(New Point(14, 2))
        points.Add(New Point(4, 12))
        points.Add(New Point(4, 10))
        points.Add(New Point(5, 6))
        points.Add(New Point(10, 2))
        points.Add(New Point(1, 2))
        points.Add(New Point(1, 10))
        points.Add(New Point(5, 2))
        points.Add(New Point(11, 2))
        points.Add(New Point(4, 11))
        points.Add(New Point(12, 4))
        points.Add(New Point(3, 1))
        points.Add(New Point(2, 6))
        points.Add(New Point(2, 4))
        points.Add(New Point(7, 8))
        points.Add(New Point(5, 5))

        Call points.GrahamScanDemo()
        Call points.JarvisMarchDemo()

        Pause()
    End Sub
End Module
