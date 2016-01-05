Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class LongType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToLong(ValueInterface As IConvertible) As Long
            Return Convert.ToInt64(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Long
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        Return CLng(-(valueInterface.ToBoolean(Nothing) > False))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CLng(CByte(Value))
                        End If
                        Return CLng(valueInterface.ToByte(Nothing))
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CLng(CShort(Value))
                        End If
                        Return CLng(valueInterface.ToInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CLng(CInt(Value))
                        End If
                        Return CLng(valueInterface.ToInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CLng(Value)
                        End If
                        Return valueInterface.ToInt64(Nothing)
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CLng(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CLng(Math.Round(CDbl(valueInterface.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CLng(Math.Round(CDbl(Value)))
                        End If
                        Return CLng(Math.Round(valueInterface.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        Return LongType.DecimalToLong(valueInterface)
                    Case TypeCode.String
                        Return LongType.FromString(valueInterface.ToString(Nothing))
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Long"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Long
            Dim num As Long
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return num2
                End If
                num = Convert.ToInt64(DecimalType.Parse(Value, Nothing))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Long"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

    End Class
End Namespace

