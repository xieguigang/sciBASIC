#Region "Microsoft.VisualBasic::300763d2eff4a84c10688abdf2bfca2a, Data\BinaryData\netCDF\Utils.vb"

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

    '   Total Lines: 213
    '    Code Lines: 129 (60.56%)
    ' Comment Lines: 58 (27.23%)
    '    - Xml Docs: 77.59%
    ' 
    '   Blank Lines: 26 (12.21%)
    '     File Size: 8.55 KB


    ' Module Utils
    ' 
    '     Function: CastNumber, GetRecordReader, notNetcdf, readName, readNumber
    '               readType, readVector
    ' 
    '     Sub: padding, writeName, writePadding
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.Language

Module Utils

    ''' <summary>
    ''' Throws a non-valid NetCDF exception if the statement it's true
    ''' </summary>
    ''' <param name="statement">statement - Throws if true</param>
    ''' <param name="reason$">reason - Reason to throw</param>
    Public Function notNetcdf(statement As Boolean, reason$) As Object
        If (statement) Then
            Throw New FormatException($"Not a valid NetCDF v3.x file: {reason}")
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Moves 1, 2, Or 3 bytes to next 4-byte boundary
    ''' </summary>
    ''' <param name="buffer">
    ''' buffer - Buffer for the file data
    ''' </param>
    <Extension> Public Sub padding(buffer As BinaryDataReader)
        If ((buffer.Position Mod 4) <> 0) Then
            Call buffer.Seek(4 - (buffer.Position Mod 4), SeekOrigin.Current)
        End If
    End Sub

    <Extension> Public Sub writePadding(output As BinaryDataWriter)
        Dim n As Value(Of Long) = 0

        If ((n = (output.Position Mod 4)) <> 0) Then
            For i As Integer = 1 To 4 - CLng(n)
                Call output.Write(CByte(0))
            Next
        End If
    End Sub

    ''' <summary>
    ''' Round <paramref name="n"/> up to the next multiple of 4.
    ''' </summary>
    <Extension> Public Function pad4(n As Long) As Long
        Return ((n + 3) \ 4) * 4
    End Function

    ''' <summary>
    ''' Read <paramref name="size"/> big-endian values of <paramref name="elementBytes"/> bytes
    ''' each and convert them with <paramref name="convert"/>.
    ''' </summary>
    Private Function ReadBE(Of T As Structure)(buffer As BinaryDataReader, size As Integer, elementBytes As Integer, convert As Func(Of Byte(), Integer, T)) As T()
        Dim block As Byte() = buffer.ReadBytes(size * elementBytes)
        Dim result(size - 1) As T
        Dim elem(elementBytes - 1) As Byte

        For i As Integer = 0 To size - 1
            Call Array.Copy(block, i * elementBytes, elem, 0, elementBytes)
            Call Array.Reverse(elem)
            result(i) = convert(elem, 0)
        Next

        Return result
    End Function

    ''' <summary>
    ''' write name string
    ''' </summary>
    ''' <param name="output"></param>
    ''' <param name="name"></param>
    <Extension>
    Public Sub writeName(output As BinaryDataWriter, name$)
        Call output.Write(name, BinaryStringFormat.UInt32LengthPrefix)
        Call output.writePadding
    End Sub

    ''' <summary>
    ''' Reads the name
    ''' </summary>
    ''' <param name="buffer">
    ''' buffer - Buffer for the file data
    ''' </param>
    ''' <returns>Name</returns>
    <Extension>
    Public Function readName(buffer As BinaryDataReader) As String
        ' Read name
        Dim nameLength = buffer.ReadUInt32()
        Dim name() = buffer.ReadChars(nameLength)

        ' validate name
        ' TODO

        ' Apply padding
        ' 数据的长度应该是4的整数倍,如果不是,则会使用0进行填充
        Call buffer.padding()

        Return New String(name)
    End Function

    ''' <summary>
    ''' Auxiliary function to read numeric data
    ''' </summary>
    ''' <param name="size%">size - Size of the element to read</param>
    ''' <param name="bufferReader">bufferReader - Function to read next value</param>
    ''' <returns>{Array&lt;number>|number}</returns>
    Public Function readNumber(Of T)(size%, bufferReader As Func(Of T)) As Object
        If (size <> 1) Then
            Dim numbers As T() = New T(size - 1) {}

            For i As Integer = 0 To size - 1
                numbers(i) = bufferReader()
            Next

            Return numbers
        Else
            Return bufferReader()
        End If
    End Function

    <Extension>
    Public Function readVector(buffer As BinaryDataReader, size As Integer, type As CDFDataTypes) As Array
        If buffer.EndOfStream Then
            Call $"Binary reader ""{buffer.ToString}"" offset out of boundary!".Warning
            ' 已经出现越界了
            Return Nothing
        End If

            Select Case type
                Case CDFDataTypes.NC_BYTE, CDFDataTypes.NC_UBYTE : Return buffer.ReadBytes(size)
                Case CDFDataTypes.NC_CHAR, CDFDataTypes.NC_STRING : Return buffer.ReadChars(size)
                Case CDFDataTypes.BOOLEAN
                    ' 20210212 bytes flags for maps boolean
                    Return buffer.ReadBytes(size) _
                        .Select(Function(b) b <> 0) _
                        .ToArray
                Case CDFDataTypes.NC_DOUBLE : Return buffer.ReadDoubles(size)
                Case CDFDataTypes.NC_FLOAT : Return buffer.ReadSingles(size)
                Case CDFDataTypes.NC_INT : Return buffer.ReadInt32s(size)
                Case CDFDataTypes.NC_INT64 : Return buffer.ReadInt64s(size)
                Case CDFDataTypes.NC_SHORT : Return buffer.ReadInt16s(size)
                Case CDFDataTypes.NC_USHORT
                    Return ReadBE(Of UShort)(buffer, size, 2, AddressOf BitConverter.ToUInt16) _
                        .Select(Function(u) CInt(u)) _
                        .ToArray
                Case CDFDataTypes.NC_UINT
                    Return ReadBE(Of UInteger)(buffer, size, 4, AddressOf BitConverter.ToUInt32) _
                        .Select(Function(u) CLng(u)) _
                        .ToArray
                Case CDFDataTypes.NC_UINT64
                    Return ReadBE(Of ULong)(buffer, size, 8, AddressOf BitConverter.ToUInt64) _
                        .Select(Function(u) CLng(u)) _
                        .ToArray
                Case Else
                    ' istanbul ignore next
                    Return Utils.notNetcdf(True, $"non valid type {type}")
            End Select
    End Function

    ''' <summary>
    ''' Parse record type
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetRecordReader(type As CDFDataTypes, Optional reversed As Boolean? = Nothing) As Func(Of Byte(), Object)
        If reversed Is Nothing Then
            reversed = ByteOrderHelper.NeedsReversion(ByteOrder.BigEndian)
        End If

        Select Case type
            Case CDFDataTypes.NC_BYTE, CDFDataTypes.NC_UBYTE : Return Function(buffer) buffer(Scan0)
            Case CDFDataTypes.NC_CHAR, CDFDataTypes.NC_STRING : Return Function(buffer) Encoding.UTF8.GetString(buffer)

            Case CDFDataTypes.BOOLEAN
                ' 20210212 bytes flags for maps boolean
                Return Function(buffer) buffer(Scan0) <> 0

            Case CDFDataTypes.NC_DOUBLE : Return CastNumber(Of Double)(reversed, AddressOf BitConverter.ToDouble)
            Case CDFDataTypes.NC_FLOAT : Return CastNumber(Of Single)(reversed, AddressOf BitConverter.ToSingle)
            Case CDFDataTypes.NC_INT : Return CastNumber(Of Integer)(reversed, AddressOf BitConverter.ToInt32)
            Case CDFDataTypes.NC_INT64 : Return CastNumber(Of Long)(reversed, AddressOf BitConverter.ToInt64)
            Case CDFDataTypes.NC_SHORT : Return CastNumber(Of Short)(reversed, AddressOf BitConverter.ToInt16)
            Case CDFDataTypes.NC_USHORT : Return CastNumber(Of UShort)(reversed, AddressOf BitConverter.ToUInt16)
            Case CDFDataTypes.NC_UINT : Return CastNumber(Of UInteger)(reversed, AddressOf BitConverter.ToUInt32)
            Case CDFDataTypes.NC_UINT64 : Return CastNumber(Of ULong)(reversed, AddressOf BitConverter.ToUInt64)

            Case Else
                ' istanbul ignore next
                Return Utils.notNetcdf(True, $"non valid type {type}")
        End Select
    End Function

    Private Function CastNumber(Of T)(reverse As Boolean, bitConvert As Func(Of Byte(), Integer, T)) As Func(Of Byte(), Object)
        If reverse Then
            Return Function(buffer)
                       Call Array.Reverse(buffer)
                       Return bitConvert(buffer, Scan0)
                   End Function
        Else
            Return Function(buffer) bitConvert(buffer, Scan0)
        End If
    End Function

    ''' <summary>
    ''' Given a type And a size reads the next element.
    ''' (这个函数会根据<paramref name="type"/>类以及<paramref name="size"/>的不同而返回不同的数据结果:
    ''' + 根据<paramref name="type"/>可能会返回字符串或者数字
    ''' + 如果<paramref name="size"/>等于1,则只会返回单个数字, 如果<paramref name="size"/>大于1, 则会返回一个数组
    ''' )
    ''' </summary>
    ''' <param name="buffer">buffer - Buffer for the file data</param>
    ''' <param name="type">type - Type of the data to read</param>
    ''' <param name="size">size - Size of the element to read</param>
    ''' <returns>``{string|Array&lt;number>|number}``</returns>
    Public Function readType(buffer As BinaryDataReader, type As CDFDataTypes, size As Integer) As Object
        If buffer.EndOfStream Then
            Call $"Binary reader ""{buffer.ToString}"" offset out of boundary!".Warning
            ' 已经出现越界了
            Return Nothing
        End If

        Select Case type
            Case CDFDataTypes.NC_BYTE, CDFDataTypes.NC_UBYTE
                Return buffer.ReadBytes(size)
            Case CDFDataTypes.NC_CHAR, CDFDataTypes.NC_STRING
                Return New String(buffer.ReadChars(size)).TrimNull
            Case CDFDataTypes.NC_SHORT
                Return readNumber(size, AddressOf buffer.ReadInt16)
            Case CDFDataTypes.NC_INT
                Return readNumber(size, AddressOf buffer.ReadInt32)
            Case CDFDataTypes.NC_FLOAT
                Return readNumber(size, AddressOf buffer.ReadSingle)
            Case CDFDataTypes.NC_DOUBLE
                Return readNumber(size, AddressOf buffer.ReadDouble)
            Case CDFDataTypes.NC_INT64
                Return readNumber(size, AddressOf buffer.ReadInt64)
            Case CDFDataTypes.NC_USHORT
                Return ReadBE(Of UShort)(buffer, size, 2, AddressOf BitConverter.ToUInt16)
            Case CDFDataTypes.NC_UINT
                Return ReadBE(Of UInteger)(buffer, size, 4, AddressOf BitConverter.ToUInt32)
            Case CDFDataTypes.NC_UINT64
                Return ReadBE(Of ULong)(buffer, size, 8, AddressOf BitConverter.ToUInt64)
            Case CDFDataTypes.BOOLEAN

                ' 20210212 bytes flags for maps boolean
                Return buffer.ReadBytes(size) _
                    .Select(Function(b) b <> 0) _
                    .ToArray

            Case Else
                ' istanbul ignore next
                Return Utils.notNetcdf(True, $"non valid type {type}")
        End Select
    End Function

    ''' <summary>
    ''' Return a new array of the same element type containing the slice
    ''' [<paramref name="start"/>, <paramref name="start"/> + <paramref name="count"/>)
    ''' of <paramref name="data"/>.
    ''' </summary>
    Public Function sliceVector(data As Array, type As CDFDataTypes, start As Integer, count As Integer) As Array
        Select Case type
            Case CDFDataTypes.NC_BYTE, CDFDataTypes.NC_UBYTE : Return sliceVector(Of Byte)(data, start, count)
            Case CDFDataTypes.NC_CHAR, CDFDataTypes.NC_STRING : Return sliceVector(Of Char)(data, start, count)
            Case CDFDataTypes.NC_SHORT, CDFDataTypes.NC_USHORT : Return sliceVector(Of Short)(data, start, count)
            Case CDFDataTypes.NC_INT, CDFDataTypes.NC_UINT : Return sliceVector(Of Integer)(data, start, count)
            Case CDFDataTypes.NC_INT64, CDFDataTypes.NC_UINT64 : Return sliceVector(Of Long)(data, start, count)
            Case CDFDataTypes.NC_FLOAT : Return sliceVector(Of Single)(data, start, count)
            Case CDFDataTypes.NC_DOUBLE : Return sliceVector(Of Double)(data, start, count)
            Case Else : Return notNetcdf(True, $"non valid type {type}")
        End Select
    End Function

    Private Function sliceVector(Of T)(data As Array, start As Integer, count As Integer) As T()
        Dim src As T() = DirectCast(data, T())
        Dim dst(count - 1) As T
        Call Array.Copy(src, start, dst, 0, count)
        Return dst
    End Function

    ''' <summary>
    ''' Convert a whole variable array of the given CDF type into big-endian
    ''' bytes ready to be written to a CDF file.
    ''' </summary>
    Public Function ToBigEndianBytes(data As Array, type As CDFDataTypes, encoding As Encoding) As Byte()
        Select Case type
            Case CDFDataTypes.NC_BYTE, CDFDataTypes.NC_UBYTE : Return DirectCast(data, Byte())
            Case CDFDataTypes.BOOLEAN : Return DirectCast(data, Boolean()).Select(Function(b) CByte(If(b, 1, 0))).ToArray
            Case CDFDataTypes.NC_CHAR, CDFDataTypes.NC_STRING : Return encoding.GetBytes(New String(DirectCast(data, Char())))
            Case CDFDataTypes.NC_SHORT : Return toBE(DirectCast(data, Short()), 2, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_USHORT : Return toBE(DirectCast(data, UShort()), 2, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_INT : Return toBE(DirectCast(data, Integer()), 4, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_UINT : Return toBE(DirectCast(data, UInteger()), 4, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_INT64 : Return toBE(DirectCast(data, Long()), 8, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_UINT64 : Return toBE(DirectCast(data, ULong()), 8, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_FLOAT : Return toBE(DirectCast(data, Single()), 4, AddressOf BitConverter.GetBytes)
            Case CDFDataTypes.NC_DOUBLE : Return toBE(DirectCast(data, Double()), 8, AddressOf BitConverter.GetBytes)
            Case Else : Return notNetcdf(True, $"non valid type {type}")
        End Select
    End Function

    Private Function toBE(Of T)(values As T(), elemSize As Integer, getBytes As Func(Of T, Byte())) As Byte()
        Dim out As Byte() = New Byte(values.Length * elemSize - 1) {}
        Dim tmp As Byte()

        For i As Integer = 0 To values.Length - 1
            tmp = getBytes(values(i))

            If BitConverter.IsLittleEndian Then
                Call Array.Reverse(tmp)
            End If

            Call Array.Copy(tmp, 0, out, i * elemSize, elemSize)
        Next

        Return out
    End Function
End Module
