#Region "Microsoft.VisualBasic::4f7bf90b8852e08f355af24f8db5a35f, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Distance\PearsonCorrelation.vb"

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

    '   Total Lines: 36
    '    Code Lines: 29 (80.56%)
    ' Comment Lines: 3 (8.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (11.11%)
    '     File Size: 1.67 KB


    '     Class PearsonCorrelation
    ' 
    '         Function: ComputeDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes the euclidean distance between two points, d = 1 - (cov(X,Y) / (std_dev(X) * std_dev(Y)))
    ''' </summary>
    Public Class PearsonCorrelation
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim len As Integer = std.Min(attributesOne.Length, attributesTwo.Length)

            If len = 0 Then Return 1

            ' 均值用 SIMD 归约（旧实现是标量累加）
            Dim meanOne As Double = SimdReduce.Sum(attributesOne, 0, attributesOne.Length) / attributesOne.Length
            Dim meanTwo As Double = SimdReduce.Sum(attributesTwo, 0, attributesTwo.Length) / attributesTwo.Length
            Dim covariance As Double = 0
            Dim standardDeviationOne As Double = 0
            Dim standardDeviationTwo As Double = 0

            ' 中心化乘积仍然按 4 路向量累加：三个独立累加器各自打断浮点依赖链
            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim cov0, cov1, cov2, cov3 As Vector(Of Double)
                Dim sd10, sd11, sd12, sd13 As Vector(Of Double)
                Dim sd20, sd21, sd22, sd23 As Vector(Of Double)

                cov0 = Vector(Of Double).Zero : cov1 = cov0 : cov2 = cov0 : cov3 = cov0
                sd10 = cov0 : sd11 = cov0 : sd12 = cov0 : sd13 = cov0
                sd20 = cov0 : sd21 = cov0 : sd22 = cov0 : sd23 = cov0

                Dim splatOne As New Vector(Of Double)(meanOne)
                Dim splatTwo As New Vector(Of Double)(meanTwo)

                Do While i <= len - step4
                    Dim x0 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesOne, i), splatOne)
                    Dim y0 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesTwo, i), splatTwo)
                    Dim x1 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesOne, i + count), splatOne)
                    Dim y1 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesTwo, i + count), splatTwo)
                    Dim x2 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesOne, i + count * 2), splatOne)
                    Dim y2 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesTwo, i + count * 2), splatTwo)
                    Dim x3 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesOne, i + count * 3), splatOne)
                    Dim y3 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(attributesTwo, i + count * 3), splatTwo)

                    cov0 = Vector.Add(Of Double)(cov0, Vector.Multiply(Of Double)(x0, y0))
                    cov1 = Vector.Add(Of Double)(cov1, Vector.Multiply(Of Double)(x1, y1))
                    cov2 = Vector.Add(Of Double)(cov2, Vector.Multiply(Of Double)(x2, y2))
                    cov3 = Vector.Add(Of Double)(cov3, Vector.Multiply(Of Double)(x3, y3))

                    sd10 = Vector.Add(Of Double)(sd10, Vector.Multiply(Of Double)(x0, x0))
                    sd11 = Vector.Add(Of Double)(sd11, Vector.Multiply(Of Double)(x1, x1))
                    sd12 = Vector.Add(Of Double)(sd12, Vector.Multiply(Of Double)(x2, x2))
                    sd13 = Vector.Add(Of Double)(sd13, Vector.Multiply(Of Double)(x3, x3))

                    sd20 = Vector.Add(Of Double)(sd20, Vector.Multiply(Of Double)(y0, y0))
                    sd21 = Vector.Add(Of Double)(sd21, Vector.Multiply(Of Double)(y1, y1))
                    sd22 = Vector.Add(Of Double)(sd22, Vector.Multiply(Of Double)(y2, y2))
                    sd23 = Vector.Add(Of Double)(sd23, Vector.Multiply(Of Double)(y3, y3))

                    i += step4
                Loop

                covariance = Vector.Dot(Of Double)(Vector.Add(Of Double)(Vector.Add(Of Double)(cov0, cov1), Vector.Add(Of Double)(cov2, cov3)), ones)
                standardDeviationOne = Vector.Dot(Of Double)(Vector.Add(Of Double)(Vector.Add(Of Double)(sd10, sd11), Vector.Add(Of Double)(sd12, sd13)), ones)
                standardDeviationTwo = Vector.Dot(Of Double)(Vector.Add(Of Double)(Vector.Add(Of Double)(sd20, sd21), Vector.Add(Of Double)(sd22, sd23)), ones)
            End If

            For k As Integer = i To len - 1
                Dim dx As Double = attributesOne(k) - meanOne
                Dim dy As Double = attributesTwo(k) - meanTwo

                covariance += dx * dy
                standardDeviationOne += dx * dx
                standardDeviationTwo += dy * dy
            Next

            Return std.Max(0, 1 - covariance / std.Sqrt(standardDeviationOne * standardDeviationTwo))
        End Function
    End Class
End Namespace
