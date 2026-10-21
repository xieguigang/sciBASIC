Imports System
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities

''' <summary>
''' Apriori 关联规则的非交互测试入口（本工程的 StartupObject）
''' 
''' ```bash
''' dotnet run --project test/test.vbproj -- selftest
''' dotnet run --project test/test.vbproj -- bench
''' dotnet run --project test/test.vbproj -- bench --legacy
''' ```
''' </summary>
Module AprioriTest

    Function Main(args As String()) As Integer
        ' 测试程序只关心汇总结果，静音掉两侧的进度输出
        Microsoft.VisualBasic.VBDebugger.Mute = True

        Dim command As String = If(args IsNot Nothing AndAlso args.Length > 0, args(0), "selftest").ToLower()
        Dim withLegacy As Boolean = args IsNot Nothing AndAlso (Array.IndexOf(args, "--legacy") >= 0 OrElse Array.IndexOf(args, "--legacy-all") >= 0)
        Dim legacyAll As Boolean = args IsNot Nothing AndAlso Array.IndexOf(args, "--legacy-all") >= 0

        If command = "bench" Then
            Console.WriteLine("running apriori benchmarks...")
            Call AprioriBench.Run(withLegacy, legacyAll)
            Return 0
        End If



        If command = "hamming" Then
            Console.WriteLine("running hamming clustering checks...")

            Dim hammingFailures As New List(Of String)()
            Dim hammingChecks As Integer = HammingSelfTest.Run(hammingFailures)

            If hammingFailures.Count > 0 Then
                Console.WriteLine("FAILED:")

                For Each msg As String In hammingFailures
                    Console.WriteLine("  - " & msg)
                Next

                Return 1
            End If

            Console.WriteLine($"all {hammingChecks} hamming clustering checks passed.")

            Return 0
        End If

        If command <> "selftest" Then
            Console.WriteLine("unknown command: " & command)
            Console.WriteLine("usage: Testing selftest | hamming | bench [--legacy]")
            Return 2
        End If

        Console.WriteLine("running apriori equivalence checks (new engine vs. original implementation)...")

        Dim failures As New List(Of String)()

        Call AprioriSelfTest.Run(failures)

        If failures.Count > 0 Then
            Console.WriteLine("FAILED:")

            For Each msg As String In failures
                Console.WriteLine("  - " & msg)
            Next

            Return 1
        End If

        Console.WriteLine("all apriori equivalence checks passed.")

        Return 0
    End Function
End Module
