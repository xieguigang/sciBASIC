Imports System
Imports System.ComponentModel
Imports System.Security.Permissions
Imports System.Security.Principal
Imports System.Threading

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class User
        ' Methods
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Public Sub InitializeWithWindowsUser()
            Thread.CurrentPrincipal = New WindowsPrincipal(WindowsIdentity.GetCurrent)
        End Sub

        Public Function IsInRole(role As BuiltInRole) As Boolean
            User.ValidateBuiltInRoleEnumValue(role, "role")
            Dim converter As TypeConverter = TypeDescriptor.GetConverter(GetType(BuiltInRole))
            If Me.IsWindowsPrincipal Then
                Dim role2 As WindowsBuiltInRole = DirectCast(converter.ConvertTo(role, GetType(WindowsBuiltInRole)), WindowsBuiltInRole)
                Return DirectCast(Me.InternalPrincipal, WindowsPrincipal).IsInRole(role2)
            End If
            Return Me.InternalPrincipal.IsInRole(converter.ConvertToString(role))
        End Function

        Public Function IsInRole(role As String) As Boolean
            Return Me.InternalPrincipal.IsInRole(role)
        End Function

        Private Function IsWindowsPrincipal() As Boolean
            Return TypeOf Me.InternalPrincipal Is WindowsPrincipal
        End Function

        Friend Shared Sub ValidateBuiltInRoleEnumValue(testMe As BuiltInRole, parameterName As String)
            If (((((testMe <> BuiltInRole.AccountOperator) AndAlso (testMe <> BuiltInRole.Administrator)) AndAlso ((testMe <> BuiltInRole.BackupOperator) AndAlso (testMe <> BuiltInRole.Guest))) AndAlso (((testMe <> BuiltInRole.PowerUser) AndAlso (testMe <> BuiltInRole.PrintOperator)) AndAlso ((testMe <> BuiltInRole.Replicator) AndAlso (testMe <> BuiltInRole.SystemOperator)))) AndAlso (testMe <> BuiltInRole.User)) Then
                Throw New InvalidEnumArgumentException(parameterName, CInt(testMe), GetType(BuiltInRole))
            End If
        End Sub


        ' Properties
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Property CurrentPrincipal As IPrincipal
            Get
                Return Me.InternalPrincipal
            End Get
            Set(value As IPrincipal)
                Me.InternalPrincipal = value
            End Set
        End Property

        Protected Overridable Property InternalPrincipal As IPrincipal
            Get
                Return Thread.CurrentPrincipal
            End Get
            Set(value As IPrincipal)
                Thread.CurrentPrincipal = value
            End Set
        End Property

        Public ReadOnly Property IsAuthenticated As Boolean
            Get
                Return Me.InternalPrincipal.Identity.IsAuthenticated
            End Get
        End Property

        Public ReadOnly Property Name As String
            Get
                Return Me.InternalPrincipal.Identity.Name
            End Get
        End Property

    End Class
End Namespace

