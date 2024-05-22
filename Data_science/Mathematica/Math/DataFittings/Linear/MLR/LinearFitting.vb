#Region "Microsoft.VisualBasic::b90cb22a2d18e00d87e83e68ef6df784, Data_science\Mathematica\Math\DataFittings\Linear\MLR\LinearFitting.vb"

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

    '   Total Lines: 101
    '    Code Lines: 56 (55.45%)
    ' Comment Lines: 35 (34.65%)
    '    - Xml Docs: 88.57%
    ' 
    '   Blank Lines: 10 (9.90%)
    '     File Size: 3.24 KB


    '     Module LinearFittingAlgorithm
    ' 
    '         Function: ConfidenceInterval, CurveScale, left, (+2 Overloads) LinearFitting, right
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Multivariate

    Public Module LinearFittingAlgorithm

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LinearFitting(x As Double(,), y#()) As MLRFit
            Return New NumericMatrix(x.RowIterator.ToArray).LinearFitting(y)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x">
        ''' A matrix like:
        ''' 
        ''' ```
        ''' x1  x2  x3  x4
        '''  a   b   c   d
        '''  a   b   c   d
        '''  a   b   c   d 
        '''  a   b   c   d
        '''  a   b   c   d 
        ''' ```
        ''' </param>
        ''' <param name="f">
        ''' A vector like:
        ''' 
        ''' ```
        ''' y1
        ''' y2
        ''' y3
        ''' y4
        ''' y5
        ''' ```
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function LinearFitting(x As NumericMatrix, f As Vector) As MLRFit
            Dim N = f.Length
            Dim p = x.ColumnDimension
            Dim Y As New NumericMatrix(f, N)
            Dim mean# = f.Average
            Dim beta = x.QRD.Solve(Y)
            Dim SST = ((f - mean) ^ 2).Sum
            Dim residuals As NumericMatrix = x.Multiply(B:=beta) - Y
            Dim SSE = residuals.Norm2 ^ 2
            Dim MLR As New MLRFit With {
                .beta = x.ColumnDimension _
                    .Sequence _
                    .Select(Function(i) beta(i, 0)) _
                    .ToArray,
                .N = N,
                .p = p,
                .SSE = SSE,
                .SST = SST
            }

            MLR.ErrorTest = [Error] _
                .RunTest(MLR, x, f) _
                .Select(Function(pt) DirectCast(pt, IFitError)) _
                .ToArray

            Return MLR
        End Function

        ''' <summary>
        ''' ```
        ''' h(X) = a + b*x1 + c*x2^2 + d*x3^3 + ... 
        ''' ```
        ''' </summary>
        ''' <param name="X"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CurveScale(X As IEnumerable(Of Double)) As Vector
            Return X.Select(Function(xi, i) xi ^ (i + 1)).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function left(beta#, t#, S#) As Double
            Return beta - (t * S)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function right(beta#, t#, S#) As Double
            Return beta + (t * S)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ConfidenceInterval(beta#, t#, S#) As DoubleRange
            Return {left(beta, t, S), right(beta, t, S)}
        End Function
    End Module
End Namespace
