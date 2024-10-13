#Region "Microsoft.VisualBasic::79b4804aa82cd9789b2957440c35bb0b, Microsoft.VisualBasic.Core\src\Drawing\Math\Models\EllipseShape.vb"

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
    '    Code Lines: 27 (71.05%)
    ' Comment Lines: 3 (7.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.21 KB


    '     Class EllipseShape
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EllipseDrawing, GetPolygonPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' Ellipse shape polygon generator
    ''' </summary>
    Public Class EllipseShape

        ReadOnly radiusX As Double
        ReadOnly radiusY As Double
        ReadOnly center As PointF

        Sub New(radiusX As Double, radiusY As Double, center As PointF)
            Me.center = center
            Me.radiusX = radiusX
            Me.radiusY = radiusY
        End Sub

        Public Function GetPolygonPath() As Polygon2D
            Dim path As New List(Of PointF)

            For angle As Integer = 0 To 360
                Call path.Add(EllipseDrawing(radiusX, radiusY, center, angle))
            Next

            Return New Polygon2D(path.ToArray)
        End Function

        Private Shared Function EllipseDrawing(dHalfwidthEllipse As Double, dHalfheightEllipse As Double, origin As PointF, t As Integer) As PointF
            Return New PointF(
                origin.X + dHalfwidthEllipse * std.Cos(t * std.PI / 180),
                origin.Y + dHalfheightEllipse * std.Sin(t * std.PI / 180)
            )
        End Function
    End Class
End Namespace
