#Region "Microsoft.VisualBasic::23f146f5fd67a5d9312daa68b3106957, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\MarchingCubes\Edge.vb"

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

    '   Total Lines: 42
    '    Code Lines: 34 (80.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (19.05%)
    '     File Size: 1.34 KB


    '     Structure Edge
    ' 
    '         Properties: IsValid, Reverse
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Math3D.MarchingCubes

    Friend Structure Edge
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

End Namespace
