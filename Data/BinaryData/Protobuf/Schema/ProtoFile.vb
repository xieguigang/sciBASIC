Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\ProtoFile.vb
' /********************************************************************************/

Imports System.IO

Namespace Schema

    ''' <summary>
    ''' .proto 文件的语法版本。
    ''' </summary>
    Public Enum ProtoSyntax

        ''' <summary>
        ''' 未显式声明 <c>syntax</c>，按 proto2 处理。
        ''' </summary>
        Proto2 = 2

        ''' <summary>
        ''' <c>syntax = "proto3";</c>
        ''' </summary>
        Proto3 = 3

    End Enum

    ''' <summary>
    ''' 一个已解析的 .proto 文件：描述符树的根节点。
    ''' </summary>
    Public Class ProtoFile

        ''' <summary>
        ''' 源文件名；从字符串解析时为 Nothing。
        ''' </summary>
        Public Property SourceFile As String

        ''' <summary>
        ''' 语法版本。
        ''' </summary>
        Public Property Syntax As ProtoSyntax = ProtoSyntax.Proto3

        ''' <summary>
        ''' package 名称；未声明时为空字符串。
        ''' </summary>
        Public Property Package As String = ""

        ''' <summary>
        ''' <c>import</c> 声明的文件名列表（仅记录，不自动加载）。
        ''' </summary>
        Public ReadOnly Property [Imports] As New List(Of String)

        ''' <summary>
        ''' 文件级 <c>option</c>。
        ''' </summary>
        Public ReadOnly Property Options As New Dictionary(Of String, String)

        ''' <summary>
        ''' 顶层消息。
        ''' </summary>
        Public ReadOnly Property Messages As New List(Of MessageDescriptor)

        ''' <summary>
        ''' 顶层枚举。
        ''' </summary>
        Public ReadOnly Property Enums As New List(Of EnumDescriptor)

        ''' <summary>
        ''' 服务定义。
        ''' </summary>
        Public ReadOnly Property Services As New List(Of ServiceDescriptor)

        ''' <summary>
        ''' 未能解析到定义的引用类型名。类型解析采用"尽力而为"策略，失败不会中断解析。
        ''' </summary>
        Public ReadOnly Property UnresolvedTypes As New List(Of String)

        ''' <summary>
        ''' 从 .proto 文本解析。
        ''' </summary>
        ''' <param name="text">.proto 源文本。</param>
        ''' <param name="fileName">可选的源文件名，仅用于错误信息与生成代码的头部注释。</param>
        Public Shared Function LoadText(text As String, Optional fileName As String = Nothing) As ProtoFile
            Return ProtoParser.Parse(text, fileName)
        End Function

        ''' <summary>
        ''' 从磁盘上的 .proto 文件解析。
        ''' </summary>
        Public Shared Function Load(path As String) As ProtoFile
            Return ProtoParser.ParseFile(path)
        End Function

        ''' <summary>
        ''' 深度优先枚举全部消息（含嵌套消息）。
        ''' </summary>
        Public Iterator Function AllMessages() As IEnumerable(Of MessageDescriptor)
            For Each message As MessageDescriptor In Messages
                For Each item As MessageDescriptor In message.AllMessages()
                    Yield item
                Next
            Next
        End Function

        ''' <summary>
        ''' 深度优先枚举全部枚举（含嵌套枚举）。
        ''' </summary>
        Public Iterator Function AllEnums() As IEnumerable(Of EnumDescriptor)
            For Each item As EnumDescriptor In Enums
                Yield item
            Next

            For Each message As MessageDescriptor In AllMessages()
                For Each item As EnumDescriptor In message.NestedEnums
                    Yield item
                Next
            Next
        End Function

        ''' <summary>
        ''' 按全名查找消息。
        ''' </summary>
        Public Function FindMessageByFullName(fullName As String) As MessageDescriptor
            Return AllMessages().FirstOrDefault(
                Function(m) String.Equals(m.FullName, fullName, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' 按全名查找枚举。
        ''' </summary>
        Public Function FindEnumByFullName(fullName As String) As EnumDescriptor
            Return AllEnums().FirstOrDefault(
                Function(e) String.Equals(e.FullName, fullName, StringComparison.Ordinal))
        End Function

        Public Overrides Function ToString() As String
            Return $"proto{CLng(Syntax)} file '{If(SourceFile, "<memory>")}' (package={Package}, messages={Messages.Count})"
        End Function

    End Class

End Namespace
