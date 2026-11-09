' ---------------------------------------------------------------------------
' 运行报告
'
' 两类输出并存，各自解决不同问题：
'   * 人读的文本报告（worldmaker_report.txt）：分节表格，一眼能看出每一段
'     花了多久、质量指标是多少、产物在哪；
'   * 机器读的 JSON（worldmaker_report.json）与 DebugAudit 报告：便于回归对比，
'     也满足本项目“console 测试用 DebugAudit 出报告”的规范。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Reporting

    ''' <summary>报告中的一行。</summary>
    Public Class ReportRow

        Public Property Section As String
        Public Property Name As String
        Public Property Value As String

    End Class

    ''' <summary>
    ''' 分节报告收集器。
    ''' </summary>
    Public Class RunReport

        Private ReadOnly _rows As New List(Of ReportRow)()
        Private ReadOnly _sections As New List(Of String)()

        ''' <summary>
        ''' 追加一行（自动去重节名，保持出现顺序）。
        ''' </summary>
        Public Sub Add(section As String, name As String, value As String)
            If Not _sections.Contains(section) Then Call _sections.Add(section)

            Call _rows.Add(New ReportRow With {
                .Section = section,
                .Name = name,
                .Value = If(value, "")
            })
        End Sub

        Public Sub Add(section As String, name As String, value As Integer)
            Call Add(section, name, value.ToString("N0"))
        End Sub

        Public Sub Add(section As String, name As String, value As Long)
            Call Add(section, name, value.ToString("N0"))
        End Sub

        Public Sub Add(section As String, name As String, value As Double, Optional format As String = "G4")
            Call Add(section, name, value.ToString(format))
        End Sub

        Public Sub Add(section As String, name As String, value As Boolean)
            Call Add(section, name, If(value, "yes", "no"))
        End Sub

        ''' <summary>渲染文本报告。</summary>
        Public Function ToText(title As String) As String
            Dim sb As New StringBuilder()
            Dim width As Integer = 0

            For Each row As ReportRow In _rows
                width = System.Math.Max(width, row.Name.Length)
            Next

            width = System.Math.Min(System.Math.Max(width, 18), 42)

            Call sb.AppendLine(New String("="c, 78))
            Call sb.AppendLine(title)
            Call sb.AppendLine(New String("="c, 78))
            Call sb.AppendLine()

            For Each section As String In _sections
                Call sb.AppendLine($"[{section}]")

                For Each row As ReportRow In _rows
                    If Not row.Section = section Then Continue For

                    Call sb.AppendLine("  " & row.Name.PadRight(width) & " : " & row.Value)
                Next

                Call sb.AppendLine()
            Next

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 落盘：文本 + JSON + DebugAudit 报告。
        ''' </summary>
        Public Sub Save(reportDir As String, title As String)
            If Not Directory.Exists(reportDir) Then Call Directory.CreateDirectory(reportDir)

            Dim textPath As String = Path.Combine(reportDir, "worldmaker_report.txt")
            Dim jsonPath As String = Path.Combine(reportDir, "worldmaker_report.json")
            Dim auditPath As String = Path.Combine(reportDir, "debug_audit_report.txt")

            Call File.WriteAllText(textPath, ToText(title), New UTF8Encoding(False))
            Call File.WriteAllText(jsonPath, ToJson(), New UTF8Encoding(False))

            ' DebugAudit 的报告：把关键指标也写进去，方便与其它测试程序用同一套工具对比
            For Each row As ReportRow In _rows
                Call DebugAudit.Capture(row.Value, $"{row.Section}/{row.Name}")
            Next

            Try
                Call DebugAudit.SaveReport(auditPath)
            Catch ex As Exception
                Console.WriteLine($"[warn] DebugAudit 报告写出失败: {ex.Message}")
            End Try
        End Sub

        ''' <summary>简单 JSON（保持节的层次，值一律按字符串，避免格式歧义）。</summary>
        Public Function ToJson() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine("{")

            Dim firstSection As Boolean = True

            For Each section As String In _sections
                If Not firstSection Then Call sb.AppendLine(",")

                firstSection = False

                Call sb.AppendLine($"  ""{Escape(section)}"": {{")

                Dim firstRow As Boolean = True

                For Each row As ReportRow In _rows
                    If Not row.Section = section Then Continue For

                    If Not firstRow Then Call sb.AppendLine(",")

                    firstRow = False

                    Call sb.Append($"    ""{Escape(row.Name)}"": ""{Escape(row.Value)}""")
                Next

                Call sb.AppendLine()
                Call sb.Append("  }")
            Next

            Call sb.AppendLine()
            Call sb.AppendLine("}")

            Return sb.ToString()
        End Function

        Private Shared Function Escape(text As String) As String
            If text Is Nothing Then Return ""

            Return text.Replace("\", "\\").Replace("""", "\""").Replace(vbCr, " ").Replace(vbLf, " ")
        End Function

    End Class

End Namespace
