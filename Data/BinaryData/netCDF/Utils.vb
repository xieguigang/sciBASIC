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
            Case CDFDataTypes.NC_BYTE : Return buffer.ReadBytes(size)
            Case CDFDataTypes.NC_CHAR : Return buffer.ReadChars(size)
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
            Case CDFDataTypes.NC_BYTE : Return Function(buffer) buffer(Scan0)
            Case CDFDataTypes.NC_CHAR : Return Function(buffer) Encoding.UTF8.GetString(buffer)

            Case CDFDataTypes.BOOLEAN
                ' 20210212 bytes flags for maps boolean
                Return Function(buffer) buffer(Scan0) <> 0

            Case CDFDataTypes.NC_DOUBLE : Return CastNumber(Of Double)(reversed, AddressOf BitConverter.ToDouble)
            Case CDFDataTypes.NC_FLOAT : Return CastNumber(Of Single)(reversed, AddressOf BitConverter.ToSingle)
            Case CDFDataTypes.NC_INT : Return CastNumber(Of Integer)(reversed, AddressOf BitConverter.ToInt32)
            Case CDFDataTypes.NC_INT64 : Return CastNumber(Of Long)(reversed, AddressOf BitConverter.ToInt64)
            Case CDFDataTypes.NC_SHORT : Return CastNumber(Of Short)(reversed, AddressOf BitConverter.ToInt16)

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
            Case CDFDataTypes.NC_BYTE
                Return buffer.ReadBytes(size)
            Case CDFDataTypes.NC_CHAR
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
End Module
