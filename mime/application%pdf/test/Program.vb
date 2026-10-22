#Region "Microsoft.VisualBasic::34dbaa11b1350b30c78a35ef91821a52, mime\application%pdf\test\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 124
    '    Code Lines: 94 (75.81%)
    ' Comment Lines: 10 (8.06%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 20 (16.13%)
    '     File Size: 5.18 KB


    ' Class Program
    ' 
    '     Function: Main, Run
    ' 
    '     Sub: PrintUsage
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  Program.vb  -  命令行入口
'  ----------------------------------------------------------------------------
'  用法：
'    VBNetPdfParser <input.pdf> [output.txt]
'  若不指定输出文件，则输出到与输入同名的 .txt 文件。
' ============================================================================

Imports System.IO
Imports System.Linq
Imports System.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.MIME.application.pdf

Public Class Program
    ''' <summary>不传参数时默认回归的测试样本。</summary>
    Private Shared ReadOnly DefaultSamples As String() = {
        "Z:\pdf_test\FSN3-8-1904.pdf",
        "Z:\pdf_test\P020210610394569640881.pdf"
    }

    Public Shared Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8
        ' 注册 Windows 等代码页编码（.NET Core/5+ 默认不含）
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

        Dim inputs As New List(Of String)()

        If args.Length = 0 Then
            inputs.AddRange(DefaultSamples)
        ElseIf args.Length = 1 AndAlso (args(0) = "-h" OrElse args(0) = "--help") Then
            PrintUsage()
            Return 0
        ElseIf args.Length = 1 AndAlso args(0) = "--render" Then
            ' 写入侧演示：内容块 -> PdfRender -> .pdf，再用本库解析器回读校验
            Return RenderDemo(Environment.CurrentDirectory)
        Else
            inputs.AddRange(args)
        End If

        Dim exitCode = 0

        For Each inputPath In inputs
            Dim outputPath = Path.ChangeExtension(inputPath, ".txt")

            If Not File.Exists(inputPath) Then
                Console.Error.WriteLine($"错误：找不到输入文件 {inputPath}")
                exitCode = 2
                Continue For
            End If

            Try
                If Not Run(inputPath, outputPath) Then
                    exitCode = 3
                End If
            Catch ex As Exception
                Console.Error.WriteLine($"解析失败: {inputPath}")
                Console.Error.WriteLine($"  {ex.Message}")
                Console.Error.WriteLine(ex.StackTrace)
                exitCode = 3
            End Try
        Next

        Return exitCode
    End Function

    ''' <summary>提取单个 PDF 并写出文本；提取结果有效时返回 True。</summary>
    Private Shared Function Run(inputPath As String, outputPath As String) As Boolean
        Dim sw = Diagnostics.Stopwatch.StartNew()

        Using pdfStream As Stream = File.OpenRead(inputPath)
            Dim pages = Global.Microsoft.VisualBasic.MIME.application.pdf.PDF.GetText(pdfStream).ToArray()
            sw.Stop()

            Dim totalChars = pages.Sum(Function(s) If(s Is Nothing, 0, s.Length))
            Dim nonEmptyPages = pages.Count(Function(s) s IsNot Nothing AndAlso s.Trim().Length > 0)

            Console.WriteLine("================================================================")
            Console.WriteLine($"文件: {inputPath}")
            Console.WriteLine($"大小: {New FileInfo(inputPath).Length:N0} 字节    耗时: {sw.ElapsedMilliseconds} ms")
            Console.WriteLine($"页数: {pages.Length}    非空页: {nonEmptyPages}    总字符数: {totalChars}")

            For i = 0 To pages.Length - 1
                Dim text = If(pages(i), "")
                Console.WriteLine($"  [第 {i + 1} 页] {text.Length} 字符")
            Next

            If totalChars > 0 Then
                Dim preview = String.Join(Environment.NewLine, pages)
                preview = preview.Trim().Replace(Environment.NewLine, " ")
                If preview.Length > 200 Then preview = preview.Substring(0, 200)
                Console.WriteLine($"预览: {preview}")
            End If

            File.WriteAllLines(outputPath, pages, System.Text.Encoding.UTF8)
            Console.WriteLine($"输出: {outputPath}")

            If pages.Length = 0 Then
                Console.Error.WriteLine("失败：未解析出任何页面")
                Return False
            End If
            If totalChars = 0 Then
                Console.Error.WriteLine("失败：提取到的文本长度为 0")
                Return False
            End If

            Return True
        End Using
    End Function

    ' ========================================================================
    ' 写入侧自校验：内容块 -> PdfRender -> .pdf -> 回读
    ' ========================================================================

    Private Shared Function RenderDemo(outDir As String) As Integer
        Dim failures As Integer = 0
        Dim check As Action(Of String, Boolean) =
            Sub(name As String, ok As Boolean)
                If ok Then
                    Console.WriteLine($"  [通过] {name}")
                Else
                    failures += 1
                    Console.WriteLine($"  [失败] {name}")
                End If
            End Sub

        Dim pdfPath As String = Path.Combine(outDir, "demo_render.pdf")

        Dim blocks As Block() = {
            New Block With {.type = "heading", .level = 1, .content = "第一章 报告渲染"},
            New Block With {.type = "paragraph", .content = "本文档由 PdfRender 从内容块直接渲染为 PDF 文档。"},
            New Block With {.type = "list", .ordered = True, .items = {"内容块模型", "PDF 排版", "文件保存"}},
            New Block With {.type = "table",
                            .headers = {"渲染器", "输出"},
                            .alignments = {"left", "left"},
                            .rows = {New String() {"PdfRender", "PDF"}, New String() {"WordRender", "docx"}}},
            New Block With {.type = "blockquote", .content = "PDF 与 docx / RTF 共用同一份 Block 内容模型。"},
            New Block With {.type = "hr"}
        }

        Dim render As New PdfRender() With {
            .DocumentTitle = "PdfRender 报告示例",
            .TableOfContents = True,
            .Author = "sciBASIC#",
            .Title = "PdfRender 示例",
            .Subject = "PdfRender",
            .Tags = New String() {"pdf", "report"}
        }

        Call render.Render(blocks)
        Call render.Save(pdfPath)

        Console.WriteLine($"已生成: {pdfPath} ({New FileInfo(pdfPath).Length:N0} 字节)")
        check("生成文件非空", File.Exists(pdfPath) AndAlso New FileInfo(pdfPath).Length > 0)

        Dim header As String

        Using fs As Stream = File.OpenRead(pdfPath)
            Dim buf(7) As Byte
            Call fs.ReadExactly(buf, 0, buf.Length)
            header = System.Text.Encoding.ASCII.GetString(buf)
        End Using

        check("PDF 文件头为 %PDF-", header.StartsWith("%PDF-"))

        Dim pages As String()

        Using fs As Stream = File.OpenRead(pdfPath)
            pages = PDF.GetText(fs).ToArray()
        End Using

        Dim text As String = String.Join(Environment.NewLine, pages)

        Console.WriteLine($"页数: {pages.Length}，回读字符数: {text.Length}")

        check("解析出至少一页内容", pages.Length > 0)

        For Each expected As String In {
            "PdfRender 报告示例",
            "第一章 报告渲染",
            "本文档由 PdfRender 从内容块直接渲染为 PDF 文档。",
            "内容块模型",
            "docx"
        }
            check("回读包含: " & expected, text.Contains(expected))
        Next

        ' ---- 一次性生成 ----
        Dim oneShot As String = Path.Combine(outDir, "demo_render_generate.pdf")

        Call PdfRender.Generate({New Block With {.type = "paragraph", .content = "一句话生成的 PDF 报告。"}},
                                oneShot,
                                "一句话生成 PDF")

        check("Generate 生成文件非空", File.Exists(oneShot) AndAlso New FileInfo(oneShot).Length > 0)

        Console.WriteLine()

        If failures = 0 Then
            Console.WriteLine("全部检查通过。")
            Return 0
        Else
            Console.WriteLine($"存在 {failures} 项失败。")
            Return 1
        End If
    End Function

    Private Shared Sub PrintUsage()
        Console.WriteLine("VBNetPdfParser - 从头实现的 PDF 文本解析器")
        Console.WriteLine()
        Console.WriteLine("用法:")
        Console.WriteLine("  VBNetPdfParser [<input.pdf> ...]")
        Console.WriteLine()
        Console.WriteLine("参数:")
        Console.WriteLine("  input.pdf    要解析的 PDF 文件路径，可传入多个")
        Console.WriteLine("               输出文本固定写到与输入同名的 .txt 文件")
        Console.WriteLine("               不传参数时，默认回归以下测试样本：")
        For Each s In DefaultSamples
            Console.WriteLine($"                 {s}")
        Next
        Console.WriteLine()
        Console.WriteLine("示例:")
        Console.WriteLine("  VBNetPdfParser")
        Console.WriteLine("  VBNetPdfParser paper.pdf")
    End Sub
End Class
