﻿#Region "Microsoft.VisualBasic::5b08dac12e35da1eb1e24ab1746fb57d, Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Models\PolarPoint.vb"

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

    '   Total Lines: 58
    '    Code Lines: 32 (55.17%)
    ' Comment Lines: 18 (31.03%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (13.79%)
    '     File Size: 1.90 KB


    '     Class PolarPoint
    ' 
    '         Properties: Angle, Point, Radius
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' 极坐标点
    ''' </summary>
    Public Class PolarPoint

        Public Property Radius As Double
        ''' <summary>
        ''' Unit in degree.(单位为度)
        ''' </summary>
        ''' <returns></returns>
        Public Property Angle As Single

        Sub New(Optional radius As Double = 0, Optional angle As Double = 0)
            Me.Angle = angle
            Me.Radius = radius
        End Sub

        ''' <summary>
        ''' 与这个极坐标点等价的笛卡尔直角坐标系上面的坐标点
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个是默认以[0,0]为圆心进行计算的
        ''' </remarks>
        Public ReadOnly Property Point As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (Radius, Angle).ToCartesianPoint
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(center As PointF, Optional fromDegree As Boolean = True) As PointF
            Return (Radius, Angle).ToCartesianPoint(fromDegree, offsetX:=center.X, offsetY:=center.Y)
        End Function

        ''' <summary>
        ''' 显示这个极坐标点
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"({Radius}, {Angle}°)"
        End Function

        Public Shared Widening Operator CType(polar As (radius#, angle!)) As PolarPoint
            Return New PolarPoint With {
                .Angle = polar.angle,
                .Radius = polar.radius
            }
        End Operator
    End Class
End Namespace
