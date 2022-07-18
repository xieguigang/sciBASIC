Imports System.Runtime.CompilerServices

Namespace LinearAlgebra.Matrix

    Module Multiply

        ''' <summary>
        ''' the vector size is equals to the matrix rows,
        ''' each element in target vector is multiply to
        ''' each row in matrix
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RowMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix
            Dim X As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = X.Array
            Dim buffer As Double()() = m.ArrayPack

            For i As Integer = 0 To X.ColumnDimension - 1
                Dim vi As Double = v(i)

                For j As Integer = 0 To X.RowDimension - 1
                    C(i)(j) = vi * buffer(i)(j)
                Next
            Next

            Return X
        End Function

        ''' <summary>
        ''' the vector size is equals to the matrix columns,
        ''' each element in target vector is multiply to
        ''' each column in matrix
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColumnMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix
            Dim X As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = X.Array
            Dim rows = m.RowVectors.ToArray

            For i As Integer = 0 To m.RowDimension - 1
                C(i) = rows(i) * v
            Next

            Return X
        End Function
    End Module
End Namespace