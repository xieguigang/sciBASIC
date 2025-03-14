#Region "Microsoft.VisualBasic::fae541f36d5d6613d40f6eb2026fa735, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\Matrix.vb"

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

    '   Total Lines: 111
    '    Code Lines: 92 (82.88%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (17.12%)
    '     File Size: 3.46 KB


    '     Class Matrix
    ' 
    '         Properties: All, Diag
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [get], colDot, colNorm, colSquaredNorm, getCol
    '                   index, resize
    ' 
    '         Sub: [set], getCol, muls, mulv, setCol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Framework.Optimization.LBFGSB
    Public NotInheritable Class Matrix
        Public mat As Double()
        Public rows As Integer
        Public cols As Integer

        Public Sub New(mat As Double(), rows As Integer, cols As Integer)
            Me.mat = mat
            Me.rows = rows
            Me.cols = cols
        End Sub

        Public Sub New(rows As Integer, cols As Integer)
            Me.New(New Double(rows * cols - 1) {}, rows, cols)
        End Sub

        Public Shared Function resize(m As Matrix, rows As Integer, cols As Integer) As Matrix
            If m Is Nothing Then
                Return New Matrix(rows, cols)
            Else
                If m.rows = rows AndAlso m.cols = cols Then
                    Return m
                Else
                    Return New Matrix(Vector.resize(m.mat, rows * cols), rows, cols)
                End If
            End If
        End Function

        Public WriteOnly Property All As Double
            Set(value As Double)
                Vector.setAll(mat, value)
            End Set
        End Property

        Public Sub [set](row As Integer, col As Integer, val As Double)
            mat(col * rows + row) = val
        End Sub

        Public Sub setCol(col As Integer, v As Double())
            Array.Copy(v, 0, mat, col * rows, rows)
        End Sub

        Public Function getCol(col As Integer) As Double()
            Dim res = New Double(rows - 1) {}
            Array.Copy(mat, col * rows, res, 0, rows)
            Return res
        End Function

        Public Sub getCol(col As Integer, res As Double())
            Array.Copy(mat, col * rows, res, 0, rows)
        End Sub

        Public Function [get](row As Integer, col As Integer) As Double
            Return mat(col * rows + row)
        End Function

        Public Function index(row As Integer, col As Integer) As Integer
            Return col * rows + row
        End Function

        Public WriteOnly Property Diag As Double
            Set(value As Double)
                For i = 0 To std.Min(rows, cols) - 1
                    [set](i, i, value)
                Next
            End Set
        End Property

        Public Function colSquaredNorm(col As Integer) As Double
            Dim res = 0.0
            Dim idx = col * rows
            For i = 0 To rows - 1
                res += mat(idx + i) * mat(idx + i)
            Next
            Return res
        End Function

        Public Function colNorm(col As Integer) As Double
            Return std.Sqrt(colSquaredNorm(col))
        End Function

        Public Function colDot(col As Integer, v As Double()) As Double
            Dim res = 0.0
            Dim idx = col * rows
            For i = 0 To rows - 1
                res += mat(idx + i) * v(i)
            Next

            Return res
        End Function

        Public Sub mulv(v As Double(), res As Double())
            For row = 0 To rows - 1
                Dim dot = 0.0
                For col = 0 To cols - 1
                    dot += [get](row, col) * v(col)
                Next
                res(row) = dot
            Next
        End Sub

        Public Sub muls(v As Double)
            For i = 0 To mat.Length - 1
                mat(i) *= v
            Next
        End Sub
    End Class

End Namespace
