#Region "Microsoft.VisualBasic::8062b57f99ce76b2e7acc34a4e993d19, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\Analysis\Statistics.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Linq
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NameOf
Imports NetGraph = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network

Namespace Analysis

    Public Module Statistics

        ''' <summary>
        ''' 获取得到网络之中的每一个节点的Degree度
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension> Public Function GetDegrees(net As NetGraph) As Dictionary(Of String, Integer)
            Dim degree As New Dictionary(Of String, Integer)
            Dim counts = Sub(node$)
                             If degree.ContainsKey(node) Then
                                 degree(node) += 1
                             Else
                                 Call degree.Add(node, 1)
                             End If
                         End Sub

            For Each edge As NetworkEdge In net.Edges
                Call counts(node:=edge.FromNode)
                Call counts(node:=edge.ToNode)
            Next

            Return degree
        End Function

        ''' <summary>
        ''' 计算出每一个节点的``Degree``值，并且将结果写入节点的动态属性之中
        ''' </summary>
        ''' <param name="net"></param>
        <Extension> Public Function ComputeNodeDegrees(ByRef net As NetGraph) As Dictionary(Of String, Integer)
            Dim degrees As Dictionary(Of String, Integer) = net.GetDegrees
            Dim d%

            With net.Edges.ComputeDegreeData
                For Each node As FileStream.Node In net.Nodes
                    d = degrees(node.ID)
                    node.Add(names.REFLECTION_ID_MAPPING_DEGREE, d)

                    If .in.ContainsKey(node.ID) Then
                        d = .in(node.ID)
                        node.Add(names.REFLECTION_ID_MAPPING_DEGREE_IN, d)
                    End If
                    If .out.ContainsKey(node.ID) Then
                        d = .out(node.ID)
                        node.Add(names.REFLECTION_ID_MAPPING_DEGREE_OUT, d)
                    End If
                Next
            End With

            Return degrees
        End Function

        <Extension>
        Public Function ComputeDegreeData(Of T As IInteraction)(edges As IEnumerable(Of T)) As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))
            Dim [in] As New Dictionary(Of String, Integer)
            Dim out As New Dictionary(Of String, Integer)
            Dim count = Sub(node$, ByRef table As Dictionary(Of String, Integer))
                            If table.ContainsKey(node) Then
                                table(node) += 1
                            Else
                                table.Add(node, 1)
                            End If
                        End Sub
            Dim countIn = Sub(node$) Call count(node, [in])
            Dim countOut = Sub(node$) Call count(node, out)

            For Each edge As T In edges
                Call countIn(edge.target)
                Call countOut(edge.source)
            Next

            Return ([in], out)
        End Function

        <Extension>
        Public Function ComputeNodeDegrees(ByRef net As NetworkGraph) As Dictionary(Of String, Integer)
            Dim connectNodes = net _
                .edges _
                .Select(Function(link) {link.Source.ID, link.Target.ID}) _
                .IteratesALL _
                .GroupBy(Function(id) id) _
                .ToDictionary(Function(ID) ID.Key,
                              Function(list) list.Count)
            Dim d%

            With net.edges.ComputeDegreeData
                For Each node In net.nodes
                    d = connectNodes(node.ID)
                    node.Data.Add(names.REFLECTION_ID_MAPPING_DEGREE, d)

                    If .in.ContainsKey(node.ID) Then
                        d = .in(node.ID)
                        node.Data.Add(names.REFLECTION_ID_MAPPING_DEGREE_IN, d)
                    End If
                    If .out.ContainsKey(node.ID) Then
                        d = .out(node.ID)
                        node.Data.Add(names.REFLECTION_ID_MAPPING_DEGREE_OUT, d)
                    End If
                Next
            End With

            Return connectNodes
        End Function

        ''' <summary>
        ''' 统计网络之中的每一种类型的节点的数量
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NodesGroupCount(net As NetGraph) As Dictionary(Of String, Integer)
            Return net.Nodes _
                .GroupBy(Function(n) n.NodeType) _
                .ToDictionary(Function(x) x.Key,
                              Function(c) c.Count)
        End Function
    End Module
End Namespace

