#Region "Microsoft.VisualBasic::0ee6768b5688de1f2ec7818b802c5e2b, gr\network-visualization\Datavisualization.Network\Graph\NetworkGraphStream.vb"

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

    '   Total Lines: 56
    '    Code Lines: 43 (76.79%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (23.21%)
    '     File Size: 1.69 KB


    '     Class NetworkGraphStream
    ' 
    '         Properties: graphEdges, id, name, vertex
    ' 
    '         Function: CreateNode, GetElementById, MakeGraph, SetEdgeStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Graph

    Public Class NetworkGraphStream

        Public Property id As String
        Public Property name As String

        Dim nodeSet As New Dictionary(Of String, Node)
        Dim createEdgeSet As Func(Of IEnumerable(Of Edge))

        Public ReadOnly Property vertex As IEnumerable(Of Node)
            Get
                Return nodeSet.Values
            End Get
        End Property

        Public ReadOnly Property graphEdges As IEnumerable(Of Edge)
            Get
                Return createEdgeSet()
            End Get
        End Property

        Public Function CreateNode(id As String, nodedata As NodeData) As Node
            nodeSet.Add(id, New Node With {.ID = nodeSet.Count + 1, .data = nodedata, .label = id})
            Return nodeSet(id)
        End Function

        Public Function GetElementById(id As String) As Node
            If nodeSet.ContainsKey(id) Then
                Return nodeSet(id)
            Else
                Return Nothing
            End If
        End Function

        Public Function SetEdgeStream(stream As Func(Of IEnumerable(Of Edge))) As NetworkGraphStream
            createEdgeSet = stream
            Return Me
        End Function

        Public Function MakeGraph() As NetworkGraph
            Dim g As New NetworkGraph

            For Each node As Node In vertex
                Call g.AddNode(node, assignId:=False)
            Next

            For Each link As Edge In createEdgeSet()
                Call g.CreateEdge(link.U, link.V, link.weight, link.data)
            Next

            Return g
        End Function

    End Class
End Namespace
