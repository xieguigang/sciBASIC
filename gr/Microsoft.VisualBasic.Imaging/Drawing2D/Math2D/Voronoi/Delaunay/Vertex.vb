#Region "Microsoft.VisualBasic::c03d69d2bc41f73530fed4c393cdc48e, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\Vertex.vb"

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

    '   Total Lines: 127
    '    Code Lines: 97 (76.38%)
    ' Comment Lines: 8 (6.30%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (17.32%)
    '     File Size: 4.06 KB


    '     Class Vertex
    ' 
    '         Properties: Coord, VertexIndex, x, y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Create, Init, Intersect, ToString
    ' 
    '         Sub: Dispose, SetIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Vertex : Implements ICoord

        Public Shared ReadOnly VERTEX_AT_INFINITY As Vertex = New Vertex(Single.NaN, Single.NaN)

#Region "Pool"
        Private Shared pool As Queue(Of Vertex) = New Queue(Of Vertex)()

        Private Shared nVertices As Integer = 0

        Private Shared Function Create(x As Single, y As Single) As Vertex
            If Single.IsNaN(x) OrElse Single.IsNaN(y) Then
                Return VERTEX_AT_INFINITY
            End If
            If pool.Count > 0 Then
                Return pool.Dequeue().Init(x, y)
            Else
                Return New Vertex(x, y)
            End If
        End Function
#End Region

#Region "Object"
        Private coordField As Vector2D
        Public Property Coord As Vector2D Implements ICoord.Coord
            Get
                Return coordField
            End Get
            Set(value As Vector2D)
                coordField = value
            End Set
        End Property

        Public ReadOnly Property x As Single
            Get
                Return coordField.x
            End Get
        End Property
        Public ReadOnly Property y As Single
            Get
                Return coordField.y
            End Get
        End Property

        Private vertexIndexField As Integer
        Public ReadOnly Property VertexIndex As Integer
            Get
                Return vertexIndexField
            End Get
        End Property

        Public Sub New(x As Single, y As Single)
            Init(x, y)
        End Sub

        Private Function Init(x As Single, y As Single) As Vertex
            coordField = New Vector2D(x, y)

            Return Me
        End Function

        Public Sub Dispose()
            coordField = Vector2D.Zero
            pool.Enqueue(Me)
        End Sub

        Public Sub SetIndex()
            vertexIndexField = std.Min(Threading.Interlocked.Increment(nVertices), nVertices - 1)
        End Sub

        Public Overrides Function ToString() As String
            Return "Vertex (" & vertexIndexField.ToString() & ")"
        End Function

        ' 
        ' * This is the only way to make a Vertex
        ' * 
        ' * @param halfedge0
        ' * @param halfedge1
        ' * @return
        ' * 

        Public Shared Function Intersect(halfedge0 As Halfedge, halfedge1 As Halfedge) As Vertex
            Dim edge, edge0, edge1 As Edge
            Dim halfedge As Halfedge
            Dim determinant, intersectionX, intersectionY As Single
            Dim rightOfSite As Boolean

            edge0 = halfedge0.edge
            edge1 = halfedge1.edge
            If edge0 Is Nothing OrElse edge1 Is Nothing Then
                Return Nothing
            End If
            If edge0.RightSite Is edge1.RightSite Then
                Return Nothing
            End If

            determinant = edge0.a * edge1.b - edge0.b * edge1.a
            If std.Abs(determinant) < 0.0000000001 Then
                ' The edges are parallel
                Return Nothing
            End If

            intersectionX = (edge0.c * edge1.b - edge1.c * edge0.b) / determinant
            intersectionY = (edge1.c * edge0.a - edge0.c * edge1.a) / determinant

            If Voronoi.CompareByYThenX(edge0.RightSite, edge1.RightSite) < 0 Then
                halfedge = halfedge0
                edge = edge0
            Else
                halfedge = halfedge1
                edge = edge1
            End If
            rightOfSite = intersectionX >= edge.RightSite.x
            If rightOfSite AndAlso halfedge.leftRight Is LR.LEFT OrElse Not rightOfSite AndAlso halfedge.leftRight Is LR.RIGHT Then
                Return Nothing
            End If

            Return Create(intersectionX, intersectionY)
        End Function
#End Region
    End Class
End Namespace

