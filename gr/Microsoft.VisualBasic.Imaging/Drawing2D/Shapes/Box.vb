#Region "Microsoft.VisualBasic::27ec635455d2233eda49e7b1ef310f65, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Box.vb"

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

    '   Total Lines: 70
    '    Code Lines: 39 (55.71%)
    ' Comment Lines: 19 (27.14%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 12 (17.14%)
    '     File Size: 2.33 KB


    '     Class Box
    ' 
    '         Properties: border, box, fill, Size
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: DrawRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace Drawing2D.Shapes

    ''' <summary>
    ''' rectangle model
    ''' </summary>
    Public Class Box : Inherits Shape

        Public Property box As SizeF
        Public Property fill As String
        Public Property border As Stroke

        Public Overrides ReadOnly Property Size As SizeF
            Get
                Return box
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Location"></param>
        ''' <param name="Size"></param>
        ''' <param name="Color">the fill color of the rectangle object</param>
        Sub New(location As Point, size As Size, color As Color)
            Call MyBase.New(location)
        End Sub

        ''' <summary>
        ''' Create new rectangle box with fill color
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <param name="color">the fill color</param>
        Sub New(rect As Rectangle, color As Color)
            Call MyBase.New(rect.Location)

            Me.box = New SizeF(rect.Width, rect.Height)
            Me.fill = color.ToHtmlColor
        End Sub

        ''' <summary>
        ''' Create new rectangle box with fill color
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <param name="color">the fill color</param>
        Sub New(rect As RectangleF, color As Color)
            Call MyBase.New(rect.Location)

            Me.box = rect.Size
            Me.fill = color.ToHtmlColor
        End Sub

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
