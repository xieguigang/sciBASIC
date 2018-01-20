Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes

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

        Using g As Graphics2D = New Size(1800, 500).CreateGDIDevice(filled:=Color.Transparent)

            Call g.DrawImageUnscaled(logo, New Point(50, 50))

            Call g.DrawString("sci", New Font(fontName, 120), color1, New PointF(400, 90))
            Call g.DrawString("BASIC#", New Font(fontName, 200), color2, New PointF(575, 60))
            Call g.DrawString("http://sciBASIC.NET", New Font(FontFace.SegoeUI, 48), color1, New PointF(630, 350))

            logo = g.ImageResource
        End Using

        Call logo _
            .CorpBlank(blankColor:=Color.Transparent, margin:=30) _
            .SaveAs(save)

    End Sub
End Module
