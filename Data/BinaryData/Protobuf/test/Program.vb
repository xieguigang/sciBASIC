Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf.test
'  Data\BinaryData\Protobuf\test\Program.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' protobuf 库的 demo 测试入口：依次运行 Wire / 序列化 / 解析 / 代码生成四组用例，
''' 使用 <see cref="DebugAudit"/> 记录结果并输出 JSON 报告。
''' </summary>
Module Program

    Sub Main(args As String())
        Call DebugAudit.Reset()

        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, "protobuf-test-report.json")

        Console.WriteLine("Microsoft.VisualBasic.Data.IO.Protobuf demo")
        Console.WriteLine($"运行目录：{AppContext.BaseDirectory}")
        Console.WriteLine()

        Try
            Using DebugAudit.Watch("suite:wire")
                Call WireTests.Run()
            End Using

            Using DebugAudit.Watch("suite:serialization")
                Call SerializationTests.Run()
            End Using

            Using DebugAudit.Watch("suite:schema")
                Call SchemaTests.Run()
            End Using

            Using DebugAudit.Watch("suite:codegen")
                Call CodeGenTests.Run()
            End Using
        Catch ex As Exception
            Call DebugAudit.Mark("fatal", ex.ToString())
            Call Console.WriteLine(ex.ToString())
        End Try

        Console.WriteLine()
        Console.WriteLine(DebugAudit.Dump())

        Call DebugAudit.SaveReport(reportPath)
        Console.WriteLine($"测试报告：{reportPath}")

        If DebugAudit.FailureCount > 0 Then
            Console.WriteLine($"共 {DebugAudit.FailureCount} 项断言失败。")
            Environment.ExitCode = 1
        Else
            Console.WriteLine("全部断言通过。")
        End If
    End Sub

End Module
