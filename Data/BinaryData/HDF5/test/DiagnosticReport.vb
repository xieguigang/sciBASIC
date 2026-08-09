Imports System.IO
Imports System.Text

Namespace test

    ''' <summary>
    ''' 单个对象的诊断记录。
    ''' </summary>
    Public Class DiagnosticEntry
        Public Property path As String = ""
        Public Property kind As String = ""          ' "group" | "dataset" | "attribute"
        Public Property shape As String = ""          ' dataspace 维度描述
        Public Property dataType As String = ""       ' datatype 描述
        Public Property layout As String = ""         ' 存储布局描述
        Public Property filters As String = ""        ' 过滤器管线描述
        Public Property succeeded As Boolean = False
        Public Property sampleValues As String = ""   ' 抽样值文本
        Public Property sanityWarning As String = ""  ' 合理性校验告警，无则为空
        Public Property errorType As String = ""
        Public Property errorMessage As String = ""
        Public Property errorFrame As String = ""      ' 堆栈首帧，用于定位源码位置
    End Class

    ''' <summary>
    ''' 一个文件的完整诊断报告，提供汇总统计与文本渲染、写文件方法。
    ''' </summary>
    Public Class DiagnosticReport

        Public ReadOnly Property file As String
        Public ReadOnly Property entries As New List(Of DiagnosticEntry)
        Public ReadOnly Property startedAt As Date = Date.Now

        Public Sub New(filePath As String)
            Me.file = filePath
        End Sub

        Public Function Add(entry As DiagnosticEntry) As DiagnosticEntry
            entries.Add(entry)
            Return entry
        End Function

        Public Function successCount() As Integer
            Return entries.Where(Function(e) e.succeeded).Count()
        End Function

        Public Function failureCount() As Integer
            Return entries.Where(Function(e) Not e.succeeded).Count()
        End Function

        Public Function Render() As String
            Dim sb As New StringBuilder()

            sb.AppendLine("================================================================")
            sb.AppendLine("HDF5 诊断报告")
            sb.AppendLine("文件 : " & file)
            sb.AppendLine("时间 : " & startedAt.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.AppendLine("对象总数 : " & entries.Count & "   成功 : " & successCount() & "   失败 : " & failureCount())
            sb.AppendLine("================================================================")
            sb.AppendLine()

            For Each e In entries
                sb.AppendLine(formatEntry(e))
                sb.AppendLine()
            Next

            sb.AppendLine("----------------------------------------------------------------")
            sb.AppendLine("汇总 : 成功 " & successCount() & " / 失败 " & failureCount() & " / 共 " & entries.Count)
            sb.AppendLine("----------------------------------------------------------------")

            Return sb.ToString()
        End Function

        Private Function formatEntry(e As DiagnosticEntry) As String
            Dim sb As New StringBuilder()

            sb.AppendLine("[" & (If(e.succeeded, "OK  ", "FAIL")) & "] " & e.path & "  (" & e.kind & ")")
            sb.AppendLine("    shape  : " & e.shape)
            sb.AppendLine("    dtype  : " & e.dataType)
            sb.AppendLine("    layout : " & e.layout)
            sb.AppendLine("    filter : " & e.filters)

            If e.succeeded Then
                sb.AppendLine("    sample : " & e.sampleValues)

                If Not String.IsNullOrEmpty(e.sanityWarning) Then
                    sb.AppendLine("    !WARN  : " & e.sanityWarning)
                End If
            Else
                sb.AppendLine("    !ERROR : " & e.errorType & " : " & e.errorMessage)
                sb.AppendLine("    @frame : " & e.errorFrame)
            End If

            Return sb.ToString()
        End Function

        Public Sub WriteToFile(outPath As String)
            System.IO.File.WriteAllText(outPath, Render())
        End Sub
    End Class

End Namespace
