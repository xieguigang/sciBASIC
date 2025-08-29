Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace HoughCircles

    Public Class HoughSpaceRadius : Inherits VectorTask

        ReadOnly BinarEdgeMap As Boolean(,)
        ReadOnly radius As Integer

        ReadOnly binarHeight As Integer
        ReadOnly binarWidth As Integer

        ReadOnly resultMatrix As Short(,)

        Public Sub New(edges As Boolean(,), radius As Integer, Optional verbose As Boolean = False, Optional workers As Integer? = Nothing)
            MyBase.New(edges.GetLength(0), verbose, workers)

            Me.BinarEdgeMap = edges

            binarHeight = edges.GetLength(0)
            binarWidth = edges.GetLength(1)

            resultMatrix = New Short(binarHeight - 1, binarWidth - 1) {}
        End Sub

        Public Function getHoughSpace() As Short(,)
            Return resultMatrix
        End Function

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For Y As Integer = start To ends - 1
                For X As Integer = 0 To binarWidth - 1
                    If BinarEdgeMap(Y, X) Then
                        UpdateHoughMatrix(resultMatrix, X, Y, radius)
                    End If
                Next
            Next
        End Sub

        Private Sub UpdateHoughMatrix(ByRef matrix As Short(,), x As Integer, y As Integer, radius As Integer)
            For teta = 0 To 359
                Dim a = CInt(x + radius * std.Cos(teta))
                Dim b = CInt(y + radius * std.Sin(teta))

                If a < 0 OrElse
                    b < 0 OrElse
                    b >= matrix.GetLength(0) OrElse
                    a >= matrix.GetLength(1) Then

                    Continue For
                End If

                matrix(b, a) += 1
            Next
        End Sub
    End Class
End Namespace