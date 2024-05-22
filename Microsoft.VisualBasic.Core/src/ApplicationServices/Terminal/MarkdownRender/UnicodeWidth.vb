#Region "Microsoft.VisualBasic::19c0f29c961c78ff3b2b17867594d837, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\UnicodeWidth.vb"

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

    '   Total Lines: 347
    '    Code Lines: 228 (65.71%)
    ' Comment Lines: 102 (29.39%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (4.90%)
    '     File Size: 11.55 KB


    ' 	Module UnicodeWidth
    ' 
    ' 	    Function: BinarySearch, (+2 Overloads) GetWidth
    ' 		Structure Interval
    ' 
    ' 		    Properties: First, Last
    ' 
    ' 		    Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * https://gist.github.com/gongdo/0275b9ad1f25ad9d6f7a92ff677f5ec0
' *
' * This is an implementation of wcwidth() and wcswidth() (defined in
' * IEEE Std 1002.1-2001) for Unicode.
' *
' * http://www.opengroup.org/onlinepubs/007904975/functions/wcwidth.html
' * http://www.opengroup.org/onlinepubs/007904975/functions/wcswidth.html
' *
' * In fixed-width output devices, Latin characters all occupy a single
' * "cell" position of equal width, whereas ideographic CJK characters
' * occupy two such cells. Interoperability between terminal-line
' * applications and (teletype-style) character terminals using the
' * UTF-8 encoding requires agreement on which character should advance
' * the cursor by how many cell positions. No established formal
' * standards exist at present on which Unicode character shall occupy
' * how many cell positions on character terminals. These routines are
' * a first attempt of defining such behavior based on simple rules
' * applied to data provided by the Unicode Consortium.
' *
' * For some graphical characters, the Unicode standard explicitly
' * defines a character-cell width via the definition of the East Asian
' * FullWidth (F), Wide (W), Half-width (H), and Narrow (Na) classes.
' * In all these cases, there is no ambiguity about which width a
' * terminal shall use. For characters in the East Asian Ambiguous (A)
' * class, the width choice depends purely on a preference of backward
' * compatibility with either historic CJK or Western practice.
' * Choosing single-width for these characters is easy to justify as
' * the appropriate long-term solution, as the CJK practice of
' * displaying these characters as double-width comes from historic
' * implementation simplicity (8-bit encoded characters were displayed
' * single-width and 16-bit ones double-width, even for Greek,
' * Cyrillic, etc.) and not any typographic considerations.
' *
' * Much less clear is the choice of width for the Not East Asian
' * (Neutral) class. Existing practice does not dictate a width for any
' * of these characters. It would nevertheless make sense
' * typographically to allocate two character cells to characters such
' * as for instance EM SPACE or VOLUME INTEGRAL, which cannot be
' * represented adequately with a single-width glyph. The following
' * routines at present merely assign a single-cell width to all
' * neutral characters, in the interest of simplicity. This is not
' * entirely satisfactory and should be reconsidered before
' * establishing a formal standard in this area. At the moment, the
' * decision which Not East Asian (Neutral) characters should be
' * represented by double-width glyphs cannot yet be answered by
' * applying a simple rule from the Unicode database content. Setting
' * up a proper standard for the behavior of UTF-8 character terminals
' * will require a careful analysis not only of each Unicode character,
' * but also of each presentation form, something the author of these
' * routines has avoided to do so far.
' *
' * http://www.unicode.org/unicode/reports/tr11/
' *
' * Markus Kuhn -- 2007-05-26 (Unicode 5.0)
' *
' * Permission to use, copy, modify, and distribute this software
' * for any purpose and without fee is hereby granted. The author
' * disclaims all warranties with regard to this software.
' *
' * Latest version: http://www.cl.cam.ac.uk/~mgk25/ucs/wcwidth.c
' 

Namespace ApplicationServices.Terminal

	Public Module UnicodeWidth

		Private Structure Interval
			Public ReadOnly Property First() As Integer
			Public ReadOnly Property Last() As Integer

			Public Sub New(first_Conflict As Integer, last_Conflict As Integer)
				Me.First = first_Conflict
				Me.Last = last_Conflict
			End Sub
		End Structure

		' auxiliary function for binary search in interval table 
		Private Function BinarySearch(ucs As Char, table() As Interval) As Boolean
			Dim min As Integer = 0
			Dim max As Integer = table.Length - 1
			Dim mid As Integer

			If AscW(ucs) < table(0).First OrElse AscW(ucs) > table(max).Last Then
				Return False
			End If
			Do While max >= min
				mid = (min + max) \ 2
				If AscW(ucs) > table(mid).Last Then
					min = mid + 1
				ElseIf AscW(ucs) < table(mid).First Then
					max = mid - 1
				Else
					Return True
				End If
			Loop

			Return False
		End Function

		' sorted list of non-overlapping intervals of non-spacing characters 
		' generated by "uniset +cat=Me +cat=Mn +cat=Cf -00AD +1160-11FF +200B c" 
		Private ReadOnly combining() As Interval = New Dictionary(Of Integer, Integer) From {
		{&H300, &H36F},
		{&H483, &H486},
		{&H488, &H489},
		{&H591, &H5BD},
		{&H5BF, &H5BF},
		{&H5C1, &H5C2},
		{&H5C4, &H5C5},
		{&H5C7, &H5C7},
		{&H600, &H603},
		{&H610, &H615},
		{&H64B, &H65E},
		{&H670, &H670},
		{&H6D6, &H6E4},
		{&H6E7, &H6E8},
		{&H6EA, &H6ED},
		{&H70F, &H70F},
		{&H711, &H711},
		{&H730, &H74A},
		{&H7A6, &H7B0},
		{&H7EB, &H7F3},
		{&H901, &H902},
		{&H93C, &H93C},
		{&H941, &H948},
		{&H94D, &H94D},
		{&H951, &H954},
		{&H962, &H963},
		{&H981, &H981},
		{&H9BC, &H9BC},
		{&H9C1, &H9C4},
		{&H9CD, &H9CD},
		{&H9E2, &H9E3},
		{&HA01, &HA02},
		{&HA3C, &HA3C},
		{&HA41, &HA42},
		{&HA47, &HA48},
		{&HA4B, &HA4D},
		{&HA70, &HA71},
		{&HA81, &HA82},
		{&HABC, &HABC},
		{&HAC1, &HAC5},
		{&HAC7, &HAC8},
		{&HACD, &HACD},
		{&HAE2, &HAE3},
		{&HB01, &HB01},
		{&HB3C, &HB3C},
		{&HB3F, &HB3F},
		{&HB41, &HB43},
		{&HB4D, &HB4D},
		{&HB56, &HB56},
		{&HB82, &HB82},
		{&HBC0, &HBC0},
		{&HBCD, &HBCD},
		{&HC3E, &HC40},
		{&HC46, &HC48},
		{&HC4A, &HC4D},
		{&HC55, &HC56},
		{&HCBC, &HCBC},
		{&HCBF, &HCBF},
		{&HCC6, &HCC6},
		{&HCCC, &HCCD},
		{&HCE2, &HCE3},
		{&HD41, &HD43},
		{&HD4D, &HD4D},
		{&HDCA, &HDCA},
		{&HDD2, &HDD4},
		{&HDD6, &HDD6},
		{&HE31, &HE31},
		{&HE34, &HE3A},
		{&HE47, &HE4E},
		{&HEB1, &HEB1},
		{&HEB4, &HEB9},
		{&HEBB, &HEBC},
		{&HEC8, &HECD},
		{&HF18, &HF19},
		{&HF35, &HF35},
		{&HF37, &HF37},
		{&HF39, &HF39},
		{&HF71, &HF7E},
		{&HF80, &HF84},
		{&HF86, &HF87},
		{&HF90, &HF97},
		{&HF99, &HFBC},
		{&HFC6, &HFC6},
		{&H102D, &H1030},
		{&H1032, &H1032},
		{&H1036, &H1037},
		{&H1039, &H1039},
		{&H1058, &H1059},
		{&H1160, &H11FF},
		{&H135F, &H135F},
		{&H1712, &H1714},
		{&H1732, &H1734},
		{&H1752, &H1753},
		{&H1772, &H1773},
		{&H17B4, &H17B5},
		{&H17B7, &H17BD},
		{&H17C6, &H17C6},
		{&H17C9, &H17D3},
		{&H17DD, &H17DD},
		{&H180B, &H180D},
		{&H18A9, &H18A9},
		{&H1920, &H1922},
		{&H1927, &H1928},
		{&H1932, &H1932},
		{&H1939, &H193B},
		{&H1A17, &H1A18},
		{&H1B00, &H1B03},
		{&H1B34, &H1B34},
		{&H1B36, &H1B3A},
		{&H1B3C, &H1B3C},
		{&H1B42, &H1B42},
		{&H1B6B, &H1B73},
		{&H1DC0, &H1DCA},
		{&H1DFE, &H1DFF},
		{&H200B, &H200F},
		{&H202A, &H202E},
		{&H2060, &H2063},
		{&H206A, &H206F},
		{&H20D0, &H20EF},
		{&H302A, &H302F},
		{&H3099, &H309A},
		{&HA806, &HA806},
		{&HA80B, &HA80B},
		{&HA825, &HA826},
		{&HFB1E, &HFB1E},
		{&HFE00, &HFE0F},
		{&HFE20, &HFE23},
		{&HFEFF, &HFEFF},
		{&HFFF9, &HFFFB},
		{&H10A01, &H10A03},
		{&H10A05, &H10A06},
		{&H10A0C, &H10A0F},
		{&H10A38, &H10A3A},
		{&H10A3F, &H10A3F},
		{&H1D167, &H1D169},
		{&H1D173, &H1D182},
		{&H1D185, &H1D18B},
		{&H1D1AA, &H1D1AD},
		{&H1D242, &H1D244},
		{&HE0001, &HE0001},
		{&HE0020, &HE007F},
		{&HE0100, &HE01EF}
	}.Select(Function(item) New Interval(item.Key, item.Value)).ToArray()

		'     The following two functions define the column width of an ISO 10646
		'    * character as follows:
		'    *
		'    *    - The null character (U+0000) has a column width of 0.
		'    *
		'    *    - Other C0/C1 control characters and DEL will lead to a return
		'    *      value of -1. PrettyPrompt: we use 0.
		'    *
		'    *    - Non-spacing and enclosing combining characters (general
		'    *      category code Mn or Me in the Unicode database) have a
		'    *      column width of 0.
		'    *
		'    *    - SOFT HYPHEN (U+00AD) has a column width of 1.
		'    *
		'    *    - Other format characters (general category code Cf in the Unicode
		'    *      database) and ZERO WIDTH SPACE (U+200B) have a column width of 0.
		'    *
		'    *    - Hangul Jamo medial vowels and final consonants (U+1160-U+11FF)
		'    *      have a column width of 0.
		'    *
		'    *    - Spacing characters in the East Asian Wide (W) or East Asian
		'    *      Full-width (F) category as defined in Unicode Technical
		'    *      Report #11 have a column width of 2.
		'    *
		'    *    - All remaining characters (including all printable
		'    *      ISO 8859-1 and WGL4 characters, Unicode control characters,
		'    *      etc.) have a column width of 1.
		'    *
		'    * This implementation assumes that wchar_t characters are encoded
		'    * in ISO 10646.
		'    

		Public Function GetWidth(character As Char) As Integer
			' test for 8-bit control characters 
			If AscW(character) = 0 Then
				Return 0
			End If
			If AscW(character) = 10 Then ' PrettyPrompt addition, handle newline. This is a bit suspect.
				Return 1
			End If
			If AscW(character) < 32 OrElse (AscW(character) >= &H7F AndAlso AscW(character) < &HA0) Then
				'return -1;
				Return 0 'PrettyPrompt
			End If

			' the following two conditions were added for PrettyPrompt fast paths.
			If AscW(character) < &H300 Then ' frequent character fast path
				Return 1
			End If
			If AscW(character) >= &H2500 AndAlso AscW(character) <= &H257F Then ' fast path for box drawing
				Return 1
			End If

			' binary search in table of non-spacing characters 
			If BinarySearch(character, combining) Then
				Return 0
			End If

			'PrettyPrompt's correction
			Dim tempVar As Integer = AscW(character)

			If tempVar = &H26AA Or ' ⚫
				tempVar = &H26AB Or '⚪
				tempVar = &H2B55 Or' ⭕
				tempVar = &H26A1 Or' ⚡
				tempVar = &H2B1B Or'⬛
				tempVar = &H2B1C Or'⬜
				tempVar = &H274C Or'❌
				tempVar = &H2705 Then '✅

				Return 2
			End If

			' if we arrive here, ucs is not a combining or C0/C1 control character 
			Return If(AscW(character) >= &H1100 AndAlso
				(AscW(character) <= &H115F OrElse ' Hangul Jamo init. consonants 
				AscW(character) = &H2329 OrElse AscW(character) = &H232A OrElse
				(AscW(character) >= &H2E80 AndAlso AscW(character) <= &HA4CF AndAlso
				AscW(character) <> &H303F) OrElse ' CJK ... Yi
				(AscW(character) >= &HAC00 AndAlso AscW(character) <= &HD7A3) OrElse ' Hangul Syllables
				(AscW(character) >= &HF900 AndAlso AscW(character) <= &HFAFF) OrElse ' CJK Compatibility Ideographs
				(AscW(character) >= &HFE10 AndAlso AscW(character) <= &HFE19) OrElse ' Vertical forms
				(AscW(character) >= &HFE30 AndAlso AscW(character) <= &HFE6F) OrElse ' CJK Compatibility Forms
				(AscW(character) >= &HFF00 AndAlso AscW(character) <= &HFF60) OrElse ' Fullwidth Forms
				(AscW(character) >= &HFFE0 AndAlso AscW(character) <= &HFFE6) OrElse
				(AscW(character) >= &H20000 AndAlso AscW(character) <= &H2FFFD) OrElse
				(AscW(character) >= &H30000 AndAlso AscW(character) <= &H3FFFD)),
				2, 1)
		End Function

		Public Function GetWidth(text As String) As Integer
			Dim width As Integer = 0
			For i As Integer = 0 To text.Length - 1
				Dim w As Integer = GetWidth(text(i))
				width += w
			Next i
			Return width
		End Function
	End Module
End Namespace
