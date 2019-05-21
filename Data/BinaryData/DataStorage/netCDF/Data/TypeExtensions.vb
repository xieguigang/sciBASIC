#Region "Microsoft.VisualBasic::82601396aeebadcc0545a04332c9e02b, mime\application%netcdf\Data\TypeExtensions.vb"

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

    ' Module TypeExtensions
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: num2str, readNumber, readType, sizeof, str2num
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO

Namespace netCDF

    Module TypeExtensions

        ReadOnly description As Dictionary(Of CDFDataTypes, String)
        ReadOnly enumParser As Dictionary(Of String, CDFDataTypes)

        Sub New()
            description = Enums(Of CDFDataTypes).ToDictionary(Function(type) type, Function(type) type.Description)
            enumParser = description.ReverseMaps
        End Sub

        ''' <summary>
        ''' Parse a number into their respective type
        ''' </summary>
        ''' <param name="type">type - integer that represents the type</param>
        ''' <returns>parsed value of the type</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function num2str(type As CDFDataTypes) As String
            ' ([default]:="undefined") istanbul ignore next 
            Return description.TryGetValue(type, [default]:="undefined")
        End Function

        ''' <summary>
        ''' Parse a number type identifier to his size in bytes
        ''' </summary>
        ''' <param name="type">type - integer that represents the type</param>
        ''' <returns>size of the type</returns>
        Public Function sizeof(type As CDFDataTypes) As Integer
            Select Case type
                Case CDFDataTypes.BYTE
                    Return 1
                Case CDFDataTypes.CHAR
                    Return 1
                Case CDFDataTypes.SHORT
                    Return 2
                Case CDFDataTypes.INT
                    Return 4
                Case CDFDataTypes.FLOAT
                    Return 4
                Case CDFDataTypes.DOUBLE
                    Return 8
                Case Else
                    ' istanbul ignore next 
                    Return -1
            End Select
        End Function

        ''' <summary>
        ''' Reverse search of num2str
        ''' </summary>
        ''' <param name="type">type - string that represents the type</param>
        ''' <returns>parsed value of the type</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function str2num(type As String) As CDFDataTypes
            Return enumParser.TryGetValue(LCase(type), [default]:=CDFDataTypes.undefined)
        End Function

        ''' <summary>
        ''' Auxiliary function to read numeric data
        ''' </summary>
        ''' <param name="size%">size - Size of the element to read</param>
        ''' <param name="bufferReader">bufferReader - Function to read next value</param>
        ''' <returns>{Array&lt;number>|number}</returns>
        Public Function readNumber(size%, bufferReader As Func(Of Object)) As Object
            If (size <> 1) Then
                Dim numbers As New List(Of Object)

                For i As Integer = 0 To size - 1
                    numbers.Add(bufferReader())
                Next

                Return numbers
            Else
                Return bufferReader()
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
                Case CDFDataTypes.BYTE
                    Return buffer.ReadBytes(size)
                Case CDFDataTypes.CHAR
                    Return New String(buffer.ReadChars(size)).TrimNull
                Case CDFDataTypes.SHORT
                    Return readNumber(size, AddressOf buffer.ReadInt16)
                Case CDFDataTypes.INT
                    Return readNumber(size, AddressOf buffer.ReadInt32)
                Case CDFDataTypes.FLOAT
                    Return readNumber(size, AddressOf buffer.ReadSingle)
                Case CDFDataTypes.DOUBLE
                    Return readNumber(size, AddressOf buffer.ReadDouble)

                Case Else
                    ' istanbul ignore next
                    Return Utils.notNetcdf(True, $"non valid type {type}")
            End Select
        End Function
    End Module
End Namespace