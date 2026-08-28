#Region "Microsoft.VisualBasic::091995752ea7f0af68d751f027948c4c, Data_science\DataMining\UMAP\KNN\KNearestNeighbour.vb"

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

    '   Total Lines: 100
    '    Code Lines: 54 (54.00%)
    ' Comment Lines: 29 (29.00%)
    '    - Xml Docs: 82.76%
    ' 
    '   Blank Lines: 17 (17.00%)
    '     File Size: 4.21 KB


    '     Class KNearestNeighbour
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FindNeighbors, NearestNeighbors, Round
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.DataMining.UMAP.Tree
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports System.Threading.Tasks
Imports i32 = Microsoft.VisualBasic.Language.i32
Imports std = System.Math

Namespace KNN

    ''' <summary>
    ''' K Nearest Neighbour Search
    ''' 
    ''' Uses a kd-tree to find the p number of near neighbours for each point in an input/output dataset.
    ''' 
    ''' Use the nn2 function from the RANN package, utilizes the Approximate Near Neighbor (ANN) C++ library, 
    ''' which can give the exact near neighbours or (as the name suggests) approximate near neighbours 
    ''' to within a specified error bound. For more information on the ANN library please 
    ''' visit http://www.cs.umd.edu/~mount/ANN/.
    ''' </summary>
    Public Class KNearestNeighbour

        ReadOnly m_k As Integer
        ReadOnly m_distanceFn As DistanceCalculation
        ReadOnly m_random As IProvideRandomValues
        ''' <summary>
        ''' the extended KNN arguments, the adaptive formula is used when 
        ''' the corresponding argument value is not configured.
        ''' </summary>
        ReadOnly m_args As KNNArguments
        ReadOnly m_parallelism As ParallelConfig

        Sub New(knn As Integer, Optional distanceFn As DistanceCalculation = Nothing, Optional random As IProvideRandomValues = Nothing)
            m_k = knn
            m_distanceFn = If(distanceFn, AddressOf DistanceFunctions.Cosine)
            m_random = If(random, DefaultRandomGenerator.Instance)
            m_args = New KNNArguments(knn)
            m_parallelism = ParallelConfig.Sequential
        End Sub

        Sub New(args As KNNArguments, Optional distanceFn As DistanceCalculation = Nothing, Optional random As IProvideRandomValues = Nothing)
            m_k = args.k
            m_distanceFn = If(distanceFn, AddressOf DistanceFunctions.Cosine)
            m_random = If(random, DefaultRandomGenerator.Instance)
            m_args = args
            m_parallelism = If(args.parallelism, ParallelConfig.Sequential)
        End Sub

        ''' <summary>
        ''' Compute the ``nNeighbors`` nearest points for each data point in ``X`` - this may be exact, but more likely is approximated via nearest neighbor descent.
        ''' </summary>
        Friend Function NearestNeighbors(x As Double()()) As KNNState
            Dim metricNNDescent = New NNDescent(m_distanceFn, m_random)

            Call VBDebugger.EchoLine("Create NNDescent")

            ' the tree count/leaf size/iteration number are all configurable now,
            ' the adaptive formula is only used as the fallback of the 
            ' un-configured argument value
            Dim nTrees As Integer = m_args.GetNumOfTrees(x.Length)
            Dim nIters As Integer = m_args.GetDescentIters(x.Length)

            Call VBDebugger.EchoLine("Set Iteration Parameters")

            Dim leafSize As Integer = m_args.GetLeafSize()
            Dim i As i32 = Scan0
            Dim rpForest = New FlatTree(nTrees - 1) {}

            Call VBDebugger.EchoLine($"make {nTrees} trees...")

            ' the rp-tree forest could only be built in parallel when the 
            ' random source is thread safe, otherwise the hyperplane of 
            ' each tree will be broken by the data race
            If m_parallelism.CanParallel(nTrees) AndAlso m_random.IsThreadSafe Then
                Dim opt As New ParallelOptions With {
                    .MaxDegreeOfParallelism = m_parallelism.EffectiveDegree(nTrees)
                }

                Call System.Threading.Tasks.Parallel.For(
                    fromInclusive:=0,
                    toExclusive:=nTrees,
                    parallelOptions:=opt,
                    body:=Sub(n)
                              ' x is readonly in make tree
                              ' progress can be parallel
                              rpForest(n) = Tree.FlattenTree(Tree.MakeTree(x, leafSize, n, m_random), leafSize)
                          End Sub)
            Else
                For n As Integer = 0 To nTrees - 1
                    ' x is readonly in make tree
                    ' progress can be parallel
                    rpForest(n) = Tree.FlattenTree(Tree.MakeTree(x, leafSize, n, m_random), leafSize)
                Next
            End If

            Dim leafArray As Integer()() = Tree.MakeLeafArray(rpForest, m_parallelism)

            ' Handle python3 rounding down from 0.5 discrpancy
            Return metricNNDescent.MakeNNDescent(
                data:=x,
                leafArray:=leafArray,
                nNeighbors:=m_k,
                nIters:=nIters,
                maxCandidates:=m_args.maxCandidates,
                delta:=m_args.delta,
                rho:=m_args.rho,
                rpTreeInit:=m_args.rpTreeInit,
                parallelism:=m_parallelism)
        End Function

        ''' <summary>
        ''' K Nearest Neighbour Search
        ''' </summary>
        ''' <param name="data">matrix; input data matrix</param>
        ''' <param name="k">integer; number of nearest neighbours</param>
        ''' <returns>
        ''' a n-by-k matrix of neighbor indices
        ''' </returns>
        Public Shared Function FindNeighbors(data As NumericMatrix, k As Integer,
                                             Optional distanceFn As DistanceCalculation = Nothing,
                                             Optional random As IProvideRandomValues = Nothing,
                                             Optional args As KNNArguments = Nothing) As KNNState

            If args.k <= 0 Then
                Return New KNearestNeighbour(k, distanceFn, random).NearestNeighbors(data.Array)
            Else
                Return New KNearestNeighbour(args, distanceFn, random).NearestNeighbors(data.Array)
            End If
        End Function
    End Class
End Namespace
