#Region "Microsoft.VisualBasic::667b137dbc0efeee5a2fe32d25463b32, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\MarchingCubes\MarchingCubesCase.vb"

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

    '   Total Lines: 210
    '    Code Lines: 160 (76.19%)
    ' Comment Lines: 1 (0.48%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 49 (23.33%)
    '     File Size: 7.02 KB


    '     Class MarchingCubesCase
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindVertex, GetVertexIndexBuffer, ShouldFlip
    ' 
    '         Sub: DrawGizmos, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull

Namespace Drawing3D.Math3D.MarchingCubes

    Public Class MarchingCubesCase

        Private Shared ReadOnly _sVectorLookup As Vector3() = {
            New Vector3(0F, 0F, 0F),
            New Vector3(1.0F, 0F, 0F),
            New Vector3(0F, 1.0F, 0F),
            New Vector3(1.0F, 1.0F, 0F),
            New Vector3(0F, 0F, 1.0F),
            New Vector3(1.0F, 0F, 1.0F),
            New Vector3(0F, 1.0F, 1.0F),
            New Vector3(1.0F, 1.0F, 1.0F)
        }

        Private ReadOnly _vertices As Vertex()
        Private ReadOnly _edges As Edge()
        Private ReadOnly _triangles As Triangle()

        Public Sub New(corners As Boolean())
            Dim verts = New List(Of Vertex)()
            Dim faces = New List(Of Vertex)(5) {}
            Dim i As Integer = 0

            While i < 6
                faces(i) = New List(Of Vertex)()
                i += 1
            End While

            i = 0

            While i < 8
                Dim x = i Xor &H1
                Dim y = i Xor &H2
                Dim z = i Xor &H4
                Dim vert As Vertex

                If corners(i) AndAlso Not corners(x) Then
                    vert = New Vertex(i, x)
                    verts.Add(vert)
                    faces(If((i And &H2) = 0, 2, 3)).Add(vert)
                    faces(If((i And &H4) = 0, 4, 5)).Add(vert)
                End If
                If corners(i) AndAlso Not corners(y) Then
                    vert = New Vertex(i, y)
                    verts.Add(vert)
                    faces(If((i And &H1) = 0, 0, 1)).Add(vert)
                    faces(If((i And &H4) = 0, 4, 5)).Add(vert)
                End If
                If corners(i) AndAlso Not corners(z) Then
                    vert = New Vertex(i, z)
                    verts.Add(vert)
                    faces(If((i And &H1) = 0, 0, 1)).Add(vert)
                    faces(If((i And &H2) = 0, 2, 3)).Add(vert)
                End If

                i += 1
            End While

            _vertices = verts.ToArray()

            Dim edges = New List(Of Edge)()
            i = 0

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

                        k += 1
                    End While

                    j += 1
                End While

                i += 1
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
                i += 1
            End While

            i = 0

            While i < _triangles.Length
                Dim face = _triangles(i)
                cubes.WriteFace(vertIndices(face.P0Index), vertIndices(face.P1Index), vertIndices(face.P2Index))
                i += 1
            End While
        End Sub

        Public Sub DrawGizmos(values As Single(), threshold As Single)
            Dim i = 0

            While i < _edges.Length
                Dim edge = _edges(i)
                Dim a = FindVertex(values, threshold, edge.A)
                Dim b = FindVertex(values, threshold, edge.B)

                ' Gizmos.DrawLine(a, b)
                i += 1
            End While
        End Sub
    End Class
End Namespace
