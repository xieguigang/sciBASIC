Imports Vector3 = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing3D.Math3D.MarchingCubes

    Public Class DistanceFieldSampler
#Region "Unity Junk"
        Public Path As Path
        Public Result As Texture2D
        Public Resolution As Integer = 128

        Private _lastSampleSize As Vector2
        Private _lastSampledPosition As Vector3

        Private ReadOnly Property SampleSize As Vector2
            Get
                Dim scale = transform.lossyScale
                Return New Vector2(scale.x, scale.y)
            End Get
        End Property

        Private ReadOnly Property NeedsUpdate As Boolean
            Get
                If Path Is Nothing Then Return False
                Return Result Is Nothing OrElse Result.width <> Resolution OrElse _lastSampledPosition <> transform.position OrElse Math.Abs(_lastSampleSize.X - SampleSize.X) > Single.Epsilon OrElse Math.Abs(_lastSampleSize.Y - SampleSize.Y) > Single.Epsilon
            End Get
        End Property
#End Region

        ''' <summary>
        ''' Represents a point along a path.
        ''' </summary>
        Public Structure Vertex
            ''' <summary>
            ''' Position in 3D space of the vertex.
            ''' </summary>
            Public ReadOnly Pos As Vector3

            ''' <summary>
            ''' Radius of the line segment starting at this vertex.
            ''' </summary>
            Public ReadOnly Radius As Single

            ''' <summary>
            ''' If true, this vertex has no edge preceding it.
            ''' </summary>
            Public ReadOnly StartPoint As Boolean

            Public Sub New(pos As Vector3, radius As Single, start As Boolean)
                Me.Pos = pos
                Me.Radius = radius
                StartPoint = start
            End Sub

            Public Sub New(node As PathNode)
                Pos = node.transform.position
                Radius = node.Radius
                StartPoint = node.StartPoint
            End Sub
        End Structure

        ''' <summary>
        ''' A pair of vertices that define a line segment.
        ''' </summary>
        Private Structure Edge
            ''' <summary>
            ''' Start vertex of the line segment.
            ''' </summary>
            Public ReadOnly First As Vertex

            ''' <summary>
            ''' End vertex of the line segment.
            ''' </summary>
            Public ReadOnly Second As Vertex

            Public Sub New(first As Vertex, second As Vertex)
                Me.First = first
                Me.Second = second
            End Sub
        End Structure

        <ThreadStatic>
        Private Shared _sNearbyEdges As List(Of Edge)

        ''' <summary>
        ''' Find a list of edgest from the given vertex list that will possibly intesect with the
        ''' given rectangle, defined by it's vertical position (<paramrefname="layerPos"/>), and
        ''' minimum and maximum X and Z bounds (<paramrefname="min"/> and <paramrefname="max"/>).
        ''' </summary>
        ''' <paramname="path">List of vertices to search through/</param>
        ''' <paramname="layerPos">Vertical position of the rectangle to find edges near to.</param>
        ''' <paramname="min">Minimum X and Z coordinates of the rectangle.</param>
        ''' <paramname="max">Maximum X and Z coordinates of the rectangle.</param>
        ''' <paramname="outEdges">List to append the results to.</param>
        Private Shared Sub FindNearbyEdges(path As List(Of Vertex), layerPos As Single, min As Vector2, max As Vector2, outEdges As List(Of Edge))
            Dim prev = path(path.Count - 1)
            For Each [Next] In path
                If Not [Next].StartPoint Then
                    Dim edge = New Edge(prev, [Next])
                    If IsEdgeNearSampleRange(edge, layerPos, min, max) Then
                        outEdges.Add(edge)
                    End If
                End If

                prev = [Next]()
            Next
        End Sub

        Private Shared Function IsEdgeNearSampleRange(ByRef edge As Edge, layerPos As Single, min As Vector2, max As Vector2) As Boolean
            ' Check if completely above
            If edge.First.Pos.Y - edge.First.Radius > layerPos AndAlso edge.Second.Pos.Y - edge.First.Radius > layerPos Then Return False

            ' Check if completely below
            If edge.First.Pos.Y + edge.First.Radius < layerPos AndAlso edge.Second.Pos.Y + edge.First.Radius < layerPos Then Return False

            ' TODO: Check if edge is outside of the square.
            Return True
        End Function

        Private Shared Function GetDistance(samplePos As Vector3, edge As Edge) As Single
            Dim ap = samplePos - edge.First.Pos
            Dim bp = samplePos - edge.Second.Pos
            Dim ab = edge.Second.Pos - edge.First.Pos

            If Vector3.Dot(ap, ab) < 0F Then Return ap.magnitude
            If Vector3.Dot(bp, ab) > 0F Then Return bp.magnitude

            Return Math.Sqrt(Vector3.Cross(CType(ap, Vector3), CType(bp, Vector3)).sqrMagnitude / ab.sqrMagnitude)
        End Function

        Private Shared Function DistanceToDistanceScore(distance As Single, radius As Single) As Single
            Return Math.Clamp(1.0F - distance * 0.5F / radius)
        End Function

        Private Shared Function GetDistanceScore(samplePos As Vector3, edges As List(Of Edge)) As Single
            Dim score = 0F

            For Each edge In edges
                score = Math.Max(score, DistanceToDistanceScore(GetDistance(samplePos, edge), edge.First.Radius))
            Next

            Return score
        End Function

        Public Shared Sub SampleDistanceField(path As List(Of Vertex), origin As Vector3, size As Vector2, resolution As Integer, outSamples As Single())
            If outSamples.Length < resolution * resolution Then
                Throw New ArgumentException("Expected outSamples to be at least " & resolution * resolution.ToString() & " in length.")
            End If

            Array.Clear(outSamples, 0, resolution * resolution)

            If path.Count < 2 Then Return

            Dim layerPos = origin.Y
            Dim min = New Vector2(origin.X, origin.Z)
            Dim max = min + size

            If _sNearbyEdges Is Nothing Then
                _sNearbyEdges = New List(Of Edge)()
            Else
                _sNearbyEdges.Clear()
            End If

            FindNearbyEdges(path, layerPos, min, max, _sNearbyEdges)

            Dim row = 0

            While row < resolution
                Dim z = row * (max.Y - min.Y) / resolution + min.Y
                Dim col = 0

                While col < resolution
                    Dim x = col * (max.X - min.X) / resolution + min.X
                    Dim pos = New Vector3(x, layerPos, z)

                    outSamples(col + row * resolution) = GetDistanceScore(pos, _sNearbyEdges)
                    Threading.Interlocked.Increment(col)
                End While

                Threading.Interlocked.Increment(row)
            End While
        End Sub

        <ThreadStatic>
        Private Shared _sBuffer As Single()
        <ThreadStatic>
        Private Shared _sColors As Drawing.Color()
        <ThreadStatic>
        Private Shared _sPath As List(Of Vertex)

        Private Sub Update()
            If Not NeedsUpdate Then Return

            _lastSampledPosition = transform.position
            _lastSampleSize = SampleSize

            If Result Is Nothing Then
                Result = New Texture2D(Resolution, Resolution, TextureFormat.RGB24, False)
            ElseIf Resolution <> Result.width Then
                Result.Resize(Resolution, Resolution, TextureFormat.RGB24, False)
            End If

            If _sPath Is Nothing Then
                _sPath = New List(Of Vertex)()
            Else
                _sPath.Clear()
            End If

            Path.GetVertices(_sPath)

            If _sBuffer Is Nothing OrElse _sBuffer.Length < Resolution * Resolution Then
                _sBuffer = New Single(Resolution * Resolution - 1) {}
                _sColors = New Drawing.Color(Resolution * Resolution - 1) {}
            End If

            Dim origin = _lastSampledPosition - New Vector3(_lastSampleSize.X * 0.5F, 0F, _lastSampleSize.Y * 0.5F)
            SampleDistanceField(_sPath, origin, _lastSampleSize, Resolution, _sBuffer)

            Dim i = 0

            While i < Resolution * Resolution
                Dim value = _sBuffer(i)
                _sColors(i) = New Drawing.Color(value, value, value, 1.0F)
                Threading.Interlocked.Increment(i)
            End While

            Result.SetPixels(_sColors)
            Result.Apply(False)

            Dim meshRenderer = GetComponent(Of MeshRenderer)()
            If meshRenderer IsNot Nothing Then
                meshRenderer.sharedMaterial.mainTexture = Result
            End If
        End Sub
    End Class
End Namespace