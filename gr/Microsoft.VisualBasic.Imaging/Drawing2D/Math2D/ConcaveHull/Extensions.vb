#Region "Microsoft.VisualBasic::261df770fe487574e3500871578e242c, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\Extensions.vb"

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

'   Total Lines: 49
'    Code Lines: 16 (32.65%)
' Comment Lines: 29 (59.18%)
'    - Xml Docs: 89.66%
' 
'   Blank Lines: 4 (8.16%)
'     File Size: 2.72 KB


'     Module Extensions
' 
'         Function: ConcaveHull
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

Namespace Drawing2D.Math2D.ConcaveHull

    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' The Concave Hull of a Set of Points
        ''' 
        ''' In geometry, the convex hull or convex envelope or convex closure of a shape is the 
        ''' smallest convex set that contains it. The convex hull may be defined either as the 
        ''' intersection of all convex sets containing a given subset of a Euclidean space, or 
        ''' equivalently as the set of all convex combinations of points in the subset. For a 
        ''' bounded subset of the plane, the convex hull may be visualized as the shape enclosed 
        ''' by a rubber band stretched around the subset.
        '''
        ''' Convex hulls of open sets are open, and convex hulls of compact sets are compact. Every 
        ''' compact convex set is the convex hull of its extreme points. The convex hull operator 
        ''' is an example of a closure operator, and every antimatroid can be represented by applying
        ''' this closure operator to finite sets of points. The algorithmic problems of finding the 
        ''' convex hull of a finite set of points in the plane or other low-dimensional Euclidean 
        ''' spaces, and its dual problem of intersecting half-spaces, are fundamental problems of 
        ''' computational geometry. They can be solved in time (log)(n\log n) for two or three 
        ''' dimensional point sets, and in time matching the worst-case output complexity given by 
        ''' the upper bound theorem in higher dimensions.
        '''
        ''' As well as for finite point sets, convex hulls have also been studied for simple polygons, 
        ''' Brownian motion, space curves, and epigraphs of functions. Convex hulls have wide 
        ''' applications in mathematics, statistics, combinatorial optimization, economics, geometric 
        ''' modeling, and ethology. Related structures include the orthogonal convex hull, convex 
        ''' layers, Delaunay triangulation and Voronoi diagram, and convex skull.
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="r">the ball radius</param>
        ''' <returns></returns>
        <Extension>
        Public Function ConcaveHull(points As IEnumerable(Of PointF), Optional r# = -1) As PointF()
            With New BallConcave(points)
                If r# <= 0 Then
                    r# = .RecomandedRadius
                End If

                Return .GetConcave_Ball(r) _
                       .ToArray
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ConcaveHull(points As Polygon2D, Optional r# = -1) As PointF()
            Return points.AsEnumerable.ConcaveHull(r#)
        End Function
    End Module
End Namespace
