#Region "Microsoft.VisualBasic::da41531cb5c98c45fc965cf4ec48c798, ..\visualbasic_App\UXFramework\Molk+\WindowsApplication1\MessageBubble.vb"

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
