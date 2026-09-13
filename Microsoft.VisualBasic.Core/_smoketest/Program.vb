Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

''' <summary>
''' NumericTable 二进制读写（NetworkByteOrderBuffer 大端）的自检程序。
''' </summary>
Module Program

    Private failures As Integer = 0

    Sub Main()
        Call Console.WriteLine("== NumericTable binary IO smoketest ==")

        Dim table As NumericTable = BuildTable()

        Call Section("source table")
        Call Check(table.nsamples = 4 AndAlso table.nfeatures = 3 AndAlso table.nlabels = 2,
                   $"table dimension = [{table.nsamples} x {table.nfeatures} + {table.nlabels}]")

        ' ---------- 普通二进制（文件往返） ----------
        Call Section("binary file round-trip")
        Dim file As String = Path.Combine(Path.GetTempPath(), $"numerictable_smoketest_{Guid.NewGuid().ToString("N")}.bin")

        Call Check(table.WriteBinary(file), $"WriteBinary -> {file}")

        Dim loaded As NumericTable = NumericTable.LoadBinary(file)

        Call CheckComparable(table, loaded, "plain file")

        ' ---------- gzip 压缩（内存流往返） ----------
        Call Section("gzip payload round-trip")
        Dim ms As New MemoryStream

        Call table.WriteBinary(ms, gzip:=True)

        Dim gzipBytes As Byte() = ms.ToArray()

        Call Check(gzipBytes.Length > 0, $"gzip binary size = {gzipBytes.Length} bytes")

        Dim gzipLoaded As NumericTable = NumericTable.LoadBinary(New MemoryStream(gzipBytes))

        Call CheckComparable(table, gzipLoaded, "gzip stream")

        ' ---------- 无标签表 ----------
        Call Section("labels is nothing")
        Dim noLabel As New NumericTable(CopyMatrix(table.features), table.rowNames, table.featureNames) With {
            .name = "no-label-table"
        }
        Dim noLabelLoaded As NumericTable = NumericTable.LoadBinary(New MemoryStream(ToBytes(noLabel)))

        Call Check(noLabelLoaded.nsamples = 4, $"loaded samples = {noLabelLoaded.nsamples}")
        Call Check(noLabelLoaded.labels Is Nothing, "loaded labels is Nothing")
        Call Check(noLabelLoaded.labelNames Is Nothing, "loaded labelNames is Nothing")
        Call Check(noLabelLoaded.name = "no-label-table", $"loaded table name = {noLabelLoaded.name}")
        Call Check(SameMatrix(noLabel.features, noLabelLoaded.features), "feature matrix is identical")

        ' ---------- 数值确实是网络字节序 ----------
        Call Section("network byte order (big endian)")
        Dim probe As Double() = {1.0}
        Dim raw As Byte() = New NetworkByteOrderBuffer().GetBytes(probe)

        ' 1.0 的 IEEE754 大端表示是 3F F0 00 00 00 00 00 00
        Call Check(raw(0) = &H3F AndAlso raw(1) = &HF0, $"encoded 1.0 -> {raw(0).ToString("X2")} {raw(1).ToString("X2")} ...")

        ' ---------- 损坏文件的容错 ----------
        Call Section("error handling")
        Dim payload As Byte() = ToBytes(table)

        Call Check(RejectsBadMagic(payload), "a corrupted magic header is rejected with InvalidDataException")
        Call Check(RejectsBadVersion(payload), "an unsupported format version is rejected with InvalidDataException")
        Call Check(RejectsTruncated(payload), "a truncated stream is rejected with InvalidDataException")

        Call System.IO.File.Delete(file)

        Call Console.WriteLine()
        Call Console.WriteLine($"{(If(failures = 0, "PASS", "FAIL"))}: {failures} failure(s)")

        Environment.ExitCode = If(failures = 0, 0, 1)
    End Sub

    ''' <summary>
    ''' 构造一个包含 NaN 与负值的测试用二维表对象
    ''' </summary>
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
            .description = "二进制往返测试 round-trip ✓"
        }

        Return table
    End Function

    Private Function ToBytes(table As NumericTable) As Byte()
        Dim ms As New MemoryStream

        Call table.WriteBinary(ms)

        Return ms.ToArray
    End Function

    Private Sub CheckComparable(source As NumericTable, loaded As NumericTable, tag As String)
        Call Check(loaded IsNot Nothing, $"[{tag}] the table object was loaded")
        Call Check(loaded.name = source.name, $"[{tag}] name = {loaded.name}")
        Call Check(loaded.description = source.description, $"[{tag}] description = {loaded.description}")
        Call Check(loaded.nsamples = source.nsamples, $"[{tag}] nsamples = {loaded.nsamples}")
        Call Check(loaded.nfeatures = source.nfeatures, $"[{tag}] nfeatures = {loaded.nfeatures}")
        Call Check(loaded.nlabels = source.nlabels, $"[{tag}] nlabels = {loaded.nlabels}")
        Call Check(SameStrings(source.rowNames, loaded.rowNames), $"[{tag}] rowNames = {String.Join(", ", loaded.rowNames)}")
        Call Check(SameStrings(source.featureNames, loaded.featureNames), $"[{tag}] featureNames = {String.Join(", ", loaded.featureNames)}")
        Call Check(SameStrings(source.labelNames, loaded.labelNames), $"[{tag}] labelNames = {String.Join(", ", loaded.labelNames)}")
        Call Check(SameMatrix(source.features, loaded.features), $"[{tag}] feature matrix is identical (including the NaN cell)")
        Call Check(SameMatrix(source.labels, loaded.labels), $"[{tag}] label matrix is identical")
    End Sub

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

    ''' <summary>
    ''' NaN 与 NaN 应当是相等的（按位往返）
    ''' </summary>
    Private Function SameValue(a As Double, b As Double) As Boolean
        If Double.IsNaN(a) Then
            Return Double.IsNaN(b)
        ElseIf Double.IsNaN(b) Then
            Return False
        Else
            Return a = b
        End If
    End Function

    Private Function CopyMatrix(mat As Double()()) As Double()()
        Dim copy As Double()() = New Double(mat.Length - 1)() {}

        For i As Integer = 0 To mat.Length - 1
            copy(i) = DirectCast(mat(i).Clone(), Double())
        Next

        Return copy
    End Function

    Private Function RejectsBadMagic(payload As Byte()) As Boolean
        Dim bad As Byte() = DirectCast(payload.Clone(), Byte())

        bad(0) = 0

        Return IsInvalidData(Function() NumericTable.LoadBinary(New MemoryStream(bad)))
    End Function

    Private Function RejectsBadVersion(payload As Byte()) As Boolean
        Dim bad As Byte() = DirectCast(payload.Clone(), Byte())

        ' 魔数为 7 个字节，后面紧跟大端 Int32 的版本号：把版本号改成 99
        bad(10) = 99

        Return IsInvalidData(Function() NumericTable.LoadBinary(New MemoryStream(bad)))
    End Function

    Private Function RejectsTruncated(payload As Byte()) As Boolean
        Dim bad As Byte() = New Byte(9) {}

        Call Array.ConstrainedCopy(payload, 0, bad, 0, bad.Length)

        Return IsInvalidData(Function() NumericTable.LoadBinary(New MemoryStream(bad)))
    End Function

    Private Function IsInvalidData(action As Func(Of NumericTable)) As Boolean
        Try
            Dim t As NumericTable = action()
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
