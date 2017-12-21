#Region "Microsoft.VisualBasic::0a14747f40e78a09ce9901f39f752df5, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\MMFProtocol\ProcessLock.vb"

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

Imports System.Text.Encoding

Namespace MMFProtocol

    ''' <summary>
    ''' 进程排斥锁
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProcessLock : Implements System.IDisposable

        Private Const PROCESS_CHECKOUT_SIGNAL As String = "processlock-checkout-signal::[Guid('4CBC086D-179C-494E-81C6-A040667B49F0')]"
        Private Const PROCESS_LOCKED___SIGNAL As String = "processlock-checkout-locked::[Guid('4CBC086D-179C-494E-81C6-A040667B49F0')]"

        ''' <summary>
        ''' 进程排斥锁
        ''' </summary>
        ''' <remarks>
        ''' 程序中采用一个进程排斥锁是由于待日后Mono运行时环境在Linux平台中的WinForm GTK成熟后，向Linux平台迁移，
        ''' 由于Visual Baisc/C#所编写的应用程序需要保持单个进程，则需要启用应用程序框架，而很多情况下为了优化的需求应用程序
        ''' 无法使用应用程序框架，为了实现一次编译到处运行的目的，程序的代码不会再平台间进行修改，
        ''' 由于Linux平台之上不能使用Win32API来保持单进程，为了保持程序对Windows/Linux/MAC三大操作系统的兼容性，故而在这里使用了一个进程排斥锁
        ''' </remarks>
        Dim WithEvents ProcessLock As MMFSocket

        ''' <summary>
        ''' 进程锁的排斥情况
        ''' </summary>
        ''' <remarks></remarks>
        Dim f_ProcessLock As Boolean = False

        Private Sub ProcessLockDataArrival(data() As Byte)
            Dim strMessage As String = Unicode.GetString(data)   '服务器进程锁接搜到的来自进程排斥锁的新消息

            If strMessage = PROCESS_CHECKOUT_SIGNAL Then    '这个是已经在运行的进程对新启动的进程的进程锁的检查的回应
                Call ProcessLock.SendMessage(Unicode.GetBytes(PROCESS_LOCKED___SIGNAL))
            ElseIf strMessage = PROCESS_LOCKED___SIGNAL Then
                f_ProcessLock = True    '对新启动的进程执行加锁操作，杀死新启动的服务器进程
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strHost">进程排斥锁的锁名</param>
        ''' <remarks></remarks>
        Sub New(strHost As String)
            ProcessLock = New MMFSocket(strHost, AddressOf Me.ProcessLockDataArrival)
        End Sub

        ''' <summary>
        ''' 返回当前的进程是否被加锁
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Locked() As Boolean
            Get
                'socket会首先尝试发送一个信号，然后根据有无响应来判断是否还有其他的服务器进程的运行
                Call ProcessLock.SendMessage(Unicode.GetBytes(PROCESS_CHECKOUT_SIGNAL))
                Call System.Threading.Thread.Sleep(100)       '等待响应的超时时间为100ms

                Return f_ProcessLock
            End Get
        End Property

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
