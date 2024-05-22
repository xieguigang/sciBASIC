#Region "Microsoft.VisualBasic::7f8847e2c4458ab02e9609836f29d98c, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Arrow.vb"

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

    '   Total Lines: 138
    '    Code Lines: 80 (57.97%)
    ' Comment Lines: 41 (29.71%)
    '    - Xml Docs: 95.12%
    ' 
    '   Blank Lines: 17 (12.32%)
    '     File Size: 5.61 KB


    '     Class Arrow
    ' 
    '         Properties: BodyHeightPercentage, BodySize, Color, DirectionLeft, HeadLength
    '                     HeadLengthPercentage, HeadSemiHeight, Left, Right, Size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ArrowHead, Draw, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Shapes

    ''' <summary>
    ''' 按照任意角度旋转的箭头对象
    ''' </summary>
    Public Class Arrow : Inherits Shape

        ''' <summary>
        ''' 箭头的头部占据整个长度的百分比
        ''' </summary>
        ''' <returns></returns>
        Public Property HeadLengthPercentage As Single = 0.15
        ''' <summary>
        ''' 箭头的主体部分占据整个高度的百分比
        ''' </summary>
        ''' <returns></returns>
        Public Property BodyHeightPercentage As Single = 0.85

        Public Property Color As Color
        Public Property BodySize As Size
        Public Property DirectionLeft As Boolean

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Location">箭头头部的位置</param>
        ''' <param name="Size">高度和宽度</param>
        ''' <param name="Color">填充的颜色</param>
        Sub New(Location As Point, Size As Size, Color As Color)
            Call MyBase.New(Location)
            Me.Color = Color
            Me.BodySize = Size
        End Sub

        Sub New(source As Arrow)
            Call MyBase.New(source.Location)

            Me.BodyHeightPercentage = source.BodyHeightPercentage
            Me.BodySize = source.BodySize
            Me.Color = source.Color
            Me.DirectionLeft = source.DirectionLeft
            Me.EnableAutoLayout = source.EnableAutoLayout
            Me.HeadLengthPercentage = source.HeadLengthPercentage
            Me.TooltipTag = source.TooltipTag
        End Sub

        ''' <summary>
        ''' 返回图形上面的绘图的大小，而非箭头本身的大小
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property Size As Size
            Get
                Return BodySize
            End Get
        End Property

        Protected ReadOnly Property HeadLength As Integer
            Get
                Return HeadLengthPercentage * BodySize.Width
            End Get
        End Property

        Protected ReadOnly Property HeadSemiHeight As Integer
            Get
                Return (BodySize.Height * (1 - BodyHeightPercentage)) / 2
            End Get
        End Property

        ''' <summary>
        ''' 忽略了箭头的方向，本箭头对象存粹的在进行图形绘制的时候的左右的位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Left As Integer
            Get
                Return {Location.X, Location.X + If(Not DirectionLeft, 1, -1) * BodySize.Width}.Min
            End Get
        End Property
        ''' <summary>
        ''' 忽略了箭头的方向，本箭头对象存粹的在进行图形绘制的时候的左右的位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Right As Integer
            Get
                Return {Location.X, Location.X + If(Not DirectionLeft, 1, -1) * BodySize.Width}.Max
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Left} ==> {Right}; // length={BodySize.ToString}"
        End Function

        ''' <summary>
        '''  /|_____
        ''' /       |
        ''' \       |
        '''  \|-----
        ''' </summary>
        Public Overrides Function Draw(ByRef g As IGraphics, Optional overridesLoci As Point = Nothing) As RectangleF
            Dim Path As New GraphicsPath
            Dim Direction As Integer = If(DirectionLeft, 1, -1)
            Dim Top As Integer = Me.Location.Y - BodySize.Height / 2
            Dim Left = Me.Location.X
            Dim Right = Left + Direction * BodySize.Width
            Dim Bottom = Top + BodySize.Height
            Dim prePoint As New Value(Of Point)

            Call Path.AddLine(Me.Location, prePoint = New Point(Left + Direction * HeadLength, Top))                        '/
            Call Path.AddLine(prePoint.Value, prePoint = New Point(Left + Direction * HeadLength, Top + HeadSemiHeight))    ' |
            Call Path.AddLine(prePoint.Value, prePoint = New Point(Right, Top + HeadSemiHeight))                            '  ----
            Call Path.AddLine(prePoint.Value, prePoint = New Point(Right, Bottom - HeadSemiHeight))                         '      |
            Call Path.AddLine(prePoint.Value, prePoint = New Point(Left + Direction * HeadLength, Bottom - HeadSemiHeight)) '  ----
            Call Path.AddLine(prePoint.Value, prePoint = New Point(Left + Direction * HeadLength, Bottom))                  ' |
            Call Path.AddLine(prePoint.Value, Me.Location)                                                                  '\
            Call Path.CloseFigure()

            Call g.FillPath(New SolidBrush(Me.Color), Path)

            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="height!">箭头的底部的高度的1/2</param>
        ''' <param name="length!">箭头顶部到底部的长度</param>
        ''' <returns></returns>
        Public Shared Function ArrowHead(height!, length!) As PointF()
            Dim p1 As New PointF(length, 0)  ' 顶部
            Dim p2 As New PointF(0, height)
            Dim p3 As New PointF(0, -height)
            Return {p1, p2, p3}
        End Function
    End Class
End Namespace
