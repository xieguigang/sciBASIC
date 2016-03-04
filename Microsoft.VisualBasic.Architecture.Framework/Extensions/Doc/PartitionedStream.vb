Imports System.Text

Public Delegate Function PartitioningMethod(block As String, ByRef Left As String) As String()

''' <summary>
''' 只是针对文本文件的
''' </summary>
Public Class PartitionedStream : Implements System.IDisposable

    ReadOnly _readerStream As IO.FileStream
    ReadOnly _blockSize As Integer
    ReadOnly _partitions As PartitioningMethod
    ReadOnly _encoding As System.Text.Encoding

    Sub New(path As String, blockSize As Integer, partitioning As PartitioningMethod, Optional encoding As Encoding = Nothing)
        _readerStream = New IO.FileStream(path, IO.FileMode.Open)
        _blockSize = blockSize
        _partitions = partitioning
        _Total = _readerStream.Length

        If _encoding Is Nothing Then
            _encoding = System.Text.Encoding.Default
        Else
            _encoding = encoding
        End If
    End Sub

    ''' <summary>
    ''' 依照换行符来进行分区
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="blockSize"></param>
    ''' <param name="encoding"></param>
    Sub New(path As String, blockSize As Integer, Optional encoding As Encoding = Nothing)
        Call Me.New(path, blockSize, AddressOf PartitionedStream.PartitionByLines, encoding)
    End Sub

    Public ReadOnly Property EOF As Boolean
        Get
            Return _Current >= _Total
        End Get
    End Property

    Public ReadOnly Property Current As Long
    Public ReadOnly Property Total As Long

    Dim previous As Byte()

    Public Function ReadPartition() As String()
        If EOF Then Return Nothing

        Dim chunkBuffer As Byte()

        If _Current + _blockSize > _Total Then
            chunkBuffer = New Byte(_blockSize - 1) {}
        Else
            chunkBuffer = New Byte(_Total - _Current - 1) {}
        End If

        Call _readerStream.Read(chunkBuffer, _Current, chunkBuffer.Length)
        Call previous.Add(chunkBuffer)

        Dim Text As String = _encoding.GetString(previous)
        Dim rtvl = Me._partitions(Text, Text)
        previous = _encoding.GetBytes(Text)
        Return rtvl
    End Function

    Public Overrides Function ToString() As String
        Return _readerStream.Name
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
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

    Public Shared Function PartitionByLines(block As String, ByRef Left As String) As String()
        Dim Tokens As String() = block.lTokens

        Left = Tokens.LastOrDefault
        Tokens = Tokens.Takes(Tokens.Length - 1).ToArray
        Return Tokens
    End Function
End Class