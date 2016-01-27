Namespace MMFProtocol.Pipeline

    ''' <summary>
    ''' exec cmd /var $&lt;piplineName>, this can be using in the CLI programming for passing the variables between the program more efficient
    ''' </summary>
    Public Class Pipeline

        ReadOnly _sockets As SortedDictionary(Of String, MMFSocket) =
            New SortedDictionary(Of String, MMFSocket)
        ReadOnly _netSocket As Net.TcpSynchronizationServicesSocket

        Sub New(port As Integer)
            _netSocket = New Net.TcpSynchronizationServicesSocket(port)
        End Sub

        ''' <summary>
        ''' 假若变量不存在，则返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="var"></param>
        ''' <returns></returns>
        Public Function GetValue(Of T As Net.Protocol.RawStream)(var As String) As T
            If Not _sockets.ContainsKey(var) Then
                Return Nothing
            End If

            Dim data As MMFSocket = _sockets(var)
            Dim raw As Object = Activator.CreateInstance(GetType(T), {data.ReadData})
            Dim x As T = DirectCast(raw, T)
            Return x
        End Function

        ''' <summary>
        ''' 在写数据之前需要先使用这个方法进行内存区块的创建
        ''' </summary>
        ''' <returns></returns>
        Private Function __allocated() As Net.Protocol.RawStream

        End Function
    End Class
End Namespace