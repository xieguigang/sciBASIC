#Region "Microsoft.VisualBasic::8d4e1fb426e663ef1274d278cb5d3a98, Data_science\Mathematica\Math\Math\Algebra\Solvers\SOR.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module SOR
    ' 
    '         Function: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports sys = System.Math

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

                Dim dx As Vector = x - x1, err As Double = sys.Sqrt(dx.Mod)

                If err < e Then
                    Exit For
                End If

                x1 = x
            Next

            Return x
        End Function
    End Module
End Namespace
