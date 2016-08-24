#Region "Microsoft.VisualBasic::532922c503977132167e17fa48b820fa, ..\visualbasic_App\UXFramework\ChromeUI\ChromeUI\ChromeUIRender.vb"

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
