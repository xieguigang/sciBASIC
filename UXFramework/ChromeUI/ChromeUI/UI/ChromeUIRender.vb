Imports System.Drawing
Imports System.Windows.Forms

Public Class ChromeUIRender : Inherits ToolStripSystemRenderer

    ''' <summary>
    ''' Draw MenuItem Background of MBMenuStrip
    ''' </summary>
    Protected Overrides Sub OnRenderMenuItemBackground(e As ToolStripItemRenderEventArgs)
        Dim g As Graphics = e.Graphics
        Dim rect As Rectangle = New Rectangle(New Point, e.Item.Size)

        If e.Item.Selected Then
            g.FillRectangle(__chromeMenuSelected, rect)
        Else
            g.FillRectangle(Brushes.White, rect)
        End If
    End Sub

    ReadOnly __chromeMenuSelected As New SolidBrush(Color.FromArgb(66, 129, 244))
    ReadOnly __chromeSeperator As New Pen(New SolidBrush(Color.FromArgb(233, 233, 233)), 1)

    Protected Overrides Sub OnRenderSeparator(e As ToolStripSeparatorRenderEventArgs)
        Dim h As Integer = e.Item.Height / 2

        e.Graphics.FillRectangle(Brushes.White, New Rectangle(New Point, New Size(e.ToolStrip.Width, e.Item.Height)))
        e.Graphics.DrawLine(__chromeSeperator, New Point(0, h), New Point(e.ToolStrip.Width, h))
    End Sub
End Class




