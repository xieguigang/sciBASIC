Imports System
Imports System.Diagnostics
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.Logging
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class AspLog
        Inherits Log
        ' Methods
        Public Sub New()
        End Sub

        <SecuritySafeCritical>
        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        <SecuritySafeCritical> _
        Protected Friend Overrides Sub InitializeWithDefaultsSinceNoConfigExists()
            Dim type As Type = Type.GetType("System.Web.WebPageTraceListener, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A")
            If (Not type Is Nothing) Then
                MyBase.TraceSource.Listeners.Add(DirectCast(Activator.CreateInstance(type), TraceListener))
            End If
            MyBase.TraceSource.Switch.Level = SourceLevels.Information
        End Sub

    End Class
End Namespace

