Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.Logging
Imports System
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Security.Permissions
Imports System.Threading

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class ApplicationBase
        ' Methods
        <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
        Public Sub ChangeCulture(cultureName As String)
            Thread.CurrentThread.CurrentCulture = New CultureInfo(cultureName)
        End Sub

        Public Sub ChangeUICulture(cultureName As String)
            Thread.CurrentThread.CurrentUICulture = New CultureInfo(cultureName)
        End Sub

        Public Function GetEnvironmentVariable(name As String) As String
            Dim environmentVariable As String = Environment.GetEnvironmentVariable(name)
            If (environmentVariable Is Nothing) Then
                Dim placeHolders As String() = New String() {name}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("name", "EnvVarNotFound_Name", placeHolders)
            End If
            Return environmentVariable
        End Function


        ' Properties
        Public ReadOnly Property Culture As CultureInfo
            Get
                Return Thread.CurrentThread.CurrentCulture
            End Get
        End Property

        Public ReadOnly Property Info As AssemblyInfo
            <MethodImpl(MethodImplOptions.NoInlining), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
            Get
                If (Me.m_Info Is Nothing) Then
                    Dim entryAssembly As Assembly = Assembly.GetEntryAssembly
                    If (entryAssembly Is Nothing) Then
                        entryAssembly = Assembly.GetCallingAssembly
                    End If
                    Me.m_Info = New AssemblyInfo(entryAssembly)
                End If
                Return Me.m_Info
            End Get
        End Property

        Public ReadOnly Property Log As Log
            Get
                If (Me.m_Log Is Nothing) Then
                    Me.m_Log = New Log
                End If
                Return Me.m_Log
            End Get
        End Property

        Public ReadOnly Property UICulture As CultureInfo
            Get
                Return Thread.CurrentThread.CurrentUICulture
            End Get
        End Property


        ' Fields
        Private m_Info As AssemblyInfo
        Private m_Log As Log
    End Class
End Namespace

