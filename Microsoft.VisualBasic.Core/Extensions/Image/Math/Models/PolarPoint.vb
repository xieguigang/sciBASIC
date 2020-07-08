#Region "Microsoft.VisualBasic::608abffde3f61be9ee3d4904c843e0f5, Microsoft.VisualBasic.Core\Extensions\Image\Math\Models\PolarPoint.vb"

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

    '     Class PolarPoint
    ' 
    '         Properties: Angle, Point, Radius
    ' 
    '         Function: ToString
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

#If NET_48 Then

        ''' <summary>
        ''' 与这个极坐标点等价的笛卡尔直角坐标系上面的坐标点
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Point As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (Radius, Angle).ToCartesianPoint
            End Get
        End Property

#End If

        ''' <summary>
        ''' 显示这个极坐标点
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"({Radius}, {Angle}°)"
        End Function

#If NET_48 Then

        Public Shared Widening Operator CType(polar As (radius#, angle!)) As PolarPoint
            Return New PolarPoint With {
                .Angle = polar.angle,
                .Radius = polar.radius
            }
        End Operator

#End If
    End Class
End Namespace
