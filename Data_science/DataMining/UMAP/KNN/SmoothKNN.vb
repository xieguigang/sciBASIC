#Region "Microsoft.VisualBasic::9a2b507ee71fb4c11984fc9e3298a043, Data_science\DataMining\UMAP\KNN\SmoothKNN.vb"

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

    '   Total Lines: 155
    '    Code Lines: 123
    ' Comment Lines: 4
    '   Blank Lines: 28
    '     File Size: 6.01 KB


    '     Class SmoothKNN
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ComputeMembershipStrengths, moveKnn, SmoothKNNDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports stdNum = System.Math

Namespace KNN

    Friend Class SmoothKNN

        ReadOnly target As Double
        ReadOnly knn As KNNArguments
        ReadOnly meanDistances As Double
        ReadOnly distances As Double()()

        Sub New(distances As Double()(), knn As KNNArguments)
            Me.target = stdNum.Log(knn.k, 2) * knn.bandwidth
            Me.knn = knn
            Me.distances = distances
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
                Dim index = CInt(stdNum.Floor(localConnectivity))
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
                        pSum += stdNum.Exp(-(d / mid))
                    Else
                        pSum += 1.0
                    End If
                Next

                If stdNum.Abs(pSum - target) < Umap.SMOOTH_K_TOLERANCE Then
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
            ' TODO: Use Math.Log2 (when update framework to a version that supports it) or consider a pre-computed table
            Dim rho = New Double(distances.Length - 1) {}
            Dim result = New Double(distances.Length - 1) {}
            Dim parallelExec = distances _
                .AsParallel _
                .Select(Function(ithDistances, i)
                            Dim moveSmooth = moveKnn(ithDistances, localConnectivity, nIter)
                            Dim result_i = (i, moveSmooth.rho, moveSmooth.result)

                            Return result_i
                        End Function) _
                .OrderBy(Function(d) d.i) _
                .ToArray

            For i As Integer = 0 To distances.Length - 1
                result(i) = parallelExec(i).result
                rho(i) = parallelExec(i).rho
            Next

            Return (result, rho)
        End Function

        Friend Shared Function ComputeMembershipStrengths(knnIndices As Integer()(), knnDistances As Double()(), sigmas As Double(), rhos As Double()) As IndexVector
            Dim nSamples As Integer = knnIndices.Length
            Dim nNeighbors As Integer = knnIndices(0).Length
            Dim rows = New Integer(nSamples * nNeighbors - 1) {}
            Dim cols = New Integer(nSamples * nNeighbors - 1) {}
            Dim vals = New Double(nSamples * nNeighbors - 1) {}
            Dim val As Double

            For i = 0 To nSamples - 1
                For j = 0 To nNeighbors - 1
                    If knnIndices(i)(j) = -1 Then
                        ' We didn't get the full knn for i
                        Continue For
                    End If

                    If knnIndices(i)(j) = i Then
                        val = 0
                    ElseIf knnDistances(i)(j) - rhos(i) <= 0.0 Then
                        val = 1
                    Else
                        val = CSng(stdNum.Exp(-((knnDistances(i)(j) - rhos(i)) / sigmas(i))))
                    End If

                    rows(i * nNeighbors + j) = i
                    cols(i * nNeighbors + j) = knnIndices(i)(j)
                    vals(i * nNeighbors + j) = val
                Next
            Next

            Return New IndexVector(rows, cols, vals)
        End Function

    End Class
End Namespace
