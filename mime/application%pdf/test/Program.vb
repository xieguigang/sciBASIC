' ============================================================================
'  Program.vb  -  命令行入口
'  ----------------------------------------------------------------------------
'  用法：
'    VBNetPdfParser <input.pdf> [output.txt]
'  若不指定输出文件，则输出到与输入同名的 .txt 文件。
' ============================================================================

Imports System.IO

Public Class Program
    Public Shared Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8
        ' 注册 Windows 等代码页编码（.NET Core/5+ 默认不含）
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

        If args.Length = 0 Then
            PrintUsage()
            Return 1
        End If

        Dim inputPath = args(0)
        Dim outputPath = If(args.Length > 1, args(1), Path.ChangeExtension(inputPath, ".txt"))

        If Not File.Exists(inputPath) Then
            Console.Error.WriteLine($"错误：找不到输入文件 {inputPath}")
            Return 2
        End If

        Try
            Dim sw = System.Diagnostics.Stopwatch.StartNew()
            Console.WriteLine($"[1/3] 读取 PDF 文件: {inputPath}")
            Using reader As New PdfReader(inputPath)
                Console.WriteLine($"      文件大小: {New FileInfo(inputPath).Length / 1024.0:F1} KB")
                Console.WriteLine($"      解析到 {reader.ObjectCount} 个间接对象")

                Console.WriteLine("[2/3] 遍历页面树并提取文本...")
                Dim extractor As New TextExtractor(reader)
                Dim pages = reader.GetPages()
                Console.WriteLine($"      共 {pages.Count} 页")

                Console.WriteLine("[3/3] 写入输出文件...")
                Using writer As New StreamWriter(outputPath, False, System.Text.Encoding.UTF8)
                    For i = 0 To pages.Count - 1
                        Dim text = extractor.ExtractFromPage(pages(i))
                        writer.WriteLine($"========== Page {i + 1} ==========")
                        writer.WriteLine(text)
                        writer.WriteLine()
                        If (i + 1) Mod 10 = 0 OrElse i = pages.Count - 1 Then
                            Console.WriteLine($"      已处理 {i + 1}/{pages.Count} 页")
                        End If
                    Next
                End Using

                sw.Stop()
                Console.WriteLine($"完成！耗时 {sw.ElapsedMilliseconds} ms")
                Console.WriteLine($"输出文件: {outputPath}")
                Console.WriteLine($"输出大小: {New FileInfo(outputPath).Length / 1024.0:F1} KB")
            End Using
            Return 0
        Catch ex As Exception
            Console.Error.WriteLine($"解析失败: {ex.Message}")
            Console.Error.WriteLine(ex.StackTrace)
            Return 3
        End Try
    End Function

    Private Shared Sub PrintUsage()
        Console.WriteLine("VBNetPdfParser - 从头实现的 PDF 文本解析器")
        Console.WriteLine()
        Console.WriteLine("用法:")
        Console.WriteLine("  VBNetPdfParser <input.pdf> [output.txt]")
        Console.WriteLine()
        Console.WriteLine("参数:")
        Console.WriteLine("  input.pdf    要解析的 PDF 文件路径")
        Console.WriteLine("  output.txt   输出文本文件路径（可选，默认与输入同名 .txt）")
        Console.WriteLine()
        Console.WriteLine("示例:")
        Console.WriteLine("  VBNetPdfParser paper.pdf")
        Console.WriteLine("  VBNetPdfParser paper.pdf paper_text.txt")
    End Sub
End Class
