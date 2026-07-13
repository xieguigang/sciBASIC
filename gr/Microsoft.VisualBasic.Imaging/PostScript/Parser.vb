#Region "Microsoft.VisualBasic::09a16dcbb2866c877524c56a7a890260, gr\Microsoft.VisualBasic.Imaging\PostScript\Parser.vb"

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

'   Total Lines: 47
'    Code Lines: 26 (55.32%)
' Comment Lines: 10 (21.28%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 11 (23.40%)
'    File Size: 1.59 KB


'     Class Parser
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: Load
' 
'         Sub: (+2 Overloads) Dispose
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http
Imports std = System.Math

Namespace PostScript

    ''' <summary>
    ''' A general purpose ASCII postscript parser. It performs a lexical scan of the
    ''' input, maintains a small operand stack and a minimal graphics state, and
    ''' reconstructs the drawing into a list of <see cref="PSElement"/> objects that
    ''' are expressed in the gdi+ coordinate space (top-left origin, y pointing down).
    ''' </summary>
    ''' <remarks>
    ''' The postscript coordinate system has its origin at the bottom-left corner with
    ''' the y axis pointing up. Every coordinate that is read back is therefore flipped
    ''' on the y axis using the canvas height that is parsed from the DSC
    ''' <c>%%BoundingBox:</c> comment (or estimated from the geometry when missing) so
    ''' that the produced <see cref="PSElement"/> objects match the gdi+ space used by
    ''' the rest of this module.
    ''' </remarks>
    Public Class Parser : Implements IDisposable

        ReadOnly fp As StreamReader

        Private disposedValue As Boolean

        ''' <summary>
        ''' the canvas height in the gdi+ coordinate space. used for flip the y axis.
        ''' </summary>
        Private canvasHeight As Single = 0

        ' minimal graphics state
        Private currentColor As Color = Color.Black
        Private currentLineWidth As Single = 1
        Private currentFontName As String = "Courier"
        Private currentFontSize As Single = 12

        ' path tracking (coordinates are kept in the postscript space)
        Private pathPoints As New List(Of PointF)
        Private pathStarted As Boolean = False
        Private pathClosed As Boolean = False
        Private pendingArc As ArcInfo = Nothing

        ' current point / text anchor (postscript space)
        Private currentPoint As PointF
        Private textPos As PointF
        Private currentTextRotation As Single = 0

        ' operand stack + name / string capture
        Private nums As New Stack(Of Double)
        Private lastName As String = ""
        Private lastString As String = ""

        ' image capture state
        Private imageDefs As New Dictionary(Of String, Double)
        Private imageBase64 As String = Nothing

        Sub New(file As Stream)
            fp = New StreamReader(file)
        End Sub

        Public Iterator Function Load() As IEnumerable(Of PSElement)
            Dim raw = fp.ReadToEnd()
            Dim lines = raw.Split(New Char() {vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)

            ' resolve the canvas height before interpreting the geometry
            canvasHeight = ParseCanvasHeight(lines)

            ' reset the graphics state for a clean interpretation pass
            Call ResetState()

            Dim result As New List(Of PSElement)

            For Each line As String In lines
                ' skip document structure comments and notes, they carry no geometry
                If line.TrimStart().StartsWith("%"c) Then
                    Continue For
                End If

                For Each tok As String In Tokenize(line)
                    Call InterpretToken(tok, result)
                Next
            Next

            For Each element As PSElement In result
                Yield element
            Next
        End Function

        ''' <summary>
        ''' reset all the transient graphics state used during interpretation.
        ''' </summary>
        Private Sub ResetState()
            currentColor = Color.Black
            currentLineWidth = 1
            currentFontName = "Courier"
            currentFontSize = 12
            pathPoints.Clear()
            pathStarted = False
            pathClosed = False
            pendingArc = Nothing
            currentPoint = Nothing
            textPos = Nothing
            currentTextRotation = 0
            nums.Clear()
            lastName = ""
            lastString = ""
            imageDefs.Clear()
            imageBase64 = Nothing
        End Sub

        ''' <summary>
        ''' flip a postscript y coordinate (bottom-left origin) into a gdi+ y
        ''' coordinate (top-left origin).
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GdiY(psY!) As Single
            Return canvasHeight - psY
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Gdi(p As PointF) As PointF
            Return New PointF(p.X, GdiY(p.Y))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Pop() As Double
            If nums.Count = 0 Then
                Return 0
            End If

            Return nums.Pop()
        End Function

        ''' <summary>
        ''' interpret a single postscript token and, when a terminal operator is hit,
        ''' push the reconstructed <see cref="PSElement"/> into <paramref name="result"/>.
        ''' </summary>
        Private Sub InterpretToken(tok As String, result As List(Of PSElement))
            Select Case tok
                Case "newpath"
                    pathPoints.Clear()
                    pathStarted = False
                    pathClosed = False
                    pendingArc = Nothing

                Case "moveto"
                    Dim y = Pop(), x = Pop()
                    currentPoint = New PointF(CSng(x), CSng(y))
                    textPos = currentPoint
                    currentTextRotation = 0

                    If Not pathStarted Then
                        pathPoints.Add(currentPoint)
                        pathStarted = True
                    End If

                Case "lineto"
                    Dim y = Pop(), x = Pop()
                    currentPoint = New PointF(CSng(x), CSng(y))
                    pathPoints.Add(currentPoint)

                Case "curveto"
                    Dim y3 = Pop(), x3 = Pop()
                    Dim y2 = Pop(), x2 = Pop()
                    Dim y1 = Pop(), x1 = Pop()

                    Call SampleCurveto(
                        currentPoint,
                        New PointF(CSng(x1), CSng(y1)),
                        New PointF(CSng(x2), CSng(y2)),
                        New PointF(CSng(x3), CSng(y3)))

                Case "arc", "arcn"
                    Dim a2 = Pop(), a1 = Pop()
                    Dim r = Pop(), cy = Pop(), cx = Pop()
                    pendingArc = New ArcInfo With {
                        .cx = cx,
                        .cy = cy,
                        .radius = r,
                        .start = a1,
                        .sweep = If(tok = "arcn", a1 - a2, a2 - a1)
                    }
                    currentPoint = New PointF(CSng(cx + r * std.Cos(a2 * std.PI / 180)), CSng(cy + r * std.Sin(a2 * std.PI / 180)))

                Case "closepath"
                    pathClosed = True

                Case "stroke"
                    Dim e = FinalizeShape("stroke")
                    If e IsNot Nothing Then result.Add(e)

                Case "fill"
                    Dim e = FinalizeShape("fill")
                    If e IsNot Nothing Then result.Add(e)

                Case "show"
                    Dim e = FinalizeText()
                    If e IsNot Nothing Then result.Add(e)

                Case "setlinewidth"
                    currentLineWidth = CSng(Pop())

                Case "setrgbcolor"
                    Dim b = Pop(), g = Pop(), r = Pop()
                    currentColor = Color.FromArgb(Clamp255(r * 255), Clamp255(g * 255), Clamp255(b * 255))

                Case "setgray"
                    Dim gv = Pop()
                    Dim v = Clamp255(gv * 255)
                    currentColor = Color.FromArgb(v, v, v)

                Case "scalefont"
                    currentFontSize = CSng(Pop())

                Case "setfont"
                    If Not String.IsNullOrEmpty(lastName) Then
                        currentFontName = lastName.TrimStart("/"c)
                    End If

                Case "translate"
                    Dim dy = Pop(), dx = Pop()
                    textPos = New PointF(CSng(dx), CSng(dy))
                    currentTextRotation = 0

                Case "rotate"
                    Dim ang = Pop()
                    ' the writer emits a negative postscript rotation to keep the gdi+
                    ' clockwise orientation, so the original rotation is the negation.
                    currentTextRotation = -CSng(ang)

                Case "image"
                    Dim e = FinalizeImage()
                    If e IsNot Nothing Then result.Add(e)

                Case "def"
                    If Not String.IsNullOrEmpty(lastName) AndAlso nums.Count > 0 Then
                        Dim key = lastName.TrimStart("/"c)

                        If key = "width" OrElse key = "height" OrElse key = "xPos" OrElse key = "yPos" Then
                            imageDefs(key) = Pop()
                        Else
                            Call Pop()
                        End If
                    End If

                    lastName = ""

                Case Else
                    If tok.StartsWith("/"c) Then
                        lastName = tok
                    ElseIf tok.StartsWith("("c) Then
                        lastString = Unescape(tok)
                    ElseIf tok.StartsWith("<~") Then
                        ' strip the surrounding <~ ... ~> markers
                        If tok.Length > 4 Then
                            imageBase64 = tok.Substring(2, tok.Length - 4)
                        End If
                    Else
                        Dim d As Double

                        If Double.TryParse(tok, NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
                            nums.Push(d)
                        End If
                    End If
            End Select
        End Sub

        ''' <summary>
        ''' finalize the current path / pending arc into a <see cref="PSElement"/>.
        ''' </summary>
        Private Function FinalizeShape(mode As String) As PSElement
            Dim fillColor = currentColor.ToHtmlColor
            Dim strokePen = New Pen(currentColor, currentLineWidth)
            Dim stroke = New Stroke(strokePen)

            If pendingArc IsNot Nothing Then
                Dim gdiCx = pendingArc.cx
                Dim gdiCy = GdiY(pendingArc.cy)
                Dim r = pendingArc.radius

                If std.Abs(pendingArc.sweep) >= 359.5 Then
                    ' a full circle
                    Dim topLeft = New PointF(CSng(gdiCx - r), CSng(gdiCy - r))
                    Dim d = CInt(r * 2)
                    Dim circle As New Elements.Circle With {
                        .shape = New Shapes.Circle(topLeft, d, currentColor),
                        .stroke = If(mode = "stroke", stroke, Nothing)
                    }

                    If mode = "fill" Then
                        circle.shape.fill = fillColor
                    Else
                        circle.shape.fill = ""
                    End If

                    pendingArc = Nothing

                    Return circle
                Else
                    ' a partial elliptical arc
                    Dim x = gdiCx - r
                    Dim y = gdiCy - r
                    Dim w = r * 2

                    ' postscript angles are counter-clockwise with y up, the gdi+
                    ' convention is clockwise with y down, so both angles are negated.
                    Dim startA = -pendingArc.start
                    Dim sweepA = -pendingArc.sweep

                    pendingArc = Nothing

                    Return New Elements.Arc With {
                        .x = x,
                        .y = y,
                        .width = w,
                        .height = w,
                        .startAngle = startA,
                        .sweepAngle = sweepA,
                        .stroke = stroke
                    }
                End If
            End If

            If pathPoints.Count = 2 AndAlso Not pathClosed Then
                Return New Elements.Line(strokePen, Gdi(pathPoints(0)), Gdi(pathPoints(1)))
            End If

            If pathPoints.Count >= 3 Then
                If pathClosed AndAlso IsAxisAlignedRect(pathPoints) Then
                    Dim rect = RectFromPoints(pathPoints)
                    Dim box As New Elements.Rectangle(rect, currentColor)

                    If mode = "fill" Then
                        box.shape.fill = fillColor
                        box.shape.border = Nothing
                    Else
                        box.shape.fill = ""
                        box.shape.border = stroke
                    End If

                    Return box
                End If

                If pathClosed AndAlso IsPie(pathPoints) Then
                    Return MakePie(pathPoints, mode, fillColor, stroke)
                End If

                Dim pts = pathPoints.Select(AddressOf Gdi).ToArray

                If pathClosed Then
                    Return New Elements.Polygon With {
                        .points = pts,
                        .fill = If(mode = "fill", fillColor, Nothing),
                        .stroke = If(mode = "stroke", stroke, Nothing)
                    }
                Else
                    Return New Elements.Poly With {
                        .closedPath = False,
                        .points = pts,
                        .fill = Nothing,
                        .stroke = stroke
                    }
                End If
            End If

            Return Nothing
        End Function

        Private Function FinalizeText() As PSElement
            If String.IsNullOrEmpty(lastString) Then
                Return Nothing
            End If

            Dim loc = New PointF(textPos.X, GdiY(textPos.Y))

            Return New Elements.Text With {
                .text = lastString,
                .font = New CSSFont(currentFontName, currentFontSize, FontStyle.Regular, New SolidBrush(currentColor)),
                .rotation = currentTextRotation,
                .location = loc
            }
        End Function

        Private Function FinalizeImage() As PSElement
            If imageBase64 Is Nothing Then
                Return Nothing
            End If

            Dim w = If(imageDefs.ContainsKey("width"), CInt(imageDefs("width")), 0)
            Dim h = If(imageDefs.ContainsKey("height"), CInt(imageDefs("height")), 0)
            Dim xp = If(imageDefs.ContainsKey("xPos"), imageDefs("xPos"), 0)
            Dim yp = If(imageDefs.ContainsKey("yPos"), imageDefs("yPos"), 0)

            Dim img As New Elements.ImageData With {
                .image = New DataURI(imageBase64, "image/jpeg"),
                .location = New PointF(CSng(xp), GdiY(CSng(yp))),
                .size = New Size(w, h),
                .scale = New SizeF(1, 1)
            }

            imageBase64 = Nothing
            imageDefs.Clear()

            Return img
        End Function

        ''' <summary>
        ''' sample a cubic bezier (postscript space) and append the interior points to
        ''' the current path. the first point equals the current point and is skipped.
        ''' </summary>
        Private Sub SampleCurveto(p0 As PointF, p1 As PointF, p2 As PointF, p3 As PointF)
            Dim steps = 16

            For s As Integer = 1 To steps
                Dim t = s / steps
                Dim u = 1 - t
                Dim uu = u * u
                Dim uuu = uu * u
                Dim tt = t * t
                Dim ttt = tt * t
                Dim x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X
                Dim y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y

                pathPoints.Add(New PointF(CSng(x), CSng(y)))
            Next
        End Sub

        ''' <summary>
        ''' a closed path with exactly four vertices built from two distinct x values
        ''' and two distinct y values is an axis aligned rectangle.
        ''' </summary>
        Private Shared Function IsAxisAlignedRect(pts As List(Of PointF)) As Boolean
            If pts.Count <> 4 Then
                Return False
            End If

            Dim xs = pts.Select(Function(p) p.X).Distinct().ToArray
            Dim ys = pts.Select(Function(p) p.Y).Distinct().ToArray

            Return xs.Length = 2 AndAlso ys.Length = 2
        End Function

        Private Function RectFromPoints(pts As List(Of PointF)) As RectangleF
            Dim minX = pts.Min(Function(p) p.X)
            Dim maxX = pts.Max(Function(p) p.X)
            Dim minY = pts.Min(Function(p) p.Y)
            Dim maxY = pts.Max(Function(p) p.Y)

            Return New RectangleF(minX, GdiY(maxY), maxX - minX, maxY - minY)
        End Function

        ''' <summary>
        ''' a pie is a closed path whose first vertex is the apex (close to the centroid
        ''' of the remaining arc points) while the remaining points lie roughly on a
        ''' circle around that apex.
        ''' </summary>
        Private Shared Function IsPie(pts As List(Of PointF)) As Boolean
            Dim center = pts(0)
            Dim rest = pts.Skip(1).ToArray()

            If rest.Length < 2 Then
                Return False
            End If

            Dim rs = rest.Select(Function(p) Dist(p, center)).ToArray
            Dim avg = rs.Average()

            If avg <= 0 Then
                Return False
            End If

            Dim maxDev = rs.Select(Function(r) std.Abs(r - avg)).Max()

            If maxDev > 0.1 * avg Then
                Return False
            End If

            Dim cx = rest.Average(Function(p) p.X)
            Dim cy = rest.Average(Function(p) p.Y)
            Dim dCenter = Dist(center, New PointF(CSng(cx), CSng(cy)))

            Return dCenter < 0.5 * avg
        End Function

        Private Function MakePie(pts As List(Of PointF), mode As String, fillColor As String, stroke As Stroke) As PSElement
            Dim center = Gdi(pts(0))
            Dim rest = pts.Skip(1).Select(AddressOf Gdi).ToArray

            Dim minX = rest.Min(Function(p) p.X)
            Dim maxX = rest.Max(Function(p) p.X)
            Dim minY = rest.Min(Function(p) p.Y)
            Dim maxY = rest.Max(Function(p) p.Y)

            Dim a0 = rest(0)
            Dim a1 = rest(rest.Length - 1)

            Dim ang0 = std.Atan2(a0.Y - center.Y, a0.X - center.X) * 180 / std.PI
            Dim ang1 = std.Atan2(a1.Y - center.Y, a1.X - center.X) * 180 / std.PI
            Dim sweep = ang1 - ang0

            If sweep < 0 Then
                sweep += 360
            End If

            Return New Elements.Pie With {
                .x = minX,
                .y = minY,
                .width = maxX - minX,
                .height = maxY - minY,
                .startAngle = CSng(ang0),
                .sweepAngle = CSng(sweep),
                .fill = If(mode = "fill", fillColor, Nothing),
                .stroke = If(mode = "stroke", stroke, Nothing)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Dist(a As PointF, b As PointF) As Double
            Dim dx = a.X - b.X
            Dim dy = a.Y - b.Y
            Return std.Sqrt(dx * dx + dy * dy)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Clamp255(v As Double) As Integer
            If v < 0 Then
                Return 0
            ElseIf v > 255 Then
                Return 255
            Else
                Return CInt(v)
            End If
        End Function

        ''' <summary>
        ''' strip the surrounding parentheses of a postscript literal string and undo
        ''' the backslash escape sequences.
        ''' </summary>
        Private Shared Function Unescape(tok As String) As String
            If tok.Length < 2 Then
                Return ""
            End If

            Dim inner = tok.Substring(1, tok.Length - 2)
            Dim sb As New StringBuilder
            Dim i = 0

            While i < inner.Length
                Dim c = inner(i)

                If c = "\"c AndAlso i + 1 < inner.Length Then
                    Dim n = inner(i + 1)

                    If n = "("c OrElse n = ")"c OrElse n = "\"c Then
                        sb.Append(n)
                        i += 2
                        Continue While
                    End If
                End If

                sb.Append(c)
                i += 1
            End While

            Return sb.ToString
        End Function

        ''' <summary>
        ''' resolve the canvas height. prefer the DSC <c>%%BoundingBox:</c> comment,
        ''' otherwise fall back to the maximum y coordinate that appears in the file.
        ''' </summary>
        Private Shared Function ParseCanvasHeight(lines As String()) As Single
            For Each raw As String In lines
                Dim t = raw.Trim()

                If t.StartsWith("%%BoundingBox:") Then
                    Dim parts = t.Substring(14).Trim().Split(New Char() {" "c, vbTab}, StringSplitOptions.RemoveEmptyEntries)

                    If parts.Length >= 4 Then
                        Dim ury As Double

                        If Double.TryParse(parts(3), NumberStyles.Float, CultureInfo.InvariantCulture, ury) Then
                            Return CSng(ury)
                        End If
                    End If

                    Return 0
                End If
            Next

            Dim maxY As Double = 0

            For Each raw As String In lines
                If raw.TrimStart().StartsWith("%"c) Then
                    Continue For
                End If

                Dim toks = Tokenize(raw)
                Dim stk As New Stack(Of Double)

                For Each tk As String In toks
                    Dim d As Double

                    If Double.TryParse(tk, NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
                        stk.Push(d)
                    ElseIf tk = "moveto" OrElse tk = "lineto" Then
                        If stk.Count >= 2 Then
                            Dim y = stk.Pop()
                            stk.Pop()
                            maxY = std.Max(maxY, y)
                        End If
                    ElseIf tk = "arc" OrElse tk = "arcn" Then
                        If stk.Count >= 5 Then
                            stk.Pop()
                            stk.Pop()
                            stk.Pop()
                            Dim cy = stk.Pop()
                            stk.Pop()
                            maxY = std.Max(maxY, cy)
                        End If
                    End If
                Next
            Next

            Return CSng(maxY)
        End Function

        ''' <summary>
        ''' split a single postscript line into tokens. string literals, dictionaries
        ''' (<c>&lt;&lt; &gt;&gt;</c>), procedures (<c>{ }</c>) and ascii85 image data
        ''' (<c>&lt;~ ~&gt;</c>) are each kept as a single token. postscript comments
        ''' (<c>%</c> up to the end of the line) are dropped.
        ''' </summary>
        Private Shared Function Tokenize(line As String) As List(Of String)
            Dim tokens As New List(Of String)
            Dim i = 0
            Dim n = line.Length

            While i < n
                Dim c = line(i)

                If Char.IsWhiteSpace(c) Then
                    i += 1
                    Continue While
                End If

                If c = "%"c Then
                    ' comment to the end of the line
                    Exit While
                End If

                If c = "("c Then
                    Dim j = i + 1
                    Dim sb As New StringBuilder
                    sb.Append("("c)

                    While j < n
                        Dim ch = line(j)

                        If ch = "\"c AndAlso j + 1 < n Then
                            sb.Append(ch)
                            sb.Append(line(j + 1))
                            j += 2
                            Continue While
                        End If

                        sb.Append(ch)

                        If ch = ")"c Then
                            j += 1
                            Exit While
                        End If

                        j += 1
                    End While

                    tokens.Add(sb.ToString)
                    i = j
                    Continue While
                End If

                If c = "<"c AndAlso i + 1 < n AndAlso line(i + 1) = "<"c Then
                    Dim j = i + 2

                    While j < n AndAlso Not (line(j) = ">"c AndAlso j + 1 < n AndAlso line(j + 1) = ">"c)
                        j += 1
                    End While

                    tokens.Add("<dict>")
                    i = If(j + 2 <= n, j + 2, n)
                    Continue While
                End If

                If c = "<"c AndAlso i + 1 < n AndAlso line(i + 1) = "~"c Then
                    Dim j = i + 2

                    While j < n AndAlso Not (line(j) = "~"c AndAlso j + 1 < n AndAlso line(j + 1) = ">"c)
                        j += 1
                    End While

                    Dim endIdx = If(j + 2 <= n, j + 2, n)
                    tokens.Add(line.Substring(i, endIdx - i))
                    i = endIdx
                    Continue While
                End If

                If c = "{"c Then
                    Dim depth = 1
                    Dim j = i + 1

                    While j < n AndAlso depth > 0
                        If line(j) = "{"c Then depth += 1
                        If line(j) = "}"c Then depth -= 1
                        j += 1
                    End While

                    tokens.Add("<proc>")
                    i = j
                    Continue While
                End If

                If c = "/"c Then
                    Dim j = i + 1

                    While j < n AndAlso Not Char.IsWhiteSpace(line(j)) AndAlso line(j) <> "("c AndAlso line(j) <> ")"c
                        j += 1
                    End While

                    tokens.Add(line.Substring(i, j - i))
                    i = j
                    Continue While
                End If

                If Char.IsDigit(c) OrElse ((c = "-"c OrElse c = "+"c) AndAlso i + 1 < n AndAlso Char.IsDigit(line(i + 1))) OrElse (c = "."c AndAlso i + 1 < n AndAlso Char.IsDigit(line(i + 1))) Then
                    Dim j = i

                    While j < n AndAlso (Char.IsDigit(line(j)) OrElse line(j) = "."c OrElse line(j) = "-"c OrElse line(j) = "+"c OrElse line(j) = "e"c OrElse line(j) = "E"c)
                        j += 1
                    End While

                    tokens.Add(line.Substring(i, j - i))
                    i = j
                    Continue While
                End If

                Dim k = i

                While k < n AndAlso Not Char.IsWhiteSpace(line(k)) AndAlso line(k) <> "("c AndAlso line(k) <> ")"c AndAlso line(k) <> "/"c AndAlso line(k) <> "<"c AndAlso line(k) <> "{"c
                    k += 1
                End While

                If k > i Then
                    tokens.Add(line.Substring(i, k - i))
                    i = k
                Else
                    i += 1
                End If
            End While

            Return tokens
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call fp.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' a pending elliptical arc description captured from the <c>arc</c> operator.
        ''' </summary>
        Private Class ArcInfo
            Public cx, cy, radius, start, sweep As Double
        End Class
    End Class
End Namespace
