Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Globalization
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class DoubleType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToDouble(ValueInterface As IConvertible) As Double
            Return Convert.ToDouble(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Double
            Return DoubleType.FromObject(Value, Nothing)
        End Function

        Public Shared Function FromObject(Value As Object, NumberFormat As NumberFormatInfo) As Double
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        Return CDbl(-(valueInterface.ToBoolean(Nothing) > False))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CDbl(CByte(Value))
                        End If
                        Return CDbl(valueInterface.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CDbl(CShort(Value))
                        End If
                        Return CDbl(valueInterface.ToInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CDbl(CInt(Value))
                        End If
                        Return CDbl(valueInterface.ToInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CDbl(CLng(Value))
                        End If
                        Return CDbl(valueInterface.ToInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CDbl(CSng(Value))
                        End If
                        Return CDbl(valueInterface.ToSingle(Nothing))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CDbl(Value)
                        End If
                        Return valueInterface.ToDouble(Nothing)
                    Case TypeCode.Decimal
                        Return DoubleType.DecimalToDouble(valueInterface)
                    Case TypeCode.String
                        Return DoubleType.FromString(valueInterface.ToString(Nothing), NumberFormat)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Double"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Double
            Return DoubleType.FromString(Value, Nothing)
        End Function

        Public Shared Function FromString(Value As String, NumberFormat As NumberFormatInfo) As Double
            Dim num As Double
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CDbl(num2)
                End If
                num = DoubleType.Parse(Value, NumberFormat)
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Double"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

        Public Shared Function Parse(Value As String) As Double
            Return DoubleType.Parse(Value, Nothing)
        End Function

        Public Shared Function Parse(Value As String, NumberFormat As NumberFormatInfo) As Double
            Dim num As Double
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            If (NumberFormat Is Nothing) Then
                NumberFormat = cultureInfo.NumberFormat
            End If
            Dim normalizedNumberFormat As NumberFormatInfo = DecimalType.GetNormalizedNumberFormat(NumberFormat)
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

        Friend Shared Function TryParse(Value As String, ByRef Result As Double) As Boolean
            Dim flag As Boolean
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            Dim numberFormat As NumberFormatInfo = cultureInfo.NumberFormat
            Dim normalizedNumberFormat As NumberFormatInfo = DecimalType.GetNormalizedNumberFormat(numberFormat)
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

