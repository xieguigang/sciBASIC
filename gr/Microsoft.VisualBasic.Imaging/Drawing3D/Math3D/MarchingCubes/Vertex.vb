#Region "Microsoft.VisualBasic::5f73fb8513de2f0cfc9b2d595db7cc91, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\MarchingCubes\Vertex.vb"

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

    '   Total Lines: 30
    '    Code Lines: 24 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 1.05 KB


    '     Structure Vertex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Math3D.MarchingCubes

    Friend Structure Vertex
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
End Namespace
