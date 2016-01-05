Public Class MessageBubble
    Private Sub MessageBubble_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Text = Now.ToString

        Try
            WebBrowser1.DocumentText = FileIO.FileSystem.ReadAllText("./testdialog.htm")
            Dim sz = WebBrowser1.PreferredSize
            Call sz.__DEBUG_ECHO
            Me.Size = New Size(WebBrowser1.PreferredSize.Width, WebBrowser1.PreferredSize.Height)
        Catch ex As Exception

        End Try
    End Sub

    Dim ArrowHalfHeight As Integer = 25
    Dim ArrowWidth As Integer = 30

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        MyBase.OnPaintBackground(e)

        Dim Path As New Drawing2D.GraphicsPath
        Dim FirstPoint As Point = New Point(0, Height / 2 - ArrowHalfHeight / 2)
        Dim PrePoint As Point = FirstPoint

        Call Path.AddLine(PrePoint, New Point(PrePoint.X + ArrowWidth, PrePoint.Y - ArrowHalfHeight).ShadowCopy(PrePoint))
        Call Path.AddLine(PrePoint, New Point(PrePoint.X, PrePoint.Y - Height / 2).ShadowCopy(PrePoint))
        Call Path.AddLine(PrePoint, New Point(PrePoint.X + Width - ArrowWidth, PrePoint.Y).ShadowCopy(PrePoint))
        Call Path.AddLine(PrePoint, New Point(PrePoint.X, Label1.Top - 5).ShadowCopy(PrePoint)) ' PrePoint.Y + Height - ArrowHalfHeight / 2
        Call Path.AddLine(PrePoint, New Point(ArrowWidth, PrePoint.Y).ShadowCopy(PrePoint))
        Call Path.AddLine(PrePoint, New Point(PrePoint.X, PrePoint.Y - ArrowHalfHeight).ShadowCopy(PrePoint))
        Call Path.AddLine(PrePoint, FirstPoint)

        Dim brush As New SolidBrush(color:=Color.Chocolate)

        Call e.Graphics.FillPath(brush, path:=Path)
    End Sub
End Class
