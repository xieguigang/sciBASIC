Imports System
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.InteropServices

Namespace Microsoft.VisualBasic.ApplicationServices
    <EditorBrowsable(EditorBrowsableState.Advanced), ComVisible(False)> _
    Public Class StartupEventArgs
        Inherits CancelEventArgs
        ' Methods
        Public Sub New(args As ReadOnlyCollection(Of String))
            If (args Is Nothing) Then
                args = New ReadOnlyCollection(Of String)(Nothing)
            End If
            Me.m_CommandLine = args
        End Sub


        ' Properties
        Public ReadOnly Property CommandLine As ReadOnlyCollection(Of String)
            Get
                Return Me.m_CommandLine
            End Get
        End Property


        ' Fields
        Private m_CommandLine As ReadOnlyCollection(Of String)
    End Class
End Namespace

