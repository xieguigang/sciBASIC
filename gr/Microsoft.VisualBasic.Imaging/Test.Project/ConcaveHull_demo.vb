#Region "Microsoft.VisualBasic::8aa8d1ff744124ed7d2ae273c6888c57, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Test.Project\ConcaveHull_demo.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull

Module ConcaveHull_demo

    <Extension>
    Sub Run_ConcaveHull_demo(points As List(Of Point))
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

