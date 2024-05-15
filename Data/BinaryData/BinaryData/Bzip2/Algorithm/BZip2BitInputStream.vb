#Region "Microsoft.VisualBasic::b669aeb9c1c27a7f86850e88c9f2be13, Data\BinaryData\BinaryData\Bzip2\Algorithm\BZip2BitInputStream.vb"

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

    '   Total Lines: 104
    '    Code Lines: 59
    ' Comment Lines: 27
    '   Blank Lines: 18
    '     File Size: 4.00 KB


    '     Class BZip2BitInputStream
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ReadBits, ReadBoolean, ReadInteger, ReadUnary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports System.IO

Namespace Bzip2
    ''' <summary>
    ''' Implements a bit-wise input stream
    ''' </summary>
    Friend Class BZip2BitInputStream
#Region "Private fields"
        ' The stream from which bits are read
        Private ReadOnly inputStream As Stream

        ' A buffer of bits read from the input stream that have not yet been returned
        Private bitBuffer As UInteger

        ' The number of bits currently buffered in bitBuffer
        Private bitCount As Integer
#End Region

#Region "Public methods"
        ''' <summary>Public constructor</summary>
        ''' <param name="inputStream">The input stream to wrap</param>
        Public Sub New(inputStream As Stream)
            Me.inputStream = inputStream
        End Sub

        ''' <summary>Reads a single bit from the wrapped input stream</summary>
        ''' <return>true if the bit read was 1, otherwise false</return>
        ''' <exception cref="Exception">if no more bits are available in the input stream</exception>
        Public Function ReadBoolean() As Boolean
            If bitCount > 0 Then
                bitCount -= 1
            Else
                Dim byteRead As Integer = inputStream.ReadByte()
                If byteRead < 0 Then Throw New Exception("Insufficient data")
                bitBuffer = bitBuffer << 8 Or CUInt(byteRead)
                bitCount += 7
            End If

            Return (bitBuffer And 1 << bitCount) <> 0
        End Function

        ''' <summary>Reads a zero-terminated unary number from the wrapped input stream</summary>
        ''' <return>The unary number</return>
        ''' <exception cref="Exception">if no more bits are available in the input stream</exception>
        Public Function ReadUnary() As UInteger
            Dim unaryCount As UInteger = 0

            While True

                If bitCount > 0 Then
                    bitCount -= 1
                Else
                    Dim byteRead = inputStream.ReadByte()

                    If byteRead < 0 Then
                        Throw New Exception("Insufficient data")
                    End If

                    bitBuffer = bitBuffer << 8 Or CUInt(byteRead)
                    bitCount += 7
                End If

                If (bitBuffer And 1 << bitCount) = 0 Then Return unaryCount
                unaryCount += 1
            End While

            Throw New Exception("This exception will be never happends!")
        End Function

        ''' <summary>Reads up to 32 bits from the wrapped input stream</summary>
        ''' <param name="count">The number of bits to read (maximum 32)</param>
        ''' <return>The bits requested, right-aligned within the integer</return>
        ''' <exception cref="Exception">if no more bits are available in the input stream</exception>
        Public Function ReadBits(count As Integer) As UInteger
            If bitCount < count Then
                While bitCount < count
                    Dim byteRead As Integer = inputStream.ReadByte()
                    If byteRead < 0 Then Throw New Exception("Insufficient data")
                    bitBuffer = bitBuffer << 8 Or CUInt(byteRead)
                    bitCount += 8
                End While
            End If

            bitCount -= count
            Return bitBuffer >> bitCount And (1 << count) - 1
        End Function

        ' *
        '  Reads 32 bits of input as an integer
        '  @return The integer read
        '  @ if 32 bits are not available in the input stream
        ' 

        Public Function ReadInteger() As UInteger
            Return ReadBits(16) << 16 Or ReadBits(16)
        End Function
#End Region
    End Class
End Namespace
