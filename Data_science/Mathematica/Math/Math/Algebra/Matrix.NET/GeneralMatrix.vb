
Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' [m,n]
    ''' </summary>
    Public Interface GeneralMatrix

        Default Property X(i As Integer, j As Integer) As Double

        ''' <summary>
        ''' m
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property RowDimension As Integer
        ''' <summary>
        ''' n
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property ColumnDimension As Integer

        Function Transpose() As GeneralMatrix

    End Interface
End Namespace