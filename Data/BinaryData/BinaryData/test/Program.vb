Option Strict On
Option Explicit On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Snappy demo
'  Data\BinaryData\BinaryData\test\Program.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' Snappy 库的控制台测试入口：依次运行原始块与分块流用例，
''' 使用 <see cref="DebugAudit"/> 记录结果并输出 JSON 报告。
''' </summary>
Module Program

    Sub Main(args As String())
        Call DebugAudit.Reset()
        Call TestSupport.ResetChecks()

        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, "snappy-test-report.json")

        Console.WriteLine("Microsoft.VisualBasic.Data.IO.Snappy demo")
        Console.WriteLine($"运行目录：{AppContext.BaseDirectory}")
        Console.WriteLine()

        Try
            Call RunSuite("suite:raw-vectors", AddressOf SnappyRawTests.Vectors)
            Call RunSuite("suite:raw-roundtrip", AddressOf SnappyRawTests.RoundTrip)
            Call RunSuite("suite:raw-errors", AddressOf SnappyRawTests.Errors)
            Call RunSuite("suite:framed", AddressOf SnappyFramedTests.RoundTripAndChunks)
            Call RunSuite("suite:framed-errors", AddressOf SnappyFramedTests.Errors)
        Catch ex As Exception
            Call DebugAudit.Mark("fatal", ex.ToString())
            Call Console.WriteLine(ex.ToString())
        End Try

        Call DebugAudit.Mark("assertions:total", TestSupport.Checks)

        Console.WriteLine()
        Console.WriteLine(DebugAudit.Dump())

        Call DebugAudit.SaveReport(reportPath)
        Console.WriteLine($"测试报告：{reportPath}")
        Console.WriteLine($"断言总数：{TestSupport.Checks}")

        If DebugAudit.FailureCount > 0 Then
            Console.WriteLine($"共 {DebugAudit.FailureCount} 项断言失败。")
            Environment.ExitCode = 1
        Else
            Console.WriteLine("全部断言通过。")
        End If
    End Sub

    ''' <summary>
    ''' 运行一组用例，并把该组的断言数量记录到审计报告中。
    ''' </summary>
    Private Sub RunSuite(name As String, body As Action)
        Dim baseline As Integer = TestSupport.Checks

        Using DebugAudit.Watch(name)
            Call body()
        End Using

        Call DebugAudit.Mark(name & ":assertions", TestSupport.Checks - baseline)
    End Sub

End Module
