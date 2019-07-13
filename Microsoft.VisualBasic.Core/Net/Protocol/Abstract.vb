#Region "Microsoft.VisualBasic::6f35238d4e079cd2d617b33b2fcecd44, Microsoft.VisualBasic.Core\Net\Protocol\Abstract.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Interface ISerializable
    ' 
    '         Function: Serialize
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Class RawStream
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetRawStream
    '         Operators: <=, >=
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Tcp

Namespace Net.Protocols

    ''' <summary>
    ''' 支持序列化的对象，则这个对象可以被应用于<see cref="RequestStream"/>数据载体的网络传输操作过程
    ''' </summary>
    Public Interface ISerializable
        ''' <summary>
        ''' Transform this .NET object into a raw stream object for the network data transfer. 
        ''' </summary>
        ''' <returns></returns>
        Function Serialize() As Byte()
    End Interface

    Public Delegate Sub ProcessMessagePush(message As RequestStream)

    ''' <summary>
    ''' 原始串流的基本模型，这个流对象应该具备有两个基本的方法：
    ''' 1. 从原始的字节流之中反序列化构造出自身的构造函数
    ''' 2. 将自身序列化为字节流的<see cref="ISerializable.Serialize()"/>序列化方法
    ''' </summary>
    <Serializable> Public MustInherit Class RawStream : Implements ISerializable

        ''' <summary>
        ''' You should overrides this constructor to generate a stream object.(必须要有一个这个构造函数来执行反序列化)
        ''' </summary>
        ''' <param name="rawStream"></param>
        Sub New(rawStream As Byte())

        End Sub

        Public Sub New()
        End Sub

        ''' <summary>
        ''' <see cref="ISerializable.Serialize"/>序列化方法
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function Serialize() As Byte() Implements ISerializable.Serialize

        ''' <summary>
        ''' 按照类型的定义进行反序列化操作
        ''' </summary>
        ''' <typeparam name="TRawStream"></typeparam>
        ''' <param name="rawStream"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetRawStream(Of TRawStream As RawStream)(rawStream As Byte()) As TRawStream
            Return Activator.CreateInstance(GetType(TRawStream), {rawStream})
        End Function

        Protected Shared ReadOnly _rawStreamType As Type = GetType(Byte())

        Public Const INT64 As Integer = 8
        ''' <summary>
        ''' Single/Integer
        ''' </summary>
        Public Const INT32 As Integer = 4
        ''' <summary>
        ''' System.Double
        ''' </summary>
        Public Const DblFloat As Integer = 8
        Public Const ShortInt As Integer = 2
        Public Const SingleFloat As Integer = 4
        Public Const DecimalInt As Integer = 12

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="addr">IPEndPoint string value likes 127.0.0.1:8080</param>
        ''' <param name="raw"></param>
        ''' <returns></returns>
        ''' <![CDATA[
        '''
        ''' Dim rep As RequestStream = "127.0.0.1:80" <= New RequestStream With {
        '''     ...
        ''' }
        ''' ]]>
        Public Shared Operator <=(addr As String, raw As RawStream) As RequestStream
            Dim ep As New IPEndPoint(addr)
            Dim invoke As New TcpRequest(ep)
            Dim rep As New RequestStream(invoke.SendMessage(raw.Serialize))
            Return rep
        End Operator

        Public Shared Operator >=(addr As String, raw As RawStream) As RequestStream
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace
