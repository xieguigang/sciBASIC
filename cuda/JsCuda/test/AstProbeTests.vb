Imports Microsoft.VisualBasic.Computing.JSCuda
Imports Xunit

''' <summary>Dumps the generated .cu text for inspection.</summary>
Public Class AstProbeTests

    <Fact>
    Public Sub Probe_DumpSources()
        Dim file = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "jscuda-probe.txt")
        Dim lines As New List(Of String)()

        Call Probe(lines, "cell",
            "function cell(i, j, dot, rowSumSq, rows) {
                const n = rows;
                let c = dot[i * n + j];
                if (c > 1) { c = 1; }
                if (c < -1) { c = -1; }
                return c;
            }")
        Call Probe(lines, "square", "const square = (x) => x * x;")
        Call Probe(lines, "loop",
            "function rowSum(i, x, cols) {
                let s = 0;
                for (let k = 0; k < cols; k++) { s = s + x[i * cols + k]; }
                return s;
            }")
        Call Probe(lines, "while",
            "function f(i, x) { let k = i; while (k > 0) { k = k - 1; } return k; }")
        Call Probe(lines, "modulo", "function f(i, x) { const k = i % 4; return x[k]; }")
        Call Probe(lines, "helper",
            "function helper(v) { return v; }
             function f(i, x) { return helper(x[i]); }")

        System.IO.File.WriteAllLines(file, lines)
        Assert.True(System.IO.File.Exists(file), file)
    End Sub

    Private Shared Sub Probe(lines As List(Of String), name As String, source As String)
        Call lines.Add("==== " & name & " ====")

        Try
            Call lines.Add(JsCudaTranslator.TranslateSource(source).Source)
        Catch ex As Exception
            Call lines.Add("!! " & ex.GetType().Name & ": " & ex.Message)
        End Try
    End Sub

End Class
