#Region "Microsoft.VisualBasic::5b04fed9351856c84047f15780c9049c, Data_science\Graph\test\Module1.vb"

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
    '    Code Lines: 36 (64.29%)
    ' Comment Lines: 11 (19.64%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (16.07%)
    '     File Size: 2.23 KB


    ' Module Program
    ' 
    '     Sub: Main, testFlow
    ' 
    ' /********************************************************************************/

#End Region

' =============================================================================
' 示例: 使用 DinicMaxFlowSolver 计算最大流及节点流量分布
' =============================================================================

Imports Microsoft.VisualBasic.Data.GraphTheory.DinicMaxFlow

Module Program
    Sub Main()
        Call testFlow()
    End Sub

    Sub testFlow()
        ' 创建一个 6 节点的网络图
        ' 节点编号: 0 ~ 5
        Dim solver As New DinicMaxFlowSolver(6)

        ' 添加边 (起始节点, 目标节点, 容量)
        solver.AddEdge(0, 1, 10)   ' 节点0 -> 节点1, 容量 10
        solver.AddEdge(0, 2, 8)    ' 节点0 -> 节点2, 容量 8
        solver.AddEdge(1, 2, 5)    ' 节点1 -> 节点2, 容量 5
        solver.AddEdge(1, 3, 7)    ' 节点1 -> 节点3, 容量 7
        solver.AddEdge(2, 4, 10)   ' 节点2 -> 节点4, 容量 10
        solver.AddEdge(3, 5, 10)   ' 节点3 -> 节点5, 容量 10
        solver.AddEdge(4, 3, 2)    ' 节点4 -> 节点3, 容量 2
        solver.AddEdge(4, 5, 8)    ' 节点4 -> 节点5, 容量 8

        ' 设定源点为 0, 汇点为 5
        Dim source As Integer = 0
        Dim sink As Integer = 5

        ' 计算最大流量
        Dim maxFlowValue As Integer = solver.MaxFlow(source, sink)
        Console.WriteLine(": " & maxFlowValue.ToString())
        Console.WriteLine()

        ' 获取全网络节点的流量分布
        Console.WriteLine("========== 节点流量分布 ==========")
        Dim distribution = solver.GetFlowDistribution(source, sink)
        For Each info As NodeFlowInfo In distribution
            Console.WriteLine(info.ToString())
        Next
        Console.WriteLine()

        ' 获取每条边的流量信息
        Console.WriteLine("========== 边流量信息 ==========")
        Dim edgeInfo = solver.GetEdgeFlowInfo()
        For Each line As String In edgeInfo
            Console.WriteLine(line)
        Next
        Console.WriteLine()

        ' 查看某个节点的详细流量信息
        Console.WriteLine("========== 节点 2 详情 ==========")
        Console.WriteLine(solver.GetNodeDetail(2))
    End Sub
End Module
