Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' In the context of the NNDescent algorithm, an rpTree (Random Projection Tree)
''' is a data structure used to index high-dimensional data points for efficient 
''' nearest neighbor search. Random Projection Trees are a type of metric tree,
''' which is a tree-like data structure that partitions data points to facilitate 
''' fast queries for nearest neighbors.
'''
''' The idea behind an rpTree is to project the data points onto a random hyperplane 
''' and recursively divide the points into two subsets based on which side of the 
''' hyperplane they fall. This process is repeated for each subset, creating a binary
''' tree where each node represents a hyperplane and the two child nodes represent
''' the subsets of points on either side of the hyperplane.
''' 
''' Here’s how an rpTree works in the context of NNDescent:
''' 
''' 1. Random Hyperplane: A random hyperplane is chosen in the high-dimensional space. 
'''    This hyperplane is defined by a random vector, and the projection of each data 
'''    point onto this vector determines which side of the hyperplane the point falls.
''' 2. Partitioning: The data points are divided into two groups based on their 
'''    projection onto the random hyperplane. Points that project to one side of the 
'''    hyperplane are assigned to one child node, while points on the other side are
'''    assigned to the other child node.
''' 3. Recursion: The process is repeated recursively for each child node, creating 
'''    a hierarchy of partitions. At each level of the tree, the data is further 
'''    divided by new random hyperplanes until a stopping criterion is met (e.g., a
'''    maximum tree depth is reached or there are fewer points than a predefined 
'''    threshold).
''' 4. Nearest Neighbor Search: When searching for the nearest neighbors of a query 
'''    point, the rpTree is traversed. Starting from the root, the algorithm determines 
'''    which side of each hyperplane the query point falls and traverses down the tree 
'''    accordingly. At each node, the algorithm can potentially prune branches that are
'''    guaranteed to not contain any points closer than the current nearest neighbors 
'''    found.
''' 5. Candidate Generation: As the algorithm traverses the rpTree, it collects points 
'''    from each leaf node that the query point passes through. These points are 
'''    considered candidate nearest neighbors.
''' 6. Refinement: The final set of nearest neighbors is refined by comparing the 
'''    query point to all candidate points collected from the rpTree. This step ensures
'''    that the nearest neighbors found are indeed the closest points in the dataset.
''' 
''' The use of rpTrees in NNDescent allows for efficient and approximate nearest 
''' neighbor search in high-dimensional spaces. By using random hyperplanes, rpTrees 
''' avoid the curse of dimensionality that affects other types of spatial data structures,
''' such as k-d trees, which struggle as the number of dimensions increases. The random 
''' nature of rpTrees makes them more robust to the high-dimensional data distributions
''' that are common in real-world datasets.
''' </summary>
Friend Class RPTree : Inherits VectorTask

    Public data As Double()()
    Public currentGraph As Heap
    Public wrap As NNDescent

    ReadOnly leafArray As Integer()()

    Public Sub New(leafArray As Integer()())
        MyBase.New(leafArray.Length)
        Me.leafArray = leafArray
    End Sub

    Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
        For n As Integer = start To ends
            ' nth leaf in projection tree
            Call rpTreeInit(n)
        Next
    End Sub

    Private Sub rpTreeInit(n As Integer)
        Dim d As Double

        For i As Integer = 0 To leafArray(n).Length - 1
            If leafArray(n)(i) < 0 Then
                Exit For
            End If

            For j = i + 1 To leafArray(n).Length - 1
                If leafArray(n)(j) < 0 Then
                    Exit For
                Else
                    d = wrap.distanceFn(data(leafArray(n)(i)), data(leafArray(n)(j)))
                End If

                Call Heaps.HeapPush(currentGraph, leafArray(n)(i), d, leafArray(n)(j), 1)
                Call Heaps.HeapPush(currentGraph, leafArray(n)(j), d, leafArray(n)(i), 1)
            Next
        Next
    End Sub
End Class
