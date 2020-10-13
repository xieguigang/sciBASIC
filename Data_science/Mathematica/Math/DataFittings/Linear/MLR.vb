#Region "Microsoft.VisualBasic::39d4188be766f57e9840955d8183b2d0, Data_science\Mathematica\Math\DataFittings\Linear\MLR.vb"

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

    ' Class MLRFit
    ' 
    '     Properties: beta, Fx, N, p, R2
    ' 
    '     Function: ConfidenceInterval, CurveScale, left, (+2 Overloads) LinearFitting, right
    '     Structure [Error]
    ' 
    '         Function: RunTest, ToString
    ' 
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
Imports stdNum = System.Math

''' <summary>
''' Multiple linear regression.(多元线性回归)
''' 
''' Problem of predicting appropriate values of given feature set as inputvector
''' using supervised linear regression with multiple dimensional sample input 
''' </summary>
Public Class MLRFit

    ''' <summary>
    ''' 
    ''' </summary>
    Public Property N As Integer
    ''' <summary>
    ''' number of dependent variables
    ''' </summary>
    Public Property p As Integer
    ''' <summary>
    ''' regression coefficients
    ''' </summary>
    Public Property beta As Double()
    ''' <summary>
    ''' sum of squared
    ''' </summary>
    Public SSE#, SST#

    Public ReadOnly Property R2 As Double
        Get
            Return 1.0 - SSE / SST
        End Get
    End Property

    ''' <summary>
    ''' Evaluate the regression value from a given X vector
    ''' 
    ''' ```
    ''' f(x) = ax1 + bx2 + cx3 + dx4 + ...
    ''' ```
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Overridable ReadOnly Property Fx(x As Vector) As Double
        Get
            Return (x * beta).Sum
        End Get
    End Property

    Public Structure [Error]

        Dim X As Vector
        Dim Y#
        Dim Yfit#

        Public Overrides Function ToString() As String
            Return $"{stdNum.Abs(Y - Yfit)} = |{Y} - {Yfit}|"
        End Function

        Public Shared Iterator Function RunTest(MLR As MLRFit, X As GeneralMatrix, Y As Vector) As IEnumerable(Of [Error])
            For Each xi In X.RowVectors.SeqIterator
                Dim yi = Y.Item(index:=xi)
                Dim yfit = MLR.Fx(xi)

                Yield New [Error] With {
                    .X = xi,
                    .Y = yi,
                    .Yfit = yfit
                }
            Next
        End Function
    End Structure

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function LinearFitting(x As Double(,), y#(), Optional ByRef errors As [Error]() = Nothing) As MLRFit
        Return LinearFitting(New GeneralMatrix(x.RowIterator.ToArray), y, errors)
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
    Public Shared Function LinearFitting(x As GeneralMatrix, f As Vector, Optional ByRef errors As [Error]() = Nothing) As MLRFit
        Dim N = f.Length
        Dim p = x.ColumnDimension
        Dim Y As New GeneralMatrix(f, N)
        Dim mean# = f.Average
        Dim beta = x.QRD.Solve(Y)
        Dim SST = ((f - mean) ^ 2).Sum
        Dim residuals As GeneralMatrix = x.Multiply(beta) - Y
        Dim SSE = residuals.Norm2 ^ 2
        Dim MLR = New MLRFit With {
            .beta = x.ColumnDimension _
                .Sequence _
                .Select(Function(i) beta(i, 0)) _
                .ToArray,
            .N = N,
            .p = p,
            .SSE = SSE,
            .SST = SST
        }

        errors = [Error].RunTest(MLR, x, f).ToArray

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
    Public Shared Function CurveScale(X As IEnumerable(Of Double)) As Vector
        Return X.Select(Function(xi, i) xi ^ (i + 1)).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function left(beta#, t#, S#) As Double
        Return beta - (t * S)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function right(beta#, t#, S#) As Double
        Return beta + (t * S)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ConfidenceInterval(beta#, t#, S#) As DoubleRange
        Return {MLRFit.left(beta, t, S), MLRFit.right(beta, t, S)}
    End Function
End Class
