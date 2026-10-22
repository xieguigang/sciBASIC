Imports System.Numerics
Imports System.Runtime.CompilerServices

Namespace Hamming

    ''' <summary>
    ''' Hamming 距离的纯函数实现集合。
    ''' 
    ''' 对长度均为 ``d`` 的两个符号串 ``x`` 与 ``y``，Hamming 距离定义为
    ''' 对应位置取值不同的位数之和：
    ''' 
    ''' ```
    ''' d_H(x, y) = Σ 1[x_i &lt;&gt; y_i]
    ''' ```
    ''' 
    ''' 当输入为二进制串时，该式等价于异或之后 1 的个数（``popcount``），
    ''' 因此本模块对机器字与位串都提供了可以走位运算快速路径的重载。
    ''' 该度量满足非负性、对称性与三角不等式，是二值/定类数据上的合法度量；
    ''' 在分类数据情形下也称作「简单匹配相异度」（simple matching dissimilarity）。
    ''' </summary>
    <HideModuleName>
    Public Module HammingDistance

#Region "Machine word (binary fast path)"

        ''' <summary>
        ''' 计算两个机器字之间的 Hamming 距离（异或之后 1 的个数）。
        ''' </summary>
        ''' <param name="x">第一个机器字</param>
        ''' <param name="y">第二个机器字</param>
        ''' <returns>对应二进制位取值不同的位数</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Between(x As ULong, y As ULong) As Integer
            Return BitOperations.PopCount(x Xor y)
        End Function

        ''' <summary>
        ''' 计算两组等长机器字之间的 Hamming 距离（逐字异或之后累加 1 的个数）。
        ''' </summary>
        ''' <param name="x">第一个机器字数组</param>
        ''' <param name="y">第二个机器字数组</param>
        ''' <returns>全部二进制位中取值不同的位数</returns>
        Public Function Between(x As ULong(), y As ULong()) As Integer
            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Integer = 0

            For i As Integer = 0 To x.Length - 1
                sum += BitOperations.PopCount(x(i) Xor y(i))
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 统计一组机器字中值为 1 的二进制位个数。
        ''' </summary>
        ''' <param name="words">机器字数组</param>
        ''' <returns>1 的个数</returns>
        Public Function Ones(words As ULong()) As Integer
            Dim sum As Integer = 0

            For i As Integer = 0 To words.Length - 1
                sum += BitOperations.PopCount(words(i))
            Next

            Return sum
        End Function

#End Region

#Region "Symbol sequences"

        ''' <summary>
        ''' 计算两个等长二进制位串之间的 Hamming 距离。
        ''' </summary>
        ''' <param name="x">例如 ``"10110010"``</param>
        ''' <param name="y">例如 ``"10100011"``</param>
        ''' <returns>取值不同的位数</returns>
        ''' <exception cref="ArgumentException">两个位串的长度不一致</exception>
        Public Function Between(x As String, y As String) As Integer
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Integer = 0

            For i As Integer = 0 To x.Length - 1
                If x(i) <> y(i) Then
                    sum += 1
                End If
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 计算两个等长符号序列之间的 Hamming 距离。
        ''' </summary>
        ''' <param name="x">第一个符号序列</param>
        ''' <param name="y">第二个符号序列</param>
        ''' <returns>取值不同的位置数</returns>
        Public Function Between(x As Char(), y As Char()) As Integer
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Integer = 0

            For i As Integer = 0 To x.Length - 1
                If x(i) <> y(i) Then
                    sum += 1
                End If
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 计算两个等长字符串数组（定类取值）之间的 Hamming 距离。
        ''' </summary>
        ''' <param name="x">第一个取值序列</param>
        ''' <param name="y">第二个取值序列</param>
        ''' <returns>取值不同的位置数</returns>
        Public Function Between(x As String(), y As String()) As Integer
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Integer = 0

            For i As Integer = 0 To x.Length - 1
                If Not String.Equals(x(i), y(i), StringComparison.Ordinal) Then
                    sum += 1
                End If
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 计算两个等长整数取值序列（二值或定类编码）之间的 Hamming 距离。
        ''' </summary>
        ''' <param name="x">第一个取值序列</param>
        ''' <param name="y">第二个取值序列</param>
        ''' <returns>取值不同的位置数</returns>
        Public Function Between(x As Integer(), y As Integer()) As Integer
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Integer = 0

            For i As Integer = 0 To x.Length - 1
                If x(i) <> y(i) Then
                    sum += 1
                End If
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 计算两个等长布尔序列之间的 Hamming 距离。
        ''' </summary>
        ''' <param name="x">第一个布尔序列</param>
        ''' <param name="y">第二个布尔序列</param>
        ''' <returns>取值不同的位置数</returns>
        Public Function Between(x As Boolean(), y As Boolean()) As Integer
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Integer = 0

            For i As Integer = 0 To x.Length - 1
                If x(i) <> y(i) Then
                    sum += 1
                End If
            Next

            Return sum
        End Function

#End Region

#Region "Weighted distance"

        ''' <summary>
        ''' 按位加权的 Hamming 距离：每一位的差异按照 <paramref name="weights"/>
        ''' 中对应位置的权重累加，用于「各位重要性不等」的高维二值数据。
        ''' </summary>
        ''' <param name="x">第一个位串</param>
        ''' <param name="y">第二个位串</param>
        ''' <param name="weights">每一位的权重，长度必须不小于位串长度</param>
        ''' <returns>加权距离</returns>
        Public Function Weighted(x As String, y As String, weights As Double()) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If weights Is Nothing Then
                Throw New ArgumentNullException(NameOf(weights))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            If weights.Length < x.Length Then
                Throw New ArgumentException($"the weights vector has {weights.Length} values, but the pattern length is {x.Length}!", NameOf(weights))
            End If

            Dim sum As Double = 0

            For i As Integer = 0 To x.Length - 1
                If x(i) <> y(i) Then
                    sum += weights(i)
                End If
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 按位加权的 Hamming 距离，输入为机器字数组。
        ''' </summary>
        ''' <param name="x">第一个机器字数组</param>
        ''' <param name="y">第二个机器字数组</param>
        ''' <param name="weights">按位下标的权重向量（位下标 = 字下标 * 64 + 字内位序）</param>
        ''' <returns>加权距离</returns>
        Public Function Weighted(x As ULong(), y As ULong(), weights As Double()) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If weights Is Nothing Then
                Throw New ArgumentNullException(NameOf(weights))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            Dim sum As Double = 0

            For w As Integer = 0 To x.Length - 1
                Dim diff As ULong = x(w) Xor y(w)

                While diff <> 0UL
                    Dim bit As Integer = BitOperations.TrailingZeroCount(diff)
                    Dim position As Integer = w * 64 + bit

                    If position >= weights.Length Then
                        Throw New ArgumentException($"the weights vector has {weights.Length} values, but the pattern contains bit at position {position}!", NameOf(weights))
                    End If

                    sum += weights(position)
                    diff = diff And (diff - 1UL)
                End While
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 按位加权的 Hamming 距离，输入为整数取值序列。
        ''' </summary>
        ''' <param name="x">第一个取值序列</param>
        ''' <param name="y">第二个取值序列</param>
        ''' <param name="weights">每一位的权重</param>
        ''' <returns>加权距离</returns>
        Public Function Weighted(x As Integer(), y As Integer(), weights As Double()) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If weights Is Nothing Then
                Throw New ArgumentNullException(NameOf(weights))
            End If

            EnsureSameLength(x.Length, y.Length, NameOf(y))

            If weights.Length < x.Length Then
                Throw New ArgumentException($"the weights vector has {weights.Length} values, but the pattern length is {x.Length}!", NameOf(weights))
            End If

            Dim sum As Double = 0

            For i As Integer = 0 To x.Length - 1
                If x(i) <> y(i) Then
                    sum += weights(i)
                End If
            Next

            Return sum
        End Function

#End Region

#Region "Similarity"

        ''' <summary>
        ''' 把距离换算成 ``[0, 1]`` 区间内的相似度：``1 - distance / dimension``。
        ''' 
        ''' 当 <paramref name="dimension"/> 不是正数时，改用 ``1 / (1 + distance)`` 的无量纲换算，
        ''' 以保证返回值始终落在 ``(0, 1]`` 区间。
        ''' </summary>
        ''' <param name="distance">Hamming 距离</param>
        ''' <param name="dimension">位串（特征）的总长度</param>
        ''' <returns>相似度</returns>
        Public Function Similarity(distance As Double, Optional dimension As Integer = 0) As Double
            If Double.IsNaN(distance) Then
                Return 0
            End If
            If dimension <= 0 Then
                Return 1.0 / (1.0 + distance)
            End If

            Dim ratio As Double = 1.0 - distance / dimension

            If ratio < 0 Then
                Return 0
            ElseIf ratio > 1 Then
                Return 1
            Else
                Return ratio
            End If
        End Function

#End Region

        Private Sub EnsureSameLength(length1 As Integer, length2 As Integer, argument As String)
            If length1 <> length2 Then
                Throw New ArgumentException($"the two input sequences must have the same length, but {length1} <> {length2}!", argument)
            End If
        End Sub
    End Module
End Namespace
