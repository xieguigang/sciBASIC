Imports System.Text

''' <summary>
''' 把基准结果渲染成 Markdown 表格。
''' </summary>
Public Module Report

    Private Function Find(results As BenchCases.CaseResult(), name As String) As BenchCases.CaseResult
        For i As Integer = 0 To results.Length - 1
            If results(i).Name = name Then Return results(i)
        Next

        Return New BenchCases.CaseResult()
    End Function

    ''' <summary>
    ''' 生成对比表格：<paramref name="fast"/> 通常为 SIMD + Parallel，<paramref name="baseline"/> 为纯标量串行。
    ''' </summary>
    Public Function BuildMarkdown(fast As BenchCases.CaseResult(), baseline As BenchCases.CaseResult(), title As String) As String
        Dim md As New StringBuilder()

        Call md.AppendLine($"### {title}")
        Call md.AppendLine()
        Call md.AppendLine("| 用例 | SIMD+Parallel (ms) | 纯标量串行 (ms) | 加速比 | 分配 (MB) | 标量分配 (MB) |")
        Call md.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: |")

        For Each item As BenchCases.CaseResult In fast
            Dim slow As BenchCases.CaseResult = Find(baseline, item.Name)
            Dim speedup As String = "-"
            Dim slowMs As String = "-"
            Dim slowAlloc As String = "-"

            If slow.Name IsNot Nothing Then
                slowMs = slow.Milliseconds.ToString("F1")
                slowAlloc = slow.AllocatedMB.ToString("F2")

                If item.Milliseconds > 0 Then
                    speedup = (slow.Milliseconds / item.Milliseconds).ToString("F2") & "x"
                End If
            End If

            Call md.AppendLine($"| {item.Name} | {item.Milliseconds:F1} | {slowMs} | {speedup} | {item.AllocatedMB:F2} | {slowAlloc} |")
        Next

        Call md.AppendLine()

        Return md.ToString()
    End Function

    ''' <summary>
    ''' 控制台输出（非 Markdown 场景）。
    ''' </summary>
    Public Sub PrintConsole(results As BenchCases.CaseResult(), Optional title As String = "")
        If title <> "" Then
            Call Console.WriteLine()
            Call Console.WriteLine($"--- {title} ---")
        End If

        For Each item As BenchCases.CaseResult In results
            Call Console.WriteLine($"{item.Name,-46} {item.Milliseconds,10:F1} ms {item.AllocatedMB,9:F2} MB  {item.Detail}")
        Next
    End Sub
End Module
