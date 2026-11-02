Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\ProtoContractAttribute.vb
' /********************************************************************************/

Namespace Serialization

    ''' <summary>
    ''' 标记一个类或结构体为 protobuf 消息契约。
    ''' </summary>
    ''' <remarks>
    ''' 该标记本身不是必需的：只要类型上存在至少一个 <see cref="ProtoMemberAttribute"/> 标注的成员，
    ''' 就会被识别为消息类型。显式标注可以额外指定契约名称或跳过构造函数等行为。
    ''' </remarks>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct Or AttributeTargets.Interface,
                    AllowMultiple:=False, Inherited:=True)>
    Public Class ProtoContractAttribute : Inherits Attribute

        ''' <summary>
        ''' 契约名称；Nothing 表示使用 CLR 类型名。该名称会出现在错误信息中，不参与编码。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 反序列化时是否跳过构造函数，直接创建未初始化对象。
        ''' </summary>
        ''' <remarks>
        ''' 设为 True 时使用 <c>RuntimeHelpers.GetUninitializedObject</c> 创建实例，
        ''' 适用于没有无参构造函数、或构造函数带有副作用的类型。此时字段的默认值不会被构造函数改写。
        ''' </remarks>
        Public Property SkipConstructor As Boolean

    End Class

End Namespace
