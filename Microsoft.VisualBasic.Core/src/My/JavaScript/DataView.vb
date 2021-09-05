Imports System.IO

Namespace My.JavaScript

    Public Class DataView : Implements IDisposable

        ''' <summary>
        ''' The binary data is present in big endian.
        ''' 
        ''' (network byte order)
        ''' </summary>
        Public Const BIG_ENDIAN As UShort = &HFEFF

        ''' <summary>
        ''' The binary data is present in little endian.
        ''' </summary>
        Public Const LITTLE_ENDIAN As UShort = &HFFFE

        Protected stream As MemoryStream

        Public Overridable ReadOnly Property byteLength As Integer
            Get
                Return stream.Length
            End Get
        End Property

        Sub New(bytes As Byte())
            stream = New MemoryStream(bytes)
        End Sub

        Sub New(bytes As SByte())
            Call Me.New(CType(CObj(bytes), Byte()))
        End Sub

        Sub New(bytes As MemoryStream)
            Me.stream = bytes
        End Sub

        Private disposedValue As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call stream.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace