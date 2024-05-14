#Region "Microsoft.VisualBasic::cae858b2ae57838f1a6f1596cc411651, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\DistanceFieldSampler.vb"

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

    '   Total Lines: 214
    '    Code Lines: 135
    ' Comment Lines: 34
    '   Blank Lines: 45
    '     File Size: 8.20 KB


    '     Class DistanceFieldSampler
    ' 
    '         Properties: NeedsUpdate, SampleSize
    ' 
    '         Function: DistanceToDistanceScore, GetDistance, GetDistanceScore, IsEdgeNearSampleRange
    ' 
    '         Sub: FindNearbyEdges, GetVertices, SampleDistanceField, Update
    '         Structure Vertex
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Structure Edge
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math
Imports Vector3 = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing3D.Math3D

    Public Class DistanceFieldSampler
#Region "Unity Junk"
        Public Path As Path3D
        Public Resolution As Integer = 128

        Private _lastSampleSize As Vector2D
        Private _lastSampledPosition As Vector3

        Private ReadOnly Property SampleSize As Vector2D
            Get
                Return New Vector2D(1, 1)
            End Get
        End Property

        Private ReadOnly Property NeedsUpdate As Boolean
            Get
                If Path Is Nothing Then Return False
                Return std.Abs(_lastSampleSize.x - SampleSize.x) > Single.Epsilon OrElse std.Abs(_lastSampleSize.y - SampleSize.y) > Single.Epsilon
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
        ''' given rectangle, defined by it's vertical position (<paramref name="layerPos"/>), and
        ''' minimum and maximum X and Z bounds (<paramref name="min"/> and <paramref name="max"/>).
        ''' </summary>
        ''' <param name="path">List of vertices to search through/</param>
        ''' <param name="layerPos">Vertical position of the rectangle to find edges near to.</param>
        ''' <param name="min">Minimum X and Z coordinates of the rectangle.</param>
        ''' <param name="max">Maximum X and Z coordinates of the rectangle.</param>
        ''' <param name="outEdges">List to append the results to.</param>
        Private Shared Sub FindNearbyEdges(path As List(Of Vertex), layerPos As Single, min As Vector2D, max As Vector2D, outEdges As List(Of Edge))
            Dim prev = path(path.Count - 1)
            For Each [next] As Vertex In path
                If Not [next].StartPoint Then
                    Dim edge = New Edge(prev, [next])
                    If IsEdgeNearSampleRange(edge, layerPos, min, max) Then
                        outEdges.Add(edge)
                    End If
                End If

                prev = [next]
            Next
        End Sub

        Private Shared Function IsEdgeNearSampleRange(ByRef edge As Edge, layerPos As Single, min As Vector2D, max As Vector2D) As Boolean
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

            If Vector3.Dot(ap, ab) < 0F Then Return ap.Magnitude
            If Vector3.Dot(bp, ab) > 0F Then Return bp.Magnitude

            Return std.Sqrt(Vector3.Cross(CType(ap, Vector3), CType(bp, Vector3)).SqrMagnitude / ab.SqrMagnitude)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function DistanceToDistanceScore(distance As Single, radius As Single) As Single
#If NET48 Then
            Return Math.Clamp(1.0F - distance * 0.5F / radius, 0, 1)
#Else
            Return std.Clamp(1.0F - distance * 0.5F / radius, 0, 1)
#End If
        End Function

        Private Shared Function GetDistanceScore(samplePos As Vector3, edges As List(Of Edge)) As Single
            Dim score = 0F

            For Each edge In edges
                score = std.Max(score, DistanceToDistanceScore(GetDistance(samplePos, edge), edge.First.Radius))
            Next

            Return score
        End Function

        Public Shared Sub SampleDistanceField(path As List(Of Vertex), origin As Vector3, size As Vector2D, resolution As Integer, outSamples As Single())
            If outSamples.Length < resolution * resolution Then
                Throw New ArgumentException("Expected outSamples to be at least " & resolution * resolution.ToString() & " in length.")
            End If

            Array.Clear(outSamples, 0, resolution * resolution)

            If path.Count < 2 Then Return

            Dim layerPos = origin.Y
            Dim min = New Vector2D(origin.X, origin.Z)
            Dim max = min + size

            If _sNearbyEdges Is Nothing Then
                _sNearbyEdges = New List(Of Edge)()
            Else
                _sNearbyEdges.Clear()
            End If

            FindNearbyEdges(path, layerPos, min, max, _sNearbyEdges)

            Dim row = 0

            While row < resolution
                Dim z = row * (max.y - min.y) / resolution + min.y
                Dim col = 0

                While col < resolution
                    Dim x = col * (max.x - min.x) / resolution + min.x
                    Dim pos = New Vector3(x, layerPos, z)

                    outSamples(col + row * resolution) = GetDistanceScore(pos, _sNearbyEdges)
                    col += 1
                End While

                row += 1
            End While
        End Sub

        Shared _sPath As List(Of Vertex)
        Shared _sBuffer As Single()

        Public Sub Update()
            If Not NeedsUpdate Then Return

            If _sPath Is Nothing Then
                _sPath = New List(Of Vertex)()
            Else
                _sPath.Clear()
            End If

            GetVertices(_sPath)

            If _sBuffer.TryCount <> Resolution * Resolution Then
                _sBuffer = New Single(Resolution * Resolution - 1) {}
            End If

            Dim origin = _lastSampledPosition - New Vector3(_lastSampleSize.x * 0.5F, 0F, _lastSampleSize.y * 0.5F)
            SampleDistanceField(_sPath, origin, _lastSampleSize, Resolution, _sBuffer)


        End Sub

        Friend Sub GetVertices(sPath As List(Of DistanceFieldSampler.Vertex))
            For Each pathNode As Vector3 In Path.Points
                Call sPath.Add(New Vertex(pathNode, 0, 0))
            Next
        End Sub
    End Class
End Namespace
