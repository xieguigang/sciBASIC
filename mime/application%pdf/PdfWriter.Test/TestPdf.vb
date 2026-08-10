Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.application.pdf.PdfWriter
Imports Microsoft.VisualBasic.MIME.Office.WordDocument

Module TestPdf

    Sub Main()
        ' 生成一份含标题/目录/正文/表格/图片/分页符的 PDF
        Dim out = Path.Combine(Directory.GetCurrentDirectory(), "test_report.pdf")

        Dim doc As New PdfDocument(author:="OmicsAgent", title:="PDF Writer Test")
        doc.PageSetupA4()
        doc.DocTitle("PDF Writer 测试报告")
        doc.Toc(3)
        doc.H1("1. 概述")
        doc.Paragraph("这是一段用于验证 PDF 写入模块的中文正文，包含首行缩进与自动换行功能。" &
                      "The quick brown fox jumps over the lazy dog. 混合中英文文本排版测试。")
        doc.H2("1.1 代码示例")
        doc.CodeBlock("Dim x As Integer = 42" & vbCrLf & "Console.WriteLine(x)")
        doc.H2("1.2 任务列表")
        doc.TaskList(New String() {"完成读取模块", "完成写入模块", "生成报告"}, New Boolean() {True, True, False})

        doc.H1("2. 数据表格")
        Dim headers = New String() {"名称", "数值", "备注"}
        Dim rows As New List(Of String())()
        rows.Add(New String() {"Alpha", "1.23", "正常"})
        rows.Add(New String() {"Beta", "4.56", "偏高"})
        rows.Add(New String() {"Gamma", "7.89", "异常"})
        doc.Table(headers, rows.ToArray())

        ' 生成一张示例 PNG 以验证图片嵌入路径
        Dim imgPath = Path.Combine(Directory.GetCurrentDirectory(), "sample.png")
        CreateSamplePng(imgPath)
        doc.Image(imgPath, caption:="Figure 1. Sample image")

        doc.PageBreak()
        doc.H1("3. 引用与分割线")
        doc.Blockquote("这是一段引用内容，用于验证引用样式与背景色。")
        doc.Hr()
        doc.Paragraph("Report finished.")

        doc.Save(out)
        Console.WriteLine("PDF 已生成: " & out)
        Console.WriteLine("PDF 文件大小: " & New FileInfo(out).Length & " 字节")

        ' 用现有读取模块回读文本，校验内容完整性
        Using fs As New FileStream(out, FileMode.Open, FileAccess.Read)
            Dim texts = New List(Of String)(PDF.GetText(fs))
            Dim all = String.Join(vbCrLf, texts)
            Console.WriteLine("===== 解析页数 =====")
            Console.WriteLine("页码数: " & texts.Count)
            Console.WriteLine("===== 回读文本（Latin 部分可被现有读取器解码）=====")
            Console.WriteLine(all)
            Console.WriteLine("===== 校验 =====")
            ' 现有 PdfReader 仅能解码西文；CJK 经 Type0 字体输出，读取器未实现 CMap 解码会乱码，
            ' 以下仅校验可被可靠解码的 Latin 内容，证明文档结构与文本流写入正确。
            Check(all, "PDF Writer")
            Check(all, "The quick brown fox jumps over the lazy dog")
            Check(all, "Dim x As Integer = 42")
            Check(all, "Alpha")
            Check(all, "Gamma")
            Check(all, "Beta")
            Check(all, "Report finished")
            If texts.Count >= 2 Then
                Console.WriteLine("[OK] 自动分页生效（页数 >= 2）")
            Else
                Console.WriteLine("[FAIL] 未触发分页")
            End If
        End Using
    End Sub

    ''' <summary>写入一张内嵌的 1x1 示例 PNG（base64），用于验证图片嵌入与解码路径。</summary>
    Private Sub CreateSamplePng(path As String)
        Try
            Dim b64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAC0lEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="
            Dim bytes = Convert.FromBase64String(b64)
            File.WriteAllBytes(path, bytes)
            Console.WriteLine("示例图片已生成: " & path)
        Catch ex As Exception
            Console.Error.WriteLine("[警告] 生成示例图片失败: " & ex.Message)
        End Try
    End Sub

    Private Sub Check(text As String, expect As String)
        If text.Contains(expect) Then
            Console.WriteLine("[OK] 包含: " & expect)
        Else
            Console.WriteLine("[FAIL] 缺失: " & expect)
        End If
    End Sub

End Module
