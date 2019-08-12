#Region "Microsoft.VisualBasic::e002114d43edf8ea99d7789fe250420e, gr\network-visualization\Datavisualization.Network\Graph\Model\AdjacencySet.vb"

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

    '     Class EdgeSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class AdjacencySet
    ' 
    '         Properties: Count, U
    ' 
    '         Function: Clone, (+2 Overloads) EnumerateAllEdges, ToString
    ' 
    '         Sub: Add, Remove
    ' 
    '         Operators: (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graph

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 所有的<see cref="Edge.U"/>和<see cref="Edge.V"/>都是一样的
    ''' </remarks>
    Public Class EdgeSet : Inherits List(Of Edge)

        Sub New()
            Call MyBase.New
        End Sub

        Public Overrides Function ToString() As String
            Dim first As Edge = Me.First

            If Count = 1 Then
                Return $"[{first.U.label}, {first.V.label}]"
            Else
                Return $"[{first.U.label}, {first.V.label}] have {Count} duplicated connections."
            End If
        End Function
    End Class

    ''' <summary>
    ''' 在这个集合中，所有的<see cref="Edge.U"/>都是一样的
    ''' </summary>
    Public Class AdjacencySet : Implements ICloneable(Of AdjacencySet)

        ''' <summary>
        ''' ``{V => edges}``
        ''' </summary>
        ReadOnly adjacentNodes As New Dictionary(Of String, EdgeSet)

        Public Property U As String

        Public ReadOnly Property Count As Integer
            Get
                Return adjacentNodes.Count
            End Get
        End Property

        Public Sub Add(edge As Edge)
            If Not adjacentNodes.ContainsKey(edge.V.label) Then
                adjacentNodes.Add(edge.V.label, New EdgeSet)
            End If

            adjacentNodes(edge.V.label).Add(edge)
        End Sub

        Public Sub Remove(V As Node)
            If adjacentNodes.ContainsKey(V.label) Then
                Call adjacentNodes.Remove(V.label)
            End If
        End Sub

        Public Iterator Function EnumerateAllEdges() As IEnumerable(Of Edge)
            For Each nodeV As EdgeSet In adjacentNodes.Values
                For Each edge As Edge In nodeV
                    Yield edge
                Next
            Next
        End Function

        Public Function EnumerateAllEdges(V As Node) As IEnumerable(Of Edge)
            If Not adjacentNodes.ContainsKey(V.label) Then
                Return {}
            Else
                Return adjacentNodes.TryGetValue(V.label).AsEnumerable
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"Node {U} have {adjacentNodes.Count} adjacent nodes: {adjacentNodes.Keys.GetJson}"
        End Function

        Public Function Clone() As AdjacencySet Implements ICloneable(Of AdjacencySet).Clone
            Dim [set] As New AdjacencySet With {.U = U}

            For Each nodeV In adjacentNodes
                Call [set].adjacentNodes.Add(nodeV.Key, nodeV.Value)
            Next

            Return [set]
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(vlab As String, aset As AdjacencySet) As Boolean
            Return aset.adjacentNodes.ContainsKey(vlab)
        End Operator
    End Class
End Namespace
