#Region "Microsoft.VisualBasic::f235a938c0ab940c93a57f0bf2b7467a, Microsoft.VisualBasic.Core\src\Text\IO\UnbufferedStringReader.vb"

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

    '   Total Lines: 288
    '    Code Lines: 190 (65.97%)
    ' Comment Lines: 71 (24.65%)
    '    - Xml Docs: 84.51%
    ' 
    '   Blank Lines: 27 (9.38%)
    '     File Size: 11.38 KB


    '     Class UnbufferedStreamReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadLine, ReadLineInternal
    ' 
    '         Sub: Dispose
    ' 
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
Imports System.Text

Namespace Text

    ''' <summary>
    ''' 提供无缓冲的按行读取文本流的功能。
    ''' 用于避免 StreamReader 内部缓冲区导致流位置超出实际内容末尾的问题。
    ''' </summary>
    Public Class UnbufferedStreamReader
        Inherits TextReader

        ReadOnly _encoding As Encoding
        ReadOnly _leaveOpen As Boolean
        ReadOnly _decoder As Decoder

        Dim _stream As Stream
        Dim _disposed As Boolean = False

        ''' <summary>
        ''' 初始化 UnbufferedStreamReader 类的新实例。
        ''' </summary>
        ''' <param name="stream">要读取的流。</param>
        ''' <param name="encoding">要使用的字符编码。默认为 UTF-8。</param>
        ''' <param name="leaveOpen">在释放 Reader 后是否保持流打开。默认为 False。</param>
        ''' <exception cref="ArgumentNullException">stream 为 Nothing。</exception>
        ''' <exception cref="ArgumentException">流不支持读取。</exception>
        Public Sub New(stream As Stream, Optional encoding As Encoding = Nothing, Optional leaveOpen As Boolean = False)
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If
            If Not stream.CanRead Then
                Throw New ArgumentException("Stream does not support reading.", NameOf(stream))
            End If

            _stream = stream
            _encoding = If(encoding, Encoding.UTF8)
            _leaveOpen = leaveOpen
            _decoder = _encoding.GetDecoder()
        End Sub

        ''' <summary>
        ''' 从当前流中读取一行字符。
        ''' </summary>
        ''' <returns>当前行内容；如果已到达流末尾，则为 Nothing。</returns>
        ''' <exception cref="ObjectDisposedException">方法在流被释放后调用。</exception>
        Public Overrides Function ReadLine() As String
            If _disposed Then
                Throw New ObjectDisposedException(Me.GetType().Name)
            ElseIf _stream.Length = _stream.Position Then
                Return Nothing
            Else
                Return ReadLineInternal()
            End If
        End Function

        Private Function ReadLineInternal() As String
            Dim bytes As New List(Of Byte)()
            Dim charBuffer(0) As Char ' 用于解码的字符缓冲区
            Dim currentByte As Integer

            ' 逐字节读取，直到遇到换行符或流结束
            Do While True
                currentByte = _stream.ReadByte()
                If currentByte = -1 Then
                    ' 到达流末尾
                    Exit Do
                End If

                ' 将字节添加到列表
                bytes.Add(CByte(currentByte))

                ' 尝试将累积的字节解码为字符串以检查换行符
                Dim tempBytes() As Byte = bytes.ToArray()
                Dim charCount As Integer = _decoder.GetCharCount(tempBytes, 0, tempBytes.Length, False)
                If charCount > 0 Then
                    Dim chars(charCount - 1) As Char
                    _decoder.GetChars(tempBytes, 0, tempBytes.Length, chars, 0, False)

                    ' 检查最后一个字符是否是换行符 (LF, '\n')
                    Dim lastChar As Char = chars(chars.Length - 1)
                    If lastChar = ControlChars.Lf Then
                        ' 找到换行符，移除可能的回车符 (CR, '\r') 并返回行
                        If chars.Length >= 2 AndAlso chars(chars.Length - 2) = ControlChars.Cr Then
                            ' 行以 CRLF 结束，移除 CR 和 LF
                            Return New String(chars, 0, chars.Length - 2)
                        Else
                            ' 行以 LF 结束，移除 LF
                            Return New String(chars, 0, chars.Length - 1)
                        End If
                    End If
                    ' 注意：如果字节不足以形成完整的字符，GetChars 可能不会解码任何字符。
                End If
            Loop

            ' 处理流末尾的数据：如果还有字节，则返回最后一行（没有换行符）
            If bytes.Count > 0 Then
                Dim finalBytes() As Byte = bytes.ToArray()
                Dim charCount As Integer = _decoder.GetCharCount(finalBytes, 0, finalBytes.Length, True)
                If charCount > 0 Then
                    Dim chars(charCount - 1) As Char
                    _decoder.GetChars(finalBytes, 0, finalBytes.Length, chars, 0, True)
                    Return New String(chars)
                End If
            End If

            ' 没有更多内容
            Return Nothing
        End Function

        ''' <summary>
        ''' 释放由 UnbufferedStreamReader 使用的所有资源。
        ''' </summary>
        ''' <param name="disposing">为 True 则释放托管资源和非托管资源；为 False 则仅释放非托管资源。</param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing AndAlso Not _leaveOpen Then
                    _stream?.Dispose()
                End If
                _stream = Nothing
                _disposed = True
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

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
