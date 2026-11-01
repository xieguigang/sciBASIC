Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' xgboost 回归 / 基准测试入口（唯一入口）。
'''
''' 依次执行：
''' 1. 推理回归（真实 XGBoost 模型 + 期望输出逐值比对）
''' 2. 训练回归（收敛、早停回滚、序列化往返、类别保真、可复现性、exact/hist 一致性）
''' 3. 性能基准（exact / hist / 串并行 / 预测吞吐）
'''
''' 所有断言与度量通过 <see cref="DebugAudit"/> 记录，最后输出 JSON 报告。
''' </summary>
Module Program

    Public Function Main(args As String()) As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, "xgboost-test-report.json")

        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 16384

        Console.WriteLine("==================================================")
        Console.WriteLine(" xgboost 回归 / 基准测试")
        Console.WriteLine($" 资源目录: {TestSupport.ResourcesRoot}")
        Console.WriteLine("==================================================")

        Call runSuite("推理回归", AddressOf PredictorRegression.Run)
        Call runSuite("训练回归", AddressOf TrainingRegression.Run)
        Call runSuite("性能基准", AddressOf Benchmark.Run)

        Call DebugAudit.SaveReport(reportPath)

        Console.WriteLine()
        Console.WriteLine("==================================================")
        Console.WriteLine($" 失败断言数: {DebugAudit.FailureCount}")
        Console.WriteLine($" 测试报告:   {reportPath}")
        Console.WriteLine("==================================================")

        Return DebugAudit.FailureCount
    End Function

    ''' <summary>
    ''' 执行一组用例：单组异常不会中断后续用例，但会被记录为失败断言
    ''' </summary>
    Private Sub runSuite(name As String, action As Action)
        Try
            Call action()
        Catch ex As Exception
            Call DebugAudit.Mark($"crash:{name}", ex.Message)
            Call DebugAudit.Check(False, $"{name} 抛出异常：{ex.GetType().Name}: {ex.Message}")
            Call Console.Error.WriteLine($"[{name}] 异常: {ex}")
        End Try
    End Sub
End Module
