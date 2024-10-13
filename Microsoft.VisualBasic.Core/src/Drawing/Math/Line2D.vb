#Region "Microsoft.VisualBasic::1c86d7d9ce2bd6a4484c65e7684c3a90, Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Line2D.vb"

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

    '   Total Lines: 356
    '    Code Lines: 88 (24.72%)
    ' Comment Lines: 253 (71.07%)
    '    - Xml Docs: 75.49%
    ' 
    '   Blank Lines: 15 (4.21%)
    '     File Size: 20.55 KB


    '     Module Line2D
    ' 
    '         Function: linesIntersect, ptLineDist, ptLineDistSq, ptSegDist, ptSegDistSq
    '                   relativeCCW
    ' 
    '     Module Line3D
    ' 
    '         Function: (+2 Overloads) ptLineDist, ptLineDistSq
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

'
' * Copyright (c) 1997, 2013, Oracle and/or its affiliates. All rights reserved.
' * ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
' *

Namespace Imaging.Math2D


    ''' <summary>
    ''' This <code>Line2D</code> represents a line segment in {@code (x,y)}
    ''' coordinate space.  This [Class], like all of the Java 2D API, uses a
    ''' default coordinate system called <i>user space</i> in which the y-axis
    ''' values increase downward and x-axis values increase to the right.  For
    ''' more information on the user space coordinate system, see the
    ''' <a href="https://docs.oracle.com/javase/1.3/docs/guide/2d/spec/j2d-intro.fm2.html#61857">
    ''' Coordinate Systems</a> section of the Java 2D Programmer's Guide.
    ''' 
    ''' This class is only the abstract superclass for all objects that
    ''' store a 2D line segment.
    ''' The actual storage representation of the coordinates is left to
    ''' the subclass.
    ''' 
    ''' @author      Jim Graham
    ''' @since 1.2
    ''' </summary>
    Public Module Line2D

        ''' <summary>
        ''' Returns an indicator of where the specified point
        ''' {@code (px,py)} lies with respect to the line segment from
        ''' {@code (x1,y1)} to {@code (x2,y2)}.
        ''' 
        ''' The return value can be either 1, -1, or 0 and indicates
        ''' in which direction the specified line must pivot around its
        ''' first end point, {@code (x1,y1)}, in order to point at the
        ''' specified point {@code (px,py)}.
        ''' 
        ''' A return value of 1 indicates that the line segment must
        ''' turn in the direction that takes the positive X axis towards
        ''' the negative Y axis.  In the default coordinate system used by
        ''' Java 2D, this direction is counterclockwise.
        ''' 
        ''' A return value of -1 indicates that the line segment must
        ''' turn in the direction that takes the positive X axis towards
        ''' the positive Y axis.  In the default coordinate system, this
        ''' direction is clockwise.
        ''' 
        ''' A return value of 0 indicates that the point lies
        ''' exactly on the line segment.  Note that an indicator value
        ''' of 0 is rare and not useful for determining collinearity
        ''' because of floating point rounding issues.
        ''' 
        ''' If the point is colinear with the line segment, but
        ''' not between the end points, then the value will be -1 if the point
        ''' lies "beyond {@code (x1,y1)}" or 1 if the point lies
        ''' "beyond {@code (x2,y2)}".
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="px"> the X coordinate of the specified point to be
        '''           compared with the specified line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point to be
        '''           compared with the specified line segment </param>
        ''' <returns> an integer that indicates the position of the third specified
        '''                  coordinates with respect to the line segment formed
        '''                  by the first two specified coordinates.
        ''' @since 1.2 </returns>
        Public Function relativeCCW(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Integer
            x2 -= x1
            y2 -= y1
            px -= x1
            py -= y1
            Dim ccw As Double = px * y2 - py * x2
            If ccw = 0.0 Then
                ' The point is colinear, classify based on which side of
                ' the segment the point falls on.  We can calculate a
                ' relative value using the projection of px,py onto the
                ' segment - a negative value indicates the point projects
                ' outside of the segment in the direction of the particular
                ' endpoint used as the origin for the projection.
                ccw = px * x2 + py * y2
                If ccw > 0.0 Then
                    ' Reverse the projection to be relative to the original x2,y2
                    ' x2 and y2 are simply negated.
                    ' px and py need to have (x2 - x1) or (y2 - y1) subtracted
                    '    from them (based on the original values)
                    ' Since we really want to get a positive answer when the
                    '    point is "beyond (x2,y2)", then we want to calculate
                    '    the inverse anyway - thus we leave x2 & y2 negated.
                    px -= x2
                    py -= y2
                    ccw = px * x2 + py * y2
                    If ccw < 0.0 Then ccw = 0.0
                End If
            End If
            Return If(ccw < 0.0, -1, (If(ccw > 0.0, 1, 0)))
        End Function

        ''' <summary>
        ''' Tests if the line segment from {@code (x1,y1)} to
        ''' {@code (x2,y2)} intersects the line segment from {@code (x3,y3)}
        ''' to {@code (x4,y4)}.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the first
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the first
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the first
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the first
        '''           specified line segment </param>
        ''' <param name="x3"> the X coordinate of the start point of the second
        '''           specified line segment </param>
        ''' <param name="y3"> the Y coordinate of the start point of the second
        '''           specified line segment </param>
        ''' <param name="x4"> the X coordinate of the end point of the second
        '''           specified line segment </param>
        ''' <param name="y4"> the Y coordinate of the end point of the second
        '''           specified line segment </param>
        ''' <returns> <code>true</code> if the first specified line segment
        '''                  and the second specified line segment intersect
        '''                  each other; <code>false</code> otherwise.
        ''' @since 1.2 </returns>
        Public Function linesIntersect(x1 As Double, y1 As Double, x2 As Double, y2 As Double, x3 As Double, y3 As Double, x4 As Double, y4 As Double) As Boolean
            Return ((relativeCCW(x1, y1, x2, y2, x3, y3) * relativeCCW(x1, y1, x2, y2, x4, y4) <= 0) AndAlso (relativeCCW(x3, y3, x4, y4, x1, y1) * relativeCCW(x3, y3, x4, y4, x2, y2) <= 0))
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a point to a line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the specified end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified point to the specified line segment. </returns>
        Public Function ptSegDistSq(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            ' Adjust vectors relative to x1,y1
            ' x2,y2 becomes relative vector from x1,y1 to end of segment
            x2 -= x1
            y2 -= y1
            ' px,py becomes relative vector from x1,y1 to test point
            px -= x1
            py -= y1
            Dim dotprod As Double = px * x2 + py * y2
            Dim projlenSq As Double
            If dotprod <= 0.0 Then
                ' px,py is on the side of x1,y1 away from x2,y2
                ' distance to segment is length of px,py vector
                ' "length of its (clipped) projection" is now 0.0
                projlenSq = 0.0
            Else
                ' switch to backwards vectors relative to x2,y2
                ' x2,y2 are already the negative of x1,y1=>x2,y2
                ' to get px,py to be the negative of px,py=>x2,y2
                ' the dot product of two negated vectors is the same
                ' as the dot product of the two normal vectors
                px = x2 - px
                py = y2 - py
                dotprod = px * x2 + py * y2
                If dotprod <= 0.0 Then
                    ' px,py is on the side of x2,y2 away from x1,y1
                    ' distance to segment is length of (backwards) px,py vector
                    ' "length of its (clipped) projection" is now 0.0
                    projlenSq = 0.0
                Else
                    ' px,py is between x1,y1 and x2,y2
                    ' dotprod is the length of the px,py vector
                    ' projected on the x2,y2=>x1,y1 vector times the
                    ' length of the x2,y2=>x1,y1 vector
                    projlenSq = dotprod * dotprod / (x2 * x2 + y2 * y2)
                End If
            End If
            ' Distance to line is now the length of the relative point
            ' vector minus the length of its projection onto the line
            ' (which is zero if the projection falls outside the range
            '  of the line segment).
            Dim lenSq As Double = px * px + py * py - projlenSq
            If lenSq < 0 Then lenSq = 0
            Return lenSq
        End Function

        ''' <summary>
        ''' Returns the distance from a point to a line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the specified end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <returns> a double value that is the distance from the specified point
        '''                          to the specified line segment. </returns>
        Public Function ptSegDist(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            Return std.Sqrt(ptSegDistSq(x1, y1, x2, y2, px, py))
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a point to a line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by the specified coordinates.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the specified line </param>
        ''' <param name="y1"> the Y coordinate of the start point of the specified line </param>
        ''' <param name="x2"> the X coordinate of the end point of the specified line </param>
        ''' <param name="y2"> the Y coordinate of the end point of the specified line </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified point to the specified line. </returns>
        Public Function ptLineDistSq(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            ' Adjust vectors relative to x1,y1
            ' x2,y2 becomes relative vector from x1,y1 to end of segment
            x2 -= x1
            y2 -= y1
            ' px,py becomes relative vector from x1,y1 to test point
            px -= x1
            py -= y1
            Dim dotprod As Double = px * x2 + py * y2
            ' dotprod is the length of the px,py vector
            ' projected on the x1,y1=>x2,y2 vector times the
            ' length of the x1,y1=>x2,y2 vector
            Dim projlenSq As Double = dotprod * dotprod / (x2 * x2 + y2 * y2)
            ' Distance to line is now the length of the relative point
            ' vector minus the length of its projection onto the line
            Dim lenSq As Double = px * px + py * py - projlenSq
            If lenSq < 0 Then lenSq = 0
            Return lenSq
        End Function

        ''' <summary>
        ''' Returns the distance from a point to a line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by the specified coordinates.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the specified line </param>
        ''' <param name="y1"> the Y coordinate of the start point of the specified line </param>
        ''' <param name="x2"> the X coordinate of the end point of the specified line </param>
        ''' <param name="y2"> the Y coordinate of the end point of the specified line </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <returns> a double value that is the distance from the specified
        '''                   point to the specified line. </returns>
        Public Function ptLineDist(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            Return std.Sqrt(ptLineDistSq(x1, y1, x2, y2, px, py))
        End Function
    End Module

    Public Module Line3D

        ''' <summary>
        ''' Returns the square of the distance from a point to a line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by the specified coordinates.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the specified line </param>
        ''' <param name="y1"> the Y coordinate of the start point of the specified line </param>
        ''' <param name="x2"> the X coordinate of the end point of the specified line </param>
        ''' <param name="y2"> the Y coordinate of the end point of the specified line </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified point to the specified line. </returns>
        Public Function ptLineDistSq(x1 As Double, y1 As Double, z1 As Double, x2 As Double, y2 As Double, z2 As Double, px As Double, py As Double, pz As Double) As Double
            ' Adjust vectors relative to x1,y1
            ' x2,y2 becomes relative vector from x1,y1 to end of segment
            x2 -= x1
            y2 -= y1
            z2 -= z1
            ' px,py becomes relative vector from x1,y1 to test point
            px -= x1
            py -= y1
            pz -= z1

            Dim dotprod As Double = px * x2 + py * y2 + pz * z2
            ' dotprod is the length of the px,py vector
            ' projected on the x1,y1=>x2,y2 vector times the
            ' length of the x1,y1=>x2,y2 vector
            Dim projlenSq As Double = dotprod * dotprod / (x2 * x2 + y2 * y2 + z2 * z2)
            ' Distance to line is now the length of the relative point
            ' vector minus the length of its projection onto the line
            Dim lenSq As Double = px * px + py * py + pz * pz - projlenSq
            If lenSq < 0 Then lenSq = 0
            Return lenSq
        End Function

        ''' <summary>
        ''' Returns the distance from a point to a line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by the specified coordinates.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the specified line </param>
        ''' <param name="y1"> the Y coordinate of the start point of the specified line </param>
        ''' <param name="x2"> the X coordinate of the end point of the specified line </param>
        ''' <param name="y2"> the Y coordinate of the end point of the specified line </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <returns> a double value that is the distance from the specified
        '''                   point to the specified line. </returns>
        Public Function ptLineDist(x1 As Double, y1 As Double, z1 As Double, x2 As Double, y2 As Double, z2 As Double, px As Double, py As Double, pz As Double) As Double
            Return std.Sqrt(ptLineDistSq(x1, y1, z1, x2, y2, z2, px, py, pz))
        End Function

        <Extension>
        Public Function ptLineDist(p As PointF3D, la As PointF3D, lb As PointF3D) As Double
            Return ptLineDist(la.X, la.Y, la.Z, lb.X, lb.Y, lb.Z, p.X, p.Y, p.Z)
        End Function
    End Module
End Namespace
