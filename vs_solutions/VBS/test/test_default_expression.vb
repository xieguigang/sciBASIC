' =============================================================
' demo: 默认参数表达式(非常数默认参数)
'
'   vbs ./test/test_default_expression.vb
'   vbs ./test/test_default_expression.vb --verbose
'   vbs make-project ./test/test_default_expression.vb
'
' VB.NET 只允许把**常数或常数表达式**作为可选参数的默认值; 脚本引擎会在
' 编译之前把「非常数默认值表达式」改写成等价且可以被 Roslyn 编译的代码:
'
'   1. 声明改写:
'        Optional c As testdata = If(b, New testdata(a), New testdata("default"))
'        => Optional c As testdata = Nothing
'
'   2. Function 的调用点 => 按需生成桥接函数, 调用点只替换被调用名
'        Function test_defaults(a As String, b As Boolean) As String
'            Dim b = True
'            Dim c = If(b, New testdata(a), New testdata("default"))
'            Return test(a, b, c)
'        End Function
'      (实参表原样保留, 因此 While / If 条件 / 嵌套表达式等任意位置都成立)
'
'   3. Sub 的调用点 => 就地展开为多行临时变量(Sub 调用只能是语句)
'        Call emit(a:="inline")
'        =>  Dim __vbs_emit_a_1 = "inline"
'            Dim __vbs_emit_c_2 = New testdata(__vbs_emit_a_1)
'            Call emit(__vbs_emit_a_1, __vbs_emit_c_2)
'      单行 Sub lambda 与单行 If ... Then 会先被展开为块, 再插入临时变量行。
' =============================================================

Class testdata
    Public text As String

    Sub New(text As String)
        Me.text = text
    End Sub

    Public Overrides Function ToString() As String
        Return text
    End Function
End Class

' 模块级变量: 默认值表达式可以直接引用它
Dim prefix = "[vbs] "

' ---- 非常数默认值引用更靠前的参数(b 是常量默认值, c 是非常数默认值) ----
Function test(a As String, Optional b As Boolean = True, Optional c As testdata = If(b, New testdata(a), New testdata("default"))) As String
    Return c.text
End Function

' ---- 默认值表达式引用模块级变量 ----
Function greet(name As String, Optional msg As String = prefix & name) As String
    Return msg
End Function

' ---- 常量默认值(b)与非常数默认值(c)混用 ----
' 改写之后 b 的常量默认值会被搬到桥接函数里(Dim b = True), 语义不变。
Function withConst(a As String, Optional n As Integer = 42, Optional c As testdata = New testdata(a & "/" & n)) As String
    Return c.text
End Function

' ---- Sub 的非常数默认值: 调用点就地展开 ----
Sub emit(a As String, Optional c As testdata = New testdata(a))
    Call Console.WriteLine("  emit: a=" & a & ", c=" & c.text)
End Sub

' =============================================================
' 1. Function: 具名实参子集 => 桥接函数
' =============================================================
Call Console.WriteLine("---- 1. Function 桥接函数 ----")
Call print(test(a:="xxxxx", b:=False))
Call print(test(a:="hello"))

' 跳过中间的可选参数 b(只给 a 与 c)
Call print(test(a:="skip", c:=New testdata("given")))

' 全部参数都给了 => 不需要改写
Call print(test("all", False, New testdata("explicit")))

' 出现在嵌套表达式之中
Call print("nested: " & test(a:="nested"))

' 出现在 If 条件之中
If test(a:="cond") = "cond" Then
    Call Console.WriteLine("  if 条件中的调用 OK")
End If

' 出现在 While 条件之中
Dim count = 0

While test(a:="loop").Length > 0 AndAlso count < 1
    count += 1
End While
Call Console.WriteLine("  while 条件中的调用 OK")

' =============================================================
' 2. Sub: 调用点就地展开为多行临时变量
' =============================================================
Call Console.WriteLine("---- 2. Sub 就地展开 ----")
Call emit(a:="inline-named")
Call emit("inline-positional")

' 多行 lambda 体内: 与普通语句一致
Dim outer = Sub()
                Call emit(a:="from-multiline-lambda")
            End Sub
Call outer()

' 单行 Sub lambda: 先展开为多行 lambda 块
Dim handler = Sub() Call emit(a:="from-lambda")
Call handler()

' 单行 If ... Then: 先展开为 If 块
If True Then Call emit(a:="from-ifthen")

' =============================================================
' 3. 默认值表达式引用模块级变量 / 常量默认值
' =============================================================
Call Console.WriteLine("---- 3. 模块级变量与常量默认值 ----")
Call print(greet("world"))
Call print(withConst("mixed"))

Call Console.WriteLine()
Call Console.WriteLine("expected:")
Call Console.WriteLine("  [1] ""default"" / ""hello"" / ""given"" / ""explicit""")
Call Console.WriteLine("  [5] ""nested: nested""")
Call Console.WriteLine("  [6] ""[vbs] world""")
Call Console.WriteLine("  [7] ""mixed/42""")
