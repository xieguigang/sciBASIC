Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.DataStorage.Parquet.test
'  Data\BinaryData\Parquet\test\PyArrowInteropTests.vb
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json.Nodes
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' 与 python pyarrow 的文件格式对拍测试。
''' <para>
''' 1. 读取方向：由 <c>pyarrow/gen_fixture.py</c> 生成一批覆盖类型矩阵 / 编码 / 压缩 / 嵌套的
'''    <c>.parquet</c> 文件，本测试读取并逐值断言。<br/>
''' 2. 写入方向：把 <see cref="Frames"/> 构造的 DataFrame 写出为 <c>.parquet</c>，同时把期望值导出为
'''    json，再由 <c>pyarrow/verify_vb_output.py</c> 使用 pyarrow 解析并逐值比对。<br/>
''' 3. 嵌套方向：<c>pyarrow/verify_vb_nested.py</c> 校验 VB 写出的 LIST / MAP / struct 布局是否
'''    符合 parquet 标准（可被 pyarrow 正确还原）。
''' </para>
''' 当测试机器上没有 python 或者 pyarrow 无法安装时，相关用例会以 <c>SKIP</c> 记录下来，不会导致失败。
''' </summary>
Friend Module PyArrowInteropTests

    ''' <summary>
    ''' 默认的 python 解释器位置（anaconda 发行版）。
    ''' </summary>
    Private Const DefaultPython As String = "C:\ProgramData\anaconda3\python.exe"

    Private _python As String = Nothing
    Private _pythonProbed As Boolean = False

    ''' <summary>
    ''' pyarrow 生成的夹具文件：读取方向的对拍。
    ''' </summary>
    Friend Sub RunPyArrowFixtures()
        Dim python As String = EnsurePython()

        If python Is Nothing Then
            Return
        End If

        Dim dir As String = Path.Combine(TestSupport.TempDir(), "pyarrow-fixture")
        Dim script As String = ScriptPath("gen_fixture.py")

        If Not File.Exists(script) Then
            Call TestSupport.AssertTrue(False, $"缺少夹具生成脚本：{script}")
            Return
        End If

        Call Directory.CreateDirectory(dir)

        Dim created As ProcessResult = RunPython(python, {script, dir})

        Call TestSupport.AssertEqual(0, created.ExitCode, "pyarrow 夹具生成成功" & If(created.ExitCode = 0, "", "：" & created.Detail))

        If created.ExitCode <> 0 Then
            Return
        End If

        Call ReadFlatBasic(Path.Combine(dir, "flat_basic.parquet"), "flat_basic")
        Call ReadFlatBasic(Path.Combine(dir, "flat_plain.parquet"), "flat_plain")
        Call ReadFlatBasic(Path.Combine(dir, "flat_pagev2.parquet"), "flat_pagev2")
        Call ReadFlatBasic(Path.Combine(dir, "flat_gzip.parquet"), "flat_gzip")
        Call ReadFlatBasic(Path.Combine(dir, "flat_none.parquet"), "flat_none")
        Call ReadMultiRowGroup(Path.Combine(dir, "flat_multigroup.parquet"))
        Call ReadDelta(Path.Combine(dir, "flat_delta.parquet"))
        Call ReadDelta32(Path.Combine(dir, "flat_delta32.parquet"))
        Call ReadTypes(Path.Combine(dir, "flat_types.parquet"))
        Call ReadEmpty(Path.Combine(dir, "flat_empty.parquet"))
        Call ReadSingle(Path.Combine(dir, "flat_single.parquet"))
        Call ReadIndexed(Path.Combine(dir, "flat_indexed.parquet"))
        Call ReadNested(Path.Combine(dir, "nested.parquet"))
    End Sub

    ''' <summary>
    ''' VB 写出的文件：交给 pyarrow 解析并比对。
    ''' </summary>
    Friend Sub RunWriterInterop()
        Dim python As String = EnsurePython()

        If python Is Nothing Then
            Return
        End If

        Dim dir As String = Path.Combine(TestSupport.TempDir(), "vb-output")
        Dim file As String = Path.Combine(dir, "vb-flat.parquet")
        Dim expect As String = Path.Combine(dir, "vb-flat.expect.json")
        Dim nested As String = Path.Combine(dir, "vb-nested.parquet")

        Call Directory.CreateDirectory(dir)

        Dim df As DataFrame = Frames.MakeInterop()

        Call pq.write_table(df, file)
        Call System.IO.File.WriteAllText(expect, ToExpectJson(df), New UTF8Encoding(False))

        Call ReportCheckLines("write.parquet -> pyarrow",
                              RunPython(python, {ScriptPath("verify_vb_output.py"), file, expect}))

        ' 嵌套列的写出方向：只要 pyarrow 能够按照标准的 LIST / MAP / struct 布局解析即可
        Call pq.write_table(Frames.MakeNested(), nested)

        Call ReportCheckLines("write.parquet(nested) -> pyarrow",
                              RunPython(python, {ScriptPath("verify_vb_nested.py"), nested}))
    End Sub

    ' ------------------------------------------------------------------
    '  读取方向：pyarrow 写 -> VB 读
    ' ------------------------------------------------------------------

    Private Sub ReadFlatBasic(file As String, title As String)
        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(5, df.nsamples, $"{title}: 行数")
        Call TestSupport.AssertEqual(10, df.nfeatures, $"{title}: 列数")

        ' 含 null 的 INT32 列降级为 numeric + NaN
        Call TestSupport.AssertEqual(GetType(Double), TestSupport.ColumnArray(df, "int32_null").GetType.GetElementType,
                                     $"{title}: 含 null 的 int32 降级为 double")
        Call TestSupport.AssertArray(New Double() {1, 2, Double.NaN, 4, 5}, TestSupport.ColumnArray(df, "int32_null"),
                                     $"{title}: int32_null 数值")
        Call TestSupport.AssertArray(New Integer() {1, 2, 3, 4, 5}, TestSupport.ColumnArray(df, "int32"), $"{title}: int32 数值")
        Call TestSupport.AssertArray(New Long() {10, 20, 30, 40, 50}, TestSupport.ColumnArray(df, "int64"), $"{title}: int64 数值")
        Call TestSupport.AssertArray(New Single() {0.5, 1.5, 2.5, 3.5, 4.5}, TestSupport.ColumnArray(df, "float32"),
                                     $"{title}: float32 数值")
        Call TestSupport.AssertArray(New Double() {0.1, 0.2, 0.3, 0.4, 0.5}, TestSupport.ColumnArray(df, "float64"),
                                     $"{title}: float64 数值")
        Call TestSupport.AssertArray(New Boolean() {True, False, True, False, True}, TestSupport.ColumnArray(df, "logical"),
                                     $"{title}: logical 数值")
        Call TestSupport.AssertArray(New String() {"a", "b", Nothing, "", "e"}, TestSupport.ColumnArray(df, "text"),
                                     $"{title}: text 数值（null 与空串必须区分）")
        Call TestSupport.AssertArray(MakeDates(New Date(2020, 1, 1), 0), TestSupport.ColumnArray(df, "date"),
                                     $"{title}: date32 数值")
        Call TestSupport.AssertArray(MakeDates(New Date(2020, 1, 1), 1), TestSupport.ColumnArray(df, "ts_ms"),
                                     $"{title}: timestamp(ms) 数值")
        Call TestSupport.AssertArray(MakeMicros(), TestSupport.ColumnArray(df, "ts_us"), $"{title}: timestamp(us) 数值")
    End Sub

    ''' <summary>
    ''' 生成 <paramref name="start"/> 开始的连续日期，<paramref name="stepHours"/> 为 0 表示按天递增。
    ''' </summary>
    Private Function MakeDates(start As Date, stepHours As Integer) As Date()
        Dim data As Date() = New Date(4) {}

        For i As Integer = 0 To 4
            If stepHours = 0 Then
                data(i) = start.AddDays(i)
            Else
                data(i) = start.AddHours(i)
            End If
        Next

        Return data
    End Function

    Private Function MakeMicros() As Date()
        Dim data As Date() = New Date(4) {}
        Dim start As New Date(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc)

        For i As Integer = 0 To 4
            data(i) = start.AddSeconds(i).AddMilliseconds(250)
        Next

        Return data
    End Function

    Private Sub ReadMultiRowGroup(file As String)
        Dim table As ParquetTable = pq.read_parquet_table(file)

        Call TestSupport.AssertEqual(4, table.Metadata.RowGroups.Count, "multigroup: row group 数量")
        Call TestSupport.AssertEqual(Format.CompressionCodec.GZIP, table.Metadata.RowGroups(0).Columns(0).MetaData.Codec,
                                     "multigroup: 压缩算法为 GZIP")
        Call TestSupport.AssertEqual(10L, table.NumRows, "multigroup: 文件行数")

        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(10, df.nsamples, "multigroup: 合并后的行数")
        Call TestSupport.AssertArray(New Integer() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, TestSupport.ColumnArray(df, "id"),
                                     "multigroup: 跨行组的 id 数值")

        Dim text As String() = DirectCast(TestSupport.ColumnArray(df, "text"), String())

        Call TestSupport.AssertTrue(text(0) Is Nothing, "multigroup: 第 0 行文本为 null")
        Call TestSupport.AssertEqual("text_1", text(1), "multigroup: 第 1 行文本")
        Call TestSupport.AssertTrue(text(9) Is Nothing, "multigroup: 第 9 行文本为 null")
        Call TestSupport.AssertEqual("text_8", text(8), "multigroup: 第 8 行文本")
    End Sub

    Private Sub ReadDelta(file As String)
        Dim df As DataFrame = pq.read_table(file)
        Dim id As Long() = DirectCast(TestSupport.ColumnArray(df, "id"), Long())
        Dim expected As Long() = New Long(63) {}

        For i As Integer = 0 To 63
            expected(i) = 1000L + i * 7L
        Next

        Call TestSupport.AssertEqual(64, id.Length, "delta: 行数")
        Call TestSupport.AssertArray(expected, id, "delta: DELTA_BINARY_PACKED 解码数值")
        Call TestSupport.AssertEqual(31.5R, DirectCast(TestSupport.ColumnArray(df, "score"), Double())(63),
                                     "delta: 同表的 double 列")
    End Sub

    Private Sub ReadDelta32(file As String)
        Dim df As DataFrame = pq.read_table(file)
        Dim id As Long() = DirectCast(TestSupport.ColumnArray(df, "id"), Long())
        Dim expected As Long() = New Long(31) {}

        For i As Integer = 0 To 31
            expected(i) = If(i Mod 2 = 0, 0L, 2000000000L)
        Next

        Call TestSupport.AssertArray(expected, id, "delta32: bit width 为 32 的 DELTA_BINARY_PACKED 解码数值")
    End Sub

    Private Sub ReadTypes(file As String)
        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(5, df.nsamples, "types: 行数")
        Call TestSupport.AssertArray(New Integer() {1, 2, 200, 255, 0}, TestSupport.ColumnArray(df, "u8"), "types: uint8 数值")
        Call TestSupport.AssertArray(New Integer() {1, 2, 40000, 65535, 0}, TestSupport.ColumnArray(df, "u16"),
                                     "types: uint16 数值")
        Call TestSupport.AssertArray(New Long() {1, 2, 4000000000L, 4294967295L, 0}, TestSupport.ColumnArray(df, "u32"),
                                     "types: uint32 数值（不得溢出为负数）")
        Call TestSupport.AssertArray(New Double() {1, 2, 1.8E+19, CDbl(ULong.MaxValue), 0},
                                     TestSupport.ColumnArray(df, "u64"), "types: uint64 数值")
        Call TestSupport.AssertArray(New Integer() {-1, 0, 127, -128, 5}, TestSupport.ColumnArray(df, "i8"), "types: int8 数值")
        Call TestSupport.AssertArray(New Integer() {-1, 0, 32767, -32768, 5}, TestSupport.ColumnArray(df, "i16"),
                                     "types: int16 数值")
        Call TestSupport.AssertArray(New Double() {12.34, -0.01, 100.0, 0.1, 999.99}, TestSupport.ColumnArray(df, "amount"),
                                     "types: decimal128 数值")
        Call TestSupport.AssertArray(New String() {"AAEC", "", Nothing, "//4=", "UEFSMQ=="},
                                     TestSupport.ColumnArray(df, "payload"), "types: binary 列以 base64 文本读取")
    End Sub

    Private Sub ReadEmpty(file As String)
        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(0, df.nsamples, "empty: 行数为 0")
        Call TestSupport.AssertEqual(2, df.nfeatures, "empty: 空表保留列结构")
        Call TestSupport.AssertTrue(df.features.ContainsKey("id"), "empty: 保留 id 列")
        Call TestSupport.AssertTrue(df.features.ContainsKey("text"), "empty: 保留 text 列")
    End Sub

    Private Sub ReadSingle(file As String)
        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(1, df.nsamples, "single: 行数为 1")
        Call TestSupport.AssertArray(New Integer() {42}, TestSupport.ColumnArray(df, "id"), "single: id 数值")
        Call TestSupport.AssertArray(New String() {"only"}, TestSupport.ColumnArray(df, "text"), "single: text 数值")
    End Sub

    Private Sub ReadIndexed(file As String)
        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(4, df.nsamples, "indexed: 行数")
        Call TestSupport.AssertEqual(3, df.nfeatures, "indexed: 索引列不作为普通列")
        Call TestSupport.AssertFalse(df.features.ContainsKey("__index__"), "indexed: __index__ 列已还原为行名")
        Call TestSupport.AssertArray(New String() {"r0", "r1", "r2", "r3"}, df.rownames, "indexed: 行名还原")

        Dim table As ParquetTable = pq.read_parquet_table(file)
        Dim keep As DataFrame = DataFrameMapping.ToDataFrame(table.Schema, table.Columns, CInt(table.NumRows),
                                                            restoreRowNames:=False)

        Call TestSupport.AssertTrue(keep.features.ContainsKey("__index__"), "indexed: restoreRowNames=False 时保留索引列")
        Call TestSupport.AssertTrue(keep.rownames Is Nothing, "indexed: restoreRowNames=False 时不生成行名")
    End Sub

    Private Sub ReadNested(file As String)
        Dim df As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(5, df.nsamples, "nested: 行数")
        ' struct 列会被展开为 a.b 形式的点号列名
        Call TestSupport.AssertEqual(5, df.nfeatures, "nested: 列数（struct 展开为点号列名）")
        Call TestSupport.AssertTrue(df.features.ContainsKey("record.a"), "nested: struct 成员展开为 record.a")
        Call TestSupport.AssertTrue(df.features.ContainsKey("record.b"), "nested: struct 成员展开为 record.b")

        Dim values As String() = DirectCast(TestSupport.ColumnArray(df, "values"), String())
        Dim attrs As String() = DirectCast(TestSupport.ColumnArray(df, "attrs"), String())
        Dim points As String() = DirectCast(TestSupport.ColumnArray(df, "points"), String())
        Dim field_a As Double() = DirectCast(TestSupport.ColumnArray(df, "record.a"), Double())
        Dim field_b As Double() = DirectCast(TestSupport.ColumnArray(df, "record.b"), Double())

        Call TestSupport.AssertEqual(3, DirectCast(JsonNode.Parse(values(0)), JsonArray).Count, "nested: 列表元素数量")
        Call TestSupport.AssertEqual(0, DirectCast(JsonNode.Parse(values(2)), JsonArray).Count, "nested: 空列表")
        Call TestSupport.AssertEqual("null", values(3).Trim, "nested: null 列表")

        Dim first As JsonObject = DirectCast(JsonNode.Parse(attrs(0)), JsonObject)

        Call TestSupport.AssertEqual(2, first.Count, "nested: map 元素数量")
        Call TestSupport.AssertEqual(1L, LongOf(first("a")), "nested: map 键 a 的值")

        ' map 中 value 为 null 的条目必须保持位置正确（不得让后续 value 错位）
        Dim last As JsonObject = DirectCast(JsonNode.Parse(attrs(4)), JsonObject)

        Call TestSupport.AssertTrue(last.ContainsKey("e"), "nested: map 包含 key e")
        Call TestSupport.AssertTrue(last("e") Is Nothing, "nested: map 中 e 的 value 为 null")
        Call TestSupport.AssertEqual(4L, LongOf(last("d")), "nested: map 中 null value 之后的 d 值仍然正确")

        Dim structList As JsonArray = DirectCast(JsonNode.Parse(points(0)), JsonArray)

        Call TestSupport.AssertEqual(2, structList.Count, "nested: list<struct> 元素数量")
        Call TestSupport.AssertEqual(2.5R, DoubleOf(DirectCast(structList(1), JsonObject)("y")), "nested: list<struct> 的 y 字段")

        ' struct 整体为 null 时，其成员列降级为数值 + NaN
        Call TestSupport.AssertArray(New Double() {1, 2, 3, Double.NaN, 5}, field_a, "nested: struct 标量字段")
        Call TestSupport.AssertTrue(Double.IsNaN(field_b(1)), "nested: struct 内 null 字段以 NaN 表示")
        Call TestSupport.AssertEqual(5.25R, field_b(4), "nested: struct 浮点字段")
    End Sub

    ' ------------------------------------------------------------------
    '  写入方向：VB 写 -> pyarrow 读
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' 把 DataFrame 导出为 <c>verify_vb_output.py</c> 可以识别的期望值 json。
    ''' </summary>
    Private Function ToExpectJson(df As DataFrame) As String
        Dim sb As New StringBuilder()
        Dim names As New List(Of String)

        Call names.Add(ParquetConstants.IndexColumnName)
        Call names.AddRange(df.featureNames)

        Call sb.Append("{""rows"":").Append(df.nsamples).Append(",""columns"":[")

        Dim first As Boolean = True

        For Each name As String In names
            Dim column As Array
            Dim typeName As String

            If name = ParquetConstants.IndexColumnName Then
                column = df.rownames
                typeName = "string"
            Else
                column = df.features(name).vector
                typeName = ArrowTypeName(df.features(name).type)
            End If

            If Not first Then
                Call sb.Append(",")
            End If

            first = False

            Call sb.Append("{""name"":").Append(JsonText(name))
            Call sb.Append(",""type"":").Append(JsonText(typeName))
            Call sb.Append(",""values"":[")

            For i As Integer = 0 To column.Length - 1
                If i > 0 Then
                    Call sb.Append(",")
                End If

                Call sb.Append(JsonValue(column.GetValue(i)))
            Next

            Call sb.Append("]}")
        Next

        Call sb.Append("]}")

        Return sb.ToString()
    End Function

    Private Function ArrowTypeName(type As Type) As String
        If type Is GetType(Integer) OrElse type Is GetType(Short) OrElse type Is GetType(Byte) Then
            Return "int32"
        ElseIf type Is GetType(Long) Then
            Return "int64"
        ElseIf type Is GetType(Single) Then
            Return "float"
        ElseIf type Is GetType(Double) Then
            Return "double"
        ElseIf type Is GetType(Boolean) Then
            Return "bool"
        ElseIf type Is GetType(Date) Then
            Return "timestamp_ms"
        Else
            Return "string"
        End If
    End Function

    Private Function JsonValue(value As Object) As String
        If value Is Nothing Then
            Return "null"
        End If

        If TypeOf value Is String Then
            Return JsonText(DirectCast(value, String))
        End If

        If TypeOf value Is Boolean Then
            Return If(DirectCast(value, Boolean), "true", "false")
        End If

        If TypeOf value Is Double Then
            Dim number As Double = DirectCast(value, Double)

            If Double.IsNaN(number) Then
                ' R 语言的数值缺失值（NaN）在写入时会被表示为 null 槽位
                Return "null"
            ElseIf Double.IsPositiveInfinity(number) Then
                Return """Infinity"""
            ElseIf Double.IsNegativeInfinity(number) Then
                Return """-Infinity"""
            End If

            Return number.ToString("R", CultureInfo.InvariantCulture)
        End If

        If TypeOf value Is Single Then
            Dim number As Single = DirectCast(value, Single)

            If Single.IsNaN(number) Then
                Return """NaN"""

            End If

            Return CDbl(number).ToString("R", CultureInfo.InvariantCulture)
        End If

        If TypeOf value Is Date Then
            Dim moment As Date = DirectCast(value, Date)

            If moment.Kind = DateTimeKind.Unspecified Then
                moment = DateTime.SpecifyKind(moment, DateTimeKind.Utc)
            End If

            Return JsonText(moment.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture))
        End If

        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Function JsonText(text As String) As String
        If text Is Nothing Then
            Return "null"
        End If

        Dim sb As New StringBuilder()

        Call sb.Append("""")

        For Each ch As Char In text
            Select Case ch
                Case """"c
                    Call sb.Append("\""")
                Case "\"c
                    Call sb.Append("\\")
                Case ControlChars.Cr
                    Call sb.Append("\r")
                Case ControlChars.Lf
                    Call sb.Append("\n")
                Case ControlChars.Tab
                    Call sb.Append("\t")
                Case Else
                    Call sb.Append(ch)
            End Select
        Next

        Call sb.Append("""")

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 把 python 脚本输出的 <c>PASS</c>/<c>FAIL</c> 行转换为断言记录。
    ''' </summary>
    Private Sub ReportCheckLines(title As String, result As ProcessResult)
        Dim passed As Integer = 0
        Dim failed As Integer = 0

        For Each line As String In result.Output.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            If line.StartsWith("PASS ") Then
                passed += 1
            ElseIf line.StartsWith("FAIL ") Then
                failed += 1
                Call TestSupport.AssertTrue(False, $"{title}: {line}")
            End If
        Next

        Call DebugAudit.Mark($"{title}:passed", passed)
        Call TestSupport.AssertTrue(failed = 0 AndAlso passed > 0,
                                    $"{title}: 通过 {passed} 项，失败 {failed} 项{If(failed = 0, "", "：" & result.Detail)}")
    End Sub

    ' ------------------------------------------------------------------
    '  python 运行时辅助
    ' ------------------------------------------------------------------

    Private Function ScriptPath(name As String) As String
        Return Path.Combine(AppContext.BaseDirectory, "pyarrow", name)
    End Function

    ''' <summary>
    ''' 探测一个可用的 python 解释器，并且确保 pyarrow 已经安装。
    ''' </summary>
    Private Function EnsurePython() As String
        If _pythonProbed Then
            Return _python
        End If

        _pythonProbed = True

        Dim candidates As New List(Of String)
        Dim configured As String = Environment.GetEnvironmentVariable("PARQUET_PYTHON")

        If Not String.IsNullOrWhiteSpace(configured) Then
            Call candidates.Add(configured.Trim())
        End If

        Call candidates.Add(DefaultPython)
        Call candidates.Add("python")

        For Each candidate As String In candidates
            If candidate <> "python" AndAlso Not File.Exists(candidate) Then
                Continue For
            End If

            Dim probe As ProcessResult

            Try
                probe = RunPython(candidate, {"-c", "import pyarrow; print(pyarrow.__version__)"})
            Catch ex As Exception
                Continue For
            End Try

            If probe.ExitCode = 0 Then
                _python = candidate
                Call DebugAudit.Mark("pyarrow:version", probe.Output.Trim())
                Return _python
            End If

            ' 尝试自动安装 pyarrow
            Dim installed As ProcessResult = RunPython(candidate, {"-m", "pip", "install", "pyarrow"})

            If installed.ExitCode = 0 Then
                Dim retry As ProcessResult = RunPython(candidate, {"-c", "import pyarrow; print(pyarrow.__version__)"})

                If retry.ExitCode = 0 Then
                    _python = candidate
                    Call DebugAudit.Mark("pyarrow:version", retry.Output.Trim())
                    Return _python
                End If
            End If
        Next

        Call DebugAudit.Mark("pyarrow:skip", "没有找到可用的 python + pyarrow，跳过对拍测试")

        Return Nothing
    End Function

    Private Function RunPython(executable As String, arguments As String()) As ProcessResult
        Dim info As New ProcessStartInfo(executable) With {
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .UseShellExecute = False,
            .CreateNoWindow = True
        }

        For Each argument As String In arguments
            Call info.ArgumentList.Add(argument)
        Next

        Using process As Process = Process.Start(info)
            Dim stdout As String = process.StandardOutput.ReadToEnd()
            Dim stderr As String = process.StandardError.ReadToEnd()

            Call process.WaitForExit()

            Return New ProcessResult With {
                .ExitCode = process.ExitCode,
                .Output = stdout,
                .StdErr = stderr
            }
        End Using
    End Function

    Private Function LongOf(node As JsonNode) As Long
        If node Is Nothing Then
            Return Long.MinValue
        End If

        Return Long.Parse(node.ToJsonString(), CultureInfo.InvariantCulture)
    End Function

    Private Function DoubleOf(node As JsonNode) As Double
        If node Is Nothing Then
            Return Double.NaN
        End If

        Return Double.Parse(node.ToJsonString(), CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' 外部进程的执行结果。
    ''' </summary>
    Private Class ProcessResult

        Friend Property ExitCode As Integer
        Friend Property Output As String
        Friend Property StdErr As String

        Friend ReadOnly Property Detail As String
            Get
                Dim text As String = (If(StdErr, "") & vbCrLf & If(Output, "")).Trim()

                If text.Length > 900 Then
                    Return text.Substring(0, 900) & "..."
                End If

                Return text
            End Get
        End Property

    End Class

End Module
