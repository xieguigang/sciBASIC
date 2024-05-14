#Region "Microsoft.VisualBasic::df7cdc3f3c89533e9423285f4025401b, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\StringReader.vb"

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

    '   Total Lines: 261
    '    Code Lines: 120
    ' Comment Lines: 105
    '   Blank Lines: 36
    '     File Size: 9.54 KB


    '     Interface IStringGetter
    ' 
    '         Function: GetOrdinal, GetSize, (+2 Overloads) GetString, HasKey, MoveNext
    ' 
    '     Class DictionaryWrapper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetOrdinal, GetSize, (+2 Overloads) GetString, HasKey, MoveNext
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
Imports Microsoft.VisualBasic.Scripting
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

    Friend Class DictionaryWrapper : Implements IStringGetter

        ReadOnly dict As Dictionary(Of String, String)
        ReadOnly keys As String()

        Sub New(list As Dictionary(Of String, String))
            dict = list
            keys = dict.Keys.ToArray
        End Sub

        Public Function HasKey(name As String) As Boolean Implements IStringGetter.HasKey
            Return dict.ContainsKey(name)
        End Function

        Public Function GetString(name As String) As String Implements IStringGetter.GetString
            Return dict(name)
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

    Public Class StringReader

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
            Return Byte.Parse(getter.GetString(parameter))
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
            Return Decimal.Parse(getter.GetString(parameter))
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUInt64(name As String) As ULong
            Return ULong.Parse(getter.GetString(name))
        End Function

        ''' <summary>
        ''' Gets the Single-precision floating point number Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFloat(parameter As String) As Single
            Return Single.Parse(getter.GetString(parameter))
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
            Return Int16.Parse(getter.GetString(parameter))
        End Function

        ''' <summary>
        ''' Gets the 32-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInt32(parameter As String) As Int32
            Return Integer.Parse(getter.GetString(parameter))
        End Function

        ''' <summary>
        ''' Gets the 64-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInt64(parameter As String) As Int64
            Return Long.Parse(getter.GetString(parameter))
        End Function

        ''' <summary>
        ''' Gets the String value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetString(parameter As String) As String
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

        Public Shared Function WrapDictionary(dict As Dictionary(Of String, String)) As StringReader
            Return New StringReader(New DictionaryWrapper(dict))
        End Function
    End Class
End Namespace
