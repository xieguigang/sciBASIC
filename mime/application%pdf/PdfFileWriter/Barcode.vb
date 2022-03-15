#Region "Microsoft.VisualBasic::565354d0b8ebd83ac5b0c53653b9bd5d, sciBASIC#\mime\application%pdf\PdfFileWriter\Barcode.vb"

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

    '   Total Lines: 1632
    '    Code Lines: 741
    ' Comment Lines: 628
    '   Blank Lines: 263
    '     File Size: 58.29 KB


    ' Class BarcodeBox
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    ' Class Barcode
    ' 
    '     Properties: BarCount, CodeArray, Text, TotalWidth
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: BarWidth, GetBarcodeBox
    ' 
    ' Class Barcode128
    ' 
    ' 
    '     Enum CodeSet
    ' 
    '         CodeA, CodeB, CodeC, ShiftA, ShiftB
    '         Undefined
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: BarWidth
    ' 
    '     Sub: Checksum, EncodeDigits, EncodeNonDigits
    ' 
    ' Class Barcode39
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: BarWidth
    ' 
    ' Class BarcodeEAN13
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: BarWidth, GetBarcodeBox
    ' 
    '     Sub: Checksum
    ' 
    ' Class BarcodeInterleaved2of5
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BarWidth
    ' 
    '     Sub: Checksum
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	Barcode
'	Single diminsion barcode class.
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
Imports System.Text
Imports stdNum = System.Math

''' <summary>
''' Barcode box class
''' </summary>
''' <remarks>
''' The barcode box class represent the total size of the barcode
''' plus optional text. It is used by PdfTable class.
''' </remarks>
Public Class BarcodeBox
    ''' <summary>
    ''' Barcode origin X
    ''' </summary>
    Public OriginX As Double

    ''' <summary>
    ''' Barcode origin Y
    ''' </summary>
    Public OriginY As Double

    ''' <summary>
    ''' Total width including optional text.
    ''' </summary>
    Public TotalWidth As Double

    ''' <summary>
    ''' Total height including optional text.
    ''' </summary>
    Public TotalHeight As Double

    ''' <summary>
    ''' Constructor for no text
    ''' </summary>
    ''' <param name="TotalWidth">Total width</param>
    ''' <param name="TotalHeight">Total height</param>
    Public Sub New(TotalWidth As Double, TotalHeight As Double)
        Me.TotalWidth = TotalWidth
        Me.TotalHeight = TotalHeight
        Return
    End Sub

    ''' <summary>
    ''' Constructor for text included
    ''' </summary>
    ''' <param name="OriginX">Barcode origin X</param>
    ''' <param name="OriginY">Barcode origin Y</param>
    ''' <param name="TotalWidth">Total width</param>
    ''' <param name="TotalHeight">Total height</param>
    Public Sub New(OriginX As Double, OriginY As Double, TotalWidth As Double, TotalHeight As Double)
        Me.OriginX = OriginX
        Me.OriginY = OriginY
        Me.TotalWidth = TotalWidth
        Me.TotalHeight = TotalHeight
        Return
    End Sub
End Class

''' <summary>
''' One dimension barcode base class
''' </summary>
''' <remarks>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#BarcodeSupport">2.5 Barcode Support</a>
''' </para>
''' </remarks>
Public Class Barcode

    ''' <summary>
    ''' Gets a copy of CodeArray
    ''' </summary>
    Public ReadOnly Property CodeArray As Integer()
        Get
            Return CType(_CodeArray.Clone(), Integer())
        End Get
    End Property

    Friend _CodeArray As Integer()

    ''' <summary>
    ''' Text string
    ''' </summary>
    Public Property Text As String

    ''' <summary>
    ''' Total number of black and white bars
    ''' </summary>
    Public Property BarCount As Integer

    ''' <summary>
    ''' Total barcode width in narrow bar units.
    ''' </summary>
    Public Property TotalWidth As Integer

    ''' <summary>
    ''' Protected barcode constructor
    ''' </summary>
    ''' <remarks>This class cannot be instantiated by itself.</remarks>
    Protected Sub New()
    End Sub

    ''' <summary>
    ''' Width of single bar code at indexed position expressed in narrow bar units.
    ''' </summary>
    ''' <param name="Index">Bar's index number.</param>
    ''' <returns>Bar's width in narrow bar units.</returns>
    ''' <remarks>This virtual function must be implemented by derived class 
    ''' Index range is 0 to BarCount - 1</remarks>
    Public Overridable Function BarWidth(Index As Integer) As Integer
        Throw New ApplicationException("Barcode.BarWidth: Not defined in derived class")
    End Function

    ''' <summary>
    ''' Calculate total barcode height including text
    ''' </summary>
    ''' <param name="BarWidth">Narrow bar width</param>
    ''' <param name="BarcodeHeight">Barcode height</param>
    ''' <param name="TextFont">Text font</param>
    ''' <param name="FontSize">Text font size</param>
    ''' <returns>BarcodeBox result</returns>
    Public Overridable Function GetBarcodeBox(BarWidth As Double, BarcodeHeight As Double, TextFont As PdfFont, FontSize As Double) As BarcodeBox
        ' no text
        If TextFont Is Nothing Then Return New BarcodeBox(BarWidth * TotalWidth, BarcodeHeight)

        ' calculate width
        Dim BarcodeWidth = BarWidth * TotalWidth
        Dim TextWidth = TextFont.TextWidth(FontSize, Text)
        Dim OriginX As Double = 0

        If TextWidth > BarcodeWidth Then
            OriginX = 0.5 * (TextWidth - BarcodeWidth)
            BarcodeWidth = TextWidth
        End If

        ' calculate height
        Dim TextHeight = TextFont.LineSpacing(FontSize)

        ' Barcode box
        Return New BarcodeBox(OriginX, TextHeight, BarcodeWidth, BarcodeHeight + TextHeight)
    End Function
End Class

''' <summary>
''' Barcode 128 Class
''' </summary>
''' <remarks>
''' This program supports ASCII range of 0 to 127. 
''' Character range 128 to 255 is not supported.
''' </remarks>
Public Class Barcode128
    Inherits Barcode
    ''' <summary>
    ''' Each code128 character is encoded as 3 black bars and 3 white bars.
    ''' </summary>
    Public Const CODE_CHAR_BARS As Integer = 6

    ''' <summary>
    ''' Each code128 character width is 11 narrow bars.
    ''' </summary>
    Public Const CODE_CHAR_WIDTH As Integer = 11

    ''' <summary>
    ''' Function character FNC1.
    ''' </summary>
    Public Const FNC1_CHAR As Char = Microsoft.VisualBasic.ChrW(256)

    ''' <summary>
    ''' Function character FNC2.
    ''' </summary>
    Public Const FNC2_CHAR As Char = Microsoft.VisualBasic.ChrW(257)

    ''' <summary>
    ''' Function character FNC3.
    ''' </summary>
    Public Const FNC3_CHAR As Char = Microsoft.VisualBasic.ChrW(258)

    ''' <summary>
    ''' Special code FNC1.
    ''' </summary>
    Public Const FNC1 As Integer = 102

    ''' <summary>
    ''' Special code FNC2.
    ''' </summary>
    Public Const FNC2 As Integer = 97

    ''' <summary>
    ''' Special code FNC3.
    ''' </summary>
    Public Const FNC3 As Integer = 96

    ''' <summary>
    ''' Special code SHIFT.
    ''' </summary>
    Public Const SHIFT As Integer = 98

    ''' <summary>
    ''' Special code CODEA (or FN4 for code set A).
    ''' </summary>
    Public Const CODEA As Integer = 101

    ''' <summary>
    ''' Special code CODEB (or FN4 for code set B).
    ''' </summary>
    Public Const CODEB As Integer = 100

    ''' <summary>
    ''' Special code CODEC.
    ''' </summary>
    Public Const CODEC As Integer = 99

    ''' <summary>
    ''' Special code STARTA.
    ''' </summary>
    Public Const STARTA As Integer = 103

    ''' <summary>
    ''' Special code STARTB.
    ''' </summary>
    Public Const STARTB As Integer = 104

    ''' <summary>
    ''' Special code STARTC.
    ''' </summary>
    Public Const STARTC As Integer = 105

    ''' <summary>
    ''' Special code STOP.
    ''' </summary>
    Public Const [STOP] As Integer = 106

    ''' <summary>
    ''' Code table for barcode 128
    ''' </summary>
    ''' <Remarks>
    ''' <para>
    ''' Barcode 128 consists of 107 codes.
    ''' </para>
    ''' <para>
    ''' Each code is made of 6 bars, three black bars and three white bars.
    ''' Each bar is expressed as multiple of the narrow bar.
    ''' </para>
    ''' <para>
    ''' Total width of one bar code is always 11 narrow bar units.
    ''' </para>
    ''' <para>
    ''' After the stop code there is always one more black bar
    ''' with width of two units.
    ''' </para>
    ''' <para>
    ''' Each code can have one of three possible meanings
    ''' depending on the mode (CODEA, CODEB, CODEC).
    ''' </para>
    ''' <para>
    ''' The CodeTable array dimensions are [107, 6].
    ''' </para>
    ''' </Remarks>
    Public Shared ReadOnly CodeTable As Byte(,) = {            '        CODEA   CODEB   CODEC 
        {2, 1, 2, 2, 2, 2}, ' 0		SP		SP		0
        {2, 2, 2, 1, 2, 2}, ' 1		!		!		1
        {2, 2, 2, 2, 2, 1}, ' 2		"		"		2
        {1, 2, 1, 2, 2, 3}, ' 3		#		#		3
        {1, 2, 1, 3, 2, 2}, ' 4		$		$		4
        {1, 3, 1, 2, 2, 2}, ' 5		%		%		5
        {1, 2, 2, 2, 1, 3}, ' 6		&		&		6
        {1, 2, 2, 3, 1, 2}, ' 7		'		'		7
        {1, 3, 2, 2, 1, 2}, ' 8		(		(		8
        {2, 2, 1, 2, 1, 3}, ' 9		)		)		9
        {2, 2, 1, 3, 1, 2}, ' 10		*		*		10
        {2, 3, 1, 2, 1, 2}, ' 11		+		+		11
        {1, 1, 2, 2, 3, 2}, ' 12		,		,		12
        {1, 2, 2, 1, 3, 2}, ' 13		-		-		13
        {1, 2, 2, 2, 3, 1}, ' 14		.		.		14
        {1, 1, 3, 2, 2, 2}, ' 15		/		/		15
        {1, 2, 3, 1, 2, 2}, ' 16		0		0		16
        {1, 2, 3, 2, 2, 1}, ' 17		1		1		17
        {2, 2, 3, 2, 1, 1}, ' 18		2		2		18
        {2, 2, 1, 1, 3, 2}, ' 19		3		3		19
        {2, 2, 1, 2, 3, 1}, ' 20		4		4		20
        {2, 1, 3, 2, 1, 2}, ' 21		5		5		21
        {2, 2, 3, 1, 1, 2}, ' 22		6		6		22
        {3, 1, 2, 1, 3, 1}, ' 23		7		7		23
        {3, 1, 1, 2, 2, 2}, ' 24		8		8		24
        {3, 2, 1, 1, 2, 2}, ' 25		9		9		25
        {3, 2, 1, 2, 2, 1}, ' 26		:		:		26
        {3, 1, 2, 2, 1, 2}, ' 27		;		;		27
        {3, 2, 2, 1, 1, 2}, ' 28		<		<		28
        {3, 2, 2, 2, 1, 1}, ' 29		=		=		29
        {2, 1, 2, 1, 2, 3}, ' 30		>		>		30
        {2, 1, 2, 3, 2, 1}, ' 31		?		?		31
        {2, 3, 2, 1, 2, 1}, ' 32		@		@		32
        {1, 1, 1, 3, 2, 3}, ' 33		A		A		33
        {1, 3, 1, 1, 2, 3}, ' 34		B		B		34
        {1, 3, 1, 3, 2, 1}, ' 35		C		C		35
        {1, 1, 2, 3, 1, 3}, ' 36		D		D		36
        {1, 3, 2, 1, 1, 3}, ' 37		E		E		37
        {1, 3, 2, 3, 1, 1}, ' 38		F		F		38
        {2, 1, 1, 3, 1, 3}, ' 39		G		G		39
        {2, 3, 1, 1, 1, 3}, ' 40		H		H		40
        {2, 3, 1, 3, 1, 1}, ' 41		I		I		41
        {1, 1, 2, 1, 3, 3}, ' 42		J		J		42
        {1, 1, 2, 3, 3, 1}, ' 43		K		K		43
        {1, 3, 2, 1, 3, 1}, ' 44		L		L		44
        {1, 1, 3, 1, 2, 3}, ' 45		M		M		45
        {1, 1, 3, 3, 2, 1}, ' 46		N		N		46
        {1, 3, 3, 1, 2, 1}, ' 47		O		O		47
        {3, 1, 3, 1, 2, 1}, ' 48		P		P		48
        {2, 1, 1, 3, 3, 1}, ' 49		Q		Q		49
        {2, 3, 1, 1, 3, 1}, ' 50		R		R		50
        {2, 1, 3, 1, 1, 3}, ' 51		S		S		51
        {2, 1, 3, 3, 1, 1}, ' 52		T		T		52
        {2, 1, 3, 1, 3, 1}, ' 53		U		U		53
        {3, 1, 1, 1, 2, 3}, ' 54		V		V		54
        {3, 1, 1, 3, 2, 1}, ' 55		W		W		55
        {3, 3, 1, 1, 2, 1}, ' 56		X		X		56
        {3, 1, 2, 1, 1, 3}, ' 57		Y		Y		57
        {3, 1, 2, 3, 1, 1}, ' 58		Z		Z		58
        {3, 3, 2, 1, 1, 1}, ' 59		[		[		59
        {3, 1, 4, 1, 1, 1}, ' 60		\		\		60
        {2, 2, 1, 4, 1, 1}, ' 61		]		]		61
        {4, 3, 1, 1, 1, 1}, ' 62		^		^		62
        {1, 1, 1, 2, 2, 4}, ' 63		_		_		63
        {1, 1, 1, 4, 2, 2}, ' 64		NUL		`		64
        {1, 2, 1, 1, 2, 4}, ' 65		SOH		a		65
        {1, 2, 1, 4, 2, 1}, ' 66		STX		b		66
        {1, 4, 1, 1, 2, 2}, ' 67		ETX		c		67
        {1, 4, 1, 2, 2, 1}, ' 68		EOT		d		68
        {1, 1, 2, 2, 1, 4}, ' 69		ENQ		e		69
        {1, 1, 2, 4, 1, 2}, ' 70		ACK		f		70
        {1, 2, 2, 1, 1, 4}, ' 71		BEL		g		71
        {1, 2, 2, 4, 1, 1}, ' 72		BS		h		72
        {1, 4, 2, 1, 1, 2}, ' 73		HT		i		73
        {1, 4, 2, 2, 1, 1}, ' 74		LF		j		74
        {2, 4, 1, 2, 1, 1}, ' 75		VT		k		75
        {2, 2, 1, 1, 1, 4}, ' 76		FF		I		76
        {4, 1, 3, 1, 1, 1}, ' 77		CR		m		77
        {2, 4, 1, 1, 1, 2}, ' 78		SO		n		78
        {1, 3, 4, 1, 1, 1}, ' 79		SI		o		79
        {1, 1, 1, 2, 4, 2}, ' 80		DLE		p		80
        {1, 2, 1, 1, 4, 2}, ' 81		DC1		q		81
        {1, 2, 1, 2, 4, 1}, ' 82		DC2		r		82
        {1, 1, 4, 2, 1, 2}, ' 83		DC3		s		83
        {1, 2, 4, 1, 1, 2}, ' 84		DC4		t		84
        {1, 2, 4, 2, 1, 1}, ' 85		NAK		u		85
        {4, 1, 1, 2, 1, 2}, ' 86		SYN		v		86
        {4, 2, 1, 1, 1, 2}, ' 87		ETB		w		87
        {4, 2, 1, 2, 1, 1}, ' 88		CAN		x		88
        {2, 1, 2, 1, 4, 1}, ' 89		EM		y		89
        {2, 1, 4, 1, 2, 1}, ' 90		SUB		z		90
        {4, 1, 2, 1, 2, 1}, ' 91		ESC		{		91
        {1, 1, 1, 1, 4, 3}, ' 92		FS		|		92
        {1, 1, 1, 3, 4, 1}, ' 93		GS		}		93
        {1, 3, 1, 1, 4, 1}, ' 94		RS		~		94
        {1, 1, 4, 1, 1, 3}, ' 95		US		DEL		95
        {1, 1, 4, 3, 1, 1}, ' 96		FNC 3	FNC 3	96
        {4, 1, 1, 1, 1, 3}, ' 97		FNC 2	FNC 2	97
        {4, 1, 1, 3, 1, 1}, ' 98		SHIFT	SHIFT	98
        {1, 1, 3, 1, 4, 1}, ' 99		CODE C	CODE C	99
        {1, 1, 4, 1, 3, 1}, ' 100		CODE B	FNC 4	CODE B
        {3, 1, 1, 1, 4, 1}, ' 101		FNC 4	CODE A	CODE A
        {4, 1, 1, 1, 3, 1}, ' 102		FNC 1	FNC 1	FNC 1
        {2, 1, 1, 4, 1, 2}, ' 103		Start A	Start A	Start A
        {2, 1, 1, 2, 1, 4}, ' 104		Start B	Start B	Start B
        {2, 1, 1, 2, 3, 2}, ' 105		Start C	Start C	Start C
        {2, 3, 3, 1, 1, 1}} ' 106		Stop	Stop	Stop

    ' code set
    Private Enum CodeSet
        Undefined
        CodeA
        CodeB
        CodeC
        ShiftA
        ShiftB
    End Enum


    ''' <summary>
    ''' Width of one bar at indexed position in narrow bar units.
    ''' </summary>
    ''' <param name="Index">Bar's index number.</param>
    ''' <returns>Bar's width in narrow bar units.</returns>
    ''' <remarks>This virtual function must be implemented by derived class 
    ''' Index range is 0 to BarCount - 1</remarks>
    Public Overrides Function BarWidth(Index As Integer) As Integer
        Return If(Index + 1 < BarCount, CodeTable(_CodeArray(Index / CODE_CHAR_BARS), Index Mod CODE_CHAR_BARS), 2)
    End Function


    ''' <summary>
    ''' Barcode 128 constructor
    ''' </summary>
    ''' <param name="Text">Input text</param>
    ''' <remarks>
    ''' <para>
    ''' Convert text to code 128.
    ''' </para>
    ''' <para>>
    ''' Valid input characters are ASCII 0 to 127.
    ''' </para>
    ''' <para>>
    ''' In addition three control function codes are available
    ''' </para>
    ''' <para>>
    ''' 	FNC1_CHAR = (char) 256;
    ''' </para>
    ''' <para>>
    ''' 	FNC2_CHAR = (char) 257;
    ''' </para>
    ''' <para>>
    ''' 	FNC3_CHAR = (char) 258;
    ''' </para>
    ''' <para>>
    ''' The constructor will optimize the translation of text to code.
    ''' The code array will be divided into segments of
    ''' CODEA, CODEB and CODEC
    ''' </para>
    ''' </remarks>
    Public Sub New(Text As String)
        ' test argument
        If String.IsNullOrEmpty(Text) Then Throw New ApplicationException("Barcode128: Text is null or empty")

        ' save text
        Me.Text = Text

        ' text length
        Dim TextLen = Text.Length

        ' leading FNC1
        Dim LeadFnc1End As Integer
        LeadFnc1End = 0

        While LeadFnc1End < TextLen AndAlso Text(LeadFnc1End) = FNC1_CHAR
            LeadFnc1End += 1
        End While

        ' leading digits
        Dim LeadDigitsEnd As Integer
        LeadDigitsEnd = LeadFnc1End

        While LeadDigitsEnd < TextLen AndAlso Text(LeadDigitsEnd) >= "0"c AndAlso Text(LeadDigitsEnd) <= "9"c
            LeadDigitsEnd += 1
        End While

        ' lead digits count
        Dim LeadDigitsCount = LeadDigitsEnd - LeadFnc1End

        ' if leading digits is odd remove the last one
        If (LeadDigitsCount And 1) <> 0 Then
            LeadDigitsEnd -= 1
            LeadDigitsCount -= 1
        End If

        ' trailing FNC1
        Dim TrailFnc1Start As Integer
        TrailFnc1Start = TextLen - 1

        While TrailFnc1Start >= LeadDigitsEnd AndAlso Text(TrailFnc1Start) = FNC1_CHAR
            TrailFnc1Start -= 1
        End While

        TrailFnc1Start += 1

        ' trailing digits
        Dim TrailDigitsStart As Integer
        TrailDigitsStart = TrailFnc1Start - 1

        While TrailDigitsStart >= LeadDigitsEnd AndAlso Text(TrailDigitsStart) >= "0"c AndAlso Text(TrailDigitsStart) <= "9"c
            TrailDigitsStart -= 1
        End While

        TrailDigitsStart += 1

        ' trailing digits count
        Dim TrailDigitsCount = TrailFnc1Start - TrailDigitsStart

        ' if trailing digits is odd remove the first one
        If (TrailDigitsCount And 1) <> 0 Then
            TrailDigitsStart += 1
            TrailDigitsCount -= 1
        End If

        ' initialize code array end pointer
        Dim CodeEnd = 0

        ' test for all digits with or without leading and or trailing FNC1
        If LeadDigitsEnd = TrailDigitsStart AndAlso LeadDigitsCount <> 0 Then
            ' create code array
            _CodeArray = New Integer(1 + LeadFnc1End + (LeadDigitsEnd - LeadFnc1End) / 2 + (TextLen - TrailFnc1Start) + 2 - 1) {}

            ' start with code set C
            _CodeArray(CodeEnd) = If(CodeEnd = 0, STARTC, CODEC)
            CodeEnd += 1

            ' add FNC1 if required
            For Index = 0 To LeadFnc1End - 1
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = FNC1
            Next

            ' convert to pairs of digits
            EncodeDigits(LeadFnc1End, LeadDigitsEnd, CodeEnd)

            ' add FNC1 if required

            ' text has digits and non digits
            For Index = TrailFnc1Start To TextLen - 1
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = FNC1
            Next
        Else
            ' remove leading digits if less than 4
            If LeadDigitsCount < 4 Then
                LeadDigitsEnd = 0
                LeadFnc1End = 0
                LeadDigitsCount = 0
            End If

            ' remove traling digits if less than 4
            If TrailDigitsCount < 4 Then
                TrailDigitsStart = TextLen
                TrailFnc1Start = TextLen
                TrailDigitsCount = 0
            End If

            ' create code array (worst case length)
            _CodeArray = New Integer(2 * TextLen + 2 - 1) {}

            ' lead digits
            If LeadDigitsCount <> 0 Then
                ' start with code set C
                _CodeArray(CodeEnd) = If(CodeEnd = 0, STARTC, CODEC)
                CodeEnd += 1

                ' add FNC1 if required
                For Index = 0 To LeadFnc1End - 1
                    _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = FNC1
                Next

                ' convert to pairs of digits
                EncodeDigits(LeadFnc1End, LeadDigitsEnd, CodeEnd)
            End If

            Dim StartOfNonDigits = LeadDigitsEnd
            Dim StartOfDigits = LeadDigitsEnd
            Dim EndOfDigits As Integer

            ' scan text between end of leading digits to start of trailing digits
            While True
                ' look for a digit
                While StartOfDigits < TrailDigitsStart AndAlso (Text(StartOfDigits) < "0"c OrElse Text(StartOfDigits) > "9"c)
                    StartOfDigits += 1
                End While

                EndOfDigits = StartOfDigits

                ' we have at least one
                If StartOfDigits < TrailDigitsStart Then
                    ' count how many digits we have
                    EndOfDigits += 1

                    While EndOfDigits < TrailDigitsStart AndAlso Text(EndOfDigits) >= "0"c AndAlso Text(EndOfDigits) <= "9"c
                        EndOfDigits += 1
                    End While

                    ' test for odd number of digits
                    If (EndOfDigits - StartOfDigits And 1) <> 0 Then StartOfDigits += 1

                    ' if we have less than 6 process digits as non digits
                    If EndOfDigits - StartOfDigits < 6 Then
                        StartOfDigits = EndOfDigits
                        Continue While
                    End If
                End If

                ' process non digits up to StartOfDigits
                EncodeNonDigits(StartOfNonDigits, StartOfDigits, CodeEnd)

                ' if there are no digits at the end, get out of the loop
                If StartOfDigits = TrailDigitsStart Then Exit While

                ' add code set C
                _CodeArray(CodeEnd) = If(CodeEnd = 0, STARTC, CODEC)
                CodeEnd += 1

                ' convert to pairs of digits
                EncodeDigits(StartOfDigits, EndOfDigits, CodeEnd)

                ' adjust start of digits and non digits
                StartOfDigits = EndOfDigits
                StartOfNonDigits = EndOfDigits
            End While

            ' trailing digits
            If TrailDigitsCount <> 0 Then
                ' add code set C
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = CODEC

                ' convert to pairs of digits
                EncodeDigits(TrailDigitsStart, TrailFnc1Start, CodeEnd)

                ' add trailing FNC1 if required
                For Index = TrailFnc1Start To TextLen - 1
                    _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = FNC1
                Next
            End If

            ' adjust code array to right length
            Array.Resize(_CodeArray, CodeEnd + 2)
        End If

        ' checksum and STOP
        Checksum()

        ' set number of bars for enumeration
        BarCount = CODE_CHAR_BARS * _CodeArray.Length + 1

        ' save total width
        TotalWidth = CODE_CHAR_WIDTH * _CodeArray.Length + 2

        ' exit
        Return
    End Sub


    ''' <summary>
    ''' Barcode 128 constructor
    ''' </summary>
    ''' <param name="_CodeArray">Code array</param>
    ''' <remarks>
    ''' <para>
    ''' Set Code Array and convert it to text.
    ''' </para>
    ''' <para>
    ''' Each code must be 0 to 106.
    ''' </para>
    ''' <para>
    ''' The first code must be 103, 104 or 105.
    ''' </para>
    ''' <para>
    ''' The stop code 106 if present must be the last code.
    ''' </para>
    ''' <para>
    ''' If the last code is not 106, the method calculates the checksum
    ''' and appends the checksum and the stop character to the end of the array.
    ''' </para>
    ''' <para>
    ''' If the stop code is missing you must not have a checksum.
    ''' If the last code is 106, the method recalculates the checksum
    ''' and replaces the existing checksum.
    ''' </para>
    ''' <para>
    ''' The text output is made of ASCII characters 0 to 127 and
    ''' three function characters 256, 257 and 258.
    ''' </para>
    ''' </remarks>

    Public Sub New(_CodeArray As Integer())
        ' save code array
        Me._CodeArray = _CodeArray

        ' test argument
        If _CodeArray Is Nothing OrElse _CodeArray.Length < 2 Then Throw New ApplicationException("Barcode128: Code array is null or empty")

        ' code array length
        Dim Length = _CodeArray.Length

        ' if last element is not stop, add two more codes
        If _CodeArray(Length - 1) <> [STOP] Then
            ' add two elements to the array
            Length += 2
            Array.Resize(_CodeArray, Length)
        End If

        ' checksum (we ignore user supplied checksum and override it with our own)
        ' and add STOP at the end
        Checksum()

        ' set number of bars
        BarCount = CODE_CHAR_BARS * Length + 1

        ' save total width
        TotalWidth = CODE_CHAR_WIDTH * Length + 2

        ' convert code array to text
        Dim Str As StringBuilder = New StringBuilder()

        ' conversion state
        Dim CodeSet As CodeSet

        ' start code
        Select Case _CodeArray(0)
            Case STARTA
                CodeSet = CodeSet.CodeA
            Case STARTB
                CodeSet = CodeSet.CodeB
            Case STARTC
                CodeSet = CodeSet.CodeC
            Case Else
                ' first code must be FNC1, FNC2 or FNC3
                Throw New ApplicationException("Barcode128: Code array first element must be start code (103, 104, 105)")
        End Select

        ' loop for all characters except for start, checksum and stop
        Dim [End] = Length - 2

        For Index = 1 To [End] - 1
            Dim Code = _CodeArray(Index)
            If Code < 0 OrElse Code > FNC1 Then Throw New ApplicationException("Barcode128: Code array has invalid codes (not 0 to 106)")

            Select Case CodeSet
                Case CodeSet.CodeA

                    If Code = CODEA Then
                        Throw New ApplicationException("Barcode128: No support for FNC4")
                    ElseIf Code = CODEB Then
                        CodeSet = CodeSet.CodeB
                    ElseIf Code = CODEC Then
                        CodeSet = CodeSet.CodeC
                    ElseIf Code = SHIFT Then
                        CodeSet = CodeSet.ShiftB
                    ElseIf Code = FNC1 Then
                        Str.Append(FNC1_CHAR)
                    ElseIf Code = FNC2 Then
                        Str.Append(FNC2_CHAR)
                    ElseIf Code = FNC3 Then
                        Str.Append(FNC3_CHAR)
                    ElseIf Code < 64 Then
                        Str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    Else
                        Str.Append(Microsoft.VisualBasic.ChrW(Code - 64))
                    End If

                Case CodeSet.CodeB

                    If Code = CODEA Then
                        CodeSet = CodeSet.CodeA
                    ElseIf Code = CODEB Then
                        Throw New ApplicationException("Barcode128: No support for FNC4")
                    ElseIf Code = CODEC Then
                        CodeSet = CodeSet.CodeC
                    ElseIf Code = SHIFT Then
                        CodeSet = CodeSet.ShiftB
                    ElseIf Code = FNC1 Then
                        Str.Append(FNC1_CHAR)
                    ElseIf Code = FNC2 Then
                        Str.Append(FNC2_CHAR)
                    ElseIf Code = FNC3 Then
                        Str.Append(FNC3_CHAR)
                    Else
                        Str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    End If

                Case CodeSet.ShiftA

                    If Code < 64 Then
                        Str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    ElseIf Code < 96 Then
                        Str.Append(Microsoft.VisualBasic.ChrW(Code - 64))
                    Else
                        Throw New ApplicationException("Barcode128: SHIFT error")
                    End If

                    CodeSet = CodeSet.CodeB
                Case CodeSet.ShiftB

                    If Code < 96 Then
                        Str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    Else
                        Throw New ApplicationException("Barcode128: SHIFT error")
                    End If

                    CodeSet = CodeSet.CodeA
                Case CodeSet.CodeC

                    If Code = CODEA Then
                        CodeSet = CodeSet.CodeA
                    ElseIf Code = CODEB Then
                        CodeSet = CodeSet.CodeB
                    ElseIf Code = FNC1 Then
                        Str.Append(FNC1_CHAR)
                    Else
                        Str.Append(Microsoft.VisualBasic.ChrW(Asc("0"c) + CInt(Code / 10)))
                        Str.Append(Microsoft.VisualBasic.ChrW(Asc("0"c) + Code Mod 10))
                    End If
            End Select
        Next

        ' save text
        Text = Str.ToString()

        ' exit
        Return
    End Sub


    ' Process block of digits


    Private Sub EncodeDigits(TextStart As Integer, TextEnd As Integer, ByRef CodeEnd As Integer)
        ' convert to pairs of digits
        For Index = TextStart To TextEnd - 1 Step 2
            _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = 10 * (AscW(Text(Index)) - Asc("0"c)) + (AscW(Text(Index + 1)) - Asc("0"c))
        Next

        Return
    End Sub


    ' Process block of non-digits


    Private Sub EncodeNonDigits(TextStart As Integer, TextEnd As Integer, ByRef CodeEnd As Integer)
        ' assume code set B
        Dim CodeSeg = CodeEnd
        _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = If(CodeSeg = 0, STARTB, CODEB)
        Dim CurCodeSet = CodeSet.Undefined

        For Index = TextStart To TextEnd - 1
            ' get char
            Dim CurChar As Integer = AscW(Text(Index))

            ' currect character is part of code set A
            If CurChar < 32 Then
                Select Case CurCodeSet
                    ' current segment is undefined
                    ' all characters up to this point are 32 to 95 eigther A or B
                    Case CodeSet.Undefined
                        ' change first segemnt to be code set A
                        _CodeArray(CodeSeg) = If(CodeSeg = 0, STARTA, CODEA)
                        CurCodeSet = CodeSet.CodeA

                    ' currect segment is code B
                    Case CodeSet.CodeB
                        ' save current location as start of new segment
                        CodeSeg = CodeEnd

                        ' one time shift to A
                        _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = SHIFT
                        CurCodeSet = CodeSet.ShiftA

                    ' currect segment is Code B with one time shift to A
                    Case CodeSet.ShiftA
                        ' convert the last shift A to code A
                        _CodeArray(CodeSeg) = CODEA
                        CurCodeSet = CodeSet.CodeA

                    ' currect segment is Code A with one time shift to B
                    Case CodeSet.ShiftB
                        ' disable the Shift B. this is a code A segment with one shift B
                        CurCodeSet = CodeSet.CodeA
                End Select

                ' save character
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = CurChar + 64
                Continue For
            End If

            ' current character is part of either code set A or code set B
            If CurChar < 96 Then
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = CurChar - Asc(" "c)
                Continue For
            End If

            ' currect character is part of code set B
            If CurChar < 128 Then
                Select Case CurCodeSet
                    ' current segment is undefined
                    ' all characters up to this point are 32 to 95 eigther A or B
                    Case CodeSet.Undefined
                        ' make first segemnt to be code set B
                        CurCodeSet = CodeSet.CodeB

                    ' currect segment is code A
                    Case CodeSet.CodeA
                        ' save current location as start of new segment
                        CodeSeg = CodeEnd

                        ' one time shift to B
                        _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = SHIFT
                        CurCodeSet = CodeSet.ShiftB

                    ' currect segment is Code B with one time shift to A
                    Case CodeSet.ShiftA
                        ' disable the ShiftA. this is a code B segment with one shift A
                        CurCodeSet = CodeSet.CodeB

                    ' currect segment is Code A with one time shift to B
                    Case CodeSet.ShiftB
                        ' convert the last shift B to code B
                        _CodeArray(CodeSeg) = CODEB
                        CurCodeSet = CodeSet.CodeB
                End Select

                ' save character
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = CurChar - Asc(" "c)
                Continue For
            End If

            ' function code
            If CurChar >= AscW(FNC1_CHAR) AndAlso CurChar <= AscW(FNC3_CHAR) Then
                _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodeEnd), CodeEnd - 1)) = If(CurChar = AscW(FNC1_CHAR), FNC1, If(CurChar = AscW(FNC2_CHAR), FNC2, FNC3))
                Continue For
            End If

            ' invalid character
            Throw New ApplicationException("FormaCode128 input characters must be 0 to 127 or function code (256, 257, 258)")
        Next

        Return
    End Sub


    ' Code 128 checksum calculations
    ' The method stores the checksum and STOP character


    Private Sub Checksum()
        ' calculate checksum
        Dim Length = _CodeArray.Length - 2
        Dim ChkSum = _CodeArray(0)

        For Index = 1 To Length - 1
            ChkSum += Index * _CodeArray(Index)
        Next

        ' final checksum
        _CodeArray(Length) = ChkSum Mod 103

        ' stop code
        _CodeArray(Length + 1) = [STOP]
        Return
    End Sub
End Class

''' <summary>
''' Barcode 39 class
''' </summary>
Public Class Barcode39
    Inherits Barcode
    ''' <summary>
    ''' Each code39 code is encoded as 5 black bars and 5 white bars.
    ''' </summary>
    Public Const CODE_CHAR_BARS As Integer = 10

    ''' <summary>
    ''' Total length expressed in narrow bar units.
    ''' </summary>
    Public Const CODE_CHAR_WIDTH As Integer = 16

    ''' <summary>
    ''' Barcode39 start and stop character (normally displayed as *).
    ''' </summary>
    Public Const START_STOP_CODE As Integer = 43

    ''' <summary>
    ''' Barcode39 supported characters.
    ''' </summary>
    Public Const CharSet As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%*"

    ''' <summary>
    ''' Code table for barcode 39
    ''' </summary>
    ''' <remarks>Array size [44, 10]</remarks>
    Public Shared ReadOnly CodeTable As Byte(,) = {
        {1, 1, 1, 3, 3, 1, 3, 1, 1, 1},     ' 0  0
        {3, 1, 1, 3, 1, 1, 1, 1, 3, 1},     ' 1  1
        {1, 1, 3, 3, 1, 1, 1, 1, 3, 1},     ' 2  2
        {3, 1, 3, 3, 1, 1, 1, 1, 1, 1},     ' 3  3
        {1, 1, 1, 3, 3, 1, 1, 1, 3, 1},     ' 4  4
        {3, 1, 1, 3, 3, 1, 1, 1, 1, 1},     ' 5  5
        {1, 1, 3, 3, 3, 1, 1, 1, 1, 1},     ' 6  6
        {1, 1, 1, 3, 1, 1, 3, 1, 3, 1},     ' 7  7
        {3, 1, 1, 3, 1, 1, 3, 1, 1, 1},     ' 8  8
        {1, 1, 3, 3, 1, 1, 3, 1, 1, 1},     ' 9  9
        {3, 1, 1, 1, 1, 3, 1, 1, 3, 1},     ' 10 A
        {1, 1, 3, 1, 1, 3, 1, 1, 3, 1},     ' 11 B
        {3, 1, 3, 1, 1, 3, 1, 1, 1, 1},     ' 12 C
        {1, 1, 1, 1, 3, 3, 1, 1, 3, 1},     ' 13 D
        {3, 1, 1, 1, 3, 3, 1, 1, 1, 1},     ' 14 E
        {1, 1, 3, 1, 3, 3, 1, 1, 1, 1},     ' 15 F
        {1, 1, 1, 1, 1, 3, 3, 1, 3, 1},     ' 16 G
        {3, 1, 1, 1, 1, 3, 3, 1, 1, 1},     ' 17 H
        {1, 1, 3, 1, 1, 3, 3, 1, 1, 1},     ' 18 I
        {1, 1, 1, 1, 3, 3, 3, 1, 1, 1},     ' 19 J
        {3, 1, 1, 1, 1, 1, 1, 3, 3, 1},     ' 20 K
        {1, 1, 3, 1, 1, 1, 1, 3, 3, 1},     ' 21 L
        {3, 1, 3, 1, 1, 1, 1, 3, 1, 1},     ' 22 M
        {1, 1, 1, 1, 3, 1, 1, 3, 3, 1},     ' 23 N
        {3, 1, 1, 1, 3, 1, 1, 3, 1, 1},     ' 24 O
        {1, 1, 3, 1, 3, 1, 1, 3, 1, 1},     ' 25 P
        {1, 1, 1, 1, 1, 1, 3, 3, 3, 1},     ' 26 Q
        {3, 1, 1, 1, 1, 1, 3, 3, 1, 1},     ' 27 R
        {1, 1, 3, 1, 1, 1, 3, 3, 1, 1},     ' 28 S
        {1, 1, 1, 1, 3, 1, 3, 3, 1, 1},     ' 29 T
        {3, 3, 1, 1, 1, 1, 1, 1, 3, 1},     ' 30 U
        {1, 3, 3, 1, 1, 1, 1, 1, 3, 1},     ' 31 V
        {3, 3, 3, 1, 1, 1, 1, 1, 1, 1},     ' 32 W
        {1, 3, 1, 1, 3, 1, 1, 1, 3, 1},     ' 33 X
        {3, 3, 1, 1, 3, 1, 1, 1, 1, 1},     ' 34 Y
        {1, 3, 3, 1, 3, 1, 1, 1, 1, 1},     ' 35 Z
        {1, 3, 1, 1, 1, 1, 3, 1, 3, 1},     ' 36 -
        {3, 3, 1, 1, 1, 1, 3, 1, 1, 1},     ' 37 .
        {1, 3, 3, 1, 1, 1, 3, 1, 1, 1},     ' 38 (space)
        {1, 3, 1, 3, 1, 3, 1, 1, 1, 1},     ' 39 $
        {1, 3, 1, 3, 1, 1, 1, 3, 1, 1},     ' 40 /
        {1, 3, 1, 1, 1, 3, 1, 3, 1, 1},     ' 41 +
        {1, 1, 1, 3, 1, 3, 1, 3, 1, 1},     ' 42 %
        {1, 3, 1, 1, 3, 1, 3, 1, 1, 1}}     ' 43 *

    
    ''' <summary>
    ''' Bar width as function of position in the barcode 39 
    ''' </summary>
    ''' <param name="Index">Array index.</param>
    ''' <returns>Width of one bar</returns>
    
    Public Overrides Function BarWidth(Index As Integer) As Integer
        Return CodeTable(_CodeArray(Index / CODE_CHAR_BARS), Index Mod CODE_CHAR_BARS)
    End Function

    
    ''' <summary>
    ''' Barcode 39 constructor
    ''' </summary>
    ''' <param name="Text">Barcode text</param>
    ''' <remarks>
    ''' <para>
    ''' The constructor converts the text into code.
    ''' </para>
    ''' <para>
    ''' Valid characters are:
    ''' </para>
    ''' <list type="table">
    ''' <item><description>Digits 0 to 9</description></item>
    ''' <item><description>Capital Letters A to Z</description></item>
    ''' <item><description>Dash '-'</description></item>
    ''' <item><description>Period '.'</description></item>
    ''' <item><description>Space ' '</description></item>
    ''' <item><description>Dollar '$'</description></item>
    ''' <item><description>Slash '/'</description></item>
    ''' <item><description>Plus '+'</description></item>
    ''' <item><description>Percent '%'</description></item>
    ''' <item><description>Asterisk '*' (This is the start and stop
    ''' 	character. It cannot be in the middle of the text).</description></item>
    ''' </list>
    ''' </remarks>
    
    Public Sub New(Text As String)
        ' test argument
        If String.IsNullOrEmpty(Text) Then Throw New ApplicationException("Barcode39: Text cannot be null or empty")

        ' save text
        Me.Text = Text

        ' barcode array
        _CodeArray = New Integer(Text.Length + 2 - 1) {}

        ' put * at the begining
        Dim CodePtr = 0
        _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodePtr), CodePtr - 1)) = START_STOP_CODE

        ' encode the text
        For Index = 0 To Text.Length - 1
            Dim Code = CharSet.IndexOf(Text(Index))

            If Code = START_STOP_CODE Then
                If Index = 0 OrElse Index = Text.Length - 1 Then Continue For
                Throw New ApplicationException("Barcode39: Start/Stop character (asterisk *) is not allowed in the middle of the text")
            End If

            If Code < 0 Then Throw New ApplicationException("Barcode39: Invalid character")
            _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodePtr), CodePtr - 1)) = Code
        Next

        ' put * at the end
        _CodeArray(CodePtr) = START_STOP_CODE

        ' set number of bars for enumeration
        BarCount = CODE_CHAR_BARS * _CodeArray.Length - 1

        ' set total width
        TotalWidth = CODE_CHAR_WIDTH * _CodeArray.Length - 1

        ' exit
        Return
    End Sub

    
    ''' <summary>
    ''' Barcode 39 constructor
    ''' </summary>
    ''' <param name="_CodeArray">Code array</param>
    ''' <remarks>
    ''' <para>
    ''' Sets code array and converts to equivalent text.
    ''' </para>
    ''' <para>
    ''' If the code array is missing the start and/or stop characters,
    ''' the constructor will add them.
    ''' </para>
    ''' <para>
    ''' Valid codes are:
    ''' </para>
    ''' <list type="table">
    ''' <item><term>0 to 9</term><description>Digits 0 to 9</description></item>
    ''' <item><term>10 to 35</term><description>Capital Letters A to Z</description></item>
    ''' <item><term>36</term><description>Dash '-'</description></item>
    ''' <item><term>37</term><description>Period '.'</description></item>
    ''' <item><term>38</term><description>Space ' '</description></item>
    ''' <item><term>39</term><description>Dollar '$'</description></item>
    ''' <item><term>40</term><description>Slash '/'</description></item>
    ''' <item><term>41</term><description>Plus '+'</description></item>
    ''' <item><term>42</term><description>Percent '%'</description></item>
    ''' <item><term>43</term><description>Asterisk '*' (This is the start and stop
    ''' 	character. It cannot be in the middle of the text)</description></item>
    ''' </list>
    ''' </remarks>
    
    Public Sub New(_CodeArray As Integer())
        ' save code array
        Me._CodeArray = _CodeArray

        ' test argument
        If _CodeArray Is Nothing OrElse _CodeArray.Length = 0 Then Throw New ApplicationException("Barcode39: Code array is null or empty")

        ' test for start code
        If _CodeArray(0) <> START_STOP_CODE Then
            Dim TempArray = New Integer(_CodeArray.Length + 1 - 1) {}
            TempArray(0) = START_STOP_CODE
            Array.Copy(_CodeArray, 0, TempArray, 1, _CodeArray.Length)
            _CodeArray = TempArray
        End If

        ' test for stop code
        If _CodeArray(_CodeArray.Length - 1) <> START_STOP_CODE Then
            Array.Resize(_CodeArray, _CodeArray.Length + 1)
            _CodeArray(_CodeArray.Length - 1) = START_STOP_CODE
        End If

        ' set number of bars
        BarCount = CODE_CHAR_BARS * _CodeArray.Length - 1

        ' set total width
        TotalWidth = CODE_CHAR_WIDTH * _CodeArray.Length - 1

        ' convert code array to text without start or stop characters
        Dim Str As StringBuilder = New StringBuilder()

        For Index = 1 To _CodeArray.Length - 2 - 1
            Dim Code = _CodeArray(Index)
            If Code < 0 OrElse Code >= START_STOP_CODE Then Throw New ApplicationException("Barcode39: Code array contains invalid code (0 to 42)")
            Str.Append(CharSet(Code))
        Next

        ' convert str to text
        Text = Str.ToString()

        ' exit
        Return
    End Sub
End Class

''' <summary>
''' Barcode EAN-13 or UPC-A class
''' </summary>
''' <remarks>
''' Barcode EAN-13 or UPC-A
''' Note UPC-A is a subset of EAN-13
''' UPC-A is made of 12 digits
''' EAN-13 is made of 13 digits
''' If the first digit of EAN-13 is zero it is considered to be
''' UPC-A. The zero will be eliminated.
''' The barcode in both cases is made out of 12 symbols.
''' </remarks>
Public Class BarcodeEAN13
    Inherits Barcode
    ''' <summary>
    ''' Barcode length
    ''' </summary>
    ''' <remarks>
    ''' Each code EAN-13 or UPC-A code is encoded as 2 black bars and 2 white bars
    ''' there are exactly 12 characters in a barcode.
    ''' </remarks>
    Public Const BARCODE_LEN As Integer = 12

    ''' <summary>
    ''' Barcode half length
    ''' </summary>
    ''' <remarks>
    ''' Each code EAN-13 or UPC-A code is encoded as 2 black bars and 2 white bars
    ''' there are exactly 12 characters in a barcode
    ''' </remarks>
    Public Const BARCODE_HALF_LEN As Integer = 6

    ''' <summary>
    ''' Lead bars
    ''' </summary>
    Public Const LEAD_BARS As Integer = 3

    ''' <summary>
    ''' Separator bars
    ''' </summary>
    Public Const SEPARATOR_BARS As Integer = 5

    ''' <summary>
    ''' Code character bars
    ''' </summary>
    Public Const CODE_CHAR_BARS As Integer = 4

    ''' <summary>
    ''' Code character width
    ''' </summary>
    Public Const CODE_CHAR_WIDTH As Integer = 7

    ''' <summary>
    ''' Code table for Barcode EAN-13 or UPC-A
    ''' </summary>
    ''' <remarks>Array size [20, 4]</remarks>
    Public Shared ReadOnly CodeTable As Byte(,) = {
        {3, 2, 1, 1},       ' A-0 Odd parity
        {2, 2, 2, 1},       ' A-1
        {2, 1, 2, 2},       ' A-2
        {1, 4, 1, 1},       ' A-3
        {1, 1, 3, 2},       ' A-4
        {1, 2, 3, 1},       ' A-5
        {1, 1, 1, 4},       ' A-6
        {1, 3, 1, 2},       ' A-7
        {1, 2, 1, 3},       ' A-8
        {3, 1, 1, 2},       ' A-9
        {1, 1, 2, 3},       ' B-0 Even Parity
        {1, 2, 2, 2},       ' B-1
        {2, 2, 1, 2},       ' B-2
        {1, 1, 4, 1},       ' B-3
        {2, 3, 1, 1},       ' B-4
        {1, 3, 2, 1},       ' B-5
        {4, 1, 1, 1},       ' B-6
        {2, 1, 3, 1},       ' B-7
        {3, 1, 2, 1},       ' B-8
        {2, 1, 1, 3}}       ' B-9

    ''' <summary>
    ''' Parity table
    ''' </summary>
    ''' <remarks>First digit of EAN-13 odd/even translation table</remarks>
    Public Shared ReadOnly ParityTable As Byte(,) = {
        {0, 0, 0, 0, 0},    ' 0
        {0, 10, 0, 10, 10}, ' 1
        {0, 10, 10, 0, 10}, ' 2
        {0, 10, 10, 10, 0}, ' 3
        {10, 0, 0, 10, 10}, ' 4
        {10, 10, 0, 0, 10}, ' 5
        {10, 10, 10, 0, 0}, ' 6
        {10, 0, 10, 0, 10}, ' 7
        {10, 0, 10, 10, 0}, ' 8
        {10, 10, 0, 10, 0}} ' 9
    Private FirstDigit As Integer


    ''' <summary>
    ''' Barcode width
    ''' </summary>
    ''' <param name="BarIndex">Code array index</param>
    ''' <returns>Barcode EAN-13 single bar width</returns>

    Public Overrides Function BarWidth(BarIndex As Integer) As Integer
        ' leading bars
        If BarIndex < LEAD_BARS Then Return 1

        ' left side 6 digits
        If BarIndex < LEAD_BARS + BARCODE_HALF_LEN * CODE_CHAR_BARS Then
            Dim Index = BarIndex - LEAD_BARS
            Return CodeTable(_CodeArray(Index / CODE_CHAR_BARS), Index Mod CODE_CHAR_BARS)
        End If

        ' separator bars
        If BarIndex < LEAD_BARS + BARCODE_HALF_LEN * CODE_CHAR_BARS + SEPARATOR_BARS Then Return 1

        ' right side 6 digits
        If BarIndex < LEAD_BARS + BARCODE_LEN * CODE_CHAR_BARS + SEPARATOR_BARS Then
            Dim Index = BarIndex - (LEAD_BARS + BARCODE_HALF_LEN * CODE_CHAR_BARS + SEPARATOR_BARS)
            Return CodeTable(_CodeArray(BARCODE_HALF_LEN + Index / CODE_CHAR_BARS), Index Mod CODE_CHAR_BARS)
        End If

        ' trailing bars
        Return 1
    End Function

    ''' <summary>
    ''' Calculate total barcode height including text
    ''' </summary>
    ''' <param name="BarWidth">Narrow bar width</param>
    ''' <param name="BarcodeHeight">Barcode height</param>
    ''' <param name="TextFont">Text font</param>
    ''' <param name="FontSize">Text font size</param>
    ''' <returns>BarcodeBox result</returns>
    Public Overrides Function GetBarcodeBox(BarWidth As Double, BarcodeHeight As Double, TextFont As PdfFont, FontSize As Double) As BarcodeBox
        ' no text
        If TextFont Is Nothing Then Return New BarcodeBox(BarWidth * TotalWidth, BarcodeHeight)

        ' one digit width
        Dim OriginX = TextFont.TextWidth(FontSize, "0")

        ' calculate width
        Dim BarcodeWidth = BarWidth * TotalWidth + OriginX
        If Text.Length = 12 Then BarcodeWidth += OriginX

        ' text height
        Dim OriginY = TextFont.LineSpacing(FontSize) - 5.0 * BarWidth

        ' Barcode box
        Return New BarcodeBox(OriginX, OriginY, BarcodeWidth, BarcodeHeight + OriginY)
    End Function


    ''' <summary>
    ''' Barcode EAN13 Constructor
    ''' </summary>
    ''' <param name="Text">Input text</param>
    ''' <remarks>
    ''' <para>
    ''' Convert text to code EAN-13 or UPC-A.
    ''' </para>
    ''' <para>
    ''' All characters must be digits.
    ''' </para>
    ''' <para>
    ''' The code is EAN-13 if string length is 13 characters
    ''' and first digit is not zero.
    ''' </para>
    ''' <para>
    ''' The code is UPC-A if string length is 12 characters
    ''' or string length is 13 and first character is zero.
    ''' </para>
    ''' <para>
    ''' The last character is a checksum. The checksum must be
    ''' given, however the constructor calculates the checksum and
    ''' override the one given. In other words, if you do not
    ''' know the checksum just set the last digit to 0.
    ''' </para>
    ''' </remarks>

    Public Sub New(Text As String)
        ' save text
        Me.Text = Text

        ' test argument
        If String.IsNullOrEmpty(Text) Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Text must not be null")

        ' text length
        Dim Length = Text.Length
        If Length <> 12 AndAlso Length <> 13 Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Text must be 12 for UPC-A or 13 for EAN-13")

        ' first digit
        FirstDigit = If(Length = 12, 0, Microsoft.VisualBasic.AscW(Text(0)) - Asc("0"c))
        If FirstDigit < 0 OrElse FirstDigit > 9 Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Invalid character (must be 0 to 9)")

        ' barcode array
        _CodeArray = New Integer(11) {}

        ' encode the text
        Dim CodePtr = 0

        For Index = If(Length = 12, 0, 1) To Length - 1
            Dim CodeValue As Integer = Microsoft.VisualBasic.AscW(Text(Index)) - Asc("0"c)
            If CodeValue < 0 OrElse CodeValue > 9 Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Invalid character (must be 0 to 9)")
            If FirstDigit <> 0 AndAlso Index >= 2 AndAlso Index <= 6 Then CodeValue += ParityTable(FirstDigit, Index - 2)
            _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodePtr), CodePtr - 1)) = CodeValue
        Next

        ' calculate checksum
        Checksum()

        ' add it to text
        Text = Text.Substring(0, Text.Length - 1) & Microsoft.VisualBasic.ChrW(Asc("0"c) + _CodeArray(BARCODE_LEN - 1)).ToString()

        ' set number of bars
        BarCount = BARCODE_LEN * CODE_CHAR_BARS + 2 * LEAD_BARS + SEPARATOR_BARS

        ' set total width
        TotalWidth = BARCODE_LEN * CODE_CHAR_WIDTH + 2 * LEAD_BARS + SEPARATOR_BARS

        ' exit
        Return
    End Sub


    ''' <summary>
    ''' Barcode EAN13 constructor.
    ''' </summary>
    ''' <param name="_CodeArray">Code array input.</param>
    ''' 	<remarks>
    ''' <para>
    ''' The constructor sets CodeArray and converts it to text.
    ''' </para>
    ''' <para>
    ''' CodeArray must be 12 elements long for both EAN-13 or UPC-A.
    ''' </para>
    ''' <para>
    ''' In the case of UPC-A the 12 elements of code array correspond
    ''' one to one with the 12 digits of the encoded value.
    ''' </para>
    ''' <para>
    ''' In the case of EAN-13 the 12 code elements corresponds to
    ''' element 2 to 13 of the text characters. The first text
    ''' character controls how elements 2 to 5 of the code array are
    ''' encoded. Please read the following article for full description.
    ''' http://www.barcodeisland.com/ean13.phtml.
    ''' </para>
    ''' <para>
    ''' In this class, odd parity encoding is one code element equals one digit.
    ''' </para>
    ''' <para>
    ''' Even parity is code element equals digit plus 10.
    ''' </para>
    ''' <para>
    ''' The last code element is a checksum. The checksum must be
    ''' given however the constructor calculates the checksum and
    ''' override the one given. In other words, if you do not
    ''' know the checksum just set the last element to 0.
    ''' </para>
    ''' 	</remarks>

    Public Sub New(_CodeArray As Integer())
        ' save code array
        Me._CodeArray = _CodeArray

        ' test argument
        If _CodeArray Is Nothing OrElse _CodeArray.Length <> BARCODE_LEN Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Code array must be exactly 12 characters")
        Dim Str As StringBuilder = New StringBuilder()
        Dim ParityTest = New Integer(4) {}

        ' convert code array to text
        For Index = 0 To BARCODE_LEN - 1 - 1
            Dim Code = _CodeArray(Index)
            If Code < 0 OrElse Code >= 20 OrElse Code >= 10 AndAlso (Index = 0 OrElse Index >= 6) Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Invalid code")
            If Index >= 1 AndAlso Index < 6 AndAlso Code >= 10 Then ParityTest(Index - 1) = 10

            If Index = 5 Then
                FirstDigit = 0

                While FirstDigit < 10
                    Dim Scan As Integer
                    Scan = 0

                    While Scan < 5 AndAlso ParityTable(FirstDigit, Scan) = ParityTest(Scan)
                        Scan += 1
                    End While

                    If Scan = 5 Then Exit While
                    FirstDigit += 1
                End While

                If FirstDigit = 10 Then Throw New ApplicationException("Barcode EAN-13/UPC-A: Invalid code")
                If FirstDigit <> 0 Then Str.Insert(0, Microsoft.VisualBasic.ChrW(Asc("0"c) + FirstDigit))
            End If

            Str.Append(Microsoft.VisualBasic.ChrW(Asc("0"c) + Code Mod 10))
        Next

        ' calculate checksum
        Checksum()

        ' add it to text
        Str.Append(Microsoft.VisualBasic.ChrW(Asc("0"c) + _CodeArray(BARCODE_LEN - 1)))

        ' save text
        Text = Str.ToString()

        ' set number of bars
        BarCount = BARCODE_LEN * CODE_CHAR_BARS + 2 * LEAD_BARS + SEPARATOR_BARS

        ' set total width
        TotalWidth = BARCODE_LEN * CODE_CHAR_WIDTH + 2 * LEAD_BARS + SEPARATOR_BARS

        ' exit
        Return
    End Sub


    ' Code EAN-13 checksum calculations


    Private Sub Checksum()
        ' calculate checksum
        Dim ChkSum = FirstDigit
        Dim Odd = True

        For Index = 0 To BARCODE_LEN - 1 - 1
            ChkSum += If(Odd, 3, 1) * _CodeArray(Index)
            Odd = Not Odd
        Next

        ' final checksum
        ChkSum = ChkSum Mod 10
        _CodeArray(BARCODE_LEN - 1) = If(ChkSum = 0, 0, 10 - ChkSum)
        Return
    End Sub
End Class

''' <summary>
''' Barcode interleaved 2 of 5 class
''' </summary>
Public Class BarcodeInterleaved2of5
    Inherits Barcode
    ''' <summary>
    ''' Code table for interleave 2 of 5 barcode
    ''' </summary>
    Public Shared ReadOnly CodeTable As Byte(,) = {
        {1, 1, 2, 2, 1},        ' 0
        {2, 1, 1, 1, 2},        ' 1
        {1, 2, 1, 1, 2},        ' 2
        {2, 2, 1, 1, 1},        ' 3
        {1, 1, 2, 1, 2},        ' 4
        {2, 1, 2, 1, 1},        ' 5
        {1, 2, 2, 1, 1},        ' 6
        {1, 1, 1, 2, 2},        ' 7
        {2, 1, 1, 2, 1},        ' 8
        {1, 2, 1, 2, 1}}        ' 9

    
    ''' <summary>
    ''' Barcode width
    ''' </summary>
    ''' <param name="BarIndex">Code array index</param>
    ''' <returns>float bar width</returns>
    
    Public Overrides Function BarWidth(BarIndex As Integer) As Integer
        ' leading bars
        If BarIndex < 4 Then Return 1

        ' ending bars
        If BarIndex >= BarCount - 3 Then Return If(BarIndex = BarCount - 3, 2, 1)

        ' code index
        BarIndex -= 4
        Dim CodeIndex As Integer = 2 * (BarIndex / 10)
        If (BarIndex And 1) <> 0 Then CodeIndex += 1

        ' code
        Dim Code = _CodeArray(CodeIndex)
        Return CodeTable(Code, (BarIndex Mod 10) / 2)
    End Function

    ''' <summary>
    ''' Barcode interleave 2 of 5 constructor
    ''' </summary>
    ''' <param name="Text">Text</param>
    ''' <param name="AddChecksum">Add checksum digit</param>
    Public Sub New(Text As String, Optional AddChecksum As Boolean = False)
        ' test argument
        If String.IsNullOrWhiteSpace(Text) Then Throw New ApplicationException("Barcode Interleave 2 of 5: Input text is null or empty")

        ' save text
        Me.Text = Text

        ' text length
        Dim Length = Text.Length
        If AddChecksum Then Length += 1
        If (Length And 1) <> 0 Then Throw New ApplicationException("Barcode Interleave 2 of 5: Text length must be even (including checksum)")

        ' barcode array
        _CodeArray = New Integer(Length - 1) {}

        ' make sure it is all digits
        Dim CodePtr = 0

        For Each Chr As Char In Text
            If Chr < "0"c OrElse Chr > "9"c Then Throw New ApplicationException("Barcode interleave 2 of 5: Invalid character (must be 0 to 9)")
            _CodeArray(stdNum.Min(Threading.Interlocked.Increment(CodePtr), CodePtr - 1)) = CInt(AscW(Chr) - Asc("0"c))
        Next

        ' calculate checksum
        If AddChecksum Then Checksum()

        ' set number of bars
        BarCount = 7 + 10 * (Length / 2)

        ' set total width
        TotalWidth = 8 + 14 * (Length / 2)

        ' exit
        Return
    End Sub

    
    ' Code EAN-13 checksum calculations
    

    Private Sub Checksum()
        ' calculate checksum
        Dim ChkSum = 3 * _CodeArray(0)
        Dim [End] = _CodeArray.Length - 1

        For Index = 1 To [End] - 1 Step 2
            ChkSum += _CodeArray(Index) + 3 * _CodeArray(Index + 1)
        Next

        ' final checksum
        ChkSum = ChkSum Mod 10
        If ChkSum <> 0 Then ChkSum = 10 - ChkSum
        _CodeArray([End]) = ChkSum

        ' add it to text
        Text = Text & Microsoft.VisualBasic.ChrW(ChkSum + Asc("0"c)).ToString()
        Return
    End Sub
End Class
