#Region "Microsoft.VisualBasic::161583967d9e808fb8be7845ff95f26f, gr\network-visualization\test\ColaTest.vb"

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

    '   Total Lines: 288
    '    Code Lines: 190 (65.97%)
    ' Comment Lines: 53 (18.40%)
    '    - Xml Docs: 15.09%
    ' 
    '   Blank Lines: 45 (15.62%)
    '     File Size: 11.89 KB


    ' Module ColaTest
    ' 
    '     Sub: ComplexColaTest, Main, SimpleColaTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

Imports Cola = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola
Imports ColaNode = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Node
Imports ColaLayout = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Layout
Imports ColaLink = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Link(Of Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Node)
Imports inode = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module ColaTest

    Sub Main()
        Call ImageDriver.Register()

        ' 100+ 节点的多层混合拓扑压力测试（网格块 + 长链 + 星型簇 + 独立分量）
        Call ComplexColaTest()

        ' 原有的 12 节点环+弦对照用例（已验证），需要时取消注释切换：
        ' Call SimpleColaTest()
    End Sub

    ''' <summary>
    ''' 复杂网络压力测试：构建一个 120+ 节点的多层混合拓扑，刻意用固定种子的随机散点
    ''' 作为初始坐标以制造大量边交叉，再用修正后的 Cola 应力最小化布局去交叉、去重叠，
    ''' 最终渲染为带节点标签的 PNG 供检查。
    ''' </summary>
    Sub ComplexColaTest()
        Dim g As New NetworkGraph
        Dim rand As New Random(20260821)
        Dim idCounter As Integer = 0

        ' helper: 添加节点并以固定种子随机散点作为初始坐标
        Dim addNode = Function(label As String, w As Single, h As Single) As inode
                          Dim x = rand.Next(0, 1400)
                          Dim y = rand.Next(0, 1400)

                          Dim n = New inode With {
                              .label = label,
                              .data = New NodeData With {
                                  .initialPostion = New FDGVector2(x, y),
                                  .size = {w, h}
                              }
                          }

                          Call g.AddNode(n)

                          Return n
                      End Function

        ' ------------------------------------------------------------------
        ' 1) 网格块 8 x 8 = 64 个节点，块内形成密集网格边（大量交叉来源）
        ' ------------------------------------------------------------------
        Dim gridIds(7, 7) As String
        Dim gridNodes(7, 7) As inode

        For r As Integer = 0 To 7
            For c As Integer = 0 To 7
                Dim lbl = $"G{idCounter}"
                idCounter += 1
                gridIds(r, c) = lbl
                gridNodes(r, c) = addNode(lbl, 16.0F, 16.0F)
            Next
        Next

        For r As Integer = 0 To 7
            For c As Integer = 0 To 7
                If c < 7 Then Call g.AddEdge(gridIds(r, c), gridIds(r, c + 1))
                If r < 7 Then Call g.AddEdge(gridIds(r, c), gridIds(r + 1, c))
                ' 额外对角线边，进一步制造交叉
                If r < 7 AndAlso c < 7 Then Call g.AddEdge(gridIds(r, c), gridIds(r + 1, c + 1))
            Next
        Next

        ' ------------------------------------------------------------------
        ' 2) 长链式结构 28 个节点，串联成一条长链（制造跨画布长边）
        ' ------------------------------------------------------------------
        Dim chainIds(27) As String
        For i As Integer = 0 To 27
            Dim lbl = $"C{idCounter}"
            idCounter += 1
            chainIds(i) = lbl
            Call addNode(lbl, 14.0F, 14.0F)
        Next
        For i As Integer = 0 To 26
            Call g.AddEdge(chainIds(i), chainIds(i + 1))
        Next
        ' 把长链两端接到网格上，让整体连成一张大图
        Call g.AddEdge(chainIds(0), gridIds(0, 0))
        Call g.AddEdge(chainIds(27), gridIds(7, 7))

        ' ------------------------------------------------------------------
        ' 3) 三个星型簇：每簇 1 hub + 9 leaf = 30 个节点
        ' ------------------------------------------------------------------
        For s As Integer = 0 To 2
            Dim hubLbl = $"H{idCounter}"
            idCounter += 1
            Call addNode(hubLbl, 26.0F, 26.0F)

            For leaf As Integer = 0 To 8
                Dim leafLbl = $"L{idCounter}"
                idCounter += 1
                Call addNode(leafLbl, 12.0F, 12.0F)
                Call g.AddEdge(hubLbl, leafLbl)
            Next
        Next

        ' ------------------------------------------------------------------
        ' 4) 独立环分量 8 个节点（连通分量，验证 handleDisconnected 路径）
        ' ------------------------------------------------------------------
        Dim ringIds(7) As String
        For i As Integer = 0 To 7
            Dim lbl = $"R{idCounter}"
            idCounter += 1
            ringIds(i) = lbl
            Call addNode(lbl, 15.0F, 15.0F)
        Next
        For i As Integer = 0 To 7
            Call g.AddEdge(ringIds(i), ringIds((i + 1) Mod 8))
        Next

        Console.WriteLine($"Complex network built: {g.vertex.Count} nodes, {g.graphEdges.Count} edges")

        ' Bridge NetworkGraph <-> Cola Node/Link
        Dim allNodes As inode() = g.connectedNodes
        Dim nodes(allNodes.Length - 1) As ColaNode
        Dim indexOf As New Dictionary(Of inode, Integer)

        For i As Integer = 0 To allNodes.Length - 1
            Dim n = allNodes(i)
            Dim p = n.data.initialPostion

            nodes(i) = New ColaNode With {
                .x = p.x,
                .y = p.y,
                .width = n.data.size(0),
                .height = n.data.size(1),
                .index = i
            }
            indexOf(n) = i
        Next

        Dim allEdges As Edge() = g.graphEdges.ToArray()
        Dim links(allEdges.Length - 1) As ColaLink

        For i As Integer = 0 To allEdges.Length - 1
            Dim e = allEdges(i)
            links(i) = New ColaLink With {
                .source = nodes(indexOf(e.U)),
                .target = nodes(indexOf(e.V))
            }
        Next

        ' Run the corrected Cola stress-minimization layout.
        ' symmetricDiffLinkLengths wires up the link-length calculator internally.
        ' NOTE: avoidOverlaps is disabled because Layout/Projection.vb is an incomplete
        ' stub (undefined ProjectionGroup type); the core stress-minimization path is
        ' what we are verifying here. Enable once Projection.vb is completed.
        Dim layout As New ColaLayout

        Call layout _
            .size({1400, 1400}) _
            .avoidOverlaps(False) _
            .symmetricDiffLinkLengths(90, 0.7) _
            .convergenceThreshold(0.01) _
            .nodes(nodes) _
            .links(links) _
            .start()

        ' Write the computed positions back into the network graph
        For i As Integer = 0 To nodes.Length - 1
            allNodes(i).data.initialPostion = New FDGVector2(nodes(i).x, nodes(i).y)
        Next

        ' Render with node labels enabled (displayId:=True) to verify the
        ' LabelRendering fix on the label-color branch.
        ' NOTE: the test runs with working directory = test/, so a relative
        ' path like "./Cola_complex_layout.png" lands in test/ (consistent with
        ' the other HOLA_* tests), not test/test/.
        Call NetworkVisualizer _
            .DrawImage(g, "1400,1400", displayId:=True, labelColorAsNodeColor:=False, drawEdgeBends:=True, labelerIterations:=1500, minLinkWidth:=3) _
            .Save("./Cola_complex_layout.png")

        Console.WriteLine("Complex Cola layout complete. Output written to ./test/Cola_complex_layout.png")
    End Sub

    ''' <summary>
    ''' 原有的 12 节点环+弦对照用例（已验证），保留供对比。
    ''' </summary>
    Sub SimpleColaTest()
        ' Build a small network graph with a ring topology plus a few chords,
        ' so the stress-minimizing Cola layout has a clear symmetric target.
        Dim g As New NetworkGraph
        Dim labels As String() = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"}
        Dim rand As New Random(123)

        For i As Integer = 0 To labels.Length - 1
            ' scatter initial positions so the layout visibly does work
            Dim x = rand.Next(0, 400)
            Dim y = rand.Next(0, 400)

            Call g.AddNode(New inode With {
                .label = labels(i),
                .data = New NodeData With {
                    .initialPostion = New FDGVector2(x, y),
                    .size = {18.0F, 18.0F}
                }
            })
        Next

        ' ring edges
        For i As Integer = 0 To labels.Length - 1
            Dim a = (i + 1) Mod labels.Length
            Call g.AddEdge(labels(i), labels(a))
        Next

        ' a few chords to give the layout structure
        Call g.AddEdge("0", "6")
        Call g.AddEdge("1", "7")
        Call g.AddEdge("3", "9")
        Call g.AddEdge("4", "10")

        ' Bridge NetworkGraph <-> Cola Node/Link
        Dim allNodes As inode() = g.connectedNodes
        Dim nodes(allNodes.Length - 1) As ColaNode

        ' remember the mapping from inode -> cola node index
        Dim indexOf As New Dictionary(Of inode, Integer)

        For i As Integer = 0 To allNodes.Length - 1
            Dim n = allNodes(i)
            Dim p = n.data.initialPostion

            nodes(i) = New ColaNode With {
                .x = p.x,
                .y = p.y,
                .width = n.data.size(0),
                .height = n.data.size(1),
                .index = i
            }
            indexOf(n) = i
        Next

        Dim allEdges As Edge() = g.graphEdges.ToArray()
        Dim links(allEdges.Length - 1) As ColaLink

        For i As Integer = 0 To allEdges.Length - 1
            Dim e = allEdges(i)
            links(i) = New ColaLink With {
                .source = nodes(indexOf(e.U)),
                .target = nodes(indexOf(e.V))
            }
        Next

        ' Run the corrected Cola stress-minimization layout.
        ' symmetricDiffLinkLengths wires up the link-length calculator internally.
        ' NOTE: avoidOverlaps is disabled here because the group-overlap Projection
        ' module (Layout/Projection.vb) is an incomplete stub (undefined ProjectionGroup
        ' type) in this codebase; the core stress-minimization path below is what we
        ' are verifying. Enable once Layout/Projection.vb is completed.
        Dim layout As New ColaLayout

        Call layout _
            .size({1000, 1000}) _
            .avoidOverlaps(False) _
            .symmetricDiffLinkLengths(80, 0.7) _
            .convergenceThreshold(0.01) _
            .nodes(nodes) _
            .links(links) _
            .start()

        ' Write the computed positions back into the network graph
        For i As Integer = 0 To nodes.Length - 1
            allNodes(i).data.initialPostion = New FDGVector2(nodes(i).x, nodes(i).y)
        Next

        ' Render the laid-out graph to an image for visual inspection
        Call NetworkVisualizer _
            .DrawImage(g, "1000,1000", displayId:=False, drawEdgeBends:=True, labelerIterations:=-1, minLinkWidth:=8) _
            .Save("./test/Cola_layout.png")

        Console.WriteLine("Cola layout complete. Output written to ./test/Cola_layout.png")
    End Sub

End Module
