#Region "Microsoft.VisualBasic::a429dd2d7dcb8fd4ceab3a452ec9c401, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\SparseMatrix.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 240
    '    Code Lines: 171
    ' Comment Lines: 31
    '   Blank Lines: 38
    '     File Size: 9.08 KB


    '     Class SparseMatrix
    ' 
    '         Properties: ColumnDimension, RowDimension
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: [Get], ArrayPack, ColumnProject, Dot, GetMatrix
    '                   Resize, RowVectors, ToString, Transpose, UnpackData
    ' 
    '         Sub: [Set]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization

Namespace LinearAlgebra.Matrix

    Public Class SparseMatrix : Implements GeneralMatrix

        ''' <summary>
        ''' just store the non-zero element index and the element value, all missing index is ZERO
        ''' </summary>
        Dim rows As New Dictionary(Of UInteger, Dictionary(Of UInteger, Double))
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
                Return [Get](i, j)
            End Get
            Set(value As Double)
                Call [Set](value, i, j)
            End Set
        End Property

        Default Public Overloads Property X(i As Integer, Optional byrow As Boolean = True) As Vector Implements GeneralMatrix.X
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As Vector)
                Throw New NotImplementedException()
            End Set
        End Property

        Default Public Overloads ReadOnly Property X(indices As IEnumerable(Of Integer)) As GeneralMatrix Implements GeneralMatrix.X
            Get
                Dim idxList As UInteger() = indices _
                    .Select(Function(offset) CUInt(offset)) _
                    .ToArray
                Dim rows = Me.rows _
                    .ToDictionary(Function(r) r.Key,
                                  Function(r)
                                      Return ColumnProject(r, idxList)
                                  End Function)

                Return New SparseMatrix(rows, m, idxList.Length)
            End Get
        End Property

        Private Shared Function ColumnProject(r As KeyValuePair(Of UInteger, Dictionary(Of UInteger, Double)), idxList As UInteger()) As Dictionary(Of UInteger, Double)
            Dim cols As New Dictionary(Of UInteger, Double)
            Dim src As Dictionary(Of UInteger, Double) = r.Value

            For Each j As UInteger In idxList
                If src.ContainsKey(j) Then
                    Call cols.Add(j, src(j))
                End If
            Next

            Return cols
        End Function

        Default Public Overloads ReadOnly Property X(rowIdx As BooleanVector) As GeneralMatrix Implements GeneralMatrix.X
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Private Sub New(matrix As Dictionary(Of UInteger, Dictionary(Of UInteger, Double)), m%, n%)
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
                .ToDictionary(Function(r) CUInt(r.Key),
                              Function(r)
                                  Return r.ToDictionary(Function(c) CUInt(c.ci),
                                                        Function(c)
                                                            Return c.xi
                                                        End Function)
                              End Function)
        End Sub

        Sub New(v As IndexVector)
            Call Me.New(v.Row, v.Col, v.X)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <returns>
        ''' value of missing index is ZERO
        ''' </returns>
        Public Function [Get](i As UInteger, j As UInteger) As Double
            If rows.ContainsKey(i) Then
                If rows(i).ContainsKey(j) Then
                    Return rows(i)(j)
                Else
                    Return 0.0
                End If
            Else
                Return 0.0
            End If
        End Function

        Public Sub [Set](xij As Double, i As UInteger, j As UInteger)
            If Not rows.ContainsKey(i) Then
                rows.Add(i, New Dictionary(Of UInteger, Double))
            End If
            If Not rows(i).ContainsKey(j) Then
                rows(i).Add(j, xij)
            Else
                rows(i)(j) = xij
            End If
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
        ''' <returns>A dense matrix data</returns>
        Public Function ArrayPack(Optional deepcopy As Boolean = False) As Double()() Implements GeneralMatrix.ArrayPack
            Dim real As Double()() = RectangularArray.Matrix(Of Double)(m, n)
            Dim i As UInteger

            For Each row In rows
                i = row.Key

                For Each col In row.Value
                    real(i)(col.Key) = col.Value
                Next
            Next

            Return real
        End Function

        Public Overrides Function ToString() As String
            Return $"[{RowDimension}x{ColumnDimension}]"
        End Function

        Public Iterator Function RowVectors() As IEnumerable(Of Vector) Implements GeneralMatrix.RowVectors
            For Each row As Double() In ArrayPack()
                Yield row.AsVector
            Next
        End Function

        Public Function GetMatrix(r() As Integer, j0 As Integer, j1 As Integer) As GeneralMatrix Implements GeneralMatrix.GetMatrix
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="xdata"></param>
        ''' <param name="xindices">
        ''' the corresponding index value of the <paramref name="xdata"/>, this
        ''' vector size of this parameter must be equals to the <paramref name="xdata"/>!
        ''' </param>
        ''' <param name="xindptr"></param>
        ''' <returns></returns>
        Public Shared Function UnpackData(xdata As Single(),
                                          xindices As Integer(),
                                          xindptr As Integer(),
                                          Optional maxColumns As Integer = -1) As SparseMatrix

            If xdata.Length <> xindices.Length Then
                Throw New InvalidProgramException($"the size of xdata({xdata.Length}) is not agree to the size of xindices({xindices.Length})!")
            End If

            Dim left As Integer = xindptr(Scan0)
            Dim matrix As New Dictionary(Of UInteger, Dictionary(Of UInteger, Double))
            Dim i As i32 = Scan0

            If maxColumns <= 0 Then
                maxColumns = xindices.Max + 1
            End If

            For Each idx As Integer In xindptr.Skip(1)
                Dim blocksize = idx - left
                Dim subsetData As Single() = New Single(blocksize - 1) {}
                Dim subsetIndex As Integer() = New Integer(blocksize - 1) {}
                Dim row As New Dictionary(Of UInteger, Double)

                Call Array.ConstrainedCopy(xdata, left, subsetData, Scan0, blocksize)
                Call Array.ConstrainedCopy(xindices, left, subsetIndex, Scan0, blocksize)

                For j As Integer = 0 To blocksize - 1
                    Call row.Add(key:=subsetIndex(j), value:=subsetData(j))
                Next

                left = idx
                matrix.Add(++i, row)
            Next

            Return New SparseMatrix(matrix, m:=i, n:=maxColumns)
        End Function

        Public Function Dot(m2 As GeneralMatrix) As GeneralMatrix Implements GeneralMatrix.Dot
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
