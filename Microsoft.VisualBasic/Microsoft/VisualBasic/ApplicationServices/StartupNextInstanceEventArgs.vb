Imports System
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.ApplicationServices
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Class StartupNextInstanceEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(args As ReadOnlyCollection(Of String), bringToForegroundFlag As Boolean)
            If (args Is Nothing) Then
                args = New ReadOnlyCollection(Of String)(Nothing)
            End If
            Me.m_CommandLine = args
            Me.m_BringToForeground = bringToForegroundFlag
        End Sub


        ' Properties
        Public Property BringToForeground As Boolean
            Get
                Return Me.m_BringToForeground
            End Get
            Set(value As Boolean)
                Me.m_BringToForeground = value
            End Set
        End Property

        Public ReadOnly Property CommandLine As ReadOnlyCollection(Of String)
            Get
                Return Me.m_CommandLine
            End Get
        End Property


        ' Fields
        Private m_BringToForeground As Boolean
        Private m_CommandLine As ReadOnlyCollection(Of String)
    End Class
End Namespace

