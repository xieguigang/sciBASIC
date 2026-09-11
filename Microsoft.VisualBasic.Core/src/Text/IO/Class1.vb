Imports System.IO
Imports System.Text

Namespace Text

    ''' <summary>按字节定位、以 0x0A 切行的缓冲读取器（顺带剥离结尾 CR）。</summary>
    Public NotInheritable Class BufferedLineReader
        Private ReadOnly _s As FileStream
        Private ReadOnly _enc As Encoding
        Private ReadOnly _buf As Byte()
        Private _base As Long
        Private _pos As Integer
        Private _len As Integer
        Private _lastStart, _lastEnd As Long
        Private _lastTerm As Boolean

        Public Sub New(stream As FileStream, enc As Encoding, bufferSize As Integer)
            _s = stream
            _enc = enc
            If bufferSize < 4096 Then bufferSize = 4096
            _buf = New Byte(bufferSize - 1) {}
            _base = stream.Position
        End Sub

        Public ReadOnly Property LastLineStartOffset As Long
            Get
                Return _lastStart
            End Get
        End Property
        Public ReadOnly Property LastLineEndOffset As Long
            Get
                Return _lastEnd
            End Get
        End Property
        Public ReadOnly Property LastLineWasTerminated As Boolean
            Get
                Return _lastTerm
            End Get
        End Property

        Public Sub SeekTo(offset As Long)
            _s.Position = offset
            _base = offset
            _pos = 0
            _len = 0
        End Sub

        Public Function ReadLine(ByRef line As String) As Boolean
            Dim startOff As Long = _base + _pos
            Dim ms As MemoryStream = Nothing
            Do
                If _pos >= _len Then
                    Refill()
                    If _len = 0 Then
                        If ms Is Nothing OrElse ms.Length = 0 Then Return False ' 干净 EOF
                        _lastStart = startOff
                        _lastEnd = _base + _pos
                        _lastTerm = False
                        line = Decode(ms)
                        Return True
                    End If
                End If
                Dim k As Integer = Array.IndexOf(_buf, LF, _pos, _len - _pos)
                If k >= 0 Then
                    Dim s As String
                    If ms Is Nothing Then
                        Dim cnt As Integer = k - _pos
                        Dim c2 As Integer = If(cnt > 0 AndAlso _buf(k - 1) = CR, cnt - 1, cnt)
                        s = _enc.GetString(_buf, _pos, c2)
                    Else
                        ms.Write(_buf, _pos, k - _pos)
                        s = Decode(ms)
                    End If
                    _pos = k + 1
                    _lastStart = startOff
                    _lastEnd = _base + _pos
                    _lastTerm = True
                    line = s
                    Return True
                Else
                    If ms Is Nothing Then ms = New MemoryStream()
                    ms.Write(_buf, _pos, _len - _pos)
                    _pos = _len
                End If
            Loop
        End Function

        Private Function Decode(ms As MemoryStream) As String
            Dim n As Integer = CInt(ms.Length)
            Dim c2 As Integer = If(n > 0 AndAlso ms.GetBuffer()(n - 1) = CR, n - 1, n)
            Return _enc.GetString(ms.GetBuffer(), 0, c2)
        End Function

        Private Sub Refill()
            _base += _len
            _pos = 0
            _len = _s.Read(_buf, 0, _buf.Length)
        End Sub
    End Class

End Namespace