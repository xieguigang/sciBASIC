#Region "Microsoft.VisualBasic::35adaddef71a2be077523c9e89ef86d0, Microsoft.VisualBasic.Core\src\Extensions\Math\Trigonometric\Trigonometric.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 154
    '    Code Lines: 78 (50.65%)
    ' Comment Lines: 55 (35.71%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 21 (13.64%)
    '     File Size: 6.07 KB


    '     Module Trigonometric
    ' 
    '         Function: Angle, (+2 Overloads) Distance, (+2 Overloads) GetAngle, GetAngleVector, (+2 Overloads) MovePoint
    '                   NearestPoint, ToCartesianPoint, ToDegrees, ToRadians
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

Namespace Math

    ''' <summary>
    ''' 三角函数拓展模块
    ''' </summary>
    Public Module Trigonometric

        ''' <summary>
        ''' Polar to cartesian coordinate system point.(将极坐标转换为笛卡尔坐标系直角坐标系)
        ''' </summary>
        ''' <param name="polar">(半径, 角度)</param>
        ''' <param name="fromDegree">alpha角度参数是否是度为单位，默认是真，即函数会在这里自动转换为弧度</param>
        ''' <returns></returns>
        <Extension>
        Public Function ToCartesianPoint(polar As (r#, alpha!),
                                         Optional fromDegree As Boolean = True,
                                         Optional offsetX As Double = 0,
                                         Optional offsetY As Double = 0) As PointF
            Dim alpha As Single = polar.alpha

            If fromDegree Then
                alpha = alpha * std.PI / 180
            End If

            Dim x = polar.r * std.Cos(alpha)
            Dim y = polar.r * std.Sin(alpha)

            Return New PointF(x + offsetX, y + offsetY)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="radian">``0 -> 2*<see cref="std.PI"/>``</param>
        ''' <returns></returns>
        Public Function GetAngleVector(radian As Single, Optional r As Double = 1) As PointF
            Dim x = std.Cos(radian) * r
            Dim y = std.Sin(radian) * r

            Return New PointF(x, y)
        End Function

        ''' <summary>
        ''' 计算结果为角度
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Angle(p As PointF) As Double
            Return std.Atan2(p.Y, p.X)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Distance(a As Point, b As Point) As Double
            Return std.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Distance(a As PointF, b As PointF) As Double
            Return std.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
        End Function

        ''' <summary>
        ''' 计算两个点之间的线段的夹角
        ''' </summary>
        ''' <param name="p1"></param>
        ''' <param name="p2"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAngle(p1 As Point, p2 As Point) As Double
            Return GetAngle(p1.X, p1.Y, p2.X, p2.Y)
        End Function

        Public Function GetAngle(x1!, y1!, x2!, y2!) As Double
            Dim xDiff As Double = x2 - x1
            Dim yDiff As Double = y2 - y1

            Return 180 - (ToDegrees(std.Atan2(yDiff, xDiff)) - 90)
        End Function

        ''' <summary>
        ''' 以当前的点为圆心，向<paramref name="angle"/>方向移动给定的距离
        ''' </summary>
        ''' <param name="distance#"></param>
        ''' <param name="angle#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MovePoint(p As PointF, angle As Double, distance As Integer) As PointF
            Return New PointF With {
                .X = p.X + distance * std.Sin(angle * std.PI / 180),
                .Y = p.Y + distance * std.Cos(angle * std.PI / 180)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MovePoint(p As Point, angle As Double, distance As Integer) As Point
            Return p.PointF.MovePoint(angle, distance).ToPoint
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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToRadians(angdeg As Double) As Double
            Return angdeg / 180.0 * std.PI
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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToDegrees(angrad As Double) As Double
            Return angrad * 180.0 / std.PI
        End Function

        <Extension>
        Public Function NearestPoint(points As IEnumerable(Of Point), x%, y%, radius#) As Point
            For Each pos As Point In points
                Dim dist = std.Sqrt((x - pos.X) ^ 2 + (y - pos.Y) ^ 2)

                If dist <= radius Then
                    Return pos
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace
