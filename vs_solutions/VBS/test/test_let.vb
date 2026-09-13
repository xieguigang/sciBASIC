' =============================================================
' demo: let 动态类型声明 与 LINQ Let 子句的区分
'
'   vbs ./test/test_let.vb
'
' Dim  : 由 Roslyn 自动推断为强类型
' let  : 被预处理为 Dim ... As Object, 得到动态类型变量
' LINQ : 查询表达式之中的 Let 子句不会被改写
' =============================================================

Class Person
    Public Property Name As String
    Public Property Age As Integer
End Class

' ---- Dim 由 Roslyn 推断为强类型(String) ----
Dim strongText = "hello"
Call Console.WriteLine("Dim   strongText = " & TypeName(strongText))

' ---- let 被改写为 Dim ... As Object, 得到可以在运行时重新绑定的动态变量 ----
' 注意: TypeName 返回的是变量在运行时的实际类型, 而非声明类型
let dynText = "hello"
Call Console.WriteLine("let   dynText #1 = " & TypeName(dynText) & " -> " & dynText)

dynText = 3.1415926
Call Console.WriteLine("let   dynText #2 = " & TypeName(dynText) & " -> " & dynText)

Call Console.WriteLine()

' ---- 动态类型可以在运行时切换成员绑定 ----
let value = 123
Call Console.WriteLine("let   value #1   = " & TypeName(value) & " -> " & value)

value = "now a string"
Call Console.WriteLine("let   value #2   = " & TypeName(value) & " -> " & value)

value = New Person With {.Name = "asuka", .Age = 18}
Call Console.WriteLine("let   value #3   = " & TypeName(value) & " -> " & value.Name & " / " & value.Age)

Call Console.WriteLine()

' ---- 多行 LINQ 查询: Let 子句保持原样 ----
Dim numbers = {1, 2, 3, 4, 5, 6}
Dim squares = From n In numbers
              Let sq = n * n
              Where sq > 9
              Select sq

Call Console.WriteLine("LINQ  多行 Let   = " & String.Join(", ", squares))

' ---- 单行 LINQ 查询: Let 子句同样不被改写 ----
Dim total = (From n In numbers Let doubled = n * 2 Select doubled).Sum()
Call Console.WriteLine("LINQ  单行 Let   = " & total)
