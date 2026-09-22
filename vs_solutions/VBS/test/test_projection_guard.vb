' =============================================================
' demo: @ 展开的边界(字符串 / 行尾注释 / 掩码安全)
'
'   vbs ./test/test_projection_guard.vb --verbose
'
' `@` 只对「标识符点号链 + 成员名」生效, 并且会先屏蔽字符串字面量与行尾注释,
' 因此下面这些位置里的 @ 不会被误当成投影运算符:
'
'   - 字符串字面量内部
'   - 行尾注释内部
'   - 数值字面量的 Decimal 类型字符(1.5@)
'
' 同一行里既有"字符串中的 @"又有"真正的投影"时, 两者互不干扰。
' =============================================================

Class Row
    Property v As Double
    Property label As String
End Class

Dim rows As Row() = {
    New Row With {.v = 1.25, .label = "a"},
    New Row With {.v = 2.25, .label = "b"},
    New Row With {.v = 3.25, .label = "c"}
}

' ---- 字符串字面量里的 @: 保持原样 ----
Dim literal As String = "list@x 不会被展开, 1.5@ 也不会"
Call Console.WriteLine("literal      = " & literal)

' ---- 行尾注释里的 @y: 不会被展开 ----
Dim vs = rows@v              ' 这里的 rows@label 只是注释, 不应影响本行的投影
Call Console.WriteLine("rows@v       = " & String.Join(", ", vs) & "   (" & TypeName(vs) & ")")

' ---- 同一行里字符串中的 @ 与真正的投影共存 ----
Call Console.WriteLine("mix          = " & "a@b" & String.Join(", ", rows@label))

' ---- 数值字面量的 Decimal 类型字符 ----
Dim dec As Decimal = 1.5@
Call Console.WriteLine("decimal 1.5@ = " & dec)

' ---- 投影结果照常参与运算 ----
Dim doubled = rows@v * 2
Call Console.WriteLine("rows@v * 2   = " & String.Join(", ", doubled))

Call Console.WriteLine()
Call Console.WriteLine("expected rows@v = 1.25, 2.25, 3.25")
