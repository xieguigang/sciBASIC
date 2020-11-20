#Region "Microsoft.VisualBasic::47c4a3b3242ab0c3c4ef03cec58f1542, mime\text%html\HTML\CSS\Render\CssDrawingHelper.vb"

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

    '     Class CssDrawingHelper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Enum Border
    ' 
    '             Bottom, Left, Right, Top
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: CreateCorner, GetBorderPath, GetRoundRect, RoundP, RoundR
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports rect = System.Drawing.Rectangle

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Provides some drawing functionallity
    ''' </summary>
    Friend NotInheritable Class CssDrawingHelper
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Border specifiers
        ''' </summary>
        Friend Enum Border
            Top
            Right
            Bottom
            Left
        End Enum

        ''' <summary>
        ''' Rounds the specified point
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="b"></param>Module
        ''' <returns></returns>
        Private Shared Function RoundP(p As PointF, b As CssBox) As PointF
            'HACK: Don't round if in printing mode
            'return Point.Round(p);
            Return p
        End Function

        ''' <summary>
        ''' Rounds the specified rectangle
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Shared Function RoundR(r As RectangleF, b As CssBox) As RectangleF
            'HACK: Don't round if in printing mode
            Return rect.Round(r)
        End Function

        ''' <summary>
        ''' Makes a border path
        ''' </summary>
        ''' <param name="border">Desired border</param>
        ''' <param name="b">Box wich the border corresponds</param>
        ''' <param name="isLineStart">Specifies if the border is for a starting line (no bevel on left)</param>
        ''' <param name="isLineEnd">Specifies if the border is for an ending line (no bevel on right)</param>
        ''' <returns>Beveled border path</returns>
        Public Shared Function GetBorderPath(border As Border, b As CssBox, r As RectangleF, isLineStart As Boolean, isLineEnd As Boolean) As GraphicsPath
            Dim pts As PointF() = New PointF(3) {}
            Dim bwidth As Single = 0
            Dim corner As GraphicsPath = Nothing

            Select Case border
                Case Border.Top
                    bwidth = b.ActualBorderTopWidth
                    pts(0) = RoundP(New PointF(r.Left + b.ActualCornerNW, r.Top), b)
                    pts(1) = RoundP(New PointF(r.Right - b.ActualCornerNE, r.Top), b)
                    pts(2) = RoundP(New PointF(r.Right - b.ActualCornerNE, r.Top + bwidth), b)
                    pts(3) = RoundP(New PointF(r.Left + b.ActualCornerNW, r.Top + bwidth), b)

                    If isLineEnd AndAlso b.ActualCornerNE = 0F Then
                        pts(2).X -= b.ActualBorderRightWidth
                    End If
                    If isLineStart AndAlso b.ActualCornerNW = 0F Then
                        pts(3).X += b.ActualBorderLeftWidth
                    End If

                    If b.ActualCornerNW > 0F Then
                        corner = CreateCorner(b, r, 1)
                    End If


                Case Border.Right
                    bwidth = b.ActualBorderRightWidth
                    pts(0) = RoundP(New PointF(r.Right - bwidth, r.Top + b.ActualCornerNE), b)
                    pts(1) = RoundP(New PointF(r.Right, r.Top + b.ActualCornerNE), b)
                    pts(2) = RoundP(New PointF(r.Right, r.Bottom - b.ActualCornerSE), b)
                    pts(3) = RoundP(New PointF(r.Right - bwidth, r.Bottom - b.ActualCornerSE), b)


                    If b.ActualCornerNE = 0F Then
                        pts(0).Y += b.ActualBorderTopWidth
                    End If
                    If b.ActualCornerSE = 0F Then
                        pts(3).Y -= b.ActualBorderBottomWidth
                    End If
                    If b.ActualCornerNE > 0F Then
                        corner = CreateCorner(b, r, 2)
                    End If

                Case Border.Bottom
                    bwidth = b.ActualBorderBottomWidth
                    pts(0) = RoundP(New PointF(r.Left + b.ActualCornerSW, r.Bottom - bwidth), b)
                    pts(1) = RoundP(New PointF(r.Right - b.ActualCornerSE, r.Bottom - bwidth), b)
                    pts(2) = RoundP(New PointF(r.Right - b.ActualCornerSE, r.Bottom), b)
                    pts(3) = RoundP(New PointF(r.Left + b.ActualCornerSW, r.Bottom), b)

                    If isLineStart AndAlso b.ActualCornerSW = 0F Then
                        pts(0).X += b.ActualBorderLeftWidth
                    End If
                    If isLineEnd AndAlso b.ActualCornerSE = 0F Then
                        pts(1).X -= b.ActualBorderRightWidth
                    End If

                    If b.ActualCornerSE > 0F Then
                        corner = CreateCorner(b, r, 3)
                    End If

                Case Border.Left
                    bwidth = b.ActualBorderLeftWidth
                    pts(0) = RoundP(New PointF(r.Left, r.Top + b.ActualCornerNW), b)
                    pts(1) = RoundP(New PointF(r.Left + bwidth, r.Top + b.ActualCornerNW), b)
                    pts(2) = RoundP(New PointF(r.Left + bwidth, r.Bottom - b.ActualCornerSW), b)
                    pts(3) = RoundP(New PointF(r.Left, r.Bottom - b.ActualCornerSW), b)

                    If b.ActualCornerNW = 0F Then
                        pts(1).Y += b.ActualBorderTopWidth
                    End If
                    If b.ActualCornerSW = 0F Then
                        pts(2).Y -= b.ActualBorderBottomWidth
                    End If

                    If b.ActualCornerSW > 0F Then
                        corner = CreateCorner(b, r, 4)
                    End If

            End Select

            Dim path As New GraphicsPath(pts, New Byte() {CByte(PathPointType.Line), CByte(PathPointType.Line), CByte(PathPointType.Line), CByte(PathPointType.Line)})

            If corner IsNot Nothing Then
                path.AddPath(corner, True)
            End If

            Return path
        End Function

        ''' <summary>
        ''' Creates the corner to place with the borders
        ''' </summary>
        ''' <returns></returns>
        Private Shared Function CreateCorner(b As CssBox, r As RectangleF, cornerIndex As Integer) As GraphicsPath
            Dim corner As New GraphicsPath()

            Dim outer As RectangleF = RectangleF.Empty
            Dim inner As RectangleF = RectangleF.Empty
            Dim start1 As Single = 0
            Dim start2 As Single = 0

            Select Case cornerIndex
                Case 1
                    outer = New RectangleF(r.Left, r.Top, b.ActualCornerNW, b.ActualCornerNW)
                    inner = RectangleF.FromLTRB(outer.Left + b.ActualBorderLeftWidth, outer.Top + b.ActualBorderTopWidth, outer.Right, outer.Bottom)
                    start1 = 180
                    start2 = 270

                Case 2
                    outer = New RectangleF(r.Right - b.ActualCornerNE, r.Top, b.ActualCornerNE, b.ActualCornerNE)
                    inner = RectangleF.FromLTRB(outer.Left, outer.Top + b.ActualBorderTopWidth, outer.Right - b.ActualBorderRightWidth, outer.Bottom)
                    outer.X -= outer.Width
                    inner.X -= inner.Width
                    start1 = -90
                    start2 = 0

                Case 3
                    outer = RectangleF.FromLTRB(r.Right - b.ActualCornerSE, r.Bottom - b.ActualCornerSE, r.Right, r.Bottom)
                    inner = New RectangleF(outer.Left, outer.Top, outer.Width - b.ActualBorderRightWidth, outer.Height - b.ActualBorderBottomWidth)
                    outer.X -= outer.Width
                    outer.Y -= outer.Height
                    inner.X -= inner.Width
                    inner.Y -= inner.Height
                    start1 = 0
                    start2 = 90

                Case 4
                    outer = New RectangleF(r.Left, r.Bottom - b.ActualCornerSW, b.ActualCornerSW, b.ActualCornerSW)
                    inner = RectangleF.FromLTRB(r.Left + b.ActualBorderLeftWidth, outer.Top, outer.Right, outer.Bottom - b.ActualBorderBottomWidth)
                    start1 = 90
                    start2 = 180
                    outer.Y -= outer.Height
                    inner.Y -= inner.Height

            End Select

            If outer.Width <= 0F Then
                outer.Width = 1.0F
            End If
            If outer.Height <= 0F Then
                outer.Height = 1.0F
            End If
            If inner.Width <= 0F Then
                inner.Width = 1.0F
            End If
            If inner.Height <= 0F Then
                inner.Height = 1.0F
            End If


            outer.Width *= 2
            outer.Height *= 2
            inner.Width *= 2
            inner.Height *= 2

            outer = RoundR(outer, b)
            inner = RoundR(inner, b)

            corner.AddArc(outer, start1, 90)
            corner.AddArc(inner, start2, -90)

            corner.CloseFigure()

            Return corner
        End Function

        ''' <summary>
        ''' Creates a rounded rectangle using the specified corner radius
        ''' </summary>
        ''' <param name="rect">Rectangle to round</param>
        ''' <param name="nwRadius">Radius of the north east corner</param>
        ''' <param name="neRadius">Radius of the north west corner</param>
        ''' <param name="seRadius">Radius of the south east corner</param>
        ''' <param name="swRadius">Radius of the south west corner</param>
        ''' <returns>GraphicsPath with the lines of the rounded rectangle ready to be painted</returns>
        Public Shared Function GetRoundRect(rect As RectangleF, nwRadius As Single, neRadius As Single, seRadius As Single, swRadius As Single) As GraphicsPath
            '  NW-----NE
            '  |       |
            '  |       |
            '  SW-----SE

            Dim path As New GraphicsPath()

            nwRadius *= 2
            neRadius *= 2
            seRadius *= 2
            swRadius *= 2

            'NW ---- NE
            path.AddLine(rect.X + nwRadius, rect.Y, rect.Right - neRadius, rect.Y)

            'NE Arc
            If neRadius > 0F Then
                path.AddArc(RectangleF.FromLTRB(rect.Right - neRadius, rect.Top, rect.Right, rect.Top + neRadius), -90, 90)
            End If

            ' NE
            '  |
            ' SE
            path.AddLine(rect.Right, rect.Top + neRadius, rect.Right, rect.Bottom - seRadius)

            'SE Arc
            If seRadius > 0F Then
                path.AddArc(RectangleF.FromLTRB(rect.Right - seRadius, rect.Bottom - seRadius, rect.Right, rect.Bottom), 0, 90)
            End If

            ' SW --- SE
            path.AddLine(rect.Right - seRadius, rect.Bottom, rect.Left + swRadius, rect.Bottom)

            'SW Arc
            If swRadius > 0F Then
                path.AddArc(RectangleF.FromLTRB(rect.Left, rect.Bottom - swRadius, rect.Left + swRadius, rect.Bottom), 90, 90)
            End If

            ' NW
            ' |
            ' SW
            path.AddLine(rect.Left, rect.Bottom - swRadius, rect.Left, rect.Top + nwRadius)

            'NW Arc
            If nwRadius > 0F Then
                path.AddArc(RectangleF.FromLTRB(rect.Left, rect.Top, rect.Left + nwRadius, rect.Top + nwRadius), 180, 90)
            End If

            path.CloseFigure()

            Return path
        End Function
    End Class
End Namespace
