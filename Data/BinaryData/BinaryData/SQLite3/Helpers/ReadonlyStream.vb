Imports System.IO

Namespace ManagedSqlite.Core.Helpers
    Friend MustInherit Class ReadonlyStream
        Inherits Stream
        Public Overrides Sub Flush()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetLength(value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return False
            End Get
        End Property
    End Class
End Namespace
