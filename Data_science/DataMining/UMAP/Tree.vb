Imports System.Collections.Generic
Imports System.Linq

Friend Module Tree
    ''' <summary>
    ''' Construct a random projection tree based on ``data`` with leaves of size at most ``leafSize``
    ''' </summary>
    Public Function MakeTree(ByVal data As Single()(), ByVal leafSize As Integer, ByVal n As Integer, ByVal random As UMAP.IProvideRandomValues) As UMAP.Tree.RandomProjectionTreeNode
        Dim indices = Enumerable.Range(0, data.Length).ToArray()
        Return UMAP.Tree.MakeEuclideanTree(data, indices, leafSize, n, random)
    End Function

    Private Function MakeEuclideanTree(ByVal data As Single()(), ByVal indices As Integer(), ByVal leafSize As Integer, ByVal q As Integer, ByVal random As UMAP.IProvideRandomValues) As UMAP.Tree.RandomProjectionTreeNode
        Dim indicesLeftIndicesRightHyperplaneVectorHyperplaneOffset = Nothing

        If indices.Length > leafSize Then
            indicesLeftIndicesRightHyperplaneVectorHyperplaneOffset = UMAP.Tree.EuclideanRandomProjectionSplit(data, indices, random)
            Dim leftChild = UMAP.Tree.MakeEuclideanTree(data, indicesLeft, leafSize, q + 1, random)
            Dim rightChild = UMAP.Tree.MakeEuclideanTree(data, indicesRight, leafSize, q + 1, random)
            Return New UMAP.Tree.RandomProjectionTreeNode With {
                .indices = indices,
                .leftChild = leftChild,
                .rightChild = rightChild,
                .IsLeaf = False,
                .Hyperplane = hyperplaneVector,
                .Offset = hyperplaneOffset
            }
        Else
            Return New UMAP.Tree.RandomProjectionTreeNode With {
                .indices = indices,
                .LeftChild = Nothing,
                .RightChild = Nothing,
                .IsLeaf = True,
                .Hyperplane = Nothing,
                .Offset = 0
            }
        End If
    End Function

    Public Function FlattenTree(ByVal tree As UMAP.Tree.RandomProjectionTreeNode, ByVal leafSize As Integer) As UMAP.Tree.FlatTree
        Dim nNodes = UMAP.Tree.NumNodes(tree)
        Dim nLeaves = UMAP.Tree.NumLeaves(tree)

        ' TODO[umap-js]: Verify that sparse code is not relevant...
        Dim hyperplanes = UMAP.Utils.Range(nNodes).[Select](Function(__) New Single(tree.Hyperplane.Length - 1) {}).ToArray()
        Dim offsets = New Single(nNodes - 1) {}
        Dim children = UMAP.Utils.Range(nNodes).[Select](Function(__) {-1, -1}).ToArray()
        Dim indices = UMAP.Utils.Range(nLeaves).[Select](Function(__) UMAP.Utils.Range(leafSize).[Select](Function(____) -1).ToArray()).ToArray()
        UMAP.Tree.RecursiveFlatten(tree, hyperplanes, offsets, children, indices, 0, 0)
        Return New UMAP.Tree.FlatTree With {
            .hyperplanes = hyperplanes,
            .offsets = offsets,
            .children = children,
            .indices = indices
        }
    End Function

    ''' <summary>
    ''' Given a set of ``indices`` for data points from ``data``, create a random hyperplane to split the data, returning two arrays indices that fall on either side of the hyperplane. This is
    ''' the basis for a random projection tree, which simply uses this splitting recursively. This particular split uses euclidean distance to determine the hyperplane and which side each data
    ''' sample falls on.
    ''' </summary>
    Private Function EuclideanRandomProjectionSplit(ByVal data As Single()(), ByVal indices As Integer(), ByVal random As UMAP.IProvideRandomValues) As (Integer(), Integer(), Single(), Single)
        Dim [dim] = data(0).Length

        ' Select two random points, set the hyperplane between them
        Dim leftIndex = random.Next(0, indices.Length)
        Dim rightIndex = random.Next(0, indices.Length)
        rightIndex += If(leftIndex = rightIndex, 1, 0)
        rightIndex = rightIndex Mod indices.Length
        Dim left = indices(leftIndex)
        Dim right = indices(rightIndex)

        ' Compute the normal vector to the hyperplane (the vector between the two points) and the offset from the origin
        Dim hyperplaneOffset = 0F
        Dim hyperplaneVector = New Single([dim] - 1) {}

        For i = 0 To hyperplaneVector.Length - 1
            hyperplaneVector(i) = data(left)(i) - data(right)(i)
            hyperplaneOffset -= hyperplaneVector(i) * (data(left)(i) + data(right)(i)) / 2
        Next

        ' For each point compute the margin (project into normal vector)
        ' If we are on lower side of the hyperplane put in one pile, otherwise put it in the other pile (if we hit hyperplane on the nose, flip a coin)
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

        ' Populate the arrays with indices according to which side they fell on
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

    Private Function RecursiveFlatten(ByVal tree As UMAP.Tree.RandomProjectionTreeNode, ByVal hyperplanes As Single()(), ByVal offsets As Single(), ByVal children As Integer()(), ByVal indices As Integer()(), ByVal nodeNum As Integer, ByVal leafNum As Integer) As (Integer, Integer)
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
            Dim res = UMAP.Tree.RecursiveFlatten(tree.LeftChild, hyperplanes, offsets, children, indices, nodeNum + 1, leafNum)
            nodeNum = res.nodeNum
            leafNum = res.leafNum
            children(oldNodeNum)(1) = nodeNum + 1
            res = UMAP.Tree.RecursiveFlatten(tree.RightChild, hyperplanes, offsets, children, indices, nodeNum + 1, leafNum)
            Return (res.nodeNum, res.leafNum)
        End If
    End Function

    Private Function NumNodes(ByVal tree As UMAP.Tree.RandomProjectionTreeNode) As Integer
        Return If(tree.IsLeaf, 1, 1 + UMAP.Tree.NumNodes(tree.LeftChild) + UMAP.Tree.NumNodes(tree.RightChild))
    End Function

    Private Function NumLeaves(ByVal tree As UMAP.Tree.RandomProjectionTreeNode) As Integer
        Return If(tree.IsLeaf, 1, 1 + UMAP.Tree.NumLeaves(tree.LeftChild) + UMAP.Tree.NumLeaves(tree.RightChild))
    End Function

    ''' <summary>
    ''' Generate an array of sets of candidate nearest neighbors by constructing a random projection forest and taking the leaves of all the trees. Any given tree has leaves that are
    ''' a set of potential nearest neighbors.Given enough trees the set of all such leaves gives a good likelihood of getting a good set of nearest neighbors in composite. Since such
    ''' a random projection forest is inexpensive to compute, this can be a useful means of seeding other nearest neighbor algorithms.
    ''' </summary>
    Public Function MakeLeafArray(ByVal forest As UMAP.Tree.FlatTree()) As Integer()()
        If forest.Length > 0 Then
            Dim output = New List(Of Integer())()

            For Each tree In forest

                For Each entry In tree.Indices
                    output.Add(entry)
                Next
            Next

            Return output.ToArray()
        Else
            Return {
            {-1}}
        End If
    End Function

    ''' <summary>
    ''' Searches a flattened rp-tree for a point
    ''' </summary>
    Public Function SearchFlatTree(ByVal point As Single(), ByVal tree As UMAP.Tree.FlatTree, ByVal random As UMAP.IProvideRandomValues) As Integer()
        Dim node = 0

        While tree.Children(node)(0) > 0
            Dim side = UMAP.Tree.SelectSide(tree.Hyperplanes(node), tree.Offsets(node), point, random)

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
    Private Function SelectSide(ByVal hyperplane As Single(), ByVal offset As Single, ByVal point As Single(), ByVal random As UMAP.IProvideRandomValues) As Integer
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

    Public NotInheritable Class FlatTree
        Public Property Hyperplanes As Single()()
        Public Property Offsets As Single()
        Public Property Children As Integer()()
        Public Property Indices As Integer()()
    End Class

    Public NotInheritable Class RandomProjectionTreeNode
        Public Property IsLeaf As Boolean
        Public Property Indices As Integer()
        Public Property LeftChild As UMAP.Tree.RandomProjectionTreeNode
        Public Property RightChild As UMAP.Tree.RandomProjectionTreeNode
        Public Property Hyperplane As Single()
        Public Property Offset As Single
    End Class
End Module
