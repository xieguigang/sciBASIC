#Region "Microsoft.VisualBasic::ffd6c99ce5d92930a25749aac38bdd24, Data_science\Mathematica\Math\Math\Spline\PolynomialNewton.vb"

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

    '   Total Lines: 106
    '    Code Lines: 61 (57.55%)
    ' Comment Lines: 27 (25.47%)
    '    - Xml Docs: 74.07%
    ' 
    '   Blank Lines: 18 (16.98%)
    '     File Size: 3.76 KB


    '     Module PolynomialNewton
    ' 
    '         Function: Newton, (+2 Overloads) NewtonPolynomial
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Interpolation

    ''' <summary>
    ''' 似乎对三角函数不起作用
    ''' </summary>
    Public Module PolynomialNewton

        ''' <summary>
        ''' PolynomialNewton Spline
        ''' </summary>
        ''' <param name="points">序列之中的<see cref="PointF.X"/>必须要是单调递增或者递减的</param>
        ''' <param name="divided%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NewtonPolynomial(points As IEnumerable(Of PointF), Optional divided% = 10) As PointF()
            With points.ToArray
                Dim X As Vector = .X
                ' 在这里不可以使用Linq，因为需要将原始序列之中的X点插进入向量之中
                Dim xi As New List(Of Double)

                For Each t In X.SlideWindows(2)
                    Dim x1 = t(0)
                    Dim x2 = t(1)
                    Dim sequence = seq(x1, x2, (x2 - x1) / divided).ToArray

                    ' 为了防止重复出现元素，在这里将最后一个点删除
                    ' 然后在最末尾添加上即可
                    xi += sequence.Take(sequence.Length - 1)
                Next

                ' 添加上最后一个元素
                xi += X.Last

                Return xi.AsVector.NewtonPolynomial(.ByRef)
            End With
        End Function

        <Extension>
        Public Function NewtonPolynomial(x As Vector, points As IEnumerable(Of PointF)) As PointF()
            With points.ToArray
                Return Newton(x, .X, .Y) _
                    .Select(Function(y, i)
                                Return New PointF(x(i), y)
                            End Function) _
                    .ToArray
            End With
        End Function

        ''' <summary>
        ''' 使用牛顿多项式插值计算出给定的X点<paramref name="x1"/>所对应的Y值
        ''' <paramref name="x"/>和<paramref name="y"/>是已知的实验值点
        ''' </summary>
        ''' <param name="x1#"></param>
        ''' <param name="x#"></param>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <paramref name="x"/> and <paramref name="y"/> should be length equals
        ''' </remarks>
        ''' 
        <Extension>
        Public Function Newton(x1 As Vector, x#(), y#()) As Vector
            Dim result As New Vector(y(0), x1.Dim)
            Dim node As New Vector(x1.Dim)
            Dim denominator# = 1.0
            Dim n% = x.Length

            If x.Length <> y.Length OrElse x.Length = 0 Then
                Throw New ArgumentException("Invalid vector length value!")
            End If

            For i As Integer = 1 To n - 1
                node *= 0.0

                For j As Integer = 0 To i

                    ' f[x_0,…,x_n ]
                    denominator = 1.0

                    For k As Integer = 0 To i
                        If k <> j Then
                            denominator *= (x(j) - x(k))
                        End If
                    Next

                    node += y(j) / denominator
                Next

                For k As Integer = 0 To i - 1
                    ' (x-x_0) … (x-x_(i-1) )
                    node *= (x1 - x(k))
                Next

                result += node
            Next

            Return result
        End Function
    End Module
End Namespace
