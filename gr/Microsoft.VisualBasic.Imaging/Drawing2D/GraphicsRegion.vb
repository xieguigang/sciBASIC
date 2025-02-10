#Region "Microsoft.VisualBasic::fbaa42fc3472a373e947b99a5a9db1bc, gr\Microsoft.VisualBasic.Imaging\Drawing2D\GraphicsRegion.vb"

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

    '   Total Lines: 182
    '    Code Lines: 118 (64.84%)
    ' Comment Lines: 37 (20.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 27 (14.84%)
    '     File Size: 6.47 KB


    '     Structure GraphicsRegion
    ' 
    '         Properties: EntireArea, Height, Width
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Bottom, GetAbsolutePadding, GetXLinearScaleRange, GetYLinearScaleRange, Offset2D
    '                   PlotRegion, scaler, TopCentra, ToString, XRange
    '                   XScaler, YRange, YScaler
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D

    ''' <summary>
    ''' 绘图区域的参数
    ''' </summary>
    Public Structure GraphicsRegion

        ''' <summary>
        ''' 整张画布的大小
        ''' </summary>
        Dim Size As Size
        ''' <summary>
        ''' 画布的边留白
        ''' </summary>
        Dim Padding As Padding

        Dim device As DeviceDescription

#Region "property based on the two fields value"

        ''' <summary>
        ''' Get the width of the entire canvas <see cref="Size"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Width As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Size.Width
            End Get
        End Property

        ''' <summary>
        ''' Get the height of the entire canvas <see cref="Size"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Size.Height
            End Get
        End Property

        ''' <summary>
        ''' 整张画布的大小区域
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EntireArea As Rectangle
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Rectangle(New Point, Size)
            End Get
        End Property
#End Region

        <DebuggerStepThrough>
        Sub New(size As Size, padding As Padding)
            Me.Size = size
            Me.Padding = padding
        End Sub

        <DebuggerStepThrough>
        Sub New(padding As Padding, size As Size)
            Me.Size = size
            Me.Padding = padding
        End Sub

        Sub New(size As Size, padding As Integer())
            Me.Size = size
            Me.Padding = New Padding(padding)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAbsolutePadding() As PaddingLayout
            Return PaddingLayout.EvaluateFromCSS(New CSSEnvirnment(Size), Padding)
        End Function

        ''' <summary>
        ''' 绘图区域的底部Y坐标值
        ''' </summary>
        ''' <returns></returns>
        Public Function Bottom(css As CSSEnvirnment) As Integer
            Return Size.Height - css.GetHeight(Padding.Bottom)
        End Function

        ''' <summary>
        ''' ``[left, right]`` as <see cref="DoubleRange"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function XRange(css As CSSEnvirnment) As String
            With Padding
                Return $"{css.GetWidth(.Left)},{Width - css.GetWidth(.Right)}"
            End With
        End Function

        ''' <summary>
        ''' ``[top, bottom]`` as <see cref="DoubleRange"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function YRange(css As CSSEnvirnment) As String
            With Padding
                Return $"{css.GetHeight(.Top)},{Height - css.GetHeight(.Bottom)}"
            End With
        End Function

        ''' <summary>
        ''' 整张画布出去margin部分剩余的可供绘图的区域
        ''' </summary>
        ''' <returns></returns>
        Public Function PlotRegion(css As CSSEnvirnment) As Rectangle
            Dim topLeft As New Point(css.GetWidth(Padding.Left), css.GetHeight(Padding.Top))
            Dim size As New Size With {
                .Width = Me.Size.Width - Padding.Horizontal(css),
                .Height = Me.Size.Height - Padding.Vertical(css)
            }

            Return New Rectangle(topLeft, size)
        End Function

        Public Function GetXLinearScaleRange(css As CSSEnvirnment) As Double()
            Return New Double() {
                css.GetWidth(Padding.Left),
                Size.Width - css.GetWidth(Padding.Right)
            }
        End Function

        Public Function GetYLinearScaleRange(css As CSSEnvirnment) As Double()
            Return New Double() {
                css.GetHeight(Padding.Top),
                Size.Height - css.GetHeight(Padding.Bottom)
            }
        End Function

        Public Function TopCentra(size As Size, css As CSSEnvirnment) As Point
            Dim left = (Me.Size.Width - size.Width) / 2
            Dim top = (css.GetHeight(Padding.Top) - size.Height) / 2

            Return New Point(left, top)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function XScaler(xrange As DoubleRange, css As CSSEnvirnment) As Func(Of Double, Double)
            Return scaler(xrange, DoubleRange.TryParse(Me.XRange(css)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function YScaler(yrange As DoubleRange, css As CSSEnvirnment) As Func(Of Double, Double)
            Dim rect = PlotRegion(css)
            Dim scaler = GraphicsRegion.scaler(yrange, New DoubleRange(0, rect.Height))
            Dim bottom = rect.Bottom

            Return Function(y) bottom - scaler(y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function scaler(range As DoubleRange, plotRange As DoubleRange) As Func(Of Double, Double)
            Return Function(x)
                       Return range.ScaleMapping(x, plotRange)
                   End Function
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Offset2D(dx As Double, dy As Double) As GraphicsRegion
            Return New GraphicsRegion With {
                .Size = Size,
                .Padding = Padding.Offset2D(dx, dy, New CSSEnvirnment(Size))
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
