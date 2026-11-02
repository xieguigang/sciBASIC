Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\CodeGen\ProtoCodeGenerator.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.Data.IO.Protobuf.Schema

Namespace CodeGen

    ''' <summary>
    ''' 代码生成器门面：按目标语言创建生成器，或一步完成"解析 .proto → 生成源码"。
    ''' </summary>
    Public Module ProtoCodeGenerator

        ''' <summary>
        ''' 按目标语言创建代码生成器。
        ''' </summary>
        Public Function Create(language As CodeLanguage) As IProtoCodeGenerator
            Select Case language
                Case CodeLanguage.CSharp
                    Return New CSharpCodeGenerator()
                Case Else
                    Return New VBCodeGenerator()
            End Select
        End Function

        ''' <summary>
        ''' 把描述符树生成到单个源码字符串。
        ''' </summary>
        Public Function Generate(file As ProtoFile, language As CodeLanguage) As String
            Return Create(language).Generate(file)
        End Function

        ''' <summary>
        ''' 按顶层类型拆分生成多个源文件。
        ''' </summary>
        Public Function GenerateFiles(file As ProtoFile, language As CodeLanguage) As Dictionary(Of String, String)
            Return Create(language).GenerateFiles(file)
        End Function

        ''' <summary>
        ''' 生成源码并写入目录，返回写入的文件路径列表。
        ''' </summary>
        Public Function GenerateToDirectory(file As ProtoFile, outputDir As String,
                                            language As CodeLanguage) As IReadOnlyList(Of String)

            Return Create(language).GenerateToDirectory(file, outputDir)
        End Function

        ''' <summary>
        ''' 直接从 .proto 文本生成源码。
        ''' </summary>
        Public Function GenerateFromProtoText(protoText As String,
                                              language As CodeLanguage,
                                              Optional fileName As String = Nothing) As String

            Dim file As ProtoFile = ProtoFile.LoadText(protoText, fileName)

            Return Generate(file, language)
        End Function

    End Module

End Namespace
