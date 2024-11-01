#Region "Microsoft.VisualBasic::540155b329abd3b8ecbedfa74c26e597, Microsoft.VisualBasic.Core\src\Text\IO\UnbufferedStringReader.vb"

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

    '   Total Lines: 166
    '    Code Lines: 114 (68.67%)
    ' Comment Lines: 39 (23.49%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (7.83%)
    '     File Size: 5.87 KB


    '     Class UnbufferedStringReader
    ' 
    '         Properties: Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Peek, (+2 Overloads) Read, ReadLine, ReadToEnd
    ' 
    '         Sub: Close, Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Text

    ''' <summary>
    ''' Represents a reader that can read a sequential series of characters.
    ''' </summary>
    <Serializable> Public Class UnbufferedStringReader
        Inherits TextReader

        Dim _length As Integer
        Dim _pos As Integer
        Dim _s As String

        Public Sub New(s As String)
            If s Is Nothing Then
                Throw New ArgumentNullException("s")
            End If
            Me._s = s
            Me._length = If((s Is Nothing), 0, s.Length)
        End Sub

        ''' <summary>
        ''' Closes the System.IO.TextReader and releases any system resources associated
        ''' with the TextReader.
        ''' </summary>
        Public Overrides Sub Close()
            Me.Dispose(True)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            Me._s = Nothing
            Me._pos = 0
            Me._length = 0
            MyBase.Dispose(disposing)
        End Sub

        ''' <summary>
        ''' Reads the next character without changing the state of the reader or the character
        ''' source. Returns the next available character without actually reading it from
        ''' the reader.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Peek() As Integer
            If Me._s Is Nothing Then
                Throw New Exception("object closed")
            End If
            If Me._pos = Me._length Then
                Return -1
            End If
            Return AscW(Me._s(Me._pos))
        End Function

        ''' <summary>
        ''' Reads the next character from the text reader and advances the character position
        ''' by one character.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Read() As Integer
            If Me._s Is Nothing Then
                Throw New Exception("object closed")
            End If
            If Me._pos = Me._length Then
                Return -1
            End If
            Dim c As Char = Me._s(Me._pos)
            Me._pos += 1
            Return AscW(c)
        End Function

        ''' <summary>
        ''' Reads a specified maximum number of characters from the current reader and writes
        ''' the data to a buffer, beginning at the specified index.
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="index"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Public Overrides Function Read(buffer As Char(), index As Integer, count As Integer) As Integer
            If buffer Is Nothing Then
                Throw New ArgumentNullException("buffer")
            End If
            If index < 0 Then
                Throw New ArgumentOutOfRangeException("index")
            End If
            If count < 0 Then
                Throw New ArgumentOutOfRangeException("count")
            End If
            If (buffer.Length - index) < count Then
                Throw New ArgumentException("invalid offset length")
            End If
            If Me._s Is Nothing Then
                Throw New Exception("object closed")
            End If
            Dim num As Integer = Me._length - Me._pos
            If num > 0 Then
                If num > count Then
                    num = count
                End If
                Me._s.CopyTo(Me._pos, buffer, index, num)
                Me._pos += num
            End If
            Return num
        End Function

        ''' <summary>
        ''' Reads a line of characters from the text reader and returns the data as a string.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ReadLine() As String
            If Me._s Is Nothing Then
                Throw New Exception("object closed")
            End If
            Dim num As Integer = Me._pos
            While num < Me._length
                Dim ch As Char = Me._s(num)
                Select Case ch
                    Case ControlChars.Cr, ControlChars.Lf

                        Dim text As String = Me._s.Substring(Me._pos, num - Me._pos)
                        Me._pos = num + 1
                        If ((ch = ControlChars.Cr) AndAlso (Me._pos < Me._length)) AndAlso (Me._s(Me._pos) = ControlChars.Lf) Then
                            Me._pos += 1
                        End If
                        Return text
                End Select
                num += 1
            End While
            If num > Me._pos Then
                Dim text2 As String = Me._s.Substring(Me._pos, num - Me._pos)
                Me._pos = num
                Return text2
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Reads all characters from the current position to the end of the text reader
        ''' and returns them as one string.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ReadToEnd() As String
            Dim text As String
            If Me._s Is Nothing Then
                Throw New Exception("object closed")
            End If
            If Me._pos = 0 Then
                text = Me._s
            Else
                text = Me._s.Substring(Me._pos, Me._length - Me._pos)
            End If
            Me._pos = Me._length
            Return text
        End Function

        ''' <summary>
        ''' The current read position.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Position() As Integer
            Get
                Return _pos
            End Get
        End Property
    End Class
End Namespace
