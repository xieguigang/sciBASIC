#Region "Microsoft.VisualBasic::36584906b022f219472c2e198801a55c, mime\application%pdf\test\Program.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 8 (14.81%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (14.81%)
    '     File Size: 2.13 KB


    ' Class Program
    ' 
    '     Function: Main
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
