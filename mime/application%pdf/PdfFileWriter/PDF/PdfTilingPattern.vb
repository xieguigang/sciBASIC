#Region "Microsoft.VisualBasic::22ae3ec7c1e309f80f981dbc69216a49, mime\application%pdf\PdfFileWriter\PDF\PdfTilingPattern.vb"

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

    '   Total Lines: 307
    '    Code Lines: 91
    ' Comment Lines: 177
    '   Blank Lines: 39
    '     File Size: 12.08 KB


    '     Enum TilingType
    ' 
    '         ConstantAndFast, NoDistortion
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class PdfTilingPattern
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: SetBrickPattern, SetWeavePattern
    ' 
    '         Sub: SetPatternMatrix, (+2 Overloads) SetScale, SetScaleAndOrigin, (+3 Overloads) SetTileBox, SetTilingType
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfTilingPattern
'	PDF tiling pattern resource class.
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


    ''' <summary>
    ''' PDF tiling type enumeration
    ''' </summary>
    Public Enum TilingType
        ''' <summary>
        ''' Constant
        ''' </summary>
        Constant = 1

        ''' <summary>
        ''' No distortion
        ''' </summary>
        NoDistortion

        ''' <summary>
        ''' Constant and fast
        ''' </summary>
        ConstantAndFast
    End Enum

    ''' <summary>
    ''' PDF tiling pattern resource class
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#TilingPattern">For example of using tiling pattern see 3.3. Tiling Pattern</a>
    ''' </para>
    ''' <para>
    ''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#TilingPattern2">or 3.8. Draw Rectangle with Rounded Corners and Filled with Brick Pattern</a>
    ''' </para>
    ''' </remarks>
    Public Class PdfTilingPattern
        Inherits PdfContents
        
        ''' <summary>
        ''' PDF Tiling pattern constructor.
        ''' </summary>
        ''' <param name="Document">Document object parent of the object.</param>
        ''' <remarks>
        ''' This program support only color tiling pattern: PaintType = 1.
        ''' </remarks>
        
        Public Sub New(Document As PdfDocument)
            MyBase.New(Document, "/Pattern")
            ' create resource code
            ResourceCode = Document.GenerateResourceNumber("P"c)

            ' add items to dictionary
            Dictionary.Add("/PatternType", "1")     ' Tiling pattern
            Dictionary.Add("/PaintType", "1")           ' color
            Dictionary.Add("/TilingType", "1")      ' constant
            Dictionary.AddFormat("/BBox", "[0 0 {0} {1}]", ToPt(1.0), ToPt(1.0))
            Dictionary.AddReal("/XStep", ToPt(1.0))
            Dictionary.AddReal("/YStep", ToPt(1.0))
            Return
        End Sub

        
        ''' <summary>
        ''' Set tiling type
        ''' </summary>
        ''' <param name="TilingType">Tiling type</param>
        
        Public Sub SetTilingType(TilingType As TilingType)
            ' by default the constructor set tiling type to 1 = constant
            Dictionary.AddInteger("/TilingType", TilingType)
            Return
        End Sub

        
        ''' <summary>
        ''' Set tile box
        ''' </summary>
        ''' <param name="Side">Length of one side.</param>
        ''' <remarks>
        ''' Set square bounding box and equal step
        ''' </remarks>
        
        Public Sub SetTileBox(Side As Double)
            SetTileBox(Side, Side, Side, Side)
            Return
        End Sub

        
        ''' <summary>
        ''' Set tile box
        ''' </summary>
        ''' <param name="Width">Box width.</param>
        ''' <param name="Height">Box height.</param>
        ''' <remarks>
        ''' Set rectangle bounding box and equal step.
        ''' </remarks>
        
        Public Sub SetTileBox(Width As Double, Height As Double)
            SetTileBox(Width, Height, Width, Height)
            Return
        End Sub

        
        ''' <summary>
        ''' Set bounding box and step 
        ''' </summary>
        ''' <param name="Width">Box width.</param>
        ''' <param name="Height">Box height.</param>
        ''' <param name="StepX">Horizontal step</param>
        ''' <param name="StepY">Vertical step</param>
        ''' <remarks>
        ''' Set rectangle bounding box and independent step size.
        ''' </remarks>
        
        Public Sub SetTileBox(Width As Double, Height As Double, StepX As Double, StepY As Double)
            ' by default XStep == Width
            Dictionary.AddFormat("/BBox", "[0 0 {0} {1}]", ToPt(Width), ToPt(Height))
            Dictionary.AddReal("/XStep", ToPt(StepX))
            Dictionary.AddReal("/YStep", ToPt(StepY))
            Return
        End Sub

        
        ''' <summary>
        ''' Set scale
        ''' </summary>
        ''' <param name="Scale">Scale factor.</param>
        ''' <remarks>
        ''' Warning: the program replaces the transformation matrix
        ''' with a new one [Scale 0 0 Scale 0 0].
        ''' </remarks>
        
        Public Sub SetScale(Scale As Double)
            ' add items to dictionary
            Dictionary.AddFormat("/Matrix", "[{0} 0 0 {0} 0 0]", Round(Scale))
            Return
        End Sub

        
        ''' <summary>
        ''' Set scale
        ''' </summary>
        ''' <param name="ScaleX">Horizontal scale factor.</param>
        ''' <param name="ScaleY">Vertical scale factor.</param>
        ''' <remarks>
        ''' Warning: the program replaces the transformation matrix
        ''' with a new one [ScaleX 0 0 ScaleY 0 0].
        ''' </remarks>
        
        Public Sub SetScale(ScaleX As Double, ScaleY As Double)
            ' add items to dictionary
            Dictionary.AddFormat("/Matrix", "[{0} 0 0 {1} 0 0]", Round(ScaleX), Round(ScaleY))
            Return
        End Sub

        
        ''' <summary>
        ''' Set scale and origin
        ''' </summary>
        ''' <param name="OriginX">Origin X</param>
        ''' <param name="OriginY">Origin Y</param>
        ''' <param name="ScaleX">Scale X</param>
        ''' <param name="ScaleY">Scale Y</param>
        ''' <remarks>
        ''' Warning: the program replaces the transformation matrix
        ''' with a new one [ScaleX 0 0 ScaleY OriginX OriginY].
        ''' </remarks>
        
        Public Sub SetScaleAndOrigin(OriginX As Double, OriginY As Double, ScaleX As Double, ScaleY As Double)
            ' add items to dictionary
            Dictionary.AddFormat("/Matrix", "[{0} 0 0 {1} {2} {3}]", Round(ScaleX), Round(ScaleY), ToPt(OriginX), ToPt(OriginY))
            Return
        End Sub

        
        ''' <summary>
        ''' Set pattern transformation matrix
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
        
        Public Sub SetPatternMatrix(a As Double, b As Double, c As Double, d As Double, e As Double, f As Double)
            ' create full pattern transformation matrix
            Dictionary.AddFormat("/Matrix", "[{0} {1} {2} {3} {4} {5}]", Round(a), Round(b), Round(c), Round(d), ToPt(e), ToPt(f))
            Return
        End Sub

         
        ''' <summary>
        ''' Create new PdfTilingPattern class with brick pattern.
        ''' </summary>
        ''' <param name="Document">Current document object.</param>
        ''' <param name="Scale">Scale factor.</param>
        ''' <param name="Stroking">Stroking color.</param>
        ''' <param name="NonStroking">Non-stroking color.</param>
        ''' <returns>PDF tiling pattern</returns>
        ''' <remarks>
        ''' <para>
        ''' The pattern is a square with one user unit side.
        ''' </para>
        ''' <para>
        ''' The bottom half is one brick. The top half is two half bricks.
        ''' </para>
        ''' <para>
        ''' Arguments:
        ''' </para>
        ''' <para>
        ''' Scale the pattern to your requirements.
        ''' </para>
        ''' <para>
        ''' Stroking color is the mortar color.
        ''' </para>
        ''' <para>
        ''' Nonstroking color is the brick color.
        ''' </para>
        ''' </remarks>
        
        Public Shared Function SetBrickPattern(Document As PdfDocument, Scale As Double, Stroking As Color, NonStroking As Color) As PdfTilingPattern
            Dim Pattern As PdfTilingPattern = New PdfTilingPattern(Document)
            Pattern.SetScale(Scale)
            Pattern.SaveGraphicsState()
            Pattern.SetLineWidth(0.05)
            Pattern.SetColorStroking(Stroking)
            Pattern.SetColorNonStroking(NonStroking)
            Pattern.DrawRectangle(0.025, 0.025, 0.95, 0.45, PaintOp.CloseFillStroke)
            Pattern.DrawRectangle(-0.475, 0.525, 0.95, 0.45, PaintOp.CloseFillStroke)
            Pattern.DrawRectangle(0.525, 0.525, 0.95, 0.45, PaintOp.CloseFillStroke)
            Pattern.RestoreGraphicsState()
            Return Pattern
        End Function

        
        ''' <summary>
        ''' Create new PdfTilingPattern class with weave pattern.
        ''' </summary>
        ''' <param name="Document">Current PDF document.</param>
        ''' <param name="Scale">Scale factor</param>
        ''' <param name="Background">Background color.</param>
        ''' <param name="Horizontal">Horizontal line color.</param>
        ''' <param name="Vertical">Vertical line color.</param>
        ''' <returns>PDF tiling pattern</returns>
        ''' <remarks>
        ''' <para>
        ''' The pattern in a square with one user unit side.
        ''' </para>
        ''' <para>
        ''' It is made of horizontal and vertical rectangles.
        ''' </para>
        ''' </remarks>
        
        Public Shared Function SetWeavePattern(Document As PdfDocument, Scale As Double, Background As Color, Horizontal As Color, Vertical As Color) As PdfTilingPattern
            Const RectSide1 = 4.0 / 6.0
            Const RectSide2 = 2.0 / 6.0
            Const LineWidth = 0.2 / 6.0
            Const HalfWidth = 0.5 * LineWidth
            Dim Pattern As PdfTilingPattern = New PdfTilingPattern(Document)
            Pattern.SetScale(Scale)
            Pattern.SaveGraphicsState()
            Pattern.SetTileBox(1.0)
            Pattern.SetColorNonStroking(Background)
            Pattern.DrawRectangle(0.0, 0.0, 1.0, 1.0, PaintOp.Fill)
            Pattern.SetLineWidth(LineWidth)
            Pattern.SetColorStroking(Background)
            Pattern.SetColorNonStroking(Horizontal)
            Pattern.DrawRectangle(HalfWidth, 1.0 / 6.0 + HalfWidth, RectSide1 - LineWidth, RectSide2 - LineWidth, PaintOp.CloseFillStroke)
            Pattern.DrawRectangle(-(3.0 / 6.0 - HalfWidth), 4.0 / 6.0 + HalfWidth, RectSide1 - LineWidth, RectSide2 - LineWidth, PaintOp.CloseFillStroke)
            Pattern.DrawRectangle(3.0 / 6.0 + HalfWidth, 4.0 / 6.0 + HalfWidth, RectSide1 - LineWidth, RectSide2 - LineWidth, PaintOp.CloseFillStroke)
            Pattern.SetColorNonStroking(Vertical)
            Pattern.DrawRectangle(4.0 / 6.0 + HalfWidth, HalfWidth, RectSide2 - LineWidth, RectSide1 - LineWidth, PaintOp.CloseFillStroke)
            Pattern.DrawRectangle(1.0 / 6.0 + HalfWidth, -(3.0 / 6.0 - HalfWidth), RectSide2 - LineWidth, RectSide1 - LineWidth, PaintOp.CloseFillStroke)
            Pattern.DrawRectangle(1.0 / 6.0 + HalfWidth, 3.0 / 6.0 + HalfWidth, RectSide2 - LineWidth, RectSide1 - LineWidth, PaintOp.CloseFillStroke)
            Pattern.RestoreGraphicsState()
            Return Pattern
        End Function
    End Class
