Imports Microsoft.VisualBasic.Parallel

Namespace MMFProtocol.MapStream

    Public Class MSIOReader : Implements System.IDisposable

        ''' <summary>
        ''' 内存映射文件的更新标识符
        ''' </summary>
        ''' <remarks></remarks>
        Dim _udtBadge As Long
        Dim _chunkBuffer(ChunkSize - 1) As Byte
        Dim _mappedStream As MMFStream
        Dim _mmfileStream As System.IO.MemoryMappedFiles.MemoryMappedFile

        ''' <summary>
        ''' 内存映射文件的数据块的预分配大小
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ChunkSize As Long = 5 * 1024

        Public Property DataArrivalCallBack As DataArrival

        Public ReadOnly Property URI As String

        Sub New(uri As String)
            _mmfileStream = IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(uri)
            _URI = uri

            Call Run(AddressOf __threadElapsed)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{URI} ===> {NameOf(_udtBadge)}:={_udtBadge}"
        End Function

        Public Sub Update(thisUpdate As Long)
            Me._udtBadge = thisUpdate
        End Sub

        Public Function Read() As MMFStream
            Call _mmfileStream.CreateViewStream.Read(_chunkBuffer, Scan0, ChunkSize)
            Return New MMFStream(_chunkBuffer)
        End Function

        Private Sub __threadElapsed()
            Do While Not Me.disposedValue
                Call __clientThreadElapsed()
                Call Threading.Thread.Sleep(1)
            Loop
        End Sub

        Private Sub __clientThreadElapsed()
            Call _mmfileStream.CreateViewStream.Read(_chunkBuffer, Scan0, ChunkSize)
            _mappedStream = New MMFStream(_chunkBuffer)
            If _mappedStream.udtBadge > Me._udtBadge Then  '当从数据流中所读取到的更新标识符大于对象实例中的更新标识符的时候，认为数据发生了更新
                Me._udtBadge = _mappedStream.udtBadge
                Call DataArrivalCallBack()(_mappedStream.byteData)
            End If
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
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