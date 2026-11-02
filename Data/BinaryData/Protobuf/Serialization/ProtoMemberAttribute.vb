Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\ProtoMemberAttribute.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

Namespace Serialization

    ''' <summary>
    ''' 标记一个属性或字段参与 protobuf 序列化，并指定其字段号。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 字段号在同一个消息类型内必须唯一，取值范围为 1..536870911，
    ''' 且不得落在 19000..19999（protobuf 内部保留区间）。
    ''' </para>
    ''' <para>
    ''' 未标注本特性的公开成员不会被序列化。
    ''' </para>
    ''' </remarks>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field,
                    AllowMultiple:=False, Inherited:=True)>
    Public Class ProtoMemberAttribute : Inherits Attribute

        Private ReadOnly _fieldNumber As Integer

        ''' <summary>
        ''' 声明某个成员使用指定的字段号参与序列化。
        ''' </summary>
        ''' <param name="fieldNumber">字段号，1..536870911，且不能落在 19000..19999 保留区间。</param>
        Public Sub New(fieldNumber As Integer)
            If fieldNumber < 1 OrElse fieldNumber > ProtoWireFormat.MaxFieldNumber Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(fieldNumber), fieldNumber,
                    $"非法的 protobuf 字段号：有效范围为 1..{ProtoWireFormat.MaxFieldNumber}")
            End If

            If fieldNumber >= ProtoWireFormat.MinReservedFieldNumber AndAlso
               fieldNumber <= ProtoWireFormat.MaxReservedFieldNumber Then

                Throw New ArgumentOutOfRangeException(
                    NameOf(fieldNumber), fieldNumber,
                    $"字段号 {fieldNumber} 落在 protobuf 保留区间 {ProtoWireFormat.MinReservedFieldNumber}..{ProtoWireFormat.MaxReservedFieldNumber} 内")
            End If

            _fieldNumber = fieldNumber
        End Sub

        ''' <summary>
        ''' 字段号。
        ''' </summary>
        Public ReadOnly Property FieldNumber As Integer
            Get
                Return _fieldNumber
            End Get
        End Property

        ''' <summary>
        ''' 字段名；Nothing 表示使用 CLR 成员名。该名称仅用于诊断信息。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 该字段是否必须写出。默认 False（proto3 语义：值为默认值时省略）。
        ''' </summary>
        Public Property IsRequired As Boolean

        ''' <summary>
        ''' 覆盖 repeated 数值字段的 packed 判定。Nothing 表示按类型自动判定
        ''' （数值 / 枚举类型的 repeated 字段默认 packed）。
        ''' </summary>
        Public Property IsPacked As Boolean?

        ''' <summary>
        ''' 数值编码方式。默认 <see cref="ProtoDataFormat.Auto"/>，即按 CLR 类型自动推导。
        ''' </summary>
        Public Property DataFormat As ProtoDataFormat = ProtoDataFormat.Auto

        ''' <summary>
        ''' 缺省值（仅用于文档与代码生成，反序列化时以 CLR 类型默认值为准）。
        ''' </summary>
        Public Property DefaultValue As Object

        ''' <summary>
        ''' 显式声明该成员为 map 字段（当成员类型为 IDictionary 时会自动识别，通常无需设置）。
        ''' </summary>
        Public Property IsMap As Boolean

    End Class

End Namespace
