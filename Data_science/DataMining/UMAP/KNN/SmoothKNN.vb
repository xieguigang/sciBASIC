#Region "Microsoft.VisualBasic::4a586db191356804d34ac278ffcba2f3, Data_science\DataMining\UMAP\KNN\SmoothKNN.vb"

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

    '   Total Lines: 160
    '    Code Lines: 125 (78.12%)
    ' Comment Lines: 4 (2.50%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 31 (19.38%)
    '     File Size: 6.13 KB


    '     Class SmoothKNN
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ComputeMembershipStrengths, moveKnn, SmoothKNNDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace KNN

    Friend Class SmoothKNN

        ReadOnly target As Double
        ReadOnly knn As KNNArguments
        ReadOnly meanDistances As Double
        ReadOnly distances As Double()()
        ReadOnly parallelism As ParallelConfig

        Sub New(distances As Double()(), knn As KNNArguments)
            Me.target = std.Log(knn.k, 2) * knn.bandwidth
            Me.knn = knn
            Me.distances = distances
            Me.parallelism = If(knn.parallelism, ParallelConfig.Sequential)
            Me.meanDistances = Aggregate d As Double()
                               In distances
                               Let md As Double = d.Average
                               Into Average(md)
        End Sub

        Private Function moveKnn(ithDistances As Double(), localConnectivity As Double, nIter As Integer) As (rho As Double, result As Double)
            Dim lo = 0F
            Dim hi = Single.MaxValue
            Dim mid = 1.0F
            ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
            Dim nonZeroDists = ithDistances.Where(Function(d) d > 0).ToArray()
            Dim rho_i, result_i As Double

            If nonZeroDists.Length >= localConnectivity Then
                Dim index = CInt(std.Floor(localConnectivity))
                Dim interpolation = localConnectivity - index

                If index > 0 Then
                    rho_i = nonZeroDists(index - 1)

                    If interpolation > Umap.SMOOTH_K_TOLERANCE Then
                        rho_i += interpolation * (nonZeroDists(index) - nonZeroDists(index - 1))
                    End If
                Else
                    rho_i = interpolation * nonZeroDists(0)
                End If
            ElseIf nonZeroDists.Length > 0 Then
                rho_i = nonZeroDists.Max
            End If

            For n As Integer = 0 To nIter - 1
                Dim pSum As Double = 0.0

                For j = 1 To ithDistances.Length - 1
                    Dim d = ithDistances(j) - rho_i

                    If d > 0 Then
                        pSum += std.Exp(-(d / mid))
                    Else
                        pSum += 1.0
                    End If
                Next

                If std.Abs(pSum - target) < Umap.SMOOTH_K_TOLERANCE Then
                    Exit For
                End If

                If pSum > target Then
                    hi = mid
                    mid = (lo + hi) / 2
                Else
                    lo = mid

                    If hi = Single.MaxValue Then
                        mid *= 2
                    Else
                        mid = (lo + hi) / 2
                    End If
                End If
            Next

            result_i = mid

            ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
            If rho_i > 0 Then
                Dim meanIthDistances = ithDistances.Average

                If result_i < Umap.MIN_K_DIST_SCALE * meanIthDistances Then
                    result_i = Umap.MIN_K_DIST_SCALE * meanIthDistances
                End If
            Else
                If result_i < Umap.MIN_K_DIST_SCALE * meanDistances Then
                    result_i = Umap.MIN_K_DIST_SCALE * meanDistances
                End If
            End If

            Return (rho_i, result_i)
        End Function

        Public Function SmoothKNNDistance() As (sigmas As Double(), rhos As Double())
            Dim localConnectivity As Double = knn.localConnectivity
            Dim nIter As Integer = knn.nIter

            Call VBDebugger.EchoLine("SmoothKNNDistance...")

            ' TODO: Use Math.Log2 (when update framework to a version that supports it) or consider a pre-computed table
            Dim rho = New Double(distances.Length - 1) {}
            Dim result = New Double(distances.Length - 1) {}
            Dim n As Integer = distances.Length
            Dim degree As Integer = parallelism.EffectiveDegree(n)

            ' note about: the OrderBy call of the previous implementation is 
            ' a redundant O(n*log(n)) sort, the result of each sample is 
            ' written into its own slot so that the sort is not required.
            If degree > 1 Then
                Call System.Threading.Tasks.Parallel.For(
                    fromInclusive:=0,
                    toExclusive:=n,
                    parallelOptions:=New ParallelOptions With {.MaxDegreeOfParallelism = degree},
                    body:=Sub(i)
                              Dim moveSmooth = moveKnn(distances(i), localConnectivity, nIter)

                              result(i) = moveSmooth.result
                              rho(i) = moveSmooth.rho
                          End Sub)
            Else
                For i As Integer = 0 To n - 1
                    Dim moveSmooth = moveKnn(distances(i), localConnectivity, nIter)

                    result(i) = moveSmooth.result
                    rho(i) = moveSmooth.rho
                Next
            End If

            Return (result, rho)
        End Function

        ''' <summary>
        ''' Compute the membership strength of each edge of the knn graph
        ''' </summary>
        ''' <param name="parallelism">
        ''' the parallelism configuration. each sample writes its own slot 
        ''' of the result vector, so that this procedure is a race free 
        ''' parallel workload.
        ''' </param>
        Friend Shared Function ComputeMembershipStrengths(knnIndices As Integer()(),
                                                          knnDistances As Double()(),
                                                          sigmas As Double(),
                                                          rhos As Double(),
                                                          Optional parallelism As ParallelConfig = Nothing) As IndexVector

            Dim nSamples As Integer = knnIndices.Length
            Dim nNeighbors As Integer = knnIndices(0).Length
            Dim rows = New Integer(nSamples * nNeighbors - 1) {}
            Dim cols = New Integer(nSamples * nNeighbors - 1) {}
            Dim vals = New Double(nSamples * nNeighbors - 1) {}
            Dim config As ParallelConfig = If(parallelism, ParallelConfig.Sequential)
            Dim degree As Integer = config.EffectiveDegree(nSamples)

            Call VBDebugger.EchoLine($"ComputeMembershipStrengths... [parallel: {degree}]")

            Dim solve As Action(Of Integer) =
                Sub(i)
                    For j = 0 To nNeighbors - 1
                        Dim val As Double

                        If knnIndices(i)(j) = -1 Then
                            ' We didn't get the full knn for i
                            Continue For
                        End If

                        If knnIndices(i)(j) = i Then
                            val = 0
                        ElseIf knnDistances(i)(j) - rhos(i) <= 0.0 Then
                            val = 1
                        Else
                            val = CSng(std.Exp(-((knnDistances(i)(j) - rhos(i)) / sigmas(i))))
                        End If

                        rows(i * nNeighbors + j) = i
                        cols(i * nNeighbors + j) = knnIndices(i)(j)
                        vals(i * nNeighbors + j) = val
                    Next
                End Sub

            If degree > 1 Then
                Call System.Threading.Tasks.Parallel.For(
                    fromInclusive:=0,
                    toExclusive:=nSamples,
                    parallelOptions:=New ParallelOptions With {.MaxDegreeOfParallelism = degree},
                    body:=solve)
            Else
                For i As Integer = 0 To nSamples - 1
                    Call solve(i)
                Next
            End If

            Return New IndexVector(rows, cols, vals)
        End Function

    End Class
End Namespace
