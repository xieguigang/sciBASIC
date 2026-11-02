Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\EnumDescriptor.vb
' /********************************************************************************/

Namespace Schema

    ''' <summary>
    ''' .proto 枚举中的一个取值。
    ''' </summary>
    Public Class EnumValueDescriptor

        Public Property Name As String

        Public Property Number As Integer

        Public Overrides Function ToString() As String
            Return $"{Name} = {Number}"
        End Function

    End Class

    ''' <summary>
    ''' 描述 .proto 文件中的一个 enum（可嵌套在 message 内）。
    ''' </summary>
    Public Class EnumDescriptor

        ''' <summary>
        ''' 枚举名（不含 package 与父级前缀）。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 所属 package。
        ''' </summary>
        Public Property Package As String = ""

        ''' <summary>
        ''' 父级消息（嵌套枚举）；顶层枚举为 Nothing。
        ''' </summary>
        Public Property Parent As MessageDescriptor

        ''' <summary>
        ''' 是否允许同名不同值的别名（<c>option allow_alias = true;</c>）。
        ''' </summary>
        Public Property AllowAlias As Boolean

        Public ReadOnly Property Values As New List(Of EnumValueDescriptor)

        ''' <summary>
        ''' 枚举全名。
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

        Public Function AddValue(name As String, number As Integer) As EnumValueDescriptor
            Dim item As New EnumValueDescriptor With {.Name = name, .Number = number}

            Call Values.Add(item)

            Return item
        End Function

        Public Function FindValue(name As String) As EnumValueDescriptor
            Return Values.FirstOrDefault(Function(v) String.Equals(v.Name, name, StringComparison.Ordinal))
        End Function

        Public Function FindValue(number As Integer) As EnumValueDescriptor
            Return Values.FirstOrDefault(Function(v) v.Number = number)
        End Function

        Public Overrides Function ToString() As String
            Return $"enum {FullName} ({Values.Count} values)"
        End Function

    End Class

End Namespace
