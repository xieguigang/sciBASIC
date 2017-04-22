#Region "Microsoft.VisualBasic::b101f7b4bdc307c5a85086cec9aedcf8, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Hexagon.vb"

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
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Vector.Shapes

    ''' <summary>
    ''' 六边形
    ''' </summary>
    Public Class Hexagon

        Public Shared Sub Draw(ByRef g As IGraphics, topLeft As Point, size As Size, Optional br As Brush = Nothing, Optional border As Stroke = Nothing)
            Dim rect As New Rectangle(topLeft, size)
            Dim a As New Point(topLeft.X + size.Width / 4, topLeft.Y)
            Dim b As New Point(topLeft.X + size.Width * 3 / 4, topLeft.Y)
            Dim c As New Point(rect.Right, topLeft.Y + size.Height / 2)
            Dim d As New Point(b.X, rect.Bottom)
            Dim e As New Point(a.X, rect.Bottom)
            Dim f As New Point(topLeft.X, c.Y)
            Dim hex As New GraphicsPath

            Call hex.AddLine(a, b)
            Call hex.AddLine(b, c)
            Call hex.AddLine(c, d)
            Call hex.AddLine(d, e)
            Call hex.AddLine(e, f)
            Call hex.AddLine(f, a)
            Call hex.CloseAllFigures()

            Call g.FillPath(If(br Is Nothing, Brushes.Black, br), hex)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GDIObject, hex)
            End If
        End Sub
    End Class
End Namespace
