#Region "Microsoft.VisualBasic::330a8c00e35ea25d06f6aad73e3ddbec, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\Trigonometric\Trigonometric.vb"

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

