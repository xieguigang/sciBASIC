#Region "Microsoft.VisualBasic::64cd9fae1731dd42f832faaca9e95bda, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Parallel\Tasks\AsyncHandle.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Parallel.Tasks

    ''' <summary>
    ''' Represents the status of an asynchronous operation.(背景线程加载数据)
    ''' </summary>
    ''' <typeparam name="TOut"></typeparam>
    Public Class AsyncHandle(Of TOut)

        Public ReadOnly Property Task As Func(Of TOut)
        Public ReadOnly Property Handle As IAsyncResult

        ''' <summary>
        ''' Gets a value that indicates whether the asynchronous operation has completed.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsCompleted As Boolean
            Get
                If Handle Is Nothing Then
                    Return True
                End If

                Return Handle.IsCompleted
            End Get
        End Property

        ''' <summary>
        ''' Creates a new background task from a function handle.
        ''' </summary>
        ''' <param name="Task"></param>
        Sub New(Task As Func(Of TOut))
            Me.Task = Task
        End Sub

        ''' <summary>
        ''' Start the background task thread.(启动后台背景线程)
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As AsyncHandle(Of TOut)
            If IsCompleted Then
                Me._Handle = Task.BeginInvoke(Nothing, Nothing) ' 假若没有执行完毕也调用的话，会改变handle
            End If

            Return Me
        End Function

        ''' <summary>
        ''' 没有完成会一直阻塞线程在这里
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValue() As TOut
            If Handle Is Nothing Then
                Return Me._Task()
            Else
                Return Me.Task.EndInvoke(Handle)
            End If
        End Function
    End Class
End Namespace
