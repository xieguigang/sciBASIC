Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace math

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class SparseMatrix
        Inherits Matrix

        Protected Friend Sub New(doubleMatrix2D As GeneralMatrix)
            MyBase.New(doubleMatrix2D)
        End Sub

        ' IMMUTABLE OPERATIONS 

        Public Overrides Function copy() As Matrix
            'return make(m.copy());
            Throw New NotImplementedException()
        End Function

        Public Overrides Function dot(m2 As Matrix) As Matrix
            'return make(DENSE_DOUBLE_ALGEBRA.mult(m, m2.data()));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function transpose() As Matrix
            'return make(DENSE_DOUBLE_ALGEBRA.transpose(this.m));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function addColumns(m2 As Matrix) As Matrix
            'return make(DoubleFactory2D.sparse.appendColumns(m, m2.data()));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function addRows(m2 As Matrix) As Matrix
            'return make(DoubleFactory2D.sparse.appendRows(m, m2.data()));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function splitColumns(numPieces As Integer) As IList(Of Matrix)
            Dim pieces = splitColumns(Me, numPieces)
            Dim mPieces As IList(Of Matrix) = New List(Of Matrix)(pieces.Count)
            For Each piece In pieces
                mPieces.Add(DenseMatrix.make(piece))
            Next
            Return mPieces
        End Function

        ' MUTABLE OPERATIONS 
        Public Overrides Overloads Function apply([function] As DoubleFunction) As Matrix
            'return make(m.assign(function));
            Throw New NotImplementedException()
        End Function

        Public Overrides Overloads Function apply(m2 As Matrix, [function] As DoubleDoubleFunction) As Matrix
            'return make(m.assign(m2.data(), function));
            Throw New NotImplementedException()
        End Function

        Public Shared Function make(m As GeneralMatrix) As Matrix
            Return New SparseMatrix(m)
        End Function

        Public Shared Function make(r As Integer, c As Integer) As Matrix
            'return new SparseMatrix(DoubleFactory2D.sparse.make(r, c));
            Throw New NotImplementedException()
        End Function

        Public Shared Function randomGaussian(r As Integer, c As Integer) As Matrix
            'return new SparseMatrix(DoubleFactory2D.sparse.make(r, c).assign(RANDOM_GAUSSIAN));
            Throw New NotImplementedException()
        End Function

        Public Shared Function random(r As Integer, c As Integer) As Matrix
            'return new SparseMatrix(DoubleFactory2D.sparse.make(r, c).assign(RANDOM_DOUBLE));
            Throw New NotImplementedException()
        End Function

        Public Shared Function make(m As Double()()) As Matrix
            'return new SparseMatrix(DoubleFactory2D.sparse.make(m));
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ToString() As String
            Return m.ToString()
        End Function

    End Class

End Namespace
