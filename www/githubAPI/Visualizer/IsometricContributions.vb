Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI
Imports IsometricView = Microsoft.VisualBasic.Imaging.Drawing3D.IsometricEngine

Public Module IsometricContributions

    Public Function Plot(userName$,
                         Optional schema$ = "Jet",
                         Optional size$ = "1440,900",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional rectWidth! = 0.25,
                         Optional noColor$ = NameOf(Color.Gray)) As Image

        Dim contributions = userName.GetUserContributions
        Dim max% = contributions.Values.Max
        Dim colors As List(Of Color) = Designer.GetColors(schema, max).AsList
        Dim weeks = contributions.Split(7)
        Dim view As New IsometricView
        Dim maxZ = rectWidth * 2.5
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

        Using g As Graphics2D = size.SizeParser.CreateGDIDevice
            Call view.Draw(g)
            Return g.ImageResource
        End Using
    End Function
End Module
