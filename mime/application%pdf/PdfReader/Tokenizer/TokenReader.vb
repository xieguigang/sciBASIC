#Region "Microsoft.VisualBasic::682a555806749427e2de832145c600aa, mime\application%pdf\PdfReader\Tokenizer\TokenReader.vb"

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

    '   Total Lines: 122
    '    Code Lines: 91
    ' Comment Lines: 11
    '   Blank Lines: 20
    '     File Size: 4.39 KB


    '     Class TokenReader
    ' 
    '         Properties: Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBytes, ReadBytes, ReadLine
    ' 
    '         Sub: AppendBytes, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.IO
Imports stdNum = System.Math

Namespace PdfReader
    Public Class TokenReader
        Private _stream As Stream
        Private _bytes As Byte()
        Private _start As Integer
        Private _end As Integer
        Private _position As Long

        Public Sub New(stream As Stream)
            _stream = stream
            _position = stream.Position
            _bytes = New Byte(1023) {}
        End Sub

        Public ReadOnly Property Position As Long
            Get
                Return _position
            End Get
        End Property

        Public Function GetBytes(length As Integer) As Byte()
            ' Make sure we have some data to process
            If _start = _end AndAlso ReadBytes(False) = 0 Then Return Nothing
            Dim ret = New Byte(length - 1) {}
            Dim index = 0

            ' Copy any remaining bytes in the buffer
            If _start < _end Then
                Dim copy = stdNum.Min(length, _end - _start)
                Array.Copy(_bytes, _start, ret, 0, copy)
                index += copy
                _start += copy
                _position += copy
            End If

            ' Read remaining bytes directly from the stream
            If index < length Then
                Dim copied = _stream.Read(ret, index, length - index)
                _position += copied
                If copied < length - index Then Return Nothing
            End If

            Return ret
        End Function

        Public Function ReadLine() As TokenByteSplice
            ' If there is no more content to return, then return null
            If _start = _end AndAlso ReadBytes(False) = 0 Then Return New TokenByteSplice()
            Dim ret As TokenByteSplice = New TokenByteSplice()

            Do
                Dim index = _start

                Do
                    Dim c = _bytes(index)

                    ' Reached an end of line marker?
                    If c = 13 OrElse c = 10 Then
                        ' Append the unprocessed characters before the end of line marker
                        AppendBytes(_bytes, _start, index - _start, ret)

                        ' Processing continues after the first end of line marker
                        _start = index + 1
                        _position += 1

                        ' Check if newline has a linefeed afterwards
                        If c = 13 AndAlso (_start < _end OrElse ReadBytes(True) > 0) Then
                            If _bytes(_start) = 10 Then
                                ' Skip over the linefeed
                                _start += 1
                                _position += 1
                            End If
                        End If

                        Return ret
                    End If

                    index += 1
                    _position += 1
                Loop While index < _end

                ' Append the unprocessed characters
                AppendBytes(_bytes, _start, index - _start, ret)
            Loop While ReadBytes(True) > 0

            Return ret
        End Function

        Public Sub Reset()
            _start = 0
            _end = 0
            _position = _stream.Position
        End Sub

        Private Function ReadBytes(newBuffer As Boolean) As Integer
            ' Read in a buffer of ASCII characters
            _start = 0
            If newBuffer Then _bytes = New Byte(1023) {}
            _end = _stream.Read(_bytes, 0, _bytes.Length)
            Return _end
        End Function

        Private Sub AppendBytes(bytes As Byte(), start As Integer, length As Integer, ByRef existing As TokenByteSplice)
            If existing.Bytes Is Nothing Then
                existing.Bytes = bytes
                existing.Start = start
                existing.Length = length
            Else
                Dim ret = New Byte(existing.Length + length - 1) {}
                Array.Copy(existing.Bytes, existing.Start, ret, 0, existing.Length)
                Array.Copy(bytes, start, ret, existing.Length, length)
                existing.Bytes = ret
                existing.Start = 0
                existing.Length = ret.Length
            End If
        End Sub
    End Class
End Namespace
