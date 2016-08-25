#Region "Microsoft.VisualBasic::32a170135b9dcd49b50092ed92e3709a, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\UserList\UserCard.vb"

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

Imports System.ComponentModel
Imports System.Drawing
''' <summary>
''' 一个基本的头像和用户名和小写的其他说明文本
''' </summary>
Public Class UserCard : Inherits ListControlItem

    ''' <summary>
    ''' 非选中状态只显示这个属性
    ''' </summary>
    ''' <returns></returns>
    Public Property Title As String
    Public Property Summary As String
    <Description("")>
    Public Property IconHead As Image
    ''' <summary>
    ''' 选中的颜色
    ''' </summary>
    ''' <returns></returns>
    Public Property HighlighsColor As Color = Color.DeepSkyBlue

    Protected Overrides Sub PaintDrawBackground(graph As Graphics)
        graph.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        graph.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        Dim FillColor As Color = If(Selected, HighlighsColor, Color.White)
        Dim n As Integer = CInt(Height * 0.8)

        Call graph.FillRectangle(Brushes.White, New Rectangle(New Point, Size))
        Call graph.FillRectangle(New SolidBrush(FillColor), New Rectangle(New Point(1, 1), New Size(Width - 2, Height - 2)))

        Dim hdSize As New Size(n, n)
        Dim hdPoint As New Point(3, (Height - n) / 2)

        If Not IconHead Is Nothing Then
            Call graph.DrawImage(IconHead, New Rectangle(hdPoint, hdSize))
        Else
            Call graph.FillRectangle(Brushes.Black, New Rectangle(hdPoint, hdSize))
        End If

        Dim TitleFont As New Font("Microsoft YaHei", 10)
        Dim d As Integer = 7

        If Selected Then
            Call graph.DrawString(Title, TitleFont, Brushes.Black, New Point(n + d, 5))
            Call graph.DrawString(Summary, New Font("Microsoft YaHei", 8), Brushes.White, New Point(n + d, CInt(n / 1.5)))
        Else
            Dim sz As SizeF = graph.MeasureString(Title, TitleFont)
            Call graph.DrawString(Title, TitleFont, Brushes.Black, New Point(n + 7, CInt((Height - sz.Height) / 2)))
        End If
    End Sub

    Protected Overrides Sub PaintDrawButton(gfx As Graphics)
        'Do Nothing
    End Sub

    Private Sub UserCard_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        Selected = False
    End Sub

    Private Sub UserCard_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
        Selected = True
    End Sub
End Class
