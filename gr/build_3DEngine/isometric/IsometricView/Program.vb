#Region "Microsoft.VisualBasic::db58ff13194cd94ac30cc6393fbfcd8f, sciBASIC#\gr\build_3DEngine\isometric\IsometricView\Program.vb"

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

    '   Total Lines: 25
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 716.00 B


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Program

    Sub Main()

        With New IsometricViewTest

            Call .doScreenshotCylinder()
            Call .doScreenshotExtrude()
            Call .doScreenshotGrid()
            Call .doScreenshotKnot()
            Call .doScreenshotOctahedron()
            Call .doScreenshotOne()
            Call .doScreenshotPath3D()
            Call .doScreenshotPrism()
            Call .doScreenshotPyramid()
            Call .doScreenshotRotateZ()
            Call .doScreenshotScale()
            Call .doScreenshotStairs()
            Call .doScreenshotThree()
            Call .doScreenshotTranslate()
            Call .doScreenshotTwo()

        End With
    End Sub
End Module
