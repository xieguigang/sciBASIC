#Region "Microsoft.VisualBasic::68b34c14bd34f9921a8df64f789fea93, Data_science\NLP\test\sample\Program.vb"

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

    '   Total Lines: 103
    '    Code Lines: 81 (78.64%)
    ' Comment Lines: 4 (3.88%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (17.48%)
    '     File Size: 4.51 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.NLP.ChineseTokenizer

Module Program

    Sub Mai222n(args As String())
        Console.WriteLine(New String("="c, 60))
        Console.WriteLine("中文分词算法模块演示 (VB.NET / .NET 10)")
        Console.WriteLine(New String("="c, 60))
        Console.WriteLine()

        ' ===== 演示 1：使用内置默认词典 =====
        Console.WriteLine("[演示 1] 使用内置默认词典 + Hybrid 模式")
        Console.WriteLine(New String("-"c, 60))

        Dim tokenizer As Tokenizer = Tokenizer.CreateDefault()

        Dim sentences() As String = {
            "Hi, 我喜欢学习自然语言处理",
            "[演示 1] 使用内置默认词典 + Hybrid 模式",
            "中文分词算法模块演示 (VB.NET / .NET 10)",
            "中文分词是自然语言处理的基础",
            "他正在研究机器学习和深度学习算法",
            "北京是中国的首都",
            "因为天气很好，所以我们去公园散步"
        }

        For Each s As String In sentences
            Console.WriteLine("原文: " & s)
            Console.WriteLine("分词: " & tokenizer.SegmentToString(s))
            Console.WriteLine()
        Next

        ' ===== 演示 2：对比不同算法 =====
        Console.WriteLine()
        Console.WriteLine("[演示 2] 不同分词算法对比")
        Console.WriteLine(New String("-"c, 60))

        Dim testText As String = "自然语言处理是人工智能的重要分支"
        Console.WriteLine("原文: " & testText)
        Console.WriteLine()

        For Each algo As SegmentAlgorithm In [Enum].GetValues(GetType(SegmentAlgorithm))
            tokenizer.Algorithm = algo
            Console.WriteLine(algo.ToString().PadRight(25) & ": " & tokenizer.SegmentToString(testText))
        Next

        ' ===== 演示 3：从外部词典文件加载 =====
        Console.WriteLine()
        Console.WriteLine("[演示 3] 从外部词典文件加载")
        Console.WriteLine(New String("-"c, 60))

        Dim dictPath As String = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "dict", "dict.txt")
        If Not File.Exists(dictPath) Then
            dictPath = Path.Combine(AppContext.BaseDirectory, "dict", "dict.txt")
        End If

        If File.Exists(dictPath) Then
            Dim customTokenizer As New Tokenizer(dictPath) With {.Algorithm = SegmentAlgorithm.Hybrid}
            Console.WriteLine("词典文件: " & Path.GetFullPath(dictPath))
            Console.WriteLine("词典词条数: " & customTokenizer.Dictionary.Count)
            Console.WriteLine("词典最长词长度: " & customTokenizer.Dictionary.MaxWordLength)
            Console.WriteLine()

            Dim sample As String = "自然语言处理技术正在改变世界"
            Console.WriteLine("原文: " & sample)
            Console.WriteLine("分词: " & customTokenizer.SegmentToString(sample))
        Else
            Console.WriteLine("未找到外部词典文件，跳过此演示。")
        End If

        ' ===== 演示 4：性能测试 =====
        Console.WriteLine()
        Console.WriteLine("[演示 4] 性能测试")
        Console.WriteLine(New String("-"c, 60))

        tokenizer.Algorithm = SegmentAlgorithm.Hybrid
        Dim longText As New System.Text.StringBuilder()
        For i As Integer = 1 To 1000
            longText.Append("自然语言处理是人工智能的重要分支，中文分词是其基础技术。")
        Next
        Dim input As String = longText.ToString()
        Console.WriteLine("测试文本长度: " & input.Length & " 字符")

        Dim sw As Stopwatch = Stopwatch.StartNew()
        Dim repeat As Integer = 5
        Dim totalWords As Integer = 0
        For i As Integer = 1 To repeat
            totalWords = tokenizer.Segment(input).Count
        Next
        sw.Stop()
        Console.WriteLine("分词 " & repeat & " 次耗时: " & sw.ElapsedMilliseconds & " ms")
        Console.WriteLine("平均每次: " & (sw.ElapsedMilliseconds / repeat).ToString("F2") & " ms")
        Console.WriteLine("单次分词结果词数: " & totalWords)
        Console.WriteLine("吞吐量: " & (input.Length * repeat * 1.0 / Math.Max(sw.ElapsedMilliseconds, 1) * 1000).ToString("F0") & " 字符/秒")

        Console.WriteLine()
        Console.WriteLine(New String("="c, 60))
        Console.WriteLine("演示结束")
        Console.WriteLine(New String("="c, 60))
    End Sub

End Module
