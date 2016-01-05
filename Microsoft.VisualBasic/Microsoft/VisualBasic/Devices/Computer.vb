Imports Microsoft.VisualBasic.MyServices
Imports System.Security.Permissions
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Computer
        Inherits ServerComputer
        ' Properties
        Public ReadOnly Property Audio As Audio
            Get
                If (Me.m_Audio Is Nothing) Then
                    Me.m_Audio = New Audio
                End If
                Return Me.m_Audio
            End Get
        End Property

        Public ReadOnly Property Clipboard As ClipboardProxy
            Get
                If (Computer.m_Clipboard Is Nothing) Then
                    Computer.m_Clipboard = New ClipboardProxy
                End If
                Return Computer.m_Clipboard
            End Get
        End Property

        Public ReadOnly Property Keyboard As Keyboard
            Get
                If (Computer.m_KeyboardInstance Is Nothing) Then
                    Computer.m_KeyboardInstance = New Keyboard
                End If
                Return Computer.m_KeyboardInstance
            End Get
        End Property

        Public ReadOnly Property Mouse As Mouse
            Get
                If (Computer.m_Mouse Is Nothing) Then
                    Computer.m_Mouse = New Mouse
                End If
                Return Computer.m_Mouse
            End Get
        End Property

        Public ReadOnly Property Ports As Ports
            Get
                If (Me.m_Ports Is Nothing) Then
                    Me.m_Ports = New Ports
                End If
                Return Me.m_Ports
            End Get
        End Property

        Public ReadOnly Property Screen As Screen
            Get
                Return Screen.PrimaryScreen
            End Get
        End Property


        ' Fields
        Private m_Audio As Audio
        Private Shared m_Clipboard As ClipboardProxy
        Private Shared m_KeyboardInstance As Keyboard
        Private Shared m_Mouse As Mouse
        Private m_Ports As Ports
    End Class
End Namespace

