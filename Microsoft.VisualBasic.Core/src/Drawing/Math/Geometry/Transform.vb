#Region "Microsoft.VisualBasic::ac83eab55793c9e35564ac7ab37731b2, Microsoft.VisualBasic.Core\src\Drawing\Math\Geometry\Transform.vb"

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

    '   Total Lines: 84
    '    Code Lines: 44 (52.38%)
    ' Comment Lines: 28 (33.33%)
    '    - Xml Docs: 89.29%
    ' 
    '   Blank Lines: 12 (14.29%)
    '     File Size: 2.98 KB


    '     Class Transform
    ' 
    '         Properties: scalex, scaley, theta, tx, ty
    ' 
    '         Function: ApplyTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' make geometry transform of a given <see cref="Polygon2D"/> with given <see cref="Transform"/> or <see cref="AffineTransform"/> parameters.
    ''' </summary>
    Public Interface GeometryTransform

        ''' <summary>
        ''' Apply the current transformation parameters to the target polygon object.
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        Function ApplyTo(polygon As Polygon2D) As Polygon2D

    End Interface

    ''' <summary>
    ''' 2D transformation parameters
    ''' </summary>
    Public Class Transform : Implements GeometryTransform

        ''' <summary>
        ''' angle for rotation
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property theta As Double
        ''' <summary>
        ''' translate x
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property tx As Double
        ''' <summary>
        ''' translate y
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property ty As Double
        ''' <summary>
        ''' scale x [1 means no scale]
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property scalex As Double = 1
        ''' <summary>
        ''' scale y [1 means no scale]
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property scaley As Double = 1

        Public Overrides Function ToString() As String
            Return $"rotate_theta:{theta.ToString("F2")}, translate=({tx.ToString("F2")},{ty.ToString("F2")}), scale=({scalex.ToString("F2")},{scaley.ToString("F2")})"
        End Function

        ''' <summary>
        ''' Apply the current transformation parameters to the target polygon object.
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        Public Function ApplyTo(polygon As Polygon2D) As Polygon2D Implements GeometryTransform.ApplyTo
            Dim tx = New Double(polygon.length - 1) {}
            Dim ty = New Double(polygon.length - 1) {}
            Dim cosTheta As Double = std.Cos(theta)
            Dim sinTheta As Double = std.Sin(theta)

            For i As Integer = 0 To polygon.length - 1
                Dim x As Double = polygon.xpoints(i)
                Dim y As Double = polygon.ypoints(i)

                ' 缩放
                x *= scalex
                y *= scaley

                ' 旋转
                Dim xRotated As Double = x * cosTheta - y * sinTheta
                Dim yRotated As Double = x * sinTheta + y * cosTheta

                ' 平移
                tx(i) = xRotated + Me.tx
                ty(i) = yRotated + Me.ty
            Next

            Return New Polygon2D(tx, ty)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(args As (theta As Double, tx As Double, ty As Double, scalex As Double, scaley As Double)) As Transform
            Return New Transform With {
                .theta = args.theta,
                .tx = args.tx,
                .ty = args.ty,
                .scalex = args.scalex,
                .scaley = args.scaley
            }
        End Operator

    End Class
End Namespace
