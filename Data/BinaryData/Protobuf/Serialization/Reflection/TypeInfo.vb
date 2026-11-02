Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\Reflection\TypeInfo.vb
' /********************************************************************************/

Imports System.Collections
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

Namespace Serialization.Reflection

    ''' <summary>
    ''' 一个消息类型的反射模型：字段列表、字段号索引与实例创建策略。
    ''' </summary>
    ''' <remarks>
    ''' 反射只在本对象构造时发生一次；构造完成后由 <see cref="SerializationContext"/> 缓存复用。
    ''' </remarks>
    Friend Class TypeInfo

        ''' <summary>
        ''' 被建模的 CLR 类型。
        ''' </summary>
        Friend ReadOnly Schema As Type

        ''' <summary>
        ''' 类型上的契约特性；未标注时为 Nothing。
        ''' </summary>
        Friend ReadOnly Contract As ProtoContractAttribute

        ''' <summary>
        ''' 按字段号升序排列的字段模型。
        ''' </summary>
        Friend ReadOnly Fields As SerializableField()

        ''' <summary>
        ''' 该类型是否被识别为 protobuf 消息（至少含一个 <see cref="ProtoMemberAttribute"/>）。
        ''' </summary>
        Friend ReadOnly IsMessage As Boolean

        Private ReadOnly _byNumber As Dictionary(Of Integer, SerializableField)

        Friend Sub New(schema As Type)
            If schema Is Nothing Then
                Throw New ArgumentNullException(NameOf(schema))
            End If

            Me.Schema = schema
            Me.Contract = schema.GetCustomAttribute(Of ProtoContractAttribute)()

            Dim members As New List(Of SerializableField)

            For Each item As PropertyInfo In schema.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                Dim attr As ProtoMemberAttribute = item.GetCustomAttribute(Of ProtoMemberAttribute)()

                If attr Is Nothing Then
                    Continue For
                End If

                If item.GetIndexParameters().Length > 0 Then
                    Throw New InvalidOperationException($"类型 {schema.FullName} 的索引器 {item.Name} 不支持 protobuf 序列化")
                End If

                If Not item.CanRead OrElse Not item.CanWrite Then
                    Throw New InvalidOperationException($"类型 {schema.FullName} 的属性 {item.Name} 标注了 ProtoMember，但缺少可读写的访问器")
                End If

                Call members.Add(New SerializableField(attr, item))
            Next

            For Each item As FieldInfo In schema.GetFields(BindingFlags.Public Or BindingFlags.Instance)
                Dim attr As ProtoMemberAttribute = item.GetCustomAttribute(Of ProtoMemberAttribute)()

                If attr IsNot Nothing Then
                    Call members.Add(New SerializableField(attr, item))
                End If
            Next

            Call members.Sort(Function(a, b) a.FieldNumber.CompareTo(b.FieldNumber))

            Me.Fields = members.ToArray()
            Me.IsMessage = Me.Fields.Length > 0
            Me._byNumber = New Dictionary(Of Integer, SerializableField)(Me.Fields.Length)

            For Each field As SerializableField In Me.Fields
                If _byNumber.ContainsKey(field.FieldNumber) Then
                    Throw New InvalidOperationException($"类型 {schema.FullName} 中字段号 {field.FieldNumber} 被重复使用（{field.Name}）")
                End If

                _byNumber(field.FieldNumber) = field
            Next
        End Sub

        ''' <summary>
        ''' 按字段号查找字段模型；未找到时返回 Nothing。
        ''' </summary>
        Friend Function Lookup(fieldNumber As Integer) As SerializableField
            Dim field As SerializableField = Nothing

            If _byNumber.TryGetValue(fieldNumber, field) Then
                Return field
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 创建该类型的实例。缺少无参构造函数时退化为创建未初始化对象。
        ''' </summary>
        Friend Function CreateInstance() As Object
            If Contract IsNot Nothing AndAlso Contract.SkipConstructor Then
                Return RuntimeHelpers.GetUninitializedObject(Schema)
            End If

            Try
                Return Activator.CreateInstance(Schema)
            Catch ex As MissingMethodException
                Return RuntimeHelpers.GetUninitializedObject(Schema)
            Catch ex As MemberAccessException
                Return RuntimeHelpers.GetUninitializedObject(Schema)
            End Try
        End Function

        ''' <summary>
        ''' 把实例的全部已标注字段（以及保留的未知字段）写入 <paramref name="writer"/>。
        ''' </summary>
        Friend Sub WriteTo(instance As Object, writer As ProtoWriter, context As SerializationContext)
            If instance Is Nothing Then
                Return
            End If

            For Each field As SerializableField In Fields
                Call field.WriteField(instance, writer, context)
            Next

            Dim unknown As UnknownFieldSet = GetUnknownFields(instance, context)

            If unknown IsNot Nothing Then
                Call unknown.WriteTo(writer)
            End If
        End Sub

        ''' <summary>
        ''' 从 <paramref name="reader"/> 读取字段并填充实例。传入已有实例时执行合并语义。
        ''' </summary>
        Friend Function ReadFrom(reader As ProtoReader, existing As Object, context As SerializationContext) As Object
            Dim instance As Object = If(existing Is Nothing, CreateInstance(), existing)
            Dim pending As Dictionary(Of Integer, IList) = Nothing
            Dim unknown As UnknownFieldSet = GetUnknownFields(instance, context)
            Dim fieldNumber As Integer = 0
            Dim wireType As WireType = WireType.Varint

            Do While reader.ReadTag(fieldNumber, wireType)
                Dim field As SerializableField = Lookup(fieldNumber)

                If field Is Nothing Then
                    If unknown IsNot Nothing Then
                        Call unknown.AddFrom(reader, fieldNumber, wireType)
                    Else
                        Call reader.SkipField(wireType)
                    End If
                Else
                    Call field.ReadField(instance, reader, wireType, context, pending)
                End If
            Loop

            If pending IsNot Nothing Then
                Call FlushPending(instance, pending)
            End If

            Return instance
        End Function

        Private Sub FlushPending(instance As Object, pending As Dictionary(Of Integer, IList))
            For Each field As SerializableField In Fields
                Dim list As IList = Nothing

                If pending.TryGetValue(field.FieldNumber, list) Then
                    Call field.SetArrayFromList(instance, list)
                End If
            Next
        End Sub

        Private Shared Function GetUnknownFields(instance As Object, context As SerializationContext) As UnknownFieldSet
            If instance Is Nothing OrElse context.UnknownFieldBehavior <> UnknownFieldBehavior.Preserve Then
                Return Nothing
            End If

            Dim message As IProtoMessage = TryCast(instance, IProtoMessage)

            If message Is Nothing Then
                Return Nothing
            Else
                Return message.UnknownFields
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{Schema.FullName} ({Fields.Length} fields)"
        End Function

    End Class

End Namespace
