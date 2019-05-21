#Region "Microsoft.VisualBasic::d396442c0e23d605a78385ec31baaf0d, mime\application%netcdf\HDF5\io\BinaryReader.vb"

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

    '     Class BinaryReader
    ' 
    '         Properties: bigEndian, littleEndian, maxOffset, offset, size
    ' 
    '         Function: (+2 Overloads) readASCIIString, readBytes, readInt, readLong, readShort
    ' 
    '         Sub: clearMaxOffset, setBigEndian, setLittleEndian, skipBytes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace HDF5.IO

    Public MustInherit Class BinaryReader : Implements IDisposable

        Protected Friend m_littleEndian As Boolean
        Protected Friend m_maxOffset As Long
        Protected filesize As Long

        Public Overridable ReadOnly Property maxOffset As Long
            Get
                Return Me.m_maxOffset
            End Get
        End Property

        Public Overridable Property offset As Long

        ''' <summary>
        ''' The file size in bytes
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property size As Long
            Get
                Return filesize
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

        Public Overridable Sub clearMaxOffset()
            Me.m_maxOffset = 0
        End Sub

        Public MustOverride Function readByte() As Byte

        Public MustOverride Sub close()

        Public Sub SetByteOrder(order As ByteOrder)
            If order = ByteOrder.BigEndian Then
                m_littleEndian = False
            Else
                m_littleEndian = True
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{offset}/{filesize}] {ByteOrder.ToString}"
        End Function

        Public Overridable Function readBytes(n As Integer) As Byte()
            If n < 0 Then
                Throw New ArgumentException("n should be greater than 0")
            End If

            Dim buf As Byte() = New Byte(n - 1) {}
            For i As Integer = 0 To n - 1
                buf(i) = readByte()
            Next
            Return buf
        End Function

        Public Overridable Sub skipBytes(n As Integer)
            If n < 0 Then
                Throw New ArgumentException("n should be greater than 0")
            End If

            For i As Integer = 0 To n - 1
                readByte()
            Next
        End Sub

        Public Overridable Function readInt() As Integer
            Dim data As Byte() = readBytes(4)
            Dim temp As Integer = 0

            If Me.m_littleEndian Then
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

        Public Overridable Function readLong() As Long
            Dim data As Byte() = readBytes(8)
            Dim temp As Long = 0

            If Me.m_littleEndian Then
                temp = (data(0) And &HFF)
                temp = temp Or (data(1) And &HFF) << 8
                temp = temp Or (data(2) And &HFF) << 16
                temp = temp Or (data(3) And &HFF) << 24
                temp = temp Or (data(4) And &HFF) << 32
                temp = temp Or (data(5) And &HFF) << 40
                temp = temp Or (data(6) And &HFF) << 48
                temp = temp Or (data(7) And &HFF) << 56
            Else
                temp = (data(0) And &HFF) << 56
                temp = temp Or (data(1) And &HFF) << 48
                temp = temp Or (data(2) And &HFF) << 40
                temp = temp Or (data(3) And &HFF) << 32
                temp = temp Or (data(4) And &HFF) << 24
                temp = temp Or (data(5) And &HFF) << 16
                temp = temp Or (data(6) And &HFF) << 8
                temp = temp Or (data(7) And &HFF)
            End If
            Return temp
        End Function

        ''' <summary>
        ''' read 2 byte integer
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function readShort() As Short
            Dim data As Byte() = readBytes(2)
            Dim temp As Short = 0

            If Me.m_littleEndian Then
                temp = CShort(data(0) And &HFF)
                temp = temp Or CShort((data(1) And &HFF) << 8)
            Else
                temp = CShort((data(0) And &HFF) << 8)
                temp = temp Or CShort(data(1) And &HFF)
            End If
            Return temp
        End Function

        Public Overridable Function readASCIIString() As String
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

        Public Overridable Function readASCIIString(length As Integer) As String
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
        Protected Overridable Sub Dispose(disposing As Boolean)
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
