#Region "Microsoft.VisualBasic::6fc714cbe4f6e955afdec38af5c18742, sciBASIC#\vs_solutions\logo\Program.vb"

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
    '    Code Lines: 53
    ' Comment Lines: 3
    '   Blank Lines: 19
    '     File Size: 2.84 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports stdNum = System.Math

''' <summary>
''' sciBASIC framework logo generator. A demo for the sciBASIC graphics artist system.
''' </summary>
Module Program

    Const save$ = "../../../../logo.png"

    ReadOnly BLUE As Color = Color.FromArgb(50, 60, 160)
    ReadOnly GREEN As Color = Color.FromArgb(50, 160, 60)
    ReadOnly RED As Color = Color.FromArgb(160, 60, 50)
    ReadOnly TEAL As Color = Color.FromArgb(0, 180, 180)
    ReadOnly YELLOW As Color = Color.FromArgb(180, 180, 0)
    ReadOnly LIGHT_GREEN As Color = Color.FromArgb(40, 180, 40)
    ReadOnly PURPLE As Color = Color.FromArgb(180, 0, 180)

    Sub Main()
        Dim logo As Bitmap, fontName$ = FontFace.Verdana
        Dim color1 As New SolidBrush(Color.FromArgb(0, 65, 102))
        Dim color2 As New SolidBrush(Color.FromArgb(0, 172, 221))

        Using g As Graphics2D = New Size(900, 800).CreateGDIDevice(filled:=Color.Transparent)
            Dim isometricView As New IsometricEngine

            isometricView.Add(New Knot(New Point3D(1, 1, 1), scale:=1), GREEN)
            isometricView.Draw(g)

            logo = g.ImageResource
        End Using

        logo = logo.CorpBlank(blankColor:=Color.Transparent)

        Using g As Graphics2D = stdNum.Max(logo.Width, logo.Height) _
            .SquareSize _
            .CreateGDIDevice(filled:=Color.Transparent)

            Dim topleft As New Point With {
                .X = (g.Width - logo.Width) / 2,
                .Y = (g.Height - logo.Height) / 2
            }

            Call g.DrawImageUnscaled(logo, topleft)
            Call g.ImageResource.SaveAs("../../../logo-knot.png")
        End Using

        Using g As Graphics2D = New Size(2000, 500).CreateGDIDevice(filled:=Color.Transparent)

            Call g.DrawImageUnscaled(logo, New Point(50, 50))

            Call g.DrawString("sci", New Font(fontName, 140), color1, New PointF(430, 90))
            Call g.DrawString("BASIC#", New Font(fontName, 200), color2, New PointF(670, 60))
            Call g.DrawString("http://sciBASIC.NET", New Font(FontFace.SegoeUI, 48), color1, New PointF(720, 350))

            logo = g.ImageResource
        End Using

        Call logo _
            .CorpBlank(blankColor:=Color.Transparent, margin:=30) _
            .SaveAs(save)

        logo = save.LoadImage _
            .ColorReplace(color1.Color, Color.White) _
            .ColorReplace(color2.Color, Color.White)

        Call logo.SaveAs("../../../logo-white.png")

    End Sub
End Module
