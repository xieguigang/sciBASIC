#Region "Microsoft.VisualBasic::033bd2b84123cb8a2f86d0fdf9ae6121, Data_science\Graph\test\LPATest.vb"

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

    '   Total Lines: 82
    '    Code Lines: 53 (64.63%)
    ' Comment Lines: 14 (17.07%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (18.29%)
    '     File Size: 3.29 KB


    ' Module LPATest
    ' 
    '     Sub: testLPA
    ' 
    ' /********************************************************************************/

#End Region

' =============================================================================
' 示例: 使用 LabelPropagation (LPA标签传播算法) 进行网络图社区划分
'       测试网络: 两个5节点的团(clique) + 1条桥边, 期望划分出2个社区
' =============================================================================

Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.LPA
Imports Microsoft.VisualBasic.Data.GraphTheory.Network

Module LPATest

    Public Sub testLPA()
        ' 构造测试网络:
        '   团A: A1-A5 之间的所有节点都相互连接
        '   团B: B1-B5 之间的所有节点都相互连接
        '   桥边: A5 - B5
        Dim labels As String() = {"A1", "A2", "A3", "A4", "A5",
                                  "B1", "B2", "B3", "B4", "B5"}
        Dim nodes As New List(Of Network.Node)

        For Each name As String In labels
            nodes.Add(New Network.Node With {.label = name})
        Next

        Dim edges As New List(Of Network.Edge(Of Network.Node))

        ' 团A之中的所有的连边
        For i As Integer = 0 To 4
            For j As Integer = i + 1 To 4
                edges.Add(New Network.Edge(Of Network.Node) With {
                    .U = nodes(i),
                    .V = nodes(j)
                })
            Next
        Next

        ' 团B之中的所有的连边
        For i As Integer = 5 To 9
            For j As Integer = i + 1 To 9
                edges.Add(New Network.Edge(Of Network.Node) With {
                    .U = nodes(i),
                    .V = nodes(j)
                })
            Next
        Next

        ' 团A与团B之间的桥边
        edges.Add(New Network.Edge(Of Network.Node) With {
            .U = nodes(4),
            .V = nodes(9)
        })

        ' NetworkGraph(nodes, edges)构造函数会自动为节点分配从0开始的连续ID编号
        Dim g As New NetworkGraph(Of Network.Node, Network.Edge(Of Network.Node))(nodes, edges)

        ' LPA标签传播社区划分
        Dim communities As LabelPropagation = Builder.Load(g).SolveClusters()

        Console.WriteLine()
        Console.WriteLine("========== LPA 社区划分结果 ==========")
        Console.WriteLine($"社区数量: {communities.GetClusterCount()}")
        Console.WriteLine()

        For Each cluster In communities.GetClusters()
            Console.WriteLine($"社区 [{cluster.Key}] 包含 {cluster.Value.Length} 个成员: {String.Join(", ", cluster.Value)}")
        Next

        ' 结果验证: 应该划分出2个社区，并且A系节点和B系节点各自归属同一个社区
        Dim pass As Boolean = communities.GetClusterCount() = 2

        If pass Then
            For Each members As String() In communities.GetClusters().Values
                Dim allA As Boolean = members.All(Function(n) n.StartsWith("A"))
                Dim allB As Boolean = members.All(Function(n) n.StartsWith("B"))
                pass = pass AndAlso (allA OrElse allB)
            Next
        End If

        Console.WriteLine()
        Console.WriteLine($"LPA双团测试: {If(pass, "PASS", "FAIL")}")
    End Sub
End Module
