#Region "Microsoft.VisualBasic::e9778a708715d824137f03893b897fb6, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ClosableLabel.vb"

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
