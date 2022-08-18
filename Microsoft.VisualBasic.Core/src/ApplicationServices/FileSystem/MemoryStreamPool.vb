
Imports System.IO

Namespace ApplicationServices

    Public Class MemoryStreamPool : Inherits Stream

        ReadOnly pool As MemoryStream()
        ReadOnly buffer_size As Integer

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Return pool.Sum(Function(ms) ms.Length)
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As Long)
                Throw New NotImplementedException()
            End Set
        End Property

        Public Overrides Sub Flush()
            For Each ms As MemoryStream In pool
                Call ms.Flush()
            Next
        End Sub

        Public Overrides Sub SetLength(value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace