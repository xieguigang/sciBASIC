Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Excel = Microsoft.VisualBasic.Data.csv.IO.DataSet

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    ''' <summary>
    ''' target应该是只有0和非零这两种结果的
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/foldchange")>
    <Usage("/foldchange /in <dataset.Xml> [/out <result.csv>]")>
    Public Function AnalysisFoldChange(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.foldchange.csv"
        Dim dataset As DataSet = [in].LoadXml(Of DataSet)
        Dim result As New Dictionary(Of String, Excel)
        Dim targetName$
        Dim vectorGroup As IGrouping(Of String, Sample)()
        Dim zero, nonZero As IGrouping(Of String, Sample)
        Dim namesOf = dataset.NormalizeMatrix.names

        Call namesOf _
            .DoEach(Sub(name)
                        Call result.Add(name, New Excel With {.ID = name})
                    End Sub)

        For i As Integer = 0 To dataset.output.Length - 1
            targetName = dataset.output(i)
            vectorGroup = dataset.DataSamples _
                .AsEnumerable _
                .GroupBy(Function(d)
                             If d.target(i) = 0.0 Then
                                 Return "0"
                             Else
                                 Return "1"
                             End If
                         End Function) _
                .ToArray

            zero = vectorGroup.FirstOrDefault(Function(g) g.Key = "0")
            nonZero = vectorGroup.FirstOrDefault(Function(g) g.Key = "1")

            For j As Integer = 0 To namesOf.Length - 1
                Dim A As Double() = zero.Select(Function(d) d.status(j)).ToArray
                Dim B As Double() = nonZero.Select(Function(d) d.status(j)).ToArray
                Dim foldchange# = B.Average / A.Average
                Dim pvalue# = t.Test(A, B).Pvalue

                result(namesOf(j)).Add($"FoldChange(Of {targetName})", foldchange)
                result(namesOf(j)).Add($"Log2FC(Of {targetName})", Math.Log(foldchange, 2))
                result(namesOf(j)).Add($"p.value(Of {targetName})", pvalue)
            Next
        Next

        Return result.Values.SaveTo(out).CLICode
    End Function
End Module
