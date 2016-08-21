Imports System.Drawing
Imports System.Runtime.CompilerServices

Public Module Trigonometric

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="radian">``0 -> 2*<see cref="Math.PI"/>``</param>
    ''' <returns></returns>
    Public Function GetAngleVector(radian As Single, Optional r As Double = 1) As PointF
        Dim x = Math.Cos(radian) * r
        Dim y = Math.Sin(radian) * r

        Return New PointF(x, y)
    End Function

    ''' <summary>
    ''' 计算结果为角度
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Angle(p As PointF) As Double
        Dim a As Double = Math.Atan2(p.Y, p.X)
        Return a
    End Function

    Public Function Distance(a As Point, b As Point) As Double
        Return Math.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
    End Function

    Public Function Distance(a As PointF, b As PointF) As Double
        Return Math.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
    End Function
End Module
