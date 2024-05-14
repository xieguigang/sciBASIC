#Region "Microsoft.VisualBasic::4b561929ac8aa841e80923efc6860ee8, Microsoft.VisualBasic.Core\src\ComponentModel\RelayCommand.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 74
    '    Code Lines: 51
    ' Comment Lines: 3
    '   Blank Lines: 20
    '     File Size: 2.10 KB


    '     Class ICallbackInvoke
    ' 
    '         Properties: CallbackInvoke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class RelayCommand
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CanExecute
    ' 
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Windows.Input
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace ComponentModel

    Public MustInherit Class ICallbackInvoke
        Implements ICallbackTask

        Protected ReadOnly _execute As Action

        Public ReadOnly Property CallbackInvoke As Action Implements ICallbackTask.CallbackInvoke
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
