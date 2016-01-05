Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class IntegerType
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function DecimalToInteger(ValueInterface As IConvertible) As Integer
            Return Convert.ToInt32(ValueInterface.ToDecimal(Nothing))
        End Function

        Public Shared Function FromObject(Value As Object) As Integer
            If (Value Is Nothing) Then
                Return 0
            End If
            Dim valueInterface As IConvertible = TryCast(Value, IConvertible)
            If (Not valueInterface Is Nothing) Then
                Select Case valueInterface.GetTypeCode
                    Case TypeCode.Boolean
                        Return CInt(-(valueInterface.ToBoolean(Nothing) > False))
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
                            Return CInt(Value)
                        End If
                        Return valueInterface.ToInt32(Nothing)
                    Case TypeCode.Int64
                        If TypeOf Value Is Long Then
                            Return CInt(CLng(Value))
                        End If
                        Return CInt(valueInterface.ToInt64(Nothing))
                    Case TypeCode.Single
                        If TypeOf Value Is Single Then
                            Return CInt(Math.Round(CDbl(CSng(Value))))
                        End If
                        Return CInt(Math.Round(CDbl(valueInterface.ToSingle(Nothing))))
                    Case TypeCode.Double
                        If TypeOf Value Is Double Then
                            Return CInt(Math.Round(CDbl(Value)))
                        End If
                        Return CInt(Math.Round(valueInterface.ToDouble(Nothing)))
                    Case TypeCode.Decimal
                        Return IntegerType.DecimalToInteger(valueInterface)
                    Case TypeCode.String
                        Return IntegerType.FromString(valueInterface.ToString(Nothing))
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Integer"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Integer
            Dim num As Integer
            If (Value Is Nothing) Then
                Return 0
            End If
            Try
                Dim num2 As Long
                If Utils.IsHexOrOctValue(Value, num2) Then
                    Return CInt(num2)
                End If
                num = CInt(Math.Round(DoubleType.Parse(Value)))
            Catch exception As FormatException
                Dim args As String() = New String() {Strings.Left(Value, &H20), "Integer"}
                Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args), exception)
            End Try
            Return num
        End Function

    End Class
End Namespace

