#Region "Microsoft.VisualBasic::8f3bc8d275a7175a49e1a99b96abaef9, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\TreeAPI\Operations.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

Namespace TreeAPI

    <PackageNamespace("TREE.Cluster")>
    Public Module Operations

        <ExportAPI("Tree.Build")>
        <Extension>
        Public Function BuildTree(net As IEnumerable(Of FileStream.NetworkEdge)) As BinaryTree(Of NodeTypes)
            Dim ROOTs = net.GetConnections(ROOT)
            Dim tree As New BinaryTree(Of NodeTypes)(ROOT, NodeTypes.ROOT)
            Dim netList = net.AsList

            For Each node In ROOTs
                Dim Xnext As String = node.GetConnectedNode(ROOT)
                Call netList.Remove(node)
                Call tree.Add(ROOT, New TreeNode(Of NodeTypes)(Xnext, NodeTypes.Path))
                Call __buildTREE(tree, Xnext, netList)
            Next

            Return tree
        End Function

        Private Sub __buildTREE(ByRef tree As BinaryTree(Of NodeTypes), node As String, ByRef netList As List(Of FileStream.NetworkEdge))
            Dim nexts = (From x In netList.GetNextConnects(node) Select x Group x By x.InteractionType Into Group)

            For Each part In nexts
                Dim type = __getTypes(part.InteractionType)
                Dim nextNodes = part.Group.ToArray

                If type <> NodeTypes.Path Then
                    For Each x In nextNodes
                        Call netList.Remove(x)
                    Next

                    If tree.DirectFind(node) Is Nothing Then
                        Call tree.insert(node, NodeTypes.Path)
                    End If
                End If

                If type = NodeTypes.Leaf Then
                    Dim left As Boolean = True
                    Dim Leaf As New Leaf(node)

                    Call tree.Add(node, Leaf)

                    For Each nxode In nextNodes
                        Dim nodeChild As New TreeNode(Of NodeTypes)(nxode.ToNode, NodeTypes.Leaf)
                        Call tree.Add(Leaf.Name, nodeChild, left)
                        left = Not left
                    Next
                ElseIf type = NodeTypes.LeafX Then
                    Dim Xnode As New LeafX(node) With {.LeafX = nextNodes}
                    Call tree.Add(node, Xnode, True)  ' Leaf-X 只有一个，默认为左边
                Else ' 这个是Path，则继续建树
                    For Each nxode As FileStream.NetworkEdge In nextNodes
                        Call netList.Remove(nxode)
                        Call tree.Add(node, New TreeNode(Of NodeTypes)(nxode.ToNode, __getTypes(nxode.InteractionType)))
                        Call __buildTREE(tree, nxode.ToNode, netList)
                    Next
                End If
            Next
        End Sub

        Private ReadOnly __getTypes As Dictionary(Of String, NodeTypes) =
            New Dictionary(Of String, NodeTypes) From {
 _
            {PATH, NodeTypes.Path},
            {LEAF_NODE, NodeTypes.Leaf},
            {LEAF_X, NodeTypes.LeafX},
            {ROOT, NodeTypes.ROOT}
        }

        <ExportAPI("GetType")>
        Public Function [GetType](name As String) As NodeTypes
            If __getTypes.ContainsKey(name) Then
                Return __getTypes(name)
            Else
                Return NodeTypes.ROOT
            End If
        End Function

        ''' <summary>
        ''' {最开始的节点，实体列表}
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <ExportAPI("Cluster.Parts")>
        Public Function ClusterParts(net As IEnumerable(Of FileStream.NetworkEdge)) As Dictionary(Of String, String())
            Dim tree As BinaryTree(Of NodeTypes) = net.BuildTree
            Return tree.ClusterParts(AddressOf __isLeaf, AddressOf __isLeafX, AddressOf __getEntities)
        End Function

        Private Function __isLeaf(Of T)(x As TreeNode(Of T)) As Boolean
            Return x.GetType.Equals(GetType(Leaf))
        End Function

        Private Function __isLeafX(Of T)(x As TreeNode(Of T)) As Boolean
            Return x.GetType.Equals(GetType(LeafX))
        End Function

        Private Function __getEntities(Of T)(x As TreeNode(Of T)) As String()
            Dim node As Object = x
            Return DirectCast(node, TreeNode).GetEntities
        End Function
    End Module
End Namespace
