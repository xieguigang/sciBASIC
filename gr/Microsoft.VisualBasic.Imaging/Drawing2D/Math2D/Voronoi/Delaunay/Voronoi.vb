#Region "Microsoft.VisualBasic::e814ef9aab60c74deeccb5c3a49f4e5a, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\Voronoi.vb"

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

    '   Total Lines: 396
    '    Code Lines: 296 (74.75%)
    ' Comment Lines: 47 (11.87%)
    '    - Xml Docs: 34.04%
    ' 
    '   Blank Lines: 53 (13.38%)
    '     File Size: 15.90 KB


    '     Class Voronoi
    ' 
    '         Properties: Edges, PlotBounds, SitesIndexedByLocation
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Circles, (+2 Overloads) CompareByYThenX, HullEdges, HullPointsInOrder, LeftRegion
    '                   NeighborSitesForSite, Region, Regions, RightRegion, SiteCoords
    '                   VoronoiBoundarayForSite, VoronoiDiagram
    ' 
    '         Sub: AddSite, AddSites, FortunesAlgorithm, Init, LloydRelaxation
    '              Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math
Imports weigthDistributor = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Drawing2D.Math2D.DelaunayVoronoi

    ''' <summary>
    ''' https://github.com/jfg8/csDelaunay
    ''' </summary>
    Public Class Voronoi

        Private sites As SiteList
        Private triangles As List(Of Triangle)

        Public ReadOnly Property Edges As List(Of Edge)

        ''' <summary>
        ''' TODO generalize this so it doesn't have to be a rectangle;
        ''' then we can make the fractal voronois-within-voronois
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PlotBounds As Rectf
        Public ReadOnly Property SitesIndexedByLocation As Dictionary(Of Vector2D, Site)

        Private Sub Reset()
            sites.Dispose()
            sites = Nothing

            For Each t In triangles
                t.Dispose()
            Next
            triangles.Clear()

            For Each e In Edges
                e.Dispose()
            Next
            Edges.Clear()

            _PlotBounds = Rectf.zero
            _SitesIndexedByLocation.Clear()
            _SitesIndexedByLocation = Nothing
        End Sub

        Public Sub New(points As List(Of Vector2D), plotBounds As Rectf)
            Init(points, plotBounds)
        End Sub

        Public Sub New(points As List(Of Vector2D), plotBounds As Rectf, lloydIterations As Integer)
            Init(points, plotBounds)
            LloydRelaxation(lloydIterations)
        End Sub

        Private Sub Init(points As List(Of Vector2D), plotBounds As Rectf)
            sites = New SiteList()
            _SitesIndexedByLocation = New Dictionary(Of Vector2D, Site)()
            AddSites(points)
            _PlotBounds = plotBounds
            triangles = New List(Of Triangle)()
            _Edges = New List(Of Edge)()

            FortunesAlgorithm()
        End Sub

        Private Sub AddSites(points As List(Of Vector2D))
            For i = 0 To points.Count - 1
                AddSite(points(i), i)
            Next
        End Sub

        Private Sub AddSite(p As Vector2D, index As Integer)
            Dim weigth As Single = CSng(weigthDistributor.NextDouble()) * 100
            Dim site As Site = Site.Create(p, index, weigth)
            sites.Add(site)
            SitesIndexedByLocation(p) = site
        End Sub

        Public Function Region(p As Vector2D) As List(Of Vector2D)
            Dim site As Site = Nothing
            If SitesIndexedByLocation.TryGetValue(p, site) Then
                Return site.Region(PlotBounds)
            Else
                Return New List(Of Vector2D)()
            End If
        End Function

        Public Function NeighborSitesForSite(coord As Vector2D) As List(Of Vector2D)
            Dim points As New List(Of Vector2D)()
            Dim site As Site = Nothing
            If SitesIndexedByLocation.TryGetValue(coord, site) Then
                Dim sites As List(Of Site) = site.NeighborSites()
                For Each neighbor In sites
                    points.Add(neighbor.Coord)
                Next
            End If

            Return points
        End Function

        Public Function Circles() As List(Of Circle)
            Return sites.Circles()
        End Function

        Public Function VoronoiBoundarayForSite(coord As Vector2D) As List(Of LineSegment)
            Return LineSegment.VisibleLineSegments(Edge.SelectEdgesForSitePoint(coord, Edges))
        End Function
        ' 
        Public Function VoronoiDiagram() As List(Of LineSegment)
            Return LineSegment.VisibleLineSegments(Edges)
        End Function

        Public Function HullEdges() As List(Of Edge)
            Return Edges.FindAll(Function(edge) edge.IsPartOfConvexHull())
        End Function

        Public Function HullPointsInOrder() As List(Of Vector2D)
            Dim hullEdges As List(Of Edge) = Me.HullEdges()

            Dim points As New List(Of Vector2D)()
            If hullEdges.Count = 0 Then
                Return points
            End If

            Dim reorderer As New EdgeReorderer(hullEdges, GetType(Site))
            hullEdges = reorderer.Edges
            Dim orientations = reorderer.EdgeOrientations
            reorderer.Dispose()

            Dim orientation As LR
            For i = 0 To hullEdges.Count - 1
                Dim edge = hullEdges(i)
                orientation = orientations(i)
                points.Add(edge.Site(orientation).Coord)
            Next
            Return points
        End Function

        ''' <summary>
        ''' [output] the generated region polygon data
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function Regions() As IEnumerable(Of Polygon2D)
            For Each shape As List(Of Vector2D) In sites.Regions(PlotBounds)
                If shape.Any Then
                    Yield New Polygon2D(shape)
                End If
            Next
        End Function

        ''' <summary>
        ''' the input points
        ''' </summary>
        ''' <returns></returns>
        Public Function SiteCoords() As List(Of Vector2D)
            Return sites.SiteCoords()
        End Function

        Private Sub FortunesAlgorithm()
            Dim newSite, bottomSite, topSite, tempSite As Site
            Dim v, vertex As Vertex
            Dim newIntStar = Vector2D.Zero
            Dim leftRight As LR
            Dim lbnd, rbnd, llbnd, rrbnd, bisector As Halfedge
            Dim edge As Edge

            Dim dataBounds As Rectf = sites.GetSitesBounds()

            Dim sqrtSitesNb As Integer = CInt(std.Sqrt(sites.Count() + 4))
            Dim heap As HalfedgePriorityQueue = New HalfedgePriorityQueue(dataBounds.y, dataBounds.height, sqrtSitesNb)
            Dim edgeList As EdgeList = New EdgeList(dataBounds.x, dataBounds.width, sqrtSitesNb)
            Dim halfEdges As List(Of Halfedge) = New List(Of Halfedge)()
            Dim vertices As List(Of Vertex) = New List(Of Vertex)()

            Dim bottomMostSite As Site = sites.Next()
            newSite = sites.Next()

            While True
                If Not heap.Empty() Then
                    newIntStar = heap.Min()
                End If

                If newSite IsNot Nothing AndAlso (heap.Empty() OrElse CompareByYThenX(newSite, newIntStar) < 0) Then
                    ' New site is smallest
                    'Debug.Log("smallest: new site " + newSite);

                    ' Step 8:
                    lbnd = edgeList.EdgeListLeftNeighbor(newSite.Coord)    ' The halfedge just to the left of newSite
                    'UnityEngine.Debug.Log("lbnd: " + lbnd);
                    rbnd = lbnd.edgeListRightNeighbor      ' The halfedge just to the right
                    'UnityEngine.Debug.Log("rbnd: " + rbnd);
                    bottomSite = RightRegion(lbnd, bottomMostSite)         ' This is the same as leftRegion(rbnd)
                    ' This Site determines the region containing the new site
                    'UnityEngine.Debug.Log("new Site is in region of existing site: " + bottomSite);

                    ' Step 9
                    edge = Edge.CreateBisectingEdge(bottomSite, newSite)
                    'UnityEngine.Debug.Log("new edge: " + edge);
                    Edges.Add(edge)

                    bisector = Halfedge.Create(edge, LR.LEFT)
                    halfEdges.Add(bisector)
                    ' Inserting two halfedges into edgelist constitutes Step 10:
                    ' Insert bisector to the right of lbnd:
                    edgeList.Insert(lbnd, bisector)

                    ' First half of Step 11:
                    vertex = Vertex.Intersect(lbnd, bisector)
                    If vertex IsNot Nothing Then
                        vertices.Add(vertex)
                        heap.Remove(lbnd)
                        lbnd.vertex = vertex
                        lbnd.ystar = vertex.y + newSite.Dist(vertex)
                        heap.Insert(lbnd)
                    End If

                    lbnd = bisector
                    bisector = Halfedge.Create(edge, LR.RIGHT)
                    halfEdges.Add(bisector)
                    ' Second halfedge for Step 10::
                    ' Insert bisector to the right of lbnd:
                    edgeList.Insert(lbnd, bisector)

                    ' Second half of Step 11:
                    vertex = Vertex.Intersect(bisector, rbnd)
                    If vertex IsNot Nothing Then
                        vertices.Add(vertex)
                        bisector.vertex = vertex
                        bisector.ystar = vertex.y + newSite.Dist(vertex)
                        heap.Insert(bisector)
                    End If

                    newSite = sites.Next()
                ElseIf Not heap.Empty() Then
                    ' Intersection is smallest
                    lbnd = heap.ExtractMin()
                    llbnd = lbnd.edgeListLeftNeighbor
                    rbnd = lbnd.edgeListRightNeighbor
                    rrbnd = rbnd.edgeListRightNeighbor
                    bottomSite = LeftRegion(lbnd, bottomMostSite)
                    topSite = RightRegion(rbnd, bottomMostSite)
                    ' These three sites define a Delaunay triangle
                    ' (not actually using these for anything...)
                    ' triangles.Add(new Triangle(bottomSite, topSite, RightRegion(lbnd, bottomMostSite)));

                    v = lbnd.vertex
                    v.SetIndex()
                    lbnd.edge.SetVertex(lbnd.leftRight, v)
                    rbnd.edge.SetVertex(rbnd.leftRight, v)
                    edgeList.Remove(lbnd)
                    heap.Remove(rbnd)
                    edgeList.Remove(rbnd)
                    leftRight = LR.LEFT
                    If bottomSite.y > topSite.y Then
                        tempSite = bottomSite
                        bottomSite = topSite
                        topSite = tempSite
                        leftRight = LR.RIGHT
                    End If
                    edge = Edge.CreateBisectingEdge(bottomSite, topSite)
                    Edges.Add(edge)
                    bisector = Halfedge.Create(edge, leftRight)
                    halfEdges.Add(bisector)
                    edgeList.Insert(llbnd, bisector)
                    edge.SetVertex(LR.Other(leftRight), v)
                    vertex = Vertex.Intersect(llbnd, bisector)
                    If vertex IsNot Nothing Then
                        vertices.Add(vertex)
                        heap.Remove(llbnd)
                        llbnd.vertex = vertex
                        llbnd.ystar = vertex.y + bottomSite.Dist(vertex)
                        heap.Insert(llbnd)
                    End If
                    vertex = Vertex.Intersect(bisector, rrbnd)
                    If vertex IsNot Nothing Then
                        vertices.Add(vertex)
                        bisector.vertex = vertex
                        bisector.ystar = vertex.y + bottomSite.Dist(vertex)
                        heap.Insert(bisector)
                    End If
                Else
                    Exit While
                End If
            End While

            ' Heap should be empty now
            heap.Dispose()
            edgeList.Dispose()

            For Each halfedge In halfEdges
                halfedge.ReallyDispose()
            Next
            halfEdges.Clear()

            ' we need the vertices to clip the edges
            For Each e In Edges
                e.ClipVertices(PlotBounds)
            Next
            ' But we don't actually ever use them again!
            For Each ve In vertices
                ve.Dispose()
            Next
            vertices.Clear()
        End Sub

        Public Sub LloydRelaxation(nbIterations As Integer)
            ' Reapeat the whole process for the number of iterations asked
            For i = 0 To nbIterations - 1
                Dim newPoints As List(Of Vector2D) = New List(Of Vector2D)()
                ' Go thourgh all sites
                sites.ResetListIndex()
                Dim site As Site = sites.Next()

                While site IsNot Nothing
                    ' Loop all corners of the site to calculate the centroid
                    Dim region = site.Region(PlotBounds)
                    If region.Count < 1 Then
                        site = sites.Next()
                        Continue While
                    End If

                    Dim centroid = Vector2D.Zero
                    Dim signedArea As Single = 0
                    Dim x0 As Single = 0
                    Dim y0 As Single = 0
                    Dim x1 As Single = 0
                    Dim y1 As Single = 0
                    Dim a As Single = 0
                    ' For all vertices except last
                    For j = 0 To region.Count - 1 - 1
                        x0 = region(j).x
                        y0 = region(j).y
                        x1 = region(j + 1).x
                        y1 = region(j + 1).y
                        a = x0 * y1 - x1 * y0
                        signedArea += a
                        centroid.x += (x0 + x1) * a
                        centroid.y += (y0 + y1) * a
                    Next
                    ' Do last vertex
                    x0 = region(region.Count - 1).x
                    y0 = region(region.Count - 1).y
                    x1 = region(0).x
                    y1 = region(0).y
                    a = x0 * y1 - x1 * y0
                    signedArea += a
                    centroid.x += (x0 + x1) * a
                    centroid.y += (y0 + y1) * a

                    signedArea *= 0.5F
                    centroid.x /= 6 * signedArea
                    centroid.y /= 6 * signedArea
                    ' Move site to the centroid of its Voronoi cell
                    newPoints.Add(centroid)
                    site = sites.Next()
                End While

                ' Between each replacement of the cendroid of the cell,
                ' we need to recompute Voronoi diagram:
                Dim origPlotBounds = PlotBounds
                Reset()
                Init(newPoints, origPlotBounds)
            Next
        End Sub

        Private Function LeftRegion(he As Halfedge, bottomMostSite As Site) As Site
            Dim edge = he.edge
            If edge Is Nothing Then
                Return bottomMostSite
            End If
            Return edge.Site(he.leftRight)
        End Function

        Private Function RightRegion(he As Halfedge, bottomMostSite As Site) As Site
            Dim edge = he.edge
            If edge Is Nothing Then
                Return bottomMostSite
            End If
            Return edge.Site(LR.Other(he.leftRight))
        End Function

        Public Shared Function CompareByYThenX(s1 As Site, s2 As Site) As Integer
            If s1.y < s2.y Then Return -1
            If s1.y > s2.y Then Return 1
            If s1.x < s2.x Then Return -1
            If s1.x > s2.x Then Return 1
            Return 0
        End Function

        Public Shared Function CompareByYThenX(s1 As Site, s2 As Vector2D) As Integer
            If s1.y < s2.y Then Return -1
            If s1.y > s2.y Then Return 1
            If s1.x < s2.x Then Return -1
            If s1.x > s2.x Then Return 1
            Return 0
        End Function
    End Class
End Namespace
