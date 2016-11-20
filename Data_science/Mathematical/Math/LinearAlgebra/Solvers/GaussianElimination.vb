#Region "Microsoft.VisualBasic::18727b9d6b82f6dba00371b298e22a99, ..\visualbasic_App\Data_science\Mathematical\Math\BasicR\Solvers\GaussianElimination.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace LinearAlgebra.Solvers

    Public Module GaussianElimination

        ''' <summary>
        ''' a*b=0 -> x
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns>x</returns>
        ''' <remarks></remarks>
        Public Function Solve(A As Matrix, b As Vector) As Vector
            Dim n As Integer = b.Dim
            Dim TMP As Double
            Dim Ab As Matrix = New Matrix(n, n + 1)

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Ab(i, j) = A(i, j)
                Next
                Ab(i, n) = b(i)
            Next

            For k As Integer = 0 To n - 2 'Gaussian Elimination Core
                For i = k + 1 To n - 1
                    TMP = Ab(i, k) / Ab(k, k)
                    For j = 0 To n
                        Ab(i, j) = Ab(i, j) - TMP * Ab(k, j)
                    Next
                Next
            Next

            For i = 0 To n - 1
                For j = 0 To n - 1
                    A(i, j) = Ab(i, j)
                Next
                b(i) = Ab(i, n)
            Next

            Return UpTri(A, b)
        End Function

        ''' <summary>
        ''' 上三角矩阵方程解法
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function UpTri(A As Matrix, b As Vector) As Vector
            Dim N As Integer = A.N
            Dim x As New Vector(N)

            x(N - 1) = b(N - 1) / A(N - 1, N - 1)

            For i As Integer = N - 2 To 0 Step -1
                x(i) = b(i)
                For j As Integer = i + 1 To N - 1
                    x(i) -= A(i, j) * x(j)
                Next
                x(i) /= A(i, i)
            Next
            Return x
        End Function
    End Module
End Namespace
