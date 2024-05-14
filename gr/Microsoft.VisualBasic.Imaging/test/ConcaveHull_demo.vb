#Region "Microsoft.VisualBasic::54d04c54ba3eca7e35e68261cc9fc0b6, gr\Microsoft.VisualBasic.Imaging\test\ConcaveHull_demo.vb"

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

    '   Total Lines: 22
    '    Code Lines: 11
    ' Comment Lines: 7
    '   Blank Lines: 4
    '     File Size: 738 B


    ' Module ConcaveHull_demo
    ' 
    '     Sub: Run_ConcaveHull_demo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull

Module ConcaveHull_demo

    <Extension>
    Sub Run_ConcaveHull_demo(points As List(Of PointF))
        'Dim size = 200
        'Dim x = New DoubleRange(100, 900).rand(size)
        'Dim y = New DoubleRange(100, 800).rand(size)
        'Dim z = New DoubleRange(100, 850).rand(size)
        'Dim v = size.Sequence.Select(Function(i) New Point3D With {.X = x(i), .Y = y(i), .Z = z(i)}).ToArray
        'Dim engine As New DelaunayTriangulation(v)

        'Call engine.Triangulate(150)

        With points
            Call .Draw(.ConcaveHull(200))
        End With
    End Sub
End Module
