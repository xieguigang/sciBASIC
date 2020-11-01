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
            Return New GeneralMatrix(x.RowIterator.ToArray).LinearFitting(y)
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
        Public Function LinearFitting(x As GeneralMatrix, f As Vector) As MLRFit
            Dim N = f.Length
            Dim p = x.ColumnDimension
            Dim Y As New GeneralMatrix(f, N)
            Dim mean# = f.Average
            Dim beta = x.QRD.Solve(Y)
            Dim SST = ((f - mean) ^ 2).Sum
            Dim residuals As GeneralMatrix = x.Multiply(beta) - Y
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