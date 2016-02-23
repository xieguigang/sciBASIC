Imports System.Net.NetworkInformation
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net

    ''' <summary>
    ''' State object for reading client data asynchronously
    ''' </summary>
    ''' <remarks></remarks>
    Public Class StateObject : Implements System.IDisposable

        ''' <summary>
        ''' Client  socket.
        ''' </summary>
        ''' <remarks></remarks>
        Public workSocket As Socket
        ''' <summary>
        ''' Size of receive buffer.
        ''' </summary>
        ''' <remarks></remarks>
        Public Const BufferSize As Integer = 1024 * 1024
        ''' <summary>
        ''' Receive buffer.
        ''' </summary>
        ''' <remarks></remarks>
        Public readBuffer(BufferSize) As Byte
        ''' <summary>
        ''' Received data.
        ''' </summary>
        ''' <remarks></remarks>
        Public ChunkBuffer As New List(Of Byte)

        Public Overrides Function ToString() As String
            Return workSocket.RemoteEndPoint.ToString & " <=====> " & workSocket.LocalEndPoint.ToString
        End Function

        Public Function GetRequest() As RequestStream
            Return New RequestStream(ChunkBuffer.ToArray)
        End Function

#Region "IDisposable Support"
        Protected disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then

                    On Error Resume Next

                    Dim ep As String = workSocket.RemoteEndPoint.ToString

                    ' TODO: dispose managed state (managed objects).
                    Call ChunkBuffer.Clear()
                    Call ChunkBuffer.Free
                    Call readBuffer.Free
                    Call workSocket.Shutdown(SocketShutdown.Both)
                    Call workSocket.Free

                    Call Console.WriteLine($"[DEBUG {Now.ToString}] Socket {ep} clean up!")
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