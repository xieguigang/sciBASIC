#Region "Microsoft.VisualBasic::ff8692e0dfc97084d2061c0f69d994ed, Microsoft.VisualBasic.Core\src\Language\Language\C\CFormatProvider.vb"

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

    '   Total Lines: 664
    '    Code Lines: 452
    ' Comment Lines: 138
    '   Blank Lines: 74
    '     File Size: 28.77 KB


    '     Module CLangStringFormatProvider
    ' 
    '         Function: FormatHex, FormatNumber, FormatOct, IsPositive, ReplaceMetaChars
    '                   ReplaceMetaCharsMatch, sprintf, ToInteger, ToUnsigned, UnboxToLong
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Imports"
Imports System.Math
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports sys = System.Math
#End Region

Namespace Language.C

    ''' <summary>
    ''' Provides C like format print
    ''' </summary>
    ''' <remarks>https://github.com/mlnlover11/SharpLua</remarks>
    Public Module CLangStringFormatProvider
#Region "Public Methods"

#Region "IsPositive"
        ''' <summary>
        ''' Determines whether the specified value is positive.
        ''' </summary>
        ''' <param name="Value">The value.</param>
        ''' <param name="ZeroIsPositive">if set to true treats 0 as positive.</param>
        ''' <returns>
        ''' true if the specified value is positive; otherwise, false.
        ''' </returns>
        Public Function IsPositive(Value As Object, Optional ZeroIsPositive As Boolean = True) As Boolean
            Select Case Type.GetTypeCode(Value.[GetType]())
                Case TypeCode.[SByte]
                    Return (If(ZeroIsPositive, CSByte(Value) >= 0, CSByte(Value) > 0))
                Case TypeCode.Int16
                    Return (If(ZeroIsPositive, CShort(Value) >= 0, CShort(Value) > 0))
                Case TypeCode.Int32
                    Return (If(ZeroIsPositive, CInt(Value) >= 0, CInt(Value) > 0))
                Case TypeCode.Int64
                    Return (If(ZeroIsPositive, CLng(Value) >= 0, CLng(Value) > 0))
                Case TypeCode.[Single]
                    Return (If(ZeroIsPositive, CSng(Value) >= 0, CSng(Value) > 0))
                Case TypeCode.[Double]
                    Return (If(ZeroIsPositive, CDbl(Value) >= 0, CDbl(Value) > 0))
                Case TypeCode.[Decimal]
                    Return (If(ZeroIsPositive, CDec(Value) >= 0, CDec(Value) > 0))
                Case TypeCode.[Byte]
                    Return (If(ZeroIsPositive, True, CByte(Value) > 0))
                Case TypeCode.UInt16
                    Return (If(ZeroIsPositive, True, CUShort(Value) > 0))
                Case TypeCode.UInt32
                    Return (If(ZeroIsPositive, True, CUInt(Value) > 0))
                Case TypeCode.UInt64
                    Return (If(ZeroIsPositive, True, CULng(Value) > 0))
                Case TypeCode.[Char]
                    Return (If(ZeroIsPositive, True, CChar(Value) <> ControlChars.NullChar))
                Case Else
                    Return False
            End Select
        End Function
#End Region

#Region "ToUnsigned"
        ''' <summary>
        ''' Converts the specified values boxed type to its correpsonding unsigned
        ''' type.
        ''' </summary>
        ''' <param name="Value">The value.</param>
        ''' <returns>A boxed numeric object whos type is unsigned.</returns>
        Public Function ToUnsigned(Value As Object) As Object
            Select Case Type.GetTypeCode(Value.[GetType]())
                Case TypeCode.[SByte]
                    Return CByte(CSByte(Value))
                Case TypeCode.Int16
                    Return CUShort(CShort(Value))
                Case TypeCode.Int32
                    Return CUInt(CInt(Value))
                Case TypeCode.Int64
                    Return CULng(CLng(Value))

                Case TypeCode.[Byte]
                    Return Value
                Case TypeCode.UInt16
                    Return Value
                Case TypeCode.UInt32
                    Return Value
                Case TypeCode.UInt64
                    Return Value

                Case TypeCode.[Single]
                    Return CType(Truncate(CSng(Value)), UInt32)
                Case TypeCode.[Double]
                    Return CULng(Truncate(CDbl(Value)))
                Case TypeCode.[Decimal]
                    Return CULng(Truncate(CDec(Value)))
                Case Else

                    Return Nothing
            End Select
        End Function
#End Region

#Region "ToInteger"
        ''' <summary>
        ''' Converts the specified values boxed type to its correpsonding integer
        ''' type.
        ''' </summary>
        ''' <param name="Value">The value.</param>
        ''' <returns>A boxed numeric object whos type is an integer type.</returns>
        Public Function ToInteger(Value As Object, Round As Boolean) As Object
            Select Case Type.GetTypeCode(Value.[GetType]())
                Case TypeCode.[SByte]
                    Return Value
                Case TypeCode.Int16
                    Return Value
                Case TypeCode.Int32
                    Return Value
                Case TypeCode.Int64
                    Return Value

                Case TypeCode.[Byte]
                    Return Value
                Case TypeCode.UInt16
                    Return Value
                Case TypeCode.UInt32
                    Return Value
                Case TypeCode.UInt64
                    Return Value

                Case TypeCode.[Single]
                    Return (If(Round, CInt(Truncate(sys.Round(CSng(Value)))), CInt(Truncate(CSng(Value)))))
                Case TypeCode.[Double]
                    Return (If(Round, CLng(Truncate(sys.Round(CDbl(Value)))), CLng(Truncate(CDbl(Value)))))
                Case TypeCode.[Decimal]
                    Return (If(Round, sys.Round(CDec(Value)), CDec(Value)))
                Case Else

                    Return Nothing
            End Select
        End Function
#End Region

#Region "UnboxToLong"
        Public Function UnboxToLong(Value As Object, Round As Boolean) As Long
            Select Case Type.GetTypeCode(Value.[GetType]())
                Case TypeCode.[SByte]
                    Return CLng(CSByte(Value))
                Case TypeCode.Int16
                    Return CLng(CShort(Value))
                Case TypeCode.Int32
                    Return CLng(CInt(Value))
                Case TypeCode.Int64
                    Return CLng(Value)

                Case TypeCode.[Byte]
                    Return CLng(CByte(Value))
                Case TypeCode.UInt16
                    Return CLng(CUShort(Value))
                Case TypeCode.UInt32
                    Return CLng(CUInt(Value))
                Case TypeCode.UInt64
                    Return CLng(CULng(Value))

                Case TypeCode.[Single]
                    Return (If(Round, CLng(Truncate(sys.Round(CSng(Value)))), CLng(Truncate(CSng(Value)))))
                Case TypeCode.[Double]
                    Return (If(Round, CLng(Truncate(sys.Round(CDbl(Value)))), CLng(Truncate(CDbl(Value)))))
                Case TypeCode.[Decimal]
                    Return (If(Round, CLng(Truncate(sys.Round(CDec(Value)))), CLng(Truncate(CDec(Value)))))
                Case Else

                    Return 0
            End Select
        End Function
#End Region

#Region "ReplaceMetaChars"
        ''' <summary>
        ''' Replaces the string representations of meta chars with their corresponding
        ''' character values..(替换掉转义字符)
        ''' </summary>
        ''' <param name="input">The input.</param>
        ''' <returns>A string with all string meta chars are replaced</returns>
        <Extension>
        Public Function ReplaceMetaChars(input As String) As String
            Return Regex.Replace(input, "(\\)(\d{3}|[^\d])?", New MatchEvaluator(AddressOf ReplaceMetaCharsMatch))
        End Function

        Private Function ReplaceMetaCharsMatch(m As Match) As String
            ' convert octal quotes (like \040)
            If m.Groups(2).Length = 3 Then
                Return Convert.ToChar(Convert.ToByte(m.Groups(2).Value, 8)).ToString()
            Else
                ' convert all other special meta characters
                'TODO: \xhhh hex and possible dec !!
                Select Case m.Groups(2).Value
                    Case "0"
                        ' null
                        Return vbNullChar
                    Case "a"
                        ' alert (beep)
                        Return ChrW(7)
                    Case "b"
                        ' BS
                        Return vbBack
                    Case "f"
                        ' FF
                        Return vbFormFeed
                    Case "v"
                        ' vertical tab
                        Return vbVerticalTab
                    Case "r"
                        ' CR
                        Return vbCr
                    Case "n"
                        ' LF
                        Return vbLf
                    Case "t"
                        ' Tab
                        Return vbTab
                    Case Else
                        ' if neither an octal quote nor a special meta character
                        ' so just remove the backslash
                        Return m.Groups(2).Value
                End Select
            End If
        End Function
#End Region

#Region "fprintf"

        ''' <summary>
        ''' %[parameter][flags][width][.precision][length]type
        ''' </summary>
        Const Formats As String = "\%(\d*\$)?([\'\#\-\+ ]*)(\d*)(?:\.(\d+))?([hl])?([dioxXucsfeEgGpn%])"

        ReadOnly r As New Regex(Formats)

#End Region
#Region "sprintf"

        ''' <summary>
        ''' Format string like C
        ''' </summary>
        ''' <param name="format"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function sprintf(format$, ParamArray parameters As Object()) As String
            '#Region "Variables"
            Dim f As New StringBuilder()
            'Regex r = new Regex( @"\%(\d*\$)?([\'\#\-\+ ]*)(\d*)(?:\.(\d+))?([hl])?([dioxXucsfeEgGpn%])" );
            '"%[parameter][flags][width][.precision][length]type"
            Dim m As Match = Nothing
            Dim w As String = String.Empty
            Dim defaultParamIx As Integer = 0
            Dim paramIx As Integer
            Dim o As Object = Nothing

            Dim flagLeft2Right As Boolean = False
            Dim flagAlternate As Boolean = False
            Dim flagPositiveSign As Boolean = False
            Dim flagPositiveSpace As Boolean = False
            Dim flagZeroPadding As Boolean = False
            Dim flagGroupThousands As Boolean = False

            Dim fieldLength As Integer = 0
            Dim fieldPrecision As Integer = 0
            Dim shortLongIndicator As Char = ControlChars.NullChar
            Dim formatSpecifier As Char = ControlChars.NullChar
            Dim paddingCharacter As Char = " "c
            '#End Region

            format = format.Replace("\\", "\/")
            format = format.Replace("\n", vbLf)
            format = format.Replace("\t", vbTab)

            ' find all format parameters in format string
            f.Append(format)
            m = r.Match(f.ToString())
            While m.Success
                '#Region "parameter index"
                paramIx = defaultParamIx
                If m.Groups(1) IsNot Nothing AndAlso m.Groups(1).Value.Length > 0 Then
                    Dim val As String = m.Groups(1).Value.Substring(0, m.Groups(1).Value.Length - 1)
                    paramIx = Convert.ToInt32(val) - 1
                End If


                '#End Region

                '#Region "format flags"
                ' extract format flags
                flagAlternate = False
                flagLeft2Right = False
                flagPositiveSign = False
                flagPositiveSpace = False
                flagZeroPadding = False
                flagGroupThousands = False
                If m.Groups(2) IsNot Nothing AndAlso m.Groups(2).Value.Length > 0 Then
                    Dim flags As String = m.Groups(2).Value

                    flagAlternate = (flags.IndexOf("#"c) >= 0)
                    flagLeft2Right = (flags.IndexOf("-"c) >= 0)
                    flagPositiveSign = (flags.IndexOf("+"c) >= 0)
                    flagPositiveSpace = (flags.IndexOf(" "c) >= 0)
                    flagGroupThousands = (flags.IndexOf("'"c) >= 0)

                    ' positive + indicator overrides a
                    ' positive space character
                    If flagPositiveSign AndAlso flagPositiveSpace Then
                        flagPositiveSpace = False
                    End If
                End If
                '#End Region

                '#Region "field length"
                ' extract field length and
                ' pading character
                paddingCharacter = " "c
                fieldLength = Integer.MinValue
                If m.Groups(3) IsNot Nothing AndAlso m.Groups(3).Value.Length > 0 Then
                    fieldLength = Convert.ToInt32(m.Groups(3).Value)
                    flagZeroPadding = (m.Groups(3).Value(0) = "0"c)
                End If
                '#End Region

                If flagZeroPadding Then
                    paddingCharacter = "0"c
                End If

                ' left2right allignment overrides zero padding
                If flagLeft2Right AndAlso flagZeroPadding Then
                    flagZeroPadding = False
                    paddingCharacter = " "c
                End If

                '#Region "field precision"
                ' extract field precision
                fieldPrecision = Integer.MinValue
                If m.Groups(4) IsNot Nothing AndAlso m.Groups(4).Value.Length > 0 Then
                    fieldPrecision = Convert.ToInt32(m.Groups(4).Value)
                End If
                '#End Region

                '#Region "short / long indicator"
                ' extract short / long indicator
                shortLongIndicator = [Char].MinValue
                If m.Groups(5) IsNot Nothing AndAlso m.Groups(5).Value.Length > 0 Then
                    shortLongIndicator = m.Groups(5).Value(0)
                End If
                '#End Region

                '#Region "format specifier"
                ' extract format
                formatSpecifier = [Char].MinValue
                If m.Groups(6) IsNot Nothing AndAlso m.Groups(6).Value.Length > 0 Then
                    formatSpecifier = m.Groups(6).Value(0)
                End If
                '#End Region

                ' default precision is 6 digits if none is specified except
                If fieldPrecision = Integer.MinValue AndAlso formatSpecifier <> "s"c AndAlso formatSpecifier <> "c"c AndAlso [Char].ToUpper(formatSpecifier) <> "X"c AndAlso formatSpecifier <> "o"c Then
                    fieldPrecision = 6
                End If

                '#Region "get next value parameter"
                ' get next value parameter and convert value parameter depending on short / long indicator
                If parameters Is Nothing OrElse paramIx >= parameters.Length Then
                    o = Nothing
                Else
                    o = parameters(paramIx)

                    If shortLongIndicator = "h"c Then
                        If TypeOf o Is Integer Then
                            o = CShort(CInt(o))
                        ElseIf TypeOf o Is Long Then
                            o = CShort(CLng(o))
                        ElseIf TypeOf o Is UInteger Then
                            o = CUShort(CUInt(o))
                        ElseIf TypeOf o Is ULong Then
                            o = CUShort(CULng(o))
                        End If
                    ElseIf shortLongIndicator = "l"c Then
                        If TypeOf o Is Short Then
                            o = CLng(CShort(o))
                        ElseIf TypeOf o Is Integer Then
                            o = CLng(CInt(o))
                        ElseIf TypeOf o Is UShort Then
                            o = CULng(CUShort(o))
                        ElseIf TypeOf o Is UInteger Then
                            o = CULng(CUInt(o))
                        End If
                    End If
                End If
                '#End Region

                ' convert value parameters to a string depending on the formatSpecifier
                w = String.Empty
                Select Case formatSpecifier
                    '#Region "% - character"
                    Case "%"c
                        ' % character
                        w = "%"

                    '#End Region
                    '#Region "d - integer"
                    Case "d"c, "i"c
                        ' integer
                        w = FormatNumber((If(flagGroupThousands, "n", "d")), flagAlternate, fieldLength, Integer.MinValue, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, o)
                        defaultParamIx += 1

                        '#End Region
                        '#Region "i - integer"
                        '   Case "i"c
                        ' integer
                      '  GoTo case "d"C
                    '#End Region
                    '#Region "o - octal integer"
                    Case "o"c
                        ' octal integer - no leading zero
                        w = FormatOct("o", flagAlternate, fieldLength, Integer.MinValue, flagLeft2Right, paddingCharacter,
                            o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "x - hex integer"
                    Case "x"c
                        ' hex integer - no leading zero
                        w = FormatHex("x", flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, paddingCharacter,
                            o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "X - hex integer"
                    Case "X"c
                        ' same as x but with capital hex characters
                        w = FormatHex("X", flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, paddingCharacter,
                            o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "u - unsigned integer"
                    Case "u"c
                        ' unsigned integer
                        w = FormatNumber((If(flagGroupThousands, "n", "d")), flagAlternate, fieldLength, Integer.MinValue, flagLeft2Right, False,
                            False, paddingCharacter, ToUnsigned(o))
                        defaultParamIx += 1

                    '#End Region
                    '#Region "c - character"
                    Case "c"c
                        ' character
                        If IsNumericType(o) Then
                            w = Convert.ToChar(o).ToString()
                        ElseIf TypeOf o Is Char Then
                            w = CChar(o).ToString()
                        ElseIf TypeOf o Is String AndAlso DirectCast(o, String).Length > 0 Then
                            w = DirectCast(o, String)(0).ToString()
                        End If
                        defaultParamIx += 1

                    '#End Region
                    '#Region "s - string"
                    Case "s"c
                        ' string
                        Dim t As String = "{0" & (If(fieldLength <> Integer.MinValue, "," & (If(flagLeft2Right, "-", String.Empty)) & fieldLength.ToString(), String.Empty)) & ":s}"
                        w = Scripting.ToString(o)
                        If fieldPrecision >= 0 Then
                            w = w.Substring(0, fieldPrecision)
                        End If

                        If fieldLength <> Integer.MinValue Then
                            If flagLeft2Right Then
                                w = w.PadRight(fieldLength, paddingCharacter)
                            Else
                                w = w.PadLeft(fieldLength, paddingCharacter)
                            End If
                        End If
                        defaultParamIx += 1

                    '#End Region
                    '#Region "f - double number"
                    Case "f"c
                        ' double
                        w = FormatNumber((If(flagGroupThousands, "n", "f")), flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "e - exponent number"
                    Case "e"c
                        ' double / exponent
                        w = FormatNumber("e", flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "E - exponent number"
                    Case "E"c
                        ' double / exponent
                        w = FormatNumber("E", flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "g - general number"
                    Case "g"c
                        ' double / exponent
                        w = FormatNumber("g", flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "G - general number"
                    Case "G"c
                        ' double / exponent
                        w = FormatNumber("G", flagAlternate, fieldLength, fieldPrecision, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, o)
                        defaultParamIx += 1

                    '#End Region
                    '#Region "p - pointer"
                    Case "p"c
                        ' pointer
                        If TypeOf o Is IntPtr Then

#If XBOX OrElse SILVERLIGHT Then

						w = CType(o, IntPtr).ToString()
				    End If
#Else
                            w = "0x" & CType(o, IntPtr).ToString("x")
                        End If
#End If
                        defaultParamIx += 1

                    '#End Region
                    '#Region "n - number of processed chars so far"
                    Case "n"c
                        ' number of characters so far
                        w = FormatNumber("d", flagAlternate, fieldLength, Integer.MinValue, flagLeft2Right, flagPositiveSign,
                            flagPositiveSpace, paddingCharacter, m.Index)

                    Case Else
                        '#End Region
                        w = String.Empty
                        defaultParamIx += 1

                End Select

                ' replace format parameter with parameter value
                ' and start searching for the next format parameter
                ' AFTER the position of the current inserted value
                ' to prohibit recursive matches if the value also
                ' includes a format specifier
                f.Remove(m.Index, m.Length)
                f.Insert(m.Index, w)
                m = r.Match(f.ToString(), startat:=m.Index + w?.Length)
            End While

            Call f.Replace("\/", "\")

            Return f.ToString()
        End Function
#End Region
#End Region

#Region "Private Methods"
#Region "FormatOCT"
        Private Function FormatOct(NativeFormat As String, Alternate As Boolean, FieldLength As Integer, FieldPrecision As Integer, Left2Right As Boolean, Padding As Char,
            Value As Object) As String
            Dim w As String = String.Empty
            Dim lengthFormat As String = "{0" & (If(FieldLength <> Integer.MinValue, "," & (If(Left2Right, "-", String.Empty)) & FieldLength.ToString(), String.Empty)) & "}"

            If IsNumericType(Value) Then
                w = Convert.ToString(UnboxToLong(Value, True), 8)

                If Left2Right OrElse Padding = " "c Then
                    If Alternate AndAlso w <> "0" Then
                        w = "0" & w
                    End If
                    w = String.Format(lengthFormat, w)
                Else
                    If FieldLength <> Integer.MinValue Then
                        w = w.PadLeft(FieldLength - (If(Alternate AndAlso w <> "0", 1, 0)), Padding)
                    End If
                    If Alternate AndAlso w <> "0" Then
                        w = "0" & w
                    End If
                End If
            End If

            Return w
        End Function
#End Region
#Region "FormatHEX"
        Private Function FormatHex(NativeFormat As String, Alternate As Boolean, FieldLength As Integer, FieldPrecision As Integer, Left2Right As Boolean, Padding As Char,
            Value As Object) As String
            Dim w As String = String.Empty
            Dim lengthFormat As String = "{0" & (If(FieldLength <> Integer.MinValue, "," & (If(Left2Right, "-", String.Empty)) & FieldLength.ToString(), String.Empty)) & "}"
            Dim numberFormat As String = "{0:" & NativeFormat & (If(FieldPrecision <> Integer.MinValue, FieldPrecision.ToString(), String.Empty)) & "}"

            If IsNumericType(Value) Then
                w = String.Format(numberFormat, Value)

                If Left2Right OrElse Padding = " "c Then
                    If Alternate Then
                        w = (If(NativeFormat = "x", "0x", "0X")) & w
                    End If
                    w = String.Format(lengthFormat, w)
                Else
                    If FieldLength <> Integer.MinValue Then
                        w = w.PadLeft(FieldLength - (If(Alternate, 2, 0)), Padding)
                    End If
                    If Alternate Then
                        w = (If(NativeFormat = "x", "0x", "0X")) & w
                    End If
                End If
            End If

            Return w
        End Function
#End Region
#Region "FormatNumber"
        Private Function FormatNumber(NativeFormat As String,
                                  Alternate As Boolean,
                                  FieldLength As Integer,
                                  FieldPrecision As Integer,
                                  Left2Right As Boolean,
                                  PositiveSign As Boolean,
                                  PositiveSpace As Boolean,
                                  Padding As Char,
                                  Value As Object) As String

            Dim w As String = String.Empty
            Dim lengthFormat As String = "{0" & (If(FieldLength <> Integer.MinValue, "," & (If(Left2Right, "-", String.Empty)) & FieldLength.ToString(), String.Empty)) & "}"
            Dim numberFormat As String = "{0:" & NativeFormat & (If(FieldPrecision <> Integer.MinValue, FieldPrecision.ToString(), "0")) & "}"

            If IsNumericType(Value) Then
                w = String.Format(numberFormat, Value)

                If Left2Right OrElse Padding = " "c Then
                    If IsPositive(Value, True) Then
                        w = (If(PositiveSign, "+", (If(PositiveSpace, " ", String.Empty)))) & w
                    End If
                    w = String.Format(lengthFormat, w)
                Else
                    If w.StartsWith("-") Then
                        w = w.Substring(1)
                    End If
                    If FieldLength <> Integer.MinValue Then
                        w = w.PadLeft(FieldLength - 1, Padding)
                    End If
                    If IsPositive(Value, True) Then
                        w = (If(PositiveSign, "+", (If(PositiveSpace, " ", String.Empty)))) & w
                    Else
                        w = "-" & w
                    End If
                End If
            End If

            Return w
        End Function
#End Region
#End Region
    End Module
End Namespace
