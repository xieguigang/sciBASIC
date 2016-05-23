Imports System.Windows.Input
Imports System.Diagnostics
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace ComponentModel

    Public MustInherit Class ICallbackInvoke
        Implements ICallbackTask

        Protected ReadOnly _execute As Action

        Public ReadOnly Property CallbackInvoke As Action Implements ICallbackTask.CallbackInvoke
            Get
                Return _execute
            End Get
        End Property

        Const execute As String = NameOf(execute)

        Protected Sub New(callback As Action)
            If callback Is Nothing Then
                Throw New ArgumentNullException(execute)
            Else
                _execute = callback
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return _execute.ToString
        End Function
    End Class

    ''' <summary>
    ''' Taken from http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
    ''' </summary>
    Public Class RelayCommand : Inherits ICallbackInvoke
        Implements ICommand

#Region "Members"

        ReadOnly _canExecute As Func(Of [Boolean])

#End Region

#Region "Constructors"

        Public Sub New(execute As Action)
            Me.New(execute, Nothing)
        End Sub

        Public Sub New(execute As Action, canExecute As Func(Of [Boolean]))
            Call MyBase.New(execute)
            _canExecute = canExecute
        End Sub

#End Region

#Region "ICommand Members"

        <DebuggerStepThrough>
        Public Function CanExecute(parameter As [Object]) As [Boolean] Implements ICommand.CanExecute
            Return If(_canExecute Is Nothing, True, _canExecute())
        End Function

        Public Sub Execute(parameter As [Object]) Implements ICommand.Execute
            _execute()
        End Sub

#End Region

        Public Event CanExecuteChanged(sender As Object, e As EventArgs) Implements ICommand.CanExecuteChanged
    End Class
End Namespace