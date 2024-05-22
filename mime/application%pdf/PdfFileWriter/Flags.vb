#Region "Microsoft.VisualBasic::a8e417f9a11fd7f90ef6d2a5c9a438b5, mime\application%pdf\PdfFileWriter\Flags.vb"

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

    '   Total Lines: 314
    '    Code Lines: 72 (22.93%)
    ' Comment Lines: 200 (63.69%)
    '    - Xml Docs: 99.50%
    ' 
    '   Blank Lines: 42 (13.38%)
    '     File Size: 5.48 KB


    ' Enum DrawStyle
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum PaintOp
    ' 
    '     NoOperator
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum PdfLineCap
    ' 
    '     Butt, Round, Square
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum PdfLineJoin
    ' 
    '     Bevel, Miter, Round
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum TextRendering
    ' 
    '     Clip, Fill, FillClip, FillStroke, FillStrokeClip
    '     Invisible, Stroke, StrokeClip
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum TextJustify
    ' 
    '     Center, Left, Right
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum TextBoxJustify
    ' 
    '     Center, FitToWidth, Left, Right
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum BezierPointOne
    ' 
    '     Ignore, LineTo, MoveTo
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum BlendMode
    ' 
    '     ColorBurn, ColorDodge, Darken, Difference, Exclusion
    '     HardLight, Lighten, Multiply, Normal, Overlay
    '     Screen, SoftLight
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' PDF font style flags enumeration
''' </summary>
Public Enum DrawStyle
    ''' <summary>
    ''' Normal
    ''' </summary>
    Normal = 0

    ''' <summary>
    ''' Underline
    ''' </summary>
    Underline = 4

    ''' <summary>
    ''' Strikeout
    ''' </summary>
    Strikeout = 8

    ''' <summary>
    ''' Subscript
    ''' </summary>
    Subscript = 16

    ''' <summary>
    ''' Superscript
    ''' </summary>
    Superscript = 32
End Enum

''' <summary>
''' Path painting and clipping operators enumeration
''' </summary>
''' <remarks>
''' <para>
''' Note Special path paining considerations in section 4.4
''' of the PDF specifications. EOR is even odd rule. Otherwise
''' it is nonzero winding number rule.
''' </para>
''' </remarks>
Public Enum PaintOp
    ''' <summary>
    ''' No operator
    ''' </summary>
    NoOperator

    ''' <summary>
    ''' No paint
    ''' </summary>
    NoPaint         ' n

    ''' <summary>
    ''' Stoke
    ''' </summary>
    Stroke              ' S

    ''' <summary>
    ''' Close and stroke
    ''' </summary>
    CloseStroke     ' s

    ''' <summary>
    ''' close and Fill
    ''' </summary>
    Fill                ' f

    ''' <summary>
    ''' close and fill EOR
    ''' </summary>
    FillEor         ' f*

    ''' <summary>
    ''' Fill and stoke
    ''' </summary>
    FillStroke          ' B

    ''' <summary>
    ''' Fill and stroke EOR
    ''' </summary>
    FillStrokeEor       ' B*

    ''' <summary>
    ''' Close, Fill and stroke
    ''' </summary>
    CloseFillStroke ' b

    ''' <summary>
    ''' Close, Fill and Stroke EOR
    ''' </summary>
    CloseFillStrokeEor  ' b*

    ''' <summary>
    ''' Clip path
    ''' </summary>
    ClipPathWnr     ' h W n

    ''' <summary>
    ''' Clip path EOR
    ''' </summary>
    ClipPathEor     ' h W* n

    ''' <summary>
    ''' Close sub-path
    ''' </summary>
    CloseSubPath        ' h
End Enum

''' <summary>
''' PDF line cap enumeration
''' </summary>
Public Enum PdfLineCap
    ''' <summary>
    ''' Butt
    ''' </summary>
    Butt

    ''' <summary>
    ''' Round
    ''' </summary>
    Round

    ''' <summary>
    ''' Square
    ''' </summary>
    Square
End Enum

''' <summary>
''' PDF line join enumeration
''' </summary>
Public Enum PdfLineJoin
    ''' <summary>
    ''' Miter
    ''' </summary>
    Miter

    ''' <summary>
    ''' Round
    ''' </summary>
    Round

    ''' <summary>
    ''' Bevel
    ''' </summary>
    Bevel
End Enum

''' <summary>
''' Text rendering enumeration
''' </summary>
Public Enum TextRendering
    ''' <summary>
    ''' Fill
    ''' </summary>
    Fill

    ''' <summary>
    ''' Stroke
    ''' </summary>
    Stroke

    ''' <summary>
    ''' Fill and stroke
    ''' </summary>
    FillStroke

    ''' <summary>
    ''' Invisible
    ''' </summary>
    Invisible

    ''' <summary>
    ''' Fill and clip
    ''' </summary>
    FillClip

    ''' <summary>
    ''' Stroke and clip
    ''' </summary>
    StrokeClip

    ''' <summary>
    ''' Fill, stroke and clip
    ''' </summary>
    FillStrokeClip

    ''' <summary>
    ''' Clip
    ''' </summary>
    Clip
End Enum

''' <summary>
''' Text justify enumeration
''' </summary>
Public Enum TextJustify
    ''' <summary>
    ''' Left
    ''' </summary>
    Left

    ''' <summary>
    ''' Center
    ''' </summary>
    Center

    ''' <summary>
    ''' Right
    ''' </summary>
    Right
End Enum

' text justify
''' <summary>
''' TextBox justify enumeration
''' </summary>
''' <remarks>The first three must be the same as TextJustify
''' </remarks>
Public Enum TextBoxJustify
    ''' <summary>
    ''' Left
    ''' </summary>
    Left

    ''' <summary>
    ''' Center
    ''' </summary>
    Center

    ''' <summary>
    ''' Right
    ''' </summary>
    Right

    ''' <summary>
    ''' Fit to width
    ''' </summary>
    FitToWidth
End Enum

''' <summary>
''' Draw Bezier point one control enumeration
''' </summary>
Public Enum BezierPointOne
    ''' <summary>
    ''' Ignore
    ''' </summary>
    Ignore

    ''' <summary>
    ''' Move to
    ''' </summary>
    MoveTo

    ''' <summary>
    ''' Line to
    ''' </summary>
    LineTo
End Enum

''' <summary>
''' Blend mode enumeration
''' </summary>
''' <remarks>See Blend Mode section of the PDF specifications menual.</remarks>
Public Enum BlendMode
    ''' <summary>
    ''' Normal (no blend)
    ''' </summary>
    Normal
    ''' <summary>
    ''' Multiply
    ''' </summary>
    Multiply
    ''' <summary>
    ''' Screen
    ''' </summary>
    Screen
    ''' <summary>
    ''' Overlay
    ''' </summary>
    Overlay
    ''' <summary>
    ''' Darken
    ''' </summary>
    Darken
    ''' <summary>
    ''' Lighten
    ''' </summary>
    Lighten
    ''' <summary>
    ''' Color Dodge
    ''' </summary>
    ColorDodge
    ''' <summary>
    ''' Color burn
    ''' </summary>
    ColorBurn
    ''' <summary>
    ''' Hard light
    ''' </summary>
    HardLight
    ''' <summary>
    ''' Soft light
    ''' </summary>
    SoftLight
    ''' <summary>
    ''' Difference
    ''' </summary>
    Difference
    ''' <summary>
    ''' Exclusion
    ''' </summary>
    Exclusion
End Enum
