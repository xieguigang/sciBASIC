Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' another form of <see cref="SparseMatrix"/>
    ''' </summary>
    Public Class IndexVector

        Public Property Row As Integer()
        Public Property Col As Integer()
        Public Property X As Double()

        Sub New()
        End Sub

        Sub New(row As Integer(), col As Integer(), x As Double())
            Me.Row = row
            Me.Col = col
            Me.X = x
        End Sub

    End Class
End Namespace