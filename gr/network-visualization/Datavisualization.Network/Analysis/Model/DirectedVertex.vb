#Region "Microsoft.VisualBasic::55c409264bc7b5e3d4216cf3722b90c8, gr\network-visualization\Datavisualization.Network\Analysis\Model\DirectedVertex.vb"

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

    '   Total Lines: 140
    '    Code Lines: 79 (56.43%)
    ' Comment Lines: 42 (30.00%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 19 (13.57%)
    '     File Size: 4.68 KB


    '     Class DirectedVertex
    ' 
    '         Properties: connectivity, incomingEdges, inDegree, label, outDegree
    '                     outgoingEdges
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getEdgeTo, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: addEdge, addIncomingEdge, addOutgoingEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq

Namespace Analysis.Model

    ''' <summary>
    ''' an edge collection for indexing the in/out edge connection with current node.
    ''' </summary>
    Public Class DirectedVertex : Implements IEnumerable(Of Edge)

        Public ReadOnly Property label As String

        ''' <summary>
        ''' me to target
        ''' </summary>
        Friend ReadOnly m_outgoingEdges As New HashSet(Of Edge)
        ''' <summary>
        ''' source to me
        ''' </summary>
        Friend ReadOnly m_incomingEdges As New HashSet(Of Edge)

        Public ReadOnly Property outDegree As Integer
            Get
                Return m_outgoingEdges.Count
            End Get
        End Property

        Public ReadOnly Property inDegree As Integer
            Get
                Return m_incomingEdges.Count
            End Get
        End Property

        Public ReadOnly Property outgoingEdges As IEnumerable(Of Edge)
            Get
                Return m_outgoingEdges.AsEnumerable
            End Get
        End Property

        Public ReadOnly Property incomingEdges As IEnumerable(Of Edge)
            Get
                Return m_incomingEdges.AsEnumerable
            End Get
        End Property

        ''' <summary>
        ''' summ all connected edge weights as weighted degree connectivity
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' the edge weight data should be not missing!
        ''' </remarks>
        Public ReadOnly Property connectivity As Double
            Get
                Return Aggregate link As Edge
                       In m_outgoingEdges.JoinIterates(m_incomingEdges)
                       Into Sum(link.weight)
            End Get
        End Property

        ''' <summary>
        ''' create a new node edge connection collection object
        ''' </summary>
        ''' <param name="label">the node unique id</param>
        ''' <exception cref="ArgumentNullException"></exception>
        Sub New(label As String)
            Me.label = label

            If label.StringEmpty Then
                Throw New ArgumentNullException("vertex label can not be empty!")
            End If
        End Sub

        ''' <summary>
        ''' unsafe add outgoing edge
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub addOutgoingEdge(edge As Edge)
            Call m_outgoingEdges.Add(edge)
        End Sub

        ''' <summary>
        ''' unsafe add source to me edge
        ''' </summary>
        ''' <param name="e"></param>
        Public Sub addIncomingEdge(e As Edge)
            Call m_incomingEdges.Add(e)
        End Sub

        ''' <summary>
        ''' safely add an edge to current vertex
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub addEdge(edge As Edge)
            If edge.U.label = label Then
                ' me is source
                ' me to target
                Call addOutgoingEdge(edge)
            ElseIf edge.V.label = label Then
                ' me is target
                ' source to me
                Call addIncomingEdge(edge)
            Else
                Throw New InvalidConstraintException($"the given edge({edge.U} -> {edge.V}) object is not connected to current vertex {label}!")
            End If
        End Sub

        ''' <summary>
        ''' get first edge that connected to the given <paramref name="v2"/> node.
        ''' </summary>
        ''' <param name="v2">target to</param>
        ''' <returns></returns>
        Public Function getEdgeTo(v2 As DirectedVertex) As Edge
            For Each edge As Edge In m_outgoingEdges
                If edge.V.label = v2.label Then
                    Return edge
                End If
            Next

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return label
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Edge) Implements IEnumerable(Of Edge).GetEnumerator
            For Each edge As Edge In m_outgoingEdges
                Yield edge
            Next
            For Each edge As Edge In m_incomingEdges
                Yield edge
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
