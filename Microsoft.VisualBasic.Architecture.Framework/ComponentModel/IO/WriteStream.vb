'Namespace ComponentModel.IO

'    Public Delegate Function ToString(Of T)(x As T) As String

'    Public Class WriteStream(Of T) : Implements IDisposable

'        ReadOnly _stream As System.IO.FileStream
'        ReadOnly _toString As ToString(Of T)
'        ReadOnly _encoding As System.Text.Encoding
'        ReadOnly _endT As String

'        Sub New(handle As String, ToString As ToString(Of T), Optional encoding As TextEncodings.Encodings = Encodings.UTF8, Optional endl As String = vbCrLf)
'            _stream = New System.IO.FileStream(handle, System.IO.FileMode.OpenOrCreate)
'            _toString = ToString
'            _encoding = encoding.GetEncodings
'            _endT = endl
'        End Sub

'        Public Sub Write(x As T)
'            Dim line As String = _toString(x) & _endT
'            Dim buf As Byte() = _encoding.GetBytes(line)
'            Call _stream.Write(buf, Scan0, buf.Length)
'        End Sub

'        Public Sub WriteBlock(source As IEnumerable(Of T))
'            Dim lines = source.ToArray(Function(x) _toString(x))
'            Dim block As String = String.Join(_endT, lines)
'            Dim buf As Byte() = _encoding.GetBytes(block)
'            Call _stream.Write(buf, Scan0, buf.Length)
'        End Sub

'#Region "IDisposable Support"
'        Private disposedValue As Boolean ' To detect redundant calls

'        ' IDisposable
'        Protected Overridable Sub Dispose(disposing As Boolean)
'            If Not Me.disposedValue Then
'                If disposing Then
'                    ' TODO: dispose managed state (managed objects).
'                    Call _stream.Flush()
'                    Call _stream.Close()
'                End If

'                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
'                ' TODO: set large fields to null.
'            End If
'            Me.disposedValue = True
'        End Sub

'        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
'        'Protected Overrides Sub Finalize()
'        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'        '    Dispose(False)
'        '    MyBase.Finalize()
'        'End Sub

'        ' This code added by Visual Basic to correctly implement the disposable pattern.
'        Public Sub Dispose() Implements IDisposable.Dispose
'            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'            Dispose(True)
'            ' TODO: uncomment the following line if Finalize() is overridden above.
'            ' GC.SuppressFinalize(Me)
'        End Sub
'#End Region

'    End Class
'End Namespace