#Region "Microsoft.VisualBasic::aa989f47553b72f8493b213d7993d7f7, sciBASIC#\mime\application%pdf\PdfFileWriter\FontApi.vb"

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

    '   Total Lines: 1859
    '    Code Lines: 995
    ' Comment Lines: 488
    '   Blank Lines: 376
    '     File Size: 52.01 KB


    ' Class CharInfo
    ' 
    '     Properties: ActiveChar, CharCode, DesignBBoxBottom, DesignBBoxLeft, DesignBBoxRight
    '                 DesignBBoxTop, DesignWidth, GlyphIndex, Type0Font
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CompareTo
    ' 
    ' Class SortByNewIndex
    ' 
    '     Function: Compare
    ' 
    ' Class FontBox
    ' 
    '     Properties: Bottom, Left, Right, Top
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class WinPanose
    ' 
    '     Properties: bArmStyle, bContrast, bFamilyType, bLetterform, bMidline
    '                 bProportion, bSerifStyle, bStrokeVariation, bWeight, bXHeight
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class WinKerningPair
    ' 
    '     Properties: First, KernAmount, Second
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CompareTo
    ' 
    ' Class WinTextMetric
    ' 
    '     Properties: tmAscent, tmAveCharWidth, tmBreakChar, tmCharSet, tmDefaultChar
    '                 tmDescent, tmDigitizedAspectX, tmDigitizedAspectY, tmExternalLeading, tmFirstChar
    '                 tmHeight, tmInternalLeading, tmItalic, tmLastChar, tmMaxCharWidth
    '                 tmOverhang, tmPitchAndFamily, tmStruckOut, tmUnderlined, tmWeight
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class WinOutlineTextMetric
    ' 
    '     Properties: otmAscent, otmDescent, otmEMSquare, otmfsSelection, otmfsType
    '                 otmItalicAngle, otmLineGap, otmMacAscent, otmMacDescent, otmMacLineGap
    '                 otmPanoseNumber, otmpFaceName, otmpFamilyName, otmpFullName, otmpStyleName
    '                 otmptSubscriptOffset, otmptSubscriptSize, otmptSuperscriptOffset, otmptSuperscriptSize, otmrcFontBox
    '                 otmsCapEmHeight, otmsCharSlopeRise, otmsCharSlopeRun, otmSize, otmsStrikeoutPosition
    '                 otmsStrikeoutSize, otmsUnderscorePosition, otmsUnderscoreSize, otmsXHeight, otmTextMetric
    '                 otmusMinimumPPEM
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class FontApi
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BuildUnitMarix, DeleteObject, FormatMessage, GetFontData, GetFontDataApi
    '               GetGlyphIndices, GetGlyphIndicesApi, GetGlyphMetricsApi, GetGlyphMetricsApiByCode, GetGlyphMetricsApiByGlyphIndex
    '               GetGlyphOutline, GetKerningPairs, GetKerningPairsApi, GetOutlineTextMetrics, GetOutlineTextMetricsApi
    '               GetTextMetrics, GetTextMetricsApi, ReadByte, ReadChar, ReadInt16
    '               ReadInt16Array, ReadInt32, ReadInt32Array, ReadString, ReadUInt16
    '               ReadUInt32, ReadWinPoint, SelectObject
    ' 
    '     Sub: Align4, AllocateBuffer, Dispose, FreeBuffer, ThrowSystemErrorException
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	FontApi
'	Support for Windows API functions related to fonts and glyphs.
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
Imports System.Runtime.InteropServices
Imports System.Text
Imports stdNum = System.Math

''' <summary>
''' One character/Glyph information class
''' </summary>
''' <remarks>
''' This class defines all the information required to display a
''' character in the output document. Each character has an
''' associated glyph. The glyph geometry is defined in a square.
''' The square is DesignHeight by DesignHeight.
''' </remarks>
Public Class CharInfo
    Implements IComparable(Of CharInfo)
    ''' <summary>
    ''' Character code
    ''' </summary>

    ''' <summary>
    ''' Glyph index
    ''' </summary>

    ''' <summary>
    ''' Active character
    ''' </summary>

    ''' <summary>
    ''' Character code is greater than 255
    ''' </summary>

    ''' <summary>
    ''' Bounding box left in design units
    ''' </summary>

    ''' <summary>
    ''' Bounding box bottom in design units
    ''' </summary>

    ''' <summary>
    ''' Bounding box right in design units
    ''' </summary>

    ''' <summary>
    ''' Bounding box top in design units
    ''' </summary>

    ''' <summary>
    ''' Character width in design units
    ''' </summary>
    Private _CharCode As Integer, _GlyphIndex As Integer, _ActiveChar As Boolean, _Type0Font As Boolean, _DesignBBoxLeft As Integer, _DesignBBoxBottom As Integer, _DesignBBoxRight As Integer, _DesignBBoxTop As Integer, _DesignWidth As Integer

    Public Property CharCode As Integer
        Get
            Return _CharCode
        End Get
        Friend Set(value As Integer)
            _CharCode = value
        End Set
    End Property

    Public Property GlyphIndex As Integer
        Get
            Return _GlyphIndex
        End Get
        Friend Set(value As Integer)
            _GlyphIndex = value
        End Set
    End Property

    Public Property ActiveChar As Boolean
        Get
            Return _ActiveChar
        End Get
        Friend Set(value As Boolean)
            _ActiveChar = value
        End Set
    End Property

    Public Property Type0Font As Boolean
        Get
            Return _Type0Font
        End Get
        Friend Set(value As Boolean)
            _Type0Font = value
        End Set
    End Property

    Public Property DesignBBoxLeft As Integer
        Get
            Return _DesignBBoxLeft
        End Get
        Friend Set(value As Integer)
            _DesignBBoxLeft = value
        End Set
    End Property

    Public Property DesignBBoxBottom As Integer
        Get
            Return _DesignBBoxBottom
        End Get
        Friend Set(value As Integer)
            _DesignBBoxBottom = value
        End Set
    End Property

    Public Property DesignBBoxRight As Integer
        Get
            Return _DesignBBoxRight
        End Get
        Friend Set(value As Integer)
            _DesignBBoxRight = value
        End Set
    End Property

    Public Property DesignBBoxTop As Integer
        Get
            Return _DesignBBoxTop
        End Get
        Friend Set(value As Integer)
            _DesignBBoxTop = value
        End Set
    End Property

    Public Property DesignWidth As Integer
        Get
            Return _DesignWidth
        End Get
        Friend Set(value As Integer)
            _DesignWidth = value
        End Set
    End Property

    Friend NewGlyphIndex As Integer
    Friend GlyphData As Byte()
    Friend Composite As Boolean

    
    ' constructor
    

    Friend Sub New(CharCode As Integer, GlyphIndex As Integer, DC As FontApi)
        ' save char code and glyph index
        Me.CharCode = CharCode
        Me.GlyphIndex = GlyphIndex
        NewGlyphIndex = -1
        Type0Font = CharCode >= 256 OrElse GlyphIndex = 0

        ' Bounding Box
        Dim BBoxWidth As Integer = DC.ReadInt32()
        Dim BBoxHeight As Integer = DC.ReadInt32()
        DesignBBoxLeft = DC.ReadInt32()
        DesignBBoxTop = DC.ReadInt32()
        DesignBBoxRight = DesignBBoxLeft + BBoxWidth
        DesignBBoxBottom = DesignBBoxTop - BBoxHeight

        ' glyph advance horizontal and vertical
        DesignWidth = DC.ReadInt16()
        'DesignHeight = DC.ReadInt16();
        Return
    End Sub

    
    ' constructor for search and sort
    

    Friend Sub New(GlyphIndex As Integer)
        ' save char code and glyph index
        Me.GlyphIndex = GlyphIndex
        Return
    End Sub

    ''' <summary>
    ''' Compare two glyphs for sort and binary search
    ''' </summary>
    ''' <param name="Other">Other CharInfo</param>
    ''' <returns>Compare result</returns>
    Public Function CompareTo(Other As CharInfo) As Integer Implements IComparable(Of CharInfo).CompareTo
        Return GlyphIndex - Other.GlyphIndex
    End Function
End Class


' IComparer class for new glyph index sort


Friend Class SortByNewIndex
    Implements IComparer(Of CharInfo)

    Public Function Compare(CharOne As CharInfo, CharTwo As CharInfo) As Integer Implements IComparer(Of CharInfo).Compare
        Return CharOne.NewGlyphIndex - CharTwo.NewGlyphIndex
    End Function
End Class


''' <summary>
''' Font box class
''' </summary>
''' <remarks>
''' FontBox class is part of OUTLINETEXTMETRIC structure
''' </remarks>

Public Class FontBox

    ''' <summary>
    ''' Gets left side.
    ''' </summary>
    Public Property Left As Integer

    ''' <summary>
    ''' Gets top side.
    ''' </summary>
    Public Property Top As Integer

    ''' <summary>
    ''' Gets right side.
    ''' </summary>
    Public Property Right As Integer

    ''' <summary>
    ''' Gets bottom side.
    ''' </summary>
    Public Property Bottom As Integer

    Friend Sub New(DC As FontApi)
        Left = DC.ReadInt32()
        Top = DC.ReadInt32()
        Right = DC.ReadInt32()
        Bottom = DC.ReadInt32()
        Return
    End Sub
End Class


''' <summary>
''' Panose class
''' </summary>
''' <remarks>
''' The PANOSE structure describes the PANOSE font-classification
''' values for a TrueType font. These characteristics are then
''' used to associate the font with other fonts of similar
''' appearance but different names.
''' </remarks>

Public Class WinPanose
    ''' <summary>
    ''' Panose family type
    ''' </summary>

    ''' <summary>
    ''' Panose serif style
    ''' </summary>

    ''' <summary>
    ''' Panose weight
    ''' </summary>

    ''' <summary>
    ''' Panose proportion
    ''' </summary>

    ''' <summary>
    ''' Panose contrast
    ''' </summary>

    ''' <summary>
    ''' Panose stroke variation
    ''' </summary>

    ''' <summary>
    ''' Panose arm style
    ''' </summary>

    ''' <summary>
    ''' Panose letter form
    ''' </summary>

    ''' <summary>
    ''' Panose mid line
    ''' </summary>

    ''' <summary>
    ''' Panose X height
    ''' </summary>
    Private _bFamilyType As Byte, _bSerifStyle As Byte, _bWeight As Byte, _bProportion As Byte, _bContrast As Byte, _bStrokeVariation As Byte, _bArmStyle As Byte, _bLetterform As Byte, _bMidline As Byte, _bXHeight As Byte

    Public Property bFamilyType As Byte
        Get
            Return _bFamilyType
        End Get
        Private Set(value As Byte)
            _bFamilyType = value
        End Set
    End Property

    Public Property bSerifStyle As Byte
        Get
            Return _bSerifStyle
        End Get
        Private Set(value As Byte)
            _bSerifStyle = value
        End Set
    End Property

    Public Property bWeight As Byte
        Get
            Return _bWeight
        End Get
        Private Set(value As Byte)
            _bWeight = value
        End Set
    End Property

    Public Property bProportion As Byte
        Get
            Return _bProportion
        End Get
        Private Set(value As Byte)
            _bProportion = value
        End Set
    End Property

    Public Property bContrast As Byte
        Get
            Return _bContrast
        End Get
        Private Set(value As Byte)
            _bContrast = value
        End Set
    End Property

    Public Property bStrokeVariation As Byte
        Get
            Return _bStrokeVariation
        End Get
        Private Set(value As Byte)
            _bStrokeVariation = value
        End Set
    End Property

    Public Property bArmStyle As Byte
        Get
            Return _bArmStyle
        End Get
        Private Set(value As Byte)
            _bArmStyle = value
        End Set
    End Property

    Public Property bLetterform As Byte
        Get
            Return _bLetterform
        End Get
        Private Set(value As Byte)
            _bLetterform = value
        End Set
    End Property

    Public Property bMidline As Byte
        Get
            Return _bMidline
        End Get
        Private Set(value As Byte)
            _bMidline = value
        End Set
    End Property

    Public Property bXHeight As Byte
        Get
            Return _bXHeight
        End Get
        Private Set(value As Byte)
            _bXHeight = value
        End Set
    End Property

    Friend Sub New(DC As FontApi)
        bFamilyType = DC.ReadByte()
        bSerifStyle = DC.ReadByte()
        bWeight = DC.ReadByte()
        bProportion = DC.ReadByte()
        bContrast = DC.ReadByte()
        bStrokeVariation = DC.ReadByte()
        bArmStyle = DC.ReadByte()
        bLetterform = DC.ReadByte()
        bMidline = DC.ReadByte()
        bXHeight = DC.ReadByte()
        Return
    End Sub
End Class


''' <summary>
''' Kerning pair class
''' </summary>

Public Class WinKerningPair
    Implements IComparable(Of WinKerningPair)
    ''' <summary>
    ''' Gets first character
    ''' </summary>

    ''' <summary>
    ''' Gets second character
    ''' </summary>

    ''' <summary>
    ''' Gets kerning amount in design units
    ''' </summary>
    Private _First As Char, _Second As Char, _KernAmount As Integer

    Public Property First As Char
        Get
            Return _First
        End Get
        Private Set(value As Char)
            _First = value
        End Set
    End Property

    Public Property Second As Char
        Get
            Return _Second
        End Get
        Private Set(value As Char)
            _Second = value
        End Set
    End Property

    Public Property KernAmount As Integer
        Get
            Return _KernAmount
        End Get
        Private Set(value As Integer)
            _KernAmount = value
        End Set
    End Property

    Friend Sub New(DC As FontApi)
        First = DC.ReadChar()
        Second = DC.ReadChar()
        KernAmount = DC.ReadInt32()
        Return
    End Sub

    ''' <summary>
    ''' Kerning pair constructor
    ''' </summary>
    ''' <param name="First">First character</param>
    ''' <param name="Second">Second character</param>
    Public Sub New(First As Char, Second As Char)
        Me.First = First
        Me.Second = Second
        Return
    End Sub

    ''' <summary>
    ''' Compare kerning pairs
    ''' </summary>
    ''' <param name="Other">Other pair</param>
    ''' <returns>Compare result</returns>
    Public Function CompareTo(Other As WinKerningPair) As Integer Implements IComparable(Of WinKerningPair).CompareTo
        Return If(First <> Other.First, AscW(First) - AscW(Other.First), AscW(Second) - AscW(Other.Second))
    End Function
End Class


''' <summary>
''' TextMetric class
''' </summary>
''' <remarks>
''' The TEXTMETRIC structure contains basic information about a
''' physical font. All sizes are specified in logical units;
''' that is, they depend on the current mapping mode of the
''' display context.
''' </remarks>

Public Class WinTextMetric
    ''' <summary>
    ''' TextMetric height
    ''' </summary>

    ''' <summary>
    ''' TextMetric ascent
    ''' </summary>

    ''' <summary>
    ''' TextMetric descent
    ''' </summary>

    ''' <summary>
    ''' TextMetric internal leading
    ''' </summary>

    ''' <summary>
    ''' TextMetric external leading
    ''' </summary>

    ''' <summary>
    ''' TextMetric average character width
    ''' </summary>

    ''' <summary>
    ''' TextMetric maximum character width
    ''' </summary>

    ''' <summary>
    ''' TextMetric height
    ''' </summary>

    ''' <summary>
    ''' TextMetric overhang
    ''' </summary>

    ''' <summary>
    ''' TextMetric digitize aspect X
    ''' </summary>

    ''' <summary>
    ''' TextMetric digitize aspect Y
    ''' </summary>

    ''' <summary>
    ''' TextMetric first character
    ''' </summary>

    ''' <summary>
    ''' TextMetric last character
    ''' </summary>

    ''' <summary>
    ''' TextMetric default character
    ''' </summary>

    ''' <summary>
    ''' TextMetric break character
    ''' </summary>

    ''' <summary>
    ''' TextMetric italic
    ''' </summary>

    ''' <summary>
    ''' TextMetric underlined
    ''' </summary>

    ''' <summary>
    ''' TextMetric struck out
    ''' </summary>

    ''' <summary>
    ''' TextMetric pitch and family
    ''' </summary>

    ''' <summary>
    ''' TextMetric character set
    ''' </summary>
    Private _tmHeight As Integer, _tmAscent As Integer, _tmDescent As Integer, _tmInternalLeading As Integer, _tmExternalLeading As Integer, _tmAveCharWidth As Integer, _tmMaxCharWidth As Integer, _tmWeight As Integer, _tmOverhang As Integer, _tmDigitizedAspectX As Integer, _tmDigitizedAspectY As Integer, _tmFirstChar As UShort, _tmLastChar As UShort, _tmDefaultChar As UShort, _tmBreakChar As UShort, _tmItalic As Byte, _tmUnderlined As Byte, _tmStruckOut As Byte, _tmPitchAndFamily As Byte, _tmCharSet As Byte

    Public Property tmHeight As Integer
        Get
            Return _tmHeight
        End Get
        Private Set(value As Integer)
            _tmHeight = value
        End Set
    End Property

    Public Property tmAscent As Integer
        Get
            Return _tmAscent
        End Get
        Private Set(value As Integer)
            _tmAscent = value
        End Set
    End Property

    Public Property tmDescent As Integer
        Get
            Return _tmDescent
        End Get
        Private Set(value As Integer)
            _tmDescent = value
        End Set
    End Property

    Public Property tmInternalLeading As Integer
        Get
            Return _tmInternalLeading
        End Get
        Private Set(value As Integer)
            _tmInternalLeading = value
        End Set
    End Property

    Public Property tmExternalLeading As Integer
        Get
            Return _tmExternalLeading
        End Get
        Private Set(value As Integer)
            _tmExternalLeading = value
        End Set
    End Property

    Public Property tmAveCharWidth As Integer
        Get
            Return _tmAveCharWidth
        End Get
        Private Set(value As Integer)
            _tmAveCharWidth = value
        End Set
    End Property

    Public Property tmMaxCharWidth As Integer
        Get
            Return _tmMaxCharWidth
        End Get
        Private Set(value As Integer)
            _tmMaxCharWidth = value
        End Set
    End Property

    Public Property tmWeight As Integer
        Get
            Return _tmWeight
        End Get
        Private Set(value As Integer)
            _tmWeight = value
        End Set
    End Property

    Public Property tmOverhang As Integer
        Get
            Return _tmOverhang
        End Get
        Private Set(value As Integer)
            _tmOverhang = value
        End Set
    End Property

    Public Property tmDigitizedAspectX As Integer
        Get
            Return _tmDigitizedAspectX
        End Get
        Private Set(value As Integer)
            _tmDigitizedAspectX = value
        End Set
    End Property

    Public Property tmDigitizedAspectY As Integer
        Get
            Return _tmDigitizedAspectY
        End Get
        Private Set(value As Integer)
            _tmDigitizedAspectY = value
        End Set
    End Property

    Public Property tmFirstChar As UShort
        Get
            Return _tmFirstChar
        End Get
        Private Set(value As UShort)
            _tmFirstChar = value
        End Set
    End Property

    Public Property tmLastChar As UShort
        Get
            Return _tmLastChar
        End Get
        Private Set(value As UShort)
            _tmLastChar = value
        End Set
    End Property

    Public Property tmDefaultChar As UShort
        Get
            Return _tmDefaultChar
        End Get
        Private Set(value As UShort)
            _tmDefaultChar = value
        End Set
    End Property

    Public Property tmBreakChar As UShort
        Get
            Return _tmBreakChar
        End Get
        Private Set(value As UShort)
            _tmBreakChar = value
        End Set
    End Property

    Public Property tmItalic As Byte
        Get
            Return _tmItalic
        End Get
        Private Set(value As Byte)
            _tmItalic = value
        End Set
    End Property

    Public Property tmUnderlined As Byte
        Get
            Return _tmUnderlined
        End Get
        Private Set(value As Byte)
            _tmUnderlined = value
        End Set
    End Property

    Public Property tmStruckOut As Byte
        Get
            Return _tmStruckOut
        End Get
        Private Set(value As Byte)
            _tmStruckOut = value
        End Set
    End Property

    Public Property tmPitchAndFamily As Byte
        Get
            Return _tmPitchAndFamily
        End Get
        Private Set(value As Byte)
            _tmPitchAndFamily = value
        End Set
    End Property

    Public Property tmCharSet As Byte
        Get
            Return _tmCharSet
        End Get
        Private Set(value As Byte)
            _tmCharSet = value
        End Set
    End Property

    Friend Sub New(DC As FontApi)
        tmHeight = DC.ReadInt32()
        tmAscent = DC.ReadInt32()
        tmDescent = DC.ReadInt32()
        tmInternalLeading = DC.ReadInt32()
        tmExternalLeading = DC.ReadInt32()
        tmAveCharWidth = DC.ReadInt32()
        tmMaxCharWidth = DC.ReadInt32()
        tmWeight = DC.ReadInt32()
        tmOverhang = DC.ReadInt32()
        tmDigitizedAspectX = DC.ReadInt32()
        tmDigitizedAspectY = DC.ReadInt32()
        tmFirstChar = DC.ReadUInt16()
        tmLastChar = DC.ReadUInt16()
        tmDefaultChar = DC.ReadUInt16()
        tmBreakChar = DC.ReadUInt16()
        tmItalic = DC.ReadByte()
        tmUnderlined = DC.ReadByte()
        tmStruckOut = DC.ReadByte()
        tmPitchAndFamily = DC.ReadByte()
        tmCharSet = DC.ReadByte()
        Return
    End Sub
End Class


''' <summary>
''' Outline text metric class
''' </summary>
''' <remarks>
''' The OUTLINETEXTMETRIC structure contains metrics describing
''' a TrueType font.
''' </remarks>

Public Class WinOutlineTextMetric





    ''' <summary>
    ''' Outline text metric full name
    ''' </summary>
    Private _otmSize As UInteger, _otmTextMetric As WinTextMetric, _otmPanoseNumber As WinPanose, _otmfsSelection As UInteger, _otmfsType As UInteger, _otmsCharSlopeRise As Integer, _otmsCharSlopeRun As Integer, _otmItalicAngle As Integer, _otmEMSquare As UInteger, _otmAscent As Integer, _otmDescent As Integer, _otmLineGap As UInteger, _otmsCapEmHeight As UInteger, _otmsXHeight As UInteger, _otmrcFontBox As FontBox, _otmMacAscent As Integer, _otmMacDescent As Integer, _otmMacLineGap As UInteger, _otmusMinimumPPEM As UInteger, _otmptSubscriptSize As System.Drawing.Point, _otmptSubscriptOffset As System.Drawing.Point, _otmptSuperscriptSize As System.Drawing.Point, _otmptSuperscriptOffset As System.Drawing.Point, _otmsStrikeoutSize As UInteger, _otmsStrikeoutPosition As Integer, _otmsUnderscoreSize As Integer, _otmsUnderscorePosition As Integer, _otmpFamilyName As String, _otmpFaceName As String, _otmpStyleName As String, _otmpFullName As String

    ''' <summary>
    ''' Outline text metric size
    ''' </summary>
    Public Property otmSize As UInteger
        Get
            Return _otmSize
        End Get
        Private Set(value As UInteger)
            _otmSize = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric TextMetric
    ''' </summary>
    Public Property otmTextMetric As WinTextMetric
        Get
            Return _otmTextMetric
        End Get
        Private Set(value As WinTextMetric)
            _otmTextMetric = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric panose number
    ''' </summary>
    Public Property otmPanoseNumber As WinPanose
        Get
            Return _otmPanoseNumber
        End Get
        Private Set(value As WinPanose)
            _otmPanoseNumber = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric FS selection
    ''' </summary>
    Public Property otmfsSelection As UInteger
        Get
            Return _otmfsSelection
        End Get
        Private Set(value As UInteger)
            _otmfsSelection = value
        End Set
    End Property











    ''' <summary>
    ''' Outline text metric ascent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric descent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric line gap
    ''' </summary>

    ''' <summary>
    ''' Outline text metric capital M height
    ''' </summary>

    ''' <summary>
    ''' Outline text metric X height
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Font box class
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Mac ascent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Mac descent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Mac line gap
    ''' </summary>

    ''' <summary>
    ''' Outline text metric minimum PPEM
    ''' </summary>

    ''' <summary>
    ''' Outline text metric subscript size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric subscript offset
    ''' </summary>

    ''' <summary>
    ''' Outline text metric superscript size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric superscript offset
    ''' </summary>

    ''' <summary>
    ''' Outline text metric strikeout size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric strikeout position
    ''' </summary>

    ''' <summary>
    ''' Outline text metric underscore size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric underscore position
    ''' </summary>

    ''' <summary>
    ''' Outline text metric family name
    ''' </summary>

    ''' <summary>
    ''' Outline text metric face name
    ''' </summary>

    ''' <summary>
    ''' Outline text metric style name
    ''' </summary>

    ''' <summary>
    ''' Outline text metric FS type
    ''' </summary>
    Public Property otmfsType As UInteger
        Get
            Return _otmfsType
        End Get
        Private Set(value As UInteger)
            _otmfsType = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric char slope rise
    ''' </summary>
    Public Property otmsCharSlopeRise As Integer
        Get
            Return _otmsCharSlopeRise
        End Get
        Private Set(value As Integer)
            _otmsCharSlopeRise = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric char slope run
    ''' </summary>
    Public Property otmsCharSlopeRun As Integer
        Get
            Return _otmsCharSlopeRun
        End Get
        Private Set(value As Integer)
            _otmsCharSlopeRun = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric italic angle
    ''' </summary>
    Public Property otmItalicAngle As Integer
        Get
            Return _otmItalicAngle
        End Get
        Private Set(value As Integer)
            _otmItalicAngle = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric EM square
    ''' </summary>
    Public Property otmEMSquare As UInteger
        Get
            Return _otmEMSquare
        End Get
        Private Set(value As UInteger)
            _otmEMSquare = value
        End Set
    End Property

    Public Property otmAscent As Integer
        Get
            Return _otmAscent
        End Get
        Private Set(value As Integer)
            _otmAscent = value
        End Set
    End Property

    Public Property otmDescent As Integer
        Get
            Return _otmDescent
        End Get
        Private Set(value As Integer)
            _otmDescent = value
        End Set
    End Property

    Public Property otmLineGap As UInteger
        Get
            Return _otmLineGap
        End Get
        Private Set(value As UInteger)
            _otmLineGap = value
        End Set
    End Property

    Public Property otmsCapEmHeight As UInteger
        Get
            Return _otmsCapEmHeight
        End Get
        Private Set(value As UInteger)
            _otmsCapEmHeight = value
        End Set
    End Property

    Public Property otmsXHeight As UInteger
        Get
            Return _otmsXHeight
        End Get
        Private Set(value As UInteger)
            _otmsXHeight = value
        End Set
    End Property

    Public Property otmrcFontBox As FontBox
        Get
            Return _otmrcFontBox
        End Get
        Private Set(value As FontBox)
            _otmrcFontBox = value
        End Set
    End Property

    Public Property otmMacAscent As Integer
        Get
            Return _otmMacAscent
        End Get
        Private Set(value As Integer)
            _otmMacAscent = value
        End Set
    End Property

    Public Property otmMacDescent As Integer
        Get
            Return _otmMacDescent
        End Get
        Private Set(value As Integer)
            _otmMacDescent = value
        End Set
    End Property

    Public Property otmMacLineGap As UInteger
        Get
            Return _otmMacLineGap
        End Get
        Private Set(value As UInteger)
            _otmMacLineGap = value
        End Set
    End Property

    Public Property otmusMinimumPPEM As UInteger
        Get
            Return _otmusMinimumPPEM
        End Get
        Private Set(value As UInteger)
            _otmusMinimumPPEM = value
        End Set
    End Property

    Public Property otmptSubscriptSize As Point
        Get
            Return _otmptSubscriptSize
        End Get
        Private Set(value As Point)
            _otmptSubscriptSize = value
        End Set
    End Property

    Public Property otmptSubscriptOffset As Point
        Get
            Return _otmptSubscriptOffset
        End Get
        Private Set(value As Point)
            _otmptSubscriptOffset = value
        End Set
    End Property

    Public Property otmptSuperscriptSize As Point
        Get
            Return _otmptSuperscriptSize
        End Get
        Private Set(value As Point)
            _otmptSuperscriptSize = value
        End Set
    End Property

    Public Property otmptSuperscriptOffset As Point
        Get
            Return _otmptSuperscriptOffset
        End Get
        Private Set(value As Point)
            _otmptSuperscriptOffset = value
        End Set
    End Property

    Public Property otmsStrikeoutSize As UInteger
        Get
            Return _otmsStrikeoutSize
        End Get
        Private Set(value As UInteger)
            _otmsStrikeoutSize = value
        End Set
    End Property

    Public Property otmsStrikeoutPosition As Integer
        Get
            Return _otmsStrikeoutPosition
        End Get
        Private Set(value As Integer)
            _otmsStrikeoutPosition = value
        End Set
    End Property

    Public Property otmsUnderscoreSize As Integer
        Get
            Return _otmsUnderscoreSize
        End Get
        Private Set(value As Integer)
            _otmsUnderscoreSize = value
        End Set
    End Property

    Public Property otmsUnderscorePosition As Integer
        Get
            Return _otmsUnderscorePosition
        End Get
        Private Set(value As Integer)
            _otmsUnderscorePosition = value
        End Set
    End Property

    Public Property otmpFamilyName As String
        Get
            Return _otmpFamilyName
        End Get
        Private Set(value As String)
            _otmpFamilyName = value
        End Set
    End Property

    Public Property otmpFaceName As String
        Get
            Return _otmpFaceName
        End Get
        Private Set(value As String)
            _otmpFaceName = value
        End Set
    End Property

    Public Property otmpStyleName As String
        Get
            Return _otmpStyleName
        End Get
        Private Set(value As String)
            _otmpStyleName = value
        End Set
    End Property

    Public Property otmpFullName As String
        Get
            Return _otmpFullName
        End Get
        Private Set(value As String)
            _otmpFullName = value
        End Set
    End Property

    Friend Sub New(DC As FontApi)
        otmSize = DC.ReadUInt32()
        otmTextMetric = New WinTextMetric(DC)
        DC.Align4()
        otmPanoseNumber = New WinPanose(DC)
        DC.Align4()
        otmfsSelection = DC.ReadUInt32()
        otmfsType = DC.ReadUInt32()
        otmsCharSlopeRise = DC.ReadInt32()
        otmsCharSlopeRun = DC.ReadInt32()
        otmItalicAngle = DC.ReadInt32()
        otmEMSquare = DC.ReadUInt32()
        otmAscent = DC.ReadInt32()
        otmDescent = DC.ReadInt32()
        otmLineGap = DC.ReadUInt32()
        otmsCapEmHeight = DC.ReadUInt32()
        otmsXHeight = DC.ReadUInt32()
        otmrcFontBox = New FontBox(DC)
        otmMacAscent = DC.ReadInt32()
        otmMacDescent = DC.ReadInt32()
        otmMacLineGap = DC.ReadUInt32()
        otmusMinimumPPEM = DC.ReadUInt32()
        otmptSubscriptSize = DC.ReadWinPoint()
        otmptSubscriptOffset = DC.ReadWinPoint()
        otmptSuperscriptSize = DC.ReadWinPoint()
        otmptSuperscriptOffset = DC.ReadWinPoint()
        otmsStrikeoutSize = DC.ReadUInt32()
        otmsStrikeoutPosition = DC.ReadInt32()
        otmsUnderscoreSize = DC.ReadInt32()
        otmsUnderscorePosition = DC.ReadInt32()
        otmpFamilyName = DC.ReadString()
        otmpFaceName = DC.ReadString()
        otmpStyleName = DC.ReadString()
        otmpFullName = DC.ReadString()
        Return
    End Sub
End Class

''' <summary>
''' Font API class
''' </summary>
''' <remarks>
''' Windows API callable by C# program
''' </remarks>
Public Class FontApi
    Implements IDisposable

    Private BitMap As Bitmap
    Private GDI As Graphics
    Private GDIHandle As IntPtr
    Private FontHandle As IntPtr
    Private SavedFont As IntPtr
    Private Buffer As IntPtr
    Private BufPtr As Integer
    Private DesignHeight As Integer

    
    ' Device context constructor
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function SelectObject(GDIHandle As IntPtr, FontHandle As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Font API constructor
    ''' </summary>
    ''' <param name="DesignFont">Design font</param>
    ''' <param name="DesignHeight">Design height</param>
    Public Sub New(DesignFont As Font, DesignHeight As Integer)
        ' save design height
        Me.DesignHeight = DesignHeight

        ' define device context
        BitMap = New Bitmap(1, 1)
        GDI = Graphics.FromImage(BitMap)
        GDIHandle = CType(GDI.GetHdc(), IntPtr)

        ' select the font into the device context
        FontHandle = DesignFont.ToHfont()
        SavedFont = SelectObject(GDIHandle, FontHandle)

        ' exit
        Return
    End Sub

    
    ' Gets single glyph metric
    

    Private Const GGO_METRICS As UInteger = 0
    Private Const GGO_BITMAP As UInteger = 1
    Private Const GGO_NATIVE As UInteger = 2
    Private Const GGO_BEZIER As UInteger = 3
    Private Const GGO_GLYPH_INDEX As UInteger = 128

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetGlyphOutline(GDIHandle As IntPtr, CharIndex As Integer, GgoFormat As UInteger, GlyphMetrics As IntPtr, Zero As UInteger, Null As IntPtr, TransMatrix As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' Gets glyph metric
    ''' </summary>
    ''' <param name="CharCode">Character code</param>
    ''' <returns>Character info class</returns>
    Public Function GetGlyphMetricsApiByCode(CharCode As Integer) As CharInfo
        ' get glyph index for char code
        Dim GlyphIndexArray = GetGlyphIndicesApi(CharCode, CharCode)

        ' get glyph outline
        Dim Info = GetGlyphMetricsApiByGlyphIndex(GlyphIndexArray(0))
        Info.CharCode = CharCode

        ' exit
        Return Info
    End Function

    ''' <summary>
    ''' Gets glyph metric
    ''' </summary>
    ''' <param name="GlyphIndex">Character code</param>
    ''' <returns>Character info class</returns>
    Public Function GetGlyphMetricsApiByGlyphIndex(GlyphIndex As Integer) As CharInfo
        ' build unit matrix
        Dim UnitMatrix As IntPtr = BuildUnitMarix()

        ' allocate buffer to receive glyph metrics information
        AllocateBuffer(20)

        ' get one glyph
        If GetGlyphOutline(GDIHandle, GlyphIndex, GGO_GLYPH_INDEX, Buffer, 0, IntPtr.Zero, UnitMatrix) < 0 Then ThrowSystemErrorException("Calling GetGlyphOutline failed")

        ' create WinOutlineTextMetric class
        Dim Info As CharInfo = New CharInfo(0, GlyphIndex, Me)

        ' free buffer for glyph metrics
        FreeBuffer()

        ' free unit matrix buffer
        Marshal.FreeHGlobal(UnitMatrix)

        ' exit
        Return Info
    End Function

    ''' <summary>
    ''' Gets array of glyph metrics
    ''' </summary>
    ''' <param name="CharValue">Character code</param>
    ''' <returns>Array of character infos</returns>
    Public Function GetGlyphMetricsApi(CharValue As Integer) As CharInfo()
        ' first character of the 256 block
        Dim FirstChar = CharValue And &HFF00

        ' use glyph index
        Dim UseGlyphIndex = FirstChar <> 0

        ' get character code to glyph index
        ' if GlyphIndex[x] is zero glyph is undefined
        Dim GlyphIndexArray = GetGlyphIndicesApi(FirstChar, FirstChar + 255)

        ' test for at least one valid glyph
        Dim Start As Integer
        Start = 0

        While Start < 256 AndAlso GlyphIndexArray(Start) = 0
            Start += 1
        End While

        If Start = 256 Then Return Nothing

        ' build unit matrix
        Dim UnitMatrix As IntPtr = BuildUnitMarix()

        ' allocate buffer to receive glyph metrics information
        AllocateBuffer(20)

        ' result array
        Dim CharInfoArray = New CharInfo(255) {}

        ' loop for all characters
        For CharCode As Integer = Start To 256 - 1
            ' charater not defined
            Dim GlyphIndex = GlyphIndexArray(CharCode)
            If GlyphIndex = 0 Then Continue For

            ' get one glyph
            If GetGlyphOutline(GDIHandle, FirstChar + CharCode, GGO_METRICS, Buffer, 0, IntPtr.Zero, UnitMatrix) < 0 Then
                ThrowSystemErrorException("Calling GetGlyphOutline failed")
            End If

            ' reset buffer pointer
            BufPtr = 0

            ' create WinOutlineTextMetric class
            CharInfoArray(CharCode) = New CharInfo(FirstChar + CharCode, GlyphIndex, Me)
        Next

        ' free buffer for glyph metrics
        FreeBuffer()

        ' free unit matrix buffer
        Marshal.FreeHGlobal(UnitMatrix)

        ' exit
        Return CharInfoArray
    End Function

    
    ' Get kerning pairs array
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetKerningPairs(GDIHandle As IntPtr, NumberOfPairs As UInteger, PairArray As IntPtr) As UInteger
    End Function

    ''' <summary>
    ''' Gets kerning pairs array
    ''' </summary>
    ''' <param name="FirstChar">First character</param>
    ''' <param name="LastChar">Last character</param>
    ''' <returns>Array of kerning pairs</returns>
    Public Function GetKerningPairsApi(FirstChar As Integer, LastChar As Integer) As WinKerningPair()
        ' get number of pairs
        Dim Pairs As Integer = GetKerningPairs(GDIHandle, 0, IntPtr.Zero)
        If Pairs = 0 Then Return Nothing

        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(8 * Pairs)

        ' get outline text metrics information
        If GetKerningPairs(GDIHandle, Pairs, Buffer) = 0 Then ThrowSystemErrorException("Calling GetKerningPairs failed")

        ' create list because the program will ignore pairs that are outside char range
        Dim TempList As List(Of WinKerningPair) = New List(Of WinKerningPair)()

        ' kerning pairs from buffer
        For Index = 0 To Pairs - 1
            Dim KPair As WinKerningPair = New WinKerningPair(Me)
            If AscW(KPair.First) >= FirstChar AndAlso AscW(KPair.First) <= LastChar AndAlso AscW(KPair.Second) >= FirstChar AndAlso AscW(KPair.Second) <= LastChar Then
                TempList.Add(KPair)
            End If
        Next

        ' free buffer for outline text metrics
        FreeBuffer()

        ' list is empty
        If TempList.Count = 0 Then Return Nothing

        ' sort list
        TempList.Sort()

        ' exit
        Return TempList.ToArray()
    End Function

    
    ' Get OUTLINETEXTMETRICW structure
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetOutlineTextMetrics(GDIHandle As IntPtr, BufferLength As Integer, Buffer As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' Gets OUTLINETEXTMETRICW structure
    ''' </summary>
    ''' <returns>Outline text metric class</returns>
    Public Function GetOutlineTextMetricsApi() As WinOutlineTextMetric
        ' get buffer size
        Dim BufSize = GetOutlineTextMetrics(GDIHandle, 0, IntPtr.Zero)
        If BufSize = 0 Then ThrowSystemErrorException("Calling GetOutlineTextMetrics (get buffer size) failed")

        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(BufSize)

        ' get outline text metrics information
        If GetOutlineTextMetrics(GDIHandle, BufSize, Buffer) = 0 Then ThrowSystemErrorException("Calling GetOutlineTextMetrics failed")

        ' create WinOutlineTextMetric class
        Dim WOTM As WinOutlineTextMetric = New WinOutlineTextMetric(Me)

        ' free buffer for outline text metrics
        FreeBuffer()

        ' exit
        Return WOTM
    End Function

    
    ' Get TEXTMETRICW structure
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetTextMetrics(GDIHandle As IntPtr, Buffer As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' Gets TEXTMETRICW structure
    ''' </summary>
    ''' <returns>Text metric class</returns>
    Public Function GetTextMetricsApi() As WinTextMetric
        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(57)

        ' get outline text metrics information
        If GetTextMetrics(GDIHandle, Buffer) = 0 Then ThrowSystemErrorException("Calling GetTextMetrics API failed.")

        ' create WinOutlineTextMetric class
        Dim WTM As WinTextMetric = New WinTextMetric(Me)

        ' free buffer for outline text metrics
        FreeBuffer()

        ' exit
        Return WTM
    End Function

    
    ' Get font data tables
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetFontData(DeviceContextHandle As IntPtr, Table As UInteger, Offset As UInteger, Buffer As IntPtr, BufferLength As UInteger) As UInteger
    End Function

    ''' <summary>
    ''' Gets font data tables
    ''' </summary>
    ''' <param name="TableTag">Table Tag</param>
    ''' <param name="Offset">Table offset</param>
    ''' <param name="BufSize">Table size</param>
    ''' <returns>Table info as byte array</returns>
    Public Function GetFontDataApi(TableTag As UInteger, Offset As Integer, BufSize As Integer) As Byte()
        ' empty table
        If BufSize = 0 Then Return Nothing

        ' allocate buffer to receive outline text metrics information
        AllocateBuffer(BufSize)

        ' microsoft tag is in little endian format
        Dim MSTag As UInteger = TableTag << 24 Or TableTag << 8 And &HFF0000 Or TableTag >> 8 And &HFF00 Or TableTag >> 24 And &HFF

        ' get outline text metrics information
        If CInt(GetFontData(GDIHandle, MSTag, Offset, Buffer, BufSize)) <> BufSize Then ThrowSystemErrorException("Get font data file header failed")

        ' copy api result buffer to managed memory buffer
        Dim DataBuffer = New Byte(BufSize - 1) {}
        Marshal.Copy(Buffer, DataBuffer, 0, BufSize)
        BufPtr = 0

        ' free unmanaged memory buffer
        FreeBuffer()

        ' exit
        Return DataBuffer
    End Function

    
    ' Get glyph indices array
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function GetGlyphIndices(GDIHandle As IntPtr, CharBuffer As IntPtr, CharCount As Integer, GlyphArray As IntPtr, GlyphOptions As UInteger) As Integer
    End Function

    ''' <summary>
    ''' Gets glyph indices array
    ''' </summary>
    ''' <param name="FirstChar">First character</param>
    ''' <param name="LastChar">Last character</param>
    ''' <returns>Array of glyph indices.</returns>
    Public Function GetGlyphIndicesApi(FirstChar As Integer, LastChar As Integer) As Integer()
        ' character count
        Dim CharCount = LastChar - FirstChar + 1

        ' allocate character table buffer in global memory (two bytes per char)
        Dim CharBuffer = Marshal.AllocHGlobal(2 * CharCount)

        ' create array of all character codes between FirstChar and LastChar (we use short because of Unicode)
        For CharPtr = FirstChar To LastChar
            Marshal.WriteInt16(CharBuffer, 2 * (CharPtr - FirstChar), CShort(CharPtr))
        Next

        ' allocate memory for result
        AllocateBuffer(2 * CharCount)

        ' get glyph numbers for all characters including non existing glyphs
        If GetGlyphIndices(GDIHandle, CharBuffer, CharCount, Buffer, 0) <> CharCount Then ThrowSystemErrorException("Calling GetGlypeIndices failed")

        ' get result array to managed code
        Dim GlyphIndex16 = ReadInt16Array(CharCount)

        ' free local buffer
        Marshal.FreeHGlobal(CharBuffer)

        ' free result buffer
        FreeBuffer()

        ' convert to int
        Dim GlyphIndex32 = New Integer(GlyphIndex16.Length - 1) {}

        For Index = 0 To GlyphIndex16.Length - 1
            GlyphIndex32(Index) = GlyphIndex16(Index)
        Next

        ' exit
        Return GlyphIndex32
    End Function

    
    ' Allocate API result buffer
    

    Private Sub AllocateBuffer(Size As Integer)
        ' allocate memory for result
        Buffer = Marshal.AllocHGlobal(Size)
        BufPtr = 0
        Return
    End Sub

    
    ' Free API result buffer
    

    Private Sub FreeBuffer()
        ' free buffer
        Marshal.FreeHGlobal(Buffer)
        Buffer = IntPtr.Zero
        Return
    End Sub

    
    ' Align buffer pointer to 4 bytes boundry
    

    Friend Sub Align4()
        BufPtr = BufPtr + 3 And Not 3
        Return
    End Sub

    
    ' Read point (x, y) from data buffer
    

    Friend Function ReadWinPoint() As Point
        Return New Point(ReadInt32(), ReadInt32())
    End Function

    
    ' Read byte from data buffer
    

    Friend Function ReadByte() As Byte
        Return Marshal.ReadByte(Buffer, stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
    End Function

    
    ' Read character from data buffer
    

    Friend Function ReadChar() As Char
        Dim Value As Char = Microsoft.VisualBasic.ChrW(Marshal.ReadInt16(Buffer, BufPtr))
        BufPtr += 2
        Return Value
    End Function

    
    ' Read short integer from data buffer
    

    Friend Function ReadInt16() As Short
        Dim Value = Marshal.ReadInt16(Buffer, BufPtr)
        BufPtr += 2
        Return Value
    End Function

    
    ' Read unsigned short integer from data buffer
    

    Friend Function ReadUInt16() As UShort
        Dim Value As UShort = Marshal.ReadInt16(Buffer, BufPtr)
        BufPtr += 2
        Return Value
    End Function

    
    ' Read short array from result buffer
    

    Friend Function ReadInt16Array(Size As Integer) As Short()
        ' create active characters array
        Dim Result = New Short(Size - 1) {}
        Marshal.Copy(Buffer, Result, 0, Size)
        Return Result
    End Function

    
    ' Read integers from data buffer
    

    Friend Function ReadInt32() As Integer
        Dim Value = Marshal.ReadInt32(Buffer, BufPtr)
        BufPtr += 4
        Return Value
    End Function

    
    ' Read int array from result buffer
    

    Friend Function ReadInt32Array(Size As Integer) As Integer()
        ' create active characters array
        Dim Result = New Integer(Size - 1) {}
        Marshal.Copy(Buffer, Result, 0, Size)
        Return Result
    End Function

    
    ' Read unsigned integers from data buffer
    

    Friend Function ReadUInt32() As UInteger
        Dim Value As UInteger = Marshal.ReadInt32(Buffer, BufPtr)
        BufPtr += 4
        Return Value
    End Function

    
    ' Read string (null terminated) from data buffer
    

    Friend Function ReadString() As String
        Dim Ptr = Marshal.ReadInt32(Buffer, BufPtr)
        BufPtr += 4
        Dim Str As StringBuilder = New StringBuilder()

        While True
            Dim Chr As Char = Microsoft.VisualBasic.ChrW(Marshal.ReadInt16(Buffer, Ptr))
            If AscW(Chr) = 0 Then Exit While
            Str.Append(Chr)
            Ptr += 2
        End While

        Return Str.ToString()
    End Function

    
    ' Throw exception showing last system error
    

    <DllImport("Kernel32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function FormatMessage(dwFlags As UInteger, lpSource As IntPtr, dwMessageId As UInteger, dwLanguageId As UInteger, lpBuffer As IntPtr, nSize As UInteger, Arguments As IntPtr) As UInteger
    End Function

    Friend Sub ThrowSystemErrorException(AppMsg As String)
        Const FORMAT_MESSAGE_FROM_SYSTEM As UInteger = &H1000

        ' error message
        Dim ErrMsg As StringBuilder = New StringBuilder(AppMsg)

        ' get last system error
        Dim ErrCode As UInteger = CUInt(Marshal.GetLastWin32Error()) ' GetLastError();

        If ErrCode <> 0 Then
            ' allocate buffer
            Dim ErrBuffer = Marshal.AllocHGlobal(1024)

            ' add error code
            ErrMsg.AppendFormat(Microsoft.VisualBasic.Constants.vbCrLf & "System error [0x{0:X8}]", ErrCode)

            ' convert error code to text
            Dim StrLen As Integer = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, ErrCode, 0, ErrBuffer, 1024, IntPtr.Zero)

            If StrLen > 0 Then
                ErrMsg.Append(" ")
                ErrMsg.Append(Marshal.PtrToStringAuto(ErrBuffer, StrLen))

                While ErrMsg(ErrMsg.Length - 1) <= " "c
                    ErrMsg.Length -= 1
                End While
            End If

            ' free buffer

            ' unknown error
            Marshal.FreeHGlobal(ErrBuffer)
        Else
            ErrMsg.Append(Microsoft.VisualBasic.Constants.vbCrLf & "Unknown error.")
        End If

        ' exit
        Throw New ApplicationException(ErrMsg.ToString())
    End Sub

    
    ' Build unit matrix in unmanaged memory
    

    Private Function BuildUnitMarix() As IntPtr
        ' allocate buffer for transformation matrix
        Dim UnitMatrix = Marshal.AllocHGlobal(16)

        ' set transformation matrix into unit matrix
        Marshal.WriteInt32(UnitMatrix, 0, &H10000)
        Marshal.WriteInt32(UnitMatrix, 4, 0)
        Marshal.WriteInt32(UnitMatrix, 8, 0)
        Marshal.WriteInt32(UnitMatrix, 12, &H10000)
        Return UnitMatrix
    End Function

    
    ' Dispose
    

    <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
    Private Shared Function DeleteObject(Handle As IntPtr) As IntPtr
    End Function

    ''' <summary>
    ''' Dispose unmanaged resources
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' free unmanaged buffer
        Marshal.FreeHGlobal(Buffer)

        ' restore original font
        SelectObject(GDIHandle, SavedFont)

        ' delete font handle
        DeleteObject(FontHandle)

        ' release device context handle
        GDI.ReleaseHdc(GDIHandle)

        ' release GDI resources
        GDI.Dispose()

        ' release bitmap
        BitMap.Dispose()

        ' exit
        Return
    End Sub
End Class
