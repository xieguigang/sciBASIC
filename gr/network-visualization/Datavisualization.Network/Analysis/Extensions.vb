#Region "Microsoft.VisualBasic::ce010c268042d74d3fcc028dc7574ea1, gr\network-visualization\Datavisualization.Network\Analysis\Extensions.vb"

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

'     Module Extensions
' 
'         Function: isTupleEdge
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace Analysis

    <HideModuleName> Public Module Extensions

        ' a-b is tuple
        ' a-b-c is not

        ''' <summary>
        ''' 判断边的两个节点是否只有当前的边连接而再无其他的任何边连接了
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="edge"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isTupleEdge(Of Node As INamedValue, IEdge As {Class, IInteraction})(edge As IEdge, g As GraphIndex(Of Node, IEdge)) As Boolean
            Dim uset = g.GetEdges(edge.source).ToArray
            Dim vset = g.GetEdges(edge.target).ToArray

            If uset.Length = 1 AndAlso vset.Length = 1 AndAlso uset(Scan0) Is vset(Scan0) AndAlso uset(Scan0) Is edge Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Decompose a graph into components, Creates a separate graph for each component of a graph.
        ''' 
        ''' 与<see cref="GraphTheory.Network.IteratesSubNetworks"/>所不同的是，IteratesSubNetworks是分离出独立的子网络
        ''' 而这个函数则是根据连接强度进行子网络的分割
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="minVertices"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function DecomposeGraph(g As NetworkGraph,
                                                Optional weakMode As Boolean = True,
                                                Optional minVertices As Integer = 5) As IEnumerable(Of NetworkGraph)

        End Function

    End Module
End Namespace
