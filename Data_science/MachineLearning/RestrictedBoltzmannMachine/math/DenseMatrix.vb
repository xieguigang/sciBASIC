Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace math

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class DenseMatrix
        Inherits Matrix

        Protected Friend Sub New(doubleMatrix2D As GeneralMatrix)
            MyBase.New(doubleMatrix2D)
        End Sub

        Sub New(m As NumericMatrix)
            Call MyBase.New(m)
        End Sub

        ' IMMUTABLE OPERATIONS 
        Public Overrides Function transpose() As Matrix
            Return New DenseMatrix(m.Transpose)
        End Function

        Public Overrides Function copy() As Matrix
            Return New DenseMatrix(New NumericMatrix(m.RowVectors))
        End Function

        Public Overrides Function dot(m2 As Matrix) As Matrix
            Return New DenseMatrix(Me.m.Dot(m2.m))
        End Function

        Public Overrides Function addColumns(m2 As Matrix) As Matrix
            'return make(DoubleFactory2D.dense.appendColumns(m, m2.data()));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function addRows(m2 As Matrix) As Matrix
            'return make(DoubleFactory2D.dense.appendRows(m, m2.data()));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function splitColumns(numPieces As Integer) As IList(Of Matrix)
            Dim pieces = splitColumns(Me, numPieces)
            Dim mPieces As IList(Of Matrix) = New List(Of Matrix)(pieces.Count)
            For Each piece In pieces
                mPieces.Add(make(piece))
            Next
            Return mPieces
        End Function

        ' MUTABLE OPERATIONS 

        Public Overloads Overrides Function apply([function] As DoubleFunction) As Matrix
            Return New DenseMatrix(m.assign([function]))
        End Function

        Public Overloads Overrides Function apply(m2 As Matrix, [function] As DoubleDoubleFunction) As Matrix
            Return New DenseMatrix(m.assign(m2.m, [function]))
        End Function

        Public Shared Function make(r As Integer, c As Integer) As Matrix
            Return New DenseMatrix(New NumericMatrix(r, c))
        End Function

        Public Shared Function randomGaussian(r As Integer, c As Integer) As Matrix
            Return New DenseMatrix(NumericMatrix.Gauss(c, r))
        End Function

        Public Shared Function random(r As Integer, c As Integer) As Matrix
            Return New DenseMatrix(NumericMatrix.Gauss(c, r))
        End Function

        Public Shared Function make(m As Double()()) As Matrix
            Return New DenseMatrix(New NumericMatrix(m))
        End Function

        Public Shared Function make(m As Vector) As Matrix
            Return New DenseMatrix(New NumericMatrix({m.ToArray}))
        End Function

        Public Overrides Function ToString() As String
            Return m.ToString()
        End Function

    End Class

End Namespace
