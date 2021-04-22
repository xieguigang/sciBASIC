#Region "Microsoft.VisualBasic::17fe975436a0c33229024a3f96b26c7c, mime\application%pdf\PdfFileWriter\DrawWPFPath.vb"

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

    ' Enum YAxisDirection
    ' 
    '     Down, Up
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum FillRule
    ' 
    '     EvenOdd, NonZero
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class DrawWPFPath
    ' 
    '     Properties: BlendMode, BrushOpacity, DashArray, DashPhase, FillRule
    '                 LineCap, LineJoin, MiterLimit, PathBBoxHeight, PathBBoxWidth
    '                 PathBBoxX, PathBBoxY, PathYAxis, PenOpacity, PenWidth
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: PathToDrawing, SizeToDrawing
    ' 
    '     Sub: BuildPath, Draw, ResetBrush, ResetPen, SetAspectRatio
    '          (+7 Overloads) SetBrush, (+2 Overloads) SetPen, SetPenWidth, SetTransformation, UseCurrectBrush
    '          UseCurrectPen
    '     Class UseCurrent
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	DrawWPFPath
'	Draw Windows Presentation Foundation path.
'
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'	The PDF File Writer library was enhanced to allow drawing of graphic
'	artwork using Windows Presentation Foundation (WPF) classes.
'	These enhancements were proposed by Elena Malnati elena@yelleaf.com.
'	I would like to thank her for providing me with the source code
'	to implement them. Further I would like to thank Joe Cridge for
'	his contribution of code to convert elliptical arc to Bezier curve.
'	The source code was modified to be consistent in style to the rest
'	of the library. Developers of Windows Forms application can benefit
'	from all of these enhancements
'	For further information visit www.joecridge.me/bezier.pdf.
'	Also visit http://p5js.org/ for some coolness
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Imports System
Imports System.Drawing
Imports SysMedia = System.Windows.Media
Imports SysWin = System.Windows
Imports stdNum = System.Math

''' <summary>
''' Y Axis direction
''' </summary>
Public Enum YAxisDirection
    ''' <summary>
    ''' Down as per Microsoft convention.
    ''' </summary>
    Down
    ''' <summary>
    ''' Up as per PDF convention
    ''' </summary>
    Up
End Enum

''' <summary>
''' Fill rule enumeration
''' </summary>
Public Enum FillRule
    ''' <summary>
    ''' Even odd rule
    ''' </summary>
    EvenOdd
    ''' <summary>
    ''' Non zero rule
    ''' </summary>
    NonZero
End Enum

''' <summary>
''' Draw WPF path
''' </summary>
Public Class DrawWPFPath

    ''' <summary>
    ''' Input path bounding box y axis direction (default is down)
    ''' </summary>
    Dim _PathYAxis As YAxisDirection
    ''' <summary>
    ''' Input path bounding box left position
    ''' </summary>
    Dim _PathBBoxX As Double
    ''' <summary>
    ''' Input path bounding box top (y axis down) or bottom (y axis up) position
    ''' </summary>
    Dim _PathBBoxY As Double
    ''' <summary>
    ''' Input path bounding box width
    ''' </summary>
    Dim _PathBBoxWidth As Double
    ''' <summary>
    ''' Input path bounding box height
    ''' </summary>
    Dim _PathBBoxHeight As Double
    ''' <summary>
    ''' Fill rule
    ''' </summary>
    Public Property FillRule As FillRule
    ''' <summary>
    ''' Blend mode
    ''' </summary>
    Public Property BlendMode As BlendMode
    ''' <summary>
    ''' Brush opacity
    ''' </summary>
    Public Property BrushOpacity As Double
    ''' <summary>
    ''' Pen opacity
    ''' </summary>
    Public Property PenOpacity As Double
    ''' <summary>
    ''' Pen width
    ''' </summary>
    Public Property PenWidth As Double
    ''' <summary>
    ''' Line cap
    ''' </summary>
    Public Property LineCap As PdfLineCap
    ''' <summary>
    ''' Line join
    ''' </summary>
    Public Property LineJoin As PdfLineJoin
    ''' <summary>
    ''' Miter limit
    ''' </summary>
    Public Property MiterLimit As Double
    ''' <summary>
    ''' Pen's dash array
    ''' </summary>
    Public Property DashArray As Double()
    ''' <summary>
    ''' Pen's dash starting phase
    ''' </summary>
    Public Property DashPhase As Double

    Public Property PathYAxis As YAxisDirection
        Get
            Return _PathYAxis
        End Get
        Friend Set(ByVal value As YAxisDirection)
            _PathYAxis = value
        End Set
    End Property

    Public Property PathBBoxX As Double
        Get
            Return _PathBBoxX
        End Get
        Friend Set(ByVal value As Double)
            _PathBBoxX = value
        End Set
    End Property

    Public Property PathBBoxY As Double
        Get
            Return _PathBBoxY
        End Get
        Friend Set(ByVal value As Double)
            _PathBBoxY = value
        End Set
    End Property

    Public Property PathBBoxWidth As Double
        Get
            Return _PathBBoxWidth
        End Get
        Friend Set(ByVal value As Double)
            _PathBBoxWidth = value
        End Set
    End Property

    Public Property PathBBoxHeight As Double
        Get
            Return _PathBBoxHeight
        End Get
        Friend Set(ByVal value As Double)
            _PathBBoxHeight = value
        End Set
    End Property

    Friend MediaPath As SysMedia.PathGeometry
    Friend DrawRectX As Double
    Friend DrawRectY As Double
    Friend DrawRectWidth As Double
    Friend DrawRectHeight As Double
    Friend ScaleX As Double
    Friend ScaleY As Double
    Friend TransX As Double
    Friend TransY As Double
    Friend NonStroking As Object
    Friend Stroking As Object

    Private Class UseCurrent
    End Class

    ''' <summary>
    ''' Set path geometry
    ''' </summary>
    ''' <param name="PathString">Path geometry text string</param>
    ''' <param name="YAxis">Y Axix direction</param>
    Public Sub New(ByVal PathString As String, ByVal YAxis As YAxisDirection)
        Me.New(SysMedia.PathGeometry.CreateFromGeometry(SysMedia.Geometry.Parse(PathString)), YAxis)
    End Sub

    ''' <summary>
    ''' Draw WPF path constructor
    ''' </summary>
    ''' <param name="MediaPath">System.Windows.Media path geometry</param>
    ''' <param name="PathYAxis">Y Axix direction</param>
    Public Sub New(ByVal MediaPath As SysMedia.PathGeometry, ByVal PathYAxis As YAxisDirection)
        ' save media path
        Me.MediaPath = MediaPath

        ' save path rectangle and y axis direction
        PathBBoxX = MediaPath.Bounds.X
        PathBBoxY = MediaPath.Bounds.Y
        PathBBoxWidth = MediaPath.Bounds.Width
        PathBBoxHeight = MediaPath.Bounds.Height
        Me.PathYAxis = PathYAxis

        ' test arguments
        If PathBBoxWidth = 0 AndAlso PathBBoxHeight = 0 Then Throw New ApplicationException("DrawWPFPath: Path bounding box is empty")

        ' initialization values
        FillRule = If(MediaPath.FillRule = SysMedia.FillRule.EvenOdd, FillRule.EvenOdd, FillRule.NonZero)
        BlendMode = BlendMode.Normal
        BrushOpacity = 1.0
        PenOpacity = 1.0
        PenWidth = -1
        LineCap = CType(-1, PdfLineCap)
        LineJoin = CType(-1, PdfLineJoin)
        MiterLimit = -1
        Return
    End Sub

    ''' <summary>
    ''' Reset non-stroking brush to no fill
    ''' </summary>
    Public Sub ResetBrush()
        NonStroking = Nothing
        BrushOpacity = 1.0
        Return
    End Sub

    ''' <summary>
    ''' Filling the path will use the current color
    ''' </summary>
    Public Sub UseCurrectBrush()
        ResetBrush()
        NonStroking = New UseCurrent()
        Return
    End Sub

    ''' <summary>
    ''' Set brush to solid color
    ''' </summary>
    ''' <param name="BrushColor">Solid color</param>
    ''' <remarks>
    ''' <para>The method sets all 4 color components: Alpha Red Green and Blue.</para>
    ''' </remarks>
    Public Sub SetBrush(ByVal BrushColor As Color)
        NonStroking = BrushColor
        BrushOpacity = BrushColor.A / 255.0
        Return
    End Sub

    ''' <summary>
    ''' Set brush to solid color
    ''' </summary>
    ''' <param name="SolidColorBrush">Media solid color brush</param>
    Public Sub SetBrush(ByVal SolidColorBrush As SysMedia.SolidColorBrush)
        NonStroking = Color.FromArgb(SolidColorBrush.Color.R, SolidColorBrush.Color.G, SolidColorBrush.Color.B)
        BrushOpacity = SolidColorBrush.Color.A / 255.0 * SolidColorBrush.Opacity
        Return
    End Sub

    ''' <summary>
    ''' Set brush
    ''' </summary>
    ''' <param name="AxialShading">Axial shading</param>
    ''' <param name="BrushOpacity">Brush opacity (0.0 to 1.0)</param>
    Public Sub SetBrush(ByVal AxialShading As PdfAxialShading, ByVal Optional BrushOpacity As Double = 1.0)
        NonStroking = AxialShading
        Me.BrushOpacity = BrushOpacity
        Return
    End Sub

    ''' <summary>
    ''' Set brush
    ''' </summary>
    ''' <param name="LinearGradientBrush">Linear gradient brush</param>
    ''' <remarks>This method sets BrushOpacity.</remarks>
    Public Sub SetBrush(ByVal LinearGradientBrush As SysMedia.LinearGradientBrush)
        NonStroking = LinearGradientBrush
        BrushOpacity = LinearGradientBrush.Opacity
        Return
    End Sub

    ''' <summary>
    ''' Set brush
    ''' </summary>
    ''' <param name="RadialShading">PDF radial shading brush</param>
    ''' <param name="BrushOpacity">Brush opacity</param>
    Public Sub SetBrush(ByVal RadialShading As PdfRadialShading, ByVal Optional BrushOpacity As Double = 1.0)
        NonStroking = RadialShading
        Me.BrushOpacity = BrushOpacity
        Return
    End Sub

    ''' <summary>
    ''' Set brush
    ''' </summary>
    ''' <param name="RadialGradientBrush">Radial gradient brush</param>
    ''' <remarks>This method sets BrushOpacity.</remarks>
    Public Sub SetBrush(ByVal RadialGradientBrush As SysMedia.RadialGradientBrush)
        NonStroking = RadialGradientBrush
        BrushOpacity = RadialGradientBrush.Opacity
        Return
    End Sub

    ''' <summary>
    ''' Set brush
    ''' </summary>
    ''' <param name="TilingPattern">PDF tiling pattern resource</param>
    ''' <param name="BrushOpacity">Brush opacity</param>
    Public Sub SetBrush(ByVal TilingPattern As PdfTilingPattern, ByVal Optional BrushOpacity As Double = 1.0)
        NonStroking = TilingPattern
        Me.BrushOpacity = BrushOpacity
        Return
    End Sub

    ''' <summary>
    ''' Reset pen
    ''' </summary>
    ''' <remarks>Pen is not defined.</remarks>
    Public Sub ResetPen()
        Stroking = Nothing
        PenOpacity = 1.0
        PenWidth = -1
        Return
    End Sub

    ''' <summary>
    ''' Pen color will use the current color
    ''' </summary>
    Public Sub UseCurrectPen()
        ResetPen()
        Stroking = New UseCurrent()
        Return
    End Sub

    ''' <summary>
    ''' Set pen color
    ''' </summary>
    ''' <param name="PenColor">Pen color</param>
    Public Sub SetPen(ByVal PenColor As Color)
        Stroking = PenColor
        PenOpacity = PenColor.A / 255.0
        Return
    End Sub

    ''' <summary>
    ''' Set media pen
    ''' </summary>
    ''' <param name="MediaPen">Media pen</param>
    Public Sub SetPen(ByVal MediaPen As SysMedia.Pen)
        If MediaPen.Brush Is Nothing OrElse MediaPen.Brush.GetType() IsNot GetType(SysMedia.SolidColorBrush) OrElse CType(MediaPen.Brush, SysMedia.SolidColorBrush).Color = SysMedia.Colors.Transparent Then Throw New ApplicationException("DrawWPFPath: System media pen must be SolidColorBrush")
        Stroking = MediaPen
        Return
    End Sub

    ''' <summary>
    ''' Pen width
    ''' </summary>
    ''' <param name="PenWidth">Pen width in user coordinates</param>
    Public Sub SetPenWidth(ByVal PenWidth As Double)
        Me.PenWidth = PenWidth
        Return
    End Sub

    Friend Sub Draw(ByVal Contents As PdfContents, ByVal DrawRectX As Double, ByVal DrawRectY As Double, ByVal DrawRectWidth As Double, ByVal DrawRectHeight As Double, ByVal Optional Alignment As ContentAlignment = 0)
        ' save drawing rectangle in user coordinates
        Me.DrawRectX = DrawRectX
        Me.DrawRectY = DrawRectY
        Me.DrawRectWidth = DrawRectWidth
        Me.DrawRectHeight = DrawRectHeight

        ' test arguments
        If DrawRectWidth = 0 AndAlso DrawRectHeight = 0 OrElse DrawRectWidth = 0 AndAlso PathBBoxWidth <> 0 OrElse DrawRectHeight = 0 AndAlso PathBBoxHeight <> 0 Then Throw New ApplicationException("DrawWPFPath: Drawing rectangle is empty")

        ' set transformation matrix
        SetTransformation(Alignment)

        ' clip
        If Stroking Is Nothing AndAlso NonStroking Is Nothing Then
            ' build clipping path 
            BuildPath(Contents, If(FillRule = FillRule.EvenOdd, PaintOp.ClipPathEor, PaintOp.ClipPathWnr))
            Return
        End If

        ' paint operator
        Dim PaintOperator As PaintOp

        ' brush is defined as shading
        If NonStroking IsNot Nothing AndAlso (NonStroking.GetType() Is GetType(SysMedia.LinearGradientBrush) OrElse NonStroking.GetType() Is GetType(SysMedia.RadialGradientBrush) OrElse NonStroking.GetType() Is GetType(PdfAxialShading) OrElse NonStroking.GetType() Is GetType(PdfRadialShading)) Then
            ' save graphics state
            Contents.SaveGraphicsState()

            ' build clipping path 
            BuildPath(Contents, If(FillRule = FillRule.EvenOdd, PaintOp.ClipPathEor, PaintOp.ClipPathWnr))

            ' set bland mode
            If BlendMode <> BlendMode.Normal Then Contents.SetBlendMode(BlendMode)

            ' set opacity
            Contents.SetAlphaNonStroking(BrushOpacity)

            ' draw linera gradient brush shading bounded by clip path
            If NonStroking.GetType() Is GetType(SysMedia.LinearGradientBrush) Then
                Dim AxialShading As PdfAxialShading = New PdfAxialShading(Contents.Document, CType(NonStroking, SysMedia.LinearGradientBrush))
                AxialShading.SetBoundingBox(DrawRectX, DrawRectY, DrawRectWidth, DrawRectHeight)
                Contents.DrawShading(AxialShading)

                ' draw axial shading bounded by clip path
            ElseIf NonStroking.GetType() Is GetType(PdfAxialShading) Then
                CType(NonStroking, PdfAxialShading).SetBoundingBox(DrawRectX, DrawRectY, DrawRectWidth, DrawRectHeight)
                Contents.DrawShading(CType(NonStroking, PdfAxialShading))

                ' draw radial gradient brush shading bounded by clip path
            ElseIf NonStroking.GetType() Is GetType(SysMedia.RadialGradientBrush) Then
                Dim RadialShading As PdfRadialShading = New PdfRadialShading(Contents.Document, CType(NonStroking, SysMedia.RadialGradientBrush))
                RadialShading.SetBoundingBox(DrawRectX, DrawRectY, DrawRectWidth, DrawRectHeight)

                ' draw radial shading bounded by clip path
                Contents.DrawShading(RadialShading)
            Else
                CType(NonStroking, PdfRadialShading).SetBoundingBox(DrawRectX, DrawRectY, DrawRectWidth, DrawRectHeight)
                Contents.DrawShading(CType(NonStroking, PdfRadialShading))
            End If

            ' remove clipping path
            Contents.RestoreGraphicsState()

            ' no pen defined
            If Stroking Is Nothing Then Return

            ' pen is defined

            ' set paint operator for all other cases (no shading)
            PaintOperator = PaintOp.Stroke
        Else
            ' we have pen and no brush 
            If NonStroking Is Nothing Then
                PaintOperator = PaintOp.Stroke
                ' we have brush but no pen
            ElseIf Stroking Is Nothing Then
                ' we have brush and pen
                PaintOperator = If(FillRule = FillRule.EvenOdd, PaintOp.FillEor, PaintOp.Fill)
            Else
                PaintOperator = If(FillRule = FillRule.EvenOdd, PaintOp.CloseFillStrokeEor, PaintOp.CloseFillStroke)
            End If
        End If

        ' save graphics state
        Contents.SaveGraphicsState()

        ' set bland mode
        If BlendMode <> BlendMode.Normal Then Contents.SetBlendMode(BlendMode)

        ' stroking (pen) is defined
        If Stroking IsNot Nothing Then
            If Stroking.GetType() Is GetType(Color) Then
                ' pen color
                Contents.SetColorStroking(Stroking)

                ' set opacity
                If PenOpacity <> 1.0 Then Contents.SetAlphaStroking(PenOpacity)

                ' pen width
                If PenWidth >= 0 Then Contents.SetLineWidth(PenWidth)

                ' line cap
                If LineCap <> CType(-1, PdfLineCap) Then Contents.SetLineCap(LineCap)

                ' line join
                If LineJoin <> CType(-1, PdfLineJoin) Then Contents.SetLineJoin(LineJoin)

                ' Miter
                If MiterLimit <> -1 Then Contents.SetMiterLimit(MiterLimit)

                ' line is made of dashes
                If DashArray IsNot Nothing Then Contents.SetDashLine(DashArray, DashPhase)
            ElseIf Stroking.GetType() Is GetType(SysMedia.Pen) Then
                ' media pen short cut
                Dim Pen = CType(Stroking, SysMedia.Pen)

                ' media brush shortcut
                Dim Brush = CType(Pen.Brush, SysMedia.SolidColorBrush)

                ' media pen color short cut
                Dim PenColor = Brush.Color

                ' pen color
                Contents.SetColorStroking(Color.FromArgb(PenColor.R, PenColor.G, PenColor.B))

                ' pen opacity
                If PenColor.A <> 255 OrElse Brush.Opacity <> 1.0 Then Contents.SetAlphaStroking(PenColor.A / 255.0 * Brush.Opacity)

                ' pen thickness converted to user units
                Dim Thickness = Pen.Thickness * stdNum.Max(stdNum.Abs(ScaleX), stdNum.Abs(ScaleY))
                Contents.SetLineWidth(Thickness)

                ' line cap
                ' Note: PDF line cap is the same for start and end. We will ignore EndLineCap
                ' Triangle line cap will be round
                Select Case Pen.StartLineCap
                    Case SysMedia.PenLineCap.Flat
                        Contents.SetLineCap(PdfLineCap.Butt)
                    Case SysMedia.PenLineCap.Square
                        Contents.SetLineCap(PdfLineCap.Square)
                    Case Else
                        Contents.SetLineCap(PdfLineCap.Round)
                End Select

                ' line join
                Select Case Pen.LineJoin
                    Case SysMedia.PenLineJoin.Bevel
                        Contents.SetLineJoin(PdfLineJoin.Bevel)
                    Case SysMedia.PenLineJoin.Miter
                        Contents.SetLineJoin(PdfLineJoin.Miter)
                    Case Else
                        Contents.SetLineJoin(PdfLineJoin.Round)
                End Select

                ' Miter
                Contents.SetMiterLimit(Pen.MiterLimit)

                ' dash pattern
                If Pen.DashStyle.Dashes.Count > 0 Then
                    Dim [End] = Pen.DashStyle.Dashes.Count
                    Dim PenDashArray = New Double([End] - 1) {}

                    For Index = 0 To [End] - 1
                        PenDashArray(Index) = Thickness * Pen.DashStyle.Dashes(Index)
                    Next

                    Contents.SetDashLine(PenDashArray, Thickness * Pen.DashStyle.Offset)
                End If
            End If
        End If

        ' non-stroking (brush) is defined
        ' note shading brush was handled above
        If NonStroking IsNot Nothing Then
            ' set opacity
            If BrushOpacity <> 1.0 Then Contents.SetAlphaNonStroking(BrushOpacity)

            ' brush color
            If NonStroking.GetType() Is GetType(Color) Then
                Contents.SetColorNonStroking(NonStroking)
            ElseIf NonStroking.GetType() Is GetType(PdfTilingPattern) Then
                Contents.SetPatternNonStroking(CType(NonStroking, PdfTilingPattern))
            End If
        End If

        ' build path
        BuildPath(Contents, PaintOperator)

        ' restore graphics state
        Contents.RestoreGraphicsState()
        Return
    End Sub

    Private Sub BuildPath(ByVal Contents As PdfContents, ByVal PaintOperator As PaintOp)
        ' every figure is a separated subpath and contains some segments
        For Each SubPath In MediaPath.Figures
            ' get start of sub-path point
            Dim CurPoint = PathToDrawing(SubPath.StartPoint)
            Dim StartPoint = CurPoint
            Contents.MoveTo(CurPoint)

            ' process all points of one sub-path
            For Each Seg In SubPath.Segments
                ' line segment
                If Seg.GetType() Is GetType(SysMedia.LineSegment) Then
                    CurPoint = PathToDrawing(CType(Seg, SysMedia.LineSegment).Point)
                    Contents.LineTo(CurPoint)

                    ' polygon
                ElseIf Seg.GetType() Is GetType(SysMedia.PolyLineSegment) Then
                    Dim LineSegArray = CType(Seg, SysMedia.PolyLineSegment)

                    For Each PolyPoint In LineSegArray.Points
                        CurPoint = PathToDrawing(PolyPoint)
                        Contents.LineTo(CurPoint)
                    Next

                    ' cubic bezier segment
                ElseIf Seg.GetType() Is GetType(SysMedia.BezierSegment) Then
                    Dim BezierSeg = CType(Seg, SysMedia.BezierSegment)
                    CurPoint = PathToDrawing(BezierSeg.Point3)
                    Contents.DrawBezier(PathToDrawing(BezierSeg.Point1), PathToDrawing(BezierSeg.Point2), CurPoint)

                    ' cubic bezier multi segments
                ElseIf Seg.GetType() Is GetType(SysMedia.PolyBezierSegment) Then
                    Dim BezierSegArray = CType(Seg, SysMedia.PolyBezierSegment)
                    Dim Count = BezierSegArray.Points.Count

                    For Index = 0 To Count - 1 Step 3
                        CurPoint = PathToDrawing(BezierSegArray.Points(Index + 2))
                        Contents.DrawBezier(PathToDrawing(BezierSegArray.Points(Index)), PathToDrawing(BezierSegArray.Points(Index + 1)), CurPoint)
                    Next

                    ' quadratic bezier segment
                ElseIf Seg.GetType() Is GetType(SysMedia.QuadraticBezierSegment) Then
                    Dim BezierSeg = CType(Seg, SysMedia.QuadraticBezierSegment)
                    Dim NextPoint = PathToDrawing(BezierSeg.Point2)
                    Contents.DrawBezier(New BezierD(CurPoint, PathToDrawing(BezierSeg.Point1), NextPoint), BezierPointOne.Ignore)
                    CurPoint = NextPoint

                    ' quadratic bezier multi segments
                ElseIf Seg.GetType() Is GetType(SysMedia.PolyQuadraticBezierSegment) Then
                    Dim BezierSegArray = CType(Seg, SysMedia.PolyQuadraticBezierSegment)
                    Dim Count = BezierSegArray.Points.Count

                    For Index = 0 To Count - 1 Step 2
                        Dim NextPoint = PathToDrawing(BezierSegArray.Points(Index + 1))
                        Contents.DrawBezier(New BezierD(CurPoint, PathToDrawing(BezierSegArray.Points(Index)), NextPoint), BezierPointOne.Ignore)
                        CurPoint = NextPoint
                    Next

                    ' draw arc
                ElseIf Seg.GetType() Is GetType(SysMedia.ArcSegment) Then
                    Dim Arc = CType(Seg, SysMedia.ArcSegment)
                    Dim NextPoint = PathToDrawing(Arc.Point)
                    Dim ArcType As ArcType

                    If Arc.SweepDirection = If(PathYAxis = YAxisDirection.Down, SysMedia.SweepDirection.Counterclockwise, SysMedia.SweepDirection.Clockwise) Then
                        ArcType = If(Arc.IsLargeArc, ArcType.LargeCounterClockWise, ArcType.SmallCounterClockWise)
                    Else
                        ArcType = If(Arc.IsLargeArc, ArcType.LargeClockWise, ArcType.SmallClockWise)
                    End If

                    Contents.DrawArc(CurPoint, NextPoint, SizeToDrawing(Arc.Size), Arc.RotationAngle, ArcType, BezierPointOne.Ignore)

                    ' should no happen
                    CurPoint = NextPoint
                Else
                    Throw New ApplicationException("Windows Media path: unknown path segment.")
                End If
            Next

            ' for stroke set paint operator for each sub-path
            If SubPath.IsClosed Then Contents.SetPaintOp(PaintOp.CloseSubPath)
        Next

        ' paint operator
        Contents.SetPaintOp(PaintOperator)
        Return
    End Sub

    ' transformation coefficients from path to drawing
    Friend Sub SetTransformation(ByVal Alignment As ContentAlignment)
        ' preserve aspect ratio
        If Alignment <> 0 Then SetAspectRatio(Alignment)

        ' calculate transformation for x axis
        ScaleX = DrawRectWidth / PathBBoxWidth
        If Double.IsNaN(ScaleX) OrElse Double.IsInfinity(ScaleX) Then ScaleX = 0
        TransX = DrawRectX - PathBBoxX * ScaleX

        ' calculate transformation for y axis in down direction
        ScaleY = DrawRectHeight / PathBBoxHeight
        If Double.IsNaN(ScaleY) OrElse Double.IsInfinity(ScaleY) Then ScaleY = 0

        If PathYAxis = YAxisDirection.Down Then
            ScaleY = -ScaleY
            ' calculate transformation for y axis in up direction
            TransY = DrawRectY - ScaleY * (PathBBoxY + PathBBoxHeight)
        Else
            TransY = DrawRectY - PathBBoxY * ScaleY
        End If

        Return
    End Sub

    ' preserve aspect ratio
    Friend Sub SetAspectRatio(ByVal Alignment As ContentAlignment)
        ' calculate height to fit aspect ratio
        Dim RatioHeight = DrawRectWidth * PathBBoxHeight / PathBBoxWidth
        If RatioHeight = DrawRectHeight Then Return

        If Not Double.IsNaN(RatioHeight) AndAlso Not Double.IsInfinity(RatioHeight) AndAlso RatioHeight < DrawRectHeight Then
            If Alignment = ContentAlignment.MiddleLeft OrElse Alignment = ContentAlignment.MiddleCenter OrElse Alignment = ContentAlignment.MiddleRight Then
                DrawRectY += 0.5 * (DrawRectHeight - RatioHeight)
            ElseIf Alignment = ContentAlignment.TopLeft OrElse Alignment = ContentAlignment.TopCenter OrElse Alignment = ContentAlignment.TopRight Then
                DrawRectY += DrawRectHeight - RatioHeight
            End If

            ' calculate width to fit aspect ratio
            DrawRectHeight = RatioHeight
        Else
            Dim RatioWidth = DrawRectHeight * PathBBoxWidth / PathBBoxHeight

            If Not Double.IsNaN(RatioWidth) AndAlso Not Double.IsInfinity(RatioWidth) AndAlso RatioWidth < DrawRectWidth Then
                If Alignment = ContentAlignment.TopCenter OrElse Alignment = ContentAlignment.MiddleCenter OrElse Alignment = ContentAlignment.BottomCenter Then
                    DrawRectX += 0.5 * (DrawRectWidth - RatioWidth)
                ElseIf Alignment = ContentAlignment.TopRight OrElse Alignment = ContentAlignment.MiddleRight OrElse Alignment = ContentAlignment.BottomRight Then
                    DrawRectX += DrawRectWidth - RatioWidth
                End If
            End If

            DrawRectWidth = RatioWidth
        End If

        Return
    End Sub

    ' Transform path point to drawing point
    Friend Function PathToDrawing(ByVal PathPoint As SysWin.Point) As PointD
        Return New PointD(ScaleX * PathPoint.X + TransX, ScaleY * PathPoint.Y + TransY)
    End Function

    Friend Function SizeToDrawing(ByVal PathSize As SysWin.Size) As SizeD
        Return New SizeD(stdNum.Abs(ScaleX) * PathSize.Width, stdNum.Abs(ScaleY) * PathSize.Height)
    End Function
End Class
