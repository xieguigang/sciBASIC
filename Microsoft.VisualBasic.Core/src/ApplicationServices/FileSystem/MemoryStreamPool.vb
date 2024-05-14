#Region "Microsoft.VisualBasic::cea9c0f0d6afb12c884c2ba6cd238ee4, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\MemoryStreamPool.vb"

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

    '   Total Lines: 185
    '    Code Lines: 120
    ' Comment Lines: 35
    '   Blank Lines: 30
    '     File Size: 6.87 KB


    '     Class MemoryStreamPool
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FromFile, Read, Seek, ToString
    ' 
    '         Sub: Flush, SetLength, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace ApplicationServices

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' just recommended apply this object for fast binary data read
    ''' </remarks>
    Public Class MemoryStreamPool : Inherits Stream

        ReadOnly pool As MemoryStream()

        ''' <summary>
        ''' size of each stream object in pool
        ''' </summary>
        ''' <remarks>
        ''' 20221101 data type should be int64, or math overflow 
        ''' maybe happends if the offset value is greater than 
        ''' 2GB.
        ''' </remarks>
        ReadOnly buffer_size As Long

        Public Overrides ReadOnly Property CanRead As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' the total size in bytes of the stream pool
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property Length As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Aggregate ms As MemoryStream
                       In pool
                       Into Sum(ms.Length)
            End Get
        End Property

        Dim p As Long
        ''' <summary>
        ''' the block index of <see cref="pool"/>.
        ''' </summary>
        Dim block As Integer

        Public Overrides Property Position As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return p
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As Long)
                Call Seek(value, SeekOrigin.Begin)
            End Set
        End Property

        Private Sub New(pool As IEnumerable(Of MemoryStream), size As Integer)
            Me.pool = pool.ToArray
            Me.buffer_size = size
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{Position}/{Length}, block_numbers={pool.Length}, buffer_size={StringFormats.Lanudry(CDbl(buffer_size))}] current_section={block}, section_offset={pool(block).Position}"
        End Function

        Public Overrides Sub Flush()
            For Each ms As MemoryStream In pool
                Call ms.Flush()
            Next
        End Sub

        Public Overrides Sub SetLength(value As Long)
            Throw New InvalidProgramException("is a readonly stream!")
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Throw New InvalidProgramException("is a readonly stream!")
        End Sub

        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Dim current As MemoryStream = pool(block)
            Dim buffer_size As Long = If(current.Length < Me.buffer_size, current.Length, Me.buffer_size)
            Dim [end] As Long = count + current.Position

            If [end] > buffer_size Then
                Dim delta As Integer = buffer_size - current.Position
                Dim smallBuf As Byte() = New Byte(delta - 1) {}
                Dim buf2 As Byte() = New Byte(count - delta - 1) {}

                Call current.Read(smallBuf, Scan0, smallBuf.Length)
                Call Seek(buffer_size * (block + 1), SeekOrigin.Begin)
                Call Read(buf2, Scan0, buf2.Length)

                Call Array.ConstrainedCopy(smallBuf, Scan0, buffer, Scan0, smallBuf.Length)
                Call Array.ConstrainedCopy(buf2, Scan0, buffer, smallBuf.Length, buf2.Length)
            Else
                Call pool(block).Read(buffer, offset, count)
                Call Seek(count, SeekOrigin.Current)
            End If

            Return count
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Select Case origin
                Case SeekOrigin.Current : offset += Position
                Case SeekOrigin.End : offset += Length
                Case Else
                    ' from scan0, no transform
            End Select

            block = std.Floor(offset / buffer_size)
            p = offset
            offset = offset - buffer_size * block

            Call pool(block).Seek(offset, loc:=SeekOrigin.Begin)

            Return Position
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="buffer_size">
        ''' default buffer size is 1GB
        ''' </param>
        ''' <returns></returns>
        Public Shared Function FromFile(path As String, Optional buffer_size As Integer = 1024 * 1024 * 1024 * 1) As MemoryStreamPool
            Dim pool As New List(Of MemoryStream)
            Dim buffer As Byte() = New Byte(buffer_size - 1) {}
            Dim file As Stream = New FileStream(path:=path, mode:=FileMode.Open, access:=FileAccess.Read)

            If file.Length < buffer_size Then
                buffer = New Byte(file.Length - 1) {}
                file.Read(buffer, Scan0, file.Length)
                pool.Add(New MemoryStream(buffer))
            Else
                Dim size As Integer = buffer_size

                Do While file.Position < file.Length - 1
                    Call file.Read(buffer, Scan0, count:=size)
                    Call pool.Add(New MemoryStream(buffer))

                    If file.Length - file.Position < buffer_size Then
                        size = file.Length - file.Position
                        buffer = New Byte(size - 1) {}
                    Else
                        ' 20221101 the memorystream object just assign
                        ' the input array into the internal variable
                        ' directly
                        ' we needs to create a new array to break the 
                        ' class object reference
                        buffer = New Byte(size - 1) {}
                    End If
                Loop
            End If

            Call file.Dispose()

            Return New MemoryStreamPool(pool, buffer_size)
        End Function
    End Class
End Namespace
