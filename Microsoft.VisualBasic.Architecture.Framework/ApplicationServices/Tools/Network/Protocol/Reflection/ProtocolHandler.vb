#Region "Microsoft.VisualBasic::bf3e3e62bb95b64355777782b4175059, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Protocol\Reflection\ProtocolHandler.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Net
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Http

Namespace Net.Protocols.Reflection

    ''' <summary>
    ''' 这个模块只处理<see cref="Net.Abstract.DataRequestHandler"/>类型的接口
    ''' </summary>
    Public Class ProtocolHandler : Inherits IProtocolHandler

        Protected Protocols As Dictionary(Of Long, Net.Abstract.DataRequestHandler)
        ''' <summary>
        ''' 这个类型建议一般为某种枚举类型
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DeclaringType As System.Type
        Public Overrides ReadOnly Property ProtocolEntry As Long

        Public Overrides Function ToString() As String
            Return $"*{ProtocolEntry}   ---> {DeclaringType.FullName}  //{Protocols.Count} Protocols."
        End Function

        ''' <summary>
        ''' 请注意，假若没有在目标的类型定义之中查找出入口点的定义，则这个构造函数会报错，
        ''' 假若需要安全的创建对象，可以使用<see cref="ProtocolHandler.SafelyCreateObject(Of T)(T)"/>函数
        ''' </summary>
        ''' <param name="obj">Protocol的实例</param>
        Sub New(obj As Object)
            Dim type As Type = obj.GetType
            Dim entry As Protocol = Protocol.GetProtocolCategory(type)

            Me.DeclaringType = entry?.DeclaringType
            Me.ProtocolEntry = entry?.EntryPoint
            ' 解析出所有符合 WrapperClassTools.Net.DataRequestHandler 接口类型的函数方法

            Dim Methods = type.GetMethods(System.Reflection.BindingFlags.Public Or
                                          System.Reflection.BindingFlags.NonPublic Or
                                          System.Reflection.BindingFlags.Instance)
            Dim LQuery = (From entryPoint In Methods.AsParallel
                          Let Protocol As Protocol = Protocol.GetEntryPoint(entryPoint)
                          Let method As DataRequestHandler = GetMethod(obj, entryPoint)
                          Where Not (Protocol Is Nothing) AndAlso
                              Not method Is Nothing
                          Select Protocol, entryPoint, method)

            Me.Protocols = LQuery.ToDictionary(Function(element) element.Protocol.EntryPoint,
                                               Function(element) element.method)
        End Sub

        ''' <summary>
        ''' 失败会返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="App"></param>
        ''' <returns></returns>
        Public Shared Function SafelyCreateObject(Of T As Class)(App As T) As ProtocolHandler
            Try
                Return New ProtocolHandler(App)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Shared Function SafelyCreateObject(App As Object) As ProtocolHandler
            Try
                Return New ProtocolHandler(App)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Function HandlePush(uid As Long, request As RequestStream) As RequestStream
            Return HandleRequest(uid, request, Nothing)
        End Function

        ''' <summary>
        ''' Handle the data request from the client for socket events: <see cref="Net.TcpSynchronizationServicesSocket.Responsehandler"/> or <see cref="Net.SSL.SSLSynchronizationServicesSocket.Responsehandler"/>
        ''' </summary>
        ''' <param name="CA"></param>
        ''' <param name="request">The request stream object which contains the commands from the client</param>
        ''' <param name="remoteDevcie">The IPAddress of the target incoming client data request.</param>
        ''' <returns></returns>
        Public Overrides Function HandleRequest(CA As Long, request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As RequestStream
            If request.ProtocolCategory <> Me.ProtocolEntry Then
#If DEBUG Then
                Call $"Protocol_entry:={request.ProtocolCategory} was not found!".__DEBUG_ECHO
#End If
                Return NetResponse.RFC_VERSION_NOT_SUPPORTED
            End If

            If Not Me.Protocols.ContainsKey(request.Protocol) Then
#If DEBUG Then
                Call $"Protocol:={request.Protocol} was not found!".__DEBUG_ECHO
#End If
                Return NetResponse.RFC_NOT_IMPLEMENTED  ' 没有找到相对应的协议处理逻辑，则没有实现相对应的数据协议处理方法
            End If

            Dim EntryPoint As DataRequestHandler = Me.Protocols(request.Protocol)
            Dim value As RequestStream = EntryPoint(CA, request, remoteDevcie)
            Return value
        End Function

        Private Shared Function GetMethod(obj As Object, entryPoint As System.Reflection.MethodInfo) As Net.Abstract.DataRequestHandler
            Dim parameters As System.Reflection.ParameterInfo() = entryPoint.GetParameters

            If Not entryPoint.ReturnType.Equals(GetType(RequestStream)) Then
                Return Nothing
            End If

            If parameters.Length > 3 Then
                Return Nothing
            End If

            If parameters.Length = 0 Then
                Return AddressOf New __protocolInvoker(obj, entryPoint).InvokeProtocol0
            End If

            If parameters.Length = 1 Then
                If Not parameters.First.ParameterType.Equals(GetType(Long)) Then
                    Return Nothing
                End If

                Return AddressOf New __protocolInvoker(obj, entryPoint).InvokeProtocol1
            End If

            If parameters.Length = 2 Then
                If (Not parameters.First.ParameterType.Equals(GetType(Long)) OrElse
                Not parameters.Last.ParameterType.Equals(GetType(RequestStream))) Then
                    Return Nothing
                End If

                Return AddressOf New __protocolInvoker(obj, entryPoint).InvokeProtocol2
            End If

            If parameters.Length = 3 Then
                If (Not parameters.First.ParameterType.Equals(GetType(Long)) OrElse
                Not parameters(1).ParameterType.Equals(GetType(RequestStream)) OrElse
                Not parameters.Last.ParameterType.Equals(GetType(System.Net.IPEndPoint))) Then
                    Return Nothing
                End If

                Return AddressOf New __protocolInvoker(obj, entryPoint).InvokeProtocol3
            End If

            Return Nothing
        End Function
    End Class
End Namespace
