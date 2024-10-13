#Region "Microsoft.VisualBasic::41a2d40d034a7b96e632a2820ab82685, Microsoft.VisualBasic.Core\src\Drawing\Math\Geometry\Moran.vb"

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

    '   Total Lines: 133
    '    Code Lines: 96 (72.18%)
    ' Comment Lines: 9 (6.77%)
    '    - Xml Docs: 77.78%
    ' 
    '   Blank Lines: 28 (21.05%)
    '     File Size: 4.71 KB


    '     Module Moran
    ' 
    '         Function: calc_moran, calc_spatial, distanceCalculate, normalize
    '         Class MoranSpatialTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' This function is useful for calculating Moran's I 
    ''' Without holding an entire distance matrix in memory
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/mcooper/moranfast/blob/master/src/calc_moran.cpp
    ''' </remarks>
    Public Module Moran

        Friend Function distanceCalculate(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Double
            Dim x = x1 - x2
            Dim y = y1 - y2

            If x = 0.0 AndAlso y = 0.0 Then
                Return 0
            Else
                Return 1 / std.Sqrt(x ^ 2 + y ^ 2)
            End If
        End Function

        Friend Function normalize(x As Double()) As Double()
            Dim x_bar = x.Average
            Dim x_norm As Double() = Math.SIMD.Subtract.f64_op_subtract_f64_scalar(x, x_bar)

            Return x_norm
        End Function

        Public Function calc_spatial(c1 As Double(), c2 As Double(), Optional parallel As Boolean = True) As (wij As Double()(), w As Double, S1 As Double, S2 As Double)
            Dim N = c1.Length
            Dim wij As Double()() = New Double(N - 1)() {}
            Dim w As Double
            Dim S1, S2 As Double
            Dim vec As New MoranSpatialTask(N) With {.wij = wij, .c1 = c1, .c2 = c2}

            If parallel Then
                Call vec.Run()
            Else
                Call vec.Solve()
            End If

            S1 = vec.S1.Sum
            S2 = vec.S2.Sum
            w = vec.w.Sum
            S1 = S1 / 2

            Return (wij, w, S1, S2)
        End Function

        Private Class MoranSpatialTask : Inherits VectorTask

            Public wij As Double()()
            Public c1 As Double(), c2 As Double()
            Public w() As Double
            Public S1(), S2() As Double

            Dim N As Integer

            Public Sub New(N As Integer)
                MyBase.New(N)

                w = New Double(N - 1) {}
                S1 = New Double(N - 1) {}
                S2 = New Double(N - 1) {}

                Me.N = N
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim w As Double
                Dim S1, S2 As Double

                For i As Integer = start To ends
                    Dim wi As Double() = New Double(N - 1) {}
                    Dim S2_a As Double = 0

                    For j As Integer = 0 To N - 1
                        Dim w_ij As Double = distanceCalculate(c1(i), c2(i), c1(j), c2(j))

                        wi(j) = w_ij
                        w += w_ij
                        S1 += (2 * w_ij) ^ 2
                        S2_a += w_ij
                    Next

                    S2 += (2 * S2_a) ^ 2
                    wij(i) = wi

                    Me.S1(i) = S1 : S1 = 0
                    Me.S2(i) = S2 : S2 = 0
                    Me.w(i) = w : w = 0
                Next
            End Sub
        End Class

        Public Function calc_moran(x As Double(), c1 As Double(), c2 As Double(), Optional parallel As Boolean = True) As (observed As Double, expected As Double, sd As Double)
            ' Easy variables to calculate
            Dim x_norm = normalize(x)
            Dim N = x.Length
            Dim denom = Math.SIMD.Multiply.f64_op_multiply_f64(x_norm, x_norm).Sum
            ' Variables to calculate through iteration
            Dim w As Double = 0
            Dim num As Double = 0
            Dim S1, S2 As Double
            Dim wij As Double()() = Nothing

            Call calc_spatial(c1, c2, parallel:=parallel).Set(wij, w, S1, S2)

            For i As Integer = 0 To N - 1
                For j As Integer = 0 To N - 1
                    Dim w_ij = wij(i)(j)
                    Dim w_x1_x2 = w_ij * x_norm(i) * x_norm(j)

                    num += w_x1_x2
                Next
            Next

            Dim ei = -(1 / (N - 1))
            Dim k = (Math.SIMD.Divide.f64_op_divide_f64_scalar(Math.SIMD.Exponent.f64_op_exponent_f64_scalar(x_norm, 4), N).Sum) / ((denom / N) ^ 2)
            Dim up = (N * (((N ^ 2) - 3 * N + 3) * S1 - N * S2 + 3 * (w ^ 2)) - k * (N * (N - 1) * S1 - 2 * N * S2 + 6 * (w ^ 2)))
            Dim down = ((N - 1) * (N - 2) * (N - 3) * (w ^ 2))
            Dim sd2 = up / down - (ei ^ 2)
            Dim sd = std.Sqrt(std.Abs(sd2))
            Dim moran = (N / w) * (num / denom)

            Return (moran, ei, sd)
        End Function
    End Module
End Namespace
