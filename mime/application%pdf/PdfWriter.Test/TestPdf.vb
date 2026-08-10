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

        ' 图片（若存在则插入，否则跳过）
        Dim imgPath = Path.Combine(Directory.GetCurrentDirectory(), "sample.png")
        If File.Exists(imgPath) Then
            doc.Image(imgPath, caption:="图 1. 示例图片")
        End If

        doc.PageBreak()
        doc.H1("3. 引用与分割线")
        doc.Blockquote("这是一段引用内容，用于验证引用样式与背景色。")
        doc.Hr()
        doc.Paragraph("报告结束。")

        doc.Save(out)
        Console.WriteLine("PDF 已生成: " & out)

        ' 用现有读取模块回读文本，校验内容完整性
        Using fs As New FileStream(out, FileMode.Open, FileAccess.Read)
            Dim texts = PDF.GetText(fs)
            Dim all = String.Join(vbCrLf, texts)
            Console.WriteLine("===== 回读文本 =====")
            Console.WriteLine(all)
            Console.WriteLine("===== 校验 =====")
            Check(all, "PDF Writer 测试报告")
            Check(all, "概述")
            Check(all, "数据表格")
            Check(all, "Alpha")
            Check(all, "Gamma")
            Check(all, "完成读取模块")
            Check(all, "报告结束")
        End Using
    End Sub

    Private Sub Check(text As String, expect As String)
        If text.Contains(expect) Then
            Console.WriteLine("[OK] 包含: " & expect)
        Else
            Console.WriteLine("[FAIL] 缺失: " & expect)
        End If
    End Sub

End Module
