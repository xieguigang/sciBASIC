Imports System.Numerics

Namespace Drawing3D.Math3D.MarchingCubes

    Public Class MarchingCubes
        Private Shared Function VertexIndexToString(index As Integer) As String
            Return String.Format("{0}:{1}:{2}", index And 1, index >> 1 And 1, index >> 2 And 1)
        End Function

        Private Class MarchingCubesCase
            Private Structure Vertex
                Implements IEquatable(Of Vertex)
                Public ReadOnly A As Integer
                Public ReadOnly B As Integer

                Public Sub New(a As Integer, b As Integer)
                    Me.A = a
                    Me.B = b
                End Sub

                Public Overrides Function ToString() As String
                    Return String.Format("{{{0}, {1}}}", VertexIndexToString(A), VertexIndexToString(B))
                End Function

                Public Overrides Function Equals(obj As Object) As Boolean
                    If ReferenceEquals(Nothing, obj) Then Return False
                    Return TypeOf obj Is Vertex AndAlso Equals(CType(obj, Vertex))
                End Function

                Public Overloads Function Equals(other As Vertex) As Boolean Implements IEquatable(Of Vertex).Equals
                    Return A = other.A AndAlso B = other.B
                End Function

                Public Overrides Function GetHashCode() As Integer
                    Return A * 397 Xor B
                End Function
            End Structure

            Private Structure Edge
                Implements IEquatable(Of Edge)
                Public ReadOnly A As Vertex
                Public ReadOnly B As Vertex

                Public ReadOnly Property IsValid As Boolean
                    Get
                        Return Not A.Equals(B)
                    End Get
                End Property
                Public ReadOnly Property Reverse As Edge
                    Get
                        Return New Edge(B, A)
                    End Get
                End Property

                Public Sub New(a As Vertex, b As Vertex)
                    Me.A = a
                    Me.B = b
                End Sub

                Public Overrides Function ToString() As String
                    Return String.Format("({0}, {1})", A, B)
                End Function

                Public Overloads Function Equals(other As Edge) As Boolean Implements IEquatable(Of Edge).Equals
                    Return A.Equals(other.A) AndAlso B.Equals(other.B)
                End Function

                Public Overrides Function Equals(obj As Object) As Boolean
                    If ReferenceEquals(Nothing, obj) Then Return False
                    Return TypeOf obj Is Edge AndAlso Equals(CType(obj, Edge))
                End Function

                Public Overrides Function GetHashCode() As Integer
                    Return (A.GetHashCode() * 397) Xor B.GetHashCode()
                End Function
            End Structure

            Private Structure Triangle
                Public ReadOnly A As Integer
                Public ReadOnly B As Integer
                Public ReadOnly C As Integer

                Public Sub New(a As Integer, b As Integer, c As Integer)
                    Me.A = a
                    Me.B = b
                    Me.C = c
                End Sub

                Public Overrides Function ToString() As String
                    Return String.Format("({0}, {1}, {2})", A, B, C)
                End Function
            End Structure

            Private Shared ReadOnly _sVectorLookup As Vector3() = {New Vector3(0F, 0F, 0F), New Vector3(1.0F, 0F, 0F), New Vector3(0F, 1.0F, 0F), New Vector3(1.0F, 1.0F, 0F), New Vector3(0F, 0F, 1.0F), New Vector3(1.0F, 0F, 1.0F), New Vector3(0F, 1.0F, 1.0F), New Vector3(1.0F, 1.0F, 1.0F)}

            Private ReadOnly _vertices As Vertex()
            Private ReadOnly _edges As Edge()
            Private ReadOnly _triangles As Triangle()

            Public Sub New(corners As Boolean())
                Dim verts = New List(Of Vertex)()
                Dim faces = New List(Of Vertex)(5) {}

                Dim i = 0

                While i < 6
                    faces(i) = New List(Of Vertex)()
                    Threading.Interlocked.Increment(i)
                End While
                Dim i = 0

                While i < 8
                    Dim x = i Xor &H1
                    Dim y = i Xor &H2
                    Dim z = i Xor &H4

                    Dim vert As Vertex

                    If corners(i) AndAlso Not corners(x) Then
                        verts.Add(CSharpImpl.__Assign(vert, New Vertex(i, x)))
                        faces(If((i And &H2) = 0, 2, 3)).Add(vert)
                        faces(If((i And &H4) = 0, 4, 5)).Add(vert)
                    End If
                    If corners(i) AndAlso Not corners(y) Then
                        verts.Add(CSharpImpl.__Assign(vert, New Vertex(i, y)))
                        faces(If((i And &H1) = 0, 0, 1)).Add(vert)
                        faces(If((i And &H4) = 0, 4, 5)).Add(vert)
                    End If
                    If corners(i) AndAlso Not corners(z) Then
                        verts.Add(CSharpImpl.__Assign(vert, New Vertex(i, z)))
                        faces(If((i And &H1) = 0, 0, 1)).Add(vert)
                        faces(If((i And &H2) = 0, 2, 3)).Add(vert)
                    End If

                    Threading.Interlocked.Increment(i)
                End While

                _vertices = verts.ToArray()

                Dim edges = New List(Of Edge)()
                Dim i = 0

                While i < 6
                    Dim face = faces(i)
                    If face.Count = 0 Then Continue While
                    If face.Count = 2 Then
                        edges.Add(New Edge(face(0), face(1)))
                        Continue While
                    End If

                    Dim j = 0

                    While j < face.Count - 1
                        Dim k = j + 1

                        While k < face.Count
                            Dim a = face(j)
                            Dim b = face(k)

                            Select Case a.A Xor b.A
                                Case &H0, &H1, &H2, &H4
                                    edges.Add(New Edge(a, b))
                            End Select

                            Threading.Interlocked.Increment(k)
                        End While

                        Threading.Interlocked.Increment(j)
                    End While

                    Threading.Interlocked.Increment(i)
                End While

                Dim sortedEdges = New List(Of Edge)()
                Dim triangles = New List(Of Triangle)()

                While edges.Count > 0
                    Dim last = edges(0)
                    edges.RemoveAt(0)

                    If ShouldFlip(last) Then last = last.Reverse

                    sortedEdges.Add(last)

                    Dim a = Array.IndexOf(_vertices, last.A)
                    Dim b = Array.IndexOf(_vertices, last.B)

                    While edges.Count > 0
                        Dim [next] = edges.FirstOrDefault(Function(x) x.A.Equals(last.B) OrElse x.B.Equals(last.B))
                        If Not [next].IsValid Then Exit While

                        edges.Remove([next])

                        last = If([next].A.Equals(last.B), [next], [next].Reverse)
                        sortedEdges.Add(last)

                        Dim c = Array.IndexOf(_vertices, last.B)

                        triangles.Add(New Triangle(a, b, c))

                        b = c
                    End While
                End While

                _edges = sortedEdges.ToArray()
                _triangles = triangles.ToArray()
            End Sub

            Private Shared Function ShouldFlip(edge As Edge) As Boolean
                Dim aa = _sVectorLookup(edge.A.A)
                Dim ab = _sVectorLookup(edge.A.B)
                Dim ba = _sVectorLookup(edge.B.A)
                Dim bb = _sVectorLookup(edge.B.B)

                Dim a = (aa + ab) * 0.5F
                Dim b = (ba + bb) * 0.5F

                Dim solidMidPoint = (aa + ba) * 0.5F
                Dim cross = Vector3.Cross(a - solidMidPoint, b - a)

                Return Vector3.Dot(cross, (a + b) * 0.5F - New Vector3(0.5F, 0.5F, 0.5F)) >= 0F
            End Function

            <ThreadStatic>
            Private Shared _sVertexIndices As Integer()

            Private Shared Function GetVertexIndexBuffer() As Integer()
                Return If(_sVertexIndices, Function()
                                               _sVertexIndices = New Integer(15) {}
                                               Return _sVertexIndices
                                           End Function())
            End Function

            Private Function FindVertex(values As Single(), threshold As Single, vertex As Vertex) As Vector3
                Dim a = values(vertex.A)
                Dim b = values(vertex.B)
                Dim t = (threshold - a) / (b - a)

                Dim src = _sVectorLookup(vertex.A)
                Dim dst = _sVectorLookup(vertex.B)

                Select Case vertex.A Xor vertex.B
                    Case &H1
                        src.X += (dst.X - src.X) * t
                    Case &H2
                        src.Y += (dst.Y - src.Y) * t
                    Case &H4
                        src.Z += (dst.Z - src.Z) * t
                End Select

                Return src
            End Function

            Public Sub Write(cubes As MarchingCubes, values As Single(), threshold As Single)
                Dim vertIndices = GetVertexIndexBuffer()
                Dim i = 0

                While i < _vertices.Length
                    vertIndices(i) = cubes.WriteVertex(FindVertex(values, threshold, _vertices(i)))
                    Threading.Interlocked.Increment(i)
                End While

                Dim i = 0

                While i < _triangles.Length
                    Dim face = _triangles(i)
                    cubes.WriteFace(vertIndices(face.A), vertIndices(face.B), vertIndices(face.C))
                    Threading.Interlocked.Increment(i)
                End While
            End Sub

            Public Sub DrawGizmos(values As Single(), threshold As Single)
                Dim i = 0

                While i < _edges.Length
                    Dim edge = _edges(i)
                    Dim a = FindVertex(values, threshold, edge.A)
                    Dim b = FindVertex(values, threshold, edge.B)

                    Gizmos.DrawLine(a, b)
                    Threading.Interlocked.Increment(i)
                End While
            End Sub

            Private Class CSharpImpl
                <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
                Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                    target = value
                    Return value
                End Function
            End Class
        End Class

        Private ReadOnly _lookupTable As MarchingCubesCase() = New MarchingCubesCase(255) {}

        Public Property CubeSize As Vector3
        Public Property CubePos As Vector3
        Public Property Threshold As Single

        Private ReadOnly _indexMap As Dictionary(Of Vector3, Integer) = New Dictionary(Of Vector3, Integer)()
        Private ReadOnly _indices As List(Of Integer) = New List(Of Integer)()
        Private ReadOnly _vertices As List(Of Vector3) = New List(Of Vector3)()

        Public Sub New()
            PopulateLookupTable()

            CubeSize = Vector3.One
            Threshold = 0.5F
        End Sub

        Private Sub PopulateLookupTable()
            Dim i = 0

            While i < 256
                _lookupTable(i) = New MarchingCubesCase({(i And &H1) <> 0, (i And &H2) <> 0, (i And &H4) <> 0, (i And &H8) <> 0, (i And &H10) <> 0, (i And &H20) <> 0, (i And &H40) <> 0, (i And &H80) <> 0})
                Threading.Interlocked.Increment(i)
            End While
        End Sub

        Public Sub Clear()
            _indexMap.Clear()
            _indices.Clear()
            _vertices.Clear()
        End Sub

        Public Sub MoveToCube(x As Integer, y As Integer, z As Integer)
            CubePos = New Vector3(CubeSize.X * x, CubeSize.Y * y, CubeSize.Z * z)
        End Sub

        Private Function LookupCase(values As Single()) As MarchingCubesCase
            If values.Length <> 8 Then Throw New Exception("Expected 8 values.")

            Dim thresh = Threshold
            Dim lookup = 0

            Dim i = 0

            While i < 8
                lookup = lookup Or If(values(i) >= thresh, 1 << i, 0)
                Threading.Interlocked.Increment(i)
            End While

            Return _lookupTable(lookup)
        End Function

        Public Sub Write(values As Single())
            LookupCase(values).Write(Me, values, Threshold)
        End Sub

        Private Sub WriteFace(a As Integer, b As Integer, c As Integer)
            _indices.Add(a)
            _indices.Add(b)
            _indices.Add(c)
        End Sub

        Private Function WriteVertex(vertex As Vector3) As Integer
            Const res = 4
            Const invRes = 1.0F / res

            vertex.X = CInt(vertex.X * res) * invRes
            vertex.Y = CInt(vertex.Y * res) * invRes
            vertex.Z = CInt(vertex.Z * res) * invRes

            vertex = vertex * CubeSize + CubePos

            Dim index As Integer
            If _indexMap.TryGetValue(vertex, index) Then
                Return index
            End If

            index = _vertices.Count
            _vertices.Add(vertex)
            _indexMap.Add(vertex, index)

            Return index
        End Function

        Public Sub CopyToMesh(mesh As Mesh)
            mesh.Clear()
            mesh.SetVertices(_vertices)
            mesh.SetTriangles(_indices, 0)
            mesh.RecalculateBounds()
            mesh.RecalculateNormals()
            mesh.UploadMeshData(False)
        End Sub

        Public Sub DrawGizmos(values As Single())
            LookupCase(values).DrawGizmos(values, Threshold)
        End Sub
    End Class
End Namespace