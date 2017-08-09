#Region "Microsoft.VisualBasic::56c3fa26e3e0e3e8745bb3f1160015f8, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConvexHull\ConvexHull.vb"

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

Namespace Drawing2D.Math2D.ConvexHull

    ''' <summary>
    ''' In mathematics, the convex hull or convex envelope of a set X of points in the Euclidean plane 
    ''' or in a Euclidean space (or, more generally, in an affine space over the reals) is the smallest 
    ''' convex set that contains X. For instance, when X is a bounded subset of the plane, the convex 
    ''' hull may be visualized as the shape enclosed by a rubber band stretched around X.
    ''' (多边形的点的数量必须要至少3个点)
    ''' </summary>
    Public Module ConvexHull

        Public Const TURN__LEFT% = 1
        Public Const TURN_RIGHT% = -1
        Public Const TURN_NONE = 0

        Friend Function turn(p As Point, q As Point, r As Point) As Integer
            Return ((q.X - p.X) * (r.Y - p.Y()) - (r.X - p.X) * (q.Y - p.Y)).CompareTo(0)
        End Function

        ''' <summary>
        ''' 点的数量必须要多于2个!
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        Public Function JarvisMatch(points As IEnumerable(Of Point)) As Point()
            Return Math2D.ConvexHull.JarvisMatch.ConvexHull(points)
        End Function

        ''' <summary>
        ''' 点的数量必须要多于2个!
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        Public Function GrahamScan(points As IEnumerable(Of Point)) As Point()
            Return Math2D.ConvexHull.GrahamScan.ConvexHull(points)
        End Function
    End Module
End Namespace
