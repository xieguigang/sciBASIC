#Region "Microsoft.VisualBasic::faec2fde9e0319ee271e55dc44f138d1, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Tree\BinaryTree\ClusterParts.vb"

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

    '     Module ClusterParts
    ' 
    '         Function: __addCluster, ClusterParts
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: __hashLeaf
    ' 
    '             Sub: __continuteCluster
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace ComponentModel.DataStructures.BinaryTree

    Public Module ClusterParts

        Public Const PATH As String = "Path"
        Public Const LEAF_NODE As String = "Leaf"
        Public Const LEAF_X As String = "Leaf-X"
        Public Const ROOT As String = "ROOT"

        ''' <summary>
        ''' {最开始的节点，实体列表}
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("Cluster.Parts")>
        <Extension>
        Public Function ClusterParts(Of T)(tree As BinaryTree(Of T),
                                           isLeaf As IsType(Of T),
                                           isLeafX As IsType(Of T),
                                           GetEntities As GetEntities(Of T)) As Dictionary(Of String, String())

            Dim ROOT As TreeNode(Of T) = tree.FindSymbol(BinaryTree.ClusterParts.ROOT)
            Dim hash As Dictionary(Of String, String()) = New Dictionary(Of String, String())
            For Each x In ROOT.GetEnumerator
                Call x.__addCluster(hash, isLeaf, isLeafX, GetEntities)
            Next
            Return hash
        End Function

        <Extension> Private Function __addCluster(Of T)(node As TreeNode(Of T),
                                                        ByRef hash As Dictionary(Of String, String()),
                                                        isLeaf As IsType(Of T),
                                                        isLeafX As IsType(Of T),
                                                        GetEntities As GetEntities(Of T)) As Dictionary(Of String, String())
            Dim list As New List(Of String)

            For Each x In node.GetEnumerator
                If x.__hashLeaf(isLeaf, isLeafX) Then
                    Dim leafs As New List(Of String)
                    Call x.__continuteCluster(hash, leafs, isLeaf, isLeafX, GetEntities)
                    Call list.AddRange(leafs)
                Else
                    Call x.__addCluster(hash, isLeaf, isLeafX, GetEntities)
                End If
            Next

            If list.Count > 0 Then
                Call hash.Add(node.Name, list.Distinct.ToArray)
            End If

            Return hash
        End Function

        Public Delegate Function IsType(Of T)(node As TreeNode(Of T)) As Boolean
        Public Delegate Function GetEntities(Of T)(node As TreeNode(Of T)) As String()

        <Extension> Private Sub __continuteCluster(Of T)(node As TreeNode(Of T),
                                                         ByRef hash As Dictionary(Of String, String()),
                                                         ByRef list As List(Of String),
                                                         isLeaf As IsType(Of T),
                                                         isLeafX As IsType(Of T),
                                                         GetEntities As GetEntities(Of T))
            For Each x As TreeNode(Of T) In node.GetEnumerator
                If isLeaf(x) OrElse isLeafX(x) Then
                    Dim leafs As String() = GetEntities(x)
                    Call list.AddRange(leafs)
                ElseIf x.__hashLeaf(isLeaf, isLeafX) Then
                    Call x.__continuteCluster(hash, list, isLeaf, isLeafX, GetEntities)
                Else
                    Call x.__addCluster(hash, isLeaf, isLeafX, GetEntities)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 最远只允许隔着一层Path，这些就可以看作为一个cluster
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        <Extension> Private Function __hashLeaf(Of T)(node As TreeNode(Of T), isLeaf As IsType(Of T), isLeafX As IsType(Of T)) As Boolean
            For Each x In node.GetEnumerator
                If isLeaf(x) OrElse isLeafX(x) Then
                    Return True
                End If
                'For Each y In x.GetEnumerator
                '    If TypeOf y Is Leaf OrElse TypeOf x Is LeafX Then
                '        Return True
                '    End If
                'Next
            Next
            Return False
        End Function
    End Module
End Namespace
