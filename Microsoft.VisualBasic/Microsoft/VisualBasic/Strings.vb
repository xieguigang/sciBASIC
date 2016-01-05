Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Text
Imports System.Threading

Namespace Microsoft.VisualBasic
    <StandardModule, DynamicallyInvokableAttribute> _
    Public NotInheritable Class Strings
        ' Methods
        Public Shared Function Asc([String] As Char) As Integer
            Dim num As Integer
            Dim num2 As Integer = Convert.ToInt32([String])
            If (num2 < &H80) Then
                Return num2
            End If
            Try
                Dim buffer As Byte()
                Dim fileIOEncoding As Encoding = Utils.GetFileIOEncoding
                Dim chars As Char() = New Char() {[String]}
                If fileIOEncoding.IsSingleByte Then
                    buffer = New Byte(1 - 1) {}
                    fileIOEncoding.GetBytes(chars, 0, 1, buffer, 0)
                    Return buffer(0)
                End If
                buffer = New Byte(2 - 1) {}
                If (fileIOEncoding.GetBytes(chars, 0, 1, buffer, 0) = 1) Then
                    Return buffer(0)
                End If
                If BitConverter.IsLittleEndian Then
                    Dim num3 As Byte = buffer(0)
                    buffer(0) = buffer(1)
                    buffer(1) = num3
                End If
                num = BitConverter.ToInt16(buffer, 0)
            Catch exception As Exception
                Throw exception
            End Try
            Return num
        End Function

        Public Shared Function Asc([String] As String) As Integer
            If ((Not [String] Is Nothing) AndAlso ([String].Length <> 0)) Then
                Return Strings.Asc([String].Chars(0))
            End If
            Dim args As String() = New String() {"String"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function AscW([String] As Char) As Integer
            Return [String]
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function AscW([String] As String) As Integer
            If ((Not [String] Is Nothing) AndAlso ([String].Length <> 0)) Then
                Return [String].Chars(0)
            End If
            Dim args As String() = New String() {"String"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", args))
        End Function

        Public Shared Function [Chr](CharCode As Integer) As Char
            Dim ch As Char
            If ((CharCode < -32768) OrElse (CharCode > &HFFFF)) Then
                Dim args As String() = New String() {"CharCode"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_RangeTwoBytes1", args))
            End If
            If ((CharCode >= 0) AndAlso (CharCode <= &H7F)) Then
                Return Convert.ToChar(CharCode)
            End If
            Try
                Dim encoding As Encoding = Encoding.GetEncoding(Utils.GetLocaleCodePage)
                If (encoding.IsSingleByte AndAlso ((CharCode < 0) OrElse (CharCode > &HFF))) Then
                    Throw ExceptionUtils.VbMakeException(5)
                End If
                Dim chars As Char() = New Char(2 - 1) {}
                Dim bytes As Byte() = New Byte(2 - 1) {}
                Dim decoder As Decoder = encoding.GetDecoder
                If ((CharCode >= 0) AndAlso (CharCode <= &HFF)) Then
                    bytes(0) = CByte((CharCode And &HFF))
                    decoder.GetChars(bytes, 0, 1, chars, 0)
                Else
                    bytes(0) = CByte(((CharCode And &HFF00) >> 8))
                    bytes(1) = CByte((CharCode And &HFF))
                    decoder.GetChars(bytes, 0, 2, chars, 0)
                End If
                ch = chars(0)
            Catch exception As Exception
                Throw exception
            End Try
            Return ch
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function [ChrW](CharCode As Integer) As Char
            If ((CharCode >= -32768) AndAlso (CharCode <= &HFFFF)) Then
                Return Convert.ToChar(CInt((CharCode And &HFFFF)))
            End If
            Dim args As String() = New String() {"CharCode"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_RangeTwoBytes1", args))
        End Function

        Public Shared Function Filter(Source As Object(), Match As String, Optional Include As Boolean = True, <OptionCompare> Optional [Compare] As CompareMethod = 0) As String()
            Dim num As Integer = Information.UBound(source, 1)
            Dim source As String() = New String((num + 1) - 1) {}
            Try
                Dim num2 As Integer = num
                Dim i As Integer = 0
                Do While (i <= num2)
                    source(i) = Conversions.ToString(source(i))
                    i += 1
                Loop
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim args As String() = New String() {"Source", "String"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", args))
            End Try
            Return Strings.Filter(source, Match, Include, [Compare])
        End Function

        Public Shared Function Filter(Source As String(), Match As String, Optional Include As Boolean = True, <OptionCompare> Optional [Compare] As CompareMethod = 0) As String()
            Dim strArray As String()
            Try
                Dim num2 As Integer
                Dim ignoreCase As CompareOptions
                If (Source.Rank <> 1) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_RankEQOne1"))
                End If
                If ((Match Is Nothing) OrElse (Match.Length = 0)) Then
                    Return Nothing
                End If
                Dim compareInfo As CompareInfo = Utils.GetCultureInfo.CompareInfo
                If ([Compare] = CompareMethod.Text) Then
                    ignoreCase = CompareOptions.IgnoreCase
                End If
                Dim length As Integer = Source.Length
                Dim arySrc As String() = New String(((length - 1) + 1) - 1) {}
                Dim num3 As Integer = (length - 1)
                Dim i As Integer = 0
                Do While (i <= num3)
                    Dim source As String = source(i)
                    If ((Not source Is Nothing) AndAlso ((compareInfo.IndexOf(source, Match, ignoreCase) >= 0) = Include)) Then
                        arySrc(num2) = source
                        num2 += 1
                    End If
                    i += 1
                Loop
                If (num2 = 0) Then
                    Return New String(0 - 1) {}
                End If
                If (num2 = arySrc.Length) Then
                    Return arySrc
                End If
                arySrc = DirectCast(Utils.CopyArray(arySrc, New String(((num2 - 1) + 1) - 1) {}), String())
                strArray = arySrc
            Catch exception As Exception
                Throw exception
            End Try
            Return strArray
        End Function

        Public Shared Function Format(Expression As Object, Optional Style As String = "") As String
            Dim str As String
            Try
                Dim num As Double
                Dim num2 As Single
                Dim formatProvider As IFormatProvider = Nothing
                Dim formattable As IFormattable = Nothing
                If ((Expression Is Nothing) OrElse (Expression.GetType Is Nothing)) Then
                    Return ""
                End If
                If ((Style Is Nothing) OrElse (Style.Length = 0)) Then
                    Return Conversions.ToString(Expression)
                End If
                Dim convertible As IConvertible = DirectCast(Expression, IConvertible)
                Dim typeCode As TypeCode = convertible.GetTypeCode
                If (Style.Length > 0) Then
                    Try
                        Dim returnValue As String = Nothing
                        If Strings.FormatNamed(Expression, Style, returnValue) Then
                            Return returnValue
                        End If
                    Catch exception As StackOverflowException
                        Throw exception
                    Catch exception2 As OutOfMemoryException
                        Throw exception2
                    Catch exception3 As ThreadAbortException
                        Throw exception3
                    Catch exception7 As Exception
                        Return Conversions.ToString(Expression)
                    End Try
                End If
                formattable = TryCast(Expression, IFormattable)
                If (formattable Is Nothing) Then
                    typeCode = Convert.GetTypeCode(Expression)
                    If ((typeCode <> TypeCode.String) AndAlso (typeCode <> TypeCode.Boolean)) Then
                        Dim args As String() = New String() {"Expression"}
                        Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                    End If
                End If
                Select Case typeCode
                    Case TypeCode.Empty
                        Return ""
                    Case TypeCode.Object, TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Decimal, TypeCode.DateTime
                        Return formattable.ToString(Style, formatProvider)
                    Case TypeCode.DBNull
                        Return ""
                    Case TypeCode.Boolean
                        Dim objArray1 As Object() = New Object() {Conversions.ToString(convertible.ToBoolean(Nothing))}
                        Return String.Format(formatProvider, Style, objArray1)
                    Case TypeCode.Single
                        num2 = convertible.ToSingle(Nothing)
                        If ((Not Style Is Nothing) AndAlso (Style.Length <> 0)) Then
                            GoTo Label_01B6
                        End If
                        Return Conversions.ToString(num2)
                    Case TypeCode.Double
                        num = convertible.ToDouble(Nothing)
                        If ((Not Style Is Nothing) AndAlso (Style.Length <> 0)) Then
                            Exit Select
                        End If
                        Return Conversions.ToString(num)
                    Case TypeCode.String
                        Dim objArray2 As Object() = New Object() {Expression}
                        Return String.Format(formatProvider, Style, objArray2)
                    Case Else
                        GoTo Label_01E6
                End Select
                If (num = 0) Then
                    num = 0
                End If
                Return num.ToString(Style, formatProvider)
Label_01B6:
                If (num2 = 0!) Then
                    num2 = 0!
                End If
                Return num2.ToString(Style, formatProvider)
Label_01E6:
                str = formattable.ToString(Style, formatProvider)
            Catch exception4 As Exception
                Throw exception4
            End Try
            Return str
        End Function

        Public Shared Function FormatCurrency(Expression As Object, Optional NumDigitsAfterDecimal As Integer = -1, Optional IncludeLeadingDigit As TriState = -2, Optional UseParensForNegativeNumbers As TriState = -2, Optional GroupDigits As TriState = -2) As String
            Dim str As String
            Dim formatProvider As IFormatProvider = Nothing
            Try
                Strings.ValidateTriState(IncludeLeadingDigit)
                Strings.ValidateTriState(UseParensForNegativeNumbers)
                Strings.ValidateTriState(GroupDigits)
                If (NumDigitsAfterDecimal > &H63) Then
                    Dim args As String() = New String() {"NumDigitsAfterDecimal"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_Range0to99_1", args))
                End If
                If (Expression Is Nothing) Then
                    Return ""
                End If
                Dim type As Type = Expression.GetType
                If (type Is GetType(String)) Then
                    Expression = Conversions.ToDouble(Expression)
                ElseIf Not Symbols.IsNumericType(type) Then
                    Dim textArray2 As String() = New String() {Utils.VBFriendlyName(type), "Currency"}
                    Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", textArray2))
                End If
                If (IncludeLeadingDigit = TriState.False) Then
                    Dim num As Double = Conversions.ToDouble(Expression)
                    If ((num >= 1) OrElse (num <= -1)) Then
                        IncludeLeadingDigit = TriState.True
                    End If
                End If
                str = DirectCast(Expression, IFormattable).ToString(Strings.GetCurrencyFormatString(IncludeLeadingDigit, NumDigitsAfterDecimal, UseParensForNegativeNumbers, GroupDigits, formatProvider), formatProvider)
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Public Shared Function FormatDateTime(Expression As DateTime, Optional NamedFormat As DateFormat = 0) As String
            Dim str As String
            Try
                Dim str2 As String
                Select Case NamedFormat
                    Case DateFormat.GeneralDate
                        If (Expression.TimeOfDay.Ticks <> Expression.Ticks) Then
                            Exit Select
                        End If
                        str2 = "T"
                        GoTo Label_0086
                    Case DateFormat.LongDate
                        str2 = "D"
                        GoTo Label_0086
                    Case DateFormat.ShortDate
                        str2 = "d"
                        GoTo Label_0086
                    Case DateFormat.LongTime
                        str2 = "T"
                        GoTo Label_0086
                    Case DateFormat.ShortTime
                        str2 = "HH:mm"
                        GoTo Label_0086
                    Case Else
                        Throw ExceptionUtils.VbMakeException(5)
                End Select
                If (Expression.TimeOfDay.Ticks = 0) Then
                    str2 = "d"
                Else
                    str2 = "G"
                End If
Label_0086:
                str = Expression.ToString(str2, Nothing)
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Private Shared Function FormatNamed(Expression As Object, Style As String, ByRef ReturnValue As String) As Boolean
            Dim length As Integer = Style.Length
            ReturnValue = Nothing
            Select Case length
                Case 5
                    Dim ch As Char = Style.Chars(0)
                    If (((ch <> "F"c) AndAlso (ch <> "f"c)) OrElse (String.Compare(Style, "fixed", StringComparison.OrdinalIgnoreCase) <> 0)) Then
                        GoTo Label_04C5
                    End If
                    ReturnValue = Conversions.ToDouble(Expression).ToString("0.00", Nothing)
                    Return True
                Case 6
                    Dim ch2 As Char = Style.Chars(0)
                    If (ch2 > "Y"c) Then
                        If (ch2 = "o"c) Then
                            GoTo Label_00E4
                        End If
                        If (ch2 = "y"c) Then
                            Exit Select
                        End If
                    Else
                        Select Case ch2
                            Case "O"c
                                GoTo Label_00E4
                        End Select
                    End If
                    GoTo Label_04C5
                Case 7
                    Dim ch3 As Char = Style.Chars(0)
                    If (((ch3 <> "P"c) AndAlso (ch3 <> "p"c)) OrElse (String.Compare(Style, "percent", StringComparison.OrdinalIgnoreCase) <> 0)) Then
                        GoTo Label_04C5
                    End If
                    ReturnValue = Conversions.ToDouble(Expression).ToString("0.00%", Nothing)
                    Return True
                Case 8
                    Dim ch4 As Char = Style.Chars(0)
                    If (ch4 > "S"c) Then
                        If (ch4 = "c"c) Then
                            GoTo Label_01BA
                        End If
                        If (ch4 <> "s"c) Then
                            GoTo Label_04C5
                        End If
                        GoTo Label_018C
                    End If
                    Select Case ch4
                        Case "C"c
                            GoTo Label_01BA
                        Case "S"c
                            GoTo Label_018C
                    End Select
                    GoTo Label_04C5
                Case 9
                    Dim ch5 As Char = Style.Chars(5)
                    If (ch5 > "T"c) Then
                        If (ch5 = "d"c) Then
                            GoTo Label_0246
                        End If
                        If (ch5 <> "t"c) Then
                            GoTo Label_04C5
                        End If
                        GoTo Label_0217
                    End If
                    Select Case ch5
                        Case "D"c
                            GoTo Label_0246
                        Case "T"c
                            GoTo Label_0217
                    End Select
                    GoTo Label_04C5
                Case 10
                    Dim ch6 As Char = Style.Chars(6)
                    If (ch6 > "T"c) Then
                        Select Case ch6
                            Case "a"c
                                GoTo Label_02DD
                            Case "d"c
                                GoTo Label_033F
                            Case "i"c
                                GoTo Label_036E
                            Case "t"c
                                GoTo Label_0310
                        End Select
                    ElseIf (ch6 > "D"c) Then
                        Select Case ch6
                            Case "I"c
                                GoTo Label_036E
                            Case "T"c
                                GoTo Label_0310
                        End Select
                    Else
                        Select Case ch6
                            Case "A"c
                                GoTo Label_02DD
                            Case "D"c
                                GoTo Label_033F
                        End Select
                    End If
                    GoTo Label_04C5
                Case 11
                    Dim ch7 As Char = Style.Chars(7)
                    If (ch7 > "T"c) Then
                        If (ch7 = "d"c) Then
                            GoTo Label_041E
                        End If
                        If (ch7 <> "t"c) Then
                            GoTo Label_04C5
                        End If
                        GoTo Label_03EF
                    End If
                    Select Case ch7
                        Case "D"c
                            GoTo Label_041E
                        Case "T"c
                            GoTo Label_03EF
                    End Select
                    GoTo Label_04C5
                Case 12
                    Dim ch8 As Char = Style.Chars(0)
                    If (((ch8 <> "G"c) AndAlso (ch8 <> "g"c)) OrElse (String.Compare(Style, "general date", StringComparison.OrdinalIgnoreCase) <> 0)) Then
                        GoTo Label_04C5
                    End If
                    ReturnValue = Conversions.ToDate(Expression).ToString("G", Nothing)
                    Return True
                Case 14
                    Dim ch9 As Char = Style.Chars(0)
                    If (((ch9 = "G"c) OrElse (ch9 = "g"c)) AndAlso (String.Compare(Style, "general number", StringComparison.OrdinalIgnoreCase) = 0)) Then
                        ReturnValue = Conversions.ToDouble(Expression).ToString("G", Nothing)
                        Return True
                    End If
                    GoTo Label_04C5
                Case Else
                    GoTo Label_04C5
            End Select
            If (String.Compare(Style, "yes/no", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = CInt(-(Conversions.ToBoolean(Expression) > False)).ToString(Strings.CachedYesNoFormatStyle, Nothing)
            Return True
Label_00E4:
            If (String.Compare(Style, "on/off", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = CInt(-(Conversions.ToBoolean(Expression) > False)).ToString(Strings.CachedOnOffFormatStyle, Nothing)
            Return True
Label_018C:
            If (String.Compare(Style, "standard", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDouble(Expression).ToString("N2", Nothing)
            Return True
Label_01BA:
            If (String.Compare(Style, "currency", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDouble(Expression).ToString("C", Nothing)
            Return True
Label_0217:
            If (String.Compare(Style, "long time", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDate(Expression).ToString("T", Nothing)
            Return True
Label_0246:
            If (String.Compare(Style, "long date", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDate(Expression).ToString("D", Nothing)
            Return True
Label_02DD:
            If (String.Compare(Style, "true/false", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = CInt(-(Conversions.ToBoolean(Expression) > False)).ToString(Strings.CachedTrueFalseFormatStyle, Nothing)
            Return True
Label_0310:
            If (String.Compare(Style, "short time", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDate(Expression).ToString("t", Nothing)
            Return True
Label_033F:
            If (String.Compare(Style, "short date", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDate(Expression).ToString("d", Nothing)
            Return True
Label_036E:
            If (String.Compare(Style, "scientific", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            Dim d As Double = Conversions.ToDouble(Expression)
            If (Double.IsNaN(d) OrElse Double.IsInfinity(d)) Then
                ReturnValue = d.ToString("G", Nothing)
            Else
                ReturnValue = d.ToString("0.00E+00", Nothing)
            End If
            Return True
Label_03EF:
            If (String.Compare(Style, "medium time", StringComparison.OrdinalIgnoreCase) <> 0) Then
                GoTo Label_04C5
            End If
            ReturnValue = Conversions.ToDate(Expression).ToString("T", Nothing)
            Return True
Label_041E:
            If (String.Compare(Style, "medium date", StringComparison.OrdinalIgnoreCase) = 0) Then
                ReturnValue = Conversions.ToDate(Expression).ToString("D", Nothing)
                Return True
            End If
Label_04C5:
            Return False
        End Function

        Public Shared Function FormatNumber(Expression As Object, Optional NumDigitsAfterDecimal As Integer = -1, Optional IncludeLeadingDigit As TriState = -2, Optional UseParensForNegativeNumbers As TriState = -2, Optional GroupDigits As TriState = -2) As String
            Dim str As String
            Try
                Strings.ValidateTriState(IncludeLeadingDigit)
                Strings.ValidateTriState(UseParensForNegativeNumbers)
                Strings.ValidateTriState(GroupDigits)
                If (Expression Is Nothing) Then
                    Return ""
                End If
                Dim type As Type = Expression.GetType
                If (type Is GetType(String)) Then
                    Expression = Conversions.ToDouble(Expression)
                ElseIf (type Is GetType(Boolean)) Then
                    If Conversions.ToBoolean(Expression) Then
                        Expression = -1
                    Else
                        Expression = 0
                    End If
                ElseIf Not Symbols.IsNumericType(type) Then
                    Dim args As String() = New String() {Utils.VBFriendlyName(type), "Currency"}
                    Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
                End If
                str = DirectCast(Expression, IFormattable).ToString(Strings.GetNumberFormatString(NumDigitsAfterDecimal, IncludeLeadingDigit, UseParensForNegativeNumbers, GroupDigits), Nothing)
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Public Shared Function FormatPercent(Expression As Object, Optional NumDigitsAfterDecimal As Integer = -1, Optional IncludeLeadingDigit As TriState = -2, Optional UseParensForNegativeNumbers As TriState = -2, Optional GroupDigits As TriState = -2) As String
            Strings.ValidateTriState(IncludeLeadingDigit)
            Strings.ValidateTriState(UseParensForNegativeNumbers)
            Strings.ValidateTriState(GroupDigits)
            If (Expression Is Nothing) Then
                Return ""
            End If
            Dim type As Type = Expression.GetType
            If (type Is GetType(String)) Then
                Expression = Conversions.ToDouble(Expression)
            ElseIf Not Symbols.IsNumericType(type) Then
                Dim args As String() = New String() {Utils.VBFriendlyName(type), "numeric"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
            End If
            Return DirectCast(Expression, IFormattable).ToString(Strings.GetFormatString(NumDigitsAfterDecimal, IncludeLeadingDigit, UseParensForNegativeNumbers, GroupDigits, FormatType.Percent), Nothing)
        End Function

        Public Shared Function GetChar(str As String, Index As Integer) As Char
            If (str Is Nothing) Then
                Dim args As String() = New String() {"String"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", args))
            End If
            If (Index < 1) Then
                Dim textArray2 As String() = New String() {"Index"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GEOne1", textArray2))
            End If
            If (Index > str.Length) Then
                Dim textArray3 As String() = New String() {"Index", "String"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_IndexLELength2", textArray3))
            End If
            Return str.Chars((Index - 1))
        End Function

        Friend Shared Function GetCurrencyFormatString(IncludeLeadingDigit As TriState, NumDigitsAfterDecimal As Integer, UseParensForNegativeNumbers As TriState, GroupDigits As TriState, ByRef formatProvider As IFormatProvider) As String
            Dim str3 As String
            Dim str As String = "C"
            Dim format As NumberFormatInfo = DirectCast(Utils.GetCultureInfo.GetFormat(GetType(NumberFormatInfo)), NumberFormatInfo)
            format = DirectCast(format.Clone, NumberFormatInfo)
            If (GroupDigits = TriState.False) Then
                format.CurrencyGroupSizes = New Integer(1 - 1) {}
            End If
            Dim currencyPositivePattern As Integer = format.CurrencyPositivePattern
            Dim currencyNegativePattern As Integer = format.CurrencyNegativePattern
            If (UseParensForNegativeNumbers = TriState.UseDefault) Then
                Select Case currencyNegativePattern
                    Case 14, 15, 0, 4
                        UseParensForNegativeNumbers = TriState.True
                        GoTo Label_00F1
                End Select
                UseParensForNegativeNumbers = TriState.False
            ElseIf (UseParensForNegativeNumbers = TriState.False) Then
                Select Case currencyNegativePattern
                    Case 14
                        currencyNegativePattern = 9
                        Exit Select
                    Case 15
                        currencyNegativePattern = 10
                        Exit Select
                    Case 0
                        currencyNegativePattern = 1
                        Exit Select
                    Case 4
                        currencyNegativePattern = 5
                        Exit Select
                End Select
            Else
                UseParensForNegativeNumbers = TriState.True
                Select Case currencyNegativePattern
                    Case 1, 2, 3
                        currencyNegativePattern = 0
                        Exit Select
                    Case 5, 6, 7
                        currencyNegativePattern = 4
                        Exit Select
                    Case 8, 10, 13
                        currencyNegativePattern = 15
                        Exit Select
                    Case 9, 11, 12
                        currencyNegativePattern = 14
                        Exit Select
                End Select
            End If
Label_00F1:
            format.CurrencyNegativePattern = currencyNegativePattern
            If (NumDigitsAfterDecimal = -1) Then
                NumDigitsAfterDecimal = format.CurrencyDecimalDigits
            End If
            format.CurrencyDecimalDigits = NumDigitsAfterDecimal
            formatProvider = New FormatInfoHolder(format)
            If (IncludeLeadingDigit <> TriState.False) Then
                Return str
            End If
            format.NumberGroupSizes = format.CurrencyGroupSizes
            Dim str2 As String = (Strings.CurrencyPositiveFormatStrings(currencyPositivePattern) & ";" & Strings.CurrencyNegativeFormatStrings(currencyNegativePattern))
            If (GroupDigits = TriState.False) Then
                If (IncludeLeadingDigit = TriState.False) Then
                    str3 = "#"
                Else
                    str3 = "0"
                End If
            ElseIf (IncludeLeadingDigit = TriState.False) Then
                str3 = "#,###"
            Else
                str3 = "#,##0"
            End If
            If (NumDigitsAfterDecimal > 0) Then
                str3 = (str3 & "." & New String("0"c, NumDigitsAfterDecimal))
            End If
            If (String.CompareOrdinal("$", format.CurrencySymbol) <> 0) Then
                str2 = str2.Replace("$", format.CurrencySymbol.Replace("'", "''"))
            End If
            Return str2.Replace("n", str3)
        End Function

        Friend Shared Function GetFormatString(NumDigitsAfterDecimal As Integer, IncludeLeadingDigit As TriState, UseParensForNegativeNumbers As TriState, GroupDigits As TriState, FormatTypeValue As FormatType) As String
            Dim str As String
            Dim str2 As String
            Dim str3 As String
            Dim builder As New StringBuilder(30)
            Dim format As NumberFormatInfo = DirectCast(Utils.GetCultureInfo.GetFormat(GetType(NumberFormatInfo)), NumberFormatInfo)
            If (NumDigitsAfterDecimal < -1) Then
                Throw ExceptionUtils.VbMakeException(5)
            End If
            If (NumDigitsAfterDecimal = -1) Then
                If (FormatTypeValue = FormatType.Percent) Then
                    NumDigitsAfterDecimal = format.NumberDecimalDigits
                ElseIf (FormatTypeValue = FormatType.Number) Then
                    NumDigitsAfterDecimal = format.NumberDecimalDigits
                ElseIf (FormatTypeValue = FormatType.Currency) Then
                    NumDigitsAfterDecimal = format.CurrencyDecimalDigits
                End If
            End If
            If (GroupDigits = TriState.UseDefault) Then
                GroupDigits = TriState.True
                If (FormatTypeValue = FormatType.Percent) Then
                    If Strings.IsArrayEmpty(format.PercentGroupSizes) Then
                        GroupDigits = TriState.False
                    End If
                ElseIf (FormatTypeValue = FormatType.Number) Then
                    If Strings.IsArrayEmpty(format.NumberGroupSizes) Then
                        GroupDigits = TriState.False
                    End If
                ElseIf ((FormatTypeValue = FormatType.Currency) AndAlso Strings.IsArrayEmpty(format.CurrencyGroupSizes)) Then
                    GroupDigits = TriState.False
                End If
            End If
            If (UseParensForNegativeNumbers = TriState.UseDefault) Then
                UseParensForNegativeNumbers = TriState.False
                If (FormatTypeValue = FormatType.Number) Then
                    If (format.NumberNegativePattern = 0) Then
                        UseParensForNegativeNumbers = TriState.True
                    End If
                ElseIf ((FormatTypeValue = FormatType.Currency) AndAlso (format.CurrencyNegativePattern = 0)) Then
                    UseParensForNegativeNumbers = TriState.True
                End If
            End If
            If (GroupDigits = TriState.True) Then
                str = "#,##"
            Else
                str = ""
            End If
            If (IncludeLeadingDigit <> TriState.False) Then
                str2 = "0"
            Else
                str2 = "#"
            End If
            If (NumDigitsAfterDecimal > 0) Then
                str3 = ("." & New String("0"c, NumDigitsAfterDecimal))
            Else
                str3 = ""
            End If
            If (FormatTypeValue = FormatType.Currency) Then
                builder.Append(format.CurrencySymbol)
            End If
            builder.Append(str)
            builder.Append(str2)
            builder.Append(str3)
            If (FormatTypeValue = FormatType.Percent) Then
                builder.Append(format.PercentSymbol)
            End If
            If (UseParensForNegativeNumbers = TriState.True) Then
                Dim str4 As String = builder.ToString
                builder.Append(";(")
                builder.Append(str4)
                builder.Append(")")
            End If
            Return builder.ToString
        End Function

        Friend Shared Function GetNumberFormatString(NumDigitsAfterDecimal As Integer, IncludeLeadingDigit As TriState, UseParensForNegativeNumbers As TriState, GroupDigits As TriState) As String
            Dim str2 As String
            Dim format As NumberFormatInfo = DirectCast(Utils.GetCultureInfo.GetFormat(GetType(NumberFormatInfo)), NumberFormatInfo)
            If (NumDigitsAfterDecimal = -1) Then
                NumDigitsAfterDecimal = format.NumberDecimalDigits
            ElseIf ((NumDigitsAfterDecimal > &H63) OrElse (NumDigitsAfterDecimal < -1)) Then
                Dim args As String() = New String() {"NumDigitsAfterDecimal"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_Range0to99_1", args))
            End If
            If (GroupDigits = TriState.UseDefault) Then
                If ((format.NumberGroupSizes Is Nothing) OrElse (format.NumberGroupSizes.Length = 0)) Then
                    GroupDigits = TriState.False
                Else
                    GroupDigits = TriState.True
                End If
            End If
            Dim numberNegativePattern As Integer = format.NumberNegativePattern
            If (UseParensForNegativeNumbers = TriState.UseDefault) Then
                If (numberNegativePattern = 0) Then
                    UseParensForNegativeNumbers = TriState.True
                Else
                    UseParensForNegativeNumbers = TriState.False
                End If
            ElseIf (UseParensForNegativeNumbers = TriState.False) Then
                If (numberNegativePattern = 0) Then
                    numberNegativePattern = 1
                End If
            Else
                UseParensForNegativeNumbers = TriState.True
                Select Case numberNegativePattern
                    Case 1, 2, 3, 4
                        numberNegativePattern = 0
                        Exit Select
                End Select
            End If
            If (UseParensForNegativeNumbers = TriState.UseDefault) Then
                UseParensForNegativeNumbers = TriState.True
            End If
            Dim expression As String = ("n;" & Strings.NumberNegativeFormatStrings(numberNegativePattern))
            If (String.CompareOrdinal("-", format.NegativeSign) <> 0) Then
                expression = expression.Replace("-", ("""" & format.NegativeSign & """"))
            End If
            If (IncludeLeadingDigit <> TriState.False) Then
                str2 = "0"
            Else
                str2 = "#"
            End If
            If ((GroupDigits <> TriState.False) AndAlso (format.NumberGroupSizes.Length <> 0)) Then
                If (format.NumberGroupSizes.Length = 1) Then
                    str2 = ("#," & New String("#"c, format.NumberGroupSizes(0)) & str2)
                Else
                    str2 = (New String("#"c, (format.NumberGroupSizes(0) - 1)) & str2)
                    Dim upperBound As Integer = format.NumberGroupSizes.GetUpperBound(0)
                    Dim i As Integer = 1
                    Do While (i <= upperBound)
                        str2 = ("," & New String("#"c, format.NumberGroupSizes(i)) & "," & str2)
                        i += 1
                    Loop
                End If
            End If
            If (NumDigitsAfterDecimal > 0) Then
                str2 = (str2 & "." & New String("0"c, NumDigitsAfterDecimal))
            End If
            Return Strings.Replace(expression, "n", str2, 1, -1, CompareMethod.Binary)
        End Function

        Public Shared Function InStr(String1 As String, String2 As String, <OptionCompare> Optional [Compare] As CompareMethod = 0) As Integer
            If ([Compare] = CompareMethod.Binary) Then
                Return (Strings.InternalInStrBinary(0, String1, String2) + 1)
            End If
            Return (Strings.InternalInStrText(0, String1, String2) + 1)
        End Function

        Public Shared Function InStr(Start As Integer, String1 As String, String2 As String, <OptionCompare> Optional [Compare] As CompareMethod = 0) As Integer
            If (Start < 1) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GTZero1", args))
            End If
            If ([Compare] = CompareMethod.Binary) Then
                Return (Strings.InternalInStrBinary((Start - 1), String1, String2) + 1)
            End If
            Return (Strings.InternalInStrText((Start - 1), String1, String2) + 1)
        End Function

        Public Shared Function InStrRev(StringCheck As String, StringMatch As String, Optional Start As Integer = -1, <OptionCompare> Optional [Compare] As CompareMethod = 0) As Integer
            Dim num As Integer
            Try
                Dim length As Integer
                If ((Start = 0) OrElse (Start < -1)) Then
                    Dim args As String() = New String() {"Start"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_MinusOneOrGTZero1", args))
                End If
                If (StringCheck Is Nothing) Then
                    length = 0
                Else
                    length = StringCheck.Length
                End If
                If (Start = -1) Then
                    Start = length
                End If
                If ((Start > length) OrElse (length = 0)) Then
                    Return 0
                End If
                If ((StringMatch Is Nothing) OrElse (StringMatch.Length = 0)) Then
                    Return Start
                End If
                If ([Compare] = CompareMethod.Binary) Then
                    Return (Strings.m_InvariantCompareInfo.LastIndexOf(StringCheck, StringMatch, (Start - 1), Start, CompareOptions.Ordinal) + 1)
                End If
                num = (Utils.GetCultureInfo.CompareInfo.LastIndexOf(StringCheck, StringMatch, (Start - 1), Start, (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase))) + 1)
            Catch exception As Exception
                Throw exception
            End Try
            Return num
        End Function

        Private Shared Function InternalInStrBinary(StartPos As Integer, sSrc As String, sFind As String) As Integer
            Dim length As Integer
            If (Not sSrc Is Nothing) Then
                length = sSrc.Length
            Else
                length = 0
            End If
            If ((StartPos > length) OrElse (length = 0)) Then
                Return -1
            End If
            If ((sFind Is Nothing) OrElse (sFind.Length = 0)) Then
                Return StartPos
            End If
            Return Strings.m_InvariantCompareInfo.IndexOf(sSrc, sFind, StartPos, CompareOptions.Ordinal)
        End Function

        Private Shared Function InternalInStrText(lStartPos As Integer, sSrc As String, sFind As String) As Integer
            Dim length As Integer
            If (Not sSrc Is Nothing) Then
                length = sSrc.Length
            Else
                length = 0
            End If
            If ((lStartPos > length) OrElse (length = 0)) Then
                Return -1
            End If
            If ((sFind Is Nothing) OrElse (sFind.Length = 0)) Then
                Return lStartPos
            End If
            Return Utils.GetCultureInfo.CompareInfo.IndexOf(sSrc, sFind, lStartPos, (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase)))
        End Function

        Private Shared Function InternalStrReverse(Expression As String, SrcIndex As Integer, Length As Integer) As String
            Dim builder As New StringBuilder(Length) With {
                .Length = Length
            }
            Dim textElementEnumerator As TextElementEnumerator = StringInfo.GetTextElementEnumerator(Expression, SrcIndex)
            If Not textElementEnumerator.MoveNext Then
                Return ""
            End If
            Dim num2 As Integer = 0
            Dim num As Integer = (Length - 1)
            Do While (num2 < SrcIndex)
                builder.Chars(num) = Expression.Chars(num2)
                num -= 1
                num2 += 1
            Loop
            Dim elementIndex As Integer = textElementEnumerator.ElementIndex
            Do While (num >= 0)
                SrcIndex = elementIndex
                If textElementEnumerator.MoveNext Then
                    elementIndex = textElementEnumerator.ElementIndex
                Else
                    elementIndex = Length
                End If
                num2 = (elementIndex - 1)
                Do While (num2 >= SrcIndex)
                    builder.Chars(num) = Expression.Chars(num2)
                    num -= 1
                    num2 -= 1
                Loop
            Loop
            Return builder.ToString
        End Function

        Private Shared Function IsArrayEmpty(array As Array) As Boolean
            Return ((array Is Nothing) OrElse (array.Length = 0))
        End Function

        Friend Shared Function IsValidCodePage(codepage As Integer) As Boolean
            Dim flag As Boolean = False
            Try
                If (Not Encoding.GetEncoding(codepage) Is Nothing) Then
                    flag = True
                End If
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
            End Try
            Return flag
        End Function

        Public Shared Function Join(SourceArray As Object(), Optional Delimiter As String = " ") As String
            Dim num As Integer = Information.UBound(sourceArray, 1)
            Dim sourceArray As String() = New String((num + 1) - 1) {}
            Try
                Dim num3 As Integer = num
                Dim i As Integer = 0
                Do While (i <= num3)
                    sourceArray(i) = Conversions.ToString(sourceArray(i))
                    i += 1
                Loop
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim args As String() = New String() {"SourceArray", "String"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", args))
            End Try
            Return Strings.Join(sourceArray, Delimiter)
        End Function

        Public Shared Function Join(SourceArray As String(), Optional Delimiter As String = " ") As String
            Dim str As String
            Try
                If Strings.IsArrayEmpty(SourceArray) Then
                    Return Nothing
                End If
                If (SourceArray.Rank <> 1) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_RankEQOne1"))
                End If
                str = String.Join(Delimiter, SourceArray)
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Public Shared Function LCase(Value As Char) As Char
            Dim ch As Char
            Try
                ch = Thread.CurrentThread.CurrentCulture.TextInfo.ToLower(Value)
            Catch exception As Exception
                Throw exception
            End Try
            Return ch
        End Function

        Public Shared Function LCase(Value As String) As String
            Dim str As String
            Try
                If (Value Is Nothing) Then
                    Return Nothing
                End If
                str = Thread.CurrentThread.CurrentCulture.TextInfo.ToLower(Value)
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Public Shared Function Left(str As String, Length As Integer) As String
            If (Length < 0) Then
                Dim args As String() = New String() {"Length"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GEZero1", args))
            End If
            If ((Length = 0) OrElse (str Is Nothing)) Then
                Return ""
            End If
            If (Length >= str.Length) Then
                Return str
            End If
            Return str.Substring(0, Length)
        End Function

        Public Shared Function Len(Expression As Boolean) As Integer
            Return 2
        End Function

        Public Shared Function Len(Expression As Byte) As Integer
            Return 1
        End Function

        Public Shared Function Len(Expression As Char) As Integer
            Return 2
        End Function

        Public Shared Function Len(Expression As DateTime) As Integer
            Return 8
        End Function

        Public Shared Function Len(Expression As Decimal) As Integer
            Return 8
        End Function

        Public Shared Function Len(Expression As Double) As Integer
            Return 8
        End Function

        Public Shared Function Len(Expression As Short) As Integer
            Return 2
        End Function

        Public Shared Function Len(Expression As Integer) As Integer
            Return 4
        End Function

        Public Shared Function Len(Expression As Long) As Integer
            Return 8
        End Function

        <SecuritySafeCritical>
        Public Shared Function Len(Expression As Object) As Integer
            If (Expression Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Expression, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return 2
                    Case TypeCode.Char
                        Return 2
                    Case TypeCode.SByte
                        Return 1
                    Case TypeCode.Byte
                        Return 1
                    Case TypeCode.Int16
                        Return 2
                    Case TypeCode.UInt16
                        Return 2
                    Case TypeCode.Int32
                        Return 4
                    Case TypeCode.UInt32
                        Return 4
                    Case TypeCode.Int64
                        Return 8
                    Case TypeCode.UInt64
                        Return 8
                    Case TypeCode.Single
                        Return 4
                    Case TypeCode.Double
                        Return 8
                    Case TypeCode.Decimal
                        Return &H10
                    Case TypeCode.DateTime
                        Return 8
                    Case TypeCode.String
                        Return Expression.ToString.Length
                End Select
            Else
                Dim chArray As Char() = TryCast(Expression, Char())
                If (Not chArray Is Nothing) Then
                    Return chArray.Length
                End If
            End If
            If Not TypeOf Expression Is ValueType Then
                Throw ExceptionUtils.VbMakeException(13)
            End If
            Call New ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert
            PermissionSet.RevertAssert()
            Return StructUtils.GetRecordLength(Expression, 1)
        End Function

        <CLSCompliant(False)>
        Public Shared Function Len(Expression As SByte) As Integer
            Return 1
        End Function

        Public Shared Function Len(Expression As Single) As Integer
            Return 4
        End Function

        Public Shared Function Len(Expression As String) As Integer
            If (Expression Is Nothing) Then
                Return 0
            End If
            Return Expression.Length
        End Function

        <CLSCompliant(False)>
        Public Shared Function Len(Expression As UInt16) As Integer
            Return 2
        End Function

        <CLSCompliant(False)>
        Public Shared Function Len(Expression As UInt32) As Integer
            Return 4
        End Function

        <CLSCompliant(False)>
        Public Shared Function Len(Expression As UInt64) As Integer
            Return 8
        End Function

        Public Shared Function LSet(Source As String, Length As Integer) As String
            If (Length = 0) Then
                Return ""
            End If
            If (Source Is Nothing) Then
                Return New String(" "c, Length)
            End If
            If (Length > Source.Length) Then
                Return Source.PadRight(Length)
            End If
            Return Source.Substring(0, Length)
        End Function

        Public Shared Function LTrim(str As String) As String
            If ((str Is Nothing) OrElse (str.Length = 0)) Then
                Return ""
            End If
            Select Case str.Chars(0)
                Case " "c, ChrW(12288)
                    Return str.TrimStart(Utils.m_achIntlSpace)
            End Select
            Return str
        End Function

        Public Shared Function Mid(str As String, Start As Integer) As String
            Dim str2 As String
            Try
                If (str Is Nothing) Then
                    Return Nothing
                End If
                str2 = Strings.Mid(str, Start, str.Length)
            Catch exception As Exception
                Throw exception
            End Try
            Return str2
        End Function

        Public Shared Function Mid(str As String, Start As Integer, Length As Integer) As String
            If (Start <= 0) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GTZero1", args))
            End If
            If (length < 0) Then
                Dim textArray2 As String() = New String() {"Length"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GEZero1", textArray2))
            End If
            If ((length = 0) OrElse (str Is Nothing)) Then
                Return ""
            End If
            Dim length As Integer = str.Length
            If (Start > length) Then
                Return ""
            End If
            If ((Start + length) > length) Then
                Return str.Substring((Start - 1))
            End If
            Return str.Substring((Start - 1), length)
        End Function

        Private Shared Function PRIMARYLANGID(lcid As Integer) As Integer
            Return (lcid And &H3FF)
        End Function

        Private Shared Function ProperCaseString(loc As CultureInfo, dwMapFlags As Integer, sSrc As String) As String
            Dim length As Integer
            If (sSrc Is Nothing) Then
                length = 0
            Else
                length = sSrc.Length
            End If
            If (length = 0) Then
                Return ""
            End If
            Dim builder As New StringBuilder(Strings.vbLCMapString(loc, (dwMapFlags Or &H100), sSrc))
            Return loc.TextInfo.ToTitleCase(builder.ToString)
        End Function

        Public Shared Function Replace(Expression As String, Find As String, Replacement As String, Optional Start As Integer = 1, Optional Count As Integer = -1, <OptionCompare> Optional [Compare] As CompareMethod = 0) As String
            Dim str As String
            Try
                If (Count < -1) Then
                    Dim args As String() = New String() {"Count"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_GEMinusOne1", args))
                End If
                If (Start <= 0) Then
                    Dim textArray2 As String() = New String() {"Start"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_GTZero1", textArray2))
                End If
                If ((Expression Is Nothing) OrElse (Start > Expression.Length)) Then
                    Return Nothing
                End If
                If (Start <> 1) Then
                    Expression = Expression.Substring((Start - 1))
                End If
                If (((Find Is Nothing) OrElse (Find.Length = 0)) OrElse (Count = 0)) Then
                    Return Expression
                End If
                If (Count = -1) Then
                    Count = Expression.Length
                End If
                str = Strings.ReplaceInternal(Expression, Find, Replacement, Count, [Compare])
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Private Shared Function ReplaceInternal(Expression As String, Find As String, Replacement As String, Count As Integer, [Compare] As CompareMethod) As String
            Dim num3 As Integer
            Dim compareInfo As CompareInfo
            Dim ordinal As CompareOptions
            Dim length As Integer = Expression.Length
            Dim num2 As Integer = Find.Length
            Dim builder As New StringBuilder(length)
            If ([Compare] = CompareMethod.Text) Then
                compareInfo = Utils.GetCultureInfo.CompareInfo
                ordinal = (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase))
            Else
                compareInfo = Strings.m_InvariantCompareInfo
                ordinal = CompareOptions.Ordinal
            End If
            Do While (num3 < length)
                Dim num5 As Integer
                If (num5 = Count) Then
                    builder.Append(Expression.Substring(num3))
                    Exit Do
                End If
                Dim num4 As Integer = compareInfo.IndexOf(Expression, Find, num3, ordinal)
                If (num4 < 0) Then
                    builder.Append(Expression.Substring(num3))
                    Exit Do
                End If
                builder.Append(Expression.Substring(num3, (num4 - num3)))
                builder.Append(Replacement)
                num5 += 1
                num3 = (num4 + num2)
            Loop
            Return builder.ToString
        End Function

        Public Shared Function Right(str As String, Length As Integer) As String
            If (length < 0) Then
                Dim args As String() = New String() {"Length"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GEZero1", args))
            End If
            If ((length = 0) OrElse (str Is Nothing)) Then
                Return ""
            End If
            Dim length As Integer = str.Length
            If (length >= length) Then
                Return str
            End If
            Return str.Substring((length - length), length)
        End Function

        Public Shared Function RSet(Source As String, Length As Integer) As String
            If (Length = 0) Then
                Return ""
            End If
            If (Source Is Nothing) Then
                Return New String(" "c, Length)
            End If
            If (Length > Source.Length) Then
                Return Source.PadLeft(Length)
            End If
            Return Source.Substring(0, Length)
        End Function

        Public Shared Function RTrim(str As String) As String
            Dim str2 As String
            Try
                If ((str Is Nothing) OrElse (str.Length = 0)) Then
                    Return ""
                End If
                Select Case str.Chars((str.Length - 1))
                    Case " "c, ChrW(12288)
                        Return str.TrimEnd(Utils.m_achIntlSpace)
                End Select
                str2 = str
            Catch exception As Exception
                Throw exception
            End Try
            Return str2
        End Function

        Public Shared Function Space(Number As Integer) As String
            If (Number >= 0) Then
                Return New String(" "c, Number)
            End If
            Dim args As String() = New String() {"Number"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_GEZero1", args))
        End Function

        Public Shared Function Split(Expression As String, Optional Delimiter As String = " ", Optional Limit As Integer = -1, <OptionCompare> Optional [Compare] As CompareMethod = 0) As String()
            Dim strArray As String()
            Try
                Dim length As Integer
                If ((Expression Is Nothing) OrElse (Expression.Length = 0)) Then
                    Return New String() {""}
                End If
                If (Limit = -1) Then
                    Limit = (Expression.Length + 1)
                End If
                If (Delimiter Is Nothing) Then
                    length = 0
                Else
                    length = Delimiter.Length
                End If
                If (length = 0) Then
                    Return New String() {Expression}
                End If
                strArray = Strings.SplitHelper(Expression, Delimiter, Limit, CInt([Compare]))
            Catch exception As Exception
                Throw exception
            End Try
            Return strArray
        End Function

        Private Shared Function SplitHelper(sSrc As String, sFind As String, cMaxSubStrings As Integer, [Compare] As Integer) As String()
            Dim num As Integer
            Dim length As Integer
            Dim num4 As Integer
            Dim num5 As Integer
            Dim invariantCompareInfo As CompareInfo
            Dim ordinal As CompareOptions
            If (sFind Is Nothing) Then
                length = 0
            Else
                length = sFind.Length
            End If
            If (sSrc Is Nothing) Then
                num4 = 0
            Else
                num4 = sSrc.Length
            End If
            If (length = 0) Then
                Return New String() {sSrc}
            End If
            If (num4 = 0) Then
                Return New String() {sSrc}
            End If
            Dim num6 As Integer = 20
            If (num6 > cMaxSubStrings) Then
                num6 = cMaxSubStrings
            End If
            Dim arySrc As String() = New String((num6 + 1) - 1) {}
            If ([Compare] = 0) Then
                ordinal = CompareOptions.Ordinal
                invariantCompareInfo = Strings.m_InvariantCompareInfo
            Else
                invariantCompareInfo = Utils.GetCultureInfo.CompareInfo
                ordinal = (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase))
            End If
            Do While (num5 < num4)
                Dim str As String
                Dim num2 As Integer = invariantCompareInfo.IndexOf(sSrc, sFind, num5, (num4 - num5), ordinal)
                If ((num2 = -1) OrElse ((num + 1) = cMaxSubStrings)) Then
                    str = sSrc.Substring(num5)
                    If (str Is Nothing) Then
                        str = ""
                    End If
                    arySrc(num) = str
                    Exit Do
                End If
                str = sSrc.Substring(num5, (num2 - num5))
                If (str Is Nothing) Then
                    str = ""
                End If
                arySrc(num) = str
                num5 = (num2 + length)
                num += 1
                If (num > num6) Then
                    num6 = (num6 + 20)
                    If (num6 > cMaxSubStrings) Then
                        num6 = (cMaxSubStrings + 1)
                    End If
                    arySrc = DirectCast(Utils.CopyArray(arySrc, New String((num6 + 1) - 1) {}), String())
                End If
                arySrc(num) = ""
                If (num = cMaxSubStrings) Then
                    str = sSrc.Substring(num5)
                    If (str Is Nothing) Then
                        str = ""
                    End If
                    arySrc(num) = str
                    Exit Do
                End If
            Loop
            If ((num + 1) = arySrc.Length) Then
                Return arySrc
            End If
            Return DirectCast(Utils.CopyArray(arySrc, New String((num + 1) - 1) {}), String())
        End Function

        Public Shared Function StrComp(String1 As String, String2 As String, <OptionCompare> Optional [Compare] As CompareMethod = 0) As Integer
            Dim num As Integer
            Try
                If ([Compare] = CompareMethod.Binary) Then
                    Return Operators.CompareString(String1, String2, False)
                End If
                If ([Compare] = CompareMethod.Text) Then
                    num = Operators.CompareString(String1, String2, True)
                Else
                    Dim args As String() = New String() {"Compare"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                End If
            Catch exception As Exception
                Throw exception
            End Try
            Return num
        End Function

        Public Shared Function StrConv(str As String, Conversion As VbStrConv, Optional LocaleID As Integer = 0) As String
            Dim str2 As String
            Try
                Dim num As Integer
                Dim cultureInfo As CultureInfo
                Dim conv3 As VbStrConv
                If ((LocaleID = 0) OrElse (LocaleID = 1)) Then
                    cultureInfo = Utils.GetCultureInfo
                    LocaleID = cultureInfo.LCID
                Else
                    Try
                        cultureInfo = New CultureInfo((LocaleID And &HFFFF))
                    Catch exception As StackOverflowException
                        Throw exception
                    Catch exception2 As OutOfMemoryException
                        Throw exception2
                    Catch exception3 As ThreadAbortException
                        Throw exception3
                    Catch exception7 As Exception
                        Dim args As String() = New String() {Conversions.ToString(LocaleID)}
                        Throw New ArgumentException(Utils.GetResourceString("Argument_LCIDNotSupported1", args))
                    End Try
                End If
                Dim num2 As Integer = Strings.PRIMARYLANGID(LocaleID)
                If ((Conversion And Not (VbStrConv.LinguisticCasing Or (VbStrConv.TraditionalChinese Or (VbStrConv.SimplifiedChinese Or (VbStrConv.Hiragana Or (VbStrConv.Katakana Or (VbStrConv.Narrow Or (VbStrConv.Wide Or VbStrConv.ProperCase)))))))) <> VbStrConv.None) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidVbStrConv"))
                End If
                Dim num3 As Integer = (CInt(Conversion) And &H300)
                If (num3 <= &H100) Then
                    If ((num3 <> 0) AndAlso (num3 = &H100)) Then
                        GoTo Label_00BF
                    End If
                    GoTo Label_0123
                End If
                If (num3 = &H200) Then
                    GoTo Label_00F1
                End If
                If (num3 <> &H300) Then
                    GoTo Label_0123
                End If
                Throw New ArgumentException(Utils.GetResourceString("Argument_StrConvSCandTC"))
Label_00BF:
                If (Strings.IsValidCodePage(&H3A8) AndAlso Strings.IsValidCodePage(950)) Then
                    num = (num Or &H2000000)
                    GoTo Label_0123
                End If
                Throw New ArgumentException(Utils.GetResourceString("Argument_SCNotSupported"))
Label_00F1:
                If (Strings.IsValidCodePage(&H3A8) AndAlso Strings.IsValidCodePage(950)) Then
                    num = (num Or &H4000000)
                Else
                    Throw New ArgumentException(Utils.GetResourceString("Argument_TCNotSupported"))
                End If
Label_0123:
                Select Case (Conversion And VbStrConv.ProperCase)
                    Case VbStrConv.None
                        If ((Conversion And VbStrConv.LinguisticCasing) <> VbStrConv.None) Then
                            Throw New ArgumentException(Utils.GetResourceString("LinguisticRequirements"))
                        End If
                        GoTo Label_019C
                    Case VbStrConv.Uppercase
                        If (Conversion <> VbStrConv.Uppercase) Then
                            Exit Select
                        End If
                        Return cultureInfo.TextInfo.ToUpper(str)
                    Case VbStrConv.Lowercase
                        If (Conversion <> VbStrConv.Lowercase) Then
                            GoTo Label_0194
                        End If
                        Return cultureInfo.TextInfo.ToLower(str)
                    Case VbStrConv.ProperCase
                        num = 0
                        GoTo Label_019C
                    Case Else
                        GoTo Label_019C
                End Select
                num = (num Or &H200)
                GoTo Label_019C
Label_0194:
                num = (num Or &H100)
Label_019C:
                If (((Conversion And (VbStrConv.Hiragana Or VbStrConv.Katakana)) <> VbStrConv.None) AndAlso ((num2 <> &H11) OrElse Not Strings.ValidLCID(LocaleID))) Then
                    Throw New ArgumentException(Utils.GetResourceString("Argument_JPNNotSupported"))
                End If
                If ((Conversion And (VbStrConv.Narrow Or VbStrConv.Wide)) <> VbStrConv.None) Then
                    If (((num2 <> &H11) AndAlso (num2 <> &H12)) AndAlso (num2 <> 4)) Then
                        Throw New ArgumentException(Utils.GetResourceString("Argument_WideNarrowNotApplicable"))
                    End If
                    If Not Strings.ValidLCID(LocaleID) Then
                        Throw New ArgumentException(Utils.GetResourceString("Argument_LocalNotSupported"))
                    End If
                End If
                Dim conv2 As VbStrConv = (Conversion And (VbStrConv.Narrow Or VbStrConv.Wide))
                If (conv2 <= VbStrConv.Wide) Then
                    If ((conv2 <> VbStrConv.None) AndAlso (conv2 = VbStrConv.Wide)) Then
                        GoTo Label_022C
                    End If
                    GoTo Label_023E
                End If
                If (conv2 = VbStrConv.Narrow) Then
                    GoTo Label_0236
                End If
                If (conv2 <> (VbStrConv.Narrow Or VbStrConv.Wide)) Then
                    GoTo Label_023E
                End If
                Throw New ArgumentException(Utils.GetResourceString("Argument_IllegalWideNarrow"))
Label_022C:
                num = (num Or &H800000)
                GoTo Label_023E
Label_0236:
                num = (num Or &H400000)
Label_023E:
                conv3 = (Conversion And (VbStrConv.Hiragana Or VbStrConv.Katakana))
                If (conv3 <= VbStrConv.Katakana) Then
                    If ((conv3 <> VbStrConv.None) AndAlso (conv3 = VbStrConv.Katakana)) Then
                        GoTo Label_0272
                    End If
                    GoTo Label_0284
                End If
                If (conv3 = VbStrConv.Hiragana) Then
                    GoTo Label_027C
                End If
                If (conv3 <> (VbStrConv.Hiragana Or VbStrConv.Katakana)) Then
                    GoTo Label_0284
                End If
                Throw New ArgumentException(Utils.GetResourceString("Argument_IllegalKataHira"))
Label_0272:
                num = (num Or &H200000)
                GoTo Label_0284
Label_027C:
                num = (num Or &H100000)
Label_0284:
                If ((Conversion And VbStrConv.ProperCase) = VbStrConv.ProperCase) Then
                    Return Strings.ProperCaseString(cultureInfo, num, str)
                End If
                If (num <> 0) Then
                    Return Strings.vbLCMapString(cultureInfo, num, str)
                End If
                str2 = str
            Catch exception4 As Exception
                Throw exception4
            End Try
            Return str2
        End Function

        Public Shared Function StrDup(Number As Integer, Character As Char) As String
            If (Number < 0) Then
                Dim args As String() = New String() {"Number"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GEZero1", args))
            End If
            Return New String(Character, Number)
        End Function

        Public Shared Function StrDup(Number As Integer, Character As Object) As Object
            Dim ch As Char
            If (Number < 0) Then
                Dim args As String() = New String() {"Number"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (Character Is Nothing) Then
                Dim textArray2 As String() = New String() {"Character"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray2))
            End If
            Dim str As String = TryCast(Character, String)
            If (Not str Is Nothing) Then
                If (str.Length = 0) Then
                    Dim textArray3 As String() = New String() {"Character"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", textArray3))
                End If
                ch = str.Chars(0)
            Else
                Try
                    ch = Conversions.ToChar(Character)
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception6 As Exception
                    Dim textArray4 As String() = New String() {"Character"}
                    Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray4))
                End Try
            End If
            Return New String(ch, Number)
        End Function

        Public Shared Function StrDup(Number As Integer, Character As String) As String
            If (Number < 0) Then
                Dim textArray1 As String() = New String() {"Number"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_GEZero1", textArray1))
            End If
            If ((Not Character Is Nothing) AndAlso (Character.Length <> 0)) Then
                Return New String(Character.Chars(0), Number)
            End If
            Dim args As String() = New String() {"Character"}
            Throw New ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", args))
        End Function

        Public Shared Function StrReverse(Expression As String) As String
            If (Expression Is Nothing) Then
                Return ""
            End If
            Dim length As Integer = Expression.Length
            If (length = 0) Then
                Return ""
            End If
            Dim num3 As Integer = (length - 1)
            Dim i As Integer = 0
            Do While (i <= num3)
                Select Case Char.GetUnicodeCategory(Expression.Chars(i))
                    Case UnicodeCategory.Surrogate, UnicodeCategory.NonSpacingMark, UnicodeCategory.SpacingCombiningMark, UnicodeCategory.EnclosingMark
                        Return Strings.InternalStrReverse(Expression, i, length)
                End Select
                i += 1
            Loop
            Dim array As Char() = Expression.ToCharArray
            array.Reverse(array)
            Return New String(array)
        End Function

        Public Shared Function Trim(str As String) As String
            Dim str2 As String
            Try
                If ((str Is Nothing) OrElse (str.Length = 0)) Then
                    Return ""
                End If
                Select Case str.Chars(0)
                    Case " "c, ChrW(12288)
                        Return str.Trim(Utils.m_achIntlSpace)
                End Select
                Select Case str.Chars((str.Length - 1))
                    Case " "c, ChrW(12288)
                        Return str.Trim(Utils.m_achIntlSpace)
                End Select
                str2 = str
            Catch exception As Exception
                Throw exception
            End Try
            Return str2
        End Function

        Public Shared Function UCase(Value As Char) As Char
            Dim ch As Char
            Try
                ch = Thread.CurrentThread.CurrentCulture.TextInfo.ToUpper(Value)
            Catch exception As Exception
                Throw exception
            End Try
            Return ch
        End Function

        Public Shared Function UCase(Value As String) As String
            Dim str As String
            Try
                If (Value Is Nothing) Then
                    Return ""
                End If
                str = Thread.CurrentThread.CurrentCulture.TextInfo.ToUpper(Value)
            Catch exception As Exception
                Throw exception
            End Try
            Return str
        End Function

        Private Shared Sub ValidateTriState(Param As TriState)
            If (((Param <> TriState.True) AndAlso (Param <> TriState.False)) AndAlso (Param <> TriState.UseDefault)) Then
                Throw ExceptionUtils.VbMakeException(5)
            End If
        End Sub

        Friend Shared Function ValidLCID(LocaleID As Integer) As Boolean
            Dim flag As Boolean
            Try
                Call New CultureInfo(LocaleID)
                flag = True
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                flag = False
            End Try
            Return flag
        End Function

        <SecuritySafeCritical>
        Friend Shared Function vbLCMapString(loc As CultureInfo, dwMapFlags As Integer, sSrc As String) As String
            Dim length As Integer
            Dim num2 As Integer
            If (sSrc Is Nothing) Then
                length = 0
            Else
                length = sSrc.Length
            End If
            If (length = 0) Then
                Return ""
            End If
            Dim lCID As Integer = loc.LCID
            Dim encoding As Encoding = Encoding.GetEncoding(loc.TextInfo.ANSICodePage)
            If Not encoding.IsSingleByte Then
                Dim s As String = sSrc
                Dim bytes As Byte() = encoding.GetBytes(s)
                num2 = UnsafeNativeMethods.LCMapStringA(lCID, dwMapFlags, bytes, bytes.Length, Nothing, 0)
                Dim buffer2 As Byte() = New Byte(((num2 - 1) + 1) - 1) {}
                num2 = UnsafeNativeMethods.LCMapStringA(lCID, dwMapFlags, bytes, bytes.Length, buffer2, num2)
                Return encoding.GetString(buffer2)
            End If
            Dim lpDestStr As New String(" "c, length)
            num2 = UnsafeNativeMethods.LCMapString(lCID, dwMapFlags, sSrc, length, lpDestStr, length)
            Return lpDestStr
        End Function


        ' Properties
        Private Shared ReadOnly Property CachedOnOffFormatStyle As String
            Get
                Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
                Dim syncObject As Object = Strings.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    If (Not Strings.m_LastUsedOnOffCulture Is cultureInfo) Then
                        Strings.m_LastUsedOnOffCulture = cultureInfo
                        Strings.m_CachedOnOffFormatStyle = Utils.GetResourceString("OnOffFormatStyle")
                    End If
                    Return Strings.m_CachedOnOffFormatStyle
                End SyncLock
            End Get
        End Property

        Private Shared ReadOnly Property CachedTrueFalseFormatStyle As String
            Get
                Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
                Dim syncObject As Object = Strings.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    If (Not Strings.m_LastUsedTrueFalseCulture Is cultureInfo) Then
                        Strings.m_LastUsedTrueFalseCulture = cultureInfo
                        Strings.m_CachedTrueFalseFormatStyle = Utils.GetResourceString("TrueFalseFormatStyle")
                    End If
                    Return Strings.m_CachedTrueFalseFormatStyle
                End SyncLock
            End Get
        End Property

        Private Shared ReadOnly Property CachedYesNoFormatStyle As String
            Get
                Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
                Dim syncObject As Object = Strings.m_SyncObject
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject)
                SyncLock syncObject
                    If (Not Strings.m_LastUsedYesNoCulture Is cultureInfo) Then
                        Strings.m_LastUsedYesNoCulture = cultureInfo
                        Strings.m_CachedYesNoFormatStyle = Utils.GetResourceString("YesNoFormatStyle")
                    End If
                    Return Strings.m_CachedYesNoFormatStyle
                End SyncLock
            End Get
        End Property


        ' Fields
        Private Const CODEPAGE_SIMPLIFIED_CHINESE As Integer = &H3A8
        Private Const CODEPAGE_TRADITIONAL_CHINESE As Integer = 950
        Private Shared ReadOnly CurrencyNegativeFormatStrings As String() = New String() { "('$'n)", "-'$'n", "'$'-n", "'$'n-", "(n'$')", "-n'$'", "n-'$'", "n'$'-", "-n '$'", "-'$' n", "n '$'-", "'$' n-", "'$'- n", "n- '$'", "('$' n)", "(n '$')" }
        Private Shared ReadOnly CurrencyPositiveFormatStrings As String() = New String() { "'$'n", "n'$'", "'$' n", "n '$'" }
        Private Const InvariantCultureID As Integer = &H7F
        Private Shared m_CachedOnOffFormatStyle As String
        Private Shared m_CachedTrueFalseFormatStyle As String
        Private Shared m_CachedYesNoFormatStyle As String
        Friend Shared ReadOnly m_InvariantCompareInfo As CompareInfo = CultureInfo.InvariantCulture.CompareInfo
        Private Shared m_LastUsedOnOffCulture As CultureInfo
        Private Shared m_LastUsedTrueFalseCulture As CultureInfo
        Private Shared m_LastUsedYesNoCulture As CultureInfo
        Private Shared m_SyncObject As Object = New Object
        Private Const NAMEDFORMAT_CURRENCY As String = "currency"
        Private Const NAMEDFORMAT_FIXED As String = "fixed"
        Private Const NAMEDFORMAT_GENERAL_DATE As String = "general date"
        Private Const NAMEDFORMAT_GENERAL_NUMBER As String = "general number"
        Private Const NAMEDFORMAT_LONG_DATE As String = "long date"
        Private Const NAMEDFORMAT_LONG_TIME As String = "long time"
        Private Const NAMEDFORMAT_MEDIUM_DATE As String = "medium date"
        Private Const NAMEDFORMAT_MEDIUM_TIME As String = "medium time"
        Private Const NAMEDFORMAT_ON_OFF As String = "on/off"
        Private Const NAMEDFORMAT_PERCENT As String = "percent"
        Private Const NAMEDFORMAT_SCIENTIFIC As String = "scientific"
        Private Const NAMEDFORMAT_SHORT_DATE As String = "short date"
        Private Const NAMEDFORMAT_SHORT_TIME As String = "short time"
        Private Const NAMEDFORMAT_STANDARD As String = "standard"
        Private Const NAMEDFORMAT_TRUE_FALSE As String = "true/false"
        Private Const NAMEDFORMAT_YES_NO As String = "yes/no"
        Private Shared ReadOnly NumberNegativeFormatStrings As String() = New String() { "(n)", "-n", "- n", "n-", "n -" }
        Private Const STANDARD_COMPARE_FLAGS As CompareOptions = (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase))

        ' Nested Types
        Friend Enum FormatType
            ' Fields
            Currency = 2
            Number = 0
            Percent = 1
        End Enum

        Private Enum NamedFormats
            ' Fields
            CURRENCY = 13
            FIXED = 9
            GENERAL_DATE = 5
            GENERAL_NUMBER = 1
            LONG_DATE = 6
            LONG_TIME = 2
            MEDIUM_DATE = 7
            MEDIUM_TIME = 3
            ON_OFF = &H10
            PERCENT = 11
            SCIENTIFIC = 12
            SHORT_DATE = 8
            SHORT_TIME = 4
            STANDARD = 10
            TRUE_FALSE = 14
            UNKNOWN = 0
            YES_NO = 15
        End Enum
    End Class
End Namespace

