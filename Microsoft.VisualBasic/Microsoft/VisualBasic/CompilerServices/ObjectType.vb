Imports System.ComponentModel
Imports System.Globalization
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices

    <EditorBrowsable(EditorBrowsableState.Never)>
    Public NotInheritable Class ObjectType
        ' Methods
        Private Shared Function AddByte(i1 As Byte, i2 As Byte) As Object
            Dim num As Short = CShort((i1 + i2))
            If ((num >= 0) AndAlso (num <= &HFF)) Then
                Return CByte(num)
            End If
            Return num
        End Function

        Private Shared Function AddDecimal(conv1 As IConvertible, conv2 As IConvertible) As Object
            Dim num As Decimal
            If (Not conv1 Is Nothing) Then
                num = conv1.ToDecimal(Nothing)
            End If
            Dim num2 As Decimal = conv2.ToDecimal(Nothing)
            Try
                Return Decimal.Add(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToDouble(num) + Convert.ToDouble(num2))
            End Try
        End Function

        Private Shared Function AddDouble(d1 As Double, d2 As Double) As Object
            Return (d1 + d2)
        End Function

        Private Shared Function AddInt16(i1 As Short, i2 As Short) As Object
            Dim num As Integer = (i1 + i2)
            If ((num >= -32768) AndAlso (num <= &H7FFF)) Then
                Return CShort(num)
            End If
            Return num
        End Function

        Private Shared Function AddInt32(i1 As Integer, i2 As Integer) As Object
            Dim num As Long = (i1 + i2)
            If ((num >= -2147483648) AndAlso (num <= &H7FFFFFFF)) Then
                Return CInt(num)
            End If
            Return num
        End Function

        Private Shared Function AddInt64(i1 As Long, i2 As Long) As Object
            Try
                Return (i1 + i2)
            Catch exception As OverflowException
                Return Decimal.Add(New Decimal(i1), New Decimal(i2))
            End Try
        End Function

        Public Shared Function AddObj(o1 As Object, o2 As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(o1, IConvertible)
            If (convertible Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim conv As IConvertible = TryCast(o2, IConvertible)
            If (conv Is Nothing) Then
                If (o2 Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = conv.GetTypeCode
            End If
            If (((empty = TypeCode.Object) AndAlso TypeOf o1 Is Char()) AndAlso (((typeCode = TypeCode.String) OrElse (typeCode = TypeCode.Empty)) OrElse ((typeCode = TypeCode.Object) AndAlso TypeOf o2 Is Char()))) Then
                o1 = New String(CharArrayType.FromObject(o1))
                convertible = DirectCast(o1, IConvertible)
                empty = TypeCode.String
            End If
            If (((typeCode = TypeCode.Object) AndAlso TypeOf o2 Is Char()) AndAlso ((empty = TypeCode.String) OrElse (empty = TypeCode.Empty))) Then
                o2 = New String(CharArrayType.FromObject(o2))
                conv = DirectCast(o2, IConvertible)
                typeCode = TypeCode.String
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case &H72, &H85
                    Return o1
                Case &H75, &H88
                    Return ObjectType.AddInt16(convertible.ToInt16(Nothing), CShort(ObjectType.ToVBBool(conv)))
                Case 120
                    Return ObjectType.AddByte(convertible.ToByte(Nothing), conv.ToByte(Nothing))
                Case &H79, &H8B, 140
                    Return ObjectType.AddInt16(convertible.ToInt16(Nothing), conv.ToInt16(Nothing))
                Case &H7B, &H8E, &HB1, &HB2, 180
                    Return ObjectType.AddInt32(convertible.ToInt32(Nothing), conv.ToInt32(Nothing))
                Case &H7D, &H90, &HB6, &HD7, &HD8, &HDA, 220
                    Return ObjectType.AddInt64(convertible.ToInt64(Nothing), conv.ToInt64(Nothing))
                Case &H7F, &H92, &HB8, &HDE, &HFD, &HFE, &H100, &H102, 260, &H106, &H12A
                    Return ObjectType.AddSingle(convertible.ToSingle(Nothing), conv.ToSingle(Nothing))
                Case &H80, &H93, &HB9, &HDF, &H105, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H12B
                    Return ObjectType.AddDouble(convertible.ToDouble(Nothing), conv.ToDouble(Nothing))
                Case &H81, &H94, &HBA, &HE0, &H123, &H124, &H126, &H128, 300
                    Return ObjectType.AddDecimal(convertible, conv)
                Case &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F
                    Return ObjectType.AddString(convertible, empty, conv, typeCode)
                Case &H5E, 80, &H15A, &H166, 360, &H142, 320
                    Return (StringType.FromObject(o1) & StringType.FromObject(o2))
                Case 0
                    Return 0
                Case 3, 6, 7, 9, 11, 13, 14, 15
                    Return o2
                Case &H12
                    Return o2
                Case &H38
                    Return o2
                Case &H39
                    Return o1
                Case 60
                    Return ObjectType.AddInt16(CShort(ObjectType.ToVBBool(convertible)), CShort(ObjectType.ToVBBool(conv)))
                Case &H3F, &H40
                    Return ObjectType.AddInt16(CShort(ObjectType.ToVBBool(convertible)), conv.ToInt16(Nothing))
                Case &H42
                    Return ObjectType.AddInt32(ObjectType.ToVBBool(convertible), conv.ToInt32(Nothing))
                Case &H44
                    Return ObjectType.AddInt64(CLng(ObjectType.ToVBBool(convertible)), conv.ToInt64(Nothing))
                Case 70
                    Return ObjectType.AddSingle(CSng(ObjectType.ToVBBool(convertible)), conv.ToSingle(Nothing))
                Case &H47
                    Return ObjectType.AddDouble(CDbl(ObjectType.ToVBBool(convertible)), conv.ToDouble(Nothing))
                Case &H48
                    Return ObjectType.AddDecimal(ObjectType.ToVBBoolConv(convertible), conv)
                Case &H4B, &H159
                    Return ObjectType.AddString(convertible, empty, conv, typeCode)
                Case &HAB
                    Return o1
                Case &HAE
                    Return ObjectType.AddInt32(convertible.ToInt32(Nothing), ObjectType.ToVBBool(conv))
                Case &HD1
                    Return o1
                Case &HD4
                    Return ObjectType.AddInt64(convertible.ToInt64(Nothing), CLng(ObjectType.ToVBBool(conv)))
                Case &HF7, &H10A, &H11D
                    Return o1
                Case 250
                    Return ObjectType.AddSingle(convertible.ToSingle(Nothing), CSng(ObjectType.ToVBBool(conv)))
                Case &H10D
                    Return ObjectType.AddDouble(convertible.ToDouble(Nothing), CDbl(ObjectType.ToVBBool(conv)))
                Case &H120
                    Return ObjectType.AddDecimal(convertible, ObjectType.ToVBBoolConv(conv))
                Case &H156, &H158
                    Return o1
                Case &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.AddString(convertible, empty, conv, typeCode)
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function AddSingle(f1 As Single, f2 As Single) As Object
            Dim d As Double = (f1 + f2)
            If ((d <= 3.4028234663852886E+38) AndAlso (d >= -3.4028234663852886E+38)) Then
                Return CSng(d)
            End If
            If (Double.IsInfinity(d) AndAlso (Single.IsInfinity(f1) OrElse Single.IsInfinity(f2))) Then
                Return CSng(d)
            End If
            Return d
        End Function

        Private Shared Function AddString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Object
            Dim num As Double
            Dim num2 As Double
            If (tc1 = TypeCode.String) Then
                num = DoubleType.FromString(conv1.ToString(Nothing))
            ElseIf (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToDouble(Nothing)
            End If
            If (tc2 = TypeCode.String) Then
                num2 = DoubleType.FromString(conv2.ToString(Nothing))
            ElseIf (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToDouble(Nothing)
            End If
            Return (num + num2)
        End Function

        Public Shared Function BitAndObj(obj1 As Object, obj2 As Object) As Object
            Dim isEnum As Boolean
            Dim flag2 As Boolean
            If ((obj1 Is Nothing) AndAlso (obj2 Is Nothing)) Then
                Return 0
            End If
            Dim enumType As Type = Nothing
            Dim type As Type = Nothing
            If (Not obj1 Is Nothing) Then
                enumType = obj1.GetType
                isEnum = enumType.IsEnum
            End If
            If (Not obj2 Is Nothing) Then
                type = obj2.GetType
                flag2 = type.IsEnum
            End If
            Select Case ObjectType.GetWidestType(obj1, obj2, False)
                Case TypeCode.Boolean
                    If (Not enumType Is type) Then
                        Return CShort((ShortType.FromObject(obj1) And ShortType.FromObject(obj2)))
                    End If
                    Return (BooleanType.FromObject(obj1) And BooleanType.FromObject(obj2))
                Case TypeCode.Byte
                    Dim num As Byte = CByte((ByteType.FromObject(obj1) And ByteType.FromObject(obj2)))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num)
                    End If
                    Return num
                Case TypeCode.Int16
                    Dim num2 As Short = CShort((ShortType.FromObject(obj1) And ShortType.FromObject(obj2)))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num2)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num2)
                    End If
                    Return num2
                Case TypeCode.Int32
                    Dim num3 As Integer = (IntegerType.FromObject(obj1) And IntegerType.FromObject(obj2))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num3)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num3)
                    End If
                    Return num3
                Case TypeCode.Int64
                    Dim num4 As Long = (LongType.FromObject(obj1) And LongType.FromObject(obj2))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num4)
                        End If
                        If flag2 Then
                            Return [Enum].ToObject(type, num4)
                        End If
                        Exit Select
                    End If
                    Return num4
                Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                    Return (LongType.FromObject(obj1) And LongType.FromObject(obj2))
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj1, obj2)
        End Function

        Public Shared Function BitOrObj(obj1 As Object, obj2 As Object) As Object
            Dim isEnum As Boolean
            Dim flag2 As Boolean
            If ((obj1 Is Nothing) AndAlso (obj2 Is Nothing)) Then
                Return 0
            End If
            Dim enumType As Type = Nothing
            Dim type As Type = Nothing
            If (Not obj1 Is Nothing) Then
                enumType = obj1.GetType
                isEnum = enumType.IsEnum
            End If
            If (Not obj2 Is Nothing) Then
                type = obj2.GetType
                flag2 = type.IsEnum
            End If
            Select Case ObjectType.GetWidestType(obj1, obj2, False)
                Case TypeCode.Boolean
                    If (Not enumType Is type) Then
                        Return CShort((ShortType.FromObject(obj1) Or ShortType.FromObject(obj2)))
                    End If
                    Return (BooleanType.FromObject(obj1) Or BooleanType.FromObject(obj2))
                Case TypeCode.Byte
                    Dim num As Byte = CByte((ByteType.FromObject(obj1) Or ByteType.FromObject(obj2)))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num)
                    End If
                    Return num
                Case TypeCode.Int16
                    Dim num2 As Short = CShort((ShortType.FromObject(obj1) Or ShortType.FromObject(obj2)))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num2)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num2)
                    End If
                    Return num2
                Case TypeCode.Int32
                    Dim num3 As Integer = (IntegerType.FromObject(obj1) Or IntegerType.FromObject(obj2))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num3)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num3)
                    End If
                    Return num3
                Case TypeCode.Int64
                    Dim num4 As Long = (LongType.FromObject(obj1) Or LongType.FromObject(obj2))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num4)
                        End If
                        If flag2 Then
                            Return [Enum].ToObject(type, num4)
                        End If
                        Exit Select
                    End If
                    Return num4
                Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                    Return (LongType.FromObject(obj1) Or LongType.FromObject(obj2))
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj1, obj2)
        End Function

        Public Shared Function BitXorObj(obj1 As Object, obj2 As Object) As Object
            Dim isEnum As Boolean
            Dim flag2 As Boolean
            If ((obj1 Is Nothing) AndAlso (obj2 Is Nothing)) Then
                Return 0
            End If
            Dim enumType As Type = Nothing
            Dim type As Type = Nothing
            If (Not obj1 Is Nothing) Then
                enumType = obj1.GetType
                isEnum = enumType.IsEnum
            End If
            If (Not obj2 Is Nothing) Then
                type = obj2.GetType
                flag2 = type.IsEnum
            End If
            Select Case ObjectType.GetWidestType(obj1, obj2, False)
                Case TypeCode.Boolean
                    If (Not enumType Is type) Then
                        Return CShort((ShortType.FromObject(obj1) Xor ShortType.FromObject(obj2)))
                    End If
                    Return (BooleanType.FromObject(obj1) Xor BooleanType.FromObject(obj2))
                Case TypeCode.Byte
                    Dim num As Byte = CByte((ByteType.FromObject(obj1) Xor ByteType.FromObject(obj2)))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num)
                    End If
                    Return num
                Case TypeCode.Int16
                    Dim num2 As Short = CShort((ShortType.FromObject(obj1) Xor ShortType.FromObject(obj2)))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num2)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num2)
                    End If
                    Return num2
                Case TypeCode.Int32
                    Dim num3 As Integer = (IntegerType.FromObject(obj1) Xor IntegerType.FromObject(obj2))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num3)
                        End If
                        If Not flag2 Then
                            Exit Select
                        End If
                        Return [Enum].ToObject(type, num3)
                    End If
                    Return num3
                Case TypeCode.Int64
                    Dim num4 As Long = (LongType.FromObject(obj1) Xor LongType.FromObject(obj2))
                    If (((Not isEnum OrElse Not flag2) OrElse (enumType Is type)) AndAlso (isEnum AndAlso flag2)) Then
                        If isEnum Then
                            Return [Enum].ToObject(enumType, num4)
                        End If
                        If flag2 Then
                            Return [Enum].ToObject(type, num4)
                        End If
                        Exit Select
                    End If
                    Return num4
                Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                    Return (LongType.FromObject(obj1) Xor LongType.FromObject(obj2))
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj1, obj2)
        End Function

        Friend Shared Function CTypeHelper(obj As Object, toType As Type) As Object
            Dim flag As Boolean
            Dim objectValuePrimitive As Object
            If (obj Is Nothing) Then
                Return Nothing
            End If
            If (toType Is GetType(Object)) Then
                Return obj
            End If
            Dim typ As Type = obj.GetType
            If toType.IsByRef Then
                toType = toType.GetElementType
                flag = True
            End If
            If typ.IsByRef Then
                typ = typ.GetElementType
            End If
            If ((typ Is toType) OrElse (toType Is GetType(Object))) Then
                If Not flag Then
                    Return obj
                End If
                objectValuePrimitive = ObjectType.GetObjectValuePrimitive(obj)
            Else
                Dim typeCode As TypeCode = Type.GetTypeCode(toType)
                If (typeCode = TypeCode.Object) Then
                    If ((toType Is GetType(Object)) OrElse toType.IsInstanceOfType(obj)) Then
                        Return obj
                    End If
                    Dim str As String = TryCast(obj, String)
                    If ((Not str Is Nothing) AndAlso (toType Is GetType(Char()))) Then
                        Return CharArrayType.FromString(str)
                    End If
                    Dim args As String() = New String() {Utils.VBFriendlyName(typ), Utils.VBFriendlyName(toType)}
                    Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
                End If
                objectValuePrimitive = ObjectType.CTypeHelper(obj, typeCode)
            End If
            If toType.IsEnum Then
                Return [Enum].ToObject(toType, objectValuePrimitive)
            End If
            Return objectValuePrimitive
        End Function

        Friend Shared Function CTypeHelper(obj As Object, toType As TypeCode) As Object
            If (obj Is Nothing) Then
                Return Nothing
            End If
            Select Case toType
                Case TypeCode.Boolean
                    Return BooleanType.FromObject(obj)
                Case TypeCode.Char
                    Return CharType.FromObject(obj)
                Case TypeCode.Byte
                    Return ByteType.FromObject(obj)
                Case TypeCode.Int16
                    Return ShortType.FromObject(obj)
                Case TypeCode.Int32
                    Return IntegerType.FromObject(obj)
                Case TypeCode.Int64
                    Return LongType.FromObject(obj)
                Case TypeCode.Single
                    Return SingleType.FromObject(obj)
                Case TypeCode.Double
                    Return DoubleType.FromObject(obj)
                Case TypeCode.Decimal
                    Return DecimalType.FromObject(obj)
                Case TypeCode.DateTime
                    Return DateType.FromObject(obj)
                Case TypeCode.String
                    Return StringType.FromObject(obj)
            End Select
            Dim args As String() = New String() {Utils.VBFriendlyName(obj), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(toType))}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Private Shared Function DivDecimal(conv1 As IConvertible, conv2 As IConvertible) As Object
            Dim num As Decimal
            Dim num2 As Decimal
            If (Not conv1 Is Nothing) Then
                num = conv1.ToDecimal(Nothing)
            End If
            If (Not conv2 Is Nothing) Then
                num2 = conv2.ToDecimal(Nothing)
            End If
            Try
                Return Decimal.Divide(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToSingle(num) / Convert.ToSingle(num2))
            End Try
        End Function

        Private Shared Function DivDouble(d1 As Double, d2 As Double) As Object
            Return (d1 / d2)
        End Function

        Public Shared Function DivObj(o1 As Object, o2 As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(o1, IConvertible)
            If (convertible Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(o2, IConvertible)
            If (convertible2 Is Nothing) Then
                If (o2 Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return ObjectType.DivDouble(0, 0)
                Case 3, 6, 7, 9, 11, 13, 14, 15
                    Return ObjectType.DivDouble(0, convertible2.ToDouble(Nothing))
                Case &H12
                    Return ObjectType.DivString(convertible, empty, convertible2, typeCode)
                Case &H39
                    Return ObjectType.DivDouble(CDbl(ObjectType.ToVBBool(convertible)), 0)
                Case 60
                    Return ObjectType.DivDouble(CDbl(ObjectType.ToVBBool(convertible)), CDbl(ObjectType.ToVBBool(convertible2)))
                Case &H3F, &H40, &H42, &H44, &H47
                    Return ObjectType.DivDouble(CDbl(ObjectType.ToVBBool(convertible)), convertible2.ToDouble(Nothing))
                Case 70
                    Return ObjectType.DivSingle(CSng(ObjectType.ToVBBool(convertible)), convertible2.ToSingle(Nothing))
                Case &H48
                    Return ObjectType.DivDecimal(ObjectType.ToVBBoolConv(convertible), DirectCast(convertible2.ToDecimal(Nothing), IConvertible))
                Case &H4B
                    Return ObjectType.DivString(convertible, empty, convertible2, typeCode)
                Case &H72, &H85, &HAB, &HD1, &HF7, &H10A, &H11D
                    Return ObjectType.DivDouble(convertible.ToDouble(Nothing), 0)
                Case &H75, &H88, &HAE, &HD4, &H10D
                    Return ObjectType.DivDouble(convertible.ToDouble(Nothing), CDbl(ObjectType.ToVBBool(convertible2)))
                Case 120, &H79, &H7B, &H7D, &H80, &H8B, 140, &H8E, &H90, &H93, &HB1, &HB2, 180, &HB6, &HB9, &HD7, &HD8, &HDA, 220, &HDF, &H105, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H12B
                    Return ObjectType.DivDouble(convertible.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H7F, &H92, &HB8, &HDE, &HFD, &HFE, &H100, &H102, 260, &H106, &H12A
                    Return ObjectType.DivSingle(convertible.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H81, &H94, &HBA, &HE0, &H123, &H124, &H126, &H128, 300
                    Return ObjectType.DivDecimal(convertible, convertible2)
                Case &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F
                    Return ObjectType.DivString(convertible, empty, convertible2, typeCode)
                Case 250
                    Return ObjectType.DivSingle(convertible.ToSingle(Nothing), CSng(ObjectType.ToVBBool(convertible2)))
                Case &H120
                    Return ObjectType.DivDecimal(convertible, ObjectType.ToVBBoolConv(convertible2))
                Case &H156
                    Return ObjectType.DivString(convertible, empty, convertible2, typeCode)
                Case &H159
                    Return ObjectType.DivString(convertible, empty, convertible2, typeCode)
                Case &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.DivString(convertible, empty, convertible2, typeCode)
                Case 360
                    Return ObjectType.DivStringString(convertible.ToString(Nothing), convertible2.ToString(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function DivSingle(sng1 As Single, sng2 As Single) As Object
            Dim f As Single = (sng1 / sng2)
            If Not Single.IsInfinity(f) Then
                Return f
            End If
            If (Single.IsInfinity(sng1) OrElse Single.IsInfinity(sng2)) Then
                Return f
            End If
            Return (CDbl(sng1) / CDbl(sng2))
        End Function

        Private Shared Function DivString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Object
            Dim num As Double
            Dim num2 As Double
            If (tc1 = TypeCode.String) Then
                num = DoubleType.FromString(conv1.ToString(Nothing))
            ElseIf (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToDouble(Nothing)
            End If
            If (tc2 = TypeCode.String) Then
                num2 = DoubleType.FromString(conv2.ToString(Nothing))
            ElseIf (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToDouble(Nothing)
            End If
            Return (num / num2)
        End Function

        Private Shared Function DivStringString(s1 As String, s2 As String) As Object
            Dim num As Double
            Dim num2 As Double
            If (Not s1 Is Nothing) Then
                num = DoubleType.FromString(s1)
            End If
            If (Not s2 Is Nothing) Then
                num2 = DoubleType.FromString(s2)
            End If
            Return (num / num2)
        End Function

        Private Shared Function GetNoValidOperatorException(Operand As Object) As Exception
            Dim args As String() = New String() {Utils.VBFriendlyName(Operand)}
            Return New InvalidCastException(Utils.GetResourceString("NoValidOperator_OneOperand", args))
        End Function

        Private Shared Function GetNoValidOperatorException(Left As Object, Right As Object) As Exception
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
            Dim args As String() = New String() {resourceString, str2}
            Return New InvalidCastException(Utils.GetResourceString("NoValidOperator_TwoOperands", args))
        End Function

        Public Shared Function GetObjectValuePrimitive(o As Object) As Object
            If (o Is Nothing) Then
                Return Nothing
            End If
            Dim convertible As IConvertible = TryCast(o, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return convertible.ToBoolean(Nothing)
                    Case TypeCode.Char
                        Return convertible.ToChar(Nothing)
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
                    Case TypeCode.Single
                        Return convertible.ToSingle(Nothing)
                    Case TypeCode.Double
                        Return convertible.ToDouble(Nothing)
                    Case TypeCode.Decimal
                        Return convertible.ToDecimal(Nothing)
                    Case TypeCode.DateTime
                        Return convertible.ToDateTime(Nothing)
                    Case (TypeCode.DateTime Or TypeCode.Object)
                        Return o
                    Case TypeCode.String
                        Return o
                End Select
            End If
            Return o
        End Function

        Friend Shared Function GetWidestType(obj1 As Object, type2 As TypeCode) As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(obj1, IConvertible)
            If (Not convertible Is Nothing) Then
                typeCode = convertible.GetTypeCode
            ElseIf (obj1 Is Nothing) Then
                typeCode = TypeCode.Empty
            ElseIf (TypeOf obj1 Is Char() AndAlso (DirectCast(obj1, Array).Rank = 1)) Then
                typeCode = TypeCode.String
            Else
                typeCode = TypeCode.Object
            End If
            If (obj1 Is Nothing) Then
                Return type2
            End If
            Return ObjectType.TypeCodeFromVType(ObjectType.WiderType(CInt(ObjectType.VTypeFromTypeCode(typeCode)), CInt(ObjectType.VTypeFromTypeCode(type2))))
        End Function

        Friend Shared Function GetWidestType(obj1 As Object, obj2 As Object, Optional IsAdd As Boolean = False) As TypeCode
            Dim typeCode As TypeCode
            Dim empty As TypeCode
            Dim convertible As IConvertible = TryCast(obj1, IConvertible)
            Dim convertible2 As IConvertible = TryCast(obj2, IConvertible)
            If (Not convertible Is Nothing) Then
                typeCode = convertible.GetTypeCode
            ElseIf (obj1 Is Nothing) Then
                typeCode = TypeCode.Empty
            ElseIf (TypeOf obj1 Is Char() AndAlso (DirectCast(obj1, Array).Rank = 1)) Then
                typeCode = TypeCode.String
            Else
                typeCode = TypeCode.Object
            End If
            If (Not convertible2 Is Nothing) Then
                empty = convertible2.GetTypeCode
            ElseIf (obj2 Is Nothing) Then
                empty = TypeCode.Empty
            ElseIf (TypeOf obj2 Is Char() AndAlso (DirectCast(obj2, Array).Rank = 1)) Then
                empty = TypeCode.String
            Else
                empty = TypeCode.Object
            End If
            If (obj1 Is Nothing) Then
                Return empty
            End If
            If (obj2 Is Nothing) Then
                Return typeCode
            End If
            If (IsAdd AndAlso (((typeCode = TypeCode.DBNull) AndAlso (empty = TypeCode.String)) OrElse ((typeCode = TypeCode.String) AndAlso (empty = TypeCode.DBNull)))) Then
                Return TypeCode.DBNull
            End If
            Return ObjectType.TypeCodeFromVType(ObjectType.WiderType(CInt(ObjectType.VTypeFromTypeCode(typeCode)), CInt(ObjectType.VTypeFromTypeCode(empty))))
        End Function

        Private Shared Function IDivideByte(d1 As Byte, d2 As Byte) As Object
            Return CByte((d1 / d2))
        End Function

        Private Shared Function IDivideInt16(d1 As Short, d2 As Short) As Object
            Return CShort((d1 / d2))
        End Function

        Private Shared Function IDivideInt32(d1 As Integer, d2 As Integer) As Object
            Return (d1 / d2)
        End Function

        Private Shared Function IDivideInt64(d1 As Long, d2 As Long) As Object
            Return (d1 / d2)
        End Function

        Private Shared Function IDivideString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Object
            Dim num As Long
            Dim num2 As Long
            If (tc1 = TypeCode.String) Then
                Try
                    num = LongType.FromString(conv1.ToString(Nothing))
                    GoTo Label_003E
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception9 As Exception
                    Throw ObjectType.GetNoValidOperatorException(conv1, conv2)
                End Try
            End If
            If (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToInt64(Nothing)
            End If
Label_003E:
            If (tc2 = TypeCode.String) Then
                Try
                    num2 = LongType.FromString(conv2.ToString(Nothing))
                    GoTo Label_0080
                Catch exception4 As StackOverflowException
                    Throw exception4
                Catch exception5 As OutOfMemoryException
                    Throw exception5
                Catch exception6 As ThreadAbortException
                    Throw exception6
                Catch exception13 As Exception
                    Throw ObjectType.GetNoValidOperatorException(conv1, conv2)
                End Try
            End If
            If (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToInt64(Nothing)
            End If
Label_0080:
            Return (num / num2)
        End Function

        Private Shared Function IDivideStringString(s1 As String, s2 As String) As Object
            Dim num As Long
            Dim num2 As Long
            If (Not s1 Is Nothing) Then
                num = LongType.FromString(s1)
            End If
            If (Not s2 Is Nothing) Then
                num2 = LongType.FromString(s2)
            End If
            Return (num / num2)
        End Function

        Public Shared Function IDivObj(o1 As Object, o2 As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(o1, IConvertible)
            If (conv Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(o2, IConvertible)
            If (convertible2 Is Nothing) Then
                If (o2 Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return ObjectType.IDivideInt32(0, 0)
                Case 3
                    Return ObjectType.IDivideInt64(0, CLng(ObjectType.ToVBBool(convertible2)))
                Case 6
                    Return ObjectType.IDivideByte(0, convertible2.ToByte(Nothing))
                Case 7
                    Return ObjectType.IDivideInt16(0, convertible2.ToInt16(Nothing))
                Case 9
                    Return ObjectType.IDivideInt32(0, convertible2.ToInt32(Nothing))
                Case 11, 13, 14, 15
                    Return ObjectType.IDivideInt64(0, convertible2.ToInt64(Nothing))
                Case &H12
                    Return ObjectType.IDivideInt64(0, LongType.FromString(convertible2.ToString(Nothing)))
                Case &H39
                    Return ObjectType.IDivideInt16(CShort(ObjectType.ToVBBool(conv)), 0)
                Case 60
                    Return ObjectType.IDivideInt16(CShort(ObjectType.ToVBBool(conv)), CShort(ObjectType.ToVBBool(convertible2)))
                Case &H3F, &H40
                    Return ObjectType.IDivideInt16(CShort(ObjectType.ToVBBool(conv)), convertible2.ToInt16(Nothing))
                Case &H42
                    Return ObjectType.IDivideInt32(ObjectType.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H44, 70, &H47, &H48
                    Return ObjectType.IDivideInt64(CLng(ObjectType.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case &H4B
                    Return ObjectType.IDivideInt64(CLng(ObjectType.ToVBBool(conv)), LongType.FromString(convertible2.ToString(Nothing)))
                Case &H72
                    Return ObjectType.IDivideByte(conv.ToByte(Nothing), 0)
                Case &H75, &H88
                    Return ObjectType.IDivideInt16(conv.ToInt16(Nothing), CShort(ObjectType.ToVBBool(convertible2)))
                Case 120
                    Return ObjectType.IDivideByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H79, &H8B, 140
                    Return ObjectType.IDivideInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H7B, &H8E, &HB1, &HB2, 180
                    Return ObjectType.IDivideInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H7D, &H7F, &H80, &H81, &H90, &H92, &H93, &H94, &HB6, &HB8, &HB9, &HBA, &HD7, &HD8, &HDA, 220, &HDE, &HDF, &HE0, &HFD, &HFE, &H100, &H102, 260, &H105, &H106, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H123, &H124, &H126, &H128, &H12A, &H12B, 300
                    Return ObjectType.IDivideInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F
                    Return ObjectType.IDivideString(conv, empty, convertible2, typeCode)
                Case &H85
                    Return ObjectType.IDivideInt16(conv.ToInt16(Nothing), 0)
                Case &HAB
                    Return ObjectType.IDivideInt32(conv.ToInt32(Nothing), 0)
                Case &HAE
                    Return ObjectType.IDivideInt32(conv.ToInt32(Nothing), ObjectType.ToVBBool(convertible2))
                Case &HD1, &HF7, &H10A, &H11D
                    Return ObjectType.IDivideInt64(conv.ToInt64(Nothing), 0)
                Case &HD4, 250, &H10D, &H120
                    Return ObjectType.IDivideInt64(conv.ToInt64(Nothing), CLng(ObjectType.ToVBBool(convertible2)))
                Case &H156
                    Return ObjectType.IDivideInt64(LongType.FromString(conv.ToString(Nothing)), 0)
                Case &H159
                    Return ObjectType.IDivideInt64(LongType.FromString(conv.ToString(Nothing)), CLng(ObjectType.ToVBBool(convertible2)))
                Case &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.IDivideInt64(LongType.FromString(conv.ToString(Nothing)), convertible2.ToInt64(Nothing))
                Case 360
                    Return ObjectType.IDivideStringString(conv.ToString(Nothing), convertible2.ToString(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function InternalNegObj(obj As Object, conv As IConvertible, tc As TypeCode) As Object
            Select Case tc
                Case TypeCode.Empty
                    Return 0
                Case TypeCode.Boolean
                    If TypeOf obj Is Boolean Then
                        Return -CShort(-(CBool(obj) > False))
                    End If
                    Return -CShort(-(conv.ToBoolean(Nothing) > False))
                Case TypeCode.Byte
                    If TypeOf obj Is Byte Then
                        Return CShort(-CByte(obj))
                    End If
                    Return -conv.ToByte(Nothing)
                Case TypeCode.Int16
                    Dim num2 As Integer
                    If TypeOf obj Is Short Then
                        num2 = (0 - CShort(obj))
                    Else
                        num2 = (0 - conv.ToInt16(Nothing))
                    End If
                    If ((num2 < -32768) OrElse (num2 > &H7FFF)) Then
                        Return num2
                    End If
                    Return CShort(num2)
                Case TypeCode.Int32
                    Dim num3 As Long
                    If TypeOf obj Is Integer Then
                        num3 = (0 - CInt(obj))
                    Else
                        num3 = (0 - conv.ToInt32(Nothing))
                    End If
                    If ((num3 < -2147483648) OrElse (num3 > &H7FFFFFFF)) Then
                        Return num3
                    End If
                    Return CInt(num3)
                Case TypeCode.Int64
                    Try
                        If TypeOf obj Is Long Then
                            Return (0 - CLng(obj))
                        End If
                        Return (0 - conv.ToInt64(Nothing))
                    Catch exception As StackOverflowException
                        Throw exception
                    Catch exception2 As OutOfMemoryException
                        Throw exception2
                    Catch exception3 As ThreadAbortException
                        Throw exception3
                    Catch exception9 As Exception
                        Return Decimal.Negate(conv.ToDecimal(Nothing))
                    End Try
                    Exit Select
                Case TypeCode.Single
                    GoTo Label_01A3
                Case TypeCode.Double
                    If TypeOf obj Is Double Then
                        Return -CDbl(obj)
                    End If
                    Return -conv.ToDouble(Nothing)
                Case TypeCode.Decimal
                    Exit Select
                Case TypeCode.String
                    Dim str As String = TryCast(obj, String)
                    If (str Is Nothing) Then
                        Return -DoubleType.FromString(conv.ToString(Nothing))
                    End If
                    Return -DoubleType.FromString(str)
                Case Else
                    Throw ObjectType.GetNoValidOperatorException(obj)
            End Select
            Try
                If TypeOf obj Is Decimal Then
                    Return Decimal.Negate(CDec(obj))
                End If
                Return Decimal.Negate(conv.ToDecimal(Nothing))
            Catch exception4 As StackOverflowException
                Throw exception4
            Catch exception5 As OutOfMemoryException
                Throw exception5
            Catch exception6 As ThreadAbortException
                Throw exception6
            Catch exception13 As Exception
                Return -conv.ToDouble(Nothing)
            End Try
Label_01A3:
            If TypeOf obj Is Single Then
                Return -CSng(obj)
            End If
            Return -conv.ToSingle(Nothing)
        End Function

        Friend Shared Function IsWideningConversion(FromType As Type, ToType As Type) As Boolean
            Dim typeCode As TypeCode = Type.GetTypeCode(FromType)
            Dim typ As TypeCode = Type.GetTypeCode(ToType)
            If (typeCode = TypeCode.Object) Then
                If ((FromType Is GetType(Char())) AndAlso ((typ = TypeCode.String) OrElse (ToType Is GetType(Char())))) Then
                    Return True
                End If
                If (typ <> TypeCode.Object) Then
                    Return False
                End If
                If (FromType.IsArray AndAlso ToType.IsArray) Then
                    Return ((FromType.GetArrayRank = ToType.GetArrayRank) AndAlso ToType.GetElementType.IsAssignableFrom(FromType.GetElementType))
                End If
                Return ToType.IsAssignableFrom(FromType)
            End If
            If (typ = TypeCode.Object) Then
                If ((ToType Is GetType(Char())) AndAlso (typeCode = TypeCode.String)) Then
                    Return False
                End If
                Return ToType.IsAssignableFrom(FromType)
            End If
            If ToType.IsEnum Then
                Return False
            End If
            Dim cc As CC = ObjectType.ConversionClassTable(CInt(ObjectType.VType2FromTypeCode(typ)), CInt(ObjectType.VType2FromTypeCode(typeCode)))
            Return ((cc = CC.Wide) OrElse (cc = CC.Same))
        End Function

        Friend Shared Function IsWiderNumeric(Type1 As Type, Type2 As Type) As Boolean
            Dim typeCode As TypeCode = Type.GetTypeCode(Type1)
            Dim typCode As TypeCode = Type.GetTypeCode(Type2)
            If (Information.IsOldNumericTypeCode(typeCode) AndAlso Information.IsOldNumericTypeCode(typCode)) Then
                If ((typeCode = TypeCode.Boolean) OrElse (typCode = TypeCode.Boolean)) Then
                    Return False
                End If
                If Type1.IsEnum Then
                    Return False
                End If
                Return (ObjectType.WiderType(CInt(ObjectType.VTypeFromTypeCode(typeCode)), CInt(ObjectType.VTypeFromTypeCode(typCode))) = ObjectType.VTypeFromTypeCode(typeCode))
            End If
            Return False
        End Function

        Public Shared Function LikeObj(vLeft As Object, vRight As Object, CompareOption As CompareMethod) As Boolean
            Return StringType.StrLike(StringType.FromObject(vLeft), StringType.FromObject(vRight), CompareOption)
        End Function

        Private Shared Function ModByte(i1 As Byte, i2 As Byte) As Object
            Return CByte((i1 Mod i2))
        End Function

        Private Shared Function ModDecimal(conv1 As IConvertible, conv2 As IConvertible) As Object
            Dim num As Decimal
            Dim num2 As Decimal
            If (Not conv1 Is Nothing) Then
                num = conv1.ToDecimal(Nothing)
            End If
            If (Not conv2 Is Nothing) Then
                num2 = conv2.ToDecimal(Nothing)
            End If
            Return Decimal.Remainder(num, num2)
        End Function

        Private Shared Function ModDouble(d1 As Double, d2 As Double) As Object
            Return (d1 Mod d2)
        End Function

        Private Shared Function ModInt16(i1 As Short, i2 As Short) As Object
            Dim num As Integer = (i1 Mod i2)
            If ((num < -32768) OrElse (num > &H7FFF)) Then
                Return num
            End If
            Return CShort(num)
        End Function

        Private Shared Function ModInt32(i1 As Integer, i2 As Integer) As Object
            Dim num As Long = (CLng(i1) Mod CLng(i2))
            If ((num < -2147483648) OrElse (num > &H7FFFFFFF)) Then
                Return num
            End If
            Return CInt(num)
        End Function

        Private Shared Function ModInt64(i1 As Long, i2 As Long) As Object
            Try
                Return (i1 Mod i2)
            Catch exception As OverflowException
                Dim num As Decimal = Decimal.Remainder(New Decimal(i1), New Decimal(i2))
                If ((Decimal.Compare(num, -9223372036854775808) < 0) OrElse (Decimal.Compare(num, 9223372036854775807) > 0)) Then
                    Return num
                End If
                Return Convert.ToInt64(num)
            End Try
        End Function

        Public Shared Function ModObj(o1 As Object, o2 As Object) As Object
            Dim typeCode As TypeCode
            Dim empty As TypeCode
            Dim convertible As IConvertible = TryCast(o1, IConvertible)
            Dim conv As IConvertible = TryCast(o2, IConvertible)
            If (Not convertible Is Nothing) Then
                typeCode = convertible.GetTypeCode
            ElseIf (o1 Is Nothing) Then
                typeCode = TypeCode.Empty
            Else
                typeCode = TypeCode.Object
            End If
            If (Not conv Is Nothing) Then
                empty = conv.GetTypeCode
            Else
                conv = Nothing
                If (o2 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            End If
            Select Case CInt(((typeCode * (TypeCode.String Or TypeCode.Object)) + empty))
                Case 0
                    Return ObjectType.ModInt32(0, 0)
                Case 3
                    Return ObjectType.ModInt16(0, CShort(ObjectType.ToVBBool(conv)))
                Case 6
                    Return ObjectType.ModByte(0, conv.ToByte(Nothing))
                Case 7
                    Return ObjectType.ModInt16(0, CShort(ObjectType.ToVBBool(conv)))
                Case 9
                    Return ObjectType.ModInt32(0, conv.ToInt32(Nothing))
                Case 11
                    Return ObjectType.ModInt64(0, conv.ToInt64(Nothing))
                Case 13
                    Return ObjectType.ModSingle(0!, conv.ToSingle(Nothing))
                Case 14
                    Return ObjectType.ModDouble(0, conv.ToDouble(Nothing))
                Case 15
                    Return ObjectType.ModDecimal(Nothing, conv)
                Case &H12
                    Return ObjectType.ModString(convertible, typeCode, conv, empty)
                Case &H39
                    Return ObjectType.ModInt16(CShort(ObjectType.ToVBBool(convertible)), 0)
                Case 60
                    Return ObjectType.ModInt16(CShort(ObjectType.ToVBBool(convertible)), CShort(ObjectType.ToVBBool(conv)))
                Case &H3F, &H40
                    Return ObjectType.ModInt16(CShort(ObjectType.ToVBBool(convertible)), conv.ToInt16(Nothing))
                Case &H42
                    Return ObjectType.ModInt32(ObjectType.ToVBBool(convertible), conv.ToInt32(Nothing))
                Case &H44
                    Return ObjectType.ModInt64(CLng(ObjectType.ToVBBool(convertible)), conv.ToInt64(Nothing))
                Case 70
                    Return ObjectType.ModSingle(CSng(ObjectType.ToVBBool(convertible)), conv.ToSingle(Nothing))
                Case &H47
                    Return ObjectType.ModDouble(CDbl(ObjectType.ToVBBool(convertible)), conv.ToDouble(Nothing))
                Case &H48
                    Return ObjectType.ModDecimal(ObjectType.ToVBBoolConv(convertible), conv)
                Case &H4B
                    Return ObjectType.ModString(convertible, typeCode, conv, empty)
                Case &H72
                    Return ObjectType.ModByte(convertible.ToByte(Nothing), 0)
                Case &H75, &H88
                    Return ObjectType.ModInt16(convertible.ToInt16(Nothing), CShort(ObjectType.ToVBBool(conv)))
                Case 120
                    Return ObjectType.ModByte(convertible.ToByte(Nothing), conv.ToByte(Nothing))
                Case &H79, &H8B, 140
                    Return ObjectType.ModInt16(convertible.ToInt16(Nothing), conv.ToInt16(Nothing))
                Case &H7B, &H8E, &HB1, &HB2, 180
                    Return ObjectType.ModInt32(convertible.ToInt32(Nothing), conv.ToInt32(Nothing))
                Case &H7D, &H90, &HB6, &HD7, &HD8, &HDA, 220
                    Return ObjectType.ModInt64(convertible.ToInt64(Nothing), conv.ToInt64(Nothing))
                Case &H7F, &H92, &HB8, &HDE, &HFD, &HFE, &H100, &H102, 260, &H106, &H12A
                    Return ObjectType.ModSingle(convertible.ToSingle(Nothing), conv.ToSingle(Nothing))
                Case &H80, &H93, &HB9, &HDF, &H105, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H12B
                    Return ObjectType.ModDouble(convertible.ToDouble(Nothing), conv.ToDouble(Nothing))
                Case &H81, &H94, &HBA, &HE0, &H123, &H124, &H126, &H128, 300
                    Return ObjectType.ModDecimal(convertible, conv)
                Case &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F
                    Return ObjectType.ModString(convertible, typeCode, conv, empty)
                Case &H85
                    Return ObjectType.ModInt16(convertible.ToInt16(Nothing), 0)
                Case &HAB
                    Return ObjectType.ModInt32(convertible.ToInt32(Nothing), 0)
                Case &HAE
                    Return ObjectType.ModInt32(convertible.ToInt32(Nothing), ObjectType.ToVBBool(conv))
                Case &HD1
                    Return ObjectType.ModInt64(convertible.ToInt64(Nothing), 0)
                Case &HD4
                    Return ObjectType.ModInt64(convertible.ToInt64(Nothing), CLng(ObjectType.ToVBBool(conv)))
                Case &HF7
                    Return ObjectType.ModSingle(convertible.ToSingle(Nothing), 0!)
                Case 250
                    Return ObjectType.ModSingle(convertible.ToSingle(Nothing), CSng(ObjectType.ToVBBool(conv)))
                Case &H10A
                    Return ObjectType.ModDouble(convertible.ToDouble(Nothing), 0)
                Case &H10D
                    Return ObjectType.ModDouble(convertible.ToDouble(Nothing), CDbl(ObjectType.ToVBBool(conv)))
                Case &H11D
                    Return ObjectType.ModDecimal(convertible, Nothing)
                Case &H120
                    Return ObjectType.ModDecimal(convertible, ObjectType.ToVBBoolConv(conv))
                Case &H156
                    Return ObjectType.ModString(convertible, typeCode, conv, empty)
                Case &H159
                    Return ObjectType.ModString(convertible, typeCode, conv, empty)
                Case &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.ModString(convertible, typeCode, conv, empty)
                Case 360
                    Return ObjectType.ModStringString(convertible.ToString(Nothing), conv.ToString(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function ModSingle(sng1 As Single, sng2 As Single) As Object
            Return (sng1 Mod sng2)
        End Function

        Private Shared Function ModString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Object
            Dim num As Double
            Dim num2 As Double
            If (tc1 = TypeCode.String) Then
                num = DoubleType.FromString(conv1.ToString(Nothing))
            ElseIf (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToDouble(Nothing)
            End If
            If (tc2 = TypeCode.String) Then
                num2 = DoubleType.FromString(conv2.ToString(Nothing))
            ElseIf (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToDouble(Nothing)
            End If
            Return (num Mod num2)
        End Function

        Private Shared Function ModStringString(s1 As String, s2 As String) As Object
            Dim num As Double
            Dim num2 As Double
            If (Not s1 Is Nothing) Then
                num = DoubleType.FromString(s1)
            End If
            If (Not s2 Is Nothing) Then
                num2 = DoubleType.FromString(s2)
            End If
            Return (num Mod num2)
        End Function

        Private Shared Function MulByte(i1 As Byte, i2 As Byte) As Object
            Dim num As Integer = (i1 * i2)
            If ((num >= 0) AndAlso (num <= &HFF)) Then
                Return CByte(num)
            End If
            If ((num >= -32768) AndAlso (num <= &H7FFF)) Then
                Return CShort(num)
            End If
            Return num
        End Function

        Private Shared Function MulDecimal(conv1 As IConvertible, conv2 As IConvertible) As Object
            Dim num As Decimal = conv1.ToDecimal(Nothing)
            Dim num2 As Decimal = conv2.ToDecimal(Nothing)
            Try
                Return Decimal.Multiply(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToDouble(num) * Convert.ToDouble(num2))
            End Try
        End Function

        Private Shared Function MulDouble(d1 As Double, d2 As Double) As Object
            Return (d1 * d2)
        End Function

        Private Shared Function MulInt16(i1 As Short, i2 As Short) As Object
            Dim num As Integer = (i1 * i2)
            If ((num >= -32768) AndAlso (num <= &H7FFF)) Then
                Return CShort(num)
            End If
            Return num
        End Function

        Private Shared Function MulInt32(i1 As Integer, i2 As Integer) As Object
            Dim num As Long = (i1 * i2)
            If ((num >= -2147483648) AndAlso (num <= &H7FFFFFFF)) Then
                Return CInt(num)
            End If
            Return num
        End Function

        Private Shared Function MulInt64(i1 As Long, i2 As Long) As Object
            Dim obj2 As Object
            Try
                obj2 = (i1 * i2)
            Catch exception As OverflowException
                Try
                    obj2 = Decimal.Multiply(New Decimal(i1), New Decimal(i2))
                Catch exception2 As OverflowException
                    obj2 = (i1 * i2)
                End Try
            End Try
            Return obj2
        End Function

        Public Shared Function MulObj(o1 As Object, o2 As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(o1, IConvertible)
            If (conv Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(o2, IConvertible)
            If (convertible2 Is Nothing) Then
                If (o2 Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0, 9, &HAB
                    Return 0
                Case 3, 7, &H39, &H85
                    Return CShort(0)
                Case 6, &H72
                    Return CByte(0)
                Case 11, &HD1
                    Return 0
                Case 13, &HF7
                    Return 0!
                Case 14, &H10A
                    Return 0
                Case 15, &H11D
                    Return Decimal.Zero
                Case &H12, &H156
                    Return 0
                Case 60
                    Return ObjectType.MulInt16(CShort(ObjectType.ToVBBool(conv)), CShort(ObjectType.ToVBBool(convertible2)))
                Case &H3F, &H40
                    Return ObjectType.MulInt16(CShort(ObjectType.ToVBBool(conv)), convertible2.ToInt16(Nothing))
                Case &H42
                    Return ObjectType.MulInt32(ObjectType.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H44
                    Return ObjectType.MulInt64(CLng(ObjectType.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case 70
                    Return ObjectType.MulSingle(CSng(ObjectType.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H47
                    Return ObjectType.MulDouble(CDbl(ObjectType.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case &H48
                    Return ObjectType.MulDecimal(ObjectType.ToVBBoolConv(conv), convertible2)
                Case &H4B, &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F, &H159, &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.MulString(conv, empty, convertible2, typeCode)
                Case &H75, &H88
                    Return ObjectType.MulInt16(conv.ToInt16(Nothing), CShort(ObjectType.ToVBBool(convertible2)))
                Case 120
                    Return ObjectType.MulByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H79, &H8B, 140
                    Return ObjectType.MulInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H7B, &H8E, &HB1, &HB2, 180
                    Return ObjectType.MulInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H7D, &H90, &HB6, &HD7, &HD8, &HDA, 220
                    Return ObjectType.MulInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H7F, &H92, &HB8, &HDE, &HFD, &HFE, &H100, &H102, 260, &H106, &H12A
                    Return ObjectType.MulSingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H80, &H93, &HB9, &HDF, &H105, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H12B
                    Return ObjectType.MulDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H81, &H94, &HBA, &HE0, &H123, &H124, &H126, &H128, 300
                    Return ObjectType.MulDecimal(conv, convertible2)
                Case &HAE
                    Return ObjectType.MulInt32(conv.ToInt32(Nothing), ObjectType.ToVBBool(convertible2))
                Case &HD4
                    Return ObjectType.MulInt64(conv.ToInt64(Nothing), CLng(ObjectType.ToVBBool(convertible2)))
                Case 250
                    Return ObjectType.MulSingle(conv.ToSingle(Nothing), CSng(ObjectType.ToVBBool(convertible2)))
                Case &H10D
                    Return ObjectType.MulDouble(conv.ToDouble(Nothing), CDbl(ObjectType.ToVBBool(convertible2)))
                Case &H120
                    Return ObjectType.MulDecimal(conv, ObjectType.ToVBBoolConv(convertible2))
                Case 360
                    Return ObjectType.MulStringString(conv.ToString(Nothing), convertible2.ToString(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function MulSingle(f1 As Single, f2 As Single) As Object
            Dim d As Double = (f1 * f2)
            If ((d <= 3.4028234663852886E+38) AndAlso (d >= -3.4028234663852886E+38)) Then
                Return CSng(d)
            End If
            If (Double.IsInfinity(d) AndAlso (Single.IsInfinity(f1) OrElse Single.IsInfinity(f2))) Then
                Return CSng(d)
            End If
            Return d
        End Function

        Private Shared Function MulString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Object
            Dim num As Double
            Dim num2 As Double
            If (tc1 = TypeCode.String) Then
                num = DoubleType.FromString(conv1.ToString(Nothing))
            ElseIf (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToDouble(Nothing)
            End If
            If (tc2 = TypeCode.String) Then
                num2 = DoubleType.FromString(conv2.ToString(Nothing))
            ElseIf (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToDouble(Nothing)
            End If
            Return (num * num2)
        End Function

        Private Shared Function MulStringString(s1 As String, s2 As String) As Object
            Dim num As Double
            Dim num2 As Double
            If (Not s1 Is Nothing) Then
                num = DoubleType.FromString(s1)
            End If
            If (Not s2 Is Nothing) Then
                num2 = DoubleType.FromString(s2)
            End If
            Return (num * num2)
        End Function

        Public Shared Function NegObj(obj As Object) As Object
            Dim empty As TypeCode
            Dim conv As IConvertible = TryCast(obj, IConvertible)
            If (conv Is Nothing) Then
                If (obj Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Return ObjectType.InternalNegObj(obj, conv, empty)
        End Function

        Public Shared Function NotObj(obj As Object) As Object
            Dim type As Type
            Dim typeCode As TypeCode
            If (obj Is Nothing) Then
                Return -1
            End If
            Dim convertible As IConvertible = TryCast(obj, IConvertible)
            If (Not convertible Is Nothing) Then
                typeCode = convertible.GetTypeCode
            Else
                typeCode = TypeCode.Object
            End If
            Select Case typeCode
                Case TypeCode.Boolean
                    Return Not convertible.ToBoolean(Nothing)
                Case TypeCode.Byte
                    type = obj.GetType
                    Dim num As Byte = Not convertible.ToByte(Nothing)
                    If Not type.IsEnum Then
                        Return num
                    End If
                    Return [Enum].ToObject(type, num)
                Case TypeCode.Int16
                    type = obj.GetType
                    Dim num2 As Short = Not convertible.ToInt16(Nothing)
                    If Not type.IsEnum Then
                        Return num2
                    End If
                    Return [Enum].ToObject(type, num2)
                Case TypeCode.Int32
                    type = obj.GetType
                    Dim num3 As Integer = Not convertible.ToInt32(Nothing)
                    If Not type.IsEnum Then
                        Return num3
                    End If
                    Return [Enum].ToObject(type, num3)
                Case TypeCode.Int64
                    type = obj.GetType
                    Dim num4 As Long = Not convertible.ToInt64(Nothing)
                    If Not type.IsEnum Then
                        Return num4
                    End If
                    Return [Enum].ToObject(type, num4)
                Case TypeCode.Single
                    Return Not Convert.ToInt64(convertible.ToDecimal(Nothing))
                Case TypeCode.Double
                    Return Not Convert.ToInt64(convertible.ToDecimal(Nothing))
                Case TypeCode.Decimal
                    Return Not Convert.ToInt64(convertible.ToDecimal(Nothing))
                Case TypeCode.String
                    Return Not LongType.FromString(convertible.ToString(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj)
        End Function

        Public Shared Function ObjTst(o1 As Object, o2 As Object, TextCompare As Boolean) As Integer
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim convertible As IConvertible = TryCast(o1, IConvertible)
            If (convertible Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Dim conv As IConvertible = TryCast(o2, IConvertible)
            If (conv Is Nothing) Then
                If (o2 Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = conv.GetTypeCode
            End If
            If (((empty = TypeCode.Object) AndAlso TypeOf o1 Is Char()) AndAlso (((typeCode = TypeCode.String) OrElse (typeCode = TypeCode.Empty)) OrElse ((typeCode = TypeCode.Object) AndAlso TypeOf o2 Is Char()))) Then
                o1 = New String(CharArrayType.FromObject(o1))
                convertible = DirectCast(o1, IConvertible)
                empty = TypeCode.String
            End If
            If (((typeCode = TypeCode.Object) AndAlso TypeOf o2 Is Char()) AndAlso ((empty = TypeCode.String) OrElse (empty = TypeCode.Empty))) Then
                o2 = New String(CharArrayType.FromObject(o2))
                conv = DirectCast(o2, IConvertible)
                typeCode = TypeCode.String
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case &H72
                    Return ObjectType.ObjTstByte(convertible.ToByte(Nothing), 0)
                Case &H75, &H88
                    Return ObjectType.ObjTstInt16(convertible.ToInt16(Nothing), CShort(ObjectType.ToVBBool(conv)))
                Case 120
                    Return ObjectType.ObjTstByte(convertible.ToByte(Nothing), conv.ToByte(Nothing))
                Case &H79, &H8B, 140
                    Return ObjectType.ObjTstInt16(convertible.ToInt16(Nothing), conv.ToInt16(Nothing))
                Case &H7B, &H8E, &HB1, &HB2, 180
                    Return ObjectType.ObjTstInt32(convertible.ToInt32(Nothing), conv.ToInt32(Nothing))
                Case &H7D, &H90, &HB6, &HD7, &HD8, &HDA, 220
                    Return ObjectType.ObjTstInt64(convertible.ToInt64(Nothing), conv.ToInt64(Nothing))
                Case &H7F, &H92, &HB8, &HDE, &HFD, &HFE, &H100, &H102, 260, &H106, &H12A
                    Return ObjectType.ObjTstSingle(convertible.ToSingle(Nothing), conv.ToSingle(Nothing))
                Case &H80, &H93, &HB9, &HDF, &H105, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H12B
                    Return ObjectType.ObjTstDouble(convertible.ToDouble(Nothing), conv.ToDouble(Nothing))
                Case &H81, &H94, &HBA, &HE0, &H123, &H124, &H126, &H128, 300
                    Return ObjectType.ObjTstDecimal(convertible, conv)
                Case &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F
                    Return ObjectType.ObjTstString(convertible, empty, conv, typeCode)
                Case &H85
                    Return ObjectType.ObjTstInt16(convertible.ToInt16(Nothing), 0)
                Case &H5E, &H15A
                    Return ObjectType.ObjTstStringString(convertible.ToString(Nothing), conv.ToString(Nothing), TextCompare)
                Case 80
                    Return ObjectType.ObjTstChar(convertible.ToChar(Nothing), conv.ToChar(Nothing))
                Case 0
                    Return 0
                Case 3
                    Return ObjectType.ObjTstInt32(0, ObjectType.ToVBBool(conv))
                Case 4
                    Return ObjectType.ObjTstChar(ChrW(0), conv.ToChar(Nothing))
                Case 6
                    Return ObjectType.ObjTstByte(0, conv.ToByte(Nothing))
                Case 7
                    Return ObjectType.ObjTstInt16(0, conv.ToInt16(Nothing))
                Case 9
                    Return ObjectType.ObjTstInt32(0, conv.ToInt32(Nothing))
                Case 11
                    Return ObjectType.ObjTstInt64(0, conv.ToInt64(Nothing))
                Case 13
                    Return ObjectType.ObjTstSingle(0!, conv.ToSingle(Nothing))
                Case 14
                    Return ObjectType.ObjTstDouble(0, conv.ToDouble(Nothing))
                Case 15
                    Return ObjectType.ObjTstDecimal(DirectCast(0, IConvertible), conv)
                Case &H10
                    Return ObjectType.ObjTstDateTime(DateType.FromObject(Nothing), conv.ToDateTime(Nothing))
                Case &H12
                    Return ObjectType.ObjTstStringString(Nothing, o2.ToString, TextCompare)
                Case &H39
                    Return ObjectType.ObjTstInt32(ObjectType.ToVBBool(convertible), 0)
                Case 60
                    Return ObjectType.ObjTstInt16(CShort(ObjectType.ToVBBool(convertible)), CShort(ObjectType.ToVBBool(conv)))
                Case &H3F, &H40
                    Return ObjectType.ObjTstInt16(CShort(ObjectType.ToVBBool(convertible)), conv.ToInt16(Nothing))
                Case &H42
                    Return ObjectType.ObjTstInt32(ObjectType.ToVBBool(convertible), conv.ToInt32(Nothing))
                Case &H44
                    Return ObjectType.ObjTstInt64(CLng(ObjectType.ToVBBool(convertible)), conv.ToInt64(Nothing))
                Case 70
                    Return ObjectType.ObjTstSingle(CSng(ObjectType.ToVBBool(convertible)), conv.ToSingle(Nothing))
                Case &H47
                    Return ObjectType.ObjTstDouble(CDbl(ObjectType.ToVBBool(convertible)), conv.ToDouble(Nothing))
                Case &H48
                    Return ObjectType.ObjTstDecimal(DirectCast(ObjectType.ToVBBool(convertible), IConvertible), conv)
                Case &H4B
                    Return ObjectType.ObjTstBoolean(convertible.ToBoolean(Nothing), BooleanType.FromString(conv.ToString(Nothing)))
                Case &H4C
                    Return ObjectType.ObjTstChar(convertible.ToChar(Nothing), ChrW(0))
                Case &HAB
                    Return ObjectType.ObjTstInt32(convertible.ToInt32(Nothing), 0)
                Case &HAE
                    Return ObjectType.ObjTstInt32(convertible.ToInt32(Nothing), ObjectType.ToVBBool(conv))
                Case &HD1
                    Return ObjectType.ObjTstInt64(convertible.ToInt64(Nothing), 0)
                Case &HD4
                    Return ObjectType.ObjTstInt64(convertible.ToInt64(Nothing), CLng(ObjectType.ToVBBool(conv)))
                Case &HF7
                    Return ObjectType.ObjTstSingle(convertible.ToSingle(Nothing), 0!)
                Case 250
                    Return ObjectType.ObjTstSingle(convertible.ToSingle(Nothing), CSng(ObjectType.ToVBBool(conv)))
                Case &H10A
                    Return ObjectType.ObjTstDouble(convertible.ToDouble(Nothing), 0)
                Case &H10D
                    Return ObjectType.ObjTstDouble(convertible.ToDouble(Nothing), CDbl(ObjectType.ToVBBool(conv)))
                Case &H11D
                    Return ObjectType.ObjTstDecimal(convertible, DirectCast(0, IConvertible))
                Case &H120
                    Return ObjectType.ObjTstDecimal(convertible, DirectCast(ObjectType.ToVBBool(conv), IConvertible))
                Case &H130
                    Return ObjectType.ObjTstDateTime(convertible.ToDateTime(Nothing), DateType.FromObject(Nothing))
                Case &H156
                    Return ObjectType.ObjTstStringString(o1.ToString, Nothing, TextCompare)
                Case &H159
                    Return ObjectType.ObjTstBoolean(BooleanType.FromString(convertible.ToString(Nothing)), conv.ToBoolean(Nothing))
                Case &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.ObjTstString(convertible, empty, conv, typeCode)
                Case &H166
                    Return ObjectType.ObjTstDateTime(DateType.FromString(convertible.ToString(Nothing), Utils.GetCultureInfo), conv.ToDateTime(Nothing))
                Case 360
                    Return ObjectType.ObjTstStringString(convertible.ToString(Nothing), conv.ToString(Nothing), TextCompare)
                Case &H142
                    Return ObjectType.ObjTstDateTime(convertible.ToDateTime(Nothing), DateType.FromString(conv.ToString(Nothing), Utils.GetCultureInfo))
                Case 320
                    Return ObjectType.ObjTstDateTime(convertible.ToDateTime(Nothing), conv.ToDateTime(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function ObjTstBoolean(b1 As Boolean, b2 As Boolean) As Integer
            If (b1 = b2) Then
                Return 0
            End If
            If (b1 < b2) Then
                Return 1
            End If
            Return -1
        End Function

        Private Shared Function ObjTstByte(by1 As Byte, by2 As Byte) As Integer
            If (by1 < by2) Then
                Return -1
            End If
            If (by1 > by2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstChar(ch1 As Char, ch2 As Char) As Integer
            If (ch1 < ch2) Then
                Return -1
            End If
            If (ch1 > ch2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstDateTime(var1 As DateTime, var2 As DateTime) As Integer
            Dim ticks As Long = var1.Ticks
            Dim num3 As Long = var2.Ticks
            If (ticks < num3) Then
                Return -1
            End If
            If (ticks > num3) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstDecimal(i1 As IConvertible, i2 As IConvertible) As Integer
            Dim num2 As Decimal = i1.ToDecimal(Nothing)
            Dim num3 As Decimal = i2.ToDecimal(Nothing)
            If (Decimal.Compare(num2, num3) < 0) Then
                Return -1
            End If
            If (Decimal.Compare(num2, num3) > 0) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstDouble(d1 As Double, d2 As Double) As Integer
            If (d1 < d2) Then
                Return -1
            End If
            If (d1 > d2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstInt16(d1 As Short, d2 As Short) As Integer
            If (d1 < d2) Then
                Return -1
            End If
            If (d1 > d2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstInt32(d1 As Integer, d2 As Integer) As Integer
            If (d1 < d2) Then
                Return -1
            End If
            If (d1 > d2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstInt64(d1 As Long, d2 As Long) As Integer
            If (d1 < d2) Then
                Return -1
            End If
            If (d1 > d2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstSingle(d1 As Single, d2 As Single) As Integer
            If (d1 < d2) Then
                Return -1
            End If
            If (d1 > d2) Then
                Return 1
            End If
            Return 0
        End Function

        Private Shared Function ObjTstString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Integer
            Dim num As Double
            Dim num2 As Double
            If (tc1 = TypeCode.String) Then
                num = DoubleType.FromString(conv1.ToString(Nothing))
            ElseIf (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToDouble(Nothing)
            End If
            If (tc2 = TypeCode.String) Then
                num2 = DoubleType.FromString(conv2.ToString(Nothing))
            ElseIf (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToDouble(Nothing)
            End If
            Return ObjectType.ObjTstDouble(num, num2)
        End Function

        Private Shared Function ObjTstStringString(s1 As String, s2 As String, TextCompare As Boolean) As Integer
            If (s1 Is Nothing) Then
                If (s2.Length > 0) Then
                    Return -1
                End If
                Return 0
            End If
            If (s2 Is Nothing) Then
                If (s1.Length > 0) Then
                    Return 1
                End If
                Return 0
            End If
            If TextCompare Then
                Return Utils.GetCultureInfo.CompareInfo.Compare(s1, s2, (CompareOptions.IgnoreWidth Or (CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreCase)))
            End If
            Return String.CompareOrdinal(s1, s2)
        End Function

        Public Shared Function PlusObj(obj As Object) As Object
            Dim empty As TypeCode
            If (obj Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(obj, IConvertible)
            If (convertible Is Nothing) Then
                If (obj Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return 0
                Case TypeCode.Boolean
                    If TypeOf obj Is Boolean Then
                        Return CShort(-(CBool(obj) > False))
                    End If
                    Return CShort(-(convertible.ToBoolean(Nothing) > False))
                Case TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return obj
                Case TypeCode.String
                    Return DoubleType.FromObject(obj)
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj)
        End Function

        Public Shared Function PowObj(obj1 As Object, obj2 As Object) As Object
            If ((obj1 Is Nothing) AndAlso (obj2 Is Nothing)) Then
                Return 1
            End If
            Select Case ObjectType.GetWidestType(obj1, obj2, False)
                Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                    Return Math.Pow(DoubleType.FromObject(obj1), DoubleType.FromObject(obj2))
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj1, obj2)
        End Function

        Public Shared Function ShiftLeftObj(o1 As Object, amount As Integer) As Object
            Dim empty As TypeCode
            Dim convertible As IConvertible = TryCast(o1, IConvertible)
            If (convertible Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return (CInt(0) << amount)
                Case TypeCode.Boolean
                    Return CShort((CShort(-(convertible.ToBoolean(Nothing) > 0)) << (amount And 15)))
                Case TypeCode.Byte
                    Return CByte((convertible.ToByte(Nothing) << (amount And 7)))
                Case TypeCode.Int16
                    Return CShort((convertible.ToInt16(Nothing) << (amount And 15)))
                Case TypeCode.Int32
                    Return (convertible.ToInt32(Nothing) << amount)
                Case TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return (convertible.ToInt64(Nothing) << amount)
                Case TypeCode.String
                    Return (LongType.FromString(convertible.ToString(Nothing)) << amount)
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1)
        End Function

        Public Shared Function ShiftRightObj(o1 As Object, amount As Integer) As Object
            Dim empty As TypeCode
            Dim convertible As IConvertible = TryCast(o1, IConvertible)
            If (convertible Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = convertible.GetTypeCode
            End If
            Select Case empty
                Case TypeCode.Empty
                    Return (CInt(0) >> amount)
                Case TypeCode.Boolean
                    Return CShort((CShort(-(convertible.ToBoolean(Nothing) > 0)) >> (amount And 15)))
                Case TypeCode.Byte
                    Return CByte((convertible.ToByte(Nothing) >> (amount And 7)))
                Case TypeCode.Int16
                    Return CShort((convertible.ToInt16(Nothing) >> (amount And 15)))
                Case TypeCode.Int32
                    Return (convertible.ToInt32(Nothing) >> amount)
                Case TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return (convertible.ToInt64(Nothing) >> amount)
                Case TypeCode.String
                    Return (LongType.FromString(convertible.ToString(Nothing)) >> amount)
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1)
        End Function

        Public Shared Function StrCatObj(vLeft As Object, vRight As Object) As Object
            Dim flag As Boolean = TypeOf vLeft Is DBNull
            Dim flag2 As Boolean = TypeOf vRight Is DBNull
            If (flag And flag2) Then
                Return vLeft
            End If
            If (flag And Not flag2) Then
                vLeft = ""
            ElseIf (flag2 And Not flag) Then
                vRight = ""
            End If
            Return (StringType.FromObject(vLeft) & StringType.FromObject(vRight))
        End Function

        Private Shared Function SubByte(i1 As Byte, i2 As Byte) As Object
            Dim num As Short = CShort((i1 - i2))
            If ((num >= 0) AndAlso (num <= &HFF)) Then
                Return CByte(num)
            End If
            Return num
        End Function

        Private Shared Function SubDecimal(conv1 As IConvertible, conv2 As IConvertible) As Object
            Dim num As Decimal = conv1.ToDecimal(Nothing)
            Dim num2 As Decimal = conv2.ToDecimal(Nothing)
            Try
                Return Decimal.Subtract(num, num2)
            Catch exception As OverflowException
                Return (Convert.ToDouble(num) - Convert.ToDouble(num2))
            End Try
        End Function

        Private Shared Function SubDouble(d1 As Double, d2 As Double) As Object
            Return (d1 - d2)
        End Function

        Private Shared Function SubInt16(i1 As Short, i2 As Short) As Object
            Dim num As Integer = (i1 - i2)
            If ((num >= -32768) AndAlso (num <= &H7FFF)) Then
                Return CShort(num)
            End If
            Return num
        End Function

        Private Shared Function SubInt32(i1 As Integer, i2 As Integer) As Object
            Dim num As Long = (i1 - i2)
            If ((num >= -2147483648) AndAlso (num <= &H7FFFFFFF)) Then
                Return CInt(num)
            End If
            Return num
        End Function

        Private Shared Function SubInt64(i1 As Long, i2 As Long) As Object
            Dim obj2 As Object
            Try
                obj2 = (i1 - i2)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                obj2 = Decimal.Subtract(New Decimal(i1), New Decimal(i2))
            End Try
            Return obj2
        End Function

        Public Shared Function SubObj(o1 As Object, o2 As Object) As Object
            Dim empty As TypeCode
            Dim typeCode As TypeCode
            Dim conv As IConvertible = TryCast(o1, IConvertible)
            If (conv Is Nothing) Then
                If (o1 Is Nothing) Then
                    empty = TypeCode.Empty
                Else
                    empty = TypeCode.Object
                End If
            Else
                empty = conv.GetTypeCode
            End If
            Dim convertible2 As IConvertible = TryCast(o2, IConvertible)
            If (convertible2 Is Nothing) Then
                If (o2 Is Nothing) Then
                    typeCode = TypeCode.Empty
                Else
                    typeCode = TypeCode.Object
                End If
            Else
                typeCode = convertible2.GetTypeCode
            End If
            Select Case CInt(((empty * (TypeCode.String Or TypeCode.Object)) + typeCode))
                Case 0
                    Return 0
                Case 3, 6, 7, 9, 11, 13, 14, 15
                    Return ObjectType.InternalNegObj(o2, convertible2, typeCode)
                Case &H12
                    Return ObjectType.SubStringString(Nothing, convertible2.ToString(Nothing))
                Case &H39
                    Return o1
                Case 60
                    Return ObjectType.SubInt16(CShort(ObjectType.ToVBBool(conv)), CShort(ObjectType.ToVBBool(convertible2)))
                Case &H3F, &H40
                    Return ObjectType.SubInt16(CShort(ObjectType.ToVBBool(conv)), convertible2.ToInt16(Nothing))
                Case &H42
                    Return ObjectType.SubInt32(ObjectType.ToVBBool(conv), convertible2.ToInt32(Nothing))
                Case &H44
                    Return ObjectType.SubInt64(CLng(ObjectType.ToVBBool(conv)), convertible2.ToInt64(Nothing))
                Case 70
                    Return ObjectType.SubSingle(CSng(ObjectType.ToVBBool(conv)), convertible2.ToSingle(Nothing))
                Case &H47
                    Return ObjectType.SubDouble(CDbl(ObjectType.ToVBBool(conv)), convertible2.ToDouble(Nothing))
                Case &H48
                    Return ObjectType.SubDecimal(ObjectType.ToVBBoolConv(conv), convertible2)
                Case &H4B, &H84, &H97, &HBD, &HE3, &H109, &H11C, &H12F, &H159, &H15C, &H15D, &H15F, &H161, &H163, &H164, &H165
                    Return ObjectType.SubString(conv, empty, convertible2, typeCode)
                Case &H72, &H85
                    Return o1
                Case &H75, &H88
                    Return ObjectType.SubInt16(conv.ToInt16(Nothing), CShort(ObjectType.ToVBBool(convertible2)))
                Case 120
                    Return ObjectType.SubByte(conv.ToByte(Nothing), convertible2.ToByte(Nothing))
                Case &H79, &H8B, 140
                    Return ObjectType.SubInt16(conv.ToInt16(Nothing), convertible2.ToInt16(Nothing))
                Case &H7B, &H8E, &HB1, &HB2, 180
                    Return ObjectType.SubInt32(conv.ToInt32(Nothing), convertible2.ToInt32(Nothing))
                Case &H7D, &H90, &HB6, &HD7, &HD8, &HDA, 220
                    Return ObjectType.SubInt64(conv.ToInt64(Nothing), convertible2.ToInt64(Nothing))
                Case &H7F, &H92, &HB8, &HDE, &HFD, &HFE, &H100, &H102, 260, &H106, &H12A
                    Return ObjectType.SubSingle(conv.ToSingle(Nothing), convertible2.ToSingle(Nothing))
                Case &H80, &H93, &HB9, &HDF, &H105, &H110, &H111, &H113, &H115, &H117, 280, &H119, &H12B
                    Return ObjectType.SubDouble(conv.ToDouble(Nothing), convertible2.ToDouble(Nothing))
                Case &H81, &H94, &HBA, &HE0, &H123, &H124, &H126, &H128, 300
                    Return ObjectType.SubDecimal(conv, convertible2)
                Case &HAB
                    Return o1
                Case &HAE
                    Return ObjectType.SubInt32(conv.ToInt32(Nothing), ObjectType.ToVBBool(convertible2))
                Case &HD1
                    Return o1
                Case &HD4
                    Return ObjectType.SubInt64(conv.ToInt64(Nothing), CLng(ObjectType.ToVBBool(convertible2)))
                Case &HF7, &H10A, &H11D
                    Return o1
                Case 250
                    Return ObjectType.SubSingle(conv.ToSingle(Nothing), CSng(ObjectType.ToVBBool(convertible2)))
                Case &H10D
                    Return ObjectType.SubDouble(conv.ToDouble(Nothing), CDbl(ObjectType.ToVBBool(convertible2)))
                Case &H120
                    Return ObjectType.SubDecimal(conv, ObjectType.ToVBBoolConv(convertible2))
                Case &H156
                    Return ObjectType.SubStringString(conv.ToString(Nothing), Nothing)
                Case 360
                    Return ObjectType.SubStringString(conv.ToString(Nothing), convertible2.ToString(Nothing))
            End Select
            Throw ObjectType.GetNoValidOperatorException(o1, o2)
        End Function

        Private Shared Function SubSingle(f1 As Single, f2 As Single) As Object
            Dim d As Double = (f1 - f2)
            If ((d <= 3.4028234663852886E+38) AndAlso (d >= -3.4028234663852886E+38)) Then
                Return CSng(d)
            End If
            If (Double.IsInfinity(d) AndAlso (Single.IsInfinity(f1) OrElse Single.IsInfinity(f2))) Then
                Return CSng(d)
            End If
            Return d
        End Function

        Private Shared Function SubString(conv1 As IConvertible, tc1 As TypeCode, conv2 As IConvertible, tc2 As TypeCode) As Object
            Dim num As Double
            Dim num2 As Double
            If (tc1 = TypeCode.String) Then
                num = DoubleType.FromString(conv1.ToString(Nothing))
            ElseIf (tc1 = TypeCode.Boolean) Then
                num = ObjectType.ToVBBool(conv1)
            Else
                num = conv1.ToDouble(Nothing)
            End If
            If (tc2 = TypeCode.String) Then
                num2 = DoubleType.FromString(conv2.ToString(Nothing))
            ElseIf (tc2 = TypeCode.Boolean) Then
                num2 = ObjectType.ToVBBool(conv2)
            Else
                num2 = conv2.ToDouble(Nothing)
            End If
            Return (num - num2)
        End Function

        Private Shared Function SubStringString(s1 As String, s2 As String) As Object
            Dim num As Double
            Dim num2 As Double
            If (Not s1 Is Nothing) Then
                num = DoubleType.FromString(s1)
            End If
            If (Not s2 Is Nothing) Then
                num2 = DoubleType.FromString(s2)
            End If
            Return (num - num2)
        End Function

        Private Shared Function ToVBBool(conv As IConvertible) As Integer
            If conv.ToBoolean(Nothing) Then
                Return -1
            End If
            Return 0
        End Function

        Private Shared Function ToVBBoolConv(conv As IConvertible) As IConvertible
            If conv.ToBoolean(Nothing) Then
                Return DirectCast(-1, IConvertible)
            End If
            Return 0
        End Function

        Private Shared Function TypeCodeFromVType(vartyp As VType) As TypeCode
            Select Case vartyp
                Case VType.t_bool
                    Return TypeCode.Boolean
                Case VType.t_ui1
                    Return TypeCode.Byte
                Case VType.t_i2
                    Return TypeCode.Int16
                Case VType.t_i4
                    Return TypeCode.Int32
                Case VType.t_i8
                    Return TypeCode.Int64
                Case VType.t_dec
                    Return TypeCode.Decimal
                Case VType.t_r4
                    Return TypeCode.Single
                Case VType.t_r8
                    Return TypeCode.Double
                Case VType.t_char
                    Return TypeCode.Char
                Case VType.t_str
                    Return TypeCode.String
                Case VType.t_date
                    Return TypeCode.DateTime
            End Select
            Return TypeCode.Object
        End Function

        Friend Shared Function TypeFromTypeCode(vartyp As TypeCode) As Type
            Select Case vartyp
                Case TypeCode.Object
                    Return GetType(Object)
                Case TypeCode.DBNull
                    Return GetType(DBNull)
                Case TypeCode.Boolean
                    Return GetType(Boolean)
                Case TypeCode.Char
                    Return GetType(Char)
                Case TypeCode.SByte
                    Return GetType(SByte)
                Case TypeCode.Byte
                    Return GetType(Byte)
                Case TypeCode.Int16
                    Return GetType(Short)
                Case TypeCode.UInt16
                    Return GetType(UInt16)
                Case TypeCode.Int32
                    Return GetType(Integer)
                Case TypeCode.UInt32
                    Return GetType(UInt32)
                Case TypeCode.Int64
                    Return GetType(Long)
                Case TypeCode.UInt64
                    Return GetType(UInt64)
                Case TypeCode.Single
                    Return GetType(Single)
                Case TypeCode.Double
                    Return GetType(Double)
                Case TypeCode.Decimal
                    Return GetType(Decimal)
                Case TypeCode.DateTime
                    Return GetType(DateTime)
                Case TypeCode.String
                    Return GetType(String)
            End Select
            Return Nothing
        End Function

        Private Shared Function VType2FromTypeCode(typ As TypeCode) As VType2
            Select Case typ
                Case TypeCode.Boolean
                    Return VType2.t_bool
                Case TypeCode.Char
                    Return VType2.t_char
                Case TypeCode.Byte
                    Return VType2.t_ui1
                Case TypeCode.Int16
                    Return VType2.t_i2
                Case TypeCode.Int32
                    Return VType2.t_i4
                Case TypeCode.Int64
                    Return VType2.t_i8
                Case TypeCode.Single
                    Return VType2.t_r4
                Case TypeCode.Double
                    Return VType2.t_r8
                Case TypeCode.Decimal
                    Return VType2.t_dec
                Case TypeCode.DateTime
                    Return VType2.t_date
                Case TypeCode.String
                    Return VType2.t_str
            End Select
            Return VType2.t_bad
        End Function

        Private Shared Function VTypeFromTypeCode(typ As TypeCode) As VType
            Select Case typ
                Case TypeCode.Boolean
                    Return VType.t_bool
                Case TypeCode.Char
                    Return VType.t_char
                Case TypeCode.Byte
                    Return VType.t_ui1
                Case TypeCode.Int16
                    Return VType.t_i2
                Case TypeCode.Int32
                    Return VType.t_i4
                Case TypeCode.Int64
                    Return VType.t_i8
                Case TypeCode.Single
                    Return VType.t_r4
                Case TypeCode.Double
                    Return VType.t_r8
                Case TypeCode.Decimal
                    Return VType.t_dec
                Case TypeCode.DateTime
                    Return VType.t_date
                Case TypeCode.String
                    Return VType.t_str
            End Select
            Return VType.t_bad
        End Function

        Public Shared Function XorObj(obj1 As Object, obj2 As Object) As Object
            If ((obj1 Is Nothing) AndAlso (obj2 Is Nothing)) Then
                Return False
            End If
            Select Case ObjectType.GetWidestType(obj1, obj2, False)
                Case TypeCode.Boolean, TypeCode.Byte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.String
                    Return (BooleanType.FromObject(obj1) Xor BooleanType.FromObject(obj2))
            End Select
            Throw ObjectType.GetNoValidOperatorException(obj1, obj2)
        End Function


        ' Fields
        Private Shared ReadOnly ConversionClassTable As CC() = New CC() {CC.Err}
        Private Const TCMAX As Integer = &H13
        Private Shared ReadOnly WiderType As VType() = New VType() {VType.t_bad}

        ' Nested Types
        Private Enum CC As Byte
            ' Fields
            Err = 0
            Narr = 2
            Same = 1
            Wide = 3
        End Enum

        Private Enum VType
            ' Fields
            t_bad = 0
            t_bool = 1
            t_char = 9
            t_date = 11
            t_dec = 6
            t_i2 = 3
            t_i4 = 4
            t_i8 = 5
            t_r4 = 7
            t_r8 = 8
            t_str = 10
            t_ui1 = 2
        End Enum

        Private Enum VType2
            ' Fields
            t_bad = 0
            t_bool = 1
            t_char = 3
            t_date = 9
            t_dec = 10
            t_i2 = 4
            t_i4 = 5
            t_i8 = 6
            t_r4 = 7
            t_r8 = 8
            t_ref = 11
            t_str = 12
            t_ui1 = 2
        End Enum
    End Class
End Namespace

