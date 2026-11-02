Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' GCN 演示与回归测试的唯一入口。
''' </summary>
''' <remarks>
''' <para>
''' 本工程原先的入口是 <see cref="TemporalGNNDemo"/>.Main（时序图 demo），加入 GCN 用例后
''' 通过 &lt;StartupObject&gt; 把入口改到本模块，原 demo 仍保留、可单独调用。
''' </para>
''' <para>
''' 断言统一走 <see cref="DebugAudit"/>：单组用例抛异常不会中断后续用例，
''' 但会被记录为一条失败断言；结束时把全部度量与断言渲染成 JSON 报告。
''' 注意 <c>DebugAudit.Check</c> 带 <c>&lt;Conditional("DEBUG")&gt;</c>，
''' 必须以 Debug 配置运行（<c>dotnet run -c Debug</c>）才会产生断言记录。
''' </para>
''' </remarks>
Module Program

    ''' <summary>报告文件名（写到程序输出目录）</summary>
    Private Const ReportFileName As String = "gcn-test-report.json"

    ''' <summary>入口：依次跑完全部用例并输出报告。</summary>
    Public Function Main(args As String()) As Integer
        Dim reportPath = Path.Combine(AppContext.BaseDirectory, ReportFileName)

        Call DebugAudit.Reset()

        DebugAudit.MaxEvents = 16384

        Console.WriteLine(New String("="c, 72))
        Console.WriteLine(" GCN 图卷积神经网络 —— 演示与回归测试")
        Console.WriteLine($" 目标框架: {Environment.Version}")
        Console.WriteLine($" 测试报告: {reportPath}")
        Console.WriteLine(New String("="c, 72))

        Call runSuite("归一化公式（readme 4 节点例子）", AddressOf GCNTestSuite.RunNormalizationExample)
        Call runSuite("Karate Club 半监督节点分类", AddressOf GCNTestSuite.RunKarateClub)
        Call runSuite("合成 SBM 半监督节点分类", AddressOf GCNTestSuite.RunSyntheticSbm)
        Call runSuite("稠密 vs 稀疏传播", AddressOf GCNTestSuite.RunDenseSparseParity)
        Call runSuite("混合精度路由", AddressOf GCNTestSuite.RunPrecisionRouting)
        Call runSuite("早停与权重回滚", AddressOf GCNTestSuite.RunEarlyStoppingRollback)

        Call DebugAudit.SaveReport(reportPath)

        Console.WriteLine()
        Console.WriteLine(New String("="c, 72))
        Console.WriteLine($" 失败断言数: {DebugAudit.FailureCount}")
        Console.WriteLine($" 测试报告:   {reportPath}")
        Console.WriteLine(New String("="c, 72))

        Return DebugAudit.FailureCount
    End Function

    ''' <summary>
    ''' 执行一组用例：单组异常不会中断后续用例，但会被记录为失败断言。
    ''' </summary>
    Private Sub runSuite(name As String, action As Action)
        Console.WriteLine()
        Console.WriteLine($"【{name}】")

        Try
            Call action()
        Catch ex As Exception
            Call DebugAudit.Mark($"crash:{name}", ex.Message)
            Call DebugAudit.Check(False, $"{name} 抛出异常：{ex.GetType().Name}: {ex.Message}")
            Call Console.Error.WriteLine($"[{name}] 异常: {ex}")
        End Try
    End Sub

End Module
