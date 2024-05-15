#Region "Microsoft.VisualBasic::9e488b2d4dd59f6bf752ba7e6d4d30e5, Data\BinaryData\HDF5\device\ReadHelper.vb"

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

    '   Total Lines: 211
    '    Code Lines: 137
    ' Comment Lines: 33
    '   Blank Lines: 41
    '     File Size: 7.34 KB


    '     Module ReadHelper
    ' 
    '         Function: bytesToUnsignedInt, getNumBytesFromMax, padding, readL, readO
    '                   readString8, readVariableSizeFactor, readVariableSizeMax, readVariableSizeN, readVariableSizeUnsigned
    '                   unsignedByteToShort, unsignedIntToLong, unsignedShortToInt
    ' 
    '         Sub: seekBufferToNextMultipleOfEight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Some part of this code is copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports stdNum = System.Math

Namespace device

    <HideModuleName> Public Module ReadHelper

        ''' <summary>
        ''' <see cref="Superblock.sizeOfOffsets"/>
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <param name="sb"></param>
        ''' <returns></returns>
        Public Function readO([in] As BinaryReader, sb As Superblock) As Long
            If [in] Is Nothing Then
                Throw New ArgumentException("in is null")
            End If

            If sb Is Nothing Then
                Throw New ArgumentException("sb is null")
            End If

            Dim sizeOfOffsets As Integer = sb.sizeOfOffsets

            If sizeOfOffsets = 1 Then
                Return [in].readByte()
            ElseIf sizeOfOffsets = 2 Then
                Return [in].readShort()
            ElseIf sizeOfOffsets = 4 Then
                Return [in].readInt()
            ElseIf sizeOfOffsets = 8 Then
                Return [in].readLong()
            End If

            Throw New IOException("size of offsets is not specified")
        End Function

        ''' <summary>
        ''' <see cref="Superblock.sizeOfLengths"/>
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <param name="sb"></param>
        ''' <returns></returns>
        Public Function readL([in] As BinaryReader, sb As Superblock) As Long
            If [in] Is Nothing Then
                Throw New ArgumentException("in is null")
            End If

            If sb Is Nothing Then
                Throw New ArgumentException("sb is null")
            End If

            Dim sizeOfLengths As Integer = sb.sizeOfLengths

            If sizeOfLengths = 1 Then
                Return [in].readByte()
            ElseIf sizeOfLengths = 2 Then
                Return [in].readShort()
            ElseIf sizeOfLengths = 4 Then
                Return [in].readInt()
            ElseIf sizeOfLengths = 8 Then
                Return [in].readLong()
            End If

            Throw New IOException("size of lengths is not specified")
        End Function

        Public Function padding(dataLen As Integer, paddingSize As Integer) As Integer
            If dataLen < 0 Then
                Throw New ArgumentException("dataLen is negative")
            End If

            If paddingSize <= 0 Then
                Throw New ArgumentException("dataLen is 0 or negative")
            End If

            Dim remain As Integer = dataLen Mod paddingSize

            If remain <> 0 Then
                remain = paddingSize - remain
            End If

            Return remain
        End Function

        Public Function getNumBytesFromMax(maxNumber As Long) As Integer
            Dim size As Integer = 0

            While maxNumber <> 0
                size += 1
                ' right shift with zero extension
                maxNumber = CLng(CULng(maxNumber) >> 8)
            End While

            Return size
        End Function

        Public Function readVariableSizeUnsigned([in] As BinaryReader, size As Integer) As Long
            Dim vv As Long

            If size = 1 Then
                vv = unsignedByteToShort([in].readByte())
            ElseIf size = 2 Then
                Dim s As Short = [in].readShort()
                vv = unsignedShortToInt(s)
            ElseIf size = 4 Then
                vv = unsignedIntToLong([in].readInt())
            ElseIf size = 8 Then
                vv = [in].readLong()
            Else
                vv = readVariableSizeN([in], size)
            End If

            Return vv
        End Function

        Public Function readVariableSizeMax([in] As BinaryReader, maxNumber As Integer) As Long
            Dim size As Integer = getNumBytesFromMax(maxNumber)
            Return readVariableSizeUnsigned([in], size)
        End Function

        Private Function readVariableSizeN([in] As BinaryReader, nbytes As Integer) As Long
            Dim ch As Integer() = New Integer(nbytes - 1) {}
            For i As Integer = 0 To nbytes - 1
                ch(i) = [in].readByte()
            Next

            Dim result As Long = ch(nbytes - 1)

            For i As Integer = nbytes - 2 To 0 Step -1
                result = result << 8
                result += ch(i)
            Next

            Return result
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function unsignedIntToLong(i As Integer) As Long
            Return If((i < 0), CLng(i) + 4294967296L, CLng(i))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function unsignedShortToInt(s As Short) As Integer
            Return (s And &HFFFF)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function unsignedByteToShort(b As Byte) As Short
            Return CShort(b And &HFF)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function bytesToUnsignedInt(upper As Byte, lower As Byte) As Integer
            Return unsignedByteToShort(upper) * 256 + unsignedByteToShort(lower)
        End Function

        ''' <summary>
        ''' 读取以0结尾的字符串，然后padding对齐到8字节的整数倍的位置
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function readString8([in] As BinaryReader) As String
            Dim filePos As Long = [in].offset
            Dim str As String = [in].readASCIIString()
            Dim newFilePos As Long = [in].offset
            Dim readCount As Integer = CInt(newFilePos - filePos)
            ' skip to 8 byte boundary, note zero byte is skipped
            Dim padding As Integer = ReadHelper.padding(readCount, 8)

            Call [in].skipBytes(padding)

            Return str
        End Function

        Public Function readVariableSizeFactor([in] As BinaryReader, sizeFactor As Integer) As Long
            Dim size As Integer = CInt(stdNum.Truncate(stdNum.Pow(2, sizeFactor)))
            Return readVariableSizeUnsigned([in], size)
        End Function

        ''' <summary>
        ''' Moves the position of the <seealso cref="BinaryReader"/> to the next position aligned on
        ''' 8 bytes. If the buffer position is already a multiple of 8 the position will
        ''' not be changed.
        ''' </summary>
        ''' <param name="reader"> the buffer to be aligned </param>
        ''' 
        <Extension>
        Public Sub seekBufferToNextMultipleOfEight(reader As BinaryReader)
            Dim pos As Integer = reader.offset

            If pos Mod 8 = 0 Then
                ' Already on a 8 byte multiple
                Return
            Else
                reader.offset += (8 - (pos Mod 8))
            End If
        End Sub
    End Module

End Namespace
