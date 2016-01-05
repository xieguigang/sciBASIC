Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Security
Imports System.Threading

Namespace Microsoft.VisualBasic
    <StandardModule> _
    Public NotInheritable Class Conversion
        ' Methods
        Public Shared Function CTypeDynamic(Of TargetType)(Expression As Object) As TargetType
            Return DirectCast(Conversions.ChangeType(Expression, GetType(TargetType), True), TargetType)
        End Function

        Public Shared Function CTypeDynamic(Expression As Object, TargetType As Type) As Object
            Return Conversions.ChangeType(Expression, TargetType, True)
        End Function

        Public Shared Function ErrorToString() As String
            Return Information.Err.Description
        End Function

        Public Shared Function ErrorToString(ErrorNumber As Integer) As String
            If (ErrorNumber >= &HFFFF) Then
                Throw New ArgumentException(Utils.GetResourceString("MaxErrNumber"))
            End If
            If (ErrorNumber > 0) Then
                ErrorNumber = (-2146828288 Or ErrorNumber)
            End If
            If ((ErrorNumber And &H1FFF0000) = &HA0000) Then
                ErrorNumber = (ErrorNumber And &HFFFF)
                Return Utils.GetResourceString(DirectCast(ErrorNumber, vbErrors))
            End If
            If (ErrorNumber <> 0) Then
                Return Utils.GetResourceString(vbErrors.UserDefined)
            End If
            Return ""
        End Function

        Public Shared Function Fix(Number As Decimal) As Decimal
            If (Number < Decimal.Zero) Then
                Return Decimal.Negate(Decimal.Floor(Decimal.Negate(Number)))
            End If
            Return Decimal.Floor(Number)
        End Function

        Public Shared Function Fix(Number As Double) As Double
            If (Number >= 0) Then
                Return Math.Floor(Number)
            End If
            Return -Math.Floor(-Number)
        End Function

        Public Shared Function Fix(Number As Short) As Short
            Return Number
        End Function

        Public Shared Function Fix(Number As Integer) As Integer
            Return Number
        End Function

        Public Shared Function Fix(Number As Long) As Long
            Return Number
        End Function

        Public Shared Function Fix(Number As Object) As Object
            If (Number Is Nothing) Then
                Dim textArray1 As String() = New String() {"Number"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray1))
            End If
            Dim convertible As IConvertible = TryCast(Number, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return convertible.ToInt32(Nothing)
                    Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64
                        Return Number
                    Case TypeCode.Single
                        Return Conversion.Fix(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        Return Conversion.Fix(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return Conversion.Fix(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Return Conversion.Fix(Conversions.ToDouble(convertible.ToString(Nothing)))
                End Select
            End If
            Dim args As String() = New String() {"Number", Number.GetType.FullName}
            Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_NotNumericType2", args)), 13)
        End Function

        Public Shared Function Fix(Number As Single) As Single
            If (Number >= 0!) Then
                Return CSng(Math.Floor(CDbl(Number)))
            End If
            Return CSng(-Math.Floor(CDbl(-Number)))
        End Function

        Public Shared Function Hex(Number As Byte) As String
            Return Number.ToString("X")
        End Function

        Public Shared Function Hex(Number As Short) As String
            Return Number.ToString("X")
        End Function

        Public Shared Function Hex(Number As Integer) As String
            Return Number.ToString("X")
        End Function

        Public Shared Function Hex(Number As Long) As String
            Return Number.ToString("X")
        End Function

        Public Shared Function Hex(Number As Object) As String
            Dim num As Long
            If (Number Is Nothing) Then
                Dim textArray1 As String() = New String() {"Number"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray1))
            End If
            Dim convertible As IConvertible = TryCast(Number, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.SByte
                        Return Conversion.Hex(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        Return Conversion.Hex(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        Return Conversion.Hex(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        Return Conversion.Hex(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        Return Conversion.Hex(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        Return Conversion.Hex(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                        num = convertible.ToInt64(Nothing)
                        GoTo Label_0130
                    Case TypeCode.UInt64
                        Return Conversion.Hex(convertible.ToUInt64(Nothing))
                    Case TypeCode.String
                        Try
                            num = Conversions.ToLong(convertible.ToString(Nothing))
                        Catch exception As OverflowException
                            Return Conversion.Hex(Conversions.ToULong(convertible.ToString(Nothing)))
                        End Try
                        GoTo Label_0130
                End Select
            End If
            Dim args As String() = New String() {"Number", Utils.VBFriendlyName(Number)}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", args))
Label_0130:
            If (num = 0) Then
                Return "0"
            End If
            If ((num <= 0) AndAlso (num >= -2147483648)) Then
                Return Conversion.Hex(CInt(num))
            End If
            Return Conversion.Hex(num)
        End Function

        <CLSCompliant(False)>
        Public Shared Function Hex(Number As SByte) As String
            Return Number.ToString("X")
        End Function

        <CLSCompliant(False)>
        Public Shared Function Hex(Number As UInt16) As String
            Return Number.ToString("X")
        End Function

        <CLSCompliant(False)>
        Public Shared Function Hex(Number As UInt32) As String
            Return Number.ToString("X")
        End Function

        <CLSCompliant(False)>
        Public Shared Function Hex(Number As UInt64) As String
            Return Number.ToString("X")
        End Function

        Private Shared Function HexOrOctValue(InputStr As String, i As Integer) As Double
            Dim num4 As Long
            Dim num5 As Integer
            Dim num2 As Integer = 0
            Dim length As Integer = InputStr.Length
            Dim ch As Char = InputStr.Chars(i)
            i += 1
            If ((ch <> "H"c) AndAlso (ch <> "h"c)) Then
                If ((ch <> "O"c) AndAlso (ch <> "o"c)) Then
                    Return 0
                End If
                Do While ((i < length) AndAlso (num2 < &H16))
                    ch = InputStr.Chars(i)
                    i += 1
                    Dim ch3 As Char = ch
                    If ((((ch3 <> ChrW(9)) AndAlso (ch3 <> ChrW(10))) AndAlso ((ch3 <> ChrW(13)) AndAlso (ch3 <> " "c))) AndAlso (ch3 <> ChrW(12288))) Then
                        If (ch3 = "0"c) Then
                            If (num2 = 0) Then
                                Continue Do
                            End If
                            num5 = 0
                        Else
                            If ((ch3 < "1"c) OrElse (ch3 > "7"c)) Then
                                Exit Do
                            End If
                            num5 = (ch - "0"c)
                        End If
                        If (num4 >= &H1000000000000000) Then
                            num4 = ((num4 And &HFFFFFFFFFFFFFFF) * 8)
                            num4 = (num4 Or &H1000000000000000)
                        Else
                            num4 = (num4 * 8)
                        End If
                        num4 = (num4 + num5)
                        num2 += 1
                    End If
                Loop
            Else
                Do While ((i < length) AndAlso (num2 < &H11))
                    ch = InputStr.Chars(i)
                    i += 1
                    Dim ch2 As Char = ch
                    If ((((ch2 <> ChrW(9)) AndAlso (ch2 <> ChrW(10))) AndAlso ((ch2 <> ChrW(13)) AndAlso (ch2 <> " "c))) AndAlso (ch2 <> ChrW(12288))) Then
                        If (ch2 = "0"c) Then
                            If (num2 = 0) Then
                                Continue Do
                            End If
                            num5 = 0
                        ElseIf ((ch2 >= "1"c) AndAlso (ch2 <= "9"c)) Then
                            num5 = (ch - "0"c)
                        ElseIf ((ch2 >= "A"c) AndAlso (ch2 <= "F"c)) Then
                            num5 = (ch - "7"c)
                        Else
                            If ((ch2 < "a"c) OrElse (ch2 > "f"c)) Then
                                Exit Do
                            End If
                            num5 = (ch - "W"c)
                        End If
                        If ((num2 = 15) AndAlso (num4 > &H7FFFFFFFFFFFFFF)) Then
                            num4 = ((num4 And &H7FFFFFFFFFFFFFF) * &H10)
                            num4 = (num4 Or -9223372036854775808)
                        Else
                            num4 = (num4 * &H10)
                        End If
                        num4 = (num4 + num5)
                        num2 += 1
                    End If
                Loop
                If (num2 = &H10) Then
                    i += 1
                    If (i < length) Then
                        ch = InputStr.Chars(i)
                    End If
                End If
                If (num2 <= 8) Then
                    If ((num2 > 4) OrElse (ch = "&"c)) Then
                        If (num4 > &H7FFFFFFF) Then
                            num4 = (-2147483648 + (num4 And &H7FFFFFFF))
                        End If
                    ElseIf (((num2 > 2) OrElse (ch = "%"c)) AndAlso (num4 > &H7FFF)) Then
                        num4 = (-32768 + (num4 And &H7FFF))
                    End If
                End If
                Select Case ch
                    Case "%"c
                        num4 = CShort(num4)
                        Exit Select
                    Case "&"c
                        num4 = CInt(num4)
                        Exit Select
                End Select
                Return CDbl(num4)
            End If
            If (num2 = &H16) Then
                i += 1
                If (i < length) Then
                    ch = InputStr.Chars(i)
                End If
            End If
            If (num4 <= &H100000000) Then
                If ((num4 > &HFFFF) OrElse (ch = "&"c)) Then
                    If (num4 > &H7FFFFFFF) Then
                        num4 = (-2147483648 + (num4 And &H7FFFFFFF))
                    End If
                ElseIf (((num4 > &HFF) OrElse (ch = "%"c)) AndAlso (num4 > &H7FFF)) Then
                    num4 = (-32768 + (num4 And &H7FFF))
                End If
            End If
            Select Case ch
                Case "%"c
                    num4 = CShort(num4)
                    Exit Select
                Case "&"c
                    num4 = CInt(num4)
                    Exit Select
            End Select
            Return CDbl(num4)
        End Function

        Public Shared Function Int(Number As Decimal) As Decimal
            Return Decimal.Floor(Number)
        End Function

        Public Shared Function Int(Number As Double) As Double
            Return Math.Floor(Number)
        End Function

        Public Shared Function Int(Number As Short) As Short
            Return Number
        End Function

        Public Shared Function Int(Number As Integer) As Integer
            Return Number
        End Function

        Public Shared Function Int(Number As Long) As Long
            Return Number
        End Function

        Public Shared Function Int(Number As Object) As Object
            If (Number Is Nothing) Then
                Dim textArray1 As String() = New String() {"Number"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray1))
            End If
            Dim convertible As IConvertible = TryCast(Number, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return convertible.ToInt32(Nothing)
                    Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64
                        Return Number
                    Case TypeCode.Single
                        Return Conversion.Int(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        Return Conversion.Int(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return Conversion.Int(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Return Conversion.Int(Conversions.ToDouble(convertible.ToString(Nothing)))
                End Select
            End If
            Dim args As String() = New String() {"Number", Number.GetType.FullName}
            Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_NotNumericType2", args)), 13)
        End Function

        Public Shared Function Int(Number As Single) As Single
            Return CSng(Math.Floor(CDbl(Number)))
        End Function

        Public Shared Function Oct(Number As Byte) As String
            Return Utils.OctFromULong(CULng(Number))
        End Function

        Public Shared Function Oct(Number As Short) As String
            Return Utils.OctFromLong((Number And &HFFFF))
        End Function

        Public Shared Function Oct(Number As Integer) As String
            Return Utils.OctFromLong((Number And CLng(&HFFFFFFFF)))
        End Function

        Public Shared Function Oct(Number As Long) As String
            Return Utils.OctFromLong(Number)
        End Function

        Public Shared Function Oct(Number As Object) As String
            Dim num As Long
            If (Number Is Nothing) Then
                Dim textArray1 As String() = New String() {"Number"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray1))
            End If
            Dim convertible As IConvertible = TryCast(Number, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.SByte
                        Return Conversion.Oct(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        Return Conversion.Oct(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        Return Conversion.Oct(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        Return Conversion.Oct(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        Return Conversion.Oct(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        Return Conversion.Oct(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                        num = convertible.ToInt64(Nothing)
                        GoTo Label_0130
                    Case TypeCode.UInt64
                        Return Conversion.Oct(convertible.ToUInt64(Nothing))
                    Case TypeCode.String
                        Try
                            num = Conversions.ToLong(convertible.ToString(Nothing))
                        Catch exception As OverflowException
                            Return Conversion.Oct(Conversions.ToULong(convertible.ToString(Nothing)))
                        End Try
                        GoTo Label_0130
                End Select
            End If
            Dim args As String() = New String() {"Number", Utils.VBFriendlyName(Number)}
            Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", args))
Label_0130:
            If (num = 0) Then
                Return "0"
            End If
            If ((num <= 0) AndAlso (num >= -2147483648)) Then
                Return Conversion.Oct(CInt(num))
            End If
            Return Conversion.Oct(num)
        End Function

        <CLSCompliant(False)>
        Public Shared Function Oct(Number As SByte) As String
            Return Utils.OctFromLong((Number And &HFF))
        End Function

        <CLSCompliant(False)>
        Public Shared Function Oct(Number As UInt16) As String
            Return Utils.OctFromULong(CULng(Number))
        End Function

        <CLSCompliant(False)>
        Public Shared Function Oct(Number As UInt32) As String
            Return Utils.OctFromULong(CULng(Number))
        End Function

        <CLSCompliant(False)>
        Public Shared Function Oct(Number As UInt64) As String
            Return Utils.OctFromULong(Number)
        End Function

        <SecurityCritical>
        Friend Shared Function ParseInputField(Value As Object, vtInput As VariantType) As Object
            Dim num As Integer
            Dim num2 As Integer
            Dim ch As Char
            Dim str As String = Conversions.ToString(Value)
            If ((vtInput = VariantType.Empty) AndAlso ((Value Is Nothing) OrElse (Strings.Len(Conversions.ToString(Value)) = 0))) Then
                Return Nothing
            End If
            Dim projectData As ProjectData = ProjectData.GetProjectData
            Dim numprsPtr As Byte() = projectData.m_numprsPtr
            Dim digitArray As Byte() = projectData.m_DigitArray
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32(digitArray.Length)), 0, numprsPtr, 0, 4)
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32(&H954)), 0, numprsPtr, 4, 4)
            If (UnsafeNativeMethods.VarParseNumFromStr(str, &H409, -2147483648, numprsPtr, digitArray) < 0) Then
                If (vtInput <> VariantType.Empty) Then
                    Return 0
                End If
                Return str
            End If
            Dim num5 As Integer = BitConverter.ToInt32(numprsPtr, 8)
            Dim num3 As Integer = BitConverter.ToInt32(numprsPtr, 12)
            Dim num6 As Integer = BitConverter.ToInt32(numprsPtr, &H10)
            Dim num4 As Integer = BitConverter.ToInt32(numprsPtr, 20)
            If (num3 < str.Length) Then
                ch = str.Chars(num3)
            End If
            Select Case ch
                Case "!"c
                    If (vtInput <> VariantType.Double) Then
                        num = 4
                        Exit Select
                    End If
                    num = 5
                    Exit Select
                Case "#"c
                    num = 5
                    num2 = &H7FFFFFFF
                    GoTo Label_019C
                Case "%"c
                    num = 2
                    num2 = 0
                    GoTo Label_019C
                Case "&"c
                    num = 3
                    num2 = 0
                    GoTo Label_019C
                Case "@"c
                    num = 14
                    num2 = 4
                    GoTo Label_019C
                Case Else
                    If (vtInput = VariantType.Empty) Then
                        Dim dwVtBits As Integer = &H402C
                        If ((num5 And &H800) <> 0) Then
                            dwVtBits = &H20
                        End If
                        Return UnsafeNativeMethods.VarNumFromParseNum(numprsPtr, digitArray, dwVtBits)
                    End If
                    If (num6 <> 0) Then
                        Value = UnsafeNativeMethods.VarNumFromParseNum(numprsPtr, digitArray, 8)
                        Dim num8 As Integer = Conversions.ToInteger(Value)
                        If ((num8 And -65536) = 0) Then
                            num8 = CShort(num8)
                        End If
                        UnsafeNativeMethods.VariantChangeType(Value, Value, 0, CShort(vtInput))
                        Return Value
                    End If
                    Return UnsafeNativeMethods.VarNumFromParseNum(numprsPtr, digitArray, Conversion.ShiftVTBits(CInt(vtInput)))
            End Select
            num2 = &H7FFFFFFF
Label_019C:
            If ((0 - num4) > num2) Then
                Throw ExceptionUtils.VbMakeException(13)
            End If
            Value = UnsafeNativeMethods.VarNumFromParseNum(numprsPtr, digitArray, Conversion.ShiftVTBits(num))
            If (vtInput <> VariantType.Empty) Then
                UnsafeNativeMethods.VariantChangeType(Value, Value, 0, CShort(vtInput))
            End If
            Return Value
        End Function

        Private Shared Function ShiftVTBits(vt As Integer) As Integer
            Select Case vt
                Case 2
                    Return 4
                Case 3
                    Return 8
                Case 4
                    Return &H10
                Case 5
                    Return &H20
                Case 6, 14
                    Return &H4000
                Case 7
                    Return &H80
                Case 8
                    Return &H100
                Case 9
                    Return &H200
                Case 10
                    Return &H400
                Case 11
                    Return &H800
                Case 12
                    Return &H1000
                Case 13
                    Return &H2000
                Case &H11
                    Return &H20000
                Case &H12
                    Return &H40000
                Case 20
                    Return &H100000
            End Select
            Return 0
        End Function

        Public Shared Function Str(Number As Object) As String
            Dim str2 As String
            If (Number Is Nothing) Then
                Dim args As String() = New String() {"Number"}
                Throw New ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            Dim convertible As IConvertible = TryCast(Number, IConvertible)
            If (convertible Is Nothing) Then
                Dim textArray2 As String() = New String() {"Number"}
                Throw New InvalidCastException(Utils.GetResourceString("ArgumentNotNumeric1", textArray2))
            End If
            Dim typeCode As TypeCode = convertible.GetTypeCode
            Select Case typeCode
                Case TypeCode.DBNull
                    Return "Null"
                Case TypeCode.Boolean
                    If Not convertible.ToBoolean(Nothing) Then
                        Return "False"
                    End If
                    Return "True"
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    str2 = Conversions.ToString(Number)
                    Exit Select
                Case Else
                    If (typeCode = TypeCode.String) Then
                        Try
                            str2 = Conversions.ToString(Conversions.ToDouble(convertible.ToString(Nothing)))
                            Exit Select
                        Catch exception As StackOverflowException
                            Throw exception
                        Catch exception2 As OutOfMemoryException
                            Throw exception2
                        Catch exception3 As ThreadAbortException
                            Throw exception3
                        Catch exception6 As Exception
                        End Try
                    End If
                    Dim textArray3 As String() = New String() {"Number"}
                    Throw New InvalidCastException(Utils.GetResourceString("ArgumentNotNumeric1", textArray3))
            End Select
            If ((str2.Length > 0) AndAlso (str2.Chars(0) <> "-"c)) Then
                Return (" " & Utils.StdFormat(str2))
            End If
            Return Utils.StdFormat(str2)
        End Function

        Public Shared Function Val(Expression As Char) As Integer
            Dim num2 As Integer = Expression
            If ((num2 >= &H31) AndAlso (num2 <= &H39)) Then
                Return (num2 - &H30)
            End If
            Return 0
        End Function

        Public Shared Function Val(Expression As Object) As Double
            Dim str2 As String
            Dim inputStr As String = TryCast(Expression, String)
            If (Not inputStr Is Nothing) Then
                Return Conversion.Val(inputStr)
            End If
            If TypeOf Expression Is Char Then
                Return CDbl(Conversion.Val(DirectCast(Expression, Char)))
            End If
            If Versioned.IsNumeric(Expression) Then
                Return Conversions.ToDouble(Expression)
            End If
            Try
                str2 = Conversions.ToString(Expression)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Dim args As String() = New String() {"Expression", Utils.VBFriendlyName(Expression)}
                Throw ExceptionUtils.VbMakeException(New ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", args)), &H1B6)
            End Try
            Return Conversion.Val(str2)
        End Function

        Public Shared Function Val(InputStr As String) As Double
            Dim length As Integer
            Dim num4 As Integer
            Dim num5 As Integer
            Dim num6 As Integer
            Dim num7 As Double
            If (InputStr Is Nothing) Then
                length = 0
            Else
                length = InputStr.Length
            End If
            Dim num2 As Integer = 0
            Do While (num2 < length)
                Select Case InputStr.Chars(num2)
                    Case ChrW(9), ChrW(10), ChrW(13), " "c, ChrW(12288)
                        Exit Select
                    Case ChrW(11), ChrW(12)
                        GoTo Label_004C
                    Case Else
                        GoTo Label_004C
                End Select
                num2 += 1
            Loop
Label_004C:
            If (num2 >= length) Then
                Return 0
            End If
            Dim ch As Char = InputStr.Chars(num2)
            If (ch = "&"c) Then
                Return Conversion.HexOrOctValue(InputStr, (num2 + 1))
            End If
            Dim flag As Boolean = False
            Dim flag2 As Boolean = False
            Dim flag3 As Boolean = False
            Dim y As Double = 0
            ch = InputStr.Chars(num2)
            Select Case ch
                Case "-"c
                    flag3 = True
                    num2 += 1
                    Exit Select
                Case "+"c
                    num2 += 1
                    Exit Select
            End Select
            Do While (num2 < length)
                ch = InputStr.Chars(num2)
                Dim ch2 As Char = ch
                If (((ch2 = ChrW(9)) OrElse (ch2 = ChrW(10))) OrElse (((ch2 = ChrW(13)) OrElse (ch2 = " "c)) OrElse (ch2 = ChrW(12288)))) Then
                    num2 += 1
                Else
                    If (ch2 = "0"c) Then
                        If ((num4 <> 0) OrElse flag) Then
                            num7 = (((num7 * 10) + CDbl(ch)) - 48)
                            num2 += 1
                            num4 += 1
                        Else
                            num2 += 1
                        End If
                        Continue Do
                    End If
                    If ((ch2 >= "1"c) AndAlso (ch2 <= "9"c)) Then
                        num7 = (((num7 * 10) + CDbl(ch)) - 48)
                        num2 += 1
                        num4 += 1
                    Else
                        If (ch2 = "."c) Then
                            num2 += 1
                            If flag Then
                                Exit Do
                            End If
                            flag = True
                            num6 = num4
                            Continue Do
                        End If
                        If (((ch2 = "e"c) OrElse (ch2 = "E"c)) OrElse ((ch2 = "d"c) OrElse (ch2 = "D"c))) Then
                            flag2 = True
                            num2 += 1
                        End If
                        Exit Do
                    End If
                End If
            Loop
            If flag Then
                num5 = (num4 - num6)
            End If
            If Not flag2 Then
                If (flag AndAlso (num5 <> 0)) Then
                    num7 = (num7 / Math.Pow(10, CDbl(num5)))
                End If
            Else
                Dim flag4 As Boolean = False
                Dim flag5 As Boolean = False
                Do While (num2 < length)
                    ch = InputStr.Chars(num2)
                    Dim ch3 As Char = ch
                    If (((ch3 = ChrW(9)) OrElse (ch3 = ChrW(10))) OrElse (((ch3 = ChrW(13)) OrElse (ch3 = " "c)) OrElse (ch3 = ChrW(12288)))) Then
                        num2 += 1
                    ElseIf ((ch3 >= "0"c) AndAlso (ch3 <= "9"c)) Then
                        y = (((y * 10) + CDbl(ch)) - 48)
                        num2 += 1
                    Else
                        If (ch3 = "+"c) Then
                            If flag4 Then
                                Exit Do
                            End If
                            flag4 = True
                            num2 += 1
                            Continue Do
                        End If
                        If ((ch3 <> "-"c) OrElse flag4) Then
                            Exit Do
                        End If
                        flag4 = True
                        flag5 = True
                        num2 += 1
                    End If
                Loop
                If flag5 Then
                    y = (y + num5)
                    num7 = (num7 * Math.Pow(10, -y))
                Else
                    y = (y - num5)
                    num7 = (num7 * Math.Pow(10, y))
                End If
            End If
            If Double.IsInfinity(num7) Then
                Throw ExceptionUtils.VbMakeException(6)
            End If
            If flag3 Then
                num7 = -num7
            End If
            Select Case ch
                Case "&"c
                    If (num5 > 0) Then
                        Throw ExceptionUtils.VbMakeException(13)
                    End If
                    Return CInt(Math.Round(num7))
                Case "@"c
                    Return Convert.ToDouble(New Decimal(num7))
                Case "!"c
                    Return CSng(num7)
                Case "%"c
                    If (num5 > 0) Then
                        Throw ExceptionUtils.VbMakeException(13)
                    End If
                    Return CShort(Math.Round(num7))
            End Select
            Return num7
        End Function


        ' Fields
        Private Const LCID_US_ENGLISH As Integer = &H409
        Private Const LOCALE_NOUSEROVERRIDE As Integer = -2147483648
        Private Const MAX_ERR_NUMBER As Integer = &HFFFF
        Private Const NUMPRS_CURRENCY As Integer = &H400
        Private Const NUMPRS_DECIMAL As Integer = &H100
        Private Const NUMPRS_EXPONENT As Integer = &H800
        Private Const NUMPRS_HEX_OCT As Integer = &H40
        Private Const NUMPRS_INEXACT As Integer = &H20000
        Private Const NUMPRS_LEADING_MINUS As Integer = &H10
        Private Const NUMPRS_LEADING_PLUS As Integer = 4
        Private Const NUMPRS_LEADING_WHITE As Integer = 1
        Private Const NUMPRS_NEG As Integer = &H10000
        Private Const NUMPRS_PARENS As Integer = &H80
        Private Const NUMPRS_STD As Integer = &H1FFF
        Private Const NUMPRS_THOUSANDS As Integer = &H200
        Private Const NUMPRS_TRAILING_MINUS As Integer = &H20
        Private Const NUMPRS_TRAILING_PLUS As Integer = 8
        Private Const NUMPRS_TRAILING_WHITE As Integer = 2
        Private Const NUMPRS_USE_ALL As Integer = &H1000
        Private Const PRSFLAGS As Integer = &H954
        Private Const TYPE_INDICATOR_DECIMAL As Char = "@"c
        Private Const TYPE_INDICATOR_INT16 As Char = "%"c
        Private Const TYPE_INDICATOR_INT32 As Char = "&"c
        Private Const TYPE_INDICATOR_SINGLE As Char = "!"c
        Private Const VTBIT_BOOL As Integer = &H800
        Private Const VTBIT_BSTR As Integer = &H100
        Private Const VTBIT_BYTE As Integer = &H20000
        Private Const VTBIT_CHAR As Integer = &H40000
        Private Const VTBIT_CY As Integer = &H40
        Private Const VTBIT_DATAOBJECT As Integer = &H2000
        Private Const VTBIT_DATE As Integer = &H80
        Private Const VTBIT_DECIMAL As Integer = &H4000
        Private Const VTBIT_EMPTY As Integer = 0
        Private Const VTBIT_ERROR As Integer = &H400
        Private Const VTBIT_I2 As Integer = 4
        Private Const VTBIT_I4 As Integer = 8
        Private Const VTBIT_LONG As Integer = &H100000
        Private Const VTBIT_NULL As Integer = 2
        Private Const VTBIT_OBJECT As Integer = &H200
        Private Const VTBIT_R4 As Integer = &H10
        Private Const VTBIT_R8 As Integer = &H20
        Private Const VTBIT_VARIANT As Integer = &H1000
        Private Const VTBITS As Integer = &H402C
    End Class
End Namespace

