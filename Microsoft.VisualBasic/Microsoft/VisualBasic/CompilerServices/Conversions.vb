Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Dynamic
Imports System.Globalization
Imports System.Reflection
Imports System.Security
Imports System.Security.Permissions
Imports System.Threading
Imports Microsoft.VisualBasic.CompilerServices.ConversionResolution

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute> _
    Public NotInheritable Class Conversions
        ' Methods
        Private Sub New()
        End Sub

        Friend Shared Function CanUserDefinedConvert(Expression As Object, TargetType As Type) As Boolean
            Dim sourceType As Type = Expression.GetType
            If (((ConversionResolution.ClassifyPredefinedConversion(TargetType, sourceType) = ConversionClass.None) AndAlso (Symbols.IsClassOrValueType(sourceType) OrElse Symbols.IsClassOrValueType(TargetType))) AndAlso (Not Symbols.IsIntrinsicType(sourceType) OrElse Not Symbols.IsIntrinsicType(TargetType))) Then
                Dim operatorMethod As Method = Nothing
                ConversionResolution.ClassifyUserDefinedConversion(TargetType, sourceType, operatorMethod)
                Return (operatorMethod IsNot Nothing)
            End If
            Return False
        End Function

        Private Shared Function CastByteEnum(Expression As Byte, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastInt16Enum(Expression As Short, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastInt32Enum(Expression As Integer, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastInt64Enum(Expression As Long, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastSByteEnum(Expression As SByte, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastUInt16Enum(Expression As UInt16, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastUInt32Enum(Expression As UInt32, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function CastUInt64Enum(Expression As UInt64, TargetType As Type) As Object
            If Symbols.IsEnum(TargetType) Then
                Return [Enum].ToObject(TargetType, Expression)
            End If
            Return Expression
        End Function

        Private Shared Function ChangeIntrinsicType(Expression As Object, TargetType As Type) As Object
            Select Case Symbols.GetTypeCode(TargetType)
                Case TypeCode.Boolean
                    Return Conversions.ToBoolean(Expression)
                Case TypeCode.Char
                    Return Conversions.ToChar(Expression)
                Case TypeCode.SByte
                    Return Conversions.CastSByteEnum(Conversions.ToSByte(Expression), TargetType)
                Case TypeCode.Byte
                    Return Conversions.CastByteEnum(Conversions.ToByte(Expression), TargetType)
                Case TypeCode.Int16
                    Return Conversions.CastInt16Enum(Conversions.ToShort(Expression), TargetType)
                Case TypeCode.UInt16
                    Return Conversions.CastUInt16Enum(Conversions.ToUShort(Expression), TargetType)
                Case TypeCode.Int32
                    Return Conversions.CastInt32Enum(Conversions.ToInteger(Expression), TargetType)
                Case TypeCode.UInt32
                    Return Conversions.CastUInt32Enum(Conversions.ToUInteger(Expression), TargetType)
                Case TypeCode.Int64
                    Return Conversions.CastInt64Enum(Conversions.ToLong(Expression), TargetType)
                Case TypeCode.UInt64
                    Return Conversions.CastUInt64Enum(Conversions.ToULong(Expression), TargetType)
                Case TypeCode.Single
                    Return Conversions.ToSingle(Expression)
                Case TypeCode.Double
                    Return Conversions.ToDouble(Expression)
                Case TypeCode.Decimal
                    Return Conversions.ToDecimal(Expression)
                Case TypeCode.DateTime
                    Return Conversions.ToDate(Expression)
                Case TypeCode.String
                    Return Conversions.ToString(Expression)
            End Select
            Throw New Exception
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ChangeType(Expression As Object, TargetType As Type) As Object
            Return Conversions.ChangeType(Expression, TargetType, False)
        End Function

        <SecuritySafeCritical>
        Friend Shared Function ChangeType(Expression As Object, TargetType As Type, Dynamic As Boolean) As Object
            If (TargetType Is Nothing) Then
                Dim args As String() = New String() {"TargetType"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            If (Expression Is Nothing) Then
                If Symbols.IsValueType(TargetType) Then
                    Call New ReflectionPermission(ReflectionPermissionFlag.NoFlags).Demand
                    Return Activator.CreateInstance(TargetType)
                End If
                Return Nothing
            End If
            Dim type As Type = Expression.GetType
            If TargetType.IsByRef Then
                TargetType = TargetType.GetElementType
            End If
            If ((TargetType Is type) OrElse Symbols.IsRootObjectType(TargetType)) Then
                Return Expression
            End If
            If (Symbols.IsIntrinsicType(Symbols.GetTypeCode(TargetType)) AndAlso Symbols.IsIntrinsicType(Symbols.GetTypeCode(type))) Then
                Return Conversions.ChangeIntrinsicType(Expression, TargetType)
            End If
            If TargetType.IsInstanceOfType(Expression) Then
                Return Expression
            End If
            If (Symbols.IsCharArrayRankOne(TargetType) AndAlso Symbols.IsStringType(type)) Then
                Return Conversions.ToCharArrayRankOne(CStr(Expression))
            End If
            If (Symbols.IsStringType(TargetType) AndAlso Symbols.IsCharArrayRankOne(type)) Then
                Return New String(DirectCast(Expression, Char()))
            End If
            If Dynamic Then
                Dim expression As IDynamicMetaObjectProvider = IDOUtils.TryCastToIDMOP(expression)
                If (Not expression Is Nothing) Then
                    Return IDOBinder.UserDefinedConversion(expression, TargetType)
                End If
            End If
            Return Conversions.ObjectUserDefinedConversion(Expression, TargetType)
        End Function

        <Obsolete("do not use this method", True), DebuggerHidden, DebuggerStepThrough, DynamicallyInvokableAttribute>
        Public Shared Function FallbackUserDefinedConversion(Expression As Object, TargetType As Type) As Object
            Return Conversions.ObjectUserDefinedConversion(Expression, TargetType)
        End Function

        Friend Shared Function ForceValueCopy(Expression As Object, TargetType As Type) As Object
            Dim convertible As IConvertible = TryCast(Expression, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Empty, TypeCode.Object, TypeCode.DBNull, (TypeCode.DateTime Or TypeCode.Object), TypeCode.String
                        Return Expression
                    Case TypeCode.Boolean
                        Return convertible.ToBoolean(Nothing)
                    Case TypeCode.Char
                        Return convertible.ToChar(Nothing)
                    Case TypeCode.SByte
                        Return Conversions.CastSByteEnum(convertible.ToSByte(Nothing), TargetType)
                    Case TypeCode.Byte
                        Return Conversions.CastByteEnum(convertible.ToByte(Nothing), TargetType)
                    Case TypeCode.Int16
                        Return Conversions.CastInt16Enum(convertible.ToInt16(Nothing), TargetType)
                    Case TypeCode.UInt16
                        Return Conversions.CastUInt16Enum(convertible.ToUInt16(Nothing), TargetType)
                    Case TypeCode.Int32
                        Return Conversions.CastInt32Enum(convertible.ToInt32(Nothing), TargetType)
                    Case TypeCode.UInt32
                        Return Conversions.CastUInt32Enum(convertible.ToUInt32(Nothing), TargetType)
                    Case TypeCode.Int64
                        Return Conversions.CastInt64Enum(convertible.ToInt64(Nothing), TargetType)
                    Case TypeCode.UInt64
                        Return Conversions.CastUInt64Enum(convertible.ToUInt64(Nothing), TargetType)
                    Case TypeCode.Single
                        Return convertible.ToSingle(Nothing)
                    Case TypeCode.Double
                        Return convertible.ToDouble(Nothing)
                    Case TypeCode.Decimal
                        Return convertible.ToDecimal(Nothing)
                    Case TypeCode.DateTime
                        Return convertible.ToDateTime(Nothing)
                End Select
            End If
            Return Expression
        End Function

        Public Shared Function FromCharAndCount(Value As Char, Count As Integer) As String
            Return New String(Value, Count)
        End Function

        Public Shared Function FromCharArray(Value As Char()) As String
            Return New String(Value)
        End Function

        Public Shared Function FromCharArraySubset(Value As Char(), StartIndex As Integer, Length As Integer) As String
            Return New String(Value, StartIndex, Length)
        End Function

        Private Shared Function GetNormalizedNumberFormat(InNumberFormat As NumberFormatInfo) As NumberFormatInfo
            Dim info1 As NumberFormatInfo
            Dim info2 As NumberFormatInfo = InNumberFormat
            If (((((Not info2.CurrencyDecimalSeparator Is Nothing) AndAlso (Not info2.NumberDecimalSeparator Is Nothing)) AndAlso ((Not info2.CurrencyGroupSeparator Is Nothing) AndAlso (Not info2.NumberGroupSeparator Is Nothing))) AndAlso (((info2.CurrencyDecimalSeparator.Length = 1) AndAlso (info2.NumberDecimalSeparator.Length = 1)) AndAlso ((info2.CurrencyGroupSeparator.Length = 1) AndAlso (info2.NumberGroupSeparator.Length = 1)))) AndAlso (((info2.CurrencyDecimalSeparator.Chars(0) = info2.NumberDecimalSeparator.Chars(0)) AndAlso (info2.CurrencyGroupSeparator.Chars(0) = info2.NumberGroupSeparator.Chars(0))) AndAlso (info2.CurrencyDecimalDigits = info2.NumberDecimalDigits))) Then
                Return InNumberFormat
            End If
            info2 = Nothing
            Dim info3 As NumberFormatInfo = InNumberFormat
            If ((((Not info3.CurrencyDecimalSeparator Is Nothing) AndAlso (Not info3.NumberDecimalSeparator Is Nothing)) AndAlso ((info3.CurrencyDecimalSeparator.Length = info3.NumberDecimalSeparator.Length) AndAlso (Not info3.CurrencyGroupSeparator Is Nothing))) AndAlso ((Not info3.NumberGroupSeparator Is Nothing) AndAlso (info3.CurrencyGroupSeparator.Length = info3.NumberGroupSeparator.Length))) Then
                Dim num As Integer
                Dim num2 As Integer = (info3.CurrencyDecimalSeparator.Length - 1)
                num = 0
                Do While (num <= num2)
                    If (info3.CurrencyDecimalSeparator.Chars(num) <> info3.NumberDecimalSeparator.Chars(num)) Then
                        GoTo Label_0184
                    End If
                    num += 1
                Loop
                Dim num3 As Integer = (info3.CurrencyGroupSeparator.Length - 1)
                num = 0
                Do While (num <= num3)
                    If (info3.CurrencyGroupSeparator.Chars(num) <> info3.NumberGroupSeparator.Chars(num)) Then
                        GoTo Label_0184
                    End If
                    num += 1
                Loop
                Return InNumberFormat
            End If
            info3 = Nothing
Label_0184:
            info1 = DirectCast(InNumberFormat.Clone, NumberFormatInfo)
            info1.CurrencyDecimalSeparator = info1.NumberDecimalSeparator
            info1.CurrencyGroupSeparator = info1.NumberGroupSeparator
            info1.CurrencyDecimalDigits = info1.NumberDecimalDigits
            Return info1
        End Function

        <DebuggerHidden, DebuggerStepThrough>
        Private Shared Function ObjectUserDefinedConversion(Expression As Object, TargetType As Type) As Object
            Dim sourceType As Type = Expression.GetType
            If (((ConversionResolution.ClassifyPredefinedConversion(TargetType, sourceType) = ConversionClass.None) AndAlso (Symbols.IsClassOrValueType(sourceType) OrElse Symbols.IsClassOrValueType(TargetType))) AndAlso (Not Symbols.IsIntrinsicType(sourceType) OrElse Not Symbols.IsIntrinsicType(TargetType))) Then
                Dim operatorMethod As Method = Nothing
                Dim class2 As ConversionClass = ConversionResolution.ClassifyUserDefinedConversion(TargetType, sourceType, operatorMethod)
                If (Not operatorMethod Is Nothing) Then
                    Dim arguments As Object() = New Object() {Expression}
                    Return Conversions.ChangeType(New Container(operatorMethod.DeclaringType).InvokeMethod(operatorMethod, arguments, Nothing, BindingFlags.InvokeMethod), TargetType)
                End If
                If (class2 = ConversionClass.Ambiguous) Then
                    Dim textArray1 As String() = New String() {Utils.VBFriendlyName(sourceType), Utils.VBFriendlyName(TargetType)}
                    Throw New InvalidCastException(Utils.GetResourceString("AmbiguousCast2", textArray1))
                End If
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(sourceType), Utils.VBFriendlyName(TargetType)}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Private Shared Function ParseDecimal(Value As String, NumberFormat As NumberFormatInfo) As Decimal
            Dim num As Decimal
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            If (NumberFormat Is Nothing) Then
                NumberFormat = cultureInfo.NumberFormat
            End If
            Dim normalizedNumberFormat As NumberFormatInfo = Conversions.GetNormalizedNumberFormat(NumberFormat)
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo)
            Try
                num = Decimal.Parse(Value, NumberStyles.Any, normalizedNumberFormat)
            Catch obj1 As Object When (?)
                num = Decimal.Parse(Value, NumberStyles.Any, NumberFormat)
            Catch exception2 As Exception
                Throw exception2
            End Try
            Return num
        End Function

        Private Shared Function ParseDouble(Value As String) As Double
            Return Conversions.ParseDouble(Value, Nothing)
        End Function

        Private Shared Function ParseDouble(Value As String, NumberFormat As NumberFormatInfo) As Double
            Dim num As Double
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            If (NumberFormat Is Nothing) Then
                NumberFormat = cultureInfo.NumberFormat
            End If
            Dim normalizedNumberFormat As NumberFormatInfo = Conversions.GetNormalizedNumberFormat(NumberFormat)
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo)
            Try
                num = Double.Parse(Value, NumberStyles.Any, DirectCast(normalizedNumberFormat, IFormatProvider))
            Catch obj1 As Object When (?)
                num = Double.Parse(Value, NumberStyles.Any, DirectCast(NumberFormat, IFormatProvider))
            Catch exception2 As Exception
                Throw exception2
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToBoolean(Value As Object) As Boolean
            If (Value Is Nothing) Then
                Return False
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CBool(Value)
                        End If
                        Return convertible.ToBoolean(Nothing)
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return (CSByte(Value) > 0)
                        End If
                        Return (convertible.ToSByte(Nothing) > 0)
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return (CByte(Value) > 0)
                        End If
                        Return (convertible.ToByte(Nothing) > 0)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return (CShort(Value) > 0)
                        End If
                        Return (convertible.ToInt16(Nothing) > 0)
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return (CUShort(Value) > 0)
                        End If
                        Return (convertible.ToUInt16(Nothing) > 0)
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return (CInt(Value) > 0)
                        End If
                        Return (convertible.ToInt32(Nothing) > 0)
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return (DirectCast(Value, UInt32) > 0)
                        End If
                        Return (convertible.ToUInt32(Nothing) > 0)
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return (CLng(Value) > 0)
                        End If
                        Return (convertible.ToInt64(Nothing) > 0)
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return (CULng(Value) > 0)
                        End If
                        Return (convertible.ToUInt64(Nothing) > 0)
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return Not (CSng(Value) = 0!)
                        End If
                        Return Not (convertible.ToSingle(Nothing) = 0!)
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return Not (CDbl(Value) = 0)
                        End If
                        Return Not (convertible.ToDouble(Nothing) = 0)
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToBoolean(Nothing)
                        End If
                        Return Convert.ToBoolean(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToBoolean(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToBoolean(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Boolean"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToBoolean(Value As String) As Boolean
            Dim flag As Boolean
            If (Value Is Nothing) Then
                Value = ""
            End If
            Try
                Dim num As Long
                Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
                If (String.Compare(Value, Boolean.FalseString, True, cultureInfo) = 0) Then
                    Return False
                End If
                If (String.Compare(Value, Boolean.TrueString, True, cultureInfo) = 0) Then
                    Return True
                End If
                If Utils.IsHexOrOctValue(Value, num) Then
                    Return (num > 0)
                End If
                flag = Not (Conversions.ParseDouble(Value) = 0)
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Boolean"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return flag
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToByte(Value As Object) As Byte
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CByte(-(CBool(Value) > False))
                        End If
                        Return CByte(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CByte(CSByte(Value))
                        End If
                        Return CByte(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return convertible.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CByte(CShort(Value))
                        End If
                        Return CByte(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CByte(CUShort(Value))
                        End If
                        Return CByte(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CByte(CInt(Value))
                        End If
                        Return CByte(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CByte(DirectCast(Value, UInt32))
                        End If
                        Return CByte(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CByte(CLng(Value))
                        End If
                        Return CByte(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CByte(CULng(Value))
                        End If
                        Return CByte(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CByte(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CByte(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CByte(Math.Round(CDbl(Value)))
                        End If
                        Return CByte(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToByte(Nothing)
                        End If
                        Return Convert.ToByte(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToByte(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToByte(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Byte"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToByte(Value As String) As Byte
            Dim num As Byte
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CByte(num2)
                End If
                num = CByte(Math.Round(Conversions.ParseDouble(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Byte"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToChar(Value As Object) As Char
            If (Value Is Nothing) Then
                Return ChrW(0)
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Char
                        If TypeOf Value Is Char Then
                            Return DirectCast(Value, Char)
                        End If
                        Return convertible.ToChar(Nothing)
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToChar(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToChar(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Char"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToChar(Value As String) As Char
            If ((Value Is Nothing) OrElse (Value.Length = 0)) Then
                Return ChrW(0)
            End If
            Return Value.Chars(0)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToCharArrayRankOne(Value As Object) As Char()
            If (Value Is Nothing) Then
                Return "".ToCharArray
            End If
            Dim chArray2 As Char() = TryCast(Value, Char())
            If ((Not chArray2 Is Nothing) AndAlso (chArray2.Rank = 1)) Then
                Return chArray2
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If ((Not convertible Is Nothing) AndAlso (convertible.GetTypeCode = TypeCode.String)) Then
                Return convertible.ToString(Nothing).ToCharArray
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Char()"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToCharArrayRankOne(Value As String) As Char()
            If (Value Is Nothing) Then
                Value = ""
            End If
            Return Value.ToCharArray
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDate(Value As Object) As DateTime
            If (Value Is Nothing) Then
                Dim time As DateTime
                Return time
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.DateTime
                        If TypeOf Value Is DateTime Then
                            Return CDate(Value)
                        End If
                        Return convertible.ToDateTime(Nothing)
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToDate(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToDate(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Date"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDate(Value As String) As DateTime
            Dim time2 As DateTime
            If Conversions.TryParseDate(Value, time2) Then
                Return time2
            End If
            Dim args As String() = New String() {Strings.Left(Value, &H20), "Date"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDecimal(Value As Boolean) As Decimal
            If Value Then
                Return Decimal.MinusOne
            End If
            Return New Decimal
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDecimal(Value As Object) As Decimal
            Return Conversions.ToDecimal(Value, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDecimal(Value As String) As Decimal
            Return Conversions.ToDecimal(Value, Nothing)
        End Function

        Friend Shared Function ToDecimal(Value As Object, NumberFormat As NumberFormatInfo) As Decimal
            If (Value Is Nothing) Then
                Return New Decimal
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return Conversions.ToDecimal(CBool(Value))
                        End If
                        Return Conversions.ToDecimal(convertible.ToBoolean(Nothing))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return New Decimal(CInt(CSByte(Value)))
                        End If
                        Return New Decimal(CInt(convertible.ToSByte(Nothing)))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return New Decimal(CInt(CByte(Value)))
                        End If
                        Return New Decimal(CInt(convertible.ToByte(Nothing)))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return New Decimal(CInt(CShort(Value)))
                        End If
                        Return New Decimal(CInt(convertible.ToInt16(Nothing)))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return New Decimal(CInt(CUShort(Value)))
                        End If
                        Return New Decimal(CInt(convertible.ToUInt16(Nothing)))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return New Decimal(CInt(Value))
                        End If
                        Return New Decimal(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return New Decimal(DirectCast(Value, UInt32))
                        End If
                        Return New Decimal(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return New Decimal(CLng(Value))
                        End If
                        Return New Decimal(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return New Decimal(CULng(Value))
                        End If
                        Return New Decimal(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return New Decimal(CSng(Value))
                        End If
                        Return New Decimal(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return New Decimal(CDbl(Value))
                        End If
                        Return New Decimal(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return convertible.ToDecimal(Nothing)
                    Case TypeCode.String
                        Return Conversions.ToDecimal(convertible.ToString(Nothing), NumberFormat)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Decimal"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Friend Shared Function ToDecimal(Value As String, NumberFormat As NumberFormatInfo) As Decimal
            Dim num As Decimal
            If (Value Is Nothing) Then
                Return New Decimal
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return New Decimal(num2)
                End If
                num = Conversions.ParseDecimal(Value, NumberFormat)
            Catch exception As OverflowException
                Throw ExceptionUtils.VbMakeException(6)
            Catch exception2 As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Decimal"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args))
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDouble(Value As Object) As Double
            Return Conversions.ToDouble(Value, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToDouble(Value As String) As Double
            Return Conversions.ToDouble(Value, Nothing)
        End Function

        Friend Shared Function ToDouble(Value As Object, NumberFormat As NumberFormatInfo) As Double
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CDbl(-(CBool(Value) > False))
                        End If
                        Return CDbl(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CDbl(CSByte(Value))
                        End If
                        Return CDbl(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CDbl(CByte(Value))
                        End If
                        Return CDbl(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CDbl(CShort(Value))
                        End If
                        Return CDbl(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CDbl(CUShort(Value))
                        End If
                        Return CDbl(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CDbl(CInt(Value))
                        End If
                        Return CDbl(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CDbl(DirectCast(Value, UInt32))
                        End If
                        Return CDbl(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CDbl(CLng(Value))
                        End If
                        Return CDbl(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CDbl(CULng(Value))
                        End If
                        Return CDbl(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CDbl(CSng(Value))
                        End If
                        Return CDbl(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CDbl(Value)
                        End If
                        Return convertible.ToDouble(Nothing)
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToDouble(Nothing)
                        End If
                        Return Convert.ToDouble(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Return Conversions.ToDouble(convertible.ToString(Nothing), NumberFormat)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Double"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Friend Shared Function ToDouble(Value As String, NumberFormat As NumberFormatInfo) As Double
            Dim num As Double
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CDbl(num2)
                End If
                num = Conversions.ParseDouble(Value, NumberFormat)
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Double"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToGenericParameter(Of T)(Value As Object) As T
            If (Value Is Nothing) Then
                Return CType(Nothing, T)
            End If
            Select Case Symbols.GetTypeCode(GetType(T))
                Case TypeCode.Boolean
                    Return DirectCast(Conversions.ToBoolean(Value), T)
                Case TypeCode.Char
                    Return DirectCast(Conversions.ToChar(Value), T)
                Case TypeCode.SByte
                    Return DirectCast(Conversions.ToSByte(Value), T)
                Case TypeCode.Byte
                    Return DirectCast(Conversions.ToByte(Value), T)
                Case TypeCode.Int16
                    Return DirectCast(Conversions.ToShort(Value), T)
                Case TypeCode.UInt16
                    Return DirectCast(Conversions.ToUShort(Value), T)
                Case TypeCode.Int32
                    Return DirectCast(Conversions.ToInteger(Value), T)
                Case TypeCode.UInt32
                    Return DirectCast(Conversions.ToUInteger(Value), T)
                Case TypeCode.Int64
                    Return DirectCast(Conversions.ToLong(Value), T)
                Case TypeCode.UInt64
                    Return DirectCast(Conversions.ToULong(Value), T)
                Case TypeCode.Single
                    Return DirectCast(Conversions.ToSingle(Value), T)
                Case TypeCode.Double
                    Return DirectCast(Conversions.ToDouble(Value), T)
                Case TypeCode.Decimal
                    Return DirectCast(Conversions.ToDecimal(Value), T)
                Case TypeCode.DateTime
                    Return DirectCast(Conversions.ToDate(Value), T)
                Case TypeCode.String
                    Return DirectCast(Conversions.ToString(Value), T)
            End Select
            Return DirectCast(Value, T)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToInteger(Value As Object) As Integer
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CInt(-(CBool(Value) > False))
                        End If
                        Return CInt(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CSByte(Value)
                        End If
                        Return convertible.ToSByte(Nothing)
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return convertible.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CShort(Value)
                        End If
                        Return convertible.ToInt16(Nothing)
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CUShort(Value)
                        End If
                        Return convertible.ToUInt16(Nothing)
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CInt(Value)
                        End If
                        Return convertible.ToInt32(Nothing)
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CInt(DirectCast(Value, UInt32))
                        End If
                        Return CInt(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CInt(CLng(Value))
                        End If
                        Return CInt(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CInt(CULng(Value))
                        End If
                        Return CInt(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CInt(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CInt(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CInt(Math.Round(CDbl(Value)))
                        End If
                        Return CInt(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToInt32(Nothing)
                        End If
                        Return Convert.ToInt32(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToInteger(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToInteger(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Integer"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToInteger(Value As String) As Integer
            Dim num As Integer
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CInt(num2)
                End If
                num = CInt(Math.Round(Conversions.ParseDouble(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Integer"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToLong(Value As Object) As Long
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CLng(-(CBool(Value) > False))
                        End If
                        Return CLng(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CLng(CSByte(Value))
                        End If
                        Return CLng(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CLng(CByte(Value))
                        End If
                        Return CLng(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CLng(CShort(Value))
                        End If
                        Return CLng(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CLng(CUShort(Value))
                        End If
                        Return CLng(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CLng(CInt(Value))
                        End If
                        Return CLng(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CLng(DirectCast(Value, UInt32))
                        End If
                        Return CLng(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CLng(Value)
                        End If
                        Return convertible.ToInt64(Nothing)
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CLng(CULng(Value))
                        End If
                        Return CLng(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CLng(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CLng(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CLng(Math.Round(CDbl(Value)))
                        End If
                        Return CLng(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToInt64(Nothing)
                        End If
                        Return Convert.ToInt64(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToLong(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToLong(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Long"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToLong(Value As String) As Long
            Dim num As Long
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return num2
                End If
                num = Convert.ToInt64(Conversions.ParseDecimal(Value, Nothing))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Long"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToSByte(Value As Object) As SByte
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CSByte(-(CBool(Value) > False))
                        End If
                        Return CSByte(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CSByte(Value)
                        End If
                        Return convertible.ToSByte(Nothing)
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CSByte(CByte(Value))
                        End If
                        Return CSByte(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CSByte(CShort(Value))
                        End If
                        Return CSByte(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CSByte(CUShort(Value))
                        End If
                        Return CSByte(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CSByte(CInt(Value))
                        End If
                        Return CSByte(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CSByte(DirectCast(Value, UInt32))
                        End If
                        Return CSByte(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CSByte(CLng(Value))
                        End If
                        Return CSByte(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CSByte(CULng(Value))
                        End If
                        Return CSByte(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CSByte(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CSByte(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CSByte(Math.Round(CDbl(Value)))
                        End If
                        Return CSByte(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToSByte(Nothing)
                        End If
                        Return Convert.ToSByte(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToSByte(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToSByte(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "SByte"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToSByte(Value As String) As SByte
            Dim num As SByte
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CSByte(num2)
                End If
                num = CSByte(Math.Round(Conversions.ParseDouble(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "SByte"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToShort(Value As Object) As Short
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CShort(-(CBool(Value) > False))
                        End If
                        Return CShort(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CSByte(Value)
                        End If
                        Return convertible.ToSByte(Nothing)
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return convertible.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CShort(Value)
                        End If
                        Return convertible.ToInt16(Nothing)
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CShort(CUShort(Value))
                        End If
                        Return CShort(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CShort(CInt(Value))
                        End If
                        Return CShort(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CShort(DirectCast(Value, UInt32))
                        End If
                        Return CShort(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CShort(CLng(Value))
                        End If
                        Return CShort(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CShort(CULng(Value))
                        End If
                        Return CShort(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CShort(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CShort(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CShort(Math.Round(CDbl(Value)))
                        End If
                        Return CShort(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToInt16(Nothing)
                        End If
                        Return Convert.ToInt16(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToShort(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToShort(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Short"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToShort(Value As String) As Short
            Dim num As Short
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CShort(num2)
                End If
                num = CShort(Math.Round(Conversions.ParseDouble(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Short"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToSingle(Value As Object) As Single
            Return Conversions.ToSingle(Value, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToSingle(Value As String) As Single
            Return Conversions.ToSingle(Value, Nothing)
        End Function

        Friend Shared Function ToSingle(Value As Object, NumberFormat As NumberFormatInfo) As Single
            If (Value Is Nothing) Then
                Return 0!
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CSng(-(CBool(Value) > False))
                        End If
                        Return CSng(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CSng(CSByte(Value))
                        End If
                        Return CSng(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CSng(CByte(Value))
                        End If
                        Return CSng(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CSng(CShort(Value))
                        End If
                        Return CSng(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CSng(CUShort(Value))
                        End If
                        Return CSng(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CSng(CInt(Value))
                        End If
                        Return CSng(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CSng(DirectCast(Value, UInt32))
                        End If
                        Return CSng(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CSng(CLng(Value))
                        End If
                        Return CSng(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CSng(CULng(Value))
                        End If
                        Return CSng(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CSng(Value)
                        End If
                        Return convertible.ToSingle(Nothing)
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CSng(CDbl(Value))
                        End If
                        Return CSng(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToSingle(Nothing)
                        End If
                        Return Convert.ToSingle(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Return Conversions.ToSingle(convertible.ToString(Nothing), NumberFormat)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Single"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Friend Shared Function ToSingle(Value As String, NumberFormat As NumberFormatInfo) As Single
            Dim num As Single
            If (Value Is Nothing) Then
                Return 0!
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CSng(num2)
                End If
                Dim d As Double = Conversions.ParseDouble(Value, NumberFormat)
                If (((d < -3.4028234663852886E+38) OrElse (d > 3.4028234663852886E+38)) AndAlso Not Double.IsInfinity(d)) Then
                    Throw New OverflowException
                End If
                num = CSng(d)
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Single"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Boolean) As String
            If Value Then
                Return Boolean.TrueString
            End If
            Return Boolean.FalseString
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Byte) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Char) As String
            Return Value.ToString
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As DateTime) As String
            Dim ticks As Long = Value.TimeOfDay.Ticks
            If ((ticks = Value.Ticks) OrElse (((Value.Year = &H76B) AndAlso (Value.Month = 12)) AndAlso (Value.Day = 30))) Then
                Return Value.ToString("T", Nothing)
            End If
            If (ticks = 0) Then
                Return Value.ToString("d", Nothing)
            End If
            Return Value.ToString("G", Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Decimal) As String
            Return Conversions.ToString(Value, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Double) As String
            Return Conversions.ToString(Value, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Short) As String
            Return Value.ToString(Nothing, DirectCast(Nothing, IFormatProvider))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Integer) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Long) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Object) As String
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
                        Return Conversions.ToString(convertible.ToBoolean(Nothing))
                    Case TypeCode.Char
                        Return Conversions.ToString(convertible.ToChar(Nothing))
                    Case TypeCode.SByte
                        Return Conversions.ToString(CInt(convertible.ToSByte(Nothing)))
                    Case TypeCode.Byte
                        Return Conversions.ToString(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        Return Conversions.ToString(CInt(convertible.ToInt16(Nothing)))
                    Case TypeCode.UInt16
                        Return Conversions.ToString(DirectCast(convertible.ToUInt16(Nothing), UInt32))
                    Case TypeCode.Int32
                        Return Conversions.ToString(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        Return Conversions.ToString(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        Return Conversions.ToString(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        Return Conversions.ToString(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        Return Conversions.ToString(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        Return Conversions.ToString(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return Conversions.ToString(convertible.ToDecimal(Nothing))
                    Case TypeCode.DateTime
                        Return Conversions.ToString(convertible.ToDateTime(Nothing))
                    Case TypeCode.String
                        Return convertible.ToString(Nothing)
                End Select
            Else
                Dim chArray As Char() = TryCast(Value, Char())
                If (Not chArray Is Nothing) Then
                    Return New String(chArray)
                End If
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "String"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As Single) As String
            Return Conversions.ToString(Value, Nothing)
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As UInt32) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToString(Value As UInt64) As String
            Return Value.ToString(Nothing, Nothing)
        End Function

        Public Shared Function ToString(Value As Decimal, NumberFormat As NumberFormatInfo) As String
            Return Value.ToString("G", NumberFormat)
        End Function

        Public Shared Function ToString(Value As Double, NumberFormat As NumberFormatInfo) As String
            Return Value.ToString("G", NumberFormat)
        End Function

        Public Shared Function ToString(Value As Single, NumberFormat As NumberFormatInfo) As String
            Return Value.ToString(Nothing, NumberFormat)
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToUInteger(Value As Object) As UInt32
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return DirectCast(-(CBool(Value) > False), UInt32)
                        End If
                        Return DirectCast(-(convertible.ToBoolean(Nothing) > False), UInt32)
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return DirectCast(CSByte(Value), UInt32)
                        End If
                        Return DirectCast(convertible.ToSByte(Nothing), UInt32)
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return convertible.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return DirectCast(CShort(Value), UInt32)
                        End If
                        Return DirectCast(convertible.ToInt16(Nothing), UInt32)
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CUShort(Value)
                        End If
                        Return convertible.ToUInt16(Nothing)
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return DirectCast(CInt(Value), UInt32)
                        End If
                        Return DirectCast(convertible.ToInt32(Nothing), UInt32)
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return DirectCast(Value, UInt32)
                        End If
                        Return convertible.ToUInt32(Nothing)
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return DirectCast(CLng(Value), UInt32)
                        End If
                        Return DirectCast(convertible.ToInt64(Nothing), UInt32)
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return DirectCast(CULng(Value), UInt32)
                        End If
                        Return DirectCast(convertible.ToUInt64(Nothing), UInt32)
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return DirectCast(Math.Round(CDbl(CSng(Value))), UInt32)
                        End If
                        Return DirectCast(Math.Round(CDbl(convertible.ToSingle(Nothing))), UInt32)
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return DirectCast(Math.Round(CDbl(Value)), UInt32)
                        End If
                        Return DirectCast(Math.Round(convertible.ToDouble(Nothing)), UInt32)
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToUInt32(Nothing)
                        End If
                        Return Convert.ToUInt32(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToUInteger(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToUInteger(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "UInteger"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToUInteger(Value As String) As UInt32
            Dim num As UInt32
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return DirectCast(num2, UInt32)
                End If
                num = DirectCast(Math.Round(Conversions.ParseDouble(Value)), UInt32)
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "UInteger"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToULong(Value As Object) As UInt64
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CULng(CLng(-(CBool(Value) > False)))
                        End If
                        Return CULng(CLng(-(convertible.ToBoolean(Nothing) > False)))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CULng(CSByte(Value))
                        End If
                        Return CULng(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CULng(CByte(Value))
                        End If
                        Return CULng(convertible.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CULng(CShort(Value))
                        End If
                        Return CULng(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CULng(CUShort(Value))
                        End If
                        Return CULng(convertible.ToUInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CULng(CInt(Value))
                        End If
                        Return CULng(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CULng(DirectCast(Value, UInt32))
                        End If
                        Return CULng(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CULng(CLng(Value))
                        End If
                        Return CULng(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CULng(Value)
                        End If
                        Return convertible.ToUInt64(Nothing)
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CULng(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CULng(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CULng(Math.Round(CDbl(Value)))
                        End If
                        Return CULng(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToUInt64(Nothing)
                        End If
                        Return Convert.ToUInt64(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToULong(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToULong(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "ULong"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToULong(Value As String) As UInt64
            Dim num As UInt64
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As UInt64
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return num2
                End If
                num = Convert.ToUInt64(Conversions.ParseDecimal(Value, Nothing))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "ULong"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToUShort(Value As Object) As UInt16
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CUShort(-(CBool(Value) > False))
                        End If
                        Return CUShort(-(convertible.ToBoolean(Nothing) > False))
                    Case TypeCode.SByte
                        If TypeOf Value Is SByte Then
                            Return CUShort(CSByte(Value))
                        End If
                        Return CUShort(convertible.ToSByte(Nothing))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return convertible.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CUShort(CShort(Value))
                        End If
                        Return CUShort(convertible.ToInt16(Nothing))
                    Case TypeCode.UInt16
                        If TypeOf Value Is UInt16 Then
                            Return CUShort(Value)
                        End If
                        Return convertible.ToUInt16(Nothing)
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CUShort(CInt(Value))
                        End If
                        Return CUShort(convertible.ToInt32(Nothing))
                    Case TypeCode.UInt32
                        If TypeOf Value Is UInt32 Then
                            Return CUShort(DirectCast(Value, UInt32))
                        End If
                        Return CUShort(convertible.ToUInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CUShort(CLng(Value))
                        End If
                        Return CUShort(convertible.ToInt64(Nothing))
                    Case TypeCode.UInt64
                        If TypeOf Value Is UInt64 Then
                            Return CUShort(CULng(Value))
                        End If
                        Return CUShort(convertible.ToUInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CUShort(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CUShort(Math.Round(CDbl(convertible.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CUShort(Math.Round(CDbl(Value)))
                        End If
                        Return CUShort(Math.Round(convertible.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        If TypeOf Value Is Decimal Then
                            Return convertible.ToUInt16(Nothing)
                        End If
                        Return Convert.ToUInt16(convertible.ToDecimal(Nothing))
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return Conversions.ToUShort(convertible.ToString(Nothing))
                        End If
                        Return Conversions.ToUShort(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "UShort"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        <CLSCompliant(False), DynamicallyInvokableAttribute>
        Public Shared Function ToUShort(Value As String) As UInt16
            Dim num As UInt16
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CUShort(num2)
                End If
                num = CUShort(Math.Round(Conversions.ParseDouble(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "UShort"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        Friend Shared Function TryParseDate(Value As String, ByRef Result As DateTime) As Boolean
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            Return DateTime.TryParse(Utils.ToHalfwidthNumbers(Value, cultureInfo), cultureInfo, (DateTimeStyles.NoCurrentDateDefault Or DateTimeStyles.AllowWhiteSpaces), Result)
        End Function

        Friend Shared Function TryParseDouble(Value As String, ByRef Result As Double) As Boolean
            Dim flag As Boolean
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            Dim numberFormat As NumberFormatInfo = cultureInfo.NumberFormat
            Dim normalizedNumberFormat As NumberFormatInfo = Conversions.GetNormalizedNumberFormat(numberFormat)
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo)
            If (numberFormat Is normalizedNumberFormat) Then
                Return Double.TryParse(Value, NumberStyles.Any, DirectCast(normalizedNumberFormat, IFormatProvider), Result)
            End If
            Try
                Result = Double.Parse(Value, NumberStyles.Any, DirectCast(normalizedNumberFormat, IFormatProvider))
                flag = True
            Catch exception As FormatException
                Try
                    flag = Double.TryParse(Value, NumberStyles.Any, DirectCast(numberFormat, IFormatProvider), Result)
                Catch exception2 As ArgumentException
                    flag = False
                End Try
            Catch exception3 As StackOverflowException
                Throw exception3
            Catch exception4 As OutOfMemoryException
                Throw exception4
            Catch exception5 As ThreadAbortException
                Throw exception5
            Catch exception6 As Exception
                flag = False
            End Try
            Return flag
        End Function

    End Class
End Namespace

