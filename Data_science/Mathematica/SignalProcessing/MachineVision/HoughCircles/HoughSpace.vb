Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace HoughCircles

    Public Class HoughSpace : Inherits VectorTask

        ReadOnly edges As Boolean(,)
        ReadOnly resultCube As Short(,,)

        ReadOnly binarHeight As Integer
        ReadOnly binarWidth As Integer
        ReadOnly radius As Integer

        Public Sub New(edges As Boolean(,), Optional verbose As Boolean = False, Optional workers As Integer? = Nothing)
            MyBase.New(edges.GetLength(0), verbose, workers)

            Me.edges = edges

            binarHeight = edges.GetLength(0)
            binarWidth = edges.GetLength(1)
            radius = If(binarHeight < binarWidth, binarHeight, binarWidth)
            resultCube = New Short(radius - 1, binarHeight - 1, binarWidth - 1) {}
        End Sub

        Public Function getHoughSpace() As Short(,,)
            Return resultCube
        End Function

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For Y As Integer = start To ends
                For X As Integer = 0 To binarWidth - 1
                    If edges(Y, X) Then
                        UpdateHoughMatrix(resultCube, X, Y, radius)
                    End If
                Next
            Next
        End Sub

        Private Sub UpdateHoughMatrix(ByRef cube As Short(,,), x As Integer, y As Integer, maxRadius As Integer)
            For radius As Integer = 1 To maxRadius - 1
                For teta = 0 To 359
                    Dim a = CInt(x + radius * std.Cos(teta))
                    Dim b = CInt(y + radius * std.Sin(teta))

                    If a < 0 OrElse b < 0 OrElse a >= cube.GetLength(0) OrElse b >= cube.GetLength(1) Then
                        Continue For
                    End If

                    cube(radius, b, a) += 1
                Next
            Next
        End Sub
    End Class
End Namespace