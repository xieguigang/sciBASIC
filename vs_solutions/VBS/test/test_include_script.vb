' =============================================================
' demo: #include 引入其它的 VB 脚本
'
'   vbs ./test/test_include_script.vb
'
' 被引入脚本的代码会被直接复制到主脚本的顶层命名空间中:
' 其 Imports 提升到生成代码头部, 其类型定义与主脚本的类型定义并列。
' =============================================================
#include "./lib/Helper.vb"

Call Console.WriteLine("---------- 被引入脚本的类型 ----------")
Call Console.WriteLine(LibraryHelper.Greet("vbs"))
Call Console.WriteLine("---------- 被引入脚本的类型互引用 ----------")

Dim v As Vector2 = New Vector2 With {.X = 3, .Y = 4}

Call Console.WriteLine("vector   = " & v.ToString())
Call Console.WriteLine("scaled   = " & LibraryAlgebra.Scale(v, 2.5).ToString())
Call Console.WriteLine("report   = " & vbCrLf & LibraryHelper.BuildReport({"alpha", "beta", "gamma"}))
Call Console.WriteLine("includes = " & Includes().Length & " 项")
