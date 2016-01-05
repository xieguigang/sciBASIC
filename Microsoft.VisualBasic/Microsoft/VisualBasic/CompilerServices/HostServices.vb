Imports System
Imports System.ComponentModel
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.SharedState)> _
    Public NotInheritable Class HostServices
        ' Properties
        Public Shared Property VBHost As IVbHost
            Get
                Return HostServices.m_host
            End Get
            Set(Value As IVbHost)
                HostServices.m_host = Value
            End Set
        End Property


        ' Fields
        Private Shared m_host As IVbHost
    End Class
End Namespace

