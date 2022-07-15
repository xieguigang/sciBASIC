Imports System.Runtime.CompilerServices

Namespace LinearAlgebra.Matrix

    Module Multiply

        <Extension>
        Public Function RowMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                Dim vi As Double = v(i)

                For j As Integer = 0 To n - 1
                    C(i)(j) = vi * Buffer(i)(j)
                Next
            Next

            Return X
        End Function

        <Extension>
        Public Function ColumnMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                Dim vi As Double = v(i)

                For j As Integer = 0 To n - 1
                    C(i)(j) = vi * Buffer(i)(j)
                Next
            Next

            Return X
        End Function
    End Module
End Namespace