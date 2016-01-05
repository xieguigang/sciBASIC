
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
