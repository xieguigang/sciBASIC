Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

''' <summary>
''' Multiple linear regression.(多元线性回归)
''' 
''' Problem of predicting appropriate values of given feature set as inputvector
''' using supervised linear regression with multiple dimensional sample input 
''' </summary>
Public Class MLR

    ''' <summary>
    ''' 
    ''' </summary>
    Dim N%
    ''' <summary>
    ''' number of dependent variables
    ''' </summary>
    Dim p%
    ''' <summary>
    ''' regression coefficients
    ''' </summary>
    Dim beta As GeneralMatrix
    ''' <summary>
    ''' sum of squared
    ''' </summary>
    Dim SSE#, SST#

    Public ReadOnly Property R2 As Double
        Get
            Return 1.0 - SSE / SST
        End Get
    End Property

    Public Sub DoRegression(x As GeneralMatrix, f As Vector)
        N = f.Length
        p = x.ColumnDimension

        Dim Y As New GeneralMatrix(f, N)
        Dim sum# = f.Sum
        Dim mean# = sum / N

        beta = x.QRD.Solve(Y)
        SST = ((f - mean) ^ 2).Sum

        Dim residuals As GeneralMatrix = x.Multiply(beta) - Y

        SSE = residuals.Norm2 ^ 2
    End Sub

    Public Shared Function left(beta#, t#, S#) As Double
        Return beta - (t * S)
    End Function

    Public Shared Function right(beta#, t#, S#) As Double
        Return beta + (t * S)
    End Function

    Public Shared Function ConfidenceInterval(beta#, t#, S#) As DoubleRange
        Dim left = MLR.left(beta, t, S), right = MLR.right(beta, t, S)
        Dim min# = If(left < right, left, right)
        Dim max# = If(left > right, left, right)

        Return {min, max}
    End Function
End Class
