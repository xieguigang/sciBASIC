Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class ShortType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToShort(ValueInterface As IConvertible) As Short
            Return Convert.ToInt16(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Short
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        Return CShort(-(valueInterface.ToBoolean(Nothing) > False))
                    Case TypeCode.Byte
                        If TypeOf Value Is Byte Then
                            Return CByte(Value)
                        End If
                        Return valueInterface.ToByte(Nothing)
                    Case TypeCode.Int16
                        If TypeOf Value Is Short Then
                            Return CShort(Value)
                        End If
                        Return valueInterface.ToInt16(Nothing)
                    Case TypeCode.Int32
                        If TypeOf Value Is Integer Then
                            Return CShort(CInt(Value))
                        End If
                        Return CShort(valueInterface.ToInt32(Nothing))
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CShort(CLng(Value))
                        End If
                        Return CShort(valueInterface.ToInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CShort(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CShort(Math.Round(CDbl(valueInterface.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CShort(Math.Round(CDbl(Value)))
                        End If
                        Return CShort(Math.Round(valueInterface.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        Return ShortType.DecimalToShort(valueInterface)
                    Case TypeCode.String
                        Return ShortType.FromString(valueInterface.ToString(Nothing))
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Short"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Short
            Dim num As Short
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CShort(num2)
                End If
                num = CShort(Math.Round(DoubleType.Parse(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Short"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

    End Class
End Namespace

