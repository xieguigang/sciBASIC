#Region "Microsoft.VisualBasic::3201436b9bd47b1cdd93f137bfb8a514, www\githubAPI\Visualizer\IsometricContributions.vb"

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

    '   Total Lines: 205
    '    Code Lines: 159 (77.56%)
    ' Comment Lines: 6 (2.93%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 40 (19.51%)
    '     File Size: 8.92 KB


    ' Module IsometricContributions
    ' 
    '     Function: (+2 Overloads) Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI
Imports IsometricView = Microsoft.VisualBasic.Imaging.Drawing3D.IsometricEngine

Public Module IsometricContributions

    <Extension>
    Public Function Plot(userName$,
                         Optional schema$ = "Jet",
                         Optional size$ = "3000,2200",
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
                         Optional size$ = "3400,2200",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional rectWidth! = 0.5,
                         Optional noColor$ = NameOf(Color.Gray),
                         Optional statNumberColor$ = "DarkGreen",
                         Optional labelItemCSS$ = CSSFont.Win7VeryLarge,
                         Optional user As User = Nothing,
                         Optional avatarWidth% = 350) As GraphicsData

        Dim max% = contributions.Values.Max
        Dim colors As List(Of Color) = Designer.GetColors(schema, max).AsList
        Dim weeks = contributions.Split(7)
        Dim view As New IsometricView
        Dim maxZ = rectWidth * 5.5
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

        Dim streak = contributions.Split(Function(day) day.Value = 0, )
        Dim LongestStreak = streak.OrderByDescending(Function(days) days.Length).First
        Dim currentStreak = streak.Last
        Dim total$ = contributions.Sum(Function(day) day.Value).ToString("N").Replace(".00", "")
        Dim busiestDay = contributions.OrderByDescending(Function(day) day.Value).FirstOrDefault
        Dim oneYear$

        With contributions.Keys.OrderBy(Function(day) day).ToArray
            oneYear = $"{ .First.ToString("MMM dd, yyyy")} - { .Last.ToString("MMM dd, yyyy")}"
        End With

        Dim model As Surface() = view.ToArray
        Dim labelItemFont As Font = CSSFont.TryParse(labelItemCSS).GDIObject
        Dim statNumberFont As New Font(labelItemFont.Name, labelItemFont.Size * 3, FontStyle.Bold)
        Dim statNumberPen As Brush = statNumberColor.GetBrush
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                ' 30,30,120
                ' 30,60,-56.25
                Dim camera As New Camera With {
                    .screen = region.Size,
                    .fov = 10000,
                    .ViewDistance = -75,
                    .angleX = 30,
                    .angleY = 30,
                    .angleZ = 125
                }
                model = model _
                    .Centra _
                    .Offsets(model) _
                    .ToArray
                model = camera.Rotate(model).ToArray

                Call DirectCast(g, Graphics2D) _
                    .Graphics _
                    .SurfacePainter(camera, model, illumination:=False, offset:=New PointF(-100, 100))

                Dim fsize As SizeF = g.MeasureString(oneYear, labelItemFont)

                With region
                    x = .Size.Width - .Padding.Right - fsize.Width
                    y = .Padding.Top + fsize.Height
                End With

                Dim dev = g
                Dim plotLabelContent =
                    Sub(title$, item$, date$, value$)
                        Call dev.DrawString(item, labelItemFont, Brushes.Black, New PointF(x, y))
                        Call dev.DrawString([date], labelItemFont, Brushes.Gray, New PointF(x, y + labelItemFont.Height + 5))

                        With dev.MeasureString(title, labelItemFont)
                            Call dev.DrawString(title, labelItemFont, Brushes.Black, New PointF(x - .Width, y - .Height - 5))
                        End With

                        fsize = dev.MeasureString(value, statNumberFont)
                        Call dev.DrawString(value, statNumberFont, statNumberPen, New Point(x - fsize.Width, y))
                    End Sub

                ' 右上角的整年的贡献值
                Call plotLabelContent("1 year total", "contributions", oneYear, total)

                y += statNumberFont.Height * 1.5

                With busiestDay
                    Call plotLabelContent("Busiest day", "contributions", .Key.ToString("MMM dd"), .Value)
                End With

                With region
                    y = .Size.Height * 2 / 3
                    x = .Padding.Left + g.MeasureString("Longest streak", labelItemFont).Width
                End With

                With LongestStreak
                    Dim period$

                    With .Select(Function(day) day.Key).OrderBy(Function(day) day).ToArray
                        period = $"{ .First.ToString("MMM dd")} - { .Last.ToString("MMM dd")}"
                    End With

                    Call plotLabelContent("Longest streak", "days", period, .Length)
                End With

                y += statNumberFont.Height * 1.5

                With currentStreak
                    Dim period$

                    With .Select(Function(day) day.Key).OrderBy(Function(day) day).ToArray
                        period = $"{ .First.ToString("MMM dd")} - { .Last.ToString("MMM dd")}"
                    End With

                    Call plotLabelContent("Current streak", "days", period, .Length)
                End With

                If Not user Is Nothing Then
                    ' avatar userName (altName)
                    '        bio
                    '        github_url
                    With region.Padding
                        Call g.DrawImage(user.GetAvatar, .Left, .Top, avatarWidth, avatarWidth)

                        x = .Left + avatarWidth + 25
                        y = .Top - 10

                        With user

                            fsize = g.MeasureString(.bio, labelItemFont)
                            statNumberFont = New Font(labelItemFont.Name, emSize:=labelItemFont.Size * 2.5)

                            Dim y1 = y + statNumberFont.Height + 25
                            Dim y2 = y1 + fsize.Height + 5
                            Dim s$ = .bio

                            If s.Length > 60 Then
                                s = Mid(.bio, 1, 56) & "..."
                            End If

                            Dim label$ = $"{ .login} ({ .name})"
                            fsize = g.MeasureString(label, statNumberFont)

                            If fsize.Width > g.Size.Width * 0.55 Then
                                label = Mid(label, 1, 18) & "..."
                            End If

                            Call g.DrawString(label, statNumberFont, Brushes.Black, New Point(x, y))
                            Call g.DrawString(s, labelItemFont, Brushes.Gray, New Point(x, y1))
                            Call g.DrawString(.url, labelItemFont, Brushes.Black, New Point(x, y2))
                        End With
                    End With
                End If
            End Sub

        Return g.GraphicsPlots(size.SizeParser, padding, bg, plotInternal)
    End Function
End Module
