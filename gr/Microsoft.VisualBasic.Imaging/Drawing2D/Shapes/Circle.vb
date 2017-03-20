#Region "Microsoft.VisualBasic::62068313aedd93a8660ff696de63e012, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Circle.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Vector.Shapes

    Public Class Circle : Inherits Shape

        Dim Brush As SolidBrush

        Public Property FillColor As Color
            Get
                Return Brush.Color
            End Get
            Set(value As Color)
                Brush = New SolidBrush(value)
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="topLeft">左上角</param>
        ''' <param name="d">圆的直径</param>
        ''' <remarks></remarks>
        Public Sub New(topLeft As Point, d As Integer, FillColor As Color)
            Call MyBase.New(topLeft)
            _Size = New Size(d, d)
            Me.FillColor = FillColor
        End Sub

        Public Sub New(d%, fill As Color)
            Me.New(Nothing, d, fill)
        End Sub

        Public Overrides ReadOnly Property Size As Size

        Public ReadOnly Property Radius As Single
            Get
                Return Math.Min(Size.Width, Size.Height) / 2
            End Get
        End Property

        Public Overrides Function Draw(ByRef g As Graphics, Optional overridesLoci As Point = Nothing) As RectangleF
            Dim rect = MyBase.Draw(g, overridesLoci)
            Call Draw(g, Location, Radius, Brush)
            Return rect
        End Function

        ''' <summary>
        ''' 绘制圆
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="center"></param>
        ''' <param name="radius"></param>
        ''' <param name="br"></param>
        Public Overloads Shared Sub Draw(ByRef g As Graphics,
                                         center As Point,
                                         radius As Single,
                                         Optional br As Brush = Nothing,
                                         Optional border As Stroke = Nothing)
            Dim rect As New Rectangle(
                New Point(center.X - radius, center.Y - radius),
                New Size(radius * 2, radius * 2))
            Call g.FillPie(
                If(br Is Nothing, Brushes.Black, br), rect, 0, 360)

            If Not border Is Nothing Then
                rect = New Rectangle(
                    center.X - radius - border.width,
                    center.Y - radius - border.width,
                    radius * 2 + 1,
                    radius * 2 + 1)
                border.fill = If(border.fill.StringEmpty, "Black", border.fill)

                Call g.DrawPie(border.GDIObject, rect, 0, 360)
            End If
        End Sub
    End Class
End Namespace
