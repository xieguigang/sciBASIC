Imports Microsoft.VisualBasic.MyServices
Imports System
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class ServerComputer
        ' Properties
        Public ReadOnly Property Clock As Clock
            Get
                If (ServerComputer.m_Clock Is Nothing) Then
                    ServerComputer.m_Clock = New Clock
                End If
                Return ServerComputer.m_Clock
            End Get
        End Property

        Public ReadOnly Property FileSystem As FileSystemProxy
            Get
                If (Me.m_FileIO Is Nothing) Then
                    Me.m_FileIO = New FileSystemProxy
                End If
                Return Me.m_FileIO
            End Get
        End Property

        Public ReadOnly Property Info As ComputerInfo
            Get
                If (Me.m_ComputerInfo Is Nothing) Then
                    Me.m_ComputerInfo = New ComputerInfo
                End If
                Return Me.m_ComputerInfo
            End Get
        End Property

        Public ReadOnly Property Name As String
            Get
                Return Environment.MachineName
            End Get
        End Property

        Public ReadOnly Property Network As Network
            Get
                If (Me.m_Network Is Nothing) Then
                    Me.m_Network = New Network
                End If
                Return Me.m_Network
            End Get
        End Property

        Public ReadOnly Property Registry As RegistryProxy
            Get
                If (Me.m_RegistryInstance Is Nothing) Then
                    Me.m_RegistryInstance = New RegistryProxy
                End If
                Return Me.m_RegistryInstance
            End Get
        End Property


        ' Fields
        Private Shared m_Clock As Clock
        Private m_ComputerInfo As ComputerInfo
        Private m_FileIO As FileSystemProxy
        Private m_Network As Network
        Private m_RegistryInstance As RegistryProxy
    End Class
End Namespace

