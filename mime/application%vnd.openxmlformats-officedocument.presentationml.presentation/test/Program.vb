Imports System
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Office.PPTX

Module Program

    Private Const OutputFile As String = "test_output.pptx"
    Private Const ReportFile As String = "test_report.json"

    Private Function ResolveTestImage() As String
        Dim dir As New DirectoryInfo(AppContext.BaseDirectory)

        While dir IsNot Nothing
            Dim candidate As String = Path.Combine(dir.FullName, "etc", "badge.png")
            If File.Exists(candidate) Then
                Return candidate
            End If
            dir = dir.Parent
        End While

        Return Nothing
    End Function

    Sub Main(args As String())
        Call DebugAudit.Reset()
        Call Console.WriteLine("=== PPTX Generation Module Test ===")

        Try
            Dim outputPath As String = Path.GetFullPath(OutputFile)

            ' 1. 创建文档与页面
            Dim doc As New PresentationDocument()
            Dim page1 As PresentationPageGraphic = doc.AddPage()
            Dim page2 As PresentationPageGraphic = doc.AddPage(New Drawing.Size(800, 600))

            Call DebugAudit.Mark("page1.size", page1.Size.ToString())
            Call DebugAudit.Mark("page2.size", page2.Size.ToString())
            Call DebugAudit.Test(doc.Pages.Count = 2, "AddPage should create 2 pages")

            ' 2. 页面1：文本、形状、表格（GDI+ 风格调用）
            Dim titleFont As New Font("Arial", 28, FontStyle.Bold)
            Dim bodyFont As New Font("Calibri", 16)
            Dim whiteBrush As New SolidBrush(Drawing.Color.White)
            Dim redPen As New Pen(Drawing.Color.Red, 3)

            Call page1.Clear(Drawing.Color.FromArgb(30, 40, 60))
            Call page1.DrawString("PPTX Generation Test", titleFont, whiteBrush, 60, 40)
            Call page1.DrawString("Shapes, text and table via GDI+-like API", bodyFont, whiteBrush, 60, 95)
            Call page1.DrawRectangle(redPen, New Drawing.Rectangle(60, 150, 300, 120))
            Call page1.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(70, 130, 180)), 400, 150, 250, 120)
            Call page1.DrawEllipse(New Pen(Drawing.Color.Yellow, 2), 700, 150, 180, 120)
            Call page1.FillEllipse(New SolidBrush(Drawing.Color.Orange), 700, 300, 180, 100)

            Dim data(2, 2) As String
            data(0, 0) = "Name" : data(0, 1) = "Value" : data(0, 2) = "Status"
            data(1, 0) = "Alpha" : data(1, 1) = "100" : data(1, 2) = "OK"
            data(2, 0) = "Beta" : data(2, 1) = "200" : data(2, 2) = "Pending"
            Call page1.DrawTable(New Drawing.RectangleF(60, 320, 520, 180), data)

            ' 3. 页面2：变换、多边形、圆弧、文本
            Call page2.DrawString("Transforms & Curves", titleFont, New SolidBrush(Drawing.Color.Black), 30, 20)
            Call page2.TranslateTransform(50, 50)
            Call page2.DrawRectangle(New Pen(Drawing.Color.Blue, 2), 0, 0, 120, 80)
            Call page2.ResetTransform()
            Call page2.DrawPolygon(New Pen(Drawing.Color.Green, 2),
                                   New Drawing.PointF() {New Drawing.PointF(250, 60), New Drawing.PointF(400, 60), New Drawing.PointF(325, 180)})
            Call page2.FillPolygon(New SolidBrush(Drawing.Color.Purple),
                                   New Drawing.PointF() {New Drawing.PointF(450, 60), New Drawing.PointF(600, 60), New Drawing.PointF(525, 180)})
            Call page2.DrawArc(New Pen(Drawing.Color.Magenta, 2), New Drawing.RectangleF(200, 220, 300, 200), 0, 180)

            ' 图片元素（若测试图片存在）
            Dim imagePath As String = ResolveTestImage()
            If imagePath IsNot Nothing Then
                Dim img As Image = Image.FromFile(imagePath)
                Call page2.DrawImage(img, New Drawing.RectangleF(100, 450, 200, 120))
                Call DebugAudit.Mark("image.loaded", True)
            Else
                Call DebugAudit.Mark("image.loaded", False)
            End If

            ' 4. 保存
            Call doc.Save(outputPath)
            Dim fileInfo As New FileInfo(outputPath)

            Call DebugAudit.Mark("output.path", outputPath)
            Call DebugAudit.Mark("output.bytes", fileInfo.Length)
            Call DebugAudit.Test(fileInfo.Exists AndAlso fileInfo.Length > 0, "Output pptx file should exist and be non-empty")

            ' 5. 验证 ZIP 部件结构
            Using archive As ZipArchive = ZipFile.OpenRead(outputPath)
                Dim names As String() = archive.Entries.Select(Function(e) e.FullName).ToArray()

                Call DebugAudit.Test(names.Contains("[Content_Types].xml"), "Should contain [Content_Types].xml")
                Call DebugAudit.Test(names.Contains("_rels/.rels"), "Should contain _rels/.rels")
                Call DebugAudit.Test(names.Contains("ppt/presentation.xml"), "Should contain ppt/presentation.xml")
                Call DebugAudit.Test(names.Contains("ppt/slides/slide1.xml"), "Should contain slide1.xml")
                Call DebugAudit.Test(names.Contains("ppt/slides/slide2.xml"), "Should contain slide2.xml")
                Call DebugAudit.Test(names.Contains("ppt/theme/theme1.xml"), "Should contain theme1.xml")
                Call DebugAudit.Test(names.Contains("ppt/slideMasters/slideMaster1.xml"), "Should contain slideMaster1.xml")
                Call DebugAudit.Test(names.Contains("ppt/slideLayouts/slideLayout1.xml"), "Should contain slideLayout1.xml")

                If names.Any(Function(n) n.StartsWith("ppt/media/")) Then
                    Call DebugAudit.Test(names.Contains("ppt/slides/_rels/slide2.xml.rels"), "Should contain slide2 rels for image")
                    Dim relsXml As String = New StreamReader(archive.GetEntry("ppt/slides/_rels/slide2.xml.rels").Open()).ReadToEnd()
                    Call DebugAudit.Test(relsXml.Contains("/image"), "slide2 rels should contain image relationship")
                    Call DebugAudit.Test(names.Any(Function(n) n.StartsWith("ppt/media/image")), "Should contain media image")
                End If

                ' 验证 slide1 包含表格与文本
                Dim slide1 As ZipArchiveEntry = archive.GetEntry("ppt/slides/slide1.xml")
                Using reader As New StreamReader(slide1.Open())
                    Dim xml As String = reader.ReadToEnd()
                    Call DebugAudit.Test(xml.Contains("<a:tbl>"), "slide1 should contain a table")
                    Call DebugAudit.Test(xml.Contains("PPTX Generation Test"), "slide1 should contain title text")
                    Call DebugAudit.Test(xml.Contains("<a:srgbClr"), "slide1 should contain color definitions")
                End Using

                Call DebugAudit.Mark("zip.parts", names.Length)
            End Using

            ' 6. 输出测试报告
            Call DebugAudit.Test(DebugAudit.FailureCount = 0, "All tests should pass")
            Call Console.WriteLine(DebugAudit.Dump())
            Call DebugAudit.SaveReport(Path.GetFullPath(ReportFile))
            Call Console.WriteLine($"Report saved to: {Path.GetFullPath(ReportFile)}")
            Call Console.WriteLine($"PPTX saved to: {outputPath}")

        Catch ex As Exception
            Call DebugAudit.Mark("error.message", ex.Message)
            Call DebugAudit.Mark("error.stack", ex.StackTrace)
            Call Console.WriteLine($"ERROR: {ex.Message}")
            Call Console.WriteLine(ex.StackTrace)
            Call DebugAudit.SaveReport(Path.GetFullPath(ReportFile))
            Environment.ExitCode = 1
        End Try
    End Sub
End Module
