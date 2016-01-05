Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("Network.Visualizer", Publisher:="xie.guigang@gmail.com")>
Public Module NetworkVisualizer

    <ExportAPI("Draw.Image")>
    Public Function DrawImage(Network As Network, Optional Margin As Point = Nothing) As Bitmap
        Dim FrameSize = Network.FrameSize
        Margin = If(Margin = Nothing, New Point(3, 3), Margin)

        Dim Bitmap As Bitmap = New Bitmap(FrameSize.Width, FrameSize.Height)

        Using Graphic As System.Drawing.Graphics = Graphics.FromImage(Bitmap)
            If Network.BackgroundImage Is Nothing Then
                Dim BackgroundColor = Color.FromArgb(219, 243, 255)
                Call Graphic.FillRectangle(New SolidBrush(BackgroundColor), New Rectangle(New Point, New Size(FrameSize.Width, FrameSize.Height))) '绘制背景纹理
            Else
                Call Graphic.DrawImage(Network.BackgroundImage, 0, 0, FrameSize.Width, FrameSize.Height)
            End If

            Graphic.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Graphic.CompositingMode = Drawing2D.CompositingMode.SourceOver
            Graphic.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            Graphic.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

            For Each Nod In Network
                For i As Integer = 0 To Nod.Neighbours.Count - 1
                    Dim cnnId = Nod.Neighbours(i)
                    Dim otherNode = Network.Nodes(cnnId)
                    Dim Weight = Nod.Weights(i)
                    Dim Color As Color = Color.FromArgb(131, 131, 131)

                    If Weight < 0.5 Then
                        Color = Drawing.Color.Gray
                    ElseIf Weight < 0.75 Then
                        Color = Drawing.Color.Blue
                    End If

                    Dim LineColor As Pen = New Pen(Color, 5 * Weight)

                    Call Graphic.DrawLine(LineColor, Nod.Location, otherNode.Location)
                Next
            Next

            'Dim resilt = CatmullRomSpline((From node In Network Select node.Location).ToList, 0.1, True)
            'Dim path = New GraphicsPath
            'For Each p In resilt
            '    Call path.AddLine(p, p)
            'Next
            'Call path.CloseAllFigures()
            'Call Graphic.DrawPath(Pens.Black, path)

            For Each Nod In Network
                Dim Font As Font = New Font("Ubuntu", 12 + Nod.Neighbours.Count, FontStyle.Bold)
                Dim size = Graphic.MeasureString(Nod.DispName, Font)
                Dim r As Integer = If(Nod.Neighbours.Count < 30, Nod.Neighborhoods * 9, Nod.Neighborhoods * 7)

                Call Graphic.FillPie(New SolidBrush(Nod.Color), New Rectangle(New Point(Nod.Location.X - r / 2, Nod.Location.Y - r / 2), New Size(r, r)), 0, 360)

                Dim stringLocation As Point = New Point(Nod.Location.X - size.Width / 2, Nod.Location.Y + r / 2 + 2)
                If stringLocation.X < Margin.X Then
                    stringLocation = New Point(Margin.X, stringLocation.Y)
                End If
                If stringLocation.Y + size.Height > FrameSize.Height - Margin.Y Then
                    stringLocation = New Point(stringLocation.X, FrameSize.Height - Margin.Y - size.Height)
                End If
                If stringLocation.X + size.Width > FrameSize.Width - Margin.X Then
                    stringLocation = New Point(FrameSize.Width - Margin.X - size.Width, stringLocation.Y)
                End If

                Call Graphic.DrawString(Nod.DispName, Font, Brushes.Black, stringLocation)
            Next
        End Using

        Return Bitmap
    End Function

    ''' <summary>
    ''' Calculates interpolated point between two points using Catmull-Rom Spline/// </summary>
    ''' <remarks>
    ''' Points calculated exist on the spline between points two and three./// </remarks>
    ''' <param name="p0">First Point</param>
    ''' <param name="p1">Second Point</param>
    ''' <param name="p2">Third Point</param>
    ''' <param name="p3">Fourth Point</param>
    ''' <param name="t">
    ''' Normalised distance between second and third point /// where the spline point will be calculated/// </param>
    ''' <returns>Calculated Spline Point/// </returns>
    ''' 
    <ExportAPI("CatmullRom.Spline", Info:="Calculates interpolated point between two points using Catmull-Rom Spline")>
    Private Function PointOnCurve(p0 As Point, p1 As Point, p2 As Point, p3 As Point, t As Double) As Point
        Dim ret As New Point()

        Dim t2 As Single = t * t
        Dim t3 As Single = t2 * t

        ret.X = 0.5F * ((2.0F * p1.X) + (-p0.X + p2.X) * t + (2.0F * p0.X - 5.0F * p1.X + 4 * p2.X - p3.X) * t2 + (-p0.X + 3.0F * p1.X - 3.0F * p2.X + p3.X) * t3)
        ret.Y = 0.5F * ((2.0F * p1.Y) + (-p0.Y + p2.Y) * t + (2.0F * p0.Y - 5.0F * p1.Y + 4 * p2.Y - p3.Y) * t2 + (-p0.Y + 3.0F * p1.Y - 3.0F * p2.Y + p3.Y) * t3)

        Return ret
    End Function

    <ExportAPI("CatmullRom.Spline")>
    Public Function CatmullRomSpline(Points As List(Of Point),
                                     <Parameter("Interpolation.Steps")> Optional InterpolationStep As Double = 0.1,
                                     <Parameter("Is.Polygon")> Optional IsPolygon As Boolean = False) As List(Of Point)
        If Points.Count <= 2 Then
            Return Points
        End If

        If IsPolygon Then Return __catmullRomSplinePolygon(Points, InterpolationStep)

        Dim result As New List(Of Point)
        Dim yarray, xarray As New List(Of Double)
        xarray.Add(Points(0).X - (Points(1).X - Points(0).X) / 2)
        yarray.Add(Points(0).Y - (Points(1).Y - Points(0).Y) / 2)

        For Each ps As Point In Points
            xarray.Add(ps.X)
            yarray.Add(ps.Y)
        Next

        xarray.Add((Points(Points.Count - 1).X - (Points(Points.Count - 2).X) / 2 + Points(Points.Count - 1).X))
        yarray.Add((Points(Points.Count - 1).Y - (Points(Points.Count - 2).Y) / 2 + Points(Points.Count - 1).Y))

        Dim r As New List(Of Point)
        For i As Integer = 0 To yarray.Count - 1
            r.Add(New Point(xarray(i), yarray(i)))
        Next

        For i As Integer = 3 To r.Count - 1
            For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                result.Add(PointOnCurve(r(i - 3), r(i - 2), r(i - 1), r(i), k))
            Next
        Next
        result.Add(Points(Points.Count - 1))

        Return result
    End Function

    Private Function __catmullRomSplinePolygon(points As List(Of Point), InterpolationStep As Double) As List(Of Point)
        Dim result As New List(Of Point)

        For i As Integer = 0 To points.Count - 1
            If i = 0 Then
                For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                    result.Add(PointOnCurve(points(points.Count - 1), points(i), points(i + 1), points(i + 2), k))
                Next
            ElseIf i = points.Count - 1 Then
                For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                    result.Add(PointOnCurve(points(i - 1), points(i), points(0), points(1), k))
                Next
            ElseIf i = points.Count - 2 Then
                For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                    result.Add(PointOnCurve(points(i - 1), points(i), points(i + 1), points(0), k))
                Next
            Else
                For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                    result.Add(PointOnCurve(points(i - 1), points(i), points(i + 1), points(i + 2), k))
                Next
            End If
        Next

        Call result.Add(points(0))

        Return result
    End Function

End Module
