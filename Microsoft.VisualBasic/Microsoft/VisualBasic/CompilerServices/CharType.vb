Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class CharType
        ' Methods
        Private Sub New()
        End Sub

        Public Shared Function FromObject(Value As Object) As Char
            If (Value Is Nothing) Then
                Return ChrW(0)
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.Char
                        Return convertible.ToChar(Nothing)
                    Case TypeCode.String
                        Return CharType.FromString(convertible.ToString(Nothing))
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Char"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As Char
            If ((Value Is Nothing) OrElse (Value.Length = 0)) Then
                Return ChrW(0)
            End If
            Return Value.Chars(0)
        End Function

    End Class
End Namespace

