#Region "Microsoft.VisualBasic::84fb518c368899760372dd0343fc8a07, Data_science\DataMining\UMAP\Components\Tree\Tree.vb"

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

    '   Total Lines: 256
    '    Code Lines: 178 (69.53%)
    ' Comment Lines: 40 (15.62%)
    '    - Xml Docs: 67.50%
    ' 
    '   Blank Lines: 38 (14.84%)
    '     File Size: 11.23 KB


    '     Module Tree
    ' 
    '         Function: EuclideanRandomProjectionSplit, FlattenTree, MakeEuclideanTree, MakeLeafArray, MakeTree
    '                   NumLeaves, NumNodes, RecursiveFlatten, SearchFlatTree, SelectSide
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

Namespace Tree

    <HideModuleName>
    Friend Module Tree

        ''' <summary>
        ''' Construct a random projection tree based on ``data`` with leaves of size at most ``leafSize``
        ''' </summary>
        Public Function MakeTree(data As Double()(), leafSize As Integer, n As Integer, random As IProvideRandomValues) As RandomProjectionTreeNode
            Dim indices = Enumerable.Range(0, data.Length).ToArray()
            Return Tree.MakeEuclideanTree(data, indices, leafSize, n, random)
        End Function

        Private Function MakeEuclideanTree(data As Double()(),
                                           indices As Integer(),
                                           leafSize As Integer,
                                           q As Integer,
                                           random As IProvideRandomValues) As RandomProjectionTreeNode

            If indices.Length > leafSize Then
                Dim any = Tree.EuclideanRandomProjectionSplit(data, indices, random)
                Dim leftChild = Tree.MakeEuclideanTree(data, any.indicesLeft, leafSize, q + 1, random)
                Dim rightChild = Tree.MakeEuclideanTree(data, any.IndicesRight, leafSize, q + 1, random)

                Return New RandomProjectionTreeNode With {
                    .Indices = indices,
                    .LeftChild = leftChild,
                    .RightChild = rightChild,
                    .IsLeaf = False,
                    .Hyperplane = any.HyperplaneVector,
                    .Offset = any.HyperplaneOffset
                }
            Else
                Return New RandomProjectionTreeNode With {
                    .Indices = indices,
                    .LeftChild = Nothing,
                    .RightChild = Nothing,
                    .IsLeaf = True,
                    .Hyperplane = New Double() {},
                    .Offset = 0
                }
            End If
        End Function

        Public Function FlattenTree(tree As RandomProjectionTreeNode, leafSize As Integer) As FlatTree
            Dim nNodes = NumNodes(tree)
            Dim nLeaves = NumLeaves(tree)

            ' TODO[umap-js]: Verify that sparse code is not relevant...
            Dim hyperplanes = Utils.Range(nNodes).Select(Function(__) New Double(tree.Hyperplane.Length - 1) {}).ToArray()
            Dim offsets = New Double(nNodes - 1) {}
            Dim children = Utils.Range(nNodes).Select(Function(__) {-1, -1}).ToArray()
            Dim indices = Utils.Range(nLeaves).Select(Function(__) Utils.Range(leafSize).[Select](Function(____) -1).ToArray()).ToArray()

            RecursiveFlatten(tree, hyperplanes, offsets, children, indices, 0, 0)

            Return New FlatTree With {
                .Hyperplanes = hyperplanes,
                .Offsets = offsets,
                .Children = children,
                .Indices = indices
            }
        End Function

        ''' <summary>
        ''' Given a set of ``indices`` for data points from ``data``, create 
        ''' a random hyperplane to split the data, returning two arrays indices 
        ''' that fall on either side of the hyperplane. This is the basis for
        ''' a random projection tree, which simply uses this splitting recursively.
        ''' This particular split uses euclidean distance to determine the 
        ''' hyperplane and which side each data sample falls on.
        ''' </summary>
        Private Function EuclideanRandomProjectionSplit(data As Double()(), indices As Integer(), random As IProvideRandomValues) As (indicesLeft As Integer(), IndicesRight As Integer(), HyperplaneVector As Double(), HyperplaneOffset As Double)
            Dim [dim] = data(0).Length

            ' Select two random points, set the hyperplane between them
            Dim leftIndex = random.Next(0, indices.Length)
            Dim rightIndex = random.Next(0, indices.Length)
            rightIndex += If(leftIndex = rightIndex, 1, 0)
            rightIndex = rightIndex Mod indices.Length
            Dim left = indices(leftIndex)
            Dim right = indices(rightIndex)

            ' Compute the normal vector to the hyperplane (the vector between the
            ' two points) and the offset from the origin
            Dim hyperplaneOffset = 0F
            Dim hyperplaneVector = New Double([dim] - 1) {}

            For i = 0 To hyperplaneVector.Length - 1
                hyperplaneVector(i) = data(left)(i) - data(right)(i)
                hyperplaneOffset -= hyperplaneVector(i) * (data(left)(i) + data(right)(i)) / 2
            Next

            ' For each point compute the margin (project into normal vector)
            ' If we are on lower side of the hyperplane put in one pile,
            ' otherwise put it in the other pile (if we hit hyperplane on the
            ' nose, flip a coin)
            Dim nLeft = 0
            Dim nRight = 0
            Dim side = New Integer(indices.Length - 1) {}

            For i = 0 To indices.Length - 1
                Dim margin = hyperplaneOffset

                For d = 0 To [dim] - 1
                    margin += hyperplaneVector(d) * data(indices(i))(d)
                Next

                If margin = 0 Then
                    side(i) = random.Next(0, 2)

                    If side(i) = 0 Then
                        nLeft += 1
                    Else
                        nRight += 1
                    End If
                ElseIf margin > 0 Then
                    side(i) = 0
                    nLeft += 1
                Else
                    side(i) = 1
                    nRight += 1
                End If
            Next

            ' Now that we have the counts, allocate arrays
            Dim indicesLeft = New Integer(nLeft - 1) {}
            Dim indicesRight = New Integer(nRight - 1) {}

            ' Populate the arrays with indices according to which
            ' side they fell on
            nLeft = 0
            nRight = 0

            For i = 0 To side.Length - 1

                If side(i) = 0 Then
                    indicesLeft(nLeft) = indices(i)
                    nLeft += 1
                Else
                    indicesRight(nRight) = indices(i)
                    nRight += 1
                End If
            Next

            Return (indicesLeft, indicesRight, hyperplaneVector, hyperplaneOffset)
        End Function

        Private Function RecursiveFlatten(tree As RandomProjectionTreeNode,
                                          hyperplanes As Double()(),
                                          offsets As Double(),
                                          children As Integer()(),
                                          indices As Integer()(),
                                          nodeNum As Integer,
                                          leafNum As Integer) As (nodeNum As Integer, leafNum As Integer)
            If tree.IsLeaf Then
                children(nodeNum)(0) = -leafNum

                ' TODO[umap-js]: Triple check this operation corresponds to
                ' indices[leafNum : tree.indices.shape[0]] = tree.indices
                tree.Indices.CopyTo(indices(leafNum), 0)
                leafNum += 1
                Return (nodeNum, leafNum)
            Else
                hyperplanes(nodeNum) = tree.Hyperplane
                offsets(nodeNum) = tree.Offset
                children(nodeNum)(0) = nodeNum + 1
                Dim oldNodeNum = nodeNum
                Dim res = RecursiveFlatten(tree.LeftChild, hyperplanes, offsets, children, indices, nodeNum + 1, leafNum)
                nodeNum = res.nodeNum
                leafNum = res.leafNum
                children(oldNodeNum)(1) = nodeNum + 1
                res = RecursiveFlatten(tree.RightChild, hyperplanes, offsets, children, indices, nodeNum + 1, leafNum)
                Return (res.nodeNum, res.leafNum)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function NumNodes(tree As RandomProjectionTreeNode) As Integer
            Return If(tree.IsLeaf, 1, 1 + NumNodes(tree.LeftChild) + NumNodes(tree.RightChild))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function NumLeaves(tree As RandomProjectionTreeNode) As Integer
            Return If(tree.IsLeaf, 1, 1 + NumLeaves(tree.LeftChild) + NumLeaves(tree.RightChild))
        End Function

        ''' <summary>
        ''' Generate an array of sets of candidate nearest neighbors by 
        ''' constructing a random projection forest and taking the leaves
        ''' of all the trees. Any given tree has leaves that are a set 
        ''' of potential nearest neighbors.Given enough trees the set of
        ''' all such leaves gives a good likelihood of getting a good set
        ''' of nearest neighbors in composite. Since such a random 
        ''' projection forest is inexpensive to compute, this can be a 
        ''' useful means of seeding other nearest neighbor algorithms.
        ''' </summary>
        Public Function MakeLeafArray(forest As FlatTree()) As Integer()()
            If forest.Length > 0 Then
                Dim output = New List(Of Integer())()

                For Each tree In forest
                    For Each entry In tree.Indices
                        output.Add(entry)
                    Next
                Next

                Return output.ToArray()
            Else
                Return {New Integer() {-1}}
            End If
        End Function

        ''' <summary>
        ''' Searches a flattened rp-tree for a point
        ''' </summary>
        Public Function SearchFlatTree(point As Double(), tree As FlatTree, random As IProvideRandomValues) As Integer()
            Dim node = 0

            While tree.Children(node)(0) > 0
                Dim side = SelectSide(tree.Hyperplanes(node), tree.Offsets(node), point, random)

                If side = 0 Then
                    node = tree.Children(node)(0)
                Else
                    node = tree.Children(node)(1)
                End If
            End While

            Dim index = -1 * tree.Children(node)(0)
            Return tree.Indices(index)
        End Function

        ''' <summary>
        ''' Select the side of the tree to search during flat tree search
        ''' </summary>
        Private Function SelectSide(hyperplane As Double(), offset As Double, point As Double(), random As IProvideRandomValues) As Integer
            Dim margin = offset

            For d = 0 To point.Length - 1
                margin += hyperplane(d) * point(d)
            Next

            If margin = 0 Then
                Return random.Next(0, 2)
            ElseIf margin > 0 Then
                Return 0
            Else
                Return 1
            End If
        End Function
    End Module
End Namespace
