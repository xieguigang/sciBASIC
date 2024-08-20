#Region "Microsoft.VisualBasic::a61334b0ce33d599a75c95550fac014d, Data_science\Graph\Network\NetworkGraph.vb"

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

    '   Total Lines: 39
    '    Code Lines: 18 (46.15%)
    ' Comment Lines: 14 (35.90%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 7 (17.95%)
    '     File Size: 1.26 KB


    '     Class NetworkGraph
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Network

    ''' <summary>
    ''' The base type of the network graph
    ''' </summary>
    ''' <typeparam name="Node"></typeparam>
    ''' <typeparam name="Edge"></typeparam>
    Public Class NetworkGraph(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)})
        Inherits Graph(Of Node, Edge, NetworkGraph(Of Node, Edge))

        Public Property id As String
        Public Property name As String

        Sub New()
        End Sub

        ''' <summary>
        ''' Network model copy
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="edges"></param>
        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of Edge))
            Dim index As i32 = Scan0

            For Each node As Node In nodes
                ' because the edge object have a reference to current node
                ' so that the edge key will be updated automatic
                ' after this node id have been updated
                node.ID = ++index

                ' initialize internal components
                Call AddVertex(node)
            Next

            For Each edge As Edge In edges
                Call Insert(edge)
            Next
        End Sub
    End Class
End Namespace
