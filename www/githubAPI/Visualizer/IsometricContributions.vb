Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI
Imports IsometricView = Microsoft.VisualBasic.Imaging.Drawing3D.IsometricEngine

Public Module IsometricContributions

    <Extension>
    Public Function Plot(userName$,
                         Optional schema$ = "Jet",
                         Optional size$ = "1440,900",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional rectWidth! = 0.5,
                         Optional noColor$ = NameOf(Color.Gray)) As GraphicsData
        Dim contributions = userName.GetUserContributions
        Return contributions.Plot(schema, size, padding, bg, rectWidth, noColor)
    End Function

    <Extension>
    Public Function Plot(contributions As Dictionary(Of Date, Integer),
                         Optional schema$ = "Jet",
                         Optional size$ = "3440,2400",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional rectWidth! = 0.5,
                         Optional noColor$ = NameOf(Color.Gray)) As GraphicsData

        Dim max% = contributions.Values.Max
        Dim colors As List(Of Color) = Designer.GetColors(schema, max).AsList
        Dim weeks = contributions.Split(7)
        Dim view As New IsometricView
        Dim maxZ = rectWidth * 4
        Dim x!, y!

        Call colors.Insert(Scan0, noColor.TranslateColor)

        For Each week In weeks
            For Each day As KeyValuePair(Of Date, Integer) In week
                Dim height! = day.Value / max * maxZ
                Dim o As New Point3D(x, y, 0)
                Dim model3D As New Prism(o, rectWidth, rectWidth, height)

                x += rectWidth

                Call view.Add(model3D, colors(day.Value))
            Next

            x = 0
            y += rectWidth
        Next

        With size.SizeParser
            Call view.Measure(.Width, .Height, False)
        End With

        Dim model As Surface() = view.ToArray
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim camera As New Camera With {
                    .screen = region.Size,
                    .fov = 10000,
                    .ViewDistance = -75,
                    .angleX = 30,
                    .angleY = 30,
                    .angleZ = 120
                }
                model = model _
                    .Centra _
                    .Offsets(model) _
                    .ToArray
                model = camera.Rotate(model).ToArray

                Call DirectCast(g, Graphics2D) _
                    .Graphics _
                    .SurfacePainter(camera, model)
            End Sub

        Return g.GraphicsPlots(size.SizeParser, padding, bg, plotInternal)
    End Function
End Module
