#Region "Microsoft.VisualBasic::4a0e8a387e69725f8daa801736a12652, gr\network-visualization\Datavisualization.Network\Analysis\Model\AdjacencySet.vb"

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

'   Total Lines: 97
'    Code Lines: 59 (60.82%)
' Comment Lines: 22 (22.68%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 16 (16.49%)
'     File Size: 3.68 KB


'     Class AdjacencySet
' 
'         Properties: Count, U
' 
'         Function: Clone, (+2 Overloads) EnumerateAllEdges, hasNeighbor, ToString
' 
'         Sub: Add, Remove
' 
'         Operators: (+2 Overloads) Like
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Analysis.Model

    ''' <summary>
    ''' 在这个集合中，所有的<see cref=" SparseGraph.IInteraction.source"/>都是一样的
    ''' </summary>
    Public Class AdjacencySet(Of Edge As SparseGraph.IInteraction) : Implements ICloneable(Of AdjacencySet(Of Edge))

        ''' <summary>
        ''' ``{V => edges}``
        ''' </summary>
        ReadOnly adjacentNodes As New Dictionary(Of String, EdgeSet(Of Edge))

        ''' <summary>
        ''' 当前的这个节点的<see cref="INamedValue.Key"/>
        ''' 在这里<see cref="adjacentNodes"/>所有的边对象都是与这个label所代表的节点相连接的
        ''' </summary>
        ''' <returns></returns>
        Public Property U As String

        Public ReadOnly Property Count As Integer
            Get
                Return adjacentNodes.Count
            End Get
        End Property

        ''' <summary>
        ''' check the input node <paramref name="i"/> is a 
        ''' connected adjacent node to current node vertex.
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function hasNeighbor(i As Node) As Boolean
            Return EnumerateAllEdges.Any(Function(link) link.source = i.label OrElse link.target = i.label)
        End Function

        Public Sub Add(edge As Edge)
            If Not adjacentNodes.ContainsKey(edge.target) Then
                adjacentNodes.Add(edge.target, New EdgeSet(Of Edge))
            End If

            adjacentNodes(edge.target).Add(edge)
        End Sub

        Public Sub Remove(V As String)
            If adjacentNodes.ContainsKey(V) Then
                Call adjacentNodes.Remove(V)
            End If
        End Sub

        Public Iterator Function EnumerateAllEdges() As IEnumerable(Of Edge)
            For Each nodeV As EdgeSet(Of Edge) In adjacentNodes.Values
                For Each edge As Edge In nodeV
                    Yield edge
                Next
            Next
        End Function

        ''' <summary>
        ''' 获取目标两个节点之间的所有的重复的边连接
        ''' </summary>
        ''' <param name="V"></param>
        ''' <returns></returns>
        Public Function EnumerateAllEdges(V As INamedValue) As IEnumerable(Of Edge)
            If Not adjacentNodes.ContainsKey(V.Key) Then
                Return {}
            Else
                Return adjacentNodes.TryGetValue(V.Key).AsEnumerable
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"Node {U} have {Count} adjacent nodes: {adjacentNodes.Keys.ToArray.GetJson}"
        End Function

        Public Function Clone() As AdjacencySet(Of Edge) Implements ICloneable(Of AdjacencySet(Of Edge)).Clone
            Dim [set] As New AdjacencySet(Of Edge) With {.U = U}

            For Each nodeV In adjacentNodes
                Call [set].adjacentNodes.Add(nodeV.Key, nodeV.Value)
            Next

            Return [set]
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(vlab As String, aset As AdjacencySet(Of Edge)) As Boolean
            Return aset.adjacentNodes.ContainsKey(vlab)
        End Operator
    End Class
End Namespace
