#Region "Microsoft.VisualBasic::c69ce8ad31ea97f9c344936893785ae7, Data_science\DataMining\BinaryTree\ClusterTree\ClusterTree.vb"

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

    '   Total Lines: 134
    '    Code Lines: 68 (50.75%)
    ' Comment Lines: 49 (36.57%)
    '    - Xml Docs: 87.76%
    ' 
    '   Blank Lines: 17 (12.69%)
    '     File Size: 4.69 KB


    ' Class ClusterTree
    ' 
    '     Properties: Members
    ' 
    '     Function: Add, GetClusters
    ' 
    '     Sub: Add, populateNodes
    '     Class Argument
    ' 
    '         Function: GetSimilarity, SetTargetKey
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory

''' <summary>
''' A simple cluster tree builder
''' 
''' just provides the reference id of the target object for make the 
''' alignment or comparision.
''' 
''' the comparision for build the tree is under the given <see cref="Argument"/>.
''' </summary>
''' <remarks>
''' implements the Molecule Networking via the tree clustering operation in mzkit
''' this model is a kind of tree with multiple branches, each branches standards for
''' different similarity SCORE LEVELs.
''' </remarks>
Public Class ClusterTree : Inherits Tree(Of String)

    ''' <summary>
    ''' This property includes all data in current cluster tree node, 
    ''' also includes the <see cref="Data"/> member.
    ''' </summary>
    ''' <returns></returns>
    Public Property Members As New List(Of String)

    Public Class Argument

        Public target As String
        ''' <summary>
        ''' evaluate score by compare two dataset which are related 
        ''' to the input key name as reference id.
        ''' </summary>
        Public alignment As ComparisonProvider
        ''' <summary>
        ''' the cutoff value for set current element 
        ''' <see cref="target"/> as the member of
        ''' current node <see cref="ClusterTree"/>.
        ''' </summary>
        Public threshold As Double

        ''' <summary>
        ''' default interval score value is 0.05
        ''' </summary>
        Public diff As Double = 0.05

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSimilarity(id As String) As Double
            Return alignment.GetSimilarity(id, target)
        End Function

        Public Function SetTargetKey(target As String) As Argument
            Me.target = target
            Return Me
        End Function

    End Class

    ''' <summary>
    ''' build tree
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <remarks>
    ''' the <see cref="Argument.target"/> object will be added into <see cref="Members"/> 
    ''' if a node is asserts that the similarity score between <see cref="Argument.target"/> and
    ''' <see cref="Data"/> is greater than <see cref="Argument.threshold"/>.
    ''' 
    ''' or create a new node if no hits: the new node its <see cref="Data"/> is the
    ''' <see cref="Argument.target"/>.
    ''' </remarks>
    Public Overloads Shared Function Add(tree As ClusterTree, args As Argument, ByRef Optional find As ClusterTree = Nothing) As String
        If tree.Data.StringEmpty Then
            ' is empty node, just add target to current
            tree.Data = args.target
            tree.Childs = New Dictionary(Of String, Tree(Of String))
            tree.Members = New List(Of String) From {args.target}
            find = tree
            Return args.target
        End If

        Dim score As Double = args.GetSimilarity(tree.Data)
        Dim key As String = "zero"

        If score > 0.0 Then
            For v As Double = args.diff To 1 Step args.diff
                If score < v Then
                    key = $"<{v.ToString("F1")}"
                    Exit For
                ElseIf v >= args.threshold Then
                    key = ""
                    Exit For
                End If
            Next
        End If

        If key = "" Then
            ' is cluster member
            tree.Members.Add(args.target)
            find = tree
            Return tree.Data
        ElseIf tree.Childs.ContainsKey(key) Then
            Return Add(tree(key), args, find)
        Else
            Call tree.Add(key)
            Return Add(tree(key), args, find)
        End If
    End Function

    Public Shared Function GetClusters(root As ClusterTree) As IEnumerable(Of ClusterTree)
        Dim links As New List(Of ClusterTree)
        Call populateNodes(root, links)
        Return links
    End Function

    Private Shared Sub populateNodes(root As ClusterTree, ByRef links As List(Of ClusterTree))
        Call links.Add(root)

        If Not root.Childs.IsNullOrEmpty Then
            For Each child As Tree(Of String) In root.Childs.Values
                Call populateNodes(DirectCast(child, ClusterTree), links)
            Next
        End If
    End Sub

    ''' <summary>
    ''' add a new child node
    ''' </summary>
    ''' <param name="label"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Overloads Sub Add(label As String)
        Call Add(New ClusterTree With {.label = label})
    End Sub

End Class
