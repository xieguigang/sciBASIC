Imports System.Collections.Generic

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Site
        Implements ICoord

        Private Shared pool As Queue(Of Site) = New Queue(Of Site)()

        Public Shared Function Create(p As Vector2, index As Integer, weigth As Single) As Site
            If pool.Count > 0 Then
                Return pool.Dequeue().Init(p, index, weigth)
            Else
                Return New Site(p, index, weigth)
            End If
        End Function

        Public Shared Sub SortSites(sites As List(Of Site))
            sites.Sort(Function(s0, s1)
                           Dim returnValue = Voronoi.CompareByYThenX(s0, s1)

                           Dim tempIndex As Integer

                           If returnValue = -1 Then
                               If s0.siteIndexField > s1.SiteIndex Then
                                   tempIndex = s0.SiteIndex
                                   s0.SiteIndex = s1.SiteIndex
                                   s1.SiteIndex = tempIndex
                               End If
                           ElseIf returnValue = 1 Then
                               If s1.SiteIndex > s0.SiteIndex Then
                                   tempIndex = s1.SiteIndex
                                   s1.SiteIndex = s0.SiteIndex
                                   s0.SiteIndex = tempIndex
                               End If
                           End If

                           Return returnValue
                       End Function)
        End Sub

        Public Function Compare(s1 As Site, s2 As Site) As Integer
            Return s1.CompareTo(s2)
        End Function

        Public Function CompareTo(s1 As Site) As Integer
            Dim returnValue = Voronoi.CompareByYThenX(Me, s1)

            Dim tempIndex As Integer

            If returnValue = -1 Then
                If siteIndexField > s1.SiteIndex Then
                    tempIndex = SiteIndex
                    SiteIndex = s1.SiteIndex
                    s1.SiteIndex = tempIndex
                End If
            ElseIf returnValue = 1 Then
                If s1.SiteIndex > SiteIndex Then
                    tempIndex = s1.SiteIndex
                    s1.SiteIndex = SiteIndex
                    SiteIndex = tempIndex
                End If
            End If

            Return returnValue
        End Function

        Private Const EPSILON As Single = 0.005F
        Private Shared Function CloseEnough(p0 As Vector2, p1 As Vector2) As Boolean
            Return (p0 - p1).Length < EPSILON
        End Function

        Private siteIndexField As Integer
        Public Property SiteIndex As Integer
            Get
                Return siteIndexField
            End Get
            Set(value As Integer)
                siteIndexField = value
            End Set
        End Property

        Private coordField As Vector2
        Public Property Coord As Vector2 Implements ICoord.Coord
            Get
                Return coordField
            End Get
            Set(value As Vector2)
                coordField = value
            End Set
        End Property

        Public ReadOnly Property x As Single
            Get
                Return coordField.X
            End Get
        End Property
        Public ReadOnly Property y As Single
            Get
                Return coordField.Y
            End Get
        End Property

        Private weigthField As Single
        Public ReadOnly Property Weigth As Single
            Get
                Return weigthField
            End Get
        End Property

        ' The edges that define this Site's Voronoi region:
        Private edgesField As List(Of Edge)
        Public ReadOnly Property Edges As List(Of Edge)
            Get
                Return edgesField
            End Get
        End Property
        ' which end of each edge hooks up with the previous edge in edges:
        Private edgeOrientations As List(Of LR)
        ' ordered list of points that define the region clipped to bounds:
        Private regionField As List(Of Vector2)

        Public Sub New(p As Vector2, index As Integer, weigth As Single)
            Init(p, index, weigth)
        End Sub

        Private Function Init(p As Vector2, index As Integer, weigth As Single) As Site
            coordField = p
            siteIndexField = index
            weigthField = weigth
            edgesField = New List(Of Edge)()
            regionField = Nothing

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "Site " & siteIndexField.ToString() & ": " & coordField.ToString()
        End Function

        Private Sub Move(p As Vector2)
            Clear()
            coordField = p
        End Sub

        Public Sub Dispose()
            Clear()
            pool.Enqueue(Me)
        End Sub

        Private Sub Clear()
            If edgesField IsNot Nothing Then
                edgesField.Clear()
                edgesField = Nothing
            End If
            If edgeOrientations IsNot Nothing Then
                edgeOrientations.Clear()
                edgeOrientations = Nothing
            End If
            If regionField IsNot Nothing Then
                regionField.Clear()
                regionField = Nothing
            End If
        End Sub

        Public Sub AddEdge(edge As Edge)
            edgesField.Add(edge)
        End Sub

        Public Function NearestEdge() As Edge
            edgesField.Sort(New Comparison(Of Edge)(AddressOf Edge.CompareSitesDistances))
            Return edgesField(0)
        End Function

        Public Function NeighborSites() As List(Of Site)
            If edgesField Is Nothing OrElse edgesField.Count = 0 Then
                Return New List(Of Site)()
            End If
            If edgeOrientations Is Nothing Then
                ReorderEdges()
            End If
            Dim list As List(Of Site) = New List(Of Site)()
            For Each edge In edgesField
                list.Add(NeighborSite(edge))
            Next
            Return list
        End Function

        Private Function NeighborSite(edge As Edge) As Site
            If Me Is edge.LeftSite Then
                Return edge.RightSite
            End If
            If Me Is edge.RightSite Then
                Return edge.LeftSite
            End If
            Return Nothing
        End Function

        Public Function Region(clippingBounds As Rectf) As List(Of Vector2)
            If edgesField Is Nothing OrElse edgesField.Count = 0 Then
                Return New List(Of Vector2)()
            End If
            If edgeOrientations Is Nothing Then
                ReorderEdges()
            End If
            If regionField Is Nothing Then
                regionField = ClipToBounds(clippingBounds)
                If (New Polygon(regionField)).PolyWinding() = Winding.CLOCKWISE Then
                    regionField.Reverse()
                End If
            End If
            Return regionField
        End Function

        Private Sub ReorderEdges()
            Dim reorderer As EdgeReorderer = New EdgeReorderer(edgesField, GetType(Vertex))
            edgesField = reorderer.Edges
            edgeOrientations = reorderer.EdgeOrientations
            reorderer.Dispose()
        End Sub

        Private Function ClipToBounds(bounds As Rectf) As List(Of Vector2)
            Dim points As List(Of Vector2) = New List(Of Vector2)()
            Dim n = edgesField.Count
            Dim i = 0
            Dim edge As Edge

            While i < n AndAlso Not edgesField(i).Visible()
                i += 1
            End While

            If i = n Then
                ' No edges visible
                Return New List(Of Vector2)()
            End If
            edge = edgesField(i)
            Dim orientation = edgeOrientations(i)
            points.Add(edge.ClippedEnds(orientation))
            points.Add(edge.ClippedEnds(LR.Other(orientation)))

            For j = i + 1 To n - 1
                edge = edgesField(j)
                If Not edge.Visible() Then
                    Continue For
                End If
                Connect(points, j, bounds)
            Next
            ' Close up the polygon by adding another corner point of the bounds if needed:
            Connect(points, i, bounds, True)

            Return points
        End Function

        Private Sub Connect(ByRef points As List(Of Vector2), j As Integer, bounds As Rectf, Optional closingUp As Boolean = False)
            Dim rightPoint = points(points.Count - 1)
            Dim newEdge = edgesField(j)
            Dim newOrientation = edgeOrientations(j)

            ' The point that must be conected to rightPoint:
            Dim newPoint = newEdge.ClippedEnds(newOrientation)

            If Not CloseEnough(rightPoint, newPoint) Then
                ' The points do not coincide, so they must have been clipped at the bounds;
                ' see if they are on the same border of the bounds:
                If rightPoint.X <> newPoint.X AndAlso rightPoint.Y <> newPoint.Y Then
                    ' They are on different borders of the bounds;
                    ' insert one or two corners of bounds as needed to hook them up:
                    ' (NOTE this will not be correct if the region should take up more than
                    ' half of the bounds rect, for then we will have gone the wrong way
                    ' around the bounds and included the smaller part rather than the larger)
                    Dim rightCheck = BoundsCheck.Check(rightPoint, bounds)
                    Dim newCheck = BoundsCheck.Check(newPoint, bounds)
                    Dim px, py As Single
                    If (rightCheck And BoundsCheck.RIGHT) <> 0 Then
                        px = bounds.right

                        If (newCheck And BoundsCheck.BOTTOM) <> 0 Then
                            py = bounds.bottom
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.TOP) <> 0 Then
                            py = bounds.top
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.LEFT) <> 0 Then
                            If rightPoint.Y - bounds.y + newPoint.Y - bounds.y < bounds.height Then
                                py = bounds.top
                            Else
                                py = bounds.bottom
                            End If
                            points.Add(New Vector2(px, py))
                            points.Add(New Vector2(bounds.left, py))
                        End If
                    ElseIf (rightCheck And BoundsCheck.LEFT) <> 0 Then
                        px = bounds.left

                        If (newCheck And BoundsCheck.BOTTOM) <> 0 Then
                            py = bounds.bottom
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.TOP) <> 0 Then
                            py = bounds.top
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.RIGHT) <> 0 Then
                            If rightPoint.Y - bounds.y + newPoint.Y - bounds.y < bounds.height Then
                                py = bounds.top
                            Else
                                py = bounds.bottom
                            End If
                            points.Add(New Vector2(px, py))
                            points.Add(New Vector2(bounds.right, py))
                        End If
                    ElseIf (rightCheck And BoundsCheck.TOP) <> 0 Then
                        py = bounds.top

                        If (newCheck And BoundsCheck.RIGHT) <> 0 Then
                            px = bounds.right
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.LEFT) <> 0 Then
                            px = bounds.left
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.BOTTOM) <> 0 Then
                            If rightPoint.X - bounds.x + newPoint.X - bounds.x < bounds.width Then
                                px = bounds.left
                            Else
                                px = bounds.right
                            End If
                            points.Add(New Vector2(px, py))
                            points.Add(New Vector2(px, bounds.bottom))
                        End If
                    ElseIf (rightCheck And BoundsCheck.BOTTOM) <> 0 Then
                        py = bounds.bottom

                        If (newCheck And BoundsCheck.RIGHT) <> 0 Then
                            px = bounds.right
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.LEFT) <> 0 Then
                            px = bounds.left
                            points.Add(New Vector2(px, py))

                        ElseIf (newCheck And BoundsCheck.TOP) <> 0 Then
                            If rightPoint.X - bounds.x + newPoint.X - bounds.x < bounds.width Then
                                px = bounds.left
                            Else
                                px = bounds.right
                            End If
                            points.Add(New Vector2(px, py))
                            points.Add(New Vector2(px, bounds.top))
                        End If
                    End If
                End If
                If closingUp Then
                    ' newEdge's ends have already been added
                    Return
                End If
                points.Add(newPoint)
            End If
            Dim newRightPoint = newEdge.ClippedEnds(LR.Other(newOrientation))
            If Not CloseEnough(points(0), newRightPoint) Then
                points.Add(newRightPoint)
            End If
        End Sub

        Public Function Dist(p As ICoord) As Single
            Return (Coord - p.Coord).Length
        End Function
    End Class

    Public Class BoundsCheck
        Public Const TOP As Integer = 1
        Public Const BOTTOM As Integer = 2
        Public Const LEFT As Integer = 4
        Public Const RIGHT As Integer = 8

        ' 
        ' * 
        ' * @param point
        ' * @param bounds
        ' * @return an int with the appropriate bits set if the Point lies on the corresponding bounds lines

        Public Shared Function Check(point As Vector2, bounds As Rectf) As Integer
            Dim value = 0
            If point.X = bounds.left Then
                value = value Or LEFT
            End If
            If point.X = bounds.right Then
                value = value Or RIGHT
            End If
            If point.Y = bounds.top Then
                value = value Or TOP
            End If
            If point.Y = bounds.bottom Then
                value = value Or BOTTOM
            End If

            Return value
        End Function
    End Class
End Namespace
