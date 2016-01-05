Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Globalization

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class DateType
        ' Methods
        Private Sub New()
        End Sub

        Public Shared Function FromObject(Value As Object) As DateTime
            If (Value Is Nothing) Then
                Dim time As DateTime
                Return time
            End If
            Dim convertible As IConvertible = TryCast(Value, IConvertible)
            If (Not convertible Is Nothing) Then
                Select Case convertible.GetTypeCode
                    Case TypeCode.DateTime
                        Return convertible.ToDateTime(Nothing)
                    Case TypeCode.String
                        Return DateType.FromString(convertible.ToString(Nothing), Utils.GetCultureInfo)
                End Select
            End If
            Dim args As String() = New String() {Utils.VBFriendlyName(Value), "Date"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", args))
        End Function

        Public Shared Function FromString(Value As String) As DateTime
            Return DateType.FromString(Value, Utils.GetCultureInfo)
        End Function

        Public Shared Function FromString(Value As String, culture As CultureInfo) As DateTime
            Dim time2 As DateTime
            If DateType.TryParse(Value, time2) Then
                Return time2
            End If
            Dim args As String() = New String() {Strings.Left(Value, &H20), "Date"}
            Throw New InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", args))
        End Function

        Friend Shared Function TryParse(Value As String, ByRef Result As DateTime) As Boolean
            Dim cultureInfo As CultureInfo = Utils.GetCultureInfo
            Return DateTime.TryParse(Utils.ToHalfwidthNumbers(Value, cultureInfo), cultureInfo, (DateTimeStyles.NoCurrentDateDefault Or DateTimeStyles.AllowWhiteSpaces), Result)
        End Function

    End Class
End Namespace

