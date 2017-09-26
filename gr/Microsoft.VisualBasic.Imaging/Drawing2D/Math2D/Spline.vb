Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Interpolation

Namespace Drawing2D.Math2D

    Public Module Spline

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BezierCurve(points As IEnumerable(Of Point), Optional iteration% = 10) As PointF()
            Return points.PointF.BezierCurve(iteration)
        End Function

        <Extension>
        Public Function BezierCurve(points As IEnumerable(Of PointF), Optional iteration% = 10) As PointF()
            Dim smooth As New List(Of PointF)
            Dim pointData = points.ToArray

            For Each block In pointData.Join({pointData.First, pointData(1)}).SlideWindows(3)
                Dim bezier As New BezierCurve(block(0), block(1), block(2), iteration)
                smooth += bezier.BezierPoints.AsEnumerable
            Next

            Return smooth
        End Function
    End Module
End Namespace