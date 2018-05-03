Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Reflection
Imports System.Threading
Imports Microsoft.VisualBasic.CompilerServices.Symbols

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute> _
    Public NotInheritable Class Operators
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function AddByte(Left As Byte, Right As Byte) As Object
            Dim num As Short = CShort((Left + Right))
            If (num > &HFF) Then
                Return num
            End If
            Return CByte(num)
        End Function

        Private Shared Function AddDecimal(Left As IConvertible, Right As IConvertible) As Object
            Dim num As Decimal = Left.ToDecimal(Nothing)
            Dim num2 As Decimal = Right.ToDecimal(Nothing)
            Try
                Return Decimal.Add(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToDouble(num) + Convert.ToDouble(num2))
            End Try
        End Function

        Private Shared Function AddDouble(Left As Double, Right As Double) As Object
            Return (Left + Right)
        End Function

        Private Shared Function AddInt16(Left As Short, Right As Short) As Object
            Dim num As Integer = (Left + Right)
            If ((num > &H7FFF) OrElse (num < -32768)) Then
                Return num
            End If
            Return CShort(num)
        End Function

        Private Shared Function AddInt32(Left As Integer, Right As Integer) As Object
            Dim num As Long = (Left + Right)
            If ((num > &H7FFFFFFF) OrElse (num < -2147483648)) Then
                Return num
            End If
            Return CInt(num)
        End Function

        Private Shared Function AddInt64(Left As Long, Right As Long) As Object
            Try
                Return (Left + Right)
            Catch exception As OverflowException
                Return Decimal.Add(New Decimal(Left), New Decimal(Right))
            End Try
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function AddObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            If (empty = TypeCode.Object) Then
                Dim chArray As Char() = TryCast(Left, Char())
                If ((Not chArray Is Nothing) AndAlso (((typeCode = TypeCode.String) OrElse (typeCode = TypeCode.Empty)) OrElse ((typeCode = TypeCode.Object) AndAlso TypeOf Right Is Char()))) Then
                    Left = New String(chArray)
                    conv = DirectCast(Left, IConvertible)
                    empty = TypeCode.String
                End If
            End If
            If (typeCode = TypeCode.Object) Then
                Dim chArray2 As Char() = TryCast(Right, Char())
                If ((Not chArray2 Is Nothing) AndAlso ((empty = TypeCode.String) OrElse (empty = TypeCode.Empty))) Then
                    Right = New String(chArray2)
                    convertible2 = DirectCast(Right, IConvertible)
                    typeCode = TypeCode.String
                End If
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.Boxed_ZeroInteger
                Case 3
                    Return Operators.AddInt16(0, Operators.ToVBBool(convertible2))
                Case 4
                    Return Operators.AddString(ChrW(0), convertible2.ToString(Nothing))
                Case 5
                    Return convertible2.ToSByte(Nothing)
                Case 6
                    Return convertible2.ToByte(Nothing)
                Case 7
                    Return convertible2.ToInt16(Nothing)
                Case 8
                    Return convertible2.ToUInt16(Nothing)
                Case 9
                    Return convertible2.ToInt32(Nothing)
                Case 10
                    Return convertible2.ToUInt32(Nothing)
                Case 11
                    Return convertible2.ToInt64(Nothing)
                Case 12
                    Return convertible2.ToUInt64(Nothing)
                Case 13, 14, 15, &H12, &H38
                    Return Right
                Case &H10
                    Return Operators.AddString(Conversions.ToString(DateTime.MinValue), Conversions.ToString(convertible2.ToDateTime(Nothing)))
                Case &H39
                    Return Operators.AddInt16(Operators.ToVBBool(conv), 0)
                Case 60
                    Return Operators.AddInt16(Operators.ToVBBool(conv), Operators.ToVBBool(convertible2))
                Case &H3E
                    Return Operators.AddSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing))
                Case &H3F, &H40
                    Return Operators.AddInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing))
                Case &H41, &H42
                    Return Operators.AddInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H43, &H44
                    Return Operators.AddInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case &H45, &H48
                    Return Operators.AddDecimal(Operators.ToVBBoolConv(conv), DirectCast(convertible2.ToDecimal(Nothing), IConvertible))
                Case 70
                    Return Operators.AddSingle(CSng(Operators.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H47
                    Return Operators.AddDouble(CDbl(Operators.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case &H4B
                    Return Operators.AddDouble(CDbl(Operators.ToVBBool(conv)), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H4C
                    Return Operators.AddString(conv.ToString(Nothing), ChrW(0))
                Case 80, &H5E, &H15A
                    Return Operators.AddString(conv.ToString(Nothing), convertible2.ToString(Nothing))
                Case &H5F
                    Return conv.ToSByte(Nothing)
                Case &H62
                    Return Operators.AddSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2))
                Case 100
                    Return Operators.AddSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B, 140
                    Return Operators.AddInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3, 180
                    Return Operators.AddInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H69, &H6A, &H7D, &H8F, &H90, &HA3, &HB5, &HB6, &HC3, &HC5, &HC7, &HC9, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220
                    Return Operators.AddInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H6B, 110, &H81, &H91, &H94, &HA7, &HB7, &HBA, &HCD, &HDD, &HE0, &HE9, &HEB, &HED, &HEF, &HF3, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, 300
                    Return Operators.AddDecimal(conv, convertible2)
                Case &H6C, &H7F, &H92, &HA5, &HB8, &HCB, &HDE, &HF1, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H106, &H12A
                    Return Operators.AddSingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H6D, &H80, &H93, &HA6, &HB9, &HCC, &HDF, &HF2, &H105, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, &H12B
                    Return Operators.AddDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.AddDouble(conv.ToDouble(Nothing), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H72
                    Return conv.ToByte(Nothing)
                Case &H75, &H88
                    Return Operators.AddInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2))
                Case 120
                    Return Operators.AddByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H7A, &H9E, 160
                    Return Operators.AddUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing))
                Case &H7C, &HA2, &HC4, &HC6, 200
                    Return Operators.AddUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing))
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE, 240
                    Return Operators.AddUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing))
                Case &H85
                    Return conv.ToInt16(Nothing)
                Case &H98
                    Return conv.ToUInt16(Nothing)
                Case &H9B, &HAE
                    Return Operators.AddInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2))
                Case &HAB
                    Return conv.ToInt32(Nothing)
                Case 190
                    Return conv.ToUInt32(Nothing)
                Case &HC1, &HD4
                    Return Operators.AddInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)))
                Case &HD1
                    Return conv.ToInt64(Nothing)
                Case &HE4
                    Return conv.ToUInt64(Nothing)
                Case &HE7, &H120
                    Return Operators.AddDecimal(conv, Operators.ToVBBoolConv(convertible2))
                Case &HF7, &H10A, &H11D, &H156, &H158
                    Return Left
                Case 250
                    Return Operators.AddSingle(conv.ToSingle(Nothing), CSng(Operators.ToVBBool(convertible2)))
                Case &H10D
                    Return Operators.AddDouble(conv.ToDouble(Nothing), CDbl(Operators.ToVBBool(convertible2)))
                Case &H130
                    Return Operators.AddString(Conversions.ToString(conv.ToDateTime(Nothing)), Conversions.ToString(DateTime.MinValue))
                Case 320
                    Return Operators.AddString(Conversions.ToString(conv.ToDateTime(Nothing)), Conversions.ToString(convertible2.ToDateTime(Nothing)))
                Case &H142
                    Return Operators.AddString(Conversions.ToString(conv.ToDateTime(Nothing)), convertible2.ToString(Nothing))
                Case &H159
                    Return Operators.AddDouble(Conversions.ToDouble(conv.ToString(Nothing)), CDbl(Operators.ToVBBool(convertible2)))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.AddDouble(Conversions.ToDouble(conv.ToString(Nothing)), convertible2.ToDouble(Nothing))
                Case &H166
                    Return Operators.AddString(conv.ToString(Nothing), Conversions.ToString(convertible2.ToDateTime(Nothing)))
                Case 360
                    Return Operators.AddString(conv.ToString(Nothing), convertible2.ToString(Nothing))
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Plus, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Plus, arguments)
        End Function

        Private Shared Function AddSByte(Left As SByte, Right As SByte) As Object
            Dim num As Short = CShort((Left + Right))
            If ((num > &H7F) OrElse (num < -128)) Then
                Return num
            End If
            Return CSByte(num)
        End Function

        Private Shared Function AddSingle(Left As Single, Right As Single) As Object
            Dim d As Double = (Left + Right)
            If ((d <= 3.4028234663852886E+38) AndAlso (d >= -3.4028234663852886E+38)) Then
                Return CSng(d)
            End If
            If (Double.IsInfinity(d) AndAlso (Single.IsInfinity(Left) OrElse Single.IsInfinity(Right))) Then
                Return CSng(d)
            End If
            Return d
        End Function

        Private Shared Function AddString(Left As String, Right As String) As Object
            Return (Left & Right)
        End Function

        Private Shared Function AddUInt16(Left As UInt16, Right As UInt16) As Object
            Dim num As Integer = (Left + Right)
            If (num > &HFFFF) Then
                Return num
            End If
            Return CUShort(num)
        End Function

        Private Shared Function AddUInt32(Left As UInt32, Right As UInt32) As Object
            Dim num As Long = (Left + Right)
            If (num > &HFFFFFFFF) Then
                Return num
            End If
            Return DirectCast(num, UInt32)
        End Function

        Private Shared Function AddUInt64(Left As UInt64, Right As UInt64) As Object
            Try
                Return (Left + Right)
            Catch exception As OverflowException
                Return Decimal.Add(New Decimal(Left), New Decimal(Right))
            End Try
        End Function

        Private Shared Function AndBoolean(Left As Boolean, Right As Boolean) As Object
            Return (Left And Right)
        End Function

        Private Shared Function AndByte(Left As Byte, Right As Byte, Optional EnumType As Type = Nothing) As Object
            Dim num As Byte = CByte((Left And Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function AndInt16(Left As Short, Right As Short, Optional EnumType As Type = Nothing) As Object
            Dim num As Short = CShort((Left And Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function AndInt32(Left As Integer, Right As Integer, Optional EnumType As Type = Nothing) As Object
            Dim num As Integer = (Left And Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function AndInt64(Left As Long, Right As Long, Optional EnumType As Type = Nothing) As Object
            Dim num As Long = (Left And Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function AndObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.Boxed_ZeroInteger
                Case 3, &H39
                    Return False
                Case 5, &H5F
                    Return Operators.AndSByte(0, 0, Operators.GetEnumResult(Left, Right))
                Case 6, &H72
                    Return Operators.AndByte(0, 0, Operators.GetEnumResult(Left, Right))
                Case 7, &H85
                    Return Operators.AndInt16(0, 0, Operators.GetEnumResult(Left, Right))
                Case 8, &H98
                    Return Operators.AndUInt16(0, 0, Operators.GetEnumResult(Left, Right))
                Case 9, &HAB
                    Return Operators.AndInt32(0, 0, Operators.GetEnumResult(Left, Right))
                Case 10, 190
                    Return Operators.AndUInt32(0, 0, Operators.GetEnumResult(Left, Right))
                Case 11, &HD1
                    Return Operators.AndInt64(0, 0, Operators.GetEnumResult(Left, Right))
                Case 12, &HE4
                    Return Operators.AndUInt64(0, 0, Operators.GetEnumResult(Left, Right))
                Case 13, 14, 15
                    Return Operators.AndInt64(0, convertible2.ToInt64(Nothing), Nothing)
                Case &H12
                    Return Operators.AndInt64(0, Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
                Case 60
                    Return Operators.AndBoolean(conv.ToBoolean(Nothing), convertible2.ToBoolean(Nothing))
                Case &H3E
                    Return Operators.AndSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing), Nothing)
                Case &H3F, &H40
                    Return Operators.AndInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing), Nothing)
                Case &H41, &H42
                    Return Operators.AndInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing), Nothing)
                Case &H43, &H44, &H45, 70, &H47, &H48
                    Return Operators.AndInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing), Nothing)
                Case &H4B
                    Return Operators.AndBoolean(conv.ToBoolean(Nothing), Conversions.ToBoolean(convertible2.ToString(Nothing)))
                Case &H62
                    Return Operators.AndSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 100
                    Return Operators.AndSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B
                    Return Operators.AndInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing), Nothing)
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3
                    Return Operators.AndInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing), Nothing)
                Case &H69, &H6A, &H6B, &H6C, &H6D, 110, &H7D, &H7F, &H80, &H81, &H8F, &H90, &H91, &H92, &H93, &H94, &HA3, &HA5, &HA6, &HA7, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HC3, &HC5, &HC7, &HC9, &HCB, &HCC, &HCD, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, &HDD, &HDE, &HDF, &HE0, &HE9, &HEB, &HED, &HEF, &HF1, &HF2, &HF3, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H105, &H106, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, &H12A, &H12B, 300
                    Return Operators.AndInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing), Nothing)
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.AndInt64(conv.ToInt64(Nothing), Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
                Case &H75, &H88
                    Return Operators.AndInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 120
                    Return Operators.AndByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H7A, &H9E
                    Return Operators.AndUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing), Nothing)
                Case &H7C, &HA2, &HC4, &HC6
                    Return Operators.AndUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing), Nothing)
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE
                    Return Operators.AndUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing), Nothing)
                Case 140
                    Return Operators.AndInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H9B, &HAE
                    Return Operators.AndInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 160
                    Return Operators.AndUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case 180
                    Return Operators.AndInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HC1, &HD4, &HE7, 250, &H10D, &H120
                    Return Operators.AndInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)), Nothing)
                Case 200
                    Return Operators.AndUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case 220
                    Return Operators.AndInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case 240
                    Return Operators.AndUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HF7, &H10A, &H11D
                    Return Operators.AndInt64(conv.ToInt64(Nothing), 0, Nothing)
                Case &H156
                    Return Operators.AndInt64(Conversions.ToLong(conv.ToString(Nothing)), 0, Nothing)
                Case &H159
                    Return Operators.AndBoolean(Conversions.ToBoolean(conv.ToString(Nothing)), convertible2.ToBoolean(Nothing))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.AndInt64(Conversions.ToLong(conv.ToString(Nothing)), convertible2.ToInt64(Nothing), Nothing)
                Case 360
                    Return Operators.AndInt64(Conversions.ToLong(conv.ToString(Nothing)), Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.And, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.And, arguments)
        End Function

        Private Shared Function AndSByte(Left As SByte, Right As SByte, Optional EnumType As Type = Nothing) As Object
            Dim num As SByte = CSByte((Left And Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function AndUInt16(Left As UInt16, Right As UInt16, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt16 = CUShort((Left And Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function AndUInt32(Left As UInt32, Right As UInt32, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt32 = (Left And Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function AndUInt64(Left As UInt64, Right As UInt64, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt64 = (Left And Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

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
                            num2 = Operators.MultipleAsteriskSkip(Pattern, Source, num2, CompareOption)
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

        Friend Shared Function CollectOperators(Op As UserDefinedOperator, Type1 As Type, Type2 As Type, ByRef FoundType1Operators As Boolean, ByRef FoundType2Operators As Boolean) As List(Of Method)
            Dim list As List(Of Method)
            Dim num As Integer
            Dim num2 As Integer
            If (Not Symbols.IsRootObjectType(Type1) AndAlso Symbols.IsClassOrValueType(Type1)) Then
                num = 0
                num2 = 0
                list = OverloadResolution.CollectOverloadCandidates(New Container(Type1).LookupNamedMembers(Symbols.OperatorCLSNames(CInt(Op))), Nothing, Interaction.IIf(Of Integer)(Symbols.IsUnaryOperator(Op), 1, 2), Nothing, Nothing, True, Nothing, num, num2, Nothing)
                If (list.Count > 0) Then
                    FoundType1Operators = True
                End If
            Else
                list = New List(Of Method)
            End If
            If (((Type2 IsNot Nothing) AndAlso Not Symbols.IsRootObjectType(Type2)) AndAlso Symbols.IsClassOrValueType(Type2)) Then
                Dim baseType As Type = Type1
                Do While (Not baseType Is Nothing)
                    If Symbols.IsOrInheritsFrom(Type2, baseType) Then
                        Exit Do
                    End If
                    baseType = baseType.BaseType
                Loop
                num2 = 0
                num = 0
                Dim collection As List(Of Method) = OverloadResolution.CollectOverloadCandidates(New Container(Type2).LookupNamedMembers(Symbols.OperatorCLSNames(CInt(Op))), Nothing, Interaction.IIf(Of Integer)(Symbols.IsUnaryOperator(Op), 1, 2), Nothing, Nothing, True, baseType, num2, num, Nothing)
                If (collection.Count > 0) Then
                    FoundType2Operators = True
                End If
                list.AddRange(collection)
            End If
            Return list
        End Function

        Private Shared Function CompareBoolean(Left As Boolean, Right As Boolean) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left < Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Private Shared Function CompareChar(Left As Char, Right As Char) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Private Shared Function CompareDate(Left As DateTime, Right As DateTime) As CompareClass
            Dim num As Integer = DateTime.Compare(Left, Right)
            If (num = 0) Then
                Return CompareClass.Equal
            End If
            If (num > 0) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Private Shared Function CompareDecimal(Left As IConvertible, Right As IConvertible) As CompareClass
            Dim num As Integer = Decimal.Compare(Left.ToDecimal(Nothing), Right.ToDecimal(Nothing))
            If (num = 0) Then
                Return CompareClass.Equal
            End If
            If (num > 0) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Private Shared Function CompareDouble(Left As Double, Right As Double) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left < Right) Then
                Return CompareClass.Less
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Unordered
        End Function

        Private Shared Function CompareInt32(Left As Integer, Right As Integer) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Private Shared Function CompareInt64(Left As Long, Right As Long) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Public Shared Function CompareObject(Left As Object, Right As Object, TextCompare As Boolean) As Integer
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return 0
                Case CompareClass.UserDefined, CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.IsTrue, Left, Right)
            End Select
            Return CInt(class2)
        End Function

        Private Shared Function CompareObject2(Left As Object, Right As Object, TextCompare As Boolean) As CompareClass
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim right As IConvertible = TryCast(right, IConvertible)
            If (right Is Nothing) Then
                If (right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = right.GetTypeCode
            End If
            If (empty = TypeCode.Object) Then
                Dim chArray As Char() = TryCast(Left, Char())
                If ((Not chArray Is Nothing) AndAlso (((typeCode = TypeCode.String) OrElse (typeCode = TypeCode.Empty)) OrElse ((typeCode = TypeCode.Object) AndAlso TypeOf right Is Char()))) Then
                    Left = New String(chArray)
                    conv = DirectCast(Left, IConvertible)
                    empty = TypeCode.String
                End If
            End If
            If (typeCode = TypeCode.Object) Then
                Dim chArray2 As Char() = TryCast(right, Char())
                If ((Not chArray2 Is Nothing) AndAlso ((empty = TypeCode.String) OrElse (empty = TypeCode.Empty))) Then
                    right = New String(chArray2)
                    right = DirectCast(right, IConvertible)
                    typeCode = TypeCode.String
                End If
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return CompareClass.Equal
                Case 3
                    Return Operators.CompareBoolean(False, right.ToBoolean(Nothing))
                Case 4
                    Return Operators.CompareChar(ChrW(0), right.ToChar(Nothing))
                Case 5
                    Return Operators.CompareInt32(0, right.ToSByte(Nothing))
                Case 6
                    Return Operators.CompareInt32(0, right.ToByte(Nothing))
                Case 7
                    Return Operators.CompareInt32(0, right.ToInt16(Nothing))
                Case 8
                    Return Operators.CompareInt32(0, right.ToUInt16(Nothing))
                Case 9
                    Return Operators.CompareInt32(0, right.ToInt32(Nothing))
                Case 10
                    Return Operators.CompareUInt32(0, right.ToUInt32(Nothing))
                Case 11
                    Return Operators.CompareInt64(0, right.ToInt64(Nothing))
                Case 12
                    Return Operators.CompareUInt64(0, right.ToUInt64(Nothing))
                Case 13
                    Return Operators.CompareSingle(0!, right.ToSingle(Nothing))
                Case 14
                    Return Operators.CompareDouble(0, right.ToDouble(Nothing))
                Case 15
                    Return Operators.CompareDecimal(DirectCast(Decimal.Zero, IConvertible), right)
                Case &H10
                    Return Operators.CompareDate(DateTime.MinValue, right.ToDateTime(Nothing))
                Case &H12
                    Return DirectCast(Operators.CompareString(Nothing, right.ToString(Nothing), TextCompare), CompareClass)
                Case &H39
                    Return Operators.CompareBoolean(conv.ToBoolean(Nothing), False)
                Case 60
                    Return Operators.CompareBoolean(conv.ToBoolean(Nothing), right.ToBoolean(Nothing))
                Case &H3E
                    Return Operators.CompareInt32(Operators.ToVBBool(conv), right.ToSByte(Nothing))
                Case &H3F, &H40
                    Return Operators.CompareInt32(Operators.ToVBBool(conv), right.ToInt16(Nothing))
                Case &H41, &H42
                    Return Operators.CompareInt32(Operators.ToVBBool(conv), right.ToInt32(Nothing))
                Case &H43, &H44
                    Return Operators.CompareInt64(CLng(Operators.ToVBBool(conv)), right.ToInt64(Nothing))
                Case &H45, &H48
                    Return Operators.CompareDecimal(Operators.ToVBBoolConv(conv), right)
                Case 70
                    Return Operators.CompareSingle(CSng(Operators.ToVBBool(conv)), right.ToSingle(Nothing))
                Case &H47
                    Return Operators.CompareDouble(CDbl(Operators.ToVBBool(conv)), right.ToDouble(Nothing))
                Case &H4B
                    Return Operators.CompareBoolean(conv.ToBoolean(Nothing), Conversions.ToBoolean(right.ToString(Nothing)))
                Case &H4C
                    Return Operators.CompareChar(conv.ToChar(Nothing), ChrW(0))
                Case 80
                    Return Operators.CompareChar(conv.ToChar(Nothing), right.ToChar(Nothing))
                Case &H5E, &H15A, 360
                    Return DirectCast(Operators.CompareString(conv.ToString(Nothing), right.ToString(Nothing), TextCompare), CompareClass)
                Case &H5F
                    Return Operators.CompareInt32(conv.ToSByte(Nothing), 0)
                Case &H62
                    Return Operators.CompareInt32(conv.ToSByte(Nothing), Operators.ToVBBool(right))
                Case 100
                    Return Operators.CompareInt32(conv.ToSByte(Nothing), right.ToSByte(Nothing))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B, 140
                    Return Operators.CompareInt32(conv.ToInt16(Nothing), right.ToInt16(Nothing))
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3, 180
                    Return Operators.CompareInt32(conv.ToInt32(Nothing), right.ToInt32(Nothing))
                Case &H69, &H6A, &H7D, &H8F, &H90, &HA3, &HB5, &HB6, &HC3, &HC5, &HC7, &HC9, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220
                    Return Operators.CompareInt64(conv.ToInt64(Nothing), right.ToInt64(Nothing))
                Case &H6B, 110, &H81, &H91, &H94, &HA7, &HB7, &HBA, &HCD, &HDD, &HE0, &HE9, &HEB, &HED, &HEF, &HF3, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, 300
                    Return Operators.CompareDecimal(conv, right)
                Case &H6C, &H7F, &H92, &HA5, &HB8, &HCB, &HDE, &HF1, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H106, &H12A
                    Return Operators.CompareSingle(conv.ToSingle(Nothing), right.ToSingle(Nothing))
                Case &H6D, &H80, &H93, &HA6, &HB9, &HCC, &HDF, &HF2, &H105, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, &H12B
                    Return Operators.CompareDouble(conv.ToDouble(Nothing), right.ToDouble(Nothing))
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.CompareDouble(conv.ToDouble(Nothing), Conversions.ToDouble(right.ToString(Nothing)))
                Case &H72
                    Return Operators.CompareInt32(conv.ToByte(Nothing), 0)
                Case &H75
                    Return Operators.CompareInt32(conv.ToInt16(Nothing), Operators.ToVBBool(right))
                Case 120
                    Return Operators.CompareInt32(conv.ToByte(Nothing), right.ToByte(Nothing))
                Case &H7A, &H9E, 160
                    Return Operators.CompareInt32(conv.ToUInt16(Nothing), right.ToUInt16(Nothing))
                Case &H7C, &HA2, &HC4, &HC6, 200
                    Return Operators.CompareUInt32(conv.ToUInt32(Nothing), right.ToUInt32(Nothing))
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE, 240
                    Return Operators.CompareUInt64(conv.ToUInt64(Nothing), right.ToUInt64(Nothing))
                Case &H85
                    Return Operators.CompareInt32(conv.ToInt16(Nothing), 0)
                Case &H88
                    Return Operators.CompareInt32(conv.ToInt16(Nothing), Operators.ToVBBool(right))
                Case &H98
                    Return Operators.CompareInt32(conv.ToUInt16(Nothing), 0)
                Case &H9B
                    Return Operators.CompareInt32(conv.ToInt32(Nothing), Operators.ToVBBool(right))
                Case &HAB
                    Return Operators.CompareInt32(conv.ToInt32(Nothing), 0)
                Case &HAE
                    Return Operators.CompareInt32(conv.ToInt32(Nothing), Operators.ToVBBool(right))
                Case 190
                    Return Operators.CompareUInt32(conv.ToUInt32(Nothing), 0)
                Case &HC1
                    Return Operators.CompareInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(right)))
                Case &HD1
                    Return Operators.CompareInt64(conv.ToInt64(Nothing), 0)
                Case &HD4
                    Return Operators.CompareInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(right)))
                Case &HE4
                    Return Operators.CompareUInt64(conv.ToUInt64(Nothing), 0)
                Case &HE7
                    Return Operators.CompareDecimal(conv, Operators.ToVBBoolConv(right))
                Case &HF7
                    Return Operators.CompareSingle(conv.ToSingle(Nothing), 0!)
                Case 250
                    Return Operators.CompareSingle(conv.ToSingle(Nothing), CSng(Operators.ToVBBool(right)))
                Case &H10A
                    Return Operators.CompareDouble(conv.ToDouble(Nothing), 0)
                Case &H10D
                    Return Operators.CompareDouble(conv.ToDouble(Nothing), CDbl(Operators.ToVBBool(right)))
                Case &H11D
                    Return Operators.CompareDecimal(conv, DirectCast(Decimal.Zero, IConvertible))
                Case &H120
                    Return Operators.CompareDecimal(conv, Operators.ToVBBoolConv(right))
                Case &H130
                    Return Operators.CompareDate(conv.ToDateTime(Nothing), DateTime.MinValue)
                Case 320
                    Return Operators.CompareDate(conv.ToDateTime(Nothing), right.ToDateTime(Nothing))
                Case &H142
                    Return Operators.CompareDate(conv.ToDateTime(Nothing), Conversions.ToDate(right.ToString(Nothing)))
                Case &H156
                    Return DirectCast(Operators.CompareString(conv.ToString(Nothing), Nothing, TextCompare), CompareClass)
                Case &H159
                    Return Operators.CompareBoolean(Conversions.ToBoolean(conv.ToString(Nothing)), right.ToBoolean(Nothing))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.CompareDouble(Conversions.ToDouble(conv.ToString(Nothing)), right.ToDouble(Nothing))
                Case &H166
                    Return Operators.CompareDate(Conversions.ToDate(conv.ToString(Nothing)), right.ToDateTime(Nothing))
            End Select
            If ((empty = TypeCode.Object) OrElse (typeCode = TypeCode.Object)) Then
                Return CompareClass.UserDefined
            End If
            Return CompareClass.Undefined
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareObjectEqual(Left As Object, Right As Object, TextCompare As Boolean) As Object
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Equal, arguments)
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Equal, Left, Right)
            End Select
            Return (class2 = CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareObjectGreater(Left As Object, Right As Object, TextCompare As Boolean) As Object
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Greater, arguments)
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Greater, Left, Right)
            End Select
            Return (class2 > CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareObjectGreaterEqual(Left As Object, Right As Object, TextCompare As Boolean) As Object
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.GreaterEqual, arguments)
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.GreaterEqual, Left, Right)
            End Select
            Return (class2 >= CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareObjectLess(Left As Object, Right As Object, TextCompare As Boolean) As Object
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Less, arguments)
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Less, Left, Right)
            End Select
            Return (class2 < CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareObjectLessEqual(Left As Object, Right As Object, TextCompare As Boolean) As Object
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.LessEqual, arguments)
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.LessEqual, Left, Right)
            End Select
            Return (class2 <= CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareObjectNotEqual(Left As Object, Right As Object, TextCompare As Boolean) As Object
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return True
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.NotEqual, arguments)
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.NotEqual, Left, Right)
            End Select
            Return (class2 > CompareClass.Equal)
        End Function

        Private Shared Function CompareSingle(Left As Single, Right As Single) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left < Right) Then
                Return CompareClass.Less
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Unordered
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function CompareString(Left As String, Right As String, TextCompare As Boolean) As Integer
            Dim num2 As Integer
            If (Left Is Right) Then
                Return 0
            End If
            If (Left Is Nothing) Then
                If (Right.Length = 0) Then
                    Return 0
                End If
                Return -1
            End If
            If (Right Is Nothing) Then
                If (Left.Length = 0) Then
                    Return 0
                End If
                Return 1
            End If
            If TextCompare Then
                num2 = Utils.GetCultureInfo.CompareInfo.Compare(Left, Right, (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase)))
            Else
                num2 = String.CompareOrdinal(Left, Right)
            End If
            If (num2 = 0) Then
                Return 0
            End If
            If (num2 > 0) Then
                Return 1
            End If
            Return -1
        End Function

        Private Shared Function CompareUInt32(Left As UInt32, Right As UInt32) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        Private Shared Function CompareUInt64(Left As UInt64, Right As UInt64) As CompareClass
            If (Left = Right) Then
                Return CompareClass.Equal
            End If
            If (Left > Right) Then
                Return CompareClass.Greater
            End If
            Return CompareClass.Less
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConcatenateObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(Left, IConvertible)
            If (convertible Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            If ((empty = TypeCode.Object) AndAlso TypeOf Left Is Char()) Then
                empty = TypeCode.String
            End If
            If ((typeCode = TypeCode.Object) AndAlso TypeOf Right Is Char()) Then
                typeCode = TypeCode.String
            End If
            If ((empty = TypeCode.Object) OrElse (typeCode = TypeCode.Object)) Then
                Dim arguments As Object() = New Object() {Left, Right}
                Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Concatenate, arguments)
            End If
            Dim flag As Boolean = (empty = TypeCode.DBNull)
            Dim flag2 As Boolean = (typeCode = TypeCode.DBNull)
            If (flag And flag2) Then
                Return Left
            End If
            If (flag And Not flag2) Then
                Left = ""
            ElseIf (flag2 And Not flag) Then
                Right = ""
            End If
            Return (Conversions.ToString(Left) & Conversions.ToString(Right))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConditionalCompareObjectEqual(Left As Object, Right As Object, TextCompare As Boolean) As Boolean
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(UserDefinedOperator.Equal, arguments))
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Equal, Left, Right)
            End Select
            Return (class2 = CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConditionalCompareObjectGreater(Left As Object, Right As Object, TextCompare As Boolean) As Boolean
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(UserDefinedOperator.Greater, arguments))
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Greater, Left, Right)
            End Select
            Return (class2 > CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConditionalCompareObjectGreaterEqual(Left As Object, Right As Object, TextCompare As Boolean) As Boolean
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(UserDefinedOperator.GreaterEqual, arguments))
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.GreaterEqual, Left, Right)
            End Select
            Return (class2 >= CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConditionalCompareObjectLess(Left As Object, Right As Object, TextCompare As Boolean) As Boolean
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(UserDefinedOperator.Less, arguments))
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Less, Left, Right)
            End Select
            Return (class2 < CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConditionalCompareObjectLessEqual(Left As Object, Right As Object, TextCompare As Boolean) As Boolean
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return False
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(UserDefinedOperator.LessEqual, arguments))
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.LessEqual, Left, Right)
            End Select
            Return (class2 <= CompareClass.Equal)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ConditionalCompareObjectNotEqual(Left As Object, Right As Object, TextCompare As Boolean) As Boolean
            Dim class2 As CompareClass = Operators.CompareObject2(Left, Right, TextCompare)
            Select Case class2
                Case CompareClass.Unordered
                    Return True
                Case CompareClass.UserDefined
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(UserDefinedOperator.NotEqual, arguments))
                Case CompareClass.Undefined
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.NotEqual, Left, Right)
            End Select
            Return (class2 > CompareClass.Equal)
        End Function

        Private Shared Function DivideDecimal(Left As IConvertible, Right As IConvertible) As Object
            Dim num As Decimal = Left.ToDecimal(Nothing)
            Dim num2 As Decimal = Right.ToDecimal(Nothing)
            Try
                Return Decimal.Divide(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToSingle(num) / Convert.ToSingle(num2))
            End Try
        End Function

        Private Shared Function DivideDouble(Left As Double, Right As Double) As Object
            Return (Left / Right)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function DivideObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.DivideDouble(0, 0)
                Case 3
                    Return Operators.DivideDouble(0, CDbl(Operators.ToVBBool(convertible2)))
                Case 5, 6, 7, 8, 9, 10, 11, 12, 14
                    Return Operators.DivideDouble(0, convertible2.ToDouble(Nothing))
                Case 13
                    Return Operators.DivideSingle(0!, convertible2.ToSingle(Nothing))
                Case 15
                    Return Operators.DivideDecimal(DirectCast(Decimal.Zero, IConvertible), convertible2)
                Case &H12
                    Return Operators.DivideDouble(0, Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H39
                    Return Operators.DivideDouble(CDbl(Operators.ToVBBool(conv)), 0)
                Case 60
                    Return Operators.DivideDouble(CDbl(Operators.ToVBBool(conv)), CDbl(Operators.ToVBBool(convertible2)))
                Case &H3E, &H3F, &H40, &H41, &H42, &H43, &H44, &H45, &H47
                    Return Operators.DivideDouble(CDbl(Operators.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case 70
                    Return Operators.DivideSingle(CSng(Operators.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H48
                    Return Operators.DivideDecimal(Operators.ToVBBoolConv(conv), convertible2)
                Case &H4B
                    Return Operators.DivideDouble(CDbl(Operators.ToVBBool(conv)), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H5F, &H72, &H85, &H98, &HAB, 190, &HD1, &HE4, &H10A
                    Return Operators.DivideDouble(conv.ToDouble(Nothing), 0)
                Case &H62, &H75, &H88, &H9B, &HAE, &HC1, &HD4, &HE7, &H10D
                    Return Operators.DivideDouble(conv.ToDouble(Nothing), CDbl(Operators.ToVBBool(convertible2)))
                Case 100, &H65, &H66, &H67, &H68, &H69, &H6A, &H6B, &H6D, &H77, 120, &H79, &H7A, &H7B, &H7C, &H7D, &H7E, &H80, &H8A, &H8B, 140, &H8D, &H8E, &H8F, &H90, &H91, &H93, &H9D, &H9E, &H9F, 160, &HA1, &HA2, &HA3, &HA4, &HA6, &HB0, &HB1, &HB2, &HB3, 180, &HB5, &HB6, &HB7, &HB9, &HC3, &HC4, &HC5, &HC6, &HC7, 200, &HC9, &HCA, &HCC, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220, &HDD, &HDF, &HE9, &HEA, &HEB, &HEC, &HED, &HEE, &HEF, 240, &HF2, &H105, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, &H12B
                    Return Operators.DivideDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H6C, &H7F, &H92, &HA5, &HB8, &HCB, &HDE, &HF1, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H106, &H12A
                    Return Operators.DivideSingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case 110, &H81, &H94, &HA7, &HBA, &HCD, &HE0, &HF3, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, 300
                    Return Operators.DivideDecimal(conv, convertible2)
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.DivideDouble(conv.ToDouble(Nothing), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &HF7
                    Return Operators.DivideSingle(conv.ToSingle(Nothing), 0!)
                Case 250
                    Return Operators.DivideSingle(conv.ToSingle(Nothing), CSng(Operators.ToVBBool(convertible2)))
                Case &H11D
                    Return Operators.DivideDecimal(conv, DirectCast(Decimal.Zero, IConvertible))
                Case &H120
                    Return Operators.DivideDecimal(conv, Operators.ToVBBoolConv(convertible2))
                Case &H156
                    Return Operators.DivideDouble(Conversions.ToDouble(conv.ToString(Nothing)), 0)
                Case &H159
                    Return Operators.DivideDouble(Conversions.ToDouble(conv.ToString(Nothing)), CDbl(Operators.ToVBBool(convertible2)))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.DivideDouble(Conversions.ToDouble(conv.ToString(Nothing)), convertible2.ToDouble(Nothing))
                Case 360
                    Return Operators.DivideDouble(Conversions.ToDouble(conv.ToString(Nothing)), Conversions.ToDouble(convertible2.ToString(Nothing)))
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Divide, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Divide, arguments)
        End Function

        Private Shared Function DivideSingle(Left As Single, Right As Single) As Object
            Dim f As Single = (Left / Right)
            If Not Single.IsInfinity(f) Then
                Return f
            End If
            If (Single.IsInfinity(Left) OrElse Single.IsInfinity(Right)) Then
                Return f
            End If
            Return (CDbl(Left) / CDbl(Right))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ExponentObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim num As Double
            Dim num2 As Double
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    num = 0
                    Exit Select
                Case TypeCode.Object
                    Dim arguments As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Power, arguments)
                Case TypeCode.Boolean
                    num = Operators.ToVBBool(conv)
                    Exit Select
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    num = conv.ToDouble(Nothing)
                    Exit Select
                Case TypeCode.String
                    num = Conversions.ToDouble(conv.ToString(Nothing))
                    Exit Select
                Case Else
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Power, Left, Right)
            End Select
            Select Case typeCode
                Case TypeCode.Empty
                    num2 = 0
                    Exit Select
                Case TypeCode.Object
                    Dim objArray2 As Object() = New Object() {Left, Right}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Power, objArray2)
                Case TypeCode.Boolean
                    num2 = Operators.ToVBBool(convertible2)
                    Exit Select
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    num2 = convertible2.ToDouble(Nothing)
                    Exit Select
                Case TypeCode.String
                    num2 = Conversions.ToDouble(convertible2.ToString(Nothing))
                    Exit Select
                Case Else
                    Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Power, Left, Right)
            End Select
            Return Math.Pow(num, num2)
        End Function

        <Obsolete("do not use this method", True), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function FallbackInvokeUserDefinedOperator(vbOp As Object, Arguments As Object()) As Object
            Return Operators.InvokeObjectUserDefinedOperator(DirectCast(Conversions.ToSByte(vbOp), UserDefinedOperator), Arguments)
        End Function

        Friend Shared Function GetCallableUserDefinedOperator(Op As UserDefinedOperator, ParamArray Arguments As Object()) As Method
            Dim targetProcedure As Method = Operators.ResolveUserDefinedOperator(Op, Arguments, False)
            If (((Not targetProcedure Is Nothing) AndAlso Not targetProcedure.ArgumentsValidated) AndAlso Not OverloadResolution.CanMatchArguments(targetProcedure, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, False, Nothing)) Then
                Return Nothing
            End If
            Return targetProcedure
        End Function

        Private Shared Function GetEnumResult(Left As Object, Right As Object) As Type
            If (Not Left Is Nothing) Then
                If TypeOf Left Is [Enum] Then
                    If (Right Is Nothing) Then
                        Return Left.GetType
                    End If
                    If TypeOf Right Is [Enum] Then
                        Dim type As Type = Left.GetType
                        If (type Is Right.GetType) Then
                            Return type
                        End If
                    End If
                End If
            ElseIf TypeOf Right Is [Enum] Then
                Return Right.GetType
            End If
            Return Nothing
        End Function

        Private Shared Function GetNoValidOperatorException(Op As UserDefinedOperator, Operand As Object) As Exception
            Dim args As String() = New String() {Symbols.OperatorNames(CInt(Op)), Utils.VBFriendlyName(Operand)}
            Return New InvalidCastException(Utils.GetResourceString("UnaryOperand2", args))
        End Function

        Private Shared Function GetNoValidOperatorException(Op As UserDefinedOperator, Left As Object, Right As Object) As Exception
            Dim resourceString As String
            Dim str2 As String
            If (Left Is Nothing) Then
                resourceString = "'Nothing'"
            Else
                Dim str3 As String = TryCast(Left, String)
                If (Not str3 Is Nothing) Then
                    Dim textArray1 As String() = New String() {Strings.Left(str3, &H20)}
                    resourceString = Utils.GetResourceString("NoValidOperator_StringType1", textArray1)
                Else
                    Dim textArray2 As String() = New String() {Utils.VBFriendlyName(Left)}
                    resourceString = Utils.GetResourceString("NoValidOperator_NonStringType1", textArray2)
                End If
            End If
            If (Right Is Nothing) Then
                str2 = "'Nothing'"
            Else
                Dim str As String = TryCast(Right, String)
                If (Not str Is Nothing) Then
                    Dim textArray3 As String() = New String() {Strings.Left(str, &H20)}
                    str2 = Utils.GetResourceString("NoValidOperator_StringType1", textArray3)
                Else
                    Dim textArray4 As String() = New String() {Utils.VBFriendlyName(Right)}
                    str2 = Utils.GetResourceString("NoValidOperator_NonStringType1", textArray4)
                End If
            End If
            Dim args As String() = New String() {Symbols.OperatorNames(CInt(Op)), resourceString, str2}
            Return New InvalidCastException(Utils.GetResourceString("BinaryOperands3", args))
        End Function

        Private Shared Function IntDivideByte(Left As Byte, Right As Byte) As Object
            Return CByte((Left / Right))
        End Function

        Private Shared Function IntDivideInt16(Left As Short, Right As Short) As Object
            If ((Left = -32768) AndAlso (Right = -1)) Then
                Return &H8000
            End If
            Return CShort((Left / Right))
        End Function

        Private Shared Function IntDivideInt32(Left As Integer, Right As Integer) As Object
            If ((Left = -2147483648) AndAlso (Right = -1)) Then
                Return CLng(&H80000000)
            End If
            Return (Left / Right)
        End Function

        Private Shared Function IntDivideInt64(Left As Long, Right As Long) As Object
            Return (Left / Right)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function IntDivideObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.IntDivideInt32(0, 0)
                Case 3
                    Return Operators.IntDivideInt16(0, Operators.ToVBBool(convertible2))
                Case 5
                    Return Operators.IntDivideSByte(0, convertible2.ToSByte(Nothing))
                Case 6
                    Return Operators.IntDivideByte(0, convertible2.ToByte(Nothing))
                Case 7
                    Return Operators.IntDivideInt16(0, convertible2.ToInt16(Nothing))
                Case 8
                    Return Operators.IntDivideUInt16(0, convertible2.ToUInt16(Nothing))
                Case 9
                    Return Operators.IntDivideInt32(0, convertible2.ToInt32(Nothing))
                Case 10
                    Return Operators.IntDivideUInt32(0, convertible2.ToUInt32(Nothing))
                Case 11
                    Return Operators.IntDivideInt64(0, convertible2.ToInt64(Nothing))
                Case 12
                    Return Operators.IntDivideUInt64(0, convertible2.ToUInt64(Nothing))
                Case 13, 14, 15
                    Return Operators.IntDivideInt64(0, convertible2.ToInt64(Nothing))
                Case &H12
                    Return Operators.IntDivideInt64(0, Conversions.ToLong(convertible2.ToString(Nothing)))
                Case &H39
                    Return Operators.IntDivideInt16(Operators.ToVBBool(conv), 0)
                Case 60
                    Return Operators.IntDivideInt16(Operators.ToVBBool(conv), Operators.ToVBBool(convertible2))
                Case &H3E
                    Return Operators.IntDivideSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing))
                Case &H3F, &H40
                    Return Operators.IntDivideInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing))
                Case &H41, &H42
                    Return Operators.IntDivideInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H43, &H44, &H45, 70, &H47, &H48
                    Return Operators.IntDivideInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case &H4B
                    Return Operators.IntDivideInt64(CLng(Operators.ToVBBool(conv)), Conversions.ToLong(convertible2.ToString(Nothing)))
                Case &H5F
                    Return Operators.IntDivideSByte(conv.ToSByte(Nothing), 0)
                Case &H62
                    Return Operators.IntDivideSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2))
                Case 100
                    Return Operators.IntDivideSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B, 140
                    Return Operators.IntDivideInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3, 180
                    Return Operators.IntDivideInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H69, &H6A, &H6B, &H6C, &H6D, 110, &H7D, &H7F, &H80, &H81, &H8F, &H90, &H91, &H92, &H93, &H94, &HA3, &HA5, &HA6, &HA7, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HC3, &HC5, &HC7, &HC9, &HCB, &HCC, &HCD, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220, &HDD, &HDE, &HDF, &HE0, &HE9, &HEB, &HED, &HEF, &HF1, &HF2, &HF3, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H105, &H106, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, &H12A, &H12B, 300
                    Return Operators.IntDivideInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.IntDivideInt64(conv.ToInt64(Nothing), Conversions.ToLong(convertible2.ToString(Nothing)))
                Case &H72
                    Return Operators.IntDivideByte(conv.ToByte(Nothing), 0)
                Case &H75, &H88
                    Return Operators.IntDivideInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2))
                Case 120
                    Return Operators.IntDivideByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H7A, &H9E, 160
                    Return Operators.IntDivideUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing))
                Case &H7C, &HA2, &HC4, &HC6, 200
                    Return Operators.IntDivideUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing))
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE, 240
                    Return Operators.IntDivideUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing))
                Case &H85
                    Return Operators.IntDivideInt16(conv.ToInt16(Nothing), 0)
                Case &H98
                    Return Operators.IntDivideUInt16(conv.ToUInt16(Nothing), 0)
                Case &H9B, &HAE
                    Return Operators.IntDivideInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2))
                Case &HAB
                    Return Operators.IntDivideInt32(conv.ToInt32(Nothing), 0)
                Case 190
                    Return Operators.IntDivideUInt32(conv.ToUInt32(Nothing), 0)
                Case &HC1, &HD4, &HE7, 250, &H10D, &H120
                    Return Operators.IntDivideInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)))
                Case &HD1
                    Return Operators.IntDivideInt64(conv.ToInt64(Nothing), 0)
                Case &HE4
                    Return Operators.IntDivideUInt64(conv.ToUInt64(Nothing), 0)
                Case &HF7, &H10A, &H11D
                    Return Operators.IntDivideInt64(conv.ToInt64(Nothing), 0)
                Case &H156
                    Return Operators.IntDivideInt64(Conversions.ToLong(conv.ToString(Nothing)), 0)
                Case &H159
                    Return Operators.IntDivideInt64(Conversions.ToLong(conv.ToString(Nothing)), CLng(Operators.ToVBBool(convertible2)))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.IntDivideInt64(Conversions.ToLong(conv.ToString(Nothing)), convertible2.ToInt64(Nothing))
                Case 360
                    Return Operators.IntDivideInt64(Conversions.ToLong(conv.ToString(Nothing)), Conversions.ToLong(convertible2.ToString(Nothing)))
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.IntegralDivide, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.IntegralDivide, arguments)
        End Function

        Private Shared Function IntDivideSByte(Left As SByte, Right As SByte) As Object
            If ((Left = -128) AndAlso (Right = -1)) Then
                Return CShort(&H80)
            End If
            Return CSByte((Left / Right))
        End Function

        Private Shared Function IntDivideUInt16(Left As UInt16, Right As UInt16) As Object
            Return CUShort((Left / Right))
        End Function

        Private Shared Function IntDivideUInt32(Left As UInt32, Right As UInt32) As Object
            Return (Left / Right)
        End Function

        Private Shared Function IntDivideUInt64(Left As UInt64, Right As UInt64) As Object
            Return (Left / Right)
        End Function

        Friend Shared Function InvokeObjectUserDefinedOperator(Op As UserDefinedOperator, Arguments As Object()) As Object
            Dim operatorMethod As Method = Operators.ResolveUserDefinedOperator(Op, Arguments, True)
            If (Not operatorMethod Is Nothing) Then
                Return Operators.InvokeUserDefinedOperator(operatorMethod, False, Arguments)
            End If
            If (Arguments.Length > 1) Then
                Throw Operators.GetNoValidOperatorException(Op, Arguments(0), Arguments(1))
            End If
            Throw Operators.GetNoValidOperatorException(Op, Arguments(0))
        End Function

        Friend Shared Function InvokeUserDefinedOperator(Op As UserDefinedOperator, ParamArray Arguments As Object()) As Object
            If (Not IDOUtils.TryCastToIDMOP(Arguments(0)) Is Nothing) Then
                Return IDOBinder.InvokeUserDefinedOperator(Op, Arguments)
            End If
            Return Operators.InvokeObjectUserDefinedOperator(Op, Arguments)
        End Function

        Friend Shared Function InvokeUserDefinedOperator(OperatorMethod As Method, ForceArgumentValidation As Boolean, ParamArray Arguments As Object()) As Object
            If ((Not OperatorMethod.ArgumentsValidated OrElse ForceArgumentValidation) AndAlso Not OverloadResolution.CanMatchArguments(OperatorMethod, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, False, Nothing)) Then
                Dim str As String = ""
                Dim errors As New List(Of String)
                OverloadResolution.CanMatchArguments(OperatorMethod, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, False, errors)
                Dim str2 As String
                For Each str2 In errors
                    str = (str & ChrW(13) & ChrW(10) & "    " & str2)
                Next
                Dim args As String() = New String() {OperatorMethod.ToString, str}
                Throw New InvalidCastException(Utils.GetResourceString("MatchArgumentFailure2", args))
            End If
            Return New Container(OperatorMethod.DeclaringType).InvokeMethod(OperatorMethod, Arguments, Nothing, BindingFlags.InvokeMethod)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function LeftShiftObject(Operand As Object, Amount As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(Operand, IConvertible)
            If (convertible Is Nothing) Then
                If (Operand Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Amount, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Amount Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            If ((empty = TypeCode.Object) OrElse (typeCode = TypeCode.Object)) Then
                Dim arguments As Object() = New Object() {Operand, Amount}
                Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.ShiftLeft, arguments)
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return (CInt(0) << Conversions.ToInteger(Amount))
                Case TypeCode.Boolean
                    Return CShort((CShort(-(convertible.ToBoolean(Nothing) > 0)) << (Conversions.ToInteger(Amount) And 15)))
                Case TypeCode.SByte
                    Return CSByte((convertible.ToSByte(Nothing) << (Conversions.ToInteger(Amount) And 7)))
                Case TypeCode.Byte
                    Return CByte((convertible.ToByte(Nothing) << (Conversions.ToInteger(Amount) And 7)))
                Case TypeCode.Int16
                    Return CShort((convertible.ToInt16(Nothing) << (Conversions.ToInteger(Amount) And 15)))
                Case TypeCode.UInt16
                    Return CUShort((convertible.ToUInt16(Nothing) << (Conversions.ToInteger(Amount) And 15)))
                Case TypeCode.Int32
                    Return (convertible.ToInt32(Nothing) << Conversions.ToInteger(Amount))
                Case TypeCode.UInt32
                    Return (convertible.ToUInt32(Nothing) << Conversions.ToInteger(Amount))
                Case TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return (convertible.ToInt64(Nothing) << Conversions.ToInteger(Amount))
                Case TypeCode.UInt64
                    Return (convertible.ToUInt64(Nothing) << Conversions.ToInteger(Amount))
                Case TypeCode.String
                    Return (Conversions.ToLong(convertible.ToString(Nothing)) << Conversions.ToInteger(Amount))
            End Select
            Throw Operators.GetNoValidOperatorException(UserDefinedOperator.ShiftLeft, Operand)
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
            Return Operators.LikeString(Conversions.ToString(Source), Conversions.ToString(Pattern), CompareOption)
        End Function

        Public Shared Function LikeString(Source As String, Pattern As String, CompareOption As CompareMethod) As Boolean
            If (CompareOption = CompareMethod.Binary) Then
                Return Operators.LikeStringBinary(Source, Pattern)
            End If
            Return Operators.LikeStringText(Source, Pattern)
        End Function

        Private Shared Function LikeStringBinary(Source As String, Pattern As String) As Boolean
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
                    Dim num5 As Integer = Operators.AsteriskSkip(Pattern.Substring((num2 + 1)), Source.Substring(num), (num3 - num), CompareMethod.Binary, Strings.m_InvariantCompareInfo)
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
                                        flag4 = Operators.LikeStringCompareBinary(flag6, flag4, p, ch2)
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

        Private Shared Function LikeStringCompare(ci As CompareInfo, SeenNot As Boolean, Match As Boolean, p As Char, s As Char, Options As CompareOptions) As Boolean
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

        Private Shared Function LikeStringCompareBinary(SeenNot As Boolean, Match As Boolean, p As Char, s As Char) As Boolean
            If (SeenNot AndAlso Match) Then
                Return (p <> s)
            End If
            If (Not SeenNot AndAlso Not Match) Then
                Return (p = s)
            End If
            Return Match
        End Function

        Private Shared Function LikeStringText(Source As String, Pattern As String) As Boolean
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
                    Dim num5 As Integer = Operators.AsteriskSkip(Pattern.Substring((num2 + 1)), Source.Substring(num), (num3 - num), CompareMethod.Text, compareInfo)
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
                                        flag4 = Operators.LikeStringCompare(compareInfo, flag6, flag4, p, ch2, options)
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

        Private Shared Function ModByte(Left As Byte, Right As Byte) As Object
            Return CByte((Left Mod Right))
        End Function

        Private Shared Function ModDecimal(Left As IConvertible, Right As IConvertible) As Object
            Return Decimal.Remainder(Left.ToDecimal(Nothing), Right.ToDecimal(Nothing))
        End Function

        Private Shared Function ModDouble(Left As Double, Right As Double) As Object
            Return (Left Mod Right)
        End Function

        Private Shared Function ModInt16(Left As Short, Right As Short) As Object
            Dim num As Integer = (Left Mod Right)
            If ((num < -32768) OrElse (num > &H7FFF)) Then
                Return num
            End If
            Return CShort(num)
        End Function

        Private Shared Function ModInt32(Left As Integer, Right As Integer) As Object
            Dim num As Long = (CLng(Left) Mod CLng(Right))
            If ((num < -2147483648) OrElse (num > &H7FFFFFFF)) Then
                Return num
            End If
            Return CInt(num)
        End Function

        Private Shared Function ModInt64(Left As Long, Right As Long) As Object
            If ((Left = -9223372036854775808) AndAlso (Right = -1)) Then
                Return 0
            End If
            Return (Left Mod Right)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ModObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.ModInt32(0, 0)
                Case 3
                    Return Operators.ModInt16(0, Operators.ToVBBool(convertible2))
                Case 5
                    Return Operators.ModSByte(0, convertible2.ToSByte(Nothing))
                Case 6
                    Return Operators.ModByte(0, convertible2.ToByte(Nothing))
                Case 7
                    Return Operators.ModInt16(0, convertible2.ToInt16(Nothing))
                Case 8
                    Return Operators.ModUInt16(0, convertible2.ToUInt16(Nothing))
                Case 9
                    Return Operators.ModInt32(0, convertible2.ToInt32(Nothing))
                Case 10
                    Return Operators.ModUInt32(0, convertible2.ToUInt32(Nothing))
                Case 11
                    Return Operators.ModInt64(0, convertible2.ToInt64(Nothing))
                Case 12
                    Return Operators.ModUInt64(0, convertible2.ToUInt64(Nothing))
                Case 13
                    Return Operators.ModSingle(0!, convertible2.ToSingle(Nothing))
                Case 14
                    Return Operators.ModDouble(0, convertible2.ToDouble(Nothing))
                Case 15
                    Return Operators.ModDecimal(DirectCast(Decimal.Zero, IConvertible), DirectCast(convertible2.ToDecimal(Nothing), IConvertible))
                Case &H12
                    Return Operators.ModDouble(0, Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H39
                    Return Operators.ModInt16(Operators.ToVBBool(conv), 0)
                Case 60
                    Return Operators.ModInt16(Operators.ToVBBool(conv), Operators.ToVBBool(convertible2))
                Case &H3E
                    Return Operators.ModSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing))
                Case &H3F, &H40
                    Return Operators.ModInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing))
                Case &H41, &H42
                    Return Operators.ModInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H43, &H44
                    Return Operators.ModInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case &H45, &H48
                    Return Operators.ModDecimal(Operators.ToVBBoolConv(conv), DirectCast(convertible2.ToDecimal(Nothing), IConvertible))
                Case 70
                    Return Operators.ModSingle(CSng(Operators.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H47
                    Return Operators.ModDouble(CDbl(Operators.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case &H4B
                    Return Operators.ModDouble(CDbl(Operators.ToVBBool(conv)), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H5F
                    Return Operators.ModSByte(conv.ToSByte(Nothing), 0)
                Case &H62
                    Return Operators.ModSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2))
                Case 100
                    Return Operators.ModSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B, 140
                    Return Operators.ModInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3, 180
                    Return Operators.ModInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H69, &H6A, &H7D, &H8F, &H90, &HA3, &HB5, &HB6, &HC3, &HC5, &HC7, &HC9, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220
                    Return Operators.ModInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H6B, 110, &H81, &H91, &H94, &HA7, &HB7, &HBA, &HCD, &HDD, &HE0, &HE9, &HEB, &HED, &HEF, &HF3, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, 300
                    Return Operators.ModDecimal(conv, convertible2)
                Case &H6C, &H7F, &H92, &HA5, &HB8, &HCB, &HDE, &HF1, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H106, &H12A
                    Return Operators.ModSingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H6D, &H80, &H93, &HA6, &HB9, &HCC, &HDF, &HF2, &H105, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, &H12B
                    Return Operators.ModDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.ModDouble(conv.ToDouble(Nothing), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H72
                    Return Operators.ModByte(conv.ToByte(Nothing), 0)
                Case &H75, &H88
                    Return Operators.ModInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2))
                Case 120
                    Return Operators.ModByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H7A, &H9E, 160
                    Return Operators.ModUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing))
                Case &H7C, &HA2, &HC4, &HC6, 200
                    Return Operators.ModUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing))
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE, 240
                    Return Operators.ModUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing))
                Case &H85
                    Return Operators.ModInt16(conv.ToInt16(Nothing), 0)
                Case &H98
                    Return Operators.ModUInt16(conv.ToUInt16(Nothing), 0)
                Case &H9B, &HAE
                    Return Operators.ModInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2))
                Case &HAB
                    Return Operators.ModInt32(conv.ToInt32(Nothing), 0)
                Case 190
                    Return Operators.ModUInt32(conv.ToUInt32(Nothing), 0)
                Case &HC1, &HD4
                    Return Operators.ModInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)))
                Case &HD1
                    Return Operators.ModInt64(conv.ToInt64(Nothing), 0)
                Case &HE4
                    Return Operators.ModUInt64(conv.ToUInt64(Nothing), 0)
                Case &HE7, &H120
                    Return Operators.ModDecimal(conv, Operators.ToVBBoolConv(convertible2))
                Case &HF7
                    Return Operators.ModSingle(conv.ToSingle(Nothing), 0!)
                Case 250
                    Return Operators.ModSingle(conv.ToSingle(Nothing), CSng(Operators.ToVBBool(convertible2)))
                Case &H10A
                    Return Operators.ModDouble(conv.ToDouble(Nothing), 0)
                Case &H10D
                    Return Operators.ModDouble(conv.ToDouble(Nothing), CDbl(Operators.ToVBBool(convertible2)))
                Case &H11D
                    Return Operators.ModDecimal(conv, DirectCast(Decimal.Zero, IConvertible))
                Case &H156
                    Return Operators.ModDouble(Conversions.ToDouble(conv.ToString(Nothing)), 0)
                Case &H159
                    Return Operators.ModDouble(Conversions.ToDouble(conv.ToString(Nothing)), CDbl(Operators.ToVBBool(convertible2)))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.ModDouble(Conversions.ToDouble(conv.ToString(Nothing)), convertible2.ToDouble(Nothing))
                Case 360
                    Return Operators.ModDouble(Conversions.ToDouble(conv.ToString(Nothing)), Conversions.ToDouble(convertible2.ToString(Nothing)))
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Modulus, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Modulus, arguments)
        End Function

        Private Shared Function ModSByte(Left As SByte, Right As SByte) As Object
            Return CSByte((Left Mod Right))
        End Function

        Private Shared Function ModSingle(Left As Single, Right As Single) As Object
            Return (Left Mod Right)
        End Function

        Private Shared Function ModUInt16(Left As UInt16, Right As UInt16) As Object
            Return CUShort((Left Mod Right))
        End Function

        Private Shared Function ModUInt32(Left As UInt32, Right As UInt32) As Object
            Return (Left Mod Right)
        End Function

        Private Shared Function ModUInt64(Left As UInt64, Right As UInt64) As Object
            Return (Left Mod Right)
        End Function

        Private Shared Function MultipleAsteriskSkip(Pattern As String, Source As String, Count As Integer, CompareOption As CompareMethod) As Integer
            Dim num As Integer = Strings.Len(Source)
            Do While (Count < num)
                Dim flag As Boolean
                Dim source As String = source.Substring((num - Count))
                Try
                    flag = Operators.LikeString(source, Pattern, CompareOption)
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

        Private Shared Function MultiplyByte(Left As Byte, Right As Byte) As Object
            Dim num As Integer = (Left * Right)
            If (num > &HFF) Then
                If (num > &H7FFF) Then
                    Return num
                End If
                Return CShort(num)
            End If
            Return CByte(num)
        End Function

        Private Shared Function MultiplyDecimal(Left As IConvertible, Right As IConvertible) As Object
            Dim num As Decimal = Left.ToDecimal(Nothing)
            Dim num2 As Decimal = Right.ToDecimal(Nothing)
            Try
                Return Decimal.Multiply(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToDouble(num) * Convert.ToDouble(num2))
            End Try
        End Function

        Private Shared Function MultiplyDouble(Left As Double, Right As Double) As Object
            Return (Left * Right)
        End Function

        Private Shared Function MultiplyInt16(Left As Short, Right As Short) As Object
            Dim num As Integer = (Left * Right)
            If ((num > &H7FFF) OrElse (num < -32768)) Then
                Return num
            End If
            Return CShort(num)
        End Function

        Private Shared Function MultiplyInt32(Left As Integer, Right As Integer) As Object
            Dim num As Long = (Left * Right)
            If ((num > &H7FFFFFFF) OrElse (num < -2147483648)) Then
                Return num
            End If
            Return CInt(num)
        End Function

        Private Shared Function MultiplyInt64(Left As Long, Right As Long) As Object
            Try
                Return (Left * Right)
            Catch exception As OverflowException
            End Try
            Try
                Return Decimal.Multiply(New Decimal(Left), New Decimal(Right))
            Catch exception2 As OverflowException
                Return (Left * Right)
            End Try
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function MultiplyObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0, 9, &HAB
                    Return Operators.Boxed_ZeroInteger
                Case 3, 7, &H39, &H85
                    Return Operators.Boxed_ZeroShort
                Case 5, &H5F
                    Return Operators.Boxed_ZeroSByte
                Case 6, &H72
                    Return Operators.Boxed_ZeroByte
                Case 8, &H98
                    Return Operators.Boxed_ZeroUShort
                Case 10, 190
                    Return Operators.Boxed_ZeroUInteger
                Case 11, &HD1
                    Return Operators.Boxed_ZeroLong
                Case 12, &HE4
                    Return Operators.Boxed_ZeroULong
                Case 13, &HF7
                    Return Operators.Boxed_ZeroSinge
                Case 14, &H10A
                    Return Operators.Boxed_ZeroDouble
                Case 15, &H11D
                    Return Operators.Boxed_ZeroDecimal
                Case &H12
                    Return Operators.MultiplyDouble(0, Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case 60
                    Return Operators.MultiplyInt16(Operators.ToVBBool(conv), Operators.ToVBBool(convertible2))
                Case &H3E
                    Return Operators.MultiplySByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing))
                Case &H3F, &H40
                    Return Operators.MultiplyInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing))
                Case &H41, &H42
                    Return Operators.MultiplyInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H43, &H44
                    Return Operators.MultiplyInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case &H45, &H48
                    Return Operators.MultiplyDecimal(Operators.ToVBBoolConv(conv), DirectCast(convertible2.ToDecimal(Nothing), IConvertible))
                Case 70
                    Return Operators.MultiplySingle(CSng(Operators.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H47
                    Return Operators.MultiplyDouble(CDbl(Operators.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case &H4B
                    Return Operators.MultiplyDouble(CDbl(Operators.ToVBBool(conv)), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H62
                    Return Operators.MultiplySByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2))
                Case 100
                    Return Operators.MultiplySByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B, 140
                    Return Operators.MultiplyInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3, 180
                    Return Operators.MultiplyInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H69, &H6A, &H7D, &H8F, &H90, &HA3, &HB5, &HB6, &HC3, &HC5, &HC7, &HC9, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220
                    Return Operators.MultiplyInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H6B, 110, &H81, &H91, &H94, &HA7, &HB7, &HBA, &HCD, &HDD, &HE0, &HE9, &HEB, &HED, &HEF, &HF3, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, 300
                    Return Operators.MultiplyDecimal(conv, convertible2)
                Case &H6C, &H7F, &H92, &HA5, &HB8, &HCB, &HDE, &HF1, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H106, &H12A
                    Return Operators.MultiplySingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H6D, &H80, &H93, &HA6, &HB9, &HCC, &HDF, &HF2, &H105, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, &H12B
                    Return Operators.MultiplyDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.MultiplyDouble(conv.ToDouble(Nothing), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H75, &H88
                    Return Operators.MultiplyInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2))
                Case 120
                    Return Operators.MultiplyByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H7A, &H9E, 160
                    Return Operators.MultiplyUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing))
                Case &H7C, &HA2, &HC4, &HC6, 200
                    Return Operators.MultiplyUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing))
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE, 240
                    Return Operators.MultiplyUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing))
                Case &H9B, &HAE
                    Return Operators.MultiplyInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2))
                Case &HC1, &HD4
                    Return Operators.MultiplyInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)))
                Case &HE7, &H120
                    Return Operators.MultiplyDecimal(conv, Operators.ToVBBoolConv(convertible2))
                Case 250
                    Return Operators.MultiplySingle(conv.ToSingle(Nothing), CSng(Operators.ToVBBool(convertible2)))
                Case &H10D
                    Return Operators.MultiplyDouble(conv.ToDouble(Nothing), CDbl(Operators.ToVBBool(convertible2)))
                Case &H156
                    Return Operators.MultiplyDouble(Conversions.ToDouble(conv.ToString(Nothing)), 0)
                Case &H159
                    Return Operators.MultiplyDouble(Conversions.ToDouble(conv.ToString(Nothing)), CDbl(Operators.ToVBBool(convertible2)))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.MultiplyDouble(Conversions.ToDouble(conv.ToString(Nothing)), convertible2.ToDouble(Nothing))
                Case 360
                    Return Operators.MultiplyDouble(Conversions.ToDouble(conv.ToString(Nothing)), Conversions.ToDouble(convertible2.ToString(Nothing)))
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Multiply, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Multiply, arguments)
        End Function

        Private Shared Function MultiplySByte(Left As SByte, Right As SByte) As Object
            Dim num As Short = CShort((Left * Right))
            If ((num > &H7F) OrElse (num < -128)) Then
                Return num
            End If
            Return CSByte(num)
        End Function

        Private Shared Function MultiplySingle(Left As Single, Right As Single) As Object
            Dim d As Double = (Left * Right)
            If ((d <= 3.4028234663852886E+38) AndAlso (d >= -3.4028234663852886E+38)) Then
                Return CSng(d)
            End If
            If (Double.IsInfinity(d) AndAlso (Single.IsInfinity(Left) OrElse Single.IsInfinity(Right))) Then
                Return CSng(d)
            End If
            Return d
        End Function

        Private Shared Function MultiplyUInt16(Left As UInt16, Right As UInt16) As Object
            Dim num As Long = (Left * Right)
            If (num > &HFFFF) Then
                If (num > &H7FFFFFFF) Then
                    Return num
                End If
                Return CInt(num)
            End If
            Return CUShort(num)
        End Function

        Private Shared Function MultiplyUInt32(Left As UInt32, Right As UInt32) As Object
            Dim num As UInt64 = (Left * Right)
            If (num > &HFFFFFFFF) Then
                If (Decimal.Compare(New Decimal(num), 9223372036854775807) > 0) Then
                    Return New Decimal(num)
                End If
                Return CLng(num)
            End If
            Return DirectCast(num, UInt32)
        End Function

        Private Shared Function MultiplyUInt64(Left As UInt64, Right As UInt64) As Object
            Try
                Return (Left * Right)
            Catch exception As OverflowException
            End Try
            Try
                Return Decimal.Multiply(New Decimal(Left), New Decimal(Right))
            Catch exception2 As OverflowException
                Return (Left * Right)
            End Try
        End Function

        Private Shared Function NegateBoolean(Operand As Boolean) As Object
            Return -CShort(-(Operand > False))
        End Function

        Private Shared Function NegateByte(Operand As Byte) As Object
            Return CShort(-Operand)
        End Function

        Private Shared Function NegateDecimal(Operand As Decimal) As Object
            Try
                Return Decimal.Negate(Operand)
            Catch exception As OverflowException
                Return -Convert.ToDouble(Operand)
            End Try
        End Function

        Private Shared Function NegateDouble(Operand As Double) As Object
            Return -Operand
        End Function

        Private Shared Function NegateInt16(Operand As Short) As Object
            If (Operand = -32768) Then
                Return &H8000
            End If
            Return -Operand
        End Function

        Private Shared Function NegateInt32(Operand As Integer) As Object
            If (Operand = -2147483648) Then
                Return CLng(&H80000000)
            End If
            Return (0 - Operand)
        End Function

        Private Shared Function NegateInt64(Operand As Long) As Object
            If (Operand = -9223372036854775808) Then
                Return 9223372036854775808
            End If
            Return (0 - Operand)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function NegateObject(Operand As Object) As Object
            Dim empty As TypeCode
            Dim convertible As IConvertible = TryCast(Operand, IConvertible)
            If (convertible Is Nothing) Then
                If (Operand Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return Operators.Boxed_ZeroInteger
                Case TypeCode.Object
                    Dim arguments As Object() = New Object() {Operand}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Negate, arguments)
                Case TypeCode.Boolean
                    If TypeOf Operand Is Boolean Then
                        Return Operators.NegateBoolean(CBool(Operand))
                    End If
                    Return Operators.NegateBoolean(convertible.ToBoolean(Nothing))
                Case TypeCode.SByte
                    If TypeOf Operand Is SByte Then
                        Return Operators.NegateSByte(CSByte(Operand))
                    End If
                    Return Operators.NegateSByte(convertible.ToSByte(Nothing))
                Case TypeCode.Byte
                    If TypeOf Operand Is Byte Then
                        Return Operators.NegateByte(CByte(Operand))
                    End If
                    Return Operators.NegateByte(convertible.ToByte(Nothing))
                Case TypeCode.Int16
                    If TypeOf Operand Is Short Then
                        Return Operators.NegateInt16(CShort(Operand))
                    End If
                    Return Operators.NegateInt16(convertible.ToInt16(Nothing))
                Case TypeCode.UInt16
                    If TypeOf Operand Is UInt16 Then
                        Return Operators.NegateUInt16(CUShort(Operand))
                    End If
                    Return Operators.NegateUInt16(convertible.ToUInt16(Nothing))
                Case TypeCode.Int32
                    If TypeOf Operand Is Integer Then
                        Return Operators.NegateInt32(CInt(Operand))
                    End If
                    Return Operators.NegateInt32(convertible.ToInt32(Nothing))
                Case TypeCode.UInt32
                    If TypeOf Operand Is UInt32 Then
                        Return Operators.NegateUInt32(DirectCast(Operand, UInt32))
                    End If
                    Return Operators.NegateUInt32(convertible.ToUInt32(Nothing))
                Case TypeCode.Int64
                    If TypeOf Operand Is Long Then
                        Return Operators.NegateInt64(CLng(Operand))
                    End If
                    Return Operators.NegateInt64(convertible.ToInt64(Nothing))
                Case TypeCode.UInt64
                    If TypeOf Operand Is UInt64 Then
                        Return Operators.NegateUInt64(CULng(Operand))
                    End If
                    Return Operators.NegateUInt64(convertible.ToUInt64(Nothing))
                Case TypeCode.Single
                    If TypeOf Operand Is Single Then
                        Return Operators.NegateSingle(CSng(Operand))
                    End If
                    Return Operators.NegateSingle(convertible.ToSingle(Nothing))
                Case TypeCode.Double
                    If TypeOf Operand Is Double Then
                        Return Operators.NegateDouble(CDbl(Operand))
                    End If
                    Return Operators.NegateDouble(convertible.ToDouble(Nothing))
                Case TypeCode.Decimal
                    If TypeOf Operand Is Decimal Then
                        Return Operators.NegateDecimal(CDec(Operand))
                    End If
                    Return Operators.NegateDecimal(convertible.ToDecimal(Nothing))
                Case TypeCode.String
                    Dim operand As String = TryCast(operand, String)
                    If (operand Is Nothing) Then
                        Return Operators.NegateString(convertible.ToString(Nothing))
                    End If
                    Return Operators.NegateString(operand)
            End Select
            Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Negate, Operand)
        End Function

        Private Shared Function NegateSByte(Operand As SByte) As Object
            If (Operand = -128) Then
                Return CShort(&H80)
            End If
            Return -Operand
        End Function

        Private Shared Function NegateSingle(Operand As Single) As Object
            Return -Operand
        End Function

        Private Shared Function NegateString(Operand As String) As Object
            Return -Conversions.ToDouble(Operand)
        End Function

        Private Shared Function NegateUInt16(Operand As UInt16) As Object
            Return (0 - Operand)
        End Function

        Private Shared Function NegateUInt32(Operand As UInt32) As Object
            Return (0 - Operand)
        End Function

        Private Shared Function NegateUInt64(Operand As UInt64) As Object
            Return Decimal.Negate(New Decimal(Operand))
        End Function

        Private Shared Function NotBoolean(Operand As Boolean) As Object
            Return Not Operand
        End Function

        Private Shared Function NotByte(Operand As Byte, OperandType As Type) As Object
            Dim num As Byte = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function NotInt16(Operand As Short, OperandType As Type) As Object
            Dim num As Short = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function NotInt32(Operand As Integer, OperandType As Type) As Object
            Dim num As Integer = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function NotInt64(Operand As Long) As Object
            Return Not Operand
        End Function

        Private Shared Function NotInt64(Operand As Long, OperandType As Type) As Object
            Dim num As Long = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function NotObject(Operand As Object) As Object
            Dim empty As TypeCode
            Dim convertible As IConvertible = TryCast(Operand, IConvertible)
            If (convertible Is Nothing) Then
                If (Operand Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return -1
                Case TypeCode.Object
                    Dim arguments As Object() = New Object() {Operand}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Not, arguments)
                Case TypeCode.Boolean
                    Return Operators.NotBoolean(convertible.ToBoolean(Nothing))
                Case TypeCode.SByte
                    Return Operators.NotSByte(convertible.ToSByte(Nothing), Operand.GetType)
                Case TypeCode.Byte
                    Return Operators.NotByte(convertible.ToByte(Nothing), Operand.GetType)
                Case TypeCode.Int16
                    Return Operators.NotInt16(convertible.ToInt16(Nothing), Operand.GetType)
                Case TypeCode.UInt16
                    Return Operators.NotUInt16(convertible.ToUInt16(Nothing), Operand.GetType)
                Case TypeCode.Int32
                    Return Operators.NotInt32(convertible.ToInt32(Nothing), Operand.GetType)
                Case TypeCode.UInt32
                    Return Operators.NotUInt32(convertible.ToUInt32(Nothing), Operand.GetType)
                Case TypeCode.Int64
                    Return Operators.NotInt64(convertible.ToInt64(Nothing), Operand.GetType)
                Case TypeCode.UInt64
                    Return Operators.NotUInt64(convertible.ToUInt64(Nothing), Operand.GetType)
                Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return Operators.NotInt64(convertible.ToInt64(Nothing))
                Case TypeCode.String
                    Return Operators.NotInt64(Conversions.ToLong(convertible.ToString(Nothing)))
            End Select
            Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Not, Operand)
        End Function

        Private Shared Function NotSByte(Operand As SByte, OperandType As Type) As Object
            Dim num As SByte = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function NotUInt16(Operand As UInt16, OperandType As Type) As Object
            Dim num As UInt16 = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function NotUInt32(Operand As UInt32, OperandType As Type) As Object
            Dim num As UInt32 = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function NotUInt64(Operand As UInt64, OperandType As Type) As Object
            Dim num As UInt64 = Not Operand
            If OperandType.IsEnum Then
                Return [Enum].ToObject(OperandType, num)
            End If
            Return num
        End Function

        Private Shared Function OrBoolean(Left As Boolean, Right As Boolean) As Object
            Return (Left Or Right)
        End Function

        Private Shared Function OrByte(Left As Byte, Right As Byte, Optional EnumType As Type = Nothing) As Object
            Dim num As Byte = CByte((Left Or Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function OrInt16(Left As Short, Right As Short, Optional EnumType As Type = Nothing) As Object
            Dim num As Short = CShort((Left Or Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function OrInt32(Left As Integer, Right As Integer, Optional EnumType As Type = Nothing) As Object
            Dim num As Integer = (Left Or Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function OrInt64(Left As Long, Right As Long, Optional EnumType As Type = Nothing) As Object
            Dim num As Long = (Left Or Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function OrObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.Boxed_ZeroInteger
                Case 3
                    Return Operators.OrBoolean(False, convertible2.ToBoolean(Nothing))
                Case 5, 6, 7, 8, 9, 10, 11, 12
                    Return Right
                Case 13, 14, 15
                    Return Operators.OrInt64(0, convertible2.ToInt64(Nothing), Nothing)
                Case &H12
                    Return Operators.OrInt64(0, Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
                Case &H39
                    Return Operators.OrBoolean(conv.ToBoolean(Nothing), False)
                Case 60
                    Return Operators.OrBoolean(conv.ToBoolean(Nothing), convertible2.ToBoolean(Nothing))
                Case &H3E
                    Return Operators.OrSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing), Nothing)
                Case &H3F, &H40
                    Return Operators.OrInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing), Nothing)
                Case &H41, &H42
                    Return Operators.OrInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing), Nothing)
                Case &H43, &H44, &H45, 70, &H47, &H48
                    Return Operators.OrInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing), Nothing)
                Case &H4B
                    Return Operators.OrBoolean(conv.ToBoolean(Nothing), Conversions.ToBoolean(convertible2.ToString(Nothing)))
                Case &H5F, &H72, &H85, &H98, &HAB, 190, &HD1, &HE4
                    Return Left
                Case &H62
                    Return Operators.OrSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 100
                    Return Operators.OrSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B
                    Return Operators.OrInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing), Nothing)
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3
                    Return Operators.OrInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing), Nothing)
                Case &H69, &H6A, &H6B, &H6C, &H6D, 110, &H7D, &H7F, &H80, &H81, &H8F, &H90, &H91, &H92, &H93, &H94, &HA3, &HA5, &HA6, &HA7, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HC3, &HC5, &HC7, &HC9, &HCB, &HCC, &HCD, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, &HDD, &HDE, &HDF, &HE0, &HE9, &HEB, &HED, &HEF, &HF1, &HF2, &HF3, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H105, &H106, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, &H12A, &H12B, 300
                    Return Operators.OrInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing), Nothing)
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.OrInt64(conv.ToInt64(Nothing), Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
                Case &H75, &H88
                    Return Operators.OrInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 120
                    Return Operators.OrByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H7A, &H9E
                    Return Operators.OrUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing), Nothing)
                Case &H7C, &HA2, &HC4, &HC6
                    Return Operators.OrUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing), Nothing)
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE
                    Return Operators.OrUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing), Nothing)
                Case 140
                    Return Operators.OrInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H9B, &HAE
                    Return Operators.OrInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 160
                    Return Operators.OrUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case 180
                    Return Operators.OrInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HC1, &HD4, &HE7, 250, &H10D, &H120
                    Return Operators.OrInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)), Nothing)
                Case 200
                    Return Operators.OrUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case 220
                    Return Operators.OrInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case 240
                    Return Operators.OrUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HF7, &H10A, &H11D
                    Return Operators.OrInt64(conv.ToInt64(Nothing), 0, Nothing)
                Case &H156
                    Return Operators.OrInt64(Conversions.ToLong(conv.ToString(Nothing)), 0, Nothing)
                Case &H159
                    Return Operators.OrBoolean(Conversions.ToBoolean(conv.ToString(Nothing)), convertible2.ToBoolean(Nothing))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.OrInt64(Conversions.ToLong(conv.ToString(Nothing)), convertible2.ToInt64(Nothing), Nothing)
                Case 360
                    Return Operators.OrInt64(Conversions.ToLong(conv.ToString(Nothing)), Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Or, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Or, arguments)
        End Function

        Private Shared Function OrSByte(Left As SByte, Right As SByte, Optional EnumType As Type = Nothing) As Object
            Dim num As SByte = CSByte((Left Or Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function OrUInt16(Left As UInt16, Right As UInt16, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt16 = CUShort((Left Or Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function OrUInt32(Left As UInt32, Right As UInt32, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt32 = (Left Or Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function OrUInt64(Left As UInt64, Right As UInt64, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt64 = (Left Or Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function PlusObject(Operand As Object) As Object
            Dim empty As TypeCode
            If (Operand Is Nothing) Then
                Return Operators.Boxed_ZeroInteger
            End If
            Dim convertible As IConvertible = TryCast(Operand, IConvertible)
            If (convertible Is Nothing) Then
                If (Operand Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return Operators.Boxed_ZeroInteger
                Case TypeCode.Object
                    Dim arguments As Object() = New Object() {Operand}
                    Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.UnaryPlus, arguments)
                Case TypeCode.Boolean
                    Return CShort(-(convertible.ToBoolean(Nothing) > False))
                Case TypeCode.SByte
                    Return convertible.ToSByte(Nothing)
                Case TypeCode.Byte
                    Return convertible.ToByte(Nothing)
                Case TypeCode.Int16
                    Return convertible.ToInt16(Nothing)
                Case TypeCode.UInt16
                    Return convertible.ToUInt16(Nothing)
                Case TypeCode.Int32
                    Return convertible.ToInt32(Nothing)
                Case TypeCode.UInt32
                    Return convertible.ToUInt32(Nothing)
                Case TypeCode.Int64
                    Return convertible.ToInt64(Nothing)
                Case TypeCode.UInt64
                    Return convertible.ToUInt64(Nothing)
                Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return Operand
                Case TypeCode.String
                    Return Conversions.ToDouble(convertible.ToString(Nothing))
            End Select
            Throw Operators.GetNoValidOperatorException(UserDefinedOperator.UnaryPlus, Operand)
        End Function

        Friend Shared Function ResolveUserDefinedOperator(Op As UserDefinedOperator, Arguments As Object(), ReportErrors As Boolean) As Method
            Dim type As Type
            Dim flag As Boolean
            Dim flag2 As Boolean
            Arguments = DirectCast(Arguments.Clone, Object())
            Dim type2 As Type = Nothing
            If (Arguments(0) Is Nothing) Then
                type2 = Arguments(1).GetType
                type = type2
                Arguments(0) = New TypedNothing(type)
            Else
                type = Arguments(0).GetType
                If (Arguments.Length > 1) Then
                    If (Not Arguments(1) Is Nothing) Then
                        type2 = Arguments(1).GetType
                    Else
                        type2 = type
                        Arguments(1) = New TypedNothing(type2)
                    End If
                End If
            End If
            Dim candidates As List(Of Method) = Operators.CollectOperators(Op, type, type2, flag, flag2)
            If (candidates.Count > 0) Then
                Dim failure As ResolutionFailure
                Return OverloadResolution.ResolveOverloadedCall(Symbols.OperatorNames(CInt(Op)), candidates, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, BindingFlags.InvokeMethod, ReportErrors, failure)
            End If
            Return Nothing
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function RightShiftObject(Operand As Object, Amount As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(Operand, IConvertible)
            If (convertible Is Nothing) Then
                If (Operand Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Amount, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Amount Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            If ((empty = TypeCode.Object) OrElse (typeCode = TypeCode.Object)) Then
                Dim arguments As Object() = New Object() {Operand, Amount}
                Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.ShiftRight, arguments)
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return (CInt(0) >> Conversions.ToInteger(Amount))
                Case TypeCode.Boolean
                    Return CShort((CShort(-(convertible.ToBoolean(Nothing) > 0)) >> (Conversions.ToInteger(Amount) And 15)))
                Case TypeCode.SByte
                    Return CSByte((convertible.ToSByte(Nothing) >> (Conversions.ToInteger(Amount) And 7)))
                Case TypeCode.Byte
                    Return CByte((convertible.ToByte(Nothing) >> (Conversions.ToInteger(Amount) And 7)))
                Case TypeCode.Int16
                    Return CShort((convertible.ToInt16(Nothing) >> (Conversions.ToInteger(Amount) And 15)))
                Case TypeCode.UInt16
                    Return CUShort((convertible.ToUInt16(Nothing) >> (Conversions.ToInteger(Amount) And 15)))
                Case TypeCode.Int32
                    Return (convertible.ToInt32(Nothing) >> Conversions.ToInteger(Amount))
                Case TypeCode.UInt32
                    Return (convertible.ToUInt32(Nothing) >> Conversions.ToInteger(Amount))
                Case TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return (convertible.ToInt64(Nothing) >> Conversions.ToInteger(Amount))
                Case TypeCode.UInt64
                    Return (convertible.ToUInt64(Nothing) >> Conversions.ToInteger(Amount))
                Case TypeCode.String
                    Return (Conversions.ToLong(convertible.ToString(Nothing)) >> Conversions.ToInteger(Amount))
            End Select
            Throw Operators.GetNoValidOperatorException(UserDefinedOperator.ShiftRight, Operand)
        End Function

        Private Shared Function SubtractByte(Left As Byte, Right As Byte) As Object
            Dim num As Short = CShort((Left - Right))
            If (num < 0) Then
                Return num
            End If
            Return CByte(num)
        End Function

        Private Shared Function SubtractDecimal(Left As IConvertible, Right As IConvertible) As Object
            Dim num As Decimal = Left.ToDecimal(Nothing)
            Dim num2 As Decimal = Right.ToDecimal(Nothing)
            Try
                Return Decimal.Subtract(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToDouble(num) - Convert.ToDouble(num2))
            End Try
        End Function

        Private Shared Function SubtractDouble(Left As Double, Right As Double) As Object
            Return (Left - Right)
        End Function

        Private Shared Function SubtractInt16(Left As Short, Right As Short) As Object
            Dim num As Integer = (Left - Right)
            If ((num < -32768) OrElse (num > &H7FFF)) Then
                Return num
            End If
            Return CShort(num)
        End Function

        Private Shared Function SubtractInt32(Left As Integer, Right As Integer) As Object
            Dim num As Long = (Left - Right)
            If ((num < -2147483648) OrElse (num > &H7FFFFFFF)) Then
                Return num
            End If
            Return CInt(num)
        End Function

        Private Shared Function SubtractInt64(Left As Long, Right As Long) As Object
            Try
                Return (Left - Right)
            Catch exception As OverflowException
                Return Decimal.Subtract(New Decimal(Left), New Decimal(Right))
            End Try
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function SubtractObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.Boxed_ZeroInteger
                Case 3
                    Return Operators.SubtractInt16(0, Operators.ToVBBool(convertible2))
                Case 5
                    Return Operators.SubtractSByte(0, convertible2.ToSByte(Nothing))
                Case 6
                    Return Operators.SubtractByte(0, convertible2.ToByte(Nothing))
                Case 7
                    Return Operators.SubtractInt16(0, convertible2.ToInt16(Nothing))
                Case 8
                    Return Operators.SubtractUInt16(0, convertible2.ToUInt16(Nothing))
                Case 9
                    Return Operators.SubtractInt32(0, convertible2.ToInt32(Nothing))
                Case 10
                    Return Operators.SubtractUInt32(0, convertible2.ToUInt32(Nothing))
                Case 11
                    Return Operators.SubtractInt64(0, convertible2.ToInt64(Nothing))
                Case 12
                    Return Operators.SubtractUInt64(0, convertible2.ToUInt64(Nothing))
                Case 13
                    Return Operators.SubtractSingle(0!, convertible2.ToSingle(Nothing))
                Case 14
                    Return Operators.SubtractDouble(0, convertible2.ToDouble(Nothing))
                Case 15
                    Return Operators.SubtractDecimal(DirectCast(Decimal.Zero, IConvertible), convertible2)
                Case &H12
                    Return Operators.SubtractDouble(0, Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H39
                    Return Operators.SubtractInt16(Operators.ToVBBool(conv), 0)
                Case 60
                    Return Operators.SubtractInt16(Operators.ToVBBool(conv), Operators.ToVBBool(convertible2))
                Case &H3E
                    Return Operators.SubtractSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing))
                Case &H3F, &H40
                    Return Operators.SubtractInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing))
                Case &H41, &H42
                    Return Operators.SubtractInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H43, &H44
                    Return Operators.SubtractInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case &H45, &H48
                    Return Operators.SubtractDecimal(Operators.ToVBBoolConv(conv), DirectCast(convertible2.ToDecimal(Nothing), IConvertible))
                Case 70
                    Return Operators.SubtractSingle(CSng(Operators.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H47
                    Return Operators.SubtractDouble(CDbl(Operators.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case &H4B
                    Return Operators.SubtractDouble(CDbl(Operators.ToVBBool(conv)), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H5F
                    Return conv.ToSByte(Nothing)
                Case &H62
                    Return Operators.SubtractSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2))
                Case 100
                    Return Operators.SubtractSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B, 140
                    Return Operators.SubtractInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3, 180
                    Return Operators.SubtractInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H69, &H6A, &H7D, &H8F, &H90, &HA3, &HB5, &HB6, &HC3, &HC5, &HC7, &HC9, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, 220
                    Return Operators.SubtractInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H6B, 110, &H81, &H91, &H94, &HA7, &HB7, &HBA, &HCD, &HDD, &HE0, &HE9, &HEB, &HED, &HEF, &HF3, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, 300
                    Return Operators.SubtractDecimal(conv, convertible2)
                Case &H6C, &H7F, &H92, &HA5, &HB8, &HCB, &HDE, &HF1, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H106, &H12A
                    Return Operators.SubtractSingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H6D, &H80, &H93, &HA6, &HB9, &HCC, &HDF, &HF2, &H105, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, &H12B
                    Return Operators.SubtractDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.SubtractDouble(conv.ToDouble(Nothing), Conversions.ToDouble(convertible2.ToString(Nothing)))
                Case &H72
                    Return conv.ToByte(Nothing)
                Case &H75, &H88
                    Return Operators.SubtractInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2))
                Case 120
                    Return Operators.SubtractByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H7A, &H9E, 160
                    Return Operators.SubtractUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing))
                Case &H7C, &HA2, &HC4, &HC6, 200
                    Return Operators.SubtractUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing))
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE, 240
                    Return Operators.SubtractUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing))
                Case &H85
                    Return conv.ToInt16(Nothing)
                Case &H98
                    Return conv.ToUInt16(Nothing)
                Case &H9B, &HAE
                    Return Operators.SubtractInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2))
                Case &HAB
                    Return conv.ToInt32(Nothing)
                Case 190
                    Return conv.ToUInt32(Nothing)
                Case &HC1, &HD4
                    Return Operators.SubtractInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)))
                Case &HD1
                    Return conv.ToInt64(Nothing)
                Case &HE4
                    Return conv.ToUInt64(Nothing)
                Case &HE7, &H120
                    Return Operators.SubtractDecimal(conv, Operators.ToVBBoolConv(convertible2))
                Case &HF7, &H10A, &H11D
                    Return Left
                Case 250
                    Return Operators.SubtractSingle(conv.ToSingle(Nothing), CSng(Operators.ToVBBool(convertible2)))
                Case &H10D
                    Return Operators.SubtractDouble(conv.ToDouble(Nothing), CDbl(Operators.ToVBBool(convertible2)))
                Case &H156
                    Return Conversions.ToDouble(conv.ToString(Nothing))
                Case &H159
                    Return Operators.SubtractDouble(Conversions.ToDouble(conv.ToString(Nothing)), CDbl(Operators.ToVBBool(convertible2)))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.SubtractDouble(Conversions.ToDouble(conv.ToString(Nothing)), convertible2.ToDouble(Nothing))
                Case 360
                    Return Operators.SubtractDouble(Conversions.ToDouble(conv.ToString(Nothing)), Conversions.ToDouble(convertible2.ToString(Nothing)))
            End Select
            If (((((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) AndAlso ((empty <> TypeCode.DateTime) OrElse (typeCode <> TypeCode.DateTime))) AndAlso ((empty <> TypeCode.DateTime) OrElse (typeCode <> TypeCode.Empty))) AndAlso ((empty <> TypeCode.Empty) OrElse (typeCode <> TypeCode.DateTime))) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Minus, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Minus, arguments)
        End Function

        Private Shared Function SubtractSByte(Left As SByte, Right As SByte) As Object
            Dim num As Short = CShort((Left - Right))
            If ((num < -128) OrElse (num > &H7F)) Then
                Return num
            End If
            Return CSByte(num)
        End Function

        Private Shared Function SubtractSingle(Left As Single, Right As Single) As Object
            Dim d As Double = (Left - Right)
            If ((d <= 3.4028234663852886E+38) AndAlso (d >= -3.4028234663852886E+38)) Then
                Return CSng(d)
            End If
            If (Double.IsInfinity(d) AndAlso (Single.IsInfinity(Left) OrElse Single.IsInfinity(Right))) Then
                Return CSng(d)
            End If
            Return d
        End Function

        Private Shared Function SubtractUInt16(Left As UInt16, Right As UInt16) As Object
            Dim num As Integer = (Left - Right)
            If (num < 0) Then
                Return num
            End If
            Return CUShort(num)
        End Function

        Private Shared Function SubtractUInt32(Left As UInt32, Right As UInt32) As Object
            Dim num As Long = (Left - Right)
            If (num < 0) Then
                Return num
            End If
            Return DirectCast(num, UInt32)
        End Function

        Private Shared Function SubtractUInt64(Left As UInt64, Right As UInt64) As Object
            Try
                Return (Left - Right)
            Catch exception As OverflowException
                Return Decimal.Subtract(New Decimal(Left), New Decimal(Right))
            End Try
        End Function

        Private Shared Function ToVBBool(conv As IConvertible) As SByte
            Return CSByte(-(conv.ToBoolean(Nothing) > False))
        End Function

        Private Shared Function ToVBBoolConv(conv As IConvertible) As IConvertible
            Return DirectCast(CSByte(-(conv.ToBoolean(Nothing) > False)), IConvertible)
        End Function

        Private Shared Function XorBoolean(Left As Boolean, Right As Boolean) As Object
            Return (Left Xor Right)
        End Function

        Private Shared Function XorByte(Left As Byte, Right As Byte, Optional EnumType As Type = Nothing) As Object
            Dim num As Byte = CByte((Left Xor Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function XorInt16(Left As Short, Right As Short, Optional EnumType As Type = Nothing) As Object
            Dim num As Short = CShort((Left Xor Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function XorInt32(Left As Integer, Right As Integer, Optional EnumType As Type = Nothing) As Object
            Dim num As Integer = (Left Xor Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function XorInt64(Left As Long, Right As Long, Optional EnumType As Type = Nothing) As Object
            Dim num As Long = (Left Xor Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function XorObject(Left As Object, Right As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(Left, IConvertible)
            If (conv Is Nothing) Then
                If (Left Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(Right, IConvertible)
            If (convertible2 Is Nothing) Then
                If (Right Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return Operators.Boxed_ZeroInteger
                Case 3
                    Return Operators.XorBoolean(False, convertible2.ToBoolean(Nothing))
                Case 5
                    Return Operators.XorSByte(0, convertible2.ToSByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case 6
                    Return Operators.XorByte(0, convertible2.ToByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case 7
                    Return Operators.XorInt16(0, convertible2.ToInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case 8
                    Return Operators.XorUInt16(0, convertible2.ToUInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case 9
                    Return Operators.XorInt32(0, convertible2.ToInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case 10
                    Return Operators.XorUInt32(0, convertible2.ToUInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case 11
                    Return Operators.XorInt64(0, convertible2.ToInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case 12
                    Return Operators.XorUInt64(0, convertible2.ToUInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case 13, 14, 15
                    Return Operators.XorInt64(0, convertible2.ToInt64(Nothing), Nothing)
                Case &H12
                    Return Operators.XorInt64(0, Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
                Case &H39
                    Return Operators.XorBoolean(conv.ToBoolean(Nothing), False)
                Case 60
                    Return Operators.XorBoolean(conv.ToBoolean(Nothing), convertible2.ToBoolean(Nothing))
                Case &H3E
                    Return Operators.XorSByte(Operators.ToVBBool(conv), convertible2.ToSByte(Nothing), Nothing)
                Case &H3F, &H40
                    Return Operators.XorInt16(Operators.ToVBBool(conv), convertible2.ToInt16(Nothing), Nothing)
                Case &H41, &H42
                    Return Operators.XorInt32(Operators.ToVBBool(conv), convertible2.ToInt32(Nothing), Nothing)
                Case &H43, &H44, &H45, 70, &H47, &H48
                    Return Operators.XorInt64(CLng(Operators.ToVBBool(conv)), convertible2.ToInt64(Nothing), Nothing)
                Case &H4B
                    Return Operators.XorBoolean(conv.ToBoolean(Nothing), Conversions.ToBoolean(convertible2.ToString(Nothing)))
                Case &H5F
                    Return Operators.XorSByte(conv.ToSByte(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case &H62
                    Return Operators.XorSByte(conv.ToSByte(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 100
                    Return Operators.XorSByte(conv.ToSByte(Nothing), convertible2.ToSByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H65, &H66, &H77, &H79, &H8A, &H8B
                    Return Operators.XorInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing), Nothing)
                Case &H67, &H68, &H7B, &H8D, &H8E, &H9D, &H9F, &HA1, &HB0, &HB1, &HB2, &HB3
                    Return Operators.XorInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing), Nothing)
                Case &H69, &H6A, &H6B, &H6C, &H6D, 110, &H7D, &H7F, &H80, &H81, &H8F, &H90, &H91, &H92, &H93, &H94, &HA3, &HA5, &HA6, &HA7, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HC3, &HC5, &HC7, &HC9, &HCB, &HCC, &HCD, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, &HDD, &HDE, &HDF, &HE0, &HE9, &HEB, &HED, &HEF, &HF1, &HF2, &HF3, &HFC, &HFD, &HFE, &HFF, &H100, &H101, &H102, &H103, 260, &H105, &H106, &H10F, &H110, &H111, &H112, &H113, &H114, &H115, &H116, &H117, 280, &H119, 290, &H123, &H124, &H125, &H126, &H127, &H128, &H129, &H12A, &H12B, 300
                    Return Operators.XorInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing), Nothing)
                Case &H71, &H84, &H97, 170, &HBD, &HD0, &HE3, &HF6, &H109, &H11C, &H12F
                    Return Operators.XorInt64(conv.ToInt64(Nothing), Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
                Case &H72
                    Return Operators.XorByte(conv.ToByte(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case &H75, &H88
                    Return Operators.XorInt16(conv.ToInt16(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 120
                    Return Operators.XorByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H7A, &H9E
                    Return Operators.XorUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing), Nothing)
                Case &H7C, &HA2, &HC4, &HC6
                    Return Operators.XorUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing), Nothing)
                Case &H7E, &HA4, &HCA, &HEA, &HEC, &HEE
                    Return Operators.XorUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing), Nothing)
                Case &H85
                    Return Operators.XorInt16(conv.ToInt16(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case 140
                    Return Operators.XorInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case &H98
                    Return Operators.XorUInt16(conv.ToUInt16(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case &H9B, &HAE
                    Return Operators.XorInt32(conv.ToInt32(Nothing), Operators.ToVBBool(convertible2), Nothing)
                Case 160
                    Return Operators.XorUInt16(conv.ToUInt16(Nothing), convertible2.ToUInt16(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HAB
                    Return Operators.XorInt32(conv.ToInt32(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case 180
                    Return Operators.XorInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case 190
                    Return Operators.XorUInt32(conv.ToUInt32(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case &HC1, &HD4, &HE7, 250, &H10D, &H120
                    Return Operators.XorInt64(conv.ToInt64(Nothing), CLng(Operators.ToVBBool(convertible2)), Nothing)
                Case 200
                    Return Operators.XorUInt32(conv.ToUInt32(Nothing), convertible2.ToUInt32(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HD1
                    Return Operators.XorInt64(conv.ToInt64(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case 220
                    Return Operators.XorInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HE4
                    Return Operators.XorUInt64(conv.ToUInt64(Nothing), 0, Operators.GetEnumResult(Left, Right))
                Case 240
                    Return Operators.XorUInt64(conv.ToUInt64(Nothing), convertible2.ToUInt64(Nothing), Operators.GetEnumResult(Left, Right))
                Case &HF7, &H10A, &H11D
                    Return Operators.XorInt64(conv.ToInt64(Nothing), 0, Nothing)
                Case &H156
                    Return Operators.XorInt64(Conversions.ToLong(conv.ToString(Nothing)), 0, Nothing)
                Case &H159
                    Return Operators.XorBoolean(Conversions.ToBoolean(conv.ToString(Nothing)), convertible2.ToBoolean(Nothing))
                Case &H15B, &H15C, &H15D, 350, &H15F, &H160, &H161, &H162, &H163, &H164, &H165
                    Return Operators.XorInt64(Conversions.ToLong(conv.ToString(Nothing)), convertible2.ToInt64(Nothing), Nothing)
                Case 360
                    Return Operators.XorInt64(Conversions.ToLong(conv.ToString(Nothing)), Conversions.ToLong(convertible2.ToString(Nothing)), Nothing)
            End Select
            If ((empty <> TypeCode.Object) AndAlso (typeCode <> TypeCode.Object)) Then
                Throw Operators.GetNoValidOperatorException(UserDefinedOperator.Xor, Left, Right)
            End If
            Dim arguments As Object() = New Object() {Left, Right}
            Return Operators.InvokeUserDefinedOperator(UserDefinedOperator.Xor, arguments)
        End Function

        Private Shared Function XorSByte(Left As SByte, Right As SByte, Optional EnumType As Type = Nothing) As Object
            Dim num As SByte = CSByte((Left Xor Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function XorUInt16(Left As UInt16, Right As UInt16, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt16 = CUShort((Left Xor Right))
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function XorUInt32(Left As UInt32, Right As UInt32, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt32 = (Left Xor Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function

        Private Shared Function XorUInt64(Left As UInt64, Right As UInt64, Optional EnumType As Type = Nothing) As Object
            Dim num As UInt64 = (Left Xor Right)
            If (Not EnumType Is Nothing) Then
                Return [Enum].ToObject(EnumType, num)
            End If
            Return num
        End Function


        ' Fields
        Friend Shared ReadOnly Boxed_ZeroByte As Object = CByte(0)
        Friend Shared ReadOnly Boxed_ZeroDecimal As Object = Decimal.Zero
        Friend Shared ReadOnly Boxed_ZeroDouble As Object = 0
        Friend Shared ReadOnly Boxed_ZeroInteger As Object = 0
        Friend Shared ReadOnly Boxed_ZeroLong As Object = 0
        Friend Shared ReadOnly Boxed_ZeroSByte As Object = CSByte(0)
        Friend Shared ReadOnly Boxed_ZeroShort As Object = CShort(0)
        Friend Shared ReadOnly Boxed_ZeroSinge As Object = 0!
        Friend Shared ReadOnly Boxed_ZeroUInteger As Object = 0
        Friend Shared ReadOnly Boxed_ZeroULong As Object = CULng(0)
        Friend Shared ReadOnly Boxed_ZeroUShort As Object = CUShort(0)
        Private Const TCMAX As Integer = &H13

        ' Nested Types
        Private Enum CompareClass
            ' Fields
            Equal = 0
            Greater = 1
            Less = -1
            Undefined = 4
            Unordered = 2
            UserDefined = 3
        End Enum
    End Class
End Namespace

