#Region "Microsoft.VisualBasic::a721d226be9e7488212fd43152ca6395, ..\visualbasic_App\Data_science\Mathematical\Math\BasicR\Solvers\SOR.vb"

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

    Public Module SOR

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="b"></param>
        ''' <param name="Omiga">松弛因子</param>
        ''' <param name="e">误差容限</param>
        ''' <param name="Iteration">最大允许迭代次数</param>
        ''' <returns></returns>
        Public Function Solve(A As Matrix, b As Vector, Optional Omiga As Double = 1.2, Optional e As Double = 0.00000001, Optional Iteration As Integer = 50) As Vector
            Dim N As Integer = A.N
            Dim x1 As Vector = New Vector(N), x As Vector = New Vector(N)

            For k As Integer = 0 To Iteration
                For i As Integer = 0 To N - 1
                    Dim sum As Double
                    For j As Integer = 0 To N - 1
                        If j < i Then
                            sum += A(i, j) * x(j)
                        ElseIf j > i Then
                            sum += A(i, j) * x1(j)
                        End If
                    Next

                    x(i) = (b(i) - sum) * Omiga / A(i, i) + (1.0 - Omiga) * x1(i)
                Next

                Dim dx As Vector = x - x1, err As Double = Math.Sqrt(dx.Mod)

                If err < e Then
                    Exit For
                End If

                Call x.CopyTo(x1)
            Next

            Return x
        End Function
    End Module
End Namespace
