Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Interpolation

    ''' <summary>
    ''' B-spline
    ''' </summary>
    ''' <remarks>https://github.com/kerrot/B_Spline</remarks>
    Public Module B_Spline

        ''' <summary>
        ''' B-spline curve interpolation
        ''' </summary>
        ''' <param name="ctrlPts">Control points</param>
        ''' <param name="degree%"></param>
        ''' <param name="RESOLUTION%"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Compute(ctrlPts As Point(), Optional degree% = 5, Optional RESOLUTION% = 10) As List(Of PointF)
            Return ctrlPts.ToArray(
                Function(pt) New PointF With {
                    .X = pt.X,
                    .Y = pt.Y
                }).Compute(degree, RESOLUTION)
        End Function

        ''' <summary>
        ''' B-spline curve interpolation
        ''' </summary>
        ''' <param name="ctrlPts">Control points</param>
        ''' <param name="degree%"></param>
        ''' <param name="RESOLUTION%"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Compute(ctrlPts As PointF(), Optional degree% = 5, Optional RESOLUTION% = 10) As List(Of PointF)
            Dim out As New List(Of PointF)

            If ctrlPts.Length > 1 Then
                Dim ustep As Double = 1.0 / (RESOLUTION * (ctrlPts.Length - 1))

                Dim n As Integer = ctrlPts.Length - 1
                Dim k As Integer = If((ctrlPts.Length > degree), degree + 1, ctrlPts.Length)
                Dim m As Integer = k + n

                Dim t As Double() = New Double(m) {}
                For i As Integer = 0 To k - 1
                    t(i) = 0
                    t(t.Length - 1 - i) = 1
                Next
                If m + 1 > 2 * k Then
                    Dim tstep As Double = 1.0 / (m - 2 * (k - 1))
                    For i As Integer = k To m - k
                        t(i) = t(i - 1) + tstep
                    Next
                End If

                Dim u As Double = 0

                While u < 1
                    Call ctrlPts.OutputPoint(out, t, k, u)
                    u += ustep
                End While

                out.Add(ctrlPts.Last())
            End If

            Return out
        End Function

        <Extension>
        Private Sub OutputPoint(ctrlPts As PointF(), out As List(Of PointF), t As Double(), k As Integer, u As Double)
            Dim i As Integer, j As Integer, r As Integer
            Dim d1 As Double, d2 As Double
            Dim l As Integer = 0

            While l < t.Length AndAlso t(l) <= u
                l += 1
            End While

            l -= 1

            Dim A As PointF() = New PointF(k - 1) {}

            For j = 0 To k - 1
                Dim index As Integer = l - k + 1 + j
                If index < 0 OrElse index > ctrlPts.Length - 1 Then
                    Return
                End If
                A(j) = New PointF() With {
                    .X = ctrlPts(index).X,
                    .Y = ctrlPts(index).Y
                }
            Next

            For r = 1 To k - 1
                For j = k - 1 To r Step -1
                    i = l - k + 1 + j
                    d1 = u - t(i)
                    d2 = t(i + k - r) - u
                    A(j).X = (d1 * A(j).X + d2 * A(j - 1).X) / (d1 + d2)
                    A(j).Y = (d1 * A(j).Y + d2 * A(j - 1).Y) / (d1 + d2)
                Next
            Next

            out.Add(A(k - 1))
        End Sub
    End Module
End Namespace