#Region "Microsoft.VisualBasic::5332c0499cfa15521590ac37ca061ed3, mime\application%pdf\PdfFileWriter\PDF\PdfFontFile.vb"

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

    '   Total Lines: 1443
    '    Code Lines: 755
    ' Comment Lines: 310
    '   Blank Lines: 378
    '     File Size: 49.60 KB


    ' Class PdfFontFile
    ' 
    ' 
    '     Enum Tag
    ' 
    '         cmap, cvt, fpgm, glyf, head
    '         hhea, hmtx, loca, maxp, prep
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BuildEmbeddedFile, CreateFontFile, CreateGlyphDataList, ReadInt16BigEndian, ReadInt64BigEndian
    '               ReadUInt16BigEndian, ReadUInt32BigEndian, SelectcmapSubTable, TableChecksum, TagBinToStr
    ' 
    '     Sub: AddCompositeGlyphs, AddGlyph, BuildCharMapTable, BuildFontProgramTables, BuildGlyphArray
    '          BuildGlyphArray1, BuildHeadTable, BuildHheaTable, BuildhmtxTable, BuildLocaTable
    '          BuildMaxpTable, CalculateGlyphChksum, GetcmapTable, GetCompositeGlyph, GetFontFileHeaderApi
    '          GetheadTable, GethheaTable, GethmtxTable, GetlocaTable, GetmaxpTable
    '          ReplaceGlyphCode, WriteInt16BigEndian, WriteInt64BigEndian, WriteUInt16BigEndian, WriteUInt32BigEndian
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfFontFile
'	Support Class to embed font with the PDF File.
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
Imports System.Text
Imports i32 = Microsoft.VisualBasic.Language.i32
Imports stdNum = System.Math

Friend Class PdfFontFile
    Inherits PdfObject

    Private PdfFont As PdfFont
    Private FontInfo As FontApi
    Private FirstChar As Integer
    Private LastChar As Integer
    Private GlyphIndexFont As Boolean
    Private SymbolicFont As Boolean
    Private CharInfoArray As CharInfo()()
    Private FileHeader As FontFileHeader
    Private cmapSubTbl As cmapSubTbl
    Private headTable As headTable
    Private hheaTable As hheaTable
    Private hmtxTable As UShort()
    Private locaTable As Integer()
    Private maxpTable As maxpTable
    Private CharToGlyphArray As UShort()
    Private GlyphArray As CharInfo()
    Private Buffer As Byte()
    Private BufPtr As Integer

    ' table tags
    Private Const cmapTag As UInteger = &H636D6170  ' "cmap"
    Private Const cvtTag As UInteger = &H63767420   ' "cvt"
    Private Const fpgmTag As UInteger = &H6670676D  ' "fpgm"
    Private Const glyfTag As UInteger = &H676C7966  ' "glyf"
    Private Const headTag As UInteger = &H68656164  ' "head"
    Private Const hheaTag As UInteger = &H68686561  ' "hhea"
    Private Const hmtxTag As UInteger = &H686D7478  ' "hmtx"
    Private Const locaTag As UInteger = &H6C6F6361  ' "loca"
    Private Const maxpTag As UInteger = &H6D617870  ' "maxp"
    Private Const prepTag As UInteger = &H70726570  ' "prep"

    ' this array must be in sorted order
    Private TableRecordArray As TableRecord() = New TableRecord() {New TableRecord(cmapTag), New TableRecord(cvtTag), New TableRecord(fpgmTag), New TableRecord(glyfTag), New TableRecord(headTag), New TableRecord(hheaTag), New TableRecord(hmtxTag), New TableRecord(locaTag), New TableRecord(maxpTag), New TableRecord(prepTag)}

    Private Enum Tag
        cmap
        cvt
        fpgm
        glyf
        head
        hhea
        hmtx
        loca
        maxp
        prep
    End Enum

    Friend Sub New(PdfFont As PdfFont, FirstChar As Integer, LastChar As Integer)
        MyBase.New(PdfFont.Document, ObjectType.Stream)
        ' save input arguments
        Me.PdfFont = PdfFont
        FontInfo = PdfFont.FontApi
        Me.FirstChar = FirstChar
        Me.LastChar = LastChar
        GlyphIndexFont = FirstChar = 0 AndAlso LastChar = 0
        SymbolicFont = PdfFont.SymbolicFont
        CharInfoArray = PdfFont.CharInfoArray

        ' font file
        ObjectValueArray = CreateFontFile()

        ' add font file length (uncompressed)
        Dictionary.AddInteger("/Length1", ObjectValueArray.Length)

        ' debug
        If Document.Debug Then ObjectValueArray = Document.TextToByteArray("*** FONT FILE PLACE HOLDER ***")

        ' write stream
        MyBase.WriteObjectToPdfFile()
        Return
    End Sub

    
    ' Create font file
    

    Private Function CreateFontFile() As Byte()
        ' get file signature
        GetFontFileHeaderApi()

        ' get head table
        GetheadTable()

        ' get horizontal head table
        GethheaTable()

        ' get maximum profile table
        GetmaxpTable()

        ' get character code to glyph code table
        If Not GlyphIndexFont Then GetcmapTable()

        ' get horizontal metrics table
        GethmtxTable()

        ' get glyph code to glyph data location in the table
        GetlocaTable()

        ' get glyph data
        If Not GlyphIndexFont Then
            BuildGlyphArray()
        Else
            BuildGlyphArray1()
        End If

        ' replace old glyph codes with new ones for composite glyphs
        ReplaceGlyphCode()

        ' calculate glyph table checksum
        CalculateGlyphChksum()

        ' build new glyph location table
        BuildLocaTable()

        ' build new character map table
        If Not GlyphIndexFont Then BuildCharMapTable()

        ' build new horizontal metrics table
        BuildhmtxTable()

        ' build new head table
        BuildHeadTable()

        ' build new hhea table
        BuildHheaTable()

        ' build new maxp table
        BuildMaxpTable()

        ' load ctv, fpgm and prep tables
        BuildFontProgramTables()

        ' build font file
        BuildEmbeddedFile()

        ' exit
        Return Buffer
    End Function

    
    ' Get Font Data File header and table records
    

    Private Sub GetFontFileHeaderApi()
        ' read font file header
        Buffer = FontInfo.GetFontDataApi(0, 0, 12)
        BufPtr = 0
        FileHeader = New FontFileHeader()
        FileHeader.FileVersion = ReadUInt32BigEndian()
        FileHeader.NumTables = ReadUInt16BigEndian()

        ' number of bytes to retrieve
        Dim BufSize = 16 * FileHeader.NumTables

        ' read all table records from input file
        Buffer = FontInfo.GetFontDataApi(0, 12, BufSize)
        BufPtr = 0

        ' load table records
        For Table = 0 To FileHeader.NumTables - 1
            ' get table tag (4 bytes)
            Dim TableTag As UInteger = ReadUInt32BigEndian()

            ' search table record
            Dim Index As Integer
            Index = 0

            While Index < TableRecordArray.Length AndAlso TableTag <> TableRecordArray(Index).Tag
                Index += 1
            End While

            ' we do not need this table
            If Index = TableRecordArray.Length Then
                ' skip 12 bytes
                BufPtr += 12
                Continue For
            End If

            ' shortcut
            Dim TR = TableRecordArray(Index)

            ' test for duplicate
            If TR.Length <> 0 Then Throw New ApplicationException("Font file in error duplicate table")

            ' read info for this table
            TR.Checksum = ReadUInt32BigEndian()
            TR.Offset = CInt(ReadUInt32BigEndian())
            TR.Length = CInt(ReadUInt32BigEndian())
        Next

        ' make sure all required tables are available
        ' three tables are optional cvt, fpgm and prep
        ' these tables are programming hints
        For Each TR In TableRecordArray
            If TR.Length = 0 AndAlso TR.Tag <> cvtTag AndAlso TR.Tag <> fpgmTag AndAlso TR.Tag <> prepTag Then Throw New ApplicationException("Required font file table is missing")
        Next

        ' load all tables except for glyf table
        For Each TR In TableRecordArray
            ' load all tables but glyf
            If TR.Tag <> glyfTag Then TR.Data = FontInfo.GetFontDataApi(TR.Tag, 0, TR.Length)
        Next

        ' exit
        Return
    End Sub

    
    ' Read "head" table
    

    Private Sub GetheadTable()
        ' set buffer for decoding
        Buffer = TableRecordArray(Tag.head).Data
        BufPtr = 0

        ' decode head table
        headTable = New headTable()
        headTable.TableVersion = ReadUInt32BigEndian()
        headTable.FontRevision = ReadUInt32BigEndian()
        headTable.ChecksumAdjustment = ReadUInt32BigEndian()
        headTable.MagicNumber = ReadUInt32BigEndian()
        headTable.Flags = ReadUInt16BigEndian()
        headTable.UnitsPerEm = ReadUInt16BigEndian()
        headTable.TimeCreated = ReadInt64BigEndian()
        headTable.TimeModified = ReadInt64BigEndian()
        headTable.xMin = ReadInt16BigEndian()
        headTable.yMin = ReadInt16BigEndian()
        headTable.xMax = ReadInt16BigEndian()
        headTable.yMax = ReadInt16BigEndian()
        headTable.MacStyle = ReadUInt16BigEndian()
        headTable.LowestRecPPEM = ReadUInt16BigEndian()
        headTable.FontDirectionHint = ReadInt16BigEndian()
        headTable.IndexToLocFormat = ReadInt16BigEndian()
        headTable.glyphDataFormat = ReadInt16BigEndian()

        ' exit
        Return
    End Sub

    
    ' Read "hhea" table
    

    Private Sub GethheaTable()
        ' set buffer for decoding
        Buffer = TableRecordArray(Tag.hhea).Data
        BufPtr = 0

        ' decode head table
        hheaTable = New hheaTable()
        hheaTable.TableVersion = ReadUInt32BigEndian()
        hheaTable.Ascender = ReadInt16BigEndian()
        hheaTable.Descender = ReadInt16BigEndian()
        hheaTable.LineGap = ReadInt16BigEndian()
        hheaTable.advanceWidthMax = ReadUInt16BigEndian()
        hheaTable.minLeftSideBearing = ReadInt16BigEndian()
        hheaTable.minRightSideBearing = ReadInt16BigEndian()
        hheaTable.xMaxExtent = ReadInt16BigEndian()
        hheaTable.caretSlopeRise = ReadInt16BigEndian()
        hheaTable.caretSlopeRun = ReadInt16BigEndian()
        hheaTable.caretOffset = ReadInt16BigEndian()
        hheaTable.Reserved1 = ReadInt16BigEndian()
        hheaTable.Reserved2 = ReadInt16BigEndian()
        hheaTable.Reserved3 = ReadInt16BigEndian()
        hheaTable.Reserved4 = ReadInt16BigEndian()
        hheaTable.metricDataFormat = ReadInt16BigEndian()
        hheaTable.numberOfHMetrics = ReadUInt16BigEndian()

        ' exit
        Return
    End Sub

    
    ' Read "maxp" table
    

    Private Sub GetmaxpTable()
        ' set buffer for decoding
        Buffer = TableRecordArray(Tag.maxp).Data
        BufPtr = 0

        ' decode maxp table
        maxpTable = New maxpTable()
        maxpTable.TableVersion = ReadUInt32BigEndian()
        maxpTable.numGlyphs = ReadUInt16BigEndian()
        maxpTable.maxPoints = ReadUInt16BigEndian()
        maxpTable.maxContours = ReadUInt16BigEndian()
        maxpTable.maxCompositePoints = ReadUInt16BigEndian()
        maxpTable.maxCompositeContours = ReadUInt16BigEndian()
        maxpTable.maxZones = ReadUInt16BigEndian()
        maxpTable.maxTwilightPoints = ReadUInt16BigEndian()
        maxpTable.maxStorage = ReadUInt16BigEndian()
        maxpTable.maxFunctionDefs = ReadUInt16BigEndian()
        maxpTable.maxInstructionDefs = ReadUInt16BigEndian()
        maxpTable.maxStackElements = ReadUInt16BigEndian()
        maxpTable.maxSizeOfInstructions = ReadUInt16BigEndian()
        maxpTable.maxComponentElements = ReadUInt16BigEndian()
        maxpTable.maxComponentDepth = ReadUInt16BigEndian()
        Return
    End Sub

    
    ' Read "cmap" table
    

    Private Sub GetcmapTable()
        ' set buffer for decoding
        Buffer = TableRecordArray(Tag.cmap).Data
        BufPtr = 0

        ' create cmap object
        If ReadUInt16BigEndian() <> 0 Then Throw New ApplicationException("CMAP table version number is not zero")
        Dim NumberOfTables As Integer = ReadUInt16BigEndian()
        Dim SubTblArray = New cmapSubTbl(NumberOfTables - 1) {}

        ' loop for tables
        For Index = 0 To NumberOfTables - 1
            Dim SubTbl As cmapSubTbl = New cmapSubTbl()
            SubTblArray(Index) = SubTbl
            SubTbl.PlatformID = ReadUInt16BigEndian()
            SubTbl.EncodingID = ReadUInt16BigEndian()
            SubTbl.Offset = ReadUInt32BigEndian()

            ' save buffer pointer
            Dim SaveBufPtr = BufPtr

            ' set offset
            BufPtr = CInt(SubTbl.Offset)

            ' read format code
            SubTbl.Format = ReadUInt16BigEndian()

            ' process format 0
            If SubTbl.Format = 0 Then
                SubTbl.Length = ReadUInt16BigEndian()
                SubTbl.Language = ReadUInt16BigEndian()
                SubTbl.GlyphArray = New UShort(255) {}

                For Code = 0 To 256 - 1
                    SubTbl.GlyphArray(Code) = Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
                Next

                ' process format 4
            ElseIf SubTbl.Format = 4 Then
                SubTbl.Length = ReadUInt16BigEndian()
                SubTbl.Language = ReadUInt16BigEndian()
                SubTbl.SegCount = CUShort((ReadUInt16BigEndian() / 2))
                BufPtr += 6 ' skip search range, entry selector and range shift
                SubTbl.SegArray = New cmapSeg(SubTbl.SegCount - 1) {}

                For Seg = 0 To SubTbl.SegCount - 1
                    SubTbl.SegArray(Seg) = New cmapSeg(ReadUInt16BigEndian())
                Next ' EndChar

                ReadUInt16BigEndian() ' skip reserved padding

                For Seg = 0 To SubTbl.SegCount - 1
                    SubTbl.SegArray(Seg).StartChar = ReadUInt16BigEndian()
                Next

                For Seg = 0 To SubTbl.SegCount - 1
                    SubTbl.SegArray(Seg).IDDelta = ReadInt16BigEndian()
                Next

                For Seg = 0 To SubTbl.SegCount - 1
                    SubTbl.SegArray(Seg).IDRangeOffset = CUShort((ReadUInt16BigEndian() / 2))
                Next

                Dim GlyphCount As Integer = (SubTbl.Length - 16 - 8 * SubTbl.SegCount) / 2
                SubTbl.GlyphArray = New UShort(GlyphCount - 1) {}

                For Glyph = 0 To GlyphCount - 1
                    SubTbl.GlyphArray(Glyph) = ReadUInt16BigEndian()
                Next
            End If

            ' restore buffer pointer
            BufPtr = SaveBufPtr
        Next

        ' sort table
        Array.Sort(SubTblArray)

        ' select 'best' sub-table for character code to glyph code translation
        cmapSubTbl = SelectcmapSubTable(SubTblArray)

        ' exit
        Return
    End Sub

    
    ' Select best sub-table in "cmap" table
    

    Private Function SelectcmapSubTable(SubTblArray As cmapSubTbl()) As cmapSubTbl
        ' search for platform ID = 3 Windows, encoding ID = 0 or 1 Unicode and format 4
        Dim SearchSubTbl As cmapSubTbl = New cmapSubTbl(3, If(SymbolicFont, 0, 1), 4)
        Dim Index = Array.BinarySearch(SubTblArray, SearchSubTbl)
        If Index >= 0 Then Return SubTblArray(Index)

        ' search for platform ID = 3 Windows, encoding ID = 0 or 1 Unicode and format 0
        SearchSubTbl.Format = 0
        Index = Array.BinarySearch(SubTblArray, SearchSubTbl)
        If Index >= 0 Then Return SubTblArray(Index)

        ' not found
        Throw New ApplicationException("Required cmap sub-table is missing")
    End Function

    
    ' Read "hmtx" table
    

    Private Sub GethmtxTable()
        ' set buffer for decoding
        Buffer = TableRecordArray(Tag.hmtx).Data
        BufPtr = 0

        ' create table for advance width 
        hmtxTable = New UShort(hheaTable.numberOfHMetrics - 1) {}

        ' read long horizontal metric array
        ' the program ignores the left side bearing values
        ' in the new table the left side bearing will be taken from xMin
        Dim Index As Integer

        For Index = 0 To hheaTable.numberOfHMetrics - 1
            hmtxTable(Index) = ReadUInt16BigEndian()
            BufPtr += 2
        Next

        ' exit
        Return
    End Sub

    
    ' Read "loca" table
    

    Private Sub GetlocaTable()
        ' set buffer for decoding
        Buffer = TableRecordArray(Tag.loca).Data
        BufPtr = 0

        ' calculate size based on table length
        Dim TblSize As Integer = If(headTable.IndexToLocFormat = 0, Buffer.Length / 2, Buffer.Length / 4)

        ' allocate array
        locaTable = New Integer(TblSize - 1) {}

        ' load short table
        If headTable.IndexToLocFormat = 0 Then

            ' long format
            For Index = 0 To TblSize - 1
                locaTable(Index) = 2 * ReadUInt16BigEndian()
            Next
        Else

            For Index = 0 To TblSize - 1
                locaTable(Index) = CInt(ReadUInt32BigEndian())
            Next
        End If

        ' exit
        Return
    End Sub

    
    ' Build glyph array for character range
    

    Private Sub BuildGlyphArray()
        ' create character code to glyph code array
        CharToGlyphArray = New UShort(LastChar - FirstChar + 1 - 1) {}

        ' reset bounding box in head table
        headTable.xMin = Short.MaxValue
        headTable.yMin = Short.MaxValue
        headTable.xMax = Short.MinValue
        headTable.yMax = Short.MinValue

        ' reset some values in horizontal matrix header table
        hheaTable.advanceWidthMax = UShort.MinValue
        hheaTable.minLeftSideBearing = Short.MaxValue
        hheaTable.minRightSideBearing = Short.MaxValue
        hheaTable.xMaxExtent = Short.MinValue

        ' create a temp list of components glyph codes of composite glyphs
        Dim CompList As List(Of Integer) = New List(Of Integer)()

        ' create a glyph data list
        Dim GlyphList = CreateGlyphDataList(CompList)

        ' loop for all possible characters of row zero
        Dim ZeroRow = CharInfoArray(0)

        For Col = FirstChar To LastChar
            ' one char short cut
            Dim CharInfo = ZeroRow(Col)

            ' character is not active
            If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then Continue For

            ' this old glyph index is in the list already (two character codes withe the same glyph)
            Dim Index = GlyphList.BinarySearch(CharInfo)

            If Index >= 0 Then
                ' set new glyph index
                CharInfo.NewGlyphIndex = GlyphList(Index).NewGlyphIndex

                ' save new glyph number in CharToGlyph array
                CharToGlyphArray(Col - FirstChar) = CUShort(CharInfo.NewGlyphIndex)
                Continue For
            End If

            ' set new glyph number for active characters
            CharInfo.NewGlyphIndex = GlyphList.Count

            ' add it to the glyph list 
            GlyphList.Insert(Not Index, CharInfo)

            ' save new glyph number in CharToGlyph array
            CharToGlyphArray(Col - FirstChar) = CUShort(CharInfo.NewGlyphIndex)

            ' add char/glyph to GlyphList
            AddGlyph(CharInfo, GlyphList, CompList)
        Next

        ' add composite glyphs
        If CompList.Count <> 0 Then AddCompositeGlyphs(GlyphList, CompList)

        ' convert list to array		
        GlyphArray = GlyphList.ToArray()

        ' save number of glyphs in maxpTable
        maxpTable.numGlyphs = CUShort(GlyphArray.Length)

        ' exit
        Return
    End Sub

    
    ' Build glyph array for character range
    

    Private Sub BuildGlyphArray1()
        ' reset bounding box in head table
        headTable.xMin = Short.MaxValue
        headTable.yMin = Short.MaxValue
        headTable.xMax = Short.MinValue
        headTable.yMax = Short.MinValue

        ' reset some values in horizontal matrix header table
        hheaTable.advanceWidthMax = UShort.MinValue
        hheaTable.minLeftSideBearing = Short.MaxValue
        hheaTable.minRightSideBearing = Short.MaxValue
        hheaTable.xMaxExtent = Short.MinValue

        ' create a temp list of components glyph codes of composite glyphs
        Dim CompList As List(Of Integer) = New List(Of Integer)()

        ' create a glyph data list
        Dim GlyphList = CreateGlyphDataList(CompList)

        ' loop for all characters
        For Row = 1 To 256 - 1
            ' get one row of char info
            Dim OneRow = CharInfoArray(Row)
            If OneRow Is Nothing Then Continue For

            For Col = 0 To 256 - 1
                ' get one char info
                Dim CharInfo = OneRow(Col)
                If CharInfo Is Nothing OrElse Not CharInfo.ActiveChar Then Continue For

                ' this old glyph index is in the list already (two character codes withe the same glyph)
                Dim Index = GlyphList.BinarySearch(CharInfo)

                If Index >= 0 Then
                    ' we have two char with the same old glyph number but two different new glyph number
                    GlyphList.Insert(Index, CharInfo)
                Else
                    ' add it to the glyph list 
                    GlyphList.Insert(Not Index, CharInfo)
                End If

                ' add char/glyph to glyph list
                AddGlyph(CharInfo, GlyphList, CompList)
            Next
        Next

        ' add composite glyphs
        If CompList.Count <> 0 Then AddCompositeGlyphs(GlyphList, CompList)

        ' convert list to array		
        GlyphArray = GlyphList.ToArray()

        ' save number of glyphs in maxpTable
        maxpTable.numGlyphs = CUShort(GlyphArray.Length)

        ' exit
        Return
    End Sub

    
    ' create a glyph data list
    

    Private Function CreateGlyphDataList(CompList As List(Of Integer)) As List(Of CharInfo)
        ' create a glyph data list
        Dim GlyphList As List(Of CharInfo) = New List(Of CharInfo)()

        ' glyphs zero, one and two are reserved
        GlyphList.Add(PdfFont.UndefinedCharInfo)
        AddGlyph(PdfFont.UndefinedCharInfo, GlyphList, CompList)
        Dim CharInfo = FontInfo.GetGlyphMetricsApiByGlyphIndex(1)
        CharInfo.NewGlyphIndex = 1
        GlyphList.Add(CharInfo)
        AddGlyph(CharInfo, GlyphList, CompList)
        CharInfo = FontInfo.GetGlyphMetricsApiByGlyphIndex(2)
        CharInfo.NewGlyphIndex = 2
        GlyphList.Add(CharInfo)
        AddGlyph(CharInfo, GlyphList, CompList)
        Return GlyphList
    End Function

    
    ' add additional glyphs from the composite glyphs to the list
    

    Private Sub AddCompositeGlyphs(GlyphList As List(Of CharInfo), ExtraList As List(Of Integer))
        ' create a temp list of components of composite glyphs
        Dim CompList As List(Of Integer) = New List(Of Integer)()

        ' loop for all characters
        For Each GlyphIndex In ExtraList
            ' test if this old glyph index is already in the list
            Dim Index As Integer = GlyphList.BinarySearch(New CharInfo(GlyphIndex))
            If Index >= 0 Then Continue For

            ' create new char info with no char code
            Dim CharInfo = FontInfo.GetGlyphMetricsApiByGlyphIndex(GlyphIndex)

            ' create new glyph number
            CharInfo.NewGlyphIndex = If(GlyphIndexFont, stdNum.Min(Threading.Interlocked.Increment(PdfFont.NewGlyphIndex), PdfFont.NewGlyphIndex - 1), GlyphList.Count)

            ' add it to the glyph list 
            GlyphList.Insert(Not Index, CharInfo)

            ' add some info
            AddGlyph(CharInfo, GlyphList, CompList)
        Next

        ' add extra glyphs
        If CompList.Count <> 0 Then AddCompositeGlyphs(GlyphList, CompList)

        ' exit
        Return
    End Sub

    
    ' add additional glyphs to the list
    

    Private Sub AddGlyph(CharInfo As CharInfo, GlyphList As List(Of CharInfo), CompList As List(Of Integer))
        ' find glyph location and length within this table
        Dim GlyphLoc = locaTable(CharInfo.GlyphIndex)
        Dim GlyphLen = locaTable(CharInfo.GlyphIndex + 1) - GlyphLoc

        ' load glyph data
        Buffer = FontInfo.GetFontDataApi(glyfTag, GlyphLoc, GlyphLen)
        BufPtr = 0

        ' save glyph data block
        CharInfo.GlyphData = Buffer

        ' blank glyph
        If Buffer Is Nothing Then Return

        ' decode number of contours
        Dim Contours As Short = ReadInt16BigEndian()
        CharInfo.Composite = Contours < 0

        ' bounding box
        Dim xMin As Short = CharInfo.DesignBBoxLeft
        Dim yMin As Short = CharInfo.DesignBBoxBottom
        Dim xMax As Short = CharInfo.DesignBBoxRight
        Dim yMax As Short = CharInfo.DesignBBoxTop

        ' update head table
        If xMin < headTable.xMin Then headTable.xMin = xMin
        If yMin < headTable.yMin Then headTable.yMin = yMin
        If xMax > headTable.xMax Then headTable.xMax = xMax
        If yMax > headTable.yMax Then headTable.yMax = yMax

        ' update hhea table
        If CharInfo.DesignWidth > hheaTable.advanceWidthMax Then hheaTable.advanceWidthMax = CUShort(CharInfo.DesignWidth)
        If xMin < hheaTable.minLeftSideBearing Then hheaTable.minLeftSideBearing = xMin
        Dim Rsb As Short = CharInfo.DesignWidth - xMax
        If Rsb < hheaTable.minRightSideBearing Then hheaTable.minRightSideBearing = Rsb
        If xMax > hheaTable.xMaxExtent Then hheaTable.xMaxExtent = xMax

        ' add component glyphs of a composite glyph to the list
        If Contours < 0 Then GetCompositeGlyph(GlyphList, CompList)

        ' exit
        Return
    End Sub

    
    ' Read one composite glyph from "glyf" table
    

    Private Sub GetCompositeGlyph(MainList As List(Of CharInfo), CompList As List(Of Integer))
        ' the glyph is not in main or composit lists, add it to the composit list
        Dim Index As i32 = 0

        ' skip boundig box
        BufPtr = 10

        ' loop for components glyphs
        While True
            ' read flags and glyph code
            Dim Flags As CompFlag = CType(ReadUInt16BigEndian(), CompFlag)
            Dim GlyphIndex As Integer = ReadUInt16BigEndian()

            If MainList.BinarySearch(New CharInfo(GlyphIndex)) < 0 AndAlso (Index = CompList.BinarySearch(GlyphIndex)) < 0 Then
                CompList.Insert(Not Index, GlyphIndex)
            End If

            ' read argument1 and 2
            If (Flags And CompFlag.Arg1AndArg2AreWords) = 0 Then
                BufPtr += 2
            Else
                BufPtr += 4
            End If

            ' we have one scale factor
            If (Flags And CompFlag.WeHaveAScale) <> 0 Then
                BufPtr += 2

                ' we have two scale factors
            ElseIf (Flags And CompFlag.WeHaveXYScale) <> 0 Then
                BufPtr += 4

                ' we have a transformation matrix
            ElseIf (Flags And CompFlag.WeHave2By2) <> 0 Then
                BufPtr += 8
            End If

            ' no more components
            If (Flags And CompFlag.MoreComponents) = 0 Then Exit While
        End While

        Return
    End Sub

    
    ' Read one composite glyph from "glyf" table
    

    Private Sub ReplaceGlyphCode()
        ' loop looking for composite glyphs
        For Each CharInfo In GlyphArray
            ' not a composite glyph
            If Not CharInfo.Composite Then Continue For

            ' get buffer
            Buffer = CharInfo.GlyphData
            BufPtr = 10

            ' loop for components glyphs
            While True
                ' read flags and old glyph code
                Dim Flags As CompFlag = CType(ReadUInt16BigEndian(), CompFlag)
                Dim GlyphIndex As Integer = ReadUInt16BigEndian()

                ' translate old glyph code to new one
                Dim Index As Integer = Array.BinarySearch(GlyphArray, New CharInfo(GlyphIndex))
                If Index < 0 Then Throw New ApplicationException("Composite glyph number change")

                ' replace glyph code
                BufPtr -= 2
                WriteUInt16BigEndian(CUShort(GlyphArray(Index).NewGlyphIndex))

                ' read argument1 and 2
                If (Flags And CompFlag.Arg1AndArg2AreWords) = 0 Then
                    BufPtr += 2
                Else
                    BufPtr += 4
                End If

                ' we have one scale factor
                If (Flags And CompFlag.WeHaveAScale) <> 0 Then
                    BufPtr += 2

                    ' we have two scale factors
                ElseIf (Flags And CompFlag.WeHaveXYScale) <> 0 Then
                    BufPtr += 4

                    ' we have a transformation matrix
                ElseIf (Flags And CompFlag.WeHave2By2) <> 0 Then
                    BufPtr += 8
                End If

                ' no more components
                If (Flags And CompFlag.MoreComponents) = 0 Then Exit While
            End While
        Next

        Return
    End Sub

    
    ' Calculate "glyf" table checksum
    

    Private Sub CalculateGlyphChksum()
        Dim Checksum As UInteger = 0
        Dim Ptr As i32 = 0

        ' loop for all glyphs
        For Each CharInfo In GlyphArray

            If CharInfo.GlyphData IsNot Nothing Then
                For Each B In CharInfo.GlyphData
                    Checksum += CUInt(B) << 24 - 8 * (++Ptr And 3)
                Next
            End If
        Next

        ' save total length in table record array
        TableRecordArray(Tag.glyf).Length = Ptr

        ' save checksum
        TableRecordArray(Tag.glyf).Checksum = Checksum
        Return
    End Sub

    
    ' build new glyph data file location table
    

    Private Sub BuildLocaTable()
        ' create location array
        Dim LocArray = New Integer(GlyphArray.Length + 1 - 1) {}

        ' reset new glyph table length
        Dim GlyphTableLength = 0

        ' sort by new glyph
        Array.Sort(GlyphArray, New SortByNewIndex())

        ' loop for all glyphs
        For Each CharInfo In GlyphArray
            ' save file location in array
            LocArray(CharInfo.NewGlyphIndex) = GlyphTableLength
            If (GlyphTableLength And 1) <> 0 Then Throw New ApplicationException("Glyph table length must be even")

            ' update file location (for non blank glyphs)
            If CharInfo.GlyphData IsNot Nothing Then GlyphTableLength += CharInfo.GlyphData.Length
        Next

        ' save final length at the last array location
        LocArray(GlyphArray.Length) = GlyphTableLength

        ' save it in table record
        If TableRecordArray(Tag.glyf).Length <> GlyphTableLength Then Throw New ApplicationException("Glyph table length does not match header")

        ' test if the table can be stored in short integer
        headTable.IndexToLocFormat = If((GlyphTableLength And &HFFFE0000) = 0, CShort(0), CShort(1))

        ' replace location array
        If headTable.IndexToLocFormat = 0 Then
            ' short format
            Buffer = New Byte(2 * LocArray.Length - 1) {}
            BufPtr = 0

            For Each Loc As Integer In LocArray
                WriteInt16BigEndian(Loc >> 1)
            Next
        Else
            ' long format
            Buffer = New Byte(4 * LocArray.Length - 1) {}
            BufPtr = 0

            For Each Loc As Integer In LocArray
                WriteUInt32BigEndian(Loc)
            Next
        End If

        ' save in table record array
        TableRecordArray(Tag.loca).Data = Buffer

        ' calculate checksum
        TableRecordArray(Tag.loca).Checksum = TableChecksum(Buffer)

        ' exit 
        Return
    End Sub

    
    ' build new cmap table
    

    Private Sub BuildCharMapTable()
        ' create a new cmap sub table
        Dim NewSubTbl As cmapSubTbl = New cmapSubTbl(cmapSubTbl.PlatformID, cmapSubTbl.EncodingID, 4)
        NewSubTbl.Language = cmapSubTbl.Language
        NewSubTbl.SegCount = 2
        NewSubTbl.SegArray = New cmapSeg(1) {}
        NewSubTbl.GlyphArray = CharToGlyphArray

        ' test type of font
        If cmapSubTbl.EncodingID <> 0 Then
            ' alphabetic font
            NewSubTbl.SegArray(0) = New cmapSeg(FirstChar, LastChar, 0, 2)
        Else
            ' symbolic font
            NewSubTbl.SegArray(0) = New cmapSeg(&HF000 + FirstChar, &HF000 + LastChar, 0, 2)
        End If

        NewSubTbl.SegArray(1) = New cmapSeg(&HFFFF, &HFFFF, 1, 0)

        ' table size
        Dim TblSize = 4 + 8 + 16 + 8 * NewSubTbl.SegCount + 2 * CharToGlyphArray.Length
        Buffer = New Byte(TblSize - 1) {}
        BufPtr = 0

        ' table version number is 0
        WriteUInt16BigEndian(0)

        ' number of tables is 1
        WriteUInt16BigEndian(1)

        ' platform id
        WriteUInt16BigEndian(NewSubTbl.PlatformID)

        ' encoding id
        WriteUInt16BigEndian(NewSubTbl.EncodingID)

        ' offset
        WriteUInt32BigEndian(4 + 8)

        ' format
        WriteUInt16BigEndian(NewSubTbl.Format)

        ' table length
        WriteInt16BigEndian(16 + 8 * NewSubTbl.SegCount + 2 * CharToGlyphArray.Length)

        ' language
        WriteUInt16BigEndian(NewSubTbl.Language)

        ' segment count times 2
        WriteInt16BigEndian(NewSubTbl.SegCount * 2)

        ' search range
        WriteUInt16BigEndian(NewSubTbl.SearchRange)

        ' entry selector
        WriteUInt16BigEndian(NewSubTbl.EntrySelector)

        ' range shift
        WriteUInt16BigEndian(NewSubTbl.RangeShift)

        ' end character
        For Seg = 0 To NewSubTbl.SegCount - 1
            WriteUInt16BigEndian(NewSubTbl.SegArray(Seg).EndChar)
        Next

        ' padding
        WriteUInt16BigEndian(0)

        ' start character
        For Seg = 0 To NewSubTbl.SegCount - 1
            WriteUInt16BigEndian(NewSubTbl.SegArray(Seg).StartChar)
        Next

        ' IDDelta
        For Seg = 0 To NewSubTbl.SegCount - 1
            WriteInt16BigEndian(NewSubTbl.SegArray(Seg).IDDelta)
        Next

        ' IDRangeOffset
        For Seg = 0 To NewSubTbl.SegCount - 1
            WriteUInt16BigEndian(CUShort(NewSubTbl.SegArray(Seg).IDRangeOffset * 2))
        Next

        ' char to glyph translation
        For Glyph = 0 To NewSubTbl.GlyphArray.Length - 1
            WriteUInt16BigEndian(NewSubTbl.GlyphArray(Glyph))
        Next

        ' save
        TableRecordArray(Tag.cmap).Data = Buffer

        ' calculate checksum
        TableRecordArray(Tag.cmap).Checksum = TableChecksum(Buffer)

        ' exit
        Return
    End Sub

    
    ' Build new hmtx table
    

    Private Sub BuildhmtxTable()
        ' number of advance width and left bearing pairs
        Dim HMSize = GlyphArray.Length - 1
        Dim AdvanceWidth = GlyphArray(HMSize).DesignWidth
        HMSize -= 1

        While HMSize >= 0 AndAlso GlyphArray(HMSize).DesignWidth = AdvanceWidth
            HMSize -= 1
        End While

        HMSize += 2

        ' calculate size of new table
        Dim TableSize = 4 * HMSize
        If HMSize < GlyphArray.Length Then TableSize += 2 * (GlyphArray.Length - HMSize)

        ' allocate buffer
        Buffer = New Byte(TableSize - 1) {}
        BufPtr = 0

        ' output advance width and left bearing pairs
        Dim Index As Integer

        For Index = 0 To HMSize - 1
            WriteUInt16BigEndian(CUShort(GlyphArray(Index).DesignWidth))
            WriteInt16BigEndian(GlyphArray(Index).DesignBBoxLeft)
        Next

        ' output left bearing pairs
        While Index < GlyphArray.Length
            WriteInt16BigEndian(GlyphArray(Index).DesignBBoxLeft)
            Index += 1
        End While

        ' save number of advance width and left bearing pairs
        hheaTable.numberOfHMetrics = CUShort(HMSize)

        ' save in table record array
        TableRecordArray(Tag.hmtx).Data = Buffer

        ' calculate checksum
        TableRecordArray(Tag.hmtx).Checksum = TableChecksum(Buffer)

        ' exit
        Return
    End Sub

    
    ' build new header table
    ' must be after BuildGlyphLocationTable()
    

    Private Sub BuildHeadTable()
        ' allocate buffer for head table
        Buffer = New Byte(53) {}
        BufPtr = 0

        ' move info into buffer
        WriteUInt32BigEndian(headTable.TableVersion)
        WriteUInt32BigEndian(headTable.FontRevision)
        WriteUInt32BigEndian(0)
        WriteUInt32BigEndian(headTable.MagicNumber)
        WriteUInt16BigEndian(headTable.Flags)
        WriteUInt16BigEndian(headTable.UnitsPerEm)
        WriteInt64BigEndian(headTable.TimeCreated)
        WriteInt64BigEndian(headTable.TimeModified)
        WriteInt16BigEndian(headTable.xMin)
        WriteInt16BigEndian(headTable.yMin)
        WriteInt16BigEndian(headTable.xMax)
        WriteInt16BigEndian(headTable.yMax)
        WriteUInt16BigEndian(headTable.MacStyle)
        WriteUInt16BigEndian(headTable.LowestRecPPEM)
        WriteInt16BigEndian(headTable.FontDirectionHint)
        WriteInt16BigEndian(headTable.IndexToLocFormat)
        WriteInt16BigEndian(headTable.glyphDataFormat)

        ' save in table record array
        TableRecordArray(Tag.head).Data = Buffer

        ' calculate checksum
        TableRecordArray(Tag.head).Checksum = TableChecksum(Buffer)

        ' exit
        Return
    End Sub

    
    ' Build new "hhea" table
    

    Private Sub BuildHheaTable()
        ' allocate buffer
        Buffer = New Byte(35) {}
        BufPtr = 0

        ' build new hhea table
        WriteUInt32BigEndian(hheaTable.TableVersion)
        WriteInt16BigEndian(hheaTable.Ascender)
        WriteInt16BigEndian(hheaTable.Descender)
        WriteInt16BigEndian(hheaTable.LineGap)
        WriteUInt16BigEndian(hheaTable.advanceWidthMax)
        WriteInt16BigEndian(hheaTable.minLeftSideBearing)
        WriteInt16BigEndian(hheaTable.minRightSideBearing)
        WriteInt16BigEndian(hheaTable.xMaxExtent)
        WriteInt16BigEndian(hheaTable.caretSlopeRise)
        WriteInt16BigEndian(hheaTable.caretSlopeRun)
        WriteInt16BigEndian(hheaTable.caretOffset)
        WriteInt16BigEndian(hheaTable.Reserved1)
        WriteInt16BigEndian(hheaTable.Reserved2)
        WriteInt16BigEndian(hheaTable.Reserved3)
        WriteInt16BigEndian(hheaTable.Reserved4)
        WriteInt16BigEndian(hheaTable.metricDataFormat)
        WriteUInt16BigEndian(hheaTable.numberOfHMetrics)

        ' save in table record array
        TableRecordArray(Tag.hhea).Data = Buffer

        ' calculate checksum
        TableRecordArray(Tag.hhea).Checksum = TableChecksum(Buffer)

        ' exit
        Return
    End Sub

    
    ' Read "maxp" table
    

    Private Sub BuildMaxpTable()
        ' allocate buffer
        Buffer = New Byte(31) {}
        BufPtr = 0

        ' build new hhea table
        WriteUInt32BigEndian(maxpTable.TableVersion)
        WriteUInt16BigEndian(maxpTable.numGlyphs)
        WriteUInt16BigEndian(maxpTable.maxPoints)
        WriteUInt16BigEndian(maxpTable.maxContours)
        WriteUInt16BigEndian(maxpTable.maxCompositePoints)
        WriteUInt16BigEndian(maxpTable.maxCompositeContours)
        WriteUInt16BigEndian(maxpTable.maxZones)
        WriteUInt16BigEndian(maxpTable.maxTwilightPoints)
        WriteUInt16BigEndian(maxpTable.maxStorage)
        WriteUInt16BigEndian(maxpTable.maxFunctionDefs)
        WriteUInt16BigEndian(maxpTable.maxInstructionDefs)
        WriteUInt16BigEndian(maxpTable.maxStackElements)
        WriteUInt16BigEndian(maxpTable.maxSizeOfInstructions)
        WriteUInt16BigEndian(maxpTable.maxComponentElements)
        WriteUInt16BigEndian(maxpTable.maxComponentDepth)

        ' save in table record array
        TableRecordArray(Tag.maxp).Data = Buffer

        ' calculate checksum
        TableRecordArray(Tag.maxp).Checksum = TableChecksum(Buffer)

        ' exit
        Return
    End Sub

    
    ' build new font program tables
    

    Private Sub BuildFontProgramTables()
        ' recalculate checksum
        ' in some cases the calculated checksum does not agree with the one returned by the api
        If TableRecordArray(Tag.cvt).Length <> 0 Then TableRecordArray(Tag.cvt).Checksum = TableChecksum(TableRecordArray(Tag.cvt).Data)
        If TableRecordArray(Tag.fpgm).Length <> 0 Then TableRecordArray(Tag.fpgm).Checksum = TableChecksum(TableRecordArray(Tag.fpgm).Data)
        If TableRecordArray(Tag.prep).Length <> 0 Then TableRecordArray(Tag.prep).Checksum = TableChecksum(TableRecordArray(Tag.prep).Data)
        Return
    End Sub

    
    ' build new font file
    

    Private Function BuildEmbeddedFile() As Byte()
        ' cmap is not required for type0 fonts
        If GlyphIndexFont Then
            TableRecordArray(Tag.cmap).Offset = 0
            TableRecordArray(Tag.cmap).Length = 0
        End If

        ' replace number of tables in file header
        Dim Tables = 0

        For Each TR In TableRecordArray
            If TR.Length <> 0 Then Tables += 1
        Next

        FileHeader.NumTables = CUShort(Tables)

        ' allocate buffer for file header plus table records
        Dim HeaderSize = 12 + 16 * Tables
        Buffer = New Byte(HeaderSize - 1) {}
        BufPtr = 0

        ' write file header to embedded file
        WriteUInt32BigEndian(FileHeader.FileVersion)
        WriteUInt16BigEndian(FileHeader.NumTables)
        WriteUInt16BigEndian(FileHeader.SearchRange)
        WriteUInt16BigEndian(FileHeader.EntrySelector)
        WriteUInt16BigEndian(FileHeader.RangeShift)

        ' table offset
        Dim FileLength = HeaderSize

        ' reset file checksum
        Dim ChecksumAdjustment As UInteger = 0

        ' write table record array
        For Each TR In TableRecordArray
            ' skip unused table
            If TR.Length = 0 Then Continue For

            ' table tag
            WriteUInt32BigEndian(TR.Tag)

            ' table checksum
            WriteUInt32BigEndian(TR.Checksum)
            ChecksumAdjustment += TR.Checksum

            ' file offset
            WriteUInt32BigEndian(FileLength)
            TR.Offset = FileLength

            ' length of actual data
            Dim Length = If(TR.Tag <> glyfTag, TR.Data.Length, TR.Length)
            WriteUInt32BigEndian(Length)

            ' make sure offset is on 4 bytes boundry
            FileLength += Length + 3 And Not 3
        Next

        ' calculate checksum of header plus table records
        ChecksumAdjustment = &HB1B0AFBA - (ChecksumAdjustment + TableChecksum(Buffer))

        ' save header buffer
        Dim Header = Buffer

        ' allocate buffer for full size file
        Buffer = New Byte(FileLength - 1) {}

        ' copy header to buffer
        Array.Copy(Header, Buffer, Header.Length)
        BufPtr = Header.Length

        ' we do not need header buffer
        Header = Nothing

        ' write tables
        For Each TR In TableRecordArray
            ' skip unused table
            If TR.Length = 0 Then Continue For

            ' test program logic
            If BufPtr <> TR.Offset Then Throw New ApplicationException("Table offset")

            ' all tables but glyph
            If TR.Tag <> glyfTag Then
                Array.Copy(TR.Data, 0, Buffer, BufPtr, TR.Data.Length)

                ' glyph table
                BufPtr += TR.Data.Length
            Else

                For Each CharInfo In GlyphArray
                    If CharInfo.GlyphData Is Nothing Then Continue For
                    Array.Copy(CharInfo.GlyphData, 0, Buffer, BufPtr, CharInfo.GlyphData.Length)
                    BufPtr += CharInfo.GlyphData.Length
                Next
            End If

            ' make sure buffer pointer is on 4 bytes boundry
            While (BufPtr And 3) <> 0
                Buffer(BufPtr) = 0
                BufPtr += 1
            End While
        Next

        If BufPtr <> FileLength Then Throw New ApplicationException("Table offset")

        ' insert checksum adjustment to head table
        BufPtr = TableRecordArray(Tag.head).Offset + 8
        WriteUInt32BigEndian(ChecksumAdjustment)

        ' write 
        Return Buffer
    End Function

    
    ' Read short from byte array big endian style
    

    Private Function ReadInt16BigEndian() As Short
        Return Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) << 8 Or Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
    End Function

    
    ' Read ushort from byte array big endian style
    

    Private Function ReadUInt16BigEndian() As UShort
        Return CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 8 Or Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
    End Function

    
    ' Read uint from byte array big endian style
    

    Private Function ReadUInt32BigEndian() As UInteger
        Return CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 24 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 16 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 8 Or Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
    End Function

    
    ' Read long from byte array big endian style
    

    Private Function ReadInt64BigEndian() As Long
        Return CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 56 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 48 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 40 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 32 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 24 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 16 Or CUInt(Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))) << 8 Or Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1))
    End Function

    
    ' Write short to byte list big endian style
    

    Private Sub WriteInt16BigEndian(Value As Integer)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 8)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value)
        Return
    End Sub

    
    ' Write short or ushort to byte list big endian style
    

    Private Sub WriteUInt16BigEndian(Value As UInteger)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 8)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value)
        Return
    End Sub

    
    ' Write int or uint to byte list big endian style
    

    Private Sub WriteUInt32BigEndian(Value As UInteger)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 24)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 16)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 8)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value)
        Return
    End Sub

    
    ' Write long or ulong to byte list big endian style
    

    Private Sub WriteInt64BigEndian(Value As Long)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 56)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 48)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 40)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 32)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 24)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 16)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value >> 8)
        Buffer(stdNum.Min(Threading.Interlocked.Increment(BufPtr), BufPtr - 1)) = CByte(Value)
        Return
    End Sub

    
    ' Calculate table checksum
    

    Private Function TableChecksum(Table As Byte()) As UInteger
        Dim ChkSum As UInteger = 0

        For Ptr = 0 To Table.Length - 1
            ChkSum += CUInt(Table(Ptr)) << 24 - 8 * (Ptr And 3)
        Next

        Return ChkSum
    End Function

    
    ' convert table tag from binary to string
    

    Private Shared Function TagBinToStr(BinTag As UInteger) As String
        Dim StrTag As StringBuilder = New StringBuilder("????")

        For Index = 0 To 4 - 1
            Dim Ch As Byte = BinTag >> 24 - 8 * Index
            If Ch >= 32 AndAlso Ch <= 126 Then StrTag(Index) = Microsoft.VisualBasic.ChrW(Ch)
        Next

        Return StrTag.ToString()
    End Function
End Class
