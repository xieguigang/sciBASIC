Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Globalization

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class BooleanType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToBoolean(ValueInterface As IConvertible) As Boolean
            Return Convert.ToBoolean(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Boolean
            If (Value Is Nothing) Then
                Return False
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        If TypeOf Value Is Boolean Then
                            Return CBool(Value)
                        End If
                        Return valueInterface.ToBoolean(Nothing)
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return (CByte(Value) > 0)
                        End If
                        Return (valueInterface.ToByte(Nothing) > 0)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return (CShort(Value) > 0)
                        End If
                        Return (valueInterface.ToInt16(Nothing) > 0)
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return (CInt(Value) > 0)
                        End If
                        Return (valueInterface.ToInt32(Nothing) > 0)
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return (CLng(Value) > 0)
                        End If
                        Return (valueInterface.ToInt64(Nothing) > 0)
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return Not (CSng(Value) = 0!)
                        End If
                        Return Not (valueInterface.ToSingle(Nothing) = 0!)
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return Not (CDbl(Value) = 0)
                        End If
                        Return Not (valueInterface.ToDouble(Nothing) = 0)
                    Case TypeCode.Decimal
                        Return BooleanType.DecimalToBoolean(valueInterface)
                    Case TypeCode.String
                        Dim str As String = TryCast(Value, String)
                        If (str Is Nothing) Then
                            Return BooleanType.FromString(valueInterface.ToString(Nothing))
                        End If
                        Return BooleanType.FromString(str)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Boolean"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Boolean
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
                flag = Not (DoubleType.Parse(Value) = 0)
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Boolean"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return flag
        End Function

    End Class
End Namespace

