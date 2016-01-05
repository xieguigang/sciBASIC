Imports System

Namespace Microsoft.VisualBasic.Devices
    Public Class NetworkAvailableEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(networkAvailable As Boolean)
            Me.m_NetworkAvailable = networkAvailable
        End Sub


        ' Properties
        Public ReadOnly Property IsNetworkAvailable As Boolean
            Get
                Return Me.m_NetworkAvailable
            End Get
        End Property


        ' Fields
        Private m_NetworkAvailable As Boolean
    End Class
End Namespace

