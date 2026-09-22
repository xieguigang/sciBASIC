' =============================================================
' demo: 向量化的运算符覆盖与 VB 逐元素语义
'
'   vbs ./test/test_vectorize_ops.vb
'
' 覆盖 + - * / \ Mod ^ 与一元负号, 以及标量在左/在右两种形态。
' 结果类型严格遵循 VB 的逐元素语义:
'
'   + - * Mod : 取两侧的公共数值类型(Short < Integer < Long < Single < Double)
'   /   ^     : 恒为 Double
'   \         : 整型(VB 的整除)
' =============================================================

Dim a = {7, 8, 9, 10}
Dim b = {2, 3, 4, 5}

Call Console.WriteLine("a        = " & String.Join(", ", a) & "   (" & TypeName(a) & ")")
Call Console.WriteLine("b        = " & String.Join(", ", b) & "   (" & TypeName(b) & ")")
Call Console.WriteLine()

' ---- 向量与向量 ----
Call Console.WriteLine("a + b    = " & String.Join(", ", a + b))
Call Console.WriteLine("a - b    = " & String.Join(", ", a - b))
Call Console.WriteLine("a * b    = " & String.Join(", ", a * b))
Call Console.WriteLine("a / b    = " & String.Join(", ", a / b) & "   (" & TypeName(a / b) & ")")
Call Console.WriteLine("a \ b    = " & String.Join(", ", a \ b) & "   (" & TypeName(a \ b) & ")")
Call Console.WriteLine("a Mod b  = " & String.Join(", ", a Mod b) & "   (" & TypeName(a Mod b) & ")")
Call Console.WriteLine("a ^ 2    = " & String.Join(", ", a ^ 2) & "   (" & TypeName(a ^ 2) & ")")
Call Console.WriteLine()

' ---- 向量与标量(标量在右) ----
Call Console.WriteLine("a + 100  = " & String.Join(", ", a + 100))
Call Console.WriteLine("a - 100  = " & String.Join(", ", a - 100))
Call Console.WriteLine("a * 10   = " & String.Join(", ", a * 10))
Call Console.WriteLine("a / 2    = " & String.Join(", ", a / 2))
Call Console.WriteLine("a \ 2    = " & String.Join(", ", a \ 2))
Call Console.WriteLine("a Mod 3  = " & String.Join(", ", a Mod 3))
Call Console.WriteLine("a ^ 0.5  = " & String.Join(", ", a ^ 0.5))
Call Console.WriteLine()

' ---- 向量与标量(标量在左) ----
Call Console.WriteLine("100 - a  = " & String.Join(", ", 100 - a))
Call Console.WriteLine("100 / a  = " & String.Join(", ", 100 / a))
Call Console.WriteLine("100 \ a  = " & String.Join(", ", 100 \ a))
Call Console.WriteLine("100 Mod a= " & String.Join(", ", 100 Mod a))
Call Console.WriteLine("2 ^ b    = " & String.Join(", ", 2 ^ b))
Call Console.WriteLine()

' ---- 一元负号 ----
Call Console.WriteLine("-a       = " & String.Join(", ", -a))
Call Console.WriteLine()

' ---- 混合类型的类型提升: Integer() 与 Double 标量 => Double() ----
Dim mixed = a + 0.5
Call Console.WriteLine("a + 0.5  = " & String.Join(", ", mixed) & "   (" & TypeName(mixed) & ")")

' ---- Single 向量保持 Single ----
Dim s As Single() = {1.5F, 2.5F, 3.5F}
Dim s2 = s * 2.0F
Call Console.WriteLine("s * 2.0F = " & String.Join(", ", s2) & "   (" & TypeName(s2) & ")")
