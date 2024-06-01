#Region "Microsoft.VisualBasic::5eab8f09a2490a791bef284a154d834b, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Hexagon.vb"

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


    ' Code Statistics:

    '   Total Lines: 39
    '    Code Lines: 30 (76.92%)
    ' Comment Lines: 3 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (15.38%)
    '     File Size: 1.42 KB


    '     Class Hexagon
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace Drawing2D.Shapes

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
            Dim css As CSSEnvirnment = g.LoadEnvironment

            Call hex.AddLine(a, b)
            Call hex.AddLine(b, c)
            Call hex.AddLine(c, d)
            Call hex.AddLine(d, e)
            Call hex.AddLine(e, f)
            Call hex.AddLine(f, a)
            Call hex.CloseAllFigures()

            Call g.FillPath(br Or BlackBrush, hex)

            If Not border Is Nothing Then
                Call g.DrawPath(css.GetPen(border), hex)
            End If
        End Sub
    End Class
End Namespace
