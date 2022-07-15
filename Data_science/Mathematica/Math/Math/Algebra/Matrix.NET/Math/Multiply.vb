Imports System.Runtime.CompilerServices

Namespace LinearAlgebra.Matrix

    Module Multiply

        <Extension>
        Public Function RowMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix

        End Function

        <Extension>
        Public Function ColumnMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix

        End Function
    End Module
End Namespace