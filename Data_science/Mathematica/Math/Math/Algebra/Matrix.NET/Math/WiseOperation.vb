
Imports Microsoft.VisualBasic.Linq

Namespace LinearAlgebra.Matrix

    Public Class WiseOperation

        Dim matrix_wise As Vector()

        Public Function Sum() As Vector
            Return matrix_wise.Select(Function(xi) xi.Sum).AsVector
        End Function

        Public Shared Function RowWise(m As GeneralMatrix) As WiseOperation
            Return New WiseOperation With {
                .matrix_wise = m.RowVectors.ToArray
            }
        End Function

        Public Shared Function ColWise(m As GeneralMatrix) As WiseOperation
            Return New WiseOperation With {
                .matrix_wise = m.ColumnDimension _
                    .Sequence _
                    .Select(Function(i)
                                Return m.ColumnVector(i)
                            End Function) _
                    .ToArray
            }
        End Function

    End Class
End Namespace