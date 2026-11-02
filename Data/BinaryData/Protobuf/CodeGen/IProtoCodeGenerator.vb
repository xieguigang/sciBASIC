Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\CodeGen\IProtoCodeGenerator.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.Data.IO.Protobuf.Schema

Namespace CodeGen

    ''' <summary>
    ''' 从 .proto 描述符树生成目标语言实体类源码的生成器契约。
    ''' </summary>
    Public Interface IProtoCodeGenerator

        ''' <summary>
        ''' 目标语言。
        ''' </summary>
        ReadOnly Property Language As CodeLanguage

        ''' <summary>
        ''' 把整个 .proto 文件生成到单个源码字符串中。
        ''' </summary>
        Function Generate(file As ProtoFile) As String

        ''' <summary>
        ''' 按顶层类型拆分生成多个源文件，返回"文件名 → 源码"的字典。
        ''' </summary>
        Function GenerateFiles(file As ProtoFile) As Dictionary(Of String, String)

        ''' <summary>
        ''' 生成源码并写入指定目录，返回写入的文件绝对路径列表。
        ''' </summary>
        Function GenerateToDirectory(file As ProtoFile, outputDir As String) As IReadOnlyList(Of String)

    End Interface

End Namespace
