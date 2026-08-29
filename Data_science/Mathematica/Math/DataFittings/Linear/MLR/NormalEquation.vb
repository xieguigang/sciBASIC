#Region "Microsoft.VisualBasic::4ef5940b61e15649557f84fe76061cec, Data_science\Mathematica\Math\DataFittings\Linear\MLR\NormalEquation.vb"

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

    '   Total Lines: 55
    '    Code Lines: 39 (70.91%)
    ' Comment Lines: 7 (12.73%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 9 (16.36%)
    '     File Size: 1.72 KB


    '     Module NormalEquation
    ' 
    '         Function: LinearRegression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Multivariate

    Public Module NormalEquation

        ''' <summary>
        ''' 多元线性回归（最小二乘法）
        ''' β = (X'X)^(-1) X'y
        ''' </summary>
        Public Function LinearRegression(X As Double(,), y As Double(), nS As Integer, nP As Integer) As Double()
            Dim p1 As Integer = nP + 1  ' 含截距

            ' X'X
            Dim XtX As Double(,) = New Double(p1 - 1, p1 - 1) {}
            For i = 0 To p1 - 1
                For j = 0 To p1 - 1
                    Dim sum As Double = 0
                    For k = 0 To nS - 1
                        sum += X(k, i) * X(k, j)
                    Next
                    XtX(i, j) = sum
                Next
            Next

            ' X'y
            Dim Xty As Double() = New Double(p1 - 1) {}
            For i = 0 To p1 - 1
                Dim sum As Double = 0
                For k = 0 To nS - 1
                    sum += X(k, i) * y(k)
                Next
                Xty(i) = sum
            Next

            ' 求解 β = (X'X)^(-1) X'y
            Dim invXtX As Double(,) = MatrixOps.Inverse(XtX, strict:=True, throwSingularity:=False)

            If invXtX Is Nothing Then
                Return Nothing
            End If

            Dim beta As Double() = New Double(p1 - 1) {}
            For i = 0 To p1 - 1
                Dim sum As Double = 0
                For j = 0 To p1 - 1
                    sum += invXtX(i, j) * Xty(j)
                Next
                beta(i) = sum
            Next

            Return beta
        End Function
    End Module
End Namespace
