#Region "Microsoft.VisualBasic::17999381e12c52a1965ae4f001bdc985, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\UserTaskCancelAction.vb"

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

    '   Total Lines: 126
    '    Code Lines: 73 (57.94%)
    ' Comment Lines: 28 (22.22%)
    '    - Xml Docs: 32.14%
    ' 
    '   Blank Lines: 25 (19.84%)
    '     File Size: 4.38 KB


    '     Class UserTaskCancelAction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Dispose, handleCancel
    ' 
    '     Class ConsoleUserTaskAction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class UserTaskSaveAction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: detectKeyEvent, Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading

Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' A finalize action after the user cancel current task operations.(ctrl + C)
    ''' </summary>
    Public Class UserTaskCancelAction : Inherits ConsoleUserTaskAction
        Implements IDisposable

        Sub New(finalize As Action)
            Call MyBase.New(userAction:=finalize)

            AddHandler Console.CancelKeyPress, AddressOf handleCancel
        End Sub

        Private Sub handleCancel(sender As Object, terminate As ConsoleCancelEventArgs)
            ' ctrl + C just break the current executation
            ' not exit program running
            terminate.Cancel = True
            userAction()
        End Sub

#Region "IDisposable Support"
        ' IDisposable
        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                    RemoveHandler Console.CancelKeyPress, AddressOf handleCancel
                End If

                ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
                ' TODO: 将大型字段设置为 null。
            End If
            disposedValue = True
        End Sub
#End Region
    End Class

    Public MustInherit Class ConsoleUserTaskAction : Implements IDisposable

        Protected disposedValue As Boolean

        ''' <summary>
        ''' 用户自定义操作
        ''' </summary>
        Protected ReadOnly userAction As Action

        Sub New(userAction As Action)
            Me.userAction = userAction
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{MyClass.GetType.Name}] {userAction.ToString}"
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    ''' <summary>
    ''' ``ctrl + S``
    ''' </summary>
    Public Class UserTaskSaveAction : Inherits ConsoleUserTaskAction
        Implements IDisposable

        ReadOnly workerThread As Thread

        Sub New(save As Action)
            Call MyBase.New(userAction:=save)

            workerThread = New Thread(AddressOf detectKeyEvent)
            workerThread.Start()
        End Sub

        Private Sub detectKeyEvent()
            Do While App.Running AndAlso Not disposedValue
                Dim key As ConsoleKeyInfo = Console.ReadKey()

                If key.Modifiers = ConsoleModifiers.Control AndAlso key.Key = ConsoleKey.S Then
                    Call userAction()
                End If
            Loop
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Try
                        Call workerThread.Abort()
                    Catch ex As Exception

                    End Try
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub
    End Class
End Namespace
