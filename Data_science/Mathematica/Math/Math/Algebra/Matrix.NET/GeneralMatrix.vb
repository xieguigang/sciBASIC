
Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' [m,n]
    ''' </summary>
    Public Interface GeneralMatrix

        ''' <summary>
        ''' get/set cell element value
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <returns></returns>
        Default Property X(i As Integer, j As Integer) As Double
        Default Property X(i As Integer) As Vector

        ''' <summary>
        ''' column projection via column index
        ''' </summary>
        ''' <remarks>
        ''' select column values for each row for create a new matrix
        ''' </remarks>
        ''' <param name="indices"></param>
        ''' <returns></returns>
        Default ReadOnly Property X(indices As IEnumerable(Of Integer)) As GeneralMatrix

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
        Function ArrayPack(Optional deepcopy As Boolean = False) As Double()()
        Function Resize(m As Integer, n As Integer) As GeneralMatrix
        Function RowVectors() As IEnumerable(Of Vector)

    End Interface
End Namespace