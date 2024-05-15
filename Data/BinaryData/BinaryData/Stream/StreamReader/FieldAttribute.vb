#Region "Microsoft.VisualBasic::6e33c60335f9ea05fe1ba3916d64712b, Data\BinaryData\BinaryData\Stream\StreamReader\FieldAttribute.vb"

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

    '   Total Lines: 74
    '    Code Lines: 46
    ' Comment Lines: 17
    '   Blank Lines: 11
    '     File Size: 2.46 KB


    ' Class FieldAttribute
    ' 
    '     Properties: N, offset, ReadArray
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Read, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Serialization

Public Class FieldAttribute : Inherits Field

    ''' <summary>
    ''' the array length
    ''' </summary>
    ''' <returns></returns>
    Public Property N As Integer
    ''' <summary>
    ''' the binary data file offset
    ''' </summary>
    ''' <returns></returns>
    Public Property offset As Long = -1

    Public ReadOnly Property ReadArray As Boolean
        Get
            Return N > 0
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ordinal">The ordinal position in the rawdata layout</param>
    ''' <param name="n">for array used only, means array length to read, default negative means scalar</param>
    Sub New(ordinal As Integer, Optional n As Integer = -1)
        Call MyBase.New(ordinal)
        Me.N = n
    End Sub

    Public Function Read(buf As BinaryDataReader, p As PropertyInfo) As Object
        Dim type As Type = p.PropertyType
        Dim code As TypeCode = Type.GetTypeCode(type)

        If offset >= 0 Then
            Call buf.Seek(offset, SeekOrigin.Begin)
        End If

        If type.IsArray Then
            Dim scalar As Type = type.GetElementType
            Dim sizeof As Integer = Marshal.SizeOf(scalar)
            Dim len As Integer = sizeof * N
            Dim view As New MemoryStream(buf.ReadBytes(len))

            Return RawStream.GetData(view, code:=Type.GetTypeCode(scalar))
        ElseIf type Is GetType(String) AndAlso ReadArray Then
            ' read chars array with fix length
            Dim chars As Char() = buf.ReadChars(N)
            Dim si As New String(chars)

            ' fix length string may contains ZERO bytes
            ' removes it
            Return Strings.Trim(si).Trim(vbNullChar)
        Else
            ' read scalar
            Return ReaderProvider.ReadScalar(code)(buf)
        End If
    End Function

    Public Overrides Function ToString() As String
        Dim offset As String = If(Me.offset >= 0, $"offset:{StringFormats.Lanudry(Me.offset)}", "")

        If ReadArray Then
            Return $"#{Index} vec; {offset}".Trim
        Else
            Return $"#{Index} scalar; {offset}".Trim
        End If
    End Function
End Class
