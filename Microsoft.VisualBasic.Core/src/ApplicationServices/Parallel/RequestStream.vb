#Region "Microsoft.VisualBasic::76d2ce41b722afffab60aed9d63311ab, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\RequestStream.vb"

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

    '   Total Lines: 448
    '    Code Lines: 232
    ' Comment Lines: 159
    '   Blank Lines: 57
    '     File Size: 18.01 KB


    '     Delegate Function
    ' 
    '         Properties: IsPing, IsPlantText, IsSSL_PublicToken, IsSSLHandshaking, IsSSLProtocol
    '                     TryGetSystemProtocol
    ' 
    '         Function: SystemProtocol
    '         Operators: (+3 Overloads) <>, (+3 Overloads) =
    '     Class RequestStream
    ' 
    '         Properties: BufferLength, ChunkBuffer, FullRead, Protocol, ProtocolCategory
    '                     TotalBytes
    ' 
    '         Constructor: (+7 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) CreatePackage, CreateProtocol, GetDoubles, GetIntegers, GetRawStream
    '                   GetString, GetUTF8String, IsAvaliableStream, (+2 Overloads) LoadObject, ToString
    ' 
    '         Sub: Serialize, WriteBuffer
    '         Enum Protocols
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Parallel

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="request"></param>
    ''' <param name="RemoteAddress"></param>
    ''' <returns></returns>
    Public Delegate Function DataRequestHandler(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe

    ''' <summary>
    ''' Socket user client => Socket server data request &amp;
    ''' Socket server => Socket user client data response package.
    ''' (Socket客户端 => Socket服务器所发送的数据请求以及从
    ''' Socket服务器 => Socket客户端所返回数据的数据响应包)
    ''' </summary>
    <Serializable> Public Class RequestStream : Inherits RawStream
        Implements ISerializable

        ''' <summary>
        ''' This property indicates the protocol processor module for the server object.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("Entry")>
        Public Property ProtocolCategory As Int64
        ''' <summary>
        ''' This property indicates which the specifics protocol processor will be 
        ''' used for the incoming client request.
        ''' (协议的头部)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("Protocol")>
        Public Property Protocol As Int64
        ''' <summary>
        ''' Buffer length of the protocol request raw stream data <see cref="ChunkBuffer"/>.
        ''' (协议数据的长度)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("bufLen")>
        Public Property BufferLength As Int64

        ''' <summary>
        ''' The raw stream data of the details data request or the server response data.
        ''' (协议的具体数据请求)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("rawBuf")>
        Public Property ChunkBuffer As Byte()

        ''' <summary>
        ''' verify that the stream data in <see cref="ChunkBuffer"/> is recieve complete?
        ''' (<see cref="ChunkBuffer"/>部分的数据是否完整？)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FullRead As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return BufferLength = ChunkBuffer.Length
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="protocolCategory"></param>
        ''' <param name="protocol"></param>
        ''' <param name="buffer"><see cref="ChunkBuffer"/></param>
        Sub New(protocolCategory&, protocol&, buffer As Byte())
            Me.ProtocolCategory = protocolCategory
            Me.Protocol = protocol
            Me.BufferLength = buffer.Length
            Me.ChunkBuffer = buffer
        End Sub

        ''' <summary>
        ''' 构建一个无参数的网络协议对象
        ''' </summary>
        ''' <param name="ProtocolCategory"></param>
        ''' <param name="Protocol"></param>
        Sub New(protocolCategory&, protocol&)
            Call Me.New(protocolCategory, protocol, New Byte() {})
        End Sub

        ''' <summary>
        ''' The default text encoding is <see cref="Encoding.UTF8"/>
        ''' </summary>
        ''' <param name="protocolCategory"></param>
        ''' <param name="protocol"></param>
        ''' <param name="strData">Protocol request argument parameters</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(protocolCategory As Long, protocol As Long, strData As String)
            Call Me.New(protocolCategory, protocol, UTF8WithoutBOM.GetBytes(strData))
        End Sub

        Sub New(ProtocolCategory As Long, Protocol As Long, str As String, encoding As Encoding)
            Call Me.New(ProtocolCategory, Protocol, encoding.GetBytes(str))
        End Sub

        Sub New(ProtocolCategory As Long, Protocol As Long, buffer As ISerializable)
            Call Me.New(ProtocolCategory, Protocol, buffer.Serialize)
        End Sub

        ''' <summary>
        ''' 其余的协议参数都是值 <see cref="HTTP_RFC.RFC_OK"/>
        ''' </summary>
        ''' <param name="data"></param>
        Sub New(data As String)
            Call Me.New(HTTP_RFC.RFC_OK, HTTP_RFC.RFC_OK, data)
        End Sub

        ''' <summary>
        ''' Deserialize (当还有剩余数据的时候会将数据进行剪裁)
        ''' </summary>
        ''' <param name="rawStream"></param>
        Sub New(rawStream As Byte())
            Call MyBase.New(rawStream)

            Dim bitChunk As Byte() = New Byte(INT64 - 1) {}
            Dim p As i32 = Scan0

            Call Array.ConstrainedCopy(rawStream, ++p, bitChunk, Scan0, INT64)
            Me.ProtocolCategory = BitConverter.ToInt64(bitChunk, Scan0)

            Call Array.ConstrainedCopy(rawStream, ++(p + INT64), bitChunk, Scan0, INT64)
            Me.Protocol = BitConverter.ToInt64(bitChunk, Scan0)

            bitChunk = New Byte(INT64 - 1) {}
            Call Array.ConstrainedCopy(rawStream, p + INT64, bitChunk, Scan0, INT64)

            Me.BufferLength = BitConverter.ToInt64(bitChunk, Scan0)
            Me.ChunkBuffer = New Byte(Me.BufferLength - 1) {}

            If CLng(p + INT64) + BufferLength > rawStream.Length Then
                ' 越界了，则数据没有读完
                ChunkBuffer = New Byte() {}
            Else
                Call Array.ConstrainedCopy(
                    rawStream, p,
                    ChunkBuffer, Scan0, BufferLength
                )
            End If
        End Sub

        ''' <summary>
        ''' 默认是使用UTF8编码来编码字符串的
        ''' </summary>
        ''' <returns>
        ''' 这个函数总是返回一个不为空值的字符串
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUTF8String() As String
            Return If(UTF8WithoutBOM.GetString(ChunkBuffer), "")
        End Function

        ''' <summary>
        ''' 按照指定的编码构建出一个字符串值
        ''' </summary>
        ''' <param name="encoding"></param>
        ''' <returns>
        ''' 这个函数总是返回一个不为空值的字符串
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetString(encoding As Encoding) As String
            Return If(encoding.GetString(ChunkBuffer), "")
        End Function

        Public Function GetIntegers() As Integer()
            Return ChunkBuffer _
                .Split(4) _
                .Select(Function(byts) BitConverter.ToInt32(byts, Scan0)) _
                .ToArray
        End Function

        Public Function GetDoubles() As Double()
            Return ChunkBuffer _
                .Split(8) _
                .Select(Function(byts) BitConverter.ToDouble(byts, Scan0)) _
                .ToArray
        End Function

        ''' <summary>
        ''' 将数据首先生成字符串，然后根据函数指针<paramref name="handler"/>句柄的描述从字符串之中反序列化加载对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="handler"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadObject(Of T)(handler As LoadObject(Of T)) As T
            Return handler(GetUTF8String)
        End Function

        ''' <summary>
        ''' json
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadObject(Of T)() As T
            Return GetUTF8String.LoadJSON(Of T)
        End Function

        ''' <summary>
        ''' 从原始数据流<see cref="ChunkBuffer"/>之中进行反序列化得到一个嵌套的数据串流对象
        ''' </summary>
        ''' <typeparam name="TStream"></typeparam>
        ''' <returns></returns>
        Public Overloads Function GetRawStream(Of TStream As RawStream)() As TStream
            Return GetRawStream(Of TStream)(rawStream:=ChunkBuffer)
        End Function

        Public Overrides Function ToString() As String
            Dim str As String = $"(string) {NameOf(ChunkBuffer)}:={GetUTF8String()};  {TotalBytes()} bytes"
            Return $"{NameOf(ProtocolCategory)}:={ProtocolCategory}; {NameOf(Protocol)}:={Protocol}; {NameOf(BufferLength)}:={BufferLength};  // {str}"
        End Function

        ''' <summary>
        ''' 这个函数是使用json序列化参数信息的
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="category"></param>
        ''' <param name="protocol"></param>
        ''' <param name="params"></param>
        ''' <returns></returns>
        Public Shared Function CreateProtocol(Of T)(category As Long, protocol As Long, params As T) As RequestStream
            Dim json As String = params.GetJson
            Return New RequestStream(category, protocol, json)
        End Function

        ''' <summary>
        ''' 服务器端返回数据所使用的，默认使用json序列化，所有的标签为<see cref="HTTP_RFC.RFC_OK"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Shared Function CreatePackage(Of T)(obj As T) As RequestStream
            Return New RequestStream(HTTP_RFC.RFC_OK, HTTP_RFC.RFC_OK, obj.GetJson)
        End Function

        ''' <summary>
        ''' 服务器端返回数据所使用的，所有的标签为<see cref="HTTP_RFC.RFC_OK"/>
        ''' </summary>
        ''' <param name="pack"></param>
        ''' <returns></returns>
        Public Shared Function CreatePackage(pack As Byte()) As RequestStream
            Return New RequestStream(HTTP_RFC.RFC_OK, HTTP_RFC.RFC_OK, pack)
        End Function

        Private Shared ReadOnly ___offset As Byte() = New Byte() {RequestStream.BITWISE_FLAG}

        Const BITWISE_FLAG As Byte = 255
        Const MIN_LEN As Integer = INT64 + 1 + INT64 + 1 + INT64

        Public Shared Function IsAvaliableStream(stream As Byte()) As Boolean
            If stream.Length < MIN_LEN Then
                Return False
            End If

            If stream(INT64) <> BITWISE_FLAG OrElse stream(INT64 + 1 + INT64) <> BITWISE_FLAG Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' 注意：在协议头部之间还存在着一个字节的offset，这个字节的值为255
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TotalBytes() As Long
            Get
                Return INT64 + 1 + ' ProtocolCategory
                    INT64 + 1 +    ' Protocol
                    INT64 +        ' BufferLength
                    BufferLength   ' ChunkBuffer
            End Get
        End Property

        ''' <summary>
        ''' 执行序列化进行网络之间的数据传输
        ''' </summary>
        Public Overrides Sub Serialize(buffer As Stream)
            Dim protocolCategory As Byte() = BitConverter.GetBytes(Me.ProtocolCategory)
            Dim protocol As Byte() = BitConverter.GetBytes(Me.Protocol)
            Dim bufferSize As Byte() = BitConverter.GetBytes(Me.BufferLength)
            Dim bufs As Byte() = New Byte(TotalBytes - 1) {}
            Dim p As i32 = Scan0
            Dim l As i32 = Scan0

            Call Array.ConstrainedCopy(protocolCategory, Scan0, bufs, Scan0, INT64)
            Call Array.ConstrainedCopy(___offset, Scan0, bufs, INT64, 1)
            Call Array.ConstrainedCopy(protocol, Scan0, bufs, INT64 + 1, INT64)
            Call Array.ConstrainedCopy(___offset, Scan0, bufs, INT64 + 1 + INT64, 1)
            Call Array.ConstrainedCopy(bufferSize, Scan0, bufs, INT64 + 1 + INT64 + 1, INT64)
            Call Array.ConstrainedCopy(ChunkBuffer, Scan0, bufs, INT64 + 1 + INT64 + 1 + INT64, ChunkBuffer.Length)

            Call buffer.Write(bufs, Scan0, bufs.Length)
            Call buffer.Flush()
        End Sub

        Public Sub WriteBuffer(buf As Stream)
            Dim protocolCategory As Byte() = BitConverter.GetBytes(Me.ProtocolCategory)
            Dim protocol As Byte() = BitConverter.GetBytes(Me.Protocol)
            Dim bufferSize As Byte() = BitConverter.GetBytes(Me.BufferLength)

            Call buf.Write(protocolCategory, Scan0, INT64)
            Call buf.Write(___offset, Scan0, ___offset.Length)
            Call buf.Write(protocol, Scan0, INT64)
            Call buf.Write(___offset, Scan0, ___offset.Length)
            Call buf.Write(bufferSize, Scan0, INT64)
            Call buf.Write(ChunkBuffer, Scan0, ChunkBuffer.Length)
            Call buf.Flush()
        End Sub

        ''' <summary>
        ''' 系统里面最基本的基本数据协议
        ''' </summary>
        Public Const SYS_PROTOCOL As Long = -8888888L

        ''' <summary>
        ''' 最基本的Socket数据串流协议
        ''' </summary>
        Public Enum Protocols As Long
            OK = HTTP_RFC.RFC_OK

            Ping = 10L
            ''' <summary>
            ''' 私有密匙加密
            ''' </summary>
            SSL = -107L
            SSLHandshake = -200L
            ''' <summary>
            ''' 公共密匙加密
            ''' </summary>
            SSL_PublicToken = -405L
            ''' <summary>
            ''' 无效的数字证书
            ''' </summary>
            InvalidCertificates = -404L

            NOT_SYS = 0L
        End Enum

        Public Shared Function SystemProtocol(Protocol As Protocols, Message As String) As RequestStream
            Return New RequestStream(SYS_PROTOCOL, Protocol, Message)
        End Function

        Public ReadOnly Property TryGetSystemProtocol As Protocols
            Get
                Try
                    Return CType(Protocol, Protocols)
                Catch ex As Exception
                    Return Protocols.NOT_SYS
                End Try
            End Get
        End Property

        Public ReadOnly Property IsPing As Boolean
            Get
                Return ProtocolCategory = SYS_PROTOCOL AndAlso Protocol = Protocols.Ping
            End Get
        End Property

        ''' <summary>
        ''' 这个请求数据是一个SSL加密数据（使用用户的私有密匙）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsSSLProtocol As Boolean
            Get
                Return ProtocolCategory = SYS_PROTOCOL AndAlso Protocol = Protocols.SSL
            End Get
        End Property

        ''' <summary>
        ''' 使用公共密匙
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsSSL_PublicToken As Boolean
            Get
                Return ProtocolCategory = SYS_PROTOCOL AndAlso Protocol = Protocols.SSL_PublicToken
            End Get
        End Property

        ''' <summary>
        ''' 这个数据仅仅是一个文本，没有包含有任何协议头数据
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsPlantText As Boolean
            Get
                Return ProtocolCategory = Scan0 AndAlso Protocol = Scan0
            End Get
        End Property

        ''' <summary>
        ''' 这个请求数据是否为握手协议
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsSSLHandshaking As Boolean
            Get
                Return ProtocolCategory = SYS_PROTOCOL AndAlso Protocol = Protocols.SSLHandshake
            End Get
        End Property

        ''' <summary>
        ''' 简单的字符串等价
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(a As RequestStream, b As RequestStream) As Boolean
            Return String.Equals(a.GetUTF8String, b.GetUTF8String)
        End Operator

        Public Overloads Shared Operator <>(a As RequestStream, b As RequestStream) As Boolean
            Return Not String.Equals(a.GetUTF8String, b.GetUTF8String)
        End Operator

        Public Overloads Shared Operator =(request As String, requestStream As RequestStream) As Boolean
            Return String.Equals(request, requestStream.GetUTF8String)
        End Operator

        Public Overloads Shared Operator <>(request As String, requestStream As RequestStream) As Boolean
            Return Not String.Equals(request, requestStream.GetUTF8String)
        End Operator

        Public Overloads Shared Operator =(requestStream As RequestStream, request As String) As Boolean
            Return String.Equals(request, requestStream.GetUTF8String)
        End Operator

        Public Overloads Shared Operator <>(requestStream As RequestStream, request As String) As Boolean
            Return Not String.Equals(request, requestStream.GetUTF8String)
        End Operator
    End Class
End Namespace
