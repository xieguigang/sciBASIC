Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Globalization
Imports System.Text
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class StringType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function AsteriskSkip(Pattern As String, Source As String, SourceEndIndex As Integer, CompareOption As CompareMethod, ci As CompareInfo) As Integer
            Dim num2 As Integer
            Dim num4 As Integer
            Dim num3 As Integer = Strings.Len(Pattern)
            Do While (num4 < num3)
                Dim flag As Boolean
                Dim flag2 As Boolean
                Dim flag3 As Boolean
                Select Case Pattern.Chars(num4)
                    Case "!"c
                        If (Pattern.Chars((num4 + 1)) = "]"c) Then
                            flag = True
                        Else
                            flag2 = True
                        End If
                        Exit Select
                    Case "#"c, "?"c
                        If flag3 Then
                            flag = True
                        Else
                            num2 += 1
                            flag2 = True
                        End If
                        Exit Select
                    Case "*"c
                        Dim ordinal As CompareOptions
                        If (num2 <= 0) Then
                            Exit Select
                        End If
                        If flag2 Then
                            num2 = StringType.MultipleAsteriskSkip(Pattern, Source, num2, CompareOption)
                            Return (SourceEndIndex - num2)
                        End If
                        Dim str As String = Pattern.Substring(0, num4)
                        If (CompareOption = CompareMethod.Binary) Then
                            ordinal = CompareOptions.Ordinal
                        Else
                            ordinal = (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or (CompareOptions.IgnoreNonSpace Or CompareOptions.IgnoreCase)))
                        End If
                        Return ci.LastIndexOf(Source, str, ordinal)
                    Case "["c
                        If flag3 Then
                            flag = True
                        Else
                            flag3 = True
                        End If
                        Exit Select
                    Case "]"c
                        If (flag OrElse Not flag3) Then
                            num2 += 1
                            flag2 = True
                        End If
                        flag = False
                        flag3 = False
                        Exit Select
                    Case "-"c
                        If (Pattern.Chars((num4 + 1)) = "]"c) Then
                            flag = True
                        End If
                        Exit Select
                    Case Else
                        If flag3 Then
                            flag = True
                        Else
                            num2 += 1
                        End If
                        Exit Select
                End Select
                num4 += 1
            Loop
            Return (SourceEndIndex - num2)
        End Function

        Public Shared Function FromBoolean(Value As Boolean) As String
            If Value Then
                Return Boolean.TrueString
            End If
            Return Boolean.FalseString
        End Function

        Public Shared Function FromByte(Value As Byte) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        Public Shared Function FromChar(Value As Char) As String
            Return Value.ToString
        End Function

        Public Shared Function FromDate(Value As DateTime) As String
            Dim ticks As Long = Value.TimeOfDay.Ticks
            If ((ticks = Value.Ticks) OrElse (((Value.Year = &H76B) AndAlso (Value.Month = 12)) AndAlso (Value.Day = 30))) Then
                Return Value.ToString("T", Nothing)
            End If
            If (ticks = 0) Then
                Return Value.ToString("d", Nothing)
            End If
            Return Value.ToString("G", Nothing)
        End Function

        Public Shared Function FromDecimal(Value As Decimal) As String
            Return StringType.FromDecimal(Value, Nothing)
        End Function

        Public Shared Function FromDecimal(Value As Decimal, NumberFormat As NumberFormatInfo) As String
            Return Value.ToString("G", NumberFormat)
        End Function

        Public Shared Function FromDouble(Value As Double) As String
            Return StringType.FromDouble(Value, Nothing)
        End Function

        Public Shared Function FromDouble(Value As Double, NumberFormat As NumberFormatInfo) As String
            Return Value.ToString("G", NumberFormat)
        End Function

        Public Shared Function FromInteger(Value As Integer) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        Public Shared Function FromLong(Value As Long) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        Public Shared Function FromObject(Value As Object) As String
            If (Value Is Nothing) Then
                Return Nothing
            End If
            Dim str2 As String = TryCast(Value, String)
            If (Not str2 Is Nothing) Then
                Return str2
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return StringType.FromBoolean(convertible.ToBoolean(Nothing))
                    Case TypeCode.Char
                        Return StringType.FromChar(convertible.ToChar(Nothing))
                    Case TypeCode.Byte
                        Return StringType.FromByte(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        Return StringType.FromShort(convertible.ToInt16(Nothing))
                    Case TypeCode.Int32
                        Return StringType.FromInteger(convertible.ToInt32(Nothing))
                    Case TypeCode.Int64
                        Return StringType.FromLong(convertible.ToInt64(Nothing))
                    Case TypeCode.Single
                        Return StringType.FromSingle(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        Return StringType.FromDouble(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return StringType.FromDecimal(convertible.ToDecimal(Nothing))
                    Case TypeCode.DateTime
                        Return StringType.FromDate(convertible.ToDateTime(Nothing))
                    Case TypeCode.String
                        Return convertible.ToString(Nothing)
                End Select
            Else
                Dim chArray As Char() = TryCast(Value, Char())
                If ((Not chArray Is Nothing) AndAlso (chArray.Rank = 1)) Then
                    Return New String(CharArrayType.FromObject(Value))
                End If
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "String"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromShort(Value As Short) As String
            Return Value.ToString(Nothing, DirectCast(Nothing, IFormatProvider))
        End Function

        Public Shared Function FromSingle(Value As Single) As String
            Return StringType.FromSingle(Value, Nothing)
        End Function

        Public Shared Function FromSingle(Value As Single, NumberFormat As NumberFormatInfo) As String
            Return Value.ToString(Nothing, NumberFormat)
        End Function

        Public Shared Sub MidStmtStr(ByRef sDest As String, StartPosition As Integer, MaxInsertLength As Integer, sInsert As String)
            Dim length As Integer
            Dim num2 As Integer
            If (Not sDest Is Nothing) Then
                length = sDest.Length
            End If
            If (Not sInsert Is Nothing) Then
                num2 = sInsert.Length
            End If
            StartPosition -= 1
            If ((StartPosition < 0) OrElse (StartPosition >= length)) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            If (MaxInsertLength < 0) Then
                Dim textArray2 As String() = New String() {"Length"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
            End If
            If (num2 > MaxInsertLength) Then
                num2 = MaxInsertLength
            End If
            If (num2 > (length - StartPosition)) Then
                num2 = (length - StartPosition)
            End If
            If (num2 <> 0) Then
                Dim builder As New StringBuilder(length)
                If (StartPosition > 0) Then
                    builder.Append(sDest, 0, StartPosition)
                End If
                builder.Append(sInsert, 0, num2)
                Dim count As Integer = (length - (StartPosition + num2))
                If (count > 0) Then
                    builder.Append(sDest, (StartPosition + num2), count)
                End If
                sDest = builder.ToString
            End If
        End Sub

        Private Shared Function MultipleAsteriskSkip(Pattern As String, Source As String, Count As Integer, CompareOption As CompareMethod) As Integer
            Dim num As Integer = Strings.Len(Source)
            Do While (Count < num)
                Dim flag As Boolean
                Dim source As String = source.Substring((num - Count))
                Try
                    flag = StringType.StrLike(source, Pattern, CompareOption)
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception6 As Exception
                    flag = False
                End Try
                If flag Then
                    Return Count
                End If
                Count += 1
            Loop
            Return Count
        End Function

        Public Shared Function StrCmp(sLeft As String, sRight As String, TextCompare As Boolean) As Integer
            If (sLeft Is sRight) Then
                Return 0
            End If
            If (sLeft Is Nothing) Then
                If (sRight.Length = 0) Then
                    Return 0
                End If
                Return -1
            End If
            If (sRight Is Nothing) Then
                If (sLeft.Length = 0) Then
                    Return 0
                End If
                Return 1
            End If
            If TextCompare Then
                Return Utils.GetCultureInfo.CompareInfo.Compare(sLeft, sRight, (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase)))
            End If
            Return String.CompareOrdinal(sLeft, sRight)
        End Function

        Public Shared Function StrLike(Source As String, Pattern As String, CompareOption As CompareMethod) As Boolean
            If (CompareOption = CompareMethod.Binary) Then
                Return StringType.StrLikeBinary(Source, Pattern)
            End If
            Return StringType.StrLikeText(Source, Pattern)
        End Function

        Public Shared Function StrLikeBinary(Source As String, Pattern As String) As Boolean
            Dim num As Integer
            Dim num2 As Integer
            Dim num3 As Integer
            Dim length As Integer
            Dim ch2 As Char
            Dim flag2 As Boolean
            Dim flag7 As Boolean = False
            If (Pattern Is Nothing) Then
                length = 0
            Else
                length = Pattern.Length
            End If
            If (Source Is Nothing) Then
                num3 = 0
            Else
                num3 = Source.Length
            End If
            If (num < num3) Then
                ch2 = Source.Chars(num)
            End If
            Do While (num2 < length)
                Dim p As Char = Pattern.Chars(num2)
                If ((p = "*"c) AndAlso Not flag2) Then
                    Dim num5 As Integer = StringType.AsteriskSkip(Pattern.Substring((num2 + 1)), Source.Substring(num), (num3 - num), CompareMethod.Binary, Strings.m_InvariantCompareInfo)
                    If (num5 < 0) Then
                        Return False
                    End If
                    If (num5 > 0) Then
                        num = (num + num5)
                        If (num < num3) Then
                            ch2 = Source.Chars(num)
                        End If
                    End If
                ElseIf ((p = "?"c) AndAlso Not flag2) Then
                    num += 1
                    If (num < num3) Then
                        ch2 = Source.Chars(num)
                    End If
                ElseIf ((p = "#"c) AndAlso Not flag2) Then
                    If Not Char.IsDigit(ch2) Then
                        Exit Do
                    End If
                    num += 1
                    If (num < num3) Then
                        ch2 = Source.Chars(num)
                    End If
                Else
                    Dim flag3 As Boolean
                    Dim flag5 As Boolean
                    If ((((p = "-"c) AndAlso flag2) AndAlso (flag5 AndAlso Not flag7)) AndAlso (Not flag3 AndAlso (((num2 + 1) >= length) OrElse (Pattern.Chars((num2 + 1)) <> "]"c)))) Then
                        flag3 = True
                    Else
                        Dim flag4 As Boolean
                        Dim flag6 As Boolean
                        If (((p = "!"c) AndAlso flag2) AndAlso Not flag6) Then
                            flag6 = True
                            flag4 = True
                        Else
                            Dim ch3 As Char
                            Dim ch4 As Char
                            If ((p = "["c) AndAlso Not flag2) Then
                                flag2 = True
                                ch3 = ChrW(0)
                                ch4 = ChrW(0)
                                flag5 = False
                            ElseIf ((p = "]"c) AndAlso flag2) Then
                                flag2 = False
                                If flag5 Then
                                    If Not flag4 Then
                                        Exit Do
                                    End If
                                    num += 1
                                    If (num < num3) Then
                                        ch2 = Source.Chars(num)
                                    End If
                                ElseIf flag3 Then
                                    If Not flag4 Then
                                        Exit Do
                                    End If
                                ElseIf flag6 Then
                                    If ("!"c <> ch2) Then
                                        Exit Do
                                    End If
                                    num += 1
                                    If (num < num3) Then
                                        ch2 = Source.Chars(num)
                                    End If
                                End If
                                flag4 = False
                                flag5 = False
                                flag6 = False
                                flag3 = False
                            Else
                                flag5 = True
                                flag7 = False
                                If flag2 Then
                                    If flag3 Then
                                        flag3 = False
                                        flag7 = True
                                        ch4 = p
                                        If (ch3 > ch4) Then
                                            Throw ExceptionUtils.VbMakeException(&H5D)
                                        End If
                                        If ((flag6 AndAlso flag4) OrElse (Not flag6 AndAlso Not flag4)) Then
                                            flag4 = ((ch2 > ch3) AndAlso (ch2 <= ch4))
                                            If flag6 Then
                                                flag4 = Not flag4
                                            End If
                                        End If
                                    Else
                                        ch3 = p
                                        flag4 = StringType.StrLikeCompareBinary(flag6, flag4, p, ch2)
                                    End If
                                Else
                                    If ((p <> ch2) AndAlso Not flag6) Then
                                        Exit Do
                                    End If
                                    flag6 = False
                                    num += 1
                                    If (num < num3) Then
                                        ch2 = Source.Chars(num)
                                    ElseIf (num > num3) Then
                                        Return False
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                num2 += 1
            Loop
            If flag2 Then
                If (num3 = 0) Then
                    Return False
                End If
                Dim args As String() = New String() {"Pattern"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Return ((num2 = length) AndAlso (num = num3))
        End Function

        Private Shared Function StrLikeCompare(ci As CompareInfo, SeenNot As Boolean, Match As Boolean, p As Char, s As Char, Options As CompareOptions) As Boolean
            If (SeenNot AndAlso Match) Then
                If (Options = CompareOptions.Ordinal) Then
                    Return (p <> s)
                End If
                Return (ci.Compare(Conversions.ToString(p), Conversions.ToString(s), Options) > 0)
            End If
            If (Not SeenNot AndAlso Not Match) Then
                If (Options = CompareOptions.Ordinal) Then
                    Return (p = s)
                End If
                Return (ci.Compare(Conversions.ToString(p), Conversions.ToString(s), Options) = 0)
            End If
            Return Match
        End Function

        Private Shared Function StrLikeCompareBinary(SeenNot As Boolean, Match As Boolean, p As Char, s As Char) As Boolean
            If (SeenNot AndAlso Match) Then
                Return (p <> s)
            End If
            If (Not SeenNot AndAlso Not Match) Then
                Return (p = s)
            End If
            Return Match
        End Function

        Public Shared Function StrLikeText(Source As String, Pattern As String) As Boolean
            Dim num As Integer
            Dim num2 As Integer
            Dim num3 As Integer
            Dim length As Integer
            Dim ch2 As Char
            Dim flag2 As Boolean
            Dim flag7 As Boolean = False
            If (Pattern Is Nothing) Then
                length = 0
            Else
                length = Pattern.Length
            End If
            If (Source Is Nothing) Then
                num3 = 0
            Else
                num3 = Source.Length
            End If
            If (num < num3) Then
                ch2 = Source.Chars(num)
            End If
            Dim compareInfo As CompareInfo = Utils.GetCultureInfo.CompareInfo
            Dim options As CompareOptions = (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or (CompareOptions.IgnoreNonSpace Or CompareOptions.IgnoreCase)))
            Do While (num2 < length)
                Dim p As Char = Pattern.Chars(num2)
                If ((p = "*"c) AndAlso Not flag2) Then
                    Dim num5 As Integer = StringType.AsteriskSkip(Pattern.Substring((num2 + 1)), Source.Substring(num), (num3 - num), CompareMethod.Text, compareInfo)
                    If (num5 < 0) Then
                        Return False
                    End If
                    If (num5 > 0) Then
                        num = (num + num5)
                        If (num < num3) Then
                            ch2 = Source.Chars(num)
                        End If
                    End If
                ElseIf ((p = "?"c) AndAlso Not flag2) Then
                    num += 1
                    If (num < num3) Then
                        ch2 = Source.Chars(num)
                    End If
                ElseIf ((p = "#"c) AndAlso Not flag2) Then
                    If Not Char.IsDigit(ch2) Then
                        Exit Do
                    End If
                    num += 1
                    If (num < num3) Then
                        ch2 = Source.Chars(num)
                    End If
                Else
                    Dim flag3 As Boolean
                    Dim flag5 As Boolean
                    If ((((p = "-"c) AndAlso flag2) AndAlso (flag5 AndAlso Not flag7)) AndAlso (Not flag3 AndAlso (((num2 + 1) >= length) OrElse (Pattern.Chars((num2 + 1)) <> "]"c)))) Then
                        flag3 = True
                    Else
                        Dim flag4 As Boolean
                        Dim flag6 As Boolean
                        If (((p = "!"c) AndAlso flag2) AndAlso Not flag6) Then
                            flag6 = True
                            flag4 = True
                        Else
                            Dim ch3 As Char
                            Dim ch4 As Char
                            If ((p = "["c) AndAlso Not flag2) Then
                                flag2 = True
                                ch3 = ChrW(0)
                                ch4 = ChrW(0)
                                flag5 = False
                            ElseIf ((p = "]"c) AndAlso flag2) Then
                                flag2 = False
                                If flag5 Then
                                    If Not flag4 Then
                                        Exit Do
                                    End If
                                    num += 1
                                    If (num < num3) Then
                                        ch2 = Source.Chars(num)
                                    End If
                                ElseIf flag3 Then
                                    If Not flag4 Then
                                        Exit Do
                                    End If
                                ElseIf flag6 Then
                                    If (compareInfo.Compare("!", Conversions.ToString(ch2)) <> 0) Then
                                        Exit Do
                                    End If
                                    num += 1
                                    If (num < num3) Then
                                        ch2 = Source.Chars(num)
                                    End If
                                End If
                                flag4 = False
                                flag5 = False
                                flag6 = False
                                flag3 = False
                            Else
                                flag5 = True
                                flag7 = False
                                If flag2 Then
                                    If flag3 Then
                                        flag3 = False
                                        flag7 = True
                                        ch4 = p
                                        If (ch3 > ch4) Then
                                            Throw ExceptionUtils.VbMakeException(&H5D)
                                        End If
                                        If ((flag6 AndAlso flag4) OrElse (Not flag6 AndAlso Not flag4)) Then
                                            If (options = CompareOptions.Ordinal) Then
                                                flag4 = ((ch2 > ch3) AndAlso (ch2 <= ch4))
                                            Else
                                                flag4 = ((compareInfo.Compare(Conversions.ToString(ch3), Conversions.ToString(ch2), options) < 0) AndAlso (compareInfo.Compare(Conversions.ToString(ch4), Conversions.ToString(ch2), options) >= 0))
                                            End If
                                            If flag6 Then
                                                flag4 = Not flag4
                                            End If
                                        End If
                                    Else
                                        ch3 = p
                                        flag4 = StringType.StrLikeCompare(compareInfo, flag6, flag4, p, ch2, options)
                                    End If
                                Else
                                    If (options = CompareOptions.Ordinal) Then
                                        If ((p <> ch2) AndAlso Not flag6) Then
                                            Exit Do
                                        End If
                                    Else
                                        Dim str As String = Conversions.ToString(p)
                                        Dim str2 As String = Conversions.ToString(ch2)
                                        Do While (((num2 + 1) < length) AndAlso ((UnicodeCategory.ModifierSymbol = Char.GetUnicodeCategory(Pattern.Chars((num2 + 1)))) OrElse (UnicodeCategory.NonSpacingMark = Char.GetUnicodeCategory(Pattern.Chars((num2 + 1))))))
                                            str = (str & Conversions.ToString(Pattern.Chars((num2 + 1))))
                                            num2 += 1
                                        Loop
                                        Do While (((num + 1) < num3) AndAlso ((UnicodeCategory.ModifierSymbol = Char.GetUnicodeCategory(Source.Chars((num + 1)))) OrElse (UnicodeCategory.NonSpacingMark = Char.GetUnicodeCategory(Source.Chars((num + 1))))))
                                            str2 = (str2 & Conversions.ToString(Source.Chars((num + 1))))
                                            num += 1
                                        Loop
                                        If ((compareInfo.Compare(str, str2, (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase))) <> 0) AndAlso Not flag6) Then
                                            Exit Do
                                        End If
                                    End If
                                    flag6 = False
                                    num += 1
                                    If (num < num3) Then
                                        ch2 = Source.Chars(num)
                                    ElseIf (num > num3) Then
                                        Return False
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                num2 += 1
            Loop
            If flag2 Then
                If (num3 = 0) Then
                    Return False
                End If
                Dim args As String() = New String() {"Pattern"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
            End If
            Return ((num2 = length) AndAlso (num = num3))
        End Function


        ' Fields
        Private Const GENERAL_FORMAT As String = "G"
    End Class
End Namespace

