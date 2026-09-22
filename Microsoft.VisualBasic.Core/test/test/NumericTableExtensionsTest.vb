#Region "Microsoft.VisualBasic::NumericTableExtensionsTest, Microsoft.VisualBasic.Core\test\test\NumericTableExtensionsTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Data
Imports Microsoft.VisualBasic.Data

''' <summary>
''' <see cref="NumericTableExtensions.dataframe(ArgumentReference())"/> 的回归测试。
''' 
''' 直接运行：
''' 
''' ```
''' test.exe --numeric-table
''' ```
''' </summary>
Friend Module NumericTableExtensionsTest

    ''' <summary>
    ''' 当前失败的断言数量
    ''' </summary>
    Private failedChecks As Integer = 0

    Private Enum TestColor As Integer
        Red = 1
        Green = 2
    End Enum

    Public Sub Run()
        failedChecks = 0

        Call TestDocumentExample()
        Call TestEnumColumn()
        Call TestCharFactor()
        Call TestDateAndTimeSpan()
        Call TestSequenceInput()
        Call TestLabelEncoding()
        Call TestErrorCases()
        Call TestEmptyInput()

        Console.WriteLine()

        If failedChecks = 0 Then
            Console.WriteLine("NumericTableExtensionsTest: all checks passed.")
        Else
            Throw New Exception($"NumericTableExtensionsTest: {failedChecks} check(s) failed!")
        End If
    End Sub

    ''' <summary>
    ''' 文档之中的示例：数值、布尔、因子以及 ``label:`` 前缀列
    ''' </summary>
    Private Sub TestDocumentExample()
        Console.WriteLine("[dataframe] document example")

        Dim table = dataframe(
            name("field1") = {1, 3, 5, 7, 9},
            name("field2") = {2, 4, 6, 8, 0},
            name("flags") = {True, False, True, True, False},
            name("factors") = {"a", "b", "c", "c", "b"},
            name("label:class") = {1, 2, 2, 2, 3}
        )

        Call AssertTrue(table.nsamples = 5, "nsamples = 5")
        Call AssertTrue(table.nfeatures = 6, "nfeatures = 6")
        Call AssertTrue(table.nlabels = 1, "nlabels = 1")
        Call AssertTrue(
            table.featureNames.SequenceEqual(New String() {"field1", "field2", "flags", "factors:a", "factors:b", "factors:c"}),
            "feature names, factor levels are ordered by alphabet"
        )
        Call AssertTrue(table.labelNames.SequenceEqual(New String() {"class"}), "label: prefix was removed from the label name")
        Call AssertTrue(table.RowNamesOrDefault().SequenceEqual(New String() {"1", "2", "3", "4", "5"}), "default row names are 1..n")

        Call AssertRow(table, 0, New Double() {1, 2, 1, 1, 0, 0}, "row 0")
        Call AssertRow(table, 1, New Double() {3, 4, 0, 0, 1, 0}, "row 1")
        Call AssertRow(table, 2, New Double() {5, 6, 1, 0, 0, 1}, "row 2")
        Call AssertRow(table, 3, New Double() {7, 8, 1, 0, 0, 1}, "row 3")
        Call AssertRow(table, 4, New Double() {9, 0, 0, 0, 1, 0}, "row 4")

        Call AssertTrue(table.GetLabel("class").SequenceEqual(New Double() {1, 2, 2, 2, 3}), "label class values")
    End Sub

    ''' <summary>
    ''' 枚举列按照底层基类型转成数值（option 1，不做 one-hot 展开）
    ''' </summary>
    Private Sub TestEnumColumn()
        Console.WriteLine("[dataframe] enum column -> base type to double")

        Dim table = dataframe(name("color") = {TestColor.Red, TestColor.Green, TestColor.Red})

        Call AssertTrue(table.nfeatures = 1, "enum column makes a single feature column")
        Call AssertTrue(table.featureNames.SequenceEqual(New String() {"color"}), "enum feature name")
        Call AssertTrue(table.Feature("color").SequenceEqual(New Double() {1, 2, 1}), "enum values -> base type double")
    End Sub

    ''' <summary>
    ''' 字符列作为分类因子进行 one-hot 展开
    ''' </summary>
    Private Sub TestCharFactor()
        Console.WriteLine("[dataframe] char column -> one-hot factor")

        Dim table = dataframe(name("ch") = {"a"c, "b"c, "a"c})

        Call AssertTrue(table.featureNames.SequenceEqual(New String() {"ch:a", "ch:b"}), "char factor names")
        Call AssertRow(table, 0, New Double() {1, 0}, "char factor row 0")
        Call AssertRow(table, 1, New Double() {0, 1}, "char factor row 1")
        Call AssertRow(table, 2, New Double() {1, 0}, "char factor row 2")
    End Sub

    ''' <summary>
    ''' 日期转换为 Unix 时间戳（秒），时间间隔转换为总秒数
    ''' </summary>
    Private Sub TestDateAndTimeSpan()
        Console.WriteLine("[dataframe] date -> unix timestamp, timespan -> total seconds")

        Dim dates = dataframe(name("t") = {
            New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            New Date(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        })

        Call AssertTrue(dates.Feature("t").SequenceEqual(New Double() {0, 86400}), "date values -> unix timestamp seconds")

        Dim spans = dataframe(name("s") = {TimeSpan.FromSeconds(1.5), TimeSpan.FromMinutes(1)})

        Call AssertTrue(spans.Feature("s").SequenceEqual(New Double() {1.5, 60}), "timespan values -> total seconds")
    End Sub

    ''' <summary>
    ''' 数组、List 以及 LINQ 枚举序列都可以作为列数据
    ''' </summary>
    Private Sub TestSequenceInput()
        Console.WriteLine("[dataframe] array / list / linq input")

        Dim fromArray = dataframe(name("v") = New Integer() {1, 2, 3})

        Call AssertTrue(fromArray.Feature("v").SequenceEqual(New Double() {1, 2, 3}), "integer array input")

        Dim fromList = dataframe(name("v") = New List(Of Double) From {1.5, 2.5, 3.5})

        Call AssertTrue(fromList.Feature("v").SequenceEqual(New Double() {1.5, 2.5, 3.5}), "List(Of Double) input")

        Dim fromLinq = dataframe(name("v") = Enumerable.Range(1, 3).Select(Function(i) CDbl(i * 2)))

        Call AssertTrue(fromLinq.Feature("v").SequenceEqual(New Double() {2, 4, 6}), "linq enumerable input")
    End Sub

    ''' <summary>
    ''' 标签列同样应用类型转换规则（含因子展开）
    ''' </summary>
    Private Sub TestLabelEncoding()
        Console.WriteLine("[dataframe] label column encoding")

        Dim table = dataframe(
            name("v") = {1, 2, 3},
            name("label:g") = {"x", "y", "x"}
        )

        Call AssertTrue(table.labelNames.SequenceEqual(New String() {"g:x", "g:y"}), "label factor names")
        Call AssertTrue(table.GetLabel("g:x").SequenceEqual(New Double() {1, 0, 1}), "label factor g:x")
        Call AssertTrue(table.GetLabel("g:y").SequenceEqual(New Double() {0, 1, 0}), "label factor g:y")
    End Sub

    ''' <summary>
    ''' 非法输入的异常路径
    ''' </summary>
    Private Sub TestErrorCases()
        Console.WriteLine("[dataframe] error cases")

        Call AssertThrows(
            Function() dataframe(name("a") = {1, 2}, name("b") = {1, 2, 3}),
            "mismatched column length"
        )
        Call AssertThrows(
            Function() dataframe(name("") = {1, 2}),
            "empty column name"
        )
        Call AssertThrows(
            Function() dataframe(name("label:") = {1, 2}),
            "label column with an empty label name"
        )
        Call AssertThrows(
            Function() dataframe(name("x") = {1, 2}, name("x") = {3, 4}),
            "duplicated feature column name"
        )
    End Sub

    ''' <summary>
    ''' 空输入以及全部为 Nothing 的列
    ''' </summary>
    Private Sub TestEmptyInput()
        Console.WriteLine("[dataframe] empty / missing values")

        Dim empty = dataframe()

        Call AssertTrue(empty IsNot Nothing AndAlso empty.nsamples = 0, "empty cols -> empty table")

        Dim blanks = dataframe(name("a") = New Object() {Nothing, Nothing})

        Call AssertTrue(blanks.nsamples = 2, "all-Nothing column keeps the sample size")
        Call AssertTrue(blanks.featureNames.SequenceEqual(New String() {"a"}), "all-Nothing column name")
        Call AssertTrue(Double.IsNaN(blanks.Feature("a")(0)) AndAlso Double.IsNaN(blanks.Feature("a")(1)), "all-Nothing column -> NaN")
    End Sub

    Private Sub AssertTrue(condition As Boolean, message As String)
        If condition Then
            Console.WriteLine($"  [PASS] {message}")
        Else
            Console.WriteLine($"  [FAIL] {message}")
            failedChecks += 1
        End If
    End Sub

    Private Sub AssertRow(table As NumericTable, row As Integer, expected As Double(), message As String)
        Call AssertTrue(table.features(row).SequenceEqual(expected), message)
    End Sub

    Private Sub AssertThrows(action As Func(Of Object), message As String)
        Try
            Call action()

            Call AssertTrue(False, message & " (no exception was thrown)")
        Catch ex As InvalidConstraintException
            Call AssertTrue(True, message)
        Catch ex As Exception
            Call AssertTrue(False, $"{message} (unexpected exception: {ex.GetType.Name})")
        End Try
    End Sub

End Module
