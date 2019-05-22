#Region "Microsoft.VisualBasic::a52f224044e46fb7dcafc4ecf1739dbf, Microsoft.VisualBasic.Core\ApplicationServices\Tools\WinForm\ChromeUIRender.vb"

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

    '     Class ChromeUIRender
    ' 
    '         Sub: OnRenderMenuItemBackground, OnRenderSeparator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Windows.Forms

    Public Class ChromeUIRender : Inherits ToolStripSystemRenderer

        ''' <summary>
        ''' Draw MenuItem Background of MBMenuStrip
        ''' </summary>
        Protected Overrides Sub OnRenderMenuItemBackground(e As ToolStripItemRenderEventArgs)
            Dim g As Graphics = e.Graphics
            Dim rect As New Rectangle With {
                .Location = New Point,
                .Size = e.Item.Size
            }

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
            Dim layout As New Rectangle With {
                .Location = New Point,
                .Size = New Size With {
                    .Width = e.ToolStrip.Width,
                    .Height = e.Item.Height
                }
            }
            Dim pa As New Point(0, h)
            Dim pb As New Point With {
                .X = e.ToolStrip.Width,
                .Y = h
            }

            e.Graphics.FillRectangle(Brushes.White, layout)
            e.Graphics.DrawLine(__chromeSeperator, pa, pb)
        End Sub
    End Class
End Namespace
