#Region "Microsoft.VisualBasic::8299c942ce8646eb1510a972593ba7cb, Data_science\Visualization\test\ImageDataPlots.vb"

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

    ' Module ImageDataPlots
    ' 
    '     Sub: Main, Plot2DMap, Plot3DMap
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.ImageDataExtensions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver

Module ImageDataPlots

    Sub Main()
        ' Call Plot2DMap()
        Call Plot3DMap()

        Pause()
    End Sub

    Sub Plot2DMap()
        Dim img As Image = LoadImage("G:\GCModeller\src\runtime\sciBASIC#\etc\lena\f13e6388b975d9434ad9e1a41272d242_1_orig.jpg")
        Dim out As GraphicsData = Image2DMap(img)

        Call out.Save("./testmap.png")
    End Sub

    Sub Plot3DMap()
        Dim img As Image = LoadImage("G:\GCModeller\src\runtime\sciBASIC#\etc\lena\f13e6388b975d9434ad9e1a41272d242_1_orig.jpg")
        Dim out As GraphicsData = Image3DMap(
            img, New Camera With {
                .ViewDistance = -50,
                .screen = New Size(img.Width * 8, img.Height * 5),
                .angleX = 90,
                .angleY = 90,
                .angleZ = 10,
                .offset = New Point(-0, 0)
            },
            steps:=10)

        Call out.Save("./testmap3.png")
    End Sub
End Module
