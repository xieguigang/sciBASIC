#Region "Microsoft.VisualBasic::126a3b8b1d0672c924a7a45d0da0d8dd, Microsoft.VisualBasic.Core\ComponentModel\File\BufferedStream.vb"

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

    '     Class BufferedStream
    ' 
    '         Properties: EndRead, FileName
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: BufferProvider, LinesIterator, ReadAllLines, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel

    ''' <summary>
    ''' Buffered large text dataset reader
    ''' </summary>
    Public Class BufferedStream
        Implements IDisposable

        ''' <summary>
        ''' The File location of this text file.
        ''' </summary>
        ''' <returns></returns>
        <XmlIgnore> <ScriptIgnore> Public Property FileName As String
            Get
                Return __fileName
            End Get
            Protected Set(value As String)
                __fileName = value
            End Set
        End Property

        Protected __fileName As String
        Protected __innerBuffer As String()
        Protected __innerStream As FileStream

        Const maxBufferSize As Integer = 512 * 1024 * 1024

        Protected __bufferSize As Integer
        Protected __encoding As Encoding

        Public Overrides Function ToString() As String
            Dim encodes As String = __encoding.ToString
            Dim x As New With {encodes, EndRead, .lefts = Me.lefts.Length, __bufferSize, FileName}
            Return x.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"><see cref="System.Text.Encoding.Default"/>, if null</param>
        Sub New(path$, Optional encoding As Encoding = Nothing, Optional maxBufferSize As Integer = BufferedStream.maxBufferSize)
            If Not path.FileExists Then
                Throw New FileNotFoundException("Buffer file is not found!", path.GetFullPath)
            ElseIf maxBufferSize > BufferedStream.maxBufferSize Then
                Throw New InternalBufferOverflowException($"String reader buffer(size={maxBufferSize} bytes) is too large!")
            Else
                FileName = path
                encoding = encoding Or DefaultEncoding
                __encoding = encoding
            End If

            Dim file As FileInfo = FileIO.FileSystem.GetFileInfo(path)

            If file.Length > maxBufferSize Then
                __innerStream = IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                __bufferSize = maxBufferSize
            Else
                __innerBuffer = IO.File.ReadAllLines(path, encoding)
            End If
        End Sub

        Sub New(stream As FileStream, Optional readSize As Integer = BufferedStream.maxBufferSize)
            __innerStream = stream
            __bufferSize = readSize
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' End of buffer read?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EndRead As Boolean = False

        Dim lefts As Byte() = New Byte(-1) {}

        ''' <summary>
        ''' Reset the stream buffer reader to its initial state.
        ''' </summary>
        Public Overridable Sub Reset()
            _EndRead = False
            lefts = New Byte(-1) {}
            If Not __innerStream Is Nothing Then
                __innerStream.Position = Scan0
            End If
        End Sub

        Dim l As Integer

        ''' <summary>
        ''' 当<see cref="EndRead"/>之后，这个函数将不会返回任何值
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function BufferProvider() As String()
            If EndRead Then
                Return Nothing
            Else
                If __innerBuffer Is Nothing Then
                    Dim buffer As Byte()

                    If __innerStream.Length - __innerStream.Position >= __bufferSize Then
                        l = lefts.Length + __bufferSize
                        _EndRead = False
                    Else
                        l = __innerStream.Length - __innerStream.Position
                        _EndRead = True
                    End If

                    buffer = New Byte(lefts.Length + l - 1) {}
                    Call __innerStream.Read(buffer, lefts.Length, l)
                    Call Array.ConstrainedCopy(lefts, Scan0, buffer, Scan0, lefts.Length)

                    Dim s As String = __encoding.GetString(buffer)
                    Dim sbuf As String() = s.LineTokens()

                    If Not EndRead Then
                        Dim last As String = sbuf.Last
                        Dim lch As Char = s.Last
                        If lch = vbLf OrElse lch = vbCr Then
                            last &= vbCrLf  ' 由于ltokens会替换掉换行符，可能会导致bug，所以在这里进行判断，尝试进行补齐操作
                        End If
                        lefts = __encoding.GetBytes(last)
                        sbuf = sbuf.Take(sbuf.Length - 1).ToArray
                    End If

                    Return sbuf
                Else
                    _EndRead = True
                    Return DirectCast(__innerBuffer.Clone, String())
                End If
            End If
        End Function

        Public Iterator Function ReadAllLines() As IEnumerable(Of String)
            Call Reset()

            Do While Not EndRead
                For Each line As String In BufferProvider()
                    Yield line
                Next
            Loop
        End Function

        Public Shared Iterator Function LinesIterator(path As String, Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of String)
            Using read As New BufferedStream(path, encoding.CodePage)
                Do While Not read.EndRead
                    For Each line As String In read.BufferProvider
                        Yield line
                    Next
                Loop
            End Using
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If Not __innerStream Is Nothing Then
                        Call __innerStream.Dispose()
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
