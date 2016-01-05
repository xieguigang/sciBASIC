Imports System
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Threading

Namespace Microsoft.VisualBasic.ApplicationServices
    <EditorBrowsable(EditorBrowsableState.Advanced), ComVisible(False)> _
    Public Class UnhandledExceptionEventArgs
        Inherits ThreadExceptionEventArgs
        ' Methods
        Public Sub New(exitApplication As Boolean, exception As Exception)
            MyBase.New(exception)
            Me.m_ExitApplication = exitApplication
        End Sub


        ' Properties
        Public Property ExitApplication As Boolean
            Get
                Return Me.m_ExitApplication
            End Get
            Set(value As Boolean)
                Me.m_ExitApplication = value
            End Set
        End Property


        ' Fields
        Private m_ExitApplication As Boolean
    End Class
End Namespace

