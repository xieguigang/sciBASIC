Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Parallel

Namespace Math.Parallel

    ''' <summary>
    ''' module for run matrix dot product in parallel
    ''' </summary>
    Public Class MatrixDotProduct : Inherits VectorTask

        ReadOnly a, b, c As Double()()
        ReadOnly n As Integer
        ReadOnly m As Integer

        Private Sub New(a As Double()(), b As Double()(), ByRef C As Double()(), ncolB As Integer)
            MyBase.New(ncolB)

            Me.a = a
            Me.b = b
            Me.c = C
            Me.n = a(0).Length
            Me.m = a.Length
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim Bcolj As Double() = New Double(n - 1) {}

            For j As Integer = start To ends
                For k As Integer = 0 To n - 1
                    Bcolj(k) = b(k)(j)
                Next
                For i As Integer = 0 To m - 1
                    Dim Arowi As Double() = a(i)
                    Dim s As Double = 0

                    For k As Integer = 0 To n - 1
                        s += Arowi(k) * Bcolj(k)
                    Next

                    c(i)(j) = s
                Next
            Next
        End Sub

        Public Shared Function Resolve(a As Double()(), b As Double()()) As Double()()
            Dim nrowA As Integer = a.Length
            Dim ncolB As Integer = b(0).Length
            Dim c As Double()() = RectangularArray.Matrix(Of Double)(nrowA, ncolB)
            Dim solver As New MatrixDotProduct(a, b, c, ncolB)
            Call solver.Run()
            Return c
        End Function
    End Class
End Namespace