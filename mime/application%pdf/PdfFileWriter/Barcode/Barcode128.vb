#Region "Microsoft.VisualBasic::c037f928ddf1f489c00e7cc3d3785155, mime\application%pdf\PdfFileWriter\Barcode\Barcode128.vb"

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

    '   Total Lines: 793
    '    Code Lines: 416
    ' Comment Lines: 248
    '   Blank Lines: 129
    '     File Size: 28.74 KB


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
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports stdNum = System.Math

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
        Dim str As New StringBuilder()

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
                        str.Append(FNC1_CHAR)
                    ElseIf Code = FNC2 Then
                        str.Append(FNC2_CHAR)
                    ElseIf Code = FNC3 Then
                        str.Append(FNC3_CHAR)
                    ElseIf Code < 64 Then
                        str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    Else
                        str.Append(Microsoft.VisualBasic.ChrW(Code - 64))
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
                        str.Append(FNC1_CHAR)
                    ElseIf Code = FNC2 Then
                        str.Append(FNC2_CHAR)
                    ElseIf Code = FNC3 Then
                        str.Append(FNC3_CHAR)
                    Else
                        str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    End If

                Case CodeSet.ShiftA

                    If Code < 64 Then
                        str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
                    ElseIf Code < 96 Then
                        str.Append(Microsoft.VisualBasic.ChrW(Code - 64))
                    Else
                        Throw New ApplicationException("Barcode128: SHIFT error")
                    End If

                    CodeSet = CodeSet.CodeB
                Case CodeSet.ShiftB

                    If Code < 96 Then
                        str.Append(Microsoft.VisualBasic.ChrW(Asc(" "c) + Code))
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
                        str.Append(FNC1_CHAR)
                    Else
                        str.Append(Microsoft.VisualBasic.ChrW(Asc("0"c) + CInt(Code / 10)))
                        str.Append(Microsoft.VisualBasic.ChrW(Asc("0"c) + Code Mod 10))
                    End If
            End Select
        Next

        ' save text
        Text = str.ToString()

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
