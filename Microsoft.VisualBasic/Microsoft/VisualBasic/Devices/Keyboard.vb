Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Security
Imports System.Security.Permissions
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Keyboard
        ' Methods
        Public Sub SendKeys(keys As String)
            Me.SendKeys(keys, False)
        End Sub

        Public Sub SendKeys(keys As String, wait As Boolean)
            If wait Then
                SendKeys.SendWait(keys)
            Else
                SendKeys.Send(keys)
            End If
        End Sub


        ' Properties
        Public ReadOnly Property AltKeyDown As Boolean
            Get
                Return ((Control.ModifierKeys And Keys.Alt) > Keys.None)
            End Get
        End Property

        Public ReadOnly Property CapsLock As Boolean
            <SecuritySafeCritical> _
            Get
                Return ((UnsafeNativeMethods.GetKeyState(20) And 1) > 0)
            End Get
        End Property

        Public ReadOnly Property CtrlKeyDown As Boolean
            Get
                Return ((Control.ModifierKeys And Keys.Control) > Keys.None)
            End Get
        End Property

        Public ReadOnly Property NumLock As Boolean
            <SecuritySafeCritical> _
            Get
                Return ((UnsafeNativeMethods.GetKeyState(&H90) And 1) > 0)
            End Get
        End Property

        Public ReadOnly Property ScrollLock As Boolean
            <SecuritySafeCritical> _
            Get
                Return ((UnsafeNativeMethods.GetKeyState(&H91) And 1) > 0)
            End Get
        End Property

        Public ReadOnly Property ShiftKeyDown As Boolean
            Get
                Return ((Control.ModifierKeys And Keys.Shift) > Keys.None)
            End Get
        End Property

    End Class
End Namespace

