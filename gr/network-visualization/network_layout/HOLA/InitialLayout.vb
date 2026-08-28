#Region "Microsoft.VisualBasic::e5c8b730a13207fae68b52c35943ef80, gr\network-visualization\network_layout\HOLA\InitialLayout.vb"

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

    '   Total Lines: 91
    '    Code Lines: 60 (65.93%)
    ' Comment Lines: 17 (18.68%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 14 (15.38%)
    '     File Size: 3.79 KB


    '     Module InitialLayout
    ' 
    '         Function: Build, FindConnectedComponents
    ' 
    '         Sub: ScatterMissing
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 阶段 1：初始布局。
    ''' 负责把 <see cref="NetworkGraph"/> 转换为内部布局状态，并对缺少初始坐标的节点
    ''' 按连通分量做确定性的网格散布，作为后续松弛迭代的起点。
    ''' </summary>
    Public Module InitialLayout

        ''' <summary>
        ''' 从图中提取参与布局的节点与无向边，构建布局状态。
        ''' 只保留有边相连的节点（与 connectedNodes 语义一致），孤立点不参与 HOLA。
        ''' </summary>
        Public Function Build(graph As NetworkGraph, opts As HolaOptions) As HolaLayoutState
            Dim nodes = graph.connectedNodes
            Dim edgeList As New List(Of (u As Integer, v As Integer))

            For Each e As Edge In graph.graphEdges
                Dim u = Array.IndexOf(nodes, e.U)
                Dim v = Array.IndexOf(nodes, e.V)
                If u >= 0 AndAlso v >= 0 Then
                    edgeList.Add((u, v))
                End If
            Next

            Dim state = New HolaLayoutState(nodes, edgeList.ToArray, opts)

            Call ScatterMissing(state, opts)
            Return state
        End Function

        ''' <summary>
        ''' 对缺少初始坐标的节点，按其所属连通分量做圆形散布，
        ''' 使松驰阶段有合理的起点而非全部堆叠在原点。
        ''' </summary>
        Private Sub ScatterMissing(state As HolaLayoutState, opts As HolaOptions)
            Dim components = FindConnectedComponents(state)
            Dim baseAngle As Double = 0.0

            For Each comp In components
                If comp.All(Function(i) state.positions(i).x = 0 AndAlso state.positions(i).y = 0) Then
                    ' 整个分量都没有初始坐标，做圆形布局
                    Dim r = System.Math.Max(opts.desiredEdgeLength, opts.nodeGap * 2)
                    Dim cx = 200 + baseAngle * 50
                    Dim cy = 200 + baseAngle * 30
                    For k As Integer = 0 To comp.Length - 1
                        Dim a = 2 * System.Math.PI * k / comp.Length
                        state.positions(comp(k)) = New FDGVector2(cx + r * System.Math.Cos(a), cy + r * System.Math.Sin(a))
                    Next
                    baseAngle += 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' 用 BFS 在状态内找出连通分量（返回每组节点的索引数组）。
        ''' </summary>
        Public Function FindConnectedComponents(state As HolaLayoutState) As Integer()()
            Dim visited(state.nodes.Length - 1) As Boolean
            Dim comps As New List(Of Integer())

            For i As Integer = 0 To state.nodes.Length - 1
                If visited(i) Then Continue For

                Dim queue As New Queue(Of Integer)
                Dim comp As New List(Of Integer)
                queue.Enqueue(i)
                visited(i) = True

                While queue.Count > 0
                    Dim cur = queue.Dequeue()
                    comp.Add(cur)
                    For Each nb In state.neighbours(cur)
                        If Not visited(nb) Then
                            visited(nb) = True
                            queue.Enqueue(nb)
                        End If
                    Next
                End While

                comps.Add(comp.ToArray)
            Next

            Return comps.ToArray
        End Function
    End Module
End Namespace

