Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Interpolation

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CubicSpline(points As IEnumerable(Of PointF), Optional expected# = 100) As PointF()
            Return Interpolation.CubicSpline.RecalcSpline(points, expected).ToArray
        End Function
    End Module
End Namespace