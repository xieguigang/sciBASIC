#Region "Microsoft.VisualBasic::8bc08d9f05783835a22defa6d3bde2f9, Data_science\Visualization\Plots\g\Axis\DataScaler\DataScaler.vb"

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

    '   Total Lines: 158
    '    Code Lines: 107 (67.72%)
    ' Comment Lines: 26 (16.46%)
    '    - Xml Docs: 96.15%
    ' 
    '   Blank Lines: 25 (15.82%)
    '     File Size: 5.37 KB


    '     Class DataScaler
    ' 
    '         Properties: AxisTicks, X, xmax, xmin, xscale
    '                     ymax, ymin
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+3 Overloads) Translate, (+2 Overloads) TranslateSize, TranslateWidth, (+2 Overloads) TranslateX
    ' 
    '     Module DataScalerExtensions
    ' 
    '         Function: TupleScaler
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Graphic.Axis

    ''' <summary>
    ''' 将用户数据转换为作图的时候所需要的空间数据
    ''' </summary>
    Public Class DataScaler : Inherits YScaler

        ''' <summary>
        ''' X坐标轴主要是为了兼容数值类型的连续映射以及标签类型的坐标值的离散映射
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Scaler
        Public Property AxisTicks As (X As Vector, Y As Vector)

        Public ReadOnly Property xscale As scalers
            Get
                Return X.type
            End Get
        End Property

        Public ReadOnly Property xmin As Double
            Get
                Return AxisTicks.X.Min
            End Get
        End Property

        Public ReadOnly Property xmax As Double
            Get
                Return AxisTicks.X.Max
            End Get
        End Property

        Public ReadOnly Property ymin As Double
            Get
                Return AxisTicks.Y.Min
            End Get
        End Property

        Public ReadOnly Property ymax As Double
            Get
                Return AxisTicks.Y.Max
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rev">是否需要将Y坐标轴上下翻转颠倒</param>
        Sub New(Optional rev As Boolean = False)
            Call MyBase.New(reversed:=rev)
        End Sub

        Sub New(x As Scaler, y As Scaler, Optional rev As Boolean = False)
            Call Me.New(rev)

            Me.X = x
            Me.Y = y
        End Sub

        ''' <summary>
        ''' translate the realworld data into the view model world point 2D 
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(x#, y#) As PointF
            Return New PointF With {
                .X = TranslateX(x),
                .Y = TranslateY(y)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(point As PointData) As PointF
            Dim x As Single

            If TypeOf Me.X Is OrdinalScale Then
                x = DirectCast(Me.X, OrdinalScale)(point.axisLabel)
            Else
                x = DirectCast(Me.X, LinearScale)(point.pt.X)
            End If

            Return New PointF(x, TranslateY(point.pt.Y))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(point As PointF) As PointF
            Return Translate(point.X, point.Y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x#) As Double
            Return Me.X(x)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateX(x As String) As Double
            Return Me.X(x)
        End Function

        Public Function TranslateWidth(x1 As Double, x2 As Double) As Double
            x1 = TranslateX(x1)
            x2 = TranslateX(x2)

            Return std.Max(x1, x2) - std.Min(x1, x2)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateSize(size As SizeF) As SizeF
            Return TranslateSize(size.Width, size.Height)
        End Function

        Public Function TranslateSize(w As Double, h As Double) As SizeF
            Dim x1 = TranslateX(0)
            Dim x2 = TranslateX(w)
            Dim y1 = TranslateY(0)
            Dim y2 = TranslateY(h)

            Return New SizeF(std.Abs(x1 - x2), std.Abs(y1 - y2))
        End Function
    End Class

    <HideModuleName>
    Public Module DataScalerExtensions

        ''' <summary>
        ''' Translate the x/y value as a geom point.
        ''' </summary>
        ''' <param name="scaler"></param>
        ''' <param name="bottom">
        ''' 如果是正常的坐标系，那么这个值就必须是一个正数，值为绘图区域的<paramref name="bottom"/>的y值，
        ''' 否则获取得到的y值将会是颠倒过来的，除非将<see cref="IGraphics"/>的旋转矩阵给颠倒了
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
End Namespace
