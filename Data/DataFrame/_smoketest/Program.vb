Imports System
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework

''' <summary>
''' NumericTable 的 csv/tsv 文本读写与 DataFrame 双向转换的自检程序。
''' </summary>
Module Program

    Private failures As Integer = 0

    Sub Main()
        Call Console.WriteLine("== NumericTable text IO / DataFrame conversion smoketest ==")

        Dim table As NumericTable = BuildTable()

        Call Section("source table")
        Call Check(table.nsamples = 4 AndAlso table.nfeatures = 3 AndAlso table.nlabels = 2,
                   $"table dimension = [{table.nsamples} x {table.nfeatures} + {table.nlabels}]")

        ' ---------- csv 文件往返 ----------
        Call Section("csv file round-trip")
        Dim csv As String = Path.Combine(Path.GetTempPath(), $"numerictable_smoketest_{Guid.NewGuid().ToString("N")}.csv")

        Call Check(table.WriteCsv(csv), $"WriteCsv -> {csv}")

        Dim header As String = System.IO.File.ReadAllLines(csv)(0)

        Call Check(header = ",f1,f2,f3,label:cluster,label:score", $"csv header = '{header}'")

        Dim csvLoaded As NumericTable = NumericTableIO.ReadCsv(csv)

        Call CheckComparable(table, csvLoaded, "csv")

        ' ---------- tsv 流往返 ----------
        Call Section("tsv stream round-trip")
        Dim ms As New MemoryStream

        Call table.WriteTsv(ms)

        Dim tsvText As String = Encoding.UTF8.GetString(ms.ToArray())
        Dim tsvHeader As String = tsvText.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)(0)

        Call Check(tsvHeader = "" & vbTab & "f1" & vbTab & "f2" & vbTab & "f3" & vbTab & "label:cluster" & vbTab & "label:score",
                   $"tsv header = '{tsvHeader.Replace(vbTab, "\t")}'")

        Dim tsvLoaded As NumericTable = NumericTableIO.ReadTsv(New MemoryStream(ms.ToArray()))

        Call CheckComparable(table, tsvLoaded, "tsv")

        ' ---------- DataFrame 双向转换 ----------
        Call Section("dataframe conversion")
        Dim df As DataFrame = table.AsDataFrame()

        Call Check(df.nsamples = 4, $"dataframe rows = {df.nsamples}")
        Call Check(df.nfeatures = 5, $"dataframe columns = {df.nfeatures} ({String.Join(", ", df.featureNames)})")
        Call Check(df.featureNames.Contains("label:cluster"), "the label column is prefixed in the dataframe")

        Dim back As NumericTable = df.AsNumericTable()

        Call CheckComparable(table, back, "dataframe", withMeta:=True)

        ' ---------- labels 参数叠加 ----------
        Call Section("labels parameter")
        Dim extra As NumericTable = NumericTableIO.ReadCsv(csv, labels:={"f1"})

        Call Check(extra.nfeatures = 2, $"feature columns = {extra.nfeatures} ({String.Join(", ", extra.featureNames)})")
        Call Check(extra.nlabels = 3, $"label columns = {extra.nlabels} ({String.Join(", ", extra.labelNames)})")
        Call Check(extra.nlabels = 3 AndAlso extra.labelNames.Contains("f1"), "the 'f1' column was moved into the label matrix")
        ' 对比源表的 f1 列（注意矩阵是按行主序存放的：labels(i)(0) 才是第 0 列的数值）
        Dim f1 As Double() = Enumerable.Range(0, table.nsamples).Select(Function(i) table.features(i)(0)).ToArray()
        Dim moved As Double() = Enumerable.Range(0, extra.nsamples).Select(Function(i) extra.labels(i)(0)).ToArray()

        Call Check(SameVector(f1, moved), "the moved column keeps its values")

        ' ---------- 数值解析策略 ----------
        Call Section("numeric parsing policy")
        Dim dirty As String = ",f1,f2" & vbCrLf & "s1,1.5,abc" & vbCrLf & "s2,2.5,3.5" & vbCrLf

        Call Check(ThrowsInvalidData(Sub() Call NumericTableIO.ReadCsv(TextStream(dirty))),
                   "strict mode reports a clear error for a non-numeric cell")

        Dim lenient As NumericTable = NumericTableIO.ReadCsv(TextStream(dirty), strict:=False)

        Call Check(lenient.nsamples = 2, $"lenient mode loaded {lenient.nsamples} rows")
        Call Check(Double.IsNaN(lenient.features(0)(1)), "the unparsable cell becomes Double.NaN in lenient mode")

        Dim naTable As NumericTable = NumericTableIO.ReadCsv(TextStream(",f1" & vbCrLf & "s1,NA" & vbCrLf))

        Call Check(Double.IsNaN(naTable.features(0)(0)), "an 'NA' cell becomes Double.NaN")

        Call System.IO.File.Delete(csv)

        Call Console.WriteLine()
        Call Console.WriteLine($"{(If(failures = 0, "PASS", "FAIL"))}: {failures} failure(s)")

        Environment.ExitCode = If(failures = 0, 0, 1)
    End Sub

    Private Function BuildTable() As NumericTable
        Dim features As Double()() = {
            New Double() {1.5, -2.25, 0.0},
            New Double() {3.75, 4.5, -5.125},
            New Double() {Double.NaN, 6.5, 7.25},
            New Double() {-8.0, 9.5, 10.75}
        }
        Dim labels As Double()() = {
            New Double() {1.0, 0.1},
            New Double() {1.0, 0.2},
            New Double() {2.0, 0.3},
            New Double() {2.0, 0.4}
        }
        Dim table As New NumericTable(features,
                                     New String() {"s1", "s2", "s3", "s4"},
                                     New String() {"f1", "f2", "f3"}) With {
            .labels = labels,
            .labelNames = New String() {"cluster", "score"},
            .name = "smoketest-table",
            .description = "文本往返测试 round-trip"
        }

        Return table
    End Function

    Private Function TextStream(text As String) As Stream
        Return New MemoryStream(New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False).GetBytes(text))
    End Function

    Private Sub CheckComparable(source As NumericTable, loaded As NumericTable, tag As String, Optional withMeta As Boolean = False)
        Call Check(loaded IsNot Nothing, $"[{tag}] the table object was loaded")
        Call Check(loaded.nsamples = source.nsamples, $"[{tag}] nsamples = {loaded.nsamples}")
        Call Check(loaded.nfeatures = source.nfeatures, $"[{tag}] nfeatures = {loaded.nfeatures}")
        Call Check(loaded.nlabels = source.nlabels, $"[{tag}] nlabels = {loaded.nlabels}")
        Call Check(SameStrings(source.rowNames, loaded.rowNames), $"[{tag}] rowNames = {String.Join(", ", loaded.rowNames)}")
        Call Check(SameStrings(source.featureNames, loaded.featureNames), $"[{tag}] featureNames = {String.Join(", ", loaded.featureNames)}")
        Call Check(SameStrings(source.labelNames, loaded.labelNames), $"[{tag}] labelNames = {String.Join(", ", loaded.labelNames)}")
        Call Check(SameMatrix(source.features, loaded.features), $"[{tag}] feature matrix is identical (including the NaN cell)")
        Call Check(SameMatrix(source.labels, loaded.labels), $"[{tag}] label matrix is identical")

        If withMeta Then
            Call Check(loaded.name = source.name, $"[{tag}] name = {loaded.name}")
            Call Check(loaded.description = source.description, $"[{tag}] description = {loaded.description}")
        End If
    End Sub

    Private Function SameVector(a As Double(), b As Double()) As Boolean
        If a Is Nothing OrElse b Is Nothing Then
            Return a Is Nothing AndAlso b Is Nothing
        End If
        If a.Length <> b.Length Then
            Return False
        End If

        For i As Integer = 0 To a.Length - 1
            If Not SameValue(a(i), b(i)) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function SameStrings(a As String(), b As String()) As Boolean
        If a Is Nothing OrElse b Is Nothing Then
            Return a Is Nothing AndAlso b Is Nothing
        End If
        If a.Length <> b.Length Then
            Return False
        End If

        For i As Integer = 0 To a.Length - 1
            If Not String.Equals(a(i), b(i), StringComparison.Ordinal) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function SameMatrix(a As Double()(), b As Double()()) As Boolean
        If a Is Nothing OrElse b Is Nothing Then
            Return a Is Nothing AndAlso b Is Nothing
        End If
        If a.Length <> b.Length Then
            Return False
        End If

        For i As Integer = 0 To a.Length - 1
            If a(i) Is Nothing OrElse b(i) Is Nothing Then
                If Not (a(i) Is Nothing AndAlso b(i) Is Nothing) Then
                    Return False
                End If
            ElseIf a(i).Length <> b(i).Length Then
                Return False
            Else
                For j As Integer = 0 To a(i).Length - 1
                    If Not SameValue(a(i)(j), b(i)(j)) Then
                        Return False
                    End If
                Next
            End If
        Next

        Return True
    End Function

    Private Function SameValue(a As Double, b As Double) As Boolean
        If Double.IsNaN(a) Then
            Return Double.IsNaN(b)
        ElseIf Double.IsNaN(b) Then
            Return False
        Else
            Return a = b
        End If
    End Function

    Private Function ThrowsInvalidData(action As Action) As Boolean
        Try
            Call action()
            Return False
        Catch ex As Exception
            Return TypeOf ex Is InvalidDataException
        End Try
    End Function

    Private Sub Section(title As String)
        Call Console.WriteLine()
        Call Console.WriteLine($"-- {title} --")
    End Sub

    Private Sub Check(condition As Boolean, message As String)
        If condition Then
            Call Console.WriteLine($"   [ok]   {message}")
        Else
            failures += 1
            Call Console.WriteLine($"   [FAIL] {message}")
        End If
    End Sub
End Module
