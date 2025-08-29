#Region "Microsoft.VisualBasic::ab07708db26cf614833ef8024ab85c3d, Data\BinaryData\DataStorage\XPT\IO.vb"

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

    '   Total Lines: 110
    '    Code Lines: 81 (73.64%)
    ' Comment Lines: 13 (11.82%)
    '    - Xml Docs: 84.62%
    ' 
    '   Blank Lines: 16 (14.55%)
    '     File Size: 4.24 KB


    '     Class IO
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: concat, getString, readByte, readBytes, readDouble
    '                   readInt, readNumber, readShort, readString, toBytes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace Xpt

    ''' <summary>
    ''' Contains static convenience methods for low level operations (typically close
    ''' to IO).
    ''' 
    ''' @author Kasper Sørensen
    ''' </summary>
    Public Class IO

        Private Const CHARSET_NAME As String = "windows-1252"

        Private Sub New()
            ' prevent instantiation
        End Sub

        ''' <summary>
        ''' Converts an int-array to a byte-array. Makes it more convenient to use int
        ''' literals in code.
        ''' </summary>
        ''' <param name="arr">
        ''' @return </param>
        Public Shared Function toBytes(ParamArray arr As Integer()) As Byte()
            If arr Is Nothing Then
                Return Nothing
            End If
            Dim result = New Byte(arr.Length - 1) {}
            For i = 0 To result.Length - 1
                result(i) = CByte(arr(i))
            Next
            Return result
        End Function

        Public Shared Function readString(buffer As Byte(), off As Integer, len As Integer) As String
            Dim subset = readBytes(buffer, off, len)

            Dim str As String = getString(subset, CHARSET_NAME).Trim()
            Return str
        End Function

        Private Shared Function getString(bytes As Byte(), encoding As String) As String
            Dim s As Stream = New MemoryStream(bytes.[Select](Function(b) b).ToArray())
            Dim encoder As Encoding = encoding.ParseEncodingsName().CodePage()
            Dim reader As StreamReader = New StreamReader(s, encoder)
            Dim chars = New Char(bytes.Length * 2 - 1) {}
            Dim read = reader.Read(chars, 0, chars.Length)
            chars = chars.CopyOf(read)
            Return New String(chars)
        End Function

        Public Shared Function readByte(buffer As Byte(), off As Integer) As Byte
            Dim bb = ByteBuffer.wrap(buffer)
            bb.order(ByteOrder.LittleEndian)
            Return bb.get(off)
        End Function

        Public Shared Function readInt(buffer As Byte(), off As Integer) As Integer
            Dim bb = ByteBuffer.wrap(buffer)
            bb.order(ByteOrder.LittleEndian)
            Return bb.getInt(off)
        End Function

        Public Shared Function readDouble(buffer As Byte(), off As Integer) As Double
            Dim bb = ByteBuffer.wrap(buffer)
            bb.order(ByteOrder.LittleEndian)
            Return bb.getDouble(off)
        End Function

        Public Shared Function readBytes(data As Byte(), off As Integer, len As Integer) As Byte()
            If data.Length < off + len Then
                Throw New Exception("readBytes failed! data.length: " & data.Length.ToString() & ", off: " & off.ToString() & ", len: " & len.ToString())
            End If
            Dim subset = New Byte(len - 1) {}
            Array.Copy(data, off, subset, 0, len)
            Return subset
        End Function

        Public Shared Function readShort(buffer As Byte(), off As Integer) As Short
            Dim bb = ByteBuffer.wrap(buffer)
            bb.order(ByteOrder.LittleEndian)
            Return bb.getShort(off)
        End Function

        Public Shared Function readNumber(buffer As Byte(), off As Integer, len As Integer) As IComparable
            If len = 1 Then
                Return readByte(buffer, off)
            ElseIf len = 2 Then
                Return readShort(buffer, off)
            ElseIf len = 4 Then
                Return readInt(buffer, off)
            ElseIf len = 8 Then
                Return readDouble(buffer, off)
            Else
                Throw New NotSupportedException("Number byte-length not supported: " & len.ToString())
            End If
        End Function

        Public Shared Function concat(arr1 As Byte(), arr2 As Byte()) As Byte()
            Dim result = New Byte(arr1.Length + arr2.Length - 1) {}
            Array.Copy(arr1, 0, result, 0, arr1.Length)
            Array.Copy(arr2, 0, result, arr1.Length, arr2.Length)
            Return result
        End Function
    End Class

End Namespace
