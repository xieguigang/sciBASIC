Public Class ClosableLabel

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        MyBase.OnPaintBackground(e)

        e.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

        Dim szz = e.Graphics.MeasureString(Text, Font)
        Dim sz As Size = New Size(szz.Width, szz.Height)
        Call e.Graphics.DrawString(Text, Font, New SolidBrush(ForeColor), 3, (Height - sz.Height) / 2)
        sz = New Size(Height * 0.6, Height * 0.6)
        ClickRECt = New Rectangle(New Point(Width - sz.Width - 2, (Height - sz.Height) / 2), sz)
        Call e.Graphics.DrawImage(My.Resources.close_button, ClickRECt.Left, ClickRECt.Top, sz.Width, sz.Height)

    End Sub

    Private Sub ClosableLabel_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
        Dim pt = e.Location

        If pt.X >= ClickRECt.X - 2 AndAlso pt.X <= ClickRECt.Right + 2 AndAlso
            pt.Y >= ClickRECt.Y - 2 AndAlso pt.Y <= ClickRECt.Bottom + 2 Then
            RaiseEvent CloseInvoke()
        End If
    End Sub

    Dim ClickRECt As Rectangle

    Public Event CloseInvoke()

End Class
