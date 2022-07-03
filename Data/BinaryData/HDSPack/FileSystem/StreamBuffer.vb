Imports System.IO

Namespace FileSystem

    Public Class StreamBuffer : Inherits Stream

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property Length As Long
            Get
                Return buffer.Length
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides Property Position As Long
            Get
                Return buffer.Position
            End Get
            Set(value As Long)
                buffer.Position = value
            End Set
        End Property

        ReadOnly basefile As Stream
        ReadOnly buffer As MemoryStream
        ReadOnly block As StreamBlock

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="block"></param>
        ''' <param name="buffer_size"></param>
        Friend Sub New(buffer As Stream,
                       block As StreamBlock,
                       Optional buffer_size As Integer = 1024)

            Me.block = block
            Me.basefile = buffer
            Me.buffer = New MemoryStream(capacity:=buffer_size)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Flush()
            Call buffer.Flush()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub SetLength(value As Long)
            Call buffer.SetLength(value)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Call Me.buffer.Write(buffer, offset, count)
        End Sub

        ''' <inheritdoc />
        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Return Me.buffer.Read(buffer, offset, count)
        End Function

        ''' <inheritdoc />
        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Return buffer.Seek(offset, origin)
        End Function

        ''' <summary>
        ''' write the in-memory content data into 
        ''' the base file stream, and then update 
        ''' the stream block content data
        ''' </summary>
        Private Sub writeBuffer()
            block.size = buffer.Length
            block.offset = basefile.Length
            buffer.Flush()
            basefile.Position = block.offset
            basefile.Write(buffer.ToArray, offset:=Scan0, count:=buffer.Length)
            basefile.Flush()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            writeBuffer()
            buffer.Dispose()
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace