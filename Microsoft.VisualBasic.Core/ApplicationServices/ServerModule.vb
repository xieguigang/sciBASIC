Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices

    ''' <summary>
    ''' The Tcp socket server abstract
    ''' </summary>
    Public MustInherit Class ServerModule : Implements IDisposable

        Protected socket As TcpServicesSocket

        Sub New(port%)
            socket = New TcpServicesSocket(port, AddressOf LogException) With {
                .Responsehandler = ProtocolHandler()
            }
        End Sub

        Protected MustOverride Sub LogException(ex As Exception)
        Protected MustOverride Function ProtocolHandler() As ProtocolHandler

        Public Overridable Function Run() As Integer
            Return socket.Run
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call socket.Dispose()
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

    Public Class ProtocolInvoker(Of T As {IComparable, IFormattable, IConvertible})

        Public ReadOnly Property Protocol As New Protocol(GetType(T))
        Public ReadOnly Property TcpRequest As TcpRequest

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(hostName$, remotePort%)
            TcpRequest = New TcpRequest(hostName, remotePort)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SendMessage(protocol As T, message$, Optional encoding As Encoding = Nothing) As RequestStream
            Return SendMessage(protocol, (encoding Or DefaultEncoding).GetBytes(message))
        End Function

        Public Function SendMessage(protocol As T, data As Byte()) As RequestStream
            Dim category& = Me.Protocol.EntryPoint
            Dim protocolL& = CLng(CObj(protocol))
            Dim message As New RequestStream(category, protocolL, data)

            Return TcpRequest.SendMessage(message)
        End Function

        Public Function SendMessage(Of V As {New, Class})(protocol As T, data As V) As RequestStream
            Return SendMessage(protocol, data.GetJson)
        End Function
    End Class
End Namespace