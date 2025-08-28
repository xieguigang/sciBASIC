#Region "Microsoft.VisualBasic::4aa299b44f9e644e102b93714eaab60f, gr\Microsoft.VisualBasic.Imaging\test\filter_test.vb"

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

    '   Total Lines: 44
    '    Code Lines: 34 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.33 KB


    ' Module filter_test
    ' 
    '     Sub: Main, testDraw
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.DelaunayVoronoi
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions

Module filter_test

    Sub Main()
        Call testDraw()

        Dim img = "Z:\Capture.PNG".LoadImage
        Dim bitmap = BitmapBuffer.FromImage(img)
        Dim bin = bitmap.ostuFilter(flip:=False)

        Call bin.Save("Z:/aaa.bmp")
    End Sub

    Sub testDraw()
        Dim size As New Size(1000, 800)
        Dim points = PoissonDiskGenerator.Generate(50, 1000)
        Dim split As New Voronoi(points, New Rectf(0, 0, 1000, 800))
        Dim g As Graphics2D = size.CreateGDIDevice(Color.White)

        For Each pt In points
            Call g.DrawCircle(New PointF(pt.x, pt.y), 8, Brushes.Red)
        Next

        Dim regions = split.Regions

        For Each r As Polygon2D In regions
            Call g.DrawPolygon(Pens.Blue, r.AsEnumerable.ToArray)
        Next

        Call g.Flush()
        Call g.ImageResource.SaveAs("Z:/test.png")

        Pause()
    End Sub
End Module
