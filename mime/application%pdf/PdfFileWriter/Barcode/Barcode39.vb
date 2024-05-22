#Region "Microsoft.VisualBasic::3915f3aed4c978739bfcc32f92cec1f9, mime\application%pdf\PdfFileWriter\Barcode\Barcode39.vb"

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

    '   Total Lines: 230
    '    Code Lines: 101 (43.91%)
    ' Comment Lines: 96 (41.74%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 33 (14.35%)
    '     File Size: 9.03 KB


    ' Class Barcode39
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: BarWidth
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports stdNum = System.Math

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
        Dim str As New StringBuilder()

        For Index = 1 To _CodeArray.Length - 2 - 1
            Dim Code = _CodeArray(Index)
            If Code < 0 OrElse Code >= START_STOP_CODE Then Throw New ApplicationException("Barcode39: Code array contains invalid code (0 to 42)")
            str.Append(CharSet(Code))
        Next

        ' convert str to text
        Text = str.ToString()

        ' exit
        Return
    End Sub
End Class
