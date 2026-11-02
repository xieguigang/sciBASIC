Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\ServiceDescriptor.vb
' /********************************************************************************/

Namespace Schema

    ''' <summary>
    ''' .proto <c>service</c> 中的一个 rpc 方法。
    ''' </summary>
    Public Class MethodDescriptor

        Public Property Name As String

        ''' <summary>
        ''' 请求消息类型名（.proto 中的原始写法）。
        ''' </summary>
        Public Property InputType As String

        ''' <summary>
        ''' 响应消息类型名。
        ''' </summary>
        Public Property OutputType As String

        ''' <summary>
        ''' 请求是否为客户端流。
        ''' </summary>
        Public Property ClientStreaming As Boolean

        ''' <summary>
        ''' 响应是否为服务端流。
        ''' </summary>
        Public Property ServerStreaming As Boolean

        Public Overrides Function ToString() As String
            Return $"rpc {Name}({InputType}) returns ({OutputType})"
        End Function

    End Class

    ''' <summary>
    ''' 描述 .proto 文件中的一个 service。本库不生成 RPC 存根，仅保留描述信息。
    ''' </summary>
    Public Class ServiceDescriptor

        Public Property Name As String

        Public Property Package As String = ""

        Public ReadOnly Property Methods As New List(Of MethodDescriptor)

        Public ReadOnly Property FullName As String
            Get
                If Not String.IsNullOrEmpty(Package) Then
                    Return $"{Package}.{Name}"
                Else
                    Return Name
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"service {FullName} ({Methods.Count} methods)"
        End Function

    End Class

End Namespace
