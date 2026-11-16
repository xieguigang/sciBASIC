' ------------------------------------------------------------------------
' 控制台输出层：框架只产出结构化数据，排版在这里完成
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Diagnostics
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Operators

Namespace DirectMLDemo

    Friend Module Reporter

        ''' <summary>打印一节标题。</summary>
        Public Sub Title(text As String)
            Call Console.WriteLine()
            Call Console.WriteLine(text)
            Call Console.WriteLine(New String("-"c, text.Length))
        End Sub

        ''' <summary>打印环境报告。</summary>
        Public Sub Environment(report As EnvironmentReport)
            Call Title("DirectML environment")
            Call Console.WriteLine(report.ToText())
        End Sub

        ''' <summary>打印能力矩阵。</summary>
        Public Sub Capabilities(levelText As String, Optional level As DML_FEATURE_LEVEL? = Nothing)
            Call Title($"ONNX operator mapping (device level {levelText})")
            Call Console.WriteLine(OperatorCatalog.Report(level))
        End Sub

        Public Sub Info(text As String)
            Call Console.WriteLine("  " & text)
        End Sub

        Public Sub Ok(text As String)
            Call Console.WriteLine("  [ok]   " & text)
        End Sub

        Public Sub Warn(text As String)
            Call Console.WriteLine("  [warn] " & text)
        End Sub

        Public Sub Skip(text As String)
            Call Console.WriteLine("  [skip] " & text)
        End Sub

        Public Sub Fail(text As String)
            Call Console.WriteLine("  [fail] " & text)
        End Sub

    End Module

End Namespace
