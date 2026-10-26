Imports std = System.Math

''' <summary>
''' IL→CUDA 教程用的目标函数集合。
''' <para>
''' 这些是普通的 VB.NET 静态方法，内部没有任何 CUDA 概念：<c>stdlib.cuda</c> 会在运行时
''' 通过 IL 反编译把它们翻译成 CUDA 内核源码。放在标准库程序集里（而不是脚本里）的原因是
''' 两个脚本引擎都必须能拿到可反射的 <c>Shared</c> 方法（JS 函数不是 CLR 方法）。
''' </para>
''' <para>
''' 索引约定（详见 ILCuda 的 README）：
''' ① 参数表里出现 <c>i</c> → 一维内核；② 同时出现 <c>i</c> 与 <c>j</c> → 二维内核；
''' ③ 纯标量且不访问数组 → 自动按元素包裹成一维内核。
''' </para>
''' </summary>
Public Class PearsonMetrics

    ''' <summary>行求和；约定 ①，<paramref name="i"/> 是线程索引。</summary>
    Public Shared Function RowSum(x As Single(), cols As Integer, i As Integer) As Single
        Dim sum As Single = 0.0F

        For k As Integer = 0 To cols - 1
            sum += x(i * cols + k)
        Next

        Return sum
    End Function

    ''' <summary>行平方和；一维内核。</summary>
    Public Shared Function RowSumSq(x As Single(), cols As Integer, i As Integer) As Single
        Dim sumSq As Single = 0.0F

        For k As Integer = 0 To cols - 1
            Dim v As Single = x(i * cols + k)
            sumSq += v * v
        Next

        Return sumSq
    End Function

    ''' <summary>两行的点积；约定 ②，同时出现 i 与 j → 二维内核。</summary>
    Public Shared Function GramDot(x As Single(), cols As Integer, i As Integer, j As Integer) As Single
        Dim acc As Single = 0.0F

        For k As Integer = 0 To cols - 1
            acc += x(i * cols + k) * x(j * cols + k)
        Next

        Return acc
    End Function

    ''' <summary>皮尔逊相关系数的单个单元格（含 guard 分支与 MathF 调用）。</summary>
    Public Shared Function CorrelationCell(dot As Single(), rowSum As Single(), rowSumSq As Single(),
                                           rows As Integer, cols As Integer,
                                           i As Integer, j As Integer) As Single
        If i = j Then
            Return 1.0F
        End If

        Dim n As Single = CSng(cols)
        Dim meanI As Single = rowSum(i) / n
        Dim meanJ As Single = rowSum(j) / n
        Dim varI As Single = rowSumSq(i) - n * meanI * meanI
        Dim varJ As Single = rowSumSq(j) - n * meanJ * meanJ
        Dim d As Single = dot(i * rows + j)
        Dim cov As Single = d - n * meanI * meanJ
        Dim denom As Single = MathF.Sqrt(MathF.Max(varI, 0.0F)) * MathF.Sqrt(MathF.Max(varJ, 0.0F))
        Dim c As Single

        If denom > 1.0E-12F Then
            c = cov / denom
        Else
            c = 0.0F
        End If

        Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
    End Function

    ''' <summary>欧氏距离的单个单元格。</summary>
    Public Shared Function DistanceCell(dot As Single(), rowSumSq As Single(),
                                        rows As Integer, i As Integer, j As Integer) As Single
        If i = j Then
            Return 0.0F
        End If

        Dim d As Single = dot(i * rows + j)
        Dim d2 As Single = MathF.Max(rowSumSq(i) + rowSumSq(j) - 2.0F * d, 0.0F)

        Return MathF.Sqrt(d2)
    End Function

    ''' <summary>纯标量限幅；约定 ③，自动包裹成逐元素内核。</summary>
    Public Shared Function PearsonClamp(cov As Single, denom As Single) As Single
        Dim c As Single

        If denom > 1.0E-12F Then
            c = cov / denom
        Else
            c = 0.0F
        End If

        Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
    End Function
End Class
