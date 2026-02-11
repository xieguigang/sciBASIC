#Region "Microsoft.VisualBasic::8d57b5ca19659ecbf15691ebf31ccf10, Data_science\DataMining\DBNCode\utils\Edge.vb"

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

    '   Total Lines: 70
    '    Code Lines: 57 (81.43%)
    ' Comment Lines: 2 (2.86%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (15.71%)
    '     File Size: 2.31 KB


    '     Class Edge
    ' 
    '         Properties: Head, OriginalWeight, Tail, Weight
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo, Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace utils

    Public Class Edge : Implements IComparable(Of Edge)

        Public Overridable ReadOnly Property Tail As Integer
        Public Overridable ReadOnly Property Head As Integer
        Public Overridable ReadOnly Property OriginalWeight As Double

        Public Overridable Property Weight As Double
            Get
                Return updatedWeight
            End Get
            Set(value As Double)
                updatedWeight = value
            End Set
        End Property

        Dim updatedWeight As Double

        Public Sub New(tail As Integer, head As Integer, weight As Double)
            _Tail = tail
            _Head = head
            updatedWeight = weight
            OriginalWeight = weight
        End Sub

        Public Sub New(tail As Integer, head As Integer)
            Me.New(tail, head, 0)
        End Sub

        Public Overridable Function CompareTo(anotherEdge As Edge) As Integer Implements IComparable(Of Edge).CompareTo
            Return -1 * updatedWeight.CompareTo(anotherEdge.updatedWeight)
        End Function

        Public Overrides Function ToString() As String
            '		DecimalFormat df = new DecimalFormat("0.00"); 
            '		return "("+ tail + ", "+ head +") "+ df.format(updatedWeight);
            Return "(" & Tail.ToString() & ", " & Head.ToString() & ")"
        End Function

        Public Overrides Function GetHashCode() As Integer
            Const prime = 31
            Dim result = 1
            result = prime * result + Head
            result = prime * result + Tail
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Not (TypeOf obj Is Edge) Then
                Return False
            End If
            Dim other As Edge = CType(obj, Edge)
            If Head <> other.Head Then
                Return False
            End If
            If Tail <> other.Tail Then
                Return False
            End If
            Return True
        End Function
    End Class

End Namespace

