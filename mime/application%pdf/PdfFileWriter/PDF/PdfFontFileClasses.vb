#Region "Microsoft.VisualBasic::7ce7c05b9c6336cca3a9b90d188ff439, mime\application%pdf\PdfFileWriter\PDF\PdfFontFileClasses.vb"

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

    '   Total Lines: 323
    '    Code Lines: 187
    ' Comment Lines: 94
    '   Blank Lines: 42
    '     File Size: 15.12 KB


    '     Class FontFileHeader
    ' 
    '         Properties: EntrySelector, RangeShift, SearchRange
    ' 
    '     Class TableRecord
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class cmapSubTbl
    ' 
    '         Properties: EntrySelector, RangeShift, SearchRange, SegCountX2
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo
    ' 
    '     Class cmapSeg
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo
    ' 
    '     Class headTable
    ' 
    ' 
    ' 
    '     Class hheaTable
    ' 
    ' 
    ' 
    '     Class maxpTable
    ' 
    ' 
    ' 
    '     Enum CompFlag
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfFontFileClasses
'	Support classes for the PdfFontFile classs.
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


    '
    ' Font file header
    '

    Friend Class FontFileHeader
        Friend FileVersion As UInteger      ' 0x00010000 for version 1.0.
        Friend NumTables As UShort          ' Number of tables.

        ' 16 * (maximum power of 2 <= numTables)
        Friend ReadOnly Property SearchRange As UShort
            Get
                Dim Mask As Integer
                Mask = 1

                While Mask <= NumTables
                    Mask <<= 1
                End While

                Return Mask << 3
            End Get
        End Property

        ' Log2(maximum power of 2 <= numTables).
        Friend ReadOnly Property EntrySelector As UShort
            Get
                Dim Power As Integer
                Power = 1

                While 1 << Power <= NumTables
                    Power += 1
                End While

                Return Power - 1
            End Get
        End Property

        ' NumTables x 16-searchRange.
        Friend ReadOnly Property RangeShift As UShort
            Get
                Return 16 * NumTables - SearchRange
            End Get
        End Property
    End Class

    '
    ' Font file table record
    '

    Friend Class TableRecord
        Friend Tag As UInteger          ' 4 -byte identifier
        Friend Checksum As UInteger     ' Checksum for this table
        Friend Offset As Integer        ' Offset from beginning of TrueType font file
        Friend Length As Integer        ' Length of this table
        Friend Data As Byte()       ' table data in big endian format

        ' constructor
        Friend Sub New(Tag As UInteger)
            Me.Tag = Tag
            Return
        End Sub
    End Class

    '
    ' 'cmap' encoding sub-table
    '

    Friend Class cmapSubTbl
        Implements IComparable(Of cmapSubTbl)

        Friend PlatformID As UShort             ' Platform ID. Should be 3 for windows
        Friend EncodingID As UShort             ' Platform-specific encoding ID. Should be 1 for Unicode and 0 for symbol
        Friend Format As UShort                 ' Format number (the program supports format 0 or 4)
        Friend Offset As UInteger                   ' Byte offset from beginning of table to the sub-table for this encoding
        Friend Length As UShort                 ' This is the length in bytes of the sub-table.
        Friend Language As UShort               ' this field is relevant to Macintosh (platform ID 1)
        Friend SegCount As UShort               ' (Format 4) SegCount.
        Friend SegArray As cmapSeg()                ' (Format 4) segment array
        Friend GlyphArray As UShort()               ' glyph array translate character for format 0 or index for format 4 to glyph code

        ' default constructor
        Friend Sub New()
        End Sub

        ' search constructor
        Friend Sub New(PlatformID As UShort, EncodingID As UShort, Format As UShort)
            Me.PlatformID = PlatformID
            Me.EncodingID = EncodingID
            Me.Format = Format
            Return
        End Sub

        ' compare two sub-tables for sort and binary search
        Public Function CompareTo(Other As cmapSubTbl) As Integer Implements IComparable(Of cmapSubTbl).CompareTo
            If PlatformID <> Other.PlatformID Then Return PlatformID - Other.PlatformID
            If EncodingID <> Other.EncodingID Then Return EncodingID - Other.EncodingID
            Return Format - Other.Format
        End Function

        ' 2 x segCount
        Friend ReadOnly Property SegCountX2 As UShort
            Get
                Return 2 * SegCount
            End Get
        End Property

        ' 2 * (maximum power of 2 <= numTables)
        Friend ReadOnly Property SearchRange As UShort
            Get
                Dim Mask As Integer
                Mask = 1

                While Mask <= SegCount
                    Mask <<= 1
                End While

                Return Mask
            End Get
        End Property

        ' Log2(maximum power of 2 <= numTables).
        Friend ReadOnly Property EntrySelector As UShort
            Get
                Dim Power As Integer
                Power = 1

                While 1 << Power <= SegCount
                    Power += 1
                End While

                Return Power - 1
            End Get
        End Property

        ' NumTables x 16-searchRange.
        Friend ReadOnly Property RangeShift As UShort
            Get
                Return 2 * SegCount - SearchRange
            End Get
        End Property
    End Class

    '
    ' 'cmap' format 4 encoding sub-table segment record
    '

    Friend Class cmapSeg
        Implements IComparable(Of cmapSeg)

        Friend StartChar As UShort              ' Start character code for each segment. Array length=segCount
        Friend EndChar As UShort                ' End characterCode for each segment, last=0xFFFF. Array length=segCount
        Friend IDDelta As Short             ' Delta for all character codes in segment. Array length=segCount
        Friend IDRangeOffset As UShort          ' Offsets (in byte) into glyphIdArray or 0. Array length=segCount

        ' search constructor
        Friend Sub New(StartChar As Integer, EndChar As Integer, IDDelta As Integer, IDRangeOffset As Integer)
            Me.StartChar = CUShort(StartChar)
            Me.EndChar = CUShort(EndChar)
            Me.IDDelta = CShort(IDDelta)
            Me.IDRangeOffset = CUShort(IDRangeOffset)
            Return
        End Sub

        ' search constructor
        Friend Sub New(EndCount As Integer)
            EndChar = CUShort(EndCount)
            Return
        End Sub

        ' compare two records for sort and binary search
        Public Function CompareTo(Other As cmapSeg) As Integer Implements IComparable(Of cmapSeg).CompareTo
            Return EndChar - Other.EndChar
        End Function
    End Class

    '
    ' 'head' font file header table
    '

    Friend Class headTable
        Friend TableVersion As UInteger         ' 0x00010000 for version 1.0.
        Friend FontRevision As UInteger         ' Set by font manufacturer.
        Friend ChecksumAdjustment As UInteger       ' font file overall checksum. To compute: set it to 0, sum the entire font, then store 0xB1B0AFBA - sum.
        Friend MagicNumber As UInteger          ' Set to 0x5F0F3CF5.
        Friend Flags As UShort                  ' Bit 0: Baseline for font at y=0;
        ' Bit 1: Left sidebearing point at x=0;
        ' Bit 2: Instructions may depend on point size; 
        ' Bit 3: Force ppem to integer values for all internal scaler math; may use fractional
        '        ppem sizes if this bit is clear; 
        ' Bit 4: Instructions may alter advance width (the advance widths might not scale linearly); 
        ' Bits 5-10: These should be set according to Apple's specification.
        '        However, they are not implemented in OpenType. 
        ' Bit 11: Font data is 'lossless,' as a result of having been compressed and decompressed
        '         with the Agfa MicroType Express engine.
        ' Bit 12: Font converted (produce compatible metrics)
        ' Bit 13: Font optimized for ClearType™. Note, fonts that rely on embedded bitmaps (EBDT)
        '         for rendering should not be considered optimized for ClearType,
        '		   and therefore should keep this bit cleared.
        ' Bit 14: Reserved, set to 0
        ' Bit 15: Reserved, set to 0 
        Friend UnitsPerEm As UShort             ' Valid range is from 16 to 16384. This value should be a power of 2 for fonts that have TrueType outlines.
        Friend TimeCreated As Long          ' Number of seconds since 12:00 midnight, January 1, 1904. 64-bit integer
        Friend TimeModified As Long         ' Number of seconds since 12:00 midnight, January 1, 1904. 64-bit integer
        Friend xMin As Short                    ' For all glyph bounding boxes.
        Friend yMin As Short                    ' For all glyph bounding boxes.
        Friend xMax As Short                    ' For all glyph bounding boxes.
        Friend yMax As Short                    ' For all glyph bounding boxes.
        Friend MacStyle As UShort               ' Bit 0: Bold (if set to 1); 
        ' Bit 1: Italic (if set to 1) 
        ' Bit 2: Underline (if set to 1) 
        ' Bit 3: Outline (if set to 1) 
        ' Bit 4: Shadow (if set to 1) 
        ' Bit 5: Condensed (if set to 1) 
        ' Bit 6: Extended (if set to 1) 
        ' Bits 7-15: Reserved (set to 0).
        Friend LowestRecPPEM As UShort          ' Smallest readable size in pixels.
        Friend FontDirectionHint As Short       ' Deprecated (Set to 2). 
        ' 0: Fully mixed directional glyphs; 
        ' 1: Only strongly left to right; 
        ' 2: Like 1 but also contains neutrals; 
        ' -1: Only strongly right to left; 
        ' -2: Like -1 but also contains neutrals. 1
        Friend IndexToLocFormat As Short        ' 0 for short offsets, 1 for long.
        Friend glyphDataFormat As Short     ' 0 for current format.
    End Class

    '
    ' 'head' horizontal header table
    '

    Friend Class hheaTable
        Friend TableVersion As UInteger         ' 0x00010000 for version 1.0.
        Friend Ascender As Short                ' Typographic ascent. (Distance from baseline of highest ascender)
        Friend Descender As Short               ' Typographic descent. (Distance from baseline of lowest descender)
        Friend LineGap As Short             ' Typographic line gap. Negative LineGap values are treated as zero
        ' in Windows 3.1, System 6, and System 7.
        Friend advanceWidthMax As UShort        ' Maximum advance width value in 'hmtx' table.
        Friend minLeftSideBearing As Short      ' Minimum left sidebearing value in 'hmtx' table.
        Friend minRightSideBearing As Short ' Minimum right sidebearing value; calculated as Min(aw - lsb - (xMax - xMin)).
        Friend xMaxExtent As Short              ' Max(lsb + (xMax - xMin)).
        Friend caretSlopeRise As Short          ' Used to calculate the slope of the cursor (rise/run); 1 for vertical.
        Friend caretSlopeRun As Short           ' 0 for vertical.
        Friend caretOffset As Short         ' The amount by which a slanted highlight on a glyph needs to be shifted
        ' to produce the best appearance. Set to 0 for non-slanted fonts
        Friend Reserved1 As Short               ' set to 0
        Friend Reserved2 As Short               ' set to 0
        Friend Reserved3 As Short               ' set to 0
        Friend Reserved4 As Short               ' set to 0
        Friend metricDataFormat As Short        ' 0 for current format.
        Friend numberOfHMetrics As UShort       ' Number of hMetric entries in 'hmtx' table
    End Class

    '
    ' 'maxp' font maximum values
    '

    Friend Class maxpTable
        Friend TableVersion As UInteger         ' 0x00010000 for version 1.0.
        Friend numGlyphs As UShort              ' The number of glyphs in the font.
        Friend maxPoints As UShort              ' Maximum points in a non-composite glyph.
        Friend maxContours As UShort            ' Maximum contours in a non-composite glyph.
        Friend maxCompositePoints As UShort     ' Maximum points in a composite glyph.
        Friend maxCompositeContours As UShort   ' Maximum contours in a composite glyph.
        Friend maxZones As UShort               ' 1 if instructions do not use the twilight zone (Z0), or
        ' 2 if instructions do use Z0; should be set to 2 in most cases.
        Friend maxTwilightPoints As UShort      ' Maximum points used in Z0.
        Friend maxStorage As UShort             ' Number of Storage Area locations.
        Friend maxFunctionDefs As UShort        ' Number of FDEFs.
        Friend maxInstructionDefs As UShort     ' Number of IDEFs.
        Friend maxStackElements As UShort       ' Maximum stack depth2.
        Friend maxSizeOfInstructions As UShort  ' Maximum byte count for glyph instructions.
        Friend maxComponentElements As UShort   ' Maximum number of components referenced at “top level” for any composite glyph.
        Friend maxComponentDepth As UShort      ' Maximum levels of recursion; 1 for simple components.
    End Class

    '
    ' Glyph table support
    '

    ' glyph flags for comosite glyphs
    Friend Enum CompFlag
        Arg1AndArg2AreWords = 1         ' bit0	If this is set, the arguments are words; otherwise, they are bytes.
        ArgsAreXYValues = 2             ' bit1	If this is set, the arguments are xy values; otherwise, they are points.
        RoundXYToGrid = 4                   ' bit2	For the xy values if the preceding is true.
        WeHaveAScale = 8                    ' bit3	This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
        Reserve = &H10                      ' bit4	This bit is reserved. Set it to 0.
        MoreComponents = &H20               ' bit5	Indicates at least one more glyph after this one.
        WeHaveXYScale = &H40                ' bit6	The x direction will use a different scale from the y direction.
        WeHave2By2 = &H80                   ' bit7	There is a 2 by 2 transformation that will be used to scale the component.
        WeHaveInstructions = &H100          ' bit8	Following the last component are instructions for the composite character.
        UseMyMetrics = &H200                ' bit9	If set, this forces the aw and lsb (and rsb) for the composite to be equal
        ' to those from this original glyph. This works for hinted and unhinted characters.
        OverlapCompound = &H400         ' bit10 Used by Apple in GX fonts.
        ScaledComponentOffset = &H800       ' bit11 Composite designed to have the component offset scaled (designed for Apple rasterizer).
        UnscaledComponentOffset = &H1000    ' bit12 Composite designed not to have the component offset scaled (designed for the Microsoft TrueType rasterizer).
    End Enum
