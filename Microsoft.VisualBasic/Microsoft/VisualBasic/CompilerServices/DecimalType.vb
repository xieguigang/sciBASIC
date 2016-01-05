Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Globalization

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class DecimalType
        ' Methods
        Private Sub New()
        End Sub

        Public Shared Function FromBoolean(Value As Boolean) As Decimal
            If Value Then
                Return Decimal.MinusOne
            End If
            Return New Decimal
        End Function

        Public Shared Function FromObject(Value As Object) As Decimal
            Return DecimalType.FromObject(Value, Nothing)
        End Function

        Public Shared Function FromObject(Value As Object, NumberFormat As NumberFormatInfo) As Decimal
            If (Value Is Nothing) Then
                Return New Decimal
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Boolean
                        Return DecimalType.FromBoolean(convertible.ToBoolean(Nothing))
                    Case TypeCode.Byte
                        Return New Decimal(CInt(convertible.ToByte(Nothing)))
                    Case TypeCode.Int16
                        Return New Decimal(CInt(convertible.ToInt16(Nothing)))
                    Case TypeCode.Int32
                        Return New Decimal(convertible.ToInt32(Nothing))
                    Case TypeCode.Int64
                        Return New Decimal(convertible.ToInt64(Nothing))
                    Case TypeCode.Single
                        Return New Decimal(convertible.ToSingle(Nothing))
                    Case TypeCode.Double
                        Return New Decimal(convertible.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return convertible.ToDecimal(Nothing)
                    Case TypeCode.String
                        Return DecimalType.FromString(convertible.ToString(Nothing), NumberFormat)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Decimal"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Decimal
            Return DecimalType.FromString(Value, Nothing)
        End Function

        Public Shared Function FromString(Value As String, NumberFormat As NumberFormatInfo) As Decimal
            Dim num As Decimal
            If (Value Is Nothing) Then
                Return New Decimal
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return New Decimal(num2)
                End If
                num = DecimalType.Parse(Value, NumberFormat)
            Catch exception As OverflowException
                Throw ExceptionUtils.VbMakeException(6)
            Catch exception2 As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Decimal"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args))
            End Try
            Return num
        End Function

        Friend Shared Function GetNormalizedNumberFormat(InNumberFormat As NumberFormatInfo) As NumberFormatInfo
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

        Public Shared Function Parse(Value As String, NumberFormat As NumberFormatInfo) As Decimal
            Dim num As Decimal
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            If (NumberFormat Is Nothing) Then
                NumberFormat = cultureInfo.NumberFormat
            End If
            Dim normalizedNumberFormat As NumberFormatInfo = DecimalType.GetNormalizedNumberFormat(NumberFormat)
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

    End Class
End Namespace

