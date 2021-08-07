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

        Default Overloads Property X(i As Integer, j As Integer) As Double Implements GeneralMatrix.X
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

        Default Public Overloads Property X(i As Integer) As Vector Implements GeneralMatrix.X
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As Vector)
                Throw New NotImplementedException()
            End Set
        End Property

        Default Public Overloads ReadOnly Property X(indices As IEnumerable(Of Integer)) As GeneralMatrix Implements GeneralMatrix.X
            Get
                Dim idxList As Integer() = indices.ToArray
                Dim rows = Me.rows _
                    .ToDictionary(Function(r) r.Key,
                                  Function(r)
                                      Dim cols As New Dictionary(Of Integer, Double)
                                      Dim src As Dictionary(Of Integer, Double) = r.Value

                                      For Each j As Integer In idxList
                                          If src.ContainsKey(j) Then
                                              cols.Add(j, src(j))
                                          End If
                                      Next

                                      Return cols
                                  End Function)

                Return New SparseMatrix(rows, m, idxList.Length)
            End Get
        End Property

        Private Sub New(matrix As Dictionary(Of Integer, Dictionary(Of Integer, Double)), m%, n%)
            Me.rows = matrix
            Me.m = m
            Me.n = n
        End Sub

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

        Sub New(v As IndexVector)
            Call Me.New(v.Row, v.Col, v.X)
        End Sub

        Public Function Resize(M As Integer, N As Integer) As GeneralMatrix Implements GeneralMatrix.Resize
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

        Public Iterator Function RowVectors() As IEnumerable(Of Vector) Implements GeneralMatrix.RowVectors
            For Each row As Double() In ArrayPack()
                Yield row.AsVector
            Next
        End Function

        Public Function GetMatrix(r() As Integer, j0 As Integer, j1 As Integer) As GeneralMatrix Implements GeneralMatrix.GetMatrix
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace