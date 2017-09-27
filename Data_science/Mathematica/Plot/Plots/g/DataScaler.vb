#Region "Microsoft.VisualBasic::a36147bf0a06c402ad28c0cbc69c6fb2, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\g\DataScaler.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Graphic

    Public Module DataScalerExtensions

        ''' <summary>
        ''' Translate the x/y value as a geom point.
        ''' </summary>
        ''' <param name="scaler"></param>
        ''' <param name="bottom">
        ''' 如果是正常的坐标系，那么这个值就必须是一个正数，值为绘图区域的<paramref name="bottom"/>的y值，
        ''' 否则获取得到的y值将会是颠倒过来的，除非将<see cref="Graphics"/>的旋转矩阵给颠倒了
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function TupleScaler(scaler As (x As d3js.scale.LinearScale, y As d3js.scale.LinearScale), Optional bottom% = -1) As Func(Of Double, Double, PointF)
            With scaler
                If bottom > 0 Then
                    Return Function(x, y)
                               Return New PointF(.x(x), bottom - .y(y))
                           End Function
                Else
                    Return Function(x, y)
                               Return New PointF(.x(x), .y(y))
                           End Function
                End If
            End With
        End Function
    End Module

    Public Structure DataScaler

        Dim X As LinearScale
        Dim Y As LinearScale
        Dim AxisTicks As (X As Vector, Y As Vector)
        Dim ChartRegion As Rectangle

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(x#, y#) As PointF
            Return New PointF With {
                .X = TranslateX(x),
                .Y = TranslateY(y)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x#) As Double
            Return Me.X(x)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateY(y#) As Double
            Return ChartRegion.Bottom - Me.Y(y)
        End Function
    End Structure
End Namespace
