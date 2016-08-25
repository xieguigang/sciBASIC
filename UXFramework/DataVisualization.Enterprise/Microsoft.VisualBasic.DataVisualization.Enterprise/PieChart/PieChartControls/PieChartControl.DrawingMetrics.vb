#Region "Microsoft.VisualBasic::8971348d6b16ca4a137c5e69aa92ba1c, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.DrawingMetrics.vb"

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

Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Windows.Forms.Nexus

    Partial Public Class PieChart
        Inherits Control
        ''' <summary>
        ''' Represents the drawing data for a single pie slice. This includes
        ''' GraphicsPath items for each surface and edge of the slice, Regions for
        ''' hit testing, and Pens and Brushes for each surface.
        ''' </summary>
        Friend Class DrawingMetrics2
            Implements IComparable(Of DrawingMetrics2)
#Region "Constructor"
            ''' <summary>
            ''' Constructs a new DrawingMetrics.
            ''' </summary>
            ''' <param name="control">The control this object is associated with.</param>
            ''' <param name="item">The item this object is associated with.</param>
            ''' <param name="startAngle">The start angle of this pie slice, in radians.</param>
            ''' <param name="sweepAngle">The sweep angle of this pie slice, in radians.</param>
            Public Sub New(control As PieChart, item As PieChartItem, drawingBounds As Rectangle, startAngle As Single, sweepAngle As Single)
                Me._Control = control
                Me._Item = item
                Me._DrawingBounds = drawingBounds
                Me._StartAngle = CSng(startAngle Mod (2 * Math.PI))
                Me._SweepAngle = sweepAngle

                ConstructGraphics()
                ConstructPaths()
                ConstructRegions()
            End Sub
#End Region

#Region "Fields"
            Private _Control As PieChart
            Private _Item As PieChartItem
            Private _StartAngle As Single
            Private _SweepAngle As Single

            Private _SurfaceBrush As Brush
            Private _StartBrush As Brush
            Private _EndBrush As Brush
            Private _ExteriorBrush As Brush
            Private _EdgePen As Pen

            Private _VisibleRegion As Region
            Private _TopRegion As Region
            Private _InteriorRegion As Region
            Private _ExteriorRegion As Region

            Private _TopFace As GraphicsPath
            Private _BottomFace As GraphicsPath
            Private _StartFace As GraphicsPath
            Private _EndFace As GraphicsPath
            Private _ExteriorFace As GraphicsPath
            Private _StartFaceEdges As GraphicsPath
            Private _EndFaceEdges As GraphicsPath
            Private _ExteriorFaceEdges As GraphicsPath
#End Region

#Region "Properties"
            ''' <summary>
            ''' The control this object is associated with.
            ''' </summary>
            Public ReadOnly Property Control() As PieChart
                Get
                    Return _Control
                End Get
            End Property

            ''' <summary>
            ''' The item this control is associated with.
            ''' </summary>
            Public ReadOnly Property Item() As PieChartItem
                Get
                    Return _Item
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the bounds which the whole pie fits in.
            ''' </summary>
            Public Property DrawingBounds() As Rectangle

            ''' <summary>
            ''' The start angle of this control, in radians.
            ''' </summary>
            Public ReadOnly Property StartAngle() As Single
                Get
                    Return _StartAngle
                End Get
            End Property

            ''' <summary>
            ''' The sweep angle of this control, in radians.
            ''' </summary>
            Public ReadOnly Property SweepAngle() As Single
                Get
                    Return _SweepAngle
                End Get
            End Property

            ''' <summary>
            ''' The end angle of this control, in radians.
            ''' </summary>
            Public ReadOnly Property EndAngle() As Single
                Get
                    Dim result As Double = StartAngle + SweepAngle
                    If result > 2 * Math.PI Then
                        result -= 2 * Math.PI
                    End If
                    Return CSng(result)
                End Get
            End Property

            ''' <summary>
            ''' The transformed start angle.  See the TransformAngle method for details about why the angles need to be transformed.
            ''' </summary>
            Public ReadOnly Property TransformedStartAngle() As Single
                Get
                    Return TransformAngle(StartAngle)
                End Get
            End Property

            ''' <summary>
            ''' The transformed end angle. See the TransformAngle method for details about why the angles need to be transformed.
            ''' </summary>
            Public ReadOnly Property TransformedSweepAngle() As Single
                Get
                    ' if the SweepAngle is sufficiently close to 2 * pi, return 2 * pi to avoid floating point error incurred in
                    ' transforming the angle.
                    If Math.Abs(SweepAngle - 2 * Math.PI) < 0.0001 Then
                        Return CSng(2 * Math.PI)
                    End If

                    ' otherwise, use the transformed angle
                    Dim result As Double = TransformAngle(StartAngle + SweepAngle) - TransformedStartAngle
                    If result < 0 Then
                        result += 2 * Math.PI
                    End If
                    Return CSng(result)
                End Get
            End Property

            ''' <summary>
            ''' Gets the offset size of the pie slice.  This offset is the real screen offset
            ''' at which the pie slice should be drawn.
            ''' </summary>
            Public ReadOnly Property OffsetSize() As SizeF
                Get
                    Dim angle As Double = StartAngle + SweepAngle / 2
                    Dim x As Double = Item.Offset * Math.Cos(angle)
                    Dim y As Double = Item.Offset * Math.Sin(angle) * Control.Style.HeightWidthRatio
                    Return New SizeF(CSng(x), CSng(y))
                End Get
            End Property

            ''' <summary>
            ''' Gets whether or not the PieChartItem that this DrawingMetrics represents is the focused one.
            ''' </summary>
            Public ReadOnly Property IsFocused() As Boolean
                Get
                    Return Control.FocusedItem Is Me.Item
                End Get
            End Property
#End Region

#Region "Resource Methods"
            ''' <summary>
            ''' Constructs all of the pens and brushes required for drawing the slice.
            ''' </summary>
            Private Sub ConstructGraphics()
                Me._SurfaceBrush = CreateSurfaceBrush()
                Me._EdgePen = CreateEdgePen()

                _StartBrush = CreateInteriorBrush(CSng(StartAngle - Math.PI))
                _EndBrush = CreateInteriorBrush(EndAngle)
                Me._ExteriorBrush = CreateExteriorBrush()
            End Sub

            ''' <summary>
            ''' Disposes of all pens and brushes.
            ''' </summary>
            Private Sub DestroyGraphics()
                _SurfaceBrush.Dispose()
                _StartBrush.Dispose()
                _EndBrush.Dispose()
                _ExteriorBrush.Dispose()
                _EdgePen.Dispose()
            End Sub

            ''' <summary>
            ''' Recreates all of the pens and brushes required for drawing the slice.
            ''' </summary>
            Public Sub RecreateGraphics()
                DestroyGraphics()
                ConstructGraphics()
            End Sub

            ''' <summary>
            ''' Creates a brush for the exterior face.
            ''' </summary>
            ''' <returns>A brush used for filling the exterior face.</returns>
            Private Function CreateExteriorBrush() As Brush
                ' get the alpha-corrected surface color
                Dim trueColor As Color = GetAlphaTransparentSurfaceColor(Item.Color)

                ' blend the color using the brightness factor
                Dim blend As New ColorBlend()
                blend.Colors = New Color() {ChangeColorBrightness(trueColor, Control.Style.ShadowBrightnessFactor / 2), trueColor, ChangeColorBrightness(trueColor, Control.Style.ShadowBrightnessFactor)}

                ' set the blend positions
                blend.Positions = New Single() {0, 0.1F, 1.0F}

                ' create the brush and set the interpolation colors
                Dim brush As New LinearGradientBrush(New RectangleF(-DrawingBounds.Width \ 2, -DrawingBounds.Height \ 2, DrawingBounds.Width, DrawingBounds.Height), Color.Blue, Color.White, LinearGradientMode.Horizontal)
                brush.InterpolationColors = blend
                Return brush
            End Function

            ''' <summary>
            ''' Creates a brush for the top surface of the slice.
            ''' </summary>
            ''' <returns>A brush used for filling the top face.</returns>
            Private Function CreateSurfaceBrush() As Brush
                Return New SolidBrush(GetAlphaTransparentSurfaceColor(Item.Color))
            End Function

            ''' <summary>
            ''' Creates a brush for drawing the interior edge, based on the angle the edge is shown at.
            ''' </summary>
            ''' <param name="angle">The angle of the interior edge.</param>
            ''' <returns>A brush used for filling the interior face.</returns>
            Private Function CreateInteriorBrush(angle As Single) As Brush
                Return New SolidBrush(ChangeColorBrightness(GetAlphaTransparentSurfaceColor(Item.Color), CSng(Control.Style.ShadowBrightnessFactor * (1 - 0.8 * Math.Cos(angle)))))
            End Function

            ''' <summary>
            ''' Creates a brush for drawing the edges.
            ''' </summary>
            ''' <returns>A pen used for drawing the edges.</returns>
            Private Function CreateEdgePen() As Pen
                Dim brightnessFactor As Single = Control.ItemStyle.EdgeBrightnessFactor
                If IsFocused Then
                    brightnessFactor = Control.FocusedItemStyle.EdgeBrightnessFactor
                End If
                Return New Pen(ChangeColorBrightness(Item.Color, brightnessFactor), 1)
            End Function

            ''' <summary>
            ''' Gets the brush used for drawing the top surface.
            ''' </summary>
            Public ReadOnly Property SurfaceBrush() As Brush
                Get
                    Return _SurfaceBrush
                End Get
            End Property

            ''' <summary>
            ''' Gets the brush used for drawing the starting interior surface.
            ''' </summary>
            Public ReadOnly Property StartBrush() As Brush
                Get
                    Return _StartBrush
                End Get
            End Property

            ''' <summary>
            ''' Gets the brush used for drawing the ending interior surface.
            ''' </summary>
            Public ReadOnly Property EndBrush() As Brush
                Get
                    Return _EndBrush
                End Get
            End Property

            ''' <summary>
            ''' Gets the brush used for drawing the exterior surface.
            ''' </summary>
            Public ReadOnly Property ExteriorBrush() As Brush
                Get
                    Return _ExteriorBrush
                End Get
            End Property

            ''' <summary>
            ''' Gets the pen used for drawing the edges.
            ''' </summary>
            Public ReadOnly Property EdgePen() As Pen
                Get
                    Return _EdgePen
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the top surface.
            ''' </summary>
            Public ReadOnly Property TopFace() As GraphicsPath
                Get
                    Return _TopFace
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the bottom surface.
            ''' </summary>
            Public ReadOnly Property BottomFace() As GraphicsPath
                Get
                    Return _BottomFace
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the starting interior surface.
            ''' </summary>
            Public ReadOnly Property StartFace() As GraphicsPath
                Get
                    Return _StartFace
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the ending interior surface.
            ''' </summary>
            Public ReadOnly Property EndFace() As GraphicsPath
                Get
                    Return _EndFace
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the exterior surface.
            ''' </summary>
            Public ReadOnly Property ExteriorFace() As GraphicsPath
                Get
                    Return _ExteriorFace
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the edges bordering the starting interior face.
            ''' </summary>
            Public ReadOnly Property StartFaceEdges() As GraphicsPath
                Get
                    Return _StartFaceEdges
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the edges bordering the ending interior face.
            ''' </summary>
            Public ReadOnly Property EndFaceEdges() As GraphicsPath
                Get
                    Return _EndFaceEdges
                End Get
            End Property

            ''' <summary>
            ''' Gets the GraphicsPath that represents the edges bordering the exterior face.
            ''' </summary>
            Public ReadOnly Property ExteriorFaceEdges() As GraphicsPath
                Get
                    Return _ExteriorFaceEdges
                End Get
            End Property

            ''' <summary>
            ''' Gets the Region which represents the entire surface.
            ''' </summary>
            Public ReadOnly Property VisibleRegion() As Region
                Get
                    Return _VisibleRegion
                End Get
            End Property

            ''' <summary>
            ''' Gets the Region which represents the top surface.
            ''' </summary>
            Public ReadOnly Property TopRegion() As Region
                Get
                    Return _TopRegion
                End Get
            End Property

            ''' <summary>
            ''' Gets the Region which represents the interior surface(s).
            ''' </summary>
            Public ReadOnly Property InteriorRegion() As Region
                Get
                    Return _InteriorRegion
                End Get
            End Property

            ''' <summary>
            ''' Gets the Region which represents the exterior surface(s).
            ''' </summary>
            Public ReadOnly Property ExteriorRegion() As Region
                Get
                    Return _ExteriorRegion
                End Get
            End Property

            ''' <summary>
            ''' Disposes of all graphics and regions.
            ''' </summary>
            Friend Sub DestroyResources()
                DestroyGraphics()
                DestroyRegions()
            End Sub

            ''' <summary>
            ''' Disposes of all regions.
            ''' </summary>
            Private Sub DestroyRegions()
                _VisibleRegion.Dispose()
                _TopRegion.Dispose()
                _InteriorRegion.Dispose()
                _ExteriorRegion.Dispose()
            End Sub

            ''' <summary>
            ''' Constructs regions which represent the boundaries of all faces of the pie slice.
            ''' </summary>
            Private Sub ConstructRegions()
                ' create the top region
                Me._TopRegion = New Region(TopFace)

                ' create the exterior region
                Me._ExteriorRegion = New Region()
                Me._ExteriorRegion.MakeEmpty()
                If ExteriorFace IsNot Nothing Then
                    Me._ExteriorRegion.Union(ExteriorFace)
                End If

                ' create the interior region
                Me._InteriorRegion = New Region()
                Me._InteriorRegion.MakeEmpty()
                If StartFace IsNot Nothing Then
                    Me._InteriorRegion.Union(StartFace)
                End If
                If EndFace IsNot Nothing Then
                    Me._InteriorRegion.Union(EndFace)
                End If

                ' create the visible region
                Me._VisibleRegion = New Region()
                Me._VisibleRegion.MakeEmpty()
                Me._VisibleRegion.Union(Me._TopRegion)
                Me._VisibleRegion.Union(Me._ExteriorRegion)
                Me._VisibleRegion.Union(Me._InteriorRegion)
            End Sub
#End Region

#Region "Surface Methods"
            ''' <summary>
            ''' Constructs all GraphicsPaths which represent every face of the slice.
            ''' These GraphicsPaths are centered at (0,0), and a transform is applied to the Graphics
            ''' object which draws the GraphicsPaths.
            ''' </summary>
            Private Sub ConstructPaths()
                Dim offset As SizeF = OffsetSize

                ' create the bottom face
                _BottomFace = New GraphicsPath()
                _BottomFace.AddPie(offset.Width - Control.Style.EllipseWidth, offset.Height - Control.Style.EllipseHeight, 2 * Control.Style.EllipseWidth, 2 * Control.Style.EllipseHeight, CSng(TransformedStartAngle * 180 / Math.PI), CSng(TransformedSweepAngle * 180 / Math.PI))

                ' create the top face
                _TopFace = New GraphicsPath()
                _TopFace.AddPie(offset.Width - Control.Style.EllipseWidth, -Control.Style.VisualThickness + offset.Height - Control.Style.EllipseHeight, 2 * Control.Style.EllipseWidth, 2 * Control.Style.EllipseHeight, CSng(TransformedStartAngle * 180 / Math.PI), CSng(TransformedSweepAngle * 180 / Math.PI))

                ' create the interior start face if it is visible
                If StartAngle >= Math.PI / 2 AndAlso StartAngle <= 3 * Math.PI / 2 Then
                    _StartFace = CreateInteriorFace(StartAngle, _StartFaceEdges)
                Else
                    _StartFace = Nothing
                End If

                ' create the interior end face if it is visible
                If EndAngle >= 3 * Math.PI / 2 OrElse EndAngle <= Math.PI / 2 Then
                    _EndFace = CreateInteriorFace(EndAngle, _EndFaceEdges)
                Else
                    _EndFace = Nothing
                End If

                ' create the exterior face
                _ExteriorFace = CreateExteriorFace(_ExteriorFaceEdges)
            End Sub

            ''' <summary>
            ''' Creates an interior face of a pie slice.
            ''' </summary>
            ''' <param name="angle">The angle of the face.</param>
            ''' <param name="edgePath">The GraphicsPath which represents the edges of this face.</param>
            ''' <returns>The GraphicsPath which represents the surface of the face.</returns>
            Private Function CreateInteriorFace(angle As Single, ByRef edgePath As GraphicsPath) As GraphicsPath
                Dim path As New GraphicsPath()
                Dim offset As SizeF = OffsetSize
                Dim points As PointF() = New PointF(3) {}
                points(0) = New PointF(offset.Width, offset.Height - Control.Style.VisualThickness)
                points(1) = New PointF(offset.Width, offset.Height)
                points(2) = New PointF(CSng(offset.Width + Control.Style.EllipseWidth * Math.Cos(angle)), CSng(offset.Height + Control.Style.EllipseHeight * Math.Sin(angle)))
                points(3) = New PointF(points(2).X, points(2).Y - Control.Style.VisualThickness)
                path.AddPolygon(points)

                edgePath = New GraphicsPath()
                edgePath.AddLine(points(0), points(1))
                edgePath.StartFigure()
                edgePath.AddLine(points(2), points(3))

                Return path
            End Function

            ''' <summary>
            ''' Creates an exterior face of a pie slice.
            ''' </summary>
            ''' <param name="edgePath">The GraphicsPath which represents the edges of this face.</param>
            ''' <returns>The GraphicsPath which represents the surface of the face.</returns>
            Private Function CreateExteriorFace(ByRef edgePath As GraphicsPath) As GraphicsPath
                Dim offset As SizeF = OffsetSize

                ' create an array to hold the possible angles to the parts of the external face
                Dim angles As Single() = New Single(3) {}
                ' create an array to hold the points on the exterior of the pie at each of the above angles
                Dim points As PointF() = New PointF(3) {}

                ' boolean values to determine which of the above angles an exterior face should be drawn between
                Dim drawBetween01 As Boolean = False
                Dim drawBetween12 As Boolean = False
                Dim drawBetween23 As Boolean = False
                Dim drawBetween03 As Boolean = False

                ' start with angle 0 being 0 radians and angle 3 being pi radians (which represents the entire visible part of the pie)
                angles(0) = 0
                angles(3) = CSng(Math.PI)
                points(0) = New PointF(offset.Width + Control.Style.EllipseWidth, offset.Height)
                points(3) = New PointF(offset.Width - Control.Style.EllipseWidth, offset.Height)

                ' test where the StartAngle and EndAngle are to determine which angles an exterior face needs to be drawn between
                If StartAngle >= Math.PI AndAlso EndAngle >= Math.PI Then
                    If SweepAngle >= Math.PI Then
                        ' in this case, the whole front side of the graph contains the exterior of this slice
                        drawBetween03 = True
                    Else
                        ' in this case, the whole exterior is hidden on the backside of the pie, so return null
                        edgePath = Nothing
                        Return Nothing
                    End If
                ElseIf EndAngle >= Math.PI Then
                    ' the start angle is in visible range, but not the end angle
                    drawBetween23 = True
                    angles(1) = 0
                    angles(2) = StartAngle
                ElseIf StartAngle >= Math.PI Then
                    ' the end angle is in visible range, but not the start angle
                    drawBetween01 = True
                    angles(1) = EndAngle
                    angles(2) = CSng(Math.PI)
                Else
                    If SweepAngle < Math.PI Then
                        ' the exterior is only between the StartAngle and the EndAngle on the visible side of the pie
                        drawBetween12 = True
                        angles(1) = StartAngle
                        angles(2) = EndAngle
                    Else
                        ' the exterior starts on the visible side, wraps around the backside, and reappears on the visible side
                        ' two exterior pieces must be drawn
                        drawBetween01 = True
                        drawBetween23 = True
                        angles(1) = StartAngle
                        angles(2) = EndAngle
                    End If
                End If

                ' for the angles 1 and 2, calculate the endpoints of those edges, and then transform the angles into drawing-ready angles
                For i As Integer = 1 To 2
                    points(i) = New PointF(CSng(offset.Width + Control.Style.EllipseWidth * Math.Cos(angles(i))), CSng(offset.Height + Control.Style.EllipseHeight * Math.Sin(angles(i))))
                    angles(i) = TransformAngle(angles(i))
                Next

                ' construct new GraphicsPaths for the surface and edges, and add the appropriate figures
                Dim path As New GraphicsPath()
                edgePath = New GraphicsPath()
                If drawBetween01 Then
                    AddExteriorFigure(path, edgePath, points(0), points(1), angles(0), angles(1) - angles(0))
                End If
                If drawBetween12 Then
                    AddExteriorFigure(path, edgePath, points(1), points(2), angles(1), angles(2) - angles(1))
                End If
                If drawBetween23 Then
                    AddExteriorFigure(path, edgePath, points(2), points(3), angles(2), angles(3) - angles(2))
                End If
                If drawBetween03 Then
                    AddExteriorFigure(path, edgePath, points(0), points(3), angles(0), angles(3) - angles(0))
                End If

                Return path
            End Function

            ''' <summary>
            ''' Adds an exterior face part to the graphics paths given.
            ''' </summary>
            ''' <param name="path">The GraphicsPath which represents the surface of the exterior.</param>
            ''' <param name="edgePath">The GraphicsPath which represents the edges on the exterior.</param>
            ''' <param name="rightPoint">The exterior point at the left of the exterior arc.</param>
            ''' <param name="leftPoint">The exterior point at the right of the exterior arc.</param>
            ''' <param name="startAngle">The start angle of the arc.</param>
            ''' <param name="sweepAngle">The sweep angle of the arc.</param>
            Private Sub AddExteriorFigure(path As GraphicsPath, edgePath As GraphicsPath, rightPoint As PointF, leftPoint As PointF, startAngle As Single, sweepAngle As Single)
                Dim offset As SizeF = OffsetSize

                ' calculate the exterior points with the thickness of the slice taken into effect
                Dim rightPointThick As New PointF(rightPoint.X, rightPoint.Y - Control.Style.VisualThickness)
                Dim leftPointThick As New PointF(leftPoint.X, leftPoint.Y - Control.Style.VisualThickness)

                ' draw the exterior figure
                path.StartFigure()
                path.AddLine(rightPoint, rightPointThick)
                path.AddArc(offset.Width - Control.Style.EllipseWidth, offset.Height - Control.Style.VisualThickness - Control.Style.EllipseHeight, 2 * Control.Style.EllipseWidth, 2 * Control.Style.EllipseHeight, CSng(startAngle * 180 / Math.PI), CSng(sweepAngle * 180 / Math.PI))
                path.AddLine(leftPointThick, leftPoint)
                path.AddArc(offset.Width - Control.Style.EllipseWidth, offset.Height - Control.Style.EllipseHeight, 2 * Control.Style.EllipseWidth, 2 * Control.Style.EllipseHeight, CSng((startAngle + sweepAngle) * 180 / Math.PI), CSng(-sweepAngle * 180 / Math.PI))

                ' draw the exterior edges
                edgePath.StartFigure()
                edgePath.AddLine(rightPoint, rightPointThick)
                edgePath.StartFigure()
                edgePath.AddLine(leftPoint, leftPointThick)
            End Sub
#End Region

#Region "Rendering Methods"
            ''' <summary>
            ''' Draws the bottom edges of the slice.
            ''' </summary>
            ''' <param name="g">The Graphics on which the control is being rendered.</param>
            Friend Sub RenderBottom(g As Graphics)
                If Control.Style.ShowEdges Then
                    g.DrawPath(EdgePen, BottomFace)
                End If
            End Sub

            ''' <summary>
            ''' Draws the top faces and edges of the slice.
            ''' </summary>
            ''' <param name="g">The Graphics on which the control is being rendered.</param>
            Friend Sub RenderTop(g As Graphics)
                g.FillPath(SurfaceBrush, TopFace)
                If Control.Style.ShowEdges Then
                    g.DrawPath(EdgePen, TopFace)
                End If
            End Sub

            ''' <summary>
            ''' Draws the interior faces and edges of the slice.
            ''' </summary>
            ''' <param name="g">The Graphics on which the control is being rendered.</param>
            Friend Sub RenderInterior(g As Graphics)
                If StartFace IsNot Nothing Then
                    g.FillPath(StartBrush, StartFace)
                    If Control.Style.ShowEdges Then
                        g.DrawPath(EdgePen, StartFaceEdges)
                    End If
                End If

                If EndFace IsNot Nothing Then
                    g.FillPath(EndBrush, EndFace)
                    If Control.Style.ShowEdges Then
                        g.DrawPath(EdgePen, EndFaceEdges)
                    End If
                End If
            End Sub

            ''' <summary>
            ''' Draws the exterior faces and edges of the slice.
            ''' </summary>
            ''' <param name="g">The Graphics on which the control is being rendered.</param>
            Friend Sub RenderExterior(g As Graphics)
                If ExteriorFace IsNot Nothing Then
                    g.FillPath(ExteriorBrush, ExteriorFace)
                    If Control.Style.ShowEdges Then
                        g.DrawPath(EdgePen, ExteriorFaceEdges)
                    End If
                End If
            End Sub

            ''' <summary>
            ''' Gets the position at which the text of the object should be drawn.
            ''' </summary>
            ''' <remarks>This method returns the center point in the block where the text is drawn.</remarks>
            ''' <returns>The point at which the text should be drawn.</returns>
            Public Function GetTextPosition() As PointF
                ' draw the text along the bisecting line of the control, 2/3 of the way out on the radius
                Dim angle As Single = StartAngle + SweepAngle / 2
                Return New PointF(CSng(OffsetSize.Width + Control.Style.EllipseWidth * 2 / 3 * Math.Cos(angle)), CSng(-Control.Style.VisualThickness + OffsetSize.Height + Control.Style.EllipseHeight * 2 / 3 * Math.Sin(angle)))
            End Function

            ''' <summary>
            ''' Draws the text of the slice.
            ''' </summary>
            ''' <param name="g">The Graphics on which the control is being rendered.</param>
            Friend Sub RenderText(g As Graphics)
                If Not String.IsNullOrEmpty(Item.Text) AndAlso Control.Style.TextDisplayMode <> TextDisplayTypes.Never Then
                    Dim format As New StringFormat()
                    format.Alignment = StringAlignment.Center
                    format.LineAlignment = StringAlignment.Center
                    Dim textPosition As PointF = GetTextPosition()
                    If Control.Style.TextDisplayMode = TextDisplayTypes.Always Then
                        g.DrawString(Item.Text, Control.Font, SystemBrushes.ControlText, textPosition, format)
                    Else
                        Dim size As SizeF = g.MeasureString(Item.Text, Control.Font, textPosition, format)
                        Dim bounds As New Rectangle(CInt(Math.Truncate(Math.Round(textPosition.X - size.Width / 2))), CInt(Math.Truncate(Math.Round(textPosition.Y - size.Height / 2))), CInt(Math.Truncate(Math.Round(size.Width))), CInt(Math.Truncate(Math.Round(size.Height))))
                        If TopRegion.IsVisible(bounds.Left, bounds.Top) AndAlso TopRegion.IsVisible(bounds.Left, bounds.Bottom) AndAlso TopRegion.IsVisible(bounds.Right, bounds.Top) AndAlso TopRegion.IsVisible(bounds.Right, bounds.Bottom) Then
                            g.DrawString(Item.Text, Control.Font, SystemBrushes.ControlText, textPosition, format)
                        End If
                    End If
                End If
            End Sub
#End Region

#Region "Methods"
            ''' <summary>
            ''' Transforms the real angle of a pie slice into the angle that can be drawn at the given inclination.
            ''' The angle must be transformed because when the circle representing the pie is drawn inclined,
            ''' the angle on the circle must be made to match the angle on the corresponding ellipse.
            ''' </summary>
            ''' <param name="angle">The angle on a circle being transformed into an ellipse.</param>
            ''' <returns>The angle as it appears on an ellipse.</returns>
            Private Function TransformAngle(angle As Single) As Single
                Dim x As Double = Control.Style.EllipseWidth * Math.Cos(angle)
                Dim y As Double = Control.Style.EllipseHeight * Math.Sin(angle)
                Dim result As Double = Math.Atan2(y, x)
                If result < 0 Then
                    result += 2 * Math.PI
                End If
                Return CSng(result)
            End Function

            ''' <summary>
            ''' Changes the brightness of a color by a correctionFactor, which is an integer between
            ''' -1 and 1.  A correction factor of -1 will make the color black, while a correction
            ''' factor of 1 will make the color white.
            ''' </summary>
            ''' <param name="color">The color to change the brightness of.</param>
            ''' <param name="correctionFactor">The factor to change the color's brightness by.</param>
            ''' <returns>The color with modified brightness.</returns>
            Friend Shared Function ChangeColorBrightness(color As Color, correctionFactor As Single) As Color
                If correctionFactor = 0 OrElse Single.IsNaN(correctionFactor) Then
                    Return color
                End If

                Dim red As Single = CSng(color.R)
                Dim green As Single = CSng(color.G)
                Dim blue As Single = CSng(color.B)
                If correctionFactor < 0 Then
                    correctionFactor += 1
                    red *= correctionFactor
                    green *= correctionFactor
                    blue *= correctionFactor
                Else
                    red += (255 - red) * correctionFactor
                    green += (255 - green) * correctionFactor
                    blue += (255 - blue) * correctionFactor
                End If
                Return System.Drawing.Color.FromArgb(color.A, CInt(Math.Truncate(red)), CInt(Math.Truncate(green)), CInt(Math.Truncate(blue)))
            End Function

            ''' <summary>
            ''' Gets the correct alpha-transparent surface color from a given color.  This
            ''' takes into effect the proper brightness correction as well, and differentiates
            ''' between the focused and nonfocused state.
            ''' </summary>
            ''' <param name="color">The color to transform.</param>
            ''' <returns>The transformed color.</returns>
            Public Function GetAlphaTransparentSurfaceColor(color As Color) As Color
                Dim alpha As Single = Control.ItemStyle.SurfaceAlphaTransparency
                Dim brightnessFactor As Single = Control.ItemStyle.SurfaceBrightnessFactor
                If Control.FocusedItem Is Me.Item Then
                    alpha = Control.FocusedItemStyle.SurfaceAlphaTransparency
                    brightnessFactor = Control.FocusedItemStyle.SurfaceBrightnessFactor
                End If

                Return Color.FromArgb(CInt(Math.Truncate(alpha * color.A)), ChangeColorBrightness(color, brightnessFactor))
            End Function
#End Region

#Region "IComparable<T>"
            ''' <summary>
            ''' Gets whether or not this slice is a "top" slice, meaning that the slice contains 3*pi / 2 in its range
            ''' and is thus at the top of the chart on the screen.
            ''' </summary>
            Public ReadOnly Property IsTopItem() As Boolean
                Get
                    Dim start As Double = StartAngle
                    Dim [end] As Double = StartAngle + SweepAngle
                    If start > 3 * Math.PI / 2 Then
                        start -= 2 * Math.PI
                        [end] -= 2 * Math.PI
                    End If

                    Return start < 3 * Math.PI / 2 AndAlso [end] > 3 * Math.PI / 2
                End Get
            End Property

            ''' <summary>
            ''' Gets whether or not this is a "left" slice, meaning that the slice is bound between
            ''' pi / 2 and 3 * pi / 2 and is thus on the left of the chart on the screen.
            ''' </summary>
            Public ReadOnly Property IsLeftItem() As Boolean
                Get
                    Return StartAngle > Math.PI / 2 AndAlso StartAngle < 3 * Math.PI / 2 AndAlso EndAngle > Math.PI / 2 AndAlso EndAngle < 3 * Math.PI / 2
                End Get
            End Property

            ''' <summary>
            ''' Gets whether or not this is a "right" slice, meaning that the slice is bound
            ''' between 3 * pi / 2 and 5 * pi / 2 and is thus on the right of the chart on the screen.
            ''' </summary>
            Public ReadOnly Property IsRightItem() As Boolean
                Get
                    Return (StartAngle < Math.PI / 2 OrElse StartAngle > 3 * Math.PI / 2) AndAlso (EndAngle < Math.PI / 2 OrElse EndAngle > 3 * Math.PI / 2)
                End Get
            End Property

            ''' <summary>
            ''' Gets whether or not this slice is a "bottom" slice, meaning that the slice contains pi / 2 in its range
            ''' and is thus at the bottom of the chart on the screen.
            ''' </summary>
            Public ReadOnly Property IsBottomItem() As Boolean
                Get
                    Dim start As Double = StartAngle
                    Dim [end] As Double = StartAngle + SweepAngle
                    If start > Math.PI / 2 Then
                        start -= 2 * Math.PI
                        [end] -= 2 * Math.PI
                    End If

                    Return start < Math.PI / 2 AndAlso [end] > Math.PI / 2
                End Get
            End Property

            ''' <summary>
            ''' Compares two DrawingMetrics.  The criteria for comparison is what order
            ''' the items should be drawn in.
            ''' </summary>
            ''' <param name="other">The other object being compared.</param>
            ''' <returns>A positive value if this object is greater, 0 if the objects are equal, or a negative value if this object is less.</returns>
            Public Function CompareTo(other As DrawingMetrics2) As Integer Implements IComparable(Of PieChart.DrawingMetrics2).CompareTo
                If Me Is other Then
                    Return 0
                End If

                Dim thisTop As Boolean = Me.IsTopItem
                Dim thisLeft As Boolean = Me.IsLeftItem
                Dim thisRight As Boolean = Me.IsRightItem
                Dim thisBottom As Boolean = Me.IsBottomItem
                Dim otherTop As Boolean = other.IsTopItem
                Dim otherLeft As Boolean = other.IsLeftItem
                Dim otherRight As Boolean = other.IsRightItem
                Dim otherBottom As Boolean = other.IsBottomItem

                If thisTop AndAlso otherTop Then
                    Return 0
                    ' if both are on top, they are equal
                ElseIf otherTop Then
                    Return 1
                    ' if the other is on top and this is not, other is greater because it should be drawn first
                ElseIf thisTop Then
                    Return -1
                End If
                ' if this is on top and the other is not, other is less because this should be drwan first
                If thisBottom AndAlso otherBottom Then
                    Return 0
                    ' if both are on bottom, they are equal
                ElseIf thisBottom Then
                    Return 1
                    ' if this is on the bottom and the other is not, other is less because this should be drawn first
                ElseIf otherBottom Then
                    Return -1
                End If
                ' if other is on the bottom and the this is not, other is greater because it should be drawn first
                If thisLeft AndAlso otherLeft Then
                    Return -Me.StartAngle.CompareTo(other.StartAngle)
                    ' if both on left, use opposite of the angle comparison
                ElseIf thisRight AndAlso otherRight Then
                    ' use the angle comparison
                    Dim thisAngle As Double = If(Me.EndAngle < Math.PI, Me.EndAngle + Math.PI * 2, Me.EndAngle)
                    Dim otherAngle As Double = If(other.EndAngle < Math.PI, other.EndAngle + Math.PI * 2, other.EndAngle)
                    Return thisAngle.CompareTo(otherAngle)
                Else
                    Return If(thisLeft, 1, -1)
                End If
                ' return the left ones greater than the right ones for consistent ordering
            End Function
#End Region
        End Class
    End Class
End Namespace
