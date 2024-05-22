#Region "Microsoft.VisualBasic::ec3fdbbc3a810e6a9140ce658e80474e, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\MarchingCubes\MarchingCubes.vb"

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

    '   Total Lines: 112
    '    Code Lines: 86 (76.79%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 26 (23.21%)
    '     File Size: 3.44 KB


    '     Class MarchingCubes
    ' 
    '         Properties: CubePos, CubeSize, Threshold
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: LookupCase, WriteVertex
    ' 
    '         Sub: Clear, CopyToMesh, MoveToCube, PopulateLookupTable, Write
    '              WriteFace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models

Namespace Drawing3D.Math3D.MarchingCubes

    Public Class MarchingCubes

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
                _lookupTable(i) = New MarchingCubesCase({
                     (i And &H1) <> 0,
                     (i And &H2) <> 0,
                     (i And &H4) <> 0,
                     (i And &H8) <> 0,
                     (i And &H10) <> 0,
                     (i And &H20) <> 0,
                     (i And &H40) <> 0,
                     (i And &H80) <> 0
                })

                i += 1
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
            If values.Length <> 8 Then
                Throw New Exception("Expected 8 values.")
            End If

            Dim thresh = Threshold
            Dim lookup = 0

            Dim i = 0

            While i < 8
                lookup = lookup Or If(values(i) >= thresh, 1 << i, 0)
                i += 1
            End While

            Return _lookupTable(lookup)
        End Function

        Public Sub Write(values As Single())
            LookupCase(values).Write(Me, values, Threshold)
        End Sub

        Friend Sub WriteFace(a As Integer, b As Integer, c As Integer)
            _indices.Add(a)
            _indices.Add(b)
            _indices.Add(c)
        End Sub

        Friend Function WriteVertex(vertex As Vector3) As Integer
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
        End Sub
    End Class
End Namespace
