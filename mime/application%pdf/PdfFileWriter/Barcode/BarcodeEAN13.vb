#Region "Microsoft.VisualBasic::4608d2af6cc3f52effcfbfa1ad5cb21e, mime\application%pdf\PdfFileWriter\Barcode\BarcodeEAN13.vb"

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

    '   Total Lines: 339
    '    Code Lines: 129 (38.05%)
    ' Comment Lines: 153 (45.13%)
    '    - Xml Docs: 78.43%
    ' 
    '   Blank Lines: 57 (16.81%)
    '     File Size: 11.93 KB


    ' Class BarcodeEAN13
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: BarWidth, GetBarcodeBox
    ' 
    '     Sub: Checksum
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports stdNum = System.Math

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
