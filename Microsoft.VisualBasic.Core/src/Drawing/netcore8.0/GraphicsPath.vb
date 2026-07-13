#Region "Microsoft.VisualBasic::c240b24023c3d035c8cc934ebc37462d, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\GraphicsPath.vb"

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

    '   Total Lines: 179
    '    Code Lines: 131 (73.18%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 48 (26.82%)
    '     File Size: 5.59 KB


    '     Class PathData
    ' 
    '         Properties: Points
    ' 
    '     Class GraphicsPath
    ' 
    '         Properties: PathData, PathPoints
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GenericEnumerator
    ' 
    '         Sub: (+2 Overloads) AddArc, AddBezier, AddCurve, AddEllipse, AddLine
    '              AddLines, AddPolygon, AddRectangle, AddString, CloseAllFigures
    '              CloseFigure, Reset
    '         Class op
    ' 
    ' 
    ' 
    '         Class op_AddLine
    ' 
    '             Properties: a, b
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class op_AddBezier
    ' 
    '             Properties: pt1, pt2, pt3, pt4
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class op_AddCurve
    ' 
    '             Properties: points
    ' 
    '         Class op_AddLines
    ' 
    '             Properties: points
    ' 
    '         Class op_Reset
    ' 
    ' 
    ' 
    '         Class op_CloseAllFigures
    ' 
    ' 
    ' 
    '         Class op_CloseFigure
    ' 
    ' 
    ' 
    '         Class op_AddArc
    ' 
    '             Properties: rect, startAngle, sweepAngle
    ' 
    '         Class op_AddRectangle
    ' 
    '             Properties: rect
    ' 
    '         Class op_AddPolygon
    ' 
    '             Properties: points
    ' 
    '         Class op_AddEllipse
    ' 
    '             Properties: r1, r2, x, y
    ' 
    '         Class op_AddString
    ' 
    '             Properties: fontFamily, format, pos, s, size
    '                         style
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Linq

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    Public Class PathData

        ''' <summary>
        ''' Gets or sets an array of PointF structures that represent the points through which the path is constructed.
        ''' </summary>
        Public Property Points As PointF()

        ''' <summary>
        ''' Gets or sets the types of the corresponding points in the path. 
        ''' 0=Start, 1=Line, 3=Bezier/Bezier3, 0x80=CloseSubpath flag
        ''' </summary>
        Public Property Types As Byte()

    End Class

    Public Enum FillMode
        ''' <summary>
        ''' Specifies the alternate fill mode.
        ''' </summary>
        Alternate = 0
        ''' <summary>
        ''' Specifies the winding fill mode.
        ''' </summary>
        Winding = 1
    End Enum

    Public Class GraphicsPath : Implements Enumeration(Of op)

        ''' <summary>
        ''' Gets or sets the fill mode that determines how the interior of shapes in this GraphicsPath is filled.
        ''' </summary>
        Public Property FillMode As FillMode = FillMode.Alternate

        ''' <summary>
        ''' Gets the PathData for this GraphicsPath, containing both Points and Types.
        ''' </summary>
        Public ReadOnly Property PathData As PathData
            Get
                Return CollectPathData()
            End Get
        End Property

        ''' <summary>
        ''' Gets the points in the path.
        ''' </summary>
        Public ReadOnly Property PathPoints As PointF()
            Get
                Return CollectPathPoints()
            End Get
        End Property

        ''' <summary>
        ''' Gets the types of the corresponding points in the path.
        ''' </summary>
        Public ReadOnly Property PathTypes As Byte()
            Get
                Return CollectPathTypes()
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(points As IEnumerable(Of PointF))
            Call AddPolygon(points.ToArray)
            Call CloseAllFigures()
        End Sub

        Private Function CollectPathPoints() As PointF()
            Dim pts As New List(Of PointF)
            Dim resetSeen As Boolean = False

            For Each o As op In Me.opSet
                If resetSeen AndAlso TypeOf o Is op_Reset Then
                    pts.Clear()
                    resetSeen = True
                    Continue For
                End If

                If TypeOf o Is op_AddLine Then
                    Dim line As op_AddLine = DirectCast(o, op_AddLine)
                    If pts.Count = 0 OrElse pts.Last <> line.a Then
                        pts.Add(line.a)
                    End If
                    pts.Add(line.b)
                ElseIf TypeOf o Is op_AddBezier Then
                    Dim bz As op_AddBezier = DirectCast(o, op_AddBezier)
                    pts.Add(bz.pt1)
                    pts.Add(bz.pt2)
                    pts.Add(bz.pt3)
                    pts.Add(bz.pt4)
                ElseIf TypeOf o Is op_AddPolygon Then
                    Dim pg As op_AddPolygon = DirectCast(o, op_AddPolygon)
                    pts.AddRange(pg.points)
                ElseIf TypeOf o Is op_AddLines Then
                    Dim ls As op_AddLines = DirectCast(o, op_AddLines)
                    pts.AddRange(ls.points)
                ElseIf TypeOf o Is op_AddCurve Then
                    Dim cv As op_AddCurve = DirectCast(o, op_AddCurve)
                    pts.AddRange(cv.points)
                ElseIf TypeOf o Is op_AddRectangle Then
                    Dim rc As op_AddRectangle = DirectCast(o, op_AddRectangle)
                    pts.Add(New PointF(rc.rect.Left, rc.rect.Top))
                    pts.Add(New PointF(rc.rect.Right, rc.rect.Top))
                    pts.Add(New PointF(rc.rect.Right, rc.rect.Bottom))
                    pts.Add(New PointF(rc.rect.Left, rc.rect.Bottom))
                ElseIf TypeOf o Is op_AddEllipse Then
                    Dim el As op_AddEllipse = DirectCast(o, op_AddEllipse)
                    pts.Add(New PointF(el.x + el.r1, el.y))
                    pts.Add(New PointF(el.x, el.y + el.r2))
                    pts.Add(New PointF(el.x + el.r1, el.y + el.r2 * 2))
                    pts.Add(New PointF(el.x + el.r1 * 2, el.y + el.r2))
                End If
            Next

            Return pts.ToArray
        End Function

        Private Function CollectPathData() As PathData
            Dim data As New PathData With {
                .Points = CollectPathPoints(),
                .Types = CollectPathTypes()
            }
            Return data
        End Function

        Private Function CollectPathTypes() As Byte()
            Dim types As New List(Of Byte)
            Dim pts As PointF() = CollectPathPoints()

            ' Build simplified type array
            For i As Integer = 0 To pts.Length - 1
                If i = 0 Then
                    types.Add(0) ' Start
                Else
                    types.Add(1) ' Line (default)
                End If
            Next

            Return types.ToArray
        End Function

        Public MustInherit Class op

        End Class

        Public Class op_AddLine : Inherits op

            Public Property a As PointF
            Public Property b As PointF

            Sub New(a As PointF, b As PointF)
                _a = a
                _b = b
            End Sub

        End Class

        Public Class op_AddBezier : Inherits op

            Public Property pt1 As PointF
            Public Property pt2 As PointF
            Public Property pt3 As PointF
            Public Property pt4 As PointF

            Sub New(pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
                _pt1 = pt1
                _pt2 = pt2
                _pt3 = pt3
                _pt4 = pt4
            End Sub
        End Class

        Public Class op_AddCurve : Inherits op

            Public Property points As PointF()

        End Class

        Public Class op_AddLines : Inherits op

            Public Property points As PointF()

        End Class

        Public Class op_Reset : Inherits op
        End Class

        Public Class op_CloseAllFigures : Inherits op
        End Class

        Public Class op_CloseFigure : Inherits op
        End Class

        Public Class op_AddArc : Inherits op
            Public Property rect As RectangleF
            Public Property startAngle As Single
            Public Property sweepAngle As Single
        End Class

        Public Class op_AddRectangle : Inherits op
            Public Property rect As RectangleF
        End Class

        Public Class op_AddPolygon : Inherits op
            Public Property points As PointF()
        End Class

        Public Class op_AddEllipse : Inherits op
            Public Property x As Single
            Public Property y As Single
            Public Property r1 As Single
            Public Property r2 As Single
        End Class

        Public Class op_AddString : Inherits op
            Public Property s As String
            Public Property fontFamily As FontFamily
            Public Property style As FontStyle
            Public Property size As Single
            Public Property pos As PointF
            Public Property format As StringFormat
        End Class

        Public Class op_AddPie : Inherits op
            Public Property rect As RectangleF
            Public Property startAngle As Single
            Public Property sweepAngle As Single
        End Class

        Public Class op_AddClosedCurve : Inherits op
            Public Property points As PointF()
            Public Property tension As Single
        End Class

        Public Class op_AddPath : Inherits op
            Public Property path As GraphicsPath
            Public Property connect As Boolean
        End Class

        Public Class op_AddBeziers : Inherits op
            Public Property points As PointF()
        End Class

        Public Class op_AddEllipseRect : Inherits op
            Public Property rect As RectangleF
        End Class

        Public Class op_StartFigure : Inherits op
        End Class

        Public Class op_Flatten : Inherits op
            Public Property matrix As Matrix
            Public Property flatness As Single
        End Class

        Public Class op_Widen : Inherits op
            Public Property pen As Pen
            Public Property matrix As Matrix
            Public Property flatness As Single
        End Class

        Public Class op_Warp : Inherits op
            Public Property destPoints As PointF()
            Public Property srcRect As RectangleF
            Public Property matrix As Matrix
            Public Property warpMode As WarpMode
            Public Property flatness As Single
        End Class

        Public Class op_Transform : Inherits op
            Public Property matrix As Matrix
        End Class

        Public Class op_Reverse : Inherits op
        End Class

        Public Class op_GetBounds : Inherits op
            Public Property matrix As Matrix
            Public Property pen As Pen
        End Class

        Dim opSet As New List(Of op)

        Public Sub AddString(s As String, fontFamily As fontfamily, style As FontStyle, size As Single, pos As PointF, format As StringFormat)
            Call opSet.Add(New op_AddString With {.fontFamily = fontFamily, .format = format, .pos = pos, .s = s, .size = size, .style = style})
        End Sub

        Public Sub AddEllipse(x As Single, y As Single, r1 As Single, r2 As Single)
            Call opSet.Add(New op_AddEllipse With {.x = x, .y = y, .r1 = r1, .r2 = r2})
        End Sub

        Public Sub AddPolygon(points As PointF())
            Call opSet.Add(New op_AddPolygon With {.points = points})
        End Sub

        Public Sub AddRectangle(rect As RectangleF)
            Call opSet.Add(New op_AddRectangle With {.rect = rect})
        End Sub

        Public Sub AddArc(rect As RectangleF, startAngle!, sweepAngle!)
            Call opSet.Add(New op_AddArc With {.rect = rect, .startAngle = startAngle, .sweepAngle = sweepAngle})
        End Sub

        Public Sub AddArc(x As Single, y As Single, width As Single, height As Single, startAngle!, sweepAngle!)
            Call opSet.Add(New op_AddArc With {.rect = New RectangleF(x, y, width, height), .startAngle = startAngle, .sweepAngle = sweepAngle})
        End Sub

        Public Sub AddLine(a As PointF, b As PointF)
            Call opSet.Add(New op_AddLine(a, b))
        End Sub

        Public Sub AddBezier(pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
            Call opSet.Add(New op_AddBezier(pt1, pt2, pt3, pt4))
        End Sub

        Public Sub AddCurve(ParamArray points As PointF())
            Call opSet.Add(New op_AddCurve With {.points = points})
        End Sub

        Public Sub AddLines(ParamArray points As PointF())
            Call opSet.Add(New op_AddLines With {.points = points})
        End Sub

        Public Sub Reset()
            Call opSet.Add(New op_Reset())
        End Sub

        Public Sub CloseAllFigures()
            Call opSet.Add(New op_CloseAllFigures())
        End Sub

        Public Sub CloseFigure()
            Call opSet.Add(New op_CloseFigure)
        End Sub

        ''' <summary>
        ''' Starts a new figure without closing the previous figure.
        ''' </summary>
        Public Sub StartFigure()
            Call opSet.Add(New op_StartFigure)
        End Sub

        ''' <summary>
        ''' Adds the outline of a pie shape to this path.
        ''' </summary>
        Public Sub AddPie(rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Call opSet.Add(New op_AddPie With {.rect = rect, .startAngle = startAngle, .sweepAngle = sweepAngle})
        End Sub

        ''' <summary>
        ''' Adds a pie shape using individual coordinates.
        ''' </summary>
        Public Sub AddPie(x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Call opSet.Add(New op_AddPie With {.rect = New RectangleF(x, y, width, height), .startAngle = startAngle, .sweepAngle = sweepAngle})
        End Sub

        ''' <summary>
        ''' Adds a closed cardinal spline curve to this path.
        ''' </summary>
        Public Sub AddClosedCurve(points As PointF(), Optional tension As Single = 0.5F)
            Call opSet.Add(New op_AddClosedCurve With {.points = points, .tension = tension})
        End Sub

        ''' <summary>
        ''' Appends the specified GraphicsPath to this path.
        ''' </summary>
        Public Sub AddPath(addingPath As GraphicsPath, connect As Boolean)
            Call opSet.Add(New op_AddPath With {.path = addingPath, .connect = connect})
        End Sub

        ''' <summary>
        ''' Adds a sequence of connected Bezier splines to the current figure.
        ''' </summary>
        Public Sub AddBeziers(points As PointF())
            Call opSet.Add(New op_AddBeziers With {.points = points})
        End Sub

        ''' <summary>
        ''' Adds the geometry of the entire ellipse to the current figure.
        ''' </summary>
        Public Sub AddEllipse(rect As RectangleF)
            Call opSet.Add(New op_AddEllipseRect With {.rect = rect})
        End Sub

        ''' <summary>
        ''' Returns a rectangle that bounds this GraphicsPath.
        ''' </summary>
        Public Function GetBounds(Optional matrix As Matrix = Nothing, Optional pen As Pen = Nothing) As RectangleF
            Call opSet.Add(New op_GetBounds With {.matrix = matrix, .pen = pen})

            ' Compute bounding box from stored points
            Dim pts As PointF() = CollectPathPoints()
            If pts.Length = 0 Then
                Return New RectangleF(0, 0, 0, 0)
            End If

            Dim minX As Single = pts(0).X, minY As Single = pts(0).Y
            Dim maxX As Single = pts(0).X, maxY As Single = pts(0).Y

            For Each pt As PointF In pts
                If pt.X < minX Then minX = pt.X
                If pt.Y < minY Then minY = pt.Y
                If pt.X > maxX Then maxX = pt.X
                If pt.Y > maxY Then maxY = pt.Y
            Next

            Return New RectangleF(minX, minY, maxX - minX, maxY - minY)
        End Function

        ''' <summary>
        ''' Converts each curve in this path into a sequence of connected line segments.
        ''' </summary>
        Public Sub Flatten(Optional matrix As Matrix = Nothing, Optional flatness As Single = 0.25F)
            Call opSet.Add(New op_Flatten With {.matrix = matrix, .flatness = flatness})
        End Sub

        ''' <summary>
        ''' Replaces this path with curves that enclose the area that is filled when the path is drawn by the specified pen.
        ''' </summary>
        Public Sub Widen(pen As Pen, Optional matrix As Matrix = Nothing, Optional flatness As Single = 0.25F)
            Call opSet.Add(New op_Widen With {.pen = pen, .matrix = matrix, .flatness = flatness})
        End Sub

        ''' <summary>
        ''' Applies a warp transform, defined by a rectangle and a parallelogram, to this GraphicsPath.
        ''' </summary>
        Public Sub Warp(destPoints As PointF(), srcRect As RectangleF, Optional matrix As Matrix = Nothing, Optional warpMode As WarpMode = WarpMode.Perspective, Optional flatness As Single = 0.25F)
            Call opSet.Add(New op_Warp With {.destPoints = destPoints, .srcRect = srcRect, .matrix = matrix, .warpMode = warpMode, .flatness = flatness})
        End Sub

        ''' <summary>
        ''' Applies a transform matrix to this GraphicsPath.
        ''' </summary>
        Public Sub Transform(matrix As Matrix)
            Call opSet.Add(New op_Transform With {.matrix = matrix})
        End Sub

        ''' <summary>
        ''' Reverses the order of points in the PathPoints array of this GraphicsPath.
        ''' </summary>
        Public Sub Reverse()
            Call opSet.Add(New op_Reverse)
        End Sub

        ''' <summary>
        ''' Indicates whether the specified point is contained within this GraphicsPath.
        ''' </summary>
        Public Function IsVisible(x As Single, y As Single, Optional g As IGraphics = Nothing) As Boolean
            Return IsVisible(New PointF(x, y), g)
        End Function

        ''' <summary>
        ''' Indicates whether the specified point is contained within this GraphicsPath.
        ''' </summary>
        Public Function IsVisible(point As PointF, Optional g As IGraphics = Nothing) As Boolean
            Dim bounds As RectangleF = GetBounds()
            Return bounds.Contains(point)
        End Function

        ''' <summary>
        ''' Indicates whether the specified point is contained within (under) the outline of this GraphicsPath when drawn with the specified Pen.
        ''' </summary>
        Public Function IsOutlineVisible(x As Single, y As Single, pen As Pen, Optional g As IGraphics = Nothing) As Boolean
            Return IsOutlineVisible(New PointF(x, y), pen, g)
        End Function

        ''' <summary>
        ''' Indicates whether the specified point is contained within (under) the outline of this GraphicsPath when drawn with the specified Pen.
        ''' </summary>
        Public Function IsOutlineVisible(point As PointF, pen As Pen, Optional g As IGraphics = Nothing) As Boolean
            Dim bounds As RectangleF = GetBounds()
            If pen IsNot Nothing Then
                ' Expand bounds by pen width
                bounds.Inflate(pen.Width, pen.Width)
            End If
            Return bounds.Contains(point)
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of op) Implements Enumeration(Of op).GenericEnumerator
            For Each op As op In opSet
                Yield op
            Next
        End Function
    End Class

    ''' <summary>
    ''' Specifies the warping mode.
    ''' </summary>
    Public Enum WarpMode
        ''' <summary>
        ''' Specifies a perspective warp.
        ''' </summary>
        Perspective = 0
        ''' <summary>
        ''' Specifies a bilinear warp.
        ''' </summary>
        Bilinear = 1
    End Enum

    ''' <summary>
    ''' Specifies the smoothing/antialiasing quality applied to lines, curves and edges.
    ''' </summary>
    Public Enum SmoothingMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default mode (no antialiasing).
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies low speed, high quality (antialiased).
        ''' </summary>
        HighSpeed = 1

        ''' <summary>
        ''' Specifies high speed, low quality (no antialiasing).
        ''' </summary>
        HighQuality = 2

        ''' <summary>
        ''' Specifies no antialiasing.
        ''' </summary>
        None = 3

        ''' <summary>
        ''' Specifies antialiased rendering.
        ''' </summary>
        AntiAlias = 4
    End Enum

    ''' <summary>
    ''' Specifies how intermediate values between two endpoints are calculated during scaling or rotation.
    ''' </summary>
    Public Enum InterpolationMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default interpolation mode.
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies low quality interpolation (equivalent to NearestNeighbor).
        ''' </summary>
        Low = 1

        ''' <summary>
        ''' Specifies high quality interpolation (equivalent to HighQualityBicubic).
        ''' </summary>
        High = 2

        ''' <summary>
        ''' Specifies bilinear interpolation. No prefiltering is done.
        ''' </summary>
        Bilinear = 3

        ''' <summary>
        ''' Specifies bicubic interpolation. No prefiltering is done.
        ''' </summary>
        Bicubic = 4

        ''' <summary>
        ''' Specifies nearest-neighbor interpolation.
        ''' </summary>
        NearestNeighbor = 5

        ''' <summary>
        ''' Specifies high-quality, bilinear interpolation. Prefiltering ensures high-quality shrinking.
        ''' </summary>
        HighQualityBilinear = 6

        ''' <summary>
        ''' Specifies high-quality, bicubic interpolation. Prefiltering ensures high-quality shrinking.
        ''' </summary>
        HighQualityBicubic = 7
    End Enum

    ''' <summary>
    ''' Specifies how the source colors are combined with the background colors during rendering.
    ''' </summary>
    Public Enum CompositingMode

        ''' <summary>
        ''' Specifies that the color being rendered overwrites the background color.
        ''' </summary>
        SourceOver = 0

        ''' <summary>
        ''' Specifies that the color being rendered is blended with the background color. 
        ''' The blend is determined by the alpha component of the color being rendered.
        ''' </summary>
        SourceCopy = 1
    End Enum

    ''' <summary>
    ''' Specifies how pixels are offset during rendering.
    ''' </summary>
    Public Enum PixelOffsetMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default mode.
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies high speed, low quality rendering.
        ''' </summary>
        HighSpeed = 1

        ''' <summary>
        ''' Specifies high quality, low speed rendering.
        ''' </summary>
        HighQuality = 2

        ''' <summary>
        ''' Specifies no pixel offset.
        ''' </summary>
        None = 3

        ''' <summary>
        ''' Specifies that pixels are offset by -.5 units both horizontally and vertically 
        ''' for high speed antialiasing.
        ''' </summary>
        Half = 4
    End Enum

    ''' <summary>
    ''' Specifies the overall quality when rendering GDI+ objects.
    ''' </summary>
    Public Enum QualityMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default mode.
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies low quality, high speed rendering.
        ''' </summary>
        Low = 1

        ''' <summary>
        ''' Specifies high quality, low speed rendering.
        ''' </summary>
        High = 2
    End Enum

    ''' <summary>
    ''' Represents the state of a Graphics object. Returned by Save() and passed to Restore().
    ''' </summary>
    Public Class GraphicsState

        Private ReadOnly _stateIndex As Integer

        Sub New(stateIndex As Integer)
            _stateIndex = stateIndex
        End Sub

        ''' <summary>
        ''' Gets the index of this state in the state stack.
        ''' </summary>
        Public ReadOnly Property StateIndex As Integer
            Get
                Return _stateIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"GraphicsState[{_stateIndex}]"
        End Function
    End Class
#End If
End Namespace
