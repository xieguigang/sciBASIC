#Region "Microsoft.VisualBasic::190df25fbad54e34a7d42666819ef248, gr\network-visualization\Datavisualization.Network\Analysis\Model\EdgeTraversalPolicy.vb"

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

    '   Total Lines: 37
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.19 KB


    '     Interface EdgeTraversalPolicy
    ' 
    '         Function: edges, vertex
    ' 
    '     Class ForwardTraversal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: edges, vertex
    ' 
    '     Class BackwardTraversal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: edges, vertex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis.Model

    Public Interface EdgeTraversalPolicy
        Function edges(v As DirectedVertex) As ISet(Of Edge)
        Function vertex(e As Edge) As Node
    End Interface

    Friend Class ForwardTraversal : Implements EdgeTraversalPolicy

        Public Sub New()
        End Sub

        Public Overridable Function edges(v As DirectedVertex) As ISet(Of Edge) Implements EdgeTraversalPolicy.edges
            Return v.outgoingEdges
        End Function

        Public Overridable Function vertex(e As Edge) As Node Implements EdgeTraversalPolicy.vertex
            Return e.U
        End Function
    End Class

    Friend Class BackwardTraversal : Implements EdgeTraversalPolicy

        Public Sub New()
        End Sub

        Public Overridable Function edges(v As DirectedVertex) As ISet(Of Edge) Implements EdgeTraversalPolicy.edges
            Return v.incomingEdges
        End Function

        Public Overridable Function vertex(e As Edge) As Node Implements EdgeTraversalPolicy.vertex
            Return e.V
        End Function
    End Class
End Namespace
