Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Security.Permissions
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Mouse
        ' Properties
        Public ReadOnly Property ButtonsSwapped As Boolean
            Get
                If Not SystemInformation.MousePresent Then
                    Throw ExceptionUtils.GetInvalidOperationException("Mouse_NoMouseIsPresent", New String(0  - 1) {})
                End If
                Return SystemInformation.MouseButtonsSwapped
            End Get
        End Property

        Public ReadOnly Property WheelExists As Boolean
            Get
                If Not SystemInformation.MousePresent Then
                    Throw ExceptionUtils.GetInvalidOperationException("Mouse_NoMouseIsPresent", New String(0  - 1) {})
                End If
                Return SystemInformation.MouseWheelPresent
            End Get
        End Property

        Public ReadOnly Property WheelScrollLines As Integer
            Get
                If Not Me.WheelExists Then
                    Throw ExceptionUtils.GetInvalidOperationException("Mouse_NoWheelIsPresent", New String(0  - 1) {})
                End If
                Return SystemInformation.MouseWheelScrollLines
            End Get
        End Property

    End Class
End Namespace

