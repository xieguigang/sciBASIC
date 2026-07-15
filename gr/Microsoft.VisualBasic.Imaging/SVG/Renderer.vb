#Region "Microsoft.VisualBasic::8acde70f1478dfdefd03c1d2bf71b27e, gr\Microsoft.VisualBasic.Imaging\SVG\Renderer.vb"

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

'   Total Lines: 74
'    Code Lines: 52 (70.27%)
' Comment Lines: 14 (18.92%)
'    - Xml Docs: 78.57%
' 
'   Blank Lines: 8 (10.81%)
'     File Size: 2.62 KB


'     Module Renderer
' 
'         Function: DrawImage, SVGColorHelper
' 
'         Sub: drawLayer
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG.PathHelper
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports std = System.Math

Namespace SVG

    ''' <summary>
    ''' Rendering svg vector image to gdi+ pixel image.
    ''' </summary>
    ''' <remarks>
    ''' (将SVG图像渲染为gdi+图像<see cref="Image"/>)
    ''' </remarks>
    Public Module Renderer

        <Extension>
        Public Function SVGColorHelper(br As Brush) As String
            If br Is Nothing Then
                ' 透明色
                Return Nothing
            ElseIf TypeOf br Is SolidBrush Then
                Dim color As Color = DirectCast(br, SolidBrush).Color

                If color.IsTransparent Then
                    Return Nothing
                Else
                    Return color.ToHtmlColor
                End If
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>
        ''' Rendering the SVG document as bitmap image.
        ''' </summary>
        ''' <param name="svg"></param>
        ''' <returns></returns>
        Public Function DrawImage(svg As SVGData) As Image
            Using g As IGraphics = Driver.CreateGraphicsDevice(svg.Layout.Size)
                With g
                    Call .Clear(svg.SVG.bg.GetBrush)
                    Call .drawLayer(svg.SVG.svg)

                    Return DirectCast(g, GdiRasterGraphics).ImageResource
                End With
            End Using
        End Function

#Region "Helper Methods"

        ''' <summary>
        ''' Parse an SVG color string to a <see cref="Color"/> value.
        ''' Supports hex colors (#RGB, #RRGGBB), named colors, and "none".
        ''' </summary>
        ''' <param name="colorStr">The SVG color string (e.g., "#FF0000", "red", "none").</param>
        ''' <returns>The parsed color, or <see cref="Color.Empty"/> if the string is "none" or empty.</returns>
        Private Function ParseSvgColor(colorStr As String) As Color
            If String.IsNullOrEmpty(colorStr) OrElse colorStr.Equals("none", StringComparison.OrdinalIgnoreCase) Then
                Return Color.Empty
            End If

            ' Use the existing TranslateColor extension from the framework
            Return colorStr.TranslateColor(throwEx:=False)
        End Function

        ''' <summary>
        ''' Apply fill and stroke to a given <see cref="SvgElement"/> using the provided draw actions.
        ''' </summary>
        Private Sub DrawShape(g As IGraphics, element As SvgElement,
                              fillAction As Action(Of Brush),
                              strokeAction As Action(Of Pen))

            Dim fillColor As Color = ParseSvgColor(element.Fill)
            Dim strokeColor As Color = ParseSvgColor(element.Stroke)
            Dim strokeWidth As Double = element.StrokeWidth
            Dim fillOpacity As Double = element.FillOpacity
            Dim strokeOpacity As Double = element.StrokeOpacity

            ' Apply fill (SVG default fill is black, so we always fill unless explicitly "none")
            If fillColor <> Color.Empty Then
                Dim alpha As Integer = CInt(std.Max(0, std.Min(1, fillOpacity)) * 255)
                If alpha > 0 Then
                    Dim fillCol As Color = Color.FromArgb(alpha, fillColor)
                    Using br As New SolidBrush(fillCol)
                        fillAction(br)
                    End Using
                End If
            End If

            ' Apply stroke (only if width > 0 and color is not "none")
            If strokeColor <> Color.Empty AndAlso strokeWidth > 0 Then
                Dim alpha As Integer = CInt(std.Max(0, std.Min(1, strokeOpacity)) * 255)
                If alpha > 0 Then
                    Dim strokeCol As Color = Color.FromArgb(alpha, strokeColor)
                    Using pen As New Pen(strokeCol, CSng(strokeWidth))
                        ' Apply stroke line cap style
                        Select Case element.StrokeLineCap.enum
                            Case SvgStrokeLineCap.cap_round
                                pen.StartCap = LineCap.Round
                                pen.EndCap = LineCap.Round
                            Case SvgStrokeLineCap.cap_square
                                pen.StartCap = LineCap.Square
                                pen.EndCap = LineCap.Square
                            Case Else
                                ' Butt (default)
                                pen.StartCap = LineCap.Flat
                                pen.EndCap = LineCap.Flat
                        End Select

                        ' Apply stroke dash array if specified
                        Dim dashArray As Double() = element.StrokeDashArray
                        If dashArray IsNot Nothing AndAlso dashArray.Length > 0 Then
                            pen.DashPattern = dashArray.Select(Function(d) CSng(d)).ToArray()
                        End If

                        strokeAction(pen)
                    End Using
                End If
            End If
        End Sub

        ''' <summary>
        ''' Parse the SVG transform attribute and apply it to the graphics context.
        ''' Supports translate, scale, rotate, skewX, skewY, and matrix.
        ''' </summary>
        Private Sub ApplyTransform(g As IGraphics, transformStr As String)
            If String.IsNullOrEmpty(transformStr) Then
                Return
            End If

            ' Parse individual transform commands from the SVG transform attribute
            ' e.g. "translate(10,20) rotate(45) scale(1.5)"
            Dim remaining As String = transformStr.Trim()

            While remaining.Length > 0
                Dim parenIndex As Integer = remaining.IndexOf("("c)
                If parenIndex < 0 Then Exit While

                Dim funcName As String = remaining.Substring(0, parenIndex).Trim().ToLower()
                Dim closeParen As Integer = remaining.IndexOf(")"c, parenIndex)
                If closeParen < 0 Then Exit While

                Dim argsStr As String = remaining.Substring(parenIndex + 1, closeParen - parenIndex - 1)
                Dim args As String() = argsStr.Split({","c, " "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)

                Select Case funcName
                    Case "translate"
                        Dim tx As Single = CSng(Val(args(0)))
                        Dim ty As Single = If(args.Length > 1, CSng(Val(args(1))), 0.0F)
                        g.TranslateTransform(tx, ty)

                    Case "scale"
                        Dim sx As Single = CSng(Val(args(0)))
                        Dim sy As Single = If(args.Length > 1, CSng(Val(args(1))), sx)
                        g.ScaleTransform(sx, sy)

                    Case "rotate"
                        Dim angle As Single = CSng(Val(args(0)))
                        If args.Length >= 3 Then
                            ' rotate around a center point
                            Dim cx As Single = CSng(Val(args(1)))
                            Dim cy As Single = CSng(Val(args(2)))
                            g.TranslateTransform(cx, cy)
                            g.RotateTransform(angle)
                            g.TranslateTransform(-cx, -cy)
                        Else
                            g.RotateTransform(angle)
                        End If

                    Case "skewx"
                        Dim angleX As Single = CSng(Val(args(0)))
#If NET48 Then
                        Dim gdiGraphics As Graphics = TryGetGdiGraphics(g)
                        If gdiGraphics IsNot Nothing Then
                            gdiGraphics.MultiplyTransform(New Matrix(1, 0, CSng(Math.Tan(angleX * Math.PI / 180)), 1, 0, 0))
                        End If
#End If

                    Case "skewy"
                        Dim angleY As Single = CSng(Val(args(0)))
#If NET48 Then
                        Dim gdiGraphics2 As Graphics = TryGetGdiGraphics(g)
                        If gdiGraphics2 IsNot Nothing Then
                            gdiGraphics2.MultiplyTransform(New Matrix(1, CSng(Math.Tan(angleY * Math.PI / 180)), 0, 1, 0, 0))
                        End If
#End If

                    Case "matrix"
                        ' matrix(a, b, c, d, e, f)
                        ' If we have access to the underlying GDI+ Graphics, apply the matrix there
                        ' Otherwise, decompose and apply approximate transforms
                        If args.Length >= 6 Then
#If NET48 Then
                            Dim gdiGraphics As Graphics = TryGetGdiGraphics(g)
                            If gdiGraphics IsNot Nothing Then
                                Dim a As Single = CSng(Val(args(0)))
                                Dim b As Single = CSng(Val(args(1)))
                                Dim c As Single = CSng(Val(args(2)))
                                Dim d As Single = CSng(Val(args(3)))
                                Dim e As Single = CSng(Val(args(4)))
                                Dim f As Single = CSng(Val(args(5)))
                                gdiGraphics.MultiplyTransform(New Matrix(a, b, c, d, e, f))
                            End If
#End If
                        End If
                End Select

                ' Move past the processed command
                remaining = remaining.Substring(closeParen + 1).Trim()
            End While
        End Sub

#If NET48 Then
        ''' <summary>
        ''' Try to get the underlying <see cref="System.Drawing.Graphics"/> object from an <see cref="IGraphics"/> instance.
        ''' Returns Nothing if unavailable.
        ''' </summary>
        Private Function TryGetGdiGraphics(g As IGraphics) As Graphics
            If TypeOf g Is Graphics2D Then
                Return DirectCast(g, Graphics2D).Graphics
            End If
            Return Nothing
        End Function
#End If

        ''' <summary>
        ''' Build a <see cref="GraphicsPath"/> for a rounded rectangle.
        ''' </summary>
        Private Function BuildRoundedRectPath(x As Single, y As Single, w As Single, h As Single,
                                              rx As Single, ry As Single) As GraphicsPath
            Dim path As New GraphicsPath

            If w <= 0 OrElse h <= 0 Then
                Return path
            End If

            ' Clamp corner radius to half of width/height
            rx = std.Min(rx, w / 2)
            ry = std.Min(ry, h / 2)

            If rx <= 0 AndAlso ry <= 0 Then
                ' Regular rectangle
                path.AddRectangle(New RectangleF(x, y, w, h))
                Return path
            End If

            If rx = 0 Then rx = ry
            If ry = 0 Then ry = rx

            Dim r As New RectangleF(x, y, rx * 2, ry * 2)

            ' Top-left corner
            path.AddArc(r, 180, 90)
            ' Top edge to top-right corner
            r.X = x + w - rx * 2
            path.AddArc(r, 270, 90)
            ' Right edge to bottom-right corner
            r.Y = y + h - ry * 2
            path.AddArc(r, 0, 90)
            ' Bottom edge to bottom-left corner
            r.X = x
            path.AddArc(r, 90, 90)
            path.CloseFigure()

            Return path
        End Function

#End Region

        <Extension>
        Private Sub drawLayer(g As IGraphics, layer As SvgContainer)
            ' Note: GetElements() recursively yields ALL descendant elements (flattened).
            ' Therefore, when we encounter a SvgContainer in the loop, we skip it
            ' (Continue For) because its children are already yielded separately.
            ' This avoids drawing elements multiple times.
            For Each element As SvgElement In layer.GetElements
                ' Skip invisible elements
                If Not element.Visible Then Continue For

                Select Case element.GetType
                    Case GetType(SvgContainer)
                        ' Container children are already yielded by GetElements() -
                        ' skip to avoid double-rendering
                        Continue For

                    Case GetType(SvgCircle)
                        Dim circle As SvgCircle = DirectCast(element, SvgCircle)
                        If circle.R > 0 Then
                            Call g.ResetTransform()
                            Call ApplyTransform(g, circle.Transform)
                            Call DrawShape(g, circle,
                                Sub(br)
                                    g.FillEllipse(br, CSng(circle.CX - circle.R), CSng(circle.CY - circle.R),
                                                  CSng(circle.R * 2), CSng(circle.R * 2))
                                End Sub,
                                Sub(pen)
                                    g.DrawEllipse(pen, CSng(circle.CX - circle.R), CSng(circle.CY - circle.R),
                                                  CSng(circle.R * 2), CSng(circle.R * 2))
                                End Sub)
                            Call g.ResetTransform()
                        End If

                    Case GetType(SvgEllipse)
                        Dim ellipse As SvgEllipse = DirectCast(element, SvgEllipse)
                        If ellipse.RX > 0 AndAlso ellipse.RY > 0 Then
                            Call g.ResetTransform()
                            Call ApplyTransform(g, ellipse.Transform)
                            Call DrawShape(g, ellipse,
                                Sub(br)
                                    g.FillEllipse(br, CSng(ellipse.CX - ellipse.RX), CSng(ellipse.CY - ellipse.RY),
                                                  CSng(ellipse.RX * 2), CSng(ellipse.RY * 2))
                                End Sub,
                                Sub(pen)
                                    g.DrawEllipse(pen, CSng(ellipse.CX - ellipse.RX), CSng(ellipse.CY - ellipse.RY),
                                                  CSng(ellipse.RX * 2), CSng(ellipse.RY * 2))
                                End Sub)
                            Call g.ResetTransform()
                        End If

                    Case GetType(SvgLine)
                        Dim line As SvgLine = DirectCast(element, SvgLine)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, line.Transform)
                        ' Lines only have stroke, no fill
                        Dim strokeColor As Color = ParseSvgColor(line.Stroke)
                        If strokeColor <> Color.Empty AndAlso line.StrokeWidth > 0 Then
                            Dim strokeOpacity As Double = line.StrokeOpacity
                            Dim alpha As Integer = CInt(std.Max(0, std.Min(1, strokeOpacity)) * 255)
                            Dim strokeCol As Color = Color.FromArgb(alpha, strokeColor)

                            Using pen As New Pen(strokeCol, CSng(line.StrokeWidth))
                                ' Apply dash array if present
                                Dim dashArray As Double() = line.StrokeDashArray
                                If dashArray IsNot Nothing AndAlso dashArray.Length > 0 Then
                                    pen.DashPattern = dashArray.Select(Function(d) CSng(d)).ToArray()
                                End If
                                Select Case line.StrokeLineCap.enum
                                    Case SvgStrokeLineCap.cap_round
                                        pen.StartCap = LineCap.Round
                                        pen.EndCap = LineCap.Round
                                    Case SvgStrokeLineCap.cap_square
                                        pen.StartCap = LineCap.Square
                                        pen.EndCap = LineCap.Square
                                    Case Else
                                        pen.StartCap = LineCap.Flat
                                        pen.EndCap = LineCap.Flat
                                End Select
                                g.DrawLine(pen, CSng(line.X1), CSng(line.Y1), CSng(line.X2), CSng(line.Y2))
                            End Using
                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgRect)
                        Dim rect As SvgRect = DirectCast(element, SvgRect)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, rect.Transform)

                        If rect.rx > 0 OrElse rect.ry > 0 Then
                            ' Rounded rectangle - use GraphicsPath
                            Dim rx As Single = CSng(rect.rx)
                            Dim ry As Single = CSng(If(rect.ry > 0, rect.ry, rect.rx))

                            Using roundPath As GraphicsPath = BuildRoundedRectPath(
                                CSng(rect.X), CSng(rect.Y), CSng(rect.Width), CSng(rect.Height), rx, ry)
                                Call DrawShape(g, rect, fillAction:=Sub(br) g.FillPath(br, roundPath), strokeAction:=Sub(pen) g.DrawPath(pen, roundPath))
                            End Using
                        Else
                            ' Regular rectangle
                            Call DrawShape(g, rect,
                                Sub(br)
                                    g.FillRectangle(br, CSng(rect.X), CSng(rect.Y),
                                                    CSng(rect.Width), CSng(rect.Height))
                                End Sub,
                                Sub(pen)
                                    g.DrawRectangle(pen, CSng(rect.X), CSng(rect.Y),
                                                    CSng(rect.Width), CSng(rect.Height))
                                End Sub)
                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgPath)
                        Dim svgPath As SvgPath = DirectCast(element, SvgPath)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, svgPath.Transform)

                        ' Use the existing ModelBuilder to convert SVG path data to GDI+ GraphicsPath
                        Dim gdiPath As GraphicsPath = svgPath.ParseSVGPathData()

                        If gdiPath IsNot Nothing Then
                            Call DrawShape(g, svgPath,
                                Sub(br) g.FillPath(br, gdiPath),
                                Sub(pen) g.DrawPath(pen, gdiPath))
                            gdiPath.Dispose()
                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgPolygon)
                        Dim polygon As SvgPolygon = DirectCast(element, SvgPolygon)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, polygon.Transform)

                        Dim pointsData As Double() = polygon.Points
                        If pointsData IsNot Nothing AndAlso pointsData.Length >= 6 Then
                            Dim ptCount As Integer = pointsData.Length \ 2
                            Dim points(ptCount - 1) As PointF
                            For i As Integer = 0 To ptCount - 1
                                points(i) = New PointF(CSng(pointsData(i * 2)), CSng(pointsData(i * 2 + 1)))
                            Next

                            Call DrawShape(g, polygon,
                    Sub(br) g.FillPolygon(br, points),
                                Sub(pen) g.DrawPolygon(pen, points))
                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgPolyLine)
                        Dim polyline As SvgPolyLine = DirectCast(element, SvgPolyLine)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, polyline.Transform)

                        Dim pointsData As Double() = polyline.Points
                        If pointsData IsNot Nothing AndAlso pointsData.Length >= 4 Then
                            Dim ptCount As Integer = pointsData.Length \ 2
                            Dim points(ptCount - 1) As PointF
                            For i As Integer = 0 To ptCount - 1
                                points(i) = New PointF(CSng(pointsData(i * 2)), CSng(pointsData(i * 2 + 1)))
                            Next

                            ' Polyline: stroke-only (open path)
                            Dim strokeColor As Color = ParseSvgColor(polyline.Stroke)
                            If strokeColor <> Color.Empty AndAlso polyline.StrokeWidth > 0 Then
                                Dim strokeOpacity As Double = polyline.StrokeOpacity
                                Dim alpha As Integer = CInt(std.Max(0, std.Min(1, strokeOpacity)) * 255)
                                Dim strokeCol As Color = Color.FromArgb(alpha, strokeColor)

                                Using pen As New Pen(strokeCol, CSng(polyline.StrokeWidth))
                                    g.DrawLines(pen, points)
                                End Using
                            End If

                            ' Also support fill for polyline if fill is specified
                            Dim fillColor As Color = ParseSvgColor(polyline.Fill)
                            If fillColor <> Color.Empty Then
                                Dim fillOpacity As Double = polyline.FillOpacity
                                Dim alpha As Integer = CInt(std.Max(0, std.Min(1, fillOpacity)) * 255)
                                If alpha > 0 Then
                                    Dim fillCol As Color = Color.FromArgb(alpha, fillColor)
                                    Using br As New SolidBrush(fillCol)
                                        g.FillPolygon(br, points)
                                    End Using
                                End If
                            End If
                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgText)
                        Dim text As SvgText = DirectCast(element, SvgText)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, text.Transform)

                        Dim fontFamily As String = text.FontFamily
                        If String.IsNullOrEmpty(fontFamily) Then
                            fontFamily = "Arial"
                        End If
                        Dim fontSize As Single = CSng(text.FontSize)
                        If fontSize <= 0 Then
                            fontSize = 12.0F
                        End If

                        Dim textStr As String = text.Text
                        If Not String.IsNullOrEmpty(textStr) Then
                            Dim font As New Font(fontFamily, fontSize, FontStyle.Regular)
                            Dim fillColor As Color = ParseSvgColor(text.Fill)
                            If fillColor <> Color.Empty Then
                                Dim fillOpacity As Double = text.FillOpacity
                                Dim alpha As Integer = CInt(std.Max(0, std.Min(1, fillOpacity)) * 255)
                                If alpha > 0 Then
                                    Dim fillCol As Color = Color.FromArgb(alpha, fillColor)
                                    Using br As New SolidBrush(fillCol)
                                        g.DrawString(textStr, font, br, CSng(text.X), CSng(text.Y))
                                    End Using
                                End If
                            End If

                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgImage)
                        Dim svgImg As SvgImage = DirectCast(element, SvgImage)
                        Call g.ResetTransform()
                        Call ApplyTransform(g, svgImg.Transform)

                        If Not String.IsNullOrEmpty(svgImg.HRef) Then
                            Try
                                Dim img As Bitmap = svgImg.GetGDIObject()
                                If img IsNot Nothing Then
                                    Dim destRect As New RectangleF(
                                        CSng(svgImg.X), CSng(svgImg.Y),
                                        CSng(If(svgImg.Width > 0, svgImg.Width, img.Width)),
                                        CSng(If(svgImg.Height > 0, svgImg.Height, img.Height)))
                                    g.DrawImage(img, destRect)
                                End If
                            Catch ex As Exception
                                ' Log and skip images that cannot be decoded
                                Call $"Failed to render embedded SVG image: {ex.Message}".warning
                            End Try
                        End If
                        Call g.ResetTransform()

                    Case GetType(SvgTitle)
                        ' Title is a metadata element - skip rendering

                    Case Else
                        Throw New NotImplementedException(element.GetType.FullName)
                End Select
            Next
        End Sub
    End Module
End Namespace
