#Region "Microsoft.VisualBasic::3c47a3eb8410c791a4fb5c0be1e82290, gr\network-visualization\Datavisualization.Network\Graph\Extensions.vb"

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
    '         Function: GetNeighbours, NodesID, RemoveDuplicated, RemoveSelfLoop
    ' 
    '         Sub: ApplyAnalysis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Graph

    Public Module Extensions

        ''' <summary>
        ''' Get all of the connected nodes ID from the edges data
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NodesID(network As IEnumerable(Of IInteraction)) As String()
            Return network _
                .Select(Function(link) {link.source, link.target}) _
                .IteratesALL _
                .Distinct _
                .Where(Function(id) Not id.StringEmpty) _
                .ToArray
        End Function

        ''' <summary>
        ''' 生成诸如degree之类的信息
        ''' </summary>
        ''' <param name="net"></param>
        <Extension>
        Public Sub ApplyAnalysis(ByRef net As NetworkGraph)
            For Each node In net.vertex
                node.data.neighbours = net.GetNeighbours(node.Label).ToArray
                node.data(NamesOf.REFLECTION_ID_MAPPING_DEGREE) = node.data.neighborhoods
            Next
        End Sub

        ''' <summary>
        ''' 枚举出和当前的给定编号的节点所连接的节点的索引编号
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GetNeighbours(net As NetworkGraph, node As String) As IEnumerable(Of Integer)
            For Each edge As Edge In net.graphEdges
                Dim connected As String = edge.GetConnectedNode(node)

                If Not String.IsNullOrEmpty(connected) Then
                    Yield net.GetNode(connected).ID
                End If
            Next
        End Function

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        ''' <param name="directed">是否忽略方向？</param>
        ''' <param name="ignoreTypes">是否忽略边的类型？</param>
        <Extension> Public Function RemoveDuplicated(Of T As NetworkEdge)(
                                                    edges As IEnumerable(Of T),
                                                    Optional directed As Boolean = True,
                                                    Optional ignoreTypes As Boolean = False) As T()
            Dim uid = Function(edge As T) As String
                          If directed Then
                              Return edge.GetDirectedGuid(ignoreTypes)
                          Else
                              Return edge.GetNullDirectedGuid(ignoreTypes)
                          End If
                      End Function
            Dim LQuery = edges _
                .GroupBy(uid) _
                .Select(Function(g) g.First) _
                .ToArray

            Return LQuery
        End Function

        ''' <summary>
        ''' 移除自身与自身的边
        ''' </summary>
        ''' <remarks></remarks>
        <Extension> Public Function RemoveSelfLoop(Of T As NetworkEdge)(edges As IEnumerable(Of T)) As T()
            Dim LQuery = LinqAPI.Exec(Of T) <=
 _
                From x As T
                In edges
                Where Not x.SelfLoop
                Select x

            Return LQuery
        End Function
    End Module
End Namespace
