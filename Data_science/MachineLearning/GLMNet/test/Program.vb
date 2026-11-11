Imports System
Imports GLMNet

''' <summary>
''' glmnet 5.1 复刻的自检入口：用 DebugAudit 记录对拍结果并生成 JSON 报告。
''' </summary>
Module Program

    Sub Main(args As String())
        Dim failures As Integer = GlmnetAudit.Run()

        Console.WriteLine($"glmnet audit finished with {failures} failure(s).")
        Console.WriteLine($"report: {GlmnetAudit.ReportPath}")
        Environment.ExitCode = If(failures > 0, 1, 0)
    End Sub

End Module
