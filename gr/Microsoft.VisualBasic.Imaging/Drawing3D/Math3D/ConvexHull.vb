Imports System.Numerics
Imports std = System.Math

Namespace Drawing3D.Math3D

    ''' <summary>
    ''' The convex hull of a point cloud: the vertices that survived and the
    ''' triangle index list around them.
    ''' </summary>
    Public Class ConvexHullResult
        Public Property Vertices As Vector3() = New Vector3() {}
        ''' <summary>the triangles, counter clockwise seen from the outside</summary>
        Public Property Faces As Integer()() = New Integer()() {}
    End Class

    ''' <summary>
    ''' A quick hull of a three dimensional point cloud.
    ''' </summary>
    ''' <remarks>
    ''' The physics engine only collides convex shapes, and the MuJoCo compiler
    ''' treats a collision mesh the very same way (it takes the convex hull of the
    ''' triangle soup), so a hull of the vertices of an STL mesh is the closest
    ''' proxy of the original collision geometry that this engine can express.
    '''
    ''' The implementation is the classic quick hull: an initial tetrahedron is
    ''' expanded by the farthest outside point of a face, the visible faces are
    ''' dropped and the horizon of the visible set is stitched to the new point.
    ''' Every face keeps its own conflict list, so one pass over the points of a
    ''' removed face is enough to place it again.
    '''
    ''' A triangle soup of a decimated stl holds tens of thousands of corners,
    ''' nearly all of them on the same face of the hull. The cloud is therefore
    ''' snapped to a grid before the hull is computed: the proxy of a 5 cm foot
    ''' keeps a resolution of about a millimetre, which is far below the contact
    ''' tolerance of the solver.
    ''' </remarks>
    Public Module ConvexHull

        Private Const DefaultEpsilon As Double = 0.000001
        Private Const MaxIterations As Integer = 6000
        ''' <summary>the most corners that the hull stage keeps of one mesh</summary>
        Private Const MaxCorners As Integer = 400

        ''' <summary>
        ''' compute the hull of a point cloud, the duplicates are removed first
        ''' </summary>
        ''' <exception cref="InvalidOperationException">the point cloud is flat or has less than four corners</exception>
        Public Function Compute(points As Vector3(), Optional epsilon As Double = DefaultEpsilon) As ConvexHullResult
            Dim cloud As Vector3() = Reduce(points)

            If cloud.Length < 4 Then
                Throw New InvalidOperationException($"a convex hull needs at least four distinct corners, got {cloud.Length}")
            End If

            Dim simplex As Integer() = InitialSimplex(cloud)

            If simplex Is Nothing Then
                Throw New InvalidOperationException("the point cloud is degenerate, it has no volume")
            End If

            Dim faces As New List(Of HullFace)()
            Dim i0 As Integer = simplex(0)
            Dim i1 As Integer = simplex(1)
            Dim i2 As Integer = simplex(2)
            Dim i3 As Integer = simplex(3)

            Call CreateFace(cloud, faces, i0, i1, i2, i3)
            Call CreateFace(cloud, faces, i0, i1, i3, i2)
            Call CreateFace(cloud, faces, i0, i2, i3, i1)
            Call CreateFace(cloud, faces, i1, i2, i3, i0)

            If faces.Count < 4 Then
                Throw New InvalidOperationException("the point cloud is degenerate, the initial tetrahedron is flat")
            End If

            For i As Integer = 0 To cloud.Length - 1
                If i = i0 OrElse i = i1 OrElse i = i2 OrElse i = i3 Then
                    Continue For
                End If

                For Each face As HullFace In faces
                    If face.Distance(cloud(i)) > epsilon Then
                        Call face.Outside.Add(i)
                        Call face.UpdateFarthest(cloud)
                    End If
                Next
            Next

            Dim pending As New Stack(Of HullFace)()

            For Each face As HullFace In faces
                If face.Outside.Count > 0 Then
                    Call pending.Push(face)
                End If
            Next

            Dim iterations As Integer = 0

            Do While pending.Count > 0
                Dim target As HullFace = pending.Pop()

                If target.Removed Then
                    Continue Do
                End If

                Call target.Prune(cloud, epsilon)

                If target.Outside.Count = 0 Then
                    Continue Do
                End If

                iterations += 1

                If iterations > MaxIterations Then
                    Throw New InvalidOperationException($"the quick hull did not converge after {iterations} expansions")
                End If

                Dim apex As Integer = target.FarthestPoint
                Dim apexPoint As Vector3 = cloud(apex)
                Dim visible As New List(Of HullFace)()

                For Each face As HullFace In faces
                    If face.Removed Then
                        Continue For
                    End If

                    If face.Distance(apexPoint) > epsilon Then
                        Call visible.Add(face)
                    End If
                Next

                If visible.Count = 0 Then
                    Continue Do
                End If

                ' the horizon: the edges that are seen by exactly one visible
                ' face. An edge that two visible faces share cancels itself out.
                Dim edges As New Dictionary(Of Long, Integer())()

                For Each face As HullFace In visible
                    Call AddEdge(edges, face.A, face.B)
                    Call AddEdge(edges, face.B, face.C)
                    Call AddEdge(edges, face.C, face.A)
                Next

                If edges.Count = 0 Then
                    Throw New InvalidOperationException("the quick hull lost its horizon, the face topology is not closed")
                End If

                Dim orphan As New List(Of Integer)()

                For Each face As HullFace In visible
                    face.Removed = True

                    For Each index As Integer In face.Outside
                        Call orphan.Add(index)
                    Next

                    face.Outside.Clear()
                Next

                Dim survivors As New List(Of HullFace)(faces.Count)

                For Each face As HullFace In faces
                    If Not face.Removed Then
                        Call survivors.Add(face)
                    End If
                Next

                faces = survivors

                For Each edge As KeyValuePair(Of Long, Integer()) In edges
                    Dim created As HullFace = CreateFace(cloud, faces, edge.Value(0), edge.Value(1), apex, -1)

                    If created Is Nothing Then
                        ' a flat ear of the horizon: the edge is already covered
                        ' by a face that contains the apex
                        Continue For
                    End If
                Next

                Call AssignOrphans(cloud, faces, orphan, epsilon)

                For Each face As HullFace In faces
                    If face.Outside.Count > 0 AndAlso Not face.Queued Then
                        face.Queued = True
                        Call pending.Push(face)
                    End If
                Next
            Loop

            Return Build(cloud, faces)
        End Function

        ' /********************************************************************************/
        '   helpers
        ' /********************************************************************************/

        Private Class HullFace
            Public A As Integer
            Public B As Integer
            Public C As Integer
            Public Normal As Vector3
            Public Offset As Double
            Public Removed As Boolean
            Public Queued As Boolean
            Public FarthestPoint As Integer = -1
            Public FarthestDistance As Double = 0
            Public ReadOnly Outside As New List(Of Integer)()

            Public Function Distance(point As Vector3) As Double
                Return Vector3.Dot(Normal, point) - Offset
            End Function

            ''' <summary>
            ''' drop the points of the conflict list that the expansion of another
            ''' face has swallowed
            ''' </summary>
            Public Sub Prune(cloud As Vector3(), epsilon As Double)
                FarthestDistance = -1
                FarthestPoint = -1

                Dim kept As New List(Of Integer)(Outside.Count)

                For Each index As Integer In Outside
                    Dim separation As Double = Distance(cloud(index))

                    If separation > epsilon Then
                        Call kept.Add(index)

                        If separation > FarthestDistance Then
                            FarthestDistance = separation
                            FarthestPoint = index
                        End If
                    End If
                Next

                Outside.Clear()
                Call Outside.AddRange(kept)
            End Sub

            Public Sub UpdateFarthest(cloud As Vector3())
                Dim index As Integer = Outside(Outside.Count - 1)
                Dim separation As Double = Distance(cloud(index))

                If separation > FarthestDistance OrElse FarthestPoint < 0 Then
                    FarthestDistance = separation
                    FarthestPoint = index
                End If
            End Sub
        End Class

        ''' <summary>
        ''' create a triangle face, the winding is corrected so that the normal
        ''' points away from <paramref name="inside"/> when it is given
        ''' </summary>
        Private Function CreateFace(cloud As Vector3(), faces As List(Of HullFace),
                                    a As Integer, b As Integer, c As Integer, inside As Integer) As HullFace
            Dim cross As Vector3 = Vector3.Cross(cloud(b) - cloud(a), cloud(c) - cloud(a))
            Dim length As Double = cross.Length()

            If length <= 0.000000000001 Then
                Return Nothing
            End If

            Dim normal As Vector3 = cross / CSng(length)

            If inside >= 0 Then
                If Vector3.Dot(normal, cloud(inside) - cloud(a)) > 0 Then
                    Dim swap As Integer = b
                    b = c
                    c = swap
                    normal = -normal
                End If
            End If

            Dim face As New HullFace With {
                .A = a,
                .B = b,
                .C = c,
                .Normal = normal,
                .Offset = Vector3.Dot(normal, cloud(a))
            }

            Call faces.Add(face)

            Return face
        End Function

        Private Sub AddEdge(edges As Dictionary(Of Long, Integer()), a As Integer, b As Integer)
            Dim key As Long = (CLng(a) << 32) Or CLng(b)

            If edges.ContainsKey(key) Then
                Call edges.Remove(key)
            Else
                edges((CLng(b) << 32) Or CLng(a)) = New Integer() {a, b}
            End If
        End Sub

        Private Sub AssignOrphans(cloud As Vector3(), faces As List(Of HullFace),
                                  orphan As List(Of Integer), epsilon As Double)
            For Each index As Integer In orphan
                Dim point As Vector3 = cloud(index)
                Dim best As HullFace = Nothing
                Dim bestDistance As Double = epsilon

                For Each face As HullFace In faces
                    Dim separation As Double = face.Distance(point)

                    If separation > bestDistance Then
                        bestDistance = separation
                        best = face
                    End If
                Next

                If best IsNot Nothing Then
                    Call best.Outside.Add(index)

                    If bestDistance > best.FarthestDistance OrElse best.FarthestPoint < 0 Then
                        best.FarthestDistance = bestDistance
                        best.FarthestPoint = index
                    End If
                End If
            Next
        End Sub

        Private Function Build(cloud As Vector3(), faces As List(Of HullFace)) As ConvexHullResult
            Dim map As New Dictionary(Of Integer, Integer)()
            Dim vertices As New List(Of Vector3)()
            Dim indices As New List(Of Integer)()

            For Each face As HullFace In faces
                If face.Removed Then
                    Continue For
                End If

                For Each source As Integer In {face.A, face.B, face.C}
                    Dim mapped As Integer = 0

                    If Not map.TryGetValue(source, mapped) Then
                        mapped = vertices.Count
                        map(source) = mapped
                        Call vertices.Add(cloud(source))
                    End If

                    Call indices.Add(mapped)
                Next
            Next

            Dim vertexList As Vector3() = vertices.ToArray()
            Dim faceList(indices.Count \ 3 - 1)() As Integer

            ' the centroid stands inside the hull, so a face that points towards
            ' it is wound the wrong way: the collision solver of the engine reads
            ' the winding as the outward normal
            Dim center As Vector3 = Vector3.Zero

            For Each vertex As Vector3 In vertexList
                center += vertex
            Next

            center /= vertexList.Length

            For i As Integer = 0 To faceList.Length - 1
                Dim a As Integer = indices(i * 3)
                Dim b As Integer = indices(i * 3 + 1)
                Dim c As Integer = indices(i * 3 + 2)
                Dim normal As Vector3 = Vector3.Cross(vertexList(b) - vertexList(a), vertexList(c) - vertexList(a))

                If Vector3.Dot(normal, vertexList(a) - center) < 0 Then
                    Dim swap As Integer = b
                    b = c
                    c = swap
                End If

                faceList(i) = New Integer() {a, b, c}
            Next

            Return New ConvexHullResult With {
                .Vertices = vertexList,
                .Faces = faceList
            }
        End Function

        ''' <summary>
        ''' snap the cloud to a grid so that the hull stage keeps a workable
        ''' number of corners: the resolution starts at one 48th of the longest
        ''' axis and is coarsened until the cloud fits
        ''' </summary>
        Private Function Reduce(points As Vector3()) As Vector3()
            If points Is Nothing OrElse points.Length = 0 Then
                Return New Vector3() {}
            End If

            Dim min As Vector3 = points(0)
            Dim max As Vector3 = points(0)

            For Each point As Vector3 In points
                min = Vector3.Min(min, point)
                max = Vector3.Max(max, point)
            Next

            Dim extent As Single = std.Max(max.X - min.X, std.Max(max.Y - min.Y, max.Z - min.Z))

            If extent <= 0 Then
                Return New Vector3() {}
            End If

            Dim cell As Single = extent / 32.0F
            Dim cloud As Vector3() = Quantize(points, cell)

            For pass As Integer = 1 To 10
                If cloud.Length <= MaxCorners Then
                    Exit For
                End If

                cell *= 1.3F
                cloud = Quantize(points, cell)
            Next

            Return cloud
        End Function

        Private Function Quantize(points As Vector3(), cell As Single) As Vector3()
            Dim cells As New Dictionary(Of (Integer, Integer, Integer), Vector3)()

            For Each point As Vector3 In points
                If Single.IsNaN(point.X) OrElse Single.IsNaN(point.Y) OrElse Single.IsNaN(point.Z) Then
                    Continue For
                End If

                Dim key As (Integer, Integer, Integer) = (
                    CInt(std.Floor(point.X / cell)),
                    CInt(std.Floor(point.Y / cell)),
                    CInt(std.Floor(point.Z / cell)))

                If Not cells.ContainsKey(key) Then
                    cells(key) = point
                End If
            Next

            Dim result As New List(Of Vector3)(cells.Count)

            For Each pair As KeyValuePair(Of (Integer, Integer, Integer), Vector3) In cells
                Call result.Add(pair.Value)
            Next

            Return result.ToArray()
        End Function

        ''' <summary>
        ''' the four corners of the initial tetrahedron: two extreme points, the
        ''' farthest point from that line and the farthest point from that plane
        ''' </summary>
        Private Function InitialSimplex(cloud As Vector3()) As Integer()
            Dim a As Integer = 0
            Dim b As Integer = 0
            Dim best As Double = -1

            ' the farthest pair of the axis extremes is a much better seed than
            ' the first two points of an stl file
            Dim extremes As New List(Of Integer)()

            For Each axis As Integer In {0, 1, 2}
                Dim min As Integer = 0
                Dim max As Integer = 0

                For i As Integer = 1 To cloud.Length - 1
                    If Component(cloud(i), axis) < Component(cloud(min), axis) Then min = i
                    If Component(cloud(i), axis) > Component(cloud(max), axis) Then max = i
                Next

                Call extremes.Add(min)
                Call extremes.Add(max)
            Next

            For Each first As Integer In extremes
                For Each second As Integer In extremes
                    Dim separation As Double = Vector3.DistanceSquared(cloud(first), cloud(second))

                    If separation > best Then
                        best = separation
                        a = first
                        b = second
                    End If
                Next
            Next

            If best <= 1.0E-18 Then
                Return Nothing
            End If

            Dim ab As Vector3 = cloud(b) - cloud(a)
            Dim c As Integer = -1
            Dim area As Double = 0.000000000001

            For i As Integer = 0 To cloud.Length - 1
                Dim cross As Vector3 = Vector3.Cross(ab, cloud(i) - cloud(a))
                Dim length As Double = cross.Length()

                If length > area Then
                    area = length
                    c = i
                End If
            Next

            If c < 0 Then
                Return Nothing
            End If

            Dim normal As Vector3 = Vector3.Normalize(Vector3.Cross(ab, cloud(c) - cloud(a)))
            Dim d As Integer = -1
            Dim height As Double = 0.000000001

            For i As Integer = 0 To cloud.Length - 1
                Dim separation As Double = std.Abs(Vector3.Dot(normal, cloud(i) - cloud(a)))

                If separation > height Then
                    height = separation
                    d = i
                End If
            Next

            If d < 0 Then
                Return Nothing
            End If

            Return New Integer() {a, b, c, d}
        End Function

        Private Function Component(point As Vector3, axis As Integer) As Single
            Select Case axis
                Case 0
                    Return point.X
                Case 1
                    Return point.Y
                Case Else
                    Return point.Z
            End Select
        End Function
    End Module
End Namespace
