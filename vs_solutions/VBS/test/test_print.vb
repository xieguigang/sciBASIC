' =============================================================
' demo: print - 以 GNU R 的向量/表格格式打印数据(脚本调试用)
'
'   vbs ./test/test_print.vb
'
' 覆盖: 基础数据元素类型(integer/byte/short/long/uint/ulong/single/
'       double/boolean/string/char) + enum + NumericTable
'
' 向量格式与 GNU R 一致:
'   [起始下标] + 元素对齐(字符串左对齐 / 数值右对齐) + 超宽自动折行
'       [1]  1  3 24 23  4 23
' =============================================================

Imports Microsoft.VisualBasic.Data

Enum Gender
    Male
    Female
    Unknown
End Enum

' ---- 数值向量: 与 GNU R 的 print(c(1,3,24,23,4,23)) 输出完全一致 ----
Dim x = {1, 3, 24, 23, 4, 23}
Call print(x)                                                       ' [1]  1  3 24 23  4 23

Dim vector As New List(Of Double) From {1, 3, 24, 23, 4, 23}
Call print(vector)                                                  ' [1]  1  3 24 23  4 23

' ---- 各个基础数值类型 ----
Call print(New Byte() {1, 2, 255})                                  ' [1]   1   2 255
Call print(New Short() {-1, 2, 3})                                  ' [1] -1  2  3
Call print(New Long() {1, 2, 3000000000})                           ' [1]          1          2 3000000000
Call print(New UInteger() {1, 2, 3})                                ' [1] 1 2 3
Call print(New ULong() {1, 2, 3})                                   ' [1] 1 2 3

' ---- 浮点数: 按 R 的 options(digits=7) 截断有效位 ----
Call print(New Single() {1.1, 2.25, 1 / 3})                         ' [1]       1.1      2.25 0.3333333
Call print(New Double() {1 / 3, Math.Sqrt(2), 100000000.0})         ' [1] 0.3333333  1.414214    1e+08
Call print(New Double() {Double.NaN, Double.PositiveInfinity})      ' [1] NaN Inf

' ---- 逻辑 / 字符串 / 字符 ----
Call print(New Boolean() {True, False, True})                       ' [1]  True False  True
Call print(New String() {"asuka", "xie", "a"})                      ' [1] "asuka" "xie"   "a"
Call print(New Char() {"a"c, "b"c})                                 ' [1] "a" "b"

' ---- 枚举: ToString 之后按字符串向量打印 ----
Dim genders = {Gender.Male, Gender.Female, Gender.Unknown}
Call print(genders)                                                 ' [1] "Male"    "Female"  "Unknown"

' ---- 标量 / Nothing ----
Call print(42)                                                      ' [1] 42
Call print(1.0 / 3)                                                 ' [1] 0.3333333
Call print("done")                                                  ' [1] "done"
Call print(True)                                                    ' [1] True

Dim missing As New List(Of Object) From {1, Nothing, 3}
Call print(missing)                                                 ' [1]  1 NA  3

Dim nothingTable As NumericTable = Nothing
Call print(nothingTable)                                            ' NULL

' ---- 空集合: 按 R 的 numeric(0) / character(0) 形式打印 ----
Dim emptyNums As Double() = {}
Dim emptyTexts As String() = {}
Call print(emptyNums)                                               ' numeric(0)
Call print(emptyTexts)                                              ' character(0)

' ---- 长向量: 超过宽度(缺省 80 字符)以后自动折行并打印续行起始下标 ----
Dim seq = Enumerable.Range(1, 40)
Call print(seq)
'  [1]  1  2  3  4  5 ... 25
' [26] 26 27 28 29 30 ... 40

' width 与 digits 都是具名可选参数
Call print(seq, width:=24)
'  [1]  1  2  3  4  5  6
'  [7]  7  8  9 10 11 12
' [13] 13 14 15 16 17 18
' ...

Call print(New Double() {1 / 3, Math.PI}, digits:=3)                ' [1] 0.333  3.14

' ---- 嵌套集合: 按 R 的 list 格式打印 ----
Dim matrix = {New Double() {1, 2}, New Double() {3, 4}}
Call print(matrix)
' [[1]]
' [1] 1 2
'
' [[2]]
' [1] 3 4

' ---- NumericTable: 带边框的表格(列头 = 特征列 + 标签列, 首列为行名) ----
Dim tbl = NumericTable.FromRows(
    {"s1", "s2", "s3"},
    {New Double() {1, 4}, New Double() {2, 5}, New Double() {3, 6}},
    {"x", "y"})

Call tbl.SetLabel("cluster", {0, 0, 1})

Call Console.WriteLine("NumericTable(强类型变量):")
Call print(tbl)

Call Console.WriteLine("NumericTable(let 动态变量):")
let dynamicTable = tbl
Call print(dynamicTable)

' ---- 也可以只取字符串形式, 自行决定输出方式 ----
Call Console.WriteLine("ToText: " & Microsoft.VisualBasic.Printing.Print.ToText(New Double() {1, 2, 3}))
