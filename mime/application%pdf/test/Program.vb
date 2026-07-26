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
