' =============================================================
' demo: 动态类型( let )与魔法方法的综合运用
'
'   vbs ./test/test_dynamics.vb
' =============================================================
#package "VBScript.Demo.Dynamics"
#author "xieguigang"
#title "VBS Dynamic Type Demo"
#version "0.2.0"

' ---- 使用 let 声明动态变量, 并在运行时不断切换其结构 ----
let payload = New With {.Name = "vbscript", .Count = 0}
Call Console.WriteLine("[1] " & TypeName(payload) & " -> " & payload.Name & " / " & payload.Count)

payload = New With {.Path = ScriptFile(), .Engine = "vbs", .Version = Version()}
Call Console.WriteLine("[2] " & TypeName(payload) & " -> " & payload.Path & " / v" & payload.Version)

Call Console.WriteLine()

' ---- 强类型(Dim)与动态类型(let)的对照 ----
Dim strong = 42
let weak = 42
Call Console.WriteLine("Dim strong = " & TypeName(strong) & " : " & strong)
Call Console.WriteLine("let weak   = " & TypeName(weak) & " : " & weak)

weak = "forty-two"
Call Console.WriteLine("let weak   = " & TypeName(weak) & " : " & weak)

Call Console.WriteLine()

' ---- 通过魔法方法读回本脚本的元数据 ----
Call Console.WriteLine("Package() = " & Package())
Call Console.WriteLine("Author()  = " & Author())
Call Console.WriteLine("Title()   = " & Title())
Call Console.WriteLine("Version() = " & Version())
Call Console.WriteLine("Self()    = " & Self().GetName().Name)
