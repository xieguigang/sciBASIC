#Region "Microsoft.VisualBasic::419f9e77e3e738ccf86d921ddb2e0743, sciBASIC#\Data_science\DataMining\UMAP\KNN\SmoothKNN.vb"

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

    '   Total Lines: 130
    '    Code Lines: 100
    ' Comment Lines: 4
    '   Blank Lines: 26
    '     File Size: 5.19 KB


    '     Class SmoothKNN
    ' 
    '         Function: ComputeMembershipStrengths, SmoothKNNDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports stdNum = System.Math

Namespace KNN

    Friend Class SmoothKNN

        Friend Shared Function SmoothKNNDistance(distances As Double()(), knn As KNNArguments) As (sigmas As Double(), rhos As Double())
            Dim k As Integer = knn.k
            Dim localConnectivity As Double = knn.localConnectivity
            Dim nIter As Integer = knn.nIter
            Dim bandwidth As Double = knn.bandwidth

            ' TODO: Use Math.Log2 (when update framework to a version that supports it) or consider a pre-computed table
            Dim target = stdNum.Log(k, 2) * bandwidth
            Dim rho = New Double(distances.Length - 1) {}
            Dim result = New Double(distances.Length - 1) {}

            For i As Integer = 0 To distances.Length - 1
                Dim lo = 0F
                Dim hi = Single.MaxValue
                Dim mid = 1.0F

                ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
                Dim ithDistances = distances(i)
                Dim nonZeroDists = ithDistances.Where(Function(d) d > 0).ToArray()

                If nonZeroDists.Length >= localConnectivity Then
                    Dim index = CInt(stdNum.Floor(localConnectivity))
                    Dim interpolation = localConnectivity - index

                    If index > 0 Then
                        rho(i) = nonZeroDists(index - 1)

                        If interpolation > Umap.SMOOTH_K_TOLERANCE Then
                            rho(i) += interpolation * (nonZeroDists(index) - nonZeroDists(index - 1))
                        End If
                    Else
                        rho(i) = interpolation * nonZeroDists(0)
                    End If
                ElseIf nonZeroDists.Length > 0 Then
                    rho(i) = nonZeroDists.Max
                End If

                For n As Integer = 0 To nIter - 1
                    Dim psum = 0.0

                    For j = 1 To distances(i).Length - 1
                        Dim d = distances(i)(j) - rho(i)

                        If d > 0 Then
                            psum += stdNum.Exp(-(d / mid))
                        Else
                            psum += 1.0
                        End If
                    Next

                    If stdNum.Abs(psum - target) < Umap.SMOOTH_K_TOLERANCE Then
                        Exit For
                    End If

                    If psum > target Then
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

                result(i) = mid

                ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
                If rho(i) > 0 Then
                    Dim meanIthDistances = ithDistances.Average

                    If result(i) < Umap.MIN_K_DIST_SCALE * meanIthDistances Then
                        result(i) = Umap.MIN_K_DIST_SCALE * meanIthDistances
                    End If
                Else
                    Dim meanDistances = distances.Select(Function(d) d.Average).Average

                    If result(i) < Umap.MIN_K_DIST_SCALE * meanDistances Then
                        result(i) = Umap.MIN_K_DIST_SCALE * meanDistances
                    End If
                End If
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
