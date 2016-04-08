Imports System.IO
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel

    ''' <summary>
    ''' Buffered large text dataset reader
    ''' </summary>
    Public Class BufferedStream

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
        Sub New(path As String, Optional encoding As Encoding = Nothing, Optional maxBufferSize As Integer = BufferedStream.maxBufferSize)
            If Not path.FileExists Then
                Throw New FileNotFoundException("Buffer file is not found!", path.GetFullPath)
            ElseIf maxBufferSize > BufferedStream.maxBufferSize Then
                Throw New InternalBufferOverflowException($"String reader buffer(size={maxBufferSize} bytes) is too large!")
            Else
                FileName = path
                encoding = encoding.Assertion
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
                    Dim sbuf As String() = s.lTokens

                    If Not EndRead Then
                        Dim last As String = sbuf.Last
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
    End Class
End Namespace