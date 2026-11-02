Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\MessageDescriptor.vb
' /********************************************************************************/

Namespace Schema

    ''' <summary>
    ''' 描述 .proto 文件中的一个 message（可嵌套）。
    ''' </summary>
    Public Class MessageDescriptor

        ''' <summary>
        ''' 消息名（不含 package 与父级前缀）。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 所属 package。
        ''' </summary>
        Public Property Package As String = ""

        ''' <summary>
        ''' 父级消息；顶层消息为 Nothing。
        ''' </summary>
        Public Property Parent As MessageDescriptor

        ''' <summary>
        ''' 字段列表（按在 .proto 中的声明顺序）。
        ''' </summary>
        Public ReadOnly Property Fields As New List(Of FieldDescriptor)

        ''' <summary>
        ''' 嵌套消息。
        ''' </summary>
        Public ReadOnly Property NestedMessages As New List(Of MessageDescriptor)

        ''' <summary>
        ''' 嵌套枚举。
        ''' </summary>
        Public ReadOnly Property NestedEnums As New List(Of EnumDescriptor)

        ''' <summary>
        ''' oneof 组名列表，下标即 <see cref="FieldDescriptor.OneofIndex"/>。
        ''' </summary>
        Public ReadOnly Property Oneofs As New List(Of String)

        ''' <summary>
        ''' <c>reserved</c> 声明的原文（数字区间或名称）。
        ''' </summary>
        Public ReadOnly Property ReservedRanges As New List(Of String)

        ''' <summary>
        ''' 该消息是否为编译器为 <c>map&lt;K,V&gt;</c> 合成的 entry 消息。
        ''' </summary>
        Public Property MapEntry As Boolean

        ''' <summary>
        ''' 消息全名（包含 package 与父级消息路径）。
        ''' </summary>
        Public ReadOnly Property FullName As String
            Get
                If Parent IsNot Nothing Then
                    Return $"{Parent.FullName}.{Name}"
                ElseIf Not String.IsNullOrEmpty(Package) Then
                    Return $"{Package}.{Name}"
                Else
                    Return Name
                End If
            End Get
        End Property

        ''' <summary>
        ''' 添加一个字段，并自动回填 <see cref="FieldDescriptor.Parent"/>。
        ''' </summary>
        Public Function AddField(field As FieldDescriptor) As FieldDescriptor
            If field Is Nothing Then
                Throw New ArgumentNullException(NameOf(field))
            End If

            field.Parent = Me
            Call Fields.Add(field)

            Return field
        End Function

        ''' <summary>
        ''' 按字段名查找字段。
        ''' </summary>
        Public Function FindField(name As String) As FieldDescriptor
            Return Fields.FirstOrDefault(Function(f) String.Equals(f.Name, name, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' 按字段号查找字段。
        ''' </summary>
        Public Function FindFieldByNumber(number As Integer) As FieldDescriptor
            Return Fields.FirstOrDefault(Function(f) f.Number = number)
        End Function

        ''' <summary>
        ''' 按名称查找直接嵌套的消息。
        ''' </summary>
        Public Function FindNestedMessage(name As String) As MessageDescriptor
            Return NestedMessages.FirstOrDefault(Function(m) String.Equals(m.Name, name, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' 按名称查找直接嵌套的枚举。
        ''' </summary>
        Public Function FindNestedEnum(name As String) As EnumDescriptor
            Return NestedEnums.FirstOrDefault(Function(e) String.Equals(e.Name, name, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' 深度优先枚举本消息以及全部嵌套消息。
        ''' </summary>
        Public Iterator Function AllMessages() As IEnumerable(Of MessageDescriptor)
            Yield Me

            For Each child As MessageDescriptor In NestedMessages
                For Each nested As MessageDescriptor In child.AllMessages()
                    Yield nested
                Next
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"message {FullName} ({Fields.Count} fields)"
        End Function

    End Class

End Namespace
