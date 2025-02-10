#Region "Microsoft.VisualBasic::376ca2acfde7da317e468fb9a7b23c0c, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Triangle.vb"

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

    '   Total Lines: 82
    '    Code Lines: 55 (67.07%)
    ' Comment Lines: 14 (17.07%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 13 (15.85%)
    '     File Size: 2.94 KB


    '     Class Triangle
    ' 
    '         Properties: Angle, Color, Size, Vertex1, Vertex2
    '                     Vertex3
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DrawAsRightTriangle
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

    Public Class Triangle : Inherits Shape

        Public Property Color As Color

        Public Property Vertex1 As PointF
        Public Property Vertex2 As PointF
        Public Property Vertex3 As PointF
        Public Property Angle As Single

        '      vertex1
        '     /       \
        ' vertex2 -- vertex3

        ''' <summary>
        ''' get the possible shape size of the triangle shape object
        ''' </summary>
        ''' <returns>
        ''' width is the delta of the vertex2 and vertex3 x location;
        ''' height is the delta of the vertex1 with mean of vertex2 and vertex3 y location value.
        ''' </returns>
        Public Overrides ReadOnly Property Size As SizeF
            Get
                Dim height As Double = Vertex1.Y - (Vertex2.Y + Vertex3.Y) / 2
                Dim width As Double = Vertex3.X - Vertex2.X

                Return New SizeF(width, height)
            End Get
        End Property

        Sub New(Location As Point, Color As Color)
            Call MyBase.New(Location)
            Me.Color = Color
        End Sub

        Sub New(Location As PointF, Color As Color)
            Call MyBase.New(Location)
            Me.Color = Color
        End Sub

        Sub New()
            Call MyBase.New(Nothing)
        End Sub

        ''' <summary>
        ''' 直角三角形
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DrawAsRightTriangle(a As Integer, b As Integer) As Triangle
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="topLeft"></param>
        ''' <param name="size"></param>
        ''' <param name="br"></param>
        ''' <param name="border"></param>
        ''' <param name="reversed">默认是顶部向上，如果reverse为真的话，则顶部向下</param>
        Public Overloads Shared Sub Draw(ByRef g As IGraphics,
                                         topLeft As Point,
                                         size As Size,
                                         Optional br As Brush = Nothing,
                                         Optional border As Stroke = Nothing,
                                         Optional reversed As Boolean = False)
            Dim t As New GraphicsPath
            Dim css As CSSEnvirnment = g.LoadEnvironment

            If Not reversed Then
                Dim a As New Point(topLeft.X + size.Width / 2, topLeft.Y)
                Dim rect As New Rectangle(topLeft, size)
                Dim b As New Point(rect.Right, rect.Bottom)
                Dim c As New Point(rect.Left, rect.Bottom)

                Call t.AddLine(a, b)
                Call t.AddLine(b, c)
                Call t.AddLine(c, a)
            Else
                Dim b = topLeft
                Dim c = New Point(topLeft.X + size.Width, topLeft.Y)
                Dim a = New Point(topLeft.X + size.Width / 2, topLeft.Y + size.Height)

                Call t.AddLine(b, c)
                Call t.AddLine(c, a)
                Call t.AddLine(a, b)
            End If

            Call t.CloseAllFigures()
            Call g.FillPath(br Or BlackBrush, t)

            If Not border Is Nothing Then
                Call g.DrawPath(css.GetPen(border), t)
            End If
        End Sub
    End Class
End Namespace
