Imports System.Windows.Input
Imports System.Diagnostics

''' <summary>
''' Taken from http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
''' </summary>
Public Class RelayCommand
    Implements ICommand
#Region "Members"

    Private ReadOnly _canExecute As Func(Of [Boolean])
    Private ReadOnly _execute As Action

#End Region

#Region "Constructors"

    Public Sub New(execute As Action)
        Me.New(execute, Nothing)
    End Sub

    Public Sub New(execute As Action, canExecute As Func(Of [Boolean]))
        If execute Is Nothing Then
            Throw New ArgumentNullException("execute")
        End If
        _execute = execute
        _canExecute = canExecute
    End Sub

#End Region

#Region "ICommand Members"

    <DebuggerStepThrough> _
    Public Function CanExecute(parameter As [Object]) As [Boolean] Implements ICommand.CanExecute
        Return If(_canExecute Is Nothing, True, _canExecute())
    End Function

    Public Sub Execute(parameter As [Object]) Implements ICommand.Execute
        _execute()
    End Sub

#End Region

    Public Event CanExecuteChanged(sender As Object, e As EventArgs) Implements ICommand.CanExecuteChanged
End Class
