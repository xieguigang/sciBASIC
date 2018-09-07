#Region "Microsoft.VisualBasic::101f479c2f1ea836c79f49966c0e3b41, Data\BinaryData\BinaryData\Storage\StringWriter.vb"

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

    ' Class StringWriter
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) Append, ToString, Write
    ' 
    '     Sub: Close, (+2 Overloads) Dispose
    ' 
    ' Class StringReader
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: ReadString, ReadStringArray
    ' 
    '     Sub: (+2 Overloads) Dispose, Seek
    ' 
    ' Structure SectionHeader
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) CreateBuffer, ToArray
    ' 
    ' Enum Types
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Class StringWriter
    Implements IDisposable

    Dim encoding As Encodings
    Dim codepage As Encoding

    Public ReadOnly stream As BinaryWriter

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(path$, Optional encoding As Encodings = Encodings.ASCII)
        Call Me.New(path.Open, encoding)
    End Sub

    Sub New(out As Stream, Optional encoding As Encodings = Encodings.ASCII)
        Me.stream = New BinaryWriter(out)
        Me.encoding = encoding
        Me.codepage = encoding.CodePage
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Append(text As String) As Integer
        Dim buffer = SectionHeader.CreateBuffer(text, codepage)
        Return Write(buffer.head, buffer.bytes)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function Write(header As SectionHeader, bytes As Byte()) As Integer
        ' 5 = 4 + 1
        Call stream.Write(header.ToArray, Scan0, 5)
        Call stream.Write(bytes, Scan0, bytes.Length)

        Return 5 + bytes.Length
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Append(array As IEnumerable(Of String)) As Integer
        Dim buffer = SectionHeader.CreateBuffer(array, codepage)
        Return Write(buffer.head, buffer.bytes)
    End Function

    Public Overrides Function ToString() As String
        Return stream.ToString
    End Function

    Public Sub Close()
        Call stream.Flush()
        Call stream.BaseStream.Flush()
        Call stream.BaseStream.Close()
        Call stream.Close()
        Call stream.Dispose()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 要检测冗余调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)。
                Call Close()
            End If

            ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
            ' TODO: 将大型字段设置为 null。
        End If
        disposedValue = True
    End Sub

    ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
    'Protected Overrides Sub Finalize()
    '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic 添加此代码以正确实现可释放模式。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        Dispose(True)
        ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

Public Class StringReader : Implements IDisposable

    Dim encoding As Encodings
    Dim codepage As Encoding
    Dim stream As BinaryReader

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(path$, Optional encoding As Encodings = Encodings.ASCII)
        Call Me.New(path.Open(FileMode.Open, doClear:=False), encoding)
    End Sub

    Sub New(out As Stream, Optional encoding As Encodings = Encodings.ASCII)
        Me.stream = New BinaryReader(out)
        Me.encoding = encoding
        Me.codepage = encoding.CodePage
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Seek(offset As Long)
        Call stream.BaseStream.Seek(offset, SeekOrigin.Begin)
    End Sub

    Public Function ReadString() As String
        Dim header As New SectionHeader(stream.ReadBytes(5))
        Dim bytes = stream.ReadBytes(header.bytes)
        Dim text = codepage.GetString(bytes, Scan0, bytes.Length)
        Return text
    End Function

    Public Function ReadStringArray() As String()
        Return ReadString.LoadJSON(Of String())
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 要检测冗余调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)。
                Call stream.Close()
            End If

            ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
            ' TODO: 将大型字段设置为 null。
        End If
        disposedValue = True
    End Sub

    ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
    'Protected Overrides Sub Finalize()
    '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic 添加此代码以正确实现可释放模式。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        Dispose(True)
        ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

Public Structure SectionHeader

    Dim bytes As Integer
    Dim type As Types

    Const offset As Integer = 4

    Sub New(bytes As Byte())
        Me.bytes = BitConverter.ToInt32(bytes, Scan0)
        Me.type = CType(bytes(offset), Types)
    End Sub

    Public Function ToArray() As Byte()
        Dim buffer As Byte() = New Byte(5) {}
        Dim int As Byte() = BitConverter.GetBytes(bytes)
        Array.ConstrainedCopy(int, Scan0, buffer, Scan0, offset)
        buffer(offset) = type
        Return buffer
    End Function

    Public Shared Function CreateBuffer(text$, codepage As Encoding) As (head As SectionHeader, bytes As Byte())
        Dim bytes As Byte() = codepage.GetBytes(text)
        Dim head As New SectionHeader With {
            .bytes = bytes.Length,
            .type = Types.String
        }

        Return (head, bytes)
    End Function

    Public Shared Function CreateBuffer(list As IEnumerable(Of String), codepage As Encoding) As (head As SectionHeader, bytes As Byte())
        Dim bytes As Byte() = codepage.GetBytes(list.ToArray.GetJson)
        Dim head As New SectionHeader With {
            .bytes = bytes.Length,
            .type = Types.StringArray
        }

        Return (head, bytes)
    End Function
End Structure

Public Enum Types As Byte
    [String] = 0
    StringArray = 2
End Enum
