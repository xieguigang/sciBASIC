#Region "Microsoft.VisualBasic::d3769f376effdde50e6dfcc6310c7463, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Box.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20 (74.07%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (25.93%)
    '     File Size: 933 B


    '     Class Box
    ' 
    '         Properties: Size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DrawRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace Drawing2D.Shapes

    Public Class Box : Inherits Shape

        Sub New(Location As Point, Size As Size, Color As Color)
            Call MyBase.New(Location)
        End Sub

        Public Overrides ReadOnly Property Size As Size

        Public Shared Sub DrawRectangle(ByRef g As IGraphics,
                                        topLeft As Point,
                                        size As Size,
                                        Optional br As Brush = Nothing,
                                        Optional border As Stroke = Nothing)
            Dim css = g.LoadEnvironment

            Call g.FillRectangle(br Or BlackBrush, New Rectangle(topLeft, size))

            If Not border Is Nothing Then
                Call g.DrawRectangle(css.GetPen(border), New Rectangle(topLeft, size))
            End If
        End Sub
    End Class
End Namespace
