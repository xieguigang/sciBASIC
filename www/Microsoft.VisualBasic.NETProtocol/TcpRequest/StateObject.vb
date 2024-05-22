#Region "Microsoft.VisualBasic::16f9df96bc137c5bf876d1b056d24273, www\Microsoft.VisualBasic.NETProtocol\TcpRequest\StateObject.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 81
    '    Code Lines: 33 (40.74%)
    ' Comment Lines: 34 (41.98%)
    '    - Xml Docs: 58.82%
    ' 
    '   Blank Lines: 14 (17.28%)
    '     File Size: 2.87 KB


    '     Class StateObject
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Sockets

Namespace Tcp

    ''' <summary>
    ''' State object for reading client data asynchronously
    ''' </summary>
    ''' <remarks></remarks>
    Public Class StateObject : Implements IDisposable

        ''' <summary>
        ''' Size of receive buffer.
        ''' </summary>
        ''' <remarks></remarks>
        Public Const BufferSize As Integer = 1024 * 1024 * 32

        ''' <summary>
        ''' Client  socket.
        ''' </summary>
        ''' <remarks></remarks>
        Public workSocket As Socket
        ''' <summary>
        ''' Receive buffer.
        ''' </summary>
        ''' <remarks></remarks>
        Public readBuffer(BufferSize - 1) As Byte
        ''' <summary>
        ''' Received data.
        ''' </summary>
        ''' <remarks></remarks>
        Public received As Stream

        Public Overrides Function ToString() As String
            Return workSocket.RemoteEndPoint.ToString & " <==> " & workSocket.LocalEndPoint.ToString
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
                    Call received.Dispose()
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
