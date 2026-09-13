' =============================================================
' demo: 脚本引擎注入的"魔法方法"
'
'   vbs ./test/test_magics.vb
'
' 这些方法由引擎在预处理阶段烘焙进生成代码, 脚本无需任何 import。
' =============================================================
#title "VBS Magic Methods Demo"
#version "0.1.0"

' 引用宿主自身的 assembly, 用于演示 Includes() / Locate()
#include "vbs.dll"

Call Console.WriteLine("---------- 脚本自身上下文 ----------")
Call Console.WriteLine("ScriptFile()        = " & ScriptFile())
Call Console.WriteLine("ScriptDir()         = " & ScriptDir())
Call Console.WriteLine("ScriptName()        = " & ScriptName())
Call Console.WriteLine("ScriptName(False)   = " & ScriptName(False))
Call Console.WriteLine("Here(""data.txt"")    = " & Here("data.txt"))

Dim lines = ScriptLines()
Call Console.WriteLine("ScriptLines()       = " & lines.Length & " 行")
Call Console.WriteLine("ScriptText()        = " & ScriptText().Length & " 字符")
Call Console.WriteLine("Self().FullName     = " & Self().FullName)

Call Console.WriteLine()
Call Console.WriteLine("---------- 依赖与路径定位 ----------")
Call Console.WriteLine("Includes()          = " & Includes().Length & " 项")
For Each dll In Includes()
    Call Console.WriteLine("    -> " & dll)
Next

Dim located = Locate("test_magics.vb")
Call Console.WriteLine("Locate(self)        = " & If(located Is Nothing, "<Not Found>", located))
Call Console.WriteLine("Locate(missing.dll) = " & If(Locate("missing.dll") Is Nothing, "<Nothing>", Locate("missing.dll")))
