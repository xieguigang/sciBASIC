#Region "Microsoft.VisualBasic::f9d4f4f2bc3f41d5a64ef8f880078e0f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\Trigonometric\Trigonometric.vb"

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

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports sys = System.Math

Namespace Math

    Public Module Trigonometric

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="r#"></param>
        ''' <param name="alpha!"></param>
        ''' <param name="fromDegree"><paramref name="alpha"/>角度参数是否是度为单位，默认是真，即函数会在这里自动转换为弧度</param>
        ''' <returns></returns>
        <Extension> Public Function ToPoint(r#, alpha!, Optional fromDegree As Boolean = True) As PointF
            If fromDegree Then
                alpha = alpha * sys.PI / 180
            End If

            Dim x = r * sys.Cos(alpha)
            Dim y = r * sys.Sin(alpha)

            Return New PointF(x, y)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="radian">``0 -> 2*<see cref="Math.PI"/>``</param>
        ''' <returns></returns>
        Public Function GetAngleVector(radian As Single, Optional r As Double = 1) As PointF
            Dim x = sys.Cos(radian) * r
            Dim y = sys.Sin(radian) * r

            Return New PointF(x, y)
        End Function

        ''' <summary>
        ''' 计算结果为角度
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Angle(p As PointF) As Double
            Dim a As Double = sys.Atan2(p.Y, p.X)
            Return a
        End Function

        Public Function Distance(a As Point, b As Point) As Double
            Return sys.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
        End Function

        Public Function Distance(a As PointF, b As PointF) As Double
            Return sys.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
        End Function

        Public Function GetAngle(p1 As Point, p2 As Point) As Double
            Dim xDiff As Double = p2.X - p1.X
            Dim yDiff As Double = p2.Y - p1.Y
            Return 180 - (ToDegrees(sys.Atan2(yDiff, xDiff)) - 90)
        End Function

        <Extension>
        Public Function MovePoint(p As Point, angle As Double, distance As Integer) As Point
            p = New Point(p)
            p.X += distance * sys.Sin(angle)
            p.Y += distance * sys.Cos(angle)

            Return p
        End Function

        ''' <summary>
        ''' Converts an angle measured in degrees to an approximately
        ''' equivalent angle measured in radians.  The conversion from
        ''' degrees to radians is generally inexact.
        ''' </summary>
        ''' <param name="angdeg">   an angle, in degrees </param>
        ''' <returns>  the measurement of the angle {@code angdeg}
        '''          in radians.
        ''' @since   1.2 </returns>
        <Extension> Public Function ToRadians(angdeg As Double) As Double
            Return angdeg / 180.0 * sys.PI
        End Function

        ''' <summary>
        ''' Converts an angle measured in radians to an approximately
        ''' equivalent angle measured in degrees.  The conversion from
        ''' radians to degrees is generally inexact; users should
        ''' <i>not</i> expect {@code cos(toRadians(90.0))} to exactly
        ''' equal {@code 0.0}.
        ''' </summary>
        ''' <param name="angrad">   an angle, in radians </param>
        ''' <returns>  the measurement of the angle {@code angrad}
        '''          in degrees.
        ''' @since   1.2 </returns>
        <Extension> Public Function ToDegrees(angrad As Double) As Double
            Return angrad * 180.0 / sys.PI
        End Function

        <Extension>
        Public Function NearestPoint(points As IEnumerable(Of Point), x As Integer, y As Integer, radius As Integer) As Point
            For Each pos As Point In points
                Dim dist As Double = sys.Sqrt((x - pos.X) ^ 2 + sys.Pow(y - pos.Y, 2))
                If dist <= radius Then
                    Return pos
                End If
            Next
            Return Nothing
        End Function
    End Module
End Namespace
