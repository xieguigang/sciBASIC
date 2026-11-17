' ------------------------------------------------------------------------
' 环境报告：原生库探测结果 + 可用适配器 + 修复建议
' ------------------------------------------------------------------------

Imports System.Text
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Diagnostics

    ''' <summary>一条可操作的修复建议。</summary>
    Public NotInheritable Class FixSuggestion

        Public Sub New(problem As String, action As String)
            Me.Problem = problem
            Me.Action = action
        End Sub

        ''' <summary>问题描述。</summary>
        Public ReadOnly Property Problem As String

        ''' <summary>建议的修复动作。</summary>
        Public ReadOnly Property Action As String

        Public Overrides Function ToString() As String
            Return $"- {Problem} -> {Action}"
        End Function

    End Class

    ''' <summary>环境报告。</summary>
    Public NotInheritable Class EnvironmentReport

        ''' <summary>三个原生库是否都已加载。</summary>
        Public Property IsAvailable As Boolean

        ''' <summary>DirectML.dll 的实际路径。</summary>
        Public Property DirectMLPath As String

        ''' <summary>D3D12/CXGI 是否可用。</summary>
        Public Property D3D12Available As Boolean
        Public Property DxgiAvailable As Boolean

        ''' <summary>全部加载尝试。</summary>
        Public Property Attempts As New List(Of NativeProbeAttempt)()

        ''' <summary>可用适配器。</summary>
        Public Property Adapters As New List(Of AdapterReport)()

        ''' <summary>探测失败原因。</summary>
        Public Property ErrorMessage As String

        ''' <summary>修复建议。</summary>
        Public Property Suggestions As New List(Of FixSuggestion)()

        ''' <summary>
        ''' 探测运行环境。不会抛异常。
        ''' </summary>
        Public Shared Function Probe() As EnvironmentReport
            Dim report As New EnvironmentReport()

            Try
                report.IsAvailable = DmlRuntime.Probe()
                report.DirectMLPath = DmlRuntime.DirectMLPath
                report.D3D12Available = DmlRuntime.D3D12 IsNot Nothing
                report.DxgiAvailable = DmlRuntime.Dxgi IsNot Nothing
                report.ErrorMessage = DmlRuntime.LastError

                For Each attempt As NativeProbeAttempt In DmlRuntime.Attempts
                    Call report.Attempts.Add(attempt)
                Next

                If report.IsAvailable Then
                    Dim adapters As List(Of DmlAdapter) = DmlAdapter.Enumerate()

                    Try
                        For Each adapter As DmlAdapter In adapters
                            Call report.Adapters.Add(AdapterReport.FromAdapter(adapter))
                        Next
                    Finally
                        For Each adapter As DmlAdapter In adapters
                            adapter.Dispose()
                        Next
                    End Try
                End If
            Catch ex As Exception
                report.ErrorMessage = ex.Message
                report.IsAvailable = False
            End Try

            report.Suggestions = Suggest(report)

            Return report
        End Function

        ''' <summary>根据报告生成修复建议。</summary>
        Public Shared Function Suggest(report As EnvironmentReport) As List(Of FixSuggestion)
            Dim list As New List(Of FixSuggestion)()

            If report Is Nothing Then
                Return list
            End If

            If Not report.IsAvailable Then
                Call list.Add(New FixSuggestion(
                    "找不到可用的 DirectML/D3D12 运行时",
                    "确认系统为 Windows 10 1903 及以上；或把 Microsoft.AI.DirectML 分发目录中的 DirectML.dll 复制到程序目录"))

                Call list.Add(New FixSuggestion(
                    "DirectML.dll 不在默认搜索路径",
                    "使用 EngineOptions.DirectMLDirectory 指向 DirectML.dll 所在目录，或设置 DML_EXTRA_PATH 环境变量"))
            End If

            If report.IsAvailable AndAlso report.Adapters.Count = 0 Then
                Call list.Add(New FixSuggestion(
                    "没有支持 D3D12 的适配器",
                    "更新显卡驱动；或在无显卡环境下使用 WARP 软件适配器（DmlAdapter.Enumerate(includeSoftware:=True)）"))
            End If

            Return list
        End Function

        ''' <summary>生成可读文本。</summary>
        Public Function ToText() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine($"directml dll      : {If(DirectMLPath, "<not loaded>")}")
            Call sb.AppendLine($"d3d12 / dxgi      : {D3D12Available} / {DxgiAvailable}")

            If Not String.IsNullOrEmpty(ErrorMessage) Then
                Call sb.AppendLine($"error             : {ErrorMessage}")
            End If

            Call sb.AppendLine("library probes:")

            For Each attempt As NativeProbeAttempt In Attempts
                Call sb.AppendLine("  " & attempt.ToString())
            Next

            Call sb.AppendLine("adapters:")

            For Each adapter As AdapterReport In Adapters
                Call sb.AppendLine("  " & adapter.ToString())
            Next

            If Suggestions.Count > 0 Then
                Call sb.AppendLine("suggestions:")

                For Each suggestion As FixSuggestion In Suggestions
                    Call sb.AppendLine("  " & suggestion.ToString())
                Next
            End If

            Return sb.ToString()
        End Function

        Public Overrides Function ToString() As String
            Return ToText()
        End Function

    End Class

End Namespace
