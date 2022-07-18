
Imports System.Runtime.CompilerServices

Namespace LinearAlgebra.Matrix

    Module Subtraction

        ''' <summary>
        ''' the dimension of the vector should be equals
        ''' to the row dimension of the input matrix.
        ''' 
        ''' <paramref name="v"/> - each column in m
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="m"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RowSubtraction(v As Vector, m As GeneralMatrix) As GeneralMatrix
            Dim m2 As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim buffer = m2.Array
            Dim v2 As Vector

            For i As Integer = 0 To m2.ColumnDimension - 1
                v2 = m2.ColumnVector(i)
                v2 = v - v2

                For j As Integer = 0 To buffer.Length - 1
                    Dim x = buffer(j)
                    x(i) = v2(j)
                Next
            Next

            Return m2
        End Function
    End Module
End Namespace