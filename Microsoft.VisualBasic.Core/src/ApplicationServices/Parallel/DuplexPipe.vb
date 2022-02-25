#Region "Microsoft.VisualBasic::1601487dba82b3ae9a81782a6e7caf90, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\DuplexPipe.vb"

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

'     Class DuplexPipe
' 
'         Properties: Length
' 
'         Function: GetBlocks, Read
' 
'         Sub: Close, Wait, Write
' 
'     Class BufferPipe
' 
' 
' 
'     Class StreamPipe
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetBlocks, Read
' 
'         Sub: (+2 Overloads) Dispose
' 
'     Class DataPipe
' 
'         Constructor: (+4 Overloads) Sub New
' 
'         Function: GetBlocks, Read
' 
'         Sub: (+2 Overloads) Dispose
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel

    Public Class DuplexPipe : Inherits BufferPipe

        ReadOnly dataFragments As New Queue(Of Byte())
        ReadOnly writeClose As New Value(Of Boolean)(False)

        Public ReadOnly Property Length As Long

        Public Sub Close()
            writeClose.Value = True
        End Sub

        Public Sub Write(buffer As Byte())
            SyncLock dataFragments
                _Length += buffer.Length
                dataFragments.Enqueue(buffer)
            End SyncLock
        End Sub

        Public Sub Wait()
            Do While dataFragments.Count > 0
                Call Thread.Sleep(1)
            Loop
        End Sub

        Public Overrides Function Read() As Byte()
            Do While dataFragments.Count = 0
                If writeClose.Value AndAlso dataFragments.Count = 0 Then
                    Return {}
                Else
                    Call Thread.Sleep(1)
                End If
            Loop

            SyncLock dataFragments
                Return dataFragments.Dequeue
            End SyncLock
        End Function

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Do While Not writeClose.Value
                Yield Read()
            Loop
        End Function
    End Class

    Public MustInherit Class BufferPipe

        Public MustOverride Iterator Function GetBlocks() As IEnumerable(Of Byte())
        Public MustOverride Function Read() As Byte()

    End Class

    Public Class StreamPipe : Inherits BufferPipe
        Implements IDisposable

        ReadOnly buf As Stream
        Private disposedValue As Boolean

        Sub New(buf As Stream)
            Me.buf = buf
        End Sub

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Dim chunk As Byte() = New Byte(1024 - 1) {}
            Dim delta As Long

            Do While buf.Position <= (buf.Length - 1)
                delta = buf.Length - buf.Position

                If delta < chunk.Length Then
                    chunk = New Byte(delta - 1) {}
                End If

                buf.Read(chunk, Scan0, chunk.Length)

                Yield chunk
            Loop
        End Function

        Public Overrides Function Read() As Byte()
            Return GetBlocks.IteratesALL.ToArray
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    buf.Close()
                    buf.Dispose()
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

    Public Class DataPipe : Inherits BufferPipe
        Implements IDisposable

        Dim data As Byte()
        Dim disposedValue As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Byte))
            Me.data = data.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(str As String)
            Me.data = Encoding.UTF8.GetBytes(str)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Double))
            Me.data = data _
                .Select(Function(mzi) BitConverter.GetBytes(mzi)) _
                .IteratesALL _
                .ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As RawStream)
            Call Me.New(data.Serialize)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As MemoryStream)
            Call Me.New(data.ToArray)
        End Sub

        <DebuggerStepThrough>
        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Yield data
        End Function

        <DebuggerStepThrough>
        Public Overrides Function Read() As Byte()
            Return data
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Erase data
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
End Namespace
