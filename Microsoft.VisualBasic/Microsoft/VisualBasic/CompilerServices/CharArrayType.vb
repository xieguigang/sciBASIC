Imports System
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class CharArrayType
        ' Methods
        Private Sub New()
        End Sub

        Public Shared Function FromObject(Value As Object) As Char()
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

        Public Shared Function FromString(Value As String) As Char()
            If (Value Is Nothing) Then
                Value = ""
            End If
            Return Value.ToCharArray
        End Function

    End Class
End Namespace

