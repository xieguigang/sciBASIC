Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports System.Runtime.CompilerServices

''' <summary>
''' Wrapper for the file operations.
''' </summary>
''' <remarks></remarks>
<[Namespace]("Large_Text_File")>
Public Module FileOperations

    ''' <summary>
    ''' 将文本之中的所有行读取出来
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <ExportAPI("Read.Lines"), Extension>
    Public Function ReadAllLines(path As String, Optional encoding As Encodings = Encodings.Default) As String()
        Return IO.File.ReadAllLines(path, encoding.GetEncodings)
    End Function

    <ExportAPI("Partitioning")>
    Public Function TextPartition(data As Generic.IEnumerable(Of String)) As String()()
        Dim maxSize As Double = New StringBuilder(1024 * 1024).MaxCapacity
        Return __textPartitioning(data.ToArray, maxSize)
    End Function

    Private Function __textPartitioning(dat As String(), maxSize As Double) As String()()
        Dim currentSize As Double = (From s As String In dat.AsParallel Select CDbl(Len(s))).ToArray.Sum
        If currentSize > maxSize Then
            Dim SplitTokens = dat.Split(CInt(dat.Length / 2))
            If SplitTokens.Length > 1 Then
                Return (From n In SplitTokens Select __textPartitioning(n, maxSize)).ToArray.MatrixToVector
            Else
                Return SplitTokens
            End If
        Else
            Return New String()() {dat}
        End If
    End Function

    ''' <summary>
    ''' 当一个文件非常大以致无法使用任何现有的文本编辑器查看的时候，可以使用本方法查看其中的一部分数据 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Peeks")>
    Public Function Peeks(path As String, length As Integer) As String
        Dim ChunkBuffer As Char() = New Char(length - 1) {}
        Using Reader = FileIO.FileSystem.OpenTextFileReader(path)
            Call Reader.ReadBlock(ChunkBuffer, 0, ChunkBuffer.Length)
        End Using
        Return New String(value:=ChunkBuffer)
    End Function

    ''' <summary>
    ''' 尝试查看大文件的尾部的数据
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="length">字符的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Tails")>
    Public Function Tails(path As String,
                          <Parameter("characters", "The number of the characters, not the bytes value.")> length As Integer) As String
        length *= 8

        Using Reader = New IO.FileStream(path, IO.FileMode.OpenOrCreate)
            If Reader.Length < length Then
                length = Reader.Length
            End If

            Dim ChunkBuffer As Byte() = New Byte(length - 1) {}

            Call Reader.Seek(Reader.Length - length, IO.SeekOrigin.Begin)
            Call Reader.Read(ChunkBuffer, 0, ChunkBuffer.Length)

            Dim value As String = System.Text.Encoding.Default.GetString(ChunkBuffer)
            Return value
        End Using
    End Function

    <ExportAPI(".Merge", Info:="Please make sure all of the file in the target directory is text file not binary file.")>
    Public Function Merge(<Parameter("Dir", "The default directory parameter value is the current directory.")> Optional dir As String = "./") As String
        Dim Texts = From file As String
                    In FileIO.FileSystem.GetFiles(dir, FileIO.SearchOption.SearchAllSubDirectories, "*.*")
                    Select FileIO.FileSystem.ReadAllText(file)
        Dim Merged As String = String.Join(vbCr, Texts)
        Return Merged
    End Function

    Public Delegate Function PartitioningMethod(block As String, ByRef Left As String) As String()

    ''' <summary>
    ''' 只是针对文本文件的
    ''' </summary>
    Public Class PartitionedStream : Implements System.IDisposable

        ReadOnly _readerStream As IO.FileStream
        ReadOnly _blockSize As Integer
        ReadOnly _partitions As PartitioningMethod
        ReadOnly _encoding As System.Text.Encoding

        Sub New(path As String, blockSize As Integer, partitioning As PartitioningMethod, Optional encoding As System.Text.Encoding = Nothing)
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
        Sub New(path As String, blockSize As Integer, Optional encoding As System.Text.Encoding = Nothing)
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
End Module
