Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace MMFProtocol.Pipeline

    ''' <summary>
    ''' exec cmd /var $&lt;piplineName>, this can be using in the CLI programming for passing the variables between the program more efficient
    ''' </summary>
    ''' 
    <Protocol(GetType(API.Protocols))>
    Public Class Pipeline

        ReadOnly _sockets As SortedDictionary(Of String, MapStream.MSWriter) =
            New SortedDictionary(Of String, MapStream.MSWriter)
        ReadOnly _netSocket As Net.TcpSynchronizationServicesSocket
        ReadOnly _protocols As Net.Abstract.IProtocolHandler

        Sub New(Optional port As Integer = API.PeplinePort)
            _protocols = New ProtocolHandler(Me)
            _netSocket = New Net.TcpSynchronizationServicesSocket(port)
            _netSocket.Responsehandler = AddressOf _protocols.HandleRequest

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
        <Protocol(API.Protocols.Allocation)>
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

        <Protocol(API.Protocols.Destroy)>
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