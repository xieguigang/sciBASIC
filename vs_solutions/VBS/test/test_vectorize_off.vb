' =============================================================
' demo: 关闭向量化
'
'   vbs ./test/test_vectorize_off.vb
'   vbs ./test/test_vectorize_off.vb --no-vectorize
'
' 脚本头部写 #no-vectorize 可以按脚本关闭向量化改写;
' 命令行 --no-vectorize 可以按次关闭。
' 关闭之后脚本保持原有形态: 向量运算必须手写成循环,
' 数组下标访问(x(i))也不会被误解为向量运算。
' =============================================================

#no-vectorize

Dim x = {1, 2, 3, 4, 5}
Dim sum(4) As Integer

For i As Integer = 0 To x.Length - 1
    sum(i) = x(i) + 5
Next

Call Console.WriteLine("x       = " & String.Join(", ", x))
Call Console.WriteLine("x(i) + 5= " & String.Join(", ", sum))
