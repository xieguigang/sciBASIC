﻿#Region "Microsoft.VisualBasic::6d74f93d76061ef74bce15538772d2b8, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Diamond.vb"

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

    '     Class Diamond
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Shapes

    Public Class Diamond

        Public Shared Sub Draw(ByRef g As IGraphics,
                               topLeft As Point,
                               size As Size,
                               Optional br As Brush = Nothing,
                               Optional border As Stroke = Nothing)

            Dim a As New Point(topLeft.X + size.Width / 2, topLeft.Y)
            Dim b As New Point(topLeft.X + size.Width, topLeft.Y + size.Height / 2)
            Dim c As New Point(a.X, topLeft.Y + size.Height)
            Dim d As New Point(topLeft.X, b.Y)
            Dim diamond As New GraphicsPath

            diamond.AddLine(a, b)
            diamond.AddLine(b, c)
            diamond.AddLine(c, d)
            diamond.AddLine(d, a)
            diamond.CloseFigure()

            Call g.FillPath(br Or BlackBrush, diamond)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GDIObject, diamond)
            End If
        End Sub
    End Class
End Namespace
