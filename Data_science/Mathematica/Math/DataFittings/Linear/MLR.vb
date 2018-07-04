Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function LinearFitting(x As Double(,), y#()) As MLRFit
        Return LinearFitting(New GeneralMatrix(x.RowIterator.ToArray), y)
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
    Public Shared Function LinearFitting(x As GeneralMatrix, f As Vector) As MLRFit
        Dim N = f.Length
        Dim p = x.ColumnDimension
        Dim Y As New GeneralMatrix(f, N)
        Dim mean# = f.Average
        Dim beta = x.QRD.Solve(Y)
        Dim SST = ((f - mean) ^ 2).Sum
        Dim residuals As GeneralMatrix = x.Multiply(beta) - Y
        Dim SSE = residuals.Norm2 ^ 2

        Return New MLRFit With {
            .beta = x.ColumnDimension _
                .Sequence _
                .Select(Function(i) beta(i, 0)) _
                .ToArray,
            .N = N,
            .p = p,
            .SSE = SSE,
            .SST = SST
        }
    End Function

    ''' <summary>
    ''' ```
    ''' h(X) = a + b*x1 + c*x2^2 + d*x3^3 + ... 
    ''' ```
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    <MethodImpl>
    Public Function CurveScale(X As IEnumerable(Of Double)) As Vector
        Return X.Select(Function(xi, i) xi ^ (i + 1)).AsVector
    End Function

    Public Shared Function left(beta#, t#, S#) As Double
        Return beta - (t * S)
    End Function

    Public Shared Function right(beta#, t#, S#) As Double
        Return beta + (t * S)
    End Function

    Public Shared Function ConfidenceInterval(beta#, t#, S#) As DoubleRange
        Dim left = MLRFit.left(beta, t, S), right = MLRFit.right(beta, t, S)
        Dim min# = If(left < right, left, right)
        Dim max# = If(left > right, left, right)

        Return {min, max}
    End Function
End Class
