#Region "Microsoft.VisualBasic::da73ff4ab972b01ffb300a7c90ccab1e, WebCloud\SMRUCC.HTTPInternal\Core\HttpRequest\POSTReader\ReadSubStream.vb"

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

    '     Class ReadSubStream
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Read, ReadByte, Seek
    ' 
    '         Sub: Flush, SetLength, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace ApplicationServices

    Public Class ReadSubStream : Inherits Stream

        ReadOnly s As Stream

        Dim offset As Long
        Dim [end] As Long
        Dim m_position As Long

        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Length() As Long
            Get
                Return [end] - offset
            End Get
        End Property

        Public Overrides Property Position() As Long
            Get
                Return m_position - offset
            End Get
            Set
                If Value > Length Then
                    Throw New ArgumentOutOfRangeException()
                End If

                m_position = Seek(Value, SeekOrigin.Begin)
            End Set
        End Property

        Public Sub New(s As Stream, offset As Long, length As Long)
            Me.s = s
            Me.offset = offset
            Me.[end] = offset + length

            m_position = offset
        End Sub

        Public Overrides Sub Flush()
        End Sub

        Public Overrides Function Read(buffer As Byte(), dest_offset As Integer, count As Integer) As Integer
            If buffer Is Nothing Then
                Throw New ArgumentNullException("buffer")
            End If

            If dest_offset < 0 Then
                Throw New ArgumentOutOfRangeException("dest_offset", "< 0")
            End If

            If count < 0 Then
                Throw New ArgumentOutOfRangeException("count", "< 0")
            End If

            Dim len As Integer = buffer.Length

            If dest_offset > len Then
                Throw New ArgumentException("destination offset is beyond array size")
            End If
            ' reordered to avoid possible integer overflow
            If dest_offset > len - count Then
                Throw New ArgumentException("Reading would overrun buffer")
            End If

            If count > [end] - m_position Then
                count = CInt([end] - m_position)
            End If

            If count <= 0 Then
                Return 0
            Else
                s.Position = m_position
            End If

            Dim result As Integer = s.Read(buffer, dest_offset, count)

            If result > 0 Then
                m_position += result
            Else
                m_position = [end]
            End If

            Return result
        End Function

        Public Overrides Function ReadByte() As Integer
            If m_position >= [end] Then
                Return -1
            Else
                s.Position = m_position
            End If

            Dim result As Integer = s.ReadByte()

            If result < 0 Then
                m_position = [end]
            Else
                m_position += 1
            End If

            Return result
        End Function

        Public Overrides Function Seek(d As Long, origin As SeekOrigin) As Long
            Dim real As Long

            Select Case origin
                Case SeekOrigin.Begin
                    real = offset + d
                Case SeekOrigin.[End]
                    real = [end] + d
                Case SeekOrigin.Current
                    real = m_position + d
                Case Else
                    Throw New ArgumentException()
            End Select

            Dim virt As Long = real - offset

            If virt < 0 OrElse virt > Length Then
                Throw New ArgumentException()
            Else
                m_position = s.Seek(real, SeekOrigin.Begin)
            End If

            Return m_position
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotSupportedException()
        End Sub
    End Class
End Namespace
