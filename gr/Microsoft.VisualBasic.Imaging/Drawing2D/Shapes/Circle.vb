#Region "Microsoft.VisualBasic::82de2410ed858d64e7e58ca39b45cfb2, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Circle.vb"

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

    '     Class Circle
    ' 
    '         Properties: FillColor, Radius, Size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Draw, (+2 Overloads) PathIterator
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports stdNum = System.Math

Namespace Drawing2D.Shapes

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
                Return Min(Size.Width, Size.Height) / 2
            End Get
        End Property

        Public Overrides Function Draw(ByRef g As IGraphics, Optional overridesLoci As Point = Nothing) As RectangleF
            Dim rect = MyBase.Draw(g, overridesLoci)
            Call Draw(g, Location, Radius, Brush)
            Return rect
        End Function

        Shared ReadOnly black As [Default](Of String) = NameOf(black)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function PathIterator(center As PointF, radius!, Optional vertices% = 30) As IEnumerable(Of PointF)
            Return PathIterator(center.X, center.Y, radius, vertices)
        End Function

        Public Shared Iterator Function PathIterator(centerX!, centerY!, radius!, Optional vertices% = 30) As IEnumerable(Of PointF)
            Dim deltaAngle# = 2 * stdNum.PI / vertices
            Dim X#, Y#

            For i As Integer = 0 To vertices - 1
                X = (radius * Cos(i * deltaAngle)) + centerX
                Y = (radius * Sin(i * deltaAngle)) + centerY

                Yield New PointF(X, Y)
            Next
        End Function

        ''' <summary>
        ''' 绘制圆
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="center"></param>
        ''' <param name="radius"></param>
        ''' <param name="br"></param>
        Public Overloads Shared Sub Draw(ByRef g As IGraphics, center As PointF, radius!,
                                         Optional br As Brush = Nothing,
                                         Optional border As Stroke = Nothing)

            Dim rect As New RectangleF With {
                .Location = New PointF(center.X - radius, center.Y - radius),
                .Size = New SizeF With {
                    .Width = radius * 2,
                    .Height = .Width
                }
            }
            Call g.FillPie(br Or BlackBrush, rect, 0, 360)

            If Not border Is Nothing Then
                rect = New RectangleF With {
                    .X = center.X - radius - border.width,
                    .Y = center.Y - radius - border.width,
                    .Width = radius * 2 + 1,
                    .Height = .Width
                }
                border.fill = border.fill Or black.When(border.fill.StringEmpty)

                Call g.DrawCircle(
                    centra:=rect.Centre,
                    r:=radius,
                    color:=border.GDIObject,
                    fill:=False
                )
            End If
        End Sub
    End Class
End Namespace
