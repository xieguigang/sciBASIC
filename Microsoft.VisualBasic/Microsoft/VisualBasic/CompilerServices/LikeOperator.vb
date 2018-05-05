Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Text

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)>
    Public NotInheritable Class LikeOperator
        ' Methods
        Shared Sub New()
            LikeOperator.LigatureMap(&H19) = 1
            LikeOperator.LigatureMap(&H19) = 2
            LikeOperator.LigatureMap(0) = 3
            LikeOperator.LigatureMap(&H20) = 4
            LikeOperator.LigatureMap(&H18) = 5
            LikeOperator.LigatureMap(&H38) = 6
            LikeOperator.LigatureMap(140) = 7
            LikeOperator.LigatureMap(&H8D) = 8
        End Sub

        Private Sub New()
        End Sub

        Private Shared Sub BuildPatternGroups(Source As String, SourceLength As Integer, ByRef SourceIndex As Integer, SourceLigatureInfo As LigatureInfo(), Pattern As String, PatternLength As Integer, ByRef PatternIndex As Integer, PatternLigatureInfo As LigatureInfo(), ByRef PatternError As Boolean, ByRef PGIndexForLastAsterisk As Integer, Comparer As CompareInfo, Options As CompareOptions, ByRef PatternGroups As PatternGroup())
            PatternError = False
            PGIndexForLastAsterisk = 0
            PatternGroups = New PatternGroup(&H10 - 1) {}
            Dim num2 As Integer = 15
            Dim nONE As PatternType = PatternType.NONE
            Dim index As Integer = 0
            Do
                Dim numRef As Integer
                If (index >= num2) Then
                    Dim array As PatternGroup() = New PatternGroup(((num2 + &H10) + 1) - 1) {}
                    PatternGroups.CopyTo(array, 0)
                    PatternGroups = array
                    num2 = (num2 + &H10)
                End If
                Select Case Pattern.Chars(PatternIndex)
                    Case "?"c, &HFF1F
                        If (nONE = PatternType.ANYCHAR) Then
                            numRef = CInt(AddressOf PatternGroups((index - 1)).CharCount) = (numRef + 1)
                        Else
                            PatternGroups(index).PatType = PatternType.ANYCHAR
                            PatternGroups(index).CharCount = 1
                            index += 1
                            nONE = PatternType.ANYCHAR
                        End If
                        Exit Select
                    Case "["c, &HFF3B
                        Dim seenNot As Boolean = False
                        Dim rangeList As New List(Of Range)
                        If Not LikeOperator.ValidateRangePattern(Pattern, PatternLength, PatternIndex, PatternLigatureInfo, Comparer, Options, seenNot, rangeList) Then
                            PatternError = True
                            Return
                        End If
                        If (rangeList.Count <> 0) Then
                            If seenNot Then
                                nONE = PatternType.EXCLIST
                            Else
                                nONE = PatternType.INCLIST
                            End If
                            PatternGroups(index).PatType = nONE
                            PatternGroups(index).CharCount = 1
                            PatternGroups(index).RangeList = rangeList
                            index += 1
                        End If
                        Exit Select
                    Case "#"c, &HFF03
                        If (nONE = PatternType.DIGIT) Then
                            numRef = CInt(AddressOf PatternGroups((index - 1)).CharCount) = (numRef + 1)
                        Else
                            PatternGroups(index).PatType = PatternType.DIGIT
                            PatternGroups(index).CharCount = 1
                            index += 1
                            nONE = PatternType.DIGIT
                        End If
                        Exit Select
                    Case "*"c, &HFF0A
                        If (nONE <> PatternType.STAR) Then
                            nONE = PatternType.STAR
                            PatternGroups(index).PatType = PatternType.STAR
                            PGIndexForLastAsterisk = index
                            index += 1
                        End If
                        Exit Select
                    Case Else
                        Dim num4 As Integer = PatternIndex
                        Dim num5 As Integer = PatternIndex
                        If (num5 >= PatternLength) Then
                            num5 = (PatternLength - 1)
                        End If
                        If (nONE = PatternType.STRING) Then
                            numRef = CInt(AddressOf PatternGroups((index - 1)).CharCount) = (numRef + 1)
                            PatternGroups((index - 1)).StringPatternEnd = num5
                        Else
                            PatternGroups(index).PatType = PatternType.STRING
                            PatternGroups(index).CharCount = 1
                            PatternGroups(index).StringPatternStart = num4
                            PatternGroups(index).StringPatternEnd = num5
                            index += 1
                            nONE = PatternType.STRING
                        End If
                        Exit Select
                End Select
                PatternIndex += 1
            Loop While (PatternIndex < PatternLength)
            PatternGroups(index).PatType = PatternType.NONE
            PatternGroups(index).MinSourceIndex = SourceLength
            Dim num3 As Integer = SourceLength
            Do While (index > 0)
                Select Case PatternGroups(index).PatType
                    Case PatternType.STRING
                        num3 = (num3 - PatternGroups(index).CharCount)
                        Exit Select
                    Case PatternType.EXCLIST, PatternType.INCLIST
                        num3 -= 1
                        Exit Select
                    Case PatternType.DIGIT, PatternType.ANYCHAR
                        num3 = (num3 - PatternGroups(index).CharCount)
                        Exit Select
                End Select
                PatternGroups(index).MaxSourceIndex = num3
                index -= 1
            Loop
        End Sub

        Private Shared Function CanCharExpand(ch As Char, LocaleSpecificLigatureTable As Byte(), Comparer As CompareInfo, Options As CompareOptions) As Integer
            Dim num As Integer
            Dim index As Byte = LikeOperator.LigatureIndex(ch)
            If (index = 0) Then
                Return 0
            End If
            If (LocaleSpecificLigatureTable(index) = 0) Then
                If (Comparer.Compare(Conversions.ToString(ch), LikeOperator.LigatureExpansions(index)) = 0) Then
                    LocaleSpecificLigatureTable(index) = 1
                Else
                    LocaleSpecificLigatureTable(index) = 2
                End If
            End If
            If (LocaleSpecificLigatureTable(index) = 1) Then
                num = index
            End If
            Return num
        End Function

        Private Shared Function CompareChars(Left As Char, Right As Char, Comparer As CompareInfo, Options As CompareOptions) As Integer
            If (Options = CompareOptions.Ordinal) Then
                Return (Left - Right)
            End If
            Return Comparer.Compare(Conversions.ToString(Left), Conversions.ToString(Right), Options)
        End Function

        Private Shared Function CompareChars(Left As String, Right As String, Comparer As CompareInfo, Options As CompareOptions) As Integer
            If (Options = CompareOptions.Ordinal) Then
                Return (Left.Chars(0) - Right.Chars(0))
            End If
            Return Comparer.Compare(Left, Right, Options)
        End Function

        Private Shared Function CompareChars(Left As String, LeftLength As Integer, LeftStart As Integer, ByRef LeftEnd As Integer, LeftLigatureInfo As LigatureInfo(), Right As String, RightLength As Integer, RightStart As Integer, ByRef RightEnd As Integer, RightLigatureInfo As LigatureInfo(), Comparer As CompareInfo, Options As CompareOptions, Optional MatchBothCharsOfExpandedCharInRight As Boolean = False, Optional UseUnexpandedCharForRight As Boolean = False) As Integer
            LeftEnd = LeftStart
            RightEnd = RightStart
            If (Options = CompareOptions.Ordinal) Then
                Return (Left.Chars(LeftStart) - Right.Chars(RightStart))
            End If
            If UseUnexpandedCharForRight Then
                If ((Not RightLigatureInfo Is Nothing) AndAlso (RightLigatureInfo(RightEnd).Kind = CharKind.ExpandedChar1)) Then
                    Right = Right.Substring(RightStart, (RightEnd - RightStart))
                    Right = (Right & Conversions.ToString(RightLigatureInfo(RightEnd).CharBeforeExpansion))
                    RightEnd += 1
                    Return LikeOperator.CompareChars(Left.Substring(LeftStart, ((LeftEnd - LeftStart) + 1)), Right, Comparer, Options)
                End If
            ElseIf MatchBothCharsOfExpandedCharInRight Then
                LikeOperator.SkipToEndOfExpandedChar(RightLigatureInfo, RightLength, RightEnd)
                If (RightEnd < RightEnd) Then
                    Dim num2 As Integer = 0
                    If ((LeftEnd + 1) < LeftLength) Then
                        num2 = 1
                    End If
                    Dim num1 As Integer = LikeOperator.CompareChars(Left.Substring(LeftStart, (((LeftEnd - LeftStart) + 1) + num2)), Right.Substring(RightStart, ((RightEnd - RightStart) + 1)), Comparer, Options)
                    If (num1 = 0) Then
                        LeftEnd = (LeftEnd + num2)
                    End If
                    Return num1
                End If
            End If
            If ((LeftEnd Is LeftStart) AndAlso (RightEnd Is RightStart)) Then
                Return Comparer.Compare(Conversions.ToString(Left.Chars(LeftStart)), Conversions.ToString(Right.Chars(RightStart)), Options)
            End If
            Return LikeOperator.CompareChars(Left.Substring(LeftStart, ((LeftEnd - LeftStart) + 1)), Right.Substring(RightStart, ((RightEnd - RightStart) + 1)), Comparer, Options)
        End Function

        Private Shared Sub ExpandString(ByRef Input As String, ByRef Length As Integer, ByRef InputLigatureInfo As LigatureInfo(), LocaleSpecificLigatureTable As Byte(), Comparer As CompareInfo, Options As CompareOptions, ByRef WidthChanged As Boolean, UseFullWidth As Boolean)
            WidthChanged = False
            If (Not Length Is 0) Then
                Dim num2 As Integer
                Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
                Dim dwMapFlags As Integer = &H100
                Dim flag As Boolean = False
                If Not Encoding.GetEncoding(cultureInfo.TextInfo.ANSICodePage).IsSingleByte Then
                    dwMapFlags = &H400100
                    If Strings.IsValidCodePage(&H3A4) Then
                        If UseFullWidth Then
                            dwMapFlags = &HA00100
                        Else
                            dwMapFlags = &H600100
                        End If
                        Input = Strings.vbLCMapString(cultureInfo, dwMapFlags, Input)
                        flag = True
                        If (Not Input.Length = Length) Then
                            Length = Input.Length
                            WidthChanged = True
                        End If
                    End If
                End If
                If Not flag Then
                    Input = Strings.vbLCMapString(cultureInfo, dwMapFlags, Input)
                End If
                Dim num3 As Integer = (Length - 1)
                Dim i As Integer = 0
                Do While (i <= num3)
                    If (LikeOperator.CanCharExpand(Input.Chars(i), LocaleSpecificLigatureTable, Comparer, Options) <> 0) Then
                        num2 += 1
                    End If
                    i += 1
                Loop
                If (num2 > 0) Then
                    InputLigatureInfo = New LigatureInfo((((Length + num2) - 1) + 1) - 1) {}
                    Dim builder As New StringBuilder(((Length + num2) - 1))
                    Dim index As Integer = 0
                    Dim num6 As Integer = (Length - 1)
                    Dim j As Integer = 0
                    Do While (j <= num6)
                        Dim ch As Char = Input.Chars(j)
                        If (LikeOperator.CanCharExpand(ch, LocaleSpecificLigatureTable, Comparer, Options) <> 0) Then
                            Dim str As String = LikeOperator.GetCharExpansion(ch, LocaleSpecificLigatureTable, Comparer, Options)
                            builder.Append(str)
                            InputLigatureInfo(index).Kind = CharKind.ExpandedChar1
                            InputLigatureInfo(index).CharBeforeExpansion = ch
                            index += 1
                            InputLigatureInfo(index).Kind = CharKind.ExpandedChar2
                            InputLigatureInfo(index).CharBeforeExpansion = ch
                        Else
                            builder.Append(ch)
                        End If
                        index += 1
                        j += 1
                    Loop
                    Input = builder.ToString
                    Length = builder.Length
                End If
            End If
        End Sub

        Private Shared Function GetCharExpansion(ch As Char, LocaleSpecificLigatureTable As Byte(), Comparer As CompareInfo, Options As CompareOptions) As String
            Dim index As Integer = LikeOperator.CanCharExpand(ch, LocaleSpecificLigatureTable, Comparer, Options)
            If (index = 0) Then
                Return Conversions.ToString(ch)
            End If
            Return LikeOperator.LigatureExpansions(index)
        End Function

        Private Shared Function LigatureIndex(ch As Char) As Byte
            If ((Strings.Asc(ch) < &HC6) OrElse (Strings.Asc(ch) > &H153)) Then
                Return 0
            End If
            Return LikeOperator.LigatureMap((Strings.Asc(ch) - &HC6))
        End Function

        Public Shared Function LikeObject(Source As Object, Pattern As Object, CompareOption As CompareMethod) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(Source, IConvertible)
            If (convertible Is Nothing) Then
                If (Source Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Pattern, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Pattern Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            If ((empty = TypeCode.Object) AndAlso TypeOf Source Is Char()) Then
                empty = TypeCode.String
            End If
            If ((typeCode = TypeCode.Object) AndAlso TypeOf Pattern Is Char()) Then
                typeCode = TypeCode.String
            End If
            If ((empty = TypeCode.Object) OrElse (typeCode = TypeCode.Object)) Then
                Dim arguments As Object() = New Object() {Source, Pattern}
                Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Like, arguments)
            End If
            Return LikeOperator.LikeString(Conversions.ToString(Source), Conversions.ToString(Pattern), CompareOption)
        End Function

        Public Shared Function LikeString(Source As String, Pattern As String, CompareOption As CompareMethod) As Boolean
            Dim num As Integer
            Dim num2 As Integer
            Dim num3 As Integer
            Dim length As Integer
            Dim inputLigatureInfo As LigatureInfo() = Nothing
            Dim infoArray2 As LigatureInfo() = Nothing
            Dim ordinal As CompareOptions
            Dim compareInfo As CompareInfo
            Dim ch As Char
            Dim flag2 As Boolean
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
            If (CompareOption = CompareMethod.Binary) Then
                ordinal = CompareOptions.Ordinal
                compareInfo = Nothing
            Else
                compareInfo = Utils.GetCultureInfo.CompareInfo
                ordinal = (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase))
                Dim localeSpecificLigatureTable As Byte() = New Byte(((LikeOperator.LigatureExpansions.Length - 1) + 1) - 1) {}
                flag2 = False
                LikeOperator.ExpandString(Source, num3, inputLigatureInfo, localeSpecificLigatureTable, compareInfo, ordinal, flag2, False)
                flag2 = False
                LikeOperator.ExpandString(Pattern, length, infoArray2, localeSpecificLigatureTable, compareInfo, ordinal, flag2, False)
            End If
            Do While ((num2 < length) AndAlso (num < num3))
                ch = Pattern.Chars(num2)
                Select Case ch
                    Case "?"c, &HFF1F
                        LikeOperator.SkipToEndOfExpandedChar(inputLigatureInfo, num3, num)
                        Exit Select
                    Case "["c, &HFF3B
                        Dim flag3 As Boolean
                        Dim flag4 As Boolean
                        Dim flag5 As Boolean
                        flag2 = False
                        LikeOperator.MatchRange(Source, num3, num, inputLigatureInfo, Pattern, length, num2, infoArray2, flag3, flag4, flag5, compareInfo, ordinal, flag2, Nothing, False)
                        If flag5 Then
                            Dim args As String() = New String() {"Pattern"}
                            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", args))
                        End If
                        If flag4 Then
                            Return False
                        End If
                        If Not flag3 Then
                            Exit Select
                        End If
                        num2 += 1
                        Continue Do
                    Case "#"c, &HFF03
                        If Char.IsDigit(Source.Chars(num)) Then
                            Exit Select
                        End If
                        Return False
                    Case "*"c, &HFF0A
                        Dim flag6 As Boolean
                        Dim flag7 As Boolean
                        LikeOperator.MatchAsterisk(Source, num3, num, inputLigatureInfo, Pattern, length, num2, infoArray2, flag6, flag7, compareInfo, ordinal)
                        If flag7 Then
                            Dim textArray2 As String() = New String() {"Pattern"}
                            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", textArray2))
                        End If
                        Return Not flag6
                    Case Else
                        If (LikeOperator.CompareChars(Source, num3, num, num, inputLigatureInfo, Pattern, length, num2, num2, infoArray2, compareInfo, ordinal, False, False) <> 0) Then
                            Return False
                        End If
                        Exit Select
                End Select
                num2 += 1
                num += 1
            Loop
            Do While (num2 < length)
                ch = Pattern.Chars(num2)
                If ((ch = "*"c) OrElse (ch = &HFF0A)) Then
                    num2 += 1
                Else
                    If (((num2 + 1) >= length) OrElse (((ch <> "["c) OrElse (Pattern.Chars((num2 + 1)) <> "]"c)) AndAlso ((ch <> &HFF3B) OrElse (Pattern.Chars((num2 + 1)) <> &HFF3D)))) Then
                        Exit Do
                    End If
                    num2 = (num2 + 2)
                End If
            Loop
            Return ((num2 >= length) AndAlso (num >= num3))
        End Function

        Private Shared Sub MatchAsterisk(Source As String, SourceLength As Integer, SourceIndex As Integer, SourceLigatureInfo As LigatureInfo(), Pattern As String, PatternLength As Integer, PatternIndex As Integer, PattternLigatureInfo As LigatureInfo(), ByRef Mismatch As Boolean, ByRef PatternError As Boolean, Comparer As CompareInfo, Options As CompareOptions)
            Mismatch = False
            PatternError = False
            If (PatternIndex < PatternLength) Then
                Dim num2 As Integer
                Dim patternGroups As PatternGroup() = Nothing
                LikeOperator.BuildPatternGroups(Source, SourceLength, SourceIndex, SourceLigatureInfo, Pattern, PatternLength, PatternIndex, PattternLigatureInfo, PatternError, num2, Comparer, Options, patternGroups)
                If Not PatternError Then
                    If (patternGroups((num2 + 1)).PatType <> PatternType.NONE) Then
                        Dim num4 As Integer
                        Dim num3 As Integer = SourceIndex
                        Dim index As Integer = (num2 + 1)
                        Do
                            num4 = (num4 + patternGroups(index).CharCount)
                            index += 1
                        Loop While (patternGroups(index).PatType <> PatternType.NONE)
                        SourceIndex = SourceLength
                        LikeOperator.SubtractChars(Source, SourceLength, SourceIndex, num4, SourceLigatureInfo, Options)
                        LikeOperator.MatchAsterisk(Source, SourceLength, SourceIndex, SourceLigatureInfo, Pattern, PattternLigatureInfo, patternGroups, num2, Mismatch, PatternError, Comparer, Options)
                        If (PatternError OrElse Mismatch) Then
                            Return
                        End If
                        SourceLength = patternGroups((num2 + 1)).StartIndexOfPossibleMatch
                        If (SourceLength <= 0) Then
                            Return
                        End If
                        patternGroups(index).MaxSourceIndex = SourceLength
                        patternGroups(index).MinSourceIndex = SourceLength
                        patternGroups(index).StartIndexOfPossibleMatch = 0
                        patternGroups((num2 + 1)) = patternGroups(index)
                        patternGroups(num2).MinSourceIndex = 0
                        patternGroups(num2).StartIndexOfPossibleMatch = 0
                        index = (num2 + 1)
                        Dim num5 As Integer = SourceLength
                        Do While (index > 0)
                            Select Case patternGroups(index).PatType
                                Case PatternType.STRING
                                    num5 = (num5 - patternGroups(index).CharCount)
                                    Exit Select
                                Case PatternType.EXCLIST, PatternType.INCLIST
                                    num5 -= 1
                                    Exit Select
                                Case PatternType.DIGIT, PatternType.ANYCHAR
                                    num5 = (num5 - patternGroups(index).CharCount)
                                    Exit Select
                            End Select
                            patternGroups(index).MaxSourceIndex = num5
                            index -= 1
                        Loop
                        SourceIndex = num3
                    End If
                    LikeOperator.MatchAsterisk(Source, SourceLength, SourceIndex, SourceLigatureInfo, Pattern, PattternLigatureInfo, patternGroups, 0, Mismatch, PatternError, Comparer, Options)
                End If
            End If
        End Sub

        Private Shared Sub MatchAsterisk(Source As String, SourceLength As Integer, SourceIndex As Integer, SourceLigatureInfo As LigatureInfo(), Pattern As String, PatternLigatureInfo As LigatureInfo(), PatternGroups As PatternGroup(), PGIndex As Integer, ByRef Mismatch As Boolean, ByRef PatternError As Boolean, Comparer As CompareInfo, Options As CompareOptions)
            Dim group As PatternGroup
            Dim index As Integer = PGIndex
            Dim maxSourceIndex As Integer = SourceIndex
            Dim num3 As Integer = -1
            Dim num4 As Integer = -1
            PatternGroups(PGIndex).MinSourceIndex = SourceIndex
            PatternGroups(PGIndex).StartIndexOfPossibleMatch = SourceIndex
            PGIndex += 1
Label_002D:
            group = PatternGroups(PGIndex)
            Select Case group.PatType
                Case PatternType.STRING
Label_0069:
                    If (SourceIndex > group.MaxSourceIndex) Then
                        Mismatch = True
                        Return
                    End If
                    PatternGroups(PGIndex).StartIndexOfPossibleMatch = SourceIndex
                    Dim stringPatternStart As Integer = group.StringPatternStart
                    Dim num6 As Integer = 0
                    Dim leftStart As Integer = SourceIndex
                    Dim flag As Boolean = True
                    Do
                        If flag Then
                            flag = False
                            num6 = (leftStart + 1)
                        End If
                        If (LikeOperator.CompareChars(Source, SourceLength, leftStart, leftStart, SourceLigatureInfo, Pattern, (group.StringPatternEnd + 1), stringPatternStart, stringPatternStart, PatternLigatureInfo, Comparer, Options, False, False) <> 0) Then
                            SourceIndex = num6
                            index = (PGIndex - 1)
                            maxSourceIndex = SourceIndex
                            GoTo Label_0069
                        End If
                        stringPatternStart += 1
                        leftStart += 1
                        If (stringPatternStart > group.StringPatternEnd) Then
                            SourceIndex = leftStart
                            GoTo Label_02E8
                        End If
                    Loop While (leftStart < SourceLength)
                    Mismatch = True
                    Return
                Case PatternType.EXCLIST, PatternType.INCLIST
                    Do While True
                        If (SourceIndex > group.MaxSourceIndex) Then
                            Mismatch = True
                            Return
                        End If
                        PatternGroups(PGIndex).StartIndexOfPossibleMatch = SourceIndex
                        If LikeOperator.MatchRangeAfterAsterisk(Source, SourceLength, SourceIndex, SourceLigatureInfo, Pattern, PatternLigatureInfo, group, Comparer, Options) Then
                            GoTo Label_02E8
                        End If
                        index = (PGIndex - 1)
                        maxSourceIndex = SourceIndex
                    Loop
                Case PatternType.DIGIT
Label_0102:
                    If (SourceIndex > group.MaxSourceIndex) Then
                        Mismatch = True
                        Return
                    End If
                    PatternGroups(PGIndex).StartIndexOfPossibleMatch = SourceIndex
                    Dim num8 As Integer = group.CharCount
                    Dim j As Integer = 1
                    Do While (j <= num8)
                        SourceIndex += 1
                        If Not Char.IsDigit(Source.Chars(SourceIndex)) Then
                            index = (PGIndex - 1)
                            maxSourceIndex = SourceIndex
                            GoTo Label_0102
                        End If
                        j += 1
                    Loop
                    GoTo Label_02E8
                Case PatternType.ANYCHAR
                    If (SourceIndex <= group.MaxSourceIndex) Then
                        Exit Select
                    End If
                    Mismatch = True
                    Return
                Case PatternType.STAR
                    PatternGroups(PGIndex).StartIndexOfPossibleMatch = SourceIndex
                    group.MinSourceIndex = SourceIndex
                    If (PatternGroups(index).PatType = PatternType.STAR) Then
                        GoTo Label_02DD
                    End If
                    If (SourceIndex <= group.MaxSourceIndex) Then
                        GoTo Label_0279
                    End If
                    Mismatch = True
                    Return
                Case PatternType.NONE
                    PatternGroups(PGIndex).StartIndexOfPossibleMatch = group.MaxSourceIndex
                    If (SourceIndex < group.MaxSourceIndex) Then
                        index = (PGIndex - 1)
                        maxSourceIndex = group.MaxSourceIndex
                    End If
                    If ((PatternGroups(index).PatType = PatternType.STAR) OrElse (PatternGroups(index).PatType = PatternType.NONE)) Then
                        Return
                    End If
                    GoTo Label_0279
                Case Else
                    GoTo Label_02E8
            End Select
            PatternGroups(PGIndex).StartIndexOfPossibleMatch = SourceIndex
            Dim charCount As Integer = group.CharCount
            Dim i As Integer = 1
            Do While (i <= charCount)
                If (SourceIndex >= SourceLength) Then
                    Mismatch = True
                    Return
                End If
                LikeOperator.SkipToEndOfExpandedChar(SourceLigatureInfo, SourceLength, SourceIndex)
                SourceIndex += 1
                i += 1
            Loop
            GoTo Label_02E8
Label_0279:
            num3 = PGIndex
            SourceIndex = maxSourceIndex
            PGIndex = index
            Do
                LikeOperator.SubtractChars(Source, SourceLength, SourceIndex, PatternGroups(PGIndex).CharCount, SourceLigatureInfo, Options)
                PGIndex -= 1
            Loop While (PatternGroups(PGIndex).PatType <> PatternType.STAR)
            SourceIndex = Math.Max(SourceIndex, (PatternGroups(PGIndex).MinSourceIndex + 1))
            PatternGroups(PGIndex).MinSourceIndex = SourceIndex
            num4 = PGIndex
Label_02DD:
            PGIndex += 1
            GoTo Label_002D
Label_02E8:
            If (PGIndex = index) Then
                If (SourceIndex = maxSourceIndex) Then
                    SourceIndex = PatternGroups(num3).MinSourceIndex
                    PGIndex = num3
                    index = num3
                ElseIf (SourceIndex < maxSourceIndex) Then
                    Dim numRef As Integer
                    numRef = CInt(AddressOf PatternGroups(num4).MinSourceIndex) = (numRef + 1)
                    SourceIndex = PatternGroups(num4).MinSourceIndex
                    PGIndex = (num4 + 1)
                Else
                    PGIndex += 1
                    index = num4
                End If
            Else
                PGIndex += 1
            End If
            GoTo Label_002D
        End Sub

        Private Shared Sub MatchRange(Source As String, SourceLength As Integer, ByRef SourceIndex As Integer, SourceLigatureInfo As LigatureInfo(), Pattern As String, PatternLength As Integer, ByRef PatternIndex As Integer, PatternLigatureInfo As LigatureInfo(), ByRef RangePatternEmpty As Boolean, ByRef Mismatch As Boolean, ByRef PatternError As Boolean, Comparer As CompareInfo, Options As CompareOptions, ByRef Optional SeenNot As Boolean = False, Optional RangeList As List(Of Range) = Nothing, Optional ValidatePatternWithoutMatching As Boolean = False)
            Dim str As String
            Dim range As Range
            Dim num As Integer
            Dim num2 As Integer
            Dim num3 As Integer
            RangePatternEmpty = False
            Mismatch = False
            PatternError = False
            SeenNot = False
            PatternIndex += 1
            If (PatternIndex >= PatternLength) Then
                PatternError = True
                Return
            End If
            Dim ch As Char = Pattern.Chars(PatternIndex)
            Select Case ch
                Case "!"c, ChrW(&HFF01)
                    SeenNot = True
                    PatternIndex += 1
                    If (PatternIndex >= PatternLength) Then
                        Mismatch = True
                        Return
                    End If
                    ch = Pattern.Chars(PatternIndex)
                    Exit Select
            End Select
            If ((ch = "]"c) OrElse (AscW(ch) = &HFF3D)) Then
                If SeenNot Then
                    SeenNot = False
                    If Not ValidatePatternWithoutMatching Then
                        Mismatch = (LikeOperator.CompareChars(Source.Chars(SourceIndex), "!"c, Comparer, Options) > 0)
                    End If
                    If (Not RangeList Is Nothing) Then
                        range.Start = (PatternIndex - 1)
                        range.StartLength = 1
                        range.End = -1
                        range.EndLength = 0
                        RangeList.Add(range)
                    End If
                    Return
                End If
                RangePatternEmpty = True
                Return
            End If
Label_00C9:
            str = Nothing
            Dim right As String = Nothing
            If ((ch = "]"c) OrElse (AscW(ch) = &HFF3D)) Then
                Mismatch = Not SeenNot
                Return
            End If
            If ((Not ValidatePatternWithoutMatching AndAlso (Not PatternLigatureInfo Is Nothing)) AndAlso (PatternLigatureInfo(PatternIndex).Kind = CharKind.ExpandedChar1)) Then
                If (LikeOperator.CompareChars(Source, SourceLength, SourceIndex, num, SourceLigatureInfo, Pattern, PatternLength, PatternIndex, num2, PatternLigatureInfo, Comparer, Options, True, False) <> 0) Then
                    GoTo Label_013B
                End If
                SourceIndex = num
                PatternIndex = num2
                GoTo Label_0369
            End If
            num2 = PatternIndex
            LikeOperator.SkipToEndOfExpandedChar(PatternLigatureInfo, PatternLength, num2)
Label_013B:
            range.Start = PatternIndex
            range.StartLength = ((num2 - PatternIndex) + 1)
            If (Options = CompareOptions.Ordinal) Then
                str = Conversions.ToString(Pattern.Chars(PatternIndex))
            ElseIf ((Not PatternLigatureInfo Is Nothing) AndAlso (PatternLigatureInfo(PatternIndex).Kind = CharKind.ExpandedChar1)) Then
                str = Conversions.ToString(PatternLigatureInfo(PatternIndex).CharBeforeExpansion)
                PatternIndex = num2
            Else
                str = Pattern.Substring(PatternIndex, ((num2 - PatternIndex) + 1))
                PatternIndex = num2
            End If
            If ((((num2 + 2) >= PatternLength) OrElse ((Pattern.Chars((num2 + 1)) <> "-"c) AndAlso (AscW(Pattern.Chars((num2 + 1))) <> &HFF0D))) OrElse ((Pattern.Chars((num2 + 2)) = "]"c) OrElse (Pattern.Chars((num2 + 2)) = &HFF3D))) Then
                If Not ValidatePatternWithoutMatching Then
                    num3 = 0
                    If (LikeOperator.CompareChars(Source, SourceLength, SourceIndex, num, SourceLigatureInfo, Pattern, (range.Start + range.StartLength), range.Start, num3, PatternLigatureInfo, Comparer, Options, False, True) = 0) Then
                        GoTo Label_0369
                    End If
                End If
                range.End = -1
                range.EndLength = 0
                GoTo Label_03EE
            End If
            PatternIndex = (PatternIndex + 2)
            If ((Not ValidatePatternWithoutMatching AndAlso (Not PatternLigatureInfo Is Nothing)) AndAlso (PatternLigatureInfo(PatternIndex).Kind = CharKind.ExpandedChar1)) Then
                If (LikeOperator.CompareChars(Source, SourceLength, SourceIndex, num, SourceLigatureInfo, Pattern, PatternLength, PatternIndex, num2, PatternLigatureInfo, Comparer, Options, True, False) <> 0) Then
                    GoTo Label_0269
                End If
                PatternIndex = num2
                GoTo Label_0369
            End If
            num2 = PatternIndex
            LikeOperator.SkipToEndOfExpandedChar(PatternLigatureInfo, PatternLength, num2)
Label_0269:
            range.End = PatternIndex
            range.EndLength = ((num2 - PatternIndex) + 1)
            If (Options = CompareOptions.Ordinal) Then
                right = Conversions.ToString(Pattern.Chars(PatternIndex))
            ElseIf ((Not PatternLigatureInfo Is Nothing) AndAlso (PatternLigatureInfo(PatternIndex).Kind = CharKind.ExpandedChar1)) Then
                right = Conversions.ToString(PatternLigatureInfo(PatternIndex).CharBeforeExpansion)
                PatternIndex = num2
            Else
                right = Pattern.Substring(PatternIndex, ((num2 - PatternIndex) + 1))
                PatternIndex = num2
            End If
            If (LikeOperator.CompareChars(str, right, Comparer, Options) > 0) Then
                PatternError = True
                Return
            End If
            If ValidatePatternWithoutMatching Then
                GoTo Label_03EE
            End If
            num3 = 0
            If (LikeOperator.CompareChars(Source, SourceLength, SourceIndex, num, SourceLigatureInfo, Pattern, (range.Start + range.StartLength), range.Start, num3, PatternLigatureInfo, Comparer, Options, False, True) < 0) Then
                GoTo Label_03EE
            End If
            num3 = 0
            If (LikeOperator.CompareChars(Source, SourceLength, SourceIndex, num, SourceLigatureInfo, Pattern, (range.End + range.EndLength), range.End, num3, PatternLigatureInfo, Comparer, Options, False, True) > 0) Then
                GoTo Label_03EE
            End If
Label_0369:
            If SeenNot Then
                Mismatch = True
                Return
            End If
            Do
                PatternIndex += 1
                If (PatternIndex >= PatternLength) Then
                    PatternError = True
                    Return
                End If
            Loop While ((Pattern.Chars(PatternIndex) <> "]"c) AndAlso (AscW(Pattern.Chars(PatternIndex)) <> &HFF3D))
            SourceIndex = num
            Return
Label_03EE:
            If (Not RangeList Is Nothing) Then
                RangeList.Add(range)
            End If
            PatternIndex += 1
            If (PatternIndex >= PatternLength) Then
                PatternError = True
            Else
                ch = Pattern.Chars(PatternIndex)
                GoTo Label_00C9
            End If
        End Sub

        Private Shared Function MatchRangeAfterAsterisk(Source As String, SourceLength As Integer, ByRef SourceIndex As Integer, SourceLigatureInfo As LigatureInfo(), Pattern As String, PatternLigatureInfo As LigatureInfo(), PG As PatternGroup, Comparer As CompareInfo, Options As CompareOptions) As Boolean
            Dim rangeList As List(Of Range) = PG.RangeList
            Dim leftEnd As Integer = SourceIndex
            Dim flag As Boolean = False
            Dim range As Range
            For Each range In rangeList
                Dim num4 As Integer
                Dim num2 As Integer = 1
                If ((Not PatternLigatureInfo Is Nothing) AndAlso (PatternLigatureInfo(range.Start).Kind = CharKind.ExpandedChar1)) Then
                    num4 = 0
                    If (LikeOperator.CompareChars(Source, SourceLength, SourceIndex, leftEnd, SourceLigatureInfo, Pattern, (range.Start + range.StartLength), range.Start, num4, PatternLigatureInfo, Comparer, Options, True, False) = 0) Then
                        flag = True
                        Exit For
                    End If
                End If
                num4 = 0
                Dim num3 As Integer = LikeOperator.CompareChars(Source, SourceLength, SourceIndex, leftEnd, SourceLigatureInfo, Pattern, (range.Start + range.StartLength), range.Start, num4, PatternLigatureInfo, Comparer, Options, False, True)
                If ((num3 > 0) AndAlso (range.End >= 0)) Then
                    num4 = 0
                    num2 = LikeOperator.CompareChars(Source, SourceLength, SourceIndex, leftEnd, SourceLigatureInfo, Pattern, (range.End + range.EndLength), range.End, num4, PatternLigatureInfo, Comparer, Options, False, True)
                End If
                If ((num3 = 0) OrElse ((num3 > 0) AndAlso (num2 <= 0))) Then
                    flag = True
                    Exit For
                End If
            Next
            If (PG.PatType = PatternType.EXCLIST) Then
                flag = Not flag
            End If
            SourceIndex = (leftEnd + 1)
            Return flag
        End Function

        Private Shared Sub SkipToEndOfExpandedChar(InputLigatureInfo As LigatureInfo(), Length As Integer, ByRef Current As Integer)
            If (((Not InputLigatureInfo Is Nothing) AndAlso (Current < Length)) AndAlso (InputLigatureInfo(Current).Kind = CharKind.ExpandedChar1)) Then
                Current += 1
            End If
        End Sub

        Private Shared Sub SubtractChars(Input As String, InputLength As Integer, ByRef Current As Integer, CharsToSubtract As Integer, InputLigatureInfo As LigatureInfo(), Options As CompareOptions)
            If (Options = CompareOptions.Ordinal) Then
                Current = (Current - CharsToSubtract)
                If (Current < 0) Then
                    Current = 0
                End If
            Else
                Dim num As Integer = CharsToSubtract
                Dim i As Integer = 1
                Do While (i <= num)
                    LikeOperator.SubtractOneCharInTextCompareMode(Input, InputLength, Current, InputLigatureInfo, Options)
                    If (Current < 0) Then
                        Current = 0
                        Return
                    End If
                    i += 1
                Loop
            End If
        End Sub

        Private Shared Sub SubtractOneCharInTextCompareMode(Input As String, InputLength As Integer, ByRef Current As Integer, InputLigatureInfo As LigatureInfo(), Options As CompareOptions)
            If (Current >= InputLength) Then
                Current -= 1
            ElseIf ((Not InputLigatureInfo Is Nothing) AndAlso (InputLigatureInfo(Current).Kind = CharKind.ExpandedChar2)) Then
                Current = (Current - 2)
            Else
                Current -= 1
            End If
        End Sub

        Private Shared Function ValidateRangePattern(Pattern As String, PatternLength As Integer, ByRef PatternIndex As Integer, PatternLigatureInfo As LigatureInfo(), Comparer As CompareInfo, Options As CompareOptions, ByRef SeenNot As Boolean, ByRef RangeList As List(Of Range)) As Boolean
            Dim flag As Boolean
            Dim sourceIndex As Integer = -1
            Dim rangePatternEmpty As Boolean = False
            Dim mismatch As Boolean = False
            LikeOperator.MatchRange(Nothing, -1, sourceIndex, Nothing, Pattern, PatternLength, PatternIndex, PatternLigatureInfo, rangePatternEmpty, mismatch, flag, Comparer, Options, SeenNot, RangeList, True)
            Return Not flag
        End Function


        ' Fields
        Private Shared LigatureExpansions As String() = New String() {"", "ss", "sz", "AE", "ae", "TH", "th", "OE", "oe"}
        Private Shared LigatureMap As Byte() = New Byte(&H8E - 1) {}

        ' Nested Types
        Private Enum CharKind
            ' Fields
            ExpandedChar1 = 1
            ExpandedChar2 = 2
            None = 0
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Private Structure LigatureInfo
            Friend Kind As CharKind
            Friend CharBeforeExpansion As Char
        End Structure

        Private Enum Ligatures
            ' Fields
            ae = 230
            aeUpper = &HC6
            Invalid = 0
            Max = &H153
            Min = &HC6
            oe = &H153
            oeUpper = &H152
            ssBeta = &HDF
            szBeta = &HDF
            th = &HFE
            thUpper = &HDE
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Private Structure PatternGroup
            Friend PatType As PatternType
            Friend MaxSourceIndex As Integer
            Friend CharCount As Integer
            Friend StringPatternStart As Integer
            Friend StringPatternEnd As Integer
            Friend MinSourceIndex As Integer
            Friend RangeList As List(Of Range)
            Public StartIndexOfPossibleMatch As Integer
        End Structure

        Private Enum PatternType
            ' Fields
            ANYCHAR = 4
            DIGIT = 3
            EXCLIST = 1
            INCLIST = 2
            NONE = 6
            STAR = 5
            [STRING] = 0
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Private Structure Range
            Friend Start As Integer
            Friend StartLength As Integer
            Friend [End] As Integer
            Friend EndLength As Integer
        End Structure
    End Class
End Namespace

