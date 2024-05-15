#Region "Microsoft.VisualBasic::9aca15337758225566a100dddef3957e, mime\application%pdf\PdfFileWriter\PDF\PdfFont.vb"

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

    '   Total Lines: 1430
    '    Code Lines: 649
    ' Comment Lines: 454
    '   Blank Lines: 327
    '     File Size: 52.57 KB


    ' Enum PdfFontFlags
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class KerningAdjust
    ' 
    '     Properties: Adjust, Text
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class PdfFont
    ' 
    '     Properties: PdfLeading
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: Ascent, AscentPlusLeading, CapHeight, CharBoundingBox, CharCodeFontDescriptor
    '               CharCodeFontWidthArray, (+2 Overloads) CharWidth, CompareTo, CreatePdfFont, Descent
    '               DescentPlusLeading, FontDescriptorCommon, FontDesignToPdfUnits, FontDesignToUserUnits, GetCharInfo
    '               GlyphIndexFontDescriptor, GlyphIndexToUnicode, GlyphIndexWidthArray, LineSpacing, StemV
    '               StrikeoutPosition, StrikeoutWidth, SubscriptPosition, SubscriptSize, SuperscriptPosition
    '               SuperscriptSize, TextBoundingBox, TextFitToWidth, TextKerning, TextKerningWidth
    '               TextWidth, UnderlinePosition, UnderlineWidth
    ' 
    '     Sub: CharCodeToPdfFile, CreateGlyphIndexFont, Dispose, GlyphIndexToPdfFile, WriteObjectToPdfFile
    ' 
    ' Class UnicodeRange
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CompareTo
    ' 
    ' Class GlyphWidth
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CompareTo
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfFont
'	PDF Font resource.
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

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Linq
Imports stdNum = System.Math

''' <summary>
''' PDF font descriptor flags enumeration
''' </summary>
Public Enum PdfFontFlags
    ''' <summary>
    ''' None
    ''' </summary>
    None = 0

    ''' <summary>
    ''' Fixed pitch font
    ''' </summary>
    FixedPitch = 1

    ''' <summary>
    ''' Serif font
    ''' </summary>
    Serif = 1 << 1

    ''' <summary>
    ''' Symbolic font
    ''' </summary>
    Symbolic = 1 << 2

    ''' <summary>
    ''' Script font
    ''' </summary>
    Script = 1 << 3

    ''' <summary>
    ''' Non-symbolic font
    ''' </summary>
    Nonsymbolic = 1 << 5

    ''' <summary>
    ''' Italic font
    ''' </summary>
    Italic = 1 << 6

    ''' <summary>
    ''' All cap font
    ''' </summary>
    AllCap = 1 << 16

    ''' <summary>
    ''' Small cap font
    ''' </summary>
    SmallCap = 1 << 17

    ''' <summary>
    ''' Force bold font
    ''' </summary>
    ForceBold = 1 << 18
End Enum

''' <summary>
''' Kerning adjustment class
''' </summary>
''' <remarks>
''' Text position adjustment for TJ operator.
''' The adjustment is for a font height of one point.
''' Mainly used for font kerning.
''' </remarks>
Public Class KerningAdjust
    ''' <summary>
    ''' Gets or sets Text
    ''' </summary>
    Public Property Text As String

    ''' <summary>
    ''' Gets or sets adjustment
    ''' </summary>
    ''' <remarks>
    ''' Adjustment units are in PDF design unit. To convert to user units: Adjust * FontSize / (1000.0 * ScaleFactor)
    ''' </remarks>
    Public Property Adjust As Double

    ''' <summary>
    ''' Kerning adjustment constructor
    ''' </summary>
    ''' <param name="Text">Text</param>
    ''' <param name="Adjust">Adjustment</param>
    Public Sub New(Text As String, Adjust As Double)
        Me.Text = Text
        Me.Adjust = Adjust
        Return
    End Sub
End Class

''' <summary>
''' PDF font class
''' </summary>
''' <remarks>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#LanguageSupport">2.3 Language Support</a>
''' </para>
''' <para>
''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#FontResources">For example of defining font resources see 3.2. Font Resources</a>
''' </para>
''' </remarks>
Public Class PdfFont
    Inherits PdfObject
    Implements IDisposable, IComparable(Of PdfFont)

    Friend FontApi As FontApi
    Friend SymbolicFont As Boolean
    Friend CharInfoArray As CharInfo()()
    Friend CharInfoBlockEmpty As Boolean()
    Friend UndefinedCharInfo As CharInfo
    Friend NewGlyphIndex As Integer
    Friend GlyphIndexFont As PdfObject
    Friend ResourceCodeGlyph As String      ' resource code automatically generated by the program
    Friend FontResCodeUsed As Boolean       ' 0-255
    Friend FontResGlyphUsed As Boolean      ' 255-0xffff
    Friend FontFamily As FontFamily
    Friend FontFamilyName As String
    Friend FontStyle As FontStyle
    Friend EmbeddedFont As Boolean
    Friend DesignFont As Font
    Friend FontFlags As PdfFontFlags
    Friend PdfLineSpacing As Integer
    Friend PdfAscent As Integer
    Friend PdfDescent As Integer

    Friend ReadOnly Property PdfLeading As Integer
        Get
            Return PdfLineSpacing - PdfAscent - PdfDescent
        End Get
    End Property

    Friend DesignCapHeight As Integer
    Friend DesignStrikeoutWidth As Integer
    Friend DesignStrikeoutPosition As Integer
    Friend DesignUnderlineWidth As Integer
    Friend DesignUnderlinePosition As Integer
    Friend DesignSubscriptSize As Integer
    Friend DesignSubscriptPosition As Integer
    Friend DesignSuperscriptSize As Integer
    Friend DesignSuperscriptPosition As Integer
    Friend DesignItalicAngle As Integer
    Friend DesignFontWeight As Integer
    Friend DesignHeight As Integer

    ''' <summary>
    ''' PDF Font resource constructor
    ''' </summary>
    ''' <param name="Document">Document object</param>
    ''' <param name="FontFamilyName">Font family name</param>
    ''' <param name="FontStyle">Font style</param>
    ''' <param name="EmbeddedFont">Embedded font</param>
    ''' <returns>PdfFont resource</returns>
    ''' <remarks>The returned result is either a new PdfFont or an
    ''' existing one with the same properties.</remarks>
    Public Shared Function CreatePdfFont(Document As PdfDocument, FontFamilyName As String, FontStyle As FontStyle, Optional EmbeddedFont As Boolean = True) As PdfFont     ' PDF document main object
        ' font family name
        ' font style (Regular, Bold, Italic or Bold | Italic
        ' embed font in PDF document file
        If Document.FontArray Is Nothing Then Document.FontArray = New List(Of PdfFont)()
        Dim Index As Integer = Document.FontArray.BinarySearch(New PdfFont(FontFamilyName, FontStyle, EmbeddedFont))
        If Index >= 0 Then Return Document.FontArray(Index)
        Dim NewFont As PdfFont = New PdfFont(Document, FontFamilyName, FontStyle, EmbeddedFont)
        Document.FontArray.Insert(Not Index, NewFont)
        Return NewFont
    End Function

    ' for search only
    Private Sub New(FontFamilyName As String, FontStyle As FontStyle, Optional EmbeddedFont As Boolean = True)        ' font family name
        ' font style (Regular, Bold, Italic or Bold | Italic
        ' embed font in PDF document file
        ' save parameters
        Me.FontFamilyName = FontFamilyName
        Me.FontStyle = FontStyle
        Me.EmbeddedFont = EmbeddedFont
        Return
    End Sub

    Private Sub New(Document As PdfDocument, FontFamilyName As String, FontStyle As FontStyle, Optional EmbeddedFont As Boolean = True)     ' PDF document main object
        ' font family name
        ' font style (Regular, Bold, Italic or Bold | Italic
        ' embed font in PDF document file
        MyBase.New(Document, ObjectType.Dictionary, "/Font")
        ' save parameters
        Me.FontFamilyName = FontFamilyName
        Me.FontStyle = FontStyle
        Me.EmbeddedFont = EmbeddedFont

        ' font style cannot be underline or strikeout
        If (FontStyle And (FontStyle.Underline Or FontStyle.Strikeout)) <> 0 Then Throw New ApplicationException("Font resource cannot have underline or strikeout")

        ' create two resource codes
        ResourceCode = Document.GenerateResourceNumber("F"c)
        ResourceCodeGlyph = Document.GenerateResourceNumber("F"c)

        ' initialize new glyph index to 3
        NewGlyphIndex = 3

        ' get font family structure
        FontFamily = New FontFamily(FontFamilyName)

        ' test font style availability
        If Not FontFamily.IsStyleAvailable(FontStyle) Then Throw New ApplicationException("Font style not available for font family")

        ' design height
        DesignHeight = FontFamily.GetEmHeight(FontStyle)

        ' Ascent, descent and line spacing for a one point font
        PdfAscent = FontFamily.GetCellAscent(FontStyle)
        PdfDescent = FontFamily.GetCellDescent(FontStyle) ' positive number
        PdfLineSpacing = FontFamily.GetLineSpacing(FontStyle)

        ' create design font
        DesignFont = New Font(FontFamily, DesignHeight, FontStyle, GraphicsUnit.Pixel)

        ' create windows sdk font info object
        FontApi = New FontApi(DesignFont, DesignHeight)

        ' create empty array of character information
        CharInfoArray = New CharInfo(255)() {}
        CharInfoBlockEmpty = New Boolean(255) {}

        ' get undefined character info
        UndefinedCharInfo = FontApi.GetGlyphMetricsApiByGlyphIndex(0)
        UndefinedCharInfo.NewGlyphIndex = 0

        ' get outline text metrics structure
        Dim OTM As WinOutlineTextMetric = FontApi.GetOutlineTextMetricsApi()

        ' make sure we have true type font and not device font
        If (OTM.otmTextMetric.tmPitchAndFamily And &HE) <> 6 Then Throw New ApplicationException("Font must be True Type and vector")

        ' PDF font flags
        FontFlags = 0
        If (OTM.otmfsSelection And 1) <> 0 Then FontFlags = FontFlags Or PdfFontFlags.Italic
        ' roman font is a serif font
        If OTM.otmTextMetric.tmPitchAndFamily >> 4 = 1 Then FontFlags = FontFlags Or PdfFontFlags.Serif
        If OTM.otmTextMetric.tmPitchAndFamily >> 4 = 4 Then FontFlags = FontFlags Or PdfFontFlags.Script
        ' #define SYMBOL_CHARSET 2
        If OTM.otmTextMetric.tmCharSet = 2 Then
            FontFlags = FontFlags Or PdfFontFlags.Symbolic
            SymbolicFont = True
        Else
            FontFlags = FontFlags Or PdfFontFlags.Nonsymbolic
            SymbolicFont = False
        End If

        ' #define TMPF_FIXED_PITCH 0x01 (Note very carefully that those meanings are the opposite of what the constant name implies.)
        If (OTM.otmTextMetric.tmPitchAndFamily And 1) = 0 Then FontFlags = FontFlags Or PdfFontFlags.FixedPitch

        ' strikeout
        DesignStrikeoutPosition = OTM.otmsStrikeoutPosition
        DesignStrikeoutWidth = CInt(OTM.otmsStrikeoutSize)

        ' underline
        DesignUnderlinePosition = OTM.otmsUnderscorePosition
        DesignUnderlineWidth = OTM.otmsUnderscoreSize

        ' subscript
        DesignSubscriptSize = OTM.otmptSubscriptSize.Y
        DesignSubscriptPosition = OTM.otmptSubscriptOffset.Y

        ' superscript
        DesignSuperscriptSize = OTM.otmptSuperscriptSize.Y
        DesignSuperscriptPosition = OTM.otmptSuperscriptOffset.Y

        ' italic angle is 10th of a degree
        DesignItalicAngle = OTM.otmItalicAngle
        DesignFontWeight = OTM.otmTextMetric.tmWeight
        DesignCapHeight = FontApi.GetGlyphMetricsApiByCode(Microsoft.VisualBasic.AscW("M"c)).DesignBBoxTop

        ' exit
        Return
    End Sub

    
    ' Create glyph index font object on first usage
    

    Friend Sub CreateGlyphIndexFont()
        GlyphIndexFont = New PdfObject(Document, ObjectType.Dictionary, "/Font")
        FontResGlyphUsed = True
        Return
    End Sub

    ''' <summary>
    ''' Get character information
    ''' </summary>
    ''' <param name="CharValue">Character value</param>
    ''' <returns>Character information class</returns>
    Public Function GetCharInfo(CharValue As Integer) As CharInfo
        ' no support for control characters 
        If CharValue < Asc(" "c) OrElse CharValue > Asc("~"c) AndAlso CharValue < 160 OrElse CharValue > &HFFFF Then
            Throw New ApplicationException("No support for control characters 0-31 or 127-159")
        End If

        ' split input character
        Dim RowIndex = CharValue >> 8
        Dim ColIndex = CharValue And 255

        ' define row if required
        If CharInfoArray(RowIndex) Is Nothing Then
            ' we know that this block is empty
            If CharInfoBlockEmpty(RowIndex) Then Return UndefinedCharInfo

            ' get block array
            Dim Block = FontApi.GetGlyphMetricsApi(CharValue)

            If Block Is Nothing Then
                CharInfoBlockEmpty(RowIndex) = True
                Return UndefinedCharInfo
            End If

            ' save block
            CharInfoArray(RowIndex) = Block
        End If

        ' get charater info
        Dim Info = CharInfoArray(RowIndex)(ColIndex)

        ' undefined
        If Info Is Nothing Then Return UndefinedCharInfo

        ' character info
        Return Info
    End Function

    
    ''' <summary>
    ''' Font units to user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Value">Design value</param>
    ''' <returns>Design value in user units</returns>
    
    Public Function FontDesignToUserUnits(FontSize As Double, Value As Integer) As Double
        Return Value * FontSize / (DesignHeight * ScaleFactor)
    End Function

    
    ''' <summary>
    ''' Font design units to PDF design units
    ''' </summary>
    ''' <param name="Value">Font design value</param>
    ''' <returns>PDF dictionary value</returns>
    
    Public Function FontDesignToPdfUnits(Value As Integer) As Double
        Return 1000.0 * Value / DesignHeight
    End Function

    
    ''' <summary>
    ''' Line spacing in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Line spacing</returns>
    
    Public Function LineSpacing(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, PdfLineSpacing)
    End Function

    ''' <summary>
    ''' Font ascent in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Font ascent</returns>
    Public Function Ascent(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, PdfAscent)
    End Function

    ''' <summary>
    ''' Font ascent in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Font ascent plus half of internal leading.</returns>
    Public Function AscentPlusLeading(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, PdfAscent + (PdfLeading + 1) / 2)
    End Function

    
    ''' <summary>
    ''' Font descent in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Font descent</returns>
    
    Public Function Descent(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, PdfDescent)
    End Function

    
    ''' <summary>
    ''' Font descent in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Font descent plus half of internal leading.</returns>
    
    Public Function DescentPlusLeading(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, PdfDescent + PdfLeading / 2)
    End Function

    
    ''' <summary>
    ''' Capital M height in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Capital M height</returns>
    
    Public Function CapHeight(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignCapHeight)
    End Function

    
    ''' <summary>
    ''' Strikeout position in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Strikeout position</returns>
    
    Public Function StrikeoutPosition(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignStrikeoutPosition)
    End Function

    
    ''' <summary>
    ''' Strikeout width in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Strikeout line width.</returns>
    
    Public Function StrikeoutWidth(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignStrikeoutWidth)
    End Function

    
    ''' <summary>
    ''' Underline position in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Underline position</returns>
    
    Public Function UnderlinePosition(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignUnderlinePosition)
    End Function

    
    ''' <summary>
    ''' Underline width in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Underline line width.</returns>
    
    Public Function UnderlineWidth(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignUnderlineWidth)
    End Function

    
    ''' <summary>
    ''' Subscript position in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Subscript position</returns>
    
    Public Function SubscriptPosition(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignSubscriptPosition)
    End Function

    
    ''' <summary>
    ''' Subscript character size in points
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Subscript font size</returns>
    
    Public Function SubscriptSize(FontSize As Double) As Double
        ' note: font size is in always points
        Return FontSize * DesignSubscriptSize / DesignHeight
    End Function

    
    ''' <summary>
    ''' Superscript character position
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Superscript position</returns>
    
    Public Function SuperscriptPosition(FontSize As Double) As Double
        Return FontDesignToUserUnits(FontSize, DesignSuperscriptPosition)
    End Function

    
    ''' <summary>
    ''' Superscript character size in points
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <returns>Superscript font size</returns>
    
    Public Function SuperscriptSize(FontSize As Double) As Double
        ' note: font size is in always points
        Return FontSize * DesignSuperscriptSize / DesignHeight
    End Function

    
    ''' <summary>
    ''' Character width in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="CharValue">Character code</param>
    ''' <returns>Character width</returns>
    
    Public Function CharWidth(FontSize As Double, CharValue As Char) As Double
        Return Me.FontDesignToUserUnits(FontSize, Me.GetCharInfo(Microsoft.VisualBasic.AscW(CharValue)).DesignWidth)
    End Function

    
    ''' <summary>
    ''' Character width in user units
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="DrawStyle">Draw style</param>
    ''' <param name="CharValue">Character code</param>
    ''' <returns>Character width</returns>
    
    Public Function CharWidth(FontSize As Double, DrawStyle As DrawStyle, CharValue As Char) As Double
        ' character style is not superscript or subscript
        If (DrawStyle And (DrawStyle.Subscript Or DrawStyle.Superscript)) = 0 Then Return Me.FontDesignToUserUnits(FontSize, Me.GetCharInfo(Microsoft.VisualBasic.AscW(CharValue)).DesignWidth)

        ' superscript
        If (DrawStyle And DrawStyle.Superscript) <> 0 Then Return Me.FontDesignToUserUnits(SubscriptSize(FontSize), Me.GetCharInfo(Microsoft.VisualBasic.AscW(CharValue)).DesignWidth)

        ' subscript
        Return Me.FontDesignToUserUnits(SuperscriptSize(FontSize), Me.GetCharInfo(Microsoft.VisualBasic.AscW(CharValue)).DesignWidth)
    End Function

    
    ''' <summary>
    ''' Character bounding box in user coordinate units.
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="CharValue">Character</param>
    ''' <returns>Bounding box</returns>
    
    Public Function CharBoundingBox(FontSize As Double, CharValue As Char) As PdfRectangle
        ' get character info
        Dim CharInfo = Me.GetCharInfo(AscW(CharValue))

        ' convert to user coordinate units
        Dim Factor = FontSize / (DesignHeight * ScaleFactor)
        Return New PdfRectangle(Factor * CharInfo.DesignBBoxLeft, Factor * CharInfo.DesignBBoxBottom, Factor * CharInfo.DesignBBoxRight, Factor * CharInfo.DesignBBoxTop)
    End Function

    
    ''' <summary>
    ''' Text width
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Width</returns>
    
    Public Function TextWidth(FontSize As Double, Text As String) As Double
        ' text width
        Dim Width = 0

        For Each CharValue In Text
            Width += Me.GetCharInfo(Microsoft.VisualBasic.AscW(CharValue)).DesignWidth
        Next

        ' to user unit of measure
        Return FontDesignToUserUnits(FontSize, Width)
    End Function

    
    ''' <summary>
    ''' Word spacing to stretch text to given width
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="ReqWidth">Required width</param>
    ''' <param name="WordSpacing">Output word spacing</param>
    ''' <param name="CharSpacing">Output character spacing</param>
    ''' <param name="Text">Text</param>
    ''' <returns>True-done, False-not done.</returns>
    
    Public Function TextFitToWidth(FontSize As Double, ReqWidth As Double, <Out> ByRef WordSpacing As Double, <Out> ByRef CharSpacing As Double, Text As String) As Boolean
        WordSpacing = 0
        CharSpacing = 0
        If Equals(Text, Nothing) OrElse Text.Length < 2 Then Return False
        Dim Width = 0
        Dim SpaceCount = 0

        For Each CharValue In Text
            ' character width
            Width += Me.GetCharInfo(Microsoft.VisualBasic.AscW(CharValue)).DesignWidth

            ' space count
            If CharValue = " "c Then SpaceCount += 1
        Next

        ' to user unit of measure
        Dim UserUnitsWidth = FontDesignToUserUnits(FontSize, Width)

        ' extra spacing required
        Dim ExtraSpace = ReqWidth - UserUnitsWidth

        ' string is too wide
        If ExtraSpace < -Document.Epsilon Then Return False

        ' string is just right
        If ExtraSpace < Document.Epsilon Then Return True

        ' String does not have any spacesS
        If SpaceCount = 0 Then
            CharSpacing = ExtraSpace / (Text.Length - 1)
            Return True
        End If

        ' extra space per word
        WordSpacing = ExtraSpace / SpaceCount

        ' extra space is equal or less than one blank
        If WordSpacing <= Me.FontDesignToUserUnits(FontSize, Me.GetCharInfo(Microsoft.VisualBasic.AscW(" "c)).DesignWidth) Then Return True

        ' extra space is larger that one blank
        ' increase character and word spacing
        CharSpacing = ExtraSpace / (10 * SpaceCount + Text.Length - 1)
        WordSpacing = 10 * CharSpacing
        Return True
    End Function

    
    ''' <summary>
    ''' Text bounding box in user coordinate units.
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="Text">Text</param>
    ''' <returns>Bounding box</returns>
    
    Public Function TextBoundingBox(FontSize As Double, Text As String) As PdfRectangle
        If String.IsNullOrEmpty(Text) Then Return Nothing

        ' initialize result box to first character
        Dim FirstChar = Me.GetCharInfo(AscW(Text(0)))
        Dim Left = FirstChar.DesignBBoxLeft
        Dim Bottom = FirstChar.DesignBBoxBottom
        Dim Right = FirstChar.DesignBBoxRight
        Dim Top = FirstChar.DesignBBoxTop
        Dim Width = FirstChar.DesignWidth

        ' more than one character
        If Text.Length > 1 Then
            ' loop from second character
            For Index = 1 To Text.Length - 1
                ' get bounding box for current character
                Dim Info = Me.GetCharInfo(AscW(Text(Index)))

                ' update bottom
                If Info.DesignBBoxBottom < Bottom Then Bottom = Info.DesignBBoxBottom

                ' update top
                If Info.DesignBBoxTop > Top Then Top = Info.DesignBBoxTop

                ' accumulate width
                Width += Info.DesignWidth
            Next

            ' last character
            Dim LastChar = Me.GetCharInfo(AscW(Text(Text.Length - 1)))
            Right = Width - LastChar.DesignWidth + LastChar.DesignBBoxRight
        End If

        ' convert to user coordinate units
        Dim Factor = FontSize / (DesignHeight * ScaleFactor)
        Return New PdfRectangle(Factor * Left, Factor * Bottom, Factor * Right, Factor * Top)
    End Function

    
    ''' <summary>
    ''' Text Kerning
    ''' </summary>
    ''' <param name="Text">Text</param>
    ''' <returns>Kerning adjustment pairs</returns>
    
    Public Function TextKerning(Text As String) As KerningAdjust()
        ' string is empty or one character
        If String.IsNullOrEmpty(Text) OrElse Text.Length = 1 Then Return Nothing

        ' find first and last characters of the text
        Dim First As Integer = AscW(Text(0))
        Dim Last As Integer = AscW(Text(0))

        For Each Chr As Integer In Text.ToCharArray.Select(AddressOf AscW)

            If Chr < First Then
                First = Chr
            ElseIf Chr > Last Then
                Last = Chr
            End If
        Next

        ' get kerning information
        Dim KP = FontApi.GetKerningPairsApi(First, Last)

        ' no kerning info available for this font or for this range
        If KP Is Nothing Then Return Nothing

        ' prepare a list of kerning adjustments
        Dim KA As List(Of KerningAdjust) = New List(Of KerningAdjust)()

        ' look for pairs with adjustments
        Dim Ptr1 = 0

        For Ptr2 = 1 To Text.Length - 1
            ' search for a pair of characters
            Dim Index As Integer = Array.BinarySearch(KP, New WinKerningPair(Text(Ptr2 - 1), Text(Ptr2)))

            ' not kerning information for this pair
            If Index < 0 Then Continue For

            ' add kerning adjustment in PDF font units (windows design units divided by windows font design height)
            KA.Add(New KerningAdjust(Text.Substring(Ptr1, Ptr2 - Ptr1), FontDesignToPdfUnits(KP(Index).KernAmount)))

            ' adjust pointer
            Ptr1 = Ptr2
        Next

        ' list is empty
        If KA.Count = 0 Then Return Nothing

        ' add last
        KA.Add(New KerningAdjust(Text.Substring(Ptr1, Text.Length - Ptr1), 0))

        ' exit
        Return KA.ToArray()
    End Function

    
    ''' <summary>
    ''' Text kerning width
    ''' </summary>
    ''' <param name="FontSize">Font size</param>
    ''' <param name="KerningArray">Kerning array</param>
    ''' <returns>Width</returns>
    
    Public Function TextKerningWidth(FontSize As Double, KerningArray As KerningAdjust()) As Double     ' in points
        ' text is null or empty
        If KerningArray Is Nothing OrElse KerningArray.Length = 0 Then Return 0

        ' total width
        Dim Width As Double = 0

        ' draw text
        Dim LastStr = KerningArray.Length - 1

        For Index = 0 To LastStr - 1
            Dim KA = KerningArray(Index)
            Width += TextWidth(FontSize, KA.Text) + KA.Adjust * FontSize / (1000.0 * ScaleFactor)
        Next

        ' last string
        Width += TextWidth(FontSize, KerningArray(LastStr).Text)
        Return Width
    End Function

    
    ' Write object to PDF file
    

    Friend Overrides Sub WriteObjectToPdfFile()
        ' pdf font name
        Dim PdfFontName As StringBuilder = New StringBuilder("/")

        ' for embedded font add 6 alpha characters prefix
        If EmbeddedFont Then
            PdfFontName.Append("PFWAAA+")
            Dim Ptr1 = 6
            Dim Ptr2 = ResourceCode.Length - 1

            While Ptr2 >= 0 AndAlso Char.IsDigit(ResourceCode(Ptr2))
                PdfFontName(stdNum.Max(Threading.Interlocked.Decrement(Ptr1), Ptr1 + 1)) = Microsoft.VisualBasic.ChrW(Microsoft.VisualBasic.AscW(ResourceCode(Ptr2)) + (Asc("A"c) - Asc("0"c)))
                Ptr2 -= 1
            End While
        End If

        ' invariant name
        Dim PostScriptName = FontFamily.GetName(127)

        ' PDF readers are not happy with space in font name
        PdfFontName.Append(PostScriptName.Replace(" ", "#20"))

        ' font name
        If (DesignFont.Style And FontStyle.Bold) <> 0 Then
            If (DesignFont.Style And FontStyle.Italic) <> 0 Then
                PdfFontName.Append(",BoldItalic")
            Else
                PdfFontName.Append(",Bold")
            End If
        ElseIf (DesignFont.Style And FontStyle.Italic) <> 0 Then
            PdfFontName.Append(",Italic")
        End If

        ' we have one byte characters 
        If FontResCodeUsed Then CharCodeToPdfFile(PdfFontName.ToString())

        ' we have two bytes characters 
        If FontResGlyphUsed Then GlyphIndexToPdfFile(PdfFontName.ToString())

        ' dispose resources
        Dispose()
        Return
    End Sub

    
    ' Write character code oject to PDF file
    

    Friend Sub CharCodeToPdfFile(PdfFontName As String)
        ' look for first and last character
        Dim FirstChar As Integer
        Dim LastChar As Integer
        FirstChar = 0

        While FirstChar < 256 AndAlso (CharInfoArray(0)(FirstChar) Is Nothing OrElse Not CharInfoArray(0)(FirstChar).ActiveChar)
            FirstChar += 1
        End While

        If FirstChar = 256 Then Return
        LastChar = 255

        While CharInfoArray(0)(LastChar) Is Nothing OrElse Not CharInfoArray(0)(LastChar).ActiveChar
            LastChar -= 1
        End While

        ' add items to dictionary
        Dictionary.Add("/Subtype", "/TrueType")
        Dictionary.Add("/BaseFont", PdfFontName)

        ' add first and last characters
        Dictionary.AddInteger("/FirstChar", FirstChar)
        Dictionary.AddInteger("/LastChar", LastChar)

        ' create font descriptor
        Dictionary.AddIndirectReference("/FontDescriptor", CharCodeFontDescriptor(PdfFontName, FirstChar, LastChar))

        ' create width object array
        Dictionary.AddIndirectReference("/Widths", CharCodeFontWidthArray(FirstChar, LastChar))

        ' set encoding
        Dictionary.Add("/Encoding", "/WinAnsiEncoding")

        ' call base write PdfObject to file method
        MyBase.WriteObjectToPdfFile()

        ' exit
        Return
    End Sub

    
    ' Character code font descriptor
    

    Private Function CharCodeFontDescriptor(PdfFontName As String, FirstChar As Integer, LastChar As Integer) As PdfObject
        ' create font descriptor
        Dim FontDescriptor = FontDescriptorCommon(PdfFontName)

        ' build bounding box and calculate maximum width
        Dim Left = Integer.MaxValue
        Dim Bottom = Integer.MaxValue
        Dim Right = Integer.MinValue
        Dim Top = Integer.MinValue
        Dim MaxWidth = Integer.MinValue

        For Index = FirstChar To LastChar
            ' shortcut
            Dim CharInfo = CharInfoArray(0)(Index)

            ' not used
            If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then Continue For

            ' bounding box
            If CharInfo.DesignBBoxLeft < Left Then Left = CharInfo.DesignBBoxLeft
            If CharInfo.DesignBBoxBottom < Bottom Then Bottom = CharInfo.DesignBBoxBottom
            If CharInfo.DesignBBoxRight > Right Then Right = CharInfo.DesignBBoxRight
            If CharInfo.DesignBBoxTop > Top Then Top = CharInfo.DesignBBoxTop

            ' max width
            If CharInfo.DesignWidth > MaxWidth Then MaxWidth = CharInfo.DesignWidth
        Next

        ' add to font descriptor array
        FontDescriptor.Dictionary.AddReal("/MaxWidth", FontDesignToPdfUnits(MaxWidth))
        FontDescriptor.Dictionary.AddFormat("/FontBBox", "[{0} {1} {2} {3}]", FontDesignToPdfUnits(Left), FontDesignToPdfUnits(Bottom), FontDesignToPdfUnits(Right), FontDesignToPdfUnits(Top))

        ' create font file
        If EmbeddedFont Then
            ' create font file stream
            Dim EmbeddedFontObj As PdfFontFile = New PdfFontFile(Me, FirstChar, LastChar)

            ' add link to font object
            FontDescriptor.Dictionary.AddIndirectReference("/FontFile2", EmbeddedFontObj)
        End If

        ' output font descriptor
        FontDescriptor.WriteObjectToPdfFile()

        ' return with reference to font descriptor
        Return FontDescriptor
    End Function

    
    ' Character code font descriptor
    

    Private Function FontDescriptorCommon(PdfFontName As String) As PdfObject
        ' create font descriptor
        Dim FontDescriptor As PdfObject = New PdfObject(Document, ObjectType.Dictionary, "/FontDescriptor")

        ' font descriptor dictionary
        FontDescriptor.Dictionary.Add("/FontName", PdfFontName) ' must be the same as BaseFont above
        FontDescriptor.Dictionary.AddInteger("/Flags", FontFlags)
        FontDescriptor.Dictionary.AddReal("/ItalicAngle", DesignItalicAngle / 10.0)
        FontDescriptor.Dictionary.AddInteger("/FontWeight", DesignFontWeight)
        FontDescriptor.Dictionary.AddReal("/Leading", FontDesignToPdfUnits(PdfLeading))
        FontDescriptor.Dictionary.AddReal("/Ascent", FontDesignToPdfUnits(PdfAscent))
        FontDescriptor.Dictionary.AddReal("/Descent", FontDesignToPdfUnits(-PdfDescent))

        ' alphabetic (non symbolic) fonts
        If (FontFlags And PdfFontFlags.Symbolic) = 0 Then
            ' character info for small x
            Dim CharInfo = FontApi.GetGlyphMetricsApiByCode(Asc("x"c))
            FontDescriptor.Dictionary.AddReal("/XHeight", FontDesignToPdfUnits(CharInfo.DesignBBoxTop))
            FontDescriptor.Dictionary.AddReal("/AvgWidth", FontDesignToPdfUnits(CharInfo.DesignWidth))

            ' character info for capital M
            CharInfo = FontApi.GetGlyphMetricsApiByCode(Asc("M"c))
            FontDescriptor.Dictionary.AddReal("/CapHeight", FontDesignToPdfUnits(CharInfo.DesignBBoxTop))
            FontDescriptor.Dictionary.AddReal("/StemV", StemV())
        End If

        ' return with reference to font descriptor
        Return FontDescriptor
    End Function

    
    ' Character code font width array
    

    Friend Function CharCodeFontWidthArray(FirstChar As Integer, LastChar As Integer) As PdfObject
        ' create width object array
        Dim FontWidthArray As PdfObject = New PdfObject(Document, ObjectType.Other)
        FontWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW("["c))
        Dim EolLength = 100

        For Index = FirstChar To LastChar
            ' shortcut
            Dim CharInfo = CharInfoArray(0)(Index)

            ' add new line after a 100 character block
            If FontWidthArray.ObjectValueList.Count > EolLength Then
                FontWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW(Microsoft.VisualBasic.Strings.ChrW(10)))
                EolLength = FontWidthArray.ObjectValueList.Count + 100
            End If

            ' not used
            If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then
                ' used
                FontWidthArray.ObjectValueAppend("0 ")
            Else
                ' add width to width array
                FontWidthArray.ObjectValueFormat("{0} ", CSng(FontDesignToPdfUnits(CharInfo.DesignWidth)))
            End If
        Next

        ' terminate width array
        FontWidthArray.ObjectValueList(FontWidthArray.ObjectValueList.Count - 1) = Microsoft.VisualBasic.AscW("]"c)

        ' output object to pdf file
        FontWidthArray.WriteObjectToPdfFile()

        ' return reference to font width
        Return FontWidthArray
    End Function

    
    ' Write glyph index font oject to PDF file
    

    Friend Sub GlyphIndexToPdfFile(PdfFontName As String)
        ' add items to dictionary
        GlyphIndexFont.Dictionary.Add("/Subtype", "/Type0")
        GlyphIndexFont.Dictionary.Add("/BaseFont", PdfFontName)
        GlyphIndexFont.Dictionary.Add("/Encoding", "/Identity-H")

        ' create to unicode
        GlyphIndexFont.Dictionary.AddIndirectReference("/ToUnicode", GlyphIndexToUnicode())

        ' create descended fonts object
        Dim DescendedFonts As PdfObject = New PdfObject(Document, ObjectType.Dictionary, "/Font")
        GlyphIndexFont.Dictionary.AddFormat("/DescendantFonts", "[{0} 0 R]", DescendedFonts.ObjectNumber)

        ' add items to dictionary
        DescendedFonts.Dictionary.Add("/Subtype", "/CIDFontType2")
        DescendedFonts.Dictionary.Add("/BaseFont", PdfFontName)

        ' add CIDSystem info
        Dim CIDSystemInfo As PdfDictionary = New PdfDictionary(DescendedFonts)
        DescendedFonts.Dictionary.AddDictionary("/CIDSystemInfo", CIDSystemInfo)
        CIDSystemInfo.AddPdfString("/Ordering", "Identity")
        CIDSystemInfo.AddPdfString("/Registry", "Adobe")
        CIDSystemInfo.AddInteger("/Supplement", 0)

        ' create font descriptor
        DescendedFonts.Dictionary.AddIndirectReference("/FontDescriptor", GlyphIndexFontDescriptor(PdfFontName))

        ' create character width array
        DescendedFonts.Dictionary.AddIndirectReference("/W", GlyphIndexWidthArray())

        ' send glyph index font to output file
        GlyphIndexFont.WriteObjectToPdfFile()

        ' exit
        Return
    End Sub

    
    ' Glyph index font descriptor
    

    Private Function GlyphIndexFontDescriptor(PdfFontName As String) As PdfObject
        ' create font descriptor
        Dim FontDescriptor = FontDescriptorCommon(PdfFontName)

        ' build bounding box and calculate maximum width
        Dim Undef = UndefinedCharInfo.ActiveChar
        Dim Left = If(Undef, UndefinedCharInfo.DesignBBoxLeft, Integer.MaxValue)
        Dim Bottom = If(Undef, UndefinedCharInfo.DesignBBoxBottom, Integer.MaxValue)
        Dim Right = If(Undef, UndefinedCharInfo.DesignBBoxRight, Integer.MinValue)
        Dim Top = If(Undef, UndefinedCharInfo.DesignBBoxTop, Integer.MinValue)
        Dim MaxWidth = If(Undef, UndefinedCharInfo.DesignWidth, Integer.MinValue)

        ' look for all used characters
        For Row = 1 To 256 - 1
            Dim OneRow = CharInfoArray(Row)
            If OneRow Is Nothing Then Continue For

            For Col = 0 To 256 - 1
                Dim CharInfo = OneRow(Col)
                If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then Continue For

                ' bounding box
                If CharInfo.DesignBBoxLeft < Left Then Left = CharInfo.DesignBBoxLeft
                If CharInfo.DesignBBoxBottom < Bottom Then Bottom = CharInfo.DesignBBoxBottom
                If CharInfo.DesignBBoxRight > Right Then Right = CharInfo.DesignBBoxRight
                If CharInfo.DesignBBoxTop > Top Then Top = CharInfo.DesignBBoxTop

                ' max width
                If CharInfo.DesignWidth > MaxWidth Then MaxWidth = CharInfo.DesignWidth
            Next
        Next

        ' add to font descriptor array
        FontDescriptor.Dictionary.AddReal("/MaxWidth", FontDesignToPdfUnits(MaxWidth))
        FontDescriptor.Dictionary.AddFormat("/FontBBox", "[{0} {1} {2} {3}]", FontDesignToPdfUnits(Left), FontDesignToPdfUnits(Bottom), FontDesignToPdfUnits(Right), FontDesignToPdfUnits(Top))

        ' create font file
        If EmbeddedFont Then
            ' create font file stream
            Dim EmbeddedFontObj As PdfFontFile = New PdfFontFile(Me, 0, 0)

            ' add link to font object
            FontDescriptor.Dictionary.AddIndirectReference("/FontFile2", EmbeddedFontObj)
        End If

        ' send font descriptor to output file
        FontDescriptor.WriteObjectToPdfFile()

        ' return reference to font descriptor
        Return FontDescriptor
    End Function

    
    ' Glyph index to Unicode stream
    

    Private Function GlyphIndexToUnicode() As PdfObject
        Dim Header As String = "/CIDInit /ProcSet findresource begin" & Microsoft.VisualBasic.Constants.vbLf & "14 dict begin" & Microsoft.VisualBasic.Constants.vbLf & "begincmap" & Microsoft.VisualBasic.Constants.vbLf & "/CIDSystemInfo<</Registry(Adobe)/Ordering (UCS)/Supplement 0>>def" & Microsoft.VisualBasic.Constants.vbLf & "/CMapName/Adobe-Identity-UCS def" & Microsoft.VisualBasic.Constants.vbLf & "/CMapType 2 def" & Microsoft.VisualBasic.Constants.vbLf & "1 begincodespacerange" & Microsoft.VisualBasic.Constants.vbLf & "<0000><FFFF>" & Microsoft.VisualBasic.Constants.vbLf & "endcodespacerange" & Microsoft.VisualBasic.Constants.vbLf
        Dim Trailer As String = "endcmap" & Microsoft.VisualBasic.Constants.vbLf & "CMapName currentdict /CMap defineresource pop" & Microsoft.VisualBasic.Constants.vbLf & "end" & Microsoft.VisualBasic.Constants.vbLf & "end" & Microsoft.VisualBasic.Constants.vbLf

        ' create array of glyph index to character code
        Dim RangeArray As List(Of UnicodeRange) = New List(Of UnicodeRange)()

        ' add one entry for undefined character
        If UndefinedCharInfo.ActiveChar Then RangeArray.Add(New UnicodeRange(0, 0))

        ' look for all used characters
        For Row = 1 To 256 - 1
            Dim OneRow = CharInfoArray(Row)
            If OneRow Is Nothing Then Continue For

            For Col = 0 To 256 - 1
                Dim CharInfo = OneRow(Col)
                If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then Continue For
                RangeArray.Add(New UnicodeRange(CharInfo.NewGlyphIndex, CharInfo.CharCode))
            Next
        Next

        ' sort by glyph index
        RangeArray.Sort()

        ' look for ranges
        Dim Last = RangeArray(0)
        Dim Run = 1
        Dim Index = 1

        While Index < RangeArray.Count
            Dim [Next] = RangeArray(Index)

            ' we have duplicate glyph index (i.e. space and non-breaking space)
            ' remove the higher char code
            If [Next].GlyphStart = Last.GlyphStart Then
                If [Next].CharCode < Last.CharCode Then Last.CharCode = [Next].CharCode
                RangeArray.RemoveAt(Index)
                Continue While
            End If

            ' range is found
            If [Next].GlyphStart = Last.GlyphEnd + 1 AndAlso [Next].CharCode = Last.CharCode + Run Then
                Last.GlyphEnd += 1
                Run += 1
                RangeArray.RemoveAt(Index)
                Continue While
            End If

            ' start new range
            Last = [Next]
            Run = 1
            Index += 1
        End While

        ' create ToUnicode stream object
        Dim ToUnicode As PdfObject = New PdfObject(Document, ObjectType.Stream)

        ' ouput header
        ToUnicode.ObjectValueAppend(Header)

        ' output ranges
        Run = 0

        For Index = 0 To RangeArray.Count - 1

            If Run = 0 Then
                If Index <> 0 Then ToUnicode.ObjectValueAppend("endbfrange" & Microsoft.VisualBasic.Constants.vbLf)
                Run = stdNum.Min(100, RangeArray.Count - Index)
                ToUnicode.ObjectValueFormat("{0} beginbfrange" & Microsoft.VisualBasic.Constants.vbLf, Run)
            End If

            Run -= 1
            Dim Range = RangeArray(Index)
            Dim RangeStr = String.Format("<{0:x4}><{1:x4}><{2:x4}>" & Microsoft.VisualBasic.Constants.vbLf, Range.GlyphStart, Range.GlyphEnd, Range.CharCode)

            For Each Chr As Char In RangeStr
                ToUnicode.ObjectValueList.Add(Microsoft.VisualBasic.AscW(Chr))
            Next
        Next

        If RangeArray.Count > 0 Then ToUnicode.ObjectValueAppend("endbfrange" & Microsoft.VisualBasic.Constants.vbLf)

        ' output trailer
        ToUnicode.ObjectValueAppend(Trailer)

        ' send to output file
        ToUnicode.WriteObjectToPdfFile()

        ' return reference to glyph index to unicode translation
        Return ToUnicode
    End Function

    
    ' Glyph index to width array
    

    Private Function GlyphIndexWidthArray() As PdfObject
        ' create array of glyph index to character code
        Dim WidthArray As List(Of GlyphWidth) = New List(Of GlyphWidth)()

        ' add undefined glyph
        If UndefinedCharInfo.ActiveChar Then WidthArray.Add(New GlyphWidth(0, UndefinedCharInfo.DesignWidth))

        ' look for all used characters
        For Row = 1 To 256 - 1
            Dim OneRow = CharInfoArray(Row)
            If OneRow Is Nothing Then Continue For

            For Col = 0 To 256 - 1
                Dim CharInfo = OneRow(Col)
                If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then Continue For
                WidthArray.Add(New GlyphWidth(CharInfo.NewGlyphIndex, CharInfo.DesignWidth))
            Next
        Next

        ' sort by glyph index
        WidthArray.Sort()

        ' create ToUnicode stream object
        Dim GlyphWidthArray As PdfObject = New PdfObject(Document, ObjectType.Other)

        ' ouput header
        GlyphWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW("["c))

        ' output ranges
        Dim LastIndex = WidthArray(0).GlyphIndex
        Dim LastWidth As Double = WidthArray(0).Width
        Dim StartIndex = 0
        Dim StartWidth = 0
        Dim Item As GlyphWidth = Nothing

        For Index = 1 To WidthArray.Count

            If Index < WidthArray.Count Then
                ' shortcut
                Item = WidthArray(Index)

                ' it is possible to have two entries with the save glyph index
                If Item.GlyphIndex = LastIndex Then
                    WidthArray.RemoveAt(Index)
                    Index -= 1
                    Continue For
                End If

                ' two consecutive glyphs 
                If Item.GlyphIndex = LastIndex + 1 Then
                    ' two consecutive glyphs have the same width
                    If Item.Width = LastWidth Then
                        LastIndex += 1
                        Continue For
                    End If
                    ' width is not the same and the last group is too small
                    If Index - StartWidth < 3 Then
                        StartWidth = Index
                        LastIndex += 1
                        Continue For
                    End If
                End If
            End If

            ' either glyphs are not consecutives
            ' or 3 or more glyphs have the same width
            ' for first case if there are less than 3 equal width eliminate equal block
            If Index - StartWidth < 3 Then StartWidth = Index

            ' output GlyphIndex [W W W] between StartIndex and StartWidth
            If StartWidth > StartIndex Then
                If StartIndex <> 0 Then GlyphWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW(Microsoft.VisualBasic.Strings.ChrW(10)))
                GlyphWidthArray.ObjectValueFormat("{0}[{1}", WidthArray(StartIndex).GlyphIndex, CSng(FontDesignToPdfUnits(WidthArray(StartIndex).Width)))

                For Ptr = StartIndex + 1 To StartWidth - 1
                    GlyphWidthArray.ObjectValueList.Add(If((Ptr - StartIndex) Mod 12 = 11, Microsoft.VisualBasic.AscW(Microsoft.VisualBasic.Strings.ChrW(10)), Microsoft.VisualBasic.AscW(" "c)))
                    GlyphWidthArray.ObjectValueFormat("{0}", CSng(FontDesignToPdfUnits(WidthArray(Ptr).Width)))
                Next

                GlyphWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW("]"c))
            End If

            If Index > StartWidth Then
                If StartWidth <> 0 Then GlyphWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW(Microsoft.VisualBasic.Strings.ChrW(10)))

                ' output C(StartWidth) C(Index - 1) W
                GlyphWidthArray.ObjectValueFormat("{0} {1} {2}", WidthArray(StartWidth).GlyphIndex, WidthArray(Index - 1).GlyphIndex, CSng(FontDesignToPdfUnits(WidthArray(StartWidth).Width)))
            End If

            ' exit the loop
            If Index = WidthArray.Count Then Exit For

            ' reset block
            LastIndex = Item.GlyphIndex
            LastWidth = Item.Width
            StartIndex = Index
            StartWidth = Index
        Next

        ' terminate width array
        GlyphWidthArray.ObjectValueList.Add(Microsoft.VisualBasic.AscW("]"c))

        ' send to output file
        GlyphWidthArray.WriteObjectToPdfFile()

        ' return reference to glyph width array
        Return GlyphWidthArray
    End Function

    
    ' Calculate StemV from capital I
    

    Private Function StemV() As Double
        ' convert I to graphics path
        Dim GP As GraphicsPath = New GraphicsPath()
        GP.AddString("I", FontFamily, DesignFont.Style, 1000, Point.Empty, StringFormat.GenericDefault)

        ' center point of I
        Dim Rect As RectangleF = GP.GetBounds()
        Dim X As Integer = (Rect.Left + Rect.Right) / 2
        Dim Y As Integer = (Rect.Bottom + Rect.Top) / 2

        ' bounding box converted to integer
        Dim LeftLimit As Integer = Rect.Left
        Dim RightLimit As Integer = Rect.Right

        ' make sure we are within the I
        If Not GP.IsVisible(X, Y) Then Return RightLimit - LeftLimit

        ' look for left edge
        Dim Left As Integer
        Left = X - 1

        While Left >= LeftLimit AndAlso GP.IsVisible(Left, Y)
            Left -= 1
        End While

        ' look for right edge
        Dim Right As Integer
        Right = X + 1

        While Right < RightLimit AndAlso GP.IsVisible(Right, Y)
            Right += 1
        End While

        ' exit
        Return Right - Left - 1
    End Function

    ''' <summary>
    ''' Compage PDF font objects
    ''' </summary>
    ''' <param name="Other">Other PDFFont</param>
    ''' <returns>Compare result</returns>
    Public Overloads Function CompareTo(Other As PdfFont) As Integer Implements IComparable(Of PdfFont).CompareTo
        Dim Cmp = String.Compare(FontFamilyName, Other.FontFamilyName, True)
        If Cmp <> 0 Then Return Cmp
        Cmp = FontStyle - Other.FontStyle
        If Cmp <> 0 Then Return Cmp
        Return If(EmbeddedFont, 1, 0) - If(Other.EmbeddedFont, 1, 0)
    End Function

    ''' <summary>
    ''' Dispose FontApi
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If FontApi IsNot Nothing Then
            FontApi.Dispose()
            FontApi = Nothing
        End If

        Return
    End Sub
End Class


' Support class for glyph index to unicode translation


Friend Class UnicodeRange
    Implements IComparable(Of UnicodeRange)

    Friend GlyphStart As Integer
    Friend GlyphEnd As Integer
    Friend CharCode As Integer

    Friend Sub New(GlyphIndex As Integer, CharCode As Integer)
        GlyphStart = GlyphIndex
        GlyphEnd = GlyphIndex
        Me.CharCode = CharCode
        Return
    End Sub

    Public Function CompareTo(Other As UnicodeRange) As Integer Implements IComparable(Of UnicodeRange).CompareTo
        Return GlyphStart - Other.GlyphStart
    End Function
End Class


' Support class for glyph index to unicode translation


Friend Class GlyphWidth
    Implements IComparable(Of GlyphWidth)

    Friend GlyphIndex As Integer
    Friend Width As Integer

    Friend Sub New(GlyphIndex As Integer, Width As Integer)
        Me.GlyphIndex = GlyphIndex
        Me.Width = Width
        Return
    End Sub

    Public Function CompareTo(Other As GlyphWidth) As Integer Implements IComparable(Of GlyphWidth).CompareTo
        Return GlyphIndex - Other.GlyphIndex
    End Function
End Class
