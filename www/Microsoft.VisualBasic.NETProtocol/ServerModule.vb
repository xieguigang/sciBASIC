#Region "Microsoft.VisualBasic::88fd998f4e0101c2b71d2b3f42694a96, www\Microsoft.VisualBasic.NETProtocol\ServerModule.vb"

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

    '   Total Lines: 107
    '    Code Lines: 61 (57.01%)
    ' Comment Lines: 28 (26.17%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 18 (16.82%)
    '     File Size: 4.39 KB


    '     Class ServerModule
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Run
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class ProtocolInvoker
    ' 
    '         Properties: Protocol, TcpRequest, TextEncoding
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+3 Overloads) SendMessage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices

    ''' <summary>
    ''' The Tcp socket server abstract
    ''' </summary>
    Public MustInherit Class ServerModule : Implements IDisposable

        ''' <summary>
        ''' Tcp socket
        ''' </summary>
        Protected socket As TcpServicesSocket

        ''' <summary>
        ''' Create a new server module based on a tcp server socket.
        ''' </summary>
        ''' <param name="port">The listen port of the tcp socket.</param>
        Sub New(port As Integer)
            socket = New TcpServicesSocket(port, AddressOf LogException) With {
                .ResponseHandler = ProtocolHandler()
            }
        End Sub

        Protected MustOverride Sub LogException(ex As Exception)
        ''' <summary>
        ''' Generally, using a <see cref="ProtocolAttribute"/> attribute using reflection way is recommended.
        ''' </summary>
        ''' <returns></returns>
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

        Public ReadOnly Property Protocol As New ProtocolAttribute(GetType(T))
        Public ReadOnly Property TcpRequest As TcpRequest
        Public ReadOnly Property TextEncoding As [Default](Of Encoding)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(hostName$, remotePort%, Optional encoding As Encodings = Encodings.UTF8WithoutBOM)
            TcpRequest = New TcpRequest(hostName, remotePort)
            TextEncoding = encoding.CodePage
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SendMessage(protocol As T, message$, Optional textEncoding As Encoding = Nothing) As RequestStream
            Return SendMessage(protocol, (textEncoding Or Me.TextEncoding).GetBytes(message))
        End Function

        Public Function SendMessage(protocol As T, data As Byte()) As RequestStream
            Dim category& = Me.Protocol.EntryPoint
            Dim protocolL& = CLng(CObj(protocol))
            Dim message As New RequestStream(category, protocolL, data)

            Return TcpRequest.SendMessage(message)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SendMessage(Of V As {New, Class})(protocol As T, data As V) As RequestStream
            Return SendMessage(protocol, data.GetJson)
        End Function
    End Class
End Namespace
