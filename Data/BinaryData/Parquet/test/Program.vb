Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.DataStorage.Parquet.test
'  Data\BinaryData\Parquet\test\Program.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' Parquet 库的 demo 测试入口：依次运行扁平表 / 压缩 / 编码 / 嵌套 / 互操作五组用例，
''' 使用 <see cref="DebugAudit"/> 记录结果并输出 JSON 报告。
''' </summary>
Module Program

    Sub Main(args As String())
        Call DebugAudit.Reset()
        Call TestSupport.ResetChecks()

        Dim reportPath As String = System.IO.Path.Combine(AppContext.BaseDirectory, "parquet-test-report.json")

        Console.WriteLine("Microsoft.VisualBasic.DataStorage.Parquet demo")
        Console.WriteLine($"运行目录：{AppContext.BaseDirectory}")
        Console.WriteLine()

        Call RunQuickDemo()

        Try
            Call RunSuite("suite:flat", AddressOf FlatTableTests.Run)
            Call RunSuite("suite:flat-multi-row-group", AddressOf FlatTableTests.RunMultipleRowGroups)
            Call RunSuite("suite:flat-boundary", AddressOf FlatTableTests.RunRowGroupBoundary)
            Call RunSuite("suite:flat-schema", AddressOf FlatTableTests.RunSchemaAndStatistics)
            Call RunSuite("suite:flat-edge", AddressOf FlatTableTests.RunEdgeCases)
            Call RunSuite("suite:compression", AddressOf CompressionTests.Run)
            Call RunSuite("suite:compression-ratio", AddressOf CompressionTests.RunCompressionRatio)
            Call RunSuite("suite:encoding", AddressOf EncodingTests.Run)
            Call RunSuite("suite:nested", AddressOf NestedTests.Run)
            Call RunSuite("suite:nested-roundtrip", AddressOf NestedTests.RunStableRoundtrip)
            Call RunSuite("suite:nested-schema", AddressOf NestedTests.RunNestedSchema)
            Call RunSuite("suite:interop", AddressOf InteropTests.Run)
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
    ''' 最小可运行示例：<c>pq.write_table</c> 与 <c>pq.read_table</c>。
    ''' </summary>
    Private Sub RunQuickDemo()
        Dim df As DataFrame = Frames.MakeFlat(5)
        Dim file As String = System.IO.Path.Combine(TestSupport.TempDir(), "demo.parquet")

        Call pq.write_table(df, file)

        Console.WriteLine($"[write_table] {file}")
        Console.WriteLine($"[dataframe ] {df}")
        Console.WriteLine($"[file size ] {New System.IO.FileInfo(file).Length} bytes")

        Dim df2 As DataFrame = pq.read_table(file)

        Console.WriteLine($"[read_table] {df2}")
        Console.WriteLine($"[rownames  ] {String.Join(", ", df2.rownames)}")
        Console.WriteLine()

        Dim schema As Schema.ParquetSchema = pq.read_schema(file)

        Console.WriteLine("[schema]")
        PrintSchema(schema.Root, 0)
        Console.WriteLine()
    End Sub

    Private Sub PrintSchema(node As Schema.ParquetSchemaNode, indent As Integer)
        If Not node.IsRoot Then
            Dim prefix As String = New String(" "c, indent * 2)

            If node.IsGroup Then
                Console.WriteLine($"{prefix}{node.Name} (group, {node.Repetition})")
            Else
                Console.WriteLine($"{prefix}{node.Name} ({node.PhysicalType}, {node.Repetition})")
            End If
        End If

        For Each child As Schema.ParquetSchemaNode In node.Children
            Call PrintSchema(child, indent + 1)
        Next
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
