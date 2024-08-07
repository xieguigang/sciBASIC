#Region "Microsoft.VisualBasic::5d1d27ac04a00170a6b61231cd7c9736, mime\application%pdf\PdfFileWriter\PDF\PdfContents.vb"

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

    '   Total Lines: 2588
    '    Code Lines: 936 (36.17%)
    ' Comment Lines: 1264 (48.84%)
    '    - Xml Docs: 81.09%
    ' 
    '   Blank Lines: 388 (14.99%)
    '     File Size: 102.11 KB


    ' Class PdfContents
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: CreateFontResStr, (+3 Overloads) DrawBarcode, (+13 Overloads) DrawText, DrawTextInternal, (+2 Overloads) DrawTextWithAnnotation
    '               (+2 Overloads) DrawTextWithKerning, (+2 Overloads) DrawWebLink, PaintOpStr, ReverseString, TextFitToWidth
    ' 
    '     Sub: AddToUsedResources, BeginTextMode, ClipText, CommitToPdfFile, DrawArc
    '          DrawBarcodeText, (+3 Overloads) DrawBezier, (+2 Overloads) DrawBezierNoP1, (+2 Overloads) DrawBezierNoP2, DrawDoubleBezierPath
    '          (+2 Overloads) DrawHeart, (+2 Overloads) DrawImage, DrawInwardCornerRectangle, (+7 Overloads) DrawLine, (+2 Overloads) DrawOval
    '          (+2 Overloads) DrawPolygon, (+2 Overloads) DrawRectangle, (+2 Overloads) DrawRegularPolygon, (+2 Overloads) DrawRoundedRectangle, (+2 Overloads) DrawShading
    '          (+4 Overloads) DrawStar, (+4 Overloads) DrawXObject, EndTextMode, GrayLevelNonStroking, GrayLevelStroking
    '          LayerEnd, LayerStart, (+2 Overloads) LineTo, (+2 Overloads) MoveTo, OutputOneByte
    '          RestoreGraphicsState, SaveGraphicsState, Scale, (+2 Overloads) SetAlphaNonStroking, (+2 Overloads) SetAlphaStroking
    '          SetBlendMode, SetCharacterSpacing, SetColorNonStroking, SetColorStroking, SetDashLine
    '          SetLineCap, SetLineJoin, SetLineWidth, SetMiterLimit, (+2 Overloads) SetPaintOp
    '          SetPatternNonStroking, SetPatternStroking, SetTextPosition, SetTextRenderingMode, SetTransMatrix
    '          SetWordSpacing, (+2 Overloads) Translate, (+4 Overloads) TranslateScale, (+3 Overloads) TranslateScaleRotate, WriteObjectToPdfFile
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfContents
'	PDF contents indirect object. Support for page contents,
'  X Objects and Tilling Patterns.
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
'

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text
Imports i32 = Microsoft.VisualBasic.Language.i32
Imports std = System.Math

''' <summary>
''' PDF contents class
''' </summary>
''' <remarks>
''' <para>
''' For more information go to 
''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#CoordinateSystem">
''' 2.1 Coordinate System
''' </a>
''' </para>
''' </remarks>
Public Class PdfContents
    Inherits PdfObject

    ''' <summary>
    ''' true for page contents, false for X objects and pattern
    ''' </summary>
    Friend PageContents As Boolean
    Friend ResObjects As List(Of PdfObject)

    Private Shared ReadOnly PaintStr As String() = New String() {"", "n", "S", "s", "f", "f*", "B", "B*", "b", "b*", "h W n", "h W* n", "h"}

    ''' <summary>
    ''' PdfContents constructor for page contents
    ''' </summary>
    ''' <param name="Page">Page parent</param>
    Public Sub New(Page As PdfPage)
        MyBase.New(Page.Document, ObjectType.Stream)
        ' set page contents flag
        PageContents = True

        ' add contents to page's list of contents
        Page.AddContents(Me)
    End Sub

    ''' <summary>
    ''' PdfContents constructor unattached
    ''' </summary>
    ''' <param name="Document">Current PdfDocument</param>
    ''' <remarks>
    ''' This contents object must be explicitly attached to a page object
    ''' </remarks>
    Public Sub New(Document As PdfDocument)
        MyBase.New(Document, ObjectType.Stream)
    End Sub

    ''' <summary>
    ''' Constructor for XObject or Pattern
    ''' </summary>
    ''' <param name="Document"></param>
    ''' <param name="PdfObjectType"></param>
    Friend Sub New(Document As PdfDocument, PdfObjectType As String)
        MyBase.New(Document, ObjectType.Stream, PdfObjectType)
    End Sub

    ''' <summary>
    ''' Save graphics state
    ''' </summary>
    Public Sub SaveGraphicsState()
        MyBase.ObjectValueAppend("q" & Microsoft.VisualBasic.Constants.vbLf)
    End Sub


    ''' <summary>
    ''' Restore graphics state
    ''' </summary>

    Public Sub RestoreGraphicsState()
        MyBase.ObjectValueAppend("Q" & Microsoft.VisualBasic.Constants.vbLf)
    End Sub

    ''' <summary>
    ''' Layer start
    ''' </summary>
    ''' <param name="Layer">Layer object</param>
    Public Sub LayerStart(Layer As PdfLayer)
        ' add to list of resources
        AddToUsedResources(Layer)

        ' write to content stream
        MyBase.ObjectValueFormat("/OC {0} BDC" & Microsoft.VisualBasic.Constants.vbLf, Layer.ResourceCode)
    End Sub

    ''' <summary>
    ''' Layer end
    ''' </summary>
    Public Sub LayerEnd()
        ' write to content stream
        MyBase.ObjectValueFormat("EMC" & Microsoft.VisualBasic.Constants.vbLf)
    End Sub


    ''' <summary>
    ''' Convert PaintOp enumeration to String
    ''' </summary>
    ''' <param name="PP">Paint operator</param>
    ''' <returns>Paint operator string</returns>
    Public Function PaintOpStr(PP As PaintOp) As String
        ' apply paint operator
        Return PaintStr(PP)
    End Function


    ''' <summary>
    ''' Set paint operator
    ''' </summary>
    ''' <param name="PP">Paint operator</param>
    Public Sub SetPaintOp(PP As PaintOp)
        ' apply paint operator
        If PP <> PaintOp.NoOperator Then
            MyBase.ObjectValueFormat("{0}" & Microsoft.VisualBasic.Constants.vbLf, PaintStr(CInt(PP)))
        End If
    End Sub

    ''' <summary>
    ''' Set line width
    ''' </summary>
    ''' <param name="Width">Line width</param>
    ''' <remarks>
    ''' Set line width for future path operations
    ''' </remarks>
    Public Sub SetLineWidth(Width As Double)
        MyBase.ObjectValueFormat("{0} w" & Microsoft.VisualBasic.Constants.vbLf, ToPt(Width))
    End Sub

    ''' <summary>
    ''' Set line cap
    ''' </summary>
    ''' <param name="LineCap">Line cap enumeration</param>
    Public Sub SetLineCap(LineCap As PdfLineCap)
        MyBase.ObjectValueFormat("{0} J" & Microsoft.VisualBasic.Constants.vbLf, CInt(LineCap))
    End Sub

    ''' <summary>
    ''' Set line join
    ''' </summary>
    ''' <param name="LineJoin">Set line join enumeration</param>
    Public Sub SetLineJoin(LineJoin As PdfLineJoin)
        MyBase.ObjectValueFormat("{0} j" & Microsoft.VisualBasic.Constants.vbLf, CInt(LineJoin))
    End Sub

    ''' <summary>
    ''' Set miter limit
    ''' </summary>
    ''' <param name="MiterLimit">Miter limit</param>
    Public Sub SetMiterLimit(MiterLimit As Double)        ' default 10.0
        MyBase.ObjectValueFormat("{0} M" & Microsoft.VisualBasic.Constants.vbLf, Round(MiterLimit))
    End Sub

    ''' <summary>
    ''' Set dash line pattern
    ''' </summary>
    ''' <param name="DashArray">Dash array</param>
    ''' <param name="DashPhase">Dash phase</param>
    Public Sub SetDashLine(DashArray As Double(), DashPhase As Double)      ' default []
        ' default 0
        ' restore default condition of solid line
        If DashArray Is Nothing OrElse DashArray.Length = 0 Then
            ' dash line
            MyBase.ObjectValueAppend("[] 0 d" & Microsoft.VisualBasic.Constants.vbLf)
        Else
            ObjectValueList.Add(Microsoft.VisualBasic.AscW("["c))

            For Each Value In DashArray
                ObjectValueFormat("{0} ", ToPt(Value))
            Next

            MyBase.ObjectValueFormat("] {0} d" & Microsoft.VisualBasic.Constants.vbLf, ToPt(DashPhase))
        End If
    End Sub

    ''' <summary>
    ''' Set gray level for non stroking (fill or brush) operations
    ''' </summary>
    ''' <param name="GrayLevel">Gray level (0.0 to 1.0)</param>
    ''' <remarks>
    ''' Gray level must be 0.0 (black) to 1.0 (white).
    ''' </remarks>
    Public Sub GrayLevelNonStroking(GrayLevel As Double)
        MyBase.ObjectValueFormat("{0} g" & Microsoft.VisualBasic.Constants.vbLf, Round(GrayLevel))
    End Sub

    ''' <summary>
    ''' Set gray level for stroking (outline or pen) operations
    ''' </summary>
    ''' <param name="GrayLevel">Gray level (0.0 to 1.0)</param>
    ''' <remarks>
    ''' Gray level must be 0.0 (black) to 1.0 (white).
    ''' </remarks>

    Public Sub GrayLevelStroking(GrayLevel As Double)
        MyBase.ObjectValueFormat("{0} G" & Microsoft.VisualBasic.Constants.vbLf, Round(GrayLevel))
    End Sub

    ''' <summary>
    ''' Set color for non stroking (fill or brush) operations
    ''' </summary>
    ''' <param name="Color">Color</param>
    ''' <remarks>Set red, green and blue components. Alpha is ignored</remarks>
    Public Sub SetColorNonStroking(Color As Color)
        MyBase.ObjectValueFormat("{0} {1} {2} rg" & Microsoft.VisualBasic.Constants.vbLf, Round(Color.R / 255.0), Round(Color.G / 255.0), Round(Color.B / 255.0))
    End Sub

    ''' <summary>
    ''' Set color for stroking (outline or pen) operations
    ''' </summary>
    ''' <param name="Color">Color</param>
    ''' <remarks>Set red, green and blue components. Alpha is ignored</remarks>
    Public Sub SetColorStroking(Color As Color)
        MyBase.ObjectValueFormat("{0} {1} {2} RG" & Microsoft.VisualBasic.Constants.vbLf, Round(Color.R / 255.0), Round(Color.G / 255.0), Round(Color.B / 255.0))
    End Sub

    ''' <summary>
    ''' Set opacity value (alpha) of color of for stroking operations
    ''' </summary>
    ''' <param name="Color">Color value</param>
    ''' <remarks>Set alpha component. Ignore red, green and blue.</remarks>
    Public Sub SetAlphaStroking(Color As Color)
        SetAlphaStroking(Color.A / 255.0)
    End Sub

    ''' <summary>
    ''' Set opacity value for stroking operations
    ''' </summary>
    ''' <param name="Alpha">Opacity value 0.0=transparent to 1.0=Opaque</param>
    Public Sub SetAlphaStroking(Alpha As Double)
        Dim AlphaStr As String

        If Alpha < 0.001 Then
            AlphaStr = "0"
        ElseIf Alpha > 0.999 Then
            AlphaStr = "1"
        Else
            AlphaStr = Alpha.ToString("0.0##", PeriodDecSep)
        End If

        Dim ExtGState = PdfExtGState.CreateExtGState(Document, "/CA", AlphaStr)
        AddToUsedResources(ExtGState)
        MyBase.ObjectValueFormat("{0} gs" & Microsoft.VisualBasic.Constants.vbLf, ExtGState.ResourceCode)
    End Sub

    ''' <summary>
    ''' Set opacity value (alpha) of color of for non-stroking operations
    ''' </summary>
    ''' <param name="Color">Color value</param>
    ''' <remarks>Set alpha component. Ignore red, green and blue.</remarks>
    Public Sub SetAlphaNonStroking(Color As Color)
        SetAlphaNonStroking(Color.A / 255.0)
    End Sub

    ''' <summary>
    ''' Set opacity value for non-stroking operations
    ''' </summary>
    ''' <param name="Alpha">Opacity value 0.0=transparent to 1.0=Opaque</param>
    Public Sub SetAlphaNonStroking(Alpha As Double)
        Dim AlphaStr As String

        If Alpha < 0.001 Then
            AlphaStr = "0"
        ElseIf Alpha > 0.999 Then
            AlphaStr = "1"
        Else
            AlphaStr = Alpha.ToString("0.0##", PeriodDecSep)
        End If

        Dim ExtGState = PdfExtGState.CreateExtGState(Document, "/ca", AlphaStr)
        AddToUsedResources(ExtGState)
        MyBase.ObjectValueFormat("{0} gs" & Microsoft.VisualBasic.Constants.vbLf, ExtGState.ResourceCode)
    End Sub

    ''' <summary>
    ''' Set color blend mode
    ''' </summary>
    ''' <param name="Blend">Blend method enumeration</param>
    Public Sub SetBlendMode(Blend As BlendMode)
        Dim ExtGState As PdfExtGState = PdfExtGState.CreateExtGState(Document, "/BM", "/" & Blend.ToString())
        AddToUsedResources(ExtGState)
        MyBase.ObjectValueFormat("{0} gs" & Microsoft.VisualBasic.Constants.vbLf, ExtGState.ResourceCode)
    End Sub

    ''' <summary>
    ''' Set pattern for non stroking (fill) operations
    ''' </summary>
    ''' <param name="Pattern">Pattern resource</param>
    Public Sub SetPatternNonStroking(Pattern As PdfTilingPattern)
        AddToUsedResources(Pattern)
        MyBase.ObjectValueFormat("/Pattern cs {0} scn" & Microsoft.VisualBasic.Constants.vbLf, Pattern.ResourceCode)
    End Sub

    ''' <summary>
    ''' Set pattern for stroking (outline) operations
    ''' </summary>
    ''' <param name="Pattern">Pattern resource</param>
    Public Sub SetPatternStroking(Pattern As PdfContents)
        AddToUsedResources(Pattern)
        MyBase.ObjectValueFormat("/Pattern CS {0} SCN" & Microsoft.VisualBasic.Constants.vbLf, Pattern.ResourceCode)
    End Sub

    ''' <summary>
    ''' Draw axial shading pattern
    ''' </summary>
    ''' <param name="Shading">Axial shading resource</param>
    Public Sub DrawShading(Shading As PdfAxialShading)
        AddToUsedResources(Shading)
        MyBase.ObjectValueFormat("{0} sh" & Microsoft.VisualBasic.Constants.vbLf, Shading.ResourceCode)
    End Sub

    ''' <summary>
    ''' Draw radial shading pattern
    ''' </summary>
    ''' <param name="Shading">Radial shading resource</param>
    Public Sub DrawShading(Shading As PdfRadialShading)
        AddToUsedResources(Shading)
        MyBase.ObjectValueFormat("{0} sh" & Microsoft.VisualBasic.Constants.vbLf, Shading.ResourceCode)
    End Sub

    ''' <summary>
    ''' Set current transformation matrix
    ''' </summary>
    ''' <param name="a">A</param>
    ''' <param name="b">B</param>
    ''' <param name="c">C</param>
    ''' <param name="d">D</param>
    ''' <param name="e">E</param>
    ''' <param name="f">F</param>
    ''' <remarks>
    ''' Xpage = a * Xuser + c * Yuser + e
    ''' Ypage = b * Xuser + d * Yuser + f
    ''' </remarks>	
    Public Sub SetTransMatrix(a As Double, b As Double, c As Double, d As Double, e As Double, f As Double) ' ScaleX * Cos(Rotate)
        ' ScaleX * Sin(Rotate)
        ' ScaleY * (-Sin(Rotate))
        ' ScaleY * Cos(Rotate)
        MyBase.ObjectValueFormat("{0} {1} {2} {3} {4} {5} cm" & Microsoft.VisualBasic.Constants.vbLf, Round(a), Round(b), Round(c), Round(d), ToPt(e), ToPt(f))
    End Sub

    ''' <summary>
    ''' Translate origin
    ''' </summary>
    ''' <param name="Orig">New origin</param>
    Public Sub Translate(Orig As PointD)
        Translate(Orig.X, Orig.Y)
    End Sub

    ''' <summary>
    ''' Translate origin
    ''' </summary>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    Public Sub Translate(OriginX As Double, OriginY As Double)
        MyBase.ObjectValueFormat("1 0 0 1 {0} {1} cm" & Microsoft.VisualBasic.Constants.vbLf, ToPt(OriginX), ToPt(OriginY))
    End Sub

    ''' <summary>
    ''' Scale
    ''' </summary>
    ''' <param name="pScale">New scale</param>
    Public Sub Scale(pScale As Double)
        MyBase.ObjectValueFormat("{0} 0 0 {0} 0 0 cm" & Microsoft.VisualBasic.Constants.vbLf, Round(pScale))
    End Sub

    ''' <summary>
    ''' Translate and scale
    ''' </summary>
    ''' <param name="Orig">Origin point</param>
    ''' <param name="Scale">Scale</param>
    Public Sub TranslateScale(Orig As PointD, Scale As Double)
        TranslateScale(Orig.X, Orig.Y, Scale)
    End Sub

    ''' <summary>
    ''' Translate and scale
    ''' </summary>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="Scale">Scale</param>
    Public Sub TranslateScale(OriginX As Double, OriginY As Double, Scale As Double)
        MyBase.ObjectValueFormat("{2} 0 0 {2} {0} {1} cm" & Microsoft.VisualBasic.Constants.vbLf, ToPt(OriginX), ToPt(OriginY), Round(Scale))
    End Sub

    ''' <summary>
    ''' Translate and scale
    ''' </summary>
    ''' <param name="Orig">Origin point</param>
    ''' <param name="ScaleX">Horizontal scale</param>
    ''' <param name="ScaleY">Vertical scale</param>
    Public Sub TranslateScale(Orig As PointD, ScaleX As Double, ScaleY As Double)
        TranslateScale(Orig.X, Orig.Y, ScaleX, ScaleY)
    End Sub

    ''' <summary>
    ''' Translate and scale
    ''' </summary>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="ScaleX">Horizontal scale</param>
    ''' <param name="ScaleY">Vertical scale</param>
    Public Sub TranslateScale(OriginX As Double, OriginY As Double, ScaleX As Double, ScaleY As Double)
        MyBase.ObjectValueFormat("{2} 0 0 {3} {0} {1} cm" & Microsoft.VisualBasic.Constants.vbLf, ToPt(OriginX), ToPt(OriginY), Round(ScaleX), Round(ScaleY))
    End Sub

    ''' <summary>
    ''' Translate, scale and rotate
    ''' </summary>
    ''' <param name="Orig">Origin point</param>
    ''' <param name="Scale">Scale</param>
    ''' <param name="Rotate">Rotate (radians)</param>
    Public Sub TranslateScaleRotate(Orig As PointD, Scale As Double, Rotate As Double)        ' radians
        TranslateScaleRotate(Orig.X, Orig.Y, Scale, Rotate)
    End Sub

    ''' <summary>
    ''' Translate, scale and rotate
    ''' </summary>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="Scale">Scale</param>
    ''' <param name="Rotate">Rotate (radians)</param>
    Public Sub TranslateScaleRotate(OriginX As Double, OriginY As Double, Scale As Double, Rotate As Double)
        MyBase.ObjectValueFormat("{2} {3} {4} {2} {0} {1} cm" & Microsoft.VisualBasic.Constants.vbLf, ToPt(OriginX), ToPt(OriginY), Round(Scale * std.Cos(Rotate)), Round(Scale * std.Sin(Rotate)), Round(Scale * std.Sin(-Rotate)))
    End Sub

    ''' <summary>
    ''' Translate, scale and rotate
    ''' </summary>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="ScaleX">Horizontal scale</param>
    ''' <param name="ScaleY">Vertical scale</param>
    ''' <param name="Rotate">Rotate (radians)</param>
    Public Sub TranslateScaleRotate(OriginX As Double, OriginY As Double, ScaleX As Double, ScaleY As Double, Rotate As Double)
        MyBase.ObjectValueFormat("{2} {3} {4} {5} {0} {1} cm" & Microsoft.VisualBasic.Constants.vbLf, ToPt(OriginX), ToPt(OriginY), Round(ScaleX * std.Cos(Rotate)), Round(ScaleY * std.Sin(Rotate)), Round(ScaleX * std.Sin(-Rotate)), Round(ScaleY * std.Cos(Rotate)))
    End Sub

    ''' <summary>
    ''' Move current pointer to new position
    ''' </summary>
    ''' <param name="Point">New point</param>
    Public Sub MoveTo(Point As PointD)
        MoveTo(Point.X, Point.Y)
    End Sub

    ''' <summary>
    ''' Move current pointer to new position
    ''' </summary>
    ''' <param name="X">New X position</param>
    ''' <param name="Y">New Y position</param>
    Public Sub MoveTo(X As Double, Y As Double)
        MyBase.ObjectValueFormat("{0} {1} m" & Microsoft.VisualBasic.Constants.vbLf, ToPt(X), ToPt(Y))
    End Sub

    ''' <summary>
    ''' Draw line from last position to new position
    ''' </summary>
    ''' <param name="Point">New point</param>
    Public Sub LineTo(Point As PointD)
        LineTo(Point.X, Point.Y)
    End Sub

    ''' <summary>
    ''' Draw line from last position to new position
    ''' </summary>
    ''' <param name="X">New X position</param>
    ''' <param name="Y">New Y position</param>

    Public Sub LineTo(X As Double, Y As Double)
        MyBase.ObjectValueFormat("{0} {1} l" & Microsoft.VisualBasic.Constants.vbLf, ToPt(X), ToPt(Y))
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path
    ''' </summary>
    ''' <param name="Bezier">Bezier object</param>
    ''' <param name="Point1Action">Point1 action</param>
    Public Sub DrawBezier(Bezier As BezierD, Point1Action As BezierPointOne)
        Select Case Point1Action
            Case BezierPointOne.MoveTo
                MoveTo(Bezier.P1.X, Bezier.P1.Y)
            Case BezierPointOne.LineTo
                LineTo(Bezier.P1.X, Bezier.P1.Y)
        End Select

        DrawBezier(Bezier.P2.X, Bezier.P2.Y, Bezier.P3.X, Bezier.P3.Y, Bezier.P4.X, Bezier.P4.Y)
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path
    ''' </summary>
    ''' <param name="P1">Point 1</param>
    ''' <param name="P2">Point 2</param>
    ''' <param name="P3">Point 3</param>
    Public Sub DrawBezier(P1 As PointD, P2 As PointD, P3 As PointD)
        DrawBezier(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y)
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path
    ''' </summary>
    ''' <param name="X1">Point 1 X</param>
    ''' <param name="Y1">Point 1 Y</param>
    ''' <param name="X2">Point 2 X</param>
    ''' <param name="Y2">Point 2 Y</param>
    ''' <param name="X3">Point 3 X</param>
    ''' <param name="Y3">Point 3 Y</param>
    Public Sub DrawBezier(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double, X3 As Double, Y3 As Double)
        MyBase.ObjectValueFormat("{0} {1} {2} {3} {4} {5} c" & Microsoft.VisualBasic.Constants.vbLf, ToPt(X1), ToPt(Y1), ToPt(X2), ToPt(Y2), ToPt(X3), ToPt(Y3))
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path (P1 is the same as current point)
    ''' </summary>
    ''' <param name="P2">Point 2</param>
    ''' <param name="P3">Point 3</param>
    Public Sub DrawBezierNoP1(P2 As PointD, P3 As PointD)
        DrawBezierNoP1(P2.X, P2.Y, P3.X, P3.Y)
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path (P1 is the same as current point)
    ''' </summary>
    ''' <param name="X2">Point 2 X</param>
    ''' <param name="Y2">Point 2 Y</param>
    ''' <param name="X3">Point 3 X</param>
    ''' <param name="Y3">Point 3 Y</param>
    Public Sub DrawBezierNoP1(X2 As Double, Y2 As Double, X3 As Double, Y3 As Double)
        MyBase.ObjectValueFormat("{0} {1} {2} {3} v" & Microsoft.VisualBasic.Constants.vbLf, ToPt(X2), ToPt(Y2), ToPt(X3), ToPt(Y3))
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path (P2 is the same as P3)
    ''' </summary>
    ''' <param name="P1">Point 1</param>
    ''' <param name="P3">Point 3</param>
    Public Sub DrawBezierNoP2(P1 As PointD, P3 As PointD)
        DrawBezierNoP2(P1.X, P1.Y, P3.X, P3.Y)
    End Sub

    ''' <summary>
    ''' Draw Bezier cubic path (P2 is the same as P3)
    ''' </summary>
    ''' <param name="X1">Point 1 X</param>
    ''' <param name="Y1">Point 1 Y</param>
    ''' <param name="X3">Point 3 X</param>
    ''' <param name="Y3">Point 3 Y</param>
    Public Sub DrawBezierNoP2(X1 As Double, Y1 As Double, X3 As Double, Y3 As Double)
        MyBase.ObjectValueFormat("{0} {1} {2} {3} y" & Microsoft.VisualBasic.Constants.vbLf, ToPt(X1), ToPt(Y1), ToPt(X3), ToPt(Y3))
    End Sub

    ''' <summary>
    ''' Draw arc
    ''' </summary>
    ''' <param name="ArcStart">Arc start point</param>
    ''' <param name="ArcEnd">Arc end point</param>
    ''' <param name="Radius">RadiusX as width and RadiusY as height</param>
    ''' <param name="Rotate">X axis rotation angle in radians</param>
    ''' <param name="Type">Arc type enumeration</param>
    ''' <param name="OutputStartPoint">Output start point</param>
    Public Sub DrawArc(ArcStart As PointD, ArcEnd As PointD, Radius As SizeD, Rotate As Double, Type As ArcType, OutputStartPoint As BezierPointOne)
        ' starting point
        Select Case OutputStartPoint
            Case BezierPointOne.MoveTo
                MoveTo(ArcStart.X, ArcStart.Y)
            Case BezierPointOne.LineTo
                LineTo(ArcStart.X, ArcStart.Y)
        End Select

        ' create arc
        Dim SegArray = CreateArc(ArcStart, ArcEnd, Radius, Rotate, Type)

        ' output
        Dim Index As i32 = 1

        While Index < SegArray.Length
            DrawBezier(SegArray(Index).X, SegArray(++Index).Y, SegArray(Index).X, SegArray(++Index).Y, SegArray(Index).X, SegArray(++Index).Y)
        End While
    End Sub

    ''' <summary>
    ''' Draw line
    ''' </summary>
    ''' <param name="Line">Line object</param>
    Public Sub DrawLine(Line As LineD)
        DrawLine(Line.P1.X, Line.P1.Y, Line.P2.X, Line.P2.Y)
    End Sub

    ''' <summary>
    ''' Draw line
    ''' </summary>
    ''' <param name="P1">Point 1</param>
    ''' <param name="P2">Point 2</param>
    Public Sub DrawLine(P1 As PointD, P2 As PointD)
        DrawLine(P1.X, P1.Y, P2.X, P2.Y)
    End Sub

    ''' <summary>
    ''' Draw line
    ''' </summary>
    ''' <param name="X1">Point 1 X</param>
    ''' <param name="Y1">Point 1 Y</param>
    ''' <param name="X2">Point 2 X</param>
    ''' <param name="Y2">Point 2 X</param>
    Public Sub DrawLine(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double)
        MyBase.ObjectValueFormat("{0} {1} m {2} {3} l S" & Microsoft.VisualBasic.Constants.vbLf, ToPt(X1), ToPt(Y1), ToPt(X2), ToPt(Y2))
    End Sub

    ''' <summary>
    ''' Draw line with given line width
    ''' </summary>
    ''' <param name="Line">Line</param>
    ''' <param name="LineWidth">Line width</param>
    Public Sub DrawLine(Line As LineD, LineWidth As Double)
        DrawLine(Line.P1.X, Line.P1.Y, Line.P2.X, Line.P2.Y, LineWidth)
    End Sub

    ''' <summary>
    ''' Draw line with given line width
    ''' </summary>
    ''' <param name="P1">Point 1</param>
    ''' <param name="P2">Point 2</param>
    ''' <param name="LineWidth">Line width</param>
    Public Sub DrawLine(P1 As PointD, P2 As PointD, LineWidth As Double)
        DrawLine(P1.X, P1.Y, P2.X, P2.Y, LineWidth)
    End Sub

    ''' <summary>
    ''' Draw line with given line width
    ''' </summary>
    ''' <param name="X1">Point 1 X</param>
    ''' <param name="Y1">Point 1 Y</param>
    ''' <param name="X2">Point 2 X</param>
    ''' <param name="Y2">Point 2 X</param>
    ''' <param name="LineWidth">Line width</param>
    Public Sub DrawLine(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double, LineWidth As Double)
        MyBase.ObjectValueFormat("q {0} w {1} {2} m {3} {4} l S Q" & Microsoft.VisualBasic.Constants.vbLf, ToPt(LineWidth), ToPt(X1), ToPt(Y1), ToPt(X2), ToPt(Y2))
    End Sub

    ''' <summary>
    ''' Draw border line 
    ''' </summary>
    ''' <param name="X1">Point 1 X</param>
    ''' <param name="Y1">Point 1 Y</param>
    ''' <param name="X2">Point 2 X</param>
    ''' <param name="Y2">Point 2 X</param>
    ''' <param name="BorderStyle">PdfTableBorderStyle</param>
    Public Sub DrawLine(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double, BorderStyle As PdfTableBorderStyle)
        If BorderStyle.Display Then
            MyBase.ObjectValueFormat("q {0} w {1} {2} {3} RG 0 J {4} {5} m {6} {7} l S Q" & Microsoft.VisualBasic.Constants.vbLf, ToPt(BorderStyle.Width), Round(CDbl(BorderStyle.Color.R) / 255.0), Round(CDbl(BorderStyle.Color.G) / 255.0), Round(CDbl(BorderStyle.Color.B) / 255.0), ToPt(X1), ToPt(Y1), ToPt(X2), ToPt(Y2))
        End If
    End Sub

    ''' <summary>
    ''' Draw rectangle
    ''' </summary>
    ''' <param name="Origin">Origin (left-bottom)</param>
    ''' <param name="Size">Size</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawRectangle(Origin As PointD, Size As SizeD, PP As PaintOp)
        DrawRectangle(Origin.X, Origin.Y, Size.Width, Size.Height, PP)
    End Sub

    ''' <summary>
    ''' Draw Rectangle
    ''' </summary>
    ''' <param name="OriginX">Origin X (left)</param>
    ''' <param name="OriginY">Origin Y (bottom)</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawRectangle(OriginX As Double, OriginY As Double, Width As Double, Height As Double, PP As PaintOp)
        ' draw rectangle
        MyBase.ObjectValueFormat("{0} {1} {2} {3} re {4}" & Microsoft.VisualBasic.Constants.vbLf, ToPt(OriginX), ToPt(OriginY), ToPt(Width), ToPt(Height), PaintOpStr(PP))
    End Sub

    ''' <summary>
    ''' Draw oval
    ''' </summary>
    ''' <param name="Origin">Origin (left-bottom)</param>
    ''' <param name="Size">Size</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawOval(Origin As PointD, Size As SizeD, PP As PaintOp)
        DrawOval(Origin.X, Origin.Y, Size.Width, Size.Height, PP)
    End Sub

    ''' <summary>
    ''' Draw oval
    ''' </summary>
    ''' <param name="OriginX">Origin X (left)</param>
    ''' <param name="OriginY">Origin Y (bottom)</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawOval(OriginX As Double, OriginY As Double, Width As Double, Height As Double, PP As PaintOp)
        Width /= 2
        Height /= 2
        OriginX += Width
        OriginY += Height
        DrawBezier(BezierD.OvalFirstQuarter(OriginX, OriginY, Width, Height), BezierPointOne.MoveTo)
        DrawBezier(BezierD.OvalSecondQuarter(OriginX, OriginY, Width, Height), BezierPointOne.Ignore)
        DrawBezier(BezierD.OvalThirdQuarter(OriginX, OriginY, Width, Height), BezierPointOne.Ignore)
        DrawBezier(BezierD.OvalFourthQuarter(OriginX, OriginY, Width, Height), BezierPointOne.Ignore)
        SetPaintOp(PP)
    End Sub

    ''' <summary>
    ''' Draw heart
    ''' </summary>
    ''' <param name="CenterLine">Center line</param>
    ''' <param name="PP">Paint operator</param>
    ''' <remarks>
    ''' <para>
    ''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DrawHeart">For example of drawing heart see 3.10. Draw Heart</a>
    ''' </para>
    ''' </remarks>
    Public Sub DrawHeart(CenterLine As LineD, PP As PaintOp)
        ' PI / 1.5 = 120 deg and PI / 2 = 90 deg
        DrawDoubleBezierPath(CenterLine, 1.0, std.PI / 1.5, 1.0, 0.5 * std.PI, PP)
    End Sub

    ''' <summary>
    ''' Draw heart
    ''' </summary>
    ''' <param name="CenterLineTopX">Center line top X</param>
    ''' <param name="CenterLineTopY">Center line top Y</param>
    ''' <param name="CenterLineBottomX">Center line bottom X</param>
    ''' <param name="CenterLineBottomY">Center line bottom Y</param>
    ''' <param name="PP">Paint operator</param>
    ''' <remarks>
    ''' <para>
    ''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DrawHeart">
    ''' For example of drawing heart see 3.10. Draw Heart</a>
    ''' </para>
    ''' </remarks>
    Public Sub DrawHeart(CenterLineTopX As Double, CenterLineTopY As Double, CenterLineBottomX As Double, CenterLineBottomY As Double, PP As PaintOp)
        DrawHeart(New LineD(CenterLineTopX, CenterLineTopY, CenterLineBottomX, CenterLineBottomY), PP)
    End Sub

    ''' <summary>
    ''' Draw double Bezier path
    ''' </summary>
    ''' <param name="CenterLine">Center line</param>
    ''' <param name="Factor1">Factor 1</param>
    ''' <param name="Alpha1">Alpha 1</param>
    ''' <param name="Factor2">Factor 2</param>
    ''' <param name="Alpha2">Alpha 2</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawDoubleBezierPath(CenterLine As LineD, Factor1 As Double, Alpha1 As Double, Factor2 As Double, Alpha2 As Double, PP As PaintOp)
        ' two symmetric Bezier curves
        DrawBezier(New BezierD(CenterLine.P1, Factor1, -0.5 * Alpha1, Factor2, -0.5 * Alpha2, CenterLine.P2), BezierPointOne.MoveTo)
        DrawBezier(New BezierD(CenterLine.P2, Factor2, std.PI + 0.5 * Alpha2, Factor1, std.PI + 0.5 * Alpha1, CenterLine.P1), BezierPointOne.Ignore)

        ' set paint operator
        SetPaintOp(PP)
    End Sub

    ''' <summary>
    ''' Draw Rounded Rectangle
    ''' </summary>
    ''' <param name="Origin">Origin (left-bottom)</param>
    ''' <param name="Size">Size</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawRoundedRectangle(Origin As PointD, Size As SizeD, Radius As Double, PP As PaintOp)
        DrawRoundedRectangle(Origin.X, Origin.Y, Size.Width, Size.Height, Radius, PP)
    End Sub

    ''' <summary>
    ''' Draw Rounded Rectangle
    ''' </summary>
    ''' <param name="OriginX">Origin X (left)</param>
    ''' <param name="OriginY">Origin Y (right)</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawRoundedRectangle(OriginX As Double, OriginY As Double, Width As Double, Height As Double, Radius As Double, PP As PaintOp)
        ' make sure radius is not too big
        If Radius > 0.5 * Width Then Radius = 0.5 * Width
        If Radius > 0.5 * Height Then Radius = 0.5 * Height

        ' draw path
        MoveTo(OriginX + Radius, OriginY)
        DrawBezier(BezierD.CircleFourthQuarter(OriginX + Width - Radius, OriginY + Radius, Radius), BezierPointOne.LineTo)
        DrawBezier(BezierD.CircleFirstQuarter(OriginX + Width - Radius, OriginY + Height - Radius, Radius), BezierPointOne.LineTo)
        DrawBezier(BezierD.CircleSecondQuarter(OriginX + Radius, OriginY + Height - Radius, Radius), BezierPointOne.LineTo)
        DrawBezier(BezierD.CircleThirdQuarter(OriginX + Radius, OriginY + Radius, Radius), BezierPointOne.LineTo)

        ' set paint operator
        SetPaintOp(PP)
    End Sub

    ''' <summary>
    ''' Draw Rectangle with Inward Corners
    ''' </summary>
    ''' <param name="OriginX">Origin X (left)</param>
    ''' <param name="OriginY">Origin Y (right)</param>
    ''' <param name="Width">Width</param>
    ''' <param name="Height">Height</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawInwardCornerRectangle(OriginX As Double, OriginY As Double, Width As Double, Height As Double, Radius As Double, PP As PaintOp)
        ' make sure radius is not too big
        If Radius > 0.5 * Width Then Radius = 0.5 * Width
        If Radius > 0.5 * Height Then Radius = 0.5 * Height

        ' draw path
        MoveTo(OriginX, OriginY + Radius)
        DrawBezier(BezierD.CircleFourthQuarter(OriginX, OriginY + Height, Radius), BezierPointOne.LineTo)
        DrawBezier(BezierD.CircleThirdQuarter(OriginX + Width, OriginY + Height, Radius), BezierPointOne.LineTo)
        DrawBezier(BezierD.CircleSecondQuarter(OriginX + Width, OriginY, Radius), BezierPointOne.LineTo)
        DrawBezier(BezierD.CircleFirstQuarter(OriginX, OriginY, Radius), BezierPointOne.LineTo)

        ' set paint operator
        SetPaintOp(PP)
    End Sub

    ''' <summary>
    ''' Draw polygon
    ''' </summary>
    ''' <param name="PathArray">Path array (min 2 points)</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawPolygon(PathArray As PointF(), PP As PaintOp)
        ' program error
        If PathArray.Length < 2 Then Throw New ApplicationException("Draw polygon error: path array must have at least two points")

        ' move to first point
        MyBase.ObjectValueFormat("{0} {1} m" & Microsoft.VisualBasic.Constants.vbLf, ToPt(PathArray(CInt(0)).X), ToPt(PathArray(CInt(0)).Y))

        ' draw lines		
        For Index = 1 To PathArray.Length - 1
            MyBase.ObjectValueFormat("{0} {1} l" & Microsoft.VisualBasic.Constants.vbLf, ToPt(PathArray(CInt(Index)).X), ToPt(PathArray(CInt(Index)).Y))
        Next

        ' set paint operator
        SetPaintOp(PP)
    End Sub

    ''' <summary>
    ''' Draw polygon
    ''' </summary>
    ''' <param name="PathArray">Path array of X and Y values (min 4 and even)</param>
    ''' <param name="PP">Paint operator</param>
    Public Sub DrawPolygon(PathArray As Single(), PP As PaintOp)    ' pairs of x and y values
        ' program error
        If PathArray.Length < 4 Then Throw New ApplicationException("Draw polygon error: path array must have at least 4 items")

        ' program error
        If (PathArray.Length And 1) <> 0 Then Throw New ApplicationException("Draw polygon error: path array must have even number of items")

        ' move to first point
        MyBase.ObjectValueFormat("{0} {1} m" & Microsoft.VisualBasic.Constants.vbLf, ToPt(PathArray(0)), ToPt(PathArray(1)))

        ' draw lines		
        For Index = 2 To PathArray.Length - 1 Step 2
            MyBase.ObjectValueFormat("{0} {1} l" & Microsoft.VisualBasic.Constants.vbLf, ToPt(PathArray(Index)), ToPt(PathArray(Index + 1)))
        Next

        ' set paint operator
        SetPaintOp(PP)
    End Sub

    ''' <summary>
    ''' Draw regular polygon
    ''' </summary>
    ''' <param name="CenterX">Center X</param>
    ''' <param name="CenterY">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="Alpha">Initial angle</param>
    ''' <param name="Sides">Number of sides</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawRegularPolygon(CenterX As Double, CenterY As Double, Radius As Double, Alpha As Double, Sides As Integer, PP As PaintOp)
        DrawRegularPolygon(New PointD(CenterX, CenterY), Radius, Alpha, Sides, PP)
        Return
    End Sub


    ''' <summary>
    ''' Draw regular polygon
    ''' </summary>
    ''' <param name="Center">Center position</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="Alpha">Initial angle</param>
    ''' <param name="Sides">Number of sides</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawRegularPolygon(Center As PointD, Radius As Double, Alpha As Double, Sides As Integer, PP As PaintOp)
        ' validate sides
        If Sides < 3 Then
            Throw New ApplicationException("Draw regular polygon. Number of sides must be 3 or more")
        End If

        ' polygon angle
        Dim DeltaAlpha = 2.0 * std.PI / Sides

        ' first corner coordinates
        MoveTo(New PointD(Center, Radius, Alpha))

        For Side = 1 To Sides - 1
            Alpha += DeltaAlpha
            LineTo(New PointD(Center, Radius, Alpha))
        Next

        ' set paint operator
        SetPaintOp(PP)
        Return
    End Sub


    ''' <summary>
    ''' Draw star
    ''' </summary>
    ''' <param name="CenterX">Center X</param>
    ''' <param name="CenterY">Center Y</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="Alpha">Initial angle</param>
    ''' <param name="Sides">Number of sides</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawStar(CenterX As Double, CenterY As Double, Radius As Double, Alpha As Double, Sides As Integer, PP As PaintOp)
        DrawStar(New PointD(CenterX, CenterY), Radius, Alpha, Sides, PP)
        Return
    End Sub


    ''' <summary>
    ''' Draw star
    ''' </summary>
    ''' <param name="Center">Center position</param>
    ''' <param name="Radius">Radius</param>
    ''' <param name="Alpha">Initial angle</param>
    ''' <param name="Sides">Number of sides</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawStar(Center As PointD, Radius As Double, Alpha As Double, Sides As Integer, PP As PaintOp)
        ' inner radius
        Dim Radius1 As Double = 0

        ' for polygon with less than 5, set inner radius to half the main radius
        If Sides < 5 Then

            ' for polygons with 5 sides, calculate inner radius
            Radius1 = 0.5 * Radius
        Else
            ' polygon angle
            Dim DeltaAlpha = 2.0 * std.PI / Sides

            ' first line
            Dim L1 As LineD = New LineD(New PointD(Center, Radius, Alpha), New PointD(Center, Radius, Alpha + 2.0 * DeltaAlpha))

            ' second line
            Dim L2 As LineD = New LineD(New PointD(Center, Radius, Alpha - DeltaAlpha), New PointD(Center, Radius, Alpha + DeltaAlpha))

            ' inner radius
            Radius1 = (New LineD(New PointD(L1, L2), Center)).Length
        End If

        ' draw star
        DrawStar(Center, Radius, Radius1, Alpha, Sides, PP)
        Return
    End Sub


    ''' <summary>
    ''' Draw star
    ''' </summary>
    ''' <param name="CenterX">Center X</param>
    ''' <param name="CenterY">Center Y</param>
    ''' <param name="Radius1">Radius 1</param>
    ''' <param name="Radius2">Radius 2</param>
    ''' <param name="Alpha">Initial angle</param>
    ''' <param name="Sides">Number of sides</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawStar(CenterX As Double, CenterY As Double, Radius1 As Double, Radius2 As Double, Alpha As Double, Sides As Integer, PP As PaintOp)
        DrawStar(New PointD(CenterX, CenterY), Radius1, Radius2, Alpha, Sides, PP)
        Return
    End Sub


    ''' <summary>
    ''' Draw star
    ''' </summary>
    ''' <param name="Center">Center point</param>
    ''' <param name="Radius1">Radius 1</param>
    ''' <param name="Radius2">Radius 2</param>
    ''' <param name="Alpha">Initial angle</param>
    ''' <param name="Sides">Number of sides</param>
    ''' <param name="PP">Paint operator</param>

    Public Sub DrawStar(Center As PointD, Radius1 As Double, Radius2 As Double, Alpha As Double, Sides As Integer, PP As PaintOp)
        ' validate sides
        If Sides < 3 Then Throw New ApplicationException("Draw star. Number of sides must be 3 or more")

        ' move to first point
        MoveTo(New PointD(Center, Radius1, Alpha))

        ' increment angle
        Dim DeltaAlpha = std.PI / Sides

        ' double number of sides
        Sides *= 2

        ' line to the rest of the points
        For Side = 1 To Sides - 1
            Alpha += DeltaAlpha
            LineTo(New PointD(Center, If((Side And 1) <> 0, Radius2, Radius1), Alpha))
        Next

        ' set paint operator
        SetPaintOp(PP)
        Return
    End Sub


    ''' <summary>
    ''' Begin text mode
    ''' </summary>

    Public Sub BeginTextMode()
        MyBase.ObjectValueAppend("BT" & Microsoft.VisualBasic.Constants.vbLf)
        Return
    End Sub


    ''' <summary>
    ''' End text mode
    ''' </summary>

    Public Sub EndTextMode()
        MyBase.ObjectValueAppend("ET" & Microsoft.VisualBasic.Constants.vbLf)
        Return
    End Sub


    ''' <summary>
    ''' Set text position
    ''' </summary>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>

    Public Sub SetTextPosition(PosX As Double, PosY As Double)
        MyBase.ObjectValueFormat("{0} {1} Td" & Microsoft.VisualBasic.Constants.vbLf, ToPt(PosX), ToPt(PosY))
        Return
    End Sub


    ''' <summary>
    ''' Set text rendering mode
    ''' </summary>
    ''' <param name="TR">Text rendering mode enumeration</param>

    Public Sub SetTextRenderingMode(TR As TextRendering)
        MyBase.ObjectValueFormat("{0} Tr" & Microsoft.VisualBasic.Constants.vbLf, CInt(TR))
        Return
    End Sub


    ''' <summary>
    ''' Set character extra spacing
    ''' </summary>
    ''' <param name="ExtraSpacing">Character extra spacing</param>

    Public Sub SetCharacterSpacing(ExtraSpacing As Double)
        MyBase.ObjectValueFormat("{0} Tc" & Microsoft.VisualBasic.Constants.vbLf, ToPt(ExtraSpacing))
        Return
    End Sub


    ''' <summary>
    ''' Set word extra spacing
    ''' </summary>
    ''' <param name="Spacing">Word extra spacing</param>

    Public Sub SetWordSpacing(Spacing As Double)
        MyBase.ObjectValueFormat("{0} Tw" & Microsoft.VisualBasic.Constants.vbLf, ToPt(Spacing))
        Return
    End Sub


    ''' <summary>
    ''' Reverse characters in a string
    ''' </summary>
    ''' <param name="Text">Input string</param>
    ''' <returns>Output string</returns>

    Public Function ReverseString(Text As String) As String
        Dim RevText As Char() = Text.ToCharArray()
        Array.Reverse(RevText)
        Return New String(RevText)
    End Function


    ''' <summary>
    ''' Draw text
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>
    ''' <remarks>
    ''' This method must be used together with BeginTextMode,
    ''' EndTextMode and SetTextPosition.
    ''' </remarks>

    Public Function DrawText(Font As PdfFont, FontSize As Double, Text As String) As Double           ' font object
        ' in points
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then Return 0

        ' add font code to current list of font codes
        AddToUsedResources(Font)

        ' draw text
        Return DrawTextInternal(Font, FontSize, Text)
    End Function

    Friend Function DrawTextInternal(Font As PdfFont, FontSize As Double, Text As String) As Double           ' font object
        Dim FontResCode As Byte() = Nothing
        Dim FontResGlyph As Byte() = Nothing

        ' convert font sise to string
        Dim FontSizeStr = String.Format(PeriodDecSep, "{0}", Round(FontSize))

        ' set last use glyph index
        Dim GlyphIndexFlag = False

        ' text width
        Dim Width = 0

        ' loop for all text characters
        For Ptr = 0 To Text.Length - 1
            ' get character information
            Dim CharInfo = Font.GetCharInfo(AscW(Text(Ptr)))

            ' set active flag
            CharInfo.ActiveChar = True

            ' accumulate width
            Width += CharInfo.DesignWidth

            ' change between 0-255 and 255-65535
            If Ptr = 0 OrElse CharInfo.Type0Font <> GlyphIndexFlag Then
                ' ")Tj"
                If Ptr <> 0 Then
                    ObjectValueList.Add(Microsoft.VisualBasic.AscW(")"c))
                    ObjectValueList.Add(Microsoft.VisualBasic.AscW("T"c))
                    ObjectValueList.Add(Microsoft.VisualBasic.AscW("j"c))
                End If

                ' save glyph index
                GlyphIndexFlag = CharInfo.Type0Font

                ' ouput font resource and font size
                If Not GlyphIndexFlag Then
                    If FontResCode Is Nothing Then
                        FontResCode = CreateFontResStr(Font.ResourceCode, FontSizeStr)
                        Font.FontResCodeUsed = True
                    End If

                    ObjectValueList.AddRange(FontResCode)
                Else

                    If FontResGlyph Is Nothing Then
                        FontResGlyph = CreateFontResStr(Font.ResourceCodeGlyph, FontSizeStr)

                        If Not Font.FontResGlyphUsed Then
                            Font.CreateGlyphIndexFont()
                        End If
                    End If

                    ObjectValueList.AddRange(FontResGlyph)
                End If

                ObjectValueList.Add(Microsoft.VisualBasic.AscW("("c))
            End If

            ' output character code
            If Not GlyphIndexFlag Then

                ' output glyph index
                OutputOneByte(CharInfo.CharCode)
            Else
                If CharInfo.NewGlyphIndex < 0 Then
                    CharInfo.NewGlyphIndex = If(Font.EmbeddedFont, std.Min(Threading.Interlocked.Increment(Font.NewGlyphIndex), Font.NewGlyphIndex - 1), CharInfo.GlyphIndex)
                End If

                OutputOneByte(CharInfo.NewGlyphIndex >> 8)
                OutputOneByte(CharInfo.NewGlyphIndex And &HFF)
            End If
        Next

        ' ")Tj"
        ObjectValueList.Add(Microsoft.VisualBasic.AscW(")"c))
        ObjectValueList.Add(Microsoft.VisualBasic.AscW("T"c))
        ObjectValueList.Add(Microsoft.VisualBasic.AscW("j"c))
        ObjectValueList.Add(Microsoft.VisualBasic.AscW(Microsoft.VisualBasic.Strings.ChrW(10)))

        Return Font.FontDesignToUserUnits(FontSize, Width)
    End Function

    Friend Sub OutputOneByte(CharCode As Integer)
        Select Case CharCode
            Case 13
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("\"c))
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("r"c))
            Case 10
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("\"c))
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("n"c))
            Case Asc("("c)
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("\"c))
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("("c))
            Case Asc(")"c)
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("\"c))
                ObjectValueList.Add(Microsoft.VisualBasic.AscW(")"c))
            Case Asc("\"c)
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("\"c))
                ObjectValueList.Add(Microsoft.VisualBasic.AscW("\"c))
            Case Else
                ObjectValueList.Add(CharCode)
        End Select
    End Sub

    Friend Function CreateFontResStr(ResCode As String, SizeStr As String) As Byte()
        Dim FontRes = New Byte(ResCode.Length + SizeStr.Length + 4 - 1) {}
        Dim Index As i32 = 0

        For Each TextChar In ResCode
            FontRes(++Index) = Microsoft.VisualBasic.AscW(TextChar)
        Next

        FontRes(++Index) = Microsoft.VisualBasic.AscW(" "c)

        For Each TextChar In SizeStr
            FontRes(++Index) = Microsoft.VisualBasic.AscW(TextChar)
        Next

        FontRes(++Index) = Microsoft.VisualBasic.AscW(" "c)
        FontRes(++Index) = Microsoft.VisualBasic.AscW("T"c)
        FontRes(++Index) = Microsoft.VisualBasic.AscW("f"c)

        Return FontRes
    End Function

    ''' <summary>
    ''' Draw one line of text left justified
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>
    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, Text As String) As Double       ' in points
        Return DrawText(Font, FontSize, PosX, PosY, TextJustify.Left, Text)
    End Function

    ''' <summary>
    ''' Draw one line of text
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Justify">Text justify enumeration</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>
    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, Justify As TextJustify, Text As String) As Double     ' in points
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then
            Return 0
        End If

        ' add font code to current list of font codes
        AddToUsedResources(Font)

        ' adjust position
        Select Case Justify
            ' right
            Case TextJustify.Right
                PosX -= Font.TextWidth(FontSize, Text)

            ' center
            Case TextJustify.Center
                PosX -= 0.5 * Font.TextWidth(FontSize, Text)
        End Select

        ' draw text
        BeginTextMode()
        SetTextPosition(PosX, PosY)
        Dim Width = DrawTextInternal(Font, FontSize, Text)
        EndTextMode()

        ' return text width
        Return Width
    End Function


    ''' <summary>
    ''' Draw one line of text width draw style
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="DrawStyle">Drawing style enumeration</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>

    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, DrawStyle As DrawStyle, Text As String) As Double     ' in points
        Return DrawText(Font, FontSize, PosX, PosY, TextJustify.Left, DrawStyle, Color.Empty, Text)
    End Function


    ''' <summary>
    ''' Draw one line of text with a given color
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="TextColor">Color</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>

    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, TextColor As Color, Text As String) As Double     ' in points
        Return DrawText(Font, FontSize, PosX, PosY, TextJustify.Left, DrawStyle.Normal, TextColor, Text)
    End Function


    ' Draw text width draw style

    ''' <summary>
    ''' Draw one line of text with text justification, drawing style and color
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Justify">Text justify enumeration</param>
    ''' <param name="DrawStyle">Drawing style enumeration</param>
    ''' <param name="TextColor">Color</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>
    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, Justify As TextJustify, DrawStyle As DrawStyle, TextColor As Color, Text As String) As Double     ' in points
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then
            Return 0
        End If

        ' text width
        Dim TextWidth As Double = 0

        ' we have color
        If TextColor <> Color.Empty Then
            ' save graphics state
            SaveGraphicsState()

            ' change non stroking color
            SetColorNonStroking(TextColor)
        End If

        ' not subscript or superscript
        If (DrawStyle And (DrawStyle.Subscript Or DrawStyle.Superscript)) = 0 Then
            ' draw text string
            TextWidth = DrawText(Font, FontSize, PosX, PosY, Justify, Text)

            ' not regular style
            If DrawStyle <> DrawStyle.Normal Then
                ' change stroking color
                If TextColor <> Color.Empty Then SetColorStroking(TextColor)

                ' adjust position
                Select Case Justify
                    ' right
                    Case TextJustify.Right
                        PosX -= TextWidth

                    ' center
                    Case TextJustify.Center
                        PosX -= 0.5 * TextWidth
                End Select

                ' underline
                If (DrawStyle And DrawStyle.Underline) <> 0 Then
                    Dim UnderlinePos = PosY + Font.UnderlinePosition(FontSize)
                    DrawLine(PosX, UnderlinePos, PosX + TextWidth, UnderlinePos, Font.UnderlineWidth(FontSize))
                End If

                ' strikeout
                If (DrawStyle And DrawStyle.Strikeout) <> 0 Then
                    Dim StrikeoutPos = PosY + Font.StrikeoutPosition(FontSize)
                    DrawLine(PosX, StrikeoutPos, PosX + TextWidth, StrikeoutPos, Font.StrikeoutWidth(FontSize))
                End If

                ' subscript or superscript
            End If
        Else
            ' subscript
            If (DrawStyle And (DrawStyle.Subscript Or DrawStyle.Superscript)) = DrawStyle.Subscript Then
                ' subscript font size and location
                PosY -= Font.SubscriptPosition(FontSize)
                FontSize = Font.SubscriptSize(FontSize)

                ' draw text string
                TextWidth = DrawText(Font, FontSize, PosX, PosY, Justify, Text)
            End If

            ' superscript
            If (DrawStyle And (DrawStyle.Subscript Or DrawStyle.Superscript)) = DrawStyle.Superscript Then
                ' superscript font size and location
                PosY += Font.SuperscriptPosition(FontSize)
                FontSize = Font.SuperscriptSize(FontSize)

                ' draw text string
                TextWidth = DrawText(Font, FontSize, PosX, PosY, Justify, Text)
            End If
        End If

        ' we have color
        If TextColor <> Color.Empty Then
            ' save graphics state
            RestoreGraphicsState()
        End If

        ' return text width
        Return TextWidth
    End Function


    ''' <summary>
    ''' Draw text with kerning array
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="KerningArray">Kerning array</param>
    ''' <returns>Text width</returns>
    ''' <remarks>
    ''' Each kerning item consists of text and position adjustment.
    ''' The adjustment is a negative number.
    ''' </remarks>

    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, KerningArray As KerningAdjust()) As Double      ' in points
        ' text is null or empty
        If KerningArray Is Nothing OrElse KerningArray.Length = 0 Then Return 0

        ' add font code to current list of font codes
        AddToUsedResources(Font)

        ' draw text initialization
        BeginTextMode()
        SetTextPosition(PosX, PosY)

        ' draw text with kerning
        Dim Width = DrawTextWithKerning(Font, FontSize, KerningArray)

        ' draw text termination
        EndTextMode()

        ' exit
        Return Width
    End Function

    Friend Function DrawTextWithKerning(Font As PdfFont, FontSize As Double, KerningArray As KerningAdjust()) As Double           ' font object
        Dim FontResCode As Byte() = Nothing
        Dim FontResGlyph As Byte() = Nothing

        ' convert font sise to string
        Dim FontSizeStr = String.Format(PeriodDecSep, "{0}", Round(FontSize))

        ' set last use glyph index
        Dim GlyphIndexFlag = False

        ' text width
        Dim Width = 0

        ' loop for kerning pairs
        Dim Index = 0

        While True
            Dim KA = KerningArray(Index)
            Dim Text = KA.Text

            ' loop for all text characters
            For Ptr = 0 To Text.Length - 1
                ' get character information
                Dim CharInfo = Font.GetCharInfo(AscW(Text(Ptr)))

                ' set active flag
                CharInfo.ActiveChar = True

                ' accumulate width
                Width += CharInfo.DesignWidth

                ' change between 0-255 and 255-65535
                If Index = 0 AndAlso Ptr = 0 OrElse CharInfo.Type0Font <> GlyphIndexFlag Then
                    ' close partial string
                    If Ptr <> 0 Then
                        ObjectValueList.Add(Microsoft.VisualBasic.AscW(")"c))
                    End If

                    ' close code/glyph area
                    If Index <> 0 Then
                        ObjectValueList.Add(Microsoft.VisualBasic.AscW("]"c))
                        ObjectValueList.Add(Microsoft.VisualBasic.AscW("T"c))
                        ObjectValueList.Add(Microsoft.VisualBasic.AscW("J"c))
                    End If

                    ' save glyph index
                    GlyphIndexFlag = CharInfo.Type0Font

                    ' ouput font resource and font size
                    If Not GlyphIndexFlag Then
                        If FontResCode Is Nothing Then
                            FontResCode = CreateFontResStr(Font.ResourceCode, FontSizeStr)
                            Font.FontResCodeUsed = True
                        End If

                        ObjectValueList.AddRange(FontResCode)
                    Else

                        If FontResGlyph Is Nothing Then
                            FontResGlyph = CreateFontResStr(Font.ResourceCodeGlyph, FontSizeStr)
                            If Not Font.FontResGlyphUsed Then Font.CreateGlyphIndexFont()
                        End If

                        ObjectValueList.AddRange(FontResGlyph)
                    End If

                    ObjectValueList.Add(Microsoft.VisualBasic.AscW("["c))
                    ObjectValueList.Add(Microsoft.VisualBasic.AscW("("c))
                ElseIf Ptr = 0 Then
                    ObjectValueList.Add(Microsoft.VisualBasic.AscW("("c))
                End If

                ' output character code
                If Not GlyphIndexFlag Then

                    ' output glyph index
                    OutputOneByte(CharInfo.CharCode)
                Else
                    If CharInfo.NewGlyphIndex < 0 Then CharInfo.NewGlyphIndex = If(Font.EmbeddedFont, std.Min(Threading.Interlocked.Increment(Font.NewGlyphIndex), Font.NewGlyphIndex - 1), CharInfo.GlyphIndex)
                    OutputOneByte(CharInfo.NewGlyphIndex >> 8)
                    OutputOneByte(CharInfo.NewGlyphIndex And &HFF)
                End If
            Next

            ObjectValueList.Add(Microsoft.VisualBasic.AscW(")"c))

            ' test for end of kerning array
            Index += 1
            If Index = KerningArray.Length Then Exit While

            ' add adjustment
            ObjectValueFormat("{0}", Round(-KA.Adjust))

            ' convert the adjustment width to font design width
            Width += CInt(std.Round(KA.Adjust * Font.DesignHeight / 1000.0, 0, MidpointRounding.AwayFromZero))
        End While

        ' "]Tj"
        ObjectValueList.Add(Microsoft.VisualBasic.AscW("]"c))
        ObjectValueList.Add(Microsoft.VisualBasic.AscW("T"c))
        ObjectValueList.Add(Microsoft.VisualBasic.AscW("J"c))
        ObjectValueList.Add(Microsoft.VisualBasic.AscW(Microsoft.VisualBasic.Strings.ChrW(10)))
        Return Font.FontDesignToUserUnits(FontSize, Width)
    End Function


    ''' <summary>
    ''' Draw text with kerning
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>

    Public Function DrawTextWithKerning(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, Text As String) As Double        ' in points
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then Return 0

        ' create text position adjustment array based on kerning information
        Dim KernArray = Font.TextKerning(Text)

        ' no kerning
        If KernArray Is Nothing Then Return DrawText(Font, FontSize, PosX, PosY, Text)

        ' draw text with adjustment
        Return DrawText(Font, FontSize, PosX, PosY, KernArray)
    End Function


    ''' <summary>
    ''' Draw text with special effects
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Justify">Text justify enumeration</param>
    ''' <param name="OutlineWidth">Outline width</param>
    ''' <param name="StrokingColor">Stoking (outline) color</param>
    ''' <param name="NonStokingColor">Non stroking (fill) color</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Text width</returns>

    Public Function DrawText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, Justify As TextJustify, OutlineWidth As Double, StrokingColor As Color, NonStokingColor As Color, Text As String) As Double     ' in points
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then Return 0

        ' add font code to current list of font codes
        AddToUsedResources(Font)

        ' save graphics state
        SaveGraphicsState()

        ' create text position adjustment array based on kerning information
        Dim KernArray = Font.TextKerning(Text)

        ' text width
        Dim Width = If(KernArray Is Nothing, Font.TextWidth(FontSize, Text), Font.TextKerningWidth(FontSize, KernArray))

        ' adjust position
        Select Case Justify
            ' right
            Case TextJustify.Right
                PosX -= Width

            ' center
            Case TextJustify.Center
                PosX -= 0.5 * Width
        End Select

        ' special effects
        Dim TR = TextRendering.Fill

        If Not StrokingColor.IsEmpty Then
            SetLineWidth(OutlineWidth)
            SetColorStroking(StrokingColor)
            TR = TextRendering.Stroke
        End If

        If Not NonStokingColor.IsEmpty Then
            SetColorNonStroking(NonStokingColor)
            TR = If(StrokingColor.IsEmpty, TextRendering.Fill, TextRendering.FillStroke)
        End If

        ' draw text initialization
        BeginTextMode()
        SetTextPosition(PosX, PosY)
        SetTextRenderingMode(TR)

        ' draw text without kerning
        If KernArray Is Nothing Then

            ' draw text with kerning
            DrawTextInternal(Font, FontSize, Text)
        Else
            DrawTextWithKerning(Font, FontSize, KernArray)
        End If

        ' draw text termination
        EndTextMode()

        ' restore graphics state
        RestoreGraphicsState()

        ' exit
        Return Width
    End Function


    ''' <summary>
    ''' Draw text with annotation action
    ''' </summary>
    ''' <param name="Page">Current page</param>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="TextAbsPosX">Text absolute position X</param>
    ''' <param name="TextAbsPosY">Text absolute position Y</param>
    ''' <param name="Text">Text</param>
    ''' <param name="AnnotAction">Annotation action</param>
    ''' <returns>Text width</returns>
    ''' <remarks>
    ''' 	The position arguments are in relation to the
    ''' 	bottom left corner of the paper.
    ''' </remarks>

    Public Function DrawTextWithAnnotation(Page As PdfPage, Font As PdfFont, FontSize As Double, TextAbsPosX As Double, TextAbsPosY As Double, Text As String, AnnotAction As AnnotAction) As Double      ' in points
        Return DrawTextWithAnnotation(Page, Font, FontSize, TextAbsPosX, TextAbsPosY, TextJustify.Left, DrawStyle.Underline, Color.DarkBlue, Text, AnnotAction)
    End Function


    ''' <summary>
    ''' Draw web link with one line of text
    ''' </summary>
    ''' <param name="Page">Current page</param>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="TextAbsPosX">Text absolute position X</param>
    ''' <param name="TextAbsPosY">Text absolute position Y</param>
    ''' <param name="Text">Text</param>
    ''' <param name="WebLinkStr">Web link</param>
    ''' <returns>Text width</returns>
    ''' <remarks>
    ''' 	The position arguments are in relation to the
    ''' 	bottom left corner of the paper.
    ''' 	Text will be drawn left justified, underlined and in dark blue.
    ''' </remarks>

    Public Function DrawWebLink(Page As PdfPage, Font As PdfFont, FontSize As Double, TextAbsPosX As Double, TextAbsPosY As Double, Text As String, WebLinkStr As String) As Double       ' in points
        Return DrawTextWithAnnotation(Page, Font, FontSize, TextAbsPosX, TextAbsPosY, TextJustify.Left, DrawStyle.Underline, Color.DarkBlue, Text, New AnnotWebLink(WebLinkStr))
    End Function


    ''' <summary>
    ''' Draw web link with one line of text
    ''' </summary>
    ''' <param name="Page">Current page</param>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="TextAbsPosX">Text absolute position X</param>
    ''' <param name="TextAbsPosY">Text absolute position Y</param>
    ''' <param name="Justify">Text justify enumeration.</param>
    ''' <param name="DrawStyle">Draw style enumeration</param>
    ''' <param name="TextColor">Color</param>
    ''' <param name="Text">Text</param>
    ''' <param name="WebLinkStr">Web link</param>
    ''' <returns>Text width</returns>
    ''' <remarks>
    ''' 	The position arguments are in relation to the
    ''' 	bottom left corner of the paper.
    ''' </remarks>

    Public Function DrawWebLink(Page As PdfPage, Font As PdfFont, FontSize As Double, TextAbsPosX As Double, TextAbsPosY As Double, Justify As TextJustify, DrawStyle As DrawStyle, TextColor As Color, Text As String, WebLinkStr As String) As Double     ' in points
        Return DrawTextWithAnnotation(Page, Font, FontSize, TextAbsPosX, TextAbsPosY, Justify, DrawStyle, TextColor, Text, New AnnotWebLink(WebLinkStr))
    End Function


    ''' <summary>
    ''' Draw text with annotation action
    ''' </summary>
    ''' <param name="Page">Current page</param>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="TextAbsPosX">Text absolute position X</param>
    ''' <param name="TextAbsPosY">Text absolute position Y</param>
    ''' <param name="Justify">Text justify enumeration.</param>
    ''' <param name="DrawStyle">Draw style enumeration</param>
    ''' <param name="TextColor">Color</param>
    ''' <param name="Text">Text</param>
    ''' <param name="AnnotAction">Annotation action</param>
    ''' <returns>Text width</returns>
    ''' <remarks>
    ''' 	The position arguments are in relation to the
    ''' 	bottom left corner of the paper.
    ''' </remarks>

    Public Function DrawTextWithAnnotation(Page As PdfPage, Font As PdfFont, FontSize As Double, TextAbsPosX As Double, TextAbsPosY As Double, Justify As TextJustify, DrawStyle As DrawStyle, TextColor As Color, Text As String, AnnotAction As AnnotAction) As Double        ' in points
        Dim Width = DrawText(Font, FontSize, TextAbsPosX, TextAbsPosY, Justify, DrawStyle, TextColor, Text)
        If Width = 0.0 Then Return 0.0

        ' adjust position
        Select Case Justify
            ' right
            Case TextJustify.Right
                TextAbsPosX -= Width

            ' center
            Case TextJustify.Center
                TextAbsPosX -= 0.5 * Width
        End Select

        Dim AnnotRect As PdfRectangle = New PdfRectangle(TextAbsPosX, TextAbsPosY - Font.DescentPlusLeading(FontSize), TextAbsPosX + Width, TextAbsPosY + Font.AscentPlusLeading(FontSize))
        Page.AddAnnotInternal(AnnotRect, AnnotAction)
        Return Width
    End Function

    ''' <summary>
    ''' Draw TextBox
    ''' </summary>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosYTop">Position Y (by reference)</param>
    ''' <param name="PosYBottom">Position Y bottom</param>
    ''' <param name="LineNo">Start at line number</param>
    ''' <param name="TextBox">TextBox</param>
    ''' <param name="Page">Page if TextBox contains web link segment</param>
    ''' <returns>Next line number</returns>
    ''' <remarks>
    ''' Before calling this method you must add text to a TextBox object.
    ''' <para>
    ''' Set the PosX and PosYTop to the left top corner of the text area.
    ''' Note PosYTop is by reference. This variable will be updated to
    ''' the next vertical line position after the method was executed.
    ''' </para>
    ''' <para>
    ''' Set the PosYBottom to the bottom of your page. The method will
    ''' not print below this value.
    ''' </para>
    ''' <para>
    ''' Set the LineNo to the first line to be printed. Initially 
    ''' this will be zero. After the method returns, PosYTop is set 
    ''' to next print line on the page and LineNo is set to next line 
    ''' within the box.
    ''' </para>
    ''' <para>
    ''' If LineNo is equals to TextBox.LineCount the box was fully printed. 
    ''' </para>
    ''' <para>
    ''' If LineNo is less than TextBox.LineCount box printing was not
    ''' done. Start a new PdfPage and associated PdfContents. Set 
    ''' PosYTop to desired start position. Set LineNo to the value
    ''' returned by this method, and call the method again.
    ''' </para>
    ''' <para>
    ''' If your TextBox contains WebLink segment you must supply
    ''' Page argument and position X and Y must be relative to
    ''' page bottom left corner.
    ''' </para>
    ''' </remarks>
    Public Function DrawText(PosX As Double, ByRef PosYTop As Double, PosYBottom As Double, LineNo As Integer, TextBox As TextBox, Optional Page As PdfPage = Nothing) As Integer
        Return DrawText(PosX, PosYTop, PosYBottom, LineNo, 0.0, 0.0, TextBoxJustify.Left, TextBox, Page)
    End Function


    ' Draw Text for TextBox with web link

    ''' <summary>
    ''' Draw TextBox
    ''' </summary>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosYTop">Position Y (by reference)</param>
    ''' <param name="PosYBottom">Position Y bottom</param>
    ''' <param name="LineNo">Start at line number</param>
    ''' <param name="LineExtraSpace">Extra line spacing</param>
    ''' <param name="ParagraphExtraSpace">Extra paragraph spacing</param>
    ''' <param name="Justify">TextBox justify enumeration</param>
    ''' <param name="TextBox">TextBox</param>
    ''' <param name="Page">Page if TextBox contains web link segment</param>
    ''' <returns>Next line number</returns>
    ''' <remarks>
    ''' Before calling this method you must add text to a TextBox object.
    ''' <para>
    ''' Set the PosX and PosYTop to the left top corner of the text area.
    ''' Note PosYTop is by reference. This variable will be updated to
    ''' the next vertical line position after the method was executed.
    ''' </para>
    ''' <para>
    ''' Set the PosYBottom to the bottom of your page. The method will
    ''' not print below this value.
    ''' </para>
    ''' <para>
    ''' Set the LineNo to the first line to be printed. Initially 
    ''' this will be zero. After the method returns, PosYTop is set 
    ''' to next print line on the page and LineNo is set to next line 
    ''' within the box.
    ''' </para>
    ''' <para>
    ''' If LineNo is equals to TextBox.LineCount the box was fully printed. 
    ''' </para>
    ''' <para>
    ''' If LineNo is less than TextBox.LineCount box printing was not
    ''' done. Start a new PdfPage and associated PdfContents. Set 
    ''' PosYTop to desired start position. Set LineNo to the value
    ''' returned by this method, and call the method again.
    ''' </para>
    ''' <para>
    ''' If your TextBox contains WebLink segment you must supply
    ''' Page argument and position X and Y must be relative to
    ''' page bottom left corner.
    ''' </para>
    ''' <para>
    ''' TextBoxJustify controls horizontal justification. FitToWidth
    ''' will display a straight right edge.
    ''' </para>
    ''' </remarks>

    Public Function DrawText(PosX As Double,
                             ByRef PosYTop As Double,
                             PosYBottom As Double,
                             LineNo As Integer,
                             LineExtraSpace As Double,
                             ParagraphExtraSpace As Double,
                             Justify As TextBoxJustify,
                             TextBox As TextBox,
                             Optional Page As PdfPage = Nothing) As Integer

        TextBox.Terminate()

        While LineNo < TextBox.LineCount
            ' short cut
            Dim Line = TextBox(LineNo)

            ' break out of the loop if printing below bottom line
            If PosYTop - Line.LineHeight < PosYBottom Then Exit While

            ' adjust PosY to font base line
            PosYTop -= Line.Ascent

            ' text horizontal position
            Dim X = PosX
            Dim W = TextBox.BoxWidth

            ' if we have first line indent, adjust text x position for first line of a paragraph
            If TextBox.FirstLineIndent <> 0 AndAlso (LineNo = 0 OrElse TextBox(LineNo - 1).EndOfParagraph) Then
                X += TextBox.FirstLineIndent
                W -= TextBox.FirstLineIndent
            End If

            ' draw text to fit box width
            If Justify = TextBoxJustify.FitToWidth AndAlso Not Line.EndOfParagraph Then
                DrawText(X, PosYTop, W, Line, Page)

                ' draw text center or right justified
            ElseIf Justify = TextBoxJustify.Center OrElse Justify = TextBoxJustify.Right Then

                ' draw text normal
                DrawText(X, PosYTop, W, Justify, Line, Page)
            Else
                DrawText(X, PosYTop, Line, Page)
            End If


            ' advance position y to next line
            PosYTop -= Line.Descent + LineExtraSpace
            If Line.EndOfParagraph Then PosYTop -= ParagraphExtraSpace
            LineNo += 1
        End While

        Return LineNo
    End Function

    ''' <summary>
    ''' Draw text within text box left justified
    ''' </summary>
    ''' <param name="PosX"></param>
    ''' <param name="PosY"></param>
    ''' <param name="Line"></param>
    ''' <param name="Page"></param>
    ''' <returns></returns>
    Private Function DrawText(PosX As Double, PosY As Double, Line As TextBoxLine, Page As PdfPage) As Double
        Dim SegPosX = PosX

        For Each Seg In Line.SegArray
            Dim SegWidth = DrawText(Seg.Font, Seg.FontSize, SegPosX, PosY, TextJustify.Left, Seg.DrawStyle, Seg.FontColor, Seg.Text)

            If Seg.AnnotAction IsNot Nothing Then
                If Page Is Nothing Then Throw New ApplicationException("TextBox with WebLink. You must call DrawText with PdfPage")
                Dim AnnotRect As PdfRectangle = New PdfRectangle(SegPosX, PosY - Seg.Font.DescentPlusLeading(Seg.FontSize), SegPosX + SegWidth, PosY + Seg.Font.AscentPlusLeading(Seg.FontSize))
                Page.AddAnnotInternal(AnnotRect, Seg.AnnotAction)
            End If

            SegPosX += SegWidth
        Next

        Return SegPosX - PosX
    End Function

    ''' <summary>
    ''' Draw text within text box center or right justified
    ''' </summary>
    ''' <param name="PosX"></param>
    ''' <param name="PosY"></param>
    ''' <param name="Width"></param>
    ''' <param name="Justify"></param>
    ''' <param name="Line"></param>
    ''' <param name="Page"></param>
    ''' <returns></returns>
    Private Function DrawText(PosX As Double, PosY As Double, Width As Double, Justify As TextBoxJustify, Line As TextBoxLine, Page As PdfPage) As Double
        Dim LineWidth As Double = 0

        For Each Seg In Line.SegArray
            LineWidth += Seg.SegWidth
        Next

        Dim SegPosX = PosX

        If Justify = TextBoxJustify.Right Then
            SegPosX += Width - LineWidth
        Else
            SegPosX += 0.5 * (Width - LineWidth)
        End If

        For Each Seg In Line.SegArray
            Dim SegWidth = DrawText(Seg.Font, Seg.FontSize, SegPosX, PosY, TextJustify.Left, Seg.DrawStyle, Seg.FontColor, Seg.Text)

            If Seg.AnnotAction IsNot Nothing Then
                If Page Is Nothing Then Throw New ApplicationException("TextBox with WebLink. You must call DrawText with PdfPage")
                Dim AnnotRect As PdfRectangle = New PdfRectangle(SegPosX, PosY - Seg.Font.DescentPlusLeading(Seg.FontSize), SegPosX + SegWidth, PosY + Seg.Font.AscentPlusLeading(Seg.FontSize))
                Page.AddAnnotInternal(AnnotRect, Seg.AnnotAction)
            End If

            SegPosX += SegWidth
        Next

        Return SegPosX - PosX
    End Function

    ''' <summary>
    ''' Draw text justify to width within text box
    ''' </summary>
    ''' <param name="PosX"></param>
    ''' <param name="PosY"></param>
    ''' <param name="Width"></param>
    ''' <param name="Line"></param>
    ''' <param name="Page"></param>
    ''' <returns></returns>
    Private Function DrawText(PosX As Double, PosY As Double, Width As Double, Line As TextBoxLine, Page As PdfPage) As Double
        Dim WordSpacing As Double = Nothing, CharSpacing As Double = Nothing
        If Not TextFitToWidth(Width, WordSpacing, CharSpacing, Line) Then Return DrawText(PosX, PosY, Line, Page)
        SaveGraphicsState()
        SetWordSpacing(WordSpacing)
        SetCharacterSpacing(CharSpacing)
        Dim SegPosX = PosX

        For Each Seg In Line.SegArray
            Dim SegWidth = DrawText(Seg.Font, Seg.FontSize, SegPosX, PosY, TextJustify.Left, Seg.DrawStyle, Seg.FontColor, Seg.Text) + Seg.SpaceCount * WordSpacing + Seg.Text.Length * CharSpacing

            If Seg.AnnotAction IsNot Nothing Then
                If Page Is Nothing Then Throw New ApplicationException("TextBox with WebLink. You must call DrawText with PdfPage")
                Dim AnnotRect As PdfRectangle = New PdfRectangle(SegPosX, PosY - Seg.Font.DescentPlusLeading(Seg.FontSize), SegPosX + SegWidth, PosY + Seg.Font.AscentPlusLeading(Seg.FontSize))
                Page.AddAnnotInternal(AnnotRect, Seg.AnnotAction)
            End If

            SegPosX += SegWidth
        Next

        RestoreGraphicsState()
        Return SegPosX - PosX
    End Function

    ''' <summary>
    ''' Stretch text to given width
    ''' </summary>
    ''' <param name="ReqWidth"></param>
    ''' <param name="WordSpacing"></param>
    ''' <param name="CharSpacing"></param>
    ''' <param name="Line"></param>
    ''' <returns></returns>
    Private Function TextFitToWidth(ReqWidth As Double, <Out> ByRef WordSpacing As Double, <Out> ByRef CharSpacing As Double, Line As TextBoxLine) As Boolean
        WordSpacing = 0
        CharSpacing = 0
        Dim CharCount = 0
        Dim Width As Double = 0
        Dim SpaceCount = 0
        Dim SpaceWidth As Double = 0

        For Each Seg In Line.SegArray
            ' accumulate line width
            CharCount += Seg.Text.Length
            Width += Seg.SegWidth

            ' count spaces
            SpaceCount += Seg.SpaceCount

            ' accumulate space width
            SpaceWidth += Seg.SpaceCount * Seg.Font.CharWidth(Seg.FontSize, " "c)
        Next

        ' reduce character count by one
        CharCount -= 1
        If CharCount <= 0 Then Return False

        ' extra spacing required
        Dim ExtraSpace = ReqWidth - Width

        ' highest possible output device resolution (12000 dots per inch)
        Dim MaxRes = 0.006 / ScaleFactor

        ' string is too wide
        If ExtraSpace < -MaxRes Then Return False

        ' string is just right
        If ExtraSpace < MaxRes Then Return True

        ' String does not have any blank characters
        If SpaceCount = 0 Then
            CharSpacing = ExtraSpace / CharCount
            Return True
        End If

        ' extra space per word
        WordSpacing = ExtraSpace / SpaceCount

        ' extra space is equal or less than one blank
        If WordSpacing <= SpaceWidth / SpaceCount Then Return True

        ' extra space is larger that one blank
        ' increase character and word spacing
        CharSpacing = ExtraSpace / (10 * SpaceCount + CharCount)
        WordSpacing = 10 * CharSpacing
        Return True
    End Function

    ''' <summary>
    ''' Clip text exposing area underneath
    ''' </summary>
    ''' <param name="Font">Font</param>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Text">Text</param>
    Public Sub ClipText(Font As PdfFont, FontSize As Double, PosX As Double, PosY As Double, Text As String)      ' in points
        ' text is null or empty
        If String.IsNullOrEmpty(Text) Then Return

        ' add font code to current list of font codes
        AddToUsedResources(Font)

        ' draw text
        BeginTextMode()
        SetTextPosition(PosX, PosY)
        SetTextRenderingMode(TextRendering.Clip)
        DrawTextInternal(Font, FontSize, Text)
        EndTextMode()
        Return
    End Sub


    ''' <summary>
    ''' Draw barcode
    ''' </summary>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="BarWidth">Narrow bar width</param>
    ''' <param name="BarHeight">Barcode height</param>
    ''' <param name="Barcode">Derived barcode class</param>
    ''' <param name="TextFont">Optional text font</param>
    ''' <param name="FontSize">Optional text font size</param>
    ''' <returns>Barcode width</returns>
    ''' <remarks>
    ''' <para>
    ''' PosX can be the left, centre or right side of the barcode.
    ''' The Justify argument controls the meaning of PosX.
    ''' PosY is the position of the bottom side of the barcode. 
    ''' If optional text is displayed it will be
    ''' displayed below PosY. If optional text is wider than the
    ''' barcode it will be extended to the left and right sides
    ''' of the barcode.
    ''' </para>
    ''' <para>
    ''' The BarWidth argument is the width of the narrow bar.
    ''' </para>
    ''' <para>
    ''' The BarcodeHeight argument is the height of the barcode 
    ''' excluding optional text.
    ''' </para>
    ''' <para>
    ''' Set Barcode to one of the derived classes. 
    ''' This library supports: Barcode128, Barcode39 and BarcodeEAN13.
    ''' Note BarcodeEAN13 supports Barcode UPC-A.
    ''' </para>
    ''' <para>
    ''' Barcode text is optional. If TextFont and FontSize are omitted 
    ''' no text will be drawn under the barcode. If TextFont and
    ''' FontSize are specified the barcode text will be displayed
    ''' under the barcode. It will be horizontally centered in relation
    ''' to the barcode.
    ''' </para>
    ''' <para>
    ''' Barcode text is displayed below PosY. Make sure to leave
    ''' space under the barcode.
    ''' </para>
    ''' </remarks>
    Public Function DrawBarcode(PosX As Double, PosY As Double, BarWidth As Double, BarHeight As Double, Barcode As Barcode, Optional TextFont As PdfFont = Nothing, Optional FontSize As Double = 0.0) As Double
        Return DrawBarcode(PosX, PosY, TextJustify.Left, BarWidth, BarHeight, Color.Black, Barcode, TextFont, FontSize)
    End Function

    ''' <summary>
    ''' Draw barcode
    ''' </summary>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Justify">Barcode justify (using TextJustify enumeration)</param>
    ''' <param name="BarWidth">Narrow bar width</param>
    ''' <param name="BarcodeHeight">Barcode height</param>
    ''' <param name="Barcode">Derived barcode class</param>
    ''' <param name="TextFont">Text font</param>
    ''' <param name="FontSize">Text font size</param>
    ''' <returns>Barcode width</returns>
    ''' <remarks>
    ''' <para>
    ''' PosX can be the left, centre or right side of the barcode.
    ''' The Justify argument controls the meaning of PosX.
    ''' PosY is the position of the bottom side of the barcode. 
    ''' If optional text is displayed it will be
    ''' displayed below PosY. If optional text is wider than the
    ''' barcode it will be extended to the left and right sides
    ''' of the barcode.
    ''' </para>
    ''' <para>
    ''' The BarWidth argument is the width of the narrow bar.
    ''' </para>
    ''' <para>
    ''' The BarcodeHeight argument is the height of the barcode 
    ''' excluding optional text.
    ''' </para>
    ''' <para>
    ''' Set Barcode to one of the derived classes. 
    ''' This library supports: Barcode128, Barcode39 and BarcodeEAN13.
    ''' Note BarcodeEAN13 supports Barcode UPC-A.
    ''' </para>
    ''' <para>
    ''' Barcode text is optional. If TextFont and FontSize are omitted 
    ''' no text will be drawn under the barcode. If TextFont and
    ''' FontSize are specified the barcode text will be displayed
    ''' under the barcode. It will be horizontally centered in relation
    ''' to the barcode.
    ''' </para>
    ''' <para>
    ''' Barcode text is displayed below PosY. Make sure to leave
    ''' space under the barcode.
    ''' </para>
    ''' </remarks>
    Public Function DrawBarcode(PosX As Double, PosY As Double, Justify As TextJustify, BarWidth As Double, BarcodeHeight As Double, Barcode As Barcode, Optional TextFont As PdfFont = Nothing, Optional FontSize As Double = 0.0) As Double
        Return DrawBarcode(PosX, PosY, Justify, BarWidth, BarcodeHeight, Color.Black, Barcode, TextFont, FontSize)
    End Function


    ''' <summary>
    ''' Draw barcode
    ''' </summary>
    ''' <param name="PosX">Position X</param>
    ''' <param name="PosY">Position Y</param>
    ''' <param name="Justify">Barcode justify (using TextJustify enumeration)</param>
    ''' <param name="BarWidth">Narrow bar width</param>
    ''' <param name="BarHeight">Barcode height</param>
    ''' <param name="BarColor">Barcode color</param>
    ''' <param name="Barcode">Derived barcode class</param>
    ''' <param name="TextFont">Text font</param>
    ''' <param name="FontSize">Text font size</param>
    ''' <returns>Barcode width</returns>
    ''' <remarks>
    ''' <para>
    ''' PosX can be the left, centre or right side of the barcode.
    ''' The Justify argument controls the meaning of PosX.
    ''' PosY is the position of the bottom side of the barcode. 
    ''' If optional text is displayed it will be
    ''' displayed below PosY. If optional text is wider than the
    ''' barcode it will be extended to the left and right sides
    ''' of the barcode.
    ''' </para>
    ''' <para>
    ''' The BarWidth argument is the width of the narrow bar.
    ''' </para>
    ''' <para>
    ''' The BarcodeHeight argument is the height of the barcode 
    ''' excluding optional text.
    ''' </para>
    ''' <para>
    ''' Set Barcode to one of the derived classes. 
    ''' This library supports: Barcode128, Barcode39 and BarcodeEAN13.
    ''' Note BarcodeEAN13 supports Barcode UPC-A.
    ''' </para>
    ''' <para>
    ''' Barcode text is optional. If TextFont and FontSize are omitted 
    ''' no text will be drawn under the barcode. If TextFont and
    ''' FontSize are specified the barcode text will be displayed
    ''' under the barcode. It will be horizontally centered in relation
    ''' to the barcode.
    ''' </para>
    ''' <para>
    ''' Barcode text is displayed below PosY. Make sure to leave
    ''' space under the barcode.
    ''' </para>
    ''' <para>
    ''' If color other than black is given make sure there is
    ''' a good contrast to white.
    ''' </para>
    ''' </remarks>
    Public Function DrawBarcode(PosX As Double, PosY As Double, Justify As TextJustify, BarWidth As Double, BarHeight As Double, BarColor As Color, Barcode As Barcode, Optional TextFont As PdfFont = Nothing, Optional FontSize As Double = 0.0) As Double
        ' save graphics state
        SaveGraphicsState()

        ' set barcode color
        SetColorNonStroking(BarColor)

        ' barcode width
        Dim TotalWidth = BarWidth * Barcode.TotalWidth

        ' switch for adjustment
        Select Case Justify
            Case TextJustify.Center
                PosX -= 0.5 * TotalWidth
            Case TextJustify.Right
                PosX -= TotalWidth
        End Select

        ' initial bar position
        Dim BarPosX = PosX

        ' initialize bar color to black
        Dim Bar = True

        ' all barcodes except EAN-13 or UPC-A
        If Barcode.GetType() IsNot GetType(BarcodeEAN13) Then
            ' loop for all bars
            For Index = 0 To Barcode.BarCount - 1
                ' bar width in user units
                Dim Width = BarWidth * Barcode.BarWidth(Index)

                ' draw black bars
                If Bar Then DrawRectangle(BarPosX, PosY, Width, BarHeight, PaintOp.Fill)

                ' update bar position and color
                BarPosX += Width
                Bar = Not Bar
            Next

            ' display text if font is specified

            ' EAN-13 or UPC-A
            If TextFont IsNot Nothing Then
                DrawBarcodeText(TextFont, FontSize, PosX + 0.5 * TotalWidth, PosY, TextJustify.Center, Barcode.Text)
            End If
        Else
            ' loop for all bars
            For Index = 0 To Barcode.BarCount - 1
                ' bar width in user units
                Dim Width = BarWidth * Barcode.BarWidth(Index)

                ' adjust vertical position
                Dim DeltaY = If(Index < 7 OrElse Index >= 27 AndAlso Index < 32 OrElse Index >= 52, 0.0, 5 * BarWidth)

                ' draw black bars
                If Bar Then DrawRectangle(BarPosX, PosY + DeltaY, Width, BarHeight - DeltaY, PaintOp.Fill)

                ' update bar position and color
                BarPosX += Width
                Bar = Not Bar
            Next

            ' display text if font is specified
            If TextFont IsNot Nothing Then
                ' substrings positions
                Dim PosX1 = PosX - 2.0 * BarWidth
                Dim PosX2 = PosX + 27.5 * BarWidth
                Dim PosX3 = PosX + 67.5 * BarWidth
                Dim PosX4 = PosX + 97.0 * BarWidth
                Dim PosY1 = PosY + 5.0 * BarWidth

                ' UPC-A
                If Barcode.Text.Length = 12 Then
                    DrawBarcodeText(TextFont, FontSize, PosX1, PosY1, TextJustify.Right, Barcode.Text.Substring(0, 1))
                    DrawBarcodeText(TextFont, FontSize, PosX2, PosY1, TextJustify.Center, Barcode.Text.Substring(1, 5))
                    DrawBarcodeText(TextFont, FontSize, PosX3, PosY1, TextJustify.Center, Barcode.Text.Substring(6, 5))
                    ' EAN-13
                    DrawBarcodeText(TextFont, FontSize, PosX4, PosY1, TextJustify.Left, Barcode.Text.Substring(11))
                Else
                    DrawBarcodeText(TextFont, FontSize, PosX1, PosY1, TextJustify.Right, Barcode.Text.Substring(0, 1))
                    DrawBarcodeText(TextFont, FontSize, PosX2, PosY1, TextJustify.Center, Barcode.Text.Substring(1, 6))
                    DrawBarcodeText(TextFont, FontSize, PosX3, PosY1, TextJustify.Center, Barcode.Text.Substring(7))
                End If
            End If
        End If

        ' restore graphics state
        RestoreGraphicsState()

        ' return width
        Return TotalWidth
    End Function

    ''' <summary>
    ''' Draw barcode text
    ''' </summary>
    ''' <param name="Font"></param>
    ''' <param name="FontSize"></param>
    ''' <param name="CenterPos"></param>
    ''' <param name="TopPos"></param>
    ''' <param name="Justify"></param>
    ''' <param name="Text"></param>
    Private Sub DrawBarcodeText(Font As PdfFont, FontSize As Double, CenterPos As Double, TopPos As Double, Justify As TextJustify, Text As String)
        ' test for non printable characters
        Dim Index As Integer = 0

        While Index < Text.Length AndAlso Text(Index) >= " "c AndAlso Text(Index) <= "~"c
            Index += 1
        End While

        If Index < Text.Length Then
            Dim Str As New StringBuilder(Text)

            While Index < Text.Length
                If Str(Index) < " "c OrElse Str(Index) > "~"c Then
                    Str(Index) = " "c
                End If

                Index += 1
            End While

            Text = Str.ToString()
        End If

        ' draw the text
        DrawText(Font, FontSize, CenterPos, TopPos - Font.Ascent(FontSize), Justify, Text)
    End Sub

    ''' <summary>
    ''' Draw image (Height is calculated from width as per aspect ratio)
    ''' </summary>
    ''' <param name="Image">PdfImage resource</param>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="Width">Display width</param>
    ''' <remarks>
    ''' The chart will be stretched or shrunk to fit the display width
    ''' and display height. Use PdfImage.ImageSize(...) or 
    ''' PdfImage.ImageSizePosition(...) to ensure correct aspect ratio 
    ''' and positioning.
    ''' </remarks>
    Public Sub DrawImage(Image As PdfImage, OriginX As Double, OriginY As Double, Width As Double)
        ' add image code to current list of resources
        AddToUsedResources(Image)

        ' draw image
        MyBase.ObjectValueFormat("q {0} 0 0 {1} {2} {3} cm {4} Do Q" & Microsoft.VisualBasic.Constants.vbLf, ToPt(Width), ToPt(Width * Image.HeightPix / Image.WidthPix), ToPt(OriginX), ToPt(OriginY), Image.ResourceCode)
    End Sub

    ''' <summary>
    ''' Draw image
    ''' </summary>
    ''' <param name="Image">PdfImage resource</param>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="Width">Display width</param>
    ''' <param name="Height">Display height</param>
    ''' <remarks>
    ''' The chart will be stretched or shrunk to fit the display width
    ''' and display height. Use PdfImage.ImageSize(...) or 
    ''' PdfImage.ImageSizePosition(...) to ensure correct aspect ratio 
    ''' and positioning.
    ''' </remarks>
    Public Sub DrawImage(Image As PdfImage, OriginX As Double, OriginY As Double, Width As Double, Height As Double)
        ' add image code to current list of resources
        AddToUsedResources(Image)

        ' draw image
        MyBase.ObjectValueFormat("q {0} 0 0 {1} {2} {3} cm {4} Do Q" & Microsoft.VisualBasic.Constants.vbLf, ToPt(Width), ToPt(Height), ToPt(OriginX), ToPt(OriginY), Image.ResourceCode)
    End Sub

    ''' <summary>
    ''' Draw X Object
    ''' </summary>
    ''' <param name="XObject">X Object resource</param>
    ''' <remarks>
    ''' X object is displayed at current position. X object Size
    ''' is as per X object.
    ''' </remarks>
    Public Sub DrawXObject(XObject As PdfXObject)
        ' add image code to current list of resources
        AddToUsedResources(XObject)

        ' draw object
        MyBase.ObjectValueFormat("{0} Do" & Microsoft.VisualBasic.Constants.vbLf, XObject.ResourceCode)
    End Sub

    ''' <summary>
    ''' Draw X Object
    ''' </summary>
    ''' <param name="XObject">X Object resource</param>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <remarks>
    ''' X object Size is as per X object.
    ''' </remarks>
    Public Sub DrawXObject(XObject As PdfXObject, OriginX As Double, OriginY As Double)
        SaveGraphicsState()
        Translate(OriginX, OriginY)
        DrawXObject(XObject)
        RestoreGraphicsState()
    End Sub

    ''' <summary>
    ''' Draw X Object
    ''' </summary>
    ''' <param name="XObject">X Object resource</param>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="ScaleX">Horizontal scale factor</param>
    ''' <param name="ScaleY">Vertical scale factor</param>
    Public Sub DrawXObject(XObject As PdfXObject, OriginX As Double, OriginY As Double, ScaleX As Double, ScaleY As Double)
        SaveGraphicsState()
        TranslateScale(OriginX, OriginY, ScaleX, ScaleY)
        DrawXObject(XObject)
        RestoreGraphicsState()
    End Sub

    ''' <summary>
    ''' Draw X Object
    ''' </summary>
    ''' <param name="XObject">X Object resource</param>
    ''' <param name="OriginX">Origin X</param>
    ''' <param name="OriginY">Origin Y</param>
    ''' <param name="ScaleX">Horizontal scale factor</param>
    ''' <param name="ScaleY">Vertical scale factor</param>
    ''' <param name="Alpha">Rotation angle</param>
    Public Sub DrawXObject(XObject As PdfXObject, OriginX As Double, OriginY As Double, ScaleX As Double, ScaleY As Double, Alpha As Double)
        SaveGraphicsState()
        TranslateScaleRotate(OriginX, OriginY, ScaleX, ScaleY, Alpha)
        DrawXObject(XObject)
        RestoreGraphicsState()
    End Sub

    ''' <summary>
    ''' Add resource to list of used resources
    ''' </summary>
    ''' <param name="ResObject"></param>
    Friend Sub AddToUsedResources(ResObject As PdfObject)
        If ResObjects Is Nothing Then ResObjects = New List(Of PdfObject)()
        Dim Index = ResObjects.BinarySearch(ResObject)
        If Index < 0 Then ResObjects.Insert(Not Index, ResObject)
        Return
    End Sub

    ''' <summary>
    ''' Commit object to PDF file
    ''' </summary>
    ''' <param name="GCCollect">Activate Garbage Collector</param>
    Public Sub CommitToPdfFile(GCCollect As Boolean)
        ' make sure object was not written before
        If FilePosition = 0 Then
            ' call PdfObject routine
            WriteObjectToPdfFile()

            ' activate garbage collector
            If GCCollect Then GC.Collect()
        End If
    End Sub

    ''' <summary>
    ''' Write object to PDF file
    ''' </summary>
    Friend Overrides Sub WriteObjectToPdfFile()
        ' build resource dictionary for non page contents
        If Not PageContents Then Dictionary.Add("/Resources", BuildResourcesDictionary(ResObjects, False))

        ' call PdfObject routine
        MyBase.WriteObjectToPdfFile()
    End Sub

    ''' <summary>
    ''' Set paint operator
    ''' </summary>
    ''' <param name="PP">Paint operator from custom string</param>
    Public Sub SetPaintOp(PP As String)
        ' apply paint operator
        MyBase.ObjectValueFormat("{0}" & Microsoft.VisualBasic.Constants.vbLf, PP)
    End Sub
End Class
