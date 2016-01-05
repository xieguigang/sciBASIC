Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.MyServices.Internal
Imports System
Imports System.Security.Permissions
Imports System.Security.Principal

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class WebUser
        Inherits User
        ' Properties
        Protected Overrides Property InternalPrincipal As IPrincipal
            Get
                Dim current As Object = SkuSafeHttpContext.Current
                If (current Is Nothing) Then
                    Throw ExceptionUtils.GetInvalidOperationException("WebNotSupportedOnThisSKU", New String(0  - 1) {})
                End If
                Return DirectCast(NewLateBinding.LateGet(current, Nothing, "User", New Object(0  - 1) {}, Nothing, Nothing, Nothing), IPrincipal)
            End Get
            Set(value As IPrincipal)
                Dim current As Object = SkuSafeHttpContext.Current
                If (current Is Nothing) Then
                    Throw ExceptionUtils.GetInvalidOperationException("WebNotSupportedOnThisSKU", New String(0 - 1) {})
                End If
                Dim arguments As Object() = New Object() {value}
                NewLateBinding.LateSet(current, Nothing, "User", arguments, Nothing, Nothing)
            End Set
        End Property

    End Class
End Namespace

