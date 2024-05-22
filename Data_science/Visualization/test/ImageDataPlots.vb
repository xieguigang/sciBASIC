#Region "Microsoft.VisualBasic::8299c942ce8646eb1510a972593ba7cb, Data_science\Visualization\test\ImageDataPlots.vb"

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

    '   Total Lines: 38
    '    Code Lines: 30 (78.95%)
    ' Comment Lines: 1 (2.63%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (18.42%)
    '     File Size: 1.20 KB


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
