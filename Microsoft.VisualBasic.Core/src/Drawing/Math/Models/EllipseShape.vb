#Region "Microsoft.VisualBasic::5be62aff17cdb2d20d72d0bea6aeec1c, Microsoft.VisualBasic.Core\src\Drawing\Math\Models\EllipseShape.vb"

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

    '   Total Lines: 64
    '    Code Lines: 36 (56.25%)
    ' Comment Lines: 17 (26.56%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (17.19%)
    '     File Size: 2.16 KB


    '     Class EllipseShape
    ' 
    '         Properties: center, radiusX, radiusY, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: EllipseDrawing, GetPolygonPath, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' Ellipse shape polygon generator
    ''' </summary>
    Public Class EllipseShape

        Public ReadOnly Property radiusX As Double
        Public ReadOnly Property radiusY As Double
        Public ReadOnly Property center As PointF

        ''' <summary>
        ''' any other tagged value with current circle model
        ''' </summary>
        ''' <returns></returns>
        Public Property value As Double

        Sub New(radiusX As Double, radiusY As Double, center As PointF)
            Me.center = center
            Me.radiusX = radiusX
            Me.radiusY = radiusY
        End Sub

        ''' <summary>
        ''' create a new circle model
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="r"></param>
        Sub New(x As Single, y As Single, r As Double)
            Call Me.New(r, r, New PointF(x, y))
        End Sub

        ''' <summary>
        ''' generates the ellipse or circle drawing path
        ''' </summary>
        ''' <returns></returns>
        Public Function GetPolygonPath() As Polygon2D
            Dim path As New List(Of PointF)

            For angle As Integer = 0 To 360
                Call path.Add(EllipseDrawing(radiusX, radiusY, center, angle))
            Next

            Return New Polygon2D(path.ToArray)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function EllipseDrawing(dHalfwidthEllipse As Double, dHalfheightEllipse As Double, origin As PointF, t As Integer) As PointF
            Return New PointF(
                origin.X + dHalfwidthEllipse * std.Cos(t * std.PI / 180),
                origin.Y + dHalfheightEllipse * std.Sin(t * std.PI / 180)
            )
        End Function

        Public Overrides Function ToString() As String
            Return $"[{center.X},{center.Y}] r1={radiusX},r2={radiusY}"
        End Function
    End Class
End Namespace
