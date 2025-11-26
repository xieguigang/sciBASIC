#Region "Microsoft.VisualBasic::76f4978f291d8dcd9657ff7cf0863048, Microsoft.VisualBasic.Core\src\Drawing\Math\Geometry\AffineTransform.vb"

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

    '   Total Lines: 74
    '    Code Lines: 30 (40.54%)
    ' Comment Lines: 36 (48.65%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (10.81%)
    '     File Size: 2.79 KB


    '     Class AffineTransform
    ' 
    '         Properties: a, b, c, d, e
    '                     f
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ApplyTo, ApplyToPoint, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq

Namespace Imaging.Math2D

    ''' <summary>
    ''' 2D Affine Transformation parameters.
    ''' This represents a more general transform than similarity, allowing for non-uniform scaling and shearing.
    ''' x' = ax + by + c
    ''' y' = dx + ey + f
    ''' </summary>
    Public Class AffineTransform : Implements GeometryTransform

        ''' <summary>
        ''' The ‘a’ parameter in x’ = ax + by + c
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property a As Double = 1
        ''' <summary>
        ''' The ‘b’ parameter in x’ = ax + by + c
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property b As Double = 0
        ''' <summary>
        ''' The ‘c’ parameter (translation x) in x’ = ax + by + c
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property c As Double = 0
        ''' <summary>
        ''' The ‘d’ parameter in y’ = dx + ey + f
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property d As Double = 0
        ''' <summary>
        ''' The ‘e’ parameter in y’ = dx + ey + f
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property e As Double = 1
        ''' <summary>
        ''' The ‘f’ parameter (translation y) in y’ = dx + ey + f
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property f As Double = 0

        ''' <summary>
        ''' Creates an identity (no-op) transform.
        ''' </summary>
        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"2D Affine Transform {{ x' = {a.ToString("F4")} x + {b.ToString("F4")} y + {c.ToString("F4")}; y' = {d.ToString("F4")} x + {e.ToString("F4")} y + {f.ToString("F4")} }}"
        End Function

        ''' <summary>
        ''' Applies this transform to a point.
        ''' </summary>
        Public Function ApplyToPoint(pt As PointF) As PointF
            Return New PointF(
                a * pt.X + b * pt.Y + c,
                d * pt.X + e * pt.Y + f
            )
        End Function

        Public Function ApplyTo(polygon As Polygon2D) As Polygon2D Implements GeometryTransform.ApplyTo
            Dim t = polygon.AsEnumerable.Select(Function(pt) ApplyToPoint(pt)).ToArray
            Dim x As Double() = t.Select(Function(pt) CDbl(pt.X)).ToArray
            Dim y As Double() = t.Select(Function(pt) CDbl(pt.Y)).ToArray

            Return New Polygon2D(x, y)
        End Function
    End Class
End Namespace
