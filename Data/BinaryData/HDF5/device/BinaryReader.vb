#Region "Microsoft.VisualBasic::e3bf27565108d6b78fb336088ee33590, Data\BinaryData\HDF5\device\BinaryReader.vb"

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

    '   Total Lines: 275
    '    Code Lines: 182 (66.18%)
    ' Comment Lines: 42 (15.27%)
    '    - Xml Docs: 47.62%
    ' 
    '   Blank Lines: 51 (18.55%)
    '     File Size: 9.04 KB


    '     Class BinaryReader
    ' 
    '         Properties: ByteOrder, debugView, deltaSize, maxOffset, offset
    '                     size
    ' 
    '         Function: (+2 Overloads) readASCIIString, readBytes, readInt, readLong, readShort
    '                   ToInteger, ToLong, ToShort, ToString
    ' 
    '         Sub: clearMaxOffset, (+2 Overloads) Dispose, Mark, Reset, SetByteOrder
    '              skipBytes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace device

    Public MustInherit Class BinaryReader
        Implements IDisposable
        Implements IReaderDebugAccess

        Protected Friend m_littleEndian As Boolean
        Protected Friend m_maxOffset As Long
        Protected filesize As Long

        Dim markedPos As Long

        Public ReadOnly Property debugView As String
            Get
                Dim debug As String
                Dim pos As Long = offset
                Dim width As Integer = 64

                ' randomaccessfile.Position = pos - width
                debug = Helpers.getDebugView(Me, width)
                offset = pos

                Return debug
            End Get
        End Property

        Public ReadOnly Property maxOffset As Long
            Get
                Return Me.m_maxOffset
            End Get
        End Property

        ''' <summary>
        ''' the read position of the file
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property offset As Long Implements IReaderDebugAccess.Position

        ''' <summary>
        ''' The file size in bytes
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Long Implements IReaderDebugAccess.Length
            Get
                Return filesize
            End Get
        End Property

        Public ReadOnly Property deltaSize As Integer
            Get
                Return offset - markedPos
            End Get
        End Property

        ''' <summary>
        ''' Represents the possible endianness of binary data.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByteOrder As ByteOrder
            Get
                If m_littleEndian Then
                    Return ByteOrder.LittleEndian
                Else
                    Return ByteOrder.BigEndian
                End If
            End Get
        End Property

        Public Sub clearMaxOffset()
            Me.m_maxOffset = 0
        End Sub

        Public MustOverride Function getBuffer() As ByteBuffer

        Public MustOverride Function readByte() As Byte

        Public MustOverride Sub close()

        Public Sub SetByteOrder(order As ByteOrder)
            If order = ByteOrder.BigEndian Then
                m_littleEndian = False
            Else
                m_littleEndian = True
            End If
        End Sub

        Public Sub Mark()
            markedPos = offset
        End Sub

        Public Sub Reset()
            offset = markedPos
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{offset}/{filesize}] {ByteOrder.ToString}"
        End Function

        Public Function readBytes(n As Integer) As Byte() Implements IReaderDebugAccess.ReadBytes
            If n < 0 Then
                ' Throw New ArgumentException("n should be greater than 0")
                Return {}
            End If

            Dim buf As Byte() = New Byte(n - 1) {}
            For i As Integer = 0 To n - 1
                buf(i) = readByte()
            Next
            Return buf
        End Function

        Public Sub skipBytes(n As Integer)
            If n < 0 Then
                Throw New ArgumentException("n should be greater than 0")
            End If

            For i As Integer = 0 To n - 1
                readByte()
            Next
        End Sub

        ''' <summary>
        ''' Read 4 bytes 32 bit integer
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function readInt() As Integer
            Return ToInteger(readBytes(4), m_littleEndian)
        End Function

        Public Shared Function ToInteger(data As Byte(), littleEndian As Boolean) As Integer
            Dim temp As Integer = 0

            If littleEndian Then
                temp = (data(0) And &HFF)
                temp = temp Or (data(1) And &HFF) << 8
                temp = temp Or (data(2) And &HFF) << 16
                temp = temp Or (data(3) And &HFF) << 24
            Else
                temp = (data(0) And &HFF) << 24
                temp = temp Or (data(1) And &HFF) << 16
                temp = temp Or (data(2) And &HFF) << 8
                temp = temp Or (data(3) And &HFF)
            End If

            Return temp
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function readLong() As Long
            Return ToLong(readBytes(8), m_littleEndian)
        End Function

        Public Shared Function ToLong(data As Byte(), littleEndian As Boolean) As Long
            ' 此前使用 Integer 左移 (<< 24/32/40/48/56) 来拼接字节，
            ' 当 byte ≥ 0x80 时 Integer 溢出，OR 到 Long 后被符号扩展为 0xFFFFFFFF_xxxxxx，
            ' 导致所有 ≥ 2^31 的地址被读成负数。
            ' 修复：用 BitConverter.ToInt64 直接转换，彻底避免移位溢出。
            If data.Length < 8 Then
                Dim padded(7) As Byte
                Array.Copy(data, padded, data.Length)
                data = padded
            End If

            If littleEndian = BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt64(data, 0)
            Else
                Dim rev As Byte() = {data(7), data(6), data(5), data(4), data(3), data(2), data(1), data(0)}
                Return BitConverter.ToInt64(rev, 0)
            End If
        End Function

        ''' <summary>
        ''' read 2 byte integer
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function readShort() As Short
            Return ToShort(readBytes(2), m_littleEndian)
        End Function

        Public Shared Function ToShort(data As Byte(), littleEndian As Boolean) As Short
            Dim temp As Short = 0

            If littleEndian Then
                temp = CShort(data(0) And &HFF)
                temp = temp Or CShort((data(1) And &HFF) << 8)
            Else
                temp = CShort((data(0) And &HFF) << 8)
                temp = temp Or CShort(data(1) And &HFF)
            End If

            Return temp
        End Function

        Public Function readASCIIString() As String
            Dim sb As New StringBuilder()

            For i As Long = Me.offset To Me.size - 1
                Dim c As Char = ChrW(readByte())

                If c = ControlChars.NullChar Then
                    Exit For
                Else
                    sb.Append(c)
                End If
            Next

            Return sb.ToString()
        End Function

        Public Function readASCIIString(length As Integer) As String
            Dim sb As New StringBuilder()
            Dim nCount As Integer = 0

            For i As Long = 0 To length - 1
                Dim c As Char = ChrW(readByte())
                nCount += 1
                If c = ControlChars.NullChar Then
                    Exit For
                Else
                    sb.Append(c)
                End If
            Next

            If nCount < length Then
                skipBytes(length - nCount)
            End If

            Return sb.ToString()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用



        ' IDisposable
        Protected Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                    Call close()
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

End Namespace
