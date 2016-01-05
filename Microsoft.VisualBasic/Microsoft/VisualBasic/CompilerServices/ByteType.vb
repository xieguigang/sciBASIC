Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class ByteType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToByte(ValueInterface As IConvertible) As Byte
            Return Convert.ToByte(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Byte
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        Return CByte(-(valueInterface.ToBoolean(Nothing) > False))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return valueInterface.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CByte(CShort(Value))
                        End If
                        Return CByte(valueInterface.ToInt16(Nothing))
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CByte(CInt(Value))
                        End If
                        Return CByte(valueInterface.ToInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CByte(CLng(Value))
                        End If
                        Return CByte(valueInterface.ToInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CByte(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CByte(Math.Round(CDbl(valueInterface.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CByte(Math.Round(CDbl(Value)))
                        End If
                        Return CByte(Math.Round(valueInterface.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        Return ByteType.DecimalToByte(valueInterface)
                    Case TypeCode.String
                        Return ByteType.FromString(valueInterface.ToString(Nothing))
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Byte"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Byte
            Dim num As Byte
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CByte(num2)
                End If
                num = CByte(Math.Round(DoubleType.Parse(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Byte"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

    End Class
End Namespace

