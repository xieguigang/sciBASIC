#Region "Microsoft.VisualBasic::8ab76efcdd2ed32e6729e43c2f44e714, gr\network-visualization\Network.IO.Extensions\Analysis.vb"

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

    '   Total Lines: 102
    '    Code Lines: 59 (57.84%)
    ' Comment Lines: 32 (31.37%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 11 (10.78%)
    '     File Size: 4.24 KB


    ' Module AnalysisAPI
    ' 
    '     Function: AnalysisDegrees, ComputeNodeDegrees, GetDegrees, NodesGroupCount
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NamesOf

Public Module AnalysisAPI

    ''' <summary>
    ''' 统计网络之中的每一种类型的节点的数量
    ''' </summary>
    ''' <param name="net"></param>
    ''' <returns></returns>
    <Extension>
    Public Function NodesGroupCount(net As NetworkTables) As Dictionary(Of String, Integer)
        Return net.nodes _
            .GroupBy(Function(n) n.NodeType) _
            .ToDictionary(Function(x) x.Key,
                          Function(c)
                              Return c.Count
                          End Function)
    End Function

    ''' <summary>
    ''' 获取得到网络之中的每一个节点的Degree度
    ''' </summary>
    ''' <param name="net"></param>
    ''' <returns></returns>
    <Extension> Public Function GetDegrees(net As NetworkTables) As Dictionary(Of String, Integer)
        Dim degree As New Dictionary(Of String, Integer)
        Dim counts = Sub(node$)
                         If degree.ContainsKey(node) Then
                             degree(node) += 1
                         Else
                             Call degree.Add(node, 1)
                         End If
                     End Sub

        For Each edge As NetworkEdge In net.edges
            ' count只会统计edge链接的两个node，故而假若node是孤立的节点，
            ' 则在这个for循环之中是没有被包含进入结果之中的
            Call counts(node:=edge.fromNode)
            Call counts(node:=edge.toNode)
        Next

        For Each node In net.nodes
            ' 补齐这些孤立的节点
            If Not degree.ContainsKey(node.ID) Then
                degree.Add(node.ID, 0)
            End If
        Next

        Return degree
    End Function

    ''' <summary>
    ''' 计算出每一个节点的``Degree``值，并且将结果写入节点的动态属性之中
    ''' 这个函数之中包含有了节点的degree，并且还计算出了indegree和outdegree
    ''' </summary>
    ''' <param name="net"></param>
    <Extension> Public Function AnalysisDegrees(net As NetworkTables) As NetworkTables
        Call net.ComputeNodeDegrees
        Return net
    End Function

    ''' <summary>
    ''' 计算出每一个节点的``Degree``值，并且将结果写入节点的动态属性之中
    ''' 这个函数之中包含有了节点的degree，并且还计算出了indegree和outdegree
    ''' </summary>
    ''' <param name="net"></param>
    <Extension> Public Function ComputeNodeDegrees(ByRef net As NetworkTables) As Dictionary(Of String, Integer)
        Dim degrees As Dictionary(Of String, Integer) = net.GetDegrees
        Dim d%

        With net.edges.ComputeDegreeData
            ' 通过节点之间的边链接关系计算出indegre和outdegree
            ' 如果边连接是没有方向的，则这个计算结果无意义
            For Each node As FileStream.Node In net.nodes
                'If Not degrees.ContainsKey(node.ID) Then
                '    Dim ex As New Exception("nodes: " & net.Nodes.Keys.GetJson)
                '    ex = New Exception("degrees: " & degrees.GetJson, ex)
                '    ex = New Exception("Current node was not found! => " & node.GetJson, ex)
                '    Throw ex
                'Else
                d = degrees(node.ID)
                Call node.Add(names.REFLECTION_ID_MAPPING_DEGREE, d)
                'End If

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
End Module
