Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Globalization

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class SingleType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToSingle(ValueInterface As IConvertible) As Single
            Return Convert.ToSingle(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Single
            Return SingleType.FromObject(Value, Nothing)
        End Function

        Public Shared Function FromObject(Value As Object, NumberFormat As NumberFormatInfo) As Single
            If (Value Is Nothing) Then
                Return 0!
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        Return CSng(-(valueInterface.ToBoolean(Nothing) > False))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CSng(CByte(Value))
                        End If
                        Return CSng(valueInterface.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CSng(CShort(Value))
                        End If
                        Return CSng(valueInterface.ToInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CSng(CInt(Value))
                        End If
                        Return CSng(valueInterface.ToInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CSng(CLng(Value))
                        End If
                        Return CSng(valueInterface.ToInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CSng(Value)
                        End If
                        Return valueInterface.ToSingle(Nothing)
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CSng(CDbl(Value))
                        End If
                        Return CSng(valueInterface.ToDouble(Nothing))
                    Case TypeCode.Decimal
                        Return SingleType.DecimalToSingle(valueInterface)
                    Case TypeCode.String
                        Return SingleType.FromString(valueInterface.ToString(Nothing), NumberFormat)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Single"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Single
            Return SingleType.FromString(Value, Nothing)
        End Function

        Public Shared Function FromString(Value As String, NumberFormat As NumberFormatInfo) As Single
            Dim num As Single
            If (Value Is Nothing) Then
                Return 0!
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CSng(num2)
                End If
                Dim d As Double = DoubleType.Parse(Value, NumberFormat)
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

    End Class
End Namespace

