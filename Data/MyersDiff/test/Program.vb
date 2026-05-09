

' ============================================================================
' 演示程序（控制台应用入口）
' ============================================================================
' 以下代码展示如何使用 MyersDiff 模块。如果将此文件集成到自己的项目中，
' 可以删除此 Module。
' ============================================================================

Imports System.IO
Imports System.Text

Module MyersDiffDemo

    Sub Main()
        Console.WriteLine("============================================")
        Console.WriteLine("  Myers 差异算法模块 — 演示程序")
        Console.WriteLine("============================================")
        Console.WriteLine()

        ' ---- 演示 1：行级差异比较 ----
        DemoLineDiff()

        ' ---- 演示 2：字符级差异比较 ----
        DemoCharDiff()

        ' ---- 演示 3：文件差异比较 ----
        DemoFileDiff()

        Console.WriteLine()
        Console.WriteLine("按任意键退出...")
        Console.ReadKey()
    End Sub

    ''' <summary>
    ''' 演示行级差异比较
    ''' </summary>
    Private Sub DemoLineDiff()
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine("  演示 1：行级差异比较")
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine()

        Dim oldLines As String() = {
            "Imports System",
            "Imports System.Collections",
            "",
            "Public Class Calculator",
            "    Private value As Integer = 0",
            "",
            "    Public Sub New()",
            "        value = 0",
            "    End Sub",
            "",
            "    Public Function Add(x As Integer) As Integer",
            "        value += x",
            "        Return value",
            "    End Function",
            "",
            "    Public Function Subtract(x As Integer) As Integer",
            "        value -= x",
            "        Return value",
            "    End Function",
            "End Class"
        }

        Dim newLines As String() = {
            "Imports System",
            "Imports System.Collections.Generic",
            "",
            "Public Class Calculator",
            "    Private value As Integer = 0",
            "    Private history As New List(Of Integer)()",
            "",
            "    Public Sub New()",
            "        value = 0",
            "        history.Clear()",
            "    End Sub",
            "",
            "    Public Function Add(x As Integer) As Integer",
            "        value += x",
            "        history.Add(value)",
            "        Return value",
            "    End Function",
            "",
            "    Public Function Subtract(x As Integer) As Integer",
            "        value -= x",
            "        history.Add(value)",
            "        Return value",
            "    End Function",
            "",
            "    Public Function GetHistory() As List(Of Integer)",
            "        Return history",
            "    End Function",
            "",
            "End Class"
        }

        Dim differ As New MyersDiffAlgorithm.MyersDiff()
        Dim result As MyersDiffAlgorithm.DiffResult = differ.Compare(oldLines, newLines)

        ' 输出统计摘要
        Console.WriteLine(result.ToSummary())
        Console.WriteLine()

        ' 输出统一差异格式
        Console.WriteLine("统一差异格式输出:")
        Console.WriteLine(result.ToUnifiedDiff("Calculator.vb (旧)", "Calculator.vb (新)", 2))
        Console.WriteLine()

        ' 输出并排对照格式
        Console.WriteLine("并排对照格式输出:")
        Console.WriteLine(result.ToSideBySide(40))
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' 演示字符级差异比较
    ''' </summary>
    Private Sub DemoCharDiff()
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine("  演示 2：字符级差异比较")
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine()

        Dim oldText As String = "The quick brown fox jumps over the lazy dog."
        Dim newText As String = "The quick red fox leaps over the sleepy dog."

        Dim differ As New MyersDiffAlgorithm.MyersDiff()
        Dim result As MyersDiffAlgorithm.DiffResult = differ.CompareChars(oldText, newText)

        Console.WriteLine("旧文本: {0}", oldText)
        Console.WriteLine("新文本: {0}", newText)
        Console.WriteLine()

        ' 字符级差异输出
        Console.Write("差异: ")
        For Each item In result.Items
            Select Case item.Type
                Case MyersDiffAlgorithm.EditType.Equal
                    Console.Write(item.Value)
                Case MyersDiffAlgorithm.EditType.Delete
                    Console.Write("[-" & item.Value & "-]")
                Case MyersDiffAlgorithm.EditType.Insert
                    Console.Write("[+" & item.Value & "+]")
            End Select
        Next
        Console.WriteLine()
        Console.WriteLine()

        Console.WriteLine(result.ToSummary())
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' 演示文件差异比较
    ''' </summary>
    Private Sub DemoFileDiff()
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine("  演示 3：文件差异比较")
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine()

        ' 创建临时测试文件
        Dim tempDir As String = Path.GetTempPath()
        Dim oldFile As String = Path.Combine(tempDir, "myers_diff_test_old.txt")
        Dim newFile As String = Path.Combine(tempDir, "myers_diff_test_new.txt")

        Try
            ' 写入测试内容
            File.WriteAllText(oldFile,
                "第一行：这是原始文件" & vbCrLf &
                "第二行：内容保持不变" & vbCrLf &
                "第三行：这一行将被修改" & vbCrLf &
                "第四行：这一行将被删除" & vbCrLf &
                "第五行：内容保持不变" & vbCrLf,
                Encoding.UTF8)

            File.WriteAllText(newFile,
                "第一行：这是原始文件" & vbCrLf &
                "第二行：内容保持不变" & vbCrLf &
                "第三行：这一行已被修改" & vbCrLf &
                "第五行：内容保持不变" & vbCrLf &
                "新增行：这是插入的新行" & vbCrLf,
                Encoding.UTF8)

            ' 执行文件比较
            Dim diffText As String = MyersDiffAlgorithm.DiffUtils.DiffFiles(oldFile, newFile, 2)

            Console.WriteLine("旧文件: {0}", oldFile)
            Console.WriteLine("新文件: {0}", newFile)
            Console.WriteLine()
            Console.WriteLine(diffText)

            ' 获取详细结果
            Dim result As MyersDiffAlgorithm.DiffResult =
                MyersDiffAlgorithm.DiffUtils.CompareFiles(oldFile, newFile)
            Console.WriteLine(result.ToSummary())

        Finally
            ' 清理临时文件
            If File.Exists(oldFile) Then File.Delete(oldFile)
            If File.Exists(newFile) Then File.Delete(newFile)
        End Try
    End Sub

End Module
