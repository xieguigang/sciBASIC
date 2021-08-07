Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace LinearAlgebra.Matrix

    Public Class SparseMatrix : Implements GeneralMatrix

        Dim rows As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
        Dim m, n As Integer

        Public ReadOnly Property RowDimension As Integer Implements GeneralMatrix.RowDimension
            Get
                Return m
            End Get
        End Property

        Public ReadOnly Property ColumnDimension As Integer Implements GeneralMatrix.ColumnDimension
            Get
                Return n
            End Get
        End Property

        Default Property Xij(i As Integer, j As Integer) As Double Implements GeneralMatrix.X
            Get
                If rows.ContainsKey(i) Then
                    If rows(i).ContainsKey(j) Then
                        Return rows(i)(j)
                    Else
                        Return 0.0
                    End If
                Else
                    Return 0.0
                End If
            End Get
            Set(value As Double)
                If Not rows.ContainsKey(i) Then
                    rows.Add(i, New Dictionary(Of Integer, Double))
                End If
                If Not rows(i).ContainsKey(j) Then
                    rows(i).Add(j, value)
                Else
                    rows(i)(j) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="m">nrows</param>
        ''' <param name="n">ncols</param>
        Sub New(m As Integer, n As Integer)
            Me.m = m
            Me.n = n
        End Sub

        Sub New(row As Integer(), col As Integer(), x As Double())
            Me.rows = row _
                .Select(Function(ri, i)
                            Return (ri, ci:=col(i), xi:=x(i))
                        End Function) _
                .GroupBy(Function(r) r.ri) _
                .ToDictionary(Function(r) r.Key,
                              Function(r)
                                  Return r.ToDictionary(Function(c) c.ci,
                                                        Function(c)
                                                            Return c.xi
                                                        End Function)
                              End Function)
        End Sub

        Public Function Resize(M As Integer, N As Integer) As SparseMatrix
            Me.m = M
            Me.n = N
            Return Me
        End Function

        Public Function Transpose() As GeneralMatrix Implements GeneralMatrix.Transpose
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' convert to real [m,n] matrix
        ''' </summary>
        ''' <param name="deepcopy"></param>
        ''' <returns></returns>
        Public Function ArrayPack(Optional deepcopy As Boolean = False) As Double()() Implements GeneralMatrix.ArrayPack
            Dim real As Double()() = MAT(Of Double)(m, n)
            Dim i As Integer

            For Each row In rows
                i = row.Key

                For Each col In row.Value
                    real(i)(col.Key) = col.Value
                Next
            Next

            Return real
        End Function
    End Class
End Namespace