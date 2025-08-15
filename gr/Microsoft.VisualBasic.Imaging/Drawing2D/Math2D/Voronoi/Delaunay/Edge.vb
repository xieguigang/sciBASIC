#Region "Microsoft.VisualBasic::371e1d40dc8f2053e8f8e4285457532f, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\Edge.vb"

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

    '   Total Lines: 318
    '    Code Lines: 255 (80.19%)
    ' Comment Lines: 14 (4.40%)
    '    - Xml Docs: 21.43%
    ' 
    '   Blank Lines: 49 (15.41%)
    '     File Size: 10.89 KB


    '     Class Edge
    ' 
    '         Properties: ClippedEnds, EdgeIndex, LeftSite, LeftVertex, RightSite
    '                     RightVertex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CompareSitesDistances, CompareSitesDistances_MAX, Create, CreateBisectingEdge, Init
    '                   IsPartOfConvexHull, SelectEdgesForSitePoint, Site, SitesDistance, ToString
    '                   Vertex, Visible
    ' 
    '         Sub: ClipVertices, Dispose, SetVertex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Drawing2D.Math2D.DelaunayVoronoi

    ''' <summary> The line segment connecting the two Sites is part of the Delaunay triangulation
    ''' The line segment connecting the two Vertices is part of the Voronoi diagram </summary>
    Public Class Edge

#Region "Pool"
        Private Shared pool As Queue(Of Edge) = New Queue(Of Edge)()

        Private Shared nEdges As Integer = 0

        ''' <summary> This is the only way to create a new Edge </summary>
        Public Shared Function CreateBisectingEdge(s0 As Site, s1 As Site) As Edge
            Dim dx, dy As Single
            Dim absdx, absdy As Single
            Dim a, b, c As Single

            dx = s1.x - s0.x
            dy = s1.y - s0.y
            absdx = If(dx > 0, dx, -dx)
            absdy = If(dy > 0, dy, -dy)
            c = s0.x * dx + s0.y * dy + (dx * dx + dy * dy) * 0.5F

            If absdx > absdy Then
                a = 1
                b = dy / dx
                c /= dx
            Else
                b = 1
                a = dx / dy
                c /= dy
            End If

            Dim edge As Edge = Create()

            edge.LeftSite = s0
            edge.RightSite = s1
            s0.AddEdge(edge)
            s1.AddEdge(edge)

            edge.a = a
            edge.b = b
            edge.c = c

            Return edge
        End Function

        Private Shared Function Create() As Edge
            Dim edge As Edge
            If pool.Count > 0 Then
                edge = pool.Dequeue()
                edge.Init()
            Else
                edge = New Edge()
            End If

            Return edge
        End Function
#End Region

        Public Shared Function SelectEdgesForSitePoint(coord As Vector2D, edgesToTest As List(Of Edge)) As List(Of Edge)
            Return edgesToTest.FindAll(Function(e)
                                           If e.LeftSite IsNot Nothing Then
                                               If e.LeftSite.Coord Is coord Then Return True
                                           End If
                                           If e.RightSite IsNot Nothing Then
                                               If e.RightSite.Coord Is coord Then Return True
                                           End If
                                           Return False
                                       End Function)
        End Function

        Public Shared ReadOnly DELETED As Edge = New Edge()

#Region "Object"
        ' The equation of the edge: ax + by = c
        Public a, b, c As Single

        ' The two Voronoi vertices that the edge connects (if one of them is null, the edge extends to infinity)
        Private leftVertexField As Vertex
        Public ReadOnly Property LeftVertex As Vertex
            Get
                Return leftVertexField
            End Get
        End Property

        Private rightVertexField As Vertex
        Public ReadOnly Property RightVertex As Vertex
            Get
                Return rightVertexField
            End Get
        End Property

        Public Function Vertex(leftRight As LR) As Vertex
            Return If(leftRight Is LR.LEFT, leftVertexField, rightVertexField)
        End Function

        Public Sub SetVertex(leftRight As LR, v As Vertex)
            If leftRight Is LR.LEFT Then
                leftVertexField = v
            Else
                rightVertexField = v
            End If
        End Sub

        Public Function IsPartOfConvexHull() As Boolean
            Return leftVertexField Is Nothing OrElse rightVertexField Is Nothing
        End Function

        Public Function SitesDistance() As Single
            Return (LeftSite.Coord - RightSite.Coord).Length
        End Function

        Public Shared Function CompareSitesDistances_MAX(edge0 As Edge, edge1 As Edge) As Integer
            Dim length0 As Single = edge0.SitesDistance()
            Dim length1 As Single = edge1.SitesDistance()
            If length0 < length1 Then
                Return 1
            End If
            If length0 > length1 Then
                Return -1
            End If
            Return 0
        End Function

        Public Shared Function CompareSitesDistances(edge0 As Edge, edge1 As Edge) As Integer
            Return -CompareSitesDistances_MAX(edge0, edge1)
        End Function

        ' Once clipVertices() is called, this Disctinary will hold two Points
        ' representing the clipped coordinates of the left and the right ends...
        Private clippedVertices As LRCollection(Of Vector2D)
        Public ReadOnly Property ClippedEnds As LRCollection(Of Vector2D)
            Get
                Return clippedVertices
            End Get
        End Property

        ' Unless the entire Edge is outside the bounds.
        ' In that case visible will be false:
        Public Function Visible() As Boolean
            Return clippedVertices IsNot Nothing
        End Function

        ' The two input Sites for which this Edge is a bisector:
        Private sites As LRCollection(Of Site)
        Public Property LeftSite As Site
            Get
                Return sites(LR.LEFT)
            End Get
            Set(value As Site)
                sites(LR.LEFT) = value
            End Set
        End Property
        Public Property RightSite As Site
            Get
                Return sites(LR.RIGHT)
            End Get
            Set(value As Site)
                sites(LR.RIGHT) = value
            End Set
        End Property

        Public Function Site(leftRight As LR) As Site
            Return sites(leftRight)
        End Function

        Private edgeIndexField As Integer
        Public ReadOnly Property EdgeIndex As Integer
            Get
                Return edgeIndexField
            End Get
        End Property

        Public Sub Dispose()
            leftVertexField = Nothing
            rightVertexField = Nothing
            If clippedVertices IsNot Nothing Then
                clippedVertices.Clear()
                clippedVertices = Nothing
            End If
            sites.Clear()
            sites = Nothing

            pool.Enqueue(Me)
        End Sub

        Public Sub New()
            edgeIndexField = std.Min(Threading.Interlocked.Increment(nEdges), nEdges - 1)
            Init()
        End Sub

        Public Function Init() As Edge
            sites = New LRCollection(Of Site)()

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "Edge " & edgeIndexField.ToString() & "; sites " & sites(LR.LEFT).ToString() & ", " & sites(LR.RIGHT).ToString() & "; endVertices " & (If(leftVertexField IsNot Nothing, leftVertexField.VertexIndex.ToString(), "null")) & ", " & (If(rightVertexField IsNot Nothing, rightVertexField.VertexIndex.ToString(), "null")) & "::"
        End Function

        ' 
        ' * Set clippedVertices to contain the two ends of the portion of the Voronoi edge that is visible
        ' * within the bounds. If no part of the Edge falls within the bounds, leave clippedVertices null
        ' * @param bounds

        Public Sub ClipVertices(bounds As Rectf)
            Dim xmin = bounds.x
            Dim ymin = bounds.y
            Dim xmax = bounds.right
            Dim ymax = bounds.bottom

            Dim vertex0, vertex1 As Vertex
            Dim x0, x1, y0, y1 As Single

            If a = 1 AndAlso b >= 0 Then
                vertex0 = rightVertexField
                vertex1 = leftVertexField
            Else
                vertex0 = leftVertexField
                vertex1 = rightVertexField
            End If

            If a = 1 Then
                y0 = ymin
                If vertex0 IsNot Nothing AndAlso vertex0.y > ymin Then
                    y0 = vertex0.y
                End If
                If y0 > ymax Then
                    Return
                End If
                x0 = c - b * y0

                y1 = ymax
                If vertex1 IsNot Nothing AndAlso vertex1.y < ymax Then
                    y1 = vertex1.y
                End If
                If y1 < ymin Then
                    Return
                End If
                x1 = c - b * y1

                If x0 > xmax AndAlso x1 > xmax OrElse x0 < xmin AndAlso x1 < xmin Then
                    Return
                End If

                If x0 > xmax Then
                    x0 = xmax
                    y0 = (c - x0) / b
                ElseIf x0 < xmin Then
                    x0 = xmin
                    y0 = (c - x0) / b
                End If

                If x1 > xmax Then
                    x1 = xmax
                    y1 = (c - x1) / b
                ElseIf x1 < xmin Then
                    x1 = xmin
                    y1 = (c - x1) / b
                End If
            Else
                x0 = xmin
                If vertex0 IsNot Nothing AndAlso vertex0.x > xmin Then
                    x0 = vertex0.x
                End If
                If x0 > xmax Then
                    Return
                End If
                y0 = c - a * x0

                x1 = xmax
                If vertex1 IsNot Nothing AndAlso vertex1.x < xmax Then
                    x1 = vertex1.x
                End If
                If x1 < xmin Then
                    Return
                End If
                y1 = c - a * x1

                If y0 > ymax AndAlso y1 > ymax OrElse y0 < ymin AndAlso y1 < ymin Then
                    Return
                End If

                If y0 > ymax Then
                    y0 = ymax
                    x0 = (c - y0) / a
                ElseIf y0 < ymin Then
                    y0 = ymin
                    x0 = (c - y0) / a
                End If

                If y1 > ymax Then
                    y1 = ymax
                    x1 = (c - y1) / a
                ElseIf y1 < ymin Then
                    y1 = ymin
                    x1 = (c - y1) / a
                End If
            End If

            clippedVertices = New LRCollection(Of Vector2D)()
            If vertex0 Is leftVertexField Then
                clippedVertices(LR.LEFT) = New Vector2D(x0, y0)
                clippedVertices(LR.RIGHT) = New Vector2D(x1, y1)
            Else
                clippedVertices(LR.RIGHT) = New Vector2D(x0, y0)
                clippedVertices(LR.LEFT) = New Vector2D(x1, y1)
            End If
        End Sub
#End Region
    End Class
End Namespace

