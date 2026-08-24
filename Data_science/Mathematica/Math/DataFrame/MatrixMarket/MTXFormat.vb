#Region "Microsoft.VisualBasic::4cf004b5c847b4a054d0e431b8f263ee, Data_science\Mathematica\Math\DataFrame\MatrixMarket\MTXFormat.vb"

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

    '   Total Lines: 119
    '    Code Lines: 57 (47.90%)
    ' Comment Lines: 46 (38.66%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 16 (13.45%)
    '     File Size: 5.00 KB


    '     Class MTXFormat
    ' 
    '         Function: (+3 Overloads) ReadMatrix
    ' 
    '         Sub: (+2 Overloads) WriteMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace MatrixMarket

    ''' <summary>
    ''' ```
    ''' %%MatrixMarket matrix coordinate real general
    ''' %=================================================================================
    ''' %
    ''' % This ASCII file represents a sparse MxN matrix with L 
    ''' % nonzeros in the following Matrix Market format:
    ''' %
    ''' % +----------------------------------------------+
    ''' % |%%MatrixMarket matrix coordinate real general | &lt;--- header line
    ''' % |%                                             | &lt;--+
    ''' % |% comments                                    |    |-- 0 Or more comment lines
    ''' % |%                                             | &lt;--+         
    ''' % |    M  N  L                                   | &lt;--- rows, columns, entries
    ''' % |    I1  J1  A(I1, J1)                         | &lt;--+
    ''' % |    I2  J2  A(I2, J2)                         |    |
    ''' % |    I3  J3  A(I3, J3)                         |    |-- L lines
    ''' % |        . . .                                 |    |
    ''' % |    IL JL  A(IL, JL)                          | &lt;--+
    ''' % +----------------------------------------------+   
    ''' %
    ''' % Indices are 1-based, i.e. A(1,1) Is the first element.
    ''' %
    ''' %=================================================================================
    '''  5  5  8
    '''    1     1   1.000e+00
    '''    2     2   1.050e+01
    '''    3     3   1.500e-02
    '''    1     4   6.000e+00
    '''    4     2   2.505e+02
    '''    4     4  -2.800e+02
    '''    4     5   3.332e+01
    '''    5     5   1.200e+01 
    ''' ```
    ''' </summary>
    Public Class MTXFormat

        Public Shared Function ReadMatrix(filepath As String) As SparseMatrix
            Using s As Stream = filepath.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return ReadMatrix(s)
            End Using
        End Function

        Public Shared Function ReadMatrix(file As Stream) As SparseMatrix
            Using reader As New StreamReader(file)
                Return ReadMatrix(reader)
            End Using
        End Function

        Public Shared Function ReadMatrix(reader As StreamReader) As SparseMatrix
            Dim line As Value(Of String) = ""

            Do While (line = reader.ReadLine).First = "%"c
            Loop

            Dim tokens As String() = CType(line, String).Trim.StringSplit("\s+")
            Dim M As Integer = Integer.Parse(tokens(Scan0))
            Dim N As Integer = Integer.Parse(tokens(1))
            Dim L As Integer = Integer.Parse(tokens(2))
            Dim matrix As New SparseMatrix(M, N)

            Do While Not line = reader.ReadLine Is Nothing
                tokens = CType(line, String).Trim.StringSplit("\s+")

                ' Indices are 1-based, i.e. A(1,1) Is the first element.
                M = Integer.Parse(tokens(Scan0)) - 1
                N = Integer.Parse(tokens(1)) - 1
                matrix(M, N) = Double.Parse(tokens(2))
            Loop

            Return matrix
        End Function

        ''' <summary>
        ''' Saves a <see cref="SparseMatrix"/> into a Matrix Market coordinate
        ''' text file (<c>%%MatrixMarket matrix coordinate real general</c>).
        ''' </summary>
        Public Shared Sub WriteMatrix(matrix As SparseMatrix, filepath As String)
            Using file As Stream = filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call WriteMatrix(matrix, New StreamWriter(file))
            End Using
        End Sub

        ''' <summary>
        ''' Writes a <see cref="SparseMatrix"/> as a Matrix Market coordinate
        ''' stream. The indices are emitted 1-based to match
        ''' <see cref="ReadMatrix"/>.
        ''' </summary>
        Public Shared Sub WriteMatrix(matrix As SparseMatrix, writer As StreamWriter)
            Dim M As Integer = matrix.RowDimension
            Dim N As Integer = matrix.ColumnDimension
            Dim L As Integer = matrix.nnz

            Call writer.WriteLine("%%MatrixMarket matrix coordinate real general")
            Call writer.WriteLine("% written by sciBASIC# MTXFormat")
            Call writer.WriteLine($"{M}  {N}  {L}")

            For i As Integer = 0 To M - 1
                For j As Integer = 0 To N - 1
                    Dim v As Double = matrix(i, j)

                    If v <> 0.0 Then
                        ' indices are 1-based
                        Call writer.WriteLine($"{i + 1}  {j + 1}  {v.ToString("g", CultureInfo.InvariantCulture)}")
                    End If
                Next
            Next

            Call writer.Flush()
        End Sub
    End Class
End Namespace
