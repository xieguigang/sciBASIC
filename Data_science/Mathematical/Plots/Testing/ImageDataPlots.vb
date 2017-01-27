#Region "Microsoft.VisualBasic::f30181cbe315f779d0087c4468dc1503, ..\sciBASIC#\Data_science\Mathematical\Plots\Testing\ImageDataPlots.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots.ImageDataExtensions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Module ImageDataPlots

    Sub Main()
        ' Call Plot2DMap()
        Call Plot3DMap()

        Pause()
    End Sub

    Sub Plot2DMap()
        Dim img As Image = LoadImage("G:\GCModeller\src\runtime\sciBASIC#\etc\lena\f13e6388b975d9434ad9e1a41272d242_1_orig.jpg")
        Dim out As Image = Image2DMap(img)

        Call out.SaveAs("./testmap.png")
    End Sub

    Sub Plot3DMap()
        Dim img As Image = LoadImage("G:\GCModeller\src\runtime\sciBASIC#\etc\lena\f13e6388b975d9434ad9e1a41272d242_1_orig.jpg")
        Dim out As Image = Image3DMap(
            img, New Camera With {
                .ViewDistance = -50,
                .screen = New Size(img.Width * 8, img.Height * 5),
                .angleX = 90,
                .angleY = 90,
                .angleZ = 10,
                .offset = New Point(-0, 0)
            },
            steps:=10)

        Call out.SaveAs("./testmap3.png")
    End Sub
End Module
