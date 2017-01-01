Namespace Matrix

    Public Module Extensions

        Public Function size(M As GeneralMatrix, d%) As Integer
            If d = 1 Then
                Return M.RowDimension
            ElseIf d = 2 Then
                Return M.ColumnDimension
            Else

            End If
        End Function

        ''' <summary>Generate matrix with random elements</summary>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <param name="n">   Number of colums.
        ''' </param>
        ''' <returns>     An m-by-n matrix with uniformly distributed random elements.
        ''' </returns>

        Public Function rand(m As Integer, n As Integer) As GeneralMatrix
            Dim random__1 As New Random()

            Dim A As New GeneralMatrix(m, n)
            Dim X As Double()() = A.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    X(i)(j) = random__1.NextDouble()
                Next
            Next
            Return A
        End Function
    End Module
End Namespace