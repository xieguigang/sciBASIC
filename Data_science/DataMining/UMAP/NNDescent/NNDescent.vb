#Region "Microsoft.VisualBasic::acf853cf8a93d65166b3cb3d3c44e36c, Data_science\DataMining\UMAP\Components\NNDescent.vb"

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

'   Total Lines: 222
'    Code Lines: 156 (70.27%)
' Comment Lines: 24 (10.81%)
'    - Xml Docs: 95.83%
' 
'   Blank Lines: 42 (18.92%)
'     File Size: 8.95 KB


' Interface NNDescentFn
' 
'     Function: NNDescent
' 
' Class NNDescent
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: MakeNNDescent, NNDescentLoop, NNDescentLoopPar, rpTreeInit
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.DataMining.UMAP.KNN
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

''' <summary>
''' UMAP (Uniform Manifold Approximation and Projection) is a nonlinear dimensionality reduction
''' technique widely used in data science and machine learning to visualize high-dimensional
''' datasets. NNDescent (Nearest Neighbor Descent) is a crucial step in the UMAP algorithm that
''' efficiently finds nearest neighbors in high-dimensional spaces.
'''
''' Understanding the proximity between data points is essential for preserving the local structure
''' of the data before dimensionality reduction. NNDescent is an algorithm for fast approximate 
''' nearest neighbor search in high-dimensional spaces. It iteratively refines estimates of the 
''' proximity between points to build a nearest neighbor graph that captures the intrinsic local
''' structure of the data.
''' 
''' The NNDescent algorithm works as follows:
''' 
''' 1. Initialization: Randomly select some points as initial neighbors and establish an initial 
'''    nearest neighbor graph.
''' 2. Iterative Search: Update the neighbor sets for each point through multiple rounds of iteration.
'''    In each iteration, the algorithm checks whether the current neighbors of a point are truly
'''    the closest and attempts to find closer neighbors if not.
''' 3. Candidate Set Construction: To find closer neighbors, the algorithm uses a technique 
'''    called “candidate set construction,” which speculatively generates potential closer neighbors
'''    based on the current known neighbors.
''' 4. Distance Calculation: The algorithm computes the distance from the current point to all 
'''    points in its candidate set and updates the neighbor list accordingly.
''' 5. Pruning: During iteration, the algorithm prunes, or removes, neighbors that are too distant
'''    to manage the size of the neighbor sets and reduce computational load.
''' 6. Convergence: The algorithm stops iterating if no better neighbors are found for several 
'''    consecutive iterations or if a predefined number of iterations is reached.
''' 
''' The advantage of NNDescent is its ability to quickly find a good approximation of nearest 
''' neighbors, and it is more efficient than other nearest neighbor search algorithms, such as 
''' k-d trees or ball trees, especially when dealing with high-dimensional and large-scale datasets.
''' In UMAP, NNDescent is used to construct the local structure of the data, which is then 
''' preserved during the dimensionality reduction process.
''' 
''' In summary, NNDescent is an algorithm used in UMAP for efficiently constructing the local 
''' structure of high-dimensional data by iteratively optimizing the search for nearest neighbors, 
''' providing a crucial foundation for the subsequent dimensionality reduction steps.
''' </summary>
Friend Class NNDescent : Implements NNDescentFn

    Public ReadOnly distanceFn As DistanceCalculation
    Public ReadOnly random As IProvideRandomValues

    Sub New(distanceFn As DistanceCalculation, random As IProvideRandomValues)
        Me.distanceFn = distanceFn
        Me.random = random
    End Sub

    ''' <summary>
    ''' Create a version of nearest neighbor descent.
    ''' </summary>
    Public Function MakeNNDescent(data As Double()(), leafArray As Integer()(), nNeighbors As Integer,
                                  Optional nIters As Integer = 10,
                                  Optional maxCandidates As Integer = 50,
                                  Optional delta As Double = 0.001F,
                                  Optional rho As Double = 0.5F,
                                  Optional rpTreeInit As Boolean = True) As KNNState Implements NNDescentFn.NNDescent

        Dim nVertices As Integer = data.Length
        Dim currentGraph As Heap = Heaps.MakeHeap(nVertices, nNeighbors)
        Dim d As Double

        Call VBDebugger.EchoLine("[MakeNNDescent] Start sample rejection loop...")

        For Each i As Integer In Tqdm.Range(0, nVertices)
            Dim indices As Integer() = Utils.RejectionSample(nNeighbors, data.Length, random)

            For j As Integer = 0 To indices.Length - 1
                d = distanceFn(data(i), data(indices(j)))

                Call Heaps.HeapPush(currentGraph, i, d, indices(j), 1)
                Call Heaps.HeapPush(currentGraph, indices(j), d, i, 1)
            Next
        Next

        If rpTreeInit Then
            Dim rpTree As New RPTree(leafArray) With {
                .wrap = Me,
                .data = data,
                .currentGraph = currentGraph
            }

            rpTree.Run()
            currentGraph = rpTree.currentGraph
        End If

        Dim candidateNeighbors As Heap
        Dim c As Integer
        Dim dataSize As Integer = data.Length
        Dim nnDescentLoopPar As New NNDescentLoop(nVertices) With {
            .currentGraph = currentGraph,
            .data = data,
            .maxCandidates = maxCandidates,
            .rho = rho,
            .wrap = Me
        }

        Dim t0 As DateTime = Now

        ' 这里是限速步骤
        For n As Integer = 0 To nIters - 1
            candidateNeighbors = Heaps.BuildCandidates(currentGraph, nVertices, nNeighbors, maxCandidates, random)
            nnDescentLoopPar.candidateNeighbors = candidateNeighbors
            nnDescentLoopPar.Reset()
            nnDescentLoopPar.Run()
            c = nnDescentLoopPar.c.Sum

            If c <= delta * nNeighbors * dataSize Then
                Exit For
            End If
        Next

        Dim t1 As DateTime = Now
        Dim span = t1 - t0

        Call VBDebugger.EchoLine($"Nearest Neighbor Descent: {StringFormats.ReadableElapsedTime(span)}")

        Return Heaps.DeHeapSort(currentGraph)
    End Function
End Class
