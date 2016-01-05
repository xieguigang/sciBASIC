Imports System
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Deployment.Application
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class ConsoleApplicationBase
        Inherits ApplicationBase
        ' Properties
        Public ReadOnly Property CommandLineArgs As ReadOnlyCollection(Of String)
            Get
                If (Me.m_CommandLineArgs Is Nothing) Then
                    Dim commandLineArgs As String() = Environment.GetCommandLineArgs
                    If (commandLineArgs.GetLength(0) >= 2) Then
                        Dim destinationArray As String() = New String(((commandLineArgs.GetLength(0) - 2) + 1)  - 1) {}
                        Array.Copy(commandLineArgs, 1, destinationArray, 0, (commandLineArgs.GetLength(0) - 1))
                        Me.m_CommandLineArgs = New ReadOnlyCollection(Of String)(destinationArray)
                    Else
                        Me.m_CommandLineArgs = New ReadOnlyCollection(Of String)(New String(0  - 1) {})
                    End If
                End If
                Return Me.m_CommandLineArgs
            End Get
        End Property

        Public ReadOnly Property Deployment As ApplicationDeployment
            Get
                Return ApplicationDeployment.CurrentDeployment
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected WriteOnly Property InternalCommandLine As ReadOnlyCollection(Of String)
            Set(value As ReadOnlyCollection(Of String))
                Me.m_CommandLineArgs = value
            End Set
        End Property

        Public ReadOnly Property IsNetworkDeployed As Boolean
            Get
                Return ApplicationDeployment.IsNetworkDeployed
            End Get
        End Property


        ' Fields
        Private m_CommandLineArgs As ReadOnlyCollection(Of String)
    End Class
End Namespace

