#Region "Microsoft.VisualBasic::cdb6317f59bc112da3eed8020d859471, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Geom\Polygon.vb"

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
    '    Code Lines: 36 (81.82%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (18.18%)
    '     File Size: 1.40 KB


    '     Class Polygon
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Area, PolyWinding, SignedDoubleArea
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Class Polygon

        Private vertices As List(Of Vector2D)

        Public Sub New(vertices As List(Of Vector2D))
            Me.vertices = vertices
        End Sub

        Public Function Area() As Single
            Return std.Abs(SignedDoubleArea() * 0.5F)
        End Function

        Public Function PolyWinding() As Winding
            Dim signedDoubleArea As Single = Me.SignedDoubleArea()
            If signedDoubleArea < 0 Then
                Return Winding.CLOCKWISE
            End If
            If signedDoubleArea > 0 Then
                Return Winding.COUNTERCLOCKWISE
            End If
            Return Winding.NONE
        End Function

        Private Function SignedDoubleArea() As Single
            Dim index, nextIndex As Integer
            Dim n = vertices.Count
            Dim point, [next] As Vector2D
            Dim lSignedDoubleArea As Single = 0

            For index = 0 To n - 1
                nextIndex = (index + 1) Mod n
                point = vertices(index)
                [next] = vertices(nextIndex)
                lSignedDoubleArea += point.x * [next].y - [next].x * point.y
            Next

            Return lSignedDoubleArea
        End Function
    End Class
End Namespace
