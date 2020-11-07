#Region "Microsoft.VisualBasic::fb46febd16184332fa93352718dbb594, www\Microsoft.VisualBasic.NETProtocol\Pipeline\Pipeline.vb"

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

    '     Class Pipeline
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __allocated, __destroy, GetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Parallel.MMFProtocol
Imports Microsoft.VisualBasic.Serialization

Namespace MMFProtocol.Pipeline

    ''' <summary>
    ''' exec cmd /var $&lt;piplineName>, this can be using in the CLI programming for passing the variables between the program more efficient
    ''' </summary>
    ''' 
    <ProtocolAttribute(GetType(API.Protocols))>
    Public Class Pipeline

        ReadOnly _sockets As SortedDictionary(Of String, MapStream.MSWriter) =
            New SortedDictionary(Of String, MapStream.MSWriter)
        ReadOnly _netSocket As TcpServicesSocket
        ReadOnly _protocols As IProtocolHandler

        Sub New(Optional port As Integer = API.PeplinePort)
            _protocols = New ProtocolHandler(Me)
            _netSocket = New TcpServicesSocket(port)
            _netSocket.ResponseHandler = AddressOf _protocols.HandleRequest

            Call Parallel.RunTask(AddressOf _netSocket.Run)
        End Sub

        ''' <summary>
        ''' 假若变量不存在，则返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="var"></param>
        ''' <returns></returns>
        Public Function GetValue(Of T As RawStream)(var As String) As T
            If Not _sockets.ContainsKey(var) Then
                Return Nothing
            End If

            Dim data As MapStream.MSWriter = _sockets(var)
            Dim buf As Byte() = data.Read.byteData
            Dim raw As Object = Activator.CreateInstance(GetType(T), {buf})
            Dim x As T = DirectCast(raw, T)
            Return x
        End Function

        ''' <summary>
        ''' 在写数据之前需要先使用这个方法进行内存区块的创建
        ''' </summary>
        ''' <returns></returns>
        <ProtocolAttribute(API.Protocols.Allocation)>
        Private Function __allocated(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim s As String = request.GetUTF8String
            If Not API.IsRef(s) Then
                Return NetResponse.RFC_TOKEN_INVALID
            End If

            Dim tokens As String() = s.Split(":"c)
            Dim var As String = tokens(Scan0)
            Dim size As Long = Scripting.CTypeDynamic(Of Long)(tokens(1))

            If _sockets.ContainsKey(var) Then
                Call _sockets.Remove(var)
            End If
            Call _sockets.Add(var, New MapStream.MSWriter(var, size))

            Return NetResponse.RFC_OK
        End Function

        <ProtocolAttribute(API.Protocols.Destroy)>
        Private Function __destroy(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
            Dim var As String = request.GetUTF8String

            If _sockets.ContainsKey(var) Then
                Dim x = _sockets(var)

                Call _sockets.Remove(var)
                Call x.Free

                Return NetResponse.RFC_OK
            Else
                Return NetResponse.RFC_TOKEN_INVALID
            End If
        End Function
    End Class
End Namespace
