Imports System.Windows.Forms

''' <summary>
''' 三维画布页：把 3D 画面<b>直接画在页面自身上</b>。
''' </summary>
''' <remarks>
''' 不使用 <c>PictureBox</c> 子控件，是因为实测 <see cref="TabPage"/> 的布局引擎
''' 不会可靠地把 <c>Dock=Fill</c> 子控件重排到新尺寸（页面已变成 1110x819，
''' 子控件仍停留在设计尺寸 884x497，且显式设尺寸会被同步驳回）。
''' 而 TabPage 自身尺寸始终是正确的，因此让页面自己当画布最稳。
''' 打开双缓冲以避免每帧重绘闪烁。
''' </remarks>
Public Class CanvasTabPage
    Inherits TabPage

    Public Sub New()
        Call SetStyle(ControlStyles.UserPaint Or
                      ControlStyles.AllPaintingInWmPaint Or
                      ControlStyles.OptimizedDoubleBuffer Or
                      ControlStyles.ResizeRedraw, True)

        Call UpdateStyles()
    End Sub
End Class
