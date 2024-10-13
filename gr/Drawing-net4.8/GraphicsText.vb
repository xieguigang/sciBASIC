#Region "Microsoft.VisualBasic::738682e48d3c0d6023b5998138e9303b, gr\Drawing-net4.8\GraphicsText.vb"

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

    '   Total Lines: 148
    '    Code Lines: 94 (63.51%)
    ' Comment Lines: 32 (21.62%)
    '    - Xml Docs: 68.75%
    ' 
    '   Blank Lines: 22 (14.86%)
    '     File Size: 5.34 KB


    ' Class GraphicsText
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: ConvertSize, GetRotatePoint
    ' 
    '     Sub: (+2 Overloads) DrawString, DrawStringInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language.Default

''' <summary>
''' 利用GDI+绘制旋转文字，矩形内可以根据布局方式排列文本
''' </summary>
''' <remarks>http://www.xuebuyuan.com/1613072.html</remarks>
Public Class GraphicsText

    ReadOnly g As Graphics
    ReadOnly defaultFormat As New [Default](Of StringFormat)(Function() New StringFormat, isLazy:=False)

    Public Sub New(g As Graphics)
        Me.g = g
    End Sub

    Public Sub New(g As Graphics2D)
        Me.g = g.Graphics
    End Sub

    ''' <summary>
    ''' 绘制根据矩形旋转文本
    ''' </summary>
    ''' <param name="s">文本</param>
    ''' <param name="font">字体</param>
    ''' <param name="brush">填充</param>
    ''' <param name="layoutRectangle">局部矩形</param>
    ''' <param name="format">布局方式</param>
    ''' <param name="angle">角度</param>
    Public Sub DrawString(s$, font As Font, brush As Brush, layoutRectangle As RectangleF, format As StringFormat, angle!)
        ' 求取字符串大小        
        Dim size As SizeF = g.MeasureString(s, font)
        ' 根据旋转角度，求取旋转后字符串大小       
        Dim sizeRotate As SizeF = ConvertSize(size, angle)
        ' 根据旋转后尺寸、布局矩形、布局方式计算文本旋转点          
        Dim rotatePt As PointF = GetRotatePoint(sizeRotate, layoutRectangle, format)
        ' 重设布局方式都为Center
        Dim newFormat As New StringFormat(format) With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }

        ' 绘制旋转后文本
        Call DrawString(s, font, brush, rotatePt, angle, newFormat)
    End Sub

    ''' <summary>
    ''' 绘制根据点旋转文本，一般旋转点给定位文本包围盒中心点
    ''' </summary>
    ''' <param name="s">文本</param>
    ''' <param name="font">字体</param>
    ''' <param name="brush">填充</param>
    ''' <param name="point">旋转点</param>
    ''' <param name="format">布局方式</param>
    ''' <param name="angle">角度</param>
    Public Sub DrawString(s$, font As Font, brush As Brush, point As PointF, angle!, Optional format As StringFormat = Nothing)
        SyncLock g
            Call DrawStringInternal(s, font, brush, point, format Or defaultFormat, angle)
        End SyncLock
    End Sub

    Private Sub DrawStringInternal(s$, font As Font, brush As Brush, point As PointF, format As StringFormat, angle!)
        ' Save the matrix
        Dim mtxSave As Matrix = g.Transform
        Dim mtxRotate As Matrix = g.Transform

        Call mtxRotate.RotateAt(angle, point)

        With g
            .Transform = mtxRotate
            .DrawString(s, font, brush, point, format)
            ' Reset the matrix
            .Transform = mtxSave
        End With
    End Sub

    Private Function ConvertSize(size As SizeF, angle!) As SizeF
        Dim matrix As New Matrix()

        Call matrix.Rotate(angle)

        ' 旋转矩形四个顶点
        Dim pts As PointF() = New PointF(3) {}

        pts(0).X = -size.Width / 2.0F
        pts(0).Y = -size.Height / 2.0F
        pts(1).X = -size.Width / 2.0F
        pts(1).Y = size.Height / 2.0F
        pts(2).X = size.Width / 2.0F
        pts(2).Y = size.Height / 2.0F
        pts(3).X = size.Width / 2.0F
        pts(3).Y = -size.Height / 2.0F

        Call matrix.TransformPoints(pts)

        ' 求取四个顶点的包围盒
        Dim left As Single = Single.MaxValue
        Dim right As Single = Single.MinValue
        Dim top As Single = Single.MaxValue
        Dim bottom As Single = Single.MinValue

        For Each pt As PointF In pts
            ' 求取并集
            If pt.X < left Then
                left = pt.X
            End If
            If pt.X > right Then
                right = pt.X
            End If
            If pt.Y < top Then
                top = pt.Y
            End If
            If pt.Y > bottom Then
                bottom = pt.Y
            End If
        Next

        Return New SizeF(right - left, bottom - top)
    End Function

    Private Function GetRotatePoint(size As SizeF, layoutRectangle As RectangleF, format As StringFormat) As PointF
        Dim x!, y!

        Select Case format.Alignment
            Case StringAlignment.Near
                x = layoutRectangle.Left + size.Width / 2.0F
            Case StringAlignment.Center
                x = (layoutRectangle.Left + layoutRectangle.Right) / 2.0F
            Case StringAlignment.Far
                x = layoutRectangle.Right - size.Width / 2.0F
            Case Else
        End Select

        Select Case format.LineAlignment
            Case StringAlignment.Near
                y = layoutRectangle.Top + size.Height / 2.0F
            Case StringAlignment.Center
                y = (layoutRectangle.Top + layoutRectangle.Bottom) / 2.0F
            Case StringAlignment.Far
                y = layoutRectangle.Bottom - size.Height / 2.0F
            Case Else
        End Select

        Return New PointF(x, y)
    End Function
End Class
