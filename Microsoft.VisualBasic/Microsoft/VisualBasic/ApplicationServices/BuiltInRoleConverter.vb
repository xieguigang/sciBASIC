Imports System
Imports System.ComponentModel
Imports System.Globalization
Imports System.Security.Permissions
Imports System.Security.Principal

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, SharedState:=True)> _
    Public Class BuiltInRoleConverter
        Inherits TypeConverter
        ' Methods
        Public Overrides Function CanConvertTo(context As ITypeDescriptorContext, destinationType As Type) As Boolean
            Return (((Not destinationType Is Nothing) AndAlso destinationType.Equals(GetType(WindowsBuiltInRole))) OrElse MyBase.CanConvertTo(context, destinationType))
        End Function

        Public Overrides Function ConvertTo(context As ITypeDescriptorContext, culture As CultureInfo, value As Object, destinationType As Type) As Object
            If ((Not destinationType Is Nothing) AndAlso destinationType.Equals(GetType(WindowsBuiltInRole))) Then
                User.ValidateBuiltInRoleEnumValue(DirectCast(value, BuiltInRole), "value")
                Return Me.GetWindowsBuiltInRole(value)
            End If
            Return MyBase.ConvertTo(context, culture, value, destinationType)
        End Function

        Private Function GetWindowsBuiltInRole(role As Object) As WindowsBuiltInRole
            Dim role2 As WindowsBuiltInRole
            Dim name As String = [Enum].GetName(GetType(BuiltInRole), role)
            Dim obj2 As Object = [Enum].Parse(GetType(WindowsBuiltInRole), name)
            If (Not obj2 Is Nothing) Then
                role2 = DirectCast(obj2, WindowsBuiltInRole)
            End If
            Return role2
        End Function

    End Class
End Namespace

