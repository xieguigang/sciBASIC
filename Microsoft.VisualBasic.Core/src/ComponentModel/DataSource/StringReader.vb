#Region "Microsoft.VisualBasic::2ea7ff38eee8ab3936381e2ebbcd3d6c, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\StringReader.vb"

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

    '   Total Lines: 378
    '    Code Lines: 215 (56.88%)
    ' Comment Lines: 105 (27.78%)
    '    - Xml Docs: 88.57%
    ' 
    '   Blank Lines: 58 (15.34%)
    '     File Size: 12.82 KB


    '     Interface IStringGetter
    ' 
    '         Function: GetOrdinal, GetSize, (+2 Overloads) GetString, HasKey, MoveNext
    ' 
    '     Class DictionaryWrapper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetOrdinal, GetSize, (+2 Overloads) GetString, HasKey, MoveNext
    ' 
    '     Class StringArrayPointer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) ReadDouble, (+2 Overloads) ReadInteger, (+2 Overloads) ReadString, ToString
    ' 
    '     Class StringReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBoolean, GetByte, GetBytes, GetChar, GetChars
    '                   GetDateTime, GetDecimal, GetDouble, GetFloat, GetGuid
    '                   GetInt16, GetInt32, GetInt64, GetString, GetUInt64
    '                   IsNull, WrapDictionary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' a simple helper object for get string value
    ''' </summary>
    Public Interface IStringGetter

        ''' <summary>
        ''' check the given key name is existed inside current string collection data source
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Function HasKey(name As String) As Boolean
        ''' <summary>
        ''' get a string by a given key name.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Function GetString(name As String) As String
        ''' <summary>
        ''' get a string by a given collection index(offset).
        ''' </summary>
        ''' <param name="ordinal"></param>
        ''' <returns></returns>
        Function GetString(ordinal As Integer) As String
        ''' <summary>
        ''' get the string collection size
        ''' </summary>
        ''' <returns></returns>
        Function GetSize() As Integer

        ''' <summary>
        ''' Return the index Of the named field. 
        ''' </summary>
        ''' <returns>If the name is not exists in the parameter list, 
        ''' then a -1 value will be return.</returns>
        Function GetOrdinal(name As String) As Integer

        ''' <summary>
        ''' optionally implements this function for move the reader
        ''' cursor to next row if this data source consists with
        ''' mutliple rows.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' non-table liked data source should always returns false
        ''' </remarks>
        Function MoveNext() As Boolean

    End Interface

    Friend Class DictionaryWrapper : Implements IStringGetter, IKeyDataReader

        ReadOnly dict As Dictionary(Of String, String)
        ReadOnly keys As String()
        ReadOnly unsafe As Boolean

        Sub New(list As Dictionary(Of String, String), Optional unsafe As Boolean = True)
            dict = list
            keys = dict.Keys.ToArray

            Me.unsafe = unsafe
        End Sub

        Public Function HasKey(name As String) As Boolean Implements IStringGetter.HasKey
            Return dict.ContainsKey(name)
        End Function

        Public Function GetString(name As String) As String Implements IStringGetter.GetString, IKeyDataReader.GetData
            If unsafe Then
                Return dict(name)
            ElseIf Not dict.ContainsKey(name) Then
                Return ""
            Else
                Return dict(name)
            End If
        End Function

        Public Function GetString(ordinal As Integer) As String Implements IStringGetter.GetString
            Return dict(keys(ordinal))
        End Function

        Public Function GetSize() As Integer Implements IStringGetter.GetSize
            Return dict.Count
        End Function

        Public Function MoveNext() As Boolean Implements IStringGetter.MoveNext
            Return False
        End Function

        Public Function GetOrdinal(name As String) As Integer Implements IStringGetter.GetOrdinal
            Return keys.IndexOf(name)
        End Function
    End Class

    Public Class StringArrayPointer

        Dim i As Integer
        Dim vec As IReadOnlyCollection(Of String)
        Dim len As Integer

        Sub New(array As IReadOnlyCollection(Of String))
            vec = array
            len = array.Count
        End Sub

        Public Function ReadString() As String
            Dim str As String

            If i < vec.Count Then
                str = vec(i)
            Else
                str = Nothing
            End If

            i += 1

            Return str
        End Function

        Public Function ReadDouble() As Double
            Return Val(ReadString)
        End Function

        Public Function ReadInteger() As Integer
            Return CInt(Val(ReadString))
        End Function

        Public Function ReadInteger(offset As Integer) As Integer
            If offset < 0 OrElse offset >= len Then
                Return 0
            Else
                Return CInt(Val(vec(offset)))
            End If
        End Function

        Public Function ReadDouble(offset As Integer) As Double
            If offset < 0 OrElse offset >= len Then
                Return 0
            Else
                Return Val(vec(offset))
            End If
        End Function

        Public Function ReadString(offset As Integer, Optional strip As Boolean = False) As String
            If offset < 0 OrElse offset >= len Then
                Return Nothing
            Else
                Return If(strip, vec(offset).Trim(""""c), vec(offset))
            End If
        End Function

        Public Overrides Function ToString() As String
            Return vec.ToArray.GetJson
        End Function

    End Class

    Public Class StringReader : Implements IKeyDataReader

        ReadOnly getter As IStringGetter

        Sub New(stringGetter As IStringGetter)
            getter = stringGetter
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetBoolean(parameter As String) As Boolean
            Return getter.GetString(parameter).ParseBoolean
        End Function

        ''' <summary>
        ''' Gets the 8-bit unsigned Integer value Of the specified column.
        ''' </summary>
        ''' <param name="parameter"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetByte(parameter As String) As Byte
            Dim b As Byte

            If Byte.TryParse(getter.GetString(parameter), b) Then
                Return b
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Reads a stream Of bytes from the specified column offset into the buffer As an array, starting at the given buffer offset.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBytes(parameter As String) As Byte()
            Dim tokens As String() = getter.GetString(parameter).Split(","c)
            Return (From s As String In tokens Select CByte(Val(s))).ToArray
        End Function

        ''' <summary>
        ''' Gets the character value Of the specified column.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetChar(parameter As String) As Char
            Dim s As String = getter.GetString(parameter)

            If String.IsNullOrEmpty(s) Then
                Return ASCII.NUL
            Else
                Return s.First
            End If
        End Function

        ''' <summary>
        ''' Reads a stream Of characters from the specified column offset into the buffer As an array, starting at the given buffer offset.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetChars(parameter As String) As Char()
            Return getter.GetString(parameter).ToArray
        End Function

        ''' <summary>
        ''' Gets the Date And time data value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDateTime(parameter As String) As DateTime
            Return getter.GetString(parameter).ParseDateTime
        End Function

        ''' <summary>
        ''' Gets the fixed-position numeric value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDecimal(parameter As String) As Decimal
            Dim f128 As Decimal

            If Decimal.TryParse(getter.GetString(parameter), f128) Then
                Return f128
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Gets the Double-precision floating point number Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDouble(parameter As String) As Double
            Return Val(getter.GetString(parameter))
        End Function

        ''' <summary>
        ''' Gets the Single-precision floating point number Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFloat(parameter As String) As Single
            Dim f32 As Single

            If Single.TryParse(getter.GetString(parameter), f32) Then
                Return f32
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns the GUID value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetGuid(parameter As String) As Guid
            Return Guid.Parse(getter.GetString(parameter))
        End Function

        ''' <summary>
        ''' Gets the 16-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInt16(parameter As String) As Int16
            Dim i16 As Short

            If Int16.TryParse(getter.GetString(parameter), i16) Then
                Return i16
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Gets the 32-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInt32(parameter As String) As Int32
            Dim str As String = getter.GetString(parameter)
            Dim i32 As Integer

            If Integer.TryParse(str, i32) Then
                Return i32
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Gets the 64-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInt64(parameter As String) As Int64
            Dim i64 As Long

            If Long.TryParse(getter.GetString(parameter), i64) Then
                Return i64
            Else
                Return 0
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUInt64(name As String) As ULong
            Dim i64 As ULong

            If ULong.TryParse(getter.GetString(name), i64) Then
                Return i64
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Gets the String value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetString(parameter As String) As String Implements IKeyDataReader.GetData
            Return getter.GetString(parameter)
        End Function

        ''' <summary>
        ''' Return whether the specified field Is Set To null.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsNull(parameter As String) As Boolean
            If getter.HasKey(parameter) Then
                Return getter.GetString(parameter) Is Nothing
            Else
                Return True
            End If
        End Function

        Public Shared Function WrapDictionary(dict As Dictionary(Of String, String), Optional unsafe As Boolean = True) As StringReader
            Return New StringReader(New DictionaryWrapper(dict, unsafe))
        End Function
    End Class
End Namespace
