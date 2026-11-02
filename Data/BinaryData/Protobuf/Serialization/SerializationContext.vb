Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\SerializationContext.vb
' /********************************************************************************/

Imports ProtoTypeInfo = Microsoft.VisualBasic.Data.IO.Protobuf.Serialization.Reflection.TypeInfo

Namespace Serialization

    ''' <summary>
    ''' 序列化上下文：缓存 <see cref="ProtoTypeInfo"/> 并提供全局策略开关。
    ''' </summary>
    ''' <remarks>
    ''' 类型模型在首次访问时构建并缓存，之后的序列化调用不再做反射扫描。
    ''' 多线程场景下缓存访问由 <c>SyncLock</c> 保护，命中缓存后不持有锁。
    ''' </remarks>
    Public Class SerializationContext

        ''' <summary>
        ''' 全局默认上下文。<c>ProtoSerializer</c> 默认使用该实例。
        ''' </summary>
        Public Shared ReadOnly DefaultContext As New SerializationContext()

        Private ReadOnly _typeInfos As New Dictionary(Of Type, ProtoTypeInfo)
        Private ReadOnly _sync As New Object()

        ''' <summary>
        ''' 允许的最大消息嵌套深度，默认 64。用于防止循环引用或恶意构造的超深递归消息。
        ''' </summary>
        Public Property MaxDepth As Integer = Wire.ProtoWriter.MaxDepth

        ''' <summary>
        ''' 是否省略"默认值"字段（proto3 语义）。默认 True。
        ''' </summary>
        ''' <remarks>
        ''' 数值 0、<c>False</c>、空字符串、空字节数组、空枚举值、<c>DateTime.MinValue</c>、<c>Guid.Empty</c>
        ''' 均视为默认值。设为 False 可以强制写出所有非 Nothing 字段，便于与依赖"字段存在性"的旧实现互通。
        ''' </remarks>
        Public Property SkipDefaults As Boolean = True

        ''' <summary>
        ''' 未知字段的处理策略，默认保留（需要消息类型实现 <see cref="IProtoMessage"/>）。
        ''' </summary>
        Public Property UnknownFieldBehavior As UnknownFieldBehavior = UnknownFieldBehavior.Preserve

        ''' <summary>
        ''' 字符串编解码使用的文本编码（当前固定为 UTF-8，保留该属性以便未来扩展）。
        ''' </summary>
        Public Property Encoding As System.Text.Encoding = System.Text.Encoding.UTF8

        ''' <summary>
        ''' 获取（或构建并缓存）指定类型的字段模型。
        ''' </summary>
        Friend Function GetTypeInfo(type As Type) As ProtoTypeInfo
            If type Is Nothing Then
                Throw New ArgumentNullException(NameOf(type))
            End If

            SyncLock _sync
                Dim info As ProtoTypeInfo = Nothing

                If Not _typeInfos.TryGetValue(type, info) Then
                    info = New ProtoTypeInfo(type)
                    _typeInfos(type) = info
                End If

                Return info
            End SyncLock
        End Function

        ''' <summary>
        ''' 获取指定类型的字段模型。
        ''' </summary>
        Friend Function GetMessageModel(Of T)() As ProtoTypeInfo
            Return GetTypeInfo(GetType(T))
        End Function

        ''' <summary>
        ''' 清空已缓存的类型模型。
        ''' </summary>
        Public Sub Reset()
            SyncLock _sync
                Call _typeInfos.Clear()
            End SyncLock
        End Sub

        Public Overrides Function ToString() As String
            Return $"SerializationContext (cached={_typeInfos.Count}, skipDefaults={SkipDefaults}, unknown={UnknownFieldBehavior})"
        End Function

    End Class

End Namespace
