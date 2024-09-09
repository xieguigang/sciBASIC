#Region "Microsoft.VisualBasic::80a7474dcc33720a8dd2c34c82ec3227, gr\network-visualization\Datavisualization.Network\Graph\Extensions.vb"

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
    '    Code Lines: 35 (62.50%)
    ' Comment Lines: 15 (26.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (10.71%)
    '     File Size: 2.14 KB


    '     Module Extensions
    ' 
    '         Function: GetNeighbours, NodesID
    ' 
    '         Sub: ApplyAnalysis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Graph

    <HideModuleName> Public Module Extensions

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
        ''' <param name="g"></param>
        <Extension>
        Public Sub ApplyAnalysis(ByRef g As NetworkGraph)
            For Each node In g.vertex
                node.data.neighbours = g.GetNeighbours(node.label).ToArray
                node.data(NamesOf.REFLECTION_ID_MAPPING_DEGREE) = node.data.neighborhoods
            Next

            Call g.ComputeNodeDegrees
        End Sub

        ''' <summary>
        ''' 枚举出和当前的给定编号的节点所连接的节点的索引编号
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GetNeighbours(g As NetworkGraph, node As String) As IEnumerable(Of Integer)
            For Each edge As Edge In g.graphEdges
                Dim connected As String = edge.GetConnectedNode(node)

                If Not String.IsNullOrEmpty(connected) Then
                    Yield g.GetElementByID(connected).ID
                End If
            Next
        End Function
    End Module
End Namespace
