#Region "Microsoft.VisualBasic::9dd628a6670c8ba59934c76ffca948d8, gr\network-visualization\Datavisualization.Network\Graph\GraphTree.vb"

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

    '   Total Lines: 123
    '    Code Lines: 80
    ' Comment Lines: 25
    '   Blank Lines: 18
    '     File Size: 4.29 KB


    '     Class GraphTreeNode
    ' 
    '         Properties: Childs, Node, Parents
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class GraphTree
    ' 
    '         Properties: Count, Trees
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: IterateTrees, ToString
    ' 
    '         Sub: IterateTrees
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Graph

    ''' <summary>
    ''' 这是一个节点对象
    ''' </summary>
    Public Class GraphTreeNode

        ''' <summary>
        ''' 从外部链接到当前节点的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Parents As List(Of GraphTreeNode)
        ''' <summary>
        ''' 从当前的节点出发，链接到的其他的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Childs As List(Of GraphTreeNode)
        Public Property Node As Node

        Sub New()
            Parents = New List(Of GraphTreeNode)
            Childs = New List(Of GraphTreeNode)
        End Sub

        Public Overrides Function ToString() As String
            Return Node.ToString
        End Function
    End Class

    Public Class GraphTree

        ''' <summary>
        ''' 一个大网络可能会是由几个小的独立的网络模块所构成，也可能由独立的节点构成
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Trees As GraphTreeNode()

        Dim treeTable As New Dictionary(Of Node, GraphTreeNode)

        ''' <summary>
        ''' Gets the node counts and edge counts.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As (Nodes%, Edges%)
            Get
                Dim edges% = treeTable.Values _
                    .Select(Function(node)
                                Return {node.Parents, node.Childs}
                            End Function) _
                    .IteratesALL _
                    .Distinct _
                    .Count
                Return (Trees.Length, edges%)
            End Get
        End Property

        Sub New(graph As NetworkGraph)
            Trees = IterateTrees(graph, treeTable)
        End Sub

        Public Overrides Function ToString() As String
            With Count
                Return $"Graph tree have { .Nodes} nodes and { .Edges} edges."
            End With
        End Function

        Private Shared Function IterateTrees(graph As NetworkGraph, ByRef travels As Dictionary(Of Node, GraphTreeNode)) As GraphTreeNode()
            Dim trees As New List(Of GraphTreeNode)

            For Each node As Node In graph.vertex
                Dim root As New GraphTreeNode With {
                    .Node = node
                }
                IterateTrees(root, graph, travels)
                trees.Add(root)
            Next

            Return trees.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="current"></param>
        ''' <param name="graph"></param>
        ''' <param name="treeTable">如果下一个节点在这个列表之中，则不会再进行递归操作了</param>
        Private Shared Sub IterateTrees(current As GraphTreeNode, graph As NetworkGraph, ByRef treeTable As Dictionary(Of Node, GraphTreeNode))
            If current Is Nothing Then
                Return
            ElseIf treeTable.ContainsKey(current.Node) Then
                Return
            Else
                Call treeTable.Add(current.Node, current)
            End If

            For Each edge As Edge In graph.graphEdges
                Dim [next] As GraphTreeNode = Nothing

                With current
                    If edge.U = .Node Then
                        [next] = New GraphTreeNode With {
                            .Node = edge.V
                        }
                        [next].Parents.Add(current)
                        .Childs.Add([next])
                    ElseIf edge.V = .Node Then
                        [next] = New GraphTreeNode With {
                            .Node = edge.U
                        }
                        [next].Childs.Add(current)
                        .Parents.Add([next])
                    End If
                End With

                If Not [next] Is Nothing Then
                    Call IterateTrees([next], graph, treeTable)
                End If
            Next
        End Sub
    End Class
End Namespace
