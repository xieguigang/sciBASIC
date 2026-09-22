' =============================================================
' demo: @ 多属性投影(匿名类型数组)
'
'   vbs ./test/test_projection_multi.vb --verbose
'
'   Dim p = list@{x, y}
'   => Dim p = list.Select(Function(__vbs_o)
'                             New With {.x = __vbs_o.x, .y = __vbs_o.y}
'                         ).ToArray()
'
' 多属性投影的结果是**匿名类型数组**: 可以正常读取成员,
' 但没有可命名的元素类型, 因此不参与 SIMD 向量化, 也不能继续用 @ 取成员。
' =============================================================

Class Person
    Property name As String
    Property age As Integer
    Property score As Double
End Class

Dim people As Person() = {
    New Person With {.name = "asuka", .age = 18, .score = 91.5},
    New Person With {.name = "xie", .age = 25, .score = 88.0},
    New Person With {.name = "guigang", .age = 31, .score = 95.25}
}

' ---- 双属性投影 ----
Dim pairs = people@{name, age}
Call Console.WriteLine($"pairs.Length = {pairs.Length}")
Call Console.WriteLine($"pairs(0)     = {pairs(0).name} / {pairs(0).age}")
Call Console.WriteLine($"pairs(2)     = {pairs(2).name} / {pairs(2).age}")
Call Console.WriteLine()

' ---- 三属性投影 ----
Dim triples = people@{name, age, score}
Call Console.WriteLine($"triples(1)   = {triples(1).name} / {triples(1).age} / {triples(1).score}")
Call Console.WriteLine()

' ---- 花括号里只写一个成员同样得到匿名类型元素 ----
Dim oneProp = people@{name}
Call Console.WriteLine($"oneProp(0).name = {oneProp(0).name}")
Call Console.WriteLine()

' ---- 投影之后可以继续 LINQ(匿名类型成员可读) ----
For Each row In triples
    Call Console.WriteLine($"  {row.name} => {row.score}")
Next
